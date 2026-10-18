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

        /// <summary>
        /// A box of the file still standing after this many presses is said once, so a run that may wait for a person
        /// shows it, F227, Bader's answer to Q210, where it was AppendBoxWatch's const.
        /// </summary>
        public const int DefaultPressesBeforeSaying = 10;

        private int everyMilliseconds = DefaultEveryMilliseconds;

        private int pressesBeforeSaying = DefaultPressesBeforeSaying;

        /// <summary>The presses after which a box still standing is said. Refused below one, since no box stands after no press.</summary>
        public int PressesBeforeSaying
        {
            get
            {
                return pressesBeforeSaying;
            }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value", value,
                        "A box still standing is said after one press or more.");
                }

                pressesBeforeSaying = value;
            }
        }

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
