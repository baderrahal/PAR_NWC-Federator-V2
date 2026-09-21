using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Views;

namespace Federator.Core.Clash
{
    /// <summary>
    /// What the penetration rule decided across one group, and the block that says so.
    ///
    /// IT READS AS THE SETS BLOCK READS: one line per clash moved, then a blank, then the
    /// totals with one line per reason. Four blocks in this log already read that way and
    /// a fifth reading differently is how a log stops being scanned.
    ///
    /// EVERY CLASH LEFT ALONE IS COUNTED BY REASON and none of them is folded into a
    /// leftover. A run that moved ninety statuses has to be readable as ninety, and a run
    /// that moved none has to say WHY none, because those are two different answers and
    /// the second one is the one somebody will question.
    /// </summary>
    public sealed class PenetrationTally
    {
        /// <summary>
        /// How a service size is SHOWN, which is one decimal at most. It is not how it is
        /// COMPARED: `PenetrationRule` reads the full value off
        /// `PenetrationSide.LargestMillimetres` and never reads this string, so rounding
        /// here cannot move a clash. A test pins exactly that.
        /// </summary>
        public const string ShownSizeFormat = "0.#";

        private readonly List<string> moved = new List<string>();

        // Q71. Which services this tool could not measure, by category, and one row per
        // clash for the machine readable log. A count on its own says nothing about what
        // they are, and 5s found all 29 of one group carried a size the reader dropped.
        private readonly Dictionary<string, int> unmeasured = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<string> unmeasuredClashes = new List<string>();
        private readonly Dictionary<PenetrationVerdict, int> counts =
            new Dictionary<PenetrationVerdict, int>();

        /// <summary>How many clashes were looked at at all.</summary>
        public int Considered { get; private set; }

        /// <summary>
        /// Records one clash. The test name and the clash name are what the log shows, so
        /// they are the document's own and never a number this tool made up.
        /// </summary>
        public void Add(string testName, string clashName, PenetrationDecision decision)
        {
            if (decision == null)
            {
                throw new ArgumentNullException("decision");
            }

            Considered++;

            if (!counts.ContainsKey(decision.Verdict))
            {
                counts[decision.Verdict] = 0;
            }

            counts[decision.Verdict]++;

            // Q71 answered b on 2026-09-20. A count of services this tool could not
            // measure says nothing about WHICH they are, and 5s found that every one of
            // 29 of them carried a size the reader was throwing away. So the categories
            // are kept, and the clash names go in the machine readable log, which is
            // what turns the next count of this kind into something a person can check.
            if (decision.Verdict == PenetrationVerdict.SizeUnknown)
            {
                string category = decision.Service == null ? string.Empty : Words(decision.Service.Category);

                if (!unmeasured.ContainsKey(category))
                {
                    unmeasured[category] = 0;
                }

                unmeasured[category]++;
                unmeasuredClashes.Add(Words(testName) + "  " + Words(clashName) + "  [" + category + "]");
            }

            if (!decision.Moves)
            {
                return;
            }

            moved.Add(Line(testName, clashName, decision));
        }

        /// <summary>How many clashes this group moved to Reviewed.</summary>
        public int MovedCount
        {
            get { return Of(PenetrationVerdict.Reviewed); }
        }

        /// <summary>How many carried that verdict.</summary>
        public int Of(PenetrationVerdict verdict)
        {
            return counts.ContainsKey(verdict) ? counts[verdict] : 0;
        }

        /// <summary>Every clash moved, in the order they were decided.</summary>
        public ReadOnlyCollection<string> MovedLines
        {
            get { return new ReadOnlyCollection<string>(moved); }
        }

        /// <summary>
        /// The PENETRATION block. The clashes moved first, because those are what changed
        /// in the file, then the totals and one line per reason nothing changed.
        /// </summary>
        public IList<string> Lines(PenetrationSettings settings, SizeSettings sizes)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (sizes == null)
            {
                throw new ArgumentNullException("sizes");
            }

            List<string> lines = new List<string>();

            for (int i = 0; i < moved.Count; i++)
            {
                lines.Add(moved[i]);
            }

            lines.Add(string.Empty);
            lines.Add("clashes looked at : " + Considered);
            lines.Add("moved to Reviewed : " + MovedCount);

            // EVERY REASON, including the ones at zero, because a reason missing from the
            // block reads as a reason nobody thought of rather than one that did not come
            // up. The SETS block hides a zero and this one does not, and that is the
            // difference between a count of things made and a count of things NOT done.
            PenetrationVerdict[] order = PenetrationRule.InOrder();

