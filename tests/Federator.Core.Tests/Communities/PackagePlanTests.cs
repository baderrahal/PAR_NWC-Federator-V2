using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using Federator.Core.Communities;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F208. Each package's report folder mirrors its path from the parent under the Report folder, which is refused inside
    /// the parent, and each package's groups are made from its own files only, with their NWF and NWD in its own folders.
    /// </summary>
    [TestFixture]
    public class PackagePlanTests
    {
        private string root;

        [SetUp]
        public void MakeRoot()
        {
            root = TempFolder.Make("plan");
        }

        [TearDown]
        public void RemoveRoot()
        {
            TempFolder.Remove(root);
        }

        private void Make(params string[] parts)
        {
            string path = Path.Combine(new[] { root, "Parent" }.Concat(parts).ToArray());
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, "nwc");
        }

        private string Report
        {
            get { return Path.Combine(root, "Report"); }
        }

        private PackagePlan PlanOf(Package package)
        {
            return PackagePlan.For(
                package, Report, GroupingModes.Default, new ContainerNameSettings(), new OutputNaming(),
                new DateTime(2026, 10, 17));
        }

        [Test]
        public void TheReportFolderOfAPackageMirrorsItsPathFromTheParent()
        {
            Make("C06", "Infrastructure and Landscape", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            Package package = PackageTree.Read(Path.Combine(root, "Parent"), ".nwc").Packages()[0];

            Assert.That(PackageOutput.FolderFor(Report, package),
                Is.EqualTo(Path.Combine(Report, "C06", "Infrastructure and Landscape")));
            Assert.That(PackageOutput.NwfFolderFor(Report, package),
                Is.EqualTo(Path.Combine(Report, "C06", "Infrastructure and Landscape", "NWF")));
            Assert.That(PackageOutput.NwdFolderFor(Report, package),
                Is.EqualTo(Path.Combine(Report, "C06", "Infrastructure and Landscape", "NWD")));
            Assert.That(PackageOutput.FoldersToMake(Report, package).ToArray(), Is.EqualTo(new[]
            {
                PackageOutput.FolderFor(Report, package),
                PackageOutput.NwfFolderFor(Report, package),
                PackageOutput.NwdFolderFor(Report, package)
            }));
        }

        [Test]
        public void TheReportFolderIsRefusedEmptyRelativeAtTheParentOrInsideIt()
        {
            string parent = TestPaths.At("Work", "Parent");

            Assert.That(PackageOutput.WhyNot(parent, " "), Is.EqualTo(PackageWords.PickTheReport));
            Assert.That(PackageOutput.WhyNot(parent, "Report"), Does.Contain("full path"));
            Assert.That(PackageOutput.WhyNot(parent, parent), Does.Contain("inside it"));
            Assert.That(PackageOutput.WhyNot(parent, TestPaths.At("Work", "Parent", "Report")), Does.Contain("inside it"));
            Assert.That(PackageOutput.WhyNot(parent, TestPaths.At("Work", "Report")), Is.Null);
            Assert.That(PackageOutput.WhyNot(parent, TestPaths.At("Work", "Parent Report")), Is.Null, "a sibling whose name starts the same");
        }

        [Test]
        public void APackagesGroupsAreItsOwnFilesWithTheirNwfAndNwdInItsOwnFolders()
        {
            Make("C06", "Infrastructure", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            Make("C06", "Infrastructure", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");
            Make("C06", "Infrastructure", "1104-PAR-1B06G1-ZZZ-ME-MOD-000001.nwc");
            Make("C06", "Infrastructure and Landscape", "1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc");
            PackageTree tree = PackageTree.Read(Path.Combine(root, "Parent"), ".nwc");
            Package infra = tree.Find(Path.Combine("C06", "Infrastructure"));

            PackagePlan plan = PlanOf(infra);

            Assert.That(plan.WhyNot, Is.Null);
            Assert.That(plan.Groups.Count, Is.EqualTo(2));
            PackageGroup bc = plan.Groups.Single(g => g.Building == "1B06BC");
            Assert.That(bc.Files.Count, Is.EqualTo(2), "the ST file of the other package is not in this group");
            Assert.That(bc.Files.All(f => f.StartsWith(infra.FullPath, StringComparison.OrdinalIgnoreCase)), Is.True);
            Assert.That(bc.NwfPath, Is.EqualTo(Path.Combine(Report, "C06", "Infrastructure", "NWF", bc.NwfName + ".nwf")));
            Assert.That(Path.GetDirectoryName(bc.NwdPath), Is.EqualTo(Path.Combine(Report, "C06", "Infrastructure", "NWD")));
            Assert.That(bc.Disciplines.ToArray(), Is.EqualTo(new[] { "AR", "EL" }));
            Assert.That(bc.Label, Is.EqualTo(Path.Combine("C06", "Infrastructure") + "  1B06BC"));
        }

        [Test]
        public void AFileWhoseNameDoesNotReadIsNamedAndRunsNothing()
        {
            Make("C07", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            Make("C07", "notes.nwc");
            Package package = PackageTree.Read(Path.Combine(root, "Parent"), ".nwc").Packages()[0];

            PackagePlan plan = PlanOf(package);

            Assert.That(plan.Groups.Count, Is.EqualTo(1));
            Assert.That(plan.NotRun.Single(), Does.StartWith("cannot be read  notes.nwc  "));
            Assert.That(plan.Unreadable, Is.EqualTo(1));
            Assert.That(plan.Grouped.Single().Building, Is.EqualTo("1B06BC"), "the groups the findings read");
            Assert.That(plan.Skipped, Is.Empty);
        }

        [Test]
        public void AReportFolderThatPutsTheParentsOwnPackageInsideTheParentIsRefused()
        {
            Make("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            Make("C06", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");
            string parent = Path.Combine(root, "Parent");
            PackageTree tree = PackageTree.Read(parent, ".nwc");

            Assert.That(PackageOutput.WhyNot(parent, root), Is.Null, "the Report folder alone is outside the parent");

            string why = PackageOutput.WhyNot(parent, root, tree.Packages());

            Assert.That(why, Is.Not.Null, "Report\\Parent is the parent folder itself");
            Assert.That(why, Does.StartWith("The report folder of Parent would be " + parent));
            Assert.That(PackageOutput.WhyNot(parent, Report, tree.Packages()), Is.Null, "a Report folder beside the parent is fine");
            Assert.That(PackageOutput.WhyNot(parent, root, new[] { tree.Find("C06") }), Is.Null, "C06 alone lands in root\\C06");
        }

        [Test]
        public void AGroupTheGroupingBlocksIsHandedOnWithItsFilesAndWhy()
        {
            Make("C07", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            Make("C07", "1104-XYZ-1B06BC-ZZZ-EL-MOD-000001.nwc");
            Package package = PackageTree.Read(Path.Combine(root, "Parent"), ".nwc").Packages()[0];

            PackagePlan plan = PlanOf(package);

            Assert.That(plan.Blocked, Is.EqualTo(1));
            Assert.That(plan.Skipped.Count, Is.EqualTo(1));
            Assert.That(plan.Skipped[0].Building, Is.EqualTo("1B06BC"));
            Assert.That(plan.Skipped[0].Files.Count, Is.EqualTo(2));
            Assert.That(plan.Skipped[0].Reason, Is.Not.Empty);
            Assert.That(plan.Grouped, Is.Empty);
            Assert.That(plan.Unreadable, Is.EqualTo(0));
        }

        [Test]
        public void APackageWithNoGroupThatCanRunSaysSoAndRunsNothing()
        {
            Make("C08", "notes.nwc");
            Package package = PackageTree.Read(Path.Combine(root, "Parent"), ".nwc").Packages()[0];

            PackagePlan plan = PlanOf(package);

            Assert.That(plan.Groups, Is.Empty);
            Assert.That(plan.WhyNot, Is.EqualTo("C08 makes no group that can run, so nothing of it is run"));
        }

        [Test]
        public void ANameTheScannedRunsGuardRefusesRefusesThePackage()
        {
            Make("C06", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            Make("C06", "1104-PAR-1B06G1-ZZZ-ME-MOD-000001.nwc");
            Package package = PackageTree.Read(Path.Combine(root, "Parent"), ".nwc").Packages()[0];
            OutputNaming naming = new OutputNaming();
            naming.Nwf.Level = string.Empty;

            PackagePlan plan = PackagePlan.For(
                package, Report, GroupingModes.Default, new ContainerNameSettings(), naming,
                new DateTime(2026, 10, 17));

            Assert.That(plan.WhyNot, Is.EqualTo(
                OutputNameTable.From(
                    BuildingGrouping.Group(
                        package.Files.Select(f => ContainerName.Parse(f, new ContainerNameSettings())),
                        GroupingModes.Default,
                        new ContainerNameSettings()).Groups,
                    naming,
                    new ContainerNameSettings(),
                    new DateTime(2026, 10, 17)).WhyTheRunCannotStart()));
            Assert.That(plan.WhyNot, Is.Not.Null);
        }

        [Test]
        public void TheLabelNamesThePackageThenTheGroup()
        {
            Assert.That(PackagePlan.LabelFor(Path.Combine("C06", "Infrastructure"), "1B06BC"),
                Is.EqualTo(Path.Combine("C06", "Infrastructure") + "  1B06BC"));
            Assert.That(PackagePlan.LabelFor(string.Empty, "1B06BC"), Is.EqualTo("1B06BC"), "a run of one folder keeps the plain group");
        }

        [Test]
        public void TheFolderLinesSayMadeOrThere()
        {
            Assert.That(PackageOutput.MadeLine("X"), Is.EqualTo("PACKAGES folder made     X"));
            Assert.That(PackageOutput.ThereLine("X"), Is.EqualTo("PACKAGES folder there    X"));
        }
    }
}
