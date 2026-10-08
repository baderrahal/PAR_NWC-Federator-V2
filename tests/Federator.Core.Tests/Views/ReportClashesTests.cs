using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Report;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132 attempt 2 of the add-in half, the breaker's finding R4. The clashes a view is
    /// built for are the rows the merged report holds, under the test the report holds them
    /// under, each at its row's status. Before this rule the views walked the document by
    /// the test's name and read each status off the document, so a mirror merged into its
    /// kept test lost every view, a clash only the mirror found had a row and no view, and a
    /// status Q138 B restated on the kept row was not the status the view read. The names and
    /// keys in here are sample data, the pair the shape of MirrorMergeTests.
    /// </summary>
    [TestFixture]
    public class ReportClashesTests
    {
        private static string Kept
        {
            get { return MirrorRuleTests.DuctsVsColumns; }
        }

        private static string Mirror
        {
            get { return MirrorRuleTests.ColumnsVsDucts + " (mirror)"; }
        }

        private static MirrorMerge TheMerge()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts));

            IList<MirrorMerge> merges = MirrorMerge.Of(MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked()));

            Assert.That(merges.Count, Is.EqualTo(1));
            return merges[0];
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
        /// The report as the run leaves it before the merge and after it: the kept test with
        /// Clash1 marked Resolved by Navisworks and Clash2 New, both handed, and the mirror
        /// finding Clash1 live and a clash of its own, so after the merge the kept test holds
        /// three rows, Clash1 at the live status by Q138 B, Clash2, and the mirror's own clash
        /// written as found by the mirror only, and the mirror is taken out of the report.
        /// </summary>
        private static ClashReport MergedReport()
        {
            MirrorMerge merge = TheMerge();
            ClashReport report = new ClashReport("1A02MM", "a report");
            TestReport kept = report.AddTest(merge.Kept.Name);
            kept.LeftLocator = MirrorRuleTests.Ducts;
            kept.RightLocator = MirrorRuleTests.Columns;
            kept.Priority = ClashPriority.A;
            kept.State = TestState.FoundClashes;

            ClashRow first = Row("Clash1", ClashStatus.Resolved, "duct 1", "column 1");
            ClashRow second = Row("Clash2", ClashStatus.New, "duct 2", "column 2");
            kept.Add(first);
            kept.Add(second);
            merge.KeptFound("item 1", "item 101", first.Status, first);
            merge.KeptFound("item 2", "item 102", second.Status, second);

            TestReport mirror = report.AddTest(merge.Pairs[0].MirrorName);
            mirror.LeftLocator = MirrorRuleTests.Columns;
            mirror.RightLocator = MirrorRuleTests.Ducts;
            mirror.State = TestState.FoundClashes;

            ClashRow both = Row("Clash1", ClashStatus.Active, "column 1", "duct 1");
            ClashRow only = Row("Clash2", ClashStatus.New, "column 7", "duct 8");
            mirror.Add(both);
            mirror.Add(only);
            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", both);
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", only);

            merge.AddTo(report);

            Assert.That(Named(report, Mirror), Is.Null, "the mirror is taken out of the report by the merge");
            Assert.That(Named(report, Kept).Rows.Count, Is.EqualTo(3));
            return report;
        }

        [Test]
        public void TheClashesOfATestAreTheMergedReportsRowsUnderTheKeptTest()
        {
            ReportClashesOutcome read = ReportClashes.Of(MergedReport());

            Assert.That(read.Clashes.Count, Is.EqualTo(3));
            Assert.That(read.NamedTwice, Is.Empty);

            foreach (ReportClash clash in read.Clashes)
            {
                Assert.That(clash.TestName, Is.EqualTo(Kept), "every row is viewed under the test the report holds it under");
            }
        }

        /// <summary>
        /// Q138 B. The kept test's copy is Resolved in the document and the mirror finds it
        /// live, so the report shows Active and the view reads the row, not the document.
        /// </summary>
        [Test]
        public void TheStatusOfAClashIsTheRowsStatusAfterTheMergeRestatedIt()
        {
            ReportClashesOutcome read = ReportClashes.Of(MergedReport());

            Assert.That(read.Clashes[0].ClashName, Is.EqualTo("Clash1"));
            Assert.That(read.Clashes[0].Status, Is.EqualTo(ClashStatus.Active));
            Assert.That(read.Clashes[0].Row.Status, Is.EqualTo(ClashStatus.Active));
            Assert.That(OpenClashes.StatusesFor(new ViewpointSettings().ViewStatuses), Does.Contain(read.Clashes[0].Status));
        }

        /// <summary>
        /// A clash only the mirror found is viewed under the kept test, named as the workbook
        /// names it, because its own name, Clash2 here, is a name the kept test's own rows
        /// already carry, and the view name says where Clash Detective shows it.
        /// </summary>
        [Test]
        public void AClashOnlyTheMirrorFoundIsViewedUnderTheKeptTestByItsWrittenName()
        {
            ReportClashesOutcome read = ReportClashes.Of(MergedReport());
            ReportClash only = read.Clashes[2];

            Assert.That(only.TestName, Is.EqualTo(Kept));
            Assert.That(only.ClashName, Is.EqualTo("Clash2, found by the mirror only in " + Mirror));
            Assert.That(only.Row.FoundOnlyByMirror, Is.EqualTo(Mirror));
            Assert.That(read.Clashes[1].ClashName, Is.EqualTo("Clash2"));
            Assert.That(read.Clashes[1].ClashName, Is.Not.EqualTo(only.ClashName));
        }

        /// <summary>
        /// F114's add-in pass. The row becomes the view plan's clash: the test the report holds
        /// it under, the two set names, the row's status and the test's priority off the report,
        /// and what the add-in read off the resolved result handed in, nulls where it read none.
        /// </summary>
        [Test]
        public void ToViewCarriesTheTestTheTwoSetNamesTheRowStatusThePriorityAndWhatTheAddInRead()
        {
            ReportClash first = ReportClashes.Of(MergedReport()).Clashes[0];
            ItemPath left = new ItemPath(new[] { 0, 3, 7 });
            ItemPath right = new ItemPath(new[] { 1, 2 });
            Point3 centre = new Point3(1.5, 2.5, 3.5);
            ViewClash clash = first.ToView(SizeVerdict.Large, left, right, centre, "ME.nwc", "ST.nwc");

            Assert.That(clash.TestName, Is.EqualTo(Kept));
            Assert.That(clash.ClashName, Is.EqualTo("Clash1"));
            Assert.That(first.LeftSet, Is.EqualTo("BLD-ME-Ducts"), "read off the row before the walk, for the pair");
            Assert.That(first.RightSet, Is.EqualTo("BLD-ST-Columns"));
            Assert.That(clash.LeftSet, Is.EqualTo("BLD-ME-Ducts"));
            Assert.That(clash.RightSet, Is.EqualTo("BLD-ST-Columns"));
            Assert.That(clash.Status, Is.EqualTo(ClashStatus.Active));
            Assert.That(clash.Priority, Is.EqualTo(ClashPriority.A));
            Assert.That(clash.ServiceSize, Is.EqualTo(SizeVerdict.Large));
            Assert.That(clash.FirstItem, Is.EqualTo(left));
            Assert.That(clash.SecondItem, Is.EqualTo(right));
            Assert.That(clash.Centre, Is.SameAs(centre));
            Assert.That(clash.FirstHome, Is.EqualTo("ME.nwc"));
            Assert.That(clash.SecondHome, Is.EqualTo("ST.nwc"));
            Assert.That(clash.Key, Is.EqualTo(Kept + "\nClash1"));

            ViewClash unread = first.ToView(null, null, null, null, null, null);

            Assert.That(unread.ServiceSize, Is.Null);
            Assert.That(unread.FirstItem, Is.Null);
            Assert.That(unread.Centre, Is.Null);
            Assert.That(unread.FirstHome, Is.Empty);
        }

        [Test]
        public void ATestWithNoRowsHandsNoClash()
        {
            ClashReport report = new ClashReport("1A02MM", "a report");
            report.AddTest("Passed").State = TestState.Passed;

            ReportClashesOutcome read = ReportClashes.Of(report);

            Assert.That(read.Clashes, Is.Empty);
            Assert.That(read.NamedTwice, Is.Empty);
        }

        /// <summary>
        /// A test name the report carries twice is read once and named, as the views have
        /// always done, because both would resolve to the same document test and every
        /// clash of it would be planned twice under one name.
        /// </summary>
        [Test]
        public void ATestOnTheReportTwiceIsReadOnceAndNamed()
        {
            ClashReport report = new ClashReport("1A02MM", "a report");
            TestReport once = report.AddTest("T");
            once.State = TestState.FoundClashes;
            once.Add(Row("Clash1", ClashStatus.New, "a", "b"));
            TestReport twice = report.AddTest("T");
            twice.State = TestState.FoundClashes;
            twice.Add(Row("Clash1", ClashStatus.New, "a", "b"));

            ReportClashesOutcome read = ReportClashes.Of(report);

            Assert.That(read.Clashes.Count, Is.EqualTo(1));
            Assert.That(read.Clashes[0].Row, Is.SameAs(once.Rows[0]));
            Assert.That(read.NamedTwice, Is.EqualTo(new[] { "T" }));
        }

        [Test]
        public void NoReportHandsNothingRatherThanThrowing()
        {
            ReportClashesOutcome read = ReportClashes.Of(null);

            Assert.That(read.Clashes, Is.Empty);
            Assert.That(read.NamedTwice, Is.Empty);
        }
    }
}
