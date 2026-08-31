using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Report
{
    /// <summary>
    /// What the images actually cost, per group.
    ///
    /// This exists because every number anyone has given for rendering a clash image,
    /// mine included, has been a guess. Seconds per image, total seconds, how many, and
    /// how many megabytes are all measured off the run and reported, so the next decision
    /// about caps and sizes is made on numbers.
    ///
    /// Nothing in here throws. A tally that goes wrong must never be the reason a run
    /// stops.
    /// </summary>
    public sealed class ImageTally
    {
        private readonly List<string> failures = new List<string>();
        private double seconds;
        private long bytes;
        private int written;
        private int skippedByCap;
        private int skippedByStatus;
        private double slowest;
        private double quickest = double.MaxValue;

        /// <summary>How many pictures were written.</summary>
        public int Written
        {
            get { return written; }
        }

        /// <summary>How many failed. A failure leaves its cell empty and never stops the run.</summary>
        public int Failed
        {
            get { return failures.Count; }
        }

        /// <summary>Clashes passed over because the test had hit its cap.</summary>
        public int SkippedByCap
        {
            get { return skippedByCap; }
        }

        /// <summary>Clashes passed over because their status is not one that gets a picture.</summary>
        public int SkippedByStatus
        {
            get { return skippedByStatus; }
        }

        public double TotalSeconds
        {
            get { return seconds; }
        }

        public long TotalBytes
        {
            get { return bytes; }
        }

        public double TotalMegabytes
        {
            get { return bytes / 1048576.0; }
        }

        /// <summary>Seconds per image, or zero before anything has been written.</summary>
        public double SecondsEach
        {
            get { return written == 0 ? 0.0 : seconds / written; }
        }

        public double Slowest
        {
            get { return slowest; }
        }

        public double Quickest
        {
            get { return written == 0 ? 0.0 : quickest; }
        }

        /// <summary>The names of the clashes whose picture failed, for the log.</summary>
        public IList<string> Failures
        {
            get { return failures.AsReadOnly(); }
        }

        /// <summary>
        /// One picture written. The size is read back off the disk by the caller, never
        /// estimated, so a zero here means a zero byte file and not a missing measurement.
        /// </summary>
        public void Wrote(double tookSeconds, long sizeInBytes)
        {
            written++;
            seconds += tookSeconds;
            bytes += sizeInBytes < 0 ? 0 : sizeInBytes;

            if (tookSeconds > slowest)
            {
                slowest = tookSeconds;
            }

            if (tookSeconds < quickest)
            {
                quickest = tookSeconds;
            }
        }

        /// <summary>
        /// One picture failed. The time it wasted still counts, because a run where every
        /// image fails slowly is exactly the run worth measuring.
        /// </summary>
        public void RenderFailed(string clashName, double tookSeconds)
        {
            failures.Add(string.IsNullOrEmpty(clashName) ? "unnamed clash" : clashName);
            seconds += tookSeconds;
        }

        public void CappedOne()
        {
            skippedByCap++;
        }

        public void WrongStatus()
        {
            skippedByStatus++;
        }

        public void Add(ImageTally other)
        {
            if (other == null)
            {
                return;
            }

            written += other.written;
            seconds += other.seconds;
            bytes += other.bytes;
            skippedByCap += other.skippedByCap;
            skippedByStatus += other.skippedByStatus;
            failures.AddRange(other.failures);

            if (other.slowest > slowest)
            {
                slowest = other.slowest;
            }

            if (other.written > 0 && other.quickest < quickest)
            {
                quickest = other.quickest;
            }
        }

        /// <summary>
        /// The block that goes in the log. Every number here was measured, so where
        /// nothing was written it says so rather than printing a rate over zero.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (written == 0 && Failed == 0)
            {
                lines.Add("IMAGES   none written.");

                if (skippedByStatus > 0 || skippedByCap > 0)
                {
                    lines.Add("         " + skippedByStatus + " passed over on status, "
                        + skippedByCap + " on the cap.");
                }

                return lines;
            }

            lines.Add("IMAGES   " + written + " written, "
                + Two(TotalMegabytes) + " MB, " + One(seconds) + " seconds in total.");

            if (written > 0)
            {
                lines.Add("         " + Three(SecondsEach) + " seconds each on average, "
                    + "quickest " + Three(Quickest) + ", slowest " + Three(slowest) + ".");
                lines.Add("         " + Kb() + " KB each on average.");
            }

            if (skippedByStatus > 0 || skippedByCap > 0)
            {
                lines.Add("         " + skippedByStatus + " passed over on status, "
                    + skippedByCap + " on the cap.");
            }

            if (Failed > 0)
            {
                lines.Add("         " + Failed + " failed. Their cells are empty and the run "
                    + "carried on.");

                int show = Failed < 5 ? Failed : 5;

                for (int i = 0; i < show; i++)
                {
                    lines.Add("           " + failures[i]);
                }

                if (Failed > show)
                {
                    lines.Add("           and " + (Failed - show) + " more.");
                }
            }

            return lines;
        }

        private string Kb()
        {
            return written == 0 ? "0" : Zero(bytes / 1024.0 / written);
        }

        private static string Zero(double value)
        {
            return value.ToString("0", CultureInfo.InvariantCulture);
        }

        private static string One(double value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture);
        }

        private static string Two(double value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static string Three(double value)
        {
            return value.ToString("0.000", CultureInfo.InvariantCulture);
        }
    }
}
