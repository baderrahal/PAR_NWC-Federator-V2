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
        public void TheCommunityIsThePackagesFirstFolder()
        {
            Assert.That(OneDisciplineGroups.CommunityOf("C06"), Is.EqualTo("C06"));
            Assert.That(OneDisciplineGroups.CommunityOf("C06" + Path.DirectorySeparatorChar + "Infra"), Is.EqualTo("C06"));
            Assert.That(OneDisciplineGroups.CommunityOf(null), Is.EqualTo(string.Empty));
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
            Assert.That(lines, Has.Member("group          " + "C06" + Path.DirectorySeparatorChar + "Infra  1B06PG  NOT BUILT, one discipline, not built"));
            Assert.That(lines, Has.Member("not built      C06 : 1 group not built for one discipline"));
            Assert.That(lines, Has.Member("not built      C07 : 1 group not built for one discipline"));
            Assert.That(OneDisciplineGroups.ResultLine("C06", 2), Is.EqualTo("not built      C06 : 2 groups not built for one discipline"));
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
