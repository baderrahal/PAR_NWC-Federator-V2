using System;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The numbers of a long step's lines, F143, Bader's order of 2026-10-11 headed 1A04PK IS
    /// FROZEN: a PROGRESS line at least every EverySeconds during a long step, and a CALL line for
    /// one Navisworks call of LongCallSeconds or more. Both 30 by his words, since set 06's run of
    /// 1A04PK sat 97 minutes in one test's harvest with no line and the window not answering.
    /// Since F227 it also holds CountEveryTests, how often the clash step's running count is written.
    /// </summary>
    public sealed class ProgressSettings
    {
        /// <summary>
        /// At least every 30 s, Bader's words. The VIEWS step reads it too, through
        /// ViewpointSettings, where it was 60 s for FR-071 until F143.
        /// </summary>
        public const double DefaultEverySeconds = 30.0;

        /// <summary>A single call this long or longer is named with its seconds, Bader's 30.</summary>
        public const double DefaultLongCallSeconds = 30.0;

        /// <summary>
        /// The clash step's running count goes in the log after every 25 tests, and after the last, F227, Bader's
        /// answer to Q210, where it was ClashRunner's const.
        /// </summary>
        public const int DefaultCountEveryTests = 25;

        private double longCallSeconds = DefaultLongCallSeconds;

        private int countEveryTests = DefaultCountEveryTests;

        public ProgressSettings()
        {
            EverySeconds = DefaultEverySeconds;
        }

        /// <summary>Seconds between PROGRESS lines. Zero or below writes one at every piece, as ViewsProgress has.</summary>
        public double EverySeconds { get; set; }

        /// <summary>
        /// The seconds from which one call is named. Refused at nought or below, since every call
        /// would then be a CALL line, one per clash and per test on a run of 1830.
        /// </summary>
        public double LongCallSeconds
        {
            get
            {
                return longCallSeconds;
            }

            set
            {
                if (!(value > 0.0))
                {
                    throw new ArgumentOutOfRangeException("value", value, "A long call is one of more than no seconds.");
                }

                longCallSeconds = value;
            }
        }

        /// <summary>
        /// How many tests between two running counts of the clash step. Refused below one, since the count is taken
        /// every so many tests and ClashRunOutcome.ProgressAfter divides by it.
        /// </summary>
        public int CountEveryTests
        {
            get
            {
                return countEveryTests;
            }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value", value, "The running count is taken every one test or more.");
                }

                countEveryTests = value;
            }
        }
    }
}
