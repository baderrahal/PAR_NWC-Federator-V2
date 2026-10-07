using System;
using System.Collections.Generic;

namespace Federator.Core.Sets
{
    /// <summary>Which of three things is wrong with a set that found nothing.</summary>
    public enum EmptyReason
    {
        /// <summary>It asks for a value no model in this project carries. The condition is wrong.</summary>
        NoModelCarriesTheValue = 0,

        /// <summary>It asks for a value the models DO carry and still found nothing. Something else is wrong.</summary>
        TheValueIsThereAnyway = 1,

        /// <summary>This reader cannot read what it asks, so it says so rather than guessing.</summary>
        CannotTell = 2
    }

    /// <summary>Why one set found nothing, with the value it asked for and the nearest one the models carry.</summary>
    public sealed class EmptySet
    {
        internal EmptySet(string path, EmptyReason reason, string asked, string nearest)
            : this(path, reason, asked, nearest, null)
        {
        }

        internal EmptySet(string path, EmptyReason reason, string asked, string nearest, string whyNotTold)
        {
            Path = path ?? string.Empty;
            Reason = reason;
            Asked = asked ?? string.Empty;
            Nearest = nearest ?? string.Empty;
            WhyNotTold = whyNotTold ?? string.Empty;
        }

        /// <summary>Why this reader cannot tell, or empty where it does not know why, FR-011.</summary>
        public string WhyNotTold { get; private set; }

        public string Path { get; private set; }

        public EmptyReason Reason { get; private set; }

        /// <summary>The value it asked for that nothing carries, or empty.</summary>
        public string Asked { get; private set; }

        /// <summary>
        /// The nearest value the models DO carry, or empty. A SUGGESTION AND NEVER A
        /// CORRECTION: the worksets round proved twice over that a name one letter away
        /// can be a completely different thing, `AR-EXTERIOR` against `AR-INTERIOR` and
        /// `ST-SUB` against `ST-SUP`. Nothing acts on this and a person reads it.
        /// </summary>
        public string Nearest { get; private set; }

        public string Line()
        {
            switch (Reason)
            {
                case EmptyReason.NoModelCarriesTheValue:
                    return Path + "   asks for \"" + Asked + "\" and NO MODEL MEASURED SO FAR IN THIS PROJECT CARRIES IT"
                        + (Nearest.Length == 0
                            ? ", and nothing the models carry is close to it"
                            : string.Equals(Nearest, Asked, StringComparison.OrdinalIgnoreCase)
                                // FR-027. Letter case alone is the whole of what is wrong, Q68.
                                ? ". The nearest the models carry is \"" + Nearest + "\", which differs from it by letter case"
                                    + " alone, and the match is case sensitive. It is a suggestion and not a correction"
                                : ". The nearest the models carry is \"" + Nearest + "\", which is a suggestion and not a correction");

                case EmptyReason.TheValueIsThereAnyway:
                    // WHAT THIS READER ACTUALLY KNOWS. The measured lists are the whole
                    // PROJECT, 374 categories and 39 worksets across all ten groups, so
                    // "the models carry it" means some model somewhere does, NOT that a
                    // model of this group does. A group holding two disciplines out of
                    // seven lands most of the client's 61 sets here, and on 1000BS that
                    // was 33 of 54. Saying "something else is wrong" about those would be
                    // this reader claiming to know a thing it cannot see.
                    return Path + "   asks for \"" + Asked + "\", WHICH MODELS IN THIS PROJECT DO CARRY,"
                        + " and still found nothing HERE. Either this group holds no model of that kind,"
                        + " which is ordinary, or something else is wrong, and this reader cannot tell which";

                default:
                    return Path + "   found nothing and THIS READER CANNOT TELL WHY"
                        + (WhyNotTold.Length == 0 ? string.Empty : ", because " + WhyNotTold);
            }
        }
    }

