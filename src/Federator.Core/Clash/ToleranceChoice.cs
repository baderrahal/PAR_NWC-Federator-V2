using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Exchange;
using Federator.Core.Report;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Where the tolerance on a clash test came from, F76. Kept on the report row for the
    /// same reason the Item ID keeps which property supplied it: a number is only honest
    /// while what produced it is still visible.
    /// </summary>
    public enum ToleranceOrigin
    {
        /// <summary>Nothing said. Never written to a report row on purpose.</summary>
        Unknown = 0,

        /// <summary>Read off the clash test in the open document, which is what produced the row.</summary>
        Document = 1,

        /// <summary>Chosen on the Clash step, which beats both of the other two.</summary>
        Tool = 3
    }

    /// <summary>
    /// The tolerance chosen on the Clash step, F76.
    ///
    /// WHAT THE RUN SHOWED. The client's matrix is written at 25 mm and the NWFs on disk
    /// held tests at 75 mm, and a test already in the document is left exactly as it is,
    /// so the run clashed at 75 mm while everybody believed it was clashing at 25. There
    /// was no way to say which one was wanted, because there has never been a tolerance
    /// setting in this tool: every test carried its own and whichever copy was found first
    /// won.
    ///
    /// SO THIS IS THE ONE PLACE A TOLERANCE CAN BE STATED AND IT BEATS EVERYTHING. When a
    /// value is chosen it is set on EVERY test in the run, the ones created fresh from the
    /// XML and the ones already saved in the NWF alike. It beats the XML and it beats the
    /// document. Apply file settings keeps exactly the meaning it had, which is whether
    /// the FILE'S settings reach a saved test, and this wins over it, because a value a
    /// person typed beats a value read out of a file.
    ///
    /// THE DROP DOWN IS THE ONLY SOURCE SINCE F155, Bader's decision of 2026-10-13. Use the value in the
    /// XML and Other are gone, 25 mm is selected when the window opens, and the value is set on every
    /// test of every run, so the XML's tolerance is never used again.
    ///
    /// THE COST IS SAID BEFORE THE RUN, NOT AFTER, and what the cost IS was measured on
    /// 2026-09-20, 5y. Changing the tolerance on a test already in the document RESETS
    /// NOTHING: its recorded results and every status a person set on them are kept,
    /// through a save and a reopen. What it changes is WHICH CLASHES that test finds the
    /// next time it runs, so a clash somebody marked may not come back. This comment,
    /// and four others, said it reset the results, and 175,434 saved tests across his
    /// runs had a chosen tolerance set on them while the confirm screen told him in
    /// capitals that it had reset all of them.
    ///
    /// MILLIMETRES IN, DOCUMENT UNITS OUT. The choice is in millimetres because that is
    /// what a person says, and it is converted through UnitTable, the one unit table in
    /// this repo, exactly as a file tolerance is. A unit the table has not been taught
    /// throws rather than falling back, which is F33's rule.
    /// </summary>
    public sealed class ToleranceChoice
    {
        /// <summary>
        /// The values the drop down offers, in millimetres, in its order, F155, Bader's decision of 2026-10-13: the
        /// drop down is the only source of a run's tolerance and its only control. The entry Use the value in the
        /// XML, the entry Other and its number box went with it, and the XML's tolerance is never used again.
        /// </summary>
        public static readonly double[] OfferedMillimetres = { 25.0, 50.0, 75.0 };

        /// <summary>The value selected when the window opens, and the one every button that runs no test hands on.</summary>
        public const double DefaultMillimetres = 25.0;

        /// <summary>The words beside the drop down.</summary>
        public const string PickerLabel = "Clash tolerance";

        /// <summary>The grey line under it. Twelve words, the limit, with no word about the XML.</summary>
        public const string HelpLine =
            "Set on every test of the run. Results and statuses are kept";

        /// <summary>The words that begin every line this rule writes.</summary>
        public const string Prefix = "TOLERANCE";

        /// <summary>What the drop down hands back where nothing is selected, which the window never leaves it at.</summary>
        public const string NothingSelected = "No clash tolerance is selected in the drop down.";

        private readonly double millimetres;

        private ToleranceChoice(double millimetres)
        {
            this.millimetres = millimetres;
        }

        /// <summary>The value the window opens at, DefaultMillimetres.</summary>
        public static ToleranceChoice Default()
        {
            return Of(DefaultMillimetres);
        }

        /// <summary>
        /// A tolerance in millimetres. Zero is a real tolerance and is allowed. A negative one is not a tolerance at
        /// all and is refused where it is set, rather than reaching 1830 tests and being discovered in a report.
        /// </summary>
        public static ToleranceChoice Of(double millimetres)
        {
            if (double.IsNaN(millimetres) || double.IsInfinity(millimetres))
            {
                throw new ArgumentOutOfRangeException("millimetres", "A tolerance that is not a number is not a tolerance.");
            }

            if (millimetres < 0)
            {
                throw new ArgumentOutOfRangeException("millimetres", "A tolerance below zero is not a tolerance. Zero is a real one and is allowed.");
            }

            return new ToleranceChoice(millimetres);
        }

        /// <summary>
        /// What the drop down says, read without a throw, FR-166: the entry at its place in Choices(). Null where
        /// nothing is selected, and then refused holds why, so each caller decides whether the tolerance matters to
        /// it. The window selects 25 mm when it opens and offers nothing else to type, so only a selection taken away
        /// reaches it.
        /// </summary>
        public static ToleranceChoice FromThePicker(int at, out string refused)
        {
            if (at < 0 || at >= OfferedMillimetres.Length)
            {
                refused = NothingSelected;
                return null;
            }

            refused = null;
            return Of(OfferedMillimetres[at]);
        }

        /// <summary>The value in millimetres.</summary>
        public double Millimetres
        {
            get { return millimetres; }
        }

        /// <summary>The drop down entries, in order, the default first.</summary>
        public static IList<string> Choices()
        {
            List<string> choices = new List<string>();

            foreach (double value in OfferedMillimetres)
            {
                choices.Add(Words(value));
            }

            return choices;
        }

        /// <summary>What the drop down reads once this choice is made.</summary>
        public string Label()
        {
            return Words(millimetres);
        }

        /// <summary>
        /// The tolerance in the units the open document measures in. Throws on a unit the table has not been taught,
        /// the same way a file tolerance does, because a report in the wrong unit reads as real and is not.
        /// </summary>
        public double InDocumentUnits(string documentUnits)
        {
            return ExchangeUnits.Convert(millimetres, "mm", documentUnits);
        }

        /// <summary>
        /// The report row of a test that carries the chosen value, created at it or set to it on a saved test, says
        /// that value in the document's units and that it was chosen in the tool, FR-036. A row of null is no row.
        /// </summary>
        public void PutOn(TestReport row, string documentUnits)
        {
            if (row == null)
            {
                return;
            }

            row.Tolerance = InDocumentUnits(documentUnits);
            row.ToleranceUnits = documentUnits ?? string.Empty;
            row.ToleranceFrom = ToleranceOrigin.Tool;
        }

        /// <summary>
        /// The line one group writes. Both counts are read, neither is worked out from the
        /// other, and the converted value is on it so nobody has to do the arithmetic to
        /// find out what 25 mm was in a document measuring feet. Under Set on a saved test
        /// counts only where its tolerance was changed, FR-053, and every other saved test is
        /// named by what happened to it.
        ///
        /// FR-063. The source is the plan's: with no clash matrix read the saved tests are the plan,
        /// so no tolerance was read out of an XML and none was beaten, and the line says the
        /// tolerance is the one each saved test holds. It said read per test out of the XML on
        /// every group of such a run.
        /// </summary>
        public string LogLine(ClashPlanSource source, int created, SavedTestTolerances saved, string documentUnits)
        {
            if (saved == null)
            {
                saved = new SavedTestTolerances();
            }

            bool noXml = source == ClashPlanSource.Document;

            int set = saved.Of(SavedTestTolerance.Set);
            int setOn = created + set;

            return Prefix + " " + Words(millimetres) + " chosen in the tool, so it beats "
                + (noXml ? "the document" : "both the built in matrix and the document")
                + ". Set on " + setOn + Word(setOn, " test", " tests")
                + ", " + created + " created fresh and " + set
                + " already in the document. " + NotSet(saved)
                + Words(millimetres) + " is " + Converted(documentUnits) + " in this document";
        }

        /// <summary>
        /// The saved tests whose tolerance this run did not change, each kind on its own, or
        /// nothing where there are none.
        /// </summary>
        private static string NotSet(SavedTestTolerances saved)
        {
            List<string> parts = new List<string>();
            int notSet = 0;

            foreach (KeyValuePair<SavedTestTolerance, string> kind in new[]
            {
                new KeyValuePair<SavedTestTolerance, string>(SavedTestTolerance.AlreadyAtTheValue, "already at the value"),
                new KeyValuePair<SavedTestTolerance, string>(SavedTestTolerance.NotFound, "not found"),
                new KeyValuePair<SavedTestTolerance, string>(SavedTestTolerance.WouldNotCopy, "would not copy as a clash test"),
                new KeyValuePair<SavedTestTolerance, string>(SavedTestTolerance.Threw, "threw"),
                new KeyValuePair<SavedTestTolerance, string>(SavedTestTolerance.LeftAsTheDocumentHasIt, "left as the document has them")
            })
            {
                int count = saved.Of(kind.Key);

                if (count > 0)
                {
                    parts.Add(count + " " + kind.Value);
                    notSet += count;
                }
            }

            return notSet == 0
                ? string.Empty
                : "Not set on " + notSet + Word(notSet, " saved test", " saved tests") + ": "
                    + string.Join(", ", parts.ToArray()) + ". ";
        }

        /// <summary>
        /// What the confirm screen says before the run starts. Since F155 a tolerance is chosen on every run,
        /// the drop down being its only source, so the screen says it on every run.
        /// </summary>
        public IList<string> WarningLines(int groups, int testsInTheFile)
        {
            List<string> lines = new List<string>();

            lines.Add(PickerLabel + ": " + Words(millimetres) + ", chosen in the tool.");
            lines.Add("It is set on every clash test in this run, which is " + groups
                + Word(groups, " group", " groups") + " of " + testsInTheFile
                + Word(testsInTheFile, " test", " tests") + " from the built in matrix, plus every test "
                + "already saved in each NWF.");
            lines.Add("It beats the tolerance in the built in matrix and the tolerance in the document.");
            // WHAT IT ACTUALLY COSTS, measured 5y on 2026-09-20. This line said it reset
            // the results and it does not: every recorded result and every status a
            // person set is kept, through a save and a reopen. The real cost is that the
            // test finds DIFFERENT clashes next time, so a clash somebody marked may not
            // come back, and saying the wrong one made a person hesitate over an action
            // that is free.
            lines.Add("Every recorded result and every status a person set is KEPT. What changes "
                + "is which clashes each test finds when it runs, so a clash somebody marked may "
                + "not come back at the new tolerance.");

            return lines;
        }

        /// <summary>
        /// The line beside ReadFromLine that says what its numbers ARE, Q54. On the wiring
        /// round's run the line above read 17,259 rows off the clash XML and it was right:
        /// those are the test blocks of tests that never ran, not created because a side
        /// finds nothing or already in the document with an empty side, and a row the
        /// document never produced keeps what the plan gave it. Bader wants to decide off
        /// a run rather than in the abstract, so the workbook is unchanged and this line
        /// makes the split visible. It says RAN and not carry a clash, because a test that
        /// ran and found nothing is among the ran. Every number is read off the report
        /// rows and the third is the first two subtracted, said so a reader can check it.
        /// It said their tolerance reads off the clash matrix, which a saved test read out of
        /// the document and, since FR-036, a test carrying a value chosen in the tool do not,
        /// so it points at ReadFromLine, which counts where every row's value was read.
        /// </summary>
        public static string BlocksLine(int blocks, int ran)
        {
            int neverRan = blocks - ran;
            System.Globalization.CultureInfo invariant = System.Globalization.CultureInfo.InvariantCulture;

            return "WORKBOOK " + blocks.ToString("N0", invariant) + " test blocks, "
                + ran.ToString("N0", invariant) + " ran and " + neverRan.ToString("N0", invariant)
                + " are tests that never ran. Where each block's tolerance was read is on the line below";
        }

        /// <summary>
        /// The line that says where the tolerance on every report row was READ, F76. The
        /// report is produced by the clash tests in the open document, so every one of
        /// them should read off the document. A count under either of the other two is the
        /// report saying a number the run did not clash at, which is what happened: the
        /// matrix is written at 25 mm, the NWFs on disk held tests at 75 mm, and the
        /// report said 25 while the run clashed at 75.
        ///
        /// Every count is READ off the rows. None is worked out from another, and the
        /// total is on the end so a row the tally missed shows as a difference.
        /// </summary>
        public static string ReadFromLine(int fromDocument, int fromTool, int unknown)
        {
            int total = fromDocument + fromTool + unknown;

            return Prefix + " on the report, read off " + Words(ToleranceOrigin.Document)
                + " for " + fromDocument + ", " + Words(ToleranceOrigin.Tool)
                + " for " + fromTool + ", " + Words(ToleranceOrigin.Unknown)
                + " for " + unknown + ", " + total + Word(total, " test", " tests") + " in all";
        }

        /// <summary>
        /// A tolerance in the words a person says, so 25 mm reads as 25 mm and 37.5 mm
        /// reads as 37.5 mm rather than as 37.500 mm.
        /// </summary>
        public static string Words(double millimetres)
        {
            return millimetres.ToString("0.###", CultureInfo.InvariantCulture) + " mm";
        }

        /// <summary>The words for where a tolerance came from, for the log and the check.</summary>
        public static string Words(ToleranceOrigin origin)
        {
            switch (origin)
            {
                case ToleranceOrigin.Document: return "the clash test in the document";
                case ToleranceOrigin.Tool: return "chosen in the tool";
                default: return "UNKNOWN";
            }
        }

        private string Converted(string documentUnits)
        {
            try
            {
                return InDocumentUnits(documentUnits).ToString("0.######", CultureInfo.InvariantCulture)
                    + " " + documentUnits;
            }
            catch (Exception)
            {
                // A unit the table has not been taught fails the group where the tolerance
                // is set. It must not also throw out of a log line, because logging never
                // stops a run.
                return "UNKNOWN in \"" + Said(documentUnits) + "\", which is not a unit this tool converts";
            }
        }

        private static string Said(string value)
        {
            return value == null ? "" : value;
        }

        private static string Word(int count, string one, string many)
        {
            return count == 1 ? one : many;
        }

        public override string ToString()
        {
            return Label();
        }
    }
}
