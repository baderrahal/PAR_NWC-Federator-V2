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
    /// THE SIX, AND WHY THEY ARE THE SIX. Measured on 2026-09-19 by reading all three
    /// writers. WorkbookWriter, HtmlTabularWriter and ClientReportColumns name none of
    /// Family, Type, Material, SourceFile, Discipline or IdFrom anywhere, and
    /// ClashReportXml writes two quick properties and says in its own comment that the
    /// five used to be written and are not. ClashHarvest reads every one of them off
    /// every item of every clash, which is a property lookup per item per run.
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

        /// <summary>
        /// Where the five per item properties would go if anyone wanted them. The
        /// workbook is ours and the page is the client's, so that is the answer for all
        /// of them, and Q25 is where the decision sits.
        /// </summary>
        public const string InTheWorkbook =
            "it would belong in a workbook column of ours, which is Q25";

        /// <summary>Every gap in one group's report, in the order they are always listed.</summary>
        public static IList<ReportGap> For(ClashReport report)
        {
            List<ReportGap> gaps = new List<ReportGap>();

            if (report == null)
            {
                return gaps;
            }

            int items = 0;
            int family = 0;
            int type = 0;
            int material = 0;
            int sourceFile = 0;
            int discipline = 0;
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

                        if (!string.IsNullOrEmpty(item.Family)) { family++; }
                        if (!string.IsNullOrEmpty(item.Type)) { type++; }
                        if (!string.IsNullOrEmpty(item.Material)) { material++; }
                        if (!string.IsNullOrEmpty(item.SourceFile)) { sourceFile++; }
                        if (!string.IsNullOrEmpty(item.Discipline)) { discipline++; }
                        if (!string.IsNullOrEmpty(item.IdFrom)) { idFrom++; }
                    }
                }
            }

            Add(gaps, "Family", family, items, InTheWorkbook);
            Add(gaps, "Type Name", type, items, InTheWorkbook);
            Add(gaps, "Material", material, items, InTheWorkbook);
            Add(gaps, "Source File", sourceFile, items, InTheWorkbook);
            Add(gaps, "Discipline", discipline, items, InTheWorkbook);
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
        /// block reads as a check that did not run.
        /// </summary>
        public static IList<string> Lines(ClashReport report)
        {
            IList<ReportGap> gaps = For(report);
            List<string> lines = new List<string>();

            if (gaps.Count == 0)
            {
                lines.Add(NothingHeldBack());
                return lines;
            }

            lines.Add(gaps.Count + (gaps.Count == 1 ? " number is" : " numbers are")
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
            // A property nothing carried is not a gap. Reporting it would say the run is
            // holding back something it never read, which is the opposite of true.
            if (carried < 1)
            {
                return;
            }

            ReportGap gap = new ReportGap(name, ReportGap.Counted(carried, outOf), belongs);
            gap.Carried = carried;
            gap.OutOf = outOf;
            gaps.Add(gap);
        }
    }
}
