using System;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The two numbers of a long step's lines, F143, Bader's order of 2026-10-11 headed 1A04PK IS
    /// FROZEN: a PROGRESS line at least every EverySeconds during a long step, and a CALL line for
    /// one Navisworks call of LongCallSeconds or more. Both 30 by his words, since set 06's run of
    /// 1A04PK sat 97 minutes in one test's harvest with no line and the window not answering.
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

        private double longCallSeconds = DefaultLongCallSeconds;

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
    }
}
