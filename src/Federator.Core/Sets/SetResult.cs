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
}
