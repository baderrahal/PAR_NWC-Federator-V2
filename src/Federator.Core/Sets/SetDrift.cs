using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Sets
{
    /// <summary>
    /// One condition as a SET IN THE DOCUMENT carries it, in the plain strings Core can
    /// compare. The add-in reads it off `SelectionSet.Search` and decides nothing.
    /// </summary>
    public sealed class ReadCondition
    {
        public ReadCondition(
            string categoryInternalName, string propertyInternalName, string test, string value, int flags = 0)
        {
            CategoryInternalName = categoryInternalName ?? string.Empty;
            PropertyInternalName = propertyInternalName ?? string.Empty;
            Test = test ?? string.Empty;
            Value = value ?? string.Empty;
            Flags = flags;
        }

        public string CategoryInternalName { get; private set; }

        public string PropertyInternalName { get; private set; }

        /// <summary>
        /// "equals" or "contains", the two the client's files use, in the words SetBuildPlan
        /// writes, or the comparison's own name for any other, FR-015, so it never reads as one
        /// of the two.
        /// </summary>
        public string Test { get; private set; }

        public string Value { get; private set; }

        /// <summary>
        /// The condition's options as the set in the document carries them, read as a number.
        /// Only the bits `PlannedCondition.QuestionFlagsOf` names are part of what it asks, FR-015.
        /// </summary>
        public int Flags { get; private set; }

        /// <summary>
        /// The condition's value would not read, so what it asks is UNKNOWN, FR-017. Never an
        /// empty value, which is a real question with an answer.
        /// </summary>
        public bool ValueUnread { get; private set; }

        /// <summary>
        /// Why the value would not read, the error's type and message as the add-in caught it, or
        /// empty where none was handed over. Said in the set's SET NOT READ lines, SetDrift.Lines.
        /// </summary>
        public string WhyUnread { get; private set; }

        /// <summary>
        /// A condition whose value would not read, and why, FR-017. It was read as an empty string,
        /// so its set was called drifted, asking for "", and replaced with the box on.
        /// </summary>
        public static ReadCondition Unread(string categoryInternalName, string propertyInternalName, string test, int flags, string why = null)
        {
            ReadCondition unread = new ReadCondition(categoryInternalName, propertyInternalName, test, null, flags);
            unread.ValueUnread = true;
            unread.WhyUnread = why ?? string.Empty;
            return unread;
        }

        /// <summary>
        /// The conditions a planned set asks, in the shape a set read off the document has, so a
        /// set this run CREATED is judged by the same judge as one already there, FR-027.
        /// </summary>
        public static IList<ReadCondition> Of(PlannedSet planned)
        {
            List<ReadCondition> read = new List<ReadCondition>();

            if (planned == null)
            {
                return read;
            }

            foreach (PlannedCondition condition in planned.Conditions)
            {
                read.Add(new ReadCondition(
                    condition.HasCategory ? condition.CategoryInternalName : string.Empty,
                    condition.PropertyInternalName,
                    condition.TestWord,
                    condition.Value,
                    condition.Flags));
            }

            return read;
        }

        /// <summary>
        /// The keys of a set's conditions in order, or NONE where a value of one would not read,
        /// FR-017, so a set read in part pairs with nothing, as a set asking nothing never does.
        /// </summary>
        public static IList<string> KeysOf(IEnumerable<ReadCondition> conditions)
        {
            List<string> keys = new List<string>();

            foreach (ReadCondition condition in conditions)
            {
                if (condition.ValueUnread)
                {
                    return new List<string>();
                }

                keys.Add(condition.Key());
            }

            return keys;
        }

        /// <summary>
        /// What this condition asks, written the way SetBuildPlan.Describe writes one, so the two
        /// read as one sentence: its own test, never equals for a test that is not, and not before
        /// it where it is negated, FR-016.
        /// </summary>
        public string Describe()
        {
            return (CategoryInternalName.Length == 0 ? string.Empty : CategoryInternalName + "/")
                + PropertyInternalName
                + PlannedCondition.TestWordsOf(Test, Flags)
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
            return KeyOf(CategoryInternalName, PropertyInternalName, Test, Flags, Value);
        }

        /// <summary>
        /// THE ONE SHAPE of a condition's key, for a condition read off the document and for one
        /// the picked file plans, `PlannedCondition.Key`, FR-015. It carries the flag bits that
        /// are part of the question and no other, so a negation or the start of an Or group is
        /// a different question, and the Ignore bits are not.
        /// </summary>
        internal static string KeyOf(string category, string property, string test, int flags, string value)
        {
            return (category ?? string.Empty)
                + "|" + (property ?? string.Empty)
                + "|" + (test ?? string.Empty)
                + "|" + PlannedCondition.QuestionFlagsOf(flags).ToString(CultureInfo.InvariantCulture)
                + "|" + (value ?? string.Empty);
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
    /// WHAT IS COMPARED IS THE QUESTION, AND TWO FLAG BITS ARE PART OF IT, FR-015. A
    /// condition also carries the Ignore bits that say whether a display name has to match,
    /// and 5w found 1A02MM's 61 sets carrying none of them where every other group's carry
    /// both, because that file's sets are an original import and the rest are this tool's
    /// own. Rebuilding on those would replace 61 sets in one group over something not shown
    /// to break anything, since those sets do find items, so they are left out. The
    /// negation, 32, and the start of an Or group, 64, are compared, because a set without
    /// them asks another question, `PlannedCondition.QuestionFlagsOf`. The key left every
    /// bit out until FR-015, so a set that had lost its Or or its negation read as the same.
    /// </summary>
    public sealed class SetDrift
    {
        private readonly PlannedSet wanted;

        private readonly string whyNotRead;

        private SetDrift(bool couldNotRead, IList<ReadCondition> asked, PlannedSet wanted, string whyNotRead)
        {
            Path = wanted.Path ?? string.Empty;
            CouldNotRead = couldNotRead;
            Asked = asked ?? new List<ReadCondition>();
            this.wanted = wanted;
            this.whyNotRead = whyNotRead ?? string.Empty;
        }

        public string Path { get; private set; }

        /// <summary>
        /// The set's search would not read, or a value in it would not, FR-017. Never called
        /// drifted, the way a census count that could not be taken is never called a move.
        /// </summary>
        public bool CouldNotRead { get; private set; }

        /// <summary>What the set in the document asks.</summary>
        public IList<ReadCondition> Asked { get; private set; }

        /// <summary>Whether the two differ. False where it could not be read, because not knowing is not a difference.</summary>
        public bool Drifted { get; private set; }

        /// <summary>
        /// Compares what the set carries against what the file asks. The conditions are
        /// compared IN ORDER, because a set asking A and then B is not the same set as
        /// one asking B and then A once a StartGroup bit is involved, and this tool does
        /// not pretend to know which orderings are equivalent. The file's side is keyed off
        /// the planned set here in Core, FR-015, and never a second time in the add-in.
        /// Where the search would not read, asked is null and whyNotRead is the error the
        /// add-in caught, and where a value would not, the error rides on its condition.
        /// </summary>
        public static SetDrift Compare(IList<ReadCondition> asked, PlannedSet planned, string whyNotRead = null)
        {
            if (planned == null)
            {
                throw new ArgumentNullException("planned");
            }

            List<string> wantedKeys = new List<string>();

            foreach (PlannedCondition condition in planned.Conditions)
            {
                wantedKeys.Add(condition.Key());
            }

            SetDrift drift = new SetDrift(
                asked == null || AnyUnread(asked), asked, planned, asked == null ? whyNotRead : FirstWhyUnread(asked));

            if (drift.CouldNotRead)
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

        /// <summary>Why the first of those conditions whose value would not read did not, or null where none failed.</summary>
        private static string FirstWhyUnread(IList<ReadCondition> asked)
        {
            foreach (ReadCondition condition in asked)
            {
                if (condition != null && condition.ValueUnread)
                {
                    return condition.WhyUnread;
                }
            }

            return null;
        }

        /// <summary>Whether a value of one of those conditions would not read, FR-017.</summary>
        private static bool AnyUnread(IList<ReadCondition> asked)
        {
            foreach (ReadCondition condition in asked)
            {
                if (condition != null && condition.ValueUnread)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// What the set asks now, as one sentence, its groups bracketed and joined by or by the
        /// bit it carries, the way a planned set is said, FR-016.
        /// </summary>
        public string AskedNow()
        {
            if (CouldNotRead)
            {
                return AnyUnread(Asked)
                    ? "UNKNOWN, the value of a condition in its search would not read"
                    : "UNKNOWN, its search would not read";
            }

            if (Asked.Count == 0)
            {
                return "nothing at all";
            }

            return PlannedSet.Describe(
                Asked, condition => PlannedCondition.StartsAGroupWith(condition.Flags), condition => condition.Describe());
        }

        /// <summary>What the picked file asks, as one sentence, the planned set's own, FR-016.</summary>
        public string WantedNow()
        {
            return wanted.ConditionCount == 0 ? "nothing at all" : wanted.Describe();
        }

        /// <summary>
        /// The lines the log writes for this set, decided here and nowhere else. For a drifted set
        /// the old question and the new one. For one whose search, or a value in it, would not
        /// read, what would not read and WHY, the error's type and message as the add-in caught
        /// it, the reviewer's finding on attempt 1: the add-in's two catches kept neither, and a
        /// run wrote no line for such a set beyond its present line. None for a set asking what
        /// the file asks.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (CouldNotRead)
            {
                lines.Add("SET NOT READ " + Path);
                lines.Add("   it asks  : " + AskedNow() + ", "
                    + (whyNotRead.Length == 0 ? "and why is UNKNOWN" : "because " + whyNotRead));
                lines.Add("   file asks: " + WantedNow());
                return lines;
            }

            if (!Drifted)
            {
                return lines;
            }

            lines.Add("SET DRIFT " + Path);
            lines.Add("   it asks  : " + AskedNow());
            lines.Add("   file asks: " + WantedNow());
            return lines;
        }
    }
}
