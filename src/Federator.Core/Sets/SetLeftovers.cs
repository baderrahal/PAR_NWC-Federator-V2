using System;
using System.Collections.Generic;

namespace Federator.Core.Sets
{
    /// <summary>What is to be done about one set the picked file no longer names.</summary>
    public enum LeftoverAction
    {
        /// <summary>Nothing points at it, so it goes. 5z proved a removal costs nothing but the set.</summary>
        Remove = 0,

        /// <summary>
        /// Something points at it AND the file names a twin asking the identical question
        /// that nothing points at. The twin goes and this one takes its name, so the sides
        /// keep working and start asking what the file asks.
        /// </summary>
        RemoveTheTwinThenRename = 1,

        /// <summary>Something points at it and there is no twin, so nothing is done and it is named.</summary>
        Refuse = 2
    }

    /// <summary>One leftover set and what is to be done about it.</summary>
    public sealed class LeftoverSet
    {
        internal LeftoverSet(string path, string name, int sides, LeftoverAction action, string twinPath, string twinName)
            : this(path, name, sides, action, twinPath, twinName, false)
        {
        }

        internal LeftoverSet(
            string path, string name, int sides, LeftoverAction action, string twinPath, string twinName, bool sidesUnknown)
        {
            Path = path ?? string.Empty;
            Name = name ?? string.Empty;
            Sides = sides;
            Action = action;
            TwinPath = twinPath ?? string.Empty;
            TwinName = twinName ?? string.Empty;
            SidesUnknown = sidesUnknown;
        }

        /// <summary>
        /// The clash test sides of its document could not all be counted, so it is refused
        /// whatever its own count reads, FR-013.
        /// </summary>
        public bool SidesUnknown { get; private set; }

        public string Path { get; private set; }

        public string Name { get; private set; }

        /// <summary>How many clash test sides resolve to it. The number that decides everything.</summary>
        public int Sides { get; private set; }

        public LeftoverAction Action { get; private set; }

        /// <summary>The unused set that gets removed first, or empty.</summary>
        public string TwinPath { get; private set; }

        public string TwinName { get; private set; }

        /// <summary>What the log says about it, naming what points at it every time.</summary>
        public string Line()
        {
            if (SidesUnknown)
            {
                return Path + "   the picked file does not name it and the clash test sides of this NWF could not all"
                    + " be counted. NOTHING IS DONE, because a set that looks unused may be pointed at by a side"
                    + " that was not counted, and removing it would leave that side resolving to nothing";
            }

            switch (Action)
            {
                case LeftoverAction.Remove:
                    return Path + "   the picked file does not name it and NOTHING points at it, so it is removed";

                case LeftoverAction.RemoveTheTwinThenRename:
                    return Path + "   the picked file does not name it and " + Sides
                        + " clash test side(s) point at it. It asks exactly what \"" + TwinName
                        + "\" asks, which the file DOES name and which nothing points at, so that one is"
                        + " removed and this one is renamed into it. The sides keep working and start"
                        + " asking what the file asks";

                default:
                    return Path + "   the picked file does not name it and " + Sides
                        + " clash test side(s) point at it, with no unused twin to rename it into."
                        + " NOTHING IS DONE, because removing it would leave those sides resolving to"
                        + " nothing and they could never find anything again";
            }
        }
    }

    /// <summary>One set as the document holds it, with what points at it.</summary>
    public sealed class DocumentSet
    {
        public DocumentSet(string path, string name, IList<string> conditionKeys, int sides)
        {
            Path = path ?? string.Empty;
            Name = name ?? string.Empty;
            ConditionKeys = conditionKeys ?? new List<string>();
            Sides = sides;
        }

        /// <summary>
        /// A side count that could not be taken, FR-013. It is UNKNOWN and never zero, because
        /// zero sides is what removes a set.
        /// </summary>
        public const int SidesNotCounted = -1;

        public string Path { get; private set; }

        public string Name { get; private set; }

        /// <summary>What it asks, in the same keys `SetDrift` compares on, IN ORDER.</summary>
        public IList<string> ConditionKeys { get; private set; }

