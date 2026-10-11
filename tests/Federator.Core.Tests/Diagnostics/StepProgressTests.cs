using System;
using Federator.Core.Diagnostics;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F143, Bader's order of 2026-10-11 headed 1A04PK IS FROZEN. Set 06's run of 1A04PK spent 97
    /// minutes in the harvest of one test, BLD-AR-Walls-vs-BLD-AR-Floors, 1556 clashes, with no
    /// line in the log and the window not answering, while the clash call itself took 0.3 s. A
    /// long step now writes a PROGRESS line at least every 30 s naming the step and its count, and
    /// a single Navisworks call of 30 s or more is named with its seconds on a CALL line.
    /// </summary>
    [TestFixture]
    public class StepProgressTests
    {
        [Test]
        public void ALineIsDueOnceTheSecondsHavePassedSinceTheStepOrTheLastLine()
        {
            StepProgress progress = new StepProgress(30.0, 100.0);

            Assert.That(progress.Due(129.9), Is.False);
            Assert.That(progress.Due(130.0), Is.True);
            progress.Line("HARVEST", "BLD-A-vs-BLD-B", "clash", 40, 1556, 130.0);
            Assert.That(progress.Due(159.9), Is.False, "the clock starts again at the line");
            Assert.That(progress.Due(160.0), Is.True);
        }

        [Test]
        public void TheLineNamesTheStepWhatItIsOnItsCountAndTheSecondsIntoTheStep()
        {
            StepProgress progress = new StepProgress(30.0, 100.0);

            Assert.That(progress.Line("HARVEST", "BLD-AR-Walls-vs-BLD-AR-Floors", "clash", 120, 1556, 552.16),
                Is.EqualTo("PROGRESS HARVEST BLD-AR-Walls-vs-BLD-AR-Floors clash 120 of 1556, 452.2s into the step"));
        }

        [Test]
        public void ACountNotKnownIsUnknownAndNeverNought()
        {
            StepProgress progress = new StepProgress(30.0, 0.0);

            Assert.That(progress.Line("IMAGES", "BLD-A-vs-BLD-B", "picture", 3, -1, 31.0),
                Is.EqualTo("PROGRESS IMAGES BLD-A-vs-BLD-B picture 3 of UNKNOWN, 31s into the step"));
        }

        [Test]
        public void ASettingOfZeroOrBelowWritesALineAtEveryPiece()
        {
            StepProgress progress = new StepProgress(0.0, 100.0);

            Assert.That(progress.Due(100.0), Is.True);
            progress.Line("HARVEST", "t", "clash", 1, 2, 100.0);
            Assert.That(progress.Due(100.0), Is.True);
        }

        [Test]
        public void ACallUnderTheLineIsNotNamed()
        {
            Assert.That(LongCall.Line("TestsImageForResult", "BLD-A-vs-BLD-B clash 3 of 9", 29.99, 30.0), Is.Null);
        }

        [Test]
        public void ACallAtTheLineIsNamedWithItsSecondsAndWhatItWasOn()
        {
            Assert.That(LongCall.Line("TestsImageForResult", "BLD-A-vs-BLD-B clash 3 of 9", 34.25, 30.0),
                Is.EqualTo("CALL     TestsImageForResult on BLD-A-vs-BLD-B clash 3 of 9 took 34.3s, one Navisworks call, so the window could not answer for that long"));
        }

        [Test]
        public void ACallOfNoKnownLengthIsNotNamed()
        {
            Assert.That(LongCall.Line("TestsRunTest", "x", double.NaN, 30.0), Is.Null);
            Assert.That(LongCall.Line("TestsRunTest", "x", -1.0, 30.0), Is.Null);
        }

        [Test]
        public void TheDefaultsAreThirtySecondsEach()
        {
            ProgressSettings settings = new ProgressSettings();

            Assert.That(settings.EverySeconds, Is.EqualTo(30.0));
            Assert.That(settings.LongCallSeconds, Is.EqualTo(30.0));
        }

        [Test]
        public void ALongCallLineOfNoneOrBelowIsRefusedWhereItIsSet()
        {
            ProgressSettings settings = new ProgressSettings();

            Assert.Throws<ArgumentOutOfRangeException>(() => settings.LongCallSeconds = 0.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.LongCallSeconds = -5.0);
            Assert.That(settings.LongCallSeconds, Is.EqualTo(30.0), "a refused value leaves the setting as it was");
        }

        [Test]
        public void TheViewsStepTakesTheOneProgressDefault()
        {
            Assert.That(new ViewpointSettings().ProgressEverySeconds, Is.EqualTo(ProgressSettings.DefaultEverySeconds));
            Assert.That(ProgressSettings.DefaultEverySeconds, Is.EqualTo(30.0));
        }

        // F146, the reviewer's point: the harvest and the renumber said how far they had got with
        // two copies of these words, which had already drifted on a count not known.
        [Test]
        public void TheCountWordsAreOneRuleForEveryCaller()
        {
            Assert.That(StepProgress.Count(3, 10), Is.EqualTo("3 of 10"));
            Assert.That(StepProgress.Count(4, -1), Is.EqualTo("4 of UNKNOWN"));
            Assert.That(StepProgress.Live("1A02MM", "picture renamed", 12, 2931), Is.EqualTo("1A02MM, picture renamed 12 of 2931"));
            Assert.That(StepProgress.RowText("RENUMBER", "picture renamed", 12, 2931), Is.EqualTo("RENUMBER picture renamed 12 of 2931"));
            Assert.That(StepProgress.RowText("HARVEST", "item under a group", 7, -1), Is.EqualTo("HARVEST item under a group 7 of UNKNOWN"));
            Assert.That(new StepProgress(30, 0).Line("RENUMBER", "1A02MM", "picture renamed", 12, 2931, 31.0),
                Is.EqualTo("PROGRESS RENUMBER 1A02MM picture renamed 12 of 2931, 31s into the step"));
        }
    }
}
