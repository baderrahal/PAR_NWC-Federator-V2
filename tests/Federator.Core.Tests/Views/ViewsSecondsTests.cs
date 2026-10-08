using System;
using System.Collections.Generic;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// FR-073. Where the VIEWS step's seconds went, call by call, and how many no part holds.
    ///
    /// Set 03 split them four ways and left 67.5 s of 1B06G1 and 131.9 s of 1B06PP in none
    /// of the four, and its recording part was one watch around three calls, the folders,
    /// the COM folder and the COM add, so which call cost 1602.798 s of 1B06G1's 1769.389 s
    /// was UNKNOWN. The speed work waits on that answer. The clock is driven here, so no test
    /// waits on a real second and none can fail because the machine was busy.
    /// </summary>
    [TestFixture]
    public class ViewsSecondsTests
    {
        private double now;

        [SetUp]
        public void StartTheClock()
        {
            now = 0.0;
        }

        private double Clock()
        {
            return now;
        }

        private void Spend(ViewsSeconds seconds, ViewsPart part, double howLong)
        {
            using (seconds.In(part))
            {
                now += howLong;
            }
        }

        [Test]
        public void ThePartsAndWhatNoPartHoldsAddUpToTheWhole()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            Spend(seconds, ViewsPart.ReadingTheClashes, 30.0);
            now += 2.5;
            Spend(seconds, ViewsPart.AddingTheView, 100.0);
            now += 0.5;
            seconds.Ended();

            Assert.That(seconds.Total, Is.EqualTo(133.0).Within(1e-9));
            Assert.That(seconds.InNoPart, Is.EqualTo(3.0).Within(1e-9));
            Assert.That(
                seconds.Of(ViewsPart.ReadingTheClashes) + seconds.Of(ViewsPart.AddingTheView) + seconds.InNoPart,
                Is.EqualTo(seconds.Total).Within(1e-9));
        }

        [Test]
        public void TheLineNamesEveryPartWithItsSecondsAndThenWhatNoPartHolds()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            Spend(seconds, ViewsPart.ReadingTheClashes, 30.0);
            now += 2.5;
            Spend(seconds, ViewsPart.AddingTheView, 100.0);
            now += 0.5;
            seconds.Ended();

            Assert.That(seconds.Line(), Is.EqualTo(
                "VIEWS    the step's seconds went: 30.000s reading the clashes and planning, "
                + "100.000s adding the view, and 3.000s in none of these, "
                + "of 133.000s from the first clash read to the document put back"));
        }

        /// <summary>
        /// The split the speed work waits on. Recording was one watch around three calls,
        /// so the one word over them is gone and each call carries its own seconds.
        /// </summary>
        [Test]
        public void RecordingIsSplitIntoTheCallsItIsMadeOf()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            Spend(seconds, ViewsPart.MakingTheFolders, 1.0);
            Spend(seconds, ViewsPart.MakingTheView, 2.0);
            Spend(seconds, ViewsPart.FindingTheFolder, 3.0);
            Spend(seconds, ViewsPart.AddingTheView, 4.0);
            seconds.Ended();

            string line = seconds.Line();

            Assert.That(line, Does.Contain("1.000s finding or making the folders"));
            Assert.That(line, Does.Contain("2.000s making the view and setting its camera"));
            Assert.That(line, Does.Contain("3.000s finding its folder again in the COM tree"));
            Assert.That(line, Does.Contain("4.000s adding the view"));
            Assert.That(line, Does.Not.Contain("recording"),
                "one word over three calls is what hid which call cost the time");
        }

        /// <summary>
        /// The seconds that fall between the parts are a part of their own and never spread
        /// over the others, which would be inventing numbers, the timing block's own rule.
        /// </summary>
        [Test]
        public void TheSecondsBetweenThePartsAreNeverSpreadOverThem()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            now += 7.0;
            Spend(seconds, ViewsPart.ShowingAndHiding, 1.25);
            now += 11.0;
            Spend(seconds, ViewsPart.ReadingBack, 0.75);
            now += 3.0;
            seconds.Ended();

            Assert.That(seconds.Of(ViewsPart.ShowingAndHiding), Is.EqualTo(1.25).Within(1e-9));
            Assert.That(seconds.Of(ViewsPart.ReadingBack), Is.EqualTo(0.75).Within(1e-9));
            Assert.That(seconds.InNoPart, Is.EqualTo(21.0).Within(1e-9));
            Assert.That(seconds.Line(), Does.Contain("and 21.000s in none of these, of 23.000s"));
        }

        [Test]
        public void APartEnteredManyTimesCarriesEveryOneOfItsStretches()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);

            for (int i = 0; i < 1165; i++)
            {
                Spend(seconds, ViewsPart.AddingTheView, 1.0);
                now += 0.25;
            }

            seconds.Ended();

            Assert.That(seconds.Of(ViewsPart.AddingTheView), Is.EqualTo(1165.0).Within(1e-6));
            Assert.That(seconds.InNoPart, Is.EqualTo(291.25).Within(1e-6));
            Assert.That(seconds.Line(), Does.Contain("1165.000s adding the view"));
        }

        /// <summary>
        /// The route the run takes by default never adds at the root and moves, so that part
        /// is never entered, and a part the work never entered is left off rather than read
        /// as a call that cost nothing.
        /// </summary>
        [Test]
        public void APartTheWorkNeverEnteredIsLeftOffTheLine()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            Spend(seconds, ViewsPart.FindingTheFolder, 1.0);
            Spend(seconds, ViewsPart.AddingTheView, 1.0);
            seconds.Ended();

            string line = seconds.Line();

            Assert.That(line, Does.Not.Contain(ViewsSeconds.Describe(ViewsPart.MovingIntoTheFolder)));
            Assert.That(line, Does.Not.Contain(ViewsSeconds.Describe(ViewsPart.Dimming)));
        }

        [Test]
        public void APartEnteredThatCostNothingIsStillNamed()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            Spend(seconds, ViewsPart.Framing, 0.0);
            seconds.Ended();

            Assert.That(seconds.Line(), Does.Contain("0.000s framing the camera on the clashes"));
        }

        [Test]
        public void ThePartsAreNamedInTheOrderAViewpointMeetsThem()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            Spend(seconds, ViewsPart.PuttingBack, 1.0);
            Spend(seconds, ViewsPart.ReadingBack, 1.0);
            Spend(seconds, ViewsPart.ReadingTheClashes, 1.0);
            seconds.Ended();

            string line = seconds.Line();
            int reading = line.IndexOf(ViewsSeconds.Describe(ViewsPart.ReadingTheClashes), StringComparison.Ordinal);
            int back = line.IndexOf(ViewsSeconds.Describe(ViewsPart.ReadingBack), StringComparison.Ordinal);
            int putting = line.IndexOf(ViewsSeconds.Describe(ViewsPart.PuttingBack), StringComparison.Ordinal);

            Assert.That(reading, Is.GreaterThan(0));
            Assert.That(back, Is.GreaterThan(reading));
            Assert.That(putting, Is.GreaterThan(back));
        }

        /// <summary>
        /// The break. A stretch left open across the end puts more into the parts than the
        /// whole took, which is a fault in this timing and not in the run, and the line says
        /// so in words rather than printing a negative number of seconds in no part.
        /// </summary>
        [Test]
        public void PartsAddingToMoreThanTheWholeAreSaidInWordsAndNeverAsANegative()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            IDisposable open = seconds.In(ViewsPart.AddingTheView);
            now += 10.0;
            seconds.Ended();
            now += 5.0;
            open.Dispose();

            string line = seconds.Line();

            Assert.That(seconds.InNoPart, Is.EqualTo(-5.0).Within(1e-9));
            Assert.That(line, Does.Contain(
                "and the parts add to 5.000s more than the whole, which is a fault in this timing and not in the run"));
            Assert.That(line, Does.Not.Contain("in none of these"));
            Assert.That(line, Does.Not.Contain("-5"));
        }

        [Test]
        public void AStretchEndedTwiceCountsOnce()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            IDisposable stretch = seconds.In(ViewsPart.Dimming);
            now += 2.0;
            stretch.Dispose();
            now += 3.0;
            stretch.Dispose();

            Assert.That(seconds.Of(ViewsPart.Dimming), Is.EqualTo(2.0).Within(1e-9));
        }

        [Test]
        public void TheWholeIsTakenWhenItFirstEndsAndNotAgain()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            now += 4.0;
            seconds.Ended();
            now += 9.0;
            seconds.Ended();

            Assert.That(seconds.Total, Is.EqualTo(4.0).Within(1e-9));
        }

        [Test]
        public void BeforeItEndsTheWholeIsTheSecondsSoFar()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            now += 6.0;

            Assert.That(seconds.Total, Is.EqualTo(6.0).Within(1e-9));
        }

        /// <summary>
        /// The clock is monotonic and still nothing here trusts it to be. A stretch over a
        /// clock that went back is no time at all and never less, which is the rule every
        /// step in the log already keeps.
        /// </summary>
        [Test]
        public void AClockThatGoesBackGivesAStretchNoTimeRatherThanLess()
        {
            now = 10.0;
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            IDisposable stretch = seconds.In(ViewsPart.ReadingBack);
            now = 4.0;
            stretch.Dispose();

            Assert.That(seconds.Of(ViewsPart.ReadingBack), Is.EqualTo(0.0));
            Assert.That(seconds.Total, Is.EqualTo(0.0));
        }

        [Test]
        public void WithNoPartEnteredTheLineStillSaysWhereTheSecondsWent()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            now += 0.5;
            seconds.Ended();

            Assert.That(seconds.Line(), Is.EqualTo(
                "VIEWS    the step's seconds went: 0.500s in no part, because no part was entered, "
                + "of 0.500s from the first clash read to the document put back"));
        }

        /// <summary>
        /// The parts of a per test view the add-in pass of F114 times, each a call of its own:
        /// the frame, the mark, the inventory, the removal and the tree read, in the order a
        /// view meets them, so the line reads as the step ran.
        /// </summary>
        [Test]
        public void TheAddInPassPartsAreNamedInTheOrderAViewMeetsThem()
        {
            ViewsSeconds seconds = new ViewsSeconds(Clock);
            Spend(seconds, ViewsPart.ReadingTheTree, 1.0);
            Spend(seconds, ViewsPart.PuttingBack, 1.0);
            Spend(seconds, ViewsPart.Removing, 1.0);
            Spend(seconds, ViewsPart.TakingTheInventory, 1.0);
            Spend(seconds, ViewsPart.ReadingBack, 1.0);
            Spend(seconds, ViewsPart.Marking, 1.0);
            Spend(seconds, ViewsPart.AddingTheView, 1.0);
            Spend(seconds, ViewsPart.MakingTheFolders, 1.0);
            Spend(seconds, ViewsPart.Framing, 1.0);
            Spend(seconds, ViewsPart.Dimming, 1.0);
            seconds.Ended();

            string line = seconds.Line();
            ViewsPart[] order =
            {
                ViewsPart.Dimming, ViewsPart.Framing, ViewsPart.MakingTheFolders, ViewsPart.AddingTheView,
                ViewsPart.Marking, ViewsPart.ReadingBack, ViewsPart.TakingTheInventory, ViewsPart.Removing,
                ViewsPart.PuttingBack, ViewsPart.ReadingTheTree
            };

            for (int i = 1; i < order.Length; i++)
            {
                int before = line.IndexOf(ViewsSeconds.Describe(order[i - 1]), StringComparison.Ordinal);
                int after = line.IndexOf(ViewsSeconds.Describe(order[i]), StringComparison.Ordinal);

                Assert.That(before, Is.GreaterThanOrEqualTo(0), order[i - 1].ToString());
                Assert.That(after, Is.GreaterThan(before), order[i] + " comes after " + order[i - 1]);
            }

            Assert.That(ViewsSeconds.Describe(ViewsPart.Framing), Is.EqualTo("framing the camera on the clashes"));
            Assert.That(ViewsSeconds.Describe(ViewsPart.Marking), Is.EqualTo("marking the view and its folders"));
            Assert.That(ViewsSeconds.Describe(ViewsPart.TakingTheInventory), Is.EqualTo("taking the inventory of the tree"));
            Assert.That(ViewsSeconds.Describe(ViewsPart.Removing), Is.EqualTo("removing the views of earlier runs"));
            Assert.That(ViewsSeconds.Describe(ViewsPart.ReadingTheTree), Is.EqualTo("reading the tree for the VIEWS TREE block"));
        }

        [Test]
        public void EveryPartHasWordsOfItsOwn()
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (ViewsPart part in Enum.GetValues(typeof(ViewsPart)))
            {
                string words = ViewsSeconds.Describe(part);

                Assert.That(words, Is.Not.Empty, part.ToString());
                Assert.That(words, Does.Not.Contain("UNKNOWN"), part.ToString());
                Assert.That(seen.Add(words), Is.True, "two parts read the same: " + words);
            }
        }

        [Test]
        public void ANullClockIsRefused()
        {
            Assert.That(() => new ViewsSeconds(null), Throws.ArgumentNullException);
        }
    }
}
