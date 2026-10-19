using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.DocumentParts;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Report;
using Federator.Core.Sets;
using Federator.Core.Units;
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

        /// <summary>
        /// The line the window shows while the run works, or null where nothing supplied
        /// one, which is what the two hand buttons on the Clash step do. The three steps
        /// that run once per test are opened in here, so this is how the line knows which
        /// one it is in and how it comes to say that a step is running over twice as long
        /// as it did on the group before.
        /// </summary>
        public LiveLine Live { get; set; }

        /// <summary>
        /// How many skips are written out in full for each reason as the run goes. One
        /// real run wrote 1830 near identical SKIPPED lines and a 1 MB log. The rest are
        /// counted and reported once, in the block at the end.
        /// </summary>
        private readonly Dictionary<ClashSkipReason, int> skipsLogged =
            new Dictionary<ClashSkipReason, int>();

        /// <summary>The Summary row for each buildable test, keyed on where it sat in the file.</summary>
        private Dictionary<int, TestReport> reports;

        /// <summary>
        /// Moves named clashes to a status, F54. Made once per runner so its counts cover
        /// the whole group and the five and the four are written into the log once.
        /// </summary>
        private readonly ClashStatusEditor statuses;

        /// <summary>Whether a status was actually written, which is what asks for the NWF again.</summary>
        private bool changedTheDocument;

        /// <summary>
        /// The tolerance chosen on the Clash step, F76, never null. The default is the
        /// file's own value per test. A chosen one is set on every test in the run, created
        /// fresh or already in the document, and beats the XML and the document.
        /// </summary>
        public ToleranceChoice Tolerance { get; set; }

        /// <summary>The document units a tolerance is set in, read once per group.</summary>
        private string documentUnits;

        /// <summary>
        /// How many tests this group set a tolerance on, created fresh, and what happened to
        /// the tolerance of each test already in the document, FR-053. Both are counted under
        /// the default too, because the one TOLERANCE line per group carries both whichever
        /// way it reads.
        /// </summary>
        private int toleranceOnCreated;

        private SavedTestTolerances toleranceOnExisting = new SavedTestTolerances();

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
            this.statuses = new ClashStatusEditor(log);
            Tolerance = ToleranceChoice.Default();
            SetTreeRoot = ExchangeReader.SelectionSetTreeRoot;
            Priorities = PriorityMap.NothingPicked();
            Mirrors = new MirrorSettings();
        }

        /// <summary>
        /// The priority file the run picked, F132, read by the mirror rule to choose the
        /// test kept of a pair. NothingPicked where none was, which is the default.
        /// </summary>
        public PriorityMap Priorities { get; set; }

        /// <summary>
        /// F177. True only once the picture pass has run to its end without a throw and without the
        /// pictures' guard stopping the run. False where the pass was never reached, so nothing earlier
        /// is removed on a run that did not take its pictures.
        /// </summary>
        public bool PicturesFinished { get; private set; }

        /// <summary>
        /// The clash matrix's sets, ExchangeDocument.Sets, whose whole questions pair two tests
        /// of two sets alike, F132, or null with no XML, when only the same two sets swapped
        /// pair, which the MIRROR lines say.
        /// </summary>
        public IEnumerable<SelectionSetDefinition> ExchangeSets { get; set; }

        /// <summary>
        /// This group's sets build, F132 attempts 12 and 13, Bader's answer A to Q142, read by
        /// the mirror rule before any merge: a pair merges only where every set of both its
        /// tests was created by it from the clash matrix. Null, the default, is an outcome holding
        /// nothing, so with no sets build no pair merges and the MIRROR lines say why.
        /// </summary>
        public SetBuildOutcome SetsBuilt { get; set; }

        /// <summary>The ending a mirror's name carries, a setting, MirrorSettings' default (mirror).</summary>
        public MirrorSettings Mirrors { get; set; }

        /// <summary>
        /// The seconds of the PROGRESS lines between a test's rows and of the CALL line for one long
        /// Navisworks call, F143, ProgressSettings at its defaults, 30 s each by Bader's order of
        /// 2026-10-11.
        /// </summary>
        public ProgressSettings Progress { get; set; } = new ProgressSettings();

        /// <summary>One merge per test kept whose mirrors the rule merges, MirrorMerge.Of, none where pairing threw.</summary>
        private IList<MirrorMerge> merges = new List<MirrorMerge>();

        /// <summary>The merge each test kept is the kept test of, by its name.</summary>
        private readonly Dictionary<string, MirrorMerge> mergeOfKept = new Dictionary<string, MirrorMerge>(StringComparer.Ordinal);

        /// <summary>The merge each merged mirror is a mirror of, by the name it runs under.</summary>
        private readonly Dictionary<string, MirrorMerge> mergeOfMirror = new Dictionary<string, MirrorMerge>(StringComparer.Ordinal);

        /// <summary>The pair of each merged mirror, by the name it runs under.</summary>
        private readonly Dictionary<string, MirrorPair> pairOfMirror = new Dictionary<string, MirrorPair>(StringComparer.Ordinal);

        /// <summary>The names the plan runs under whose rename in the document failed, so no test of the name is created beside the old one.</summary>
        private readonly HashSet<string> renameFailed = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// Every row read while pictures are written, by the row, with where its result sits, F149,
        /// kept whichever way its harvest ended, so the picture pass takes each by its address once
        /// every test has run and every merge is made, walking the report's order, PictureWalk, and
        /// emptied as the pass ends, F147.
        /// </summary>
        private readonly Dictionary<ClashRow, RowAddress> picturesWaiting = new Dictionary<ClashRow, RowAddress>();

        /// <summary>
        /// Only each test's models shown for its pictures, F145, one for the clash step, made once
        /// the sets are indexed where pictures are written and null otherwise.
        /// </summary>
        private TestModelsShown modelsShown;

        /// <summary>Where every row of the report came from in the document, by the row, for the views after the merge.</summary>
        private readonly Dictionary<ClashRow, RowAddress> rowAddresses = new Dictionary<ClashRow, RowAddress>();

        /// <summary>What the document holds under each kept test of a merge, by its place in the file, for its ROWS line after the merge.</summary>
        private readonly Dictionary<int, int> clashesInDocument = new Dictionary<int, int>();

        /// <summary>How many sides of the saved tests were not read as one set this document holds, said once.</summary>
        private int savedSidesNotRead;

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
        /// Autodesk.Navisworks.Api.Units to the unit code ExchangeUnits converts with,
        /// read off UnitTable. The enum values were read off the installed DLL, see
        /// docs\history\scan.md. A value the table has not been taught returns null rather than a
        /// guessed factor, and every test is then skipped by name saying so.
        /// </summary>
        public static string UnitName(Units units)
        {
            UnitRow row = UnitTable.FindByEnumName(units.ToString());
            return row == null ? null : row.ExchangeCode;
        }

        /// <summary>
        /// The workbook model this run fills as it goes, or null when nothing is being
        /// written. It is filled from the live results here, in one pass, because the
        /// results are only readable while the document is open.
        /// </summary>
        public ClashReport Report { get; set; }

        /// <summary>
        /// Where each row of the report came from in the document, F132 attempt 2: the test
        /// it was read from, its address and the index path of its result, so the views
        /// read the rows the merged report holds and resolve each one by this, under the
        /// test the report holds it under, instead of walking the document by name.
        /// </summary>
        public IDictionary<ClashRow, RowAddress> RowAddresses
        {
            get { return rowAddresses; }
        }

        /// <summary>
        /// The name each mirror the rule paired runs under, MirrorPair.MirrorName, for the
        /// views, which give no mirrored test a view, F114, or null where no mirror rule
        /// ran this group, which the plan's lines say.
        /// </summary>
        public IList<string> MirrorNames { get; private set; }

        /// <summary>
        /// The kept test's name by the run name of each mirror whose clashes a merge takes,
        /// read off the merges this run indexed, so the views treat the kept test of a mirror
        /// the clash step failed on as not read whole, row F114-K38. Empty where none was paired.
        /// </summary>
        public IDictionary<string, string> KeptOfMirror
        {
            get
            {
                Dictionary<string, string> kept = new Dictionary<string, string>(StringComparer.Ordinal);

                foreach (KeyValuePair<string, MirrorPair> pair in pairOfMirror)
                {
                    kept[pair.Key] = pair.Value.Kept.Name;
                }

                return kept;
            }
        }

        /// <summary>
        /// The plan this run ran, F127: the clash matrix's tests resolved against the document's sets
        /// and each mirror under the name it runs under, or the saved tests read with their sides,
        /// so the coverage reads every test by the name the record keeps it under and each side by
        /// its set. Null where the run stopped before it had one, and the coverage then reads the
        /// plan it was handed.
        /// </summary>
        public ClashTestPlan RunPlan { get; private set; }

        /// <summary>
        /// The names of the tests of every mirrored pair whose clashes MirrorMerge.AddTo merged this
        /// run, the kept test and each mirror by the name it ran under, `MirrorMerge.MergedNames`, for
        /// the coverage's count check, which compares none of them because the clash workbook carries
        /// a mirror's clashes in the kept test's block. Read off what AddTo did and never off the plan,
        /// so a pair AddTo refused is compared like any other test, the breaker's first finding on
        /// F127b. Empty where no pair merged.
        /// </summary>
        public ICollection<string> InAMerge
        {
            get
            {
                HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);

                foreach (MirrorMerge merge in merges)
                {
                    names.UnionWith(merge.MergedNames);
                }

                return names;
            }
        }

        /// <summary>
        /// True when this group holds fewer than two disciplines. None of its tests is run,
        /// because one discipline cannot clash with itself, and since F77 only the tests
        /// whose sides both find something are created. Recorded as
        /// SingleDiscipline rather than as a side finding nothing, which is a different
        /// fact about a different problem. D5.
        /// </summary>
        public bool SingleDisciplineGroup { get; set; }

        /// <summary>
        /// F224, Bader's answer to Q209. The set codes a side is read against, the team map's and the group's own models',
        /// so the rule of two set codes leaves out a test whose two sides carry one of them. Null or empty reads no code and
        /// leaves out nothing, and the CLASH line says so.
        /// </summary>
        public IList<string> SetCodes { get; set; }

        /// <summary>
        /// Apply the file settings to tests already in the document. Off by default,
        /// because it changes WHICH CLASHES the test finds when it next runs, and a clash
        /// somebody marked Reviewed may then not come back, which is the only record of
        /// what has been fixed. Off, this only reports what has drifted.
        ///
        /// IT DOES NOT RESET ANYTHING BY ITSELF, and this comment said it did until 5y
        /// measured it on 2026-09-20: setting a tolerance on a saved test, the same value
        /// or a doubled one, keeps every result and every status, through a save and a
        /// reopen. 175,434 saved tests across his runs had one set on them and none of
        /// them cost him anything.
        /// </summary>
        public bool ApplyFileSettings { get; set; }

        /// <summary>
        /// Writes a picture per clash. Null leaves every Image cell empty, which is what
        /// a run with images switched off does.
        /// </summary>
        public ClashImages Images { get; set; }

        /// <summary>
        /// F182, Bader's answer B to Q175. True leaves the picture pass out of Run: the tests run and the mirrors are
        /// merged, the rows are read and each is addressed, and the pictures wait until TakeTheDeferredPictures is
        /// called, which the engine does after the NWF is saved and the NWD published. Navisworks's private memory
        /// grows about 3.3 MB for each picture, so the pictures come last of what changes the document's memory.
        /// False takes them inside Run as before, the hand button's way.
        /// </summary>
        public bool DeferPictures { get; set; }

        /// <summary>
        /// Where the workbook is going, because the pictures go in a folder named after
        /// it and beside it. Known before the clash step rather than after, since that is
        /// when the pictures are rendered.
        /// </summary>
        public string WorkbookPath { get; set; }

        /// Which clashes to move to which status, F54. Empty on every run while Q33 is
        /// open, because how the tool learns which clashes cannot be solved is not decided
        /// and guessing it would write a rule nobody agreed to into the only file that
        /// records what has been fixed.
        /// </summary>
        public IList<WantedStatus> WantedStatuses { get; set; }

        /// <summary>
        /// The penetration pass, F72, or null where the box is off. Null is the default
        /// and a null one resolves nothing and walks nothing.
        /// </summary>
        public Penetrations Penetrations
        {
            get { return penetrations; }
            set { penetrations = value; }
        }

        private Penetrations penetrations;

        /// <summary>
        /// Where the penetration decisions are counted for this group, or null where the
        /// box is off. The engine owns it, so one tally covers the group and the run total
        /// can be added up across every group.
        /// </summary>
        public PenetrationTally PenetrationTally { get; set; }

        /// <summary>
        /// The by design pass, F72b, or null where the box is off. Null is the default and
        /// a null one resolves nothing and walks nothing.
        /// </summary>
        public ByDesign ByDesign
        {
            get { return byDesign; }
            set { byDesign = value; }
        }

        private ByDesign byDesign;

        /// <summary>
        /// Where the by design decisions are counted for this group, or null where the
        /// box is off. The engine owns it, the same as the penetration tally.
        /// </summary>
        public ByDesignTally ByDesignTally { get; set; }

        /// <summary>
        /// Whether a status was actually changed in the document. The NWF is saved again
        /// on this, because a status change is a write.
        /// </summary>
        public bool ChangedAStatus
        {
            get { return changedTheDocument; }
        }

        /// <summary>
        /// Everything the file and the document disagree about, by test name. Read inside
        /// this class for the DRIFT block and nowhere else.
        /// </summary>
        private readonly List<TestDifference> drift = new List<TestDifference>();

        /// <summary>How many tests already in the document were compared against the file.</summary>
        public int Compared { get; private set; }

        /// <summary>
        /// How many sides were left out of the comparison because which set one end points
        /// at could not be read. Not compared is not the same as matching, so it is counted
        /// and said rather than left to read as agreement.
        /// </summary>
        public int SidesNotCompared { get; private set; }

        /// <summary>
        /// F204. Runs the plan with every item shown and hides again exactly what was hidden. Clash Detective finds no
        /// clash on a hidden item, so an NWF saved with models hidden had the clashes of those models marked Resolved by a
        /// refresh, 10016 of 13286 on a copy of C04 on 17 Oct 2026. EverythingShown does the showing and the putting back,
        /// in the words of Federator.Core.Clash.HiddenForTheClash.
        /// </summary>
        public ClashRunOutcome Run(ClashTestPlan plan)
        {
            using (new EverythingShown(
                NavisworksApplication.ActiveDocument,
                log,
                "the clash",
                HiddenForTheClash.Found,
                HiddenForTheClash.PutBack,
                HiddenForTheClash.Threw))
            {
                return RunTheTests(plan);
            }
        }

        /// <summary>
        /// Creates whatever the plan holds and runs it. The plan is resolved against the
        /// sets actually in the document first, so a locator naming a set that is not
        /// there skips its test by name rather than creating one with an empty side.
        /// </summary>
        private ClashRunOutcome RunTheTests(ClashTestPlan plan)
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
            SidesNotCompared = 0;
            toleranceOnCreated = 0;
            toleranceOnExisting = new SavedTestTolerances();
            documentUnits = null;
            merges = new List<MirrorMerge>();
            mergeOfKept.Clear();
            mergeOfMirror.Clear();
            pairOfMirror.Clear();
            MirrorNames = null;
            RunPlan = null;
            renameFailed.Clear();
            picturesWaiting.Clear();
            rowAddresses.Clear();
            clashesInDocument.Clear();
            savedSidesNotRead = 0;
            imagesStopSaid = false;

            Stopwatch stepClock = Stopwatch.StartNew();

            // Declared outside the try, so the one finally at the end releases the set
            // wrappers and the sources built beside them whichever way Run leaves.
            Dictionary<string, SelectionSet> byPath = null;

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
                documentUnits = UnitName(document.Units);
                log.Line("CLASH    ran against " + outcome.OpenDocument);
                log.Line("CLASH    the document measures in " + Words.Or(UnitName(document.Units), "UNKNOWN units")
                    + ", every tolerance was converted into it");

                DocumentSelectionSets sets = document.SelectionSets;
                byPath = IndexSets(sets);

                if (Images != null && !PicturesDeferred)
                {
                    modelsShown = new TestModelsShown(document, log, Images.OnlyTheTestsModels);
                }

                ClashTestPlan resolved;
                IList<string> savedToo = new List<string>();

                // F132. Every test the document holds, each side as the set it points at,
                // read over the index just built, so the mirror rule pairs saved tests by
                // their sides and judges an XML's pairs against what the document holds.
                ClashTestPlan inTheDocument = ClashTestPlan.FromDocument(
                    ReadTheSavedTestsWithSides(document), documentUnits);

                if (plan.Source == ClashPlanSource.Document)
                {
                    // The tests came out of this document, so their sides are already
                    // inside them. Nothing is resolved against the sets and nothing is
                    // created, so the guard for a document whose sets resolve nothing does
                    // not apply here. A side that finds nothing is still caught per test,
                    // below, the same as a test from a file. The plan run is the one just
                    // read with its sides, the same tests at the same addresses.
                    log.Line("CLASH    the document holds " + byPath.Count
                        + (byPath.Count == 1 ? " set" : " sets") + ", and the "
                        + inTheDocument.Buildable.Count + " saved tests keep the sides they were saved with");
                    resolved = inTheDocument;

                    // F132 attempt 2, the breaker's finding R5. The sides just read are the
                    // sets the saved tests point at, so the by design pass reaches the
                    // saved tests, where before F132 the placeholders matched no pair. The
                    // pass stays, Q144, and its block says where the sides came from.
                    if (byDesign != null && ByDesignTally != null)
                    {
                        ByDesignTally.SidesReadOffTheSavedTests(inTheDocument.Buildable.Count, byDesign.Pairs.Count);
                    }
                }
                else
                {
                    int expected = plan.DistinctLocators().Count;

                    log.Line("CLASH    the document holds " + byPath.Count
                        + (byPath.Count == 1 ? " set" : " sets") + " and the tests name "
                        + expected + (expected == 1 ? " set" : " sets"));

                    // Every test carried into the plan but naming a set that is not here is
                    // moved out by name, before anything is created.
                    resolved = plan.ResolveAgainst(byPath.Keys);

                    // The guard. A document with no sets in it once took a whole run to say
                    // so 1830 times over. If there were tests that could have run and not
                    // one of them resolves a set, nothing here can clash, so it says so and
                    // stops rather than creating tests that can only report zero.
                    if (plan.Buildable.Count > 0 && resolved.Buildable.Count == 0)
                    {
                        string why = outcome.StopBecauseNoSetResolves(byPath.Count, expected);
                        log.Line("CLASH    STOPPED  " + why);
                        progress("Clash stopped. " + why);
                        log.Line("CLASH    build the sets first, or pick a file whose tests name "
                            + "the sets this document already holds");
                        return outcome;
                    }

                    // F168, Bader's answer A to Q168. A test saved in the NWF that the matrix does not name,
                    // such as one a coordinator added by hand in Clash Detective, is run too, where it sits,
                    // after the tests of the matrix, at the tolerance of the drop down like every other test.
                    // It is in the reports and in the Coverage sheet like any other, said and recorded once the
                    // rule of two set codes below has passed it.
                    resolved = resolved.WithTheSavedTestsTheMatrixDoesNotName(inTheDocument, out savedToo);
                }

                // F224, Bader's answer to Q209. Only tests across two set codes run, on both pages, a test whose two sides carry
                // one set code left out of the plan before the mirrors are paired, created and run by nothing, and said with its
                // count on one CLASH line. One saved in the NWF keeps its earlier results there. In a group of one discipline
                // Q203 runs none of its tests, so the rule is not read and every test keeps Q203's one reason.
                int planned = resolved.Buildable.Count;

                if (SingleDisciplineGroup)
                {
                    log.Line(SetCodeRule.NotReadInAGroupOfOne(planned));
                }
                else
                {
                    IDictionary<string, int> leftOutByCode;
                    char separator = new Federator.Core.Views.ViewpointSettings().SetNameSeparator;
                    resolved = resolved.AcrossTwoSetCodes(SetCodes, separator, out leftOutByCode);
                    log.Line(SetCodeRule.Line(planned - resolved.Buildable.Count, planned, leftOutByCode, SetCodes != null && SetCodes.Count > 0,
                        SetCodeRule.WithASideOfNoCode(resolved.Buildable, SetCodes, separator)));
                }

                if (savedToo.Count > 0)
                {
                    HashSet<string> stillRun = new HashSet<string>(StringComparer.Ordinal);

                    foreach (PlannedClashTest test in resolved.Buildable)
                    {
                        stillRun.Add(test.Name);
                    }

                    List<string> runToo = new List<string>();

                    foreach (string savedName in savedToo)
                    {
                        if (stillRun.Contains(savedName))
                        {
                            runToo.Add(savedName);
                            outcome.AddSavedRunToo(savedName);
                        }
                    }

                    int ruleLeftOut = savedToo.Count - runToo.Count;
                    log.Line("CLASH    " + savedToo.Count
                        + (savedToo.Count == 1 ? " test" : " tests") + " saved in the NWF that the matrix does not name, "
                        + runToo.Count + " run too, where " + (runToo.Count == 1 ? "it sits" : "they sit") + ", after the tests of the matrix"
                        + (runToo.Count > 0
                            ? ": " + string.Join(", ", runToo.GetRange(0, Math.Min(runToo.Count, 10)).ToArray())
                                + (runToo.Count > 10 ? " and " + (runToo.Count - 10) + " more" : string.Empty)
                            : string.Empty)
                        + (ruleLeftOut > 0
                            ? ", and " + ruleLeftOut + " left out by the rule of two set codes, kept in the NWF with " + (ruleLeftOut == 1 ? "its" : "their") + " results"
                            : string.Empty));
                }

                // F132. The mirrored pairs among the tests to run, judged against the
                // document and this group's sets build, their lines, then the renames of
                // Q136 A made in the document by address before any test is found by name
                // or created, and the plan given each mirror's run name. Pairing that threw
                // is said and the tests run under the plan's names, each keeping its own
                // clashes.
                MirrorRule mirrored = PairTheMirrors(resolved, inTheDocument, outcome);
                MirrorNames = MirrorNamesOf(mirrored);
                WriteTheMirrorRows(mirrored);
                DocumentClashTests clashTests = document.GetClash().TestsData;

                if (mirrored != null)
                {
                    if (resolved.Source != ClashPlanSource.Document)
                    {
                        MakeTheRenames(clashTests, mirrored);
                    }

                    resolved = resolved.WithMirrorsNamed(mirrored);
                }

                RunPlan = resolved;

                foreach (SkippedClashTest test in resolved.Skipped)
                {
                    outcome.AddSkipped(test);
                    LogSkip(test);
                }

                // One row per test in the file, in file order, before anything runs. A
                // test that never runs still has to reach the Summary carrying why.
                reports = BuildReports(resolved);

                Dictionary<string, TestAddress> present = IndexTests(clashTests);
                IndexTheMerges(mirrored);

                // Always logged, even at zero. Whether tests survive from one group into
                // the next document is the question a slowdown turns on, and this is the
                // line that answers it from the log rather than from a theory.
                log.Line("CLASH    the document already holds " + present.Count
                    + (present.Count == 1 ? " clash test" : " clash tests")
                    + (present.Count == 0
                        ? ", so everything created here is new"
                        : ", they keep their results and are not recreated"));

                // F77. Which of the planned tests are worth CREATING. The sets are already
                // resolved, so how many items each locator finds is counted once per set,
                // and a test whose side finds nothing is not created at all rather than
                // created and thrown away by the side check moments later, which was 631
                // seconds of a 1424 second run.
                IList<PlannedClashTest> toRun = PlanTheCreation(document, byPath, present, resolved, outcome);

                RunEach(document, sets, clashTests, byPath, present, toRun, outcome);

                // F132. After the run has set every test's state and never before: each
                // merge into its kept test, its MIRROR lines and the kept test's ROWS line,
                // since the merge changes rows, statuses and numbers. F149: then the picture
                // of every row, test by test in the report's order, each under its final name,
                // whether or not any merge was made. A throw out of the merge loses no picture,
                // the breaker's fourth point on attempt 1, and is the error that goes on up, a throw
                // out of those pictures then written as a failure line, the second reading.
                try
                {
                    MergeTheMirrors(clashTests);
                }
                catch (Exception)
                {
                    // F182. Deferred pictures are taken later by the engine, which still finds them waiting.
                    if (!PicturesDeferred)
                    {
                        RenderThePicturesAfterTheMergeThrew(document, clashTests);
                    }

                    throw;
                }

                // F182, Bader's answer B to Q175. With the pictures deferred nothing is taken here: the engine
                // saves the NWF, publishes the NWD and only then calls TakeTheDeferredPictures, so a picture step
                // that grows Navisworks's memory, stops or dies leaves both files already written.
                if (!PicturesDeferred)
                {
                    RenderThePicturesWaiting(document, clashTests);

                    // FR-076. Every picture is taken above, so this is where the pictures' guard can
                    // stop the run, once every test of the group has run, F149.
                    StopIfTheImagesFailed(outcome);
                }

                // F76. One line per group, whichever way the choice reads.
                WriteTheToleranceLine();

                if (drift.Count > 0 || Compared > 0)
                {
                    log.Block("DRIFT " + Words.Or(outcome.OpenDocument, "this document"),
                        TestDrift.Lines(drift, Compared, ApplyFileSettings, SidesNotCompared));
                }

                SayTheResolvedStay(outcome);
            }
            finally
            {
                // It puts back any hidden state still changed and writes its one IMAGES line, in
                // its own try so the index below is released whatever it does.
                EndTheHidingOfTheModels();

                ReleaseTheIndex(byPath);
                stepClock.Stop();
                outcome.Seconds = stepClock.Elapsed.TotalSeconds;
            }

            return outcome;
        }

        /// <summary>
        /// Puts back any hidden state the picture pass left changed and writes the one IMAGES line of the
        /// hiding, in its own try so the caller's own release is reached whatever it does.
        /// </summary>
        private void EndTheHidingOfTheModels()
        {
            if (modelsShown == null)
            {
                return;
            }

            try
            {
                modelsShown.Dispose();
            }
            catch (Exception error)
            {
                log.Failure(
                    "ending the clash step's hiding of the models for the pictures",
                    error,
                    "kept going, the sets index is released, and whatever is still hidden is carried into the views, the NWF and the NWD");
            }
            finally
            {
                modelsShown = null;
            }
        }

        /// <summary>
        /// Whether Run leaves the picture pass to TakeTheDeferredPictures, which the engine calls after the NWF is
        /// saved and the NWD published. No clash result is removed by this tool, so no result moves between Run and
        /// the pictures, and a picture is found again by its test address and the index path of its result.
        /// </summary>
        public bool PicturesDeferred
        {
            get { return DeferPictures && Images != null; }
        }

        /// <summary>Whether rows are waiting for a picture that TakeTheDeferredPictures would take.</summary>
        public bool HasPicturesWaiting
        {
            get { return PicturesDeferred && picturesWaiting.Count > 0 && Report != null; }
        }

        /// <summary>
        /// F182, Bader's answer B to Q175. The picture pass, taken after Run once the engine has saved the NWF and
        /// published the NWD. It resolves each test again by its address and name, as the pass always did, so the
        /// save and the publish between do not matter to it. A guard stop is put on the outcome as Run would
        /// have. Every throw is the caller's: the rows not yet pictured keep an empty Image cell.
        ///
        /// F206. By now Run has hidden again what the document hid, so every item is shown for the pictures as it is for
        /// the clash and hidden again after them, put back once the hiding of each test's other models has ended. The
        /// audit round's run 2 on a copy of C04 saved with 8 of 10 models hidden had 744 of 3080 pictures empty without it.
        /// </summary>
        public void TakeTheDeferredPictures(ClashRunOutcome outcome)
        {
            if (!HasPicturesWaiting)
            {
                return;
            }

            EverythingShown everything = null;

            try
            {
                Document document = NavisworksApplication.ActiveDocument;

                if (document == null)
                {
                    log.Line("IMAGES   stopped, there is no active document, so no picture was taken");
                    return;
                }

                everything = new EverythingShown(
                    document,
                    log,
                    "the pictures",
                    HiddenForThePictures.Found,
                    HiddenForThePictures.PutBack,
                    HiddenForThePictures.Threw);
                modelsShown = new TestModelsShown(document, log, Images.OnlyTheTestsModels);
                DocumentClashTests clashTests = document.GetClash().TestsData;

                RenderThePicturesWaiting(document, clashTests);
                StopIfTheImagesFailed(outcome);
            }
            finally
            {
                EndTheHidingOfTheModels();
                picturesWaiting.Clear();

                if (everything != null)
                {
                    everything.Dispose();
                }
            }
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
                    // F155. The XML's tolerance is never read into a run, so a row that never ran
                    // carries the value chosen in the drop down, as every test is set to.
                    Tolerance.PutOn(report, documentUnits);
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
        /// Says the Resolved clashes stay in the file and keep counting. F185, Bader's order of 14 Oct 2026, task 5.2:
        /// nothing in this tool removes a clash result. The Compact resolved option and its call of
        /// TestsCompactAllTests are gone, because a compact deletes every Resolved clash of every test and with it
        /// the history a coordinator keeps. A clash that is gone from a test turns Resolved and stays Resolved.
        /// </summary>
        private void SayTheResolvedStay(ClashRunOutcome outcome)
        {
            int before = outcome.Totals.Of(CoreClashStatus.Resolved);

            if (before > 0)
            {
                log.Line("CLASH    " + before
                    + " clashes are Resolved and stay in the file, counting. This tool never removes a clash result.");
            }
        }

        private void RunEach(
            Document document,
            DocumentSelectionSets sets,
            DocumentClashTests clashTests,
            Dictionary<string, SelectionSet> byPath,
            Dictionary<string, TestAddress> present,
            IList<PlannedClashTest> toRun,
            ClashRunOutcome outcome)
        {
            int total = toRun.Count;

            // FR-076. Nothing in this loop takes a picture since F149, so the pictures' guard cannot
            // fire here, and it is read once before the first test where it was read after each one.
            // It can read fired only where an earlier group fired it and its stop never reached the
            // engine, its clash step having thrown after its pictures, and then this group runs no
            // test for pictures the writer would not take.
            if (StopIfTheImagesFailed(outcome))
            {
                return;
            }

            for (int i = 0; i < total; i++)
            {
                PlannedClashTest planned = toRun[i];
                progress("Test " + (i + 1) + " of " + total + ": " + planned.Name);

                OneTest(document, sets, clashTests, byPath, present, planned, outcome);

                // A running count, so a run of well over a thousand tests is watched rather
                // than silent. Taken AFTER the test it numbers, F113, so the last one of a
                // group reads the same numbers as the block under it. How often is ProgressSettings.CountEveryTests, F227.
                string running = outcome.ProgressAfter(i + 1, total, Settings().CountEveryTests);

                if (running != null)
                {
                    log.Line("CLASH    " + running);
                }

                // Nine hours produced nothing once because nothing watched for this. A run
                // failing uniformly stops the run, not the group.
                if (guard.ShouldStopTheRun)
                {
                    outcome.StopTheWholeRun(guard.Reason);
                    log.Line("CLASH    RUN STOPPED  " + guard.Reason);

                    // The log gets what was thrown, the label gets plain words.
                    progress(guard.ReasonInPlainWords);
                    return;
                }
            }
        }

        /// <summary>
        /// FR-076. The pictures' guard is one for the whole run, like the tests', and it is
        /// read here once the picture pass has run, which since F149 is after every test of
        /// the group has run, and once before a group's first test. So a run whose pictures
        /// all fail the same way runs every test of the group the fiftieth failure in a row
        /// fell in, then stops at the end of that group's picture pass, where before F149 it
        /// stopped after the test the fiftieth fell in. The pass and the writer take, resolve
        /// and count nothing once it has fired. A run the tests' guard already stopped is left
        /// with that reason, and the pictures' guard firing as well is said on an IMAGES line
        /// with its own reason, the second reading, where before nothing said it. Said once a
        /// group, whichever of the two reads find it fired.
        /// </summary>
        private bool StopIfTheImagesFailed(ClashRunOutcome outcome)
        {
            if (Images == null || !Images.ShouldStopTheRun || imagesStopSaid)
            {
                return false;
            }

            imagesStopSaid = true;

            if (outcome.StopTheRun)
            {
                log.Line("IMAGES   the pictures' guard fired as well, after the tests' guard had stopped the run, whose reason "
                    + "stays the run's. " + Images.StopReason);
                return true;
            }

            outcome.StopTheWholeRun(Images.StopReason);
            log.Line("IMAGES   RUN STOPPED  " + Images.StopReason);
            progress(Images.StopReasonInPlainWords);
            return true;
        }

        /// <summary>Whether this group's log already says the pictures' guard fired, so it is said once, F149.</summary>
        private bool imagesStopSaid;

        private void OneTest(
            Document document,
            DocumentSelectionSets sets,
            DocumentClashTests clashTests,
            Dictionary<string, SelectionSet> byPath,
            Dictionary<string, TestAddress> present,
            PlannedClashTest planned,
            ClashRunOutcome outcome)
        {
            // F236 on the reviewer's second reading. Clash Detective's count, kept outside the try so a throw after it, in the
            // harvest, still hands it to the clash count check and the test is never dropped from it in silence.
            int counted = -1;

            try
            {
                TestAddress address;

                // F132, Q136 A. The saved test this one was to run as could not be renamed,
                // so it is left as it is and no test of this name is created beside it, which
                // would run its question twice and count a clash both find twice.
                if (renameFailed.Contains(planned.Name))
                {
                    string notRenamed = "the saved test it runs as could not be renamed to this name, so that test is "
                        + "left as it is and no test of this name is created beside it";
                    TestReport notRun = ReportFor(planned);

                    if (notRun != null)
                    {
                        notRun.State = TestState.Skipped;
                        notRun.SkippedReason = notRenamed;
                    }

                    LogSkip(outcome.AddSkipped(planned.Name, ClashSkipReason.Failed, notRenamed));
                    guard.RecordNotAttempted();
                    return;
                }

                if (planned.IsFromDocument)
                {
                    // A test read out of the document, run where it was found. Its
                    // results are its own and there is no file to compare it against, so
                    // it is neither created nor checked for drift.
                    address = TestAddress.At(planned.Address);
                    outcome.AddAlreadyPresent(planned.Name);
                    ApplyChosenTolerance(clashTests, address, planned);
                }
                else if (present.TryGetValue(planned.Name, out address))
                {
                    // An OPENED group keeps its results, so a test already there is left
                    // exactly as it is. Rebuilding it would reset every clash to New and
                    // throw away every Active and Resolved, which is the whole reason the
                    // NWF is never cleared either.
                    //
                    // Left alone means a setting changed in the file never reaches it, so the
                    // two are compared and every difference is reported by name. The tolerance
                    // is the one exception, set to the chosen value on every test since F155.
                    outcome.AddAlreadyPresent(planned.Name);
                    CompareAndMaybeApply(clashTests, address, planned, byPath, sets, outcome);
                    ApplyChosenTolerance(clashTests, address, planned);
                }
                else
                {
                    using (RunStep creating = log.Step(RunSteps.TestsCreate))
                    {
                        StepStarted(RunSteps.TestsCreate);

                        try
                        {
                            address = Create(sets, clashTests, byPath, planned);
                            creating.Changed(planned.Name);
                        }
                        finally
                        {
                            StepEnded();
                        }
                    }

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
                    toleranceOnCreated++;

                    // FR-036. Created at the chosen value, ToleranceFor, so a row that then does
                    // not run says that value and not the plan's.
                    Tolerance.PutOn(ReportFor(planned), documentUnits);
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

                    using (ClashSelection left = before.SelectionA)
                    using (ClashSelection right = before.SelectionB)
                    {
                        leftItems = ItemsOn(document, left, byPath, planned.Left.Locator);
                        rightItems = ItemsOn(document, right, byPath, planned.Right.Locator);
                    }
                }

                // F127. The two counts read off this test's own sides, which decide both skips
                // below, kept on the record so the coverage gives a skipped test the reason the
                // counts that skipped it give.
                outcome.RecordSides(planned.Name, leftItems, rightItems);

                TestReport summary = ReportFor(planned);

                if (summary != null)
                {
                    summary.LeftItems = leftItems;
                    summary.RightItems = rightItems;
                }

                if (SingleDisciplineGroup)
                {
                    // Created because both its sides find something, F77, so a later run
                    // against a fuller model finds it already there. Not run, because there is
                    // nothing here for it to run against.
                    LogSkip(outcome.AddSkipped(
                        planned.Name,
                        ClashSkipReason.SingleDiscipline,
                        "the group holds one discipline, so there is nothing for this test to clash against"));
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

                // One clock and not two. The step IS the measurement of this test, so the
                // seconds the log reports and the seconds the report row carries come off
                // the same reading and cannot disagree. It is a Stopwatch underneath,
                // monotonic, and never two wall clock readings subtracted.
                double ranFor;

                using (RunStep running = log.Step(RunSteps.TestsRun))
                {
                    StepStarted(RunSteps.TestsRun);

                    try
                    {
                        using (ClashTest test = Resolve(clashTests, address, planned.Name))
                        {
                            if (test == null)
                            {
                                running.Failed();
                                Failed(outcome, planned.Name, "the test is no longer where it was put");
                                return;
                            }

                            // F143. The call alone is timed and named where it took the setting's
                            // seconds or more, in the finally, so a run that throws is named too.
                            double from = log.ElapsedSeconds;

                            try
                            {
                                clashTests.TestsRunTest(test);
                            }
                            finally
                            {
                                LongCall.Say(log, "TestsRunTest", planned.Name, log.ElapsedSeconds - from, Settings().LongCallSeconds);
                            }
                        }
                    }
                    finally
                    {
                        StepEnded();
                    }

                    running.Changed(planned.Name);
                    ranFor = running.SecondsSoFar();
                }

                // F54. Between the run and the harvest, and never after the clash step.
                // ClashHarvest reads a result's status while building the report rows, so
                // a status applied later would leave the workbook and the page carrying
                // the status read BEFORE the change.
                //
                // Its own resolve, because TestsEditResultStatus is a mutator and every
                // mutator on DocumentClashTests is a copy form that kills the handle handed
                // to it. Sharing the handle with the count below is exactly the fault that
                // threw once per test for 8 hours 52 minutes. Nothing is resolved at all
                // when no status is wanted, which is every run while Q33 is open.
                // F72. The penetration rule decides WHICH clashes of this test should move,
                // by reading both sides of each one, and it runs here for the same reason
                // F54's slot is here. Its own resolve again, and a walk that only reads,
                // so nothing is holding a handle across the mutator below.
                //
                // On by default since F235, Q195. A run with the box off resolves nothing and
                // walks nothing and costs exactly what it cost before F72.
                IList<WantedStatus> wanted = WantedStatuses;

                if (penetrations != null && PenetrationTally != null)
                {
                    using (ClashTest toRead = Resolve(clashTests, address, planned.Name))
                    {
                        if (toRead != null)
                        {
                            wanted = penetrations.WantedFor(document, toRead, PenetrationTally);
                        }
                    }
                }

                // F72b. The by design pairs, judged AFTER the penetration rule so that rule
                // keeps a clash they both want, and the wanted names join the ONE list the
                // editor applies below. Its own resolve and a walk that only reads, like
                // the penetration pass, and it reads no item at all.
                if (byDesign != null && ByDesignTally != null)
                {
                    using (ClashTest forPairs = Resolve(clashTests, address, planned.Name))
                    {
                        if (forPairs != null)
                        {
                            List<WantedStatus> merged = new List<WantedStatus>();

                            if (wanted != null)
                            {
                                merged.AddRange(wanted);
                            }

                            merged.AddRange(byDesign.WantedFor(
                                forPairs, planned.Left.Locator, planned.Right.Locator, wanted, ByDesignTally));
                            wanted = merged;
                        }
                    }
                }

                if (statuses != null && wanted != null && wanted.Count > 0)
                {
                    using (ClashTest toEdit = Resolve(clashTests, address, planned.Name))
                    {
                        if (toEdit != null && statuses.Apply(document, clashTests, toEdit, wanted))
                        {
                            changedTheDocument = true;
                        }
                    }
                }

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
                    counted = tally.Total;

                    // Read now, in the same pass, because the results are only readable
                    // while this document is open and this handle is fresh.
                    if (summary != null)
                    {
                        summary.Seconds = ranFor;

                        // The client's Status column on the test header. Whatever the API
                        // reports, written as itself. Theirs says OK, which is not a value
                        // on ClashTestStatus, and what puts a test into any of the four is
                        // UNKNOWN, so nothing is translated. See docs\history\scan.md section 4j.
                        summary.StatusWord = after.Status.ToString();

                        // F76. The tolerance on the row is READ off the test in the
                        // document, because the document is what produced the row. It was
                        // copied off the plan, which is the XML, so on every weekly run the
                        // cell said what the file asked for while the run had clashed at
                        // what the document held.
                        summary.Tolerance = after.Tolerance;
                        summary.ToleranceFrom = ToleranceOrigin.Document;

                        ClashHarvest harvest = new ClashHarvest(log);
                        HandTheProgress(harvest, planned.Name);

                        // F132. A test in a merge hands every clash to it as the rows are
                        // read, and the merge then changes which rows the report holds, their
                        // statuses and their numbers.
                        MirrorMerge asKept;
                        MirrorMerge asMirror;
                        MirrorPair pair;

                        mergeOfKept.TryGetValue(planned.Name, out asKept);
                        mergeOfMirror.TryGetValue(planned.Name, out asMirror);
                        pairOfMirror.TryGetValue(planned.Name, out pair);
                        harvest.MergeAsKept = asKept;
                        harvest.MergeAsMirror = asMirror;
                        harvest.AsMirror = pair;

                        // Every row is recorded with where its result sits, attempt 2, so
                        // the views resolve each row of the merged report by it. F149: the
                        // harvest takes no picture. Every picture waits until every test has
                        // run and every merge is made, and is then taken test by test in the
                        // report's order, so each is written under its final name.
                        harvest.Recorded = new List<RowUnderTest>();
                        bool readWhole = false;

                        using (RunStep harvesting = log.Step(RunSteps.Harvest))
                        {
                            StepStarted(RunSteps.Harvest);

                            try
                            {
                                harvest.Into(document, after, summary);
                                readWhole = true;
                                harvesting.Changed(planned.Name);
                            }
                            finally
                            {
                                StepEnded();
                                KeepWhereTheRowsCameFrom(address, planned.Name, harvest.Recorded, readWhole);
                            }
                        }

                        // F21. What the workbook got beside what the document holds, per
                        // test, so criterion 3 can be checked off the log instead of by
                        // opening Excel and Navisworks side by side for every one of
                        // 1830 tests. The wording is Core and the rule about what the
                        // difference means is there with it.
                        // F81. NumberedRepeat and not Numbered: one run wrote 936 of these
                        // and every one said the two numbers agree. The ROW is still
                        // written for every single test, because the .tsv keeps everything
                        // and only the .log is trimmed. The key is what the line SAYS and
                        // never the sentence, because the sentence carries the test name
                        // and the names are exactly what varies.
                        // F132. The kept test of a merge writes its line after the merge,
                        // with the clashes only its mirrors found, MergeTheMirrors.
                        if (asKept != null)
                        {
                            clashesInDocument[planned.FileIndex] = tally.Total;
                        }
                        else
                        {
                            WriteTheRowsLine(planned.Name, summary.Rows.Count, tally.Total, 0);
                        }

                        summary.State = summary.HasRows
                            ? TestState.FoundClashes
                            : TestState.Passed;
                    }
                }

                ClashTestResult result = outcome.AddRan(
                    planned.Name, leftItems, rightItems, tally, ranFor);

                guard.RecordSuccess();

                // F81. A test that found CLASHES gets its own line every time, because
                // every one of those says something different. A test that PASSED says
                // the same sentence about a different name, 792 times in one run, so it
                // collapses. The row is written either way.
                log.NumberedRepeat(
                    result.Passed ? "CLASH passed, found nothing" : "CLASH found clashes",
                    "CLASH    " + result.Line(),
                    result.Passed ? RunEvents.TestPassed : RunEvents.TestFoundClashes,
                    planned.Name,
                    EventRow.Count(tally.Total),
                    result.Passed ? "ran and found nothing" : "ran and found clashes");
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

                if (counted >= 0)
                {
                    log.Row(RunEvents.TestFailedAfterItsCount, planned.Name, EventRow.Count(counted),
                        "Clash Detective counted its clashes, then the test threw: " + reason);
                }
            }
        }

        /// <summary>
        /// Where each row the harvest recorded came from, as it ends, F149. Every row is kept for
        /// its picture whichever way the harvest ended, the breaker's fifth point on attempt 1, so
        /// the rows a harvest read before it threw keep their pictures as they did before F149.
        /// The views are handed the rows of a harvest that read its test whole, as before, so a
        /// test whose harvest threw is still not read for the views, which their own line says.
        /// </summary>
        private void KeepWhereTheRowsCameFrom(TestAddress address, string testName, IList<RowUnderTest> recorded, bool readWhole)
        {
            foreach (RowUnderTest row in recorded)
            {
                RowAddress where = new RowAddress(address, testName, row);

                if (readWhole)
                {
                    rowAddresses[row.Row] = where;
                }

                if (Images != null)
                {
                    picturesWaiting[row.Row] = where;
                }
            }
        }

        /// <summary>
        /// Compares one test already in the document against what the file says it should
        /// be, and reports every difference by name. Changes nothing unless
        /// ApplyFileSettings is on, because changing a test changes which clashes it finds
        /// next time it runs. It does not reset anything by itself, 5y.
        /// </summary>
        private void CompareAndMaybeApply(
            DocumentClashTests clashTests,
            TestAddress address,
            PlannedClashTest planned,
            Dictionary<string, SelectionSet> byPath,
            DocumentSelectionSets sets,
            ClashRunOutcome outcome)
        {
            using (ClashTest test = Resolve(clashTests, address, planned.Name))
            {
                if (test == null)
                {
                    return;
                }

                Compared++;
                ReportStatus(test, planned.Name);

                TestSettings inFile = TestSettings.FromFile(planned);
                // F155. The file's settings carry the tolerance chosen in the drop down, never the XML's.
                inFile.Tolerance = ToleranceFor(planned);
                TestSettings inDocument = SettingsOf(test);

                SidesNotCompared += TestDrift.SidesNotCompared(inFile, inDocument);

                IList<TestDifference> differences = TestDrift.Compare(
                    planned.Name, inFile, inDocument);

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
            Apply(clashTests, address, planned, byPath, sets, outcome);
        }

        /// <summary>
        /// What the test in the document is actually set to. The side locators are read
        /// back by matching each side's sources against the one source per set built when
        /// the sets were indexed.
        /// </summary>
        private TestSettings SettingsOf(ClashTest test)
        {
            TestSettings settings = new TestSettings();
            settings.Tolerance = test.Tolerance;
            settings.TestType = (ClashTestKind)(int)test.TestType;
            settings.TestTypeName = test.TestType.ToString();
            settings.MergeComposites = test.MergeComposites;

            // Each read of SelectionA or SelectionB creates a wrapper. Four reads a side
            // over 1830 compared tests is 14,640 handles, so each side is read once.
            using (ClashSelection left = test.SelectionA)
            using (ClashSelection right = test.SelectionB)
            {
                settings.LeftSelfIntersect = left.SelfIntersect;
                settings.RightSelfIntersect = right.SelfIntersect;
                settings.LeftPrimitiveTypes = (int)left.PrimitiveTypes;
                settings.RightPrimitiveTypes = (int)right.PrimitiveTypes;

                settings.LeftLocator = LocatorOf(left);
                settings.RightLocator = LocatorOf(right);
            }

            return settings;
        }

        /// <summary>
        /// Which set path a side points at, found by matching the side's sources against
        /// the one source per set that IndexSets built. Empty when the side points at
        /// something that is not one of them, which is a difference worth reporting
        /// rather than hiding.
        ///
        /// It used to create a source for every indexed set, per side, per compared test,
        /// and dispose each straight after. On a weekly run plus XML that is every test
        /// compared, and 61 sets by two sides by 1830 tests is 223,260 sources made and
        /// thrown away for the comparison alone.
        /// </summary>
        private string LocatorOf(ClashSelection side)
        {
            try
            {
                using (Selection selection = side.Selection)
                {
                    SelectionSourceCollection sources = selection.SelectionSources;

                    if (sources == null || sources.Count == 0)
                    {
                        return string.Empty;
                    }

                    foreach (KeyValuePair<string, SelectionSource> pair in sourceByPath)
                    {
                        for (int i = 0; i < sources.Count; i++)
                        {
                            if (sources[i].Equals(pair.Value))
                            {
                                return pair.Key;
                            }
                        }
                    }

                    return string.Empty;
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading which set a clash side points at",
                    error,
                    "kept going, that side is reported as UNKNOWN rather than as changed");
                return TestSettings.UnknownLocator;
            }
        }

        /// <summary>
        /// Puts the file settings onto a test already in the document. This changes WHICH
        /// CLASHES it finds when it next runs, which is why it is off by default and said
        /// in the log. It RESETS NOTHING by itself, measured 5y, and this comment said it
        /// reset the results until that was read.
        /// </summary>
        private void Apply(
            DocumentClashTests clashTests,
            TestAddress address,
            PlannedClashTest planned,
            Dictionary<string, SelectionSet> byPath,
            DocumentSelectionSets sets,
            ClashRunOutcome outcome)
        {
            try
            {
                using (ClashTest replacement = new ClashTest())
                {
                    replacement.DisplayName = planned.Name;
                    replacement.TestType = (ClashTestType)(int)planned.TestType;
                    replacement.Tolerance = ToleranceFor(planned);
                    replacement.MergeComposites = planned.MergeComposites;

                    using (ClashSelection left = replacement.SelectionA)
                    using (ClashSelection right = replacement.SelectionB)
                    {
                        FillSide(sets, left, planned.Left, byPath);
                        FillSide(sets, right, planned.Right, byPath);
                    }

                    using (ClashTest existing = Resolve(clashTests, address, planned.Name))
                    {
                        if (existing == null)
                        {
                            return;
                        }

                        clashTests.TestsEditTestFromCopy(existing, replacement);
                    }
                }

                // FR-150. A write, so it asks for the NWF to be saved again even where the
                // test is then skipped and nothing runs.
                outcome.AddApplied();
                log.Line("CLASH    APPLIED  " + planned.Name
                    + "  the file's settings were put onto the test in the document, whose "
                    + "results and statuses are kept, measured 5y");
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
        /// What one test's tolerance is set to, F76: the chosen value converted into the
        /// document units, since F155 on every test, the XML's never used. A
        /// unit the table does not know throws here and the test is then skipped by name
        /// through the catch in OneTest, which is F33's rule and never a fallback.
        /// </summary>
        private double ToleranceFor(PlannedClashTest planned)
        {
            return Tolerance.InDocumentUnits(documentUnits);
        }

        /// <summary>The words on the created line, saying which of the two won.</summary>
        private string DescribeSetTolerance(PlannedClashTest planned, double set)
        {
            return Tolerance.Label() + " chosen in the tool is " + Plain(set) + " "
                + Words.Or(documentUnits, "UNKNOWN units");
        }

        /// <summary>
        /// Puts the chosen tolerance onto a test already in the document, F76, and counts
        /// the test by what happened to it, FR-053, where it was counted as set before it
        /// was even found. A test already at the
        /// chosen value is left exactly as it is, because an edit resets its results and
        /// there would be nothing to change. The copy is the test's OWN, through
        /// SavedItem.CreateCopy, so only the tolerance moves and the file's other settings
        /// never ride in on it, which is what ApplyFileSettings is for.
        /// </summary>
        private void ApplyChosenTolerance(DocumentClashTests clashTests, TestAddress address, PlannedClashTest planned)
        {
            string name = planned.Name;

            // Threw until a line below says otherwise, so a test is counted once whichever
            // way it leaves, and one whose edit went through before a log line threw is set.
            SavedTestTolerance what = SavedTestTolerance.Threw;

            try
            {
                double wanted = Tolerance.InDocumentUnits(documentUnits);

                using (ClashTest existing = Resolve(clashTests, address, name))
                {
                    if (existing == null)
                    {
                        what = SavedTestTolerance.NotFound;
                        return;
                    }

                    double already = existing.Tolerance;

                    if (Math.Abs(already - wanted) <= TestDrift.ToleranceEpsilon)
                    {
                        what = SavedTestTolerance.AlreadyAtTheValue;

                        // FR-036. Already at the chosen value, so a row that does not run says it.
                        Tolerance.PutOn(ReportFor(planned), documentUnits);
                        return;
                    }

                    using (SavedItem copied = existing.CreateCopy())
                    {
                        ClashTest copy = copied as ClashTest;

                        if (copy == null)
                        {
                            what = SavedTestTolerance.WouldNotCopy;
                            log.Line("CLASH    " + name + "  the test would not copy as a clash test, so "
                                + "its tolerance stays at " + Plain(already) + " and not the chosen "
                                + Plain(wanted));
                            return;
                        }

                        copy.Tolerance = wanted;
                        clashTests.TestsEditTestFromCopy(existing, copy);
                    }

                    what = SavedTestTolerance.Set;
                    changedTheDocument = true;

                    // FR-036. The row was given the plan's value, the old one read off the
                    // document or the XML's, and a test skipped below never reaches the reading
                    // off the document, so it said a value the document no longer holds.
                    Tolerance.PutOn(ReportFor(planned), documentUnits);

                    log.NumberedRepeat(
                        "CLASH tolerance set on a saved test",
                        "CLASH    TOLERANCE " + name + "  " + Plain(already) + " to " + Plain(wanted) + " "
                            + Words.Or(documentUnits, string.Empty)
                            + ", chosen in the tool. Its results and its statuses are KEPT, measured 5y",
                        "tolerance set on a saved test",
                        name,
                        EventRow.Tolerance(wanted),
                        "was " + Plain(already));
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "setting the chosen tolerance on " + name,
                    error,
                    "kept going, that test keeps the tolerance it had");
            }
            finally
            {
                toleranceOnExisting.Record(what);
            }
        }

        /// <summary>
        /// The one TOLERANCE line per group, F76. Both counts are read and the converted
        /// value is on it, so nobody does the arithmetic to find out what 25 mm was in a
        /// document measuring feet. A unit the table does not know cannot be converted,
        /// and then the counts are still said.
        /// </summary>
        private void WriteTheToleranceLine()
        {
            try
            {
                log.Line(Tolerance.LogLine(RunPlan.Source, toleranceOnCreated, toleranceOnExisting, documentUnits));
            }
            catch (Exception error)
            {
                log.Failure(
                    "writing the tolerance line",
                    error,
                    "kept going, the counts are " + toleranceOnCreated + " created and "
                        + toleranceOnExisting.Total + " already in the document");
            }
        }

        private static string Plain(double value)
        {
            return value.ToString("0.############", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// What the document says about this test's state.
        ///
        /// ClashTest.Status is the only thing on the type that could carry it, and its
        /// enum is New, Old, Partial, Complete. What puts a test into Old is UNKNOWN from
        /// the DLL, so the status is reported as itself and no sentence is put on it. See
        /// docs\history\scan.md section 4j.
        /// </summary>
        private void ReportStatus(ClashTest test, string name)
        {
            try
            {
                if (test.Status == ClashTestStatus.Old)
                {
                    // The word and nothing else. What Navisworks means by it is UNKNOWN, so
                    // saying what it means would be inventing the meaning.
                    log.Line("CLASH    OLD      " + name + "  Navisworks has this test marked "
                        + ClashTestStatus.Old + ".");
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
            DocumentSelectionSets sets,
            DocumentClashTests clashTests,
            Dictionary<string, SelectionSet> byPath,
            PlannedClashTest planned)
        {
            int before = clashTests.Tests.Count;
            double set = ToleranceFor(planned);

            using (ClashTest test = new ClashTest())
            {
                test.DisplayName = planned.Name;
                test.TestType = (ClashTestType)(int)planned.TestType;
                test.Tolerance = set;
                test.MergeComposites = planned.MergeComposites;

                using (ClashSelection left = test.SelectionA)
                using (ClashSelection right = test.SelectionB)
                {
                    FillSide(sets, left, planned.Left, byPath);
                    FillSide(sets, right, planned.Right, byPath);
                }

                clashTests.TestsAddCopy(test);
            }

            // F81. One run wrote 10,980 of these, three lines for every test created, and
            // every one said the same thing about a different name. The row carries all of
            // it and the .tsv keeps every row.
            //
            // It was a Detail line, which is indented 26 spaces and writes NO row. It is a
            // plain line now, on purpose: one of five examples of a repeated event reads
            // at the left margin like every other collapsed example in this log, and a
            // Detail with nothing above it to be a detail OF is just an indented line.
            log.NumberedRepeat(
                "CLASH created",
                "CLASH    created  " + planned.Name
                    + "  " + planned.TestTypeName
                    + "  tolerance " + DescribeSetTolerance(planned, set)
                    + "  merge composites " + (planned.MergeComposites ? "on" : "off"),
                "test created",
                planned.Name,
                EventRow.Tolerance(set),
                planned.TestTypeName + ", " + DescribeSetTolerance(planned, set));

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
            string nowNamed;
            ClashTest test = address.ResolveIn(clashTests, name, out nowNamed);

            if (nowNamed != null)
            {
                log.Line("CLASH    the test at " + address + " is now \"" + nowNamed
                    + "\" and not \"" + name + "\", so it was left alone");
            }

            return test;
        }

        /// <summary>
        /// Points one side at the saved set the locator names, rather than copying a list
        /// of items into it. CreateSelectionSource is the only way to make a SelectionSource,
        /// its constructors are not public, and pointing at the set is what Clash Detective
        /// itself does, so the counts match the panel.
        ///
        /// A fresh source every time and never one out of the index, because the
        /// collection takes the source it is handed and releasing the index at the end of
        /// Run would then take it back out from under the test.
        /// </summary>
        private void FillSide(
            DocumentSelectionSets sets,
            ClashSelection side,
            PlannedClashSide planned,
            Dictionary<string, SelectionSet> byPath)
        {
            side.SelfIntersect = planned.SelfIntersect;
            side.PrimitiveTypes = PrimitiveTypesFrom(planned.PrimitiveTypes, planned.Locator);

            SelectionSet set = byPath[planned.Locator];

            using (Selection selection = side.Selection)
            {
                selection.Clear();

                // The source is handed to the collection, which takes it from here. Disposing
                // it after the Add would take it back out from under the test.
                selection.SelectionSources.Add(sets.CreateSelectionSource(set));
            }
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

            using (Selection selection = side.Selection)
            using (ModelItemCollection found = selection.GetSelectedItems(document))
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
        /// Tells the live line which step this is, where one was supplied, and renders it
        /// once. Null is the ordinary case for the two hand buttons and costs one
        /// comparison.
        /// </summary>
        private void StepStarted(string name)
        {
            if (Live == null)
            {
                return;
            }

            Live.StepStarted(name);

            // The EMPTY sentence and not the rendered line. The callback handed in is the
            // engine's throttle, which asks the line whether it is worth rendering, and
            // rendering it here would mark it as said and make the throttle skip the one
            // render that matters. A step that just started always answers yes.
            progress(string.Empty);
        }

        private void StepEnded()
        {
            if (Live != null)
            {
                Live.StepEnded();
            }
        }

        /// <summary>
        /// F143. The harvest is handed the runner's callback, so the live line moves and the
        /// window answers between two rows, the PROGRESS seconds, the test it reads, and the
        /// seconds from which one render is named on a CALL line.
        /// </summary>
        private void HandTheProgress(ClashHarvest harvest, string testName)
        {
            harvest.Tick = progress;
            harvest.Progress = Settings();
            harvest.TestName = testName;

            if (Images != null)
            {
                Images.LongCallSeconds = Settings().LongCallSeconds;
            }
        }

        /// <summary>The runner's ProgressSettings, its defaults where a caller set Progress to null.</summary>
        private ProgressSettings Settings()
        {
            if (Progress == null)
            {
                Progress = new ProgressSettings();
            }

            return Progress;
        }

        /// <summary>
        /// Writes a skip into the log, at most MaxSkipExamples of them for each reason,
        /// then one line saying the rest are counted. Everything skipped still reaches the
        /// block at the end, where it is counted by reason with its examples.
        /// </summary>
        /// <summary>
        /// The tests to run, F77: every test already in the document or read out of it,
        /// plus the planned ones Federator.Core.Clash.CreationPlan says are worth creating.
        /// The rest are fed into the same skip machinery every other reason goes through
        /// and their report rows carry why, because the workbook still carries a block for
        /// every test in the file. File order is kept, so the NWF's tests sit in the order
        /// the file lists them. A test already in the document is not the plan's to
        /// create, so it is never handed to it and still gets compared and run.
        /// </summary>
        private IList<PlannedClashTest> PlanTheCreation(
            Document document,
            Dictionary<string, SelectionSet> byPath,
            Dictionary<string, TestAddress> present,
            ClashTestPlan resolved,
            ClashRunOutcome outcome)
        {
            List<PlannedClashTest> toRun = new List<PlannedClashTest>();

            if (resolved.Source == ClashPlanSource.Document)
            {
                toRun.AddRange(resolved.Buildable);
                return toRun;
            }

            List<PlannedClashTest> absent = new List<PlannedClashTest>();

            foreach (PlannedClashTest planned in resolved.Buildable)
            {
                if (!present.ContainsKey(planned.Name))
                {
                    absent.Add(planned);
                }
            }

            Dictionary<string, int> itemsByLocator =
                CountItemsPerSet(document, byPath, resolved.DistinctLocators());

            // F127. The very counts the creation plan decides on, kept on the record, so the
            // coverage reads why a test was not created off what decided it.
            outcome.KeepItemsByLocator(itemsByLocator);
            CreationPlan creation = CreationPlan.For(absent, itemsByLocator);
            outcome.NotCreatedASideFindsNothing = creation.NotCreatedCount;
            log.Line(creation.CountedLine(resolved.TestsInFile));

            // 3b's cost, and it is counted over EVERY test and not over the absent ones,
            // because a test already in the document still points at a set that finds
            // nothing. Counting the absent ones reported 60 on seven groups holding 37 to
            // 55 empty sets, which is what the weekly run had left to create and nothing
            // to do with what the empty sets cost.
            EmptySideTally cost = EmptySideCost.Count(resolved.Buildable, itemsByLocator);
            outcome.TestsWithAnEmptySide = cost.WithAnEmptySide;
            outcome.TestsNoSideCountFor = cost.CouldNotTell;

            HashSet<string> notCreated = new HashSet<string>(StringComparer.Ordinal);

            foreach (SkippedClashTest skipped in creation.NotCreated)
            {
                notCreated.Add(skipped.Name);
                outcome.AddSkipped(skipped);
                LogSkip(skipped);

                TestReport row;

                if (reports != null && reports.TryGetValue(skipped.FileIndex, out row))
                {
                    row.State = TestState.Skipped;
                    row.SkippedReason = skipped.Reason;
                }
            }

            foreach (PlannedClashTest planned in resolved.Buildable)
            {
                if (!notCreated.Contains(planned.Name))
                {
                    toRun.Add(planned);
                }
            }

            return toRun;
        }

        /// <summary>
        /// How many items each set the tests name finds in this document, F77, one resolve
        /// per set and never one per test side. A set that is not in the document is not
        /// counted, and CreationPlan then creates the test and lets the run time check
        /// answer, because a count nobody took is not a count of zero. What it cost is
        /// said, because the whole point of the plan is what creating cost before it.
        /// </summary>
        private Dictionary<string, int> CountItemsPerSet(
            Document document, Dictionary<string, SelectionSet> byPath, IList<string> locators)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);
            Stopwatch clock = Stopwatch.StartNew();

            foreach (string locator in locators)
            {
                SelectionSet set;

                if (!byPath.TryGetValue(locator, out set))
                {
                    continue;
                }

                try
                {
                    using (ModelItemCollection found = set.GetSelectedItems(document))
                    {
                        counts[locator] = found == null ? 0 : found.Count;
                    }
                }
                catch (Exception error)
                {
                    log.Failure(
                        "counting the items of the set \"" + locator + "\"",
                        error,
                        "kept going, every test naming it is created and the run time check answers");
                }
            }

            clock.Stop();
            log.Line("CLASH    counted the items of " + counts.Count
                + (counts.Count == 1 ? " set" : " sets") + " for the creation plan in "
                + Plain(clock.Elapsed.TotalSeconds) + "s");

            return counts;
        }

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

        /// <summary>
        /// The ROWS line of one test, F21, its wording Core's, ReportedCount.Line, with the
        /// clashes only its mirrors found where it is the kept test of a merge, F132.
        /// </summary>
        private void WriteTheRowsLine(string name, int rows, int inDocument, int foundOnlyByAMirror)
        {
            log.NumberedRepeat(
                "ROWS " + (rows - foundOnlyByAMirror == inDocument
                    ? "the two numbers agree"
                    : "the two numbers differ"),
                ReportedCount.Line(name, rows, inDocument, foundOnlyByAMirror),
                "rows for the workbook",
                name,
                EventRow.Count(rows),
                EventRow.Count(inDocument) + " in the document");
        }

        // ---------- the mirrored tests, F132 ----------

        /// <summary>
        /// Every test the document holds with each side as the set it points at, read over
        /// the set index, SavedTests.Read with SavedSideLocator, and one line where any side
        /// was not read, since the mirror rule pairs by sides and never pairs one not read.
        /// </summary>
        private IList<SavedClashTest> ReadTheSavedTestsWithSides(Document document)
        {
            savedSidesNotRead = 0;

            IList<SavedClashTest> saved = SavedTests.Read(document, SavedSideLocator);

            if (savedSidesNotRead > 0)
            {
                log.Line("CLASH    " + savedSidesNotRead + (savedSidesNotRead == 1 ? " side" : " sides")
                    + " of the " + saved.Count + " saved " + (saved.Count == 1 ? "test" : "tests")
                    + " point at no one set this document holds, or would not read, so each is UNKNOWN to the mirror "
                    + "rule and no pair rests on it");
            }

            return saved;
        }

        /// <summary>
        /// Which one set a saved side points at, as a test locator names it, or UNKNOWN. A
        /// side holding no source or more than one, which asks another question than any
        /// test of the XML, a side pointing at no set this document holds, and one that would
        /// not read are all UNKNOWN, counted once, and Core pairs no test on them. The one
        /// reading of which set a source is, LocatorOf, is the DRIFT block's own.
        /// </summary>
        private string SavedSideLocator(ClashSelection side)
        {
            if (SourceCount(side) != 1)
            {
                savedSidesNotRead++;
                return TestSettings.UnknownLocator;
            }

            string locator = LocatorOf(side);

            if (string.IsNullOrEmpty(locator) || !TestDrift.WasRead(locator))
            {
                savedSidesNotRead++;
                return TestSettings.UnknownLocator;
            }

            return locator;
        }

        /// <summary>How many sources a side holds, or -1 where reading them threw, which is said.</summary>
        private int SourceCount(ClashSelection side)
        {
            try
            {
                using (Selection selection = side.Selection)
                {
                    SelectionSourceCollection sources = selection.SelectionSources;
                    return sources == null ? 0 : sources.Count;
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading how many sources a saved clash side holds",
                    error,
                    "kept going, that side is UNKNOWN to the mirror rule and no pair rests on it");
                return -1;
            }
        }

        /// <summary>
        /// The mirror rule over the tests to run, Federator.Core.Clash.MirrorRule.Of: with
        /// an XML, over its tests against every test the document holds and this group's sets
        /// build, with the priority file and the XML's sets, and with none, over the saved
        /// tests by their sides. Its MIRROR lines, its renames and the names the coverage
        /// sheet gives, F127, go in the log as the Core gives them. Null where the rule threw,
        /// which is said, so every test then runs under the plan's name keeping its own clashes.
        /// </summary>
        private MirrorRule PairTheMirrors(ClashTestPlan resolved, ClashTestPlan inTheDocument, ClashRunOutcome outcome)
        {
            string where = Words.Or(outcome.OpenDocument, "this document");

            try
            {
                MirrorRule rule = resolved.Source == ClashPlanSource.Document
                    ? MirrorRule.Of(resolved.Buildable, Priorities, null, Mirrors, null, null)
                    : MirrorRule.Of(
                        resolved.Buildable, Priorities, ExchangeSets, Mirrors, inTheDocument, SetsBuilt ?? new SetBuildOutcome());

                log.Block("MIRROR " + where, rule.Lines());

                IList<string> renames = rule.Renames.Lines();

                if (renames.Count > 0)
                {
                    log.Block("MIRROR RENAMES " + where, renames);
                }

                IList<KeyValuePair<string, string>> coverage = rule.CoverageNames();

                if (coverage.Count > 0)
                {
                    List<string> lines = new List<string>();

                    foreach (KeyValuePair<string, string> name in coverage)
                    {
                        lines.Add(MirrorRule.Prefix + "   " + name.Key + "  " + name.Value);
                    }

                    log.Block("MIRROR COVERAGE " + where, lines);
                }

                return rule;
            }
            catch (Exception error)
            {
                log.Failure(
                    "pairing the mirrored tests of " + where,
                    error,
                    "kept going, no test is paired, every test runs under the name the plan gives it and keeps its own "
                        + "clashes, so a clash two tests find may be counted twice");
                return null;
            }
        }

        /// <summary>
        /// F236. One .tsv row a mirror the rule paired, its run name and the test it mirrors, so the clash count check holds a
        /// mirror to no view, since no mirror gets one. None where the rule did not run.
        /// </summary>
        private void WriteTheMirrorRows(MirrorRule mirrored)
        {
            if (mirrored == null)
            {
                return;
            }

            foreach (MirrorPair pair in mirrored.Pairs)
            {
                if (!string.IsNullOrEmpty(pair.MirrorName))
                {
                    log.Row(RunEvents.Mirror, pair.MirrorName, string.Empty, pair.Kept.Name);
                }
            }
        }

        /// <summary>
        /// F236. Where the merge took a kept test's mirrors, one .tsv row a mirror merged and one row of the clashes only they
        /// found, which the merge added to the kept test, so the clash count check holds the kept test's rows to Clash
        /// Detective's count and those, and leaves the mirrors out, their clashes being the kept test's. None where nothing
        /// merged, since then each mirror keeps its own block.
        /// </summary>
        private void WriteTheMergeRows(MirrorMerge merge)
        {
            IList<string> merged = merge.MergedNames;

            if (merged.Count == 0)
            {
                return;
            }

            try
            {
                for (int i = 1; i < merged.Count; i++)
                {
                    log.Row(RunEvents.MirrorMerged, merged[i], string.Empty, merge.Kept.Name);
                }

                log.Row(RunEvents.AddedByMirrors, merge.Kept.Name, EventRow.Count(merge.AddedToTheKeptTest), string.Empty);
            }
            catch (InvalidOperationException error)
            {
                log.Failure(
                    "writing the merge rows of " + merge.Kept.Name,
                    error,
                    "kept going, the clash count check then reads the kept test without the clashes only its mirrors found");
            }
        }

        /// <summary>The run name of every mirror the rule paired, or null where the rule did not run.</summary>
        private static IList<string> MirrorNamesOf(MirrorRule mirrored)
        {
            if (mirrored == null)
            {
                return null;
            }

            List<string> names = new List<string>();

            foreach (MirrorPair pair in mirrored.Pairs)
            {
                if (!string.IsNullOrEmpty(pair.MirrorName))
                {
                    names.Add(pair.MirrorName);
                }
            }

            return names;
        }

        /// <summary>
        /// Every rename the rule planned, Q136 A and the change of roles, made in the document
        /// by the test's address through TestsEditDisplayName before any test is found by
        /// name, F132. The results and their statuses are counted before and read back after,
        /// since nothing has measured what the rename keeps, and both counts are said. A test
        /// not at its address, not found under the new name after, or whose rename threw is
        /// said and left as it is, and the name it was to run under is not created beside it.
        /// </summary>
        private void MakeTheRenames(DocumentClashTests clashTests, MirrorRule mirrored)
        {
            foreach (MirrorRename rename in mirrored.Renames.Planned)
            {
                string oldName = rename.Saved.Name;
                string newName = rename.NewName;
                TestAddress address = TestAddress.At(rename.Saved.Address);

                try
                {
                    ClashTally before = new ClashTally();
                    int groups = 0;

                    using (ClashTest old = Resolve(clashTests, address, oldName))
                    {
                        if (old == null)
                        {
                            log.Line(MirrorRule.Prefix + "   " + oldName + " is not at " + address + " any more, so it is "
                                + "not renamed " + newName + " and no test of that name is created beside it");
                            renameFailed.Add(newName);
                            continue;
                        }

                        CountInto(old.Children, before, ref groups);
                        clashTests.TestsEditDisplayName(old, newName);
                    }

                    ClashTally after = new ClashTally();
                    groups = 0;

                    using (ClashTest renamed = Resolve(clashTests, address, newName))
                    {
                        if (renamed == null)
                        {
                            log.Line(MirrorRule.Prefix + "   after the rename no test named " + newName + " is at " + address
                                + ", so whether " + oldName + " was renamed is UNKNOWN and no test of that name is created "
                                + "beside it");
                            renameFailed.Add(newName);
                            continue;
                        }

                        CountInto(renamed.Children, after, ref groups);
                    }

                    changedTheDocument = true;
                    log.Line(MirrorRule.Prefix + "   renamed " + oldName + " to " + newName + " at " + address + ", "
                        + (SameCounts(before, after)
                            ? "its " + after.Total + (after.Total == 1 ? " result" : " results")
                                + " and the statuses read back the same, " + after.Describe()
                            : "its results read " + before.Total + " with " + before.Describe() + " before and "
                                + after.Total + " with " + after.Describe() + " after, which is not the same"));
                }
                catch (Exception error)
                {
                    renameFailed.Add(newName);
                    log.Failure(
                        "renaming " + oldName + " to " + newName + " at " + address,
                        error,
                        "kept going, the saved test is left as it is and no test of that name is created beside it");
                }
            }
        }

        private static bool SameCounts(ClashTally before, ClashTally after)
        {
            if (before.Total != after.Total)
            {
                return false;
            }

            foreach (CoreClashStatus status in ClashTally.AllStatuses)
            {
                if (before.Of(status) != after.Of(status))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// One merge per test kept whose mirrors the rule merges, MirrorMerge.Of, indexed by
        /// the kept test's name and by the name each mirror runs under, so the harvest of
        /// each hands its clashes to the right merge.
        /// </summary>
        private void IndexTheMerges(MirrorRule mirrored)
        {
            if (mirrored == null)
            {
                return;
            }

            merges = MirrorMerge.Of(mirrored);

            foreach (MirrorMerge merge in merges)
            {
                mergeOfKept[merge.Kept.Name] = merge;

                foreach (MirrorPair pair in merge.Pairs)
                {
                    mergeOfMirror[pair.MirrorName] = merge;
                    pairOfMirror[pair.MirrorName] = pair;
                }
            }
        }

        /// <summary>
        /// After the run has set every test's state, F132: each merge into its kept test,
        /// MirrorMerge.AddTo, which merges nothing where its own rule says so, then its
        /// MIRROR lines and the kept test's ROWS line with the clashes only its mirrors found.
        /// With no report there is nothing to merge into, which is said. Every picture is taken
        /// after it, by Run, off the rows the report then holds, F149.
        /// </summary>
        private void MergeTheMirrors(DocumentClashTests clashTests)
        {
            if (merges.Count == 0)
            {
                return;
            }

            if (Report == null)
            {
                log.Line(MirrorRule.Prefix + "   no report is built for this run, so nothing is merged and each test keeps "
                    + "its own clashes under its own name, where a clash both find may be counted twice");
                return;
            }

            foreach (MirrorMerge merge in merges)
            {
                try
                {
                    merge.AddTo(Report);
                }
                catch (Exception error)
                {
                    log.Failure(
                        "merging the mirrors of " + merge.Kept.Name,
                        error,
                        "kept going, nothing is merged into it and each of its mirrors stays in the report as its own test");
                }

                log.Block("MIRROR MERGE " + merge.Kept.Name, merge.Lines());
                WriteTheMergeRows(merge);

                TestReport summary = ReportFor(merge.Kept);
                int inDocument;

                if (summary == null || !clashesInDocument.TryGetValue(merge.Kept.FileIndex, out inDocument))
                {
                    continue;
                }

                try
                {
                    WriteTheRowsLine(merge.Kept.Name, summary.Rows.Count, inDocument, merge.AddedToTheKeptTest);
                }
                catch (InvalidOperationException error)
                {
                    log.Failure(
                        "writing the ROWS line of " + merge.Kept.Name,
                        error,
                        "kept going, the rows it holds are " + summary.Rows.Count + " and the document holds "
                            + inDocument);
                }
            }
        }

        /// <summary>
        /// The picture of every row, F149, once every test has run and every merge is made: each
        /// row recorded while its test was read, where the report still holds it, under the test the
        /// report holds it under now, by the index path of its result under the test it was read from,
        /// resolved again by address. A row of a mirror taken out of the report gets none, and a row
        /// only a mirror found is pictured under the kept test.
        ///
        /// THE REPORT'S ORDER, SO NO PICTURE IS RENAMED. Core's PictureWalk takes the tests in
        /// ReportOrder.Tests and each test's rows in its own order, the order the pictures are numbered
        /// in, so a test takes its block number at its first picture and each row its clash number as
        /// it comes, ClashReport.NextPictureOf, and every picture is written under the name the renumber
        /// would give it. Sets 07 to 09 spent 39 s to 44 s of 1A02MM renaming 2931 pictures taken in the
        /// order the tests ran. The renumber stays as the check that finds every picture already right.
        ///
        /// ONE STEP, PICTURES, the reviewer's third point on attempt 1, opened as the engine's InStep
        /// opens one, so the walk, the resolves and the hiding are timed, the live line names the step,
        /// and one count of PROGRESS lines runs across the whole pass. A throw out of the pass outside
        /// a stretch marks the step and goes on up, as InStep's does, except after a merge that threw,
        /// RenderThePicturesAfterTheMergeThrew.
        ///
        /// LONG RUNS KEPT FROM SLOWING, F147, Bader's order of 2026-10-11, Q152 item 2. The undo is
        /// switched off for the whole pass by Document.StartDisableUndo and its matched EndDisableUndo,
        /// a counter by the install's API reference, so the hiding and the putting back of F145 inside
        /// the pass are not meant to add to it. Whether they did before is UNKNOWN. A put back retried by
        /// modelsShown.Dispose in Run's finally runs after the undo is on again. One line reads
        /// IsUndoDisabled and the name of the step Undo would take next before and after the pass, a
        /// name and not the depth of the stack. The texture cache is watched across the pass by Core's
        /// TextureCacheWatch. Every picture is already disposed as its save returns. The pass moves no
        /// camera of its own, TestsImageForResult taking none, but the retake of F226 sets the active
        /// view's camera for a picture that drew no item of its clash, by a jump cut, and puts back the
        /// camera the view held. Every call here is on the main thread.
        /// </summary>
        private void RenderThePicturesWaiting(Document document, DocumentClashTests clashTests)
        {
            if (picturesWaiting.Count == 0 || Images == null || Report == null)
            {
                return;
            }

            using (RunStep picturing = log.Step(RunSteps.Pictures))
            {
                StepStarted(RunSteps.Pictures);

                TextureCacheWatch textures = null;
                bool? offBefore = UndoReadsOff(document);
                string undoBefore = UndoState(document);
                bool undoOff = KeepTheUndoOff(document, offBefore);
                string undoDuring = UndoState(document);

                try
                {
                    textures = new TextureCacheWatch(
                        CacheFolder(),
                        Images.TextureCacheLimitBytes,
                        Images.TextureCacheCheckEverySeconds,
                        () => log.ElapsedSeconds,
                        line => log.Line("IMAGES   " + line));

                    textures.AtTheStart();
                    // F150. Clash Detective's Hide Other set for the pass once a stretch takes a picture,
                    // and the user's value put back at its end on every path, with its one IMAGES line.
                    using (PicturesHideOther hideOther = new PicturesHideOther(log, Images.GhostTheOtherItems, Images.HideOther))
                    {
                        picturing.Changed(TakeThePictures(clashTests, textures, hideOther));
                    }

                    // F177. The pass came to its end without a throw and the pictures' guard did not stop
                    // the run, so the leftover pictures of earlier runs may be removed. The GDI backstop
                    // ending the pictures early is not a failure and does not clear this.
                    PicturesFinished = Images != null && !Images.ShouldStopTheRun;
                }
                catch (Exception)
                {
                    picturing.Failed();
                    throw;
                }
                finally
                {
                    PutTheUndoBack(document, undoOff, offBefore, undoBefore, undoDuring);

                    if (textures != null)
                    {
                        log.Line("IMAGES   " + textures.AtTheEnd());
                    }

                    // F226, Bader's answer to Q208. The one line of the pictures taken again, the count retaken and the count
                    // still empty, with its .tsv row, whichever way the pass ended.
                    string retake = Images.RetakeLine();
                    log.Numbered("IMAGES   " + retake, "retake", Report.Building,
                        Images.RetakesDrewNothing.ToString(System.Globalization.CultureInfo.InvariantCulture), retake);

                    // F147, Bader's answer to Q155. Read by this pass alone, so released as it ends.
                    picturesWaiting.Clear();
                    StepEnded();
                }
            }
        }

        /// <summary>
        /// Every picture where the merge threw, F149: taken all the same, the breaker's fourth point
        /// on attempt 1, and a throw out of them written as a failure line, the second reading, so
        /// the merge's own error is the one that goes on up and is never lost behind the pass's.
        /// </summary>
        private void RenderThePicturesAfterTheMergeThrew(Document document, DocumentClashTests clashTests)
        {
            try
            {
                RenderThePicturesWaiting(document, clashTests);
            }
            catch (Exception error)
            {
                log.Failure(
                    "taking the pictures after the mirror merge threw",
                    error,
                    "kept going, the rows not yet pictured keep an empty Image cell, and the merge's own error goes on up");
            }
        }

        /// <summary>The folder ogs under the user's temp folder, where Navisworks keeps its texture cache, or null, said, where it cannot be made.</summary>
        private string CacheFolder()
        {
            try
            {
                return TextureCacheWatch.FolderIn(System.IO.Path.GetTempPath());
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading the temp folder for the texture cache",
                    error,
                    "kept going, the texture cache is not checked in this pass");
                return null;
            }
        }

        /// <summary>Whether the undo reads off by IsUndoDisabled, null where it cannot be read.</summary>
        private static bool? UndoReadsOff(Document document)
        {
            try
            {
                return document.IsUndoDisabled;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Whether the undo reads off, by IsUndoDisabled, and the name of the step Undo would take next,
        /// a name and not the depth of the stack, nothing where there is none, each UNKNOWN where it
        /// cannot be read.
        /// </summary>
        private static string UndoState(Document document)
        {
            bool? off = UndoReadsOff(document);
            string next;

            try
            {
                next = Words.Or(document.NextUndo, "nothing");
            }
            catch (Exception error)
            {
                next = "UNKNOWN, " + error.GetType().Name;
            }

            return (off == null ? "UNKNOWN" : off.Value ? "off" : "on") + " with " + next + " as the step Undo would take next";
        }

        /// <summary>
        /// The undo switched off for the pictures, F147. Where the call threw, false and said, unless the
        /// undo now reads off where it read on before, since then the call took the counter before it
        /// threw and its matching call must still be made.
        /// </summary>
        private bool KeepTheUndoOff(Document document, bool? offBefore)
        {
            try
            {
                document.StartDisableUndo();
                return true;
            }
            catch (Exception error)
            {
                bool tookIt = offBefore == false && UndoReadsOff(document) == true;

                log.Failure(
                    "switching the undo off for the pictures",
                    error,
                    tookIt
                        ? "kept going, the undo reads off now where it read on before, so it is switched on again after the pictures"
                        : "kept going with the undo on, so each hiding and putting back of the models may add to it");
                return tookIt;
            }
        }

        /// <summary>
        /// The undo switched on again after the pictures by the call matching the one that switched it
        /// off, F147, and its state as read before, during and after the pictures on one line, so the
        /// line says what was read. An undo that reads off after where it read on before is said on a
        /// line of its own, since Undo then does nothing in that session.
        /// </summary>
        private void PutTheUndoBack(Document document, bool undoOff, bool? offBefore, string undoBefore, string undoDuring)
        {
            if (!undoOff)
            {
                return;
            }

            try
            {
                document.EndDisableUndo();
                log.Line("IMAGES   the undo for the pictures read " + undoBefore + " before them, " + undoDuring
                    + " during them, and " + UndoState(document) + " after");

                if (offBefore == false && UndoReadsOff(document) == true)
                {
                    log.Line("IMAGES   THE UNDO IS LEFT OFF after the pictures where it read on before them, so Undo does nothing in this "
                        + "Navisworks until it is restarted");
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "switching the undo on again after the pictures",
                    error,
                    "kept going, the undo may stay off in Navisworks until it is restarted");
            }
        }

        /// <summary>The pass inside its step: Core's walk, then each stretch in turn until the guard fires, the texture cache checked between two and Hide Other set as the pass asks. Hands back the step's phrase.</summary>
        private string TakeThePictures(DocumentClashTests clashTests, TextureCacheWatch textures, PicturesHideOther hideOther)
        {
            // F175, Bader's answer to Q171. The cap of a test reads its priority off the built in priority
            // map, a test the map does not name being C, F168. The report's own Priority is put on only after
            // the clash step, so it is read here for the cap and written to no cell.
            foreach (TestReport capped in Report.Tests)
            {
                capped.PicturePriority = Priorities.OfASavedTest(capped.Name);
            }

            // F203. The tests at or under their cap first, so the memory stop takes the last pictures of the largest tests.
            PictureWalkOutcome walk = PictureWalk.Of(Report, ReadFrom, Images.PictureLimitOf);

            foreach (string line in walk.MetTwice)
            {
                log.Line("IMAGES   " + line);
            }

            // One harvest and one count of pieces for the whole pass, so a PROGRESS line comes at
            // least every 30 s across it, the breaker's third point on attempt 1, where a count per
            // stretch left a test of fewer than about sixteen pictures with no line at all.
            ClashHarvest harvest = new ClashHarvest(log);
            harvest.Images = Images;
            harvest.WorkbookPath = WorkbookPath;
            HandTheProgress(harvest, string.Empty);
            harvest.BeginPieces(RunSteps.Pictures);

            // F157. The memory with the GDI and USER counts at the start, then every
            // Images.LogEveryPictures pictures with the rate, so a crash leaves numbers to read.
            pictureWatch = new PictureWatch(Math.Max(1, Images.LogEveryPictures), log.ElapsedSeconds);
            log.Line(ProcessMemory.PicturesLine("at the start, 0 of " + walk.Rows + " pictures", ProcessMemory.Read()));

            int done = 0;

            foreach (PictureStretch stretch in walk.Stretches)
            {
                // FR-076. Nothing more is resolved, asked for or counted once the guard has fired,
                // the reviewer's first point on attempt 1, and Run stops the run once the pass ends.
                // F172. The GDI backstop ends the pictures the same way and nothing else.
                if (Images.ShouldStopTheRun || Images.AStopFired)
                {
                    break;
                }

                TakeTheStretch(clashTests, harvest, stretch, done, walk.Rows, hideOther);
                done += stretch.Rows.Count;
                textures.BetweenTests();
            }

            SayTheTestsOverTheLimit();

            if (Images.GdiBackstopFired)
            {
                log.Line("IMAGES   " + GdiBackstop.LeftLine(Images.RowsLeftWithoutAPicture(Report)));
                progress("The pictures stopped at the GDI limit, the run goes on without the rest");
            }

            // F182. The memory stop's NOTICE, once the pass ends, so the count of pictures skipped is the real
            // one: the picture number, Navisworks's private bytes and the clashes that wanted a picture and
            // were not made for the stop, a test held to its own limit first.
            if (Images.MemoryStopFired)
            {
                int skipped = Images.PicturesSkippedByTheStop(Report);

                log.Line("IMAGES   " + Images.MemoryStop.Notice(skipped));
                progress("The pictures stopped at the memory limit, the run goes on without the rest");
            }

            return Words.Counted(walk.Rows, "row", "rows") + " in report order in "
                + Words.Counted(walk.Stretches.Count, "stretch", "stretches")
                + (Images.ShouldStopTheRun ? ", the pictures' guard fired" : string.Empty)
                + (Images.GdiBackstopFired ? ", stopped at the GDI limit" : string.Empty)
                + (Images.MemoryStopFired ? ", stopped at the memory limit" : string.Empty);
        }

        /// <summary>
        /// F172, F175 and F190. One line for each test whose clashes exceed its cap: its name, its priority letter,
        /// its clashes, the pictures made and the clashes whose picture was skipped by the rule.
        /// </summary>
        private void SayTheTestsOverTheLimit()
        {
            foreach (TestReport test in Report.Tests)
            {
                if (test.Rows.Count <= Images.PictureLimitOf(test))
                {
                    continue;
                }

                int made = test.ImageCount;
                int skipped = Math.Max(0, Images.RowsThatWantAPicture(test) - Images.PictureLimitOf(test));

                log.Line(PictureLimits.TestLine(test.Name, test.PicturePriority, test.RawClashes, Images.CapOverClashes, made, skipped));
            }
        }

        private PictureWatch pictureWatch;

        /// <summary>
        /// Once a picture is done, or passed over, F157: the MEMORY line and the RATE line when the
        /// count reaches the next multiple of the setting. Each line is on the log's own flush, so
        /// a crash after it loses nothing.
        /// </summary>
        private void AfterAPicture(int done, int total)
        {
            if (pictureWatch == null || !pictureWatch.Due(done))
            {
                return;
            }

            log.Line(ProcessMemory.PicturesLine(done + " of " + total + " pictures", ProcessMemory.Read()));
            log.Line(pictureWatch.RateLine(done, total, log.ElapsedSeconds));

            string split = Images == null ? null : Images.TakeTheGdiSplitLine();

            if (split != null)
            {
                log.Line(split);
            }
        }

        /// <summary>The key of the test of the document a row was read from, or null for a row that waits for no picture.</summary>
        private string ReadFrom(ClashRow row)
        {
            RowAddress where;
            return picturesWaiting.TryGetValue(row, out where) ? where.TestKey : null;
        }

        /// <summary>
        /// The pictures of one stretch, F149: consecutive rows of one test of the report read from one
        /// test of the document, resolved once and hidden for once by F145's rule. The rows before the
        /// first that takes a picture are passed over, each counted as the writer counts it, so a
        /// stretch none of whose rows takes one resolves nothing, the reviewer's eighth point on
        /// attempt 1. Every picture not taken for a fault, its test not found again or a throw ending
        /// the stretch, is counted as failed, the breaker's first point, so the IMAGES line and the
        /// guard see it.
        /// </summary>
        private void TakeTheStretch(
            DocumentClashTests clashTests, ClashHarvest harvest, PictureStretch stretch, int doneBefore, int total, PicturesHideOther hideOther)
        {
            IList<ClashRow> rows = stretch.Rows;
            TestReport into = stretch.Into;
            RowAddress first = picturesWaiting[rows[0]];
            int next = 0;

            HandTheProgress(harvest, into.Name);

            try
            {
                while (next < rows.Count && !Images.Wanted(Report, into, rows[next], WorkbookPath))
                {
                    next++;
                    harvest.Piece("picture", doneBefore + next, total);
                    AfterAPicture(doneBefore + next, total);
                }

                if (next == rows.Count)
                {
                    return;
                }

                // F150. Only once a row of the pass takes a picture, so a pass that takes none
                // reads and sets nothing, the breaker's fifth point on attempt 1. Once a pass.
                hideOther.Begin();

                PictureModels pictureModels = null;

                // F145. The models of the test the rows were read from, whose sides the items of every
                // one of them live in, read off a handle released before the hide, and the pictures
                // taken through one resolved after it.
                using (ClashTest forSides = Resolve(clashTests, first.Address, first.TestName))
                {
                    if (forSides == null)
                    {
                        NotFoundAgain(into, rows, next, first);
                        return;
                    }

                    if (modelsShown != null)
                    {
                        pictureModels = modelsShown.Read(first.TestName, forSides);
                    }
                }

                using (modelsShown == null ? null : modelsShown.Hide(first.TestName, pictureModels))
                using (ClashTest test = Resolve(clashTests, first.Address, first.TestName))
                {
                    if (test == null)
                    {
                        NotFoundAgain(into, rows, next, first);
                        return;
                    }

                    // FR-076. No row is resolved or counted once the guard has fired.
                    while (next < rows.Count && !Images.ShouldStopTheRun && !Images.AStopFired)
                    {
                        harvest.PictureLater(clashTests, test, Report, into, picturesWaiting[rows[next]].Row);
                        next++;
                        harvest.Piece("picture", doneBefore + next, total);
                        AfterAPicture(doneBefore + next, total);
                    }
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "taking the pictures of " + into.Name + " read from " + first.TestName + " once every test had run",
                    error,
                    "kept going, the rows of that stretch not yet pictured keep an empty Image cell and are counted on the "
                        + "IMAGE line after this, and the pictures after them keep their report order names");
                CountTheRowsLeft(
                    "a throw ended the pictures of " + first.TestName, into, rows, next,
                    ImageFailure.BecauseItThrew(error), ImageFailure.BecauseItsStretchEnded(error));
            }
        }

        /// <summary>The rows of a stretch from one on, whose test was not found again once every test had run, F149.</summary>
        private void NotFoundAgain(TestReport into, IList<ClashRow> rows, int from, RowAddress first)
        {
            string where = first.TestName + " is not at " + first.Address + " any more";
            ImageFailure notFound = ImageFailure.BecauseItsTestWasNotFoundAgain(where);
            CountTheRowsLeft(where, into, rows, from, notFound, notFound);
        }

        /// <summary>
        /// The rows of a stretch from one on that ONE fault left with no picture asked for, F149,
        /// the breaker's first point on attempt 1 and the second reading's blocking point. Counted
        /// by ClashImages.NotReached as the writer would have counted them, a row of a status not
        /// asked for or past the cap as passed over and every other one as failed, the fault told
        /// to the pictures' guard ONCE and not once a row, and nothing counted where the guard has
        /// already fired. Said on one line with the counts, so an empty Image cell never reads as a
        /// picture nobody asked for and one fault never reads as fifty failed renders.
        /// </summary>
        private void CountTheRowsLeft(string fault, TestReport into, IList<ClashRow> rows, int from, ImageFailure first, ImageFailure rest)
        {
            if (from >= rows.Count)
            {
                return;
            }

            string left = Words.Counted(rows.Count - from, "row", "rows") + " left in its stretch under " + into.Name;

            if (Images.ShouldStopTheRun)
            {
                log.Line("IMAGE    " + fault + ", and none of the " + left + " is counted, since the pictures' guard has fired");
                return;
            }

            RowsLeftCount counted = Images.NotReached(Report, into, rows, from, WorkbookPath, first, rest);

            log.Line("IMAGE    " + fault + ", so no picture of the " + left + " is taken: "
                + counted.Failed.Count + " counted as failed, " + counted.PassedOverOnStatus + " passed over on status and "
                + counted.PassedOverOnTheCap + " on the cap, "
                + (counted.GuardTold ? "the fault told to the pictures' guard once" : "nothing told to the pictures' guard"));
        }

        // ---------- reading the two trees ----------

        /// <summary>
        /// Every set in the document, keyed by the same path a locator is written with, so
        /// the two are compared with an ordinary string comparison. Ordinal and never
        /// trimmed, because two set names in the reference file end in a space.
        ///
        /// These wrappers are held for the whole group on purpose. Nothing in the clash
        /// step mutates the sets tree, so nothing invalidates them. Beside them, one
        /// SelectionSource per set, built here once, so reading which set a side points
        /// at compares against a source that already exists. Both are released by
        /// ReleaseTheIndex in the one finally at the end of Run, and here before a throw
        /// part way through building leaves, so nothing half built is held.
        /// </summary>
        public Dictionary<string, SelectionSet> IndexSets(DocumentSelectionSets sets)
        {
            Dictionary<string, SelectionSet> byPath =
                new Dictionary<string, SelectionSet>(StringComparer.Ordinal);

            sourceByPath = new Dictionary<string, SelectionSource>(StringComparer.Ordinal);

            try
            {
                List<string> folders = new List<string>();

                // WalkSets releases every folder it steps into, but not the one it is
                // handed, so the root is read into a using here.
                using (GroupItem root = sets.RootItem)
                {
                    WalkSets(root, folders, byPath);
                }

                foreach (KeyValuePair<string, SelectionSet> pair in byPath)
                {
                    sourceByPath.Add(pair.Key, sets.CreateSelectionSource(pair.Value));
                }
            }
            catch
            {
                ReleaseTheIndex(byPath);
                throw;
            }

            return byPath;
        }

        /// <summary>
        /// One SelectionSource per indexed set, keyed the same way as the sets. A field
        /// rather than a parameter because LocatorOf is reached through SettingsOf from
        /// CompareAndMaybeApply, while the set wrappers travel as a parameter the way they
        /// already did. Built by IndexSets, read by LocatorOf, released with the sets.
        /// </summary>
        private Dictionary<string, SelectionSource> sourceByPath;

        /// <summary>
        /// Disposes the sources first and then the set wrappers they point at. Disposing
        /// an eEXTERNAL wrapper releases the wrapper and never the document's set. A null
        /// set index is nothing to release, which is what Run holds when it stopped before
        /// indexing. Nothing here stops a run, a throw is logged and the rest is left to
        /// the finalizer.
        /// </summary>
        private void ReleaseTheIndex(Dictionary<string, SelectionSet> byPath)
        {
            try
            {
                if (sourceByPath != null)
                {
                    foreach (SelectionSource source in sourceByPath.Values)
                    {
                        source.Dispose();
                    }

                    sourceByPath = null;
                }

                if (byPath == null)
                {
                    return;
                }

                foreach (SelectionSet set in byPath.Values)
                {
                    set.Dispose();
                }

                byPath.Clear();
            }
            catch (Exception error)
            {
                log.Failure(
                    "releasing the set index",
                    error,
                    "kept going, whatever was not released goes to the finalizer");
            }
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
    }
}
