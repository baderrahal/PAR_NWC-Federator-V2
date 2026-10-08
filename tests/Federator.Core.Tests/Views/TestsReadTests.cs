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
    }
}
