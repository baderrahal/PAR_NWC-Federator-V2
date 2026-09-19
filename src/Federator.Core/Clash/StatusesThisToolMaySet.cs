using System;
using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Which result statuses this tool is allowed to set, and which it must never set.
    ///
    /// REVIEWED AND NOTHING ELSE. A clash that cannot be solved should not sit at New or
    /// Active forever, and marking it Reviewed says a person looked at it and moved on,
    /// which is true when the rule that picked it is a rule a person agreed to.
    ///
    /// APPROVED AND RESOLVED ARE NEVER SET BY THIS TOOL. Both are a person's statement
    /// about work that was actually done. Approved says somebody with the authority to
    /// accept a clash accepted it. Resolved says the clash is gone from the model. A tool
    /// that sets either is putting words in somebody's mouth about a building, and the NWF
    /// is the only record of what has been fixed, so there is nothing to compare it
    /// against afterwards.
    ///
    /// New and Active are not set either, for a duller reason: running a test produces
    /// them, so setting them by hand achieves nothing and would overwrite whatever a
    /// person had already decided.
    /// </summary>
    public static class StatusesThisToolMaySet
    {
        /// <summary>The one status this tool sets. Reviewed.</summary>
        public static ClashStatus[] All()
        {
            return new[] { ClashStatus.Reviewed };
        }

        /// <summary>Whether this tool may set that status.</summary>
        public static bool Allows(ClashStatus status)
        {
            return status == ClashStatus.Reviewed;
        }

        /// <summary>
        /// Whether this tool may set that status AS AN UNDO, F72c.
        ///
        /// THIS IS NOT A LOOSENING OF THE RULE ABOVE AND IT MUST NOT BECOME ONE. The rule
        /// above stops this tool INVENTING a status. An undo invents nothing: it puts a
        /// clash back to the exact status one of this tool's own records says this tool
        /// moved it off, in this file, and it refuses anything else. Approved and Resolved
        /// are still never set, because no record can name one: a record can only be
        /// written for a status this tool was allowed to move from, which is New or
        /// Active, and the record's own constructor refuses the other three.
        ///
        /// So the whole of what an undo may do is: read our record, read the status it
        /// names, set that. A record that names something else, or no record at all, is a
        /// refusal.
        ///
        /// Whether the record can be written at all is the measurement in scan.md 5h. If
        /// a comment cannot be written on a clash result there is no record, so there is
        /// nothing to undo, and the button says that in one line rather than guessing.
        /// </summary>
        public static bool AllowsAsUndo(ClashStatus asked, AutoReviewRecord record)
        {
            return record != null && asked == record.WasAt;
        }

        /// <summary>
        /// Why an undo may not set that status, or null where it may. Every refusal says
        /// WHICH status was asked for, the same as the one above.
        /// </summary>
        public static string WhyNotAsUndo(ClashStatus asked, AutoReviewRecord record)
        {
            if (record == null)
            {
                return "there is no record of this tool moving that clash, so there is "
                    + "nothing to undo and " + asked + " would be a status this tool invented";
            }

            if (asked != record.WasAt)
            {
                return "the record says the clash was at " + record.WasAt + " and the undo "
                    + "asked for " + asked + ". An undo puts a clash back where it was and "
                    + "nowhere else";
            }

            return null;
        }

        /// <summary>
        /// Why not, in the words the log carries, or null where it is allowed. Every
        /// refusal says WHICH status was asked for, because a line saying only that
        /// something was refused sends the reader back to the code.
        /// </summary>
        public static string WhyNot(ClashStatus status)
        {
            if (Allows(status))
            {
                return null;
            }

            if (status == ClashStatus.Approved || status == ClashStatus.Resolved)
            {
                return "this tool never sets " + status
                    + ", because it is a person's statement about work that was actually done. "
                    + "Reviewed is the only status it sets";
            }

            return "this tool never sets " + status
                + ", because running a test produces it and setting it by hand would "
                + "overwrite what a person had already decided. Reviewed is the only status it sets";
        }
    }

    /// <summary>
    /// What the five clash statuses and the four test statuses are, in words, so the log
    /// and the docs cannot drift from each other or from the enums.
    ///
    /// THE POINT OF THIS TYPE. A clash carries New, Active, Reviewed, Approved or
    /// Resolved. A TEST carries New, Old, Partial or Complete. They are different sets on
    /// different things and the two share the word New, which is how they get confused.
    /// Old is a TEST word and is not a clash status at all, so nothing in this tool ever
    /// moves a clash from Old, because no clash is ever at Old.
    /// </summary>
    public static class StatusWords
    {
        /// <summary>The block the log carries the first time a status is set in a run.</summary>
        public static IList<string> Lines()
        {
            return new List<string>
            {
                "STATUS   a clash carries one of five: New, Active, Reviewed, Approved, Resolved",
                "STATUS   a TEST carries one of four: New, Old, Partial, Complete",
                "STATUS   the two sets are different things and share only the word New",
                "STATUS   Old is a test word and never a clash word, so no clash is ever at Old "
                    + "and nothing here ever moves one from it",
                "STATUS   this tool sets Reviewed and nothing else. Approved and Resolved are a "
                    + "person's decision about work that was actually done"
            };
        }
    }
}
