using System;
using System.Collections.Generic;
using Federator.Core.Exchange;

namespace Federator.Core.Health
{
    /// <summary>
    /// Two spellings of what looks like one workset, and which models carry each, Q69.
    /// </summary>
    public sealed class WorksetDisagreement
    {
        internal WorksetDisagreement(string first, IList<string> firstModels, string second, IList<string> secondModels, bool caseOnly)
        {
            First = first;
            FirstModels = firstModels;
            Second = second;
            SecondModels = secondModels;
            CaseOnly = caseOnly;
        }

        /// <summary>The spelling more models carry, which is the one more likely to be right.</summary>
        public string First { get; private set; }

        /// <summary>The models carrying it.</summary>
        public IList<string> FirstModels { get; private set; }

        /// <summary>The other spelling.</summary>
        public string Second { get; private set; }

        /// <summary>The models carrying that one.</summary>
        public IList<string> SecondModels { get; private set; }

        /// <summary>
        /// Whether the two differ by CASE ALONE. A case difference is something a
        /// correction rule can act on without judgement. A letter difference is a
        /// different word, and this tool names it and never merges it.
        /// </summary>
        public bool CaseOnly { get; private set; }

        /// <summary>The line the export check writes, one per pair.</summary>
        public string Line()
        {
            // 4e. WHERE THE DIFFERENCE IS INVISIBLE, NAME THE CHARACTER. "Two spellings"
            // is useless when a person looks at both and sees no difference at all, which
            // is what a non breaking space does.
            string invisible = InvisibleDifference.Between(First, Second);

            return "\"" + First + "\" in " + Listed(FirstModels)
                + "   against   \"" + Second + "\" in " + Listed(SecondModels)
                + (invisible != null
                    ? "   THEY LOOK IDENTICAL AND ARE NOT: " + invisible
                    : CaseOnly
                        ? "   the same word in a different case"
                        : "   ONE OR TWO LETTERS APART, so this may be a typo");
        }

        private static string Listed(IList<string> models)
        {
            if (models.Count == 0)
            {
                return "no model";
            }

            if (models.Count <= 3)
            {
                string[] array = new string[models.Count];
                models.CopyTo(array, 0);
                return string.Join(", ", array);
            }

            return models[0] + " and " + (models.Count - 1) + " more";
        }
    }

    /// <summary>
    /// Where this project's own models disagree with each other about the name of a
    /// workset, Q69 answered b on 2026-09-20.
    ///
    /// IT NAMES AND IT NEVER MERGES. `EL-Lightining Protection` against `EL-Lightning
    /// Protection` is a typo, and `AR-EXTERIOR` against `AR-INTERIOR` is two real
    /// worksets, and they are two and one letter apart respectively, 5t. NO RULE CAN TELL
    /// THEM APART, so this tool reports both kinds and lets a person decide which is
    /// which. That is the whole point of Q69's second half: if the tool absorbs a typo
    /// silently nobody ever fixes it and the next building repeats it.
    ///
    /// WHAT IT COMPARES. Only names close enough to be one word typed twice: the same
    /// word in a different case, or one or two single letter edits apart. Anything
    /// further is two different worksets and is not a disagreement at all.
    /// </summary>
    public static class WorksetDisagreements
    {
        /// <summary>
        /// How many single letter edits still count as one word typed twice. TWO IS
        /// CHOSEN AND NOT MEASURED, and it says so. One catches every real typo in this
        /// project, `Lightining`, `Ccctv` and `equipmen`. Two also catches `AR-EXTERIOR`
        /// against `AR-INTERIOR`, which is NOT a typo, and that is exactly why this rule
        /// reports rather than acts.
        /// </summary>
        public const int Nearest = 2;

        /// <summary>
        /// What separates a discipline prefix from the rest of a workset name on this
        /// project, `ME-Ductwork` and `EL-Power`. A setting rather than a constant,
        /// because a project spelling them another way is a project this rule should not
        /// silently mis-read.
        /// </summary>
        public const char PrefixSeparator = '-';

