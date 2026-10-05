using System;
using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Two tests that ask the same question, F132: the same two sets swapped, Bader's Q114
    /// point 4, two sets that ask the same whole question, his answer B to Q121, or, with no
    /// XML, a saved test and the saved test of its name with the ending. Both are run, his
    /// answer D to Q133, and their clashes are merged by the pair of items into the one kept,
    /// Report.MirrorMerge, so the report, the views and every count hold each clash once. A
    /// swap can find more than the test it mirrors: probe P1 measured 27 clashes on the swap
    /// of a test that found 25, all 25 among them, docs\history\scan.md 5z-k on the branch
    /// fix-F114-probes. Built by MirrorRule and nothing else.
    /// </summary>
    public sealed class MirrorPair
    {
        private readonly ClashPriority keptPriority;
        private readonly ClashPriority mirrorPriority;
        private readonly string alikeSets;
        private readonly bool sidesSwapped;

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
        /// The name the mirror is created and run under: the kept test's name with the ending,
        /// MirrorSettings.NameFor, numbered before the ending where that name is taken. A test
        /// read off the document keeps its saved name, because this tool renames no test it
        /// did not create.
        /// </summary>
        public string MirrorName { get; private set; }

        /// <summary>How the two tests ask the same question, in the words of the log and the coverage sheet.</summary>
        internal string How()
        {
            if (Kind == MirrorKind.Named)
            {
                return "named as its mirror by the ending";
            }

            return Kind == MirrorKind.Swapped
                ? "the same two sets swapped"
                : "its sets carry the same rule lists as " + Kept.Name + "'s, " + alikeSets;
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
            return "a mirror of " + Kept.Name + ", " + How() + ", " + RunAs() + ", its clashes merged into "
                + Kept.Name + "'s";
        }

        private string RunAs()
        {
            return Mirror.IsFromDocument ? "run as it is saved" : "created and run as " + MirrorName;
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
        /// of the kept test it stands for. A pair found by name, read off the document, has no
        /// XML name to read a priority by and no set to tell which side stands for which, so
        /// for it only the tolerance, the type and merge composites are compared.
        /// </summary>
        private IList<string> Differences()
        {
            List<string> found = new List<string>();

            if (Kind != MirrorKind.Named && keptPriority != mirrorPriority)
            {
                found.Add("priority " + Priorities.Words(keptPriority) + " and " + Priorities.Words(mirrorPriority));
            }

            TestSettings kept = TestSettings.FromFile(Kept);
            TestSettings mirror = SideForSide(TestSettings.FromFile(Mirror));

            if (Kind == MirrorKind.Named)
            {
                kept.LeftLocator = kept.RightLocator = TestSettings.UnknownLocator;
                mirror.LeftLocator = mirror.RightLocator = TestSettings.UnknownLocator;
            }
            else
            {
                // The sets are the same question by the rule that made the pair, so only the
                // flags of each side are compared, never the set names.
                mirror.LeftLocator = kept.LeftLocator;
                mirror.RightLocator = kept.RightLocator;
            }

            foreach (TestDifference difference in TestDrift.Compare(Kept.Name, kept, mirror))
            {
                found.Add(difference.Field + " " + Valued(difference, difference.InFile, Kept) + " and "
                    + Valued(difference, difference.InDocument, Mirror));
            }

            return found;
        }

        /// <summary>The mirror's settings with its sides in the kept test's order.</summary>
        private TestSettings SideForSide(TestSettings mirror)
        {
            if (!sidesSwapped)
            {
                return mirror;
            }

            bool leftSelf = mirror.LeftSelfIntersect;
            int leftPrimitives = mirror.LeftPrimitiveTypes;

            mirror.LeftSelfIntersect = mirror.RightSelfIntersect;
            mirror.LeftPrimitiveTypes = mirror.RightPrimitiveTypes;
            mirror.RightSelfIntersect = leftSelf;
            mirror.RightPrimitiveTypes = leftPrimitives;
            return mirror;
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
