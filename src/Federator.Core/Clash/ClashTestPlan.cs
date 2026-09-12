using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Exchange;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Everything in one exchange file worked out before any Navisworks call, so all of it
    /// can be tested without Navisworks. Nothing about any one project is in here. The
    /// names, the count, the tolerance and the units all come from the file that was
    /// picked.
    /// </summary>
    public sealed class ClashTestPlan
    {
        /// <summary>
        /// The test_type strings this tool creates, and what each maps to. Only
        /// hard_conservative has been measured in a real file, see CLAUDE.md. The other
        /// four are the same lower case form and are UNVERIFIED against a real export.
        /// That costs nothing, because a string not in this table is skipped by name
        /// rather than approximated, which is the behaviour that matters.
        /// </summary>
        private static readonly Dictionary<string, ClashTestKind> KnownTestTypes =
            new Dictionary<string, ClashTestKind>(StringComparer.OrdinalIgnoreCase)
            {
                { "hard", ClashTestKind.Hard },
                { "hard_conservative", ClashTestKind.HardConservative },
                { "clearance", ClashTestKind.Clearance },
                { "duplicate", ClashTestKind.Duplicate },
                { "custom", ClashTestKind.Custom }
            };

        private readonly List<PlannedClashTest> buildable;
        private readonly List<SkippedClashTest> skipped;
        private readonly List<string> unknownTestTypes;

        private ClashTestPlan(
            int testsInFile,
            string documentUnits,
            List<PlannedClashTest> buildable,
            List<SkippedClashTest> skipped,
            List<string> unknownTestTypes)
            : this(ClashPlanSource.ExchangeFile, testsInFile, documentUnits, buildable, skipped, unknownTestTypes)
        {
        }

        private ClashTestPlan(
            ClashPlanSource source,
            int testsInFile,
            string documentUnits,
            List<PlannedClashTest> buildable,
            List<SkippedClashTest> skipped,
            List<string> unknownTestTypes)
        {
            Source = source;
            TestsInFile = testsInFile;
            DocumentUnits = documentUnits;
            this.buildable = buildable;
            this.skipped = skipped;
            this.unknownTestTypes = unknownTestTypes;
        }

        /// <summary>The picked file, or the tests already saved in the document.</summary>
        public ClashPlanSource Source { get; private set; }

        /// <summary>
        /// How many clashtest elements the file held, before anything was decided. For a
        /// plan built from the document, how many tests the document holds.
        /// </summary>
        public int TestsInFile { get; private set; }

        /// <summary>The units the open document is in, which every tolerance was converted into.</summary>
        public string DocumentUnits { get; private set; }

        public ReadOnlyCollection<PlannedClashTest> Buildable
        {
            get { return new ReadOnlyCollection<PlannedClashTest>(buildable); }
        }

        public ReadOnlyCollection<SkippedClashTest> Skipped
        {
            get { return new ReadOnlyCollection<SkippedClashTest>(skipped); }
        }

        /// <summary>Every test_type string in the file this tool does not create, once each.</summary>
        public ReadOnlyCollection<string> UnknownTestTypes
        {
            get { return new ReadOnlyCollection<string>(unknownTestTypes); }
        }

        public bool HasWork
        {
            get { return buildable.Count > 0; }
        }

        /// <summary>The test type strings this tool creates, for a message that lists them.</summary>
        public static IList<string> KnownTestTypeNames()
        {
            List<string> names = new List<string>(KnownTestTypes.Keys);
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        public static ClashTestPlan From(ExchangeDocument exchange, string documentUnits)
        {
            if (exchange == null)
            {
                throw new ArgumentNullException("exchange");
            }

            List<PlannedClashTest> buildable = new List<PlannedClashTest>();
            List<SkippedClashTest> skipped = new List<SkippedClashTest>();
            List<string> unknown = new List<string>();
            HashSet<string> seenUnknown = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < exchange.Tests.Count; i++)
            {
                Plan(exchange.Tests[i], i, documentUnits, buildable, skipped, unknown, seenUnknown);
            }

            return new ClashTestPlan(
                exchange.Tests.Count, documentUnits, buildable, skipped, unknown);
        }

        /// <summary>
        /// The second way to build a plan: from the tests already saved in the document,
        /// for a run with no XML picked. Every saved test goes in, by its address, in the
        /// order it was found. Nothing is created and nothing is compared, because there
        /// is no file to create from or to drift from. The sets are not touched either.
        ///
        /// A test with no name is skipped by reason, because it could never be reported
        /// on. A test type number that is not one on the enum is skipped by name, never
        /// approximated, the same rule the file path keeps.
        /// </summary>
        public static ClashTestPlan FromDocument(IList<SavedClashTest> saved, string documentUnits)
        {
            if (saved == null)
            {
                throw new ArgumentNullException("saved");
            }

            List<PlannedClashTest> buildable = new List<PlannedClashTest>();
            List<SkippedClashTest> skipped = new List<SkippedClashTest>();
            List<string> unknown = new List<string>();
            HashSet<string> seenUnknown = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < saved.Count; i++)
            {
                SavedClashTest test = saved[i];

                if (string.IsNullOrEmpty(test.Name))
                {
                    skipped.Add(new SkippedClashTest(
                        "UNKNOWN", ClashSkipReason.NoName, "the saved test carries no name", i));
                    continue;
                }

                if (!Enum.IsDefined(typeof(ClashTestKind), test.TestTypeNumber))
                {
                    string named = "type number " + test.TestTypeNumber.ToString(CultureInfo.InvariantCulture);

                    if (seenUnknown.Add(named))
                    {
                        unknown.Add(named);
                    }

                    skipped.Add(new SkippedClashTest(
                        test.Name,
                        ClashSkipReason.UnknownTestType,
                        "the saved test has " + named + ", which is not one this tool runs",
                        i));
                    continue;
                }

                ClashTestKind kind = (ClashTestKind)test.TestTypeNumber;

                buildable.Add(new PlannedClashTest(
                    test.Name,
                    kind,
                    kind.ToString(),
                    test.Tolerance,
                    documentUnits,
                    test.Tolerance,
                    documentUnits,
                    test.MergeComposites,
                    new PlannedClashSide(test.LeftSelfIntersect, test.LeftPrimitiveTypes, test.LeftLocator),
                    new PlannedClashSide(test.RightSelfIntersect, test.RightPrimitiveTypes, test.RightLocator),
                    i,
                    test.Address));
            }

            return new ClashTestPlan(
                ClashPlanSource.Document, saved.Count, documentUnits, buildable, skipped, unknown);
        }

        private static void Plan(
            ClashTestDefinition test,
            int fileIndex,
            string documentUnits,
            List<PlannedClashTest> buildable,
            List<SkippedClashTest> skipped,
            List<string> unknown,
            HashSet<string> seenUnknown)
        {
            // A test with no name could never be found again, so it could never be
            // reported on and could never be left alone on a rerun.
            if (string.IsNullOrEmpty(test.Name))
            {
                skipped.Add(new SkippedClashTest(
                    "UNKNOWN", ClashSkipReason.NoName, "the test carried no name attribute", fileIndex));
                return;
            }

            ClashTestKind kind;

            if (!KnownTestTypes.TryGetValue(Trimmed(test.TestType), out kind))
            {
                string named = string.IsNullOrEmpty(test.TestType) ? "UNKNOWN" : test.TestType;

                if (seenUnknown.Add(named))
                {
                    unknown.Add(named);
                }

                skipped.Add(new SkippedClashTest(
                    test.Name,
                    ClashSkipReason.UnknownTestType,
                    "test type \"" + named + "\" is not one this tool creates, the ones it creates are "
                        + string.Join(", ", new List<string>(KnownTestTypeNames()).ToArray()),
                    fileIndex));
                return;
            }

            string sideProblem = SideProblem(test);

            if (sideProblem != null)
            {
                skipped.Add(new SkippedClashTest(
                    test.Name, ClashSkipReason.NoLocator, sideProblem, fileIndex));
                return;
            }

            double tolerance;
            string unitsProblem = Convert(test, documentUnits, out tolerance);

            if (unitsProblem != null)
            {
                skipped.Add(new SkippedClashTest(
                    test.Name, ClashSkipReason.UnknownUnits, unitsProblem, fileIndex));
                return;
            }

            buildable.Add(new PlannedClashTest(
                test.Name,
                kind,
                test.TestType,
                test.ToleranceInFileUnits,
                test.FileUnits,
                tolerance,
                documentUnits,
                test.MergeComposites,
                Side(test.Left),
                Side(test.Right),
                fileIndex));
        }

        /// <summary>
        /// Never import a test with an empty side. It returns zero clashes and reads as
        /// passed, which is worse than not being there at all.
        /// </summary>
        private static string SideProblem(ClashTestDefinition test)
        {
            bool leftMissing = test.Left == null || !test.Left.HasLocator;
            bool rightMissing = test.Right == null || !test.Right.HasLocator;

            if (leftMissing && rightMissing)
            {
                return "neither side names a set";
            }

            if (leftMissing)
            {
                return "the left side names no set";
            }

            return rightMissing ? "the right side names no set" : null;
        }

        /// <summary>
        /// The tolerance is written in the file units and the document measures in its
        /// own, so it is converted before it is ever set. There is no global tolerance
        /// setting in this tool, every test carries its own.
        /// </summary>
        private static string Convert(ClashTestDefinition test, string documentUnits, out double tolerance)
        {
            tolerance = 0.0;

            if (string.IsNullOrEmpty(test.FileUnits))
            {
                return "the file gives no units, so the tolerance "
                    + test.ToleranceInFileUnits.ToString("0.##########", CultureInfo.InvariantCulture)
                    + " cannot be converted";
            }

            if (string.IsNullOrEmpty(documentUnits))
            {
                return "the units of the open document are UNKNOWN, so the tolerance cannot be converted";
            }

            if (!ExchangeUnits.IsKnown(test.FileUnits))
            {
                return "the file unit \"" + test.FileUnits + "\" is not one this tool converts";
            }

            if (!ExchangeUnits.IsKnown(documentUnits))
            {
                return "the document unit \"" + documentUnits + "\" is not one this tool converts";
            }

            tolerance = ExchangeUnits.Convert(test.ToleranceInFileUnits, test.FileUnits, documentUnits);
            return null;
        }

        private static PlannedClashSide Side(ClashSideDefinition side)
        {
            return new PlannedClashSide(side.SelfIntersect, side.PrimitiveTypes, side.Locator);
        }

        private static string Trimmed(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        /// <summary>
        /// Moves every test naming a set that is not in the document out of the buildable
        /// list and into the skipped list, reported by name. The comparison is the exact
        /// string, never trimmed, because two set names in the reference file end in a
        /// space and trimming would match the wrong set.
        /// </summary>
        public ClashTestPlan ResolveAgainst(IEnumerable<string> setPaths)
        {
            if (setPaths == null)
            {
                throw new ArgumentNullException("setPaths");
            }

            HashSet<string> known = new HashSet<string>(setPaths, StringComparer.Ordinal);

            List<PlannedClashTest> stillBuildable = new List<PlannedClashTest>();
            List<SkippedClashTest> nowSkipped = new List<SkippedClashTest>(skipped);

            foreach (PlannedClashTest test in buildable)
            {
                bool leftKnown = known.Contains(test.Left.Locator);
                bool rightKnown = known.Contains(test.Right.Locator);

                if (leftKnown && rightKnown)
                {
                    stillBuildable.Add(test);
                    continue;
                }

                nowSkipped.Add(new SkippedClashTest(
                    test.Name,
                    ClashSkipReason.LocatorNotResolved,
                    Unresolved(leftKnown, rightKnown, test),
                    test.FileIndex));
            }

            return new ClashTestPlan(
                Source, TestsInFile, DocumentUnits, stillBuildable, nowSkipped, unknownTestTypes);
        }

        private static string Unresolved(bool leftKnown, bool rightKnown, PlannedClashTest test)
        {
            if (!leftKnown && !rightKnown)
            {
                return "neither set is in the document, left \"" + test.Left.Locator
                    + "\" and right \"" + test.Right.Locator + "\"";
            }

            return leftKnown
                ? "the right set \"" + test.Right.Locator + "\" is not in the document"
                : "the left set \"" + test.Left.Locator + "\" is not in the document";
        }

        /// <summary>Every distinct set path the buildable tests name, in first seen order.</summary>
        public IList<string> DistinctLocators()
        {
            List<string> locators = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (PlannedClashTest test in buildable)
            {
                if (seen.Add(test.Left.Locator))
                {
                    locators.Add(test.Left.Locator);
                }

                if (seen.Add(test.Right.Locator))
                {
                    locators.Add(test.Right.Locator);
                }
            }

            return locators;
        }

        /// <summary>How many were skipped for each reason, for the totals block.</summary>
        public IDictionary<ClashSkipReason, int> SkipReasonCounts()
        {
            return CountReasons(skipped);
        }

        internal static IDictionary<ClashSkipReason, int> CountReasons(IEnumerable<SkippedClashTest> from)
        {
            Dictionary<ClashSkipReason, int> counts = new Dictionary<ClashSkipReason, int>();

            foreach (SkippedClashTest test in from)
            {
                int already;
                counts.TryGetValue(test.Kind, out already);
                counts[test.Kind] = already + 1;
            }

            return counts;
        }

        /// <summary>The words that go in the log for each reason, so a count reads on its own.</summary>
        public static string Describe(ClashSkipReason reason)
        {
            switch (reason)
            {
                case ClashSkipReason.UnknownTestType:
                    return "test type not one this tool creates";
                case ClashSkipReason.NoLocator:
                    return "a side names no set";
                case ClashSkipReason.UnknownUnits:
                    return "the tolerance could not be converted";
                case ClashSkipReason.NoName:
                    return "no name";
                case ClashSkipReason.LocatorNotResolved:
                    return "a set is not in the document";
                case ClashSkipReason.EmptySide:
                    return "a side finds nothing in this model";
                case ClashSkipReason.SingleDiscipline:
                    return "the group holds one discipline, so nothing in it can clash";
                case ClashSkipReason.Failed:
                    return "creating or running it threw";
                default:
                    return "UNKNOWN";
            }
        }
    }
}
