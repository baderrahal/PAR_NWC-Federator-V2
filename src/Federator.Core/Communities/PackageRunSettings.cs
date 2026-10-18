using System;

namespace Federator.Core.Communities
{
    /// <summary>
    /// The numbers of a run of packages in fresh starts, F214, Bader's answer to Q204 of 18 Oct 2026: a fresh Navisworks
    /// per package, since F222 per group, the next starting only after the last closed. Each is a setting, refused where it is set when it
    /// cannot work, and none is measured: the ceiling is chosen over the longest package read so far, the C06 and C07
    /// run of 18 Oct whose 42 groups took 60 min in one start, and the wait over the 1.55 s scan.md 5z-zj read.
    /// </summary>
    public sealed class PackageRunSettings
    {
        public const int DefaultCeilingMinutes = 240;
        public const int DefaultGoneWithinSeconds = 60;
        public const int DefaultPollMilliseconds = 200;

        private int ceilingMinutes = DefaultCeilingMinutes;
        private int goneWithinSeconds = DefaultGoneWithinSeconds;
        private int pollMilliseconds = DefaultPollMilliseconds;

        /// <summary>
        /// How often the window answers and reads the runner's status while it waits, and how often the runner looks for a
        /// Navisworks to be gone, the 200 ms the probe's parent pumped at. Refuses less than 50.
        /// </summary>
        public int PollMilliseconds
        {
            get { return pollMilliseconds; }
            set
            {
                if (value < 50)
                {
                    throw new ArgumentOutOfRangeException("value", value, "polling more often than every 50 ms is refused");
                }

                pollMilliseconds = value;
            }
        }

        /// <summary>
        /// How long the window waits on the runner before it ends it, every group's ceiling and wait added up and a minute
        /// more each, so a runner that can no longer end its own Navisworks never keeps the window waiting for ever.
        /// </summary>
        public int RunnerLimitMinutes(int groups)
        {
            return Math.Max(1, groups) * (ceilingMinutes + (goneWithinSeconds + 59) / 60 + 1);
        }

        /// <summary>
        /// How long one group's Navisworks may run before the runner ends it and goes on to the next, so a group that
        /// hangs never stops the groups after it. Refuses less than one minute.
        /// </summary>
        public int CeilingMinutes
        {
            get { return ceilingMinutes; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value", value, "a group needs at least one minute");
                }

                ceilingMinutes = value;
            }
        }

        /// <summary>
        /// How long the runner waits for a group's Navisworks to be gone after it was told to quit, before it ends it,
        /// so the next group starts only once the last has closed. Refuses less than one second.
        /// </summary>
        public int GoneWithinSeconds
        {
            get { return goneWithinSeconds; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value", value, "the wait needs at least one second");
                }

                goneWithinSeconds = value;
            }
        }
    }
}
