using System;
using System.Collections.Generic;

namespace Federator.Core.Sets
{
    /// <summary>
    /// One condition as a SET IN THE DOCUMENT carries it, in the plain strings Core can
    /// compare. The add-in reads it off `SelectionSet.Search` and decides nothing.
    /// </summary>
    public sealed class ReadCondition
    {
        public ReadCondition(string categoryInternalName, string propertyInternalName, string test, string value)
        {
            CategoryInternalName = categoryInternalName ?? string.Empty;
            PropertyInternalName = propertyInternalName ?? string.Empty;
            Test = test ?? string.Empty;
            Value = value ?? string.Empty;
        }

        public string CategoryInternalName { get; private set; }

        public string PropertyInternalName { get; private set; }

        /// <summary>"equals" or "contains", the two the client's files use, in the words SetBuildPlan writes.</summary>
        public string Test { get; private set; }

        public string Value { get; private set; }

        /// <summary>What this condition asks, written the way SetBuildPlan.Describe writes one, so the two read as one sentence.</summary>
        public string Describe()
        {
            return (CategoryInternalName.Length == 0 ? string.Empty : CategoryInternalName + "/")
                + PropertyInternalName
                + (string.Equals(Test, "contains", StringComparison.Ordinal) ? " contains " : " equals ")
                + "\"" + Value + "\"";
        }

        /// <summary>
        /// The key two conditions are the same by. Ordinal on every part, because a
        /// workset name differing only by case is a DIFFERENT question, which is the
        /// whole reason this comparison exists: the matrix now asks ME-Ductwork and
        /// every one of his saved sets still asks ME-DUCTWORK.
        /// </summary>
        public string Key()
        {
            return CategoryInternalName + "|" + PropertyInternalName + "|" + Test + "|" + Value;
        }
    }

    /// <summary>
    /// Whether the set in the DOCUMENT is still asking what the picked FILE asks, Q72
    /// answered a on 2026-09-20.
    ///
    /// WHY THIS EXISTS. F28 leaves a set already at its path exactly as it is, and the
    /// reason is good: a second copy at one path leaves the tree holding both and a clash
    /// locator resolving to whichever came first. But it means a value corrected in the
    /// file since the set was built never reaches the document. The worksets round
    /// corrected four workset values in the matrix and it changed no clash count at all,
    /// because his saved sets still ask the old question. Measured on 2026-09-20, 5w: 170
    /// conditions across his ten groups ask `ME-DUCTWORK`, `ME-PIPING`, `ME-EQUIPMENT` or
    /// `PL-Domestic Water` where the corrected file asks the spelling the models carry.
    ///
    /// WHAT IS COMPARED IS THE QUESTION AND NOT THE FLAGS. A condition also carries the
    /// Ignore bits that say whether a display name has to match, and 5w found 1A02MM's 61
    /// sets carrying none of them where every other group's carry both, because that
    /// file's sets are an original import and the rest are this tool's own. Rebuilding on
    /// a flag difference would replace 61 sets in one group over something not shown to
    /// break anything, since those sets do find items. So the flags are REPORTED and
    /// never acted on, and the question is what decides.
    /// </summary>
    public sealed class SetDrift
    {
        private SetDrift(string path, string name, bool couldNotRead, IList<ReadCondition> asked, IList<string> wanted)
        {
            Path = path ?? string.Empty;
            Name = name ?? string.Empty;
            CouldNotRead = couldNotRead;
            Asked = asked ?? new List<ReadCondition>();
            Wanted = wanted ?? new List<string>();
        }

        public string Path { get; private set; }

        public string Name { get; private set; }

        /// <summary>The set's search would not read. Never called drifted, the way a census count that could not be taken is never called a move.</summary>
        public bool CouldNotRead { get; private set; }

        /// <summary>What the set in the document asks.</summary>
        public IList<ReadCondition> Asked { get; private set; }

        /// <summary>What the picked file asks, already described.</summary>
        public IList<string> Wanted { get; private set; }

        /// <summary>Whether the two differ. False where it could not be read, because not knowing is not a difference.</summary>
        public bool Drifted { get; private set; }

        /// <summary>
        /// Compares what the set carries against what the file asks. The conditions are
        /// compared IN ORDER, because a set asking A and then B is not the same set as
        /// one asking B and then A once a StartGroup bit is involved, and this tool does
        /// not pretend to know which orderings are equivalent.
        /// </summary>
        public static SetDrift Compare(string path, string name, IList<ReadCondition> asked, IList<string> wantedKeys, IList<string> wantedDescribed)
        {
            SetDrift drift = new SetDrift(path, name, asked == null, asked, wantedDescribed);

            if (asked == null || wantedKeys == null)
            {
                return drift;
            }

            if (asked.Count != wantedKeys.Count)
            {
                drift.Drifted = true;
                return drift;
            }

            for (int i = 0; i < asked.Count; i++)
            {
                if (!string.Equals(asked[i].Key(), wantedKeys[i], StringComparison.Ordinal))
                {
                    drift.Drifted = true;
                    return drift;
                }
            }

            return drift;
        }

        /// <summary>What the set asks now, as one sentence.</summary>
        public string AskedNow()
        {
            if (CouldNotRead)
            {
                return "UNKNOWN, its search would not read";
            }

            if (Asked.Count == 0)
            {
                return "nothing at all";
            }

            List<string> parts = new List<string>();

            for (int i = 0; i < Asked.Count; i++)
            {
                parts.Add(Asked[i].Describe());
            }

            return string.Join(" and ", parts.ToArray());
        }

        /// <summary>What the picked file asks, as one sentence.</summary>
        public string WantedNow()
        {
            if (Wanted.Count == 0)
            {
                return "nothing at all";
            }

            string[] array = new string[Wanted.Count];
            Wanted.CopyTo(array, 0);
            return string.Join(" and ", array);
        }

        /// <summary>The two lines the log writes for one drifted set, the old question and the new one.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            lines.Add("SET DRIFT " + Path);
            lines.Add("   it asks  : " + AskedNow());
            lines.Add("   file asks: " + WantedNow());
            return lines;
        }
    }
}
