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

        /// <summary>
        /// What the set asks, shown on a ZERO line so it explains itself. For a CREATED set it
        /// is what the file asked, and since the drift round a PRESENT set carries what the set
        /// itself asks, read off it, 5w. Settable for that second case, because a present set is
        /// recorded before its question has been read.
        /// </summary>
        public string Asked { get; set; }

        /// <summary>
        /// The conditions a PRESENT set asks, read off it, so a rule that compares the set with
        /// a plan compares keys through SetDrift.Compare and never the prose above, F128: the
        /// prose of a set read off the document carries no display names and a plan's does, so
        /// the two never match. Null where the question was not read, or for a created set.
        /// </summary>
        public IList<ReadCondition> AskedConditions { get; set; }

        public string Path { get; private set; }

        public string Name { get; private set; }

        public int ConditionCount { get; private set; }

        /// <summary>An item count that was not taken, UNKNOWN and never zero.</summary>
        public const int NotCounted = -1;

        /// <summary>How many items it found. NotCounted when it never resolved, or when a present set could not be found again to count, FR-018.</summary>
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
                // FR-018. A present set whose count could not be taken says UNKNOWN, never -1.
                return "present " + Path + "  " + ConditionCount
                    + Word(ConditionCount, " condition", " conditions") + "  "
                    + (ItemCount < 0 ? "UNKNOWN items" : ItemCount + Word(ItemCount, " item", " items"))
                    + "  already there, left alone";
            }

            if (!Created)
            {
                return "FAILED  " + Path + "  " + ConditionCount
                    + Word(ConditionCount, " condition", " conditions") + "  " + Error;
            }

            // A created set whose count could not be taken says UNKNOWN, never -1, as a present one does.
            string line = (IsZero ? "ZERO    " : "ok      ") + Path + "  "
                + ConditionCount + Word(ConditionCount, " condition", " conditions") + "  "
                + (ItemCount < 0 ? "UNKNOWN items" : ItemCount + Word(ItemCount, " item", " items"));

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
}
