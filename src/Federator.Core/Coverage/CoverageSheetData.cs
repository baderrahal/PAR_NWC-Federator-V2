using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Coverage
{
    /// <summary>
    /// What one group's coverage holds, F127, Bader's request 2 under Q112 and his decision under Q46, FR-200: every test of
    /// the clash matrix, or of the tests saved in the document where none was picked, with what happened to it, so a test
    /// that was not created is in the COVERAGE block and the .tsv with its reason and nothing is missed without a line saying
    /// so. It was what the Coverage sheet of the internal workbook was written from, and the models, the sets and the
    /// priorities only that sheet read went with it, F243, Bader's order of 19 Oct 2026 point 4.
    ///
    /// EVERYTHING HERE WAS READ BY THE RUN AND NOTHING IS WORKED OUT HERE. The tests are CoverageRule's rows and the counts
    /// are CountCheck's verdicts. A check not handed in is UNKNOWN and never nought.
    /// </summary>
    public sealed class CoverageSheetData
    {
        /// <summary>The words the block carries where no file was picked and the saved tests were run.</summary>
        public const string SavedTests = "the tests saved in the document";

        /// <summary>The words where the clash matrix had no list of corrections beside it.</summary>
        public const string CorrectedByNothing = "corrected by nothing";

        public CoverageSheetData(
            string source,
            string sha256,
            string corrections,
            IList<TestCoverage> tests,
            CountCheck check)
        {
            if (tests == null)
            {
                throw new ArgumentNullException("tests");
            }

            Source = string.IsNullOrEmpty(source) ? SavedTests : source;
            Sha256 = string.IsNullOrEmpty(sha256) ? "UNKNOWN" : sha256;
            Corrections = string.IsNullOrEmpty(corrections) ? CorrectedByNothing : corrections;
            Tests = new ReadOnlyCollection<TestCoverage>(new List<TestCoverage>(tests));
            Check = check;
        }

        /// <summary>The clash matrix's full path, or SavedTests.</summary>
        public string Source { get; private set; }

        /// <summary>The clash matrix's sha256, FileFingerprint, or UNKNOWN.</summary>
        public string Sha256 { get; private set; }

        /// <summary>The full path of the list of corrections read beside the clash matrix, or CorrectedByNothing.</summary>
        public string Corrections { get; private set; }

        /// <summary>Every test, in the order of the file.</summary>
        public ReadOnlyCollection<TestCoverage> Tests { get; private set; }

        /// <summary>The count check, or null where it was not taken, which the block says.</summary>
        public CountCheck Check { get; private set; }

        /// <summary>Whether the tests are the clash matrix's rather than the saved ones.</summary>
        public bool FromAPickedFile
        {
            get { return !string.Equals(Source, SavedTests, StringComparison.Ordinal); }
        }
    }
}
