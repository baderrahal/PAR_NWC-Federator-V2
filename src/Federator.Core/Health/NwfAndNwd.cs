using System.Collections.Generic;
using Federator.Core.Diagnostics;

namespace Federator.Core.Health
{
    /// <summary>
    /// What the disk says about one group's NWF and NWD once the group has finished, read
    /// after the NWF was looked at the last time, for the note of a group whose clash was
    /// skipped. A file this tool did not write is never named as written: the NWF is said
    /// to be on the disk, because an NWF this run reused and did not save again is on the
    /// disk too, and the NWD is said to be published only where the publish reported
    /// success, because last week's NWD sits at the same path.
    /// </summary>
    public sealed class NwfAndNwd
    {
        private readonly string nwfPath;
        private readonly bool nwfOnDisk;
        private readonly string nwdPath;
        private readonly bool nwdOnDisk;
        private readonly bool nwdPublished;

        public NwfAndNwd(string nwfPath, bool nwfOnDisk, string nwdPath, bool nwdOnDisk, bool nwdPublished)
        {
            this.nwfPath = Words.Or(nwfPath, "an unknown path");
            this.nwfOnDisk = nwfOnDisk;
            this.nwdPath = Words.Or(nwdPath, "an unknown path");
            this.nwdOnDisk = nwdOnDisk;
            this.nwdPublished = nwdPublished;
        }

        /// <summary>One sentence for the NWF and one for the NWD, naming the file that was not written where one was not.</summary>
        internal IList<string> Lines()
        {
            List<string> lines = new List<string>();

            lines.Add(nwfOnDisk
                ? "The NWF is on disk at " + nwfPath + ", read after the NWD was published."
                : "The NWF was NOT written. It is not on disk at " + nwfPath + ".");

            if (!nwdOnDisk)
            {
                lines.Add("The NWD was NOT written. It is not on disk at " + nwdPath + ".");
            }
            else if (!nwdPublished)
            {
                lines.Add("The NWD was NOT written by this run. The file at " + nwdPath
                    + " is from an earlier run, because the publish did not report success.");
            }
            else
            {
                lines.Add("The NWD was published by this run at " + nwdPath + ".");
            }

            return lines;
        }
    }
}
