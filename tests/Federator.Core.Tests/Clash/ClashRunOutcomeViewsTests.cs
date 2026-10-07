using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The runner's own record made readable, F127. The coverage of Bader's request 2 says
    /// for every test of the picked file whether it was created, whether it ran and why it
    /// has no results, and the one place that knows each of those is ClashRunOutcome, which
    /// kept the names private and handed out counts alone. These views read the record and
    /// can never change it.
    /// </summary>
    [TestFixture]
    public class ClashRunOutcomeViewsTests
    {
        [Test]
        public void TheCreatedAndAlreadyThereNamesAreReadInTheOrderTheRunnerKeptThem()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("B");
            outcome.AddCreated("A");
            outcome.AddAlreadyPresent("C");

            Assert.That(outcome.CreatedNames.Count, Is.EqualTo(2));
            Assert.That(outcome.CreatedNames[0], Is.EqualTo("B"));
            Assert.That(outcome.CreatedNames[1], Is.EqualTo("A"));
            Assert.That(outcome.AlreadyPresentNames.Count, Is.EqualTo(1));
            Assert.That(outcome.AlreadyPresentNames[0], Is.EqualTo("C"));
        }

        [Test]
        public void TheTestsThatRanAndTheSkipsAreReadWithWhatTheRunnerRecorded()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            ClashTally two = new ClashTally();
            two.Add(ClashStatus.New, 2);
            outcome.AddRan("ran", 3, 4, two, 0.5);
            outcome.AddSkipped("skipped", ClashSkipReason.EmptySide, "the left side finds nothing");

            Assert.That(outcome.Ran.Count, Is.EqualTo(1));
            Assert.That(outcome.Ran[0].Name, Is.EqualTo("ran"));
            Assert.That(outcome.Ran[0].Tally.Total, Is.EqualTo(2));
            Assert.That(outcome.Skipped.Count, Is.EqualTo(1));
            Assert.That(outcome.Skipped[0].Kind, Is.EqualTo(ClashSkipReason.EmptySide));
            Assert.That(outcome.Skipped[0].Reason, Is.EqualTo("the left side finds nothing"));
        }

        // The break. A view that handed out the list itself would let a reader add a test
        // the runner never created.
        [Test]
        public void NoViewCanChangeTheRecord()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("A");

            Assert.Throws<NotSupportedException>(
                () => ((IList<string>)outcome.CreatedNames).Add("B"));
            Assert.Throws<NotSupportedException>(
                () => ((IList<string>)outcome.AlreadyPresentNames).Add("B"));
            Assert.Throws<NotSupportedException>(
                () => ((IList<ClashTestResult>)outcome.Ran).Clear());
            Assert.Throws<NotSupportedException>(
                () => ((IList<SkippedClashTest>)outcome.Skipped).Clear());
            Assert.That(outcome.CreatedCount, Is.EqualTo(1));
        }

        /// <summary>
        /// The counts the creation plan decided on, kept so the reason a test was not
        /// created is read off what decided it and never off a second count.
        /// </summary>
        [Test]
        public void TheCountsTheCreationPlanUsedAreKeptAsTheyWereHandedIn()
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);
            counts["tree/a"] = 0;
            counts["tree/b"] = 12;

            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.KeepItemsByLocator(counts);
            counts["tree/a"] = 99;
            counts["tree/c"] = 1;

            Assert.That(outcome.ItemsByLocator.Count, Is.EqualTo(2),
                "the record changed when the caller's dictionary did");
            Assert.That(outcome.ItemsByLocator["tree/a"], Is.EqualTo(0));
            Assert.That(outcome.ItemsByLocator["tree/b"], Is.EqualTo(12));
            Assert.Throws<NotSupportedException>(() => outcome.ItemsByLocator["tree/a"] = 5);
        }

        // Two set names in the reference file end in a space, so a locator is never trimmed.
        [Test]
        public void ALocatorIsKeptExactlyAndNeverTrimmed()
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);
            counts["tree/a "] = 3;

            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.KeepItemsByLocator(counts);

            Assert.That(outcome.ItemsByLocator.ContainsKey("tree/a "), Is.True);
            Assert.That(outcome.ItemsByLocator.ContainsKey("tree/a"), Is.False);
        }

        [Test]
        public void NoCreationPlanMeansNoCountsAndNotZeros()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.KeepItemsByLocator(null);

            Assert.That(outcome.ItemsByLocator, Is.Empty);
        }

        /// <summary>
        /// The two counts the run time check read off a test's own sides. The saved tests
        /// with no XML make no creation plan, so these are the only counts that run has.
        /// </summary>
        [Test]
        public void TheSidesTheRunTimeCheckReadAreKeptPerTest()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.RecordSides("T", 0, 44);

            int left;
            int right;

            Assert.That(outcome.TrySidesRead("T", out left, out right), Is.True);
            Assert.That(left, Is.EqualTo(0));
            Assert.That(right, Is.EqualTo(44));
            Assert.That(outcome.TrySidesRead("U", out left, out right), Is.False,
                "a test whose sides were never read reads as read");
            Assert.That(left, Is.EqualTo(-1), "a side nobody read is UNKNOWN and never zero");
            Assert.That(right, Is.EqualTo(-1));
        }

        [Test]
        public void ASideThatCouldNotBeCountedStaysMinusOne()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.RecordSides("T", -1, 5);

            int left;
            int right;
            outcome.TrySidesRead("T", out left, out right);

            Assert.That(left, Is.EqualTo(-1));
            Assert.That(right, Is.EqualTo(5));
        }

        [Test]
        public void SidesForNoNameAreRefused()
        {
            Assert.Throws<ArgumentNullException>(() => new ClashRunOutcome().RecordSides(null, 1, 1));
        }
    }
}