            for (int i = 0; i < order.Length; i++)
            {
                if (order[i] == PenetrationVerdict.Reviewed)
                {
                    continue;
                }

                lines.Add("    " + Of(order[i]).ToString().PadLeft(5) + "  "
                    + PenetrationRule.Describe(order[i]));

                // Q71 answered b. The unmeasured services are NAMED and not just counted,
                // straight under their own reason line where a reader is already looking.
                // Two lines at most: what they are, and where the rest of it is.
                if (order[i] == PenetrationVerdict.SizeUnknown && unmeasured.Count > 0)
                {
                    lines.Add("           they are " + ByCategory()
                        + ". Every one is left at the status it had, because a service this"
                        + " tool cannot measure is one a person looks at");
                    lines.Add("           each is a row in the machine readable log beside this one,"
                        + " with its test and its clash name, so the next count of these can be checked");
                }
            }

            lines.Add("the size in use   : "
                + sizes.ThresholdMillimetres.ToString("0.###", CultureInfo.InvariantCulture)
                + "mm or under is a penetration, over it stays as it was");

            if (MovedCount == 0)
            {
                lines.Add("Nothing moved. Every clash is exactly as it was.");
            }

            return lines;
        }

        /// <summary>
        /// The unmeasured services by category, most first, as one phrase. Never
        /// truncated, because a category list is short and the whole point of Q71 is
        /// knowing WHAT they are.
        /// </summary>
        private string ByCategory()
        {
            List<string> names = new List<string>(unmeasured.Keys);

            names.Sort(delegate(string a, string b)
            {
                int byCount = unmeasured[b].CompareTo(unmeasured[a]);
                return byCount != 0 ? byCount : string.Compare(a, b, StringComparison.Ordinal);
            });

            List<string> said = new List<string>();

            for (int i = 0; i < names.Count; i++)
            {
                said.Add(unmeasured[names[i]].ToString(CultureInfo.InvariantCulture)
                    + " " + (names[i].Length == 0 ? "with no category this tool could read" : names[i]));
            }

            return string.Join(", ", said.ToArray());
        }

        /// <summary>
        /// One row per service this tool could not measure, for the machine readable log,
        /// because `Block` writes none and a count nobody can check is a count nobody
        /// should trust. Empty where every service was measured.
        /// </summary>
        public ReadOnlyCollection<string> UnmeasuredRows
        {
            get { return new ReadOnlyCollection<string>(unmeasuredClashes); }
        }

        /// <summary>How many services this tool could not measure, across this group.</summary>
        public int UnmeasuredCount
        {
            get { return Of(PenetrationVerdict.SizeUnknown); }
        }

        /// <summary>
        /// The one line the RESULT block carries for the whole run, or null where nothing
        /// was asked for. Null rather than a line reading zero, because a run that never
        /// turned the box on has nothing to say about it.
        /// </summary>
        public static string ResultLine(bool wanted, int movedAcrossTheRun)
        {
            if (!wanted)
            {
                return null;
            }

            return "penetrations   : " + movedAcrossTheRun
                + (movedAcrossTheRun == 1 ? " clash moved to Reviewed" : " clashes moved to Reviewed");
        }

        /// <summary>
        /// One clash moved. It names the test, the clash, BOTH categories and the size,
        /// because a person auditing this a month later has to be able to tell whether
        /// the rule picked the right thing without opening the model.
        /// </summary>
        private static string Line(string testName, string clashName, PenetrationDecision decision)
        {
            string service = decision.Service == null
                ? "UNKNOWN"
                : Words(decision.Service.Category);
            string solid = decision.Solid == null
                ? "UNKNOWN"
                : Words(decision.Solid.Category);
            // THE DISPLAYED NUMBER IS FOR A PERSON AND THE COMPARED NUMBER IS FOR THE RULE,
            // AND THEY DO NOT HAVE TO BE THE SAME NUMBER. The fixture of 2026-09-21 read
            // real pipes as 20.997mm, 41.667mm and 82.021mm, which is 21, 41.67 and 82
            // arriving through a unit conversion and printed at full precision. Nobody
            // models a 20.997 mm pipe, so a clash report handed to a client showing that
            // reads as a tool that does not understand units. The rule is unaffected,
            // because PenetrationRule compares Service.LargestMillimetres itself and never
            // this string, and a test pins that so rounding can never change a decision.
            string size = decision.Service == null || !decision.Service.LargestMillimetres.HasValue
                ? "UNKNOWN"
                : decision.Service.LargestMillimetres.Value
                    .ToString(ShownSizeFormat, CultureInfo.InvariantCulture) + "mm";

            // The shape of the line is ReviewedLine's, because the by design rule writes
            // one too and one rule lives in one place. Only the WHY is this rule's.
            return ReviewedLine.For(clashName, testName,
                service + " " + size + " through " + solid);
        }

        private static string Words(string value)
        {
            return string.IsNullOrEmpty(value) ? "UNKNOWN" : value;
        }
    }
}
