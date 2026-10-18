using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Clash;

namespace Federator.Core.Views
{
    /// <summary>
    /// Which tests walk one read WHOLE for the views, F114 attempt 2 of the add-in pass, the
    /// breaker's B1 and the second reading's N1, C1, N2 and F3. Every call is keyed by the
    /// REPORT's test name of the row, the kept test for a clash only a mirror found, never the
    /// name the row's test ran under, so a kept test is whole only where all its rows, its own
    /// and its mirror's, were read. A resolve or walk that serves rows of more than one report
    /// test and fails or throws marks every one of them.
    ///
    /// A test is read whole when it was found at its recorded address, every IN SCOPE row of it
    /// led to its result and nothing threw in its walk. A row outside the views' statuses needs
    /// nothing read, so its result gone, Compact having removed a Resolved one, or its place not
    /// recorded, never makes its test not read, C1. A test the clash step ran with no row on the
    /// report is read whole with no row, N2, so its old views go as no longer needed.
    ///
    /// A test the clash step skipped as FAILED is never read whole, row F114-K38, the reviewer's
    /// D1: its harvest may have thrown after reading only closed rows, so the report holds no
    /// open clash of it while the document may. A failed test with no row on the report is never
    /// met and stays as it was, its old views kept.
    ///
    /// Only a whole test gets a view and loses its old views. A test read in part or not at all
    /// is NOT READ: no view, its old views kept, counted failed with why, so the group is not
    /// DONE on it.
    ///
    /// RESULT GROUPS, F114-K36, Bader's answer B of 2026-10-13, which retires the lead's F3: a
    /// test whose open rows are all result groups is no longer left without a view. The add-in
    /// reads the clashes inside each group and plans each at its own status, so a group row is
    /// read like any row and a test of groups alone is whole when its groups were read whole.
    /// </summary>
    public sealed class TestsRead
    {
        private readonly HashSet<ClashStatus> inScope;
        private readonly List<string> order = new List<string>();
        private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<string>> clashStepFailed = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        /// <summary>A record of the tests read, judged against the views' statuses.</summary>
        public TestsRead(IEnumerable<ClashStatus> inScope)
        {
            this.inScope = new HashSet<ClashStatus>(inScope ?? new ClashStatus[0]);
        }

        /// <summary>Whether a row at that status must be read for a view, which is a row the views show.</summary>
        public bool NeedsReading(ClashStatus status)
        {
            return inScope.Contains(status);
        }

        /// <summary>The test was met, by the name the report holds its rows under.</summary>
        public void Met(string test)
        {
            EntryOf(test);
        }

        /// <summary>Every test the clash step ran is met, so one with no row on the report is whole with no row, N2.</summary>
        public void Ran(IEnumerable<string> tests)
        {
            foreach (string test in tests ?? new string[0])
            {
                if (!string.IsNullOrEmpty(test))
                {
                    EntryOf(test);
                }
            }
        }

        /// <summary>
        /// Every test the clash step skipped as failed is not read whole, met before or after,
        /// F114-K38, and so is the test kept of a mirror that failed, by keptOfMirror, the kept
        /// test's name by each mirror's run name, since the mirror's clashes are merged into the
        /// kept test's rows, the breaker's point 2 on attempt 1. Null where no mirror was paired.
        /// </summary>
        public void ClashStepFailed(IEnumerable<SkippedClashTest> skipped, IDictionary<string, string> keptOfMirror)
        {
            foreach (SkippedClashTest test in skipped ?? new SkippedClashTest[0])
            {
                if (test == null || test.Kind != ClashSkipReason.Failed || string.IsNullOrEmpty(test.Name))
                {
                    continue;
                }

                MarkFailed(test.Name, "the clash step failed on it");

                string kept;

                if (keptOfMirror != null && keptOfMirror.TryGetValue(test.Name, out kept) && !string.IsNullOrEmpty(kept))
                {
                    MarkFailed(kept, "the clash step failed on its mirror " + test.Name);
                }
            }
        }

        private void MarkFailed(string test, string why)
        {
            List<string> said;

            if (!clashStepFailed.TryGetValue(test, out said))
            {
                said = new List<string>();
                clashStepFailed.Add(test, said);
            }

            if (!said.Contains(why))
            {
                said.Add(why);
            }

            Entry entry;

            if (entries.TryGetValue(test, out entry))
            {
                entry.FailedWhy = said;
            }
        }

        /// <summary>
        /// One in scope row of the test led to its result, a result group or one clash. A group
        /// is counted apart, since the add-in reads the clashes inside it, F114-K36.
        /// </summary>
        public void RowRead(string test, bool isGroup)
        {
            Entry entry = EntryOf(test);
            entry.RowsRead++;

            if (isGroup)
            {
                entry.GroupsRead++;
            }
        }

