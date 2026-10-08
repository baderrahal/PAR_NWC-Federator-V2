using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Views
{
    /// <summary>
    /// Which tests walk one read WHOLE for the views, F114 attempt 2 of the add-in pass, the
    /// breaker's B1. A test is read whole when it was found at its recorded address, every
    /// row of it led to its result and nothing threw in its walk. Only such a test gets a
    /// view and loses its old views to the inventory. A test read in part, or not at all, is
    /// a test NOT READ: no view is written for it, its old views are kept, and it is counted
    /// as failed with the reason, so the group is not DONE on it. A row with no recorded
    /// place makes its test not read too, since a view of the rest would be a short view.
    /// </summary>
    public sealed class TestsRead
    {
        private readonly List<string> order = new List<string>();
        private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.Ordinal);

        /// <summary>The test was met, by the name the report holds its rows under.</summary>
        public void Met(string test)
        {
            EntryOf(test);
        }

        /// <summary>One row of the test led to its result.</summary>
        public void RowRead(string test)
        {
            EntryOf(test).RowsRead++;
        }

        /// <summary>One row of the test no longer leads to its result.</summary>
        public void RowNotFound(string test)
        {
            EntryOf(test).RowsNotFound++;
        }

        /// <summary>One row of the test has no recorded place in the document.</summary>
        public void RowWithNoAddress(string test)
        {
            EntryOf(test).RowsWithNoAddress++;
        }

        /// <summary>The test is not at its recorded address any more.</summary>
        public void NotAtAddress(string test)
        {
            EntryOf(test).NotAtAddress = true;
        }

        /// <summary>The walk of the test threw.</summary>
        public void Threw(string test)
        {
            EntryOf(test).Threw = true;
        }

        /// <summary>Whether every row of the test led to its result and nothing stopped its walk.</summary>
        public bool IsWhole(string test)
        {
            Entry entry;
            return entries.TryGetValue(test ?? string.Empty, out entry) && entry.Whole;
        }

        /// <summary>The names of the tests read whole, in the order met, the inventory's testsRead.</summary>
        public ReadOnlyCollection<string> WholeNames
        {
            get
            {
                List<string> whole = new List<string>();

                foreach (string test in order)
                {
                    if (entries[test].Whole)
                    {
                        whole.Add(test);
                    }
                }

                return new ReadOnlyCollection<string>(whole);
            }
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
            public int RowsNotFound;
            public int RowsWithNoAddress;
            public bool NotAtAddress;
            public bool Threw;

            public bool Whole
            {
                get { return !NotAtAddress && !Threw && RowsNotFound == 0 && RowsWithNoAddress == 0; }
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
