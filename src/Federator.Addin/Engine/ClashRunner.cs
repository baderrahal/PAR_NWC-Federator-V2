using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.DocumentParts;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;
using CoreClashStatus = Federator.Core.Clash.ClashStatus;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Creates the clash tests a picked exchange file describes into whatever document is
    /// open, then runs them. Every Navisworks call here happens on the thread that calls
    /// Run, which is the plugin thread, exactly as the model side and the sets do.
    ///
    /// Nothing about any one project is in here. The names, the count, the test types, the
    /// tolerances and the set paths all come from the file that was picked.
    /// </summary>
    public sealed class ClashRunner
    {
        private readonly Action<string> progress;
        private readonly RunLog log;

        /// <summary>How often the running count goes in the log, so a long run is watchable.</summary>
        public const int ProgressEvery = 25;

        public ClashRunner(Action<string> progress, RunLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.progress = progress ?? delegate { };
            this.log = log;
            SetTreeRoot = ExchangeReader.SelectionSetTreeRoot;
        }

        /// <summary>The prefix every set path is built with. A setting, not a constant.</summary>
        public string SetTreeRoot { get; set; }

        /// <summary>
        /// The units the open document is in, as one of the strings ExchangeUnits knows,
        /// or null when there is no document. Read before the plan is built, because every
        /// tolerance is converted into these units.
        /// </summary>
        public static string DocumentUnits()
        {
            Document document = NavisworksApplication.ActiveDocument;
            return document == null ? null : UnitName(document.Units);
        }

        /// <summary>
        /// Autodesk.Navisworks.Api.Units to the unit strings ExchangeUnits converts with.
        /// The enum values were read off the installed DLL, see docs\scan.md. A value this
        /// tool has not been taught returns null rather than a guessed factor, and every
        /// test is then skipped by name saying so.
        /// </summary>
        public static string UnitName(Units units)
        {
            switch (units)
            {
                case Units.Millimeters:
                    return "mm";
                case Units.Centimeters:
                    return "cm";
                case Units.Meters:
                    return "m";
                case Units.Kilometers:
                    return "km";
                case Units.Inches:
                    return "in";
                case Units.Feet:
                    return "ft";
                case Units.Yards:
                    return "yd";
                case Units.Miles:
                    return "mi";
                case Units.Micrometers:
                    return "um";
                case Units.Mils:
                    return "mil";
                case Units.Microinches:
                    return "uin";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates whatever the plan holds and runs it. The plan is resolved against the
        /// sets actually in the document first, so a locator naming a set that is not
        /// there skips its test by name rather than creating one with an empty side.
        /// </summary>
        public ClashRunOutcome Run(ClashTestPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.TestsInFile = plan.TestsInFile;

            Stopwatch stepClock = Stopwatch.StartNew();

            try
            {
                Document document = NavisworksApplication.ActiveDocument;

                if (document == null)
                {
                    log.Line("CLASH    stopped, there is no active document");
                    outcome.AddSkipped(
                        "every test", ClashSkipReason.Failed, "there is no active document");
                    return outcome;
                }

                // Named before the first test, because a clash count means nothing without
                // knowing what it was counted against.
                outcome.OpenDocument = NavisworksFacts.OpenDocument();
                log.Line("CLASH    ran against " + outcome.OpenDocument);
                log.Line("CLASH    the document measures in " + Or(UnitName(document.Units), "UNKNOWN units")
                    + ", every tolerance was converted into it");

                DocumentSelectionSets sets = document.SelectionSets;
                Dictionary<string, SelectionSet> byPath = IndexSets(sets);
                log.Line("CLASH    the document holds " + byPath.Count
                    + (byPath.Count == 1 ? " set" : " sets") + " the tests can name");

                // Every test carried into the plan but naming a set that is not here is
                // moved out by name, before anything is created.
                ClashTestPlan resolved = plan.ResolveAgainst(byPath.Keys);

                foreach (SkippedClashTest test in resolved.Skipped)
                {
                    outcome.AddSkipped(test);
                    log.Line("CLASH    SKIPPED  " + test.Name + "  " + test.Reason);
                }

                DocumentClashTests clashTests = document.GetClash().TestsData;
                Dictionary<string, ClashTest> present = IndexTests(clashTests);

                if (present.Count > 0)
                {
                    log.Line("CLASH    " + present.Count
                        + (present.Count == 1 ? " test is" : " tests are")
                        + " already in this document, they keep their results and are not recreated");
                }

                RunEach(document, sets, clashTests, byPath, present, resolved, outcome);
            }
            finally
            {
                stepClock.Stop();
                outcome.Seconds = stepClock.Elapsed.TotalSeconds;
            }

            return outcome;
        }

        private void RunEach(
            Document document,
            DocumentSelectionSets sets,
            DocumentClashTests clashTests,
            Dictionary<string, SelectionSet> byPath,
            Dictionary<string, ClashTest> present,
            ClashTestPlan plan,
            ClashRunOutcome outcome)
        {
            int total = plan.Buildable.Count;

            for (int i = 0; i < total; i++)
            {
                PlannedClashTest planned = plan.Buildable[i];
                progress("Test " + (i + 1) + " of " + total + ": " + planned.Name);

                if ((i + 1) % ProgressEvery == 0 || i + 1 == total)
                {
                    // A running count, so a run of well over a thousand tests is watched
                    // rather than silent.
                    log.Line("CLASH    " + (i + 1) + " of " + total + " tests, "
                        + outcome.RanCount + " run, " + outcome.SkippedCount + " skipped, "
                        + outcome.TotalClashes + " clashes so far");
                }

                OneTest(document, sets, clashTests, byPath, present, planned, outcome);
            }
        }

        private void OneTest(
            Document document,
            DocumentSelectionSets sets,
            DocumentClashTests clashTests,
            Dictionary<string, SelectionSet> byPath,
            Dictionary<string, ClashTest> present,
            PlannedClashTest planned,
            ClashRunOutcome outcome)
        {
            try
            {
                ClashTest test;

                if (present.TryGetValue(planned.Name, out test))
                {
                    // An OPENED group keeps its results, so a test already there is left
                    // exactly as it is. Rebuilding it would reset every clash to New and
                    // throw away every Active and Resolved, which is the whole reason the
                    // NWF is never cleared either.
                    outcome.AddAlreadyPresent(planned.Name);
                }
                else
                {
                    test = Create(document, sets, clashTests, byPath, planned);

                    if (test == null)
                    {
                        outcome.AddSkipped(
                            planned.Name,
                            ClashSkipReason.Failed,
                            "the test was added and a fresh read of the tests does not show it");
                        log.Line("CLASH    FAILED   " + planned.Name
                            + "  added but not found again by name");
                        return;
                    }

                    outcome.AddCreated(planned.Name);
                }

                int leftItems = ItemsOn(document, test.SelectionA, byPath, planned.Left.Locator);
                int rightItems = ItemsOn(document, test.SelectionB, byPath, planned.Right.Locator);

                string why;

                if (!ClashSideCheck.CanRun(planned, leftItems, rightItems, out why))
                {
                    // Not run, and not counted as passed. A zero from a test that never
                    // ran reads exactly like a zero from a test that found nothing wrong.
                    outcome.AddSkipped(planned.Name, ClashSkipReason.EmptySide, why);
                    log.Line("CLASH    SKIPPED  " + planned.Name + "  " + why);
                    return;
                }

                Stopwatch clock = Stopwatch.StartNew();
                clashTests.TestsRunTest(test);
                clock.Stop();

                ClashTally tally = Count(test, planned.Name);
                ClashTestResult result = outcome.AddRan(
                    planned.Name, leftItems, rightItems, tally, clock.Elapsed.TotalSeconds);

                log.Line("CLASH    " + result.Line());
            }
            catch (Exception error)
            {
                outcome.AddSkipped(
                    planned.Name,
                    ClashSkipReason.Failed,
                    error.GetType().Name + ": " + error.Message);

                log.Failure(
                    "clash test " + planned.Name,
                    error,
                    "kept going with the next test, this one is reported as skipped and was not run");
            }
        }

        /// <summary>
        /// Builds one test from the file and nothing but the file, adds it, and reads it
        /// back by name. TestsAddCopy takes a copy the way DocumentSelectionSets.AddCopy
        /// does, so the object handed in is not the object in the tree and the one in the
        /// tree is the one that has to run.
        /// </summary>
        private ClashTest Create(
            Document document,
            DocumentSelectionSets sets,
            DocumentClashTests clashTests,
            Dictionary<string, SelectionSet> byPath,
            PlannedClashTest planned)
        {
            using (ClashTest test = new ClashTest())
            {
                test.DisplayName = planned.Name;
                test.TestType = (ClashTestType)(int)planned.TestType;
                test.Tolerance = planned.Tolerance;
                test.MergeComposites = planned.MergeComposites;

                FillSide(document, sets, test.SelectionA, planned.Left, byPath);
                FillSide(document, sets, test.SelectionB, planned.Right, byPath);

                clashTests.TestsAddCopy(test);
            }

            log.Detail("CLASH    created  " + planned.Name
                + "  " + planned.TestTypeName
                + "  tolerance " + planned.DescribeTolerance()
                + "  merge composites " + (planned.MergeComposites ? "on" : "off"));

            // Read again from a fresh index rather than from the object handed to AddCopy.
            return FindTest(clashTests, planned.Name);
        }

        /// <summary>
        /// Points one side at the saved set the locator names, rather than copying a list
        /// of items into it. CreateSelectionSource is the only way to make a SelectionSource,
        /// its constructors are not public, and pointing at the set is what Clash Detective
        /// itself does, so the counts match the panel.
        /// </summary>
        private void FillSide(
            Document document,
            DocumentSelectionSets sets,
            ClashSelection side,
            PlannedClashSide planned,
            Dictionary<string, SelectionSet> byPath)
        {
            side.SelfIntersect = planned.SelfIntersect;
            side.PrimitiveTypes = PrimitiveTypesFrom(planned.PrimitiveTypes, planned.Locator);

            SelectionSet set = byPath[planned.Locator];

            side.Selection.Clear();
            side.Selection.SelectionSources.Add(sets.CreateSelectionSource(set));
        }

        /// <summary>
        /// The primtypes number from the file. PrimitiveTypes is a Flags enum over int
        /// whose bits are those numbers, so the value passes straight through. Any bit
        /// that is not one of the known ones is reported rather than dropped quietly.
        /// </summary>
        private PrimitiveTypes PrimitiveTypesFrom(int flags, string locator)
        {
            int known = (int)PrimitiveTypes.Triangles
                | (int)PrimitiveTypes.Lines
                | (int)PrimitiveTypes.Points
                | (int)PrimitiveTypes.SnapPoints
                | (int)PrimitiveTypes.Text;

            int unknown = flags & ~known;

            if (unknown != 0)
            {
                log.Line("CLASH    primtypes " + flags + " on \"" + locator
                    + "\" carries bit value " + unknown
                    + " which is not one this version of Navisworks names, passed through as written");
            }

            return (PrimitiveTypes)flags;
        }

        /// <summary>
        /// How many items a side finds in this model, asked of the side itself rather than
        /// of the set, because the side is the thing that has to work. Falls back to the
        /// set when the side reports nothing at all, so a side that was wired wrongly is
        /// visible as a difference rather than as a silent zero.
        /// </summary>
        private int ItemsOn(
            Document document,
            ClashSelection side,
            Dictionary<string, SelectionSet> byPath,
            string locator)
        {
            ModelItemCollection found = side.Selection.GetSelectedItems(document);

            if (found != null && found.Count > 0)
            {
                return found.Count;
            }

            SelectionSet set;

            if (!byPath.TryGetValue(locator, out set))
            {
                return found == null ? 0 : found.Count;
            }

            ModelItemCollection fromSet = set.GetSelectedItems(document);
            int viaSet = fromSet == null ? 0 : fromSet.Count;

            if (viaSet > 0)
            {
                log.Line("CLASH    the side pointing at \"" + locator + "\" resolved to 0 items but the set "
                    + "itself resolves to " + viaSet + ", the set count is the one reported");
            }

            return viaSet;
        }

        /// <summary>
        /// Counts the results of one test by status. A result group is one row in the
        /// panel holding several clashes, so the leaves are counted and the grouping is
        /// reported, rather than a group silently counting as one.
        /// </summary>
        private ClashTally Count(ClashTest test, string name)
        {
            ClashTally tally = new ClashTally();
            int groups = 0;

            CountInto(test.Children, tally, ref groups);

            if (groups > 0)
            {
                log.Line("CLASH    " + name + " holds " + groups
                    + (groups == 1 ? " result group" : " result groups")
                    + ", the clashes inside them are counted, not the groups");
            }

            return tally;
        }

        private void CountInto(SavedItemCollection children, ClashTally tally, ref int groups)
        {
            if (children == null)
            {
                return;
            }

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];
                ClashResultGroup group = child as ClashResultGroup;

                if (group != null)
                {
                    groups++;
                    CountInto(group.Children, tally, ref groups);
                    continue;
                }

                ClashResult result = child as ClashResult;

                if (result != null)
                {
                    tally.Add((CoreClashStatus)(int)result.Status);
                }
            }
        }

        // ---------- reading the two trees ----------

        /// <summary>
        /// Every set in the document, keyed by the same path a locator is written with, so
        /// the two are compared with an ordinary string comparison. Ordinal and never
        /// trimmed, because two set names in the reference file end in a space.
        /// </summary>
        public Dictionary<string, SelectionSet> IndexSets(DocumentSelectionSets sets)
        {
            Dictionary<string, SelectionSet> byPath =
                new Dictionary<string, SelectionSet>(StringComparer.Ordinal);

            List<string> folders = new List<string>();
            WalkSets(sets.RootItem, folders, byPath);
            return byPath;
        }

        private void WalkSets(
            GroupItem parent, List<string> folders, Dictionary<string, SelectionSet> byPath)
        {
            if (parent == null)
            {
                return;
            }

            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];
                SelectionSet set = child as SelectionSet;

                if (set != null)
                {
                    string path = BuildPath(folders, set.DisplayName);

                    // Two sets at one path is a document that was built oddly, not a
                    // reason to stop. The first is kept and the second is named.
                    if (byPath.ContainsKey(path))
                    {
                        log.Line("CLASH    two sets share the path \"" + path
                            + "\", the first one found is the one the tests will use");
                        continue;
                    }

                    byPath.Add(path, set);
                    continue;
                }

                GroupItem folder = child as GroupItem;

                if (folder == null)
                {
                    continue;
                }

                folders.Add(folder.DisplayName);
                WalkSets(folder, folders, byPath);
                folders.RemoveAt(folders.Count - 1);
            }
        }

        /// <summary>Builds lcop_selection_set_tree/folder/folder/name, the locator form.</summary>
        public string BuildPath(IList<string> folders, string name)
        {
            List<string> parts = new List<string> { SetTreeRoot };

            if (folders != null)
            {
                parts.AddRange(folders);
            }

            parts.Add(name);
            return string.Join("/", parts.ToArray());
        }

        /// <summary>
        /// Every clash test already in the document, by name. A ClashTest is itself a
        /// GroupItem holding its results, so it is tested for before a folder is, or the
        /// walk would descend into the results.
        /// </summary>
        public Dictionary<string, ClashTest> IndexTests(DocumentClashTests clashTests)
        {
            Dictionary<string, ClashTest> byName =
                new Dictionary<string, ClashTest>(StringComparer.Ordinal);

            WalkTests(clashTests.Tests, byName);
            return byName;
        }

        private void WalkTests(SavedItemCollection items, Dictionary<string, ClashTest> byName)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                SavedItem item = items[i];
                ClashTest test = item as ClashTest;

                if (test != null)
                {
                    string name = test.DisplayName;

                    if (!string.IsNullOrEmpty(name) && !byName.ContainsKey(name))
                    {
                        byName.Add(name, test);
                    }

                    continue;
                }

                GroupItem folder = item as GroupItem;

                if (folder != null)
                {
                    WalkTests(folder.Children, byName);
                }
            }
        }

        private ClashTest FindTest(DocumentClashTests clashTests, string name)
        {
            Dictionary<string, ClashTest> byName = IndexTests(clashTests);
            ClashTest test;
            return byName.TryGetValue(name, out test) ? test : null;
        }

        private static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
