using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Diagnostics;
using Federator.Core.Report;

namespace Federator.Core.Coverage
{
    /// <summary>
    /// The clash count in Clash Detective against the workbook rows, per test of the picked
    /// file, F127. Bader's request 2 under Q112 says a count that differs is a FAILED line,
    /// not a note, and to use the F104 check for it, and his answer to the lead's notes says
    /// the FAILED line goes in COVERAGE and RESULT and the group keeps its own result.
    ///
    /// F104'S CHECKS 1 AND 2, RESTATED, not copied. The clash rows under a test's block
    /// against the document's results at the top level, a result group counting as one, and
    /// the Clashes cell against every clash in the test, the clashes inside each group
    /// counted. Both exact. Two differences from compare-document.ps1, said so neither is
    /// read as the other: its check 2 names an empty result group, which the harvest counts
    /// as one clash, and this check has no such words, and a test in the workbook only whose
    /// every number is nought is counted there and not judged, where here it is a test F77
    /// did not create and AGREES. compare-document.ps1 stays the independent witness, because
    /// this check reads the document through the add-in, which the harvest reads through too,
    /// and a check sharing the code it checks cannot catch a fault common to both.
    ///
    /// AGREE only when both numbers equal on both sides. A test the run did not hold as in the
    /// document that is not there, whose block reads no row and Clashes nought, is a test F77 did
    /// not create and is HELD BY NEITHER side, counted apart from Agree. FAILED for every other difference, a test the document
    /// holds with no block, and a block with numbers for a test the document does not hold.
    /// Where the run knows why, the line says it, and only what the record proves: a test
    /// already in the NWF and not run this run still holds an earlier run's results,
    /// decision 4 of the design at its default A, a test created this run that Clash
    /// Detective holds results for ran and then threw before its rows or count were taken,
    /// and Compact removed Resolved clashes after the rows were read where its count is at
    /// least the gap. NOT COMPARED, never Agree, where a count on either side is minus one, a
    /// name is on two tests of one side, a test is in neither, a side was not read, or a test
    /// the run holds as in the document was not returned by the read of it, with the reason.
    ///
    /// THE GROUP KEEPS ITS OWN RESULT BY CONSTRUCTION. Nothing here touches GroupFacts or
    /// adds an error, and a report check never fails a group.
    /// </summary>
    public sealed class CountCheck
    {
        /// <summary>
        /// What every FAILED line starts with. COVERAGE first, so the line is never read as a
        /// group's FAILED, since one prefix never reads as two things.
        /// </summary>
        public const string FailedPrefix = "COVERAGE FAILED";

        private readonly List<CountedTest> tests = new List<CountedTest>();
        private readonly List<string> notInTheXml = new List<string>();

        private CountCheck()
        {
            Tests = new ReadOnlyCollection<CountedTest>(tests);
            NotInTheXml = new ReadOnlyCollection<string>(notInTheXml);
        }

        /// <summary>One verdict per test handed in, in that order.</summary>
        public ReadOnlyCollection<CountedTest> Tests { get; private set; }

        /// <summary>The tests the document holds that the picked file does not, named and not judged.</summary>
        public ReadOnlyCollection<string> NotInTheXml { get; private set; }

        /// <summary>
        /// The verdict for each test of the coverage. The document's counts are null where
        /// they were not read, and the workbook is null where none was written or read, with
        /// the reason where known. Compacted is the clash step's own count of Resolved clashes
        /// Compact removed, minus one where compacting was not asked for.
        /// </summary>
        public static CountCheck Judge(
            IList<TestCoverage> tests,
            IList<DocumentTestCount> document,
            WorkbookTests workbook,
            string noWorkbookBecause,
            int compacted)
        {
            if (tests == null)
            {
                throw new ArgumentNullException("tests");
            }

            CountCheck check = new CountCheck();
            HashSet<string> inTheXml = new HashSet<string>(StringComparer.Ordinal);
            Dictionary<string, int> timesNamed = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (TestCoverage test in tests)
            {
                int seen;
                timesNamed[test.Name] = timesNamed.TryGetValue(test.Name, out seen) ? seen + 1 : 1;
            }

            foreach (TestCoverage test in tests)
            {
                inTheXml.Add(test.Name);

                // A name on two tests of the picked file is judged on neither, since both would be set
                // beside the one test of the document and the one block of the workbook, and the totals
                // would count a test that does not exist.
                check.tests.Add(timesNamed[test.Name] > 1
                    ? NotCompared(test.Name, "the name is on " + timesNamed[test.Name] + " tests of the picked file")
                    : One(test, document, workbook, noWorkbookBecause, compacted));
            }

            if (document != null)
            {
                HashSet<string> named = new HashSet<string>(StringComparer.Ordinal);

                foreach (DocumentTestCount held in document)
                {
                    if (held != null && !inTheXml.Contains(held.Name) && named.Add(held.Name))
                    {
                        check.notInTheXml.Add(held.Name);
                    }
                }
            }

            return check;
        }

