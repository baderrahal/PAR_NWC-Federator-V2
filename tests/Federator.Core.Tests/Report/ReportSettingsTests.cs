using System;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Every number and name that shapes a run is a setting and not a constant. These
    /// change each one and read the answer back out of the thing that uses it, and hand
    /// each one a value it must refuse, because a test that only sets a good value proves
    /// the default and nothing else.
    /// </summary>
    [TestFixture]
    public class ReportSettingsTests
    {
        [TearDown]
        public void PutTheSettingBack()
        {
            ReportPaths.Subfolder = ReportPaths.DefaultSubfolder;
        }

        [Test]
        public void TheSubfolderStartsAtTheMeasuredDefault()
        {
            Assert.That(ReportPaths.Subfolder, Is.EqualTo("Clash Reports"));
        }

        [Test]
        public void TheFolderChosenFollowsTheSubfolderSetting()
        {
            ReportPaths.Subfolder = "Weekly Clash";

            string folder = ReportPaths.Choose(null, "C:\\out\\NWF", null).Folder;

            Assert.That(folder, Does.EndWith("Weekly Clash"));
            Assert.That(folder, Does.Not.Contain("Clash Reports"));
        }

        [Test]
        public void TheLineUnderTheOutputsStepFollowsItToo()
        {
            ReportPaths.Subfolder = "Weekly Clash";

            Assert.That(
                ReportPaths.WhereTheyGo(null, "C:\\out\\NWF", null),
                Does.Contain("Weekly Clash"));
        }

        [Test]
        public void ASubfolderThatIsNoNameIsRefused()
        {
            Assert.Throws<ArgumentException>(delegate { ReportPaths.Subfolder = null; });
            Assert.Throws<ArgumentException>(delegate { ReportPaths.Subfolder = "   "; });
            Assert.That(ReportPaths.Subfolder, Is.EqualTo("Clash Reports"));
        }

        [Test]
        public void ASubfolderThatIsAPathIsRefused()
        {
            ArgumentException refused = Assert.Throws<ArgumentException>(
                delegate { ReportPaths.Subfolder = "Reports\\Weekly"; });

            Assert.That(refused.Message, Does.Contain("one folder name"));
            Assert.That(ReportPaths.Subfolder, Is.EqualTo("Clash Reports"));
        }

        [Test]
        public void TheStopAfterCountStartsAtTheGuardsOwnDefault()
        {
            Assert.That(
                new ReportOptions().StopAfterFailures,
                Is.EqualTo(RepeatedFailureGuard.DefaultThreshold));
        }

        [Test]
        public void TheStopAfterCountCanBeChanged()
        {
            ReportOptions options = new ReportOptions();
            options.StopAfterFailures = 3;

            Assert.That(options.StopAfterFailures, Is.EqualTo(3));
        }

        [Test]
        public void AStopAfterCountThatCouldNeverFireIsRefused()
        {
            ReportOptions options = new ReportOptions();

            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { options.StopAfterFailures = 0; });
            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { options.StopAfterFailures = -1; });
            Assert.That(options.StopAfterFailures, Is.EqualTo(RepeatedFailureGuard.DefaultThreshold));
        }

        [Test]
        public void TheGuardBuiltFromTheSettingStopsAfterThatMany()
        {
            ReportOptions options = new ReportOptions();
            options.StopAfterFailures = 2;

            RepeatedFailureGuard guard = new RepeatedFailureGuard(options.StopAfterFailures);
            guard.RecordFailure("the same reason");

            Assert.That(guard.ShouldStopTheRun, Is.False);

            guard.RecordFailure("the same reason");

            Assert.That(guard.ShouldStopTheRun, Is.True);
        }
    }
}
