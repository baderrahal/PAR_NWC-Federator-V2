using System;
using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>One pair of sets that clash because that is how the building goes together.</summary>
    public sealed class ByDesignPair
    {
        internal ByDesignPair(string left, string right, string reason)
        {
            Left = left ?? string.Empty;
            Right = right ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        /// <summary>The first set name, exactly as the file wrote it.</summary>
        public string Left { get; private set; }

        /// <summary>The second set name, exactly as the file wrote it.</summary>
        public string Right { get; private set; }

        /// <summary>Why that pair is by design, in the words the file gave.</summary>
        public string Reason { get; private set; }

        /// <summary>
        /// The two names sorted, which is how a pair is matched. A test can name them
        /// either way round and it is the same connection either way.
        /// </summary>
        public string Key
        {
            get { return ByDesignPairs.KeyFor(Left, Right); }
        }

        public override string ToString()
        {
            return Left + " and " + Right + ", " + Reason;
        }
    }

    /// <summary>
    /// The by design connections, F72b. A second rule beside the penetration rule, and it
    /// answers a different question.
    ///
    /// WHAT IT IS FOR. A column sitting on its foundation, a door in a wall, a valve in a
    /// pipe run. Every one of those is a clash and none of them is a problem, and a person
    /// looks at the same several hundred of them every week. Marking them Reviewed says
    /// somebody looked and moved on, which is true when the rule that picked them is a
    /// rule that person agreed to, and the agreement here is the list file itself.
    ///
    /// IT IS A LIST AND NOT A JUDGEMENT. The penetration rule works a clash out from its
    /// categories and its size. This one knows nothing about categories, sizes, items or
    /// disciplines. It reads two set names off a file somebody wrote and matches them
    /// against the two sides of a test. That is the whole of it, and it is why the pairs
    /// live in a file rather than in the code: they are this project's statement about
    /// this project's sets.
    ///
    /// SET NAMES ARE MATCHED Ordinal AND ARE NEVER TRIMMED, which is the opposite of how
    /// a CATEGORY is matched. Two set names in the reference file end in a space, and this
    /// file carries both "BLD-ME-Ducts&amp;Duct Fittings" and "BLD-DR-Pipes &amp; Pipe
    /// Fittings", which are two different spellings and both are real. Trimming or
    /// lowering either would match a set nobody named.
    ///
    /// THE TWO NAMES ARE SORTED BEFORE MATCHING, so a pair written one way round matches a
    /// test written the other way round. A clash between a column and a foundation is the
    /// same connection whichever side the matrix put first.
    ///
    /// THE SAME STATUS GUARD. Only New and Active ever move, through
    /// StatusesThisToolMayMoveFrom, and Reviewed is the only status this tool ever sets.
    /// A decision somebody made is never overwritten by either rule.
    /// </summary>
    public sealed class ByDesignPairs
    {
        /// <summary>The header the file carries, in order.</summary>
        public static readonly string[] Columns = { "left_set", "right_set", "reason" };

        /// <summary>The label on the tick box. Six words, and the limit is eight.</summary>
        public const string TickLabel = "Mark by design connections as Reviewed";

        /// <summary>
        /// The grey line under it. Twelve words, which is exactly the limit, so any
        /// rewording has to be counted again.
        ///
        /// The brief wrote it as "From a list file", which is thirteen words and one over
        /// the limit a grey line has. The word list is the one that carries least: a file
        /// picked beside a tick box is a list and the picker beside it says so.
        /// </summary>
        public const string HelpLine =
            "Column on foundation, door in wall, valve in pipe. From a file";

        private readonly Dictionary<string, ByDesignPair> byKey;
        private readonly List<ByDesignPair> pairs;
        private readonly List<string> problems;

        private ByDesignPairs()
        {
            byKey = new Dictionary<string, ByDesignPair>(StringComparer.Ordinal);
            pairs = new List<ByDesignPair>();
            problems = new List<string>();
        }

        /// <summary>Nothing picked. Nothing matches and nothing anywhere changes.</summary>
        public static ByDesignPairs NothingPicked()
        {
            return new ByDesignPairs();
        }

        public bool Picked { get; private set; }

        public string Path { get; private set; }

        public int Count
        {
            get { return pairs.Count; }
        }

        public IList<ByDesignPair> All
        {
            get { return new List<ByDesignPair>(pairs); }
        }

        public IList<string> Problems
        {
            get { return new List<string>(problems); }
        }

        /// <summary>
        /// Reads the file's text. A row this tool cannot use is kept as a problem and left
        /// out, never dropped silently, because a pair nobody noticed was missing is a
        /// pair somebody will look at every week for a year.
        /// </summary>
        public static ByDesignPairs Read(string text, string path)
        {
            ByDesignPairs found = new ByDesignPairs();
            found.Picked = true;
            found.Path = path ?? string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                found.problems.Add("the file is empty");
                return found;
            }

            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            bool first = true;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length == 0)
                {
                    continue;
                }

                IList<string> cells = Csv.Cells(lines[i]);

                if (first)
                {
                    first = false;

                    if (cells.Count > 0
                        && string.Equals(cells[0].Trim(), Columns[0], StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                if (cells.Count < 3)
                {
                    found.problems.Add("line " + (i + 1) + " holds " + cells.Count
                        + (cells.Count == 1 ? " cell" : " cells") + " and needs three");
                    continue;
                }

                if (cells[0].Length == 0 || cells[1].Length == 0)
                {
                    found.problems.Add("line " + (i + 1) + " does not name two sets");
                    continue;
                }

                ByDesignPair pair = new ByDesignPair(cells[0], cells[1], cells[2]);

                if (found.byKey.ContainsKey(pair.Key))
                {
                    found.problems.Add("line " + (i + 1) + " names the same pair as an earlier line, "
                        + pair.Left + " and " + pair.Right);
                    continue;
                }

                found.byKey[pair.Key] = pair;
                found.pairs.Add(pair);
            }

            return found;
        }

        /// <summary>
        /// The pair those two sets make, or null. The two are sorted first, so a test
        /// naming them the other way round still matches.
        /// </summary>
        public ByDesignPair For(string left, string right)
        {
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
            {
                return null;
            }

            ByDesignPair pair;
            return byKey.TryGetValue(KeyFor(left, right), out pair) ? pair : null;
        }

        /// <summary>Whether those two sets are a by design connection.</summary>
        public bool Holds(string left, string right)
        {
            return For(left, right) != null;
        }

        /// <summary>
        /// The two names sorted, Ordinal, joined by a separator no set name carries. A
        /// pair written one way round and a test written the other way round give the same
        /// key, because a column on a foundation is the same connection either way.
        /// </summary>
        public static string KeyFor(string left, string right)
        {
            string first = left ?? string.Empty;
            string second = right ?? string.Empty;

            return string.Compare(first, second, StringComparison.Ordinal) <= 0
                ? first + "\u0001" + second
                : second + "\u0001" + first;
        }

        /// <summary>
        /// The pairs in the file that matched no test in this run, F72b. A FINDING and
        /// nothing more: it names the pair, says it matched nothing, and the run carries
        /// on. Nothing is skipped, nothing is corrected and no group is judged on it.
        /// </summary>
        public IList<ByDesignPair> NotMatched(IEnumerable<string> keysSeen)
        {
            List<ByDesignPair> missed = new List<ByDesignPair>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            if (keysSeen != null)
            {
                foreach (string key in keysSeen)
                {
                    seen.Add(key);
                }
            }

            foreach (ByDesignPair pair in pairs)
            {
                if (!seen.Contains(pair.Key))
                {
                    missed.Add(pair);
                }
            }

            return missed;
        }
    }
}
