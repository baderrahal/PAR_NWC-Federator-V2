using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Report;

namespace Federator.Core.Coverage
{
    /// <summary>
    /// The COVERAGE block of one group, F127, Bader's request 2 under Q112, section 1.7 of the F127 design,
    /// which the loop keeps outside the repo as turn5\f127-design.md: the tests of the picked file, or of the
    /// tests saved in the document, counted by presence and by reason, a few named for each reason that has no
    /// results, the count check against the clash workbook and every COVERAGE FAILED line. Read off the one
    /// shape the Coverage sheet is written from, CoverageSheetData, so the block and the sheet cannot say two
    /// things about one test. Every test is on the sheet and in the .tsv, RowText, so the block names a few.
    ///
    /// THE GROUP KEEPS ITS OWN RESULT, the last line says so, because a report check never fails a group.
    /// </summary>
    public static class CoverageBlock
    {
        /// <summary>The block's title, followed by the group.</summary>
        public const string Title = "COVERAGE";

        /// <summary>The last line of every block.</summary>
        public const string KeepsItsResult = "the group keeps its own result, a coverage count never fails a group";

        /// <summary>The block of a group whose tests were read and judged.</summary>
        public static IList<string> Lines(string group, CoverageSheetData data, CoverageSettings settings)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            List<string> lines = new List<string>();

            lines.Add("the tests        : " + (data.FromAPickedFile
                ? data.Tests.Count + " in the picked file " + data.Source + ", sha256 " + data.Sha256 + ", " + data.Corrections
                : data.Tests.Count + " " + data.Source));
            lines.Add("counted          : " + CoverageSheet.Counts(data));
            lines.Add("one reason each  :");

            Dictionary<CoverageReason, List<TestCoverage>> byReason = new Dictionary<CoverageReason, List<TestCoverage>>();

            foreach (TestCoverage test in data.Tests)
            {
                List<TestCoverage> tests;

                if (!byReason.TryGetValue(test.Reason, out tests))
                {
                    tests = new List<TestCoverage>();
                    byReason.Add(test.Reason, tests);
                }

                tests.Add(test);
            }

            foreach (CoverageReason reason in Enum.GetValues(typeof(CoverageReason)))
            {
                List<TestCoverage> tests;

                if (byReason.TryGetValue(reason, out tests))
                {
                    lines.Add("    " + tests.Count.ToString(CultureInfo.InvariantCulture).PadLeft(6) + "  " + CoverageWords.For(reason));
                }
            }

            foreach (CoverageReason reason in Enum.GetValues(typeof(CoverageReason)))
            {
                List<TestCoverage> tests;

                if (reason == CoverageReason.HasClashes || reason == CoverageReason.RanAndFoundNone
                    || !byReason.TryGetValue(reason, out tests))
                {
                    continue;
                }

                int shown = Math.Min(tests.Count, settings.ExamplesPerReason);

                for (int i = 0; i < shown; i++)
                {
                    lines.Add("    " + CoverageWords.For(reason) + "  " + tests[i].Name
                        + (tests[i].Detail.Length == 0 ? string.Empty : "  " + OneLine(tests[i].Detail)));
                }

                if (tests.Count > shown)
                {
                    lines.Add("    and " + (tests.Count - shown) + " more of this reason, every one on the Coverage sheet"
                        + " of the internal workbook and in the .tsv");
                }
            }

            lines.AddRange(CheckLines(group, data.Check, settings.ExamplesPerReason));
            lines.Add("items no set catches : UNKNOWN, not counted in this build");
            lines.Add(KeepsItsResult);
            return lines;
        }

        /// <summary>
        /// The block of a group whose coverage was not taken, saying why on one line whatever an exception
        /// message held, so a missing block is never read as a check that ran and found nothing.
        /// </summary>
        public static IList<string> NotTakenLines(string why)
        {
            return new List<string>
            {
                "the tests        : UNKNOWN, no coverage was taken for this group, " + (string.IsNullOrEmpty(why) ? "and why is UNKNOWN" : OneLine(why)),
                KeepsItsResult
            };
        }

        /// <summary>
        /// The text of the one .tsv row each test gets, so the row file carries what the sheet carries and
        /// the block names only a few: its position, its presence, whether it ran, its reason and detail.
        /// </summary>
        public static string RowText(TestCoverage test)
        {
            if (test == null)
            {
                throw new ArgumentNullException("test");
            }

            return "position " + test.Position.ToString(CultureInfo.InvariantCulture) + ", " + CoverageWords.For(test.Presence)
                + ", " + (test.Ran ? "ran" : "not run") + ", " + CoverageWords.For(test.Reason)
                + (test.Detail.Length == 0 ? string.Empty : ", " + OneLine(test.Detail));
        }

        private static IList<string> CheckLines(string group, CountCheck check, int examples)
        {
            List<string> lines = new List<string>();

            if (check == null)
            {
                lines.Add("workbook against Clash Detective : UNKNOWN, the count check was not taken");
                return lines;
            }

            int agree = check.CountOf(CountVerdict.Agree);
            int failed = check.CountOf(CountVerdict.Failed);
            int notCompared = check.CountOf(CountVerdict.NotCompared);
            int neither = check.CountOf(CountVerdict.HeldByNeither);

            lines.Add("workbook against Clash Detective : " + (agree + failed == 0 ? "UNKNOWN, 0 compared" : (agree + failed) + " compared")
                + ", " + agree + " agree, " + failed + " FAILED, " + notCompared + " not compared, "
                + neither + " held by neither side");

            int shown = 0;

            foreach (CountedTest test in check.Tests)
            {
                if (test.Verdict != CountVerdict.NotCompared)
                {
                    continue;
                }

                if (shown == examples)
                {
                    lines.Add("    and " + (notCompared - shown) + " more not compared, every one on the Coverage sheet"
                        + " of the internal workbook");
                    break;
                }

                lines.Add("    not compared  " + test.Name + "  " + OneLine(test.Why));
                shown++;
            }

            lines.AddRange(check.FailedLines(group));

            if (check.NotInTheXml.Count > 0)
            {
                List<string> named = new List<string>();

                for (int i = 0; i < check.NotInTheXml.Count && i < examples; i++)
                {
                    named.Add(check.NotInTheXml[i]);
                }

                lines.Add("not in the file  : " + check.NotInTheXml.Count + (check.NotInTheXml.Count == 1 ? " test" : " tests")
                    + " in Clash Detective that the picked file does not name, not compared: " + string.Join(", ", named.ToArray())
                    + (check.NotInTheXml.Count > named.Count ? " and " + (check.NotInTheXml.Count - named.Count) + " more" : string.Empty));
            }

            return lines;
        }

        private static string OneLine(string text)
        {
            return (text ?? string.Empty).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
        }
    }
}
