using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The five counts, and what a census does with one it could not take.
    ///
    /// Everything here turns on minus one not being zero. A rebuild that could not count
    /// its clash tests does not know whether they came back, and a census that answered
    /// zero would let a run throw 1830 tests away and report that nothing moved.
    /// </summary>
    [TestFixture]
    public class DocumentCensusTests
    {
        private static DocumentCensus Census(int models, int sets, int tests, int results, int views)
        {
            return new DocumentCensus(models, sets, tests, results, views);
        }

        [Test]
        public void TheFiveCountsComeBackAsTheyWentIn()
        {
            DocumentCensus census = Census(5, 61, 1830, 412, 3);

            Assert.That(census.Of(CensusCount.Models), Is.EqualTo(5));
            Assert.That(census.Of(CensusCount.Sets), Is.EqualTo(61));
            Assert.That(census.Of(CensusCount.Tests), Is.EqualTo(1830));
            Assert.That(census.Of(CensusCount.Results), Is.EqualTo(412));
            Assert.That(census.Of(CensusCount.Viewpoints), Is.EqualTo(3));
            Assert.That(census.KnowsEverything, Is.True);
        }

        [Test]
        public void TheLineNamesWhenItWasTakenAndAllFiveCounts()
        {
            string line = Census(5, 61, 1830, 412, 3).Line("before TESTS RUN");

            Assert.That(line, Does.StartWith("CENSUS   before TESTS RUN"));
            Assert.That(line, Does.Contain("models 5"));
            Assert.That(line, Does.Contain("sets 61"));
            Assert.That(line, Does.Contain("tests 1830"));
            Assert.That(line, Does.Contain("results 412"));
            Assert.That(line, Does.Contain("views 3"));
        }

        /// <summary>
        /// The one this whole type exists for. A count that could not be taken says
        /// UNKNOWN and never zero, because zero reads as a real count.
        /// </summary>
        [Test]
        public void ACountThatCouldNotBeTakenSaysUnknownAndNeverZero()
        {
            DocumentCensus census = Census(5, -1, 1830, 412, 3);

            Assert.That(census.Knows(CensusCount.Sets), Is.False);
            Assert.That(census.Of(CensusCount.Sets), Is.EqualTo(DocumentCensus.NotCounted));
            Assert.That(census.Show(CensusCount.Sets), Is.EqualTo("UNKNOWN"));
            Assert.That(census.Line("after SETS"), Does.Contain("sets UNKNOWN"));
            Assert.That(census.Line("after SETS"), Does.Not.Contain("sets 0"));
            Assert.That(census.KnowsEverything, Is.False);
        }

        [Test]
        public void AZeroIsARealCountAndReadsAsOne()
        {
            DocumentCensus census = Census(0, 0, 0, 0, 0);

            Assert.That(census.Knows(CensusCount.Tests), Is.True);
            Assert.That(census.Show(CensusCount.Tests), Is.EqualTo("0"));
            Assert.That(census.KnowsEverything, Is.True);
        }

        [Test]
        public void AnythingBelowZeroIsNotCountedAndNotSomeOtherNumber()
        {
            Assert.That(Census(-7, 1, 1, 1, 1).Of(CensusCount.Models),
                Is.EqualTo(DocumentCensus.NotCounted));
        }

        [Test]
        public void ACensusThatCouldCountNothingKnowsNothing()
        {
            DocumentCensus census = DocumentCensus.Unknown();

            foreach (CensusCount what in DocumentCensus.All)
            {
                Assert.That(census.Knows(what), Is.False, what.ToString());
            }

            Assert.That(census.Line("after NWD"), Does.Not.Contain(" 0"));
        }

        [Test]
        public void EveryCountThatIsADifferentNumberIsAMove()
        {
            DocumentCensus before = Census(5, 61, 1830, 0, 0);
            DocumentCensus after = Census(5, 61, 1830, 412, 0);

            IList<CensusCount> moved = after.MovedSince(before);

            Assert.That(moved.Count, Is.EqualTo(1));
            Assert.That(moved[0], Is.EqualTo(CensusCount.Results));
        }

        [Test]
        public void ACountGoingDownIsAMoveJustAsMuchAsOneGoingUp()
        {
            IList<CensusCount> moved = Census(5, 0, 1830, 0, 0).MovedSince(Census(5, 61, 1830, 0, 0));

            Assert.That(moved.Count, Is.EqualTo(1));
            Assert.That(moved[0], Is.EqualTo(CensusCount.Sets));
        }

        /// <summary>
        /// The break. Comparing a real number against UNKNOWN answers nothing, so it is
        /// not a move, and it is not silence either: the count is named as one that could
        /// not be compared.
        /// </summary>
        [Test]
        public void ACountEitherCensusCouldNotTakeIsNotAMoveAndIsNamedInstead()
        {
            DocumentCensus before = Census(5, 61, 1830, 0, -1);
            DocumentCensus after = Census(5, 61, 1830, 0, 9);

            Assert.That(after.MovedSince(before), Is.Empty);

            IList<CensusCount> unknown = after.CouldNotCompare(before);

            Assert.That(unknown.Count, Is.EqualTo(1));
            Assert.That(unknown[0], Is.EqualTo(CensusCount.Viewpoints));
        }

        [Test]
        public void NothingToCompareAgainstIsNoMoveAndNoThrow()
        {
            Assert.That(Census(1, 1, 1, 1, 1).MovedSince(null), Is.Empty);
            Assert.That(Census(1, 1, 1, 1, 1).CouldNotCompare(null), Is.Empty);
        }

        [Test]
        public void TheFiveAreAlwaysInTheSameOrder()
        {
            IList<CensusCount> all = DocumentCensus.All;

            Assert.That(all.Count, Is.EqualTo(5));
            Assert.That(all[0], Is.EqualTo(CensusCount.Models));
            Assert.That(all[1], Is.EqualTo(CensusCount.Sets));
            Assert.That(all[2], Is.EqualTo(CensusCount.Tests));
            Assert.That(all[3], Is.EqualTo(CensusCount.Results));
            Assert.That(all[4], Is.EqualTo(CensusCount.Viewpoints));
        }

        [Test]
        public void EveryCountHasWordsAndAShortName()
        {
            foreach (CensusCount what in DocumentCensus.All)
            {
                Assert.That(DocumentCensus.Words(what), Is.Not.Empty.And.Not.EqualTo("UNKNOWN"));
                Assert.That(DocumentCensus.Short(what), Is.Not.Empty.And.Not.EqualTo("UNKNOWN"));
            }
        }
    }
}
