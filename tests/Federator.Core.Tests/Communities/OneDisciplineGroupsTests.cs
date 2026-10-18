using System.IO;
using System.Linq;
using Federator.Core.Communities;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F233, Bader's round of 19 Oct 2026, fix 3. A group of one discipline is not built: one log line naming its community,
    /// building, discipline and files, one RESULT line per community with the count, and an NWF of an earlier run named.
    /// </summary>
    [TestFixture]
    public class OneDisciplineGroupsTests
    {
        private string root;
        private PackageTree tree;

        [SetUp]
        public void MakeTree()
        {
            root = TempFolder.Make("onediscipline");
            Make("C06", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            Make("C06", "Infra", "1104-PAR-1B06PG-ZZZ-EL-MOD-000001.nwc");
            Make("C07", "1104-PAR-1C07BS-ZZZ-EL-MOD-000001.nwc");
            tree = PackageTree.Read(root, ".nwc");
        }

        [TearDown]
        public void RemoveTree()
        {
            TempFolder.Remove(root);
        }

        private void Make(params string[] parts)
        {
            string path = Path.Combine(new[] { root }.Concat(parts).ToArray());
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, "nwc");
        }

        [Test]
        public void TheLineNamesTheCommunityTheBuildingTheDisciplineAndTheFiles()
        {
            string line = OneDisciplineGroups.Line("C06", "1B06PG", "EL",
                new[] { Path.Combine(root, "C06", "1104-PAR-1B06PG-ZZZ-EL-MOD-000001.nwc"), Path.Combine(root, "C06", "1104-PAR-1B06PG-ZZZ-EL-MOD-000002.nwc") });

            Assert.That(line, Is.EqualTo("NOT BUILT  C06  1B06PG  EL  1104-PAR-1B06PG-ZZZ-EL-MOD-000001.nwc, 1104-PAR-1B06PG-ZZZ-EL-MOD-000002.nwc  one discipline, not built"));
            Assert.That(OneDisciplineGroups.IsTheLine(line), Is.True);
            Assert.That(OneDisciplineGroups.Line(null, "1B06PG", null, null), Is.EqualTo("NOT BUILT  UNKNOWN  1B06PG  UNKNOWN  UNKNOWN files  one discipline, not built"));
        }

        [Test]
        public void AnotherReasonIsNeverReadAsTheLine()
        {
            Assert.That(OneDisciplineGroups.IsTheLine("the package is not in the tree read off X"), Is.False);
            Assert.That(OneDisciplineGroups.IsTheLine("NOT BUILT  the NWF X of an earlier run is there, and it is not deleted and not run"), Is.False);
            Assert.That(OneDisciplineGroups.IsTheLine(null), Is.False);
        }

        [Test]
        public void AnEarlierNwfIsNamedAndNeverDeletedOrRun()
        {
            Assert.That(OneDisciplineGroups.EarlierNwfLine(@"C:\Report\C06\NWF\a.nwf"),
                Is.EqualTo(@"NOT BUILT  the NWF C:\Report\C06\NWF\a.nwf of an earlier run is there, and it is not deleted and not run"));
        }

        [Test]
        public void ANameNoFileSystemWouldTakeIsReadWholeAndNeverAThrow()
        {
            string line = OneDisciplineGroups.Line("C06", "1B06PG", "EL", new[] { @"C:\a\b<|>.nwc", "x/y.nwc", "plain.nwc" });

            Assert.That(line, Is.EqualTo("NOT BUILT  C06  1B06PG  EL  b<|>.nwc, y.nwc, plain.nwc  one discipline, not built"));
        }

        [Test]
        public void TheWordsAreTheGridsStatusWord()
        {
            Assert.That(OneDisciplineGroups.Words, Is.EqualTo("one discipline, not built"));
            Assert.That(OneDisciplineGroups.Words.Substring(1), Is.EqualTo(Federator.Core.Rerun.NwcPageRule.OneDiscipline.Substring(1)));
        }

        [Test]
        public void ResultCountsTheGroupsNotBuiltPerCommunityAndNeverAmongTheDone()
        {
            PackageTally tally = new PackageTally();
            tally.Add(tree.Find("C06"), "1B06BC", GroupOutcome.Done, 3);
            tally.NotBuilt(tree.Find("C06" + Path.DirectorySeparatorChar + "Infra"), "1B06PG");
            tally.NotBuilt(tree.Find("C07"), "1C07BS");

            string[] lines = tally.Lines().ToArray();

            Assert.That(tally.CountOf(GroupOutcome.Done), Is.EqualTo(1));
            Assert.That(tally.NotBuiltCount, Is.EqualTo(2));
            Assert.That(tally.NotRunCount, Is.EqualTo(0), "never a group that ran nothing");
            Assert.That(lines, Has.Member("group          " + "C06" + Path.DirectorySeparatorChar + "Infra  1B06PG  one discipline, not built"));
            Assert.That(lines, Has.Member("not built      C06 : 1 group not built for one discipline"));
            Assert.That(lines, Has.Member("not built      C07 : 1 group not built for one discipline"));
            Assert.That(OneDisciplineGroups.ResultLine("C06", 2), Is.EqualTo("not built      C06 : 2 groups not built for one discipline"));
        }

        [Test]
        public void TheJoinedResultWritesANotBuiltGroupsLineAndNeverSaysItRanNothing()
        {
            string line = OneDisciplineGroups.Line("C07", "1C07BS", "EL", new[] { "1104-PAR-1C07BS-ZZZ-EL-MOD-000001.nwc" });
            PackageRunResult c07 = new PackageRunResult("C07", null) { NoGroupBecause = line };
            PackageTally tally = new PackageTally();
            tally.NotBuilt(tree.Find("C07"), "1C07BS");

            System.Collections.Generic.IList<string> lines = PackageRunWords.JoinedResult(
                new[] { new PackageRunUnit("C07", TestPaths.At("Report", "C07", "NWF", "b.nwf"), "1C07BS") },
                new[] { c07 },
                new string[] { null },
                tally);

            Assert.That(lines, Has.Member("    " + line));
            Assert.That(lines.Any(l => l.Contains("it ran nothing")), Is.False);
            Assert.That(lines, Has.Member("not built      C07 : 1 group not built for one discipline"));
            Assert.That(lines, Has.Member("one discipline : 1 group not built, the counts above leave them out, each named below"));
            Assert.That(lines.Count(l => l.StartsWith("not built")), Is.EqualTo(1), "the count line is never read as a second community line");
        }

        [Test]
        public void AJoinedResultWithNoGroupNotBuiltCarriesNoSuchCount()
        {
            PackageRunResult c06 = new PackageRunResult("C06", null) { NoGroupBecause = "the package is not in the tree" };
            PackageTally tally = new PackageTally();
            tally.NotRun(tree.Find("C06"), "1B06BC", "the package is not in the tree");

            System.Collections.Generic.IList<string> lines = PackageRunWords.JoinedResult(
                new[] { new PackageRunUnit("C06", TestPaths.At("Report", "C06", "NWF", "a.nwf"), "1B06BC") },
                new[] { c06 },
                new string[] { null },
                tally);

            Assert.That(lines.Any(l => l.StartsWith("one discipline")), Is.False);
            Assert.That(lines, Has.Member("    it ran nothing, the package is not in the tree"));
        }

        [Test]
        public void AnNwfLinkingAFileNotOnTheDiskIsNotJudged()
        {
            Assert.That(OneDisciplineGroups.NotJudgedLine("C06/1B06PG", 1),
                Is.EqualTo("NOT JUDGED  C06/1B06PG  1 file it links is not on the disk, so whether it holds one discipline is UNKNOWN and it is not judged as one"));
            Assert.That(OneDisciplineGroups.NotJudgedLine(null, 2), Does.Contain("UNKNOWN  2 files it links are"));
            Assert.That(OneDisciplineGroups.IsTheLine(OneDisciplineGroups.NotJudgedLine("x", 1)), Is.False);
        }

        [Test]
        public void ACommunityWithNoneHasNoLine()
        {
            PackageTally tally = new PackageTally();
            tally.Add(tree.Find("C06"), "1B06BC", GroupOutcome.Done, 3);

            Assert.That(tally.Lines().Any(line => line.StartsWith("not built")), Is.False);
        }
    }
}
