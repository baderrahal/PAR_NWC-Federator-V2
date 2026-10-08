using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The mechanics every line saying where a step's seconds went is made of: a stretch
    /// per call on one clock handed in, the seconds each part holds, and the words for the
    /// parts against the whole. ViewsSeconds wears it for the VIEWS step, FR-073, and
    /// ImagesSeconds for the IMAGES step, FR-077. Each owner decides what its whole is and
    /// spells its parts, so this holds neither.
    ///
    /// WHAT NO PART HOLDS IS SAID AND NEVER SPREAD. The seconds between the parts are named
    /// on the line as what none of them holds, the timing block's own rule: spreading them
    /// over the parts would be inventing numbers, and leaving them off would leave a reader
    /// adding up the numbers and wondering where the rest went. Parts adding to more than
    /// the whole, which only a stretch left open across the end can do, are said in words
    /// as a fault in this timing and never printed as a negative number of seconds.
    ///
    /// ONE CLOCK, HANDED IN. The add-in hands it the run log's monotonic clock, the one every
    /// step reads, so the parts and the step come off the same clock and nothing here keeps
    /// a Stopwatch of its own. A test drives it, so no test waits on a real second.
    ///
    /// A PART THE WORK NEVER ENTERED IS LEFT OFF THE LINE. A part listed at nothing would
    /// read as a call that was made and cost nothing. A part entered and taking no
    /// measurable time is named, because it was made.
    /// </summary>
    public sealed class SecondsByPart<TPart> where TPart : struct, Enum
    {
        /// <summary>
        /// Half a thousandth of a second, the last place the line shows. Rounding in a sum of
        /// a thousand stretches can leave the rest a hair below zero, and that is not the
        /// parts holding more than the whole, so anything smaller than the line can show is
        /// read as nothing. A guard on rounding and not a number that shapes a run.
        /// </summary>
        private const double HalfOfTheLastPlace = 0.0005;

        private readonly Func<double> clock;
        private readonly Dictionary<TPart, double> seconds = new Dictionary<TPart, double>();

        public SecondsByPart(Func<double> clock)
        {
            if (clock == null)
            {
                throw new ArgumentNullException("clock");
            }

            this.clock = clock;
        }

        /// <summary>The clock now, for an owner measuring its whole off the same clock.</summary>
        public double Now
        {
            get { return clock(); }
        }

        /// <summary>
        /// Times one stretch of one part, from now until the stretch is disposed. Opened in a
        /// using block, so a call that throws still ends its stretch and the seconds it spent
        /// failing are counted, which is the rule every step keeps.
        /// </summary>
        public Stretch In(TPart part)
        {
            return new Stretch(clock, spent => Add(part, spent));
        }

        /// <summary>A stretch counted by the owner rather than by a part, for a whole made of visits.</summary>
        public Stretch Counted(Action<double> add)
        {
            if (add == null)
            {
                throw new ArgumentNullException("add");
            }

            return new Stretch(clock, add);
        }

        /// <summary>The seconds one part holds, every stretch of it added.</summary>
        public double Of(TPart part)
        {
            double held;
            return seconds.TryGetValue(part, out held) ? held : 0.0;
        }

        /// <summary>Every part added.</summary>
        public double InParts
        {
            get
            {
                double parts = 0.0;

                foreach (double one in seconds.Values)
                {
                    parts += one;
                }

                return parts;
            }
        }

        /// <summary>
        /// The body of the line: every part the work entered, in the order the enum lists
        /// them, with its seconds, then what no part of the whole holds.
        /// </summary>
        public string Body(double whole, Func<TPart, string> describe)
        {
            if (describe == null)
            {
                throw new ArgumentNullException("describe");
            }

            List<string> parts = new List<string>();

            foreach (TPart part in Enum.GetValues(typeof(TPart)))
            {
                double held;

                if (seconds.TryGetValue(part, out held))
                {
                    parts.Add(Show(held) + " " + describe(part));
                }
            }

            double rest = whole - InParts;

            if (parts.Count == 0)
            {
                return Show(rest < 0.0 ? 0.0 : rest) + " in no part, because no part was entered";
            }

            if (rest <= -HalfOfTheLastPlace)
            {
                return string.Join(", ", parts.ToArray())
                    + ", and the parts add to " + Show(-rest)
                    + " more than the whole, which is a fault in this timing and not in the run";
            }

            return string.Join(", ", parts.ToArray())
                + ", and " + Show(rest < 0.0 ? 0.0 : rest) + " in none of these";
        }

        /// <summary>Seconds to the thousandth, the one way every seconds line shows them.</summary>
        public static string Show(double value)
        {
            return value.ToString("0.000", CultureInfo.InvariantCulture) + "s";
        }

        private void Add(TPart part, double spent)
        {
            double held;
            seconds.TryGetValue(part, out held);
            seconds[part] = held + spent;
        }

        /// <summary>One stretch on the clock. Ending it twice counts it once, and it never reports less than no time.</summary>
        public sealed class Stretch : IDisposable
        {
            private readonly Func<double> clock;
            private readonly Action<double> add;
            private readonly double from;
            private bool done;
            private double spent;

            internal Stretch(Func<double> clock, Action<double> add)
            {
                this.clock = clock;
                this.add = add;
                from = clock();
            }

            /// <summary>How long so far, or how long it took once ended.</summary>
            public double Elapsed
            {
                get
                {
                    if (done)
                    {
                        return spent;
                    }

                    double now = clock();
                    return now > from ? now - from : 0.0;
                }
            }

            public void Dispose()
            {
                if (done)
                {
                    return;
                }

                spent = Elapsed;
                done = true;
                add(spent);
            }
        }
    }
}
