using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using CoreClashStatus = Federator.Core.Clash.ClashStatus;

namespace Federator.Addin.Engine
{
    /// <summary>What one clash should be moved to.</summary>
    public sealed class WantedStatus
    {
        public WantedStatus(string clashName, CoreClashStatus status)
        {
            ClashName = clashName;
            Status = status;
        }

        /// <summary>The clash's own name, matched Ordinal and never trimmed.</summary>
        public string ClashName { get; private set; }

        public CoreClashStatus Status { get; private set; }
    }

    /// <summary>
    /// Moves named clashes to a status, through the one measured member that does it.
    ///
    /// WHAT IS SETTLED AND WHAT IS NOT. The member is measured:
    /// DocumentClashTests.TestsEditResultStatus(IClashResult result, ClashResultStatus
    /// status), docs\history\scan.md line 137, and ClashResultStatus is New 0, Active 1,
    /// Reviewed 2, Approved 3, Resolved 4, line 216. So applying a status is known work and
    /// this class does it.
    ///
    /// HOW THE TOOL LEARNS WHICH CLASHES CANNOT BE SOLVED IS Q33 AND IS NOT BUILT. Nothing
    /// calls this with a real list yet. It takes a list and applies it, which is the part
    /// that needs no answer, and everything above it waits. That is deliberate: building a
    /// guess at the input would mean writing a rule nobody agreed to into the only file
    /// that records what has been fixed.
    ///
    /// REVIEWED AND NOTHING ELSE. Federator.Core.Clash.StatusesThisToolMaySet decides, and
    /// a status it refuses is logged by name and not applied, so a caller that asks for
    /// Approved gets a line saying why rather than a silent no.
    ///
    /// WHERE THIS IS CALLED FROM MATTERS. ClashHarvest reads a result's status while
    /// building the report rows, immediately after the test runs and inside the same
    /// handle. So this runs BETWEEN the run and the harvest. Applying a status after the
    /// clash step would leave the workbook and the page carrying the status read before the
    /// change, which is the one thing the feature must not do.
    /// </summary>
    public sealed class ClashStatusEditor
    {
        private readonly RunLog log;
        private bool saidTheWords;

        public ClashStatusEditor(RunLog log)
        {
            this.log = log;
        }

        /// <summary>How many clashes this editor moved across the whole run.</summary>
        public int ChangedCount { get; private set; }

        /// <summary>How many it was asked for and could not find.</summary>
        public int NotFoundCount { get; private set; }

        /// <summary>How many it refused because the status is not one this tool sets.</summary>
        public int RefusedCount { get; private set; }

        /// <summary>
        /// Applies the wanted statuses to the results of one test. Returns whether anything
        /// changed, which is what tells the caller the document was written to and the NWF
        /// needs saving again.
        ///
        /// Nothing wanted is the ordinary case today and costs one comparison, so this can
        /// sit in the per test path while Q33 is open without slowing a run.
        /// </summary>
        public bool Apply(DocumentClashTests clashTests, ClashTest test, IList<WantedStatus> wanted)
        {
            if (clashTests == null || test == null || wanted == null || wanted.Count == 0)
            {
                return false;
            }

            SayTheWordsOnce();

            Dictionary<string, CoreClashStatus> byName = ByName(wanted);

            if (byName.Count == 0)
            {
                return false;
            }

            List<string> found = new List<string>();
            bool changed = Walk(clashTests, test.Children, byName, found);

            foreach (string name in byName.Keys)
            {
                if (!found.Contains(name))
                {
                    NotFoundCount++;
                    log.Line("STATUS   not found in " + test.DisplayName + ": " + name);
                }
            }

            return changed;
        }

        /// <summary>
        /// The wanted list keyed by clash name, with anything this tool may not set logged
        /// and dropped. Ordinal, because a clash name is matched exactly everywhere else in
        /// this tool and two set names in the reference file end in a space.
        /// </summary>
        private Dictionary<string, CoreClashStatus> ByName(IList<WantedStatus> wanted)
        {
            Dictionary<string, CoreClashStatus> byName =
                new Dictionary<string, CoreClashStatus>(StringComparer.Ordinal);

            for (int i = 0; i < wanted.Count; i++)
            {
                WantedStatus one = wanted[i];

                if (one == null || string.IsNullOrEmpty(one.ClashName))
                {
                    continue;
                }

                string why = StatusesThisToolMaySet.WhyNot(one.Status);

                if (why != null)
                {
                    RefusedCount++;
                    log.Line("STATUS   refused for " + one.ClashName + ": " + why);
                    continue;
                }

                if (!byName.ContainsKey(one.ClashName))
                {
                    byName.Add(one.ClashName, one.Status);
                }
            }

            return byName;
        }

        /// <summary>
        /// Every result under this test, descending result groups. A group is one row in
        /// the panel holding several clashes, so the leaves are what carry a status.
        /// </summary>
        private bool Walk(
            DocumentClashTests clashTests,
            SavedItemCollection items,
            Dictionary<string, CoreClashStatus> byName,
            List<string> found)
        {
            if (items == null)
            {
                return false;
            }

            bool changed = false;

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    ClashResultGroup group = item as ClashResultGroup;

                    if (group != null)
                    {
                        changed |= Walk(clashTests, group.Children, byName, found);
                        continue;
                    }

                    ClashResult result = item as ClashResult;

                    if (result == null || string.IsNullOrEmpty(result.DisplayName))
                    {
                        continue;
                    }

                    if (!byName.ContainsKey(result.DisplayName))
                    {
                        continue;
                    }

                    CoreClashStatus want = byName[result.DisplayName];

                    if (!found.Contains(result.DisplayName))
                    {
                        found.Add(result.DisplayName);
                    }

                    CoreClashStatus already = (CoreClashStatus)(int)result.Status;

                    if (already == want)
                    {
                        // Already there is not a change, and reporting it as one would make
                        // a run that did nothing ask for the NWF to be saved again.
                        log.Line("STATUS   " + result.DisplayName + " is already " + want);
                        continue;
                    }

                    try
                    {
                        clashTests.TestsEditResultStatus(result, (ClashResultStatus)(int)want);
                        ChangedCount++;
                        changed = true;
                        log.Line("STATUS   " + result.DisplayName + "  " + already + " to " + want);
                    }
                    catch (Exception error)
                    {
                        log.Failure(
                            "moving the clash " + result.DisplayName + " to " + want,
                            error,
                            "kept going, the other clashes of this test are still tried");
                    }
                }
            }

            return changed;
        }

        /// <summary>
        /// The five and the four, written once per run the first time a status is touched.
        /// The wording is Core so the log and docs\workflow.md cannot drift apart.
        /// </summary>
        private void SayTheWordsOnce()
        {
            if (saidTheWords)
            {
                return;
            }

            saidTheWords = true;

            foreach (string line in StatusWords.Lines())
            {
                log.Line(line);
            }
        }
    }
}
