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

        /// <summary>
        /// Bader's answer B to Q131 on 2026-10-05: unticked when the window opens until F114
        /// merges, so nobody makes the old viewpoints, and ticked once F114 merges. F114's
        /// add-in pass is that merge, so the box opens ticked. The window opens with the
        /// setting's value, so the setting is what this pins.
        /// </summary>
        [Test]
        public void TheBoxStartsTickedOffTheSetting()
        {
            Assert.That(ViewpointRequest.DefaultMakeViewpoints, Is.True, "Q131 answer B, ticked once F114 merges");
            Assert.That(new ReportOptions().MakeViewpoints, Is.True, "the window opens with the box ticked");
        }

        [Test]
        public void TheLabelIsEightWordsAtMostAndCarriesNoIdentifier()
        {
            Assert.That(ViewpointRequest.TickLabel.Split(' ').Length, Is.LessThanOrEqualTo(8));
            Assert.That(ViewpointRequest.TickLabel, Does.Not.Contain("MakeViewpoints"));
            Assert.That(ViewpointRequest.TickLabel, Is.Not.EqualTo(ViewpointRequest.TickLabel.ToUpperInvariant()));
        }

        /// <summary>
        /// A service of 150 mm and under gets no viewpoint, so a label or a settings line
        /// saying every clash, or one per clash, says more than the code does.
        /// </summary>
        [Test]
        public void TheLabelAndTheSettingsLineDoNotClaimEveryClash()
        {
            Assert.That(ViewpointRequest.TickLabel, Does.Not.Contain("every"));
            Assert.That(ViewpointRequest.SettingsLine(true), Does.Not.Contain("every"));
            Assert.That(ViewpointRequest.SettingsLine(true), Does.Not.Contain("per clash"));
        }

        /// <summary>
        /// The tick box rule says the grey line names what the box costs and does not describe
        /// the off state, so a line about what happens unticked breaks it.
        /// </summary>
        [Test]
        public void TheGreyLineIsTwelveWordsAtMostAndDoesNotDescribeTheOffState()
        {
            Assert.That(ViewpointRequest.HelpLine.Split(' ').Length, Is.LessThanOrEqualTo(12));
            Assert.That(ViewpointRequest.HelpLine, Does.Not.Contain("MakeViewpoints"));
            Assert.That(ViewpointRequest.HelpLine, Does.Not.Contain("nticked"));
        }

        [Test]
        public void TheSettingsLineSaysWhichWayTheBoxWasSet()
        {
            Assert.That(ViewpointRequest.SettingsLine(true), Does.StartWith("viewpoints       : yes"));
            Assert.That(ViewpointRequest.SettingsLine(false), Does.StartWith("viewpoints       : no"));
            Assert.That(ViewpointRequest.SettingsLine(false), Does.Contain("unticked"));
        }

        /// <summary>
        /// The RESULT block names the viewpoints off in one line, aligned with the group
        /// counts above it, and says nothing where the box was ticked.
        /// </summary>
        [Test]
        public void TheResultLineNamesTheViewpointsOffOnlyWhereTheBoxWasUnticked()
        {
            Assert.That(ViewpointRequest.ResultLine(true), Is.Null);

            string off = ViewpointRequest.ResultLine(false);

            Assert.That(off, Is.Not.Null, "an unticked run said nothing about viewpoints in RESULT");
            Assert.That(off, Does.StartWith("viewpoints     : "));
            Assert.That(off, Does.Contain("unticked"));
            Assert.That(off.Split('\n').Length, Is.EqualTo(1), "one line");
        }
    }
}
