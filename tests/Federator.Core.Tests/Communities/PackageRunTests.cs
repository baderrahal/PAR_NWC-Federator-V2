using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Federator.Core.Diagnostics;
using Federator.Core.Communities;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F208. What a run of packages changes outside the tree: the NWFs the NWF page runs for a package, the RESULT block
    /// naming each group with its community and package, the two pickers remembered on their own, and the tree on both
    /// pages of the window, read off the XAML.
    /// </summary>
    [TestFixture]
    public class PackageRunTests
    {
        private string root;

        [SetUp]
        public void MakeRoot()
        {
            root = TempFolder.Make("packagerun");
        }

        [TearDown]
        public void RemoveRoot()
        {
            TempFolder.Remove(root);
        }

        private string Make(params string[] parts)
        {
            string path = Path.Combine(new[] { root }.Concat(parts).ToArray());
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, "x");
            return path;
        }

        private static string ReadLog(RunLog log)
        {
            using (FileStream stream = new FileStream(log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static string Xaml()
        {
            return File.ReadAllText(Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Ui", "FederatorWindow.xaml"));
        }

        [Test]
        public void TheNwfPageRunsTheNwfsInThePackagesNwfFolderAndWritesIntoItsOwnFolders()
        {
            Make("Parent", "C06", "Infrastructure", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            string report = Path.Combine(root, "Report");
            string second = Make("Report", "C06", "Infrastructure", "NWF", "1104-PAR-1B06G1-ZZZ-BM-MOD-000001.nwf");
            string first = Make("Report", "C06", "Infrastructure", "NWF", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.NWF");
            Make("Report", "C06", "Infrastructure", "NWF", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwd");
            Make("Report", "C06", "Infrastructure", "NWF", "old.nwf.bak");
            Package package = PackageTree.Read(Path.Combine(root, "Parent"), PackageTree.NwcExtension).Packages()[0];

            IList<PackageNwf> nwfs = PackageNwfs.For(report, package, NwfPickDisk.Real);

            Assert.That(nwfs.Count, Is.EqualTo(2));
            Assert.That(nwfs[0].NwfPath, Is.EqualTo(first));
            Assert.That(nwfs[1].NwfPath, Is.EqualTo(second));
            Assert.That(nwfs[0].NwdPath, Is.EqualTo(Path.Combine(report, "C06", "Infrastructure", "NWD", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwd")));
            Assert.That(nwfs[0].LogFolder, Is.EqualTo(Path.Combine(report, "C06", "Infrastructure", "Log")));
            Assert.That(nwfs[0].Package, Is.SameAs(package));
        }

        [Test]
        public void APackageWithNoNwfFolderGivesNoNwfAndItsLineSaysSo()
        {
            Make("Parent", "C07", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            Package package = PackageTree.Read(Path.Combine(root, "Parent"), PackageTree.NwcExtension).Packages()[0];

            Assert.That(PackageNwfs.For(Path.Combine(root, "Report"), package, NwfPickDisk.Real), Is.Empty);
            Assert.That(PackageWords.NoNwfYet("C07", "X"), Is.EqualTo("C07 has no NWF in X, so nothing of it was run. Run the NWC page on it first"));
        }

        [Test]
        public void TheResultBlockCarriesEachGroupWithItsPackageAndTheTotalsUnderTheGroupCounts()
        {
            Make("Parent", "C06", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            Package package = PackageTree.Read(Path.Combine(root, "Parent"), PackageTree.NwcExtension).Packages()[0];
            PackageTally tally = new PackageTally();
            tally.Add(package, "1B06BC", GroupOutcome.Done, 7);

            using (RunLog log = RunLog.Start(Path.Combine(root, "logs"), new DateTime(2026, 10, 17, 23, 0, 0)))
            {
                log.GroupFinished(PackagePlan.LabelFor(package.RelativePath, "1B06BC"), GroupOutcome.Done, 1.0, null, RunPath.Label(RerunDecision.Build, true));
                log.WriteResultBlock(null, packages: tally.Lines());

                string text = ReadLog(log);
                string result = text.Substring(text.LastIndexOf("RESULT", StringComparison.Ordinal));

                Assert.That(text, Does.Contain("GROUP    finished C06  1B06BC  DONE"));
                Assert.That(result.IndexOf("groups failed  :", StringComparison.Ordinal),
                    Is.LessThan(result.IndexOf("group          C06  1B06BC  DONE  7 clashes", StringComparison.Ordinal)));
                Assert.That(result, Does.Contain("package        C06 : 1 group, 1 done, 0 partial, 0 failed, 7 clashes"));
                Assert.That(result, Does.Contain("community      C06 : 1 package, 1 group, 1 done, 0 partial, 0 failed, 7 clashes"));
                Assert.That(result, Does.Contain("run            : 1 community, 1 package, 1 group, 1 done, 0 partial, 0 failed, 7 clashes"));
            }
        }

        [Test]
        public void ARunOfOneFolderWritesNoPackageLine()
        {
            using (RunLog log = RunLog.Start(Path.Combine(root, "logs"), new DateTime(2026, 10, 17, 23, 0, 0)))
            {
                log.GroupFinished("1B06BC", GroupOutcome.Done, 1.0, null, RunPath.Label(RerunDecision.Build, true));
                log.WriteResultBlock(null);

                Assert.That(ReadLog(log), Does.Not.Contain("package        "));
            }
        }

        [Test]
        public void TheParentAndTheReportAreRememberedEachOnItsOwn()
        {
            string file = Path.Combine(root, "folders.txt");
            FolderMemory before = FolderMemory.Load(file);
            before.Remember(PickerKind.PackageParent, TestPaths.At("Work", "Parent"));
            before.Remember(PickerKind.PackageReport, TestPaths.At("Work", "Report"));

            FolderMemory memory = FolderMemory.Load(file);

            Assert.That(memory.LastFor(PickerKind.PackageParent), Is.EqualTo(TestPaths.At("Work", "Parent")));
            Assert.That(memory.LastFor(PickerKind.PackageReport), Is.EqualTo(TestPaths.At("Work", "Report")));
            Assert.That(memory.LastFor(PickerKind.Source), Is.EqualTo(string.Empty));
            Assert.That(FolderMemory.AllKinds(), Has.Member(PickerKind.PackageParent));
            Assert.That(FolderMemory.AllKinds(), Has.Member(PickerKind.PackageReport));
        }

        [Test]
        public void TheTreeIsOnBothPagesWithItsButtonsAndOneTemplate()
        {
            string xaml = Xaml();
            int nwf = xaml.IndexOf("<TabItem Header=\"NWF\">", StringComparison.Ordinal);
            string nwcPage = xaml.Substring(0, nwf);
            string nwfPage = xaml.Substring(nwf);

            foreach (string id in new[] { "PackagesStep", "PackageParentBox", "PackageReportBox", "ReadPackagesButton", "TickAllPackagesButton", "ClearAllPackagesButton", "RunPackagesButton", "PackagesTree" })
            {
                Assert.That(nwcPage, Does.Contain("x:Name=\"" + id + "\""), id);
            }

            foreach (string id in new[] { "NwfPackageParentBox", "NwfPackageReportBox", "NwfReadPackagesButton", "NwfTickAllPackagesButton", "NwfClearAllPackagesButton", "RunNwfPackagesButton", "NwfPackagesTree" })
            {
                Assert.That(nwfPage, Does.Contain("x:Name=\"" + id + "\""), id);
            }

            Assert.That(nwcPage, Does.Contain("Click=\"OnRunPackages\""));
            Assert.That(nwfPage, Does.Contain("Click=\"OnRunNwfPackages\""));
            Assert.That(xaml.Split(new[] { "ItemTemplate=\"{StaticResource PackageLine}\"" }, StringSplitOptions.None).Length - 1, Is.EqualTo(2),
                "both trees draw a line one way");
        }

        [Test]
        public void TheWordsOfTheStepKeepTheTickBoxRule()
        {
            foreach (string label in new[] { PackageWords.TickAllButton, PackageWords.ClearAllButton, PackageWords.RunButton, PackageWords.ReadButton, PackageWords.ParentLabel, PackageWords.ReportLabel })
            {
                Assert.That(label.Split(' ').Length, Is.LessThanOrEqualTo(8), label);
            }

            Assert.That(PackageWords.ReportHelp.Split(' ').Length, Is.LessThanOrEqualTo(12));
            Assert.That(PackageWords.StepHeading, Does.Not.Contain(";"));
            Assert.That(PackageWords.StepHeading, Does.Not.Contain("—"));
        }
    }
}
