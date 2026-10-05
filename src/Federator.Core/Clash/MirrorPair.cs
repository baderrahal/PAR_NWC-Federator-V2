using System;
using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Two tests that ask the same question, F132: the same two sets swapped, Bader's Q114
    /// point 4, or two sets whose rule lists are the same, his answer B to Q121. Both are
    /// created and run, his answer D to Q133, and their clashes are merged by the pair of
    /// items into the one kept, Report.MirrorMerge, so the report, the views and every count
    /// hold each clash once. A swap can find more than the test it mirrors: probe P1 measured
    /// 27 clashes on the swap of a test that found 25, all 25 among them, docs\history\scan.md
    /// 5z-k on the branch fix-F114-probes. Built by MirrorRule and nothing else.
    /// </summary>
    public sealed class MirrorPair
    {
        private readonly ClashPriority keptPriority;
        private readonly ClashPriority mirrorPriority;
        private readonly string alikeSets;
        private readonly bool endingTaken;

        internal MirrorPair(
            PlannedClashTest kept,
            ClashPriority keptPriority,
            PlannedClashTest mirror,
            ClashPriority mirrorPriority,
            MirrorKind kind,
            string alikeSets,
            string mirrorName,
            bool endingTaken)
        {
            if (kept == null)
            {
                throw new ArgumentNullException("kept");
            }

            if (mirror == null)
            {
                throw new ArgumentNullException("mirror");
            }

            Kept = kept;
            Mirror = mirror;
            Kind = kind;
            MirrorName = mirrorName;
            this.keptPriority = keptPriority;
            this.mirrorPriority = mirrorPriority;
            this.alikeSets = alikeSets ?? string.Empty;
            this.endingTaken = endingTaken;
        }

        /// <summary>The test kept, the higher priority, and where equal the first in the XML.</summary>
        public PlannedClashTest Kept { get; private set; }

        /// <summary>The mirror as the XML, or the document where no XML was picked, names it.</summary>
        public PlannedClashTest Mirror { get; private set; }

        public MirrorKind Kind { get; private set; }

        /// <summary>
        /// The name the mirror is created and run under: the XML's name with the ending of
        /// MirrorSettings, never twice, and the saved name for a test read off the document,
        /// because this tool renames no test it did not create. Where another test handed to
        /// the rule already carries the name with the ending, the mirror keeps the XML's name,
        /// so no two tests of one name are ever created.
        /// </summary>
        public string MirrorName { get; private set; }

        /// <summary>How the two tests ask the same question, in the words of the log and the coverage sheet.</summary>
        internal string How()
        {
            return Kind == MirrorKind.Swapped
                ? "the same two sets swapped"
                : "its sets carry the same rule lists as " + Kept.Name + "'s, " + alikeSets;
        }

        /// <summary>
        /// The pair's MIRROR line. Where the two carry different priorities, test types or
        /// tolerances, both values are named, the kept test's first, because Bader asked for
        /// both in the log.
        /// </summary>
        internal string Line()
        {
            string line = MirrorRule.Prefix + "   " + Kept.Name + " is kept, " + Mirror.Name + " is its mirror, "
                + How() + ", " + RunAs();

            IList<string> differences = Differences();

            return differences.Count == 0
                ? line
                : line + ". The two differ, the kept test first: "
                    + string.Join(", ", new List<string>(differences).ToArray());
        }

        /// <summary>The words the coverage sheet gives the mirror, for F127.</summary>
        internal string CoverageOfTheMirror()
        {
            return "a mirror of " + Kept.Name + ", " + How() + ", " + RunAs() + ", its clashes merged into "
                + Kept.Name + "'s";
        }

        private string RunAs()
        {
            if (Mirror.IsFromDocument)
            {
                return "run as it is saved";
            }

            if (endingTaken)
            {
                return "created and run under its own name, because another test of the XML is named "
                    + Mirror.Name + " with the ending";
            }

            return "created and run as " + MirrorName;
        }

        /// <summary>Whether the two carry a different priority, type or tolerance.</summary>
        internal bool Differs()
        {
            return Differences().Count > 0;
        }

        private IList<string> Differences()
        {
            List<string> found = new List<string>();

            if (keptPriority != mirrorPriority)
            {
                found.Add("priority " + Priorities.Words(keptPriority) + " and " + Priorities.Words(mirrorPriority));
            }

            if (Kept.TestType != Mirror.TestType)
            {
                found.Add("type " + Kept.TestTypeName + " and " + Mirror.TestTypeName);
            }

            // TestDrift's own epsilon, because a tolerance travels through a unit conversion
            // and an exact comparison would call two equal tolerances different.
            if (Math.Abs(Kept.Tolerance - Mirror.Tolerance) > TestDrift.ToleranceEpsilon)
            {
                found.Add("tolerance " + TestDrift.Number(Kept.Tolerance) + " " + Kept.DocumentUnits
                    + " and " + TestDrift.Number(Mirror.Tolerance) + " " + Mirror.DocumentUnits);
            }

            return found;
        }
    }
}
