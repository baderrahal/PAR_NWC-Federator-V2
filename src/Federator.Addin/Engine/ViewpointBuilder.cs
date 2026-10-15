using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using Federator.Core.Views;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Puts one saved view per clash test of its open clashes into the NWF, in folders by
    /// priority and team pair, F114, Bader's Q114 points 9 to 19. The plan is
    /// Federator.Core.Views.TestViewPlan, the inventory of what goes after is
    /// ViewsInventory, the checks are ViewsTreeCheck, and nothing about any of them is
    /// decided here. This file holds the calls into Navisworks and the order they run in.
    ///
    /// THE ORDER, the design's S2, so a run that stops part way leaves the old picture or a
    /// checked new one and never neither: a fresh walk of the tree, walk one over the rows
    /// of the merged report, the plan, the hidden state read, every view's camera read once,
    /// then per view the hide, the dim, the paint, the frame of the camera read, the folders,
    /// the record, the mark and the read back, then a fresh walk and the inventory over it,
    /// the removals it gave, the document put back, and a last fresh walk for the VIEWS TREE
    /// block and its seven checks.
    ///
    /// WALK ONE KEEPS F132's SHAPE, attempt 2 of its add-in half: the rows come off the
    /// merged report, ReportClashes.Of, under the test the report holds them under and at
    /// the row's status, and each row's result is resolved by the address the harvest
    /// recorded for it, the test once per address through TestAddress.ResolveIn and the
    /// result through ResultPath.ResultAt, never by name off the document. What each row
    /// becomes is a ViewClash: the two items' index paths, plain ints and not handles, the
    /// clash centre, the model each item lives in by its file name, and the size of the
    /// larger service ONLY for a clash whose test's pair carries the size folder, row
    /// F114-K9, so the teams are read before the walk. The camera is read after the plan
    /// and before the first view is touched, for each view's camera clash alone,
    /// TestsViewpointForResult on a copy, where F85 read one per clash, and held until the
    /// views end, F141 attempt 2.
    ///
    /// WHAT A VIEW SHOWS, Bader's answer B to Q119: only the models its clashing items
    /// live in, ShownModels, every other hidden, the shown ones dimmed and every clashing
    /// item solid, the first items painted red and the second green by PaintPlan, one
    /// reset and one paint per colour over a collection, P17, and the camera zoomed to the
    /// box over the clash centres, FramingBox and ZoomBox, P16, or Clash Detective's own
    /// camera for a view of one clash.
    ///
    /// ONLY WHAT THIS TOOL MADE IS EVER REMOVED, point 16. Every view and every folder the
    /// tool makes carries the mark, ToolViewMark, written by AddComment after the add, P9.
    /// The view just made is found as the child of its folder with its name and no mark,
    /// P12, so where a person's unmarked view of that name already sits in that folder no
    /// view is written there, P22 unrun, and the person's is left as it is. The inventory
    /// is taken off a fresh walk after this run's views are written, marked and read back,
    /// and each removal re-finds its item by name, kind and mark just before RemoveAt with
    /// the parent resolved fresh, P13, a folder with everything under it in one call, P14.
    ///
    /// EVERYTHING IS PUT BACK, MEASURED. Before the first view changes anything the hidden
    /// state the document holds is read off a runtime capture that never goes into the
    /// tree, 5k, and when the group's writing ends, whichever way it ends, the dimming is
    /// taken off, everything is shown and exactly the items that were hidden are hidden
    /// again and read back as hidden. The window's view is never touched, 5m.
    ///
    /// NOTHING HERE IS A FLAG. Both flags this API offers, ContainsVisibilityOverrides and
    /// ContainsAppearanceOverrides, read TRUE on a viewpoint that recorded neither, 5o. A
    /// view counts as read back when it is there, its camera sits within the tolerance, it
    /// hides as many items as it meant to, it dims as many as it meant to, every clashing
    /// item will show the colour it was painted, and its mark reads back as this run's.
    /// </summary>
    public sealed class ViewpointBuilder
    {
        private readonly Action<string> progress;
        private readonly RunLog log;
        private readonly PenetrationSettings penetrations;
        private readonly SizeSettings sizes;
        private readonly ViewpointSettings views;
        private readonly HashSet<string> saidOnce = new HashSet<string>(StringComparer.Ordinal);

        private HiddenSnapshot snapshot;
        private bool dimmedAnything;
        private bool dimmedNow;
        private ViewsSeconds seconds;
        private string stamp;

        // Where one clash's result sits in the document, kept by the clash's key so the
        // camera of a view's camera clash is resolved again after the plan, and never a
        // handle held across the group.
        private readonly Dictionary<string, RowAddress> addressOf = new Dictionary<string, RowAddress>(StringComparer.Ordinal);

        // Which tests walk one read whole, Core's rule, the breaker's B1: only such a test gets
        // a view and loses its old views, and every other is counted failed with why.
        private TestsRead testsRead;

        public ViewpointBuilder(
            Action<string> progress,
            RunLog log,
            PenetrationSettings penetrations,
            SizeSettings sizes,
            ViewpointSettings views)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.progress = progress ?? delegate { };
            this.log = log;
            this.penetrations = penetrations ?? new PenetrationSettings();
            this.sizes = sizes ?? new SizeSettings();
            this.views = views ?? new ViewpointSettings();
        }

        /// <summary>The plan for the group, kept so the engine can write its VIEWS block.</summary>
        public TestViewPlanOutcome Plan { get; private set; }

        /// <summary>The VIEWS TREE block as the .log takes it, cut at the TreeLinesInLog setting, or null where the tree was not read.</summary>
        public IList<string> TreeLog { get; private set; }

        /// <summary>The VIEWS TREE block whole, one row each for the .tsv, or null where the tree was not read.</summary>
        public IList<string> TreeRows { get; private set; }

        /// <summary>F191. The items deleted outside the tool folder and Comments, or minus one where the step did not run.</summary>
        public int OutsideDeleted { get; private set; }

        // F194. The seconds of each view, the read back of the last one, the group and the tree at the start.
        private ViewTimings timings;
        private ViewReadBack lastReadBack;
        private string timedGroup = string.Empty;
        private int treeItemsAtStart;

        /// <summary>How many items of earlier runs the inventory's removals took out, each a change to the document.</summary>
        public int RemovedCount { get; private set; }

        // What each removal came to, one list of results the tree block's counts are read off.
        private List<RemovalOutcome> removals;

        /// <summary>
        /// The whole of one group: walk, plan, write, inventory, remove, read the tree.
        /// Never throws past a view: one that throws is recorded as failed and the rest are
        /// still tried. The tests ran, the test names, the mirrors and whether the clash step
        /// was sound are the inventory's facts, Core's contracts of rows F114-K12 and K20. The
        /// tests the clash step skipped are read for the ones it failed on, with the kept test
        /// of each merged mirror, row F114-K38.
        /// </summary>
        public ViewpointBuildOutcome BuildForGroup(
            Document document,
            string group,
            ClashReport report,
            IDictionary<ClashRow, RowAddress> addresses,
            ViewTeams teams,
            IList<ModelTeam> models,
            ICollection<string> testsRan,
            IEnumerable<SkippedClashTest> testsSkipped,
            IDictionary<string, string> keptOfMirror,
            ICollection<string> testNames,
            ICollection<string> mirrors,
            bool clashStepSound)
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();

            if (document == null || report == null || teams == null)
            {
                return outcome;
            }

            seconds = new ViewsSeconds(() => log.ElapsedSeconds);
            stamp = ToolViewMark.StampOf(DateTime.UtcNow);
            DocumentClashTests clashTests = document.GetClash().TestsData;
            IList<ModelTeam> groupModels = models ?? new List<ModelTeam>();
            List<WrittenView> written = new List<WrittenView>();
            Dictionary<string, IList<string>> hiddenReadBack = new Dictionary<string, IList<string>>(StringComparer.Ordinal);
            Dictionary<string, IList<ItemPath>> paintedReadBack = new Dictionary<string, IList<ItemPath>>(StringComparer.Ordinal);
            ICollection<string> emptyBefore = null;
            ViewsInventory inventory = null;
            snapshot = null;
            dimmedAnything = false;
            dimmedNow = false;
            addressOf.Clear();
            testsRead = new TestsRead(OpenClashes.StatusesFor(views.ViewStatuses));
            Plan = null;
            TreeLog = null;
            TreeRows = null;
            RemovedCount = 0;
            OutsideDeleted = -1;
            timings = new ViewTimings();
            lastReadBack = null;
            timedGroup = group;
            treeItemsAtStart = 0;
            removals = null;

            try
            {
                using (seconds.In(ViewsPart.TakingTheInventory))
                {
                    TreeWalk before = SavedViewpoints.ReadTree(document, null);
                    SayWalk("before anything was written", before);
                    treeItemsAtStart = before.Nodes.Count;
                    emptyBefore = ViewNode.EmptyFolderKeys(before.Nodes);
                }

                // F186, Bader's answer 1 of Q174. The tool's one top folder of an earlier run is deleted with
                // everything in it before this run's views are made, found by its name and its marker.
                using (seconds.In(ViewsPart.DeletingTheToolFolder))
                {
                    DeleteTheToolFolder(document);
                }

                using (seconds.In(ViewsPart.ReadingTheClashes))
                {
                    List<ViewClash> clashes = Collect(document, clashTests, report, addresses, teams);

                    // N2. A test the clash step ran with no row on the report is read whole with
                    // no row, so the inventory removes its old view as no longer needed.
                    testsRead.Ran(testsRan);

                    // F114-K38. A test whose harvest threw after only closed rows is not whole,
                    // and nor is the kept test of a mirror it failed on.
                    testsRead.ClashStepFailed(testsSkipped, keptOfMirror);
                    Plan = TestViewPlan.For(clashes, teams, mirrors, views);
                }

                foreach (KeyValuePair<string, string> notWhole in testsRead.NotWhole())
                {
                    outcome.AddFailed(notWhole.Key, "UNKNOWN", notWhole.Value);
                }

                if (Plan.Views.Count > 0)
                {
                    WriteTheViews(document, clashTests, groupModels, outcome, written, hiddenReadBack, paintedReadBack);
                }

                using (seconds.In(ViewsPart.TakingTheInventory))
                {
                    TreeWalk after = SavedViewpoints.ReadTree(document, emptyBefore);
                    SayWalk("after this run's views were written", after);
                    inventory = ViewsInventory.Plan(
                        ToolViewFolder.NodesUnder(after.Nodes), Plan, testsRead.WholeNames, written, stamp, clashStepSound,
                        teams.Map.KnownCodes(CodesOf(groupModels)), testNames, views);
                }

                log.Block("VIEWS INVENTORY " + group, inventory.Lines());

                using (seconds.In(ViewsPart.Removing))
                {
                    Remove(document, inventory);
                }

                // F191, Bader's order of 15 Oct 2026. Everything at the root outside the tool folder and the
                // Comments folder is deleted, after the tool folder is built. Its own part, not nested in Removing.
                using (seconds.In(ViewsPart.DeletingOutside))
                {
                    DeleteOutsideTheToolFolder(document, outcome);
                }
            }
            finally
            {
                using (seconds.In(ViewsPart.PuttingBack))
                {
                    PutBack(document);
                }

                if (Plan != null)
                {
                    using (seconds.In(ViewsPart.ReadingTheTree))
                    {
                        ReadTheTree(document, group, teams, groupModels, inventory, written, emptyBefore,
                            hiddenReadBack, paintedReadBack, testsRan, testNames);
                    }
                }

                // Last, so the whole runs from the first walk to the tree read, and written
                // whichever way the work ended, because time spent failing is time the run spent.
                seconds.Ended();
                log.Line(seconds.Line());
            }

            return outcome;
        }

        // ---------- the tool folder, F186 ----------

        /// <summary>
        /// F186, Bader's answer 1 of Q174. The tool's one top folder of an earlier run is found by its name and its
        /// marker comment and deleted with everything in it, BEFORE this run writes its own. Nothing outside it is
        /// read for removal, changed or deleted. A viewpoint a coordinator moved into it by hand goes with it, which
        /// the NWF page says before the run. A throw is logged and the run goes on, the new views are then
        /// written beside the old folder and the check of the tree says so.
        /// </summary>
        private void DeleteTheToolFolder(Document document)
        {
            try
            {
                DocumentCensus before = DocumentCensusReader.Read(document);
                ToolFolderRemoval removal = SavedViewpoints.RemoveToolFolder(document);
                DocumentCensus after = DocumentCensusReader.Read(document);

                RemovedCount += removal.ItemsUnder;
                log.Line("VIEWS    the tool folder " + ToolViewFolder.Name + " of the earlier run: "
                    + (removal.FoldersRemoved == 0
                        ? "none found by its name and its marker, nothing was deleted"
                        : removal.FoldersRemoved + " found by its name and its marker and deleted with its "
                            + removal.ItemsUnder + " items, so every view of this run is made new")
                    + (removal.LeftWithoutAMarker > 0
                        ? ", " + removal.LeftWithoutAMarker + " folder(s) of that name with no marker left as a person's"
                        : string.Empty)
                    + (removal.WhyNot != null ? ", " + removal.WhyNot : string.Empty));

                log.Line(ToolViewFolder.DeletedWithoutChangingTheResultsLine(
                    before.Of(CensusCount.Tests), before.Of(CensusCount.Results), after.Of(CensusCount.Tests), after.Of(CensusCount.Results)));
            }
            catch (Exception error)
            {
                log.Failure(
                    "deleting the tool folder " + ToolViewFolder.Name + " of the earlier run",
                    error,
                    "kept going, the views of this run are written and the earlier folder, if it is there, stays");
            }
        }

        /// <summary>
        /// F191, Bader's order of 15 Oct 2026. Everything at the root of the saved viewpoint tree is deleted except
        /// the tool folder this run built and a folder named Comments, which is kept with all inside it and made
        /// empty where there is none. It runs AFTER the tool folder is built and its marker is there, so a build
        /// that stopped or threw deletes nothing of a coordinator's. The clash tests and results are counted
        /// before, after the first item and after all, and the delete stops at the first item if the counts moved,
        /// which is said and fails the view step. A throw is logged and the run goes on.
        /// </summary>
        private void DeleteOutsideTheToolFolder(Document document, ViewpointBuildOutcome outcome)
        {
            try
            {
                IList<RootEntry> root = SavedViewpoints.ReadRoot(document, stamp);
                OutsidePlan plan = OutsideTheToolFolder.Plan(root, Plan != null && Plan.Views.Count > 0);

                if (plan.Refused)
                {
                    log.Line(OutsideTheToolFolder.Line(plan, 0, false));
                    return;
                }

                DocumentCensus before = DocumentCensusReader.Read(document);
                List<RootEntry> ordered = new List<RootEntry>(plan.Delete);
                ordered.Sort((a, b) => b.Index.CompareTo(a.Index));
                int removedItems = 0;
                int removedTop = 0;
                bool stopped = false;

                foreach (RootEntry entry in ordered)
                {
                    string whyNot = SavedViewpoints.DeleteRootItem(document, entry);

                    if (whyNot != null)
                    {
                        log.Line("VIEWS    OUTSIDE \"" + entry.Name + "\" was not deleted, " + whyNot);
                        continue;
                    }

                    removedItems += entry.Items;
                    removedTop++;

                    if (removedTop == 1)
                    {
                        DocumentCensus afterFirst = DocumentCensusReader.Read(document);

                        if (!OutsideTheToolFolder.Unchanged(before, afterFirst))
                        {
                            stopped = true;
                            break;
                        }
                    }
                }

                RemovedCount += removedItems;
                OutsideDeleted = removedItems;
                bool commentsMade = false;

                if (plan.CreateComments && !stopped)
                {
                    commentsMade = SavedViewpoints.EnsureFolders(document, new List<string> { OutsideTheToolFolder.CommentsName }).Count > 0;
                }

                DocumentCensus after = DocumentCensusReader.Read(document);
                log.Line(OutsideTheToolFolder.Line(plan, removedItems, commentsMade));
                log.Line(ToolViewFolder.DeletedWithoutChangingTheResultsLine(
                    before.Of(CensusCount.Tests), before.Of(CensusCount.Results), after.Of(CensusCount.Tests), after.Of(CensusCount.Results)));

                if (stopped || !OutsideTheToolFolder.Unchanged(before, after))
                {
                    outcome.AddFailed(
                        "outside the tool folder",
                        "UNKNOWN",
                        "deleting the viewpoints outside the tool folder changed the clash counts, so the delete stopped"
                        + " and the NWF has to be looked at before it is trusted");
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "deleting the viewpoints outside the tool folder",
                    error,
                    "kept going, what was not deleted stays at the root");
            }
        }

        // ---------- walk one ----------

        /// <summary>
        /// Every clash the merged report holds a row for, read into what the plan needs.
        /// F132 attempt 2, the breaker's finding R4: the rows come off the report and not
        /// off the document, Federator.Core.Views.ReportClashes, so a row only a mirror found
        /// is viewed under the kept test, a mirror taken out of the report gets no view, and
        /// the status is the row's, which the merge restated by Q138 B. Each row's result is
        /// resolved in the document by the address the harvest recorded for it, the test
        /// once for all its rows and never held across another test's read, and read back
        /// by name, ResultPath, since the compact after the merge can move a result from
        /// under a recorded path. A test name the report carries twice is read once.
        /// </summary>
        private List<ViewClash> Collect(
            Document document,
            DocumentClashTests clashTests,
            ClashReport report,
            IDictionary<ClashRow, RowAddress> addresses,
            ViewTeams teams)
        {
            List<ViewClash> clashes = new List<ViewClash>();
            string unitEnumName = Penetrations.UnitEnumName(document);
            IList<ClashStatus> inScope = OpenClashes.StatusesFor(views.ViewStatuses);
            ReportClashesOutcome rows = ReportClashes.Of(report);

            foreach (string name in rows.NamedTwice)
            {
                log.Line("VIEWS    " + name + " is on the report twice, so its clashes are read once and planned once");
            }

            List<string> order = new List<string>();
            Dictionary<string, List<ReportClash>> byTest = new Dictionary<string, List<ReportClash>>(StringComparer.Ordinal);
            Dictionary<string, RowAddress> firstOf = new Dictionary<string, RowAddress>(StringComparer.Ordinal);

            // Every row read or left out, with the REPORT's test it sits under, N1, judged once
            // every resolve is done, since a kept test's rows can sit under its mirror's address.
            List<CollectedRow> collected = new List<CollectedRow>();
            int noAddress = 0;

            foreach (ReportClash clash in rows.Clashes)
            {
                testsRead.Met(clash.TestName);

                // C1. A row outside the views' statuses needs nothing read: it goes to the plan,
                // which counts it as left out, and nothing about its result can make its test
                // not read, so a Resolved row Compact removed stays harmless.
                // F114-K36: a result group row is always read, whatever its own status, since the
                // clashes inside it stand at statuses of their own and are what the views show.
                if (!clash.Row.IsGroup && !testsRead.NeedsReading(clash.Status))
                {
                    collected.Add(new CollectedRow(clash.ToView(null, null, null, null, null, null), null, clash.TestName));
                    continue;
                }

                RowAddress where;

                if (addresses == null || !addresses.TryGetValue(clash.Row, out where))
                {
                    noAddress++;
                    testsRead.RowWithNoAddress(clash.TestName, clash.Status, clash.Row.IsGroup);
                    continue;
                }

                List<ReportClash> under;

                if (!byTest.TryGetValue(where.TestKey, out under))
                {
                    under = new List<ReportClash>();
                    byTest.Add(where.TestKey, under);
                    firstOf.Add(where.TestKey, where);
                    order.Add(where.TestKey);
                }

                under.Add(clash);
            }

            if (noAddress > 0)
            {
                log.Line("VIEWS    " + noAddress + " open row(s) of the report have no recorded place in the document, so their tests are not read for the views");
            }

            Counts counts = new Counts();

            foreach (string key in order)
            {
                RowAddress first = firstOf[key];
                List<ReportClash> under = byTest[key];
                List<string> reportTests = new List<string>();

                foreach (ReportClash clash in under)
                {
                    if (!reportTests.Contains(clash.TestName))
                    {
                        reportTests.Add(clash.TestName);
                    }
                }

                string nowNamed;

                try
                {
                    using (ClashTest test = first.Address.ResolveIn(clashTests, first.TestName, out nowNamed))
                    {
                        if (test == null)
                        {
                            // N1. Every report test among the rows this resolve serves is not read.
                            testsRead.NotAtAddress(reportTests);
                            log.Line("VIEWS    " + first.TestName + " is not at " + first.Address + " any more"
                                + (nowNamed == null ? string.Empty : ", which holds \"" + nowNamed + "\"")
                                + ", so " + string.Join(", ", reportTests.ToArray()) + " are not read for the views and its "
                                + under.Count + (under.Count == 1 ? " row gets" : " rows get") + " no view");
                            continue;
                        }

                        // The size is read only in a pair that carries the size folder, row
                        // F114-K9, and the pair is the test's, read off its first row's sides.
                        bool carriesSize = teams.PairOf(under[0].LeftSide, under[0].RightSide).CarriesSizeFolder;

                        foreach (ReportClash clash in under)
                        {
                            string whyNot;
                            bool moved;
                            // The views run after the compact, which can move a result from its
                            // recorded index, so a row with no name of its own is never taken on
                            // its name, F149, and is counted and said here as a row not found.
                            SavedItem item = ResultPath.ResultAt(
                                test, addresses[clash.Row].Row, first.TestName, false, out whyNot, out moved);

                            if (item == null)
                            {
                                counts.NotFound++;
                                testsRead.RowNotFound(clash.TestName, clash.Status, clash.Row.IsGroup);

                                if (counts.FirstNotFound == null)
                                {
                                    counts.FirstNotFound = whyNot;
                                }

                                continue;
                            }

                            if (moved)
                            {
                                counts.Moved++;
                            }

                            using (item)
                            {
                                ClashResult leaf = item as ClashResult;
                                ClashResultGroup resultGroup = leaf == null ? item as ClashResultGroup : null;

                                // F114-K36, Bader's answer B of 2026-10-13: the clashes inside a result group
                                // are read and planned each at its own status, in their test's view, and the
                                // group row itself is in none, so no clash is in a view twice. The camera of
                                // such a view is Clash Detective's for the group, by the group's address.
                                if (resultGroup != null)
                                {
                                    counts.GroupsRead++;
                                    ReadTheClashesInside(
                                        document, clash, resultGroup, inScope, carriesSize, unitEnumName,
                                        counts, addresses[clash.Row], collected);
                                    testsRead.RowRead(clash.TestName, true);
                                    continue;
                                }

                                ViewClash read = CollectOne(
                                    document, clash, leaf, inScope, carriesSize, unitEnumName, counts);
                                collected.Add(new CollectedRow(read, addresses[clash.Row], clash.TestName));
                                testsRead.RowRead(clash.TestName, leaf == null);
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    testsRead.Threw(reportTests);
                    log.Failure(
                        "reading the clashes of " + first.TestName + " for the views",
                        error,
                        "kept going, " + string.Join(", ", reportTests.ToArray())
                            + " are not read for the views, get no view and keep their views of earlier runs, and the next test is still read");
                }
            }

            // A TEST READ IN PART GETS NO VIEW, the breaker's B1, judged by the report's test
            // once every resolve is done, N1: a view of the rows that read would be a short view
            // replacing a whole one. Its rows are left out of the plan, it is counted failed with
            // why, and the inventory keeps its old views. A test whose open rows are all result
            // groups is left out the same way and named, F3, never failed.
            foreach (CollectedRow row in collected)
            {
                if (!testsRead.IsWhole(row.ReportTest))
                {
                    continue;
                }

                clashes.Add(row.Clash);

                if (row.Address != null)
                {
                    addressOf[row.Clash.Key] = row.Address;
                }
            }

            counts.Say(log, clashes.Count);
            return clashes;
        }

        /// <summary>One row of walk one, read or left out of scope, with the report's test it sits under.</summary>
        private sealed class CollectedRow
        {
            public CollectedRow(ViewClash clash, RowAddress address, string reportTest)
            {
                Clash = clash;
                Address = address;
                ReportTest = reportTest;
            }

            public ViewClash Clash { get; private set; }

            public RowAddress Address { get; private set; }

            public string ReportTest { get; private set; }
        }

        /// <summary>
        /// One row of the report into the plan's clash at its row status, and where it is in
        /// scope its two items' paths, its centre, the models its items live in and, in a
        /// pair carrying the size folder, the size of its larger service. The leaf is the
        /// result where the row is one clash, and null where the row is a result group,
        /// which the panel frames as one and which has no two items to read.
        /// </summary>
        private ViewClash CollectOne(
            Document document,
            ReportClash clash,
            ClashResult leaf,
            IList<ClashStatus> inScope,
            bool carriesSize,
            string unitEnumName,
            Counts counts,
            string memberName = null,
            ClashStatus? memberStatus = null)
        {
            // A clash inside a result group, F114-K36, stands at its own status and under its own name.
            ClashStatus status = memberStatus.HasValue ? memberStatus.Value : clash.Status;

            if (!inScope.Contains(status))
            {
                return memberStatus.HasValue
                    ? clash.ToViewOfMember(memberName, status, null, null, null, null, null, null)
                    : clash.ToView(null, null, null, null, null, null);
            }

            if (leaf == null)
            {
                counts.GroupRows++;
                return clash.ToView(null, null, null, null, null, null);
            }

            SizeVerdict? serviceSize = null;

            if (carriesSize)
            {
                bool parentThrew;
                serviceSize = Penetrations.ServiceSizeOf(leaf, penetrations, sizes, unitEnumName, out parentThrew);
                counts.SizeRead++;

                if (parentThrew)
                {
                    counts.SideWalkThrew++;
                }
            }

            ItemPath firstItem = null;
            ItemPath secondItem = null;
            string firstHome = null;
            string secondHome = null;
            Point3 centre = null;

            try
            {
                firstItem = ReadPlace(document, leaf.Item1, out firstHome);
                secondItem = ReadPlace(document, leaf.Item2, out secondHome);
            }
            catch (Exception error)
            {
                // Counted, and the FIRST one is written in full once per group, because the
                // fifth run of F85 counted 975 of these and could not say what threw. The clash
                // still goes in its view, with what was read before it threw.
                counts.HomesUnread++;

                if (counts.FirstHomeError == null)
                {
                    counts.FirstHomeError = error;
                }
            }

            try
            {
                using (Point3D at = leaf.Center)
                {
                    if (at != null)
                    {
                        centre = new Point3(at.X, at.Y, at.Z);
                    }
                }
            }
            catch (Exception error)
            {
                counts.CentresUnread++;

                if (counts.FirstCentreError == null)
                {
                    counts.FirstCentreError = error;
                }
            }

            if (centre == null)
            {
                counts.NoCentre++;
            }

            return memberStatus.HasValue
                ? clash.ToViewOfMember(memberName, status, serviceSize, firstItem, secondItem, centre, firstHome, secondHome)
                : clash.ToView(serviceSize, firstItem, secondItem, centre, firstHome, secondHome);
        }

        /// <summary>
        /// F114-K36, the clashes inside a result group, however deep, each read as a clash of the
        /// group row's test at its own status and name, with its two items, size and centre read
        /// as for a row of one clash. A clash at a status no view shows is kept as a row so the
        /// plan counts it left out. Every one is recorded at the group's address, which is how
        /// the camera of a view that opens with it is found. A throw goes to the caller's catch
        /// and leaves the test not read, as for any row.
        /// </summary>
        private void ReadTheClashesInside(
            Document document,
            ReportClash group,
            ClashResultGroup node,
            IList<ClashStatus> inScope,
            bool carriesSize,
            string unitEnumName,
            Counts counts,
            RowAddress address,
            List<CollectedRow> collected)
        {
            SavedItemCollection children = node.Children;

            if (children == null)
            {
                return;
            }

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    ClashResultGroup inner = child as ClashResultGroup;

                    if (inner != null)
                    {
                        ReadTheClashesInside(document, group, inner, inScope, carriesSize, unitEnumName, counts, address, collected);
                        continue;
                    }

                    ClashResult member = child as ClashResult;

                    if (member == null)
                    {
                        continue;
                    }

                    ClashStatus status = (ClashStatus)(int)member.Status;
                    string name = ClashRow.NameOf(member.DisplayName, false);
                    counts.MembersRead++;

                    if (!inScope.Contains(status))
                    {
                        collected.Add(new CollectedRow(
                            group.ToViewOfMember(name, status, null, null, null, null, null, null), null, group.TestName));
                        continue;
                    }

                    counts.MembersInScope++;
                    ViewClash read = CollectOne(document, group, member, inScope, carriesSize, unitEnumName, counts, name, status);
                    collected.Add(new CollectedRow(read, address, group.TestName));
                }
            }
        }

        /// <summary>
        /// Where one clashing item is, its index path as plain ints, and the model it lives
        /// in by that model's file name, SavedViewpoints.ModelFileOf's walk. Item1 is a fresh
        /// wrapper on every read, so it is released here, and the walk releases every parent.
        /// </summary>
        private static ItemPath ReadPlace(Document document, ModelItem item, out string home)
        {
            home = null;

            if (item == null)
            {
                return null;
            }

            try
            {
                int[] path = SavedViewpoints.PathOf(document, item);
                home = SavedViewpoints.ModelFileOf(item);
                return path == null ? null : new ItemPath(path);
            }
            finally
            {
                item.Dispose();
            }
        }

        // ---------- the views ----------

        private void WriteTheViews(
            Document document,
            DocumentClashTests clashTests,
            IList<ModelTeam> models,
            ViewpointBuildOutcome outcome,
            List<WrittenView> written,
            Dictionary<string, IList<string>> hiddenReadBack,
            Dictionary<string, IList<ItemPath>> paintedReadBack)
        {
            string unitEnumName = Penetrations.UnitEnumName(document);
            Dictionary<ModelTeam, int> indexOf = new Dictionary<ModelTeam, int>();

            // The models come in the document's order, the engine reading them off
            // Document.Models in turn, so a model's place in the list is its index.
            for (int i = 0; i < models.Count; i++)
            {
                indexOf[models[i]] = i;
            }

            ViewsProgress said = new ViewsProgress(views.ProgressEverySeconds, log.ElapsedSeconds);
            HashSet<int> shownBefore = null;
            int done = 0;
            int foldersMarked = 0;
            int foldersNotMarked = 0;
            int hidesSkipped = 0;

            // What was hidden is read before anything of this step reads a camera or changes the
            // document, so the put back returns the document to what it held.
            Touch(document);

            // THE CAMERAS FIRST, F141 attempt 2, P18's measured shape: P18 read every test's
            // camera from TestsViewpointForResult in its walk before any view, and per view only
            // copied and zoomed it after the hiding, the dim and the paint, and read every
            // override back, and F85, which set 04 ran at e4484d1, read every camera in its walk
            // too and read 2813 views back painted. WriteOne read it per view between the paint
            // and the record, and set 07's 1A02MM then wrote 69 views of which none read back a
            // material override while their hidden models read back right. That one of CameraOf's
            // three clash calls, the test resolved, the result resolved and
            // TestsViewpointForResult, undoes the dim and the paint is read off that difference
            // and not measured alone.
            Dictionary<string, Viewpoint> cameras = new Dictionary<string, Viewpoint>(StringComparer.Ordinal);
            Dictionary<string, string> noCameras = new Dictionary<string, string>(StringComparer.Ordinal);

            try
            {
                CamerasFirst(clashTests, cameras, noCameras);

                foreach (PlannedTestView view in Plan.Views)
                {
                    done++;

                    using (seconds.In(ViewsPart.SayingHowFar))
                    {
                        progress("View " + view);

                        if (said.Due(log.ElapsedSeconds))
                        {
                            log.Line(said.Line(done, Plan.Views.Count, written.Count, log.ElapsedSeconds));
                        }
                    }

                    // F194. The view's own seconds, taken from the parts before and after it, with what its read back saw.
                    double viewStarted = log.ElapsedSeconds;
                    double making0 = seconds.SecondsIn(ViewsPart.MakingTheView);
                    double adding0 = seconds.SecondsIn(ViewsPart.FindingTheFolder) + seconds.SecondsIn(ViewsPart.AddingTheView)
                        + seconds.SecondsIn(ViewsPart.MovingIntoTheFolder);
                    double hiding0 = seconds.SecondsIn(ViewsPart.ShowingAndHiding) + seconds.SecondsIn(ViewsPart.Dimming)
                        + seconds.SecondsIn(ViewsPart.Framing);
                    double marking0 = seconds.SecondsIn(ViewsPart.Marking) + seconds.SecondsIn(ViewsPart.MakingTheFolders);
                    double readBack0 = seconds.SecondsIn(ViewsPart.ReadingBack);
                    double collect0 = seconds.SecondsIn(ViewsPart.Collecting);
                    lastReadBack = null;
                    int treeNow = treeItemsAtStart + written.Count;

                    try
                    {
                        WriteOne(document, clashTests, view, models, indexOf, unitEnumName, outcome, written,
                            hiddenReadBack, paintedReadBack, cameras, noCameras,
                            ref shownBefore, ref foldersMarked, ref foldersNotMarked, ref hidesSkipped);
                    }
                    catch (Exception error)
                    {
                        // The type as well as the text, because a member of this API behaving
                        // differently from the way it was measured is the most likely failure, and
                        // the type name is what says which.
                        outcome.AddFailed(view.ToString(), view.Pair.Folder, error.GetType().Name + ": " + error.Message);
                        log.Failure(
                            "putting the view " + view + " into the NWF",
                            error,
                            "kept going, the other views are still tried and the block carries the total");
                    }

                    long privateBytes = Federator.Core.Diagnostics.ProcessMemory.PrivateBytesNow();
                    timings.Add(new ViewTiming
                    {
                        Name = view.ToString(),
                        Clashes = view.Clashes.Count,
                        Overrides = lastReadBack == null ? -1 : lastReadBack.MaterialOverrideCount,
                        Seconds = log.ElapsedSeconds - viewStarted,
                        MakingSeconds = seconds.SecondsIn(ViewsPart.MakingTheView) - making0,
                        AddingSeconds = seconds.SecondsIn(ViewsPart.FindingTheFolder) + seconds.SecondsIn(ViewsPart.AddingTheView)
                            + seconds.SecondsIn(ViewsPart.MovingIntoTheFolder) - adding0,
                        HidingAndDimmingSeconds = seconds.SecondsIn(ViewsPart.ShowingAndHiding) + seconds.SecondsIn(ViewsPart.Dimming)
                            + seconds.SecondsIn(ViewsPart.Framing) - hiding0,
                        MarkingSeconds = seconds.SecondsIn(ViewsPart.Marking) + seconds.SecondsIn(ViewsPart.MakingTheFolders) - marking0,
                        ReadBackSeconds = seconds.SecondsIn(ViewsPart.ReadingBack) - readBack0,
                        CollectSeconds = seconds.SecondsIn(ViewsPart.Collecting) - collect0,
                        ReadBackHiddenSeconds = lastReadBack == null ? 0.0 : lastReadBack.HiddenSeconds,
                        ReadBackOverrideSeconds = lastReadBack == null ? 0.0 : lastReadBack.OverrideSeconds,
                        ReadBackGetSeconds = lastReadBack == null ? 0.0 : lastReadBack.GetOverridesSeconds,
                        ReadBackOverrideColourSeconds = lastReadBack == null ? 0.0 : lastReadBack.ColourOfOverrideSeconds,
                        ReadBackOverrideItemSeconds = lastReadBack == null ? 0.0 : lastReadBack.ItemOfOverrideSeconds,
                        ReadBackOverridePathSeconds = lastReadBack == null ? 0.0 : lastReadBack.PathOfOverrideSeconds,
                        ReadBackColourSeconds = lastReadBack == null ? 0.0 : lastReadBack.ColourSeconds,
                        PrivateMegabytes = privateBytes < 0 ? -1.0 : privateBytes / (1024.0 * 1024.0),
                        TreeItems = treeNow
                    });
                }
            }
            finally
            {
                // Each in its own try, so one that throws leaves the others released and the
                // inventory and the removals still run, as a throw in a view's own release did
                // when each view read its camera.
                foreach (Viewpoint camera in cameras.Values)
                {
                    try
                    {
                        camera.Dispose();
                    }
                    catch (Exception error)
                    {
                        log.Failure(
                            "releasing a camera read for the views",
                            error,
                            "kept going, the other cameras are released and the views step carries on");
                    }
                }
            }

            log.Line("VIEWS    " + written.Count + " of " + Plan.Views.Count + " planned view(s) written, "
                + foldersMarked + " folder(s) made and marked"
                + (foldersNotMarked > 0 ? ", " + foldersNotMarked + " made and NOT marked, said above" : string.Empty)
                + (hidesSkipped > 0 ? ", the hiding skipped for " + hidesSkipped + " view(s) showing the same models as the one before" : string.Empty));

            // F194. Where the seconds of the views went, view by view, said once for the group.
            foreach (string timingLine in timings.Lines(timedGroup))
            {
                log.Line(timingLine);
            }
        }

        /// <summary>
        /// Every planned view's camera, read once before the first view is touched, each a copy
        /// held by key until the views end, or why there is none. One that throws is logged with
        /// what happens next, and that view is failed with why and nothing is recorded for it.
        /// The seconds sit in none of the parts, where they sat when each view read its own.
        /// </summary>
        private void CamerasFirst(DocumentClashTests clashTests, Dictionary<string, Viewpoint> cameras, Dictionary<string, string> noCameras)
        {
            foreach (PlannedTestView view in Plan.Views)
            {
                if (cameras.ContainsKey(view.Key) || noCameras.ContainsKey(view.Key))
                {
                    continue;
                }

                try
                {
                    string why;
                    Viewpoint camera = CameraOf(clashTests, view.CameraClash, out why);

                    if (camera != null)
                    {
                        cameras[view.Key] = camera;
                    }
                    else
                    {
                        noCameras[view.Key] = why;
                    }
                }
                catch (Exception error)
                {
                    noCameras[view.Key] = "reading it threw " + error.GetType().Name + ": " + error.Message;
                    log.Failure(
                        "reading the camera of the view " + view,
                        error,
                        "kept going, that view is failed with why and the other views are still written");
                }
            }
        }

        private void WriteOne(
            Document document,
            DocumentClashTests clashTests,
            PlannedTestView view,
            IList<ModelTeam> models,
            Dictionary<ModelTeam, int> indexOf,
            string unitEnumName,
            ViewpointBuildOutcome outcome,
            List<WrittenView> written,
            Dictionary<string, IList<string>> hiddenReadBack,
            Dictionary<string, IList<ItemPath>> paintedReadBack,
            Dictionary<string, Viewpoint> cameras,
            Dictionary<string, string> noCameras,
            ref HashSet<int> shownBefore,
            ref int foldersMarked,
            ref int foldersNotMarked,
            ref int hidesSkipped)
        {
            ShownModels shown = ShownModels.For(view.Pair, models, view.Homes);
            HashSet<int> keep = new HashSet<int>();
            List<string> shows = new List<string>();

            foreach (ModelTeam model in shown.Shown)
            {
                int index;

                if (indexOf.TryGetValue(model, out index))
                {
                    keep.Add(index);
                    shows.Add(model.Code.Length == 0 ? model.FileName : model.Code);
                }
            }

            string showsWords = shows.Count == 0 ? "no model" : string.Join(" ", shows.ToArray());

            foreach (ModelTeam exception in shown.Exceptions)
            {
                SayOnce("VIEWS    " + view + " shows " + exception.FileName + " of " + exception.Team
                    + ", a third team's model one of its clashing items lives in, Q118 A");
            }

            // A VIEW OF NOTHING IS NO VIEW, the breaker's B2: shown nothing, it is counted failed
            // before anything is hidden or dimmed for it.
            if (shown.WhyNoView != null)
            {
                outcome.AddFailed(view.ToString(), showsWords, shown.WhyNoView);
                return;
            }

            // A person's unmarked view of this name in this folder cannot be told from the
            // one about to be made, P12 and P22 unrun, so none is made and the person's stays.
            // F114-K32, Bader's answer B: it is SKIPPED and named, never FAILED, so it does not
            // cost the group DONE.
            if (SavedViewpoints.CountUnmarked(document, view.Folders, view.Name, false, views) > 0)
            {
                outcome.AddSkipped(view.ToString(), showsWords,
                    "a view of its name with no mark of this tool sits in " + ViewPlace.FolderPath(view.Folders)
                    + ", a person's or one an earlier run could not mark, so none was written there and that one is left as it is");
                return;
            }

            // NO CAMERA IS NO VIEW, failed with why before anything is hidden or dimmed for it.
            Viewpoint first;

            if (!cameras.TryGetValue(view.Key, out first) || first == null)
            {
                string noCamera;
                noCameras.TryGetValue(view.Key, out noCamera);
                outcome.AddFailed(view.ToString(), showsWords, "Clash Detective gave no camera for its first clash, " + (noCamera ?? "UNKNOWN why"));
                return;
            }

            // THE HIDING, skipped where the view before showed the same models, P18, since
            // the document already holds that state.
            using (seconds.In(ViewsPart.ShowingAndHiding))
            {
                Touch(document);

                if (shownBefore != null && shownBefore.SetEquals(keep))
                {
                    hidesSkipped++;
                }
                else
                {
                    SavedViewpoints.ShowOnlyModels(document, keep);
                    shownBefore = new HashSet<int>(keep);
                }
            }

            // WHAT THE VIEW BEFORE LEFT IS TAKEN OFF FIRST, FR-065. The dimming and the paint
            // are temporary materials on the document, so without this a view carries the
            // last one's colours and a model dimmed for an earlier view stays dimmed while
            // hidden for this one, a material override per item in every view after it.
            if (dimmedNow)
            {
                using (seconds.In(ViewsPart.Dimming))
                {
                    SavedViewpoints.Undim(document);
                    dimmedNow = false;
                }
            }

            PaintPlan paint = PaintPlan.For(view.Clashes);
            int solidCount = 0;
            int redCount = 0;
            int greenCount = 0;
            int notResolved = 0;
            bool dimmedThisOne = false;

            if (views.DimsAnything && paint.Solid.Count > 0)
            {
                using (seconds.In(ViewsPart.Dimming))
                {
                    int lost;

                    using (ModelItemCollection solid = SavedViewpoints.Resolve(document, paint.Solid, out lost))
                    {
                        notResolved += lost;

                        // Before the call, so a dim that throws part way is still taken off
                        // before the next view is written.
                        dimmedNow = true;
                        dimmedAnything = true;
                        solidCount = SavedViewpoints.DimAllBut(document, views.DimTransparency, keep, solid);
                        dimmedThisOne = solidCount > 0;
                    }

                    // THE PAINT GOES ON AFTER THE DIMMING, Q58, one call per colour, P17.
                    if (dimmedThisOne && views.ColoursAnything)
                    {
                        using (ModelItemCollection red = SavedViewpoints.Resolve(document, paint.Red, out lost))
                        {
                            notResolved += lost;
                            redCount = SavedViewpoints.PaintMany(document, red, views.FirstItemColour);
                        }

                        using (ModelItemCollection green = SavedViewpoints.Resolve(document, paint.Green, out lost))
                        {
                            notResolved += lost;
                            greenCount = SavedViewpoints.PaintMany(document, green, views.SecondItemColour);
                        }
                    }
                }
            }

            if (notResolved > 0)
            {
                SayOnce("VIEWS    " + view + ": " + notResolved + " clashing item path(s) resolved no item, so they are not solid or painted");
            }

            if (paint.NotPointedAt > 0)
            {
                SayOnce("VIEWS    " + view + ": " + paint.NotPointedAt + " clash(es) with an item that could not be pointed at, so not painted");
            }

            // THE CAMERA, Clash Detective's own for the camera clash, read for every view before
            // the first was touched, CamerasFirst, framed on the box over every clash centre where
            // there are two or more, P16, and a copy of it where there is one. The copy is this
            // view's and the camera read first is held until every view is written, F141.
            Viewpoint camera = null;

            try
            {
                using (seconds.In(ViewsPart.Framing))
                {
                    List<Point3> centres = new List<Point3>();

                    foreach (ViewClash clash in view.Clashes)
                    {
                        if (clash.Centre != null)
                        {
                            centres.Add(clash.Centre);
                        }
                    }

                    FramingBox box = FramingBox.For(centres, views.FramingMarginMillimetres, unitEnumName);

                    camera = box != null ? SavedViewpoints.Framed(first, box) : first.CreateCopy();
                }

                Point3 cameraPoint;

                using (Point3D position = camera.Position)
                {
                    cameraPoint = new Point3(position.X, position.Y, position.Z);
                }

                // THE FOLDERS, reused where there and made where not, each one made marked
                // as the tool's, row F114-K5.
                using (seconds.In(ViewsPart.MakingTheFolders))
                {
                    IList<int> made = SavedViewpoints.EnsureFolders(document, view.Folders);

                    using (seconds.In(ViewsPart.Marking))
                    {
                        foreach (int depth in made)
                        {
                            List<string> above = new List<string>(view.Folders).GetRange(0, depth);
                            string whyNotMarked;
                            int at = SavedViewpoints.Mark(
                                document, above, view.Folders[depth], true,
                                depth == 0 ? ToolViewFolder.MarkerBody(stamp) : ToolViewMark.Body(stamp, above, view.Folders[depth], null, null, views),
                                views.MarkAuthor, views, out whyNotMarked);

                            if (at >= 0)
                            {
                                foldersMarked++;
                            }
                            else
                            {
                                foldersNotMarked++;
                                log.Line("VIEWS    the folder " + ViewPlace.Of(above, view.Folders[depth])
                                    + " was made and not marked, " + whyNotMarked + ", so a later run keeps it");
                            }
                        }
                    }
                }

                SavedViewpoints.Record(document, view.Folders, view.Name, camera, views.RecordsThroughTheFolder, seconds);

                int index;
                string whyNot;

                using (seconds.In(ViewsPart.Marking))
                {
                    index = SavedViewpoints.Mark(
                        document, view.Folders, view.Name, false,
                        ToolViewMark.Body(stamp, view.Folders, view.Name, cameraPoint, null, views),
                        views.MarkAuthor, views, out whyNot);
                }

                if (index < 0)
                {
                    // NOT MARKED IS REMOVED AT ONCE, the design's S2 and the breaker's B3: an
                    // unmarked view of the tool's would read as a person's every later run and
                    // block that test's view. It is taken out among the unmarked children of
                    // its folder of that name, exactly one or it is said, and counted failed.
                    RemovalReadBack taken;

                    using (seconds.In(ViewsPart.Removing))
                    {
                        taken = SavedViewpoints.RemoveUnmarked(document, view.Folders, view.Name, views);
                    }

                    // F4. The same count rule as the inventory's removals: removed only where its
                    // folder fell by exactly one, and the FAILED words say what happened.
                    string countWords = taken.Removed ? RemovalOutcome.CountWords(taken.CountBefore, taken.CountAfter) : null;
                    written.Add(new WrittenView(view.Folders, view.Name, -1, false, false));
                    outcome.AddFailed(view.ToString(), showsWords, "it was added and could not be marked, " + whyNot
                        + (!taken.Removed
                            ? ", and it was not removed, " + taken.WhyNot + ", so it stays unmarked and is named"
                            : countWords == null
                                ? ", so it was removed at once"
                                : ", and its removal is not proved, " + countWords + ", so whether an unmarked view of it stays is UNKNOWN"));
                    return;
                }

                ViewReadBack read;

                using (seconds.In(ViewsPart.ReadingBack))
                {
                    read = SavedViewpoints.ReadBack(
                        document, view.Folders, index, view.Name, camera, paint,
                        dimmedThisOne && views.ColoursAnything ? views.FirstItemColour : null,
                        dimmedThisOne && views.ColoursAnything ? views.SecondItemColour : null);
                }

                // F196, Bader's order of 15 Oct 2026, task 5. The overrides a read back lists are managed objects, each
                // with a native item, and a probe on 15 Oct 2026 read the same view five times: 0.75, 2.5, 4.2, 4.5 and
                // 6.4 s with nothing collected between and 8.1, 0.65, 0.49, 0.69 and 0.73 s with a full collection
                // between. The run that measured 3089 s of 3294 s of the views in this one call is why the objects of
                // each read back are collected before the next view is made. No view and no tree changes.
                using (seconds.In(ViewsPart.Collecting))
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                lastReadBack = read;
                string notReadBack = WhyNotReadBack(read, view, cameraPoint, shown.Hidden.Count, dimmedThisOne, redCount + greenCount);

                if (read.Found)
                {
                    hiddenReadBack[view.Key] = read.HiddenFiles;
                    paintedReadBack[view.Key] = read.Painted;
                }

                written.Add(new WrittenView(view.Folders, view.Name, index, true, notReadBack == null));

                if (notReadBack != null)
                {
                    outcome.AddFailed(view.ToString(), showsWords, notReadBack);
                    return;
                }

                outcome.AddCreated(view.ToString(), showsWords, shown.Hidden.Count);
            }
            finally
            {
                if (camera != null)
                {
                    camera.Dispose();
                }
            }
        }

        /// <summary>
        /// Why the view is not counted as read back, or null where every count is what was
        /// asked: there under its name, the camera within the tolerance, hiding something
        /// where it meant to, dimming something where it meant to, every painted item
        /// showing its colour, and the mark reading back as this run's.
        /// </summary>
        private string WhyNotReadBack(ViewReadBack read, PlannedTestView view, Point3 cameraPoint, int hidden, bool dimmed, int painted)
        {
            if (!read.Found)
            {
                return "it was added and a fresh read does not show it at its index under its name";
            }

            if (read.Camera == null)
            {
                return "it was added and its camera would not read back";
            }

            if (read.CameraDistance > views.CameraReadBackTolerance)
            {
                return "it was added with a camera " + read.CameraDistance.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
                    + " units from the camera asked for, so it would not open on its clashes";
            }

            if (hidden > 0 && read.HiddenCount == 0)
            {
                return "it was added without its hidden state, so it would show every model";
            }

            if (read.HiddenNotRoots > 0)
            {
                return "it hides " + read.HiddenNotRoots + " item(s) that are no model root, which this tool never hides";
            }

            if (dimmed && read.MaterialOverrideCount == 0)
            {
                return "it was added without its dimming, so the clashes would be behind whatever is in front of them";
            }

            if (painted > 0 && (read.ColoursWrong > 0 || read.ColoursNotRead > 0 || read.ColoursRight < read.ColoursAsked))
            {
                return "only " + read.ColoursRight + " of its " + read.ColoursAsked + " painted items would open in the colour given, "
                    + read.ColoursWrong + " in another and " + read.ColoursNotRead + " not read";
            }

            MarkJudgement judged = ToolViewMark.Judge(view.Folders, view.Name, read.Camera, read.Comments, read.Redlines, null, views);

            if (judged.Owner != ViewOwner.Ours || !string.Equals(judged.Mark.Stamp, stamp, StringComparison.Ordinal))
            {
                return "its mark does not read back as this run's, " + judged.Why;
            }

            return null;
        }

        /// <summary>
        /// A COPY of the camera Clash Detective frames the view's camera clash with, resolved
        /// again by the address the harvest recorded, after the plan and for this one clash,
        /// or null with why. The test and the result are released before this returns.
        /// </summary>
        private Viewpoint CameraOf(DocumentClashTests clashTests, ViewClash clash, out string whyNot)
        {
            whyNot = null;
            RowAddress where;

            if (!addressOf.TryGetValue(clash.Key, out where))
            {
                whyNot = "no address was recorded for it";
                return null;
            }

            string nowNamed;

            using (ClashTest test = where.Address.ResolveIn(clashTests, where.TestName, out nowNamed))
            {
                if (test == null)
                {
                    whyNot = where.TestName + " is not at " + where.Address + " any more";
                    return null;
                }

                bool moved;
                string notFound;
                SavedItem item = ResultPath.ResultAt(test, where.Row, where.TestName, false, out notFound, out moved);

                if (item == null)
                {
                    whyNot = notFound;
                    return null;
                }

                using (item)
                using (Viewpoint framed = clashTests.TestsViewpointForResult((IClashResult)item))
                {
                    if (framed == null)
                    {
                        whyNot = "TestsViewpointForResult returned nothing";
                        return null;
                    }

                    return framed.CreateCopy();
                }
            }
        }

        // ---------- the removals ----------

        /// <summary>
        /// The inventory's removals in its order, deepest first and the latest index first,
        /// each one re-found just before its RemoveAt, P13, a folder whole, P14, in its own
        /// try so one that throws costs no other.
        /// </summary>
        private void Remove(Document document, ViewsInventory inventory)
        {
            int notRemoved = 0;
            int foundElsewhere = 0;
            int countsOff = 0;
            removals = new List<RemovalOutcome>();

            foreach (InventoryItem item in inventory.Removals)
            {
                try
                {
                    RemovalReadBack answer = SavedViewpoints.RemoveOne(document, item.Node, views);

                    if (!answer.Removed)
                    {
                        notRemoved++;
                        removals.Add(new RemovalOutcome(item.Node, item.Decision, false, answer.WhyNot));
                        log.Line("VIEWS    " + item.Node + " was not removed, " + answer.WhyNot + ", so it stays");
                        continue;
                    }

                    // The document changed whichever way the count reads, so the NWF is saved again.
                    RemovedCount++;

                    if (answer.FoundElsewhere)
                    {
                        foundElsewhere++;
                    }

                    // F4. Removed only where the folder fell by exactly one, Core's one rule, and
                    // otherwise recorded as not removed with what happened.
                    RemovalOutcome judged = RemovalOutcome.Counted(item.Node, item.Decision, answer.CountBefore, answer.CountAfter);
                    removals.Add(judged);

                    if (!judged.Removed)
                    {
                        countsOff++;
                        log.Line("VIEWS    " + item.Node + " is counted as not removed, " + judged.WhyNot);
                    }
                }
                catch (Exception error)
                {
                    notRemoved++;
                    removals.Add(new RemovalOutcome(item.Node, item.Decision, false, "RemoveAt threw " + error.GetType().Name + ": " + error.Message));
                    log.Failure(
                        "removing " + item.Node + ", " + item.Why,
                        error,
                        "kept going, it stays in the tree and the next removal is still tried");
                }
            }

            log.Line("VIEWS    " + RemovedCount + " of " + inventory.Removals.Count + " RemoveAt call(s) made"
                + (foundElsewhere > 0 ? ", " + foundElsewhere + " found at another index than the walk read" : string.Empty)
                + (notRemoved > 0 ? ", " + notRemoved + " not made, each said above" : string.Empty)
                + (countsOff > 0 ? ", " + countsOff + " whose folder count did not fall by one and counted as not removed, said above" : string.Empty));
        }

        // ---------- the tree after ----------

        /// <summary>
        /// The last fresh walk, S3, and the VIEWS TREE block with its seven checks over it,
        /// kept for the engine to write. A walk that throws is said and the block is not
        /// written, because a block read off nothing would be a check that did not run.
        /// </summary>
        private void ReadTheTree(
            Document document,
            string group,
            ViewTeams teams,
            IList<ModelTeam> models,
            ViewsInventory inventory,
            IList<WrittenView> written,
            ICollection<string> emptyBefore,
            IDictionary<string, IList<string>> hiddenReadBack,
            IDictionary<string, IList<ItemPath>> paintedReadBack,
            ICollection<string> testsRan,
            ICollection<string> testNames)
        {
            try
            {
                TreeWalk after = SavedViewpoints.ReadTree(document, emptyBefore);
                SayWalk("after the removals, for the VIEWS TREE block", after);

                ViewsTreeFacts facts = new ViewsTreeFacts
                {
                    Group = group,
                    Map = teams.Map,
                    Models = models,
                    Plan = Plan,
                    Inventory = inventory,
                    Written = written,
                    Removals = removals,
                    After = ToolViewFolder.NodesUnder(after.Nodes),
                    RunStamp = stamp,
                    HiddenReadBack = hiddenReadBack,
                    PaintedReadBack = paintedReadBack,
                    TestsRun = testsRan,
                    KnownCodes = teams.Map.KnownCodes(CodesOf(models)),
                    TestNames = testNames,
                    Settings = views
                };

                IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);
                TreeLog = ViewsTree.Lines(facts, checks, views.TreeLinesInLog);
                TreeRows = ViewsTree.Lines(facts, checks, 0);
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading the saved viewpoints tree for the VIEWS TREE block",
                    error,
                    "kept going, no VIEWS TREE block is written for this group and its seven checks did not run");
            }
        }

        private static List<string> CodesOf(IList<ModelTeam> models)
        {
            List<string> codes = new List<string>();

            foreach (ModelTeam model in models)
            {
                if (model.Code.Length > 0 && !codes.Contains(model.Code))
                {
                    codes.Add(model.Code);
                }
            }

            return codes;
        }

        private void SayWalk(string when, TreeWalk walk)
        {
            log.Line("VIEWS    the tree read " + when + ": " + walk.Viewpoints + " viewpoint(s) and "
                + (walk.Nodes.Count - walk.Viewpoints) + " folder(s)"
                + (walk.CommentsNotRead > 0 ? ", " + walk.CommentsNotRead + " kept as a person's because its comments could not be read" : string.Empty)
                + (walk.RedlinesNotRead > 0 ? ", " + walk.RedlinesNotRead + " whose redlines would not read, kept as a person's" : string.Empty)
                + (walk.CamerasNotRead > 0 ? ", " + walk.CamerasNotRead + " whose camera would not read, kept as a person's" : string.Empty)
                + (walk.NeitherKind > 0 ? ", " + walk.NeitherKind + " neither a folder nor a saved viewpoint, kept" : string.Empty));
        }

        // ---------- the document put back ----------

        /// <summary>
        /// Reads what will have to be put back, once, before the first view changes the
        /// document: the hidden state, off a capture that never goes into the tree. The
        /// window's view is never touched, because the camera goes into the view directly
        /// and not through the window, 5m, so there is nothing of it to put back.
        /// </summary>
        private void Touch(Document document)
        {
            if (snapshot == null)
            {
                snapshot = SavedViewpoints.SnapshotHidden(document);
            }
        }

        /// <summary>
        /// The dimming taken off and the hidden state put back, each in its own try,
        /// released whether or not the restore worked, and read back and said.
        /// </summary>
        private void PutBack(Document document)
        {
            if (dimmedAnything)
            {
                try
                {
                    SavedViewpoints.Undim(document);
                    dimmedNow = false;
                    log.Line("VIEWS    the dimming and the paint were taken off the models this tool dimmed, so the document is back to its own appearance");
                }
                catch (Exception error)
                {
                    log.Failure(
                        "taking the dimming off after the views",
                        error,
                        "kept going, the models this tool dimmed are left transparent in this session and nothing of that is saved");
                }
                finally
                {
                    dimmedAnything = false;
                }
            }

            if (snapshot != null)
            {
                try
                {
                    int count = snapshot.HiddenCount;
                    bool readsHidden = SavedViewpoints.RestoreHiddenState(document, snapshot);

                    if (count == 0)
                    {
                        log.Line("VIEWS    hidden state put back: nothing was hidden before the views and nothing is hidden now");
                    }
                    else
                    {
                        log.Line("VIEWS    hidden state put back: " + count + " item(s) were hidden before the views and "
                            + (readsHidden ? "read as hidden again" : "DO NOT read as hidden again, said and not hidden"));
                    }
                }
                catch (Exception error)
                {
                    log.Failure(
                        "putting the hidden state back after the views",
                        error,
                        "kept going, the NWF saved next carries whatever is hidden now");
                }
                finally
                {
                    snapshot.Dispose();
                    snapshot = null;
                }
            }
        }

        private void SayOnce(string line)
        {
            if (saidOnce.Add(line))
            {
                log.Line(line);
            }
        }

        /// <summary>What walk one counted, said once per group after it.</summary>
        private sealed class Counts
        {
            public int NotFound;
            public string FirstNotFound;
            public int Moved;
            public int GroupRows;
            public int GroupsRead;
            public int MembersRead;
            public int MembersInScope;
            public int SizeRead;
            public int SideWalkThrew;
            public int HomesUnread;
            public Exception FirstHomeError;
            public int CentresUnread;
            public Exception FirstCentreError;
            public int NoCentre;

            public void Say(RunLog log, int clashes)
            {
                log.Line("VIEWS    " + clashes + " row(s) of the report read for the views, the size read for " + SizeRead
                    + " in a pair carrying the size folder" + (SideWalkThrew > 0
                        ? ", " + SideWalkThrew + " side(s) whose parent walk threw and read as the item alone, FR-072"
                        : string.Empty));

                if (Moved > 0)
                {
                    log.Line("VIEWS    " + Moved + " row(s) were found at another index than the harvest recorded, by name "
                        + "among the siblings, the compact after the merge having removed the Resolved results before them");
                }

                if (NotFound > 0)
                {
                    log.Line("VIEWS    " + NotFound + " row(s) of the report no longer lead to their result in the document, "
                        + "so their tests are not read for the views, the first: " + FirstNotFound);
                }

                if (GroupsRead > 0)
                {
                    log.Line("VIEWS    " + GroupsRead + " result group(s) read for the views, F114-K36: " + MembersRead
                        + " clash(es) inside them, " + MembersInScope + " at a status a view shows, each in its test's view at its own "
                        + "status and the group row itself in none");
                }

                if (GroupRows > 0)
                {
                    log.Line("VIEWS    " + GroupRows + " row(s) in scope are neither a clash nor a result group the walk could open, "
                        + "in their test's view with no items to paint and no size read");
                }

                if (HomesUnread > 0)
                {
                    log.Line("VIEWS    " + HomesUnread + " clash(es) whose items' models could not be read, so their views show "
                        + "the models of their other clashes alone and check 3 names them");

                    if (FirstHomeError != null)
                    {
                        log.Failure(
                            "reading the model a clash item lives in, the first of " + HomesUnread,
                            FirstHomeError,
                            "kept going, those clashes are in their views and their homes are UNKNOWN");
                    }
                }

                if (CentresUnread > 0 && FirstCentreError != null)
                {
                    log.Failure(
                        "reading a clash centre, the first of " + CentresUnread,
                        FirstCentreError,
                        "kept going, the view is framed on the centres that read");
                }

                if (NoCentre > 0)
                {
                    log.Line("VIEWS    " + NoCentre + " clash(es) in scope with no centre, so their views are framed on the rest");
                }
            }
        }
    }
}
