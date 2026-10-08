using System;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// FR-077. Where the IMAGES step's seconds went, call by call: the render apart from the
    /// save apart from the read back, and what none of them holds.
    ///
    /// Set 03 spent 643.150 s of a 6292.198 s run in IMAGES over 5679 pictures, 0.113 s each,
    /// and how much of each was the render and how much the save was UNKNOWN, because one
    /// watch sat around both. The speed work waits on that answer. The clock is driven here,
    /// so no test waits on a real second and none can fail because the machine was busy.
    /// </summary>
    [TestFixture]
    public class ImagesSecondsTests
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

        private void Spend(ImagesSeconds seconds, ImagesPart part, double howLong)
        {
            using (seconds.In(part))
            {
                now += howLong;
            }
        }

        /// <summary>One picture the way the writer makes it: a visit holding a render, a save and a read back, with bookkeeping between.</summary>
        private void OnePicture(ImagesSeconds seconds, double render, double save, double readBack, double between)
        {
            using (seconds.Visit())
            {
                now += between;
                Spend(seconds, ImagesPart.Rendering, render);
                Spend(seconds, ImagesPart.Saving, save);
                Spend(seconds, ImagesPart.ReadingBack, readBack);
            }
        }

        [Test]
        public void TheRenderTheSaveAndTheReadBackAreApartAndTheRestIsInNoPart()
        {
            ImagesSeconds seconds = new ImagesSeconds(Clock);
            OnePicture(seconds, 0.080, 0.030, 0.001, 0.002);
            OnePicture(seconds, 0.090, 0.020, 0.001, 0.001);

            Assert.That(seconds.Of(ImagesPart.Rendering), Is.EqualTo(0.170).Within(1e-9));
            Assert.That(seconds.Of(ImagesPart.Saving), Is.EqualTo(0.050).Within(1e-9));
            Assert.That(seconds.Of(ImagesPart.ReadingBack), Is.EqualTo(0.002).Within(1e-9));
            Assert.That(seconds.Total, Is.EqualTo(0.225).Within(1e-9));
            Assert.That(seconds.InNoPart, Is.EqualTo(0.003).Within(1e-9));
            Assert.That(seconds.Visits, Is.EqualTo(2));
        }

        /// <summary>The whole is the visits alone. The walk of the harvest between two pictures is HARVEST's and never counted here.</summary>
        [Test]
        public void TheSecondsBetweenTwoVisitsAreNotTheSteps()
        {
            ImagesSeconds seconds = new ImagesSeconds(Clock);
            OnePicture(seconds, 0.100, 0.010, 0.001, 0.000);
            now += 5.0;
            OnePicture(seconds, 0.100, 0.010, 0.001, 0.000);

            Assert.That(seconds.Total, Is.EqualTo(0.222).Within(1e-9));
            Assert.That(seconds.InNoPart, Is.EqualTo(0.0).Within(1e-9));
        }

        [Test]
        public void TheLineNamesEveryPartWithItsSecondsThenWhatNoPartHoldsThenTheWholeAndTheVisits()
        {
            ImagesSeconds seconds = new ImagesSeconds(Clock);
            OnePicture(seconds, 0.080, 0.030, 0.001, 0.002);
            OnePicture(seconds, 0.090, 0.020, 0.001, 0.001);

            Assert.That(seconds.Line(), Is.EqualTo(
                "IMAGES   the step's seconds went: 0.170s rendering, 0.050s saving the JPEG, "
                + "0.002s reading the file back, and 0.003s in none of these, "
                + "of 0.225s over 2 visits to the step"));
        }

        /// <summary>A picture passed over on its status or the cap is a visit that entered no part, and the line says so rather than listing a part at nothing.</summary>
        [Test]
        public void AVisitThatEnteredNoPartIsCountedAndNoPartIsListedAtNothing()
        {
            ImagesSeconds seconds = new ImagesSeconds(Clock);

            using (seconds.Visit())
            {
                now += 0.001;
            }

            Assert.That(seconds.Line(), Is.EqualTo(
                "IMAGES   the step's seconds went: 0.001s in no part, because no part was entered, "
                + "of 0.001s over 1 visit to the step"));
        }

        [Test]
        public void NoVisitAtAllIsSaidAsSuch()
        {
            ImagesSeconds seconds = new ImagesSeconds(Clock);

            Assert.That(seconds.Line(), Is.EqualTo(
                "IMAGES   the step's seconds went: 0.000s in no part, because no part was entered, "
                + "of 0.000s over 0 visits to the step"));
        }

        /// <summary>The writer reads the visit for the tally's seconds per picture, so the two come off one clock.</summary>
        [Test]
        public void AVisitSaysHowLongItHasTakenSoFarAndThenHowLongItTook()
        {
            ImagesSeconds seconds = new ImagesSeconds(Clock);
            double soFar;
            double atTheEnd;

            using (ImagesVisit visit = seconds.Visit())
            {
                now += 0.050;
                soFar = visit.Elapsed;
                now += 0.025;
                atTheEnd = visit.Elapsed;
            }

            Assert.That(soFar, Is.EqualTo(0.050).Within(1e-9));
            Assert.That(atTheEnd, Is.EqualTo(0.075).Within(1e-9));
        }

        /// <summary>A render that throws still ends its stretch, and the seconds it spent failing are counted, the rule every step keeps.</summary>
        [Test]
        public void AStretchThatThrowsStillCountsItsSeconds()
        {
            ImagesSeconds seconds = new ImagesSeconds(Clock);

            try
            {
                using (seconds.Visit())
                {
                    using (seconds.In(ImagesPart.Rendering))
                    {
                        now += 0.200;
                        throw new InvalidOperationException("the renderer gave nothing back");
                    }
                }
            }
            catch (InvalidOperationException)
            {
            }

            Assert.That(seconds.Of(ImagesPart.Rendering), Is.EqualTo(0.200).Within(1e-9));
            Assert.That(seconds.Total, Is.EqualTo(0.200).Within(1e-9));
        }

        [Test]
        public void EndingAStretchTwiceCountsItOnce()
        {
            ImagesSeconds seconds = new ImagesSeconds(Clock);
            IDisposable stretch = seconds.In(ImagesPart.Saving);
            now += 0.030;
            stretch.Dispose();
            now += 0.030;
            stretch.Dispose();

            Assert.That(seconds.Of(ImagesPart.Saving), Is.EqualTo(0.030).Within(1e-9));
        }

        /// <summary>Parts holding more than the whole is a fault in this timing, said in words and never printed as a negative number of seconds.</summary>
        [Test]
        public void PartsAddingToMoreThanTheWholeAreSaidAsAFaultInTheTiming()
        {
            ImagesSeconds seconds = new ImagesSeconds(Clock);

            using (seconds.Visit())
            {
                now += 0.100;
            }

            Spend(seconds, ImagesPart.Rendering, 0.150);

            Assert.That(seconds.Line(), Does.Contain(
                "0.150s rendering, and the parts add to 0.050s more than the whole, "
                + "which is a fault in this timing and not in the run"));
            Assert.That(seconds.Line(), Does.Not.Contain("-0.0"));
        }

        [Test]
        public void TheClockIsRequired()
        {
            Assert.Throws<ArgumentNullException>(delegate { new ImagesSeconds(null); });
        }

        [Test]
        public void EveryPartHasWords()
        {
            foreach (ImagesPart part in Enum.GetValues(typeof(ImagesPart)))
            {
                Assert.That(ImagesSeconds.Describe(part), Is.Not.EqualTo("UNKNOWN"), part.ToString());
            }
        }
    }
}
