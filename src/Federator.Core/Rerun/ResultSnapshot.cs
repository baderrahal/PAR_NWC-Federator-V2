using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Clash;

namespace Federator.Core.Rerun
{
    /// <summary>One clash result as a coordinator left it, F187: what a refresh must keep.</summary>
    public sealed class ResultRecord
    {
        public ResultRecord(
            string test, string name, string guid, ClashStatus status, string assignedTo, IList<string> comments,
            string approvedBy, string group)
        {
            Test = test ?? string.Empty;
            Name = name ?? string.Empty;
            Guid = guid ?? string.Empty;
            Status = status;
            AssignedTo = assignedTo ?? string.Empty;
            Comments = new ReadOnlyCollection<string>(new List<string>(comments ?? new string[0]));
            ApprovedBy = approvedBy ?? string.Empty;
            Group = group ?? string.Empty;
        }

        public string Test { get; private set; }

        public string Name { get; private set; }

        public string Guid { get; private set; }

        public ClashStatus Status { get; private set; }

        public string AssignedTo { get; private set; }

        public ReadOnlyCollection<string> Comments { get; private set; }

        public string ApprovedBy { get; private set; }

        /// <summary>The result groups it sits in, outermost first, joined by a slash, empty where it sits in none.</summary>
        public string Group { get; private set; }

        internal string NameKey
        {
            get { return Test + "\u001f" + Name; }
        }
    }

    /// <summary>
    /// Every clash result of every test before a refresh run, F187, Bader's order of 14 Oct 2026, task 7.7: status,
    /// assigned to, comments, approver and group. Taken before the tests are run again in place and compared
    /// after, so what a coordinator wrote is shown kept or changed and never silently lost.
    /// </summary>
    public sealed class ResultSnapshot
    {
        private readonly List<ResultRecord> records = new List<ResultRecord>();
        private readonly List<string> tests = new List<string>();

        public ReadOnlyCollection<ResultRecord> Records
        {
            get { return new ReadOnlyCollection<ResultRecord>(records); }
        }

        /// <summary>The tests, in the order the walk met them, a test with no result among them.</summary>
        public ReadOnlyCollection<string> Tests
        {
            get { return new ReadOnlyCollection<string>(tests); }
        }

        public void AddTest(string test)
        {
            if (!tests.Contains(test))
            {
                tests.Add(test);
            }
        }

        public void Add(ResultRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            AddTest(record.Test);
            records.Add(record);
        }

