using System;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Which statuses mean somebody decided something.
    ///
    /// This rule decides what a rebuild has to see come back, and the clash results are the
    /// part of the NWF with no second copy anywhere, so the tests here are about the rule
    /// never counting a decision as disposable.
    /// </summary>
    [TestFixture]
    public class StatusesAPersonSetTests
    {
        [Test]
        public void NewIsWhereAClashStartsAndIsNotADecision()
        {
            Assert.That(StatusesAPersonSet.Counts(ClashStatus.New), Is.False);
        }

        [Test]
        public void EveryOtherStatusIsADecision()
        {
            Assert.That(StatusesAPersonSet.Counts(ClashStatus.Active), Is.True);
            Assert.That(StatusesAPersonSet.Counts(ClashStatus.Reviewed), Is.True);
            Assert.That(StatusesAPersonSet.Counts(ClashStatus.Approved), Is.True);
            Assert.That(StatusesAPersonSet.Counts(ClashStatus.Resolved), Is.True);
        }

        /// <summary>
        /// The break. A value the enum does not name must not read as disposable, because
        /// a status this Core does not recognise is the one most worth keeping: it came
        /// from somewhere this code does not know about.
        /// </summary>
        [Test]
        public void AStatusThisCoreDoesNotRecogniseStillCounts()
        {
            Assert.That(StatusesAPersonSet.Counts((ClashStatus)99), Is.True);
        }

        [Test]
        public void AllNamesTheFourAndNotNew()
        {
            ClashStatus[] all = StatusesAPersonSet.All();

            Assert.That(all.Length, Is.EqualTo(4));
            Assert.That(all, Does.Not.Contain(ClashStatus.New));
            Assert.That(all, Does.Contain(ClashStatus.Active));
            Assert.That(all, Does.Contain(ClashStatus.Reviewed));
            Assert.That(all, Does.Contain(ClashStatus.Approved));
            Assert.That(all, Does.Contain(ClashStatus.Resolved));
        }

        [Test]
        public void ATallyOfNothingButNewHoldsNoDecision()
        {
            ClashTally tally = new ClashTally();
            tally.Add(ClashStatus.New, 1830);

            Assert.That(StatusesAPersonSet.In(tally), Is.EqualTo(0));
            Assert.That(StatusesAPersonSet.Describe(tally), Is.EqualTo("none, every clash is still at New"));
        }

        [Test]
        public void TheDecisionsAreCountedAndTheNewOnesAreNot()
        {
            ClashTally tally = new ClashTally();

            tally.Add(ClashStatus.New, 100);
            tally.Add(ClashStatus.Active, 7);
            tally.Add(ClashStatus.Reviewed, 3);
            tally.Add(ClashStatus.Resolved, 11);

            Assert.That(StatusesAPersonSet.In(tally), Is.EqualTo(21));
        }

        /// <summary>
        /// A rebuild that lost something has to say WHICH kind of decision went, because
        /// eleven Resolved going is a different conversation from eleven Active going.
        /// </summary>
        [Test]
        public void DescribeNamesEachKindWithItsCount()
        {
            ClashTally tally = new ClashTally();

            tally.Add(ClashStatus.New, 100);
            tally.Add(ClashStatus.Active, 7);
            tally.Add(ClashStatus.Resolved, 11);

            string said = StatusesAPersonSet.Describe(tally);

            Assert.That(said, Does.Contain("7 Active"));
            Assert.That(said, Does.Contain("11 Resolved"));
            Assert.That(said, Does.Not.Contain("New"));
            Assert.That(said, Does.Not.Contain("Reviewed"));
        }

        [Test]
        public void NothingCountedSaysSoRatherThanZero()
        {
            Assert.That(StatusesAPersonSet.In(null), Is.EqualTo(0));
            Assert.That(StatusesAPersonSet.Describe(null), Is.EqualTo("none, nothing was counted"));
        }
    }
}
