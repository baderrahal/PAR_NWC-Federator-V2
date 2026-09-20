using System;
using System.Collections.Generic;
using Federator.Core.Exchange;

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
        {
            Path = path ?? string.Empty;
            Reason = reason;
            Asked = asked ?? string.Empty;
            Nearest = nearest ?? string.Empty;
        }

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
                    return Path + "   asks for \"" + Asked + "\" and NO MODEL IN THIS PROJECT CARRIES IT"
                        + (Nearest.Length == 0
                            ? ", and nothing the models carry is close to it"
                            : ". The nearest the models carry is \"" + Nearest + "\", which is a suggestion and not a correction");

                case EmptyReason.TheValueIsThereAnyway:
                    return Path + "   asks for \"" + Asked + "\", WHICH THE MODELS DO CARRY, and still found nothing."
                        + " Something else is wrong and a person has to look";

                default:
                    return Path + "   found nothing and THIS READER CANNOT TELL WHY";
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
    /// values the models really carry, measured in 5i, and `RevitWorksets` the 39 workset
    /// names, measured in 5t. A set asking for something neither list holds is asking for
    /// something no model in this project has.
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
        /// Why that set found nothing, judged on the FIRST condition this reader knows
        /// how to judge. One reason per set, because a set asking two things nobody has
        /// is still one wrong set and a person fixes it once.
        /// </summary>
        public static EmptySet Why(string path, IList<ReadCondition> asked)
        {
            if (asked == null || asked.Count == 0)
            {
                return new EmptySet(path, EmptyReason.CannotTell, null, null);
            }

            bool judgedAny = false;

            for (int i = 0; i < asked.Count; i++)
            {
                IList<string> known = KnownFor(asked[i].PropertyInternalName);

                if (known == null || known.Count == 0)
                {
                    continue;
                }

                judgedAny = true;

                if (Holds(known, asked[i].Value))
                {
                    continue;
                }

                return new EmptySet(
                    path, EmptyReason.NoModelCarriesTheValue, asked[i].Value, NearestIn(known, asked[i].Value));
            }

            return judgedAny
                ? new EmptySet(path, EmptyReason.TheValueIsThereAnyway, FirstJudgeable(asked), null)
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
            lines.Add("   " + wrong + " ask for a value NO MODEL IN THIS PROJECT CARRIES, so the condition is wrong");
            lines.Add("   " + there + " ask for a value the models DO carry, so something else is wrong");
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

        private static string FirstJudgeable(IList<ReadCondition> asked)
        {
            for (int i = 0; i < asked.Count; i++)
            {
                IList<string> known = KnownFor(asked[i].PropertyInternalName);

                if (known != null && known.Count > 0)
                {
                    return asked[i].Value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// The measured list for that property, or null where this tool has no list and
        /// therefore no opinion. Never a guess: a property nobody measured is one this
        /// reader says it cannot tell about.
        /// </summary>
        private static IList<string> KnownFor(string propertyInternalName)
        {
            if (string.Equals(propertyInternalName, CategoryProperty, StringComparison.Ordinal))
            {
                return RevitCategories.Measured ? RevitCategories.All() : null;
            }

            if (string.Equals(propertyInternalName, WorksetProperty, StringComparison.Ordinal))
            {
                return RevitWorksets.All();
            }

            return null;
        }

        private static bool Holds(IList<string> known, string value)
        {
            for (int i = 0; i < known.Count; i++)
            {
                if (string.Equals(known[i], value, StringComparison.Ordinal))
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
