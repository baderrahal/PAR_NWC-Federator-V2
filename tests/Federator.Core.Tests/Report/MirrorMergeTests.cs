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

        /// <summary>
        /// The group's report as the run leaves it before the merge: the kept test with that
        /// many clashes, each New, item i against item 100 + i, each handed to the merge, and
        /// one test of each mirror under the name it ran under, run, holding nothing yet.
        /// </summary>
        private static ClashReport TheReport(MirrorMerge merge, int keptClashes)
        {
            List<ClashStatus> statuses = new List<ClashStatus>();

            for (int i = 1; i <= keptClashes; i++)
            {
                statuses.Add(ClashStatus.New);
            }

            return TheReportWith(merge, statuses.ToArray());
        }

        /// <summary>
        /// The same report, its kept test's clashes carrying those statuses in that order,
        /// each handed with its status and its row, Bader's answer B to Q138.
        /// </summary>
        private static ClashReport TheReportWith(MirrorMerge merge, params ClashStatus[] keptStatuses)
        {
            ClashReport report = new ClashReport("1A02MM", "a report");
            TestReport kept = report.AddTest(merge.Kept.Name);
            kept.State = keptStatuses.Length > 0 ? TestState.FoundClashes : TestState.Passed;

            for (int i = 1; i <= keptStatuses.Length; i++)
            {
                ClashRow row = Row("Clash" + i, keptStatuses[i - 1], "duct " + i, "column " + i);

                kept.Add(row);
                merge.KeptFound("item " + i, "item " + (100 + i), row.Status, row);
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

        /// <summary>
        /// One clash of a mirror as the run leaves it: a row of the mirror's own report, and
        /// handed to the merge, which since attempt 8 merges a mirror only where the clashes
        /// handed are as many as its report holds.
        /// </summary>
        private static void Found(ClashReport report, MirrorMerge merge, int pair, string first, string second, ClashRow row)
        {
            Named(report, merge.Pairs[pair].MirrorName).Add(row);
            merge.MirrorFound(merge.Pairs[pair], first, second, row);
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
                Found(report, merge, 0, "item " + (100 + i), "item " + i, Row("Clash" + i, ClashStatus.New, "column " + i, "duct " + i));
            }

            Found(report, merge, 0, "item 201", "item 301", Row("Clash26", ClashStatus.New, "column 26", "duct 26"));
            Found(report, merge, 0, "item 202", "item 302", Row("Clash27", ClashStatus.New, "column 27", "duct 27"));
            merge.AddTo(report);

            Assert.That(merge.FoundByBoth, Is.EqualTo(25));
            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(2));
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

            Found(report, merge, 0, "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            Found(report, merge, 0, "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
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

            Found(report, merge, 0, "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            merge.AddTo(report);

            Assert.That(merge.FoundByBoth, Is.EqualTo(1));
            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(0));
            Assert.That(Named(report, Kept).Rows.Count, Is.EqualTo(1));
        }

        [Test]
        public void AClashTheMirrorGivesTwiceIsAddedOnce()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            Found(report, merge, 0, "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            Found(report, merge, 0, "item 8", "item 7", Row("Clash3", ClashStatus.New, "duct 8", "column 7"));
            merge.AddTo(report);

            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(1));
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

            Found(report, merge, 0, "item 7", "item 8", Row("Clash4", ClashStatus.Active, "column 7", "duct 8"));
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

                Found(report, merge, 0, unread, "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
                merge.AddTo(report);

                Assert.That(merge.NotCompared, Is.EqualTo(1), "read as \"" + unread + "\"");
                Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(0));
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

            Found(report, merge, 0, "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
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

            Assert.Throws<InvalidOperationException>(
                () => merge.KeptFound("item 7", "item 8", ClashStatus.New, Row("Clash2", ClashStatus.New, "duct 7", "column 8")));
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

        // The run the reviewer named on attempt 4: no XML, an NWF holding X and its mirror under
        // its own name with the ending, the sides of each read. Paired by the sides, merged by
        // the pair of items, so each clash is counted once.
        [Test]
        public void ANoXmlRunOverXAndXMirrorCountsEachClashOnce()
        {
            ClashTestPlan plan = MirrorRuleTests.SavedWithSides(
                Kept, MirrorRuleTests.Ducts, MirrorRuleTests.Columns, Mirror, MirrorRuleTests.Columns, MirrorRuleTests.Ducts);
            IList<MirrorMerge> merges = MirrorMerge.Of(
                MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings(), null));

            Assert.That(merges.Count, Is.EqualTo(1), "the pair is found by the sides of the test with the ending");

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
            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(1));
        }

        // The reviewer's fifth finding. A kept test that found nothing holds clashes once its
        // mirror's are added, so it is a test that found clashes and is counted as one.
        [Test]
        public void AKeptTestThatFoundNothingFoundClashesOnceItsMirrorsAreAdded()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 0);

            Found(report, merge, 0, "item 7", "item 8", Row("Clash1", ClashStatus.New, "column 7", "duct 8"));
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
            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(0));
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
            Found(report, merge, 0, "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(0));
            Assert.That(report.Tests.Count, Is.EqualTo(3));
            Assert.That(merge.Lines(), Does.Contain(MirrorRule.Prefix + "   " + Kept + " is in the report 2 times, "
                + "so which one its mirrors' clashes go to is UNKNOWN and nothing is merged"));
        }

        // ---------- the status of a clash both find, Bader's answer B to Q138 ----------

        // The breaker's fifth finding on attempt 5, and the first shape of Q138. A person set
        // Approved on the mirror's copy, the old test renamed under Q136 A keeping it, and the
        // kept test's copy reads New. A status a person sets wins, so the Approved reaches the
        // report's row and every count by status, and the clash is named with both statuses
        // and the one the report shows. Until Q138 the report showed New and the log gave a count.
        [Test]
        public void AStatusAPersonSetOnTheMirrorsCopyReachesTheReportAndIsNamed()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 2);
            TestReport kept = Named(report, Kept);

            Found(report, merge, 0, "item 101", "item 1", Row("Clash1", ClashStatus.Approved, "column 1", "duct 1"));
            Found(report, merge, 0, "item 102", "item 2", Row("Clash2", ClashStatus.New, "column 2", "duct 2"));
            merge.AddTo(report);

            Assert.That(kept.Rows[0].Status, Is.EqualTo(ClashStatus.Approved));
            Assert.That(kept.Rows[1].Status, Is.EqualTo(ClashStatus.New));
            Assert.That(kept.Tally.Of(ClashStatus.Approved), Is.EqualTo(1));
            Assert.That(kept.Tally.Of(ClashStatus.New), Is.EqualTo(1));
            Assert.That(kept.Tally.Total, Is.EqualTo(2));
            Assert.That(report.Totals.Of(ClashStatus.Approved), Is.EqualTo(1));

            IList<string> lines = merge.Lines();

            Assert.That(lines, Does.Contain(MirrorRule.Prefix + "   1 clash both found carries another status under "
                + Mirror + " than under " + Kept + ", each named with the status the report shows"));
            Assert.That(lines, Does.Contain(MirrorRule.Prefix + "   Clash1 of " + Kept + ", duct 1 against column 1, is "
                + "New in the report under " + Kept + " and Approved under " + Mirror + ", a status a person sets wins, so "
                + "the report shows Approved"));
            Assert.That(Text(lines), Does.Not.Contain("Clash2 of " + Kept));
        }

        // The second shape of Q138. Navisworks marked the kept test's copy Resolved, while the
        // mirror still finds the clash live. A live status wins over Resolved, so the workbook
        // does not show a live clash as Resolved.
        [Test]
        public void ALiveClashTheKeptTestMarkedResolvedReadsLiveInTheReport()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReportWith(merge, ClashStatus.Resolved);
            TestReport kept = Named(report, Kept);

            Found(report, merge, 0, "item 101", "item 1", Row("Clash1", ClashStatus.Active, "column 1", "duct 1"));
            merge.AddTo(report);

            Assert.That(kept.Rows[0].Status, Is.EqualTo(ClashStatus.Active));
            Assert.That(kept.Resolved, Is.EqualTo(0));
            Assert.That(kept.Tally.Of(ClashStatus.Active), Is.EqualTo(1));
            Assert.That(merge.Lines(), Does.Contain(MirrorRule.Prefix + "   Clash1 of " + Kept + ", duct 1 against "
                + "column 1, is Resolved in the report under " + Kept + " and Active under " + Mirror + ", a live status "
                + "wins over Resolved, so the report shows Active"));
        }

        // The kept test's status wins the other way round as well: a mirror's Resolved does not
        // hide the kept test's live clash.
        [Test]
        public void AResolvedCopyUnderTheMirrorLeavesTheKeptTestsLiveStatus()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReportWith(merge, ClashStatus.New);

            Found(report, merge, 0, "item 101", "item 1", Row("Clash1", ClashStatus.Resolved, "column 1", "duct 1"));
            merge.AddTo(report);

            Assert.That(Named(report, Kept).Rows[0].Status, Is.EqualTo(ClashStatus.New));
            Assert.That(Named(report, Kept).Tally.Of(ClashStatus.New), Is.EqualTo(1));
            Assert.That(merge.Lines(), Does.Contain(MirrorRule.Prefix + "   Clash1 of " + Kept + ", duct 1 against "
                + "column 1, is New in the report under " + Kept + " and Resolved under " + Mirror + ", a live status "
                + "wins over Resolved, so the report shows New"));
        }

        // The lead's note under Q138: where his words do not choose, both a person's or both
        // live, the kept test's status stays and the clash is named.
        [Test]
        public void WhereHisWordsDoNotChooseTheKeptTestsStatusStaysAndTheClashIsNamed()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReportWith(merge, ClashStatus.Reviewed, ClashStatus.New);
            TestReport kept = Named(report, Kept);

            Found(report, merge, 0, "item 101", "item 1", Row("Clash1", ClashStatus.Approved, "column 1", "duct 1"));
            Found(report, merge, 0, "item 102", "item 2", Row("Clash2", ClashStatus.Active, "column 2", "duct 2"));
            merge.AddTo(report);

            Assert.That(kept.Rows[0].Status, Is.EqualTo(ClashStatus.Reviewed));
            Assert.That(kept.Rows[1].Status, Is.EqualTo(ClashStatus.New));

            IList<string> lines = merge.Lines();

            Assert.That(lines, Does.Contain(MirrorRule.Prefix + "   2 clashes both found carry another status under "
                + Mirror + " than under " + Kept + ", each named with the status the report shows"));
            Assert.That(lines, Does.Contain(MirrorRule.Prefix + "   Clash1 of " + Kept + ", duct 1 against column 1, is "
                + "Reviewed in the report under " + Kept + " and Approved under " + Mirror + ", both are a person's and "
                + "the kept test's stays, so the report shows Reviewed"));
            Assert.That(lines, Does.Contain(MirrorRule.Prefix + "   Clash2 of " + Kept + ", duct 2 against column 2, is "
                + "New in the report under " + Kept + " and Active under " + Mirror + ", both are live and the kept "
                + "test's stays, so the report shows New"));
        }

        // A group is one row of the report standing for every clash under it, with the group's
        // status, so the status the rule gives one clash under it cannot reach the workbook
        // without changing the others. The group is left as it is and the line says so.
        [Test]
        public void AClashUnderAGroupOfTheKeptTestLeavesTheGroupAsItIsAndIsSaid()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 0);
            TestReport kept = Named(report, Kept);
            ClashRow group = ClashRow.ForGroup(2);

            group.Name = "Group1";
            group.Status = ClashStatus.New;
            kept.Add(group);
            kept.State = TestState.FoundClashes;
            merge.KeptFound("item 1", "item 101", ClashStatus.New, group);
            merge.KeptFound("item 2", "item 102", ClashStatus.New, group);
            Found(report, merge, 0, "item 101", "item 1", Row("Clash1", ClashStatus.Approved, "column 1", "duct 1"));
            merge.AddTo(report);

            Assert.That(group.Status, Is.EqualTo(ClashStatus.New));
            Assert.That(kept.Tally.Of(ClashStatus.New), Is.EqualTo(2));
            Assert.That(kept.Tally.Of(ClashStatus.Approved), Is.EqualTo(0));
            Assert.That(merge.Lines(), Does.Contain(MirrorRule.Prefix + "   a clash both found under the group Group1 of "
                + Kept + ", column 1 against duct 1, is New under " + Kept + " and Approved under " + Mirror + ", a status "
                + "a person sets wins, so the rule gives Approved, and the report shows the group's New for its 2 clashes, "
                + "not changed for one of them"));
        }

        // A row the report does not hold under the kept test cannot carry the status the rule
        // gives, and whether the clashes handed are the report's at all is then UNKNOWN. Nothing
        // is merged and the line says why, the way a kept test in the report twice is said.
        [Test]
        public void AKeptClashHandedWithARowTheReportDoesNotHoldMergesNothing()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            merge.KeptFound("item 2", "item 102", ClashStatus.New, Row("Clash2", ClashStatus.New, "duct 2", "column 2"));
            Found(report, merge, 0, "item 7", "item 8", Row("Clash3", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

            Assert.That(report.Tests.Count, Is.EqualTo(2), "the mirror is reported as its own test");
            Assert.That(Named(report, Kept).Rows.Count, Is.EqualTo(1));
            Assert.That(merge.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   1 clash of " + Kept + " was handed with a row the report does not hold under it, "
                    + "so which row carries its status is UNKNOWN and nothing is merged"
            }));
        }

        // ---------- what was handed against what the report holds, both ways, F132 attempt 8 ----------

        // The breaker's point on attempt 7. The kept test's report holds two clashes and only
        // one was handed. Until attempt 8 the mirror's copy of the other read as found by the
        // mirror only and was added a second time, so one clash was counted twice under the
        // kept test. Now every row the kept test holds must be handed with as many clashes as
        // it stands for, or nothing is merged and the line says why.
        [Test]
        public void AKeptTestWhoseRowsWereNotAllHandedMergesNothing()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            Named(report, Kept).Add(Row("Clash2", ClashStatus.New, "duct 2", "column 2"));
            Found(report, merge, 0, "item 102", "item 2", Row("Clash2", ClashStatus.New, "column 2", "duct 2"));
            merge.AddTo(report);

            Assert.That(Named(report, Kept).Rows.Count, Is.EqualTo(2), "no clash added a second time");
            Assert.That(report.Tests.Count, Is.EqualTo(2), "the mirror is reported as its own test");
            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(0));
            Assert.That(merge.FoundByBoth, Is.Null);
            Assert.That(merge.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   " + Kept + " holds 2 clashes in the report and 1 was handed, 1 row holding another "
                    + "number of clashes than was handed with it, so which of its clashes a mirror found too is UNKNOWN and "
                    + "nothing is merged"
            }));
        }

        // The same for a group: it stands for two clashes and one was handed with it.
        [Test]
        public void AGroupHandedWithFewerClashesThanItHoldsMergesNothing()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 0);
            TestReport kept = Named(report, Kept);
            ClashRow group = ClashRow.ForGroup(2);

            group.Name = "Group1";
            group.Status = ClashStatus.New;
            kept.Add(group);
            kept.State = TestState.FoundClashes;
            merge.KeptFound("item 1", "item 101", ClashStatus.New, group);
            Found(report, merge, 0, "item 102", "item 2", Row("Clash1", ClashStatus.New, "column 2", "duct 2"));
            merge.AddTo(report);

            Assert.That(kept.Rows.Count, Is.EqualTo(1));
            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(0));
            Assert.That(merge.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   " + Kept + " holds 2 clashes in the report and 1 was handed, 1 row holding another "
                    + "number of clashes than was handed with it, so which of its clashes a mirror found too is UNKNOWN and "
                    + "nothing is merged"
            }));
        }

        // The other way. The mirror's report holds two clashes and one was handed. Until
        // attempt 8 the mirror was taken out of the report with both, so the one not handed was
        // in no block and nothing said so. Now the mirror is not merged, it stays in the report
        // as its own test, and the line says a clash both find is then counted twice.
        [Test]
        public void AMirrorHandedFewerClashesThanItsReportHoldsIsNotMergedAndStays()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            Found(report, merge, 0, "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            Named(report, Mirror).Add(Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

            Assert.That(report.Tests.Count, Is.EqualTo(2));
            Assert.That(report.MirrorsMerged, Is.EqualTo(0));
            Assert.That(report.Totals.Total, Is.EqualTo(3), "the clash not handed is still in a block");
            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(0));
            Assert.That(merge.FoundByBoth, Is.Null);
            Assert.That(merge.NotCompared, Is.Null);
            Assert.That(merge.Lines(), Does.Contain(MirrorRule.Prefix + "   1 clash of " + Mirror + " was handed to the "
                + "merge and its report holds 2, so which clashes it found is UNKNOWN, nothing of it is merged into " + Kept
                + ", and it stays in the report as its own test, where a clash both find is counted twice"));
            Assert.That(Text(merge.Lines()), Does.Not.Contain("is taken out of the report"));
        }

        // A mirror handed a clash its report does not hold is not merged either.
        [Test]
        public void AMirrorHandedMoreClashesThanItsReportHoldsIsNotMerged()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

            Assert.That(Named(report, Kept).Rows.Count, Is.EqualTo(1));
            Assert.That(report.Tests.Count, Is.EqualTo(2));
            Assert.That(merge.Lines(), Does.Contain(MirrorRule.Prefix + "   1 clash of " + Mirror + " was handed to the "
                + "merge and its report holds 0, so which clashes it found is UNKNOWN, nothing of it is merged into " + Kept
                + ", and it stays in the report as its own test, where a clash both find is counted twice"));
        }

        // A clash and its row are handed together, and the row of one clash carries that
        // clash's own status, so two statuses for one clash are refused rather than one chosen.
        [Test]
        public void AKeptClashHandedWithAStatusItsOwnRowDoesNotCarryIsRefused()
        {
            MirrorMerge merge = TheMerge();

            Assert.Throws<ArgumentException>(() => merge.KeptFound(
                "item 1", "item 101", ClashStatus.Approved, Row("Clash1", ClashStatus.New, "duct 1", "column 1")));
            Assert.Throws<ArgumentNullException>(() => merge.KeptFound("item 1", "item 101", ClashStatus.New, null));
        }

        // The report's own rule for a status changed on a row: only a row of one clash it holds,
        // and its count by status moves with it, so the cells by status read the rows.
        [Test]
        public void ARestatedRowMovesItsCountAndOnlyARowOfOneClashHeldIsRestated()
        {
            ClashReport report = new ClashReport("1A02MM", "a report");
            TestReport test = report.AddTest(Kept);
            ClashRow row = Row("Clash1", ClashStatus.New, "duct 1", "column 1");
            ClashRow group = ClashRow.ForGroup(3);

            test.Add(row);
            test.Add(group);
            test.Restate(row, ClashStatus.Approved);

            Assert.That(row.Status, Is.EqualTo(ClashStatus.Approved));
            Assert.That(test.Tally.Of(ClashStatus.New), Is.EqualTo(3), "the group's three, New");
            Assert.That(test.Tally.Of(ClashStatus.Approved), Is.EqualTo(1));
            Assert.That(test.Tally.Total, Is.EqualTo(4));
            Assert.Throws<ArgumentException>(() => test.Restate(group, ClashStatus.Approved));
            Assert.Throws<ArgumentException>(() => test.Restate(Row("Clash9", ClashStatus.New, "a", "b"), ClashStatus.Approved));
            Assert.Throws<ArgumentException>(() => test.Restate(null, ClashStatus.Approved));
        }

        // ---------- what a merge does not know reads UNKNOWN, the breaker's finding on attempt 6 ----------

        // A mirror that did not run was never compared, so what the mirrors found by both and
        // with an item not read is UNKNOWN, never a plain 0 added to the sum. What was added to
        // the kept test is a count taken, nothing of the mirror that did not run among it.
        [Test]
        public void AMirrorThatDidNotRunLeavesWhatTheMirrorsFoundUnknown()
        {
            MirrorMerge merge = TheMergeOfTwoMirrors();
            ClashReport report = TheReport(merge, 1);

            Named(report, SecondMirror + " (mirror)").State = TestState.Skipped;
            Found(report, merge, 0, "item 1", "item 101", Row("Clash1", ClashStatus.New, "phone 1", "wall 1"));
            Found(report, merge, 0, "item 7", "item 8", Row("Clash2", ClashStatus.New, "phone 7", "wall 8"));
            merge.AddTo(report);

            Assert.That(merge.FoundByBoth, Is.Null);
            Assert.That(merge.NotCompared, Is.Null);
            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(1));
        }

        // The same where the kept test did not run and nothing was merged.
        [Test]
        public void AKeptTestThatMergedNothingLeavesWhatTheMirrorsFoundUnknown()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 0);

            Named(report, Kept).State = TestState.Skipped;
            Found(report, merge, 0, "item 7", "item 8", Row("Clash1", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

            Assert.That(merge.FoundByBoth, Is.Null);
            Assert.That(merge.NotCompared, Is.Null);
            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(0));
        }

        // A mirror in the report twice was never compared either.
        [Test]
        public void AMirrorInTheReportTwiceLeavesWhatTheMirrorsFoundUnknown()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            report.AddTest(Mirror).State = TestState.FoundClashes;
            merge.AddTo(report);

            Assert.That(merge.FoundByBoth, Is.Null);
            Assert.That(merge.NotCompared, Is.Null);
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

            Found(report, merge, 0, "item 7", "item 8", Row("Clash1", ClashStatus.New, "phone 7", "wall 8"));
            Found(report, merge, 1, "item 8", "item 7", Row("Clash1", ClashStatus.New, "wall 8", "phone 7"));
            merge.AddTo(report);

            Assert.That(merge.AddedToTheKeptTest, Is.EqualTo(1));
            Assert.That(kept.Rows.Count, Is.EqualTo(1));
            Assert.That(kept.Rows[0].FoundOnlyByMirror, Is.EqualTo(FirstMirror + " (mirror)"));
            Assert.That(merge.Lines(), Does.Contain(MirrorRule.Prefix + "   " + KeptOfTwo + " and its mirror "
                + SecondMirror + " (mirror): " + KeptOfTwo + " found 0, the mirror 1, 0 by both and 0 by the mirror only, "
                + "added to " + KeptOfTwo + ", and 1 found by an earlier mirror of " + KeptOfTwo + " as well, added once"));
            Assert.That(report.Tests.Count, Is.EqualTo(1), "both mirrors are taken out of the report");
        }

        // Q138 B over a test kept with two mirrors: each mirror's copy is weighed against the
        // status the report holds under the kept test when it comes, in the rule's order, so
        // the three copies end on the status the rule gives all three, and each line says it.
        [Test]
        public void TwoMirrorsCopiesAreWeighedInTurnAgainstWhatTheReportHolds()
        {
            MirrorMerge merge = TheMergeOfTwoMirrors();
            ClashReport report = TheReport(merge, 1);

            Found(report, merge, 0, "item 101", "item 1", Row("Clash1", ClashStatus.Approved, "phone 1", "wall 1"));
            Found(report, merge, 1, "item 1", "item 101", Row("Clash1", ClashStatus.Resolved, "wall 1", "telecom 1"));
            merge.AddTo(report);

            Assert.That(Named(report, KeptOfTwo).Rows[0].Status, Is.EqualTo(ClashStatus.Approved));

            IList<string> lines = merge.Lines();

            Assert.That(lines, Does.Contain(MirrorRule.Prefix + "   Clash1 of " + KeptOfTwo + ", duct 1 against column 1, "
                + "is New in the report under " + KeptOfTwo + " and Approved under " + FirstMirror + " (mirror), a status a "
                + "person sets wins, so the report shows Approved"));
            Assert.That(lines, Does.Contain(MirrorRule.Prefix + "   Clash1 of " + KeptOfTwo + ", duct 1 against column 1, "
                + "is Approved in the report under " + KeptOfTwo + " and Resolved under " + SecondMirror + " (mirror), a "
                + "status a person sets wins, so the report shows Approved"));
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

            Found(report, merge, 0, "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
            Found(report, merge, 0, "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
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

            Found(report, merge, 0, "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            Found(report, merge, 0, "item 9", "item 10", Row("Clash3", ClashStatus.New, "column 9", "duct 10"));
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

            Found(report, merge, 0, "item 101", "item 1", Row("Clash1", ClashStatus.New, "column 1", "duct 1"));
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

            ClashRow notRead = Row("Clash2", ClashStatus.New, "duct 5", "column 5");

            // The kept test's report holds the clash handed, as the run leaves it.
            Named(report, Kept).Add(notRead);
            merge.KeptFound(TestSettings.UnknownLocator, "item 5", ClashStatus.New, notRead);
            Found(report, merge, 0, null, "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
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

            Found(report, merge, 0, "item 7", "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            Found(report, merge, 0, "item 8", "item 7", Row("Clash3", ClashStatus.New, "duct 8", "column 7"));
            merge.AddTo(report);

            Assert.That(Text(merge.Lines()), Does.Contain(MirrorRule.Prefix + "   1 clash of " + Mirror
                + " repeats a pair of items it already gave, so it is counted once"));
        }

        // ---------- F132 attempt 6 ----------

        // Both readers' finding on attempt 5. A clash of the mirror with an item not read is
        // not added, and the mirror that held it is taken out of the report, so it is in no
        // block, and the line says so where it said only that it was not added.
        [Test]
        public void AMirrorClashWithAnItemNotReadIsSaidToBeInNoBlock()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            Found(report, merge, 0, null, "item 8", Row("Clash2", ClashStatus.New, "column 7", "duct 8"));
            merge.AddTo(report);

            Assert.That(Text(merge.Lines()), Does.Contain(MirrorRule.Prefix + "   1 clash of " + Mirror
                + " has an item that was not read, so whether " + Kept + " found it is UNKNOWN and it is not added, "
                + "and with " + Mirror + " taken out of the report it is in no block of it"));
            Assert.That(report.Totals.Total, Is.EqualTo(1), "the clash is in no block, as the line says");
        }

        // The breaker's finding on attempt 5. Before AddTo has run what each mirror found is
        // UNKNOWN, so the counts are refused and the lines say UNKNOWN, never a row of 0s.
        [Test]
        public void TheCountsAreRefusedAndTheLinesSayUnknownBeforeTheMergeHasRun()
        {
            MirrorMerge merge = TheMerge();

            TheReport(merge, 2);
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash1", ClashStatus.New, "column 7", "duct 8"));

            Assert.Throws<InvalidOperationException>(() => { int? count = merge.FoundByBoth; });
            Assert.Throws<InvalidOperationException>(() => { int count = merge.AddedToTheKeptTest; });
            Assert.Throws<InvalidOperationException>(() => { int? count = merge.NotCompared; });
            Assert.That(merge.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   the clashes of the mirrors of " + Kept + " are not merged into it yet, so what "
                    + "each found is UNKNOWN"
            }));
        }

        // The breaker's blocking finding on attempt 5, item 2. The report counts each mirror
        // it took out, and the workbook check reads that number, so a group with a mirror
        // merged is not a block missing.
        [Test]
        public void TheWorkbookCheckCountsEachMirrorTheReportTookOut()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            Assert.That(report.MirrorsMerged, Is.EqualTo(0));
            merge.AddTo(report);

            Assert.That(report.MirrorsMerged, Is.EqualTo(1));
            Assert.That(CreationPlan.BlockCountLine(report.Tests.Count, 2, report.MirrorsMerged), Is.EqualTo(
                "BLOCKS   1 in the workbook, one for every test in the file, 2 less the 1 mirror merged into its kept test"));
        }

        // A mirror that did not run keeps its place in the report and is not counted as merged.
        [Test]
        public void AMirrorThatDidNotRunIsNotCountedAsMerged()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = TheReport(merge, 1);

            Named(report, Mirror).State = TestState.Skipped;
            merge.AddTo(report);

            Assert.That(report.MirrorsMerged, Is.EqualTo(0));
            Assert.That(CreationPlan.BlockCountLine(report.Tests.Count, 2, report.MirrorsMerged),
                Is.EqualTo(CreationPlan.BlockCountLine(2, 2)));
        }
    }
}
