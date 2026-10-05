using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132, Bader's answer D to Q133. Both tests of a mirrored pair are run, then their
    /// clashes are merged by the pair of items: a clash both find is kept once, under the
    /// kept test, and a clash only the mirror finds is added to the kept test and named as
    /// found by the mirror only. The report and every count hold each clash once. The item
    /// keys and names in here are sample data. The shape of the first test is probe P1's
    /// measurement, 25 clashes on the test and 27 on its swap, all 25 among them,
    /// docs\history\scan.md 5z-k on the branch fix-F114-probes.
    /// </summary>
    [TestFixture]
    public class MirrorMergeTests
    {
        private static MirrorMerge TheMerge()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts));

            IList<MirrorMerge> merges = MirrorMerge.Of(MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked()));

            Assert.That(merges.Count, Is.EqualTo(1));
            return merges[0];
        }

        private static string Kept
        {
            get { return MirrorRuleTests.DuctsVsColumns; }
        }

        private static string Mirror
        {
            get { return MirrorRuleTests.ColumnsVsDucts + " (mirror)"; }
        }

        private static ClashRow Row(string name, ClashStatus status, string left, string right)
        {
            ClashRow row = new ClashRow();
            row.Name = name;
            row.Status = status;
            row.Left.Name = left;
            row.Right.Name = right;
            return row;
        }

        /// <summary>The kept test's report with that many clashes, each New, item i against item 100 + i.</summary>
        private static TestReport KeptReport(MirrorMerge merge, int clashes)
        {
            TestReport report = new TestReport(1, Kept);

            for (int i = 1; i <= clashes; i++)
            {
                report.Add(Row("Clash" + i, ClashStatus.New, "duct " + i, "column " + i));
                merge.KeptFound("item " + i, "item " + (100 + i));
            }

            return report;
        }

        private static string Text(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        // ---------- the merge, each clash once ----------

        [Test]
        public void TheSwapOfProbeP1AddsItsTwoClashesToTheKeptTest()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 25);

            for (int i = 1; i <= 25; i++)
            {
                merge.MirrorFound(merge.Pairs[0], "item " + (100 + i), "item " + i, Row("Clash" + i, ClashStatus.New, "column " + i, "duct " + i));
            }

            merge.MirrorFound(merge.Pairs[0], "item 201", "item 301", Row("Clash26", ClashStatus.New, "column 26", "duct 26"));
            merge.MirrorFound(merge.Pairs[0], "item 202", "item 302", Row("Clash27", ClashStatus.New, "column 27", "duct 27"));
            merge.AddTo(kept);

            Assert.That(merge.FoundByBoth, Is.EqualTo(25));
            Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(2));
            Assert.That(kept.Rows.Count, Is.EqualTo(27));
            Assert.That(kept.RawClashes, Is.EqualTo(27));
            Assert.That(kept.Tally.Total, Is.EqualTo(27));
            Assert.That(kept.Rows[25].Name, Is.EqualTo("Clash26"));
            Assert.That(kept.Rows[26].Name, Is.EqualTo("Clash27"));
        }

        [Test]
        public void OnlyTheClashesTheMirrorAloneFoundAreMarked()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 2);

            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(kept);

            Assert.That(kept.Rows[0].FoundOnlyByMirror, Is.Empty);
            Assert.That(kept.Rows[1].FoundOnlyByMirror, Is.Empty);
            Assert.That(kept.Rows[2].FoundOnlyByMirror, Is.EqualTo(Mirror));
        }

        // The pair of items is unordered: the swap holds each pair the other way round,
        // P1's point 2, and it is the same clash.
        [Test]
        public void ThePairOfItemsIsTheSameEitherWayRound()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            merge.AddTo(kept);

            Assert.That(merge.FoundByBoth, Is.EqualTo(1));
            Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(0));
            Assert.That(kept.Rows.Count, Is.EqualTo(1));
        }

        [Test]
        public void AClashTheMirrorGivesTwiceIsAddedOnce()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.MirrorFound(merge.Pairs[0], "item 8", "item 7", Row("Clash3", ClashStatus.New, "duct 8", "column 7"));
            merge.AddTo(kept);

            Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(1));
            Assert.That(kept.Rows.Count, Is.EqualTo(2));
        }

        // A clash only the mirror found keeps the status it carries, and every count by
        // status is read off the merged list.
        [Test]
        public void EveryCountByStatusIsReadOffTheMergedList()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 3);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash4", ClashStatus.Active, "column 7", "duct 8"));
            merge.AddTo(kept);

            Assert.That(kept.Tally.Of(ClashStatus.New), Is.EqualTo(3));
            Assert.That(kept.Tally.Of(ClashStatus.Active), Is.EqualTo(1));
            Assert.That(kept.Tally.Total, Is.EqualTo(4));
        }

        // An item nobody read cannot be compared. Added, it could be a clash the kept test
        // holds counted twice, so it is not added, and the log says UNKNOWN.
        [Test]
        public void AMirrorClashWithAnItemNotReadIsNotAdded()
        {
            foreach (string unread in new[] { null, string.Empty, TestSettings.UnknownLocator })
            {
                MirrorMerge merge = TheMerge();
                TestReport kept = KeptReport(merge, 1);

                merge.MirrorFound(merge.Pairs[0], unread, "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
                merge.AddTo(kept);

                Assert.That(merge.NotCompared, Is.EqualTo(1), "read as \"" + unread + "\"");
                Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(0));
                Assert.That(kept.Rows.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void AGroupIsRefusedBecauseEachClashUnderItIsHanded()
        {
            MirrorMerge merge = TheMerge();

            Assert.Throws<ArgumentException>(() => merge.MirrorFound(merge.Pairs[0], "item 1", "item 2", ClashRow.ForGroup(3)));
        }

        [Test]
        public void AddingToAnotherTestIsRefused()
        {
            MirrorMerge merge = TheMerge();

            Assert.Throws<ArgumentException>(() => merge.AddTo(new TestReport(1, MirrorRuleTests.ColumnsVsDucts)));
        }

        // Added twice, every clash only the mirror found would count twice.
        [Test]
        public void AddingTwiceIsRefused()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(kept);

            Assert.Throws<InvalidOperationException>(() => merge.AddTo(kept));
            Assert.That(kept.Rows.Count, Is.EqualTo(2));
        }

        [Test]
        public void AClashHandedAfterTheMergeIsRefused()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 1);

            merge.AddTo(kept);

            Assert.Throws<InvalidOperationException>(() => merge.KeptFound("item 7", "item 8"));
            Assert.Throws<InvalidOperationException>(
                () => merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8")));
        }

        // ---------- a test kept over two mirrors, Q121 B ----------

        private const string Telecom = MirrorRuleTests.Telecom;
        private const string Telephone = MirrorRuleTests.Telephone;
        private const string KeptOfTwo = "BLD-EL-Telecom Fixtures-vs-BLD-AR-Walls";
        private const string FirstMirror = "BLD-EL-Telephone Devices-vs-BLD-AR-Walls";
        private const string SecondMirror = "BLD-AR-Walls-vs-BLD-EL-Telecom Fixtures";

        private static MirrorMerge TheMergeOfTwoMirrors()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(KeptOfTwo, Telecom, MirrorRuleTests.Walls),
                MirrorRuleTests.Test(FirstMirror, Telephone, MirrorRuleTests.Walls),
                MirrorRuleTests.Test(SecondMirror, MirrorRuleTests.Walls, Telecom));

            IList<MirrorMerge> merges = MirrorMerge.Of(MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked()));

            Assert.That(merges.Count, Is.EqualTo(1), "one merge for the one test kept");
            Assert.That(merges[0].Pairs.Count, Is.EqualTo(2));
            return merges[0];
        }

        // Two merges would each add a clash both mirrors found and the kept test did not.
        [Test]
        public void AClashTwoMirrorsFoundAndTheKeptTestDidNotIsAddedOnce()
        {
            MirrorMerge merge = TheMergeOfTwoMirrors();
            TestReport kept = new TestReport(1, KeptOfTwo);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash1", ClashStatus.New, "phone 7", "wall 8"));
            merge.MirrorFound(merge.Pairs[1], "item 8", "item 7", Row("Clash1", ClashStatus.New, "wall 8", "phone 7"));
            merge.AddTo(kept);

            Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(1));
            Assert.That(kept.Rows.Count, Is.EqualTo(1));
            Assert.That(kept.Rows[0].FoundOnlyByMirror, Is.EqualTo(FirstMirror + " (mirror)"));
            Assert.That(merge.Lines()[2], Is.EqualTo(MirrorRule.Prefix + "   " + KeptOfTwo + " and its mirror "
                + SecondMirror + " (mirror): " + KeptOfTwo + " found 0, the mirror 1, 0 by both and 0 by the mirror only, "
                + "added to " + KeptOfTwo + ", and 1 found by an earlier mirror of " + KeptOfTwo + " as well, added once"));
        }

        [Test]
        public void EachTestKeptHasAMergeOfItsOwn()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(KeptOfTwo, Telecom, MirrorRuleTests.Walls),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts),
                MirrorRuleTests.Test(FirstMirror, Telephone, MirrorRuleTests.Walls));

            IList<MirrorMerge> merges = MirrorMerge.Of(MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked()));

            Assert.That(merges.Count, Is.EqualTo(2));
            Assert.That(merges[0].Kept.Name, Is.EqualTo(MirrorRuleTests.DuctsVsColumns));
            Assert.That(merges[1].Kept.Name, Is.EqualTo(KeptOfTwo));
            Assert.Throws<ArgumentException>(() => merges[0].MirrorFound(
                merges[1].Pairs[0], "item 1", "item 2", Row("Clash1", ClashStatus.New, "phone 1", "wall 2")));
        }

        // ---------- the lines for the log ----------

        [Test]
        public void TheLineCountsWhatEachFoundAndWhatTheMergeHolds()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 2);

            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(kept);

            IList<string> lines = merge.Lines();

            Assert.That(lines[0], Is.EqualTo(MirrorRule.Prefix + "   " + Kept + " and its mirror " + Mirror
                + ": " + Kept + " found 2, the mirror 2, 1 by both and 1 by the mirror only, added to " + Kept));
            Assert.That(lines[lines.Count - 1], Is.EqualTo(MirrorRule.Prefix + "   the report holds 3 under " + Kept));
        }

        [Test]
        public void EachClashTheMirrorAloneFoundIsNamed()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.MirrorFound(merge.Pairs[0], "item 9", "item 10", Row("Clash3", ClashStatus.New, "column 9", "duct 10"));
            merge.AddTo(kept);

            IList<string> lines = merge.Lines();

            Assert.That(lines, Does.Contain(MirrorRule.Prefix + "   found by the mirror only, added to " + Kept
                + ": Clash2 of " + Mirror + ", column 7 against duct 8"));
            Assert.That(lines, Does.Contain(MirrorRule.Prefix + "   found by the mirror only, added to " + Kept
                + ": Clash3 of " + Mirror + ", column 9 against duct 10"));
        }

        [Test]
        public void AMirrorThatFoundNothingMoreNamesNoClash()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            merge.AddTo(kept);

            IList<string> lines = merge.Lines();

            Assert.That(lines.Count, Is.EqualTo(2), "the counts and what the report holds");
            Assert.That(lines[0], Does.Contain("1 by both and 0 by the mirror only"));
            Assert.That(lines[1], Is.EqualTo(MirrorRule.Prefix + "   the report holds 1 under " + Kept));
        }

        [Test]
        public void AnItemNotReadOnEitherTestIsSaidUnknown()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 1);

            merge.KeptFound(TestSettings.UnknownLocator, "item 5");
            merge.MirrorFound(merge.Pairs[0], null, "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(kept);

            string all = Text(merge.Lines());

            Assert.That(all, Does.Contain(MirrorRule.Prefix + "   1 clash of " + Mirror
                + " has an item that was not read, so whether " + Kept + " found it is UNKNOWN and it is not added"));
            Assert.That(all, Does.Contain(MirrorRule.Prefix + "   1 clash of " + Kept
                + " has an item that was not read, so up to 1 of the clashes found by a mirror only may be "
                + Kept + "'s own as well, UNKNOWN"));
        }

        [Test]
        public void ARepeatOfTheMirrorIsSaidOnce()
        {
            MirrorMerge merge = TheMerge();
            TestReport kept = KeptReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.MirrorFound(merge.Pairs[0], "item 8", "item 7", Row("Clash3", ClashStatus.New, "duct 8", "column 7"));
            merge.AddTo(kept);

            Assert.That(Text(merge.Lines()), Does.Contain(MirrorRule.Prefix + "   1 clash of " + Mirror
                + " repeats a pair of items it already gave, so it is counted once"));
        }
    }
}
