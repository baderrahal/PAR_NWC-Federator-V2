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
    /// of the merged report, the plan, then per view the hide, the dim, the paint, the
    /// frame, the folders, the record, the mark and the read back, then a fresh walk and
    /// the inventory over it, the removals it gave, the document put back, and a last
    /// fresh walk for the VIEWS TREE block and its seven checks.
    ///
    /// WALK ONE KEEPS F132's SHAPE, attempt 2 of its add-in half: the rows come off the
    /// merged report, ReportClashes.Of, under the test the report holds them under and at
    /// the row's status, and each row's result is resolved by the address the harvest
    /// recorded for it, the test once per address through TestAddress.ResolveIn and the
    /// result through ResultPath.ResultAt, never by name off the document. What each row
    /// becomes is a ViewClash: the two items' index paths, plain ints and not handles, the
    /// clash centre, the model each item lives in by its file name, and the size of the
    /// larger service ONLY for a clash whose test's pair carries the size folder, row
    /// F114-K9, so the teams are read before the walk. The camera is read after the plan,
    /// for each view's camera clash alone, TestsViewpointForResult on a copy, where F85
    /// read one per clash.
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

        /// <summary>How many items of earlier runs the inventory's removals took out, each a change to the document.</summary>
        public int RemovedCount { get; private set; }

        // What each removal came to, one list of results the tree block's counts are read off.
        private List<RemovalOutcome> removals;

        /// <summary>
        /// The whole of one group: walk, plan, write, inventory, remove, read the tree.
        /// Never throws past a view: one that throws is recorded as failed and the rest are
        /// still tried. The tests ran, the test names, the mirrors and whether the clash step
        /// was sound are the inventory's facts, Core's contracts of rows F114-K12 and K20.
        /// </summary>
        public ViewpointBuildOutcome BuildForGroup(
            Document document,
            string group,
            ClashReport report,
            IDictionary<ClashRow, RowAddress> addresses,
            ViewTeams teams,
            IList<ModelTeam> models,
            ICollection<string> testsRan,
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
            removals = null;

            try
            {
                using (seconds.In(ViewsPart.TakingTheInventory))
                {
                    TreeWalk before = SavedViewpoints.ReadTree(document, null);
                    SayWalk("before anything was written", before);
                    emptyBefore = ViewNode.EmptyFolderKeys(before.Nodes);
                }

                using (seconds.In(ViewsPart.ReadingTheClashes))
                {
                    List<ViewClash> clashes = Collect(document, clashTests, report, addresses, teams);

                    // N2. A test the clash step ran with no row on the report is read whole with
                    // no row, so the inventory removes its old view as no longer needed.
                    testsRead.Ran(testsRan);
                    Plan = TestViewPlan.For(clashes, teams, mirrors, views);
                }

                foreach (KeyValuePair<string, string> notWhole in testsRead.NotWhole())
                {
                    outcome.AddFailed(notWhole.Key, "UNKNOWN", notWhole.Value);
                }

                // F3, the lead's decision: a person's grouping must not cost the group DONE.
                foreach (string grouped in testsRead.OnlyGroups)
                {
                    log.Line("VIEWS    " + grouped + " gets no view: its open clashes are all result groups, which carry no items to show, "
                        + "so its views of earlier runs are kept and it is not counted as failed");
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
                        after.Nodes, Plan, testsRead.WholeNames, written, stamp, clashStepSound,
                        teams.Map.KnownCodes(CodesOf(groupModels)), testNames, views);
                }

                log.Block("VIEWS INVENTORY " + group, inventory.Lines());

                using (seconds.In(ViewsPart.Removing))
                {
                    Remove(document, inventory);
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
                if (!testsRead.NeedsReading(clash.Status))
                {
                    collected.Add(new CollectedRow(clash.ToView(null, null, null, null, null, null), null, clash.TestName));
                    continue;
                }

                RowAddress where;

                if (addresses == null || !addresses.TryGetValue(clash.Row, out where))
                {
                    noAddress++;
                    testsRead.RowWithNoAddress(clash.TestName, clash.Status);
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
                        // F114-K9, and the pair is the test's, read off its first row's sets.
                        bool carriesSize = teams.PairOf(under[0].LeftSet, under[0].RightSet).CarriesSizeFolder;

                        foreach (ReportClash clash in under)
                        {
                            string whyNot;
                            bool moved;
                            SavedItem item = ResultPath.ResultAt(
                                test, addresses[clash.Row].Row, first.TestName, out whyNot, out moved);

                            if (item == null)
                            {
                                counts.NotFound++;
                                testsRead.RowNotFound(clash.TestName, clash.Status);

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
            Counts counts)
        {
            if (!inScope.Contains(clash.Status))
            {
                return clash.ToView(null, null, null, null, null, null);
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

            return clash.ToView(serviceSize, firstItem, secondItem, centre, firstHome, secondHome);
        }

        /// <summary>
        /// Where one clashing item is, its index path as plain ints, and the model it lives
        /// in by that model's file name. MEASURED on 2026-09-20, docs\history\scan.md 5n: on
        /// a clash leaf HasModel reads false and Model reads null, and the one item that
        /// carries the model is the TOPMOST of its ancestors, six to nine levels up. Item1 is
        /// a fresh wrapper on every read, and so is every parent, so each is released here.
        /// </summary>
        private static ItemPath ReadPlace(Document document, ModelItem item, out string home)
        {
            home = null;

            if (item == null)
            {
                return null;
            }

            List<ModelItem> chain = new List<ModelItem>();
            chain.Add(item);

            try
            {
                int[] path = SavedViewpoints.PathOf(document, item);
                ModelItem walker = item;

                while (chain.Count < HomeWalkBound)
                {
                    ModelItem parent = walker.Parent;

                    if (parent == null)
                    {
                        break;
                    }

                    chain.Add(parent);
                    walker = parent;
                }

                ModelItem top = chain[chain.Count - 1];

                if (top.HasModel)
                {
                    using (Model model = top.Model)
                    {
                        home = model == null ? null : model.FileName;
                    }
                }

                return path == null ? null : new ItemPath(path);
            }
            finally
            {
                for (int i = 0; i < chain.Count; i++)
                {
                    chain[i].Dispose();
                }
            }
        }

        /// <summary>
        /// The most levels the home walk climbs. 5n measured six and nine on this project,
        /// and a bound stops a malformed tree turning one clash into an endless climb. It
        /// is a guard on a walk and not a number that shapes a run.
        /// </summary>
        private const int HomeWalkBound = 64;

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

                try
                {
                    WriteOne(document, clashTests, view, models, indexOf, unitEnumName, outcome, written,
                        hiddenReadBack, paintedReadBack, ref shownBefore, ref foldersMarked, ref foldersNotMarked, ref hidesSkipped);
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
            }

            log.Line("VIEWS    " + written.Count + " of " + Plan.Views.Count + " planned view(s) written, "
                + foldersMarked + " folder(s) made and marked"
                + (foldersNotMarked > 0 ? ", " + foldersNotMarked + " made and NOT marked, said above" : string.Empty)
                + (hidesSkipped > 0 ? ", the hiding skipped for " + hidesSkipped + " view(s) showing the same models as the one before" : string.Empty));
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
            if (SavedViewpoints.CountUnmarked(document, view.Folders, view.Name, false, views) > 0)
            {
                outcome.AddFailed(view.ToString(), showsWords,
                    "a view of its name with no mark of this tool sits in " + ViewPlace.FolderPath(view.Folders)
                    + ", a person's or one an earlier run could not mark, so none was written there and that one is left as it is");
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

            // THE CAMERA, Clash Detective's own for the camera clash, framed on the box over
            // every clash centre where there are two or more, P16.
            Viewpoint camera = null;

            try
            {
                string noCamera;
                camera = CameraOf(clashTests, view.CameraClash, out noCamera);

                if (camera == null)
                {
                    outcome.AddFailed(view.ToString(), showsWords, "Clash Detective gave no camera for its first clash, " + noCamera);
                    return;
                }

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

                    if (box != null)
                    {
                        Viewpoint framed = SavedViewpoints.Framed(camera, box);
                        camera.Dispose();
                        camera = framed;
                    }
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
                                ToolViewMark.Body(stamp, above, view.Folders[depth], null, null, views),
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
                SavedItem item = ResultPath.ResultAt(test, where.Row, where.TestName, out notFound, out moved);

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
                    After = after.Nodes,
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

                if (GroupRows > 0)
                {
                    log.Line("VIEWS    " + GroupRows + " row(s) in scope are a result group, in their test's view framed on the "
                        + "group as the workbook holds them, with no items to paint and no size read, because a group has no two items");
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