        internal int CountOf(string test, ClashStatus status)
        {
            int count = 0;

            foreach (ResultRecord record in records)
            {
                if (record.Status == status && string.Equals(record.Test, test, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>One test's counts before and after, and what became of its clashes.</summary>
    public sealed class TestRefresh
    {
        internal TestRefresh(string test)
        {
            Test = test;
            Before = new int[5];
            After = new int[5];
            Changes = new List<string>();
        }

        public string Test { get; private set; }

        /// <summary>New, Active, Reviewed, Approved and Resolved before, in that order.</summary>
        public int[] Before { get; private set; }

        /// <summary>The same after.</summary>
        public int[] After { get; private set; }

        /// <summary>Clashes the test held before and found again.</summary>
        public int Kept { get; internal set; }

        /// <summary>Clashes held before as Reviewed or Approved, which a coordinator set.</summary>
        public int CoordinatorStatuses { get; internal set; }

        /// <summary>Of those, the ones found again with the same status.</summary>
        public int CoordinatorStatusesKept { get; internal set; }

        /// <summary>Clashes found again with another status, a New that is Active now among them.</summary>
        public int StatusMoved { get; internal set; }

        /// <summary>Clashes found again with the same status, assigned to, comments, approver and group.</summary>
        public int Untouched { get; internal set; }

        /// <summary>Clashes held before as New or Active that are no longer found or are Resolved after.</summary>
        public int Dropped { get; internal set; }

        /// <summary>Clashes the run found that the test did not hold before.</summary>
        public int Found { get; internal set; }

        /// <summary>Clashes held before that the test no longer holds at all, not even as Resolved.</summary>
        public int Gone { get; internal set; }

        /// <summary>A line for each field of a matched clash that is not as it was.</summary>
        public List<string> Changes { get; private set; }
    }

    /// <summary>
    /// The snapshot before a refresh against the document after it, F187. Results are matched by their Guid and
    /// otherwise by test and name. Whether Navisworks keeps a result's Guid through a run is UNKNOWN until read off
    /// a real refresh, so the second key stands in and the lines say how many were matched by which.
    /// </summary>
    public sealed class RefreshComparison
    {
        private static readonly ClashStatus[] Order =
        {
            ClashStatus.New, ClashStatus.Active, ClashStatus.Reviewed, ClashStatus.Approved, ClashStatus.Resolved
        };

        private readonly List<TestRefresh> perTest = new List<TestRefresh>();

        private RefreshComparison()
        {
        }

        internal ReadOnlyCollection<TestRefresh> PerTest
        {
            get { return new ReadOnlyCollection<TestRefresh>(perTest); }
        }

        /// <summary>Matched by Guid.</summary>
        public int MatchedByGuid { get; private set; }

        /// <summary>Matched by test and name where the Guid was not found.</summary>
        public int MatchedByName { get; private set; }

        public static RefreshComparison Compare(ResultSnapshot before, ResultSnapshot after)
        {
            if (before == null)
            {
                throw new ArgumentNullException("before");
            }

            if (after == null)
            {
                throw new ArgumentNullException("after");
            }

            RefreshComparison result = new RefreshComparison();
            Dictionary<string, ResultRecord> afterByGuid = new Dictionary<string, ResultRecord>(StringComparer.Ordinal);
            Dictionary<string, ResultRecord> afterByName = new Dictionary<string, ResultRecord>(StringComparer.Ordinal);

            foreach (ResultRecord record in after.Records)
            {
                if (record.Guid.Length > 0 && !afterByGuid.ContainsKey(record.Guid))
                {
                    afterByGuid[record.Guid] = record;
                }

                if (!afterByName.ContainsKey(record.NameKey))
                {
                    afterByName[record.NameKey] = record;
                }
            }

            List<string> tests = new List<string>(before.Tests);

            foreach (string test in after.Tests)
            {
                if (!tests.Contains(test))
                {
                    tests.Add(test);
                }
            }

            Dictionary<string, TestRefresh> byTest = new Dictionary<string, TestRefresh>(StringComparer.Ordinal);

            foreach (string test in tests)
            {
                TestRefresh row = new TestRefresh(test);

                for (int i = 0; i < Order.Length; i++)
                {
                    row.Before[i] = before.CountOf(test, Order[i]);
                    row.After[i] = after.CountOf(test, Order[i]);
                }

                byTest[test] = row;
                result.perTest.Add(row);
            }

            HashSet<ResultRecord> matched = new HashSet<ResultRecord>();

            foreach (ResultRecord old in before.Records)
            {
                TestRefresh row = byTest[old.Test];
                ResultRecord now;

                if (old.Guid.Length > 0 && afterByGuid.TryGetValue(old.Guid, out now))
                {
                    result.MatchedByGuid++;
                }
                else if (afterByName.TryGetValue(old.NameKey, out now) && !matched.Contains(now))
                {
                    result.MatchedByName++;
                }
                else
                {
                    row.Gone++;

                    if (old.Status == ClashStatus.New || old.Status == ClashStatus.Active)
                    {
                        row.Dropped++;
                    }

                    continue;
                }

                matched.Add(now);
                row.Kept++;
                bool same = true;

                // A status moved is Navisworks's own reconciliation where a clash found again goes New to Active, so it
                // is counted and never listed as a field that changed. A status a coordinator set, Reviewed or Approved,
                // that is not the same after is named, since that is what a refresh must not take away.
                bool coordinatorStatus = old.Status == ClashStatus.Reviewed || old.Status == ClashStatus.Approved;

                if (coordinatorStatus)
                {
                    row.CoordinatorStatuses++;

                    if (now.Status == old.Status)
                    {
                        row.CoordinatorStatusesKept++;
                    }
                }

                if (now.Status != old.Status)
                {
                    row.StatusMoved++;

                    if (coordinatorStatus)
                    {
                        same = false;
                        row.Changes.Add(old.Name + " status " + old.Status + " to " + now.Status);
                    }

                    if ((old.Status == ClashStatus.New || old.Status == ClashStatus.Active) && now.Status == ClashStatus.Resolved)
                    {
                        row.Dropped++;
                    }
                }

                if (!string.Equals(now.AssignedTo, old.AssignedTo, StringComparison.Ordinal))
                {
                    same = false;
                    row.Changes.Add(old.Name + " assigned to \"" + old.AssignedTo + "\" to \"" + now.AssignedTo + "\"");
                }

                if (!string.Equals(now.ApprovedBy, old.ApprovedBy, StringComparison.Ordinal))
                {
                    same = false;
                    row.Changes.Add(old.Name + " approved by \"" + old.ApprovedBy + "\" to \"" + now.ApprovedBy + "\"");
                }

                if (!string.Equals(now.Group, old.Group, StringComparison.Ordinal))
                {
                    same = false;
                    row.Changes.Add(old.Name + " group \"" + old.Group + "\" to \"" + now.Group + "\"");
                }

                if (!SameComments(old.Comments, now.Comments))
                {
                    same = false;
                    row.Changes.Add(old.Name + " comments " + old.Comments.Count + " to " + now.Comments.Count);
                }

                if (same)
                {
                    row.Untouched++;
                }
            }

            foreach (ResultRecord now in after.Records)
            {
                if (!matched.Contains(now))
                {
                    byTest[now.Test].Found++;
                }
            }

            return result;
        }

        private static bool SameComments(IList<string> a, IList<string> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The tests whose counts or clashes moved, which is what the window's table lists.</summary>
        public IList<TestRefresh> Moved()
        {
            List<TestRefresh> moved = new List<TestRefresh>();

            foreach (TestRefresh row in perTest)
            {
                bool counts = false;

                for (int i = 0; i < row.Before.Length; i++)
                {
                    if (row.Before[i] != row.After[i])
                    {
                        counts = true;
                    }
                }

                if (counts || row.Changes.Count > 0 || row.Gone > 0 || row.Found > 0)
                {
                    moved.Add(row);
                }
            }

            return moved;
        }

        /// <summary>One line a test that moved: New, Active, Reviewed, Approved, Resolved, each before and after.</summary>
        public IList<string> TableLines()
        {
            List<string> lines = new List<string>();

            lines.Add("REFRESH  per test, before to after: New, Active, Reviewed, Approved, Resolved");

            foreach (TestRefresh row in Moved())
            {
                lines.Add("REFRESH  " + row.Test + "  " + Pair(row, 0) + "  " + Pair(row, 1) + "  " + Pair(row, 2) + "  "
                    + Pair(row, 3) + "  " + Pair(row, 4));
            }

            return lines;
        }

        /// <summary>The warning for each test that dropped clashes, and the one line of totals.</summary>
        public IList<string> Warnings()
        {
            List<string> lines = new List<string>();

            foreach (TestRefresh row in perTest)
            {
                if (row.Dropped > 0)
                {
                    lines.Add("WARNING  " + row.Test + ": " + row.Dropped.ToString(CultureInfo.InvariantCulture)
                        + " clashes it held as New or Active are no longer found or are Resolved. A test whose search sets or "
                        + "models changed may drop clashes");
                }
            }

            return lines;
        }

        /// <summary>What was kept, in one line: the clashes found again, those with every field as it was, those gone and those new.</summary>
        public string Summary()
        {
            int kept = 0;
            int untouched = 0;
            int gone = 0;
            int found = 0;
            int changes = 0;
            int coordinator = 0;
            int coordinatorKept = 0;
            int moved = 0;

            foreach (TestRefresh row in perTest)
            {
                kept += row.Kept;
                untouched += row.Untouched;
                gone += row.Gone;
                found += row.Found;
                changes += row.Changes.Count;
                coordinator += row.CoordinatorStatuses;
                coordinatorKept += row.CoordinatorStatusesKept;
                moved += row.StatusMoved;
            }

            return "REFRESH  " + kept.ToString(CultureInfo.InvariantCulture) + " clashes found again. Of "
                + coordinator.ToString(CultureInfo.InvariantCulture) + " Reviewed or Approved before, "
                + coordinatorKept.ToString(CultureInfo.InvariantCulture) + " keep that status. "
                + untouched.ToString(CultureInfo.InvariantCulture) + " keep every assigned to, comment, approver and group and their Reviewed or Approved status, "
                + changes.ToString(CultureInfo.InvariantCulture) + " things a coordinator wrote changed, "
                + moved.ToString(CultureInfo.InvariantCulture) + " moved status, a New found again being Active, "
                + found.ToString(CultureInfo.InvariantCulture) + " new, " + gone.ToString(CultureInfo.InvariantCulture)
                + " no longer held at all. Matched by Guid " + MatchedByGuid.ToString(CultureInfo.InvariantCulture)
                + " and by test and name " + MatchedByName.ToString(CultureInfo.InvariantCulture);
        }

        private static string Pair(TestRefresh row, int at)
        {
            return row.Before[at].ToString(CultureInfo.InvariantCulture) + ">" + row.After[at].ToString(CultureInfo.InvariantCulture);
        }
    }
}
