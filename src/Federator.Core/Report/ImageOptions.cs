using System;
using System.Collections.Generic;
using Federator.Core.Clash;

namespace Federator.Core.Report
{
    /// <summary>
    /// What the run does about clash images.
    ///
    /// On by default, because the accepted report has them and a report without them is
    /// not the thing the client agreed to receive.
    /// </summary>
    public sealed class ImageOptions
    {
        /// <summary>
        /// The size the accepted report's own pictures are, measured off all 60 of them
        /// on 2026-08-31. Every one is 1024 by 1024.
        /// </summary>
        public const int DefaultPixels = 1024;

        /// <summary>
        /// How many failures of the same kind end the run. Read off the clash step's own
        /// guard rather than typed again, because the rule is that both stop the same way
        /// after the same count. Fifty images that all failed the same way is a broken
        /// run, not a bad model.
        /// </summary>
        public const int DefaultStopAfterFailures = Federator.Core.Clash.RepeatedFailureGuard.DefaultThreshold;

        private readonly HashSet<ClashStatus> statuses = new HashSet<ClashStatus>();

        public ImageOptions()
        {
            Write = true;
            CapPerTest = 0;
            Width = DefaultPixels;
            Height = DefaultPixels;
            EmbedThumbnail = false;
            StopAfterFailures = DefaultStopAfterFailures;

            // New, Active and Reviewed. The same three Navisworks itself calls open, but
            // set out here in its own right, because this is which clashes are worth a
            // picture and that is a different question from what the matrix counts.
            foreach (ClashStatus status in OpenClashes.StatusesFor(OpenClashCount.NavisworksOpen))
            {
                statuses.Add(status);
            }
        }

        /// <summary>
        /// On, and the window no longer sets it. The accepted report has images, so ours
        /// does too. Off only when every status is unticked, which is no picture said a
        /// longer way. F34.
        /// </summary>
        public bool Write { get; set; }

        /// <summary>
        /// The most images one test may write, or zero for no cap, which is the default.
        /// A cap is there for the test that finds 1244 clashes and would otherwise spend
        /// the whole run rendering them.
        /// </summary>
        public int CapPerTest { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        /// <summary>
        /// Off. The accepted report links to its pictures and does not paste them into
        /// cells, so a thumbnail in the cell is an addition rather than a match.
        /// </summary>
        public bool EmbedThumbnail { get; set; }

        /// <summary>How many failures in a row end the run. Zero switches the guard off.</summary>
        public int StopAfterFailures { get; set; }

        /// <summary>Is this clash one to render.</summary>
        public bool Wants(ClashStatus status)
        {
            return Write && statuses.Contains(status);
        }

        /// <summary>
        /// Is there room under the cap for one more on this test. The count is what has
        /// already been written for it.
        /// </summary>
        public bool RoomFor(int alreadyWritten)
        {
            return CapPerTest <= 0 || alreadyWritten < CapPerTest;
        }

        /// <summary>Only the statuses asked for, in the order the tally reports them.</summary>
        public IList<ClashStatus> ChosenStatuses()
        {
            List<ClashStatus> chosen = new List<ClashStatus>();

            foreach (ClashStatus status in ClashTally.AllStatuses)
            {
                if (statuses.Contains(status))
                {
                    chosen.Add(status);
                }
            }

            return chosen;
        }

        /// <summary>Replaces the status filter. Empty is refused, because it means no images at all.</summary>
        public void OnlyFor(IEnumerable<ClashStatus> wanted)
        {
            if (wanted == null)
            {
                throw new ArgumentNullException("wanted");
            }

            List<ClashStatus> asked = new List<ClashStatus>(wanted);

            if (asked.Count == 0)
            {
                throw new ArgumentException(
                    "An empty status filter writes no images. Switch images off instead, "
                    + "so the log says that is what was meant.",
                    "wanted");
            }

            statuses.Clear();

            foreach (ClashStatus status in asked)
            {
                statuses.Add(status);
            }
        }

        /// <summary>One line saying what these settings will do, for the log and the window.</summary>
        public string Describe()
        {
            if (!Write)
            {
                return "Images are off, so no picture is rendered and every Image cell is empty.";
            }

            string statusList = string.Join(", ", Names(ChosenStatuses()));
            string cap = CapPerTest > 0
                ? "at most " + CapPerTest + " per test"
                : "no cap per test";

            return "Images on, " + Width + " by " + Height + " pixels, " + statusList
                + " only, " + cap + ", "
                + (EmbedThumbnail ? "with a thumbnail in the cell" : "linked rather than pasted in")
                + ".";
        }

        private static string[] Names(IList<ClashStatus> chosen)
        {
            string[] names = new string[chosen.Count];

            for (int i = 0; i < chosen.Count; i++)
            {
                names[i] = chosen[i].ToString();
            }

            return names;
        }
    }
}
