using System.Collections.Generic;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Whether a side's set was counted is one routine, read by the creation plan and by the empty side cost, T1-N47.
    /// A set nobody counted is not a set that holds nothing, and the number is nought only because an out
    /// argument has to be something.
    /// </summary>
    [TestFixture]
    public class PlannedClashSideCountedTests
    {
        private static readonly string Locator = "lcop_selection_set_tree/Architecture/BLD-AR-Walls";

        private static PlannedClashSide Side(string locator)
        {
            return new PlannedClashSide(false, 1, locator);
        }

        [Test]
        public void ASideWhoseSetWasCountedReadsItsItems()
        {
            Dictionary<string, int> counts = new Dictionary<string, int> { { Locator, 14 } };
            int items;

            Assert.That(PlannedClashSide.Counted(counts, Side(Locator), out items), Is.True);
            Assert.That(items, Is.EqualTo(14));
        }

        [Test]
        public void ASetCountedAtNoughtIsCountedAndNotMissing()
        {
            Dictionary<string, int> counts = new Dictionary<string, int> { { Locator, 0 } };
            int items = -1;

            Assert.That(PlannedClashSide.Counted(counts, Side(Locator), out items), Is.True, "nought items is an answer");
            Assert.That(items, Is.EqualTo(0));
        }

        [Test]
        public void ASetNobodyCountedIsNotCounted()
        {
            Dictionary<string, int> counts = new Dictionary<string, int> { { Locator + "x", 14 } };
            int items = -1;

            Assert.That(PlannedClashSide.Counted(counts, Side(Locator), out items), Is.False);
            Assert.That(items, Is.EqualTo(0));
        }

        [Test]
        public void NoCountsNoSideOrNoLocatorIsNotCounted()
        {
            Dictionary<string, int> counts = new Dictionary<string, int> { { Locator, 14 } };
            int items;

            Assert.That(PlannedClashSide.Counted(null, Side(Locator), out items), Is.False, "no counts at all");
            Assert.That(PlannedClashSide.Counted(counts, null, out items), Is.False, "no side");
            Assert.That(PlannedClashSide.Counted(counts, Side(null), out items), Is.False, "a side with no locator");
            Assert.That(PlannedClashSide.Counted(counts, Side(string.Empty), out items), Is.False);
            Assert.That(items, Is.EqualTo(0));
        }
    }
}
