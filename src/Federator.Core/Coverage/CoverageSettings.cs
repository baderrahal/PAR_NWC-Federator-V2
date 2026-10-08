using System;
using Federator.Core.Diagnostics;
using Federator.Core.Report;

namespace Federator.Core.Coverage
{
    /// <summary>
    /// Every number that shapes the coverage, F127, section 1.1 of the F127 design, which the loop keeps outside the repo as turn5\f127-design.md, a
    /// setting and never a constant. A value that cannot work is refused where it is set,
    /// never at the end of a run. Nought on a count of lines means every one, the way the
    /// image cap and the viewpoint cap read nought.
    ///
    /// The width of each column of the Coverage sheet joins these with the sheet itself,
    /// step 6 of the design, because nothing here lays the sheet out yet.
    /// </summary>
    public sealed class CoverageSettings
    {
        /// <summary>Bader's own word for the sheet, request 2 under Q112.</summary>
        public const string DefaultSheetName = "Coverage";

        /// <summary>The five and a count every repeated line in the log follows, A14.</summary>
        public const int DefaultExamplesPerReason = RunLog.KeptOfARepeat;

        /// <summary>
        /// The categories no set catches named in the log per model, by item count, the rest
        /// counted. Ten is the design's choice and nothing measured it. The sheet and the
        /// .tsv carry every one, the .tsv keeping every line, Bader's answer to Q108.
        /// </summary>
        public const int DefaultCategoriesNamedPerModel = 10;

        /// <summary>Nought, every FAILED line in RESULT, his words.</summary>
        public const int DefaultFailedLinesInResult = 0;

        /// <summary>
        /// Nought, every set that found nothing in any group of the run named, his words, so
        /// a set spelled wrong or pointing at nothing shows at once. It was a constant ten,
        /// and set 03's C06 run named 10 of its 14, log lines 8346 to 8357.
        /// </summary>
        public const int DefaultSetsAtZeroNamedInTheRun = 0;

        private string sheetName;
        private int examplesPerReason;
        private int categoriesNamedPerModel;
        private int failedLinesInResult;
        private int setsAtZeroNamedInTheRun;

        public CoverageSettings()
        {
            sheetName = DefaultSheetName;
            examplesPerReason = DefaultExamplesPerReason;
            categoriesNamedPerModel = DefaultCategoriesNamedPerModel;
            failedLinesInResult = DefaultFailedLinesInResult;
            setsAtZeroNamedInTheRun = DefaultSetsAtZeroNamedInTheRun;
        }

        /// <summary>The sheet's name. One Excel refuses is refused here, by SheetNames' own rule.</summary>
        public string SheetName
        {
            get
            {
                return sheetName;
            }

            set
            {
                if (!SheetNames.IsAcceptable(value))
                {
                    throw new ArgumentException(
                        "The Coverage sheet needs a name Excel accepts: 1 to " + SheetNames.MaxLength
                            + " characters, none of " + new string(SheetNames.Refused)
                            + ", no apostrophe at either end and not blank.",
                        "value");
                }

                sheetName = value;
            }
        }

        /// <summary>The tests named for each reason in the COVERAGE block before the rest are counted, one or more.</summary>
        public int ExamplesPerReason
        {
            get { return examplesPerReason; }
            set { examplesPerReason = AtLeast(1, value, "examples for each reason"); }
        }

        /// <summary>The categories no set catches named in the log per model, one or more.</summary>
        public int CategoriesNamedPerModel
        {
            get { return categoriesNamedPerModel; }
            set { categoriesNamedPerModel = AtLeast(1, value, "categories named for each model"); }
        }

        /// <summary>The FAILED lines RESULT carries before the rest are counted, nought for every one.</summary>
        public int FailedLinesInResult
        {
            get { return failedLinesInResult; }
            set { failedLinesInResult = AtLeast(0, value, "FAILED lines in RESULT"); }
        }

        /// <summary>The sets SETS ACROSS THE RUN names before the rest are counted, nought for every one.</summary>
        public int SetsAtZeroNamedInTheRun
        {
            get { return setsAtZeroNamedInTheRun; }
            set { setsAtZeroNamedInTheRun = AtLeast(0, value, "sets named across the run"); }
        }

        private static int AtLeast(int floor, int value, string what)
        {
            if (value < floor)
            {
                throw new ArgumentOutOfRangeException(
                    "value", value, "The " + what + " is " + floor + " or more"
                        + (floor == 0 ? ", and nought means every one." : "."));
            }

            return value;
        }
    }
}