        /// <summary>The result groups of that test that were read, whose clashes the plan holds at their own statuses.</summary>
        internal int GroupsReadOf(string test)
        {
            Entry entry;
            return entries.TryGetValue(test ?? string.Empty, out entry) ? entry.GroupsRead : 0;
        }

        /// <summary>One row of the test no longer leads to its result, counted only where its status needs reading, C1.</summary>
        public void RowNotFound(string test, ClashStatus status, bool isGroup = false)
        {
            Entry entry = EntryOf(test);

            if (isGroup || NeedsReading(status))
            {
                entry.RowsNotFound++;
            }
        }

        /// <summary>One row of the test has no recorded place, counted only where its status needs reading, C1.</summary>
        public void RowWithNoAddress(string test, ClashStatus status, bool isGroup = false)
        {
            Entry entry = EntryOf(test);

            if (isGroup || NeedsReading(status))
            {
                entry.RowsWithNoAddress++;
            }
        }

        /// <summary>The address a resolve was made for is not there any more, and every report test among its rows is not read, N1.</summary>
        public void NotAtAddress(IEnumerable<string> tests)
        {
            foreach (string test in tests ?? new string[0])
            {
                EntryOf(test).NotAtAddress = true;
            }
        }

        /// <summary>The walk under one resolve threw, and every report test among its rows is not read, N1.</summary>
        public void Threw(IEnumerable<string> tests)
        {
            foreach (string test in tests ?? new string[0])
            {
                EntryOf(test).Threw = true;
            }
        }

        /// <summary>Whether the test was read whole, the only test that gets a view and loses its old ones.</summary>
        public bool IsWhole(string test)
        {
            Entry entry;
            return entries.TryGetValue(test ?? string.Empty, out entry) && entry.Whole;
        }

        /// <summary>The names of the tests read whole, in the order met, the inventory's testsRead.</summary>
        public ReadOnlyCollection<string> WholeNames
        {
            get { return Names(entry => entry.Whole); }
        }

        /// <summary>Each test not read whole with why, in the order met, each one a failed view of the group.</summary>
        public IList<KeyValuePair<string, string>> NotWhole()
        {
            List<KeyValuePair<string, string>> notWhole = new List<KeyValuePair<string, string>>();

            foreach (string test in order)
            {
                Entry entry = entries[test];

                if (!entry.Whole)
                {
                    notWhole.Add(new KeyValuePair<string, string>(test, entry.Why()));
                }
            }

            return notWhole;
        }

        private ReadOnlyCollection<string> Names(Func<Entry, bool> which)
        {
            List<string> names = new List<string>();

            foreach (string test in order)
            {
                if (which(entries[test]))
                {
                    names.Add(test);
                }
            }

            return new ReadOnlyCollection<string>(names);
        }

        private Entry EntryOf(string test)
        {
            string name = test ?? string.Empty;
            Entry entry;

            if (!entries.TryGetValue(name, out entry))
            {
                entry = new Entry();
                clashStepFailed.TryGetValue(name, out entry.FailedWhy);
                entries.Add(name, entry);
                order.Add(name);
            }

            return entry;
        }

        private sealed class Entry
        {
            public int RowsRead;
            public int GroupsRead;
            public int RowsNotFound;
            public int RowsWithNoAddress;
            public bool NotAtAddress;
            public bool Threw;
            public List<string> FailedWhy;

            public bool Whole
            {
                get { return !NotAtAddress && !Threw && FailedWhy == null && RowsNotFound == 0 && RowsWithNoAddress == 0; }
            }

            public string Why()
            {
                List<string> why = new List<string>();

                if (FailedWhy != null)
                {
                    why.Add(string.Join(" and ", FailedWhy.ToArray()) + ", so its open clashes may never have reached the report");
                }

                if (NotAtAddress)
                {
                    why.Add("it is not at its recorded address any more");
                }

                if (Threw)
                {
                    why.Add("its walk threw after " + RowsRead + " row(s) were read");
                }

                if (RowsNotFound > 0)
                {
                    why.Add(RowsNotFound + " row(s) no longer lead to their result");
                }

                if (RowsWithNoAddress > 0)
                {
                    why.Add(RowsWithNoAddress + " row(s) have no recorded place in the document");
                }

                return "not read whole for the views, " + string.Join(", ", why.ToArray())
                    + ", so no view was written for it and its views of earlier runs are kept";
            }
        }
    }
}
