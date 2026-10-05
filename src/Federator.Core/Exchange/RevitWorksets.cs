using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// Every workset name the C02 census measured in this project's Revit models, read off
    /// the models and never typed, Q68 and Q69, and no other. What the list does not hold is
    /// UNKNOWN, not absent. Spellings measured since on other run sets belong to the project
    /// and not to the tool, so they are kept as workset lines in the project's list of
    /// corrections beside the picked clash XML, Q113 answered B on 2026-10-04,
    /// MatrixCorrectionList, and read beside these.
    ///
    /// IT IS THE SPELLINGS AND NOT A JUDGEMENT. The client's matrix asks for a workset
    /// called `ME-DUCTWORK` and the C02 models write `ME-Ductwork`, and a search condition
    /// compares a value CASE SENSITIVELY unless a flag nothing sets is set, so a set asking
    /// one spelling finds nothing in the models writing another, 5q and Q102.
    /// `MatrixCorrections` asks every spelling measured here and in the project's list, as Or
    /// groups, where there are two or more, and the one measured where there is one, and
    /// THESE two are where the spellings come from. Nothing in this tool ever invents one.
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
        private static string project;

        /// <summary>
        /// The project the names were measured on, read off the list's project line, or null
        /// where it names none or was not read, FR-011.
        /// </summary>
        public static string Project
        {
            get
            {
                Load();
                return project;
            }
        }

        /// <summary>
        /// Whether the list could be READ out of the DLL at all, FR-012, shaped like
        /// RevitCategories.ResourceFound. A list missing or not readable read the same as an
        /// empty one, with no flag and no line, so the export check named the pairs a person
        /// already decided are not typos with nothing saying why.
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
        /// Every workset spelling measured for a picked clash XML: the names this list holds, then
        /// the workset lines of the project's list beside that XML, each once, Q113. THE ONE PLACE
        /// the two are put together. The corrections take their spellings from it and the judge
        /// of a set that found nothing is handed the same ones, so the log never says in a MATRIX
        /// line that a spelling was measured and in the EMPTY SETS block that no model carries
        /// it, F116. Null adds nothing, which is the names inside Core alone.
        /// </summary>
        internal static IList<string> With(IEnumerable<string> listed)
        {
            List<string> measured = new List<string>(Load());

            if (listed == null)
            {
                return measured;
            }

            foreach (string spelling in listed)
            {
                if (!string.IsNullOrEmpty(spelling) && !measured.Contains(spelling))
                {
                    measured.Add(spelling);
                }
            }

            return measured;
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
        private static void AddDecided(string line, List<string[]> into)
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
                into.Add(new[] { first, second });
            }
        }

        /// <summary>
        /// Reads the list once, out of the DLL, through Read. A resource that cannot be read is
        /// an EMPTY list and never a throw, and an empty list corrects nothing, which is the
        /// safe answer, and ResourceFound says it was not read, FR-012.
        /// </summary>
        private static List<string> Load()
        {
            lock (Gate)
            {
                if (known != null)
                {
                    return known;
                }

                List<string> names;
                List<string[]> pairs;

                using (Stream stream = typeof(RevitWorksets).Assembly.GetManifestResourceStream(ResourceName))
                {
                    resourceFound = Read(stream, out names, out pairs, out project);
                }

                decided = pairs;
                known = names;
                return known;
            }
        }

        /// <summary>
        /// The list read from that stream, the names and the decided pairs, and whether it was
        /// read, FR-012. A null stream is a list not in the DLL and a stream that throws is a list
        /// not read: each answers false with both lists empty, and never throws, because a
        /// health check is information and information never stops a run. The one reader of
        /// the list, so the DLL's copy and a test's are read by the same lines.
        /// </summary>
        internal static bool Read(Stream stream, out List<string> names, out List<string[]> pairs, out string measuredOn)
        {
            names = new List<string>();
            pairs = new List<string[]>();
            measuredOn = null;

            if (stream == null)
            {
                return false;
            }

            try
            {
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
                            AddDecided(line, pairs);
                            continue;
                        }

                        if (RevitCategories.ProjectIn(line) != null)
                        {
                            measuredOn = RevitCategories.ProjectIn(line);
                            continue;
                        }

                        names.Add(line);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                // Not swallowed: the answer is false, ResourceFound carries it, and the EXPORT
                // CHECK block says the decided pairs are UNKNOWN, FR-012.
                names = new List<string>();
                pairs = new List<string[]>();
                measuredOn = null;
                return false;
            }
        }
    }
}
