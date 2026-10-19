using System;
using Federator.Core.Coverage;

namespace Federator.Core.Report
{
    /// <summary>
    /// The counts line of a group's coverage, F127, Bader's request 2 under Q112, which the COVERAGE block of the log carries.
    /// It was the head of the Coverage sheet of the internal workbook, and the sheet went with that workbook, F243, Bader's
    /// order of 19 Oct 2026 point 4: "no more internal ... files". Q226 lists each column the sheet held that the clash count
    /// check does not, for Bader to decide.
    /// </summary>
    public static class CoverageSheet
    {
        /// <summary>
        /// The counts line, off the rows alone: how many tests, how many in the document created this run or already there,
        /// not created, run, with clashes and without, each read off TestCoverage and never off another count.
        /// </summary>
        public static string Counts(CoverageSheetData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            int created = 0;
            int alreadyThere = 0;
            int notCreated = 0;
            int unknown = 0;
            int ran = 0;
            int withClashes = 0;

            foreach (TestCoverage test in data.Tests)
            {
                switch (test.Presence)
                {
                    case TestPresence.CreatedThisRun:
                        created++;
                        break;
                    case TestPresence.AlreadyThere:
                        alreadyThere++;
                        break;
                    case TestPresence.NotInDocument:
                        notCreated++;
                        break;
                    default:
                        unknown++;
                        break;
                }

                if (test.Ran)
                {
                    ran++;

                    if (test.Reason == CoverageReason.HasClashes)
                    {
                        withClashes++;
                    }
                }
            }

            return data.Tests.Count + (data.FromAPickedFile ? " tests in the clash matrix: " : " tests saved in the document: ")
                + created + " created this run, " + alreadyThere + " already there, " + notCreated + " not created"
                + (unknown > 0 ? ", " + unknown + " whose presence is UNKNOWN" : string.Empty)
                + ", " + ran + " run, " + withClashes + " with clashes, " + (ran - withClashes) + " without";
        }
    }
}