        /// <summary>
        /// The FAILED line for one test, or null for a test that did not fail. Every number
        /// on both sides is in it, and it says the group keeps its own result.
        /// </summary>
        public static string FailedLine(string group, CountedTest test)
        {
            if (test == null || test.Verdict != CountVerdict.Failed)
            {
                return null;
            }

            string inDocument = test.InDocument
                ? "Clash Detective holds " + Count(test.DocumentTopLevel, "result", "results")
                    + " at the top level and " + Count(test.DocumentLeaves, "clash", "clashes")
                : "Clash Detective holds no test of that name";

            string inWorkbook = test.InWorkbook
                ? "the workbook " + Count(test.WorkbookRows, "row", "rows") + " and Clashes " + test.WorkbookClashes
                : "the workbook holds no block for it";

            // One line, whatever the exception message of a thrown test held.
            string why = test.Why.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");

            return FailedPrefix + "  " + Words.Or(group, "UNKNOWN") + "  " + test.Name + "  "
                + inDocument + ", " + inWorkbook
                + (why.Length > 0 ? ", " + why : string.Empty)
                + ". The group keeps its own result";
        }

        /// <summary>How many tests got that verdict, so nought FAILED is never read without what was compared.</summary>
        public int CountOf(CountVerdict verdict)
        {
            int count = 0;

            foreach (CountedTest test in tests)
            {
                if (test.Verdict == verdict)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Every FAILED line of this check, in the order of the file.</summary>
        public IList<string> FailedLines(string group)
        {
            List<string> lines = new List<string>();

            foreach (CountedTest test in tests)
            {
                string line = FailedLine(group, test);

                if (line != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        private static CountedTest One(
            TestCoverage test,
            IList<DocumentTestCount> document,
            WorkbookTests workbook,
            string noWorkbookBecause,
            int compacted)
        {
            if (workbook == null)
            {
                return NotCompared(test.Name, "no workbook was read"
                    + (string.IsNullOrEmpty(noWorkbookBecause) ? string.Empty : ", " + noWorkbookBecause));
            }

            if (document == null)
            {
                return NotCompared(test.Name, "Clash Detective's counts were not read");
            }

            List<DocumentTestCount> held = new List<DocumentTestCount>();

            foreach (DocumentTestCount count in document)
            {
                if (count != null && string.Equals(count.Name, test.Name, StringComparison.Ordinal))
                {
                    held.Add(count);
                }
            }

            IList<WorkbookTest> written = workbook.Named(test.Name);

            if (held.Count > 1)
            {
                return NotCompared(test.Name, "the name is on " + held.Count + " tests of the document");
            }

            if (written.Count > 1)
            {
                return NotCompared(test.Name, "the name is on " + written.Count + " tests of the workbook");
            }

            DocumentTestCount d = held.Count == 1 ? held[0] : null;
            WorkbookTest w = written.Count == 1 ? written[0] : null;
            int top = d == null ? -1 : d.TopLevel;
            int leaves = d == null ? -1 : d.Leaves;
            int rows = w == null ? -1 : w.Rows;
            int clashes = w == null ? -1 : w.Clashes;

            if (d != null && !d.Counted)
            {
                return new CountedTest(test.Name, CountVerdict.NotCompared, true, top, leaves, w != null, rows, clashes,
                    "Clash Detective's count of it could not be taken");
            }

            if (w != null && (w.Rows < 0 || w.Clashes < 0))
            {
                return new CountedTest(test.Name, CountVerdict.NotCompared, d != null, top, leaves, true, rows, clashes,
                    "its block could not be read whole");
            }

            if (d == null && w == null)
            {
                return NotCompared(test.Name, "it is in neither the document nor the workbook");
            }

            if (d == null)
            {
                if (w.Rows != 0 || w.Clashes != 0)
                {
                    return new CountedTest(test.Name, CountVerdict.Failed, false, -1, -1, true, rows, clashes, string.Empty);
                }

                // A test the runner created or found already there that the read did not
                // return has gone from the document, a walk that stopped early or a name
                // Navisworks changed, and a block of nought matches nothing.
                return test.Presence == TestPresence.CreatedThisRun || test.Presence == TestPresence.AlreadyThere
                    ? new CountedTest(test.Name, CountVerdict.NotCompared, false, -1, -1, true, rows, clashes,
                        "the run holds it as " + CoverageWords.For(test.Presence)
                            + " and the read of Clash Detective did not return it")
                    : new CountedTest(test.Name, CountVerdict.HeldByNeither, false, -1, -1, true, rows, clashes, string.Empty);
            }

            if (w == null)
            {
                return new CountedTest(test.Name, CountVerdict.Failed, true, top, leaves, false, -1, -1, string.Empty);
            }

            if (w.Rows == d.TopLevel && w.Clashes == d.Leaves)
            {
                return new CountedTest(test.Name, CountVerdict.Agree, true, top, leaves, true, rows, clashes, string.Empty);
            }

            return new CountedTest(test.Name, CountVerdict.Failed, true, top, leaves, true, rows, clashes,
                WhyItDiffers(test, d, w, compacted));
        }

        /// <summary>
        /// The reason the run knows for a difference, or empty where it knows none. Only what
        /// the record proves. The runner records a test as Failed for a throw anywhere from
        /// its creation to its count, its harvest included, and never as ran, so a Failed test
        /// is never said to be not run. A test created this run starts with no results, so
        /// one Clash Detective holds results for ran this run. Compact's count is the group's,
        /// so it is named as the cause only where it is at least this test's gap.
        /// </summary>
        private static string WhyItDiffers(TestCoverage test, DocumentTestCount d, WorkbookTest w, int compacted)
        {
            if (test.Reason == CoverageReason.Failed)
            {
                return test.Presence == TestPresence.CreatedThisRun && (d.TopLevel > 0 || d.Leaves > 0)
                    ? "it was created this run and Clash Detective holds results for it, so it ran this run, "
                        + "and then the clash step threw before its rows or its count were taken: " + test.Detail
                    : "the clash step threw on it, so whether it ran this run is UNKNOWN: " + test.Detail;
            }

            if (!test.Ran && test.Presence == TestPresence.AlreadyThere && d.Leaves > w.Clashes)
            {
                return "it was already in the document and was not run this run, "
                    + "so Clash Detective still holds an earlier run's results";
            }

            if (test.Ran && compacted > 0 && w.Clashes > d.Leaves)
            {
                int gap = w.Clashes - d.Leaves;

                // Compact's count is the group's and the test's own Resolved count is not handed in, so
                // Compact is named as something that could account for the gap and never as its cause.
                return "Compact removed " + Count(compacted, "Resolved clash", "Resolved clashes")
                    + " somewhere in the group after the workbook's rows were read, "
                    + (gap <= compacted
                        ? "which could account for this test's gap of " + gap
                        : "fewer than this test's gap of " + gap + ", so Compact is not the whole of it")
                    + ", and which test they were in is not recorded"
                    ;
            }

            return string.Empty;
        }

        private static CountedTest NotCompared(string name, string why)
        {
            return new CountedTest(name, CountVerdict.NotCompared, false, -1, -1, false, -1, -1, why);
        }

        private static string Count(int count, string one, string many)
        {
            return count + " " + (count == 1 ? one : many);
        }
    }
}
