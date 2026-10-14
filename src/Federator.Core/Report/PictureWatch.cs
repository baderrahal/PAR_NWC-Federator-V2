using System;
using System.Globalization;

namespace Federator.Core.Report
{
    /// <summary>
    /// When the picture pass says how it is doing, F157. After a run died at picture 8775 of 10206
    /// with no memory line since the start of the step, the pass says its memory and its rate at
    /// the start and at every Every pictures, so the next crash leaves a trail of numbers and not a
    /// guess. Holds no clock of its own. The caller hands in the seconds, so a test fixes them.
    /// </summary>
    public sealed class PictureWatch
    {
        /// <summary>How many pictures between two lines. A setting on the pass, not a number in a loop.</summary>
        public const int DefaultEvery = 250;

        private readonly int every;
        private int lastDone;
        private double lastSeconds;
        private readonly double startSeconds;

        public PictureWatch(int every, double startSeconds)
        {
            if (every < 1)
            {
                throw new ArgumentOutOfRangeException("every", "A picture count between two lines is at least one.");
            }

            this.every = every;
            this.startSeconds = startSeconds;
            lastSeconds = startSeconds;
        }

        /// <summary>
        /// Whether done has reached a new multiple of Every since the last line. A count that jumps
        /// past a multiple, as one stretch of rows can, is due once and not once per multiple.
        /// </summary>
        public bool Due(int done)
        {
            return done > lastDone && done / every > lastDone / every;
        }

        /// <summary>
        /// The rate line for the pictures done now, and the mark moved on to them. Pictures per second
        /// since the last line and since the start of the pass, and UNKNOWN where no time has passed.
        /// </summary>
        public string RateLine(int done, int total, double nowSeconds)
        {
            int since = done - lastDone;
            double window = nowSeconds - lastSeconds;
            double whole = nowSeconds - startSeconds;
            string line = "RATE     PICTURES " + done.ToString(CultureInfo.InvariantCulture)
                + (total > 0 ? " of " + total.ToString(CultureInfo.InvariantCulture) : string.Empty)
                + " done, " + since.ToString(CultureInfo.InvariantCulture) + " in the last "
                + window.ToString("0.0", CultureInfo.InvariantCulture) + " s, "
                + PerSecond(since, window) + ", and " + PerSecond(done, whole) + " since the pass began";

            lastDone = done;
            lastSeconds = nowSeconds;
            return line;
        }

        private static string PerSecond(int pictures, double seconds)
        {
            return seconds > 0
                ? (pictures / seconds).ToString("0.00", CultureInfo.InvariantCulture) + " per second"
                : "UNKNOWN per second";
        }
    }
}
