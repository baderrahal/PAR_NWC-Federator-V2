namespace Federator.Core.Report
{
    /// <summary>
    /// Clash Detective's Hide Other across one run, F150 attempt 2, the breaker's second point on
    /// attempt 1. The user's value is the one read at the first group of the run that read it, and
    /// every later group puts Hide Other back to that value, so a put back that failed in one group
    /// cannot make the value it left behind read as the user's own in the next. A group that reads
    /// another value says so, a put back owed is tried once more at the end of the run, and the
    /// group that left it changed carries an error the window shows. Made once for the run beside
    /// the pictures' guard and handed to each group's ClashImages. OtherItemsInPictures is the rule
    /// of one group.
    /// </summary>
    public sealed class HideOtherAcrossTheRun
    {
        /// <summary>The user's Hide Other read at the run's first group that read it, or null before any read.</summary>
        public bool? UsersValue { get; private set; }

        private bool owed;
        private string groupError;

        /// <summary>A group's first read. Kept as the user's value where it is the run's first.</summary>
        internal void Heard(bool value)
        {
            if (!UsersValue.HasValue)
            {
                UsersValue = value;
            }
        }

        /// <summary>Whether a put back is owed: a group left Hide Other other than the user's value, or could not say.</summary>
        internal void Owe(bool isOwed, string error)
        {
            owed = isOwed;

            if (error != null)
            {
                groupError = error;
            }
        }

        /// <summary>Whether the end of the run tries the put back once more: one is owed and the user's value is known.</summary>
        public bool OwesAPutBack
        {
            get { return owed && UsersValue.HasValue; }
        }

        /// <summary>The error of the group just ended for its errors, which the window shows, or null, cleared as it is taken.</summary>
        public string TakeTheGroupError()
        {
            string taken = groupError;
            groupError = null;
            return taken;
        }

        /// <summary>
        /// The one IMAGES line of the put back tried once more at the end of the run: what was
        /// written, whether it threw and what it read after. Reads the owed put back as paid where
        /// it then reads as the user's value.
        /// </summary>
        public string AtTheEnd(string whyItThrew, bool? readAfter, string whyNotRead)
        {
            string users = OtherItemsInPictures.OnOff(UsersValue.HasValue && UsersValue.Value);
            string said = "IMAGES   Clash Detective's Hide Other was left changed by a group of this run, so it was put back once more "
                + "at the end of the run to " + users + ", the user's own read at the run's first group that read it";

            if (whyItThrew != null)
            {
                said += ", and that threw, " + OtherItemsInPictures.Said(whyItThrew);
            }

            if (!readAfter.HasValue)
            {
                return said + ". It could not be read after, " + OtherItemsInPictures.Said(whyNotRead)
                    + ", so whether the user's setting is as it was is UNKNOWN. " + OtherItemsInPictures.WhereToSetIt + " shows it";
            }

            if (UsersValue.HasValue && readAfter.Value == UsersValue.Value)
            {
                owed = false;
                return said + ". It read " + users + " after, the user's own setting";
            }

            return said + ". It read " + OtherItemsInPictures.OnOff(readAfter.Value) + " after, not the " + users
                + " the user had, so the user's setting is left changed. " + OtherItemsInPictures.WhereToSetIt + " sets it back";
        }
    }
}