        /// <summary>How many clash test sides resolve to it, or below zero where they could not be counted.</summary>
        public int Sides { get; private set; }

        /// <summary>Whether its sides were counted.</summary>
        public bool SidesCounted
        {
            get { return Sides >= 0; }
        }
    }

    /// <summary>
    /// WHAT TO DO ABOUT A SET IN THE NWF THAT THE PICKED FILE NO LONGER NAMES, Q74.
    ///
    /// THE CASE THIS IS FOR, AND THE NUMBERS THAT INVERTED IT. Seven of his C02 groups
    /// hold 62 sets where the matrix holds 61, because `BLD-DRPipe Accessories`, the
    /// spelling with the missing hyphen, and `BLD-DR-Pipe Accessories`, the corrected one,
    /// are BOTH in there. Q74 was answered "remove what the file no longer names" before
    /// anybody had counted which was which. 5z counted it: **60 clash test sides point at
    /// the BROKEN name and NOTHING points at the corrected one**. The broken set is the
    /// one doing all the work and finding nothing. The corrected set, which this tool
    /// created from the corrected file, sits unused.
    ///
    /// So removing what the file no longer names would remove the working one and orphan
    /// 420 sides across seven groups, and removing the unused one costs nothing at all.
    ///
    /// THE PAIRING IS MEASURED AND NOT GUESSED. The two sets ask the IDENTICAL question,
    /// same category, same property, same values, same flags, read off his own files on
    /// 2026-09-20. The flags compared are the two that are part of the question, the
    /// negation and the start of an Or group, `PlannedCondition.QuestionFlagsOf`, which the
    /// keys carry since FR-015 and did not before. That is what makes them a pair rather than two sets with similar names,
    /// and it is also the cleanest evidence there is that F28 kept the correction out of
    /// the document: the corrected set was built from the corrected file and never used.
    ///
    /// ORDER IS LOAD BEARING. The corrected name already exists, so renaming onto it would
    /// leave two sets at one path, which is F28's exact prohibition and a locator resolving
    /// to whichever came first. The twin goes FIRST, then the rename takes the freed name.
    ///
    /// AND THE REFUSAL IS WIDER THAN THE BRIEF ASKED FOR. It said refuse when a test would
    /// LOSE ITS RESULTS. 5z measured that the results survive a removal and the SIDE stops
    /// resolving, which that wording would never have caught. The rule here is: refuse
    /// when anything pointing at the set would stop resolving, whatever happens to the
    /// results.
    /// </summary>
    public static class SetLeftovers
    {
        /// <summary>
        /// Every set in the document the picked file does not name, and what to do about
        /// each. Names are compared Ordinal and never trimmed, because two set names in
        /// the reference file end in a space.
        /// </summary>
        public static IList<LeftoverSet> For(IList<DocumentSet> inDocument, IList<string> namedByTheFile)
        {
            List<LeftoverSet> leftovers = new List<LeftoverSet>();

            if (inDocument == null || inDocument.Count == 0)
            {
                return leftovers;
            }

            HashSet<string> named = new HashSet<string>(StringComparer.Ordinal);

            if (namedByTheFile != null)
            {
                foreach (string name in namedByTheFile)
                {
                    named.Add(name ?? string.Empty);
                }
            }

            // FR-013. ONE SET WHOSE SIDES COULD NOT BE COUNTED REFUSES EVERY LEFTOVER, because
            // the twin a leftover would be renamed into, or the leftover itself, may be pointed
            // at by a side nobody counted, and an uncounted side made a set look unused.
            bool everyCounted = true;

            foreach (DocumentSet set in inDocument)
            {
                if (set != null && !set.SidesCounted)
                {
                    everyCounted = false;
                }
            }

            // ONE UNUSED TWIN SERVES ONE LEFTOVER, the breaker's finding on F115's third pass.
            // Two leftovers asking the twin's question were both renamed into it: the first
            // removed the twin and took its name, and the second then removed the first by that
            // name, a working set with sides, while both lines said RENAMED.
            List<DocumentSet> taken = new List<DocumentSet>();

            foreach (DocumentSet set in inDocument)
            {
                if (set == null || named.Contains(set.Name))
                {
                    continue;
                }

                if (!everyCounted)
                {
                    leftovers.Add(new LeftoverSet(set.Path, set.Name, set.Sides, LeftoverAction.Refuse, null, null, true));
                    continue;
                }

                if (set.Sides <= 0)
                {
                    leftovers.Add(new LeftoverSet(set.Path, set.Name, 0, LeftoverAction.Remove, null, null));
                    continue;
                }

                DocumentSet twin = TwinFor(set, inDocument, named, taken);

                if (twin != null)
                {
                    taken.Add(twin);
                }

                leftovers.Add(twin == null
                    ? new LeftoverSet(set.Path, set.Name, set.Sides, LeftoverAction.Refuse, null, null)
                    : new LeftoverSet(
                        set.Path, set.Name, set.Sides,
                        LeftoverAction.RemoveTheTwinThenRename, twin.Path, twin.Name));
            }

            return leftovers;
        }

