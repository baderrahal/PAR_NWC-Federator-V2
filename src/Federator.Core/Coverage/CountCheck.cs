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
    /// did not create and is HELD BY NEITHER side. compare-document.ps1 stays the independent witness, because
    /// this check reads the document through the add-in, which the harvest reads through too,
    /// and a check sharing the code it checks cannot catch a fault common to both.
    ///
    /// AGREE only when both numbers equal on both sides and the test ran this run, a test that did not
    /// run with equal counts being NOT COMPARED. A test the run did not create, F77 kept it
    /// out or the plan dropped it, that is not in the document and has no block, is HELD BY NEITHER
    /// side, counted apart from Agree. One the run left no record of is NOT COMPARED instead. SINCE
    /// F153 A TEST WITH NO BLOCK READS NOUGHT ROWS AND CLASHES NOUGHT, since a test with no clash
    /// writes nothing in the workbook, Bader's decision of 2026-10-13, so a test that ran and found
    /// none agrees with a document holding none. SINCE F225 A TEST A RULE LEFT OUT, one discipline
    /// or one set code on both sides, that the document holds with no block of this run's workbook is
    /// NOT COMPARED with the rule's words, an NWF's test saved before the rule, kept and never deleted,
    /// and counted on its own line, row F219-R1. FAILED for every other difference, a test the
    /// document holds clashes of with no block among them, and a block with numbers for a test the
    /// document does not hold.
    /// Where the run knows why, the line says it, and only what the record proves: a test
    /// the plan dropped that the document holds all the same was there before this run, a test
    /// already in the NWF and not run this run still holds an earlier run's results,
    /// decision 4 of the design at its default A, a test created this run that Clash
    /// Detective holds results for ran and then threw before its rows or count were taken,
    /// and Compact removed Resolved clashes after the rows were read where its count is at
    /// least the gap. NOT COMPARED, never Agree, where a count on either side is minus one, a
    /// name is on two tests of one side, a side was not read, or a test
    /// the run holds as in the document, or left no record of, was not returned by the read of it,
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
        private int leftOutByARule;
        private int keptFromAnEarlierRun;
        private int keptClashes;
        private int leftOutHeldByNeither;
        private int leftOutFailed;

        private CountCheck()
        {
            Tests = new ReadOnlyCollection<CountedTest>(tests);
            NotInTheXml = new ReadOnlyCollection<string>(notInTheXml);
        }

        /// <summary>One verdict per test handed in, in that order.</summary>
        public ReadOnlyCollection<CountedTest> Tests { get; private set; }

        /// <summary>The tests the document holds that the clash matrix does not, named and not judged.</summary>
        public ReadOnlyCollection<string> NotInTheXml { get; private set; }

        /// <summary>
        /// Why a test that did not run this run is not compared where its two sides hold equal counts, so
        /// agree counts only the tests that ran.
        /// </summary>
        public const string DidNotRunWhy =
            "it did not run this run, so the equal counts in Clash Detective and the workbook check nothing this run did";

        /// <summary>
        /// Why a test of a mirrored pair F132 merges is not compared, said on its row and in the block.
        /// </summary>
        public const string InAMergeWhy =
            "it is a test of a mirrored pair F132's rule merges, whose clashes may sit in the kept test's block of the"
            + " clash workbook, so that block may not be one test's count and this test is not compared";

        /// <summary>
        /// The verdict for each test of the coverage. The document's counts are null where
        /// they were not read, and the workbook is null where none was written or read, with
        /// the reason where known. Compacted is the clash step's own count of Resolved clashes
        /// Compact removed, minus one where compacting was not asked for. InAMerge names the
        /// tests of every mirrored pair the merge made this run, the kept test and each mirror by
        /// the name it ran under, null where no pair merged: a merge carries a mirror's clashes in
        /// the kept test's block and leaves no block of the mirror's, so neither block is one
        /// test's count and each is NOT COMPARED, never a FAILED line the merge made. A pair the
        /// merge refused at run time is not named, so its tests are compared like any other.
        /// </summary>
        public static CountCheck Judge(
            IList<TestCoverage> tests,
            IList<DocumentTestCount> document,
            WorkbookTests workbook,
            string noWorkbookBecause,
            int compacted,
            ICollection<string> inAMerge = null)
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

                // A name on two tests of the clash matrix is judged on neither, since both would be set
                // beside the one test of the document and the one block of the workbook, and the totals
                // would count a test that does not exist.
                CountedTest judged = timesNamed[test.Name] > 1
                    ? NotCompared(test.Name, "the name is on " + timesNamed[test.Name] + " tests of the clash matrix")
                    : One(test, document, workbook, noWorkbookBecause, compacted, inAMerge);
                check.tests.Add(judged);

                // F225. Each test a rule left out counted by what the check read of it, so a holding not read is UNKNOWN
                // and never none, the reviewer's and the breaker's finding.
                if (!test.Ran && LeftOutByARule(test.Reason))
                {
                    check.leftOutByARule++;

                    if (judged.KeptFromAnEarlierRun)
                    {
                        check.keptFromAnEarlierRun++;
                        check.keptClashes += Math.Max(0, judged.DocumentLeaves);
                    }
                    else if (judged.Verdict == CountVerdict.HeldByNeither)
                    {
                        check.leftOutHeldByNeither++;
                    }
                    else if (judged.Verdict == CountVerdict.Failed)
                    {
                        check.leftOutFailed++;
                    }
                }
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
                ? "Clash Detective holds " + Words.Counted(test.DocumentTopLevel, "result", "results")
                    + " at the top level and " + Words.Counted(test.DocumentLeaves, "clash", "clashes")
                : "Clash Detective holds no test of that name";

            string inWorkbook = test.InWorkbook
                ? "the workbook " + Words.Counted(test.WorkbookRows, "row", "rows") + " and Clashes " + test.WorkbookClashes
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

        /// <summary>
        /// F225, row F219-R1. The one line counting the tests a rule left out of this group's run by what the check read of
        /// each: still held in Clash Detective from an earlier run and with how many clashes, kept in the NWF and never
        /// deleted, never FAILED, held by neither side, FAILED, such as one created this run that holds results, and the rest
        /// whose holding is UNKNOWN, a side not read, a count not taken or a name on two tests. Null where no rule left a test
        /// out.
        /// </summary>
        public string LeftOutByARuleLine()
        {
            if (leftOutByARule == 0)
            {
                return null;
            }

            int unknown = leftOutByARule - keptFromAnEarlierRun - leftOutHeldByNeither - leftOutFailed;

            return "not run by the rule : " + Words.Counted(leftOutByARule, "test", "tests")
                + " left out by the rule of one discipline or of two set codes, "
                + keptFromAnEarlierRun + " still held in Clash Detective from an earlier run with "
                + Words.Counted(keptClashes, "clash", "clashes") + ", kept in the NWF and never deleted, not compared and never FAILED, "
                + leftOutHeldByNeither + " held by neither side, "
                + (leftOutFailed > 0 ? leftOutFailed + " FAILED, " : string.Empty)
                + unknown + " whose holding in Clash Detective is UNKNOWN, a side or a count not read or a name on two tests"
                + ", whether those held are removed is Bader's to decide, Q212";
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
            int compacted,
            ICollection<string> inAMerge)
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
            bool inWorkbook = w != null;

            // F153, Bader's decision of 2026-10-13. A test with no clash writes nothing in the workbook, so
            // a test with no block is one the workbook says found none, nought rows and Clashes nought,
            // set beside Clash Detective like any other. Before F153 it was the one row of Q73, and a test
            // with no block at all was a FAILED line whatever the document held.
            if (w == null)
            {
                w = new WorkbookTest(test.Name, 0, 0, 0);
            }

            int top = d == null ? -1 : d.TopLevel;
            int leaves = d == null ? -1 : d.Leaves;
            int rows = w.Rows;
            int clashes = w.Clashes;

            if (inAMerge != null && inAMerge.Contains(test.Name))
            {
                // A mirror merged into its kept test has its clashes in that block, so a missing block of its own
                // is not a count of nought, the reviewer's point on F153, and its cells read UNKNOWN.
                return new CountedTest(test.Name, CountVerdict.NotCompared, d != null, top, leaves, inWorkbook,
                    inWorkbook ? rows : -1, inWorkbook ? clashes : -1, InAMergeWhy);
            }

            if (d != null && !d.Counted)
            {
                return new CountedTest(test.Name, CountVerdict.NotCompared, true, top, leaves, inWorkbook, rows, clashes,
                    "Clash Detective's count of it could not be taken");
            }

            if (w.Rows < 0 || w.Clashes < 0)
            {
                return new CountedTest(test.Name, CountVerdict.NotCompared, d != null, top, leaves, inWorkbook, rows, clashes,
                    "its block could not be read whole");
            }

            // F225, row F219-R1. A test a rule left out of this run, one discipline, Q203, or one set code on both sides, Q209,
            // did not run, and one an NWF saved before the rule still holds there with an earlier run's results, kept and
            // never deleted, so its count beside a workbook of this run is never FAILED. One the rule of two set codes left out
            // of a plan before any test was looked for reads a presence UNKNOWN, one of one discipline was walked for and reads
            // already there, and one created this run holds no earlier run's results, so it is judged as before. A block of this
            // run's workbook for it, which the writer never makes, is compared as before, so a writer's fault stays FAILED.
            if (!test.Ran && d != null && w.Rows == 0 && w.Clashes == 0
                && (test.Presence == TestPresence.AlreadyThere
                    || (test.Presence == TestPresence.Unknown && test.Reason == CoverageReason.SameSetCode))
                && LeftOutByARule(test.Reason))
            {
                return new CountedTest(test.Name, CountVerdict.NotCompared, true, top, leaves, inWorkbook, rows, clashes,
                    LeftOutWhy(test.Reason, d), true);
            }

            if (d == null)
            {
                if (w.Rows != 0 || w.Clashes != 0)
                {
                    return new CountedTest(test.Name, CountVerdict.Failed, false, -1, -1, inWorkbook, rows, clashes, string.Empty);
                }

                // A test the runner created or found already there that the read did not
                // return has gone from the document, a walk that stopped early or a name
                // Navisworks changed, and a block of nought matches nothing.
                if (test.Presence == TestPresence.CreatedThisRun || test.Presence == TestPresence.AlreadyThere)
                {
                    return new CountedTest(test.Name, CountVerdict.NotCompared, false, -1, -1, inWorkbook, rows, clashes,
                        "the run holds it as " + CoverageWords.For(test.Presence)
                            + " and the read of Clash Detective did not return it");
                }

                // Held by neither side is for a test the run knows it did not create: F77 kept it out, or
                // the plan dropped it before the model. One the run left no record of, or never reached,
                // may be in the document and not returned, so it is not compared.
                if (test.Presence == TestPresence.Unknown && !RunDidNotCreate(test))
                {
                    return new CountedTest(test.Name, CountVerdict.NotCompared, false, -1, -1, inWorkbook, rows, clashes,
                        "the run left no record of whether it is in the document, and the read of Clash Detective did not return it");
                }

                return new CountedTest(test.Name, CountVerdict.HeldByNeither, false, -1, -1, inWorkbook, rows, clashes, string.Empty);
            }

            if (w.Rows == d.TopLevel && w.Clashes == d.Leaves)
            {
                // AGREE ONLY FOR A TEST THAT RAN, the breaker's third finding on F127b. Equal counts of a
                // test that did not run this run, nought and nought most often, check nothing this run did.
                return test.Ran
                    ? new CountedTest(test.Name, CountVerdict.Agree, true, top, leaves, inWorkbook, rows, clashes, string.Empty)
                    : new CountedTest(test.Name, CountVerdict.NotCompared, true, top, leaves, inWorkbook, rows, clashes, DidNotRunWhy);
            }

            return new CountedTest(test.Name, CountVerdict.Failed, true, top, leaves, inWorkbook, rows, clashes,
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

            // A test the plan dropped before the model is never looked for in the document, so its presence
            // is UNKNOWN. If Clash Detective holds a test of its name with results, this run did not create
            // it, and one it did not create was there before, with the results of the run that did. A test
            // F77 kept out is not here: the runner walked the document first and it found none of that name.
            if (test.Presence == TestPresence.Unknown && RunDidNotCreate(test) && (d.TopLevel > 0 || d.Leaves > 0))
            {
                return "the run did not create it, so the test Clash Detective holds under that name was in the "
                    + "document before this run and its results are an earlier run's";
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
                return "Compact removed " + Words.Counted(compacted, "Resolved clash", "Resolved clashes")
                    + " somewhere in the group after the workbook's rows were read, "
                    + (gap <= compacted
                        ? "which could account for this test's gap of " + gap
                        : "fewer than this test's gap of " + gap + ", so Compact is not the whole of it")
                    + ", and which test they were in is not recorded"
                    ;
            }

            return string.Empty;
        }

        /// <summary>Whether a rule of what runs left the test out, one discipline in the group, Q203, or one set code on both sides, Q209.</summary>
        private static bool LeftOutByARule(CoverageReason reason)
        {
            return reason == CoverageReason.OneDiscipline || reason == CoverageReason.SameSetCode;
        }

        /// <summary>
        /// Why a test a rule left out that Clash Detective still holds is not compared, its rule and what it holds said, and
        /// that it is kept, row F219-R1 and Q212.
        /// </summary>
        private static string LeftOutWhy(CoverageReason reason, DocumentTestCount d)
        {
            string holds = d.TopLevel > 0 || d.Leaves > 0
                ? "Clash Detective still holds " + Words.Counted(d.TopLevel, "result", "results") + " and "
                    + Words.Counted(d.Leaves, "clash", "clashes") + " for it from an earlier run"
                : "Clash Detective holds the test with no result";

            return "not run by the rule, " + CoverageWords.For(reason) + ", and " + holds
                + ", kept in the NWF and never deleted, so it is not compared";
        }

        /// <summary>
        /// Whether the record shows the run did not create the test, the plan dropped it, no test resolved a set, or the rule of
        /// two set codes left it out, F224.
        /// </summary>
        private static bool RunDidNotCreate(TestCoverage test)
        {
            return test.Reason == CoverageReason.NotCreated || test.Reason == CoverageReason.NoTestResolvesASet
                || test.Reason == CoverageReason.SameSetCode;
        }

        private static CountedTest NotCompared(string name, string why)
        {
            return new CountedTest(name, CountVerdict.NotCompared, false, -1, -1, false, -1, -1, why);
        }
    }
}
