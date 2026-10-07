using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Clash;
using Federator.Core.Health;
using Federator.Core.Sets;

namespace Federator.Core.Coverage
{
    /// <summary>
    /// What one group's Coverage sheet shows, F127, Bader's request 2 under Q112 and his
    /// decision under Q46, FR-200: every test of the picked file, or of the tests saved in the
    /// document where none was picked, with what happened to it, so a test that was not created
    /// is on the sheet with its reason and nothing is missed without a line saying so.
    ///
    /// EVERYTHING HERE WAS READ BY THE RUN AND NOTHING IS WORKED OUT HERE. The tests are
    /// CoverageRule's rows, the counts are CountCheck's verdicts, the sets are the sets step's
    /// own results and the models are the EXPORT CHECK's. A part not handed in, the check or
    /// the sets, is written as UNKNOWN on the sheet and never as nought.
    /// </summary>
    public sealed class CoverageSheetData
    {
        /// <summary>The words the sheet carries where no file was picked and the saved tests were run.</summary>
        public const string SavedTests = "the tests saved in the document";

        /// <summary>The words where the picked file had no list of corrections beside it.</summary>
        public const string CorrectedByNothing = "corrected by nothing";

        public CoverageSheetData(
            string source,
            string sha256,
            string corrections,
            IList<ModelExport> models,
            IList<TestCoverage> tests,
            CountCheck check,
            IList<SetResult> sets,
            PriorityMap priorities)
        {
            if (tests == null)
            {
                throw new ArgumentNullException("tests");
            }

            Source = string.IsNullOrEmpty(source) ? SavedTests : source;
            Sha256 = string.IsNullOrEmpty(sha256) ? "UNKNOWN" : sha256;
            Corrections = string.IsNullOrEmpty(corrections) ? CorrectedByNothing : corrections;
            Models = new ReadOnlyCollection<ModelExport>(new List<ModelExport>(models ?? new List<ModelExport>()));
            Tests = new ReadOnlyCollection<TestCoverage>(new List<TestCoverage>(tests));
            Check = check;
            Sets = sets == null ? null : new ReadOnlyCollection<SetResult>(new List<SetResult>(sets));
            Priorities = priorities ?? PriorityMap.NothingPicked();
        }

        /// <summary>The picked file's full path, or SavedTests.</summary>
        public string Source { get; private set; }

        /// <summary>The picked file's sha256, FileFingerprint, or UNKNOWN.</summary>
        public string Sha256 { get; private set; }

        /// <summary>The full path of the list of corrections read beside the picked file, or CorrectedByNothing.</summary>
        public string Corrections { get; private set; }

        /// <summary>The group's models as the EXPORT CHECK read them, each with its code and its elements.</summary>
        public ReadOnlyCollection<ModelExport> Models { get; private set; }

        /// <summary>Every test, in the order of the file.</summary>
        public ReadOnlyCollection<TestCoverage> Tests { get; private set; }

        /// <summary>The count check, or null where it was not taken, which the sheet says.</summary>
        public CountCheck Check { get; private set; }

        /// <summary>The sets step's results, or null where no set was built, which the sheet says.</summary>
        public ReadOnlyCollection<SetResult> Sets { get; private set; }

        /// <summary>The priority file, or NothingPicked.</summary>
        public PriorityMap Priorities { get; private set; }

        /// <summary>Whether the tests are the picked file's rather than the saved ones.</summary>
        public bool FromAPickedFile
        {
            get { return !string.Equals(Source, SavedTests, StringComparison.Ordinal); }
        }
    }
}
