using System;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// How often the windows of the process are read while a file is appended, F213. 200 ms is what the probe of
    /// scan.md 5z-zi read at, and it saw the box 198 to 217 ms after the call began.
    /// </summary>
    public sealed class AppendBoxSettings
    {
        public const int DefaultEveryMilliseconds = 200;

        /// <summary>The least the setting takes, since a timer that fires faster reads the windows for nothing.</summary>
        public const int LeastEveryMilliseconds = 50;

        private int everyMilliseconds = DefaultEveryMilliseconds;

        public int EveryMilliseconds
        {
            get
            {
                return everyMilliseconds;
            }

            set
            {
                if (value < LeastEveryMilliseconds)
                {
                    throw new ArgumentOutOfRangeException("value", value,
                        "The windows are read at most every " + LeastEveryMilliseconds + " ms while a file is appended.");
                }

                everyMilliseconds = value;
            }
        }
    }
}
