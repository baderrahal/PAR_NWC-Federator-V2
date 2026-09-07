using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Exchange;

namespace Federator.Core.Report
{
    /// <summary>
    /// The report goes out in METERS, always, whatever the open document measures in.
    /// F26, bug B14, Q23.
    ///
    /// WHY THIS EXISTS. The tool sets every model to the wanted units through
    /// DocumentModels.SetModelUnitsAndTransform, which is the only public managed member
    /// that sets units at all. On the run of 2026-09-07 that reported 11 of 14 groups
    /// where every model was set and Document.Units still read Feet, so the report was
    /// written in feet with the tolerance at 0.246ft. Bader's rule is that the report is
    /// always metric, so the numbers are CONVERTED here rather than waited for.
    ///
    /// WHAT THIS IS NOT. It is not a second unit system. Every number still comes out of
    /// the document in the document's units, is converted once, here, in one pass over the
    /// finished report, and the unit label is written from the same pass. So the numbers
    /// and the label cannot disagree, which is the thing the old rule was guarding against.
    ///
    /// The factors are ExchangeUnits, which is the one conversion table in this repo. A
    /// unit that table has not been taught is REFUSED: the report is not written and the
    /// group fails, because a report in the wrong unit reads as real and is not.
    /// </summary>
    public static class ReportUnits
    {
        /// <summary>The short name every report carries, which is what the cells are written with.</summary>
        public const string Short = "m";

        /// <summary>The long name the log and the window use.</summary>
        public const string Name = "Meters (m)";

        public static bool IsKnown(string units)
        {
            return ExchangeUnits.IsKnown(units);
        }

        /// <summary>
        /// One number from the document's units into meters. Throws on a unit the table
        /// has not been taught, rather than guessing at a factor.
        /// </summary>
        public static double ToMeters(double value, string fromUnits)
        {
            return ExchangeUnits.Convert(value, fromUnits, Short);
        }

        /// <summary>
        /// Converts every measured number in one finished report into meters, in one pass,
        /// and sets the unit labels to match. The tolerance of every test, and the distance
        /// and the clash point of every row.
        ///
        /// Nothing else is touched. A grid location and a level are text, a status is a
        /// status, and a raw clash count is a count.
        ///
        /// Called after the clash step and BEFORE anything is written, so the workbook, the
        /// XML and the page all read the same converted report.
        /// </summary>
        public static ReportUnitsOutcome ToMeters(ClashReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            string from = report.DocumentUnits;

            if (string.IsNullOrEmpty(from))
            {
                return ReportUnitsOutcome.Refuse(
                    from,
                    "the document did not say what units it measures in, so no number could be converted");
            }

            if (!IsKnown(from))
            {
                return ReportUnitsOutcome.Refuse(
                    from,
                    "\"" + from + "\" is not a unit this tool has been taught, so no number could be converted. "
                        + "Known units are " + string.Join(", ", new List<string>(ExchangeUnits.KnownUnits()).ToArray()));
            }

            if (string.Equals(from, Short, StringComparison.OrdinalIgnoreCase))
            {
                // Already metric. The labels are still written, because a report whose
                // label was never set reads as unitless.
                Label(report);
                return ReportUnitsOutcome.AlreadyMeters(from, report);
            }

            double factor = ExchangeUnits.Convert(1.0, from, Short);
            int tests = 0;
            int rows = 0;

            foreach (TestReport test in report.Tests)
            {
                test.Tolerance = test.Tolerance * factor;
                tests++;

                foreach (ClashRow row in test.Rows)
                {
                    row.Distance = row.Distance * factor;
                    row.X = row.X * factor;
                    row.Y = row.Y * factor;
                    row.Z = row.Z * factor;
                    rows++;
                }
            }

            Label(report);
            return ReportUnitsOutcome.Converted(from, factor, tests, rows);
        }

        /// <summary>
        /// The unit label on the report and on every test. Written from the same pass that
        /// converts the numbers, so a number and its label can never disagree.
        /// </summary>
        private static void Label(ClashReport report)
        {
            report.DocumentUnits = Short;

            foreach (TestReport test in report.Tests)
            {
                test.ToleranceUnits = Short;
            }
        }
    }

    /// <summary>What the conversion did, for the log and for the group's judgement.</summary>
    public sealed class ReportUnitsOutcome
    {
        private ReportUnitsOutcome()
        {
            From = string.Empty;
            Problem = null;
        }

        /// <summary>The units the document was measuring in.</summary>
        public string From { get; private set; }

        /// <summary>True when every number was multiplied. False when the document was already metric.</summary>
        public bool Changed { get; private set; }

        public double Factor { get; private set; }

        public int Tests { get; private set; }

        public int Rows { get; private set; }

        /// <summary>Why nothing could be converted, or null when the report is in meters.</summary>
        public string Problem { get; private set; }

        /// <summary>True when the report must not be written, because its numbers are not in meters.</summary>
        public bool Refused
        {
            get { return Problem != null; }
        }

        internal static ReportUnitsOutcome Converted(string from, double factor, int tests, int rows)
        {
            ReportUnitsOutcome outcome = new ReportUnitsOutcome();
            outcome.From = from;
            outcome.Changed = true;
            outcome.Factor = factor;
            outcome.Tests = tests;
            outcome.Rows = rows;
            return outcome;
        }

        internal static ReportUnitsOutcome AlreadyMeters(string from, ClashReport report)
        {
            ReportUnitsOutcome outcome = new ReportUnitsOutcome();
            outcome.From = from;
            outcome.Changed = false;
            outcome.Factor = 1.0;
            outcome.Tests = report.Tests.Count;
            return outcome;
        }

        internal static ReportUnitsOutcome Refuse(string from, string problem)
        {
            ReportUnitsOutcome outcome = new ReportUnitsOutcome();
            outcome.From = from ?? string.Empty;
            outcome.Problem = problem;
            return outcome;
        }

        /// <summary>The one line the log carries.</summary>
        public string Line()
        {
            if (Refused)
            {
                return "UNITS    the report was NOT written: " + Problem;
            }

            if (!Changed)
            {
                return "UNITS    the document measures in " + ReportUnits.Short
                    + " already, so the report is in " + ReportUnits.Name + " with nothing converted";
            }

            return "UNITS    every number converted from " + From + " to " + ReportUnits.Short
                + ", one " + From + " is " + Factor.ToString("0.##########", CultureInfo.InvariantCulture)
                + " " + ReportUnits.Short + ", " + Tests + (Tests == 1 ? " test and " : " tests and ")
                + Rows + (Rows == 1 ? " row" : " rows")
                + ". The report is written in " + ReportUnits.Name;
        }

        public override string ToString()
        {
            return Line();
        }
    }
}
