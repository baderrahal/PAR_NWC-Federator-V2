using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// Every workset name a Revit model in this project really carries, measured off the
    /// models and never typed, Q68 and Q69.
    ///
    /// IT IS THE SPELLINGS AND NOT A JUDGEMENT. The client's matrix asks for a workset
    /// called `ME-DUCTWORK` and every model writes `ME-Ductwork`, and a search condition
    /// compares a value CASE SENSITIVELY unless a flag nothing sets is set, so three
    /// mechanical sets found nothing in every group of every run, 5q. `MatrixCorrections`
    /// corrects the matrix to the spelling the models use and THIS is where the spellings
    /// come from. Nothing in this tool ever invents one.
    ///
    /// IT IS NOT THE PENETRATION LISTS AND NOT THE CATEGORY LIST. Those two say what
    /// somebody has decided about a category. This says what is in the models, the same
    /// way `RevitCategories` does and for the same reason, and it ships inside the DLL as
    /// an embedded resource so it cannot arrive on 26 machines and not on the 27th.
    /// </summary>
    public static class RevitWorksets
    {
        /// <summary>The resource the list lives in, named once.</summary>
        public const string ResourceName = "Federator.Core.Exchange.revit-worksets.txt";

        /// <summary>
        /// How a line in the list marks two names a person has decided are two DIFFERENT
        /// worksets rather than one typed twice. `AR-EXTERIOR` and `AR-INTERIOR` are two
        /// letters apart and both real, and so are `ST-SUB` and `ST-SUP`, and no rule can
        /// tell either pair from a typo, 5t. So a person decided once and the tool reads
        /// the decision instead of asking the same question on every group of every run.
        /// </summary>
        public const string DecidedMarker = "not-a-typo:";

        private static readonly object Gate = new object();
        private static List<string> known;
        private static List<string[]> decided;
        private static bool resourceFound;

        /// <summary>Every workset the list names, in the order the file wrote them.</summary>
        public static IList<string> All()
        {
            return new List<string>(Load());
        }

        /// <summary>How many the list names. Zero until it is measured.</summary>
        public static int Count
        {
            get { return Load().Count; }
        }

        /// <summary>Whether the list has been measured at all. Everything that reads it asks this first.</summary>
        public static bool Measured
        {
            get { return Count > 0; }
        }

        /// <summary>
        /// Whether the list could be READ out of the DLL at all. A resource that is
        /// missing is a different fact from a list nobody has filled in yet, and the two
        /// would otherwise give the same empty list and the same words.
        /// </summary>
        public static bool ResourceFound
        {
            get
            {
                Load();
                return resourceFound;
            }
        }

        /// <summary>
        /// The spellings the models carry for that value, differing from it by CASE
        /// ALONE, in the order the list holds them. A name differing by a letter is a
        /// different word and is never offered, which 5t proved twice over on this
        /// project: `AR-EXTERIOR` against `AR-INTERIOR` and `ST-SUB` against `ST-SUP` are
        /// two pairs of real worksets one and two letters apart.
        /// </summary>
        public static IList<string> SpelledLike(string value)
        {
            List<string> found = new List<string>();

            if (string.IsNullOrEmpty(value))
            {
                return found;
            }

            List<string> all = Load();

            for (int i = 0; i < all.Count; i++)
            {
                if (string.Equals(all[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    found.Add(all[i]);
                }
            }

            return found;
        }

        /// <summary>
        /// Whether a person has already decided that those two names are two DIFFERENT
        /// worksets. Either way round, because a pair is a pair, and Ordinal because a
        /// workset name is matched exactly everywhere else in this tool.
        /// </summary>
        public static bool DecidedDifferent(string first, string second)
        {
            if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second))
            {
                return false;
            }

            Load();

            for (int i = 0; i < decided.Count; i++)
            {
                bool wayRound = string.Equals(decided[i][0], first, StringComparison.Ordinal)
                    && string.Equals(decided[i][1], second, StringComparison.Ordinal);

                bool otherWay = string.Equals(decided[i][0], second, StringComparison.Ordinal)
                    && string.Equals(decided[i][1], first, StringComparison.Ordinal);

                if (wayRound || otherWay)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>How many pairs a person has decided about. Zero is a real answer.</summary>
        public static int DecidedCount
        {
            get
            {
                Load();
                return decided.Count;
            }
        }

        /// <summary>One decided line, split on the marker and the bar. A line that will not split is skipped.</summary>
        private static void AddDecided(string line)
        {
            string both = line.Substring(DecidedMarker.Length);
            int bar = both.IndexOf('|');

            if (bar < 0)
            {
                return;
            }

            string first = both.Substring(0, bar).Trim();
            string second = both.Substring(bar + 1).Trim();

            if (first.Length > 0 && second.Length > 0)
            {
                decided.Add(new[] { first, second });
            }
        }

        /// <summary>The one line about the list itself, so a reader knows whether it was read at all.</summary>
        public static string Line()
        {
            if (!ResourceFound)
            {
                return "Revit worksets known: UNKNOWN, the workset list could not be read out of "
                    + "Federator.Core.dll, so no value was corrected against them";
            }

            return Measured
                ? "Revit worksets known: " + Count
                : "Revit worksets known: none yet, so no value was corrected against them. "
                    + "The list is measured off a real federation, see the scan notes";
        }

        /// <summary>
        /// Reads the list once. A resource that cannot be read is an EMPTY list and never
        /// a throw, and an empty list corrects nothing, which is the safe answer.
        /// </summary>
        private static List<string> Load()
        {
            lock (Gate)
            {
                if (known != null)
                {
                    return known;
                }

                known = new List<string>();
                decided = new List<string[]>();

                try
                {
                    Assembly assembly = typeof(RevitWorksets).Assembly;

                    using (Stream stream = assembly.GetManifestResourceStream(ResourceName))
                    {
                        if (stream == null)
                        {
                            resourceFound = false;
                            return known;
                        }

                        resourceFound = true;

                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string line;

                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.Length == 0 || line[0] == '#')
                                {
                                    continue;
                                }

                                if (line.IndexOf(DecidedMarker, StringComparison.Ordinal) == 0)
                                {
                                    AddDecided(line);
                                    continue;
                                }

                                known.Add(line);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    known = new List<string>();
                decided = new List<string[]>();
                    resourceFound = false;
                }

                return known;
            }
        }
    }
}
