using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Federator.Core.Diagnostics;
using Federator.Core.Views;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Puts one folder per discipline into the NWF with one viewpoint in each, showing that
    /// discipline and hiding the others.
    ///
    /// READ THIS BEFORE CHANGING ANYTHING HERE.
    ///
    /// Every Navisworks call in this file rests on an API that was NEVER MEASURED. The repo
    /// has not touched DocumentSavedViewpoints and nothing about it is in
    /// docs\history\scan.md, which records the question in section 5b and says so in its
    /// heading. tools\probes\probe-viewpoints.ps1 answers it and it is step 197 of
    /// steps\03_bader_next.md.
    ///
    /// FIVE THINGS ARE ASSUMED HERE AND NOT ONE OF THEM WAS READ OFF A DLL:
    ///
    ///   1. Document.SavedViewpoints is a DocumentSavedViewpoints
    ///   2. it carries RootItem and AddCopy(GroupItem parent, SavedItem item), which is the
    ///      shape DocumentSelectionSets was MEASURED to have on 2026-08-31, section 4d
    ///   3. a FolderItem can be added into it the way one is added into the sets tree
    ///   4. a SavedViewpoint can be made from the current view and given a DisplayName
    ///   5. hiding is done through the models collection and survives into the viewpoint
    ///
    /// The shape is taken from the sets tree because that is the closest thing this tool
    /// already builds and its shape IS measured. It is taken as a starting point and not as
    /// a fact, which is the difference this file exists to hold.
    ///
    /// WHAT HAPPENS IF AN ASSUMPTION IS WRONG. The group fails, loudly, naming the
    /// viewpoint and what threw, and the NWF is not saved over on account of the
    /// viewpoints. It never half creates one and it never creates a viewpoint that failed
    /// to hide the other disciplines, because a viewpoint showing everything is not the
    /// thing that was asked for and would read as a working feature.
    /// </summary>
    public sealed class ViewpointBuilder
    {
        private readonly Action<string> progress;
        private readonly RunLog log;
        private readonly SizeSettings sizes;

        public ViewpointBuilder(Action<string> progress, RunLog log, SizeSettings sizes)
        {
            this.progress = progress;
            this.log = log;
            this.sizes = sizes ?? new SizeSettings();
        }

        /// <summary>
        /// Every planned viewpoint, one at a time. One that throws is recorded and the rest
        /// are still tried, because a folder that could not be made for one discipline says
        /// nothing about the next.
        /// </summary>
        public ViewpointBuildOutcome Build(Document document, IList<PlannedViewpoint> planned)
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();

            if (document == null || planned == null)
            {
                return outcome;
            }

            for (int i = 0; i < planned.Count; i++)
            {
                PlannedViewpoint want = planned[i];

                try
                {
                    progress("Viewpoint " + want.Path);
                    BuildOne(document, want, outcome);
                }
                catch (Exception error)
                {
                    // The message carries the type as well as the text, because the most
                    // likely failure in this file is one of the five assumptions above
                    // being wrong, and the type name is what says which.
                    outcome.AddFailed(
                        want.Path, want.Shows,
                        error.GetType().Name + ": " + error.Message);

                    log.Failure(
                        "putting the viewpoint " + want.Path + " into the NWF",
                        error,
                        "kept going, the other disciplines are still tried and the VIEWS block carries the total");
                }
            }

            return outcome;
        }

        private void BuildOne(Document document, PlannedViewpoint want, ViewpointBuildOutcome outcome)
        {
            // Already there is left exactly as it is. F28's rule, carried to viewpoints: a
            // second copy at one path leaves the tree holding both and whichever came first
            // is what anything resolving that path finds.
            if (SavedViewpoints.Exists(document, want.Folder, want.Name))
            {
                outcome.AddAlreadyPresent(want.Path, want.Shows, want.Hides.Count);
                log.Line(outcome.Results[outcome.Results.Count - 1].Line());
                return;
            }

            // F53. A sub group viewpoint holds only the items over the size threshold.
            // ItemSizes reads the properties and Federator.Core.Views.SizeRule decides, so
            // the 150 and the six property names are testable without Navisworks and this
            // file has no opinion about either.
            if (want.LargeItemsOnly)
            {
                SavedViewpoints.ShowOnlyLargeItems(document, want.Shows, want.Hides, sizes, log);
            }
            else
            {
                SavedViewpoints.ShowOnly(document, want.Shows, want.Hides);
            }

            SavedViewpoints.Add(document, want.Folder, want.Name);

            // Read back rather than trusted. AddCopy returns void everywhere else in this
            // API, so the only way to know the viewpoint is there is to look for it, which
            // is exactly what SetBuilder learned to do with a folder.
            if (!SavedViewpoints.Exists(document, want.Folder, want.Name))
            {
                outcome.AddFailed(
                    want.Path, want.Shows,
                    "it was added and a fresh read does not show it");
            }
            else
            {
                outcome.AddCreated(want.Path, want.Shows, want.Hides.Count);
            }

            log.Line(outcome.Results[outcome.Results.Count - 1].Line());
        }
    }
}
