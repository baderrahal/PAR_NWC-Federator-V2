using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Exchange;

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

        /// <summary>Read off the clash XML, converted into the document units.</summary>
        File = 2,

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
    /// THE DEFAULT CHANGES NOTHING. Use the value in the XML is what the tool has always
    /// done and is what it still does unless somebody chooses otherwise, so a run set up
    /// the old way behaves the old way.
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
        /// <summary>The first entry of the drop down and the default.</summary>
        public const string UseTheFile = "Use the value in the XML";

        /// <summary>The last entry, which turns the number box on.</summary>
        public const string Other = "Other";

        /// <summary>The three the drop down offers between those two, in millimetres.</summary>
        public static readonly double[] OfferedMillimetres = { 25.0, 50.0, 75.0 };

        /// <summary>The words beside the drop down.</summary>
        public const string PickerLabel = "Clash tolerance";

        /// <summary>The grey line under it. Twelve words, which is the limit.</summary>
        public const string HelpLine =
            "Beats the XML and the document. Changing a saved test resets results";

        /// <summary>The words that begin every line this rule writes.</summary>
        public const string Prefix = "TOLERANCE";

        private readonly bool chosen;
        private readonly double millimetres;

        private ToleranceChoice(bool chosen, double millimetres)
        {
            this.chosen = chosen;
            this.millimetres = millimetres;
        }

        /// <summary>The default. Every test keeps whatever its own source gave it.</summary>
        public static ToleranceChoice FromTheFile()
        {
            return new ToleranceChoice(false, 0.0);
        }

        /// <summary>
        /// A tolerance chosen in the tool, in millimetres. Zero is a real tolerance and is
        /// allowed, because a file may carry one and the tool must be able to say the same
        /// thing. A negative one is not a tolerance at all and is refused where it is set,
        /// rather than reaching 1830 tests and being discovered in a report.
        /// </summary>
        public static ToleranceChoice Of(double millimetres)
        {
            if (double.IsNaN(millimetres) || double.IsInfinity(millimetres))
            {
                throw new ArgumentOutOfRangeException(
                    "millimetres", "A tolerance that is not a number is not a tolerance.");
            }

            if (millimetres < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "millimetres",
                    "A tolerance below zero is not a tolerance. Zero is a real one and is allowed.");
            }

            return new ToleranceChoice(true, millimetres);
        }

        /// <summary>Whether a value was chosen at all.</summary>
        public bool ChosenInTheTool
        {
            get { return chosen; }
        }

        /// <summary>The chosen value in millimetres. Zero where nothing was chosen.</summary>
        public double Millimetres
        {
            get { return millimetres; }
        }

        /// <summary>Where a test's tolerance comes from under this choice.</summary>
        public ToleranceOrigin Origin
        {
            get { return chosen ? ToleranceOrigin.Tool : ToleranceOrigin.File; }
        }

        /// <summary>The drop down entries, in order, with the default first.</summary>
        public static IList<string> Choices()
        {
            List<string> choices = new List<string>();
            choices.Add(UseTheFile);

            foreach (double value in OfferedMillimetres)
            {
                choices.Add(Words(value));
            }

            choices.Add(Other);
            return choices;
        }

        /// <summary>What the drop down reads once this choice is made.</summary>
        public string Label()
        {
            return chosen ? Words(millimetres) : UseTheFile;
        }

        /// <summary>
        /// The chosen tolerance in the units the open document measures in. Throws on a
        /// unit the table has not been taught, the same way a file tolerance does, because
        /// a report in the wrong unit reads as real and is not.
        /// </summary>
        public double InDocumentUnits(string documentUnits)
        {
            if (!chosen)
            {
                throw new InvalidOperationException(
                    "Nothing was chosen, so there is no tolerance to convert. "
                        + "Ask ChosenInTheTool before asking for this.");
            }

            return ExchangeUnits.Convert(millimetres, "mm", documentUnits);
        }

        /// <summary>
        /// What one test's tolerance ends up as, in document units. The whole rule in one
        /// place: a chosen value wins, and otherwise whatever the caller already worked
        /// out from the file or read off the document stands.
        /// </summary>
        public double For(double otherwise, string documentUnits)
        {
            return chosen ? InDocumentUnits(documentUnits) : otherwise;
        }

        /// <summary>
        /// The line one group writes. Both counts are read, neither is worked out from the
        /// other, and the converted value is on it so nobody has to do the arithmetic to
        /// find out what 25 mm was in a document measuring feet.
        /// </summary>
        public string LogLine(int created, int alreadyInTheDocument, string documentUnits)
        {
            int total = created + alreadyInTheDocument;

            if (!chosen)
            {
                return Prefix + " read per test out of the XML, which is what this tool does "
                    + "unless a tolerance is chosen on the Clash step. " + total
                    + Word(total, " test", " tests") + ", " + created + " created fresh and "
                    + alreadyInTheDocument + " left as the document has them";
            }

            return Prefix + " " + Words(millimetres) + " chosen in the tool, so it beats both "
                + "the XML and the document. Set on " + total + Word(total, " test", " tests")
                + ", " + created + " created fresh and " + alreadyInTheDocument
                + " already in the document. " + Words(millimetres) + " is " + Converted(documentUnits)
                + " in this document";
        }

        /// <summary>
        /// What the confirm screen says before the run starts, or nothing at all where the
        /// default is in force, because a line saying the tool is doing what it has always
        /// done on every run teaches people to skip the screen.
        /// </summary>
        public IList<string> WarningLines(int groups, int testsInTheFile)
        {
            List<string> lines = new List<string>();

            if (!chosen)
            {
                return lines;
            }

            lines.Add(PickerLabel + ": " + Words(millimetres) + ", chosen in the tool.");
            lines.Add("It is set on every clash test in this run, which is " + groups
                + Word(groups, " group", " groups") + " of " + testsInTheFile
                + Word(testsInTheFile, " test", " tests") + " from the XML, plus every test "
                + "already saved in each NWF.");
            lines.Add("It beats the tolerance in the XML and the tolerance in the document.");
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
        /// </summary>
        public static string BlocksLine(int blocks, int ran)
        {
            int neverRan = blocks - ran;
            System.Globalization.CultureInfo invariant = System.Globalization.CultureInfo.InvariantCulture;

            return "WORKBOOK " + blocks.ToString("N0", invariant) + " test blocks, "
                + ran.ToString("N0", invariant) + " ran and " + neverRan.ToString("N0", invariant)
                + " are tests that never ran. Their tolerance reads off the picked file";
        }

        /// <summary>
        /// The line that says where the tolerance on every report row was READ, F76. The
        /// report is produced by the clash tests in the open document, so every one of
        /// them should read off the document. A count under any of the other three is the
        /// report saying a number the run did not clash at, which is what happened: the
        /// matrix is written at 25 mm, the NWFs on disk held tests at 75 mm, and the
        /// report said 25 while the run clashed at 75.
        ///
        /// Every count is READ off the rows. None is worked out from another, and the
        /// total is on the end so a row the tally missed shows as a difference.
        /// </summary>
        public static string ReadFromLine(int fromDocument, int fromFile, int fromTool, int unknown)
        {
            int total = fromDocument + fromFile + fromTool + unknown;

            return Prefix + " on the report, read off " + Words(ToleranceOrigin.Document)
                + " for " + fromDocument + ", " + Words(ToleranceOrigin.File)
                + " for " + fromFile + ", " + Words(ToleranceOrigin.Tool)
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
                case ToleranceOrigin.File: return "the clash XML";
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
