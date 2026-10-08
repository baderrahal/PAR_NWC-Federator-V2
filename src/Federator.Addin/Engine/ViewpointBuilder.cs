using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using Federator.Core.Views;
using CoreClashStatus = Federator.Core.Clash.ClashStatus;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Puts one saved viewpoint per clash into the NWF, three folders deep, F85. The plan
    /// is Federator.Core.Views.ClashViewpointPlan and nothing about it is decided here.
    ///
    /// TWO WALKS OVER THE RESULTS AND NOT ONE. The first reads every clash the report
    /// names into a ClashToPlan, the status, the two set names, the priority and the
    /// service size, keeps a COPY of the camera Clash Detective frames it with, and notes
    /// which models the two clashing items live in. The plan then runs over all of them
    /// at once, because a cap per test and the counts in the block are about the whole
    /// group. The second writes what the plan kept. Nothing borrowed from the document is
    /// held across either.
    ///
    /// WHAT A VIEWPOINT SHOWS. The two disciplines of the pair, every model of each, plus
    /// the model each clashing item lives in, and every other model hidden, framed on the
    /// clash the way the picture of it is framed, TestsViewpointForResult. That is what a
    /// person pressing AR vs ST expects to see, the two things that clashed in their own
    /// context, and the hiding is what 5j measured a captured viewpoint to keep. The
    /// models the items live in are kept as well as the pair's because a set code and a
    /// file code are not the same list: a DR set, drainage, lives in the ME model, and a
    /// viewpoint that hid ME for DR vs ST would hide the pipe the person is looking for,
    /// which is what the third and fourth runs did before 5n measured where the model
    /// sits. A pair with a code this tool does not know hides nothing, a model whose name
    /// will not parse is never hidden, and a pair no model carries hides nothing and says so.
    ///
    /// AND EVERYTHING BUT THE TWO CLASHING ITEMS IS DIMMED, the dimming round. F85 shipped
    /// without it, and Bader pressed two of its viewpoints and saw a grey wall: the camera
    /// Clash Detective computes sits inside a beam, and a solid beam fills the screen.
    /// Clash Detective only looks right because its own view makes everything but the two
    /// items transparent. So walk one keeps the INDEX PATH of each clashing item, plain
    /// ints rather than a handle held across the group, and walk two overrides temporary
    /// transparency on the model roots, which reaches every leaf, and resets it on those
    /// two, which brings back exactly those two. Two calls and not one per item: 2,606
    /// items dimmed one at a time, 430 times, is 1.1 million calls in one group.
    /// docs\history\scan.md 5o measured all of it, including that the record survives a
    /// save and a reopen and that the permanent override would have risked his own.
    ///
    /// EVERYTHING IS PUT BACK, MEASURED. Before the first viewpoint changes anything the
    /// hidden state the document holds is read off a runtime capture that never goes
    /// into the tree. When the group's writing ends, whichever way it ends, the dimming
    /// is taken off the models this tool dimmed, everything is shown, and exactly the
    /// items that were hidden are hidden again and read back as hidden, each in its own
    /// try so one failing cannot skip the other. scan.md 5k measured the hidden half and
    /// measured that ResetAllHiddenToModelState, which this once called, LOSES a hide the
    /// document held, and 5o measured the same trap in the permanent material override,
    /// whose only undo would clear an appearance override of his that nothing can read
    /// back first. A group that hid and dimmed nothing touches none of it.
    ///
    /// AND NOTHING HERE IS A FLAG. Both flags this API offers, ContainsVisibilityOverrides
    /// and ContainsAppearanceOverrides, read TRUE on a viewpoint that recorded neither,
    /// 5o, so F85's check that a viewpoint carries visibility overrides could not fail.
    /// The read back is four counts: it is there, its camera is within the tolerance, it
    /// hides as many items as it meant to, and it dims as many as it meant to.
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
        private int cameraRead;
        private int dimmed;
        private int notDimmed;
        private int painted;
        private bool dimmedAnything;
        private bool paintedAnything;

        // FR-065. Whether this tool's dimming and paint are on the document right now,
        // which the next viewpoint takes off before it is written, and what the written
        // viewpoints read back of it, so a run can see that each carries its own.
        private bool dimmedNow;
        private int fewestOverrides;
        private int mostOverrides;
        private int undimmedCarrying;
        private Exception firstHomeError;

        // Where the VIEWS step's seconds go, per call, because the dimming took one
        // group from 7.5 seconds to 476 and the shape of the cost was not what it
        // looked like: the group with the MOST items was one of the fastest. A step
        // that got slower says which call did it rather than leaving it to be guessed.
        // FR-073: every call the work is made of is its own part, recording split into
        // the folders, the view, the COM folder and the add, and what falls between the
        // parts is said. The rule is Federator.Core.Views.ViewsSeconds, on the log's clock.
        private ViewsSeconds seconds;

        /// <summary>
        /// Where one clash is: the models its two items live in, and the index path of
        /// each item. The PATH and not the item, because walk one names every clash in
        /// the group and walk two resolves them one at a time, and 1,950 native handles
        /// held across a group is the shape that once built 1.7 million of them.
        /// </summary>
        private sealed class ClashPlace
        {
            public ClashPlace()
            {
                Models = new HashSet<int>();
            }

            public HashSet<int> Models { get; private set; }

            public int[] FirstPath { get; set; }

            public int[] SecondPath { get; set; }

            /// <summary>Whether both items can be pointed at, which is what dimming all but two needs.</summary>
            public bool BothPlaced
            {
                get { return FirstPath != null && SecondPath != null; }
            }
        }

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

        /// <summary>The plan for the group, kept so the engine can write its block.</summary>
        public ClashViewpointPlanOutcome Plan { get; private set; }

        /// <summary>
        /// The whole of one group: read, plan, write. Never throws past a viewpoint: one
        /// that throws is recorded as failed and the rest are still tried.
        /// </summary>
        public ViewpointBuildOutcome BuildForGroup(
            Document document,
            ClashReport report,
            IDictionary<ClashRow, RowAddress> addresses,
            bool priorityPicked,
            IDictionary<int, string> modelDisciplines)
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();

            if (document == null || report == null)
            {
                return outcome;
            }

            seconds = new ViewsSeconds(() => log.ElapsedSeconds);
            DocumentClashTests clashTests = document.GetClash().TestsData;
            List<ClashToPlan> clashes = new List<ClashToPlan>();
            Dictionary<string, Viewpoint> cameras = new Dictionary<string, Viewpoint>(StringComparer.Ordinal);
            Dictionary<string, ClashPlace> places = new Dictionary<string, ClashPlace>(StringComparer.Ordinal);
            IDictionary<int, string> disciplines = modelDisciplines ?? new Dictionary<int, string>();
            snapshot = null;
            cameraRead = 0;
            dimmed = 0;
            notDimmed = 0;
            painted = 0;
            dimmedAnything = false;
            paintedAnything = false;
            dimmedNow = false;
            fewestOverrides = -1;
            mostOverrides = 0;
            undimmedCarrying = 0;
            firstHomeError = null;

            try
            {
                using (seconds.In(ViewsPart.ReadingTheClashes))
                {
                    Collect(document, clashTests, report, addresses, clashes, cameras, places, ModelIndexByFile(document));
                    Plan = ClashViewpointPlan.For(clashes, views, priorityPicked);
                }

                if (Plan.Planned.Count == 0)
                {
                    return outcome;
                }

                foreach (PlannedClashViewpoint planned in Plan.Planned)
                {
                    try
                    {
                        using (seconds.In(ViewsPart.SayingHowFar))
                        {
                            progress("Viewpoint " + planned.Path);
                        }

                        WriteOne(document, planned, cameras, places, disciplines, outcome);
                    }
                    catch (Exception error)
                    {
                        // The type as well as the text, because a member of this API
                        // behaving differently from the way 5d, 5j and 5k measured it is
                        // the most likely failure, and the type name is what says which.
                        outcome.AddFailed(planned.Path, planned.Pair.Folder, error.GetType().Name + ": " + error.Message);
                        log.Failure(
                            "putting the viewpoint " + planned.Path + " into the NWF",
                            error,
                            "kept going, the other viewpoints are still tried and the block carries the total");
                    }
                }

                if (cameraRead > 0)
                {
                    log.Line("VIEWS    read back on " + cameraRead + " created viewpoint(s): each sits within "
                        + views.CameraReadBackTolerance.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
                        + " units of its clash camera, and the items each one hides and dims were counted off it and not trusted");

                    if (views.DimsAnything)
                    {
                        // FR-065. The material overrides each one read back, the fewest
                        // and the most, so a run shows that a viewpoint carries the
                        // dimming of its own shown models and not what an earlier one
                        // left on models it hides, and that one written undimmed carries
                        // none of an earlier one's dimming or paint.
                        log.Line("VIEWS    dimmed to "
                            + views.DimTransparency.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)
                            + " with the two clashing items left solid: " + dimmed + " viewpoint(s)"
                            + (dimmed > 0 ? ", each carrying from " + fewestOverrides + " to " + mostOverrides + " material overrides" : string.Empty)
                            + (notDimmed > 0
                                ? ", and " + notDimmed + " written undimmed because their two items could not both be pointed at, "
                                    + undimmedCarrying + " of them carrying a material override"
                                : string.Empty));
                    }

                    if (paintedAnything)
                    {
                        log.Line("VIEWS    the two clashing items painted " + views.FirstItemColour
                            + " and " + views.SecondItemColour + ", read back on what each viewpoint will show: "
                            + painted + " viewpoint(s)");
                    }
                }
            }
            finally
            {
                foreach (Viewpoint camera in cameras.Values)
                {
                    camera.Dispose();
                }

                using (seconds.In(ViewsPart.PuttingBack))
                {
                    PutBack(document);
                }

                // Last, so the whole runs from the first clash read to the document put
                // back, and written whichever way the work ended, because time spent
                // failing is time the run spent.
                seconds.Ended();
                log.Line(seconds.Line());
            }

            return outcome;
        }

        /// <summary>
        /// Every clash the merged report holds a row for, read into what the plan needs,
        /// with a copy of its camera and the models its items live in kept by the name the
        /// plan will give it. F132 attempt 2, the breaker's finding R4: the rows come off
        /// the report and not off the document, Federator.Core.Views.ReportClashes, so a
        /// row only a mirror found is viewed under the kept test, a mirror taken out of the
        /// report gets no view, and the status is the row's, which the merge restated by
        /// Q138 B. Each row's result is resolved in the document by the address the harvest
        /// recorded for it, the test once for all its rows and never held across another
        /// test's read, and read back by name, ResultPath, since the compact after the
        /// merge can move a result from under a recorded path. A test name the report
        /// carries twice is read once, because both would resolve to the same document test
        /// and every clash of it would be handed to the plan twice.
        /// </summary>
        private void Collect(
            Document document,
            DocumentClashTests clashTests,
            ClashReport report,
            IDictionary<ClashRow, RowAddress> addresses,
            List<ClashToPlan> clashes,
            Dictionary<string, Viewpoint> cameras,
            Dictionary<string, ClashPlace> places,
            IDictionary<string, int> indexByFile)
        {
            string unitEnumName = Penetrations.UnitEnumName(document);
            ReportClashesOutcome rows = ReportClashes.Of(report);

            foreach (string name in rows.NamedTwice)
            {
                log.Line("VIEWS    " + name + " is on the report twice, so its clashes are read once and planned once");
            }

            List<string> order = new List<string>();
            Dictionary<string, List<ReportClash>> byTest = new Dictionary<string, List<ReportClash>>(StringComparer.Ordinal);
            Dictionary<string, RowAddress> firstOf = new Dictionary<string, RowAddress>(StringComparer.Ordinal);
            int noAddress = 0;

            foreach (ReportClash clash in rows.Clashes)
            {
                RowAddress where;

                if (addresses == null || !addresses.TryGetValue(clash.Row, out where))
                {
                    noAddress++;
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
                log.Line("VIEWS    " + noAddress + " row(s) of the report have no recorded place in the document, so they get no viewpoint");
            }

            int homesUnread = 0;
            int notFound = 0;
            int groupRows = 0;
            int movedRows = 0;
            string firstNotFound = null;

            foreach (string key in order)
            {
                RowAddress first = firstOf[key];
                List<ReportClash> under = byTest[key];
                string nowNamed;

                try
                {
                    using (ClashTest test = first.Address.ResolveIn(clashTests, first.TestName, out nowNamed))
                    {
                        if (test == null)
                        {
                            log.Line("VIEWS    " + first.TestName + " is not at " + first.Address + " any more"
                                + (nowNamed == null ? string.Empty : ", which holds \"" + nowNamed + "\"")
                                + ", so its " + under.Count + (under.Count == 1 ? " row gets" : " rows get") + " no viewpoint");
                            continue;
                        }

                        foreach (ReportClash clash in under)
                        {
                            string whyNot;
                            bool moved;
                            SavedItem item = ResultPath.ResultAt(
                                test, addresses[clash.Row].Row, first.TestName, out whyNot, out moved);

                            if (item == null)
                            {
                                notFound++;

                                if (firstNotFound == null)
                                {
                                    firstNotFound = whyNot;
                                }

                                continue;
                            }

                            if (moved)
                            {
                                movedRows++;
                            }

                            using (item)
                            {
                                homesUnread += CollectOne(
                                    document, clashTests, clash, (IClashResult)item, item as ClashResult,
                                    unitEnumName, clashes, cameras, places, indexByFile, ref groupRows);
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Failure(
                        "reading the clashes of " + first.TestName + " for the viewpoints",
                        error,
                        "kept going, the clashes read before it threw are planned and the rest are not");
                }
            }

            if (movedRows > 0)
            {
                log.Line("VIEWS    " + movedRows + " row(s) were found at another index than the harvest recorded, by name "
                    + "among the siblings, the compact after the merge having removed the Resolved results before them");
            }

            if (notFound > 0)
            {
                log.Line("VIEWS    " + notFound + " row(s) of the report no longer lead to their result in the document, "
                    + "so they get no viewpoint, the first: " + firstNotFound);
            }

            if (groupRows > 0)
            {
                log.Line("VIEWS    " + groupRows + " row(s) in scope are a result group, one viewpoint each as the workbook "
                    + "holds them, framed on the group, with no size read and not dimmed, because a group has no two items");
            }

            if (homesUnread > 0)
            {
                log.Line("VIEWS    " + homesUnread + " clash(es) whose items' models could not be read, so their viewpoints keep the pair's models only");

                if (firstHomeError != null)
                {
                    log.Failure(
                        "reading the model a clash item lives in, the first of " + homesUnread,
                        firstHomeError,
                        "kept going, those viewpoints keep the pair's models only");
                }
            }

            int withHome = 0;
            int withBothItems = 0;

            foreach (ClashPlace place in places.Values)
            {
                if (place.Models.Count > 0)
                {
                    withHome++;
                }

                if (place.BothPlaced)
                {
                    withBothItems++;
                }
            }

            log.Line("VIEWS    the model each clash item lives in was read for " + withHome + " of " + places.Count
                + " clash(es) in scope" + (withHome == places.Count ? string.Empty : ", and the rest keep the pair's models only"));

            if (views.DimsAnything)
            {
                log.Line("VIEWS    both clashing items were pointed at for " + withBothItems + " of " + places.Count
                    + " clash(es) in scope" + (withBothItems == places.Count ? string.Empty : ", and the rest are not dimmed, because dimming all but two needs both"));
            }
        }

        /// <summary>
        /// One row of the report into the plan at its row status, and where it is in scope
        /// its camera and the place of its two items. The leaf is the result where the row
        /// is one clash, and null where the row is a result group, which the panel frames
        /// as one and which has no two items to read a size or a place off. Returns how
        /// many homes could not be read, nought or one.
        /// </summary>
        private int CollectOne(
            Document document,
            DocumentClashTests clashTests,
            ReportClash clash,
            IClashResult result,
            ClashResult leaf,
            string unitEnumName,
            List<ClashToPlan> clashes,
            Dictionary<string, Viewpoint> cameras,
            Dictionary<string, ClashPlace> places,
            IDictionary<string, int> indexByFile,
            ref int groupRows)
        {
            bool inScope = ClashViewpointPlan.InScope(clash.Status);

            // The size is read only where the plan will ask about it, which is a clash it
            // would otherwise keep. A closed clash is left out on its status before the
            // size is looked at, so its items are not read.
            SizeVerdict? serviceSize = inScope && leaf != null
                ? Penetrations.ServiceSizeOf(leaf, penetrations, sizes, unitEnumName)
                : null;

            ClashToPlan planned = clash.ToPlan(serviceSize);
            clashes.Add(planned);

            if (!inScope)
            {
                return 0;
            }

            if (leaf == null)
            {
                groupRows++;
            }

            string key = ClashViewpointPlan.NameFor(planned, views);

            if (cameras.ContainsKey(key))
            {
                return 0;
            }

            using (Viewpoint framed = clashTests.TestsViewpointForResult(result))
            {
                if (framed != null)
                {
                    cameras.Add(key, framed.CreateCopy());
                }
            }

            ClashPlace place = new ClashPlace();
            int homesUnread = 0;

            if (leaf != null)
            {
                try
                {
                    place.FirstPath = ReadPlace(document, leaf.Item1, indexByFile, place.Models);
                    place.SecondPath = ReadPlace(document, leaf.Item2, indexByFile, place.Models);
                }
                catch (Exception error)
                {
                    // Counted, and the FIRST one is written in full once per group,
                    // because the fifth run counted 975 of these and could not say
                    // what threw. The viewpoint still keeps the pair's models, it
                    // just cannot also keep a model the code did not name, and a
                    // clash whose items will not read is not lost.
                    homesUnread++;

                    if (firstHomeError == null)
                    {
                        firstHomeError = error;
                    }
                }
            }

            places[key] = place;
            return homesUnread;
        }

        /// <summary>
        /// The model one clashing item lives in, by its index in the document, matched on
        /// the model's file name because that is a string and not a wrapper. MEASURED on
        /// 2026-09-20, docs\history\scan.md 5n: on a clash leaf HasModel reads false and
        /// Model reads null, and the one item that carries the model is the TOPMOST of
        /// AncestorsAndSelf, six to nine levels up. Three runs before that measurement
        /// read Model off the leaf and found no home for any clash. Item1 is a fresh
        /// wrapper on every read, and so is every ancestor enumerated, so each is released
        /// here.
        /// </summary>
        private static int[] ReadPlace(
            Document document, ModelItem item, IDictionary<string, int> indexByFile, HashSet<int> into)
        {
            if (item == null)
            {
                return null;
            }

            // Parent by Parent, each a fresh wrapper, all released at the end, which is
            // the shape Penetrations.Upwards has read sizes with on every run. Enumerating
            // AncestorsAndSelf and disposing each item as it went threw on every clash of
            // the fifth run, and disposing nothing is not an option under 4g.
            List<ModelItem> chain = new List<ModelItem>();
            chain.Add(item);

            try
            {
                // The path of the LEAF, which is the item that clashed and the one the
                // dimming brings back to solid, taken as plain ints here so walk two can
                // find it again without this handle, 5o.
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

                if (!top.HasModel)
                {
                    return path;
                }

                using (Model model = top.Model)
                {
                    int index;

                    if (model != null && indexByFile.TryGetValue(Words.Or(model.FileName, string.Empty), out index))
                    {
                        into.Add(index);
                    }
                }

                return path;
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

        private static IDictionary<string, int> ModelIndexByFile(Document document)
        {
            Dictionary<string, int> index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (document.Models == null)
            {
                return index;
            }

            for (int i = 0; i < document.Models.Count; i++)
            {
                using (Model model = document.Models[i])
                {
                    string file = Words.Or(model.FileName, string.Empty);

                    if (file.Length > 0 && !index.ContainsKey(file))
                    {
                        index.Add(file, i);
                    }
                }
            }

            return index;
        }

        private void WriteOne(
            Document document,
            PlannedClashViewpoint planned,
            Dictionary<string, Viewpoint> cameras,
            Dictionary<string, ClashPlace> places,
            IDictionary<int, string> disciplines,
            ViewpointBuildOutcome outcome)
        {
            // Already there is left exactly as it is. F28's rule, carried to viewpoints: a
            // second copy at one path leaves the tree holding both and whichever came first
            // is what anything resolving that path finds.
            bool alreadyThere;

            using (seconds.In(ViewsPart.LookingWhetherThere))
            {
                alreadyThere = SavedViewpoints.Exists(document, planned.Folders, planned.Name);
            }

            if (alreadyThere)
            {
                outcome.AddAlreadyPresent(planned.Path, planned.Pair.Folder, 0);
                return;
            }

            Viewpoint camera;

            if (!cameras.TryGetValue(planned.Name, out camera))
            {
                outcome.AddFailed(planned.Path, planned.Pair.Folder, "Clash Detective gave no camera for this clash");
                return;
            }

            HashSet<int> keep = new HashSet<int>();
            HashSet<string> hidden = new HashSet<string>(StringComparer.Ordinal);
            string hidesNothingBecause = null;

            ClashPlace place;
            places.TryGetValue(planned.Name, out place);
            HashSet<int> home = place == null ? null : place.Models;

            if (planned.Pair.BothKnown)
            {
                bool firstHasModel = false;
                bool secondHasModel = false;

                foreach (KeyValuePair<int, string> model in disciplines)
                {
                    bool first = string.Equals(model.Value, planned.Pair.First, StringComparison.Ordinal);
                    bool second = string.Equals(model.Value, planned.Pair.Second, StringComparison.Ordinal);
                    firstHasModel |= first;
                    secondHasModel |= second;

                    if (first || second || model.Value.Length == 0 || (home != null && home.Contains(model.Key)))
                    {
                        keep.Add(model.Key);
                    }
                    else
                    {
                        hidden.Add(model.Value);
                    }
                }

                if (!firstHasModel)
                {
                    SayOnce("VIEWS    no model in this group carries " + planned.Pair.First
                        + ", so a viewpoint of its pair keeps the model each clash item lives in");
                }

                if (!secondHasModel && !string.Equals(planned.Pair.First, planned.Pair.Second, StringComparison.Ordinal))
                {
                    SayOnce("VIEWS    no model in this group carries " + planned.Pair.Second
                        + ", so a viewpoint of its pair keeps the model each clash item lives in");
                }

                if (keep.Count == 0)
                {
                    bool sameCode = string.Equals(planned.Pair.First, planned.Pair.Second, StringComparison.Ordinal);
                    hidesNothingBecause = "no model in this group carries "
                        + (sameCode ? planned.Pair.First : planned.Pair.First + " or " + planned.Pair.Second)
                        + " and the models its clash items live in could not be read";
                }
            }
            else
            {
                // A code this tool does not know. Nothing is hidden, because hiding on a
                // guess would hide the thing the person is looking for, and the plan has
                // already counted it under UNKNOWN.
                hidesNothingBecause = "its pair has a code this tool does not know";
            }

            if (hidesNothingBecause != null)
            {
                SayOnce("VIEWS    " + planned.Pair.Folder + ": " + hidesNothingBecause + ", so its viewpoints hide nothing");
                hidden.Clear();
            }

            using (seconds.In(ViewsPart.ShowingAndHiding))
            {
                Touch(document);

                if (hidesNothingBecause == null)
                {
                    SavedViewpoints.ShowOnlyModels(document, keep);
                }
                else
                {
                    document.Models.ResetAllHidden();
                }
            }

            // THE DIMMING, the whole of this round. Everything goes transparent and the
            // two items the clash is between come back solid, so the viewpoint opens the
            // way Clash Detective looks at a clash. F85 shipped without it and Bader
            // pressed two viewpoints and saw a grey wall, because the camera Clash
            // Detective computes sits inside a beam and a solid beam fills the screen.
            // A clash whose two items cannot both be pointed at is left undimmed rather
            // than dimmed whole, because everything transparent and nothing solid is
            // worse than what F85 shipped, and it is counted and said.
            int solid = 0;
            bool dimmedThisOne = false;
            bool paintedThisOne = false;

            // WHAT THE VIEWPOINT BEFORE LEFT IS TAKEN OFF FIRST, FR-065. The dimming and
            // the paint are temporary materials on the document, so without this a
            // viewpoint whose two items could not both be pointed at was recorded with the
            // last one's dimming and red and green and counted as undimmed, and a model
            // dimmed for an earlier pair stayed dimmed while hidden for a later one, which
            // costs a material override per item in every viewpoint after it, the shape
            // that once put 33 MB into a 120 KB NWF. Undim is scoped to the model roots
            // and measured at under a millisecond, 5o.
            if (dimmedNow)
            {
                using (seconds.In(ViewsPart.Dimming))
                {
                    SavedViewpoints.Undim(document);
                    dimmedNow = false;
                }
            }

            if (views.DimsAnything && place != null && place.BothPlaced)
            {
                using (seconds.In(ViewsPart.Dimming))
                {
                    using (ModelItem firstItem = SavedViewpoints.ItemAt(document, place.FirstPath))
                    using (ModelItem secondItem = SavedViewpoints.ItemAt(document, place.SecondPath))
                    {
                        if (firstItem != null && secondItem != null)
                        {
                            // Before the call, so a dim that throws part way is still
                            // taken off before the next viewpoint is written.
                            dimmedNow = true;
                            solid = SavedViewpoints.DimAllBut(
                                document, views.DimTransparency, hidesNothingBecause == null ? keep : null, firstItem, secondItem);
                            dimmedThisOne = solid == 2;
                            dimmedAnything = true;

                            // THE PAINT GOES ON AFTER THE DIMMING, Q58. The transparency
                            // override on the roots reaches every leaf, so painting first
                            // would put the colour on and dim it off again in the same call.
                            if (dimmedThisOne && views.ColoursAnything)
                            {
                                paintedThisOne = SavedViewpoints.PaintTwo(
                                    document,
                                    firstItem,
                                    views.FirstItemColour,
                                    secondItem,
                                    views.SecondItemColour) == 2;
                                paintedAnything = true;
                            }
                        }
                    }

                    if (!dimmedThisOne)
                    {
                        // Dimmed on a path that resolved nothing, so it is taken straight off
                        // again and the viewpoint is written the way F85 wrote one.
                        SavedViewpoints.Undim(document);
                        dimmedNow = false;
                        solid = 0;
                        paintedThisOne = false;
                    }
                }
            }

            // Recording, which was one watch around three calls and is now one part per
            // call, FR-073: the folders here, and the view, its COM folder and the add
            // inside Record, so the next run says which of them takes the time.
            using (seconds.In(ViewsPart.MakingTheFolders))
            {
                SavedViewpoints.EnsureFolders(document, planned.Folders);
            }

            SavedViewpoints.Record(document, planned.Folders, planned.Name, camera, views.RecordsThroughTheFolder, seconds);

            // Read back rather than trusted, all three of it. The first run's tree looked
            // complete and every viewpoint opened on sky, because the route it used
            // recorded no camera, 5l, and nothing read the camera back. A viewpoint whose
            // recorded camera is not the clash camera, or which carries no overrides
            // while it was meant to hide something, is not a viewpoint of that clash and
            // is counted as failed with the reason a person can check.
            ViewpointReadBack read;

            using (seconds.In(ViewsPart.ReadingBack))
            {
                read = paintedThisOne
                    ? SavedViewpoints.ReadBack(
                        document,
                        planned.Folders,
                        planned.Name,
                        camera,
                        place.FirstPath,
                        views.FirstItemColour,
                        place.SecondPath,
                        views.SecondItemColour)
                    : SavedViewpoints.ReadBack(document, planned.Folders, planned.Name, camera);
            }

            if (!read.Found)
            {
                outcome.AddFailed(planned.Path, planned.Pair.Folder, "it was added and a fresh read does not show it");
                return;
            }

            if (read.CameraDistance > views.CameraReadBackTolerance)
            {
                outcome.AddFailed(
                    planned.Path,
                    planned.Pair.Folder,
                    "it was added with a camera " + read.CameraDistance.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
                    + " units from the clash camera, so it would not open on the clash");
                return;
            }

            // THE COUNT AND NOT THE FLAG, 5o. ContainsVisibilityOverrides reads true on a
            // viewpoint that hides nothing, so the check F85 shipped could not fail. The
            // number of items the viewpoint hides can.
            if (hidden.Count > 0 && read.HiddenCount == 0)
            {
                outcome.AddFailed(planned.Path, planned.Pair.Folder, "it was added without its hidden state, so it would show every discipline");
                return;
            }

            if (dimmedThisOne && read.MaterialOverrideCount == 0)
            {
                outcome.AddFailed(planned.Path, planned.Pair.Folder, "it was added without its dimming, so the clash would be behind whatever is in front of it");
                return;
            }

            // THE FOURTH COUNT, Q58. What the viewpoint will SHOW for each of the two,
            // which is the override's colour where it names the item and the item's own
            // colour where it does not, 5p. A viewpoint that would open with the two
            // items in the wrong colours is not the viewpoint that was asked for.
            if (read.ColoursAsked && read.ColoursRight < 2)
            {
                outcome.AddFailed(
                    planned.Path,
                    planned.Pair.Folder,
                    "it was added and only " + read.ColoursRight.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + " of the two clashing items would open in the colour it was given");
                return;
            }

            cameraRead++;

            if (dimmedThisOne)
            {
                dimmed++;
                fewestOverrides = fewestOverrides < 0 ? read.MaterialOverrideCount : Math.Min(fewestOverrides, read.MaterialOverrideCount);
                mostOverrides = Math.Max(mostOverrides, read.MaterialOverrideCount);
            }
            else
            {
                notDimmed++;

                if (read.MaterialOverrideCount > 0)
                {
                    undimmedCarrying++;
                }
            }

            if (read.ColoursAsked && read.ColoursRight == 2)
            {
                painted++;
            }

            outcome.AddCreated(planned.Path, planned.Pair.Folder, hidden.Count);
        }

        /// <summary>
        /// Reads what will have to be put back, once, before the first viewpoint changes
        /// the document: the hidden state, off a capture that never goes into the tree.
        /// The view is never touched, because the camera goes into the viewpoint directly
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
        /// The hidden state put back in its own try, released whether or not the restore
        /// worked, and read back and said.
        /// </summary>
        private void PutBack(Document document)
        {
            if (dimmedAnything)
            {
                try
                {
                    SavedViewpoints.Undim(document);
                    log.Line("VIEWS    the dimming was taken off the models this tool dimmed, so the document is back to its own appearance");
                }
                catch (Exception error)
                {
                    log.Failure(
                        "taking the dimming off after the viewpoints",
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
                        log.Line("VIEWS    hidden state put back: nothing was hidden before the viewpoints and nothing is hidden now");
                    }
                    else
                    {
                        log.Line("VIEWS    hidden state put back: " + count + " item(s) were hidden before the viewpoints and "
                            + (readsHidden ? "read as hidden again" : "DO NOT read as hidden again, said and not hidden"));
                    }
                }
                catch (Exception error)
                {
                    log.Failure(
                        "putting the hidden state back after the viewpoints",
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
    }
}