    /// <summary>
    /// Why a set found nothing, which is the thing his report makes urgent. On his own
    /// 1A02MM workbook 1,677 of 1,830 clash tests touch one of the 43 sets that never
    /// produce a clash, and NOT ONE of those 1,677 found anything, and nothing in this
    /// tool told him which of those sets is wrong and which is a model with no such
    /// content. Those are two completely different problems and only one of them is his.
    ///
    /// THE VALUES ARE MEASURED AND NEVER TYPED. `RevitCategories` holds the 374 category
    /// values the models really carry, measured in 5i. The workset spellings are handed in,
    /// every one the picked file's corrections ask, `RevitWorksets.With`: the 39 names measured
    /// in 5t inside Core, the workset lines of the list beside the picked file, Q113, and every
    /// spelling an also-ask line of it accepts, F131, so a spelling a MATRIX line says was
    /// measured or accepted is never one this calls carried by no model, F116. A set asking for
    /// something none of them holds is asking for something no model measured so far in this
    /// project has. THE LISTS ARE ONE PROJECT'S, each names its project, and `EmptySetJudge`
    /// reads them as a group's only where the group's models name the same project, FR-011.
    ///
    /// WHILE A LIST IS UNMEASURED THIS SAYS IT CANNOT TELL. A check that compared against
    /// an empty list would report every set in the file as asking for something nobody
    /// has, which is the loudest possible way of knowing nothing.
    /// </summary>
    public static class EmptySets
    {
        /// <summary>
        /// How many single letter edits still count as a near miss when suggesting the
        /// nearest value. TWO IS CHOSEN AND NOT MEASURED, the same number and the same
        /// reason as `WorksetDisagreements.Nearest`, and it is a suggestion either way.
        /// </summary>
        public const int Nearest = 2;

        /// <summary>The internal name of the Revit category property, as the client's matrix writes it.</summary>
        public const string CategoryProperty = "LcRevitPropertyElementCategory";

        /// <summary>The internal name of the Revit workset parameter, as the client's matrix writes it.</summary>
        public const string WorksetProperty = "lcldrevit_parameter_-1002053";

        /// <summary>
        /// Why that set found nothing, judged on the FIRST condition this reader knows how to
        /// judge, by what that judge knows, EmptySetJudge. One reason per set, because a set
        /// asking two things nobody has is still one wrong set and a person fixes it once. A
        /// value is said to be carried by no model only against a list of this project's
        /// models, FR-011, and where the judge knows of no such list it says it cannot tell
        /// and why.
        /// </summary>
        public static EmptySet Why(string path, IList<ReadCondition> asked, EmptySetJudge judge)
        {
            if (asked == null || asked.Count == 0 || judge == null)
            {
                return new EmptySet(path, EmptyReason.CannotTell, null, null);
            }

            string carried = null;
            string cannotTell = null;

            for (int i = 0; i < asked.Count; i++)
            {
                EmptySetJudge.Known known = Judgeable(asked[i].Test, asked[i].Flags) ? judge.KnownFor(asked[i].PropertyInternalName) : null;

                if (known == null)
                {
                    continue;
                }

                if (Carries(known.Carried, asked[i].Test, asked[i].Value))
                {
                    carried = carried ?? asked[i].Value;
                    continue;
                }

                if (!known.IsComplete)
                {
                    cannotTell = cannotTell ?? known.WhyNot;
                    continue;
                }

                if (known.Carried.Count == 0)
                {
                    continue;
                }

                return new EmptySet(
                    path, EmptyReason.NoModelCarriesTheValue, asked[i].Value, NearestIn(known.Carried, asked[i].Value));
            }

            if (cannotTell != null)
            {
                return new EmptySet(path, EmptyReason.CannotTell, null, null, cannotTell);
            }

            return carried != null
                ? new EmptySet(path, EmptyReason.TheValueIsThereAnyway, carried, null)
                : new EmptySet(path, EmptyReason.CannotTell, null, null);
        }

        /// <summary>The block, one line per empty set plus the counts and what it costs.</summary>
        public static IList<string> Lines(IList<EmptySet> empty, int testsWithAnEmptySide, int testsInAll)
        {
            return Lines(empty, testsWithAnEmptySide, testsInAll, 0);
        }

        /// <summary>
        /// The same block, plus how many tests this run could form no opinion about
        /// because a side of theirs was in no count. Said rather than folded into the
        /// cost, because a cost with a silent hole in it reads as a smaller cost.
        /// </summary>
        public static IList<string> Lines(
            IList<EmptySet> empty, int testsWithAnEmptySide, int testsInAll, int testsNoSideCountFor)
        {
            List<string> lines = new List<string>();

            if (empty == null || empty.Count == 0)
            {
                return lines;
            }

            int wrong = Of(empty, EmptyReason.NoModelCarriesTheValue);
            int there = Of(empty, EmptyReason.TheValueIsThereAnyway);
            int cannot = Of(empty, EmptyReason.CannotTell);

            lines.Add(empty.Count + " set(s) found nothing in this group, and this is why:");
            lines.Add("   " + wrong + " ask for a value NO MODEL MEASURED SO FAR IN THIS PROJECT CARRIES, so the condition is wrong"
                + " or the project has not been measured that far");
            lines.Add("   " + there + " ask for a value models in this project DO carry, so either this group"
                + " holds no model of that kind or something else is wrong");
            lines.Add("   " + cannot + " this reader cannot tell about");

            for (int i = 0; i < empty.Count; i++)
            {
                lines.Add("   " + empty[i].Line());
            }

            // WHAT IT COSTS, which is the number his own report made urgent. A set that
            // finds nothing is not one dead set, it is every clash test that points at it.
            lines.Add(Cost(testsWithAnEmptySide, testsInAll));

            if (testsNoSideCountFor > 0)
            {
                lines.Add("   and " + testsNoSideCountFor
                    + " more test(s) had a side no count was taken for, so they are in neither number");
            }

            return lines;
        }

