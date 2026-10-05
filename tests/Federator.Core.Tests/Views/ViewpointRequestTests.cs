using Federator.Core.Health;
using Federator.Core.Report;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F136. Whether a group asks for its saved viewpoints, given the box on the Clash
    /// step, the clash skipped and no report, and the line that says why not. Each test
    /// breaks one of the three and asserts the line names it, because a rule only proved
    /// on the group that makes viewpoints proves nothing about the box.
    /// </summary>
    [TestFixture]
    public class ViewpointRequestTests
    {
        [Test]
        public void TheBoxTickedWithAReportAndAClashAsksForThem()
        {
            Assert.That(ViewpointRequest.WhyNone(true, false, true), Is.Null);
        }

        [Test]
        public void TheBoxUntickedAsksForNoneAndSaysTheBoxWasUnticked()
        {
            string why = ViewpointRequest.WhyNone(false, false, true);

            Assert.That(why, Is.Not.Null, "an unticked box asked for viewpoints");
            Assert.That(why, Does.Contain("unticked"));
            Assert.That(why, Does.Contain(ViewpointRequest.TickLabel), "the line names the box a person unticked");
            Assert.That(why, Does.EndWith("so no viewpoint is made"));
        }

        /// <summary>
        /// The box is the run's own choice and holds for every group, so it is the reason
        /// named even where the clash was skipped or no report was built.
        /// </summary>
        [Test]
        public void TheBoxUntickedIsTheReasonWhateverElseHeldTheGroup()
        {
            string alone = ViewpointRequest.WhyNone(false, false, true);

            Assert.That(ViewpointRequest.WhyNone(false, true, true), Is.EqualTo(alone));
            Assert.That(ViewpointRequest.WhyNone(false, false, false), Is.EqualTo(alone));
            Assert.That(ViewpointRequest.WhyNone(false, true, false), Is.EqualTo(alone));
        }

        [Test]
        public void TheClashSkippedAsksForNoneAndSaysTheClashWasSkipped()
        {
            Assert.That(
                ViewpointRequest.WhyNone(true, true, true),
                Is.EqualTo(OffCoordinates.ClashSkippedReason + ", so no viewpoint is made"));
        }

        [Test]
        public void NoReportAsksForNoneAndSaysThereWasNoReport()
        {
            Assert.That(
                ViewpointRequest.WhyNone(true, false, false),
                Is.EqualTo("no report was built for this group, so there is nothing to plan a viewpoint from"));
        }

        /// <summary>The clash skipped is named before the missing report, as the engine said it before F136.</summary>
        [Test]
        public void TheClashSkippedIsNamedBeforeTheMissingReport()
        {
            Assert.That(
                ViewpointRequest.WhyNone(true, true, false),
                Is.EqualTo(ViewpointRequest.WhyNone(true, true, true)));
        }

        [Test]
        public void TheBoxStartsTickedOffTheSetting()
        {
            Assert.That(ViewpointRequest.DefaultMakeViewpoints, Is.True, "Q131 default A");
            Assert.That(new ReportOptions().MakeViewpoints, Is.EqualTo(ViewpointRequest.DefaultMakeViewpoints));
        }

        [Test]
        public void TheLabelIsEightWordsAtMostAndCarriesNoIdentifier()
        {
            Assert.That(ViewpointRequest.TickLabel.Split(' ').Length, Is.LessThanOrEqualTo(8));
            Assert.That(ViewpointRequest.TickLabel, Does.Not.Contain("MakeViewpoints"));
            Assert.That(ViewpointRequest.TickLabel, Is.Not.EqualTo(ViewpointRequest.TickLabel.ToUpperInvariant()));
        }

        [Test]
        public void TheGreyLineIsTwelveWordsAtMost()
        {
            Assert.That(ViewpointRequest.HelpLine.Split(' ').Length, Is.LessThanOrEqualTo(12));
            Assert.That(ViewpointRequest.HelpLine, Does.Not.Contain("MakeViewpoints"));
        }

        [Test]
        public void TheSettingsLineSaysWhichWayTheBoxWasSet()
        {
            Assert.That(ViewpointRequest.SettingsLine(true), Does.StartWith("viewpoints       : yes"));
            Assert.That(ViewpointRequest.SettingsLine(false), Does.StartWith("viewpoints       : no"));
            Assert.That(ViewpointRequest.SettingsLine(false), Does.Contain("unticked"));
        }
    }
}
