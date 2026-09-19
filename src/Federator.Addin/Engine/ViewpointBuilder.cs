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
    /// FOUR OF THE FIVE THINGS THIS FILE ONCE ASSUMED ARE MEASURED NOW.
    /// tools\probes\probe-viewpoints.ps1 was run on 2026-09-19 against the installed
    /// Autodesk.Navisworks.Api 22.0.0.0 and every line of it is in docs\history\scan.md 5d:
    ///
    ///   1. Document.SavedViewpoints is a DocumentSavedViewpoints            MEASURED
    ///   2. it carries RootItem, a FolderItem, and
    ///      AddCopy(GroupItem parent, SavedItem item)                        MEASURED
    ///   3. FolderItem has a public constructor and goes in through AddCopy,
    ///      and EditDisplayName(SavedItem, string) sets a name                MEASURED
    ///   4. new SavedViewpoint(Viewpoint) makes one, and both it and
    ///      Viewpoint are IDisposable                                        MEASURED
    ///   5. hiding is DocumentModels.SetHidden, and whether a viewpoint
    ///      RECORDS that hiding is                                           UNKNOWN
    ///
    /// The fifth is the one this feature turns on, it cannot be read off the DLL, and
    /// SavedViewpoint.ContainsVisibilityOverrides is what answers it on a real run.
    ///
    /// The two collections turned out to have the SAME shape, which is what section 5b said
    /// must not be assumed from the pattern. It was not assumed. It was read.
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
