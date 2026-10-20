using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// The rules of a matrix pack that shape which tests a run creates, F247, Bader's answer Q230 of 20 Oct 2026, read off the
    /// pack's own pack-rules.txt, MatrixPack.RulesFile, so a project's choice is a setting of its pack and never a constant
    /// in code, PR6, and the next project's pack carries its own.
    ///
    /// THE FORMAT is the team map's: one line a rule, a word and a colon, a space, then its parts split by a space, a bar
    /// and a space. A line starting with # is a comment and a blank line is skipped. Nothing is trimmed. A line the reader
    /// does not know, a rule written twice, or a part written twice in one rule makes the whole file unread, so the run
    /// reads none of its rules and says so, never half.
    ///
    ///   same-code-left-out: AR | ST
    ///
    /// SAME-CODE-LEFT-OUT names the set codes whose tests of one set code against itself are left out, SetCodeRule. A test
    /// whose two sides carry one set code of any other code runs. The rule with no part leaves out none. A file that is
    /// not there or would not read leaves out none either, and the PACK line says which, so every test of one set code runs
    /// and nothing is hidden on a guess.
    /// </summary>
    public sealed class PackRules
    {
        /// <summary>The word of the rule naming the set codes whose tests of one set code are left out.</summary>
        public const string SameCodeLeftOutRule = "same-code-left-out";

        private const string Colon = ": ";

        private const string Bar = " | ";

        private PackRules(string source, string unread, IList<string> sameCodeLeftOut)
        {
            Source = source;
            Unread = unread;
            SameCodeLeftOut = new ReadOnlyCollection<string>(new List<string>(sameCodeLeftOut ?? new string[0]));
        }

        /// <summary>Where the rules were read from, as the PACK line names it.</summary>
        public string Source { get; private set; }

        /// <summary>Why no rule was read, or null where the file read.</summary>
        public string Unread { get; private set; }

        /// <summary>Whether the file read, every rule in it.</summary>
        public bool IsRead
        {
            get { return Unread == null; }
        }

        /// <summary>The set codes whose tests of one set code against itself are left out, empty where none is or nothing read.</summary>
        public ReadOnlyCollection<string> SameCodeLeftOut { get; private set; }

        /// <summary>No rules, with why, such as a pack that holds no pack-rules.txt.</summary>
        public static PackRules NoneRead(string source, string why)
        {
            return new PackRules(source, why ?? "UNKNOWN", null);
        }

        /// <summary>The rules of that text, or none with the first fault named.</summary>
        public static PackRules Read(string text, string source)
        {
            if (text == null)
            {
                return NoneRead(source, "the file holds nothing");
            }

            Dictionary<string, List<string>> rules = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            string[] lines = text.Replace("\r\n", "\n").Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string where = "line " + (i + 1) + ", \"" + line + "\"";
                int colon = line.IndexOf(':');

                if (colon <= 0)
                {
                    return NoneRead(source, where + ", is no rule, a rule being a word, a colon and its parts");
                }

                string word = line.Substring(0, colon);

                if (!string.Equals(word, SameCodeLeftOutRule, StringComparison.Ordinal))
                {
                    return NoneRead(source, where + ", names a rule this tool does not know, " + word);
                }

                if (rules.ContainsKey(word))
                {
                    return NoneRead(source, where + ", writes the rule " + word + " a second time");
                }

                string rest = line.Substring(colon + 1);
                List<string> parts = new List<string>();

                if (rest.Length > 0)
                {
                    if (!rest.StartsWith(Colon.Substring(1), StringComparison.Ordinal))
                    {
                        return NoneRead(source, where + ", has no space after its colon");
                    }

                    foreach (string part in rest.Substring(1).Split(new[] { Bar }, StringSplitOptions.None))
                    {
                        if (part.Length == 0 || part.Contains("|"))
                        {
                            return NoneRead(source, where + ", has an empty part or a bar with no space either side");
                        }

                        // A code read off a file name holds no space and no character a person cannot see, so a part that
                        // does would never equal one, the team map's rule, read through its one check.
                        int unseen = Federator.Core.Teams.TeamMap.Unseen(part, true);

                        if (unseen >= 0)
                        {
                            return NoneRead(source, where + ", names \"" + part + "\", which has "
                                + Federator.Core.Health.InvisibleDifference.Describe(part[unseen]) + " in it");
                        }

                        if (parts.Contains(part))
                        {
                            return NoneRead(source, where + ", names " + part + " twice");
                        }

                        parts.Add(part);
                    }
                }

                rules[word] = parts;
            }

            List<string> leftOut;
            rules.TryGetValue(SameCodeLeftOutRule, out leftOut);

            return new PackRules(source, null, leftOut);
        }

        /// <summary>
        /// The PACK line saying what the rules leave out, or that none was read and why, so every test of one set code runs.
        /// </summary>
        public string Line()
        {
            if (!IsRead)
            {
                return MatrixPack.LinePrefix + "the pack's rules were not read, " + Unread + ", so no test of one set code is left out"
                    + " and every one runs, Bader's answer to Q230";
            }

            return MatrixPack.LinePrefix + "the pack's rules read off " + Source + ", "
                + (SameCodeLeftOut.Count == 0
                    ? "no set code is left out against itself, so every test of one set code runs"
                    : "a test of one set code is left out for " + Joined(SameCodeLeftOut) + " alone, and every test of one set code of"
                        + " another code runs")
                + ", Bader's answer to Q230";
        }

        /// <summary>The codes as words, AR, AR and ST, or AR, ST and EL.</summary>
        public static string Joined(IList<string> codes)
        {
            if (codes == null || codes.Count == 0)
            {
                return "none";
            }

            if (codes.Count == 1)
            {
                return codes[0];
            }

            List<string> first = new List<string>(codes);
            first.RemoveAt(first.Count - 1);
            return string.Join(", ", first.ToArray()) + " and " + codes[codes.Count - 1];
        }
    }
}