        /// <summary>
        /// How many pairs were found and NOT named because a person has already decided
        /// they are two different worksets. Counted rather than hidden, because a check
        /// that quietly stops looking at something is a check nobody can audit.
        /// </summary>
        public static int DecidedInLastRead { get; private set; }

        /// <summary>
        /// Every pair of names in that lookup close enough to be one word typed twice,
        /// less the ones a person has already decided about. The lookup is workset name
        /// against the models carrying it, which is what the export check already
        /// gathers, so nothing is read twice.
        /// </summary>
        public static IList<WorksetDisagreement> In(IDictionary<string, IList<string>> byWorkset)
        {
            List<WorksetDisagreement> found = new List<WorksetDisagreement>();
            DecidedInLastRead = 0;

            if (byWorkset == null || byWorkset.Count < 2)
            {
                return found;
            }

            List<string> names = new List<string>(byWorkset.Keys);
            names.Sort(StringComparer.Ordinal);

            for (int i = 0; i < names.Count; i++)
            {
                for (int j = i + 1; j < names.Count; j++)
                {
                    bool caseOnly = string.Equals(names[i], names[j], StringComparison.OrdinalIgnoreCase);

                    // THE DISCIPLINE PREFIX IS NOT PART OF THE COMPARISON, 4d. Two names
                    // whose prefixes differ are NEVER a typo pair however close the rest
                    // is, and two sharing a prefix and differing in the body still are.
                    //
                    // WHY. `FP-PIPING` against `ME-PIPING` is one edit apart in the body
                    // and they are two different disciplines, Fire Protection and
                    // Mechanical. Naming them is the crying wolf fault this rule already
                    // had once for `AR-EXTERIOR` against `AR-INTERIOR`, and it costs the
                    // real typo sitting beside it: C04's 1A04WM carries
                    // `EL-Lightining Protection` against `EL-Lightning Protection`, which
                    // shares its prefix and IS a typo.
                    //
                    // A prefix pair that really is one thing spelled two ways is a
                    // different question from a typo, and `RevitWorksets` is where a
                    // decision about one belongs.
                    if (!caseOnly && !SamePrefix(names[i], names[j]))
                    {
                        continue;
                    }

                    if (!caseOnly && Distance(names[i].ToLowerInvariant(), names[j].ToLowerInvariant()) > Nearest)
                    {
                        continue;
                    }

                    // A PERSON HAS ALREADY DECIDED ABOUT THIS PAIR, so the tool does not
                    // ask again on every group of every run. It was naming AR-EXTERIOR
                    // against AR-INTERIOR ten times a run, and a check that cries wolf on
                    // something known good is one a person learns to skip past, which
                    // costs the three real typos beside it.
                    if (RevitWorksets.DecidedDifferent(names[i], names[j]))
                    {
                        DecidedInLastRead++;
                        continue;
                    }

                    // The spelling MORE MODELS carry goes first, because it is the one
                    // more likely to be the right one, and a person reading the line
                    // wants the odd one out named second.
                    bool firstWins = byWorkset[names[i]].Count >= byWorkset[names[j]].Count;
                    string a = firstWins ? names[i] : names[j];
                    string b = firstWins ? names[j] : names[i];

                    found.Add(new WorksetDisagreement(a, byWorkset[a], b, byWorkset[b], caseOnly));
                }
            }

            return found;
        }

        /// <summary>
        /// The discipline prefix, which is everything before the first separator, or the
        /// whole name where it carries none. Compared case blind, because a prefix
        /// differing only in case is the same discipline and the case rule handles it.
        /// </summary>
        public static string PrefixOf(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            int at = name.IndexOf(PrefixSeparator);

            return at <= 0 ? name : name.Substring(0, at);
        }

        /// <summary>Whether two names carry the same discipline prefix.</summary>
        public static bool SamePrefix(string left, string right)
        {
            return string.Equals(PrefixOf(left), PrefixOf(right), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>How many single letter edits turn one word into the other, given up on past Nearest.</summary>
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
