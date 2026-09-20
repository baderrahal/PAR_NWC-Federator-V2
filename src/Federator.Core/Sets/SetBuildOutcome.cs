using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Sets
{
    /// <summary>
    /// The running total for a sets build. The counts always agree with the lines,
    /// because both come from the same list.
    /// </summary>
    public sealed class SetBuildOutcome
    {
        private readonly List<SetResult> results = new List<SetResult>();
        private readonly List<SkippedSet> skipped = new List<SkippedSet>();
        private readonly List<SetDrift> drifted = new List<SetDrift>();
        private readonly List<EmptySet> empty = new List<EmptySet>();
        private int rebuiltCount;

        public ReadOnlyCollection<SetResult> Results
        {
            get { return new ReadOnlyCollection<SetResult>(results); }
        }

        internal ReadOnlyCollection<SkippedSet> Skipped
        {
            get { return new ReadOnlyCollection<SkippedSet>(skipped); }
        }

        /// <summary>
        /// The document the sets were resolved against. Counts cannot be read without it,
        /// because a set at zero against an architecture model means something different
        /// from a set at zero against a federated one.
        /// </summary>
        public string OpenDocument { get; set; }

        /// <summary>
        /// A set already at this path in the open document. On a reused NWF every set from
        /// last week is already there, and adding another copy would leave the tree holding
        /// both, with a locator resolving to whichever came first. It is left alone. One
        /// call, carrying the path, the name, the condition count and how many items it
        /// finds in this document, so it has a line of its own and is never counted as
        /// created. F28.
        /// </summary>
        public SetResult AddAlreadyPresent(string path, string name, int conditionCount, int itemCount)
        {
            SetResult result = new SetResult(path, name, conditionCount, itemCount, null, null, true);
            results.Add(result);
            return result;
        }

        /// <summary>
        /// Every set whose question in the document differs from what the picked file
        /// asks, Q72, and whether this run rebuilt it. Kept apart from the results list
        /// because a drifted set is still a present set and is counted as one.
        /// </summary>
        public void AddDrift(SetDrift drift, bool rebuilt)
        {
            if (drift == null)
            {
                return;
            }

            drifted.Add(drift);

            if (rebuilt)
            {
                rebuiltCount++;
            }
        }

        /// <summary>
        /// Every set that found NOTHING, with which of three things is wrong with it, 3b.
        /// A set finding zero is not one dead set, it is every clash test pointing at it.
        /// </summary>
        public void AddEmpty(EmptySet empty)
        {
            if (empty != null)
            {
                this.empty.Add(empty);
            }
        }

        /// <summary>The sets that found nothing and why.</summary>
        public ReadOnlyCollection<EmptySet> Empty
        {
            get { return new ReadOnlyCollection<EmptySet>(empty); }
        }

        /// <summary>Sets whose question no longer matches the picked file.</summary>
        public ReadOnlyCollection<SetDrift> Drifted
        {
            get { return new ReadOnlyCollection<SetDrift>(drifted); }
        }

        /// <summary>How many of them this run rebuilt. Zero where the box is off, which is the default.</summary>
        public int RebuiltCount
        {
            get { return rebuiltCount; }
        }

        /// <summary>Sets already there and left alone. Never in CreatedCount.</summary>
        public int AlreadyPresentCount
        {
            get
            {
                int present = 0;

                foreach (SetResult result in results)
                {
                    if (result.Present)
                    {
                        present++;
                    }
                }

                return present;
            }
        }

        public void AddSkipped(SkippedSet set)
        {
            if (set == null)
            {
                throw new ArgumentNullException("set");
            }

            skipped.Add(set);
        }

        internal SetResult AddCreated(string path, string name, int conditionCount, int itemCount)
        {
            return AddCreated(path, name, conditionCount, itemCount, null);
        }

        public SetResult AddCreated(
            string path, string name, int conditionCount, int itemCount, string asked)
        {
            SetResult result = new SetResult(path, name, conditionCount, itemCount, null, asked);
            results.Add(result);
            return result;
        }

        public SetResult AddFailed(string path, string name, int conditionCount, string error)
        {
            SetResult result = new SetResult(
                path, name, conditionCount, -1, string.IsNullOrEmpty(error) ? "UNKNOWN" : error, null);
            results.Add(result);
            return result;
        }

        /// <summary>Sets that reached the model and resolved.</summary>
        public int CreatedCount
        {
            get { return Count(true, false); }
        }

        /// <summary>
        /// True when this build put at least one set into the document. That is what
        /// decides whether the NWF is saved again after the sets. A set already there
        /// was left alone and put nothing in, so it does not count either way: a rerun
        /// that finds sixty present and creates one still put one in.
        /// </summary>
        public bool PutAnythingIn
        {
            get { return CreatedCount > 0; }
        }

        /// <summary>Created sets that found at least one item.</summary>
        public int FindingItemsCount
        {
            get { return Count(true, true); }
        }

        /// <summary>Created sets that found nothing.</summary>
        public int ZeroCount
        {
            get
            {
                int zero = 0;

                foreach (SetResult result in results)
                {
                    if (result.IsZero)
                    {
                        zero++;
                    }
                }

                return zero;
            }
        }

        public int FailedCount
        {
            get
            {
                int failed = 0;

                foreach (SetResult result in results)
                {
                    if (!result.Created && !result.Present)
                    {
                        failed++;
                    }
                }

                return failed;
            }
        }

        public int SkippedCount
        {
            get { return skipped.Count; }
        }

        public int TotalItems
        {
            get
            {
                int total = 0;

                foreach (SetResult result in results)
                {
                    if (result.Created)
                    {
                        total += result.ItemCount;
                    }
                }

                return total;
            }
        }

        private int Count(bool created, bool withItems)
        {
            int count = 0;

            foreach (SetResult result in results)
            {
                if (result.Created != created)
                {
                    continue;
                }

                if (withItems && result.ItemCount <= 0)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        /// <summary>
        /// One line for the window and the log, the same words whether the sets were
        /// built by the run or by the Build sets button. Counted off the same list the
        /// lines come from. Nothing built and nothing skipped is a file holding no set,
        /// which is a normal case for a project keeping its sets in the model.
        /// </summary>
        public string Summary()
        {
            if (results.Count == 0)
            {
                return skipped.Count > 0
                    ? "No set in this file can be rebuilt. " + skipped.Count + " skipped."
                    : "This file holds no sets. Nothing to build.";
            }

            return CreatedCount + " created, "
                + AlreadyPresentCount + " already there, "
                + FindingItemsCount + " finding items, "
                + ZeroCount + " at zero"
                + (FailedCount > 0 ? ", " + FailedCount + " failed" : string.Empty)
                + (SkippedCount > 0 ? ", " + SkippedCount + " skipped" : string.Empty)
                + ".";
        }

        /// <summary>
        /// One line per set, then the totals. The totals are counted off the same list
        /// the lines came from, so they cannot disagree with them.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            foreach (SetResult result in results)
            {
                lines.Add(result.Line());
            }

            foreach (SkippedSet set in skipped)
            {
                lines.Add("SKIPPED " + set.Path + "  " + set.Reason);
            }

            lines.Add(string.Empty);
            lines.Add("ran against       : "
                + (string.IsNullOrEmpty(OpenDocument) ? "UNKNOWN" : OpenDocument));
            lines.Add("sets created      : " + CreatedCount);

            if (AlreadyPresentCount > 0)
            {
                lines.Add("already there     : " + AlreadyPresentCount + ", left alone, not copied again");

                // THE CORRECTED FILE DOES NOT REACH A SET THAT IS ALREADY THERE, and that
                // was silent until the worksets round. A set in the NWF was built from
                // whatever file was picked the FIRST time, so a value corrected in the
                // matrix since, such as ME-DUCTWORK becoming ME-Ductwork, changes the
                // file and changes nothing in the document. The run of 2026-09-20 proved
                // it by consequence: the models carry ME-Ductwork on 236 elements, the
                // corrected matrix asks for it, and the set still found nothing, so the
                // set in the document is still asking the old question.
                //
                // NOTHING IS DONE ABOUT IT HERE. Replacing a set changes what every clash
                // test pointing at it finds, and that is Bader's decision, Q72. This is
                // the same shape as the tolerance, F76, which is reported and applied
                // only when a person ticks a box.
                lines.Add("      a set already in the NWF keeps the conditions it was built with. A value");
                lines.Add("      corrected in the picked file since then does NOT reach it, so a set that");
                lines.Add("      finds nothing here may be asking a question the file no longer asks. Q72");
            }
            lines.Add("sets finding items: " + FindingItemsCount);
            lines.Add("sets at zero      : " + ZeroCount);

            if (FailedCount > 0)
            {
                lines.Add("sets that failed  : " + FailedCount);
            }

            if (SkippedCount > 0)
            {
                lines.Add("sets skipped      : " + SkippedCount);
            }

            lines.Add("items found       : " + TotalItems);
            return lines;
        }
    }
}
