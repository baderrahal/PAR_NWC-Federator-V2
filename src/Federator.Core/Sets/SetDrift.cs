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
        private SetDrift(string path, bool couldNotRead, IList<ReadCondition> asked, IList<string> wanted)
        {
            Path = path ?? string.Empty;
            CouldNotRead = couldNotRead;
            Asked = asked ?? new List<ReadCondition>();
            Wanted = wanted ?? new List<string>();
        }

        public string Path { get; private set; }

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
        public static SetDrift Compare(string path, IList<ReadCondition> asked, IList<string> wantedKeys, IList<string> wantedDescribed)
        {
            SetDrift drift = new SetDrift(path, asked == null, asked, wantedDescribed);

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

        /// <summary>
        /// WHY the two differ, in the words a person would use, so a drift line says what
        /// moved rather than only that something did.
        ///
        /// THE COMMON CASE IS A VALUE DIFFERING ONLY IN CASE and it is invisible at a
        /// glance. All twenty eight drifts of the C02 run of 2026-09-21 were that: the
        /// document asks "ME-DUCTWORK" and the file asks "ME-Ductwork", two sentences a
        /// reader has to compare character by character to tell apart. The values are
        /// compared in the order they are asked, because a set carries the same property
        /// more than once and position is what pairs them.
        ///
        /// It reads the VALUES and not the whole sentence, because the file's half names
        /// each property in words beside its internal name and the document's half cannot,
        /// so the two sentences never match even where the question does.
        /// </summary>
        public string Why()
        {
            if (CouldNotRead)
            {
                return "its search would not read, so what it asks cannot be compared with the file";
            }

            IList<string> asked = ValuesIn(AskedNow());
            IList<string> wanted = ValuesIn(WantedNow());

            if (asked.Count != wanted.Count)
            {
                return "the set asks " + asked.Count + " value(s) and the file asks " + wanted.Count;
            }

            for (int i = 0; i < asked.Count; i++)
            {
                if (string.Equals(asked[i], wanted[i], StringComparison.Ordinal))
                {
                    continue;
                }

                if (string.Equals(asked[i], wanted[i], StringComparison.OrdinalIgnoreCase))
                {
                    return "they differ only in the CASE of a value, the set asks \"" + asked[i]
                        + "\" and the file asks \"" + wanted[i] + "\"";
                }

                string invisible = Health.InvisibleDifference.Between(asked[i], wanted[i]);

                if (invisible != null)
                {
                    return "they differ by a character that does not show, " + invisible;
                }

                return "the set asks \"" + asked[i] + "\" where the file asks \"" + wanted[i] + "\"";
            }

            return "every value matches, so what differs is which property or which test is asked";
        }

        /// <summary>
        /// The quoted values of one describing sentence, in order. Both halves are written by
        /// Describe, which always quotes the value and nothing else, so the quotes are a
        /// reliable boundary rather than a guess about the wording.
        /// </summary>
        private static IList<string> ValuesIn(string sentence)
        {
            List<string> values = new List<string>();

            if (string.IsNullOrEmpty(sentence))
            {
                return values;
            }

            int at = 0;

            while (true)
            {
                int open = sentence.IndexOf('"', at);

                if (open < 0)
                {
                    return values;
                }

                int close = sentence.IndexOf('"', open + 1);

                if (close < 0)
                {
                    return values;
                }

                values.Add(sentence.Substring(open + 1, close - open - 1));
                at = close + 1;
            }
        }

        /// <summary>The three lines the log writes for one drifted set, the old question, the new one, and why they differ.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            lines.Add("SET DRIFT " + Path);
            lines.Add("   it asks  : " + AskedNow());
            lines.Add("   file asks: " + WantedNow());
            lines.Add("   why      : " + Why());
            return lines;
        }
    }
}
