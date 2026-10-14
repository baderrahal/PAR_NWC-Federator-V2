using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// F182, Bader's answer B to Q175. The lines that say which step of a group ended last, so a reader of a log
    /// that stops in the picture step, or ends there, sees that the NWF was saved and the NWD published first.
    /// The order itself is RunSteps.All. Nothing here changes what a run does.
    /// </summary>
    public static class GroupOrder
    {
        /// <summary>The line said once the NWD is published and confirmed, before the pictures begin.</summary>
        public static string WrittenBeforeThePictures(string building, long nwfBytes, long nwdBytes, bool pictures)
        {
            return "ORDER    " + Name(building) + ": the NWF is " + Size(nwfBytes) + " and the NWD is " + Size(nwdBytes)
                + ", both before the pictures. "
                + (pictures
                    ? "The pictures follow. If they stop early or fail, both files are already written"
                    : "No picture is taken in this group");
        }

        /// <summary>
        /// The line said once the picture step ends: how it ended, and that it was the last step to end so far.
        /// A stop is a normal end of the step, and a throw is not.
        /// </summary>
        public static string PicturesEnded(string building, bool stoppedByAStop, bool threw)
        {
            string how = threw
                ? "with a failure, the rows not yet pictured keep an empty Image cell"
                : stoppedByAStop
                    ? "early at a stop, which is a normal end"
                    : "at the end of its rows";

            return "ORDER    " + Name(building) + ": the last step to end was PICTURES, " + how
                + ". The workbook, the HTML and the txt files follow";
        }

        /// <summary>The line said at the end of the group: the steps in the order they ended.</summary>
        public static string EndedInOrder(string building, IList<string> ended)
        {
            return "ORDER    " + Name(building) + ": the steps ended in this order, "
                + (ended == null || ended.Count == 0 ? "none" : string.Join(", ", new List<string>(ended).ToArray()));
        }

        /// <summary>
        /// Null where the pictures, if they were taken, came after the NWD, or words naming the fault where a
        /// picture step ended before the NWD step did. A step not in the list is not judged.
        /// </summary>
        public static string FaultIn(IList<string> ended)
        {
            if (ended == null)
            {
                return null;
            }

            int nwd = ended.IndexOf(RunSteps.Nwd);
            int pictures = ended.IndexOf(RunSteps.Pictures);

            if (nwd >= 0 && pictures >= 0 && pictures < nwd)
            {
                return "the picture step ended before the NWD was published, which the order of a group does not allow";
            }

            return null;
        }

        private static string Name(string building)
        {
            return string.IsNullOrEmpty(building) ? "this group" : building;
        }

        private static string Size(long bytes)
        {
            return bytes < 0
                ? "NOT ON DISK"
                : bytes.ToString("#,##0", CultureInfo.InvariantCulture) + " bytes";
        }
    }
}
