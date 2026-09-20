using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests.Clash
{
    /// <summary>
    /// What a set that finds nothing COSTS, 3b, and the number 3b reported for one run
    /// before this existed was the wrong one. It used `CreationPlan.NotCreatedCount`,
    /// which counts only the tests this run did not CREATE, so on a weekly run where
    /// every test is already in the NWF it reads near zero however many sets are dead.
    /// The run of 2026-09-20 21:36 reported exactly 60 on seven of the ten groups while
    /// they held between 37 and 55 sets that found nothing.
    /// </summary>
    [TestFixture]
    public class EmptySideCostTests
    {
        /// <summary>One test between two named sets, read the way the engine reads one.</summary>
        private static PlannedClashTest Test(string name, string left, string right)
        {
            string xml = "<exchange units=\"ft\"><batchtest name=\"b\">"
                + "<clashtest name=\"" + name + "\" test_type=\"hard_conservative\""
                + " tolerance=\"0.2460629921\" merge_composites=\"1\">"
                + "<left><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>"
                + left + "</locator></clashselection></left>"
                + "<right><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>"
                + right + "</locator></clashselection></right>"
                + "</clashtest></batchtest></exchange>";

            return ClashTestPlan.From(new ExchangeReader().ReadText(xml), "m").Buildable[0];
        }

        private static Dictionary<string, int> Counts()
        {
            return new Dictionary<string, int>
            {
                { "a/Full", 120 },
                { "a/AlsoFull", 8 },
                { "a/Empty", 0 },
                { "a/AlsoEmpty", 0 }
            };
        }

        [Test]
        public void ATestWithOneEmptySideIsCounted()
        {
            EmptySideTally tally = EmptySideCost.Count(
                new List<PlannedClashTest> { Test("one", "a/Full", "a/Empty") }, Counts());

            Assert.That(tally.WithAnEmptySide, Is.EqualTo(1));
            Assert.That(tally.Judged, Is.EqualTo(1));
            Assert.That(tally.CouldNotTell, Is.EqualTo(0));
        }

        [Test]
        public void ATestWithBothSidesEmptyIsCountedOnceAndNeverTwice()
        {
            EmptySideTally tally = EmptySideCost.Count(
                new List<PlannedClashTest> { Test("one", "a/Empty", "a/AlsoEmpty") }, Counts());

            Assert.That(tally.WithAnEmptySide, Is.EqualTo(1));
        }

        [Test]
        public void ATestWithBothSidesFullIsNotCounted()
        {
            EmptySideTally tally = EmptySideCost.Count(
                new List<PlannedClashTest> { Test("one", "a/Full", "a/AlsoFull") }, Counts());

            Assert.That(tally.WithAnEmptySide, Is.EqualTo(0));
            Assert.That(tally.Judged, Is.EqualTo(1));
        }

        /// <summary>
        /// THIS IS THE WHOLE POINT OF THE RULE. A test already in the document still
        /// points at a set that finds nothing, and it costs exactly as much as one this
        /// run would have created. The count does not know or care which is which.
        /// </summary>
        [Test]
        public void EveryTestIsCountedAndNotOnlyTheOnesARunWouldCreate()
        {
            List<PlannedClashTest> all = new List<PlannedClashTest>();

            for (int i = 0; i < 1830; i++)
            {
                all.Add(Test("test " + i, "a/Full", i < 1677 ? "a/Empty" : "a/AlsoFull"));
            }

            EmptySideTally tally = EmptySideCost.Count(all, Counts());

            Assert.That(tally.WithAnEmptySide, Is.EqualTo(1677), "his own 1A02MM number");
            Assert.That(tally.Judged, Is.EqualTo(1830));
        }

        /// <summary>
        /// A side nobody counted is not a side finding nothing, which is the rule
        /// CreationPlan already keeps for the same dictionary. It goes in its own number
        /// so a cost with a hole in it is visible rather than silently smaller.
        /// </summary>
        [Test]
        public void ASideNobodyCountedIsItsOwnNumberAndNeverCountedAsEmpty()
        {
            EmptySideTally tally = EmptySideCost.Count(
                new List<PlannedClashTest>
                {
                    Test("one", "a/Full", "a/NobodyCountedThis"),
                    Test("two", "a/Full", "a/Empty")
                },
                Counts());

            Assert.That(tally.CouldNotTell, Is.EqualTo(1));
            Assert.That(tally.WithAnEmptySide, Is.EqualTo(1), "only the one that was judged");
            Assert.That(tally.Judged, Is.EqualTo(1));
        }

        [Test]
        public void NoCountsAtAllMeansNothingIsJudgedAndNothingIsCalledEmpty()
        {
            EmptySideTally tally = EmptySideCost.Count(
                new List<PlannedClashTest> { Test("one", "a/Full", "a/Empty") }, null);

            Assert.That(tally.WithAnEmptySide, Is.EqualTo(0));
            Assert.That(tally.CouldNotTell, Is.EqualTo(1));
        }

        [Test]
        public void NoTestsAtAllIsAllZeroesAndNeverThrows()
        {
            EmptySideTally tally = EmptySideCost.Count(null, Counts());

            Assert.That(tally.WithAnEmptySide, Is.EqualTo(0));
            Assert.That(tally.Judged, Is.EqualTo(0));
            Assert.That(tally.CouldNotTell, Is.EqualTo(0));
        }
    }
}