        /// <summary>
        /// A set the FILE names, asking the IDENTICAL question, that NOTHING points at, and
        /// that no earlier leftover has taken. All four are required. A twin something points
        /// at would orphan those sides instead, a twin asking a different question is a
        /// different set that happens to sit nearby, and a twin already taken is the renamed
        /// working set of the leftover before.
        /// </summary>
        private static DocumentSet TwinFor(
            DocumentSet leftover, IList<DocumentSet> inDocument, HashSet<string> named, IList<DocumentSet> taken)
        {
            foreach (DocumentSet other in inDocument)
            {
                if (other == null
                    || ReferenceEquals(other, leftover)
                    || other.Sides > 0
                    || !named.Contains(other.Name)
                    || Holds(taken, other)
                    || !SameQuestion(leftover.ConditionKeys, other.ConditionKeys))
                {
                    continue;
                }

                return other;
            }

            return null;
        }

        /// <summary>Whether that set is among those taken, by reference, because two sets may share a name.</summary>
        private static bool Holds(IList<DocumentSet> taken, DocumentSet set)
        {
            foreach (DocumentSet one in taken)
            {
                if (ReferenceEquals(one, set))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The same conditions in the same order. IN ORDER, because a set asking A then B
        /// is not the same set as one asking B then A once a StartGroup bit is involved,
        /// which is the rule `SetDrift` already keeps.
        /// </summary>
        private static bool SameQuestion(IList<string> left, IList<string> right)
        {
            if (left == null || right == null || left.Count != right.Count || left.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                if (!string.Equals(left[i], right[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The block, or empty where the picked file names everything the document holds.</summary>
        public static IList<string> Lines(IList<LeftoverSet> leftovers)
        {
            List<string> lines = new List<string>();

            if (leftovers == null || leftovers.Count == 0)
            {
                return lines;
            }

            int removed = 0;
            int renamed = 0;
            int refused = 0;
            int uncounted = 0;

            foreach (LeftoverSet one in leftovers)
            {
                if (one.SidesUnknown)
                {
                    uncounted++;
                }
                else if (one.Action == LeftoverAction.Remove)
                {
                    removed++;
                }
                else if (one.Action == LeftoverAction.RemoveTheTwinThenRename)
                {
                    renamed++;
                }
                else
                {
                    refused++;
                }
            }

            lines.Add(leftovers.Count + " set(s) in this NWF are not named by the picked file:");
            lines.Add("   " + removed + " nothing points at, so they are removed");
            lines.Add("   " + renamed + " are the working half of a pair, so the unused half goes and this one takes its name");
            lines.Add("   " + refused + " are pointed at with no twin, so nothing is done about them");

            if (uncounted > 0)
            {
                lines.Add("   " + uncounted + " are left alone because the clash test sides of this NWF could not all be counted");
            }

            foreach (LeftoverSet one in leftovers)
            {
                lines.Add("   " + one.Line());
            }

            return lines;
        }
    }
}
