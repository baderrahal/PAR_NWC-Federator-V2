using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Diagnostics;
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
        /// hard_conservative has been measured in a real file, see .claude\rules\core.md.
        /// The other
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

        /// <summary>The clash matrix, or the tests already saved in the document.</summary>
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

        /// <summary>
        /// The first CLASH line of a group, FR-058. It is written before the sets are counted,
        /// so for a clash matrix it cannot know how many tests are created: set 03 read 1830 to
        /// create here and 36 created eight lines on, two counts of one thing. It says how many
        /// passed the file's own checks and where the count of those created is said, the line
        /// of Federator.Core.Clash.CreationPlan. With no file the saved tests are the plan and
        /// every one is run where it sits, so that count is the one to say.
        /// </summary>
        public string PlanLine(string group)
        {
            string name = string.IsNullOrEmpty(group) ? "UNKNOWN" : group;

            if (Source == ClashPlanSource.Document)
            {
                return "CLASH    " + name + ", " + TestsInFile + " saved in the document, "
                    + buildable.Count + " to run, " + skipped.Count + " skipped before the model";
            }

            return "CLASH    " + name + ", " + TestsInFile + " in the file, " + skipped.Count
                + " skipped before the model, " + buildable.Count
                + " left, how many of them are created is said once the sets are counted";
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
        /// for a run with no clash matrix read. Every saved test goes in, by its address, in the
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

            // F167. A test with no tolerance, or a file with no units, is planned like any other. The
            // tolerance is the drop down's on every test.
            // FR-155. A setting left out read as 0 or false, which is a setting nobody wrote,
            // so the test is skipped by name.
            if (test.MissingSettings.Count > 0)
            {
                skipped.Add(new SkippedClashTest(
                    test.Name,
                    ClashSkipReason.MissingSetting,
                    "the file gives this test no " + string.Join(", ", new List<string>(test.MissingSettings).ToArray())
                        + ", and a setting is never supplied from anywhere else",
                    fileIndex));
                return;
            }

            buildable.Add(new PlannedClashTest(
                test.Name,
                kind,
                test.TestType,
                test.MergeComposites,
                Side(test.Left),
                Side(test.Right),
                fileIndex));
        }

        /// <summary>
        /// Why this test cannot be imported, or empty when it can. A side with no locator
        /// is the case the rule in .claude\rules\core.md refuses.
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
        public ClashTestPlan WithTheSavedTestsTheMatrixDoesNotName(ClashTestPlan inTheDocument, out IList<string> added)
        {
            if (inTheDocument == null)
            {
                throw new ArgumentNullException("inTheDocument");
            }

            List<string> addedNames = new List<string>();
            HashSet<string> named = new HashSet<string>(StringComparer.Ordinal);

            foreach (PlannedClashTest test in buildable)
            {
                named.Add(test.Name);
            }

            foreach (SkippedClashTest test in skipped)
            {
                named.Add(test.Name);
            }

            List<PlannedClashTest> allBuildable = new List<PlannedClashTest>(buildable);
            List<SkippedClashTest> allSkipped = new List<SkippedClashTest>(skipped);
            List<string> allUnknown = new List<string>(unknownTestTypes);
            int index = TestsInFile;

            // The tests saved in the document that the matrix names are the matrix's, run where they
            // sit by name. Only the ones it does not name are added, after its own, in the order the
            // document holds them, each once, each run where it sits and none created or compared.
            foreach (PlannedClashTest test in inTheDocument.buildable)
            {
                if (!string.IsNullOrEmpty(test.Name) && named.Add(test.Name))
                {
                    allBuildable.Add(test.AtFileIndex(index++));
                    addedNames.Add(test.Name);
                }
            }

            foreach (SkippedClashTest test in inTheDocument.skipped)
            {
                if (!string.IsNullOrEmpty(test.Name) && test.Name != "UNKNOWN" && named.Add(test.Name))
                {
                    allSkipped.Add(new SkippedClashTest(test.Name, test.Kind, test.Reason, index++));
                }
            }

            foreach (string unknown in inTheDocument.unknownTestTypes)
            {
                if (!allUnknown.Contains(unknown))
                {
                    allUnknown.Add(unknown);
                }
            }

            added = addedNames;
            return new ClashTestPlan(Source, TestsInFile, DocumentUnits, allBuildable, allSkipped, allUnknown);
        }

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

        /// <summary>
        /// The same plan with every mirror the rule found under the name it is created and
        /// run under, F132, Bader's answer D to Q133, so both tests of a pair are created and
        /// run and the mirror stays in Clash Detective with its name ending as the setting
        /// says. A mirror read off the document keeps its name and its address. The rule is
        /// the one built over this plan's own buildable tests, and a test is matched as the
        /// same object. A rule built over another list would rename nothing while its lines
        /// named each mirror by its new name, so a mirror that is not one of this plan's
        /// buildable tests is refused.
        /// </summary>
        public ClashTestPlan WithMirrorsNamed(MirrorRule mirrors)
        {
            if (mirrors == null)
            {
                throw new ArgumentNullException("mirrors");
            }

            HashSet<PlannedClashTest> ours = new HashSet<PlannedClashTest>(buildable);
            Dictionary<PlannedClashTest, string> byMirror = new Dictionary<PlannedClashTest, string>();

            foreach (MirrorPair pair in mirrors.Pairs)
            {
                if (!ours.Contains(pair.Mirror))
                {
                    throw new ArgumentException(
                        "The mirror rule was built over other tests than this plan's, so it would rename nothing "
                            + "while its lines name each mirror by its new name. " + pair.Mirror.Name
                            + " is not one of them.",
                        "mirrors");
                }

                byMirror[pair.Mirror] = pair.MirrorName;
            }

            List<PlannedClashTest> named = new List<PlannedClashTest>();

            foreach (PlannedClashTest test in buildable)
            {
                string name;

                named.Add(byMirror.TryGetValue(test, out name) && !string.Equals(name, test.Name, StringComparison.Ordinal)
                    ? test.Named(name)
                    : test);
            }

            return new ClashTestPlan(
                Source, TestsInFile, DocumentUnits, named, new List<SkippedClashTest>(skipped), unknownTestTypes);
        }

        /// <summary>
        /// F224, Bader's answer to Q209: the same plan with every test whose two sides carry one set code moved to the skipped,
        /// SameSetCode with the code named, by SetCodeRule, so it is neither created nor run, and how many each code lost. Made
        /// before the mirrors are paired, so no mirror is paired with a test the rule leaves out. A test saved in the NWF that
        /// the rule leaves out stays in the NWF with its results, since this changes the plan and never the document.
        /// SINCE F247, Bader's answer Q230 of 20 Oct 2026, only where that one code is among the codes the pack's rules leave
        /// out, PackRules.SameCodeLeftOut, AR and ST in 1104-PAR, so a test of one set code of any other code runs.
        /// </summary>
        public ClashTestPlan AcrossTwoSetCodes(IEnumerable<string> knownCodes, IEnumerable<string> leftOutCodes, char separator,
            out IDictionary<string, int> leftOutByCode)
        {
            List<string> known = new List<string>(knownCodes ?? new string[0]);
            List<PlannedClashTest> kept = new List<PlannedClashTest>();
            List<SkippedClashTest> left = new List<SkippedClashTest>(skipped);
            Dictionary<string, int> byCode = new Dictionary<string, int>(StringComparer.Ordinal);
            HashSet<string> leftOut = new HashSet<string>(leftOutCodes ?? new string[0], StringComparer.Ordinal);

            foreach (PlannedClashTest test in buildable)
            {
                string code = SetCodeRule.SameCodeOf(test, known, separator);

                if (code == null || !leftOut.Contains(code))
                {
                    kept.Add(test);
                    continue;
                }

                int seen;
                byCode[code] = byCode.TryGetValue(code, out seen) ? seen + 1 : 1;
                left.Add(new SkippedClashTest(test.Name, ClashSkipReason.SameSetCode, SetCodeRule.Reason(code), test.FileIndex));
            }

            leftOutByCode = byCode;
            return new ClashTestPlan(Source, TestsInFile, DocumentUnits, kept, left, unknownTestTypes);
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

                case ClashSkipReason.MissingSetting:
                    return "the file leaves out a setting of it";
                case ClashSkipReason.NoName:
                    return "no name";
                case ClashSkipReason.LocatorNotResolved:
                    return "a set is not in the document";
                case ClashSkipReason.EmptySide:
                    return "a side finds nothing in this model";
                case ClashSkipReason.SingleDiscipline:
                    return "the group holds one discipline, so nothing in it can clash";
                case ClashSkipReason.SameSetCode:
                    return "both sides carry one set code, a code whose tests against itself the project's rules leave out";

                case ClashSkipReason.Failed:
                    return "creating or running it threw";
                default:
                    return "UNKNOWN";
            }
        }
    }
}
