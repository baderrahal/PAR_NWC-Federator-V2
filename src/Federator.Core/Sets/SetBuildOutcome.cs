using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Sets
{
    /// <summary>What happened to one set once it reached the model.</summary>
    public sealed class SetResult
    {
        internal SetResult(
            string path, string name, int conditionCount, int itemCount, string error, string asked)
        {
            Path = path;
            Name = name;
            ConditionCount = conditionCount;
            ItemCount = itemCount;
            Error = error;
            Asked = asked;
        }

        /// <summary>What the set asked the model for. Shown on a ZERO line so it explains itself.</summary>
        public string Asked { get; private set; }

        public string Path { get; private set; }

        public string Name { get; private set; }

        public int ConditionCount { get; private set; }

        /// <summary>How many items it found. Minus one when it never resolved.</summary>
        public int ItemCount { get; private set; }

        /// <summary>Null when the set was created and resolved.</summary>
        public string Error { get; private set; }

        public bool Created
        {
            get { return Error == null; }
        }

        /// <summary>Created and resolved, but found nothing. Reported, never hidden.</summary>
        public bool IsZero
        {
            get { return Created && ItemCount == 0; }
        }

        /// <summary>The one line this set contributes to the log.</summary>
        public string Line()
        {
            if (!Created)
            {
                return "FAILED  " + Path + "  " + ConditionCount
                    + Word(ConditionCount, " condition", " conditions") + "  " + Error;
            }

            string line = (IsZero ? "ZERO    " : "ok      ") + Path + "  "
                + ConditionCount + Word(ConditionCount, " condition", " conditions") + "  "
                + ItemCount + Word(ItemCount, " item", " items");

            // A zero is not an error, but it is useless without knowing what was asked.
            return IsZero && !string.IsNullOrEmpty(Asked) ? line + "  asked for " + Asked : line;
        }

        private static string Word(int count, string one, string many)
        {
            return count == 1 ? one : many;
        }

        public override string ToString()
        {
            return Line();
        }
    }

    /// <summary>
    /// The running total for a sets build. The counts always agree with the lines,
    /// because both come from the same list.
    /// </summary>
    public sealed class SetBuildOutcome
    {
        private readonly List<SetResult> results = new List<SetResult>();
        private readonly List<SkippedSet> skipped = new List<SkippedSet>();

        public ReadOnlyCollection<SetResult> Results
        {
            get { return new ReadOnlyCollection<SetResult>(results); }
        }

        public ReadOnlyCollection<SkippedSet> Skipped
        {
            get { return new ReadOnlyCollection<SkippedSet>(skipped); }
        }

        public void AddSkipped(SkippedSet set)
        {
            if (set == null)
            {
                throw new ArgumentNullException("set");
            }

            skipped.Add(set);
        }

        public SetResult AddCreated(string path, string name, int conditionCount, int itemCount)
        {
            return AddCreated(path, name, conditionCount, itemCount, null);
        }

        public SetResult AddCreated(
            string path, string name, int conditionCount, int itemCount, string asked)
        {
            SetResult result = new SetResult(path, name, conditionCount, itemCount, null, asked);
            results.Add(result);
            return result;
        }

        public SetResult AddFailed(string path, string name, int conditionCount, string error)
        {
            SetResult result = new SetResult(
                path, name, conditionCount, -1, string.IsNullOrEmpty(error) ? "UNKNOWN" : error, null);
            results.Add(result);
            return result;
        }

        /// <summary>Sets that reached the model and resolved.</summary>
        public int CreatedCount
        {
            get { return Count(true, false); }
        }

        /// <summary>Created sets that found at least one item.</summary>
        public int FindingItemsCount
        {
            get { return Count(true, true); }
        }

        /// <summary>Created sets that found nothing.</summary>
        public int ZeroCount
        {
            get
            {
                int zero = 0;

                foreach (SetResult result in results)
                {
                    if (result.IsZero)
                    {
                        zero++;
                    }
                }

                return zero;
            }
        }

        public int FailedCount
        {
            get
            {
                int failed = 0;

                foreach (SetResult result in results)
                {
                    if (!result.Created)
                    {
                        failed++;
                    }
                }

                return failed;
            }
        }

        public int SkippedCount
        {
            get { return skipped.Count; }
        }

        public int TotalItems
        {
            get
            {
                int total = 0;

                foreach (SetResult result in results)
                {
                    if (result.Created)
                    {
                        total += result.ItemCount;
                    }
                }

                return total;
            }
        }

        private int Count(bool created, bool withItems)
        {
            int count = 0;

            foreach (SetResult result in results)
            {
                if (result.Created != created)
                {
                    continue;
                }

                if (withItems && result.ItemCount <= 0)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        /// <summary>
        /// One line per set, then the totals. The totals are counted off the same list
        /// the lines came from, so they cannot disagree with them.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            foreach (SetResult result in results)
            {
                lines.Add(result.Line());
            }

            foreach (SkippedSet set in skipped)
            {
                lines.Add("SKIPPED " + set.Path + "  " + set.Reason);
            }

            lines.Add(string.Empty);
            lines.Add("sets created      : " + CreatedCount);
            lines.Add("sets finding items: " + FindingItemsCount);
            lines.Add("sets at zero      : " + ZeroCount);

            if (FailedCount > 0)
            {
                lines.Add("sets that failed  : " + FailedCount);
            }

            if (SkippedCount > 0)
            {
                lines.Add("sets skipped      : " + SkippedCount);
            }

            lines.Add("items found       : " + TotalItems);
            return lines;
        }
    }
}
