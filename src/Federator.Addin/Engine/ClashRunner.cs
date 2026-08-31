using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.DocumentParts;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Naming;
using Federator.Core.Report;
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
    ///
    /// HOW A CLASH TEST IS HELD, AND WHY IT IS NEVER HELD FOR LONG.
    ///
    /// A ClashTest read out of DocumentClashTests.Tests is a borrowed view of an object the
    /// document owns. Measured off the installed DLL on 2026-08-31: GroupItem.GetChild
    /// creates the wrapper through SavedItem.InternalCreator with ownership eEXTERNAL, and
    /// NativeHandle holds the native object through an LcUWeakReferenceHandle. So the
    /// wrapper is valid only while the native object behind it is, and every mutator on
    /// DocumentClashTests is a copy form, TestsAddCopy, TestsEditTestFromCopy,
    /// TestsReplaceWithCopy, which replaces that object.
    ///
    /// TestsRunTest writes results into the test, so it is a mutation too. A wrapper held
    /// across it is dead the moment it returns, and reading Children off it throws
    /// ObjectDisposedException "Object has been Disposed (WeakRef)". A real run threw that
    /// once per test, tens of thousands of times, over 8 hours 52 minutes.
    ///
    /// So a test is addressed by where it sits, never held. TestAddress is the path of
    /// child indexes from the root, and every use resolves it again, uses it, and disposes
    /// the wrapper. That also removes the per test walk of the whole tests collection,
    /// which was O(n squared) over 1830 tests and created about 1.7 million short lived
    /// native handles for each group.
    /// </summary>
    public sealed class ClashRunner
    {
        private readonly Action<string> progress;
        private readonly RunLog log;
        private readonly RepeatedFailureGuard guard;

        /// <summary>How often the running count goes in the log, so a long run is watchable.</summary>
        public const int ProgressEvery = 25;

        /// <summary>
        /// How many skips are written out in full for each reason as the run goes. One
        /// real run wrote 1830 near identical SKIPPED lines and a 1 MB log. The rest are
        /// counted and reported once, in the block at the end.
        /// </summary>
        private readonly Dictionary<ClashSkipReason, int> skipsLogged =
            new Dictionary<ClashSkipReason, int>();

        /// <summary>The Summary row for each buildable test, keyed on where it sat in the file.</summary>
        private Dictionary<int, TestReport> reports;

        public ClashRunner(Action<string> progress, RunLog log)
            : this(progress, log, new RepeatedFailureGuard())
        {
        }

        /// <summary>
        /// The guard is handed in so it can live for the whole run rather than for one
        /// group. A run failing uniformly must stop the run, and a per group guard would
        /// have let the same nine hours pass 24 times over.
        /// </summary>
        public ClashRunner(Action<string> progress, RunLog log, RepeatedFailureGuard guard)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.progress = progress ?? delegate { };
            this.log = log;
            this.guard = guard ?? new RepeatedFailureGuard();
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
        /// <summary>
        /// The workbook model this run fills as it goes, or null when nothing is being
        /// written. It is filled from the live results here, in one pass, because the
        /// results are only readable while the document is open.
        /// </summary>
        public ClashReport Report { get; set; }

        /// <summary>How the discipline is read off a source file name. A setting.</summary>
        public ContainerNameSettings NameSettings { get; set; }

        /// <summary>
        /// True when this group holds one NWC. Every test is still created, so the NWF is
        /// complete and matches the other groups, and none of them is run, because one
        /// model cannot clash with anything. Recorded as SingleModel rather than as a side
        /// finding nothing, which is a different fact about a different problem.
        /// </summary>
        public bool SingleModelGroup { get; set; }

        /// <summary>
        /// Apply the file's settings to tests already in the document. Off by default,
        /// because doing it RESETS their results, and those results are the only record of
        /// what has been fixed. Off, this only reports what has drifted.
        /// </summary>
        public bool ApplyFileSettings { get; set; }

        /// <summary>
        /// Writes a picture per clash. Null leaves every Image cell empty, which is what
        /// a run with images switched off does.
        /// </summary>
        public ClashImages Images { get; set; }

        /// <summary>
        /// Where the workbook is going, because the pictures go in a folder named after
        /// it and beside it. Known before the clash step rather than after, since that is
        /// when the pictures are rendered.
        /// </summary>
        public string WorkbookPath { get; set; }

        /// <summary>
        /// Remove Resolved clashes after the tests have run. Off by default, and never
        /// done silently, because it destroys the record of what was resolved.
        /// </summary>
        public bool CompactResolved { get; set; }

        /// <summary>Everything the file and the document disagree about, by test name.</summary>
        public IList<TestDifference> Drift
        {
            get { return drift; }
        }

        private readonly List<TestDifference> drift = new List<TestDifference>();

        /// <summary>How many tests already in the document were compared against the file.</summary>
        public int Compared { get; private set; }

        public ClashRunOutcome Run(ClashTestPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.TestsInFile = plan.TestsInFile;
            skipsLogged.Clear();
            reports = null;
            drift.Clear();
            Compared = 0;

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
                setsForLookup = sets;
                Dictionary<string, SelectionSet> byPath = IndexSets(sets);
                int expected = plan.DistinctLocators().Count;

                log.Line("CLASH    the document holds " + byPath.Count
                    + (byPath.Count == 1 ? " set" : " sets") + " and the tests name "
                    + expected + (expected == 1 ? " set" : " sets"));

                // Every test carried into the plan but naming a set that is not here is
                // moved out by name, before anything is created.
                ClashTestPlan resolved = plan.ResolveAgainst(byPath.Keys);

                // The guard. A document with no sets in it once took a whole run to say so
                // 1830 times over. If there were tests that could have run and not one of
                // them resolves a set, nothing here can clash, so it says so and stops
                // rather than creating tests that can only report zero.
                if (plan.Buildable.Count > 0 && resolved.Buildable.Count == 0)
                {
                    string why = outcome.StopBecauseNoSetResolves(byPath.Count, expected);
                    log.Line("CLASH    STOPPED  " + why);
                    progress("Clash stopped. " + why);
                    log.Line("CLASH    build the sets first, or pick a file whose tests name "
                        + "the sets this document already holds");
                    return outcome;
                }

                foreach (SkippedClashTest test in resolved.Skipped)
                {
                    outcome.AddSkipped(test);
                    LogSkip(test);
                }

                // One row per test in the file, in file order, before anything runs. A
                // test that never runs still has to reach the Summary carrying why.
                reports = BuildReports(resolved);

                DocumentClashTests clashTests = document.GetClash().TestsData;
                Dictionary<string, TestAddress> present = IndexTests(clashTests);

                // Always logged, even at zero. Whether tests survive from one group into
                // the next document is the question a slowdown turns on, and this is the
                // line that answers it from the log rather than from a theory.
                log.Line("CLASH    the document already holds " + present.Count
                    + (present.Count == 1 ? " clash test" : " clash tests")
                    + (present.Count == 0
                        ? ", so everything created here is new"
                        : ", they keep their results and are not recreated"));

                RunEach(document, sets, clashTests, byPath, present, resolved, outcome);

                if (drift.Count > 0 || Compared > 0)
                {
                    log.Block("DRIFT " + Or(outcome.OpenDocument, "this document"),
                        TestDrift.Lines(drift, Compared, ApplyFileSettings));
                }

                Compact(clashTests, outcome);
            }
            finally
            {
                stepClock.Stop();
                outcome.Seconds = stepClock.Elapsed.TotalSeconds;
            }

            return outcome;
        }

        /// <summary>
        /// A Summary row for every test in the file, numbered in file order so a sheet
        /// number is the same on every run. Skipped tests are in here too, carrying why,
        /// because skipped and passed are different numbers and both have to be readable
        /// off one sheet.
        /// </summary>
        private Dictionary<int, TestReport> BuildReports(ClashTestPlan plan)
        {
            Dictionary<int, TestReport> byIndex = new Dictionary<int, TestReport>();

            if (Report == null)
            {
                return byIndex;
            }

            List<int> order = new List<int>();
            Dictionary<int, PlannedClashTest> buildable = new Dictionary<int, PlannedClashTest>();
            Dictionary<int, SkippedClashTest> skipped = new Dictionary<int, SkippedClashTest>();

            foreach (PlannedClashTest test in plan.Buildable)
            {
                if (!buildable.ContainsKey(test.FileIndex))
                {
                    buildable.Add(test.FileIndex, test);
                    order.Add(test.FileIndex);
                }
            }

            foreach (SkippedClashTest test in plan.Skipped)
            {
                if (test.FileIndex >= 0 && !skipped.ContainsKey(test.FileIndex)
                    && !buildable.ContainsKey(test.FileIndex))
                {
                    skipped.Add(test.FileIndex, test);
                    order.Add(test.FileIndex);
                }
            }

            order.Sort();

            foreach (int index in order)
            {
                PlannedClashTest planned;

                if (buildable.TryGetValue(index, out planned))
                {
                    TestReport report = Report.AddTest(planned.Name);
                    report.LeftLocator = planned.Left.Locator;
                    report.RightLocator = planned.Right.Locator;
                    report.Tolerance = planned.Tolerance;
                    report.ToleranceUnits = planned.DocumentUnits;
                    report.TestTypeName = planned.TestTypeName;
                    report.State = TestState.Skipped;
                    byIndex.Add(index, report);
                    continue;
                }

                SkippedClashTest missed = skipped[index];
                TestReport row = Report.AddTest(missed.Name);
                row.State = TestState.Skipped;
                row.SkippedReason = missed.Reason;
            }

            return byIndex;
        }

        /// <summary>The Summary row for one test, or null when nothing is being written.</summary>
        private TestReport ReportFor(PlannedClashTest planned)
        {
            TestReport found;

            return reports != null && reports.TryGetValue(planned.FileIndex, out found)
                ? found
                : null;
        }

        /// <summary>
        /// Removes Resolved clashes, which stay in the file and keep counting until
        /// something takes them out. Never silent, and never on unless it was asked for,
        /// because it destroys the record of what was resolved.
        ///
        /// TestsCompactAllTests and TestsCompactTest are both on DocumentClashTests, read
        /// off the installed DLL on 2026-08-31, so this is reachable rather than guessed.
        /// </summary>
        private void Compact(DocumentClashTests clashTests, ClashRunOutcome outcome)
        {
            int before = outcome.Totals.Of(CoreClashStatus.Resolved);

            if (!CompactResolved)
            {
                if (before > 0)
                {
                    log.Line("CLASH    " + before
                        + " clashes are Resolved and stay in the file, counting, until they are "
                        + "compacted. Compacting is off, so nothing was removed.");
                }

                return;
            }

            log.Line("CLASH    COMPACTING. This removes Resolved clashes from every test in this "
                + "document. It destroys the record of what was resolved and cannot be undone.");
            log.Line("CLASH    " + before + " Resolved before compacting.");

            try
            {
                clashTests.TestsCompactAllTests();
                outcome.Compacted = before;
                log.Line("CLASH    compacted. " + before
                    + (before == 1 ? " Resolved clash was removed." : " Resolved clashes were removed."));
            }
            catch (Exception error)
            {
                log.Failure(
                    "compacting the clash tests",
                    error,
                    "kept going, nothing was removed and every Resolved clash is still there");
            }
        }

        private void RunEach(
            Document document,
            DocumentSelectionSets sets,
            DocumentClashTests clashTests,
            Dictionary<string, SelectionSet> byPath,
            Dictionary<string, TestAddress> present,
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

                // Nine hours produced nothing once because nothing watched for this. A run
                // failing uniformly stops the run, not the group.
                if (guard.ShouldStopTheRun)
                {
                    outcome.StopTheWholeRun(guard.Reason);
                    log.Line("CLASH    RUN STOPPED  " + guard.Reason);
                    progress("The run was stopped. " + guard.Reason);
                    return;
                }
            }
        }

        private void OneTest(
            Document document,
            DocumentSelectionSets sets,
            DocumentClashTests clashTests,
            Dictionary<string, SelectionSet> byPath,
            Dictionary<string, TestAddress> present,
            PlannedClashTest planned,
            ClashRunOutcome outcome)
        {
            try
            {
                TestAddress address;

                if (present.TryGetValue(planned.Name, out address))
                {
                    // An OPENED group keeps its results, so a test already there is left
                    // exactly as it is. Rebuilding it would reset every clash to New and
                    // throw away every Active and Resolved, which is the whole reason the
                    // NWF is never cleared either.
                    //
                    // Left alone means a tolerance changed in the file never reaches it, so
                    // the two are compared and every difference is reported by name.
                    outcome.AddAlreadyPresent(planned.Name);
                    CompareAndMaybeApply(clashTests, address, planned, byPath, sets);
                }
                else
                {
                    address = Create(document, sets, clashTests, byPath, planned);

                    if (address == null)
                    {
                        Failed(
                            outcome,
                            planned.Name,
                            "the test was added and a fresh read of the tests does not show it");
                        log.Line("CLASH    FAILED   " + planned.Name
                            + "  added but not found again where it was put");
                        return;
                    }

                    outcome.AddCreated(planned.Name);
                }

                int leftItems;
                int rightItems;

                // Resolved, read, disposed. Nothing is held across the run below.
                using (ClashTest before = Resolve(clashTests, address, planned.Name))
                {
                    if (before == null)
                    {
                        Failed(outcome, planned.Name, "the test is no longer where it was put");
                        return;
                    }

                    leftItems = ItemsOn(document, before.SelectionA, byPath, planned.Left.Locator);
                    rightItems = ItemsOn(document, before.SelectionB, byPath, planned.Right.Locator);
                }

                TestReport summary = ReportFor(planned);

                if (summary != null)
                {
                    summary.LeftItems = leftItems;
                    summary.RightItems = rightItems;
                }

                if (SingleModelGroup)
                {
                    // Created, so the NWF matches every other group and a later run against
                    // a fuller model finds the tests already there. Not run, because there
                    // is nothing here for them to run against.
                    LogSkip(outcome.AddSkipped(
                        planned.Name,
                        ClashSkipReason.SingleModel,
                        "the group holds one model, so there is nothing for this test to clash against"));
                    guard.RecordNotAttempted();
                    return;
                }

                string why;

                if (!ClashSideCheck.CanRun(planned, leftItems, rightItems, out why))
                {
                    if (summary != null)
                    {
                        summary.State = TestState.Skipped;
                        summary.SkippedReason = why;
                    }

                    // Not run, and not counted as passed. A zero from a test that never
                    // ran reads exactly like a zero from a test that found nothing wrong.
                    // It says nothing about whether the run is broken, so the guard is
                    // told it was not attempted rather than that it succeeded.
                    LogSkip(outcome.AddSkipped(planned.Name, ClashSkipReason.EmptySide, why));
                    guard.RecordNotAttempted();
                    return;
                }

                Stopwatch clock = Stopwatch.StartNew();

                using (ClashTest running = Resolve(clashTests, address, planned.Name))
                {
                    if (running == null)
                    {
                        Failed(outcome, planned.Name, "the test is no longer where it was put");
                        return;
                    }

                    clashTests.TestsRunTest(running);
                }

                clock.Stop();

                // A fresh handle. The one handed to TestsRunTest is dead by now, and
                // reading Children off it is exactly what threw tens of thousands of times.
                ClashTally tally;

                using (ClashTest after = Resolve(clashTests, address, planned.Name))
                {
                    if (after == null)
                    {
                        Failed(
                            outcome,
                            planned.Name,
                            "the test ran but could not be found again to count its results");
                        return;
                    }

                    tally = Count(after, planned.Name);

                    // Read now, in the same pass, because the results are only readable
                    // while this document is open and this handle is fresh.
                    if (summary != null)
                    {
                        summary.Seconds = clock.Elapsed.TotalSeconds;
                        ClashHarvest harvest = new ClashHarvest(log, NameSettings);
                        harvest.Images = Images;
                        harvest.WorkbookPath = WorkbookPath;
                        harvest.Into(document, clashTests, after, Report, summary);
                        summary.State = summary.HasSheet
                            ? TestState.FoundClashes
                            : TestState.Passed;
                    }
                }

                ClashTestResult result = outcome.AddRan(
                    planned.Name, leftItems, rightItems, tally, clock.Elapsed.TotalSeconds);

                guard.RecordSuccess();
                log.Line("CLASH    " + result.Line());
            }
            catch (Exception error)
            {
                string reason = error.GetType().Name + ": " + error.Message;

                outcome.AddSkipped(planned.Name, ClashSkipReason.Failed, reason);
                guard.RecordFailure(reason);

                log.Failure(
                    "clash test " + planned.Name,
                    error,
                    "kept going with the next test, this one is reported as skipped and was not run");
            }
        }

        /// <summary>
        /// Compares one test already in the document against what the file says it should
        /// be, and reports every difference by name. Changes nothing unless
        /// ApplyFileSettings is on, because changing a test resets its results.
        /// </summary>
        private void CompareAndMaybeApply(
            DocumentClashTests clashTests,
            TestAddress address,
            PlannedClashTest planned,
            Dictionary<string, SelectionSet> byPath,
            DocumentSelectionSets sets)
        {
            using (ClashTest test = Resolve(clashTests, address, planned.Name))
            {
                if (test == null)
                {
                    return;
                }

                Compared++;
                ReportStatus(test, planned.Name);

                IList<TestDifference> differences = TestDrift.Compare(
                    planned.Name, TestSettings.FromFile(planned), SettingsOf(test, byPath));

                if (differences.Count == 0)
                {
                    return;
                }

                foreach (TestDifference difference in differences)
                {
                    drift.Add(difference);
                }

                if (!ApplyFileSettings)
                {
                    return;
                }
            }

            // Applying is a mutation, so the handle above is finished with before this and
            // the test is resolved again inside.
            Apply(clashTests, address, planned, byPath, sets);
        }

        /// <summary>
        /// What the test in the document is actually set to. The side locators are read
        /// back by asking the document which saved set each side points at.
        /// </summary>
        private TestSettings SettingsOf(ClashTest test, Dictionary<string, SelectionSet> byPath)
        {
            TestSettings settings = new TestSettings();
            settings.Tolerance = test.Tolerance;
            settings.TestType = (ClashTestKind)(int)test.TestType;
            settings.TestTypeName = test.TestType.ToString();
            settings.MergeComposites = test.MergeComposites;

            settings.LeftSelfIntersect = test.SelectionA.SelfIntersect;
            settings.RightSelfIntersect = test.SelectionB.SelfIntersect;
            settings.LeftPrimitiveTypes = (int)test.SelectionA.PrimitiveTypes;
            settings.RightPrimitiveTypes = (int)test.SelectionB.PrimitiveTypes;

            settings.LeftLocator = LocatorOf(test.SelectionA, byPath);
            settings.RightLocator = LocatorOf(test.SelectionB, byPath);

            return settings;
        }

        /// <summary>
        /// Which set path a side points at, found by matching the sets this run indexed.
        /// Empty when the side points at something that is not one of them, which is a
        /// difference worth reporting rather than hiding.
        /// </summary>
        private string LocatorOf(ClashSelection side, Dictionary<string, SelectionSet> byPath)
        {
            try
            {
                SelectionSourceCollection sources = side.Selection.SelectionSources;

                if (sources == null || sources.Count == 0)
                {
                    return string.Empty;
                }

                foreach (KeyValuePair<string, SelectionSet> pair in byPath)
                {
                    using (SelectionSource mine = setsForLookup.CreateSelectionSource(pair.Value))
                    {
                        for (int i = 0; i < sources.Count; i++)
                        {
                            if (sources[i].Equals(mine))
                            {
                                return pair.Key;
                            }
                        }
                    }
                }

                return string.Empty;
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading which set a clash side points at",
                    error,
                    "kept going, that side is reported as UNKNOWN rather than as changed");
                return UnknownLocator;
            }
        }

        /// <summary>
        /// A side this tool could not read. Never compared, so it is not reported as drift
        /// when the truth is that nothing was read.
        /// </summary>
        public const string UnknownLocator = "UNKNOWN";

        private DocumentSelectionSets setsForLookup;

        /// <summary>
        /// Puts the file's settings onto a test already in the document. This RESETS its
        /// results, which is why it is off by default and said loudly in the log.
        /// </summary>
        private void Apply(
            DocumentClashTests clashTests,
            TestAddress address,
            PlannedClashTest planned,
            Dictionary<string, SelectionSet> byPath,
            DocumentSelectionSets sets)
        {
            try
            {
                using (ClashTest replacement = new ClashTest())
                {
                    replacement.DisplayName = planned.Name;
                    replacement.TestType = (ClashTestType)(int)planned.TestType;
                    replacement.Tolerance = planned.Tolerance;
                    replacement.MergeComposites = planned.MergeComposites;

                    FillSide(document: null, sets: sets, side: replacement.SelectionA,
                        planned: planned.Left, byPath: byPath);
                    FillSide(document: null, sets: sets, side: replacement.SelectionB,
                        planned: planned.Right, byPath: byPath);

                    using (ClashTest existing = Resolve(clashTests, address, planned.Name))
                    {
                        if (existing == null)
                        {
                            return;
                        }

                        clashTests.TestsEditTestFromCopy(existing, replacement);
                    }
                }

                log.Line("CLASH    APPLIED  " + planned.Name
                    + "  the file's settings were put onto the test in the document, which reset "
                    + "its results");
            }
            catch (Exception error)
            {
                log.Failure(
                    "applying the file's settings to " + planned.Name,
                    error,
                    "kept going, that test still holds what it held before");
            }
        }

        /// <summary>
        /// What the document says about this test's state.
        ///
        /// ClashTest.Status is the only thing on the type that could carry it, and its
        /// enum is New, Old, Partial, Complete. Old is the one Clash Detective shows when
        /// a test has been run and something has changed since. Whether Old is set for
        /// exactly the reasons a person means by altered is UNKNOWN from the DLL, so the
        /// status is reported as itself and no meaning is put on it here. See
        /// docs\scan.md section 4j.
        /// </summary>
        private void ReportStatus(ClashTest test, string name)
        {
            try
            {
                if (test.Status == ClashTestStatus.Old)
                {
                    log.Line("CLASH    OLD      " + name
                        + "  Navisworks has this test marked Old, which it does when a test has "
                        + "run and something changed after. Its results may not match the model "
                        + "as it stands.");
                }
            }
            catch (Exception)
            {
                // Reading a status is never worth failing a test over.
            }
        }

        /// <summary>
        /// A failure that did not throw. It still counts towards the guard, because a run
        /// where every test fails the same way is the case that guard exists for, whether
        /// or not an exception carried the news.
        /// </summary>
        private void Failed(ClashRunOutcome outcome, string name, string reason)
        {
            outcome.AddSkipped(name, ClashSkipReason.Failed, reason);
            guard.RecordFailure(reason);
        }

        /// <summary>
        /// Builds one test from the file and nothing but the file, adds it, and returns
        /// where it landed. TestsAddCopy adds at the root and returns void, so the new
        /// test is the last child of the root, and that is checked by name rather than
        /// assumed. Nothing walks the whole collection, which is what made this O(n
        /// squared) over 1830 tests.
        /// </summary>
        private TestAddress Create(
            Document document,
            DocumentSelectionSets sets,
            DocumentClashTests clashTests,
            Dictionary<string, SelectionSet> byPath,
            PlannedClashTest planned)
        {
            int before = clashTests.Tests.Count;

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

            int after = clashTests.Tests.Count;

            if (after != before + 1)
            {
                log.Line("CLASH    the tests went from " + before + " to " + after
                    + " when \"" + planned.Name + "\" was added, which is not one more");
                return null;
            }

            TestAddress address = TestAddress.At(before);

            // Checked by name from a fresh read, never assumed. AddCopy takes a copy, so
            // the object in the tree is not the one handed in.
            using (ClashTest landed = Resolve(clashTests, address, planned.Name))
            {
                return landed == null ? null : address;
            }
        }

        /// <summary>
        /// Reads the test at an address out of a freshly read collection, and checks it is
        /// still the test that name says. Returns null rather than the wrong test, because
        /// running the wrong test writes results into somebody else's.
        ///
        /// The caller disposes what comes back. The wrapper is created with eEXTERNAL
        /// ownership, so disposing it releases the wrapper and never the document's test.
        /// </summary>
        private ClashTest Resolve(DocumentClashTests clashTests, TestAddress address, string name)
        {
            SavedItemCollection children = clashTests.Tests;
            SavedItem item = null;

            for (int level = 0; level < address.Depth; level++)
            {
                int index = address.IndexAt(level);

                if (children == null || index < 0 || index >= children.Count)
                {
                    return null;
                }

                item = children[index];

                if (level + 1 == address.Depth)
                {
                    break;
                }

                GroupItem group = item as GroupItem;

                if (group == null)
                {
                    return null;
                }

                children = group.Children;
            }

            ClashTest test = item as ClashTest;

            if (test == null)
            {
                return null;
            }

            if (!string.Equals(test.DisplayName, name, StringComparison.Ordinal))
            {
                log.Line("CLASH    the test at " + address + " is now \"" + test.DisplayName
                    + "\" and not \"" + name + "\", so it was left alone");
                test.Dispose();
                return null;
            }

            return test;
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

            // The source is handed to the collection, which takes it from here. Disposing
            // it after the Add would take it back out from under the test.
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
            int fromSide;

            using (ModelItemCollection found = side.Selection.GetSelectedItems(document))
            {
                fromSide = found == null ? 0 : found.Count;
            }

            if (fromSide > 0)
            {
                return fromSide;
            }

            SelectionSet set;

            if (!byPath.TryGetValue(locator, out set))
            {
                return fromSide;
            }

            int viaSet;

            using (ModelItemCollection fromSet = set.GetSelectedItems(document))
            {
                viaSet = fromSet == null ? 0 : fromSet.Count;
            }

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
        ///
        /// The test handed in must be freshly resolved. Running a test replaces the native
        /// object, so a handle taken before the run throws here rather than counting.
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
                using (SavedItem child = children[i])
                {
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
        }

        /// <summary>
        /// Writes a skip into the log, at most MaxSkipExamples of them for each reason,
        /// then one line saying the rest are counted. Everything skipped still reaches the
        /// block at the end, where it is counted by reason with its examples.
        /// </summary>
        private void LogSkip(SkippedClashTest test)
        {
            int already;
            skipsLogged.TryGetValue(test.Kind, out already);
            skipsLogged[test.Kind] = already + 1;

            if (already < ClashRunOutcome.MaxSkipExamples)
            {
                log.Line("CLASH    SKIPPED  " + test.Name + "  " + test.Reason);
                return;
            }

            if (already == ClashRunOutcome.MaxSkipExamples)
            {
                log.Line("CLASH    further skips for \"" + ClashTestPlan.Describe(test.Kind)
                    + "\" are counted, not listed. The block at the end carries the total.");
            }
        }

        // ---------- reading the two trees ----------

        /// <summary>
        /// Every set in the document, keyed by the same path a locator is written with, so
        /// the two are compared with an ordinary string comparison. Ordinal and never
        /// trimmed, because two set names in the reference file end in a space.
        ///
        /// These wrappers are held for the whole group on purpose. Nothing in the clash
        /// step mutates the sets tree, so nothing invalidates them.
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
                        set.Dispose();
                        continue;
                    }

                    byPath.Add(path, set);
                    continue;
                }

                GroupItem folder = child as GroupItem;

                if (folder == null)
                {
                    child.Dispose();
                    continue;
                }

                folders.Add(folder.DisplayName);
                WalkSets(folder, folders, byPath);
                folders.RemoveAt(folders.Count - 1);
                folder.Dispose();
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
        /// Where every clash test already in the document sits, by name. Addresses, not
        /// handles, because the first TestsAddCopy would invalidate any handle kept here.
        /// Walked once for the whole group, never once per test.
        ///
        /// A ClashTest is itself a GroupItem holding its results, so it is tested for
        /// before a folder is, or the walk would descend into the results.
        /// </summary>
        public Dictionary<string, TestAddress> IndexTests(DocumentClashTests clashTests)
        {
            Dictionary<string, TestAddress> byName =
                new Dictionary<string, TestAddress>(StringComparer.Ordinal);

            WalkTests(clashTests.Tests, new List<int>(), byName);
            return byName;
        }

        private void WalkTests(
            SavedItemCollection items, List<int> path, Dictionary<string, TestAddress> byName)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    path.Add(i);

                    ClashTest test = item as ClashTest;

                    if (test != null)
                    {
                        string name = test.DisplayName;

                        if (!string.IsNullOrEmpty(name) && !byName.ContainsKey(name))
                        {
                            byName.Add(name, TestAddress.At(path));
                        }
                    }
                    else
                    {
                        GroupItem folder = item as GroupItem;

                        if (folder != null)
                        {
                            WalkTests(folder.Children, path, byName);
                        }
                    }

                    path.RemoveAt(path.Count - 1);
                }
            }
        }

        private static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }

    /// <summary>
    /// Where a clash test sits, as the path of child indexes from the root of the tests
    /// tree. A test is addressed rather than held, because every mutation through
    /// DocumentClashTests replaces the native object and kills any handle onto it.
    /// </summary>
    public sealed class TestAddress
    {
        private readonly int[] path;

        private TestAddress(int[] path)
        {
            this.path = path;
        }

        public static TestAddress At(int index)
        {
            return new TestAddress(new[] { index });
        }

        public static TestAddress At(IList<int> path)
        {
            if (path == null || path.Count == 0)
            {
                throw new ArgumentException("A test address needs at least one index.", "path");
            }

            int[] copy = new int[path.Count];

            for (int i = 0; i < path.Count; i++)
            {
                copy[i] = path[i];
            }

            return new TestAddress(copy);
        }

        public int Depth
        {
            get { return path.Length; }
        }

        public int IndexAt(int level)
        {
            return path[level];
        }

        public override string ToString()
        {
            string[] parts = new string[path.Length];

            for (int i = 0; i < path.Length; i++)
            {
                parts[i] = path[i].ToString();
            }

            return "tests[" + string.Join("][", parts) + "]";
        }
    }
}
