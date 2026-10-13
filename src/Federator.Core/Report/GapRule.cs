using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;

namespace Federator.Core.Report
{
    /// <summary>
    /// What counts as a gap, and the block that says so at the end of every group.
    ///
    /// BADER'S STANDING RULE, BUILT INTO THE TOOL. When the code knows something the
    /// report does not show, it becomes a question in the next round. That rule has been
    /// worked by hand every time: somebody reads the code, notices a property being read
    /// and written nowhere, and writes a question. Q25 is exactly that, and it took an
    /// audit of every file under src to find it. This makes the run say it itself.
    ///
    /// WHAT A GAP IS. Something the run MEASURED off the model, that no output carries.
    /// Not something the code could have measured and did not, and not something the
    /// report leaves out on purpose that a reader can see for themselves. It has to be a
    /// number the run paid for and then threw away.
    ///
    /// WHAT IT IS NOT. It is not a fault and nothing acts on it. A gap does not fail a
    /// group, does not stop a run and does not change a single output. It is information
    /// in the log, which is the rule this whole tool is built on.
    ///
    /// THE TWO, AND WHY THEY ARE THE TWO. There were six, measured on 2026-09-19 by
    /// reading all three writers. Bader deleted Family, Type Name, Material and the per
    /// item discipline with their harvest on 2026-10-06, Q35 to Q39, FR-198. The two left
    /// are read off every item and reach no output, each kept for one reader: the source
    /// file for the mirror merge's key and the id's property for the ITEM IDS block.
    ///
    /// READ AND EMPTY EVERY TIME, Q76. A property read on every item that came back empty
    /// on every one is said with its count. It was left out, so a block naming nothing
    /// could not be told from a block whose outputs carried everything.
    /// </summary>
    public static class GapRule
    {
        /// <summary>The block's title.</summary>
        public const string BlockTitle = "GAP";

        /// <summary>
        /// The line prefix, padded the way every other kind in the log is, so a gap line
        /// lines up with the rest of the file.
        /// </summary>
        public const string Prefix = "GAP      ";

        /// <summary>Every gap in one group's report, in the order they are always listed.</summary>
        public static IList<ReportGap> For(ClashReport report)
        {
            List<ReportGap> gaps = new List<ReportGap>();

            if (report == null)
            {
                return gaps;
            }

            int items = 0;
            int sourceFile = 0;
            int idFrom = 0;

            foreach (TestReport test in report.Tests)
            {
                foreach (ClashRow row in test.Rows)
                {
                    foreach (ClashItem item in new[] { row.Left, row.Right })
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        items++;

                        if (!string.IsNullOrEmpty(item.SourceFile)) { sourceFile++; }
                        if (!string.IsNullOrEmpty(item.IdFrom)) { idFrom++; }
                    }
                }
            }

            Add(
                gaps,
                "Source File",
                sourceFile,
                items,
                "the mirror merge reads it and the SOURCE FINDINGS block names each model's source, Q38");
            Add(
                gaps,
                "Id From",
                idFrom,
                items,
                "the ITEM IDS block counts it per property and no output names it per item");

            return gaps;
        }

        /// <summary>
        /// The whole block, written even when it is empty. An empty GAP block saying
        /// nothing was held back is worth more than no block at all, because a missing
        /// block reads as a check that did not run. noReportBecause is why this group has
        /// no report, where the engine knows a reason the empty block's words would hide, or
        /// null: a group whose clash was skipped for the coordinates said the report
        /// carries everything the run read, about a report that was never made.
        /// </summary>
        public static IList<string> Lines(ClashReport report, string noReportBecause = null)
        {
            IList<ReportGap> gaps = For(report);
            List<string> lines = new List<string>();

            if (report == null && !string.IsNullOrEmpty(noReportBecause))
            {
                lines.Add("no clash report was made, " + noReportBecause
                    + ", so nothing was measured that an output could hold back");
                return lines;
            }

            if (gaps.Count == 0)
            {
                lines.Add(NothingHeldBack());
                return lines;
            }

            // For gives both properties or none, so the count is never one.
            lines.Add(gaps.Count + " numbers are"
                + " measured off every item and reach no output. Nothing here is a fault "
                + "and nothing acts on it");

            foreach (ReportGap gap in gaps)
            {
                lines.Add(gap.Line());
            }

            return lines;
        }

        /// <summary>The words an empty block carries.</summary>
        public static string NothingHeldBack()
        {
            return "nothing measured this group reaches no output. The report carries "
                + "everything the run read";
        }

        private static void Add(List<ReportGap> gaps, string name, int carried, int outOf, string belongs)
        {
            // No item read, nothing measured. A property read on items and empty on every one
            // is said with its count, Q76, where it was left out and read as carried.
            if (outOf < 1)
            {
                return;
            }

            string value = carried < 1 ? ReportGap.ReadAndEmpty(outOf) : ReportGap.Counted(carried, outOf);
            ReportGap gap = new ReportGap(name, value, belongs);
            gap.Carried = carried;
            gap.OutOf = outOf;
            gaps.Add(gap);
        }
    }
}
