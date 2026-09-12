using System;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// A real run spent 8 hours 52 minutes across 24 groups and produced nothing, because
    /// every test threw the same exception and the run carried on regardless. Stopping the
    /// group would have saved none of it, because every group failed the same way.
    /// </summary>
    [TestFixture]
    public class RepeatedFailureGuardTests
    {
        private const string Disposed =
            "ObjectDisposedException: Object has been Disposed (WeakRef)";

        private static RepeatedFailureGuard AfterFailures(int howMany, string reason)
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard();

            for (int i = 0; i < howMany; i++)
            {
                guard.RecordFailure(reason);
            }

            return guard;
        }

        // ---------- it fires at the right count ----------

        [Test]
        public void TheDefaultIsFifty()
        {
            Assert.That(RepeatedFailureGuard.DefaultThreshold, Is.EqualTo(50));
        }

        [Test]
        public void FortyNineFailuresIsNotEnoughToStop()
        {
            RepeatedFailureGuard guard = AfterFailures(49, Disposed);

            Assert.That(guard.ShouldStopTheRun, Is.False);
            Assert.That(guard.Consecutive, Is.EqualTo(49));
            Assert.That(guard.Reason, Is.Null);
        }

        [Test]
        public void TheFiftiethFailureStopsIt()
        {
            RepeatedFailureGuard guard = AfterFailures(50, Disposed);

            Assert.That(guard.ShouldStopTheRun, Is.True);
            Assert.That(guard.Consecutive, Is.EqualTo(50));
        }

        [Test]
        public void ItKeepsCountingPastTheThreshold()
        {
            Assert.That(AfterFailures(1830, Disposed).Consecutive, Is.EqualTo(1830));
        }

        // The number is a setting, not a constant.
        [Test]
        public void TheThresholdIsASetting()
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard(3);

            guard.RecordFailure(Disposed);
            guard.RecordFailure(Disposed);
            Assert.That(guard.ShouldStopTheRun, Is.False);

            guard.RecordFailure(Disposed);
            Assert.That(guard.ShouldStopTheRun, Is.True);
        }

        [Test]
        public void AThresholdBelowOneIsRefusedRatherThanStoppingEverything()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { new RepeatedFailureGuard(0); });
            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { new RepeatedFailureGuard(-1); });
        }

        // ---------- only the same reason counts ----------

        [Test]
        public void FailuresForDifferentReasonsDoNotStopTheRun()
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard();

            for (int i = 0; i < 200; i++)
            {
                guard.RecordFailure("reason number " + i);
            }

            Assert.That(guard.ShouldStopTheRun, Is.False,
                "a run failing for many different reasons is not the case this guards");
            Assert.That(guard.Consecutive, Is.EqualTo(1));
        }

        [Test]
        public void ADifferentReasonPartWayThroughStartsTheCountAgain()
        {
            RepeatedFailureGuard guard = AfterFailures(49, Disposed);

            guard.RecordFailure("something else entirely");

            Assert.That(guard.ShouldStopTheRun, Is.False);
            Assert.That(guard.Consecutive, Is.EqualTo(1));
        }

        // One test doing what it was meant to means the run is not uniformly broken.
        [Test]
        public void OneSuccessStartsTheCountAgain()
        {
            RepeatedFailureGuard guard = AfterFailures(49, Disposed);

            guard.RecordSuccess();
            Assert.That(guard.Consecutive, Is.EqualTo(0));

            guard.RecordFailure(Disposed);
            Assert.That(guard.Consecutive, Is.EqualTo(1));
            Assert.That(guard.ShouldStopTheRun, Is.False);
        }

        // Most pairs have a side finding nothing in that model. That is the ordinary
        // answer and says nothing about whether the run is broken.
        [Test]
        public void ASkippedTestNeitherStopsTheRunNorClearsTheCount()
        {
            RepeatedFailureGuard guard = AfterFailures(49, Disposed);

            for (int i = 0; i < 1000; i++)
            {
                guard.RecordNotAttempted();
            }

            Assert.That(guard.Consecutive, Is.EqualTo(49),
                "a test that was never attempted must not clear the count");
            Assert.That(guard.ShouldStopTheRun, Is.False);

            guard.RecordFailure(Disposed);
            Assert.That(guard.ShouldStopTheRun, Is.True);
        }

        [Test]
        public void NothingRecordedAtAllDoesNotStopAnything()
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard();

            Assert.That(guard.ShouldStopTheRun, Is.False);
            Assert.That(guard.Consecutive, Is.EqualTo(0));
        }

        // ---------- what it says when it fires ----------

        [Test]
        public void TheReasonNamesTheCountAndCarriesTheOriginalFailure()
        {
            string reason = AfterFailures(50, Disposed).Reason;

            Assert.That(reason, Is.Not.Null.And.Not.Empty);
            Assert.That(reason, Does.Contain("50"));
            Assert.That(reason, Does.Contain(Disposed),
                "the reason the run stopped has to carry what actually went wrong");
            Assert.That(reason, Does.Contain("the rest of the run was not attempted"));
        }

        [Test]
        public void AFailureWithNoReasonStillCounts()
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard(2);

            guard.RecordFailure(null);
            guard.RecordFailure(null);

            Assert.That(guard.ShouldStopTheRun, Is.True);
            Assert.That(guard.Reason, Is.Not.Null.And.Not.Empty);
        }

        // ---------- it stops the run, not the group ----------

        // The whole point. One guard is carried across every group, so 24 groups failing
        // the same way stop after the first 50 tests rather than after nine hours.
        [Test]
        public void OneGuardAcrossGroupsStopsAfterFiftyTestsNotAfterEveryGroup()
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard();
            int attempted = 0;

            // Twenty four groups of 1830, exactly the run that produced nothing.
            for (int group = 0; group < 24 && !guard.ShouldStopTheRun; group++)
            {
                for (int test = 0; test < 1830 && !guard.ShouldStopTheRun; test++)
                {
                    guard.RecordFailure(Disposed);
                    attempted++;
                }
            }

            Assert.That(guard.ShouldStopTheRun, Is.True);
            Assert.That(attempted, Is.EqualTo(50),
                "the run carried on past the point where it was clearly broken");
        }

        // A guard made fresh for each group would never fire across groups, which is why
        // the engine keeps one for the whole run. This pins the difference.
        [Test]
        public void AGuardPerGroupWouldNeverHaveStoppedThatRun()
        {
            int attempted = 0;
            bool stopped = false;

            for (int group = 0; group < 24; group++)
            {
                RepeatedFailureGuard perGroup = new RepeatedFailureGuard(5000);

                for (int test = 0; test < 1830 && !perGroup.ShouldStopTheRun; test++)
                {
                    perGroup.RecordFailure(Disposed);
                    attempted++;
                }

                stopped = stopped || perGroup.ShouldStopTheRun;
            }

            Assert.That(stopped, Is.False);
            Assert.That(attempted, Is.EqualTo(24 * 1830),
                "this is the 43920 tests that nine hours were spent on");
        }
    }
}
