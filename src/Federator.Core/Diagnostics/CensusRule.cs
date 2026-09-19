using System;
using System.Collections.Generic;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// Which step is allowed to move which count, and what the log says when one moves
    /// that should not have.
    ///
    /// WHY THIS IS A RULE AND NOT A COMMENT. Every count here is something the NWF
    /// carries and something there is no second copy of. A run that lost 1830 clash tests
    /// during the workbook write would have written the workbook, published the NWD and
    /// reported DONE, and nothing in the log would have said a word. The counts are read
    /// before and after every step, and a move the rule does not allow is named with the
    /// step, the count, the before and the after.
    ///
    /// THE TOOL REPORTS AND DOES NOT ACT. A refused move does not stop the run, does not
    /// undo anything and does not skip the next step. It puts the group out of DONE and
    /// says exactly what it saw, and Bader decides.
    ///
    /// WHAT IS MEASURED AND WHAT IS JUDGEMENT. The allowed moves below were read off the
    /// engine on 2026-09-19, step by step. Where a step plainly cannot touch the document
    /// at all, it allows nothing, and that is the useful half: a count moving during the
    /// workbook write or the NWD publish is the kind of thing nobody would think to look
    /// for.
    /// </summary>
    public static class CensusRule
    {
        /// <summary>
        /// Whether that step is allowed to change that count.
        ///
        /// DECIDE opens the NWF that is already on disk, and opening a document replaces
        /// everything in it, so all five may move there and only there.
        /// APPEND puts the NWC files in, so the models move. Nothing else comes in with
        /// them: an NWC carries geometry and properties, not sets or tests.
        /// SETS builds the selection sets out of the picked file.
        /// TESTS CREATE creates the clash tests.
        /// TESTS RUN produces the clash results, and creates none of the rest.
        /// Every other step writes a FILE and not the document, so none of them may move
        /// anything at all.
        /// </summary>
        public static bool MayMove(string step, CensusCount what)
        {
            if (string.IsNullOrEmpty(step))
            {
                return false;
            }

            if (string.Equals(step, RunSteps.Decide, StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(step, RunSteps.Append, StringComparison.Ordinal))
            {
                return what == CensusCount.Models;
            }

            if (string.Equals(step, RunSteps.Sets, StringComparison.Ordinal))
            {
                return what == CensusCount.Sets;
            }

            if (string.Equals(step, RunSteps.TestsCreate, StringComparison.Ordinal))
            {
                return what == CensusCount.Tests;
            }

            if (string.Equals(step, RunSteps.TestsRun, StringComparison.Ordinal))
            {
                return what == CensusCount.Results;
            }

            return false;
        }

        /// <summary>
        /// The steps that may move anything. Where the census costs too much to take
        /// around every step, these are the ones it is taken around instead, because a
        /// step that may move nothing has nothing to catch except a fault, and a fault
        /// that only shows in a step nobody times is still in the group totals.
        /// </summary>
        public static IList<string> StepsThatMayWrite()
        {
            List<string> writes = new List<string>();

            foreach (string step in RunSteps.All)
            {
                foreach (CensusCount what in DocumentCensus.All)
                {
                    if (MayMove(step, what))
                    {
                        writes.Add(step);
                        break;
                    }
                }
            }

            return writes;
        }

        /// <summary>
        /// Whether the census is taken around this step at all. True for every step while
        /// the census is cheap, and only for the writing steps once it is not.
        /// </summary>
        public static bool TakenAround(string step, bool cheap)
        {
            if (cheap)
            {
                return RunSteps.IsAStep(step);
            }

            return StepsThatMayWrite().Contains(step);
        }

        /// <summary>The words that begin a refused move, so nothing else can use them.</summary>
        public const string ChangedPrefix = "CENSUS CHANGED";

        /// <summary>
        /// The line for one refused move: the step, the count, the before and the after.
        /// Every number on it was read off the document and none is worked out from
        /// another.
        /// </summary>
        public static string ChangedLine(string step, CensusCount what, int before, int after)
        {
            return ChangedPrefix + "  " + (string.IsNullOrEmpty(step) ? "UNKNOWN step" : step)
                + "  " + DocumentCensus.Words(what)
                + " went from " + before + " to " + after
                + ", and that step is not one that may change them. "
                + "Nothing was undone and the run carried on. This group is not DONE";
        }

        /// <summary>
        /// The reason that goes on the group, so the RESULT block and the window say the
        /// same thing the CENSUS CHANGED line says.
        /// </summary>
        public static string Reason(string step, CensusCount what, int before, int after)
        {
            return DocumentCensus.Words(what) + " went from " + before + " to " + after
                + " during " + (string.IsNullOrEmpty(step) ? "UNKNOWN step" : step)
                + ", which is not a step that may change them";
        }

        /// <summary>
        /// Every line one step's before and after asks for. Empty where nothing moved and
        /// everything that moved was allowed to, which is the ordinary case and writes no
        /// line at all, because a log saying nothing happened once per step per group is
        /// a log nobody reads.
        /// </summary>
        public static IList<string> Lines(string step, DocumentCensus before, DocumentCensus after)
        {
            List<string> lines = new List<string>();

            if (before == null || after == null)
            {
                return lines;
            }

            foreach (CensusCount what in after.MovedSince(before))
            {
                if (MayMove(step, what))
                {
                    continue;
                }

                lines.Add(ChangedLine(step, what, before.Of(what), after.Of(what)));
            }

            return lines;
        }

        /// <summary>
        /// Every refused move as a reason, in the same order as the lines, for the group.
        /// </summary>
        public static IList<string> Reasons(string step, DocumentCensus before, DocumentCensus after)
        {
            List<string> reasons = new List<string>();

            if (before == null || after == null)
            {
                return reasons;
            }

            foreach (CensusCount what in after.MovedSince(before))
            {
                if (MayMove(step, what))
                {
                    continue;
                }

                reasons.Add(Reason(step, what, before.Of(what), after.Of(what)));
            }

            return reasons;
        }
    }
}
