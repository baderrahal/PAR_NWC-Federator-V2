using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// What the matrix counts as still outstanding.
    ///
    /// The clash API has no open against closed notion at all, see docs\scan.md section
    /// 4h, so neither choice is read off it. Both are stated rules, and the sheet says
    /// which one it used so nobody reads an API meaning into a number that has none.
    /// </summary>
    [TestFixture]
    public class OpenClashesTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string Floors = Root + "/Architecture/BLD-AR-Floors";
        private const string Ducts = Root + "/Mechanical/BLD-ME-Ducts";

        private static ClashTally Tally()
        {
            ClashTally tally = new ClashTally();
            tally.Add(ClashStatus.New, 10);
            tally.Add(ClashStatus.Active, 5);
            tally.Add(ClashStatus.Reviewed, 3);
            tally.Add(ClashStatus.Approved, 2);
            tally.Add(ClashStatus.Resolved, 40);
            return tally;
        }

        // ---------- the two choices ----------

        [Test]
        public void NavisworksOpenIsTheDefaultBecauseItIsTheProductsOwnDefinition()
        {
            Assert.That(OpenClashes.Default, Is.EqualTo(OpenClashCount.NavisworksOpen));
            Assert.That(new ClashReport("b", "n").OpenCount, Is.EqualTo(OpenClashCount.NavisworksOpen));
            Assert.That(new ReportOptions().OpenCount, Is.EqualTo(OpenClashCount.NavisworksOpen));
        }

        [Test]
        public void NewAndActiveCountsTwoStatuses()
        {
            Assert.That(OpenClashes.Of(Tally(), OpenClashCount.NewAndActive), Is.EqualTo(15));
            Assert.That(OpenClashes.StatusesFor(OpenClashCount.NewAndActive).Length, Is.EqualTo(2));
        }

        [Test]
        public void NavisworksOpenAddsReviewed()
        {
            Assert.That(OpenClashes.Of(Tally(), OpenClashCount.NavisworksOpen), Is.EqualTo(18));
            Assert.That(OpenClashes.StatusesFor(OpenClashCount.NavisworksOpen).Length, Is.EqualTo(3));
        }

        [Test]
        public void ApprovedAndResolvedAreClosedUnderBoth()
        {
            foreach (OpenClashCount which in OpenClashes.All())
            {
                List<ClashStatus> counted = new List<ClashStatus>(OpenClashes.StatusesFor(which));

                Assert.That(counted, Does.Not.Contain(ClashStatus.Approved), which.ToString());
                Assert.That(counted, Does.Not.Contain(ClashStatus.Resolved), which.ToString());
            }
        }

        [Test]
        public void AnEmptyTallyIsZeroUnderBoth()
        {
            foreach (OpenClashCount which in OpenClashes.All())
            {
                Assert.That(OpenClashes.Of(new ClashTally(), which), Is.EqualTo(0));
                Assert.That(OpenClashes.Of(null, which), Is.EqualTo(0));
            }
        }

        // ---------- it changes the matrix ----------

        private static ClashReport Report(OpenClashCount which)
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001");
            report.SetTreeRoot = Root;
            report.OpenCount = which;

            TestReport test = report.AddTest("AR v ME");
            test.LeftLocator = Floors;
            test.RightLocator = Ducts;
            test.State = TestState.FoundClashes;

            foreach (ClashStatus status in ClashTally.AllStatuses)
            {
                ClashRow row = new ClashRow();
                row.Name = status.ToString();
                row.Status = status;
                row.RawClashes = status == ClashStatus.Resolved ? 40 : Count(status);
                row.IsGroup = row.RawClashes > 1;
                test.Add(row);
            }

            return report;
        }

        private static int Count(ClashStatus status)
        {
            if (status == ClashStatus.New) { return 10; }
            if (status == ClashStatus.Active) { return 5; }
            if (status == ClashStatus.Reviewed) { return 3; }
            return 2;
        }

        // The one the brief asks for by name.
        [Test]
        public void TheSettingChangesWhatTheMatrixCellHolds()
        {
            MatrixCell newAndActive = ClashMatrix
                .From(Report(OpenClashCount.NewAndActive)).At(Floors, Ducts);

            MatrixCell navisworksOpen = ClashMatrix
                .From(Report(OpenClashCount.NavisworksOpen)).At(Floors, Ducts);

            Assert.That(newAndActive.Text(), Is.EqualTo("15"));
            Assert.That(navisworksOpen.Text(), Is.EqualTo("18"),
                "the setting did not reach the matrix");
            Assert.That(navisworksOpen.NewPlusActive - newAndActive.NewPlusActive, Is.EqualTo(3),
                "the difference is exactly the Reviewed clashes");
        }

        [Test]
        public void TheMatrixSaysWhichOneItUsed()
        {
            Assert.That(ClashMatrix.From(Report(OpenClashCount.NavisworksOpen)).OpenCount,
                Is.EqualTo(OpenClashCount.NavisworksOpen));
            Assert.That(ClashMatrix.From(Report(OpenClashCount.NewAndActive)).OpenCount,
                Is.EqualTo(OpenClashCount.NewAndActive));
        }

        [Test]
        public void ASkippedPairStillSaysSkippedUnderBoth()
        {
            foreach (OpenClashCount which in OpenClashes.All())
            {
                ClashReport report = new ClashReport("b", "n");
                report.OpenCount = which;

                TestReport test = report.AddTest("skipped");
                test.LeftLocator = Floors;
                test.RightLocator = Ducts;
                test.State = TestState.Skipped;

                Assert.That(ClashMatrix.From(report).At(Floors, Ducts).Text(),
                    Is.EqualTo("skipped"), which.ToString());
            }
        }

        // ---------- the sheet is labelled with which one ----------

        [Test]
        public void EachChoiceHasItsOwnWordsAndTheyAreDifferent()
        {
            Assert.That(OpenClashes.Describe(OpenClashCount.NewAndActive),
                Is.Not.EqualTo(OpenClashes.Describe(OpenClashCount.NavisworksOpen)));
            Assert.That(OpenClashes.Heading(OpenClashCount.NavisworksOpen),
                Does.Contain("Reviewed"));
            Assert.That(OpenClashes.Heading(OpenClashCount.NewAndActive),
                Does.Not.Contain("Reviewed"));
        }

        [Test]
        public void TheLabelSaysWhatIsCountedAndWhatIsNot()
        {
            Assert.That(OpenClashes.SheetLabel(OpenClashCount.NavisworksOpen),
                Does.Contain("Navisworks' own definition of open"));
            Assert.That(OpenClashes.SheetLabel(OpenClashCount.NavisworksOpen),
                Does.Contain("Approved and Resolved counted as closed"));
            Assert.That(OpenClashes.SheetLabel(OpenClashCount.NewAndActive),
                Does.Contain("Reviewed is NOT counted here"));
        }

        [Test]
        public void BothChoicesAreOffered()
        {
            Assert.That(OpenClashes.All().Length, Is.EqualTo(2));
            Assert.That(OpenClashes.All().Length,
                Is.EqualTo(Enum.GetValues(typeof(OpenClashCount)).Length),
                "a choice was added and the window does not offer it");
            Assert.That(OpenClashes.All()[0], Is.EqualTo(OpenClashes.Default),
                "the default should be the first one offered");
        }

        // ---------- resolved clashes are visible per test ----------

        [Test]
        public void EachTestReportsHowManyAreResolved()
        {
            TestReport test = Report(OpenClashCount.NavisworksOpen).Tests[0];

            Assert.That(test.Resolved, Is.EqualTo(40),
                "resolved clashes stay in the file and this is how the growth is seen");
            Assert.That(test.OpenUnder(OpenClashCount.NavisworksOpen), Is.EqualTo(18));
            Assert.That(test.RawClashes, Is.EqualTo(60));
        }

        [Test]
        public void TheReportAddsUpTheResolvedAcrossEveryTest()
        {
            Assert.That(Report(OpenClashCount.NavisworksOpen).TotalResolved, Is.EqualTo(40));
        }

        [Test]
        public void NothingCompactedIsMinusOneRatherThanZero()
        {
            Assert.That(new ClashReport("b", "n").CompactedAway, Is.EqualTo(-1),
                "zero removed and never asked to remove are different things");
            Assert.That(new ClashRunOutcome().Compacted, Is.EqualTo(-1));
        }
    }
}
