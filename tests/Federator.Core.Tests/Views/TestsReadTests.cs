using System.Collections.Generic;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The breaker's B1 of F114's add-in pass. Only a test every row of which led to its
    /// result gets a view and loses its old views. A test not at its address, a row that no
    /// longer leads anywhere, a throw in the walk or a row with no recorded place is a test
    /// not read, kept by the inventory and counted as failed with why.
    /// </summary>
    [TestFixture]
    public class TestsReadTests
    {
        [Test]
        public void ATestWhoseEveryRowWasReadIsWholeAndNamedForTheInventory()
        {
            TestsRead read = new TestsRead();
            read.Met("Walls");
            read.RowRead("Walls");
            read.RowRead("Walls");

            Assert.That(read.IsWhole("Walls"), Is.True);
            Assert.That(read.WholeNames, Is.EqualTo(new[] { "Walls" }));
            Assert.That(read.NotWhole(), Is.Empty);
        }

        [Test]
        public void ATestWithARowNotFoundIsNotWholeAndSaysWhy()
        {
            TestsRead read = new TestsRead();
            read.Met("Walls");
            read.RowRead("Walls");
            read.RowNotFound("Walls");

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
            TestsRead read = new TestsRead();
            read.Met("Moved");
            read.NotAtAddress("Moved");
            read.Met("Threw");
            read.RowRead("Threw");
            read.Threw("Threw");
            read.RowWithNoAddress("Placeless");
            read.Met("Whole");
            read.RowRead("Whole");

            Assert.That(read.WholeNames, Is.EqualTo(new[] { "Whole" }));
            IList<KeyValuePair<string, string>> notWhole = read.NotWhole();
            Assert.That(notWhole.Count, Is.EqualTo(3));
            Assert.That(notWhole[0].Value, Does.Contain("not at its recorded address"));
            Assert.That(notWhole[1].Value, Does.Contain("its walk threw after 1 row(s) were read"));
            Assert.That(notWhole[2].Value, Does.Contain("1 row(s) have no recorded place"));
            Assert.That(read.IsWhole("never met"), Is.False);
        }
    }
}
