using System;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// THE ONE RULE FOR A DOCUMENT THAT WAS MODIFIED AND THEN FAILED, written once because
    /// two places need it and two copies of it would drift.
    ///
    /// WHY IT EXISTS. `ReshapeFromScan` could fail after taking models out and appending
    /// others, and it said "the file on disk is left exactly as it was" on the way out.
    /// That sentence was true for the one failure that happens before anything is touched
    /// and FALSE for the two that happen after, and worse, returning false there let the
    /// clear and rebuild run over the damage, read its before counts off the already
    /// damaged document, report everything kept, and SAVE THE NWF OVER. The message told
    /// him his data was safe while the run wrote the damage to disk.
    ///
    /// THE RULE HAS THREE PARTS AND THEY ONLY WORK TOGETHER.
    ///
    ///   1. THE COUNTS ARE TAKEN BEFORE THE FIRST MODIFICATION AND HELD. A count re read
    ///      off a touched document is a count of the damage, and it will always agree with
    ///      itself.
    ///   2. A FAILURE AFTER A MODIFICATION NEVER SAVES. Not the step that failed and not a
    ///      fallback after it. The in memory document is thrown away by the next group
    ///      clearing it, and the NWF on disk is the last good copy because nothing wrote
    ///      over it.
    ///   3. THE MESSAGE SAYS WHICH OF THE TWO HAPPENED. Nothing was touched, so the file
    ///      on disk is exactly as it was, OR the document in memory is damaged and was not
    ///      saved, so the file on disk is the last good copy. Those are different
    ///      sentences because they are different facts, and the second one is only true
    ///      because part 2 is kept.
    ///
    /// WHO CALLS IT. The reshape of a CHANGED group, and the set rename and removal pair
    /// of Q74, which has the same shape: a remove and a rename that are two calls and one
    /// change, with a window between them where neither set carries the corrected name.
    /// </summary>
    public static class DamagedDocument
    {
        /// <summary>
        /// What to say when the work stopped BEFORE anything in the document was changed.
        /// The file on disk is untouched and so is the document, so a fallback may still
        /// run and the next thing to read the document reads it as it was.
        /// </summary>
        public static string NothingWasTouched(string what)
        {
            return Reason(what) + "nothing in the open file had been changed yet, so the file on disk is exactly as it was";
        }

        /// <summary>
        /// What to say when the work stopped AFTER something in the document was changed.
        /// NOTHING IS SAVED FROM HERE, which is what makes the second half of the sentence
        /// true rather than a hope.
        /// </summary>
        public static string TheDocumentIsDamaged(string what)
        {
            return Reason(what)
                + "the open file had already been changed by then, so it was NOT saved and "
                + "the file on disk is the last good copy";
        }

        /// <summary>
        /// Whether a fallback may run after this failure. A fallback reads the document it
        /// is handed, so it may only run over one nothing has touched. Over a damaged one
        /// it would read the damage as the starting state and report it as intact.
        /// </summary>
        public static bool AFallbackMayRun(bool anythingWasChanged)
        {
            return !anythingWasChanged;
        }

        /// <summary>
        /// Whether the file on disk may be written. The one gate both callers pass through,
        /// so a save can never happen on a path that failed after a change.
        /// </summary>
        public static bool TheFileMayBeSaved(bool anythingWasChanged, bool everythingWorked)
        {
            return everythingWorked || !anythingWasChanged;
        }

        /// <summary>
        /// The sentence for either case, chosen by the one fact that decides it. Callers
        /// use this rather than picking, so the message and the save decision can never
        /// disagree.
        /// </summary>
        public static string Line(string what, bool anythingWasChanged)
        {
            return anythingWasChanged ? TheDocumentIsDamaged(what) : NothingWasTouched(what);
        }

        private static string Reason(string what)
        {
            return string.IsNullOrEmpty(what) ? string.Empty : what.TrimEnd() + ", and ";
        }
    }
}
