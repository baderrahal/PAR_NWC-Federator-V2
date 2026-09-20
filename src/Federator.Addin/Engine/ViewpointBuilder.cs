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
    /// file code are not the same list: a DR set lives in the AR model, and a viewpoint
    /// that hid AR for DR vs ST would hide the door the person is looking for. A pair
    /// with a code this tool does not know hides nothing, a model whose name will not
    /// parse is never hidden, and a pair no model carries hides nothing and says so.
    ///
    /// EVERYTHING IS PUT BACK, MEASURED. Before the first viewpoint changes anything the
    /// hidden state the document holds is read off a runtime capture that never goes
    /// into the tree, and the view is copied. When the group's writing ends, whichever
    /// way it ends, everything is shown, exactly the items that were hidden are hidden
    /// again and read back as hidden, and the view is put back, each in its own try so
    /// one failing cannot skip the other. scan.md 5k measured this route and measured
    /// that ResetAllHiddenToModelState, which this once called, LOSES a hide the
    /// document held. A group that hid nothing, because nothing was planned or every
    /// viewpoint was already there, touches none of it.
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
            Dictionary<string, HashSet<int>> homes = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);
            IDictionary<int, string> disciplines = modelDisciplines ?? new Dictionary<int, string>();
            snapshot = null;
            cameraRead = 0;

            try
            {
                Collect(document, clashTests, report, clashes, cameras, homes, ModelIndexByFile(document));
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
                        WriteOne(document, planned, cameras, homes, disciplines, outcome);
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
                        + " units of its clash camera and each that hides a discipline carries visibility overrides");
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
            Dictionary<string, HashSet<int>> homes,
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
                            found.Children, clashTests, test, leftSet, rightSet, unitEnumName, clashes, cameras, homes, indexByFile);
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
            }

            int withHome = 0;

            foreach (HashSet<int> home in homes.Values)
            {
                if (home.Count > 0)
                {
                    withHome++;
                }
            }

            log.Line("VIEWS    the model each clash item lives in was read for " + withHome + " of " + homes.Count
                + " clash(es) in scope" + (withHome == homes.Count ? string.Empty : ", and the rest keep the pair's models only"));
        }

        private int CollectResults(
            SavedItemCollection items,
            DocumentClashTests clashTests,
            TestReport test,
            string leftSet,
            string rightSet,
            string unitEnumName,
            List<ClashToPlan> clashes,
            Dictionary<string, Viewpoint> cameras,
            Dictionary<string, HashSet<int>> homes,
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
                            group.Children, clashTests, test, leftSet, rightSet, unitEnumName, clashes, cameras, homes, indexByFile);
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

                    HashSet<int> home = new HashSet<int>();

                    try
                    {
                        AddHome(result.Item1, indexByFile, home);
                        AddHome(result.Item2, indexByFile, home);
                    }
                    catch (Exception)
                    {
                        // Counted and said once per group. The viewpoint still keeps the
                        // pair's models, it just cannot also keep a model the code did
                        // not name, and a clash whose items will not read is not lost.
                        homesUnread++;
                    }

                    homes[key] = home;
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
        private static void AddHome(ModelItem item, IDictionary<string, int> indexByFile, HashSet<int> into)
        {
            using (item)
            {
                if (item == null)
                {
                    return;
                }

                foreach (ModelItem ancestor in item.AncestorsAndSelf)
                {
                    using (ancestor)
                    {
                        if (!ancestor.HasModel)
                        {
                            continue;
                        }

                        using (Model model = ancestor.Model)
                        {
                            int index;

                            if (model != null && indexByFile.TryGetValue(Words.Or(model.FileName, string.Empty), out index))
                            {
                                into.Add(index);
                            }
                        }

                        return;
                    }
                }
            }
        }

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
            Dictionary<string, HashSet<int>> homes,
            IDictionary<int, string> disciplines,
            ViewpointBuildOutcome outcome)
        {
            // Already there is left exactly as it is. F28's rule, carried to viewpoints: a
            // second copy at one path leaves the tree holding both and whichever came first
            // is what anything resolving that path finds.
            if (SavedViewpoints.Exists(document, planned.Folders, planned.Name))
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

            if (planned.Pair.BothKnown)
            {
                HashSet<int> home;
                homes.TryGetValue(planned.Name, out home);
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

            SavedViewpoints.EnsureFolders(document, planned.Folders);
            SavedViewpoints.Record(document, planned.Folders, planned.Name, camera);

            // Read back rather than trusted, all three of it. The first run's tree looked
            // complete and every viewpoint opened on sky, because the route it used
            // recorded no camera, 5l, and nothing read the camera back. A viewpoint whose
            // recorded camera is not the clash camera, or which carries no overrides
            // while it was meant to hide something, is not a viewpoint of that clash and
            // is counted as failed with the reason a person can check.
            ViewpointReadBack read = SavedViewpoints.ReadBack(document, planned.Folders, planned.Name, camera);

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

            if (hidden.Count > 0 && !read.ContainsVisibilityOverrides)
            {
                outcome.AddFailed(planned.Path, planned.Pair.Folder, "it was added without its hidden state, so it would show every discipline");
                return;
            }

            cameraRead++;
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
