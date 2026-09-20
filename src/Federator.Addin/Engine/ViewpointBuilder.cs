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
        private Exception firstHomeError;

        // Where the VIEWS step's seconds go, per call, because the dimming took one
        // group from 7.5 seconds to 476 and the shape of the cost was not what it
        // looked like: the group with the MOST items was one of the fastest. A step
        // that got slower says which call did it rather than leaving it to be guessed.
        private readonly System.Diagnostics.Stopwatch alreadyThereWatch = new System.Diagnostics.Stopwatch();
        private readonly System.Diagnostics.Stopwatch dimWatch = new System.Diagnostics.Stopwatch();
        private readonly System.Diagnostics.Stopwatch recordWatch = new System.Diagnostics.Stopwatch();
        private readonly System.Diagnostics.Stopwatch readBackWatch = new System.Diagnostics.Stopwatch();

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
            bool priorityPicked,
            IDictionary<int, string> modelDisciplines)
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();

            if (document == null || report == null)
            {
                return outcome;
            }

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
            firstHomeError = null;
            alreadyThereWatch.Reset();
            dimWatch.Reset();
            recordWatch.Reset();
            readBackWatch.Reset();

            try
            {
                Collect(document, clashTests, report, clashes, cameras, places, ModelIndexByFile(document));
                Plan = ClashViewpointPlan.For(clashes, views, priorityPicked);

                if (Plan.Planned.Count == 0)
                {
                    return outcome;
                }

                foreach (PlannedClashViewpoint planned in Plan.Planned)
                {
                    try
                    {
                        progress("Viewpoint " + planned.Path);
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

                    log.Line("VIEWS    the step's seconds went: "
                        + Seconds(alreadyThereWatch) + " looking whether each was already there, "
                        + Seconds(dimWatch) + " dimming, "
                        + Seconds(recordWatch) + " recording, "
                        + Seconds(readBackWatch) + " reading back");

                    if (views.DimsAnything)
                    {
                        log.Line("VIEWS    dimmed to "
                            + views.DimTransparency.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)
                            + " with the two clashing items left solid: " + dimmed + " viewpoint(s)"
                            + (notDimmed > 0 ? ", and " + notDimmed + " written undimmed because their two items could not both be pointed at" : string.Empty));
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

                PutBack(document);
            }

            return outcome;
        }

        /// <summary>
        /// Every clash of every test the report holds rows for, read into what the plan
        /// needs, with a copy of its camera and the models its items live in kept by the
        /// name the plan will give it. A test name the report carries twice is read once,
        /// because both rows would resolve to the same document test and every clash of
        /// it would be handed to the plan twice.
        /// </summary>
        private void Collect(
            Document document,
            DocumentClashTests clashTests,
            ClashReport report,
            List<ClashToPlan> clashes,
            Dictionary<string, Viewpoint> cameras,
            Dictionary<string, ClashPlace> places,
            IDictionary<string, int> indexByFile)
        {
            string unitEnumName = Penetrations.UnitEnumName(document);
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            int homesUnread = 0;

            foreach (TestReport test in report.Tests)
            {
                if (!test.HasRows)
                {
                    continue;
                }

                if (!seen.Add(test.Name))
                {
                    log.Line("VIEWS    " + test.Name + " is on the report twice, so its clashes are read once and planned once");
                    continue;
                }

                string leftSet = ByDesignRule.SetNameIn(test.LeftLocator);
                string rightSet = ByDesignRule.SetNameIn(test.RightLocator);

                using (ClashTest found = FindTest(clashTests.Tests, test.Name))
                {
                    if (found == null)
                    {
                        log.Line("VIEWS    " + test.Name + " is on the report and not in the document, so its clashes get no viewpoint");
                        continue;
                    }

                    try
                    {
                        homesUnread += CollectResults(
                            document,
                            found.Children, clashTests, test, leftSet, rightSet, unitEnumName, clashes, cameras, places, indexByFile);
                    }
                    catch (Exception error)
                    {
                        log.Failure(
                            "reading the clashes of " + test.Name + " for the viewpoints",
                            error,
                            "kept going, the clashes read before it threw are planned and the rest are not");
                    }
                }
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

        private int CollectResults(
            Document document,
            SavedItemCollection items,
            DocumentClashTests clashTests,
            TestReport test,
            string leftSet,
            string rightSet,
            string unitEnumName,
            List<ClashToPlan> clashes,
            Dictionary<string, Viewpoint> cameras,
            Dictionary<string, ClashPlace> places,
            IDictionary<string, int> indexByFile)
        {
            if (items == null)
            {
                return 0;
            }

            int homesUnread = 0;

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    ClashResultGroup group = item as ClashResultGroup;

                    if (group != null)
                    {
                        homesUnread += CollectResults(
                            document,
                            group.Children, clashTests, test, leftSet, rightSet, unitEnumName, clashes, cameras, places, indexByFile);
                        continue;
                    }

                    ClashResult result = item as ClashResult;

                    if (result == null || string.IsNullOrEmpty(result.DisplayName))
                    {
                        continue;
                    }

                    CoreClashStatus status = (CoreClashStatus)(int)result.Status;

                    // The size is read only where the plan will ask about it, which is a
                    // clash it would otherwise keep. A closed clash is left out on its
                    // status before the size is looked at, so its items are not read.
                    SizeVerdict? serviceSize = ClashViewpointPlan.InScope(status)
                        ? Penetrations.ServiceSizeOf(result, penetrations, sizes, unitEnumName)
                        : null;

                    ClashToPlan clash = new ClashToPlan(
                        test.Name, result.DisplayName, leftSet, rightSet, status, test.Priority, serviceSize);
                    clashes.Add(clash);

                    if (!ClashViewpointPlan.InScope(status))
                    {
                        continue;
                    }

                    string key = ClashViewpointPlan.NameFor(clash, views);

                    if (cameras.ContainsKey(key))
                    {
                        continue;
                    }

                    using (Viewpoint framed = clashTests.TestsViewpointForResult(result))
                    {
                        if (framed != null)
                        {
                            cameras.Add(key, framed.CreateCopy());
                        }
                    }

                    ClashPlace place = new ClashPlace();

                    try
                    {
                        place.FirstPath = ReadPlace(document, result.Item1, indexByFile, place.Models);
                        place.SecondPath = ReadPlace(document, result.Item2, indexByFile, place.Models);
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

                    places[key] = place;
                }
            }

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
            alreadyThereWatch.Start();
            bool alreadyThere = SavedViewpoints.Exists(document, planned.Folders, planned.Name);
            alreadyThereWatch.Stop();

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

            Touch(document);

            if (hidesNothingBecause == null)
            {
                SavedViewpoints.ShowOnlyModels(document, keep);
            }
            else
            {
                SayOnce("VIEWS    " + planned.Pair.Folder + ": " + hidesNothingBecause + ", so its viewpoints hide nothing");
                hidden.Clear();
                document.Models.ResetAllHidden();
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

            if (views.DimsAnything && place != null && place.BothPlaced)
            {
                dimWatch.Start();

                using (ModelItem firstItem = SavedViewpoints.ItemAt(document, place.FirstPath))
                using (ModelItem secondItem = SavedViewpoints.ItemAt(document, place.SecondPath))
                {
                    if (firstItem != null && secondItem != null)
                    {
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
                    solid = 0;
                    paintedThisOne = false;
                }

                dimWatch.Stop();
            }

            recordWatch.Start();
            SavedViewpoints.EnsureFolders(document, planned.Folders);
            SavedViewpoints.Record(document, planned.Folders, planned.Name, camera);
            recordWatch.Stop();

            // Read back rather than trusted, all three of it. The first run's tree looked
            // complete and every viewpoint opened on sky, because the route it used
            // recorded no camera, 5l, and nothing read the camera back. A viewpoint whose
            // recorded camera is not the clash camera, or which carries no overrides
            // while it was meant to hide something, is not a viewpoint of that clash and
            // is counted as failed with the reason a person can check.
            readBackWatch.Start();
            ViewpointReadBack read = paintedThisOne
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
            readBackWatch.Stop();

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
            }
            else
            {
                notDimmed++;
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

        /// <summary>One watch's total, in seconds, the way every other timing in the log reads.</summary>
        private static string Seconds(System.Diagnostics.Stopwatch watch)
        {
            return (watch.ElapsedMilliseconds / 1000.0).ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "s";
        }

        private void SayOnce(string line)
        {
            if (saidOnce.Add(line))
            {
                log.Line(line);
            }
        }

        /// <summary>
        /// The test of that name, wherever it sits, descending folders. The caller disposes
        /// what comes back, and every other wrapper is released on the way.
        /// </summary>
        private static ClashTest FindTest(SavedItemCollection items, string name)
        {
            if (items == null)
            {
                return null;
            }

            for (int i = 0; i < items.Count; i++)
            {
                SavedItem item = items[i];
                ClashTest test = item as ClashTest;

                if (test != null)
                {
                    if (string.Equals(test.DisplayName, name, StringComparison.Ordinal))
                    {
                        return test;
                    }

                    test.Dispose();
                    continue;
                }

                GroupItem folder = item as GroupItem;

                if (folder != null)
                {
                    ClashTest below = FindTest(folder.Children, name);
                    folder.Dispose();

                    if (below != null)
                    {
                        return below;
                    }

                    continue;
                }

                item.Dispose();
            }

            return null;
        }
    }
}
