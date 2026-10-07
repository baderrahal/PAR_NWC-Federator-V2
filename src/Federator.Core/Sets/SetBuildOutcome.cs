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
        private readonly List<SetDrift> notRead = new List<SetDrift>();
        private readonly List<EmptySet> empty = new List<EmptySet>();
        private int rebuiltCount;
        private readonly List<LeftoverSet> leftovers = new List<LeftoverSet>();
        private int actedOnLeftovers;

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
        /// Every present set compared with what the picked file asks, Q72, and whether this
        /// run rebuilt it. One whose question differs is kept as drifted, one whose search
        /// could not be read as not read, FR-021, and one asking what the file asks is not
        /// kept. Apart from the results list because a drifted set is still a present set and
        /// is counted as one.
        /// </summary>
        public void AddDrift(SetDrift drift, bool rebuilt)
        {
            if (drift == null)
            {
                return;
            }

            if (drift.CouldNotRead)
            {
                notRead.Add(drift);
                return;
            }

            if (!drift.Drifted)
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

        /// <summary>
        /// Judges that set when it found nothing, on what it asks, by what that judge knows of
        /// the values this group's models carry, FR-011. THE ONE RULE for which sets are judged:
        /// one at zero items, and never one whose count is UNKNOWN, minus one, FR-018, nor one
        /// whose question could not be read, asked null. A set rebuilt and not found again was
        /// recorded at 0 and judged empty on the question it asked before the rebuild.
        /// </summary>
        public void JudgeIfEmpty(SetResult result, IList<ReadCondition> asked, EmptySetJudge judge)
        {
            if (result == null || result.ItemCount != 0 || asked == null)
            {
                return;
            }

            AddEmpty(EmptySets.Why(result.Path, asked, judge));
        }

        /// <summary>The sets that found nothing and why.</summary>
        public ReadOnlyCollection<EmptySet> Empty
        {
            get { return new ReadOnlyCollection<EmptySet>(empty); }
        }

        /// <summary>
        /// Every set the picked file no longer names, and whether this run acted on it.
        /// Q74. Kept apart from the results list because a leftover is not a set this run
        /// built, and the log line for one says what pointed at it before anything moved.
        /// </summary>
        public void AddLeftover(LeftoverSet leftover, bool acted)
        {
            if (leftover == null)
            {
                return;
            }

            this.leftovers.Add(leftover);

            if (acted)
            {
                actedOnLeftovers++;
            }
        }

        /// <summary>
        /// Set where the remove and the rename pair failed BETWEEN its two halves, so the
        /// unused twin is gone and the working set did not take its name. Null on every
        /// ordinary run. The document is worse than it started and nothing may be saved
        /// from it, which is the rule `DamagedDocument` holds for both callers.
        /// </summary>
        public string TheDocumentIsDamaged { get; set; }

        /// <summary>The sets the picked file no longer names.</summary>
        public ReadOnlyCollection<LeftoverSet> Leftovers
        {
            get { return new ReadOnlyCollection<LeftoverSet>(leftovers); }
        }

        /// <summary>How many of them this run removed or renamed. Zero where the box is off.</summary>
        public int ActedOnLeftovers
        {
            get { return actedOnLeftovers; }
        }

        /// <summary>Sets whose question no longer matches the picked file.</summary>
        public ReadOnlyCollection<SetDrift> Drifted
        {
            get { return new ReadOnlyCollection<SetDrift>(drifted); }
        }

        /// <summary>
        /// Present sets whose search, or a value in it, could not be read, so whether they ask
        /// what the file asks is UNKNOWN, FR-021. Never counted as asking it.
        /// </summary>
        public ReadOnlyCollection<SetDrift> NotRead
        {
            get { return new ReadOnlyCollection<SetDrift>(notRead); }
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

        /// <summary>
        /// A set this build created from that plan, with what it found, judged by that judge
        /// where it found nothing, FR-027. Only a set already in the NWF was judged, so no EMPTY
        /// SETS block was written on a first run, the run that creates every set.
        /// </summary>
        public SetResult AddCreated(PlannedSet planned, int itemCount, EmptySetJudge judge)
        {
            if (planned == null)
            {
                throw new ArgumentNullException("planned");
            }

            SetResult result = AddCreated(planned.Path, planned.Name, planned.ConditionCount, itemCount, planned.Describe());
            JudgeIfEmpty(result, ReadCondition.Of(planned), judge);
            return result;
        }

        public SetResult AddFailed(string path, string name, int conditionCount, string error)
        {
            SetResult result = new SetResult(
                path, name, conditionCount, SetResult.NotCounted, string.IsNullOrEmpty(error) ? "UNKNOWN" : error, null);
            results.Add(result);
            return result;
        }

        /// <summary>Sets that reached the model and resolved.</summary>
        public int CreatedCount
        {
            get { return Count(true, false); }
        }

        /// <summary>
        /// True when this build changed the document: a set created, a drifted set rebuilt
        /// from the picked file, FR-020, or a set the file no longer names removed or renamed.
        /// That is what decides whether the NWF is saved again after the sets. A set already
        /// there and left alone put nothing in, so it does not count either way: a rerun that
        /// finds sixty present and creates one still put one in. A rebuild was left out, so
        /// with no test created or run the NWD was published from the rebuilt document and
        /// the NWF on disk kept the old sets.
        /// </summary>
        public bool PutAnythingIn
        {
            get { return CreatedCount > 0 || RebuiltCount > 0 || ActedOnLeftovers > 0; }
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
                    // A count not taken is UNKNOWN and left out, never summed as minus one.
                    if (result.Created && result.ItemCount > 0)
                    {
                        total += result.ItemCount;
                    }
                }

                return total;
            }
        }

        /// <summary>
        /// Created sets whose count could not be taken, so what they find is UNKNOWN, the breaker's
        /// finding on attempt 1. A null read of what a set finds was turned into zero in the add-in.
        /// </summary>
        public int CreatedNotCountedCount
        {
            get
            {
                int notCounted = 0;

                foreach (SetResult result in results)
                {
                    if (result.Created && result.ItemCount < 0)
                    {
                        notCounted++;
                    }
                }

                return notCounted;
            }
        }

        /// <summary>Sets already there that found at least one item, FR-022.</summary>
        public int PresentFindingItemsCount
        {
            get { return PresentWhere(result => result.ItemCount > 0); }
        }

        /// <summary>Sets already there that found nothing. One whose count is UNKNOWN is not among them, FR-018.</summary>
        public int PresentZeroCount
        {
            get { return PresentWhere(result => result.ItemCount == 0); }
        }

        /// <summary>Sets already there whose count could not be taken, FR-018.</summary>
        public int PresentNotCountedCount
        {
            get { return PresentWhere(result => result.ItemCount < 0); }
        }

        /// <summary>The items the sets already there found, a count not taken left out.</summary>
        public int PresentItems
        {
            get
            {
                int total = 0;

                foreach (SetResult result in results)
                {
                    if (result.Present && result.ItemCount > 0)
                    {
                        total += result.ItemCount;
                    }
                }

                return total;
            }
        }

        private int PresentWhere(Func<SetResult, bool> counts)
        {
            int count = 0;

            foreach (SetResult result in results)
            {
                if (result.Present && counts(result))
                {
                    count++;
                }
            }

            return count;
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
        /// The line the run log writes after a group's sets: what this build put into the
        /// document, the sets created, the sets already there with those this run REBUILT said
        /// apart from those left alone, and the leftovers brought up to date. Written in the engine
        /// as every set already there left alone, which since FR-020 called a set this run had
        /// just replaced left alone, the reviewer's finding on attempt 1.
        /// </summary>
        public string PutInLine()
        {
            return "put into the document: "
                + CreatedCount + " created, "
                + AlreadyPresentCount + " already there"
                + (RebuiltCount > 0
                    ? ", " + RebuiltCount + " of them rebuilt from the picked file and "
                        + (AlreadyPresentCount - RebuiltCount) + " left alone"
                    : " and left alone")
                + (Leftovers.Count > 0
                    ? ", " + ActedOnLeftovers + " of " + Leftovers.Count
                        + " set(s) the file no longer names brought up to date"
                    : string.Empty);
        }

        private static string WhetherTheyAsk(int notRead)
        {
            return (notRead == 1 ? "whether it asks" : "whether they ask") + " what the file asks is UNKNOWN";
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

            // FR-022. The finding and zero counts sit beside the kind of set they count. They
            // followed both counts and counted the sets created alone, so a weekly run whose
            // sets were all already there read 0 finding items and 0 at zero.
            return CreatedCount + " created"
                + (CreatedCount > 0
                    ? " (" + FindingItemsCount + " finding items, " + ZeroCount + " at zero"
                        + (CreatedNotCountedCount > 0 ? ", " + CreatedNotCountedCount + " not counted" : string.Empty)
                        + ")"
                    : string.Empty)
                + ", " + AlreadyPresentCount + " already there"
                + (AlreadyPresentCount > 0
                    ? " (" + PresentFindingItemsCount + " finding items, " + PresentZeroCount + " at zero"
                        + (PresentNotCountedCount > 0 ? ", " + PresentNotCountedCount + " not counted" : string.Empty)
                        + ")"
                    : string.Empty)
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
            // FR-022. Each count sits under the kind of set it counts, the created and the
            // already there, and said nowhere that it counted only the sets created.
            lines.Add("sets created      : " + CreatedCount);
            lines.Add("   finding items  : " + FindingItemsCount);
            lines.Add("   at zero        : " + ZeroCount);

            if (CreatedNotCountedCount > 0)
            {
                lines.Add("   not counted    : " + CreatedNotCountedCount + ", could not be counted, so what they find is UNKNOWN");
            }

            lines.Add("   items found    : " + TotalItems);

            if (AlreadyPresentCount > 0)
            {
                // FR-020. The same words as PutInLine, so a set this run replaced is never said
                // to be left alone in the window.
                lines.Add("already there     : " + AlreadyPresentCount
                    + (RebuiltCount > 0
                        ? ", " + RebuiltCount + " of them rebuilt from the picked file and "
                            + (AlreadyPresentCount - RebuiltCount) + " left alone, not copied again"
                        : ", left alone, not copied again"));

                // THE CORRECTED FILE DOES NOT REACH A SET THAT IS ALREADY THERE, and that
                // was silent until the worksets round. A set in the NWF was built from
                // whatever file was picked the FIRST time, so a value corrected in the
                // matrix since, such as ME-DUCTWORK becoming ME-Ductwork, changes the
                // file and changes nothing in the document. The run of 2026-09-20 proved
                // it by consequence: the models carry ME-Ductwork on 236 elements, the
                // corrected matrix asks for it, and the set still found nothing, so the
                // set in the document is still asking the old question.
                //
                // NOTHING IS DONE ABOUT IT UNLESS A PERSON TICKS THE BOX. Replacing a set
                // changes what every clash test pointing at it finds, and that is Bader's
                // decision, Q72, answered a. This is the same shape as the tolerance, F76,
                // which is reported and applied only when a person ticks a box.
                //
                // THESE LINES SAY WHAT THIS RUN FOUND AND NOT WHAT MIGHT BE TRUE. Until
                // the drift round nothing had ever read the question a set in the document
                // asks, so all this block could say was that a set finding nothing MAY be
                // asking an old question. It is read now, so the count is said instead.
                lines.Add("      a set already in the NWF keeps the conditions it was built with, so a value");
                lines.Add("      corrected in the picked file since then does not reach it on its own. Q72");

                // FR-021. A set whose search could not be read is not one that asks what the
                // file asks, so the claim that every set does is made only where all were read.
                if (Drifted.Count == 0 && NotRead.Count == 0)
                {
                    lines.Add("      none of them drifted. Every set in the document asks what the file asks");
                }
                else if (Drifted.Count == 0)
                {
                    lines.Add("      none of the " + (AlreadyPresentCount - NotRead.Count) + " read drifted. "
                        + NotRead.Count + " could not be read, so " + WhetherTheyAsk(NotRead.Count));
                }
                else if (RebuiltCount == 0)
                {
                    lines.Add("      " + Drifted.Count + " of them DRIFTED and none was rebuilt, because the box is off. Each is"
                        + " named above with the old question and the new one");
                }
                else
                {
                    lines.Add("      " + Drifted.Count + " of them DRIFTED and " + RebuiltCount
                        + " were REBUILT from the picked file. The clash tests");
                    lines.Add("      pointing at a rebuilt set keep their results and their statuses, measured 5v");
                }

                if (Drifted.Count > 0 && NotRead.Count > 0)
                {
                    lines.Add("      " + NotRead.Count + " more could not be read, so " + WhetherTheyAsk(NotRead.Count));
                }

                lines.Add("   finding items  : " + PresentFindingItemsCount);
                lines.Add("   at zero        : " + PresentZeroCount);
                lines.Add("   items found    : " + PresentItems);

                if (PresentNotCountedCount > 0)
                {
                    lines.Add("   not counted    : " + PresentNotCountedCount + ", not found again to count, so what they find is UNKNOWN");
                }
            }

            if (FailedCount > 0)
            {
                lines.Add("sets that failed  : " + FailedCount);
            }

            if (SkippedCount > 0)
            {
                lines.Add("sets skipped      : " + SkippedCount);
            }

            return lines;
        }
    }
}
