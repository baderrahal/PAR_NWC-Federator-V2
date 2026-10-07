using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// FR-071. Set 03 left the log still for up to 1269 s inside the VIEWS step, so a busy step
    /// and a hung one looked alike and only the processor clause of the hang rule kept the loop
    /// from calling it hung. A progress line is due once the setting's seconds have passed since
    /// the last, and it says how far the step got, how many views it wrote and its seconds.
    /// </summary>
    [TestFixture]
    public class ViewsProgressTests
    {
        [Test]
        public void ALineIsDueOnceTheSettingsSecondsHavePassedSinceTheLast()
        {
            ViewsProgress progress = new ViewsProgress(60.0, 100.0);

            Assert.That(progress.Due(159.9), Is.False);
            Assert.That(progress.Due(160.0), Is.True);
            progress.Line(3, 59, 3, 160.0);
            Assert.That(progress.Due(200.0), Is.False, "the clock starts again at the line");
            Assert.That(progress.Due(220.0), Is.True);
        }

        [Test]
        public void TheLineCarriesTheViewItReachedTheViewsWrittenAndTheSeconds()
        {
            ViewsProgress progress = new ViewsProgress(60.0, 100.0);

            Assert.That(progress.Line(12, 59, 11, 232.5),
                Is.EqualTo("VIEWS view 12 of 59, 11 written, 132.5s into the step"));
        }

        [Test]
        public void ASettingOfZeroOrBelowWritesALineAtEveryView()
        {
            ViewsProgress progress = new ViewsProgress(0.0, 100.0);

            Assert.That(progress.Due(100.0), Is.True);
        }
    }
}
