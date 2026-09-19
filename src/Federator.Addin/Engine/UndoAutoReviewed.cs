using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using CoreClashStatus = Federator.Core.Clash.ClashStatus;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The Undo auto Reviewed button, F72c. Reads every clash of every test in the open
    /// document, asks Core which ones carry a record of this tool moving them AND are
    /// still at Reviewed, and puts only those back, each to the status its record names,
    /// through the one ClashStatusEditor like everything else.
    ///
    /// A clash this tool set to Reviewed in week one that a person moved to Approved in
    /// week two still carries the record and is LEFT ALONE, because that person's decision
    /// is the one thing this tool never overwrites. Federator.Core.Clash.UndoAutoReview is
    /// the rule and UndoTally is the block.
    ///
    /// NOTHING IS SAVED. This changes the open document the way the two hand buttons do,
    /// and the person saves the NWF in Navisworks, or closes it and keeps what was there.
    /// </summary>
    public sealed class UndoAutoReviewed
    {
        private readonly RunLog log;
        private readonly ClashStatusEditor statuses;

        public UndoAutoReviewed(RunLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
            this.statuses = new ClashStatusEditor(log);
        }

        /// <summary>What the pass did, for the block and the line in the window.</summary>
        public UndoTally Tally { get; private set; }

        /// <summary>Whether the document was written to, so the person knows to save it.</summary>
        public bool ChangedTheDocument { get; private set; }

        /// <summary>
        /// The whole pass over the open document. Never throws past a test: a test whose
        /// results will not read is logged and the next is still tried.
        /// </summary>
        public void Run(Document document)
        {
            Tally = new UndoTally();
            ChangedTheDocument = false;

            if (document == null)
            {
                log.Line(UndoAutoReview.Prefix + " nothing is open, so there is nothing to put back");
                return;
            }

            DocumentClashTests clashTests = document.GetClash().TestsData;
            WalkTests(document, clashTests, clashTests.Tests);
            log.Block(UndoAutoReview.Prefix + " " + Words.Or(document.FileName, "the open document"), Tally.Lines());
        }

        /// <summary>Every test in the document, descending folders, the way the census walks.</summary>
        private void WalkTests(Document document, DocumentClashTests clashTests, SavedItemCollection items)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    ClashTest test = item as ClashTest;

                    if (test != null)
                    {
                        OneTest(document, clashTests, test);
                        continue;
                    }

                    GroupItem folder = item as GroupItem;

                    if (folder != null)
                    {
                        WalkTests(document, clashTests, folder.Children);
                    }
                }
            }
        }

        /// <summary>
        /// One test: a read pass that judges every clash and fills the tally, then the
        /// editor applies the ones to put back. The read pass writes nothing, so the handle
        /// is still the test's when the editor takes it.
        /// </summary>
        private void OneTest(Document document, DocumentClashTests clashTests, ClashTest test)
        {
            string testName = Words.Or(test.DisplayName, "UNKNOWN test");
            List<WantedStatus> wanted = new List<WantedStatus>();

            try
            {
                Judge(test.Children, testName, wanted);
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading the clashes in " + testName + " for the undo",
                    error,
                    "kept going, every clash this test holds is left exactly as it was");
                return;
            }

            if (wanted.Count == 0)
            {
                return;
            }

            try
            {
                if (statuses.Apply(document, clashTests, test, wanted))
                {
                    ChangedTheDocument = true;
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "putting back the clashes in " + testName,
                    error,
                    "kept going with the next test");
            }
        }

        private void Judge(SavedItemCollection items, string testName, List<WantedStatus> wanted)
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
                        Judge(group.Children, testName, wanted);
                        continue;
                    }

                    ClashResult result = item as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    string clashName = Words.Or(result.DisplayName, string.Empty);
                    CoreClashStatus now = (CoreClashStatus)(int)result.Status;
                    string comment = OurComment(result);
                    CoreClashStatus putBackTo;
                    UndoVerdict verdict = UndoAutoReview.Judge(comment, now, out putBackTo);

                    Tally.Add(testName, clashName.Length == 0 ? "an unnamed clash" : clashName, verdict, putBackTo);

                    if (verdict == UndoVerdict.PutBack && clashName.Length > 0)
                    {
                        wanted.Add(new WantedStatus(clashName, putBackTo, AutoReviewRecord.In(comment), true));
                    }
                }
            }
        }

        /// <summary>
        /// The LAST comment on the clash that is one of ours, or an empty string. A
        /// comment a person wrote is never one of ours, because it does not carry the
        /// marker, and the last of ours is the most recent move.
        /// </summary>
        private static string OurComment(ClashResult result)
        {
            string ours = string.Empty;
            CommentCollection comments = result.Comments;

            if (comments == null)
            {
                return ours;
            }

            for (int i = 0; i < comments.Count; i++)
            {
                string body = comments[i].Body;

                if (AutoReviewRecord.In(body) != null)
                {
                    ours = body;
                }
            }

            return ours;
        }
    }
}
