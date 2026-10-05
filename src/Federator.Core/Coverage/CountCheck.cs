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
    /// F104'S CHECKS 1 AND 2, RESTATED. The clash rows under a test's block against the
    /// document's results at the top level, a result group counting as one, and the Clashes
    /// cell against every clash in the test, the clashes inside each group counted. Both
    /// exact. compare-document.ps1 stays the independent witness, because this check reads
    /// the document through the add-in, which the harvest reads through too, and a check
    /// sharing the code it checks cannot catch a fault common to both.
    ///
    /// AGREE only when both numbers equal on both sides, or when the test is not in the
    /// document and its block reads no row and Clashes nought, which is a test F77 did not
    /// create. FAILED for every other difference, a test the document holds with no block,
    /// and a block with numbers for a test the document does not hold. Where the run knows
    /// why, the line says it: a test not run this run whose NWF still holds an earlier run's
    /// results, decision 4 of the design at its default A, and results Compact removed after
    /// the rows were read. NOT COMPARED, never Agree, where a count on either side is minus
    /// one, a name is on two tests of one side, a test is in neither, or a side was not read,
    /// with the reason.
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

            foreach (TestCoverage test in tests)
            {
                inTheXml.Add(test.Name);
                check.tests.Add(One(test, document, workbook, noWorkbookBecause, compacted));
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

            return FailedPrefix + "  " + Words.Or(group, "UNKNOWN") + "  " + test.Name + "  "
                + inDocument + ", " + inWorkbook
                + (test.Why.Length > 0 ? ", " + test.Why : string.Empty)
                + ". The group keeps its own result";
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
                return new CountedTest(test.Name,
                    w.Rows == 0 && w.Clashes == 0 ? CountVerdict.Agree : CountVerdict.Failed,
                    false, -1, -1, true, rows, clashes, string.Empty);
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

        /// <summary>The reason the run knows for a difference, or empty where it knows none.</summary>
        private static string WhyItDiffers(TestCoverage test, DocumentTestCount d, WorkbookTest w, int compacted)
        {
            if (!test.Ran && d.Leaves > w.Clashes)
            {
                return "it was not run this run, so the NWF still holds an earlier run's results "
                    + "and the workbook carries none of them";
            }

            if (test.Ran && compacted > 0 && w.Clashes > d.Leaves)
            {
                return "Compact removed " + Count(compacted, "Resolved clash", "Resolved clashes")
                    + " from the document after the workbook's rows were read";
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
