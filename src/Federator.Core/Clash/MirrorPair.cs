using System;
using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Two tests that ask the same question, F132: the same two sets swapped, Bader's Q114
    /// point 4, two sets that ask the same whole question, his answer B to Q121, or, with no
    /// XML, a saved test whose name ends with the ending and the one saved test without it
    /// whose question its sides ask as a mirror by the same rule. Both are run, his
    /// answer D to Q133, and their clashes are merged by the pair of items into the one kept,
    /// Report.MirrorMerge, so the report, the views and every count hold each clash once, only
    /// where MirrorRule says it Merges, Bader's answer A to Q142. A swap can find more than the test
    /// it mirrors: probe P1 measured 27 clashes on the swap of a test that found 25, all 25
    /// among them, docs\history\scan.md 5z-k on the branch fix-F114-probes. Built by
    /// MirrorRule and nothing else.
    /// </summary>
    public sealed class MirrorPair
    {
        private readonly ClashPriority keptPriority;
        private readonly ClashPriority mirrorPriority;
        private readonly string alikeSets;
        private readonly bool sidesSwapped;
        private bool heldUnderItsRunName;
        private bool renamedToItsRunName;

        internal MirrorPair(
            PlannedClashTest kept,
            ClashPriority keptPriority,
            PlannedClashTest mirror,
            ClashPriority mirrorPriority,
            MirrorKind kind,
            string alikeSets,
            string mirrorName,
            bool sidesSwapped)
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
            this.sidesSwapped = sidesSwapped;
        }

        /// <summary>The test kept, the higher priority, and where equal the first in the XML.</summary>
        public PlannedClashTest Kept { get; private set; }

        /// <summary>The mirror as the XML, or the document where no XML was picked, names it.</summary>
        public PlannedClashTest Mirror { get; private set; }

        public MirrorKind Kind { get; private set; }

        /// <summary>
        /// The name the mirror is created and run under: its own name with the ending, never
        /// the kept test's, MirrorSettings.NameFor, numbered before the ending where that name
        /// is taken by another test of the XML or an earlier mirror. A test the document holds
        /// under it is found by it and run as the mirror, keeping its own clashes, F132 attempt
        /// 12. A test read off the document with no XML keeps its saved name. A test saved
        /// before the mirror rule under the XML's name of a mirror is renamed to this name,
        /// Q136 A, MirrorRule.Renames.
        /// </summary>
        public string MirrorName { get; private set; }

        /// <summary>
        /// Whether the mirror's clashes are merged into the kept test's, Report.MirrorMerge.
        /// False until MirrorRule.JudgeTheMerges, the one rule of Bader's answer A to Q142, says
        /// so, F132 attempt 13, so a pair no judge reached merges nothing: only where this run
        /// created both tests of the pair and every set of both from the picked XML, and for
        /// every mirror of one kept test alike. Otherwise each keeps its own clashes under its
        /// own name.
        /// </summary>
        internal bool Merges { get; private set; }

        /// <summary>The mirror's clashes are merged into the kept test's, said by MirrorRule.JudgeTheMerges alone.</summary>
        internal void JudgedToMerge()
        {
            Merges = true;
        }

        /// <summary>
        /// How the mirror comes to run under its run name on an XML run, read by
        /// MirrorRule.JudgeTheMerges off the document, F132 attempt 13: the document already
        /// holds a test of that name, which is found by it and run as it is saved, or a test
        /// the document holds is renamed to it, Q136 A. Either way this run did not create it,
        /// and the pair line and the coverage words say so, never that it was created.
        /// </summary>
        internal void RunNameInTheDocument(bool held, bool renamed)
        {
            heldUnderItsRunName = held;
            renamedToItsRunName = renamed;
        }

        /// <summary>How the two tests ask the same question, in the words of the log and the coverage sheet.</summary>
        internal string How()
        {
            string how = alikeSets.Length == 0
                ? "the same two sets swapped"
                : "its sets carry the same rule lists as " + Kept.Name + "'s, " + alikeSets;

            return Kind == MirrorKind.Named ? "named a mirror by the ending, " + how : how;
        }

        /// <summary>
        /// The pair's MIRROR line. Where the two differ in priority or in a setting TestDrift
        /// compares, both values are named, the kept test's first, because Bader asked for
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
            return "a mirror of " + Kept.Name + ", " + How() + ", " + RunAs() + (Merges
                ? ", its clashes to be merged into " + Kept.Name + "'s once both run"
                : ", its clashes kept under its own name");
        }

        /// <summary>
        /// What is done with the mirror, said as it is: a saved test runs as it is saved, a
        /// test the document holds under the run name or is renamed to it runs as it is saved
        /// under that name, and only otherwise is the mirror created, F132 attempt 13.
        /// </summary>
        private string RunAs()
        {
            if (Mirror.IsFromDocument)
            {
                return "run as it is saved";
            }

            if (heldUnderItsRunName)
            {
                return "run as it is saved, the document already holding a test named " + MirrorName;
            }

            if (renamedToItsRunName)
            {
                return "run as it is saved, a test the document holds renamed " + MirrorName;
            }

            return "created and run as " + MirrorName;
        }

        /// <summary>Whether the two carry a different priority or a different setting.</summary>
        internal bool Differs()
        {
            return Differences().Count > 0;
        }

        /// <summary>
        /// The priority off the priority file, then every setting the DRIFT block compares,
        /// by its one comparison, TestDrift.Compare, and never a second copy of it: the
        /// tolerance within TestDrift's epsilon, the test type, merge composites, and each
        /// side's self intersect and primitive types, the mirror's side set against the side
        /// of the kept test it stands for. A pair read off the document has no XML name to read
        /// a priority by, so its priority is not compared. Its sides were read and ask the kept
        /// test's question, so they are compared side for side as every other pair's.
        /// </summary>
        private IList<string> Differences()
        {
            List<string> found = new List<string>();

            if (Kind != MirrorKind.Named && keptPriority != mirrorPriority)
            {
                found.Add("priority " + Priorities.Words(keptPriority) + " and " + Priorities.Words(mirrorPriority));
            }

            found.AddRange(SettingsDiffer(Kept, Mirror, sidesSwapped));
            return found;
        }

        /// <summary>
        /// Every setting TestDrift.Compare reads where two tests of one question differ, each
        /// with the first test's value then the other's, the other's sides set against the
        /// first's they stand for, swapped where sidesSwapped. The sets are the same question
        /// by the rule that judged the two, so only the flags of each side are compared, never
        /// the set names.
        /// </summary>
        private static IList<string> SettingsDiffer(PlannedClashTest first, PlannedClashTest other, bool sidesSwapped)
        {
            TestSettings firstSettings = TestSettings.FromFile(first);
            TestSettings otherSettings = SideForSide(TestSettings.FromFile(other), sidesSwapped);
            List<string> found = new List<string>();

            otherSettings.LeftLocator = firstSettings.LeftLocator;
            otherSettings.RightLocator = firstSettings.RightLocator;

            foreach (TestDifference difference in TestDrift.Compare(first.Name, firstSettings, otherSettings))
            {
                found.Add(difference.Field + " " + Valued(difference, difference.InFile, first) + " and "
                    + Valued(difference, difference.InDocument, other));
            }

            return found;
        }

        /// <summary>The other test's settings with its sides in the first test's order.</summary>
        private static TestSettings SideForSide(TestSettings other, bool sidesSwapped)
        {
            if (!sidesSwapped)
            {
                return other;
            }

            bool leftSelf = other.LeftSelfIntersect;
            int leftPrimitives = other.LeftPrimitiveTypes;

            other.LeftSelfIntersect = other.RightSelfIntersect;
            other.LeftPrimitiveTypes = other.RightPrimitiveTypes;
            other.RightSelfIntersect = leftSelf;
            other.RightPrimitiveTypes = leftPrimitives;
            return other;
        }

        /// <summary>A tolerance carries its test's units, because Bader reads the two side by side.</summary>
        private static string Valued(TestDifference difference, string value, PlannedClashTest test)
        {
            return string.Equals(difference.Field, TestDrift.ToleranceField, StringComparison.Ordinal)
                ? value + " " + test.DocumentUnits
                : value;
        }
    }
}
