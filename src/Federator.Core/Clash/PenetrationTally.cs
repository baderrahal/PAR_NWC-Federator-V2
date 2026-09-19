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
        private readonly List<string> moved = new List<string>();
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
            string size = decision.Service == null || !decision.Service.LargestMillimetres.HasValue
                ? "UNKNOWN"
                : decision.Service.LargestMillimetres.Value
                    .ToString("0.###", CultureInfo.InvariantCulture) + "mm";

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
