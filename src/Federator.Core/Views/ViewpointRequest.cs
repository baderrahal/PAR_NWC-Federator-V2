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
        /// Whether the box starts ticked. TICKED, Bader's answer B to Q131 on 2026-10-05:
        /// unticked until F114, the new viewpoints, merged, so nobody made the old viewpoints,
        /// and ticked once it merges. F114's add-in pass is that merge, so this is true again.
        /// A setting, ReportOptions.MakeViewpoints, read off here and never typed into the
        /// window.
        /// </summary>
        public const bool DefaultMakeViewpoints = true;

        /// <summary>
        /// The tick box on the Clash step. It does not say every clash, because a service of
        /// 150 mm and under gets no viewpoint, SizeSettings.
        /// </summary>
        public const string TickLabel = "Make saved viewpoints for the clashes";

        /// <summary>
        /// The grey line under it, what the box costs. The hours are measured: the C02 weekly
        /// run of 2026-10-05 wrote no log line in VIEWS for 3 h 15 min before it was closed, Q130,
        /// steps\runs\04\item2-C02.
        /// </summary>
        public const string HelpLine = "Each viewpoint adds time, so a big run can take hours";

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
                ? "yes, saved viewpoints are made for the clashes"
                : "no, the box was unticked, so no group makes a viewpoint");
        }

        /// <summary>
        /// The one line the RESULT block carries where the box was unticked, aligned with the
        /// group counts it sits under, or null where it was ticked. A group of an unticked run
        /// is judged DONE without a viewpoint, so RESULT read alone would show the same DONE
        /// count as a run that made them.
        /// </summary>
        public static string ResultLine(bool makeViewpoints)
        {
            return makeViewpoints
                ? null
                : "viewpoints     : none made, the box was unticked for this run";
        }
    }
}
