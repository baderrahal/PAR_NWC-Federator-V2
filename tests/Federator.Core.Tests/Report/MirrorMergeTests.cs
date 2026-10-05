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
    /// found by the mirror only. The report and every count hold each clash once, and the
    /// mirror's own results are not reported a second time. The item keys and names in here
    /// are sample data. The shape of the first test is probe P1's measurement, 25 clashes on
    /// the test and 27 on its swap, all 25 among them, docs\history\scan.md 5z-k on the branch
    /// fix-F114-probes.
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
            get { return MirrorRuleTests.DuctsVsColumns + " (mirror)"; }
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

        /// <summary>
        /// The group's report as the run leaves it before the merge: the kept test with that
        /// many clashes, each New, item i against item 100 + i, each handed to the merge, and
        /// one test of each mirror under the name it ran under, run, holding nothing yet.
        /// </summary>
        private static ClashReport TheReport(MirrorMerge merge, int keptClashes)
        {
            ClashReport report = new ClashReport("1A02MM", "a report");
            TestReport kept = report.AddTest(merge.Kept.Name);
            kept.State = keptClashes > 0 ? TestState.FoundClashes : TestState.Passed;

            for (int i = 1; i <= keptClashes; i++)
            {
                kept.Add(Row("Clash" + i, ClashStatus.New, "duct " + i, "column " + i));
                merge.KeptFound("item " + i, "item " + (100 + i), ClashStatus.New);
            }

            foreach (MirrorPair pair in merge.Pairs)
            {
                report.AddTest(pair.MirrorName).State = TestState.FoundClashes;
            }

            return report;
        }

        private static TestReport Named(ClashReport report, string name)
        {
            foreach (TestReport test in report.Tests)
            {
                if (test.Name == name)
                {
                    return test;
                }
            }

            return null;
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
            ClashReport report = TheReport(merge, 25);
            TestReport kept = Named(report, Kept);

            for (int i = 1; i <= 25; i++)
            {
                merge.MirrorFound(merge.Pairs[0], "item " + (100 + i), "item " + i, Row("Clash" + i, ClashStatus.New, "column " + i, "duct " + i));
            }

            merge.MirrorFound(merge.Pairs[0], "item 201", "item 301", Row("Clash26", ClashStatus.New, "column 26", "duct 26"));
            merge.MirrorFound(merge.Pairs[0], "item 202", "item 302", Row("Clash27", ClashStatus.New, "column 27", "duct 27"));
            merge.AddTo(report);

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
            ClashReport report = TheReport(merge, 2);
            TestReport kept = Named(report, Kept);

            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

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
            ClashReport report = TheReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            merge.AddTo(report);

            Assert.That(merge.FoundByBoth, Is.EqualTo(1));
            Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(0));
            Assert.That(Named(report, Kept).Rows.Count, Is.EqualTo(1));
        }

        [Test]
        public void AClashTheMirrorGivesTwiceIsAddedOnce()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.MirrorFound(merge.Pairs[0], "item 8", "item 7", Row("Clash3", ClashStatus.New, "duct 8", "column 7"));
            merge.AddTo(report);

            Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(1));
            Assert.That(Named(report, Kept).Rows.Count, Is.EqualTo(2));
        }

        // A clash only the mirror found keeps the status it carries, and every count by
        // status is read off the merged list.
        [Test]
        public void EveryCountByStatusIsReadOffTheMergedList()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 3);
            TestReport kept = Named(report, Kept);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash4", ClashStatus.Active, "column 7", "duct 8"));
            merge.AddTo(report);

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
                ClashReport report = TheReport(merge, 1);

                merge.MirrorFound(merge.Pairs[0], unread, "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
                merge.AddTo(report);

                Assert.That(merge.NotCompared, Is.EqualTo(1), "read as \"" + unread + "\"");
                Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(0));
                Assert.That(Named(report, Kept).Rows.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void AGroupIsRefusedBecauseEachClashUnderItIsHanded()
        {
            MirrorMerge merge = TheMerge();

            Assert.Throws<ArgumentException>(() => merge.MirrorFound(merge.Pairs[0], "item 1", "item 2", ClashRow.ForGroup(3)));
        }

        // Added twice, every clash only the mirror found would count twice.
        [Test]
        public void AddingTwiceIsRefused()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

            Assert.Throws<InvalidOperationException>(() => merge.AddTo(report));
            Assert.That(Named(report, Kept).Rows.Count, Is.EqualTo(2));
        }

        [Test]
        public void AClashHandedAfterTheMergeIsRefused()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            merge.AddTo(report);

            Assert.Throws<InvalidOperationException>(() => merge.KeptFound("item 7", "item 8", ClashStatus.New));
            Assert.Throws<InvalidOperationException>(
                () => merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8")));
        }

        // ---------- the mirror's own results are not reported a second time, F132 attempt 5 item 2 ----------

        // The rule the reviewer found with no carrier in Core. The mirror's own test, rows and
        // all, is taken out of the group's report by the merge, so the totals, the order, the
        // test counts and the item ids every writer reads hold each clash once.
        [Test]
        public void TheMirrorsOwnResultsAreNotReportedASecondTime()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 2);
            TestReport mirror = Named(report, Mirror);

            mirror.Add(Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            mirror.Add(Row("Clash2", ClashStatus.New, "column 2", "duct 2"));
            mirror.Add(Row("Clash3", ClashStatus.New, "column 7", "duct 8"));
            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", mirror.Rows[0]);
            merge.MirrorFound(merge.Pairs[0], "item 102", "item 2", mirror.Rows[1]);
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", mirror.Rows[2]);

            Assert.That(report.Totals.Total, Is.EqualTo(5), "before the merge both tests hold their own");

            merge.AddTo(report);

            Assert.That(report.Tests.Count, Is.EqualTo(1));
            Assert.That(report.Tests[0].Name, Is.EqualTo(Kept));
            Assert.That(report.Totals.Total, Is.EqualTo(3));
            Assert.That(ReportOrder.Tests(report).Count, Is.EqualTo(1));
            Assert.That(report.CountOf(TestState.FoundClashes), Is.EqualTo(1));
            Assert.That(Text(merge.Lines()), Does.Contain(MirrorRule.Prefix + "   " + Mirror
                + " is taken out of the report, so its own results are not reported a second time"));
        }

        // The run the reviewer named, item 1: no XML, an NWF holding X and X (mirror), each
        // handed with the placeholders for its sides. Paired by the name ending, merged by
        // the pair of items, so each clash is counted once.
        [Test]
        public void ANoXmlRunOverXAndXMirrorCountsEachClashOnce()
        {
            ClashTestPlan plan = MirrorRuleTests.SavedPlan(Kept, Mirror);
            IList<MirrorMerge> merges = MirrorMerge.Of(
                MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings()));

            Assert.That(merges.Count, Is.EqualTo(1), "the pair is found by the name ending");

            MirrorMerge merge = merges[0];
            ClashReport report = TheReport(merge, 2);
            TestReport mirror = Named(report, Mirror);

            mirror.Add(Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            mirror.Add(Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", mirror.Rows[0]);
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", mirror.Rows[1]);
            merge.AddTo(report);

            Assert.That(report.Tests.Count, Is.EqualTo(1));
            Assert.That(report.Totals.Total, Is.EqualTo(3));
            Assert.That(merge.FoundByBoth, Is.EqualTo(1));
            Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(1));
        }

        // The reviewer's fifth finding. A kept test that found nothing holds clashes once its
        // mirror's are added, so it is a test that found clashes and is counted as one.
        [Test]
        public void AKeptTestThatFoundNothingFoundClashesOnceItsMirrorsAreAdded()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 0);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash1", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

            Assert.That(Named(report, Kept).State, Is.EqualTo(TestState.FoundClashes));
            Assert.That(report.CountOf(TestState.Passed), Is.EqualTo(0));
        }

        // The breaker's third finding. A kept test that did not run found nothing to compare,
        // so a clash of its mirror is not one only the mirror found. Nothing is merged, the
        // mirror is reported as its own test, and the line says why.
        [Test]
        public void AKeptTestThatDidNotRunMergesNothingAndSaysSo()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 0);

            Named(report, Kept).State = TestState.Skipped;
            Named(report, Mirror).Add(Row("Clash1", ClashStatus.New, "column 7", "duct 8"));
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Named(report, Mirror).Rows[0]);
            merge.AddTo(report);

            Assert.That(report.Tests.Count, Is.EqualTo(2));
            Assert.That(Named(report, Kept).Rows.Count, Is.EqualTo(0));
            Assert.That(Named(report, Mirror).Rows.Count, Is.EqualTo(1));
            Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(0));
            Assert.That(merge.Lines(), Does.Contain(MirrorRule.Prefix + "   " + Kept + " did not run, so nothing is "
                + "merged into it and each of its mirrors is reported as its own test"));
        }

        // A mirror that did not run found UNKNOWN and never 0, and stays in the report as a
        // test that did not run.
        [Test]
        public void AMirrorThatDidNotRunIsUnknownAndNeverZero()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            Named(report, Mirror).State = TestState.Skipped;
            merge.AddTo(report);

            string all = Text(merge.Lines());

            Assert.That(all, Does.Contain(MirrorRule.Prefix + "   " + Mirror + " did not run, so what it finds is "
                + "UNKNOWN and nothing of it is merged into " + Kept));
            Assert.That(all, Does.Not.Contain("the mirror 0"));
            Assert.That(report.Tests.Count, Is.EqualTo(2));
        }

        // Two tests of the kept test's name in the report: which one the clashes go to is
        // UNKNOWN, so nothing is merged and the line says so.
        [Test]
        public void AReportHoldingTheKeptTestOtherThanOnceMergesNothing()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            report.AddTest(Kept).State = TestState.Passed;
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

            Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(0));
            Assert.That(report.Tests.Count, Is.EqualTo(3));
            Assert.That(merge.Lines(), Does.Contain(MirrorRule.Prefix + "   " + Kept + " is in the report 2 times, "
                + "so which one its mirrors' clashes go to is UNKNOWN and nothing is merged"));
        }

        // The breaker's fifth finding. A person can set a status on the mirror's copy of a
        // clash both find. The report shows the kept test's, and the line says how many differ.
        [Test]
        public void AStatusThatDiffersOnAClashBothFoundIsSaid()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 2);

            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", Row("Clash1", ClashStatus.Approved, "column 1", "duct 1"));
            merge.MirrorFound(merge.Pairs[0], "item 102", "item 2", Row("Clash2", ClashStatus.New, "column 2", "duct 2"));
            merge.AddTo(report);

            Assert.That(Named(report, Kept).Tally.Of(ClashStatus.Approved), Is.EqualTo(0));
            Assert.That(merge.Lines(), Does.Contain(MirrorRule.Prefix + "   1 clash both found carries another status "
                + "under " + Mirror + " than under " + Kept + ", and the report shows the status under " + Kept));
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
            ClashReport report = TheReport(merge, 0);
            TestReport kept = Named(report, KeptOfTwo);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash1", ClashStatus.New, "phone 7", "wall 8"));
            merge.MirrorFound(merge.Pairs[1], "item 8", "item 7", Row("Clash1", ClashStatus.New, "wall 8", "phone 7"));
            merge.AddTo(report);

            Assert.That(merge.FoundByTheMirrorsOnly, Is.EqualTo(1));
            Assert.That(kept.Rows.Count, Is.EqualTo(1));
            Assert.That(kept.Rows[0].FoundOnlyByMirror, Is.EqualTo(KeptOfTwo + " (mirror)"));
            Assert.That(merge.Lines(), Does.Contain(MirrorRule.Prefix + "   " + KeptOfTwo + " and its mirror "
                + KeptOfTwo + " 2 (mirror): " + KeptOfTwo + " found 0, the mirror 1, 0 by both and 0 by the mirror only, "
                + "added to " + KeptOfTwo + ", and 1 found by an earlier mirror of " + KeptOfTwo + " as well, added once"));
            Assert.That(report.Tests.Count, Is.EqualTo(1), "both mirrors are taken out of the report");
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
            ClashReport report = TheReport(merge, 2);

            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

            IList<string> lines = merge.Lines();

            Assert.That(lines[0], Is.EqualTo(MirrorRule.Prefix + "   " + Kept + " and its mirror " + Mirror
                + ": " + Kept + " found 2, the mirror 2, 1 by both and 1 by the mirror only, added to " + Kept));
            Assert.That(lines[lines.Count - 1], Is.EqualTo(MirrorRule.Prefix + "   the report holds 3 under " + Kept));
        }

        [Test]
        public void EachClashTheMirrorAloneFoundIsNamed()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.MirrorFound(merge.Pairs[0], "item 9", "item 10", Row("Clash3", ClashStatus.New, "column 9", "duct 10"));
            merge.AddTo(report);

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
            ClashReport report = TheReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            merge.AddTo(report);

            IList<string> lines = merge.Lines();

            Assert.That(lines.Count, Is.EqualTo(3), "the counts, the mirror taken out and what the report holds");
            Assert.That(lines[0], Does.Contain("1 by both and 0 by the mirror only"));
            Assert.That(lines[2], Is.EqualTo(MirrorRule.Prefix + "   the report holds 1 under " + Kept));
        }

        [Test]
        public void AnItemNotReadOnEitherTestIsSaidUnknown()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            merge.KeptFound(TestSettings.UnknownLocator, "item 5", ClashStatus.New);
            merge.MirrorFound(merge.Pairs[0], null, "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

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
            ClashReport report = TheReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.MirrorFound(merge.Pairs[0], "item 8", "item 7", Row("Clash3", ClashStatus.New, "duct 8", "column 7"));
            merge.AddTo(report);

            Assert.That(Text(merge.Lines()), Does.Contain(MirrorRule.Prefix + "   1 clash of " + Mirror
                + " repeats a pair of items it already gave, so it is counted once"));
        }
    }
}
