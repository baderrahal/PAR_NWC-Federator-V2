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
    /// service size, and keeps a COPY of the camera Clash Detective frames it with. The
    /// plan then runs over all of them at once, because a cap per test and the counts in
    /// the block are about the whole group. The second writes what the plan kept. Nothing
    /// borrowed from the document is held across either.
    ///
    /// WHAT A VIEWPOINT SHOWS. The two disciplines of the pair, every model of each, and
    /// every other model hidden, framed on the clash the way the picture of it is framed,
    /// TestsViewpointForResult. That is what a person pressing AR vs ST expects to see,
    /// the two things that clashed in their own context, and the hiding is what 5j
    /// measured a captured viewpoint to keep. A pair with a code this tool does not know
    /// hides nothing and the plan counts it.
    ///
    /// EVERYTHING IS PUT BACK. The view the document had and the hidden state the file
    /// held are restored when the group's writing ends, whichever way it ends.
    /// </summary>
    public sealed class ViewpointBuilder
    {
        private readonly Action<string> progress;
        private readonly RunLog log;
        private readonly PenetrationSettings penetrations;
        private readonly SizeSettings sizes;
        private readonly ViewpointSettings views;

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
            Viewpoint viewBefore = null;

            try
            {
                Collect(document, clashTests, report, clashes, cameras);
                Plan = ClashViewpointPlan.For(clashes, views, priorityPicked);

                if (Plan.Planned.Count == 0)
                {
                    return outcome;
                }

                viewBefore = document.CurrentViewpoint.CreateCopy();

                foreach (PlannedClashViewpoint planned in Plan.Planned)
                {
                    try
                    {
                        progress("Viewpoint " + planned.Path);
                        WriteOne(document, planned, cameras, modelDisciplines ?? new Dictionary<int, string>(), outcome);
                    }
                    catch (Exception error)
                    {
                        // The type as well as the text, because a member of this API
                        // behaving differently from the way 5d and 5j measured it is the
                        // most likely failure, and the type name is what says which.
                        outcome.AddFailed(planned.Path, planned.Pair.Folder, error.GetType().Name + ": " + error.Message);
                        log.Failure(
                            "putting the viewpoint " + planned.Path + " into the NWF",
                            error,
                            "kept going, the other viewpoints are still tried and the block carries the total");
                    }
                }
            }
            finally
            {
                foreach (Viewpoint camera in cameras.Values)
                {
                    camera.Dispose();
                }

                try
                {
                    SavedViewpoints.RestoreHiddenState(document);

                    if (viewBefore != null)
                    {
                        document.CurrentViewpoint.CopyFrom(viewBefore);
                        viewBefore.Dispose();
                    }
                }
                catch (Exception error)
                {
                    log.Failure(
                        "putting the view and the hidden state back after the viewpoints",
                        error,
                        "kept going, the NWF saved next carries whatever state the document is in");
                }
            }

            return outcome;
        }

        /// <summary>
        /// Every clash of every test the report holds rows for, read into what the plan
        /// needs, with a copy of its camera kept by the name the plan will give it.
        /// </summary>
        private void Collect(
            Document document,
            DocumentClashTests clashTests,
            ClashReport report,
            List<ClashToPlan> clashes,
            Dictionary<string, Viewpoint> cameras)
        {
            string unitEnumName = Penetrations.UnitEnumName(document);

            foreach (TestReport test in report.Tests)
            {
                if (!test.HasRows)
                {
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
                        CollectResults(found.Children, clashTests, test, leftSet, rightSet, unitEnumName, clashes, cameras);
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
        }

        private void CollectResults(
            SavedItemCollection items,
            DocumentClashTests clashTests,
            TestReport test,
            string leftSet,
            string rightSet,
            string unitEnumName,
            List<ClashToPlan> clashes,
            Dictionary<string, Viewpoint> cameras)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    ClashResultGroup group = item as ClashResultGroup;

                    if (group != null)
                    {
                        CollectResults(group.Children, clashTests, test, leftSet, rightSet, unitEnumName, clashes, cameras);
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
                }
            }
        }

        private void WriteOne(
            Document document,
            PlannedClashViewpoint planned,
            Dictionary<string, Viewpoint> cameras,
            IDictionary<int, string> modelDisciplines,
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

            int hiddenDisciplines = 0;

            if (planned.Pair.BothKnown)
            {
                HashSet<string> shown = new HashSet<string>(StringComparer.Ordinal);
                shown.Add(planned.Pair.First);
                shown.Add(planned.Pair.Second);

                List<int> keep = new List<int>();
                HashSet<string> hidden = new HashSet<string>(StringComparer.Ordinal);

                foreach (KeyValuePair<int, string> model in modelDisciplines)
                {
                    if (shown.Contains(model.Value))
                    {
                        keep.Add(model.Key);
                    }
                    else
                    {
                        hidden.Add(model.Value);
                    }
                }

                SavedViewpoints.ShowOnlyModels(document, keep);
                hiddenDisciplines = hidden.Count;
            }
            else
            {
                // A code this tool does not know. Nothing is hidden, because hiding on a
                // guess would hide the thing the person is looking for, and the plan has
                // already counted it under UNKNOWN.
                document.Models.ResetAllHidden();
            }

            document.CurrentViewpoint.CopyFrom(camera);
            SavedViewpoints.EnsureFolders(document, planned.Folders);
            SavedViewpoints.Capture(document, planned.Folders, planned.Name);

            // Read back rather than trusted. AddCopy returns void everywhere in this API,
            // so the only way to know the viewpoint is there is to look for it.
            if (!SavedViewpoints.Exists(document, planned.Folders, planned.Name))
            {
                outcome.AddFailed(planned.Path, planned.Pair.Folder, "it was added and a fresh read does not show it");
                return;
            }

            outcome.AddCreated(planned.Path, planned.Pair.Folder, hiddenDisciplines);
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
