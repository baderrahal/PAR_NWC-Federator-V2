using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Health;
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

        /// <summary>
        /// Q98 B2. The far model distance is a setting the run reads, a metre by default,
        /// Bader's number, and asked again with the 40 distances of the C06 run as Q99.
        /// </summary>
        [Test]
        public void TheFarModelDistanceStartsAtOneMetre()
        {
            Assert.That(
                new ReportOptions().FarModelMillimetres,
                Is.EqualTo(AlignmentCheck.DefaultFarModelMillimetres));
            Assert.That(new ReportOptions().FarModelMillimetres, Is.EqualTo(1000.0));
        }

        [Test]
        public void TheFarModelDistanceCanBeChangedAndTheRuleReadsIt()
        {
            ReportOptions options = new ReportOptions();
            options.FarModelMillimetres = 2000.0;

            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                new ModelPlacement("a-AR.nwc", "AR", "Site", 0.0, 0.0, 0.0),
                new ModelPlacement("a-EL.nwc", "EL", "Site", 1500.0, 0.0, 0.0)
            };

            Assert.That(options.FarModelMillimetres, Is.EqualTo(2000.0));
            Assert.That(AlignmentCheck.NotOnTheSameCoordinates(models, options.FarModelMillimetres).Any, Is.False);
            Assert.That(
                AlignmentCheck.NotOnTheSameCoordinates(models, new ReportOptions().FarModelMillimetres).Models.Count,
                Is.EqualTo(1));
        }

        /// <summary>
        /// Bader's answer to Q99 and Q100: the rule that skips the clash of a group not on
        /// the same shared coordinates is ON by default, and a setting switches it off,
        /// because a building is run once more with it off so every other fix is proved on
        /// groups that clash.
        /// </summary>
        [Test]
        public void TheRuleThatSkipsTheClashIsOnByDefaultAndCanBeSwitchedOff()
        {
            ReportOptions options = new ReportOptions();

            Assert.That(options.SkipClashOffCoordinates, Is.True);
            Assert.That(options.SkipClashOffCoordinates, Is.EqualTo(AlignmentCheck.DefaultSkipClashOffCoordinates));

            options.SkipClashOffCoordinates = false;

            Assert.That(options.SkipClashOffCoordinates, Is.False);
        }

        /// <summary>
        /// Not a number would switch the rule off without a word, because nothing is more
        /// than it, and a distance below zero would call a model sitting exactly on its
        /// reference far.
        /// </summary>
        [Test]
        public void AFarModelDistanceThatIsNotALengthIsRefused()
        {
            ReportOptions options = new ReportOptions();

            Assert.Throws<ArgumentOutOfRangeException>(delegate { options.FarModelMillimetres = -1.0; });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { options.FarModelMillimetres = double.NaN; });
            Assert.That(options.FarModelMillimetres, Is.EqualTo(AlignmentCheck.DefaultFarModelMillimetres));
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
