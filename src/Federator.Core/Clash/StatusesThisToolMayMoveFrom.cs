using System;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Which statuses a clash may be moved OFF, which is a different question from which
    /// it may be moved ON TO.
    ///
    /// StatusesThisToolMaySet answers the second: Reviewed and nothing else. This answers
    /// the first: New and Active and nothing else.
    ///
    /// NEVER OVERWRITE A DECISION. Reviewed, Approved and Resolved are all somebody's
    /// statement about a clash. A person moved it there and the NWF is the only record
    /// that they did. New and Active are not a decision at all, they are what running a
    /// test produces, so moving off one of those takes nothing away from anybody.
    ///
    /// WHY THIS IS A TYPE AND NOT A CONDITION INSIDE A LOOP. Before F72 the editor applied
    /// whatever it was handed as long as the status was not already what was wanted, so a
    /// caller that asked for an Approved clash to become Reviewed would have got exactly
    /// that, quietly, into the one file that records what has been fixed. Nothing asked,
    /// because nothing supplied a list at all while Q33 was open. F72 supplies one, so the
    /// guard has to be real and it has to be somewhere a test can reach it.
    /// </summary>
    public static class StatusesThisToolMayMoveFrom
    {
        /// <summary>The two this tool may move a clash off. New and Active.</summary>
        public static ClashStatus[] All()
        {
            return new[] { ClashStatus.New, ClashStatus.Active };
        }

        /// <summary>Whether a clash sitting at that status may be moved at all.</summary>
        public static bool Allows(ClashStatus status)
        {
            return status == ClashStatus.New || status == ClashStatus.Active;
        }

        /// <summary>
        /// Why not, in the words the log carries, or null where it is allowed. Every
        /// refusal says WHICH status the clash is at, because a line saying only that
        /// something was left alone sends the reader back to the model.
        /// </summary>
        public static string WhyNot(ClashStatus status)
        {
            if (Allows(status))
            {
                return null;
            }

            return "a person set it to " + status
                + " and this tool never moves a clash off a status a person set";
        }
    }
}
