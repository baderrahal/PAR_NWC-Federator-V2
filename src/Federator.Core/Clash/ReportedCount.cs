using System;

namespace Federator.Core.Clash
{
    /// <summary>
    /// What reached the workbook for one test, against what the document holds. F21.
    ///
    /// CRITERION 3 IS THAT THE CLASH COUNTS IN THE EXCEL MATCH THE CLASH DETECTIVE PANEL
    /// EXACTLY, and nothing in the log answered it. The tally line says what the document
    /// holds and the workbook says what was written, and the two were never put beside
    /// each other, so checking them meant opening Excel and Navisworks side by side for
    /// every test.
    ///
    /// THE TWO NUMBERS ARE NOT THE SAME THING AND THAT IS NOT A FAULT. Measured off the
    /// code on 2026-09-19. The tally counts LEAVES, descending into every result group,
    /// because a group silently counting as one would understate what a test found. The
    /// harvest writes ONE row per result group and does not descend into it, which is
    /// also what the Clash Detective panel shows. So the rows are fewer than the clashes
    /// exactly as often as a test holds result groups, and that difference is the
    /// grouping and not a loss.
    ///
    /// What IS a finding is more rows than clashes. Nothing in this tool produces that,
    /// so the line says so in capitals and leaves it to Bader, which is the rule: the
    /// tool reports what it noticed and never acts on it. The one exception is a test kept
    /// of a mirrored pair, F132, whose rows carry the clashes only its mirror found, and
    /// the line for it names them and counts only the rows past them.
    /// </summary>
    public static class ReportedCount
    {
        /// <summary>The prefix, padded like every other kind in the log.</summary>
        public const string Prefix = "ROWS     ";

        /// <summary>
        /// The one line per test. Every number measured, nothing worked out from the
        /// other.
        /// </summary>
        public static string Line(string testName, int rowsForTheWorkbook, int clashesInTheDocument)
        {
            return Line(testName, rowsForTheWorkbook, clashesInTheDocument, 0);
        }

        /// <summary>
        /// The line for a test kept of a mirrored pair, F132, Bader's answer D to Q133. The
        /// clashes only its mirrors found are added to it, one row each, and the panel shows
        /// them under the mirror and not under this test. So the rows are the panel's own
        /// count plus that many, which this tool explains, and only rows past them are the
        /// finding nothing explains. The number is MirrorMerge.AddedToTheKeptTest. With
        /// none it is the line above, word for word.
        /// </summary>
        public static string Line(
            string testName, int rowsForTheWorkbook, int clashesInTheDocument, int foundOnlyByAMirror)
        {
            int own = rowsForTheWorkbook - foundOnlyByAMirror;
            string head = Prefix + Words(testName) + "  "
                + rowsForTheWorkbook + Row(rowsForTheWorkbook) + " for the workbook, "
                + clashesInTheDocument + Clash(clashesInTheDocument) + " in the document"
                + (foundOnlyByAMirror == 0
                    ? string.Empty
                    : ", " + foundOnlyByAMirror + (foundOnlyByAMirror == 1
                        ? " of the rows is a clash only its mirror found"
                        : " of the rows are clashes only its mirror found")
                        + ", which the panel shows under the mirror");

            if (own == clashesInTheDocument)
            {
                return head + ", they agree";
            }

            if (own < clashesInTheDocument)
            {
                return head + ". The difference is the result groups, which are one row each "
                    + "in the workbook and one row each in the panel, carrying the clashes inside them";
            }

            return head + ". THERE ARE MORE ROWS THAN CLASHES, by "
                + (own - clashesInTheDocument)
                + ", and nothing in this tool explains that. The workbook will not match the panel";
        }

        /// <summary>
        /// Whether the two numbers are one this tool can account for. More rows than
        /// clashes is the only shape it cannot, and it is a finding rather than a failure.
        /// </summary>
        public static bool NothingExplainsIt(int rowsForTheWorkbook, int clashesInTheDocument)
        {
            return rowsForTheWorkbook > clashesInTheDocument;
        }

        private static string Words(string testName)
        {
            return string.IsNullOrEmpty(testName) ? "UNKNOWN test" : testName;
        }

        private static string Row(int count)
        {
            return count == 1 ? " row" : " rows";
        }

        private static string Clash(int count)
        {
            return count == 1 ? " clash" : " clashes";
        }
    }
}