        private static string Cost(int withAnEmptySide, int inAll)
        {
            if (inAll <= 0)
            {
                return "how many clash tests that costs is UNKNOWN, because no test count was read";
            }

            // A COUNT THAT WAS NOT TAKEN IS NOT A COST OF ZERO. The sides are only counted
            // on the path that creates tests from a picked file. A run over the tests saved
            // in the document never resolves a locator, so it has nothing to count with,
            // and it says so rather than reporting that the empty sets cost nothing.
            if (withAnEmptySide < 0)
            {
                return "how many clash tests that costs is UNKNOWN, because no side of any test was counted this run";
            }

            return "IT COSTS " + withAnEmptySide + " of this group's " + inAll
                + " clash tests, which have a side that finds nothing and so can never report a clash";
        }

        private static int Of(IList<EmptySet> empty, EmptyReason reason)
        {
            int count = 0;

            for (int i = 0; i < empty.Count; i++)
            {
                if (empty[i].Reason == reason)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Whether a condition with that test and those flags can be judged at all, FR-010, THE ONE
        /// RULE this judge and the HEALTH block read, SetWarnings: one asking equals or contains,
        /// the two the file writes, and NOT NEGATED. A negation asks for everything but its value,
        /// so a value no model carries leaves out nothing and stops nothing, 5g, FR-023. Another
        /// comparison is never read as equals. The HEALTH block kept only the negation half and
        /// read every other test as equals, so the two blocks disagreed about such a set.
        /// </summary>
        internal static bool Judgeable(string test, int flags)
        {
            return !PlannedCondition.NegatedWith(flags)
                && (string.Equals(test, SetBuildPlan.EqualsTest, StringComparison.Ordinal)
                    || string.Equals(test, SetBuildPlan.ContainsTest, StringComparison.Ordinal));
        }

        /// <summary>
        /// Whether those measured values carry what a condition asks, THE ONE RULE for a value
        /// and its test, read by this judge and by the HEALTH block, FR-010. Equals is the whole
        /// value, Ordinal. Contains is a stem, so Cable Tray is carried by Cable Trays and Cable
        /// Tray Fittings and is asked for by part of a name. It was read as equals here.
        /// </summary>
        internal static bool Carries(IList<string> known, string test, string value)
        {
            bool contains = string.Equals(test, SetBuildPlan.ContainsTest, StringComparison.OrdinalIgnoreCase);

            for (int i = 0; i < known.Count; i++)
            {
                if (contains
                    ? known[i].IndexOf(value, StringComparison.Ordinal) >= 0
                    : string.Equals(known[i], value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The nearest value the models carry, or empty where nothing is close.</summary>
        private static string NearestIn(IList<string> known, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string best = string.Empty;
            int bestAt = int.MaxValue;
            string lowered = value.ToLowerInvariant();

            for (int i = 0; i < known.Count; i++)
            {
                int distance = Distance(lowered, known[i].ToLowerInvariant());

                if (distance < bestAt)
                {
                    bestAt = distance;
                    best = known[i];
                }
            }

            return bestAt <= Nearest ? best : string.Empty;
        }

        private static int Distance(string a, string b)
        {
            if (Math.Abs(a.Length - b.Length) > Nearest)
            {
                return int.MaxValue;
            }

            int[] previous = new int[b.Length + 1];
            int[] current = new int[b.Length + 1];

            for (int j = 0; j <= b.Length; j++)
            {
                previous[j] = j;
            }

            for (int i = 1; i <= a.Length; i++)
            {
                current[0] = i;

                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    int best = Math.Min(current[j - 1] + 1, previous[j] + 1);
                    current[j] = Math.Min(best, previous[j - 1] + cost);
                }

                int[] swap = previous;
                previous = current;
                current = swap;
            }

            return previous[b.Length];
        }
    }
}
