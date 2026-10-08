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
    /// Only a whole test gets a view and loses its old views. A test read in part or not at all
    /// is NOT READ: no view, its old views kept, counted failed with why, so the group is not
    /// DONE on it. A test whose in scope rows are ALL result groups, F3, the lead's decision,
    /// carries no items to show, so it gets no view and keeps its old views and is named on its
    /// own, never counted failed, since a person's grouping must not cost the group DONE.
    /// </summary>
    public sealed class TestsRead
    {
        private readonly HashSet<ClashStatus> inScope;
        private readonly List<string> order = new List<string>();
        private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.Ordinal);

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

        /// <summary>One in scope row of the test led to its result, a result group or one clash.</summary>
        public void RowRead(string test, bool isGroup)
        {
            Entry entry = EntryOf(test);
            entry.RowsRead++;

            if (isGroup)
            {
                entry.GroupsRead++;
            }
        }

        /// <summary>One row of the test no longer leads to its result, counted only where its status needs reading, C1.</summary>
        public void RowNotFound(string test, ClashStatus status)
        {
            Entry entry = EntryOf(test);

            if (NeedsReading(status))
            {
                entry.RowsNotFound++;
            }
        }

        /// <summary>One row of the test has no recorded place, counted only where its status needs reading, C1.</summary>
        public void RowWithNoAddress(string test, ClashStatus status)
        {
            Entry entry = EntryOf(test);

            if (NeedsReading(status))
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

        /// <summary>Whether the test was read whole and has a row a view can show, the only test that gets a view.</summary>
        public bool IsWhole(string test)
        {
            Entry entry;
            return entries.TryGetValue(test ?? string.Empty, out entry) && entry.Whole && !entry.OnlyGroups;
        }

        /// <summary>The names of the tests read whole, in the order met, the inventory's testsRead.</summary>
        public ReadOnlyCollection<string> WholeNames
        {
            get { return Names(entry => entry.Whole && !entry.OnlyGroups); }
        }

        /// <summary>The tests read whole whose in scope rows are all result groups, F3, named on their own and never failed.</summary>
        public ReadOnlyCollection<string> OnlyGroups
        {
            get { return Names(entry => entry.Whole && entry.OnlyGroups); }
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

            public bool Whole
            {
                get { return !NotAtAddress && !Threw && RowsNotFound == 0 && RowsWithNoAddress == 0; }
            }

            public bool OnlyGroups
            {
                get { return RowsRead > 0 && GroupsRead == RowsRead; }
            }

            public string Why()
            {
                List<string> why = new List<string>();

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
