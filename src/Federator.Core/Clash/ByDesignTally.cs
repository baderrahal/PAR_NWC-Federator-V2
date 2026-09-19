using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Clash
{
    /// <summary>Why one clash was or was not moved by the by design rule, F72b.</summary>
    public enum ByDesignVerdict
    {
        /// <summary>Moved to Reviewed. The two sets are a pair in the file.</summary>
        Reviewed = 0,

        /// <summary>The two sets are not a pair in the file, which is the ordinary case.</summary>
        NotAPair = 1,

        /// <summary>
        /// At Reviewed, Approved or Resolved already. Somebody's decision, never
        /// overwritten, which is the same guard the penetration rule runs through.
        /// </summary>
        SomebodyDecided = 2,

        /// <summary>
        /// The penetration rule already asked for this one. Counted here and moved there,
        /// so the two blocks add up to the number of clashes that moved rather than to
        /// twice it.
        /// </summary>
        ThePenetrationRuleHasIt = 3,

        /// <summary>A side of the test named no set, so there was no pair to look for.</summary>
        NoSetName = 4
    }

    /// <summary>
    /// What the by design rule decided across one group, and the block that says so, F72b.
    ///
    /// It reads the way the PENETRATION block reads: one line per clash moved, then a
    /// blank, then the totals with one line per reason, including the reasons at zero,
    /// because a reason missing from a block reads as one nobody thought of.
    ///
    /// A CLASH CAN QUALIFY UNDER BOTH RULES AND THE PENETRATION RULE OWNS IT. A 100 mm
    /// pipe through a wall whose two sets are also a pair in the file is moved once, by
    /// the penetration rule, and counted here under ThePenetrationRuleHasIt. Counting it
    /// as moved in both blocks would make the RESULT block report more moves than there
    /// were clashes.
    /// </summary>
    public sealed class ByDesignTally
    {
        private readonly List<string> moved = new List<string>();
        private readonly Dictionary<ByDesignVerdict, int> counts =
            new Dictionary<ByDesignVerdict, int>();
        private readonly HashSet<string> pairsSeen = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>The four in the order the block lists them.</summary>
        public static ByDesignVerdict[] InOrder()
        {
            return new[]
            {
                ByDesignVerdict.Reviewed,
                ByDesignVerdict.NotAPair,
                ByDesignVerdict.SomebodyDecided,
                ByDesignVerdict.ThePenetrationRuleHasIt,
                ByDesignVerdict.NoSetName
            };
        }

        /// <summary>The words for one reason, so the block and nothing else spells them.</summary>
        public static string Describe(ByDesignVerdict verdict)
        {
            switch (verdict)
            {
                case ByDesignVerdict.Reviewed:
                    return "moved to Reviewed, the two sets are a pair in the file";
                case ByDesignVerdict.NotAPair:
                    return "the two sets are not a pair in the file";
                case ByDesignVerdict.SomebodyDecided:
                    return "already Reviewed, Approved or Resolved, so somebody decided and it stands";
                case ByDesignVerdict.ThePenetrationRuleHasIt:
                    return "the penetration rule already asked for this one, so it moved there";
                case ByDesignVerdict.NoSetName:
                    return "a side of the test named no set, so there was no pair to look for";
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>How many clashes were looked at at all.</summary>
        public int Considered { get; private set; }

        /// <summary>How many carried that verdict.</summary>
        public int Of(ByDesignVerdict verdict)
        {
            return counts.ContainsKey(verdict) ? counts[verdict] : 0;
        }

        /// <summary>How many clashes this group moved to Reviewed under this rule.</summary>
        public int MovedCount
        {
            get { return Of(ByDesignVerdict.Reviewed); }
        }

        /// <summary>Every clash moved, in the order they were decided.</summary>
        public ReadOnlyCollection<string> MovedLines
        {
            get { return new ReadOnlyCollection<string>(moved); }
        }

        /// <summary>
        /// The pairs this group actually met, so the pairs that matched nothing can be
        /// worked out across the whole run rather than per group. A pair naming sets that
        /// are only in one building would otherwise be reported missing by six groups out
        /// of seven.
        /// </summary>
        public IList<string> PairsSeen
        {
            get { return new List<string>(pairsSeen); }
        }

        /// <summary>
        /// Records one clash. The pair is null where the two sets are not one, and that is
        /// the ordinary case rather than a fault.
        /// </summary>
        public void Add(string testName, string clashName, ByDesignVerdict verdict, ByDesignPair pair)
        {
            Considered++;

            if (!counts.ContainsKey(verdict))
            {
                counts[verdict] = 0;
            }

            counts[verdict]++;

            if (pair != null)
            {
                pairsSeen.Add(pair.Key);
            }

            if (verdict != ByDesignVerdict.Reviewed)
            {
                return;
            }

            moved.Add(ReviewedLine.For(clashName, testName,
                pair == null ? "a pair in the file" : pair.Left + " and " + pair.Right
                    + ", " + pair.Reason));
        }

        /// <summary>The BY DESIGN block for one group.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < moved.Count; i++)
            {
                lines.Add(moved[i]);
            }

            lines.Add(string.Empty);
            lines.Add("clashes looked at : " + Considered);
            lines.Add("moved to Reviewed : " + MovedCount);

            foreach (ByDesignVerdict verdict in InOrder())
            {
                if (verdict == ByDesignVerdict.Reviewed)
                {
                    continue;
                }

                lines.Add("    " + Of(verdict).ToString().PadLeft(5) + "  " + Describe(verdict));
            }

            if (MovedCount == 0)
            {
                lines.Add("Nothing moved. Every clash is exactly as it was.");
            }

            return lines;
        }

        /// <summary>Rolls one group's counts into a run total.</summary>
        public void Add(ByDesignTally other)
        {
            if (other == null)
            {
                return;
            }

            Considered += other.Considered;

            foreach (ByDesignVerdict verdict in InOrder())
            {
                if (!counts.ContainsKey(verdict))
                {
                    counts[verdict] = 0;
                }

                counts[verdict] = counts[verdict] + other.Of(verdict);
            }

            foreach (string key in other.pairsSeen)
            {
                pairsSeen.Add(key);
            }

            moved.AddRange(other.moved);
        }

        /// <summary>
        /// The one line the brief asks for: how many this rule set, and how many pairs in
        /// the file matched no test anywhere in this run. Both are counted and neither is
        /// worked out from the other, and a pair matching nothing is a FINDING and never
        /// an action. Bader decides.
        /// </summary>
        public static string RunLine(int set, int pairsMatchingNothing)
        {
            return ReviewedLine.Prefix + " rule B " + set + " set, " + pairsMatchingNothing
                + (pairsMatchingNothing == 1 ? " pair" : " pairs")
                + " in the file matched no test in this run";
        }

        /// <summary>
        /// The one RESULT line, or null where the box was never ticked. Null rather than a
        /// line reading zero, the same as the penetration line, because a run that never
        /// turned the box on has nothing to say about it.
        /// </summary>
        public static string ResultLine(bool wanted, int movedAcrossTheRun)
        {
            if (!wanted)
            {
                return null;
            }

            return "by design      : " + movedAcrossTheRun
                + (movedAcrossTheRun == 1 ? " clash moved to Reviewed" : " clashes moved to Reviewed");
        }
    }
}
