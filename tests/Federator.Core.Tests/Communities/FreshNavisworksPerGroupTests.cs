using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Federator.Core.Communities;
using Federator.Core.Diagnostics;
using Federator.Core.Disciplines;
using Federator.Core.Generic;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F222, Bader's answer to Q211 of 18 Oct 2026: a fresh Navisworks per group, not only per package. The order names each
    /// group by its package and its NWF, each group's result carries back what its run read for the discipline reports, and
    /// the window that started the run turns those results into the groups a package's workbooks read. Each file is written
    /// to the disk and read back off it, and each check is broken one thing at a time.
    /// </summary>
    [TestFixture]
    public class FreshNavisworksPerGroupTests
    {
        private string root;

        [SetUp]
        public void MakeRoot()
        {
            root = TempFolder.Make("pergroup");
        }

        [TearDown]
        public void RemoveRoot()
        {
            TempFolder.Remove(root);
        }

        private string ResultPath
        {
            get { return Path.Combine(root, "result-001.txt"); }
        }

        /// <summary>One group's result holding two models: the first read whole, the second with its box and its walks not read.</summary>
        private static PackageRunResult ResultWithFacts()
        {
            ModelFacts ar = new ModelFacts(TestPaths.At("Parent", "C06", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc"))
                .WithBox(-12.5, 3.0000000001, 0.84551240822557006, 40.25, 60.125, 18.75)
                .WithSite(string.Empty)
                .WithCad(new[] { new ElementFact("702888", "survey\tplan.dwg", null, "Import", "Level \"GF\"") })
                .WithGeneric(new[] { new ElementFact(null, "Tank", "Generic Box", "Type 1", null), new ElementFact("5", "Pad", "F", "T", "L1") });
            ModelFacts el = new ModelFacts(TestPaths.At("Parent", "C06", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc"))
                .BoxNot("the box threw\r\nInvalidOperationException")
                .SiteNot("no Project Location")
                .CadNot("the walk stopped")
                .WithGeneric(new ElementFact[0]);

            PackageRunGroup group = new PackageRunGroup("1B06BC", GroupOutcome.Done, 812, null).WithFacts(new[] { ar, el });
            group.Files.Add(ar.File);
            group.Files.Add(el.File);

            PackageRunResult result = new PackageRunResult("C06", TestPaths.At("logs", "run-20261018-110010.log"));
            result.Groups.Add(group);
            result.ResultLines.Add("groups done    : 1");
            return result;
        }

        [Test]
        public void AGroupsResultReadsBackItsFilesAndEveryFactWhole()
        {
            ResultWithFacts().Write(ResultPath);

            string why;
            PackageRunResult read = PackageRunResult.Read(ResultPath, out why);

            Assert.That(why, Is.Null);
            Assert.That(read.Groups.Count, Is.EqualTo(1));
            PackageRunGroup group = read.Groups[0];
            Assert.That(group.Files.Count, Is.EqualTo(2));
            Assert.That(group.FactsNotRead, Is.Null);
            Assert.That(group.Facts.Count, Is.EqualTo(2));

            ModelFacts ar = group.Facts[0];
            Assert.That(ar.File, Does.EndWith("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc"));
            Assert.That(ar.HasBox, Is.True);
            Assert.That(ar.MinX, Is.EqualTo(-12.5));
            Assert.That(ar.MinY, Is.EqualTo(3.0000000001), "the box keeps every digit through the file");
            Assert.That(ar.MinZ, Is.EqualTo(0.84551240822557006), "a value the R format of .NET Framework fails to give back on 64 bit, which G17 gives back");
            Assert.That(ar.MaxZ, Is.EqualTo(18.75));
            Assert.That(ar.Site, Is.EqualTo(string.Empty), "a site read as empty stays a site read, never one not read");
            Assert.That(ar.SiteNotRead, Is.Null);
            Assert.That(ar.CadNotRead, Is.Null);
            Assert.That(ar.Cad.Count, Is.EqualTo(1));
            Assert.That(ar.Cad[0].Name, Is.EqualTo("survey\tplan.dwg"));
            Assert.That(ar.Cad[0].Family, Is.Null, "a value not read stays not read");
            Assert.That(ar.Cad[0].Level, Is.EqualTo("Level \"GF\""));
            Assert.That(ar.Generic.Count, Is.EqualTo(2));
            Assert.That(ar.Generic[0].ElementId, Is.Null);
            Assert.That(ar.Generic[1].ElementId, Is.EqualTo("5"));

            ModelFacts el = group.Facts[1];
            Assert.That(el.HasBox, Is.False);
            Assert.That(el.BoxNotRead, Is.EqualTo("the box threw\r\nInvalidOperationException"));
            Assert.That(el.Site, Is.Null);
            Assert.That(el.SiteNotRead, Is.EqualTo("no Project Location"));
            Assert.That(el.CadNotRead, Is.EqualTo("the walk stopped"), "a walk not read never reads back as none found");
            Assert.That(el.Cad.Count, Is.EqualTo(0));
            Assert.That(el.GenericNotRead, Is.Null, "an empty walk read whole stays a real answer");
            Assert.That(el.Generic.Count, Is.EqualTo(0));
        }

        [Test]
        public void AGroupWhoseFactsWereNotReadReadsBackWhy()
        {
            PackageRunResult result = new PackageRunResult("C06", null);
            result.Groups.Add(new PackageRunGroup("1B06KI", GroupOutcome.Failed, -1, "no file appended").FactsNot("the group ended before its models were read"));
            result.Write(ResultPath);

            string why;
            PackageRunGroup group = PackageRunResult.Read(ResultPath, out why).Groups[0];

            Assert.That(why, Is.Null);
            Assert.That(group.Facts, Is.Null);
            Assert.That(group.FactsNotRead, Is.EqualTo("the group ended before its models were read"));
            Assert.That(new PackageRunGroup("x", GroupOutcome.Done, 0, null).FactsNotRead, Does.Contain("UNKNOWN"),
                "a group handed no facts says so and never reads as facts read");
        }

        [Test]
        public void AFactsLineOutOfPlaceOrBrokenMakesTheWholeResultNoResult()
        {
            ResultWithFacts().Write(ResultPath);
            string good = File.ReadAllText(ResultPath);
            string why;

            string firstModel = good.Substring(good.IndexOf("model\t", StringComparison.Ordinal));
            firstModel = firstModel.Substring(0, firstModel.IndexOf("\r\n", StringComparison.Ordinal) + 2);

            string[] breaks =
            {
                good.Replace("facts\tread\t\r\n", string.Empty),
                good.Replace("group\t1B06BC\tDone\t812\t\r\n", string.Empty),
                good.Replace("facts\tread\t\r\n", "facts\tread\t\r\nfacts\tread\t\r\n"),
                good.Replace("\t-12.5\t", "\tminus twelve\t"),
                good.Replace("facts\tread\t\r\n", "facts\tmaybe\t\r\n"),
                good.Replace("cad\t702888\t", "cad\t702888\tone more\t"),
                good.Replace("facts\tread\t\r\n", "facts\tread\r\n"),
                good.Replace("file\t", "file\tone more\t"),
                good.Replace(firstModel, firstModel.Replace("\r\n", "\tone more\r\n")),
                good.Replace(firstModel, firstModel.Replace("\tread\t\t-12.5\t", "\tmaybe\t\t-12.5\t")),
                good.Replace(firstModel, string.Empty)
            };

            string[] says =
            {
                "no facts line",
                "before any group line",
                "two facts lines",
                "not a number",
                "where it reads read or not and why",
                "holds 6 values where it holds 5",
                "where it reads read or not and why",
                "a file line holds 2 values where it holds 1",
                "a model line holds 16 values where it holds 15",
                "reads maybe where it reads read or not",
                "comes before any model line"
            };

            for (int i = 0; i < breaks.Length; i++)
            {
                Assert.That(breaks[i], Is.Not.EqualTo(good), "break " + i + " changes the file");
                File.WriteAllText(ResultPath, breaks[i], new UTF8Encoding(false));
                Assert.That(PackageRunResult.Read(ResultPath, out why), Is.Null, "break " + i);
                Assert.That(why, Does.Contain(says[i]), "break " + i);
            }

            File.WriteAllText(ResultPath, good.Replace("\tnot\tthe walk stopped\t", "\tread\t\t"), new UTF8Encoding(false));
            Assert.That(PackageRunResult.Read(ResultPath, out why), Is.Not.Null, "a walk read whole with no element is a real answer");

            File.WriteAllText(ResultPath, good.Replace("model\t", "modell\t"), new UTF8Encoding(false));
            Assert.That(PackageRunResult.Read(ResultPath, out why), Is.Null);
            Assert.That(why, Does.Contain("modell"));
        }

        [Test]
        public void ACadLineUnderAWalkNotReadIsRefused()
        {
            ModelFacts model = new ModelFacts("a.nwc").BoxNot("x").SiteNot("y").CadNot("the walk stopped").GenericNot("z");
            PackageRunResult result = new PackageRunResult("C06", null);
            result.Groups.Add(new PackageRunGroup("1B06BC", GroupOutcome.Done, 0, null).WithFacts(new[] { model }));
            result.Write(ResultPath);
            string good = File.ReadAllText(ResultPath);
            string why;

            File.WriteAllText(ResultPath, good.Replace("result\t", "cad\t1\ta\tb\tc\td\r\nresult\t").Replace("end\t", "cad\t1\ta\tb\tc\td\r\nend\t"),
                new UTF8Encoding(false));
            Assert.That(PackageRunResult.Read(ResultPath, out why), Is.Null);
            Assert.That(why, Does.Contain("where its walk was not read"));
        }

        [Test]
        public void AUnitNamesItsPackageAndItsNwfOrIsRefused()
        {
            Assert.Throws<ArgumentException>(() => new PackageRunUnit(string.Empty, "a.nwf", "A"));
            Assert.Throws<ArgumentException>(() => new PackageRunUnit("C06", null, "A"));

            PackageRunUnit unit = new PackageRunUnit("C06", TestPaths.At("NWF", "a.nwf"), null);
            Assert.That(unit.Name, Is.EqualTo(unit.Group), "a group with no name of its own is named by its NWF");
            Assert.That(new PackageRunUnit("C06", "a.nwf", "1B06BC").Label, Is.EqualTo("C06  1B06BC"));
        }

        [Test]
        public void AGroupsOwnNavisworksKnowsItsGroupByItsNwfAlone()
        {
            PackageRunUnit unit = new PackageRunUnit("C06", TestPaths.At("Report", "C06", "NWF", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwf"), "1B06BC");

            Assert.That(unit.IsTheGroupOf(TestPaths.At("Report", "C06", "NWF", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwf")), Is.True);
            Assert.That(unit.IsTheGroupOf(TestPaths.At("Report", "C06", "NWF", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.NWF")), Is.True,
                "compared without case, as Windows reads a path");
            Assert.That(unit.IsTheGroupOf(TestPaths.At("Report", "C06", "NWF", "1104-PAR-1B06BS-ZZZ-BM-MOD-000001.nwf")), Is.False);
            Assert.That(unit.IsTheGroupOf(TestPaths.At("Report", "C07", "NWF", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwf")), Is.False,
                "the same name in another package's folder is another group");
            Assert.That(unit.IsTheGroupOf(null), Is.False);
        }

        [Test]
        public void AnOrderOfTwoGroupsWritingOneNwfIsRefused()
        {
            PackageRunOrder order = new PackageRunOrder
            {
                Page = PackageRunOrder.NwcPage,
                Parent = TestPaths.At("Parent"),
                Report = TestPaths.At("Report"),
                Folder = root,
                PluginId = "ParsonsNwcFederator.PARS"
            };
            order.Units.Add(new PackageRunUnit("C06", TestPaths.At("Report", "C06", "NWF", "a.nwf"), "a"));
            order.Units.Add(new PackageRunUnit("C06", TestPaths.At("Report", "C06", "NWF", "b.nwf"), "b"));
            Assert.That(order.WhyNot(), Is.Null);

            order.Units.Add(new PackageRunUnit("C06", TestPaths.At("Report", "C06", "NWF", "A.NWF"), "a again"));
            Assert.That(order.WhyNot(), Does.Contain("two groups of the order write one NWF"));
            Assert.Throws<InvalidOperationException>(() => order.Write());
        }

        [Test]
        public void AnOrderOfAnEarlierShapeOrABrokenUnitLineIsRefused()
        {
            PackageRunOrder order = new PackageRunOrder
            {
                Page = PackageRunOrder.NwfPage,
                Parent = TestPaths.At("Parent"),
                Report = TestPaths.At("Report"),
                Folder = root,
                PluginId = "ParsonsNwcFederator.PARS"
            };
            order.Units.Add(new PackageRunUnit("C06", TestPaths.At("Report", "C06", "NWF", "a.nwf"), "a"));
            order.Write();
            string good = File.ReadAllText(order.OrderPath);
            string why;

            File.WriteAllText(order.OrderPath, good.Replace(PackageRunOrder.Header, "parsons nwc federator, the order of a run of packages, 1"),
                new UTF8Encoding(false));
            Assert.That(PackageRunOrder.Read(order.OrderPath, out why), Is.Null, "an order written before F222 named packages, not groups");
            Assert.That(why, Does.Contain("does not start with"));

            File.WriteAllText(order.OrderPath, good.Replace("unit\tC06\t", "unit\t"), new UTF8Encoding(false));
            Assert.That(PackageRunOrder.Read(order.OrderPath, out why), Is.Null);
            Assert.That(why, Does.Contain("a unit line holds 2 values where it holds 3"));

            File.WriteAllText(order.OrderPath, good.Replace("unit\tC06\t", "unit\t\t"), new UTF8Encoding(false));
            Assert.That(PackageRunOrder.Read(order.OrderPath, out why), Is.Null);
            Assert.That(why, Does.Contain("names its package and its NWF"));

            File.WriteAllText(order.OrderPath, good.Replace("unit\t", "package\t"), new UTF8Encoding(false));
            Assert.That(PackageRunOrder.Read(order.OrderPath, out why), Is.Null, "the package line of the shape before F222 is no longer read");
        }

        [Test]
        public void AGroupsOwnLogDeletesNoLogWhereTheKeptCountWould()
        {
            string logs = Path.Combine(root, "logs");
            Directory.CreateDirectory(logs);

            for (int i = 0; i < 40; i++)
            {
                File.WriteAllText(Path.Combine(logs, "run-20261018-10" + i.ToString("0000") + ".log"), "x");
            }

            using (RunLog log = RunLog.StartOrDisabled(logs, new DateTime(2026, 10, 18, 12, 0, 0), RunLog.KeepEveryLog))
            {
                Assert.That(log.IsWritingToDisk, Is.True);
            }

            Assert.That(Directory.GetFiles(logs, "run-*.log").Length, Is.EqualTo(41), "the 40 before and its own");

            using (RunLog.StartOrDisabled(logs, new DateTime(2026, 10, 18, 12, 0, 1), 30))
            {
            }

            Assert.That(Directory.GetFiles(logs, "run-*.log").Length, Is.EqualTo(30), "a run a person starts still keeps its 30");
        }

        [Test]
        public void TheRunsTotalsSayTheyAreALowerBoundWhereAGroupHasNoEnd()
        {
            foreach (string community in new[] { "C06", "C07" })
            {
                string folder = Path.Combine(root, "tree", community);
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc"), "nwc");
            }

            PackageTree tree = PackageTree.Read(Path.Combine(root, "tree"), ".nwc");
            PackageTally tally = new PackageTally();
            tally.Add(tree.Find("C06"), "1B06BC", GroupOutcome.Done, 10);
            tally.Add(tree.Find("C07"), "1C07BC", GroupOutcome.Done, 5);
            tally.NoResult(tree.Find("C07"), "1C07K1", "the file is not there");
            IList<string> lines = tally.Lines();

            Assert.That(lines.Single(l => l.StartsWith("run ")), Does.EndWith("15 clashes, and 1 group with no end to count, each named above, so the clashes are at least that"));
            Assert.That(lines.Single(l => l.StartsWith("community      C07")), Does.Contain("so the clashes are at least that"));
            Assert.That(lines.Single(l => l.StartsWith("community      C06")), Does.EndWith("10 clashes"), "a community whose every group ended reads exact");
        }

        [Test]
        public void TheRunnersLastLineAddsUpEveryStartAndEveryQuit()
        {
            string line = PackageRunWords.RunnerEnded(45, 3, 1320.46, 45, 210.33, 44, 95.07, 1);

            Assert.That(line, Is.EqualTo("the runner ended after 45 groups of 3 packages in 1320.5 s, 45 starts took 210.3 s in all and 44 quits 95.1 s, "
                + "and 1 Navisworks still there past the wait was ended by the runner"), "a quit the runner had to end is said beside the quits");
            Assert.That(PackageRunWords.RunnerEnded(2, 1, 60, 2, 8, 2, 4, 0), Does.EndWith("2 quits 4.0 s"));
            Assert.That(PackageRunWords.RunnerStarted(45, 3, "order.txt"), Does.StartWith("the runner started for 45 groups of 3 packages, each group in a fresh Navisworks"));
            Assert.That(PackageRunWords.ChildStarted(2, 45, "C06  1B06BC", null), Does.StartWith("this Navisworks runs group 3 of 45, C06  1B06BC,"));
            Assert.That(PackageRunWords.NotTheGroup("C06  1B06BC", "a.nwf"), Does.Contain("a.nwf").And.Contain("nothing runs"));
            Assert.That(PackageRunWords.Waiting(45), Does.StartWith("45 groups run one after the other"));
        }

        private string Report
        {
            get { return Path.Combine(root, "Report"); }
        }

        /// <summary>A package of three groups, 1B06BC, 1B06BS and 1B06KI, its plan made as the NWC page makes it.</summary>
        private PackagePlan Plan(out Package package)
        {
            foreach (string name in new[] { "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc", "1104-PAR-1B06BS-ZZZ-EL-MOD-000001.nwc", "1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc" })
            {
                string path = Path.Combine(root, "Parent", "C06", name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, "nwc");
            }

            package = PackageTree.Read(Path.Combine(root, "Parent"), ".nwc").Packages()[0];
            return PackagePlan.For(package, Report, GroupingModes.Default, new ContainerNameSettings(), new OutputNaming(),
                new GenericModelsSettings(), new DateTime(2026, 10, 18));
        }

        [Test]
        public void OnTheNwcPageEveryGroupOfThePlanStandsWithItsFactsOrWhyNot()
        {
            Package package;
            PackagePlan plan = Plan(out package);
            Assert.That(plan.Groups.Count, Is.EqualTo(3));
            PackageGroup bc = plan.Groups.First(g => g.Building == "1B06BC");
            PackageGroup bs = plan.Groups.First(g => g.Building == "1B06BS");

            ModelFacts model = new ModelFacts(bc.Files[0]).WithBox(0, 0, 0, 1, 1, 1);
            PackageRunResult ran = new PackageRunResult(package.RelativePath, null);
            ran.Groups.Add(new PackageRunGroup("1B06BC", GroupOutcome.Done, 3, null).WithFacts(new[] { model }));

            List<PackageRunUnit> units = new List<PackageRunUnit>
            {
                new PackageRunUnit(package.RelativePath, bc.NwfPath.ToUpperInvariant(), "1B06BC"),
                new PackageRunUnit(package.RelativePath, bs.NwfPath, "1B06BS")
            };

            IList<DisciplineGroup> groups = PackageRunFacts.GroupsOf(
                package, plan, units, new List<PackageRunResult> { ran, null }, new List<string> { null, "the file is not there" });

            Assert.That(groups.Count, Is.EqualTo(3), "every group of the plan, the one left out among them");
            DisciplineGroup first = groups.First(g => g.Name == "1B06BC");
            Assert.That(first.Models.Count, Is.EqualTo(1), "the NWF is matched without case, as Windows reads a path");
            Assert.That(first.Files.Count, Is.EqualTo(1));

            DisciplineGroup second = groups.First(g => g.Name == "1B06BS");
            Assert.That(second.Models, Is.Null);
            Assert.That(second.WhyNoModels, Is.EqualTo("its Navisworks wrote no result, the file is not there"));
            Assert.That(second.Files.Count, Is.EqualTo(1), "a group with no facts keeps its files, so the naming and the dates still run on it");

            DisciplineGroup third = groups.First(g => g.Name == "1B06KI");
            Assert.That(third.Models, Is.Null);
            Assert.That(third.WhyNoModels, Is.EqualTo(PackageRunFacts.DidNotRun));
        }

        [Test]
        public void OnTheNwfPageEachGroupOfThePackageStandsByItsResult()
        {
            Package package;
            Plan(out package);

            PackageRunResult withFacts = new PackageRunResult(package.RelativePath, null);
            withFacts.Groups.Add(new PackageRunGroup("1104-PAR-1B06BC-ZZZ-BM-MOD-000001", GroupOutcome.Done, 3, null)
                .WithFacts(new[] { new ModelFacts("a.nwc"), new ModelFacts("b.nwc") }));
            PackageRunResult noFacts = new PackageRunResult(package.RelativePath, null);
            noFacts.Groups.Add(new PackageRunGroup("1104-PAR-1B06BS-ZZZ-BM-MOD-000001", GroupOutcome.Failed, -1, "x").FactsNot("it threw"));
            PackageRunResult ranNothing = new PackageRunResult(package.RelativePath, null) { NoGroupBecause = "the NWF is gone" };
            PackageRunResult otherPackage = new PackageRunResult("C07", null);

            List<PackageRunUnit> units = new List<PackageRunUnit>
            {
                new PackageRunUnit(package.RelativePath, "a.nwf", "a"),
                new PackageRunUnit(package.RelativePath, "b.nwf", "b"),
                new PackageRunUnit(package.RelativePath, "c.nwf", "c"),
                new PackageRunUnit("C07", "d.nwf", "d")
            };

            IList<DisciplineGroup> groups = PackageRunFacts.GroupsOf(
                package, null, units, new List<PackageRunResult> { withFacts, noFacts, ranNothing, otherPackage },
                new List<string> { null, null, null, null });

            Assert.That(groups.Count, Is.EqualTo(3), "the groups of this package and no other");
            Assert.That(groups[0].Files.Count, Is.EqualTo(2), "the files of a group read whole are its models'");
            Assert.That(groups[0].Models.Count, Is.EqualTo(2));
            Assert.That(groups[1].Files.Count, Is.EqualTo(0), "an NWF whose facts were not read names no file, which the missing discipline check reads as not read");
            Assert.That(groups[1].WhyNoModels, Is.EqualTo("it threw"));
            Assert.That(groups[2].Name, Is.EqualTo("c"));
            Assert.That(groups[2].WhyNoModels, Is.EqualTo("its own Navisworks ran nothing, the NWF is gone"));
            Assert.Throws<ArgumentException>(() => PackageRunFacts.GroupsOf(package, null, units, new List<PackageRunResult>(), new List<string>()));
        }
    }
}
