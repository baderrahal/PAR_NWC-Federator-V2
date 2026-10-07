using System;
using System.Collections.Generic;

namespace Federator.Core.Coverage
{
    /// <summary>
    /// The count check across the run, F127, the part of the design's section 1.7 that
    /// RESULT reads. Bader's answer to the lead's notes under Q112: a count in Clash Detective
    /// that differs from the workbook rows is a FAILED line in COVERAGE and RESULT, and the
    /// group keeps its own result. So every group's FAILED lines are kept whole and RESULT
    /// writes them all by default, CoverageSettings.FailedLinesInResult.
    ///
    /// THE TOTALS ARE KEPT BESIDE THE FAILED COUNT, so nought FAILED is never read without
    /// how many were compared. Both wave buildings skip their clash under F112's rule, so
    /// every test there is NOT COMPARED, and a run that compared nothing says UNKNOWN.
    ///
    /// It is handed to RunLog.WriteResultBlock by the run that made it and never kept on
    /// the log, the way OffCoordinatesAcrossTheRun is, so a second run of a window never
    /// carries the first run's groups. Nothing here touches a group's facts.
    /// </summary>
    public sealed class CoverageAcrossTheRun
    {
        /// <summary>
        /// The label of the totals line. Not the FAILED prefix, so a reader looking for FAILED
        /// lines never takes the count for one.
        /// </summary>
        private const string CheckedLabel = "COVERAGE checked : ";

        private readonly List<string> failedLines = new List<string>();
        private readonly int failedLinesShown;

        public CoverageAcrossTheRun(CoverageSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            failedLinesShown = settings.FailedLinesInResult;
        }

        /// <summary>How many groups were added, a group whose check could not be made among them.</summary>
        public int Groups { get; private set; }

        /// <summary>The groups added with no check, because their coverage could not be taken.</summary>
        public int GroupsNotChecked { get; private set; }

        /// <summary>The groups with at least one FAILED line.</summary>
        public int GroupsWithAFailedLine { get; private set; }

        public int Agree { get; private set; }

        public int Failed { get; private set; }

        public int NotCompared { get; private set; }

        /// <summary>
        /// One group's check, rolled in. A null check is a group whose coverage could not be
        /// taken, counted as not checked and never as a group that agreed.
        /// </summary>
        public void Add(string group, CountCheck check)
        {
            Groups++;

            if (check == null)
            {
                GroupsNotChecked++;
                return;
            }

            Agree += check.CountOf(CountVerdict.Agree);
            NotCompared += check.CountOf(CountVerdict.NotCompared);

            IList<string> lines = check.FailedLines(group);

            if (lines.Count > 0)
            {
                Failed += lines.Count;
                GroupsWithAFailedLine++;
                failedLines.AddRange(lines);
            }
        }

        /// <summary>
        /// The lines RESULT writes: the totals, then every FAILED line in the order the groups
        /// were added, or as many as the setting lists and a line counting the rest.
        /// </summary>
        public IList<string> ResultLines()
        {
            List<string> lines = new List<string>();
            int compared = Agree + Failed;
            string notChecked = GroupsNotChecked > 0
                ? ", " + Count(GroupsNotChecked, "group", "groups") + " not checked"
                : string.Empty;

            if (Groups == 0)
            {
                lines.Add(CheckedLabel + "UNKNOWN, no group's counts were handed in");
                return lines;
            }

            if (compared == 0)
            {
                lines.Add(CheckedLabel + "UNKNOWN, 0 compared in " + Count(Groups, "group", "groups") + ", "
                    + NotCompared + " not compared" + notChecked + ", so no count was checked");
                return lines;
            }

            lines.Add(CheckedLabel + compared + " compared, " + Agree + " agree, " + Failed + " FAILED in "
                + GroupsWithAFailedLine + " of " + Count(Groups, "group", "groups") + ", " + NotCompared
                + " not compared" + notChecked + ", every group keeps its own result");

            int shown = 0;

            foreach (string line in failedLines)
            {
                if (failedLinesShown > 0 && shown == failedLinesShown)
                {
                    break;
                }

                lines.Add(line);
                shown++;
            }

            if (failedLines.Count > shown)
            {
                int rest = failedLines.Count - shown;
                lines.Add("and " + rest + " more " + (rest == 1 ? "COVERAGE FAILED line" : "COVERAGE FAILED lines")
                    + ", counted and not listed here, because RESULT is set to list " + failedLinesShown);
            }

            return lines;
        }

        /// <summary>
        /// The words RESULT closes on where no group failed and no error was logged but a
        /// count differs, so Nothing failed is never written beside a FAILED line. Null where
        /// no count differs.
        /// </summary>
        public string InsteadOfNothingFailed()
        {
            if (Failed == 0)
            {
                return null;
            }

            return "No group failed and no error was logged, and "
                + (Failed == 1
                    ? "1 coverage count differs, on its COVERAGE FAILED line above"
                    : Failed + " coverage counts differ, each on its COVERAGE FAILED line above")
                + ", every group keeping its own result";
        }

        /// <summary>The line RESULT writes where the run handed it no coverage, because a missing line reads as a check that did not run.</summary>
        public static string NoneTaken()
        {
            return CheckedLabel + "UNKNOWN, no coverage was taken in this run";
        }

        private static string Count(int count, string one, string many)
        {
            return count + " " + (count == 1 ? one : many);
        }
    }
}
