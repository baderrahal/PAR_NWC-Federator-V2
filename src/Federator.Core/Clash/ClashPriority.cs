using System;
using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// A clash test's priority off the client's clash matrix, F83. A, B or C, and None
    /// where the matrix says nothing about that test.
    ///
    /// PRIORITY IS NOT STATUS AND THE TWO ARE NEVER USED FOR EACH OTHER. Priority is A, B
    /// or C and comes off the matrix. Status is the Navisworks word, New, Active,
    /// Reviewed, Approved or Resolved, and lives on a clash. Nothing in this file names a
    /// status and nothing in ClashStatus names a priority.
    /// </summary>
    public enum ClashPriority
    {
        /// <summary>The matrix says nothing about this test. Never written as a blank A.</summary>
        None = 0,

        A = 1,
        B = 2,
        C = 3
    }

    /// <summary>The words and the order for a priority, so nothing else spells them.</summary>
    public static class Priorities
    {
        /// <summary>The three, in the order they sort.</summary>
        public static readonly ClashPriority[] All =
            { ClashPriority.A, ClashPriority.B, ClashPriority.C };

        /// <summary>
        /// The four in the order they are counted and sorted, None last, because a test
        /// the matrix says nothing about is not more urgent than one it calls C.
        /// </summary>
        public static readonly ClashPriority[] AllWithNone =
            { ClashPriority.A, ClashPriority.B, ClashPriority.C, ClashPriority.None };

        /// <summary>The cell and the folder name. None is a sentence and never a blank.</summary>
        public static string Words(ClashPriority priority)
        {
            switch (priority)
            {
                case ClashPriority.A: return "A";
                case ClashPriority.B: return "B";
                case ClashPriority.C: return "C";
                case ClashPriority.None: return "No priority";
                default: return "UNKNOWN";
            }
        }

        /// <summary>
        /// The priority that word names, or None. Read without case and trimmed, because
        /// this comes off a CSV somebody typed, and " a " is the same priority as "A".
        /// Anything that is not one of the three is None, which is the honest answer: the
        /// matrix said something this tool does not understand, so it said nothing.
        /// </summary>
        public static ClashPriority From(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return ClashPriority.None;
            }

            string tidied = word.Trim();

            foreach (ClashPriority priority in All)
            {
                if (string.Equals(Words(priority), tidied, StringComparison.OrdinalIgnoreCase))
                {
                    return priority;
                }
            }

            return ClashPriority.None;
        }

        /// <summary>
        /// What the workbook cell holds. EMPTY for None, because a test the priority file
        /// says nothing about has no priority, and writing "No priority" into a column
        /// beside 1829 letters would read like a fourth letter. How many were left empty
        /// is said once in the PRIORITY line, which is where a count belongs.
        ///
        /// The folder name is Words and not this, because a viewpoint folder has to be
        /// called something and an unnamed folder is not a folder.
        /// </summary>
        public static string Cell(ClashPriority priority)
        {
            return priority == ClashPriority.None ? string.Empty : Words(priority);
        }

        /// <summary>Where that priority sorts. A first, None last.</summary>
        public static int Order(ClashPriority priority)
        {
            for (int i = 0; i < AllWithNone.Length; i++)
            {
                if (AllWithNone[i] == priority)
                {
                    return i;
                }
            }

            return AllWithNone.Length;
        }
    }
}
