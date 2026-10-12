using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The breaker's B1 of F114's add-in pass and the second reading's N1, C1, N2 and F3. Only
    /// a test every in scope row of which led to its result gets a view and loses its old views.
    /// A test not at its address, an in scope row that no longer leads anywhere, a throw in the
    /// walk or an in scope row with no recorded place is a test not read, kept by the inventory
    /// and counted as failed with why. Every call is keyed by the report's test name.
    /// </summary>
    [TestFixture]
    public class TestsReadTests
    {
        private static TestsRead NewAndActive()
        {
            return new TestsRead(new[] { ClashStatus.New, ClashStatus.Active });
        }

        [Test]
        public void ATestWhoseEveryRowWasReadIsWholeAndNamedForTheInventory()
        {
            TestsRead read = NewAndActive();
            read.Met("Walls");
            read.RowRead("Walls", false);
            read.RowRead("Walls", false);

            Assert.That(read.IsWhole("Walls"), Is.True);
            Assert.That(read.WholeNames, Is.EqualTo(new[] { "Walls" }));
            Assert.That(read.NotWhole(), Is.Empty);
        }

        [Test]
        public void ATestWithAnInScopeRowNotFoundIsNotWholeAndSaysWhy()
        {
            TestsRead read = NewAndActive();
            read.Met("Walls");
            read.RowRead("Walls", false);
            read.RowNotFound("Walls", ClashStatus.Active);

            Assert.That(read.IsWhole("Walls"), Is.False);
            Assert.That(read.WholeNames, Is.Empty);
            IList<KeyValuePair<string, string>> notWhole = read.NotWhole();
            Assert.That(notWhole.Count, Is.EqualTo(1));
            Assert.That(notWhole[0].Key, Is.EqualTo("Walls"));
            Assert.That(notWhole[0].Value, Does.Contain("1 row(s) no longer lead to their result"));
            Assert.That(notWhole[0].Value, Does.Contain("its views of earlier runs are kept"));
        }

        [Test]
        public void ATestNotAtItsAddressAThrowAndARowWithNoPlaceAreEachNotWhole()
        {
            TestsRead read = NewAndActive();
            read.Met("Moved");
            read.NotAtAddress(new[] { "Moved" });
            read.Met("Threw");
            read.RowRead("Threw", false);
            read.Threw(new[] { "Threw" });
            read.RowWithNoAddress("Placeless", ClashStatus.New);
            read.Met("Whole");
            read.RowRead("Whole", false);

            Assert.That(read.WholeNames, Is.EqualTo(new[] { "Whole" }));
            IList<KeyValuePair<string, string>> notWhole = read.NotWhole();
            Assert.That(notWhole.Count, Is.EqualTo(3));
            Assert.That(notWhole[0].Value, Does.Contain("not at its recorded address"));
            Assert.That(notWhole[1].Value, Does.Contain("its walk threw after 1 row(s) were read"));
            Assert.That(notWhole[2].Value, Does.Contain("1 row(s) have no recorded place"));
            Assert.That(read.IsWhole("never met"), Is.False);
        }

        /// <summary>
        /// N1. One resolve under a mirror's run name serves the kept test's rows the mirror alone
        /// found, so a resolve that fails or throws marks every report test among its rows, and
        /// the kept test is whole only where all its rows, its own and the mirror's, were read.
        /// </summary>
        [Test]
        public void AResolveServingRowsOfTwoReportTestsMarksBothAndAKeptTestIsWholeOnlyOnAllItsRows()
        {
            TestsRead read = NewAndActive();
            read.Met("Kept");
            read.RowRead("Kept", false);
            read.Met("Other");
            read.NotAtAddress(new[] { "Kept", "Other" });

            Assert.That(read.IsWhole("Kept"), Is.False, "its own rows read and the mirror's not");
            Assert.That(read.IsWhole("Other"), Is.False);

            TestsRead threw = NewAndActive();
            threw.Met("Kept");
            threw.Threw(new[] { "Kept", "Mirror only" });

            Assert.That(threw.WholeNames, Is.Empty);
            Assert.That(threw.NotWhole().Count, Is.EqualTo(2));
        }

        /// <summary>
        /// C1. A row whose status is outside the views' statuses needs nothing read, so with
        /// Compact ticked a Resolved row whose result is gone, or one with no recorded place,
        /// never makes its test not read.
        /// </summary>
        [Test]
        public void ARowOutOfScopeNotFoundOrWithNoPlaceNeverMakesItsTestNotRead()
        {
            TestsRead read = NewAndActive();

            Assert.That(read.NeedsReading(ClashStatus.Active), Is.True);
            Assert.That(read.NeedsReading(ClashStatus.Resolved), Is.False);
            Assert.That(read.NeedsReading(ClashStatus.Reviewed), Is.False);

            read.Met("Compacted");
            read.RowRead("Compacted", false);
            read.RowNotFound("Compacted", ClashStatus.Resolved);
            read.RowWithNoAddress("Compacted", ClashStatus.Approved);

            Assert.That(read.IsWhole("Compacted"), Is.True);
            Assert.That(read.WholeNames, Is.EqualTo(new[] { "Compacted" }));
        }

        /// <summary>
        /// N2. A test the clash step ran with no row on the report is read whole with no row, so
        /// the inventory removes its old view as no longer needed.
        /// </summary>
        [Test]
        public void ATestRanWithNoRowIsReadWholeWithNoRow()
        {
            TestsRead read = NewAndActive();
            read.Met("With rows");
            read.RowRead("With rows", false);
            read.Ran(new[] { "No clash this week", "With rows" });

            Assert.That(read.IsWhole("No clash this week"), Is.True);
            Assert.That(read.WholeNames, Is.EqualTo(new[] { "With rows", "No clash this week" }));
            Assert.That(read.NotWhole(), Is.Empty);
        }

        /// <summary>
        /// F3, the lead's decision. A test whose in scope rows are all result groups gets no view,
        /// is not counted failed, since a person's grouping must not cost the group DONE, and keeps
        /// its old views, so it is in neither list and is named on its own.
        /// </summary>
        [Test]
        public void ATestWhoseInScopeRowsAreAllResultGroupsIsNeitherWholeNorFailedAndIsNamed()
        {
            TestsRead read = NewAndActive();
            read.Met("Grouped");
            read.RowRead("Grouped", true);
            read.RowRead("Grouped", true);
            read.Met("Mixed");
            read.RowRead("Mixed", true);
            read.RowRead("Mixed", false);

            Assert.That(read.OnlyGroups, Is.EqualTo(new[] { "Grouped" }));
            Assert.That(read.IsWhole("Grouped"), Is.False);
            Assert.That(read.WholeNames, Is.EqualTo(new[] { "Mixed" }));
            Assert.That(read.NotWhole(), Is.Empty);
        }

        /// <summary>
        /// Row F114-K38, the reviewer's D1. A test whose harvest threw after reading only closed
        /// rows has no in scope row on the report, so it read whole and its old view went while its
        /// open clashes were never harvested. A test the clash step skipped as failed is not read
        /// whole, whichever way it was met, and says why. A test skipped for another reason, and a
        /// failed one with no row on the report, are left as they were.
        /// </summary>
        [Test]
        public void ATestTheClashStepFailedIsNotWholeEvenWithOnlyClosedRows()
        {
            ClashRunOutcome clash = new ClashRunOutcome();
            clash.AddSkipped("Closed only", ClashSkipReason.Failed, "IOException: the harvest threw");
            clash.AddSkipped("Not on the report", ClashSkipReason.Failed, "IOException: creating it threw");
            clash.AddSkipped("No locator", ClashSkipReason.NoLocator, "a side carried no locator");

            TestsRead read = NewAndActive();
            read.Met("Closed only");
            read.ClashStepFailed(clash.Skipped, null);
            read.Met("No locator");
            read.Met("Whole");
            read.RowRead("Whole", false);

            Assert.That(read.IsWhole("Closed only"), Is.False);
            Assert.That(read.WholeNames, Is.EqualTo(new[] { "No locator", "Whole" }));
            IList<KeyValuePair<string, string>> notWhole = read.NotWhole();
            Assert.That(notWhole.Count, Is.EqualTo(1));
            Assert.That(notWhole[0].Key, Is.EqualTo("Closed only"));
            Assert.That(notWhole[0].Value, Does.Contain("the clash step failed on it, so its open clashes may never have reached the report"));
            Assert.That(notWhole[0].Value, Does.Contain("its views of earlier runs are kept"));
            Assert.That(read.IsWhole("Not on the report"), Is.False);

            TestsRead before = NewAndActive();
            before.ClashStepFailed(clash.Skipped, null);
            before.Met("Closed only");

            Assert.That(before.IsWhole("Closed only"), Is.False, "marked whether it is met before or after");
            Assert.That(() => before.ClashStepFailed(null, null), Throws.Nothing);
        }

        /// <summary>
        /// The breaker's point 2 on attempt 1 of F114c. A mirror's clashes are merged into its kept
        /// test's rows, so where the clash step failed on the mirror the kept test is not read whole
        /// either, read by the mirror's run name, and says which mirror. A kept test whose mirror ran
        /// is left whole, and a failed test that is no mirror marks no other test.
        /// </summary>
        [Test]
        public void TheKeptTestOfAMirrorTheClashStepFailedOnIsNotWhole()
        {
            ClashRunOutcome clash = new ClashRunOutcome();
            clash.AddSkipped("Columns-vs-Ducts (mirror)", ClashSkipReason.Failed, "IOException: the harvest threw");
            clash.AddSkipped("Lonely", ClashSkipReason.Failed, "IOException: the harvest threw");
            Dictionary<string, string> keptOfMirror = new Dictionary<string, string>
            {
                { "Columns-vs-Ducts (mirror)", "Ducts-vs-Columns" },
                { "Floors-vs-Walls (mirror)", "Walls-vs-Floors" }
            };

            TestsRead read = NewAndActive();
            read.Met("Ducts-vs-Columns");
            read.RowRead("Ducts-vs-Columns", false);
            read.Met("Walls-vs-Floors");
            read.RowRead("Walls-vs-Floors", false);
            read.ClashStepFailed(clash.Skipped, keptOfMirror);

            Assert.That(read.IsWhole("Ducts-vs-Columns"), Is.False);
            Assert.That(read.WholeNames, Is.EqualTo(new[] { "Walls-vs-Floors" }));
            IList<KeyValuePair<string, string>> notWhole = read.NotWhole();
            Assert.That(notWhole.Count, Is.EqualTo(1));
            Assert.That(notWhole[0].Key, Is.EqualTo("Ducts-vs-Columns"));
            Assert.That(
                notWhole[0].Value,
                Does.Contain("the clash step failed on its mirror Columns-vs-Ducts (mirror), so its open clashes may never have reached the report"));
        }
    }
}
