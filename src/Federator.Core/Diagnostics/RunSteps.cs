using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The steps a group goes through, in the order it meets them, and the only place any
    /// of their names is written down.
    ///
    /// WHY A LIST AND NOT A STRING AT EACH CALL. The log has to answer where the time
    /// went, and it can only do that if the same work carries the same name every time it
    /// is timed. A name typed at the call is a name that can be typed differently at the
    /// next call, and then one step reads as two in the timing block and neither total is
    /// the truth. Nothing outside this file types a step name.
    ///
    /// A step is not the same as a method. APPEND covers building from scratch and
    /// rebuilding from the scan, because both put the models in and a reader asking where
    /// the time went wants one number for that. TESTS RUN is entered once per test and
    /// its seconds add up across every one of them.
    /// </summary>
    public static class RunSteps
    {
        public const string Decide = "DECIDE";
        public const string Append = "APPEND";
        public const string NwfSave = "NWF SAVE";
        public const string Units = "UNITS";
        public const string Sets = "SETS";
        public const string TestsCreate = "TESTS CREATE";
        public const string TestsRun = "TESTS RUN";
        public const string Harvest = "HARVEST";

        /// <summary>
        /// The picture pass, F149: every picture taken once every test has run and every mirror
        /// merge is made, in the report's order, so each is written under its final name. IMAGES,
        /// one visit per picture asked for, nests inside it, where it nested inside HARVEST while
        /// the pictures were taken as the rows were read. Attempt 1 ran the pass inside no step,
        /// so its walk, its resolves and its hiding came off no total, the reviewer's third point.
        /// </summary>
        public const string Pictures = "PICTURES";

        /// <summary>
        /// Building the saved viewpoints, F85. It runs after the clash step and before the
        /// second NWF save, and it was UNTIMED before this: BuildViewpoints was called
        /// outside every step, so the seconds it cost came off no total and the run read as
        /// faster than it was. One viewpoint per clash is the most expensive thing this
        /// tool does and criterion 2 is forty five minutes, so where that time went is the
        /// question the log exists to answer.
        /// </summary>
        public const string Views = "VIEWS";
        public const string Images = "IMAGES";

        /// <summary>
        /// Renaming the pictures into report order, FR-077, once the clash step has run
        /// every test and before the viewpoints read the rows. It ran inside no step before
        /// this, 20.6 s of one group of set 03 in none of the totals.
        /// </summary>
        public const string Renumber = "RENUMBER";
        public const string Workbook = "WORKBOOK";
        public const string Html = "HTML";
        public const string Xml = "XML";

        /// <summary>
        /// The coverage of the picked tests, F127's add-in half: one walk of Clash Detective's
        /// tests for their counts, the clash workbook read back test by test, the count check
        /// and the COVERAGE block. After the clash workbook, its page and its XML, because it
        /// reads that workbook, and timed because the walk's seconds were UNKNOWN in the design.
        /// </summary>
        public const string Coverage = "COVERAGE";

        /// <summary>
        /// The internal workbook beside the clash workbook, Bader's answer B to Q146, holding the
        /// Generic Models sheet of F128 and the Coverage sheet, written once both are taken. It
        /// was F128's GENERIC XLSX, when the workbook held the Generic Models sheet alone. Eight letters and
        /// not INTERNAL XLSX, because a name longer than TESTS CREATE widens every STEP line of the log.
        /// </summary>
        public const string InternalWorkbook = "INTERNAL";
        public const string Nwd = "NWD";
        public const string Confirm = "CONFIRM";

        private static readonly string[] InOrder =
        {
            Decide,
            Append,
            NwfSave,
            Units,
            Sets,
            TestsCreate,
            TestsRun,
            Harvest,
            Pictures,
            Images,
            Renumber,
            Views,
            Workbook,
            Html,
            Xml,
            Coverage,
            InternalWorkbook,
            Nwd,
            Confirm
        };

        /// <summary>
        /// Every step, in the order a group meets them. Read only, because a caller that
        /// could add to it could add a step this file does not name.
        /// </summary>
        public static IList<string> All
        {
            get { return new ReadOnlyCollection<string>(InOrder); }
        }

        /// <summary>
        /// Whether this is one of the steps. Ordinal and never trimmed, the way every
        /// other name in this tool is matched.
        /// </summary>
        public static bool IsAStep(string name)
        {
            return OrderOf(name) >= 0;
        }

        /// <summary>Where the step sits in the order, or minus one when it is not one.</summary>
        public static int OrderOf(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }

            for (int i = 0; i < InOrder.Length; i++)
            {
                if (string.Equals(InOrder[i], name, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// How wide the name column is, read off the longest name rather than typed, so
        /// adding a longer step cannot leave the block ragged.
        /// </summary>
        public static int NameWidth
        {
            get
            {
                int widest = 0;

                for (int i = 0; i < InOrder.Length; i++)
                {
                    if (InOrder[i].Length > widest)
                    {
                        widest = InOrder[i].Length;
                    }
                }

                return widest;
            }
        }

        /// <summary>The name padded to the column width, so every step line starts alike.</summary>
        public static string Padded(string name)
        {
            return (name ?? string.Empty).PadRight(NameWidth);
        }

        /// <summary>
        /// The words for a name that is not on the list. A step is refused rather than
        /// logged under a name nothing else uses, because a timing block holding a step
        /// nobody named is worse than one that is short.
        /// </summary>
        public static string NotAStep(string name)
        {
            return "\"" + (name ?? "null") + "\" is not one of the steps this tool times. "
                + "The steps are " + string.Join(", ", InOrder) + ".";
        }
    }
}
