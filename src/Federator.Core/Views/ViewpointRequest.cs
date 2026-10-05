using Federator.Core.Health;

namespace Federator.Core.Views
{
    /// <summary>
    /// Whether a group asks for its saved viewpoints, F136, and the one VIEWS line that
    /// says why not when it does not. Three things stop them, in this order: the box on
    /// the Clash step unticked, the group's clash skipped because a model is not on the
    /// same shared coordinates, and no report built for the group. A group that asks for
    /// none cannot fail at them, which is F52's rule for a step not asked for.
    ///
    /// The engine asked the second and third itself before this, and the box is Bader's
    /// word of 2026-10-05: until the new viewpoints of F114 are merged every test run has
    /// viewpoints switched off. His message says the C02 weekly run had gone about four
    /// hours with the old viewpoints, the NWF holding them by the thousand and every new
    /// one slower than the last.
    /// </summary>
    public static class ViewpointRequest
    {
        /// <summary>
        /// Whether the box starts ticked, Q131 default A. A setting,
        /// ReportOptions.MakeViewpoints, read off here and never typed into the window.
        /// </summary>
        public const bool DefaultMakeViewpoints = true;

        /// <summary>The tick box on the Clash step. Seven words.</summary>
        public const string TickLabel = "Make a saved viewpoint for every clash";

        /// <summary>The grey line under it. Twelve words.</summary>
        public const string HelpLine = "Unticked, no viewpoint is made. The clash, workbook and NWD still run";

        /// <summary>
        /// Why the group asks for no viewpoint, or null when it asks for them. The box
        /// comes first because it is the run's own choice and holds for every group, so
        /// every group of an unticked run says the same thing.
        /// </summary>
        public static string WhyNone(bool makeViewpoints, bool clashSkipped, bool reportBuilt)
        {
            if (!makeViewpoints)
            {
                return "the box " + TickLabel + " was unticked, so no viewpoint is made";
            }

            if (clashSkipped)
            {
                // Bader's answer to Q99 and Q100.
                return OffCoordinates.ClashSkippedReason + ", so no viewpoint is made";
            }

            if (!reportBuilt)
            {
                return "no report was built for this group, so there is nothing to plan a viewpoint from";
            }

            return null;
        }

        /// <summary>
        /// The line the run's settings block carries, aligned with the lines around it,
        /// so the log says at the start of the run whether this run makes viewpoints.
        /// </summary>
        public static string SettingsLine(bool makeViewpoints)
        {
            return "viewpoints       : " + (makeViewpoints
                ? "yes, one saved viewpoint per clash"
                : "no, the box was unticked, so no group makes a viewpoint");
        }
    }
}
