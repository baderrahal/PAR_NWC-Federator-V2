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
            : this(path, name, conditionCount, itemCount, error, asked, false)
        {
        }

        internal SetResult(
            string path, string name, int conditionCount, int itemCount, string error, string asked, bool present)
        {
            Path = path;
            Name = name;
            ConditionCount = conditionCount;
            ItemCount = itemCount;
            Error = error;
            Asked = asked;
            Present = present;
        }

        /// <summary>What the set asked the model for. Shown on a ZERO line so it explains itself.</summary>
        public string Asked { get; private set; }

        public string Path { get; private set; }

        public string Name { get; private set; }

        public int ConditionCount { get; private set; }

        /// <summary>How many items it found. Minus one when it never resolved.</summary>
        public int ItemCount { get; private set; }

        /// <summary>Null when the set was created and resolved, or was already there.</summary>
        public string Error { get; private set; }

        /// <summary>
        /// Already at its path in the open document before this build, so it was left
        /// alone and put nothing in. F28. It used to be counted as created as well, so
        /// every weekly run reported sixty one sets created and saved the NWF a second
        /// time for nothing.
        /// </summary>
        public bool Present { get; private set; }

        /// <summary>Created by this build and resolved. A present set is not created.</summary>
        public bool Created
        {
            get { return Error == null && !Present; }
        }

        /// <summary>Created and resolved, but found nothing. Reported, never hidden.</summary>
        public bool IsZero
        {
            get { return Created && ItemCount == 0; }
        }

        /// <summary>The one line this set contributes to the log.</summary>
        public string Line()
        {
            if (Present)
            {
                return "present " + Path + "  " + ConditionCount
                    + Word(ConditionCount, " condition", " conditions") + "  "
                    + ItemCount + Word(ItemCount, " item", " items") + "  already there, left alone";
            }

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

        /// <summary>
        /// The document the sets were resolved against. Counts cannot be read without it,
        /// because a set at zero against an architecture model means something different
        /// from a set at zero against a federated one.
        /// </summary>
        public string OpenDocument { get; set; }

        /// <summary>
        /// A set already at this path in the open document. On a reused NWF every set from
        /// last week is already there, and adding another copy would leave the tree holding
        /// both, with a locator resolving to whichever came first. It is left alone. One
        /// call, carrying the path, the name, the condition count and how many items it
        /// finds in this document, so it has a line of its own and is never counted as
        /// created. F28.
        /// </summary>
        public SetResult AddAlreadyPresent(string path, string name, int conditionCount, int itemCount)
        {
            SetResult result = new SetResult(path, name, conditionCount, itemCount, null, null, true);
            results.Add(result);
            return result;
        }

        /// <summary>Sets already there and left alone. Never in CreatedCount.</summary>
        public int AlreadyPresentCount
        {
            get
            {
                int present = 0;

                foreach (SetResult result in results)
                {
                    if (result.Present)
                    {
                        present++;
                    }
                }

                return present;
            }
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

        /// <summary>
        /// True when this build put at least one set into the document. That is what
        /// decides whether the NWF is saved again after the sets. A set already there
        /// was left alone and put nothing in, so it does not count either way: a rerun
        /// that finds sixty present and creates one still put one in.
        /// </summary>
        public bool PutAnythingIn
        {
            get { return CreatedCount > 0; }
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
                    if (!result.Created && !result.Present)
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
            lines.Add("ran against       : "
                + (string.IsNullOrEmpty(OpenDocument) ? "UNKNOWN" : OpenDocument));
            lines.Add("sets created      : " + CreatedCount);

            if (AlreadyPresentCount > 0)
            {
                lines.Add("already there     : " + AlreadyPresentCount + ", left alone, not copied again");
            }
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
