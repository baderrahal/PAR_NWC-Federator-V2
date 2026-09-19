using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using CoreClashStatus = Federator.Core.Clash.ClashStatus;

namespace Federator.Addin.Engine
{
    /// <summary>What one clash should be moved to, and the record of why, F72c.</summary>
    public sealed class WantedStatus
    {
        public WantedStatus(string clashName, CoreClashStatus status)
            : this(clashName, status, null, false)
        {
        }

        public WantedStatus(string clashName, CoreClashStatus status, AutoReviewRecord record, bool asUndo)
        {
            ClashName = clashName;
            Status = status;
            Record = record;
            AsUndo = asUndo;
        }

        /// <summary>The clash's own name, matched Ordinal and never trimmed.</summary>
        public string ClashName { get; private set; }

        public CoreClashStatus Status { get; private set; }

        /// <summary>
        /// The record this move leaves in the NWF, F72c, or null for a move that leaves
        /// none. A move to Reviewed by either rule carries one. An undo carries the record
        /// it is undoing, so the guard can check the status asked for is the one it names.
        /// </summary>
        public AutoReviewRecord Record { get; private set; }

        /// <summary>
        /// Whether this is an undo, F72c, which may set the exact status its record names
        /// and nothing else, and writes no record of its own.
        /// </summary>
        public bool AsUndo { get; private set; }
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
    /// THE RECORD IS A COMMENT ON THE CLASH, F72c, written BEFORE the status and on the
    /// same handle, through DocumentClashTests.TestsEditResultComments, which scan.md 5h
    /// read off the DLL on 2026-09-19. It goes in the NWF and never in a side file, and
    /// where it cannot be written this says so in ONE line and sets the status alone.
    /// Nothing stands in for it.
    ///
    /// REVIEWED AND NOTHING ELSE. Federator.Core.Clash.StatusesThisToolMaySet decides, and
    /// a status it refuses is logged by name and not applied, so a caller that asks for
    /// Approved gets a line saying why rather than a silent no. An UNDO is the one other
    /// thing it allows, and only to the exact status one of this tool's own records names.
    ///
    /// WHERE THIS IS CALLED FROM MATTERS. ClashHarvest reads a result's status while
    /// building the report rows, immediately after the test runs and inside the same
    /// handle. So this runs BETWEEN the run and the harvest. Applying a status after the
    /// clash step would leave the workbook and the page carrying the status read before the
    /// change, which is the one thing the feature must not do.
    /// </summary>
    public sealed class ClashStatusEditor
    {
        /// <summary>The author on every record this tool writes.</summary>
        public const string Author = "Parsons NWC Federator";

        private readonly RunLog log;
        private bool saidTheWords;
        private bool cannotWriteRecords;

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

        /// <summary>How many records it wrote into the NWF, F72c.</summary>
        public int RecordsWritten { get; private set; }

        /// <summary>
        /// Applies the wanted statuses to the results of one test. Returns whether anything
        /// changed, which is what tells the caller the document was written to and the NWF
        /// needs saving again.
        ///
        /// Nothing wanted is the ordinary case today and costs one comparison, so this can
        /// sit in the per test path without slowing a run.
        /// </summary>
        public bool Apply(Document document, DocumentClashTests clashTests, ClashTest test, IList<WantedStatus> wanted)
        {
            if (clashTests == null || test == null || wanted == null || wanted.Count == 0)
            {
                return false;
            }

            SayTheWordsOnce();

            Dictionary<string, WantedStatus> byName = ByName(wanted);

            if (byName.Count == 0)
            {
                return false;
            }

            List<string> found = new List<string>();
            bool changed = Walk(document, clashTests, test.Children, byName, found);

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
        /// this tool and two set names in the reference file end in a space. An undo is
        /// judged by the other half of the same rule, AllowsAsUndo, F72c.
        /// </summary>
        private Dictionary<string, WantedStatus> ByName(IList<WantedStatus> wanted)
        {
            Dictionary<string, WantedStatus> byName =
                new Dictionary<string, WantedStatus>(StringComparer.Ordinal);

            for (int i = 0; i < wanted.Count; i++)
            {
                WantedStatus one = wanted[i];

                if (one == null || string.IsNullOrEmpty(one.ClashName))
                {
                    continue;
                }

                string why = one.AsUndo
                    ? StatusesThisToolMaySet.WhyNotAsUndo(one.Status, one.Record)
                    : StatusesThisToolMaySet.WhyNot(one.Status);

                if (why != null)
                {
                    RefusedCount++;
                    log.Line("STATUS   refused for " + one.ClashName + ": " + why);
                    continue;
                }

                if (!byName.ContainsKey(one.ClashName))
                {
                    byName.Add(one.ClashName, one);
                }
            }

            return byName;
        }

        /// <summary>
        /// Every result under this test, descending result groups. A group is one row in
        /// the panel holding several clashes, so the leaves are what carry a status.
        /// </summary>
        private bool Walk(
            Document document,
            DocumentClashTests clashTests,
            SavedItemCollection items,
            Dictionary<string, WantedStatus> byName,
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
                        changed |= Walk(document, clashTests, group.Children, byName, found);
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

                    WantedStatus wanted = byName[result.DisplayName];
                    CoreClashStatus want = wanted.Status;

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
                        // F72c. The record first, on the same handle, and then the status.
                        // An undo writes none: the record it is undoing stays as the history
                        // of what happened, and the status alone goes back.
                        if (wanted.Record != null && !wanted.AsUndo)
                        {
                            WriteTheRecord(document, clashTests, result, wanted.Record);
                        }

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
        /// The record, as a comment on the clash, F72c. The comments already there are
        /// copied, ours is added with a unique id off the document, and the collection is
        /// put back through the one measured member, which is a copy form like every other
        /// mutator on DocumentClashTests.
        ///
        /// WHERE IT CANNOT BE WRITTEN AT ALL, which is a throw before any record has landed,
        /// the log says so ONCE in Core's words, no further record is tried, and the status
        /// alone is set. A throw after records have landed is that clash's own and is
        /// logged by name, and the next clash is still tried.
        /// </summary>
        private void WriteTheRecord(
            Document document, DocumentClashTests clashTests, ClashResult result, AutoReviewRecord record)
        {
            if (cannotWriteRecords)
            {
                return;
            }

            try
            {
                using (CommentCollection comments = new CommentCollection(result.Comments))
                using (Comment comment = document == null
                    ? new Comment(record.Text(), CommentStatus.New, Author)
                    : document.CreateCommentWithUniqueId(record.Text(), CommentStatus.New, Author))
                {
                    comments.Add(comment);
                    clashTests.TestsEditResultComments(result, comments);
                }

                RecordsWritten++;
            }
            catch (Exception error)
            {
                if (RecordsWritten == 0)
                {
                    cannotWriteRecords = true;
                    log.Failure(
                        "writing the record on " + result.DisplayName,
                        error,
                        "the status alone is set from here on and no further record is tried");
                    log.Line(UndoAutoReview.CannotLine(
                        "writing one threw " + error.GetType().Name + " on the first clash"));
                    return;
                }

                log.Failure(
                    "writing the record on " + result.DisplayName,
                    error,
                    "kept going, the status is still set and the next clash is still tried");
            }
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
