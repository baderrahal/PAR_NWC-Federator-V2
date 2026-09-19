using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Clash
{
    /// <summary>
    /// The clash matrix's priority per test, read out of a CSV, F83.
    ///
    /// WHY A SEPARATE FILE. The priority is a decision the project made about which clashes
    /// matter, and it is not in the clash XML at all: nothing Navisworks exports carries
    /// it. So it arrives beside the XML as its own file, picked on the Clash step, and
    /// WHEN NOTHING IS PICKED NOTHING CHANGES. Every output, every order and every block
    /// reads exactly as it did.
    ///
    /// THE TEST NAME IS MATCHED EXACTLY, Ordinal and never trimmed, the same rule set
    /// names and locators follow. Two set names in the reference file end in a space and a
    /// test name is built from two set names, so trimming would match a different test.
    ///
    /// WHAT IS NOT MATCHED ON. The file carries left_set and right_set as well, and they
    /// are read and kept so a person can see what a row meant, but the match is on the
    /// test name alone. Matching on the pair would need a rule about which way round they
    /// come, and the test name already holds both in the order the file wrote them.
    ///
    /// PRIORITY IS NOT STATUS. Nothing here reads, writes or names a Navisworks status.
    /// </summary>
    public sealed class PriorityMap
    {
        /// <summary>The header the file carries, in order.</summary>
        public static readonly string[] Columns = { "test_name", "left_set", "right_set", "priority" };

        /// <summary>The words that begin every line this rule writes.</summary>
        public const string Prefix = "PRIORITY";

        /// <summary>How many unmatched rows are named. The five examples rule.</summary>
        public const int ExamplesShown = 5;

        private readonly Dictionary<string, ClashPriority> byTestName;
        private readonly List<string> problems;

        private PriorityMap()
        {
            byTestName = new Dictionary<string, ClashPriority>(StringComparer.Ordinal);
            problems = new List<string>();
        }

        /// <summary>Nothing picked. Every lookup answers None and nothing anywhere changes.</summary>
        public static PriorityMap NothingPicked()
        {
            return new PriorityMap();
        }

        /// <summary>Whether a file was picked at all.</summary>
        public bool Picked { get; private set; }

        /// <summary>The path it was read from, or empty.</summary>
        public string Path { get; private set; }

        /// <summary>How many rows the file holds.</summary>
        public int RowCount
        {
            get { return byTestName.Count; }
        }

        /// <summary>Rows the file holds that this tool could not use, with the reason.</summary>
        public IList<string> Problems
        {
            get { return new List<string>(problems); }
        }

        /// <summary>
        /// Reads the file's text. A row whose priority is not A, B or C is kept as a
        /// problem and left out, rather than becoming a silent None that reads like a test
        /// the matrix never mentioned.
        /// </summary>
        public static PriorityMap Read(string text, string path)
        {
            PriorityMap map = new PriorityMap();
            map.Picked = true;
            map.Path = path ?? string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                map.problems.Add("the file is empty");
                return map;
            }

            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            bool first = true;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Length == 0)
                {
                    continue;
                }

                IList<string> cells = Csv.Cells(line);

                if (first)
                {
                    first = false;

                    if (LooksLikeTheHeader(cells))
                    {
                        continue;
                    }
                }

                if (cells.Count < 4)
                {
                    map.problems.Add("line " + (i + 1) + " holds " + cells.Count
                        + (cells.Count == 1 ? " cell" : " cells") + " and needs four");
                    continue;
                }

                string name = cells[0];
                ClashPriority priority = Priorities.From(cells[3]);

                if (name.Length == 0)
                {
                    map.problems.Add("line " + (i + 1) + " names no test");
                    continue;
                }

                if (priority == ClashPriority.None)
                {
                    map.problems.Add("line " + (i + 1) + " gives \"" + name
                        + "\" a priority of \"" + cells[3] + "\", which is not A, B or C");
                    continue;
                }

                map.byTestName[name] = priority;
            }

            return map;
        }

        /// <summary>
        /// That test's priority, or None. Matched Ordinal and never trimmed, because a
        /// test name is built out of two set names and two of those end in a space.
        /// </summary>
        public ClashPriority Of(string testName)
        {
            if (string.IsNullOrEmpty(testName))
            {
                return ClashPriority.None;
            }

            ClashPriority priority;
            return byTestName.TryGetValue(testName, out priority) ? priority : ClashPriority.None;
        }

        /// <summary>Whether the file names that test at all.</summary>
        public bool Names(string testName)
        {
            return !string.IsNullOrEmpty(testName) && byTestName.ContainsKey(testName);
        }

        /// <summary>
        /// How the file lined up with the tests this run actually has. Every number is
        /// counted off the two lists and none is worked out from another, so a count that
        /// does not add up is a fault rather than a rounding.
        /// </summary>
        public IList<string> MatchLines(IEnumerable<string> testNames)
        {
            List<string> lines = new List<string>();

            if (!Picked)
            {
                return lines;
            }

            List<string> unmatched = new List<string>();
            int matched = 0;
            int tests = 0;

            if (testNames != null)
            {
                foreach (string name in testNames)
                {
                    tests++;

                    if (Names(name))
                    {
                        matched++;
                    }
                    else
                    {
                        unmatched.Add(name);
                    }
                }
            }

            lines.Add(Prefix + " " + RowCount + " in the file, " + tests + " tests in this run, "
                + matched + " matched, " + unmatched.Count + " not named by the file");

            int shown = 0;

            foreach (string name in unmatched)
            {
                if (shown == ExamplesShown)
                {
                    break;
                }

                lines.Add("         " + name);
                shown++;
            }

            if (unmatched.Count > shown)
            {
                lines.Add("         and " + (unmatched.Count - shown)
                    + " more not named by the file, counted and not listed");
            }

            foreach (string problem in problems)
            {
                lines.Add("         " + problem);
            }

            return lines;
        }

        private static bool LooksLikeTheHeader(IList<string> cells)
        {
            if (cells == null || cells.Count < 1)
            {
                return false;
            }

            return string.Equals(cells[0].Trim(), Columns[0], StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Reading one line of a CSV somebody typed, for the two small files this tool is
    /// handed, F83 and F72b. One rule in one place, because a second copy would drift on
    /// the quoting.
    ///
    /// A quoted cell may hold a comma and a doubled quote. A newline inside a quoted cell
    /// is NOT supported and never will be: these files have one row per line and are read
    /// line by line so a fault in one row cannot swallow the next twenty.
    /// </summary>
    public static class Csv
    {
        public static IList<string> Cells(string line)
        {
            List<string> cells = new List<string>();

            if (line == null)
            {
                return cells;
            }

            System.Text.StringBuilder cell = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c != '"')
                    {
                        cell.Append(c);
                        continue;
                    }

                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                        continue;
                    }

                    inQuotes = false;
                    continue;
                }

                if (c == '"' && cell.Length == 0)
                {
                    inQuotes = true;
                    continue;
                }

                if (c == ',')
                {
                    cells.Add(cell.ToString());
                    cell.Length = 0;
                    continue;
                }

                cell.Append(c);
            }

            cells.Add(cell.ToString());
            return cells;
        }
    }
}
