using System;

namespace Federator.Core.Communities
{
    /// <summary>
    /// The numbers of a run of packages in fresh starts, F214, Bader's answer to Q204 of 18 Oct 2026: a fresh Navisworks
    /// per package, the next starting only after the last closed. Each is a setting, refused where it is set when it
    /// cannot work, and none is measured: the ceiling is chosen over the longest package read so far, the C06 and C07
    /// run of 18 Oct whose 42 groups took 60 min in one start, and the wait over the 1.55 s scan.md 5z-zj read.
    /// </summary>
    public sealed class PackageRunSettings
    {
        public const int DefaultCeilingMinutes = 240;
        public const int DefaultGoneWithinSeconds = 60;

        private int ceilingMinutes = DefaultCeilingMinutes;
        private int goneWithinSeconds = DefaultGoneWithinSeconds;

        /// <summary>
        /// How long one package's Navisworks may run before the runner ends it and goes on to the next, so a package that
        /// hangs never stops the packages after it. Refuses less than one minute.
        /// </summary>
        public int CeilingMinutes
        {
            get { return ceilingMinutes; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value", value, "a package needs at least one minute");
                }

                ceilingMinutes = value;
            }
        }

        /// <summary>
        /// How long the runner waits for a package's Navisworks to be gone after it was told to quit, before it ends it,
        /// so the next package starts only once the last has closed. Refuses less than one second.
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
