using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Views
{
    /// <summary>
    /// Where the VIEWS step's seconds went, call by call, and how many no part holds, FR-073.
    ///
    /// WHY IT EXISTS. Set 03 spent 4523.564 s of a 6292.198 s run in VIEWS, and the line that
    /// said where went four ways: looking whether each was already there, dimming, recording
    /// and reading back. RECORDING WAS ONE WATCH AROUND THREE CALLS, the folders, the COM
    /// folder and the COM add, so which of them cost 1602.798 s of 1B06G1's 1769.389 s was
    /// UNKNOWN, and that is the question the speed work has to answer before it changes
    /// anything. And the four left 67.5 s of 1B06G1 and 131.9 s of 1B06PP in none of them,
    /// because reading the clashes, showing and hiding the models and the live line ran
    /// outside every watch. So every call the viewpoint work is made of is a part of its own.
    ///
    /// WHAT NO PART HOLDS IS SAID AND NEVER SPREAD. The seconds between the parts are named
    /// on the line as what none of them holds, the timing block's own rule: spreading them
    /// over the parts would be inventing numbers, and leaving them off would leave a reader
    /// adding up four numbers and wondering where the rest went. Parts adding to more than
    /// the whole, which only a stretch left open across the end can do, are said in words
    /// as a fault in this timing and never printed as a negative number of seconds.
    ///
    /// ONE CLOCK, HANDED IN. The add-in hands it the run log's monotonic clock, the one every
    /// step reads, so the parts and the step come off the same clock and nothing here keeps
    /// a Stopwatch of its own. A test drives it, so no test waits on a real second.
    ///
    /// A PART THE WORK NEVER ENTERED IS LEFT OFF THE LINE. The route the run takes by
    /// default never adds at the root and moves, and a part listed at nothing would read as
    /// a call that was made and cost nothing. A part entered and taking no measurable time
    /// is named, because it was made.
    /// </summary>
    public sealed class ViewsSeconds
    {
        private readonly Func<double> clock;
        private readonly double startedAt;
        private readonly Dictionary<ViewsPart, double> seconds = new Dictionary<ViewsPart, double>();
        private bool ended;
        private double endedAt;

        /// <summary>Starts the whole on the clock handed in, which is read for every stretch after it.</summary>
        public ViewsSeconds(Func<double> clock)
        {
            if (clock == null)
            {
                throw new ArgumentNullException("clock");
            }

            this.clock = clock;
            startedAt = clock();
        }

        /// <summary>
        /// Times one stretch of one part, from now until the stretch is disposed. Opened in a
        /// using block, so a call that throws still ends its stretch and the seconds it spent
        /// failing are counted, which is the rule every step keeps.
        /// </summary>
        public IDisposable In(ViewsPart part)
        {
            return new Stretch(this, part, clock());
        }

        /// <summary>Marks the end of the whole. The first call wins, so the whole is read once.</summary>
        public void Ended()
        {
            if (ended)
            {
                return;
            }

            ended = true;
            endedAt = clock();
        }

        /// <summary>The whole, from the start to the end, or to now where it has not ended. Never less than nothing.</summary>
        internal double Total
        {
            get
            {
                double until = ended ? endedAt : clock();
                return until > startedAt ? until - startedAt : 0.0;
            }
        }

        /// <summary>What no part holds. Below zero only where the parts hold more than the whole, which Line says in words.</summary>
        internal double InNoPart
        {
            get
            {
                double parts = 0.0;

                foreach (double one in seconds.Values)
                {
                    parts += one;
                }

                return Total - parts;
            }
        }

        /// <summary>The seconds one part holds, every stretch of it added.</summary>
        internal double Of(ViewsPart part)
        {
            double held;
            return seconds.TryGetValue(part, out held) ? held : 0.0;
        }

        /// <summary>
        /// The one line the log carries: every part the work entered, in the order a viewpoint
        /// meets them, with its seconds, then what no part holds, then the whole.
        /// </summary>
        public string Line()
        {
            List<string> parts = new List<string>();

            foreach (ViewsPart part in Enum.GetValues(typeof(ViewsPart)))
            {
                double held;

                if (seconds.TryGetValue(part, out held))
                {
                    parts.Add(Show(held) + " " + Describe(part));
                }
            }

            double rest = InNoPart;
            string body;

            if (parts.Count == 0)
            {
                body = Show(rest) + " in no part, because no part was entered";
            }
            else if (rest <= -HalfOfTheLastPlace)
            {
                body = string.Join(", ", parts.ToArray())
                    + ", and the parts add to " + Show(-rest)
                    + " more than the whole, which is a fault in this timing and not in the run";
            }
            else
            {
                body = string.Join(", ", parts.ToArray())
                    + ", and " + Show(rest < 0.0 ? 0.0 : rest) + " in none of these";
            }

            return "VIEWS    the step's seconds went: " + body
                + ", of " + Show(Total) + " from the first clash read to the document put back";
        }

        /// <summary>The words for one part, so the line and nothing else spells them.</summary>
        internal static string Describe(ViewsPart part)
        {
            switch (part)
            {
                case ViewsPart.ReadingTheClashes:
                    return "reading the clashes and planning";
                case ViewsPart.SayingHowFar:
                    return "saying how far it has got";
                case ViewsPart.LookingWhetherThere:
                    return "looking whether each was already there";
                case ViewsPart.ShowingAndHiding:
                    return "showing and hiding the models";
                case ViewsPart.Dimming:
                    return "dimming";
                case ViewsPart.Framing:
                    return "framing the camera on the clashes";
                case ViewsPart.MakingTheFolders:
                    return "finding or making the folders";
                case ViewsPart.MakingTheView:
                    return "making the view and setting its camera";
                case ViewsPart.FindingTheFolder:
                    return "finding its folder again in the COM tree";
                case ViewsPart.AddingTheView:
                    return "adding the view";
                case ViewsPart.MovingIntoTheFolder:
                    return "copying it from the root into its folder and removing the root one";
                case ViewsPart.Marking:
                    return "marking the view and its folders";
                case ViewsPart.ReadingBack:
                    return "reading back";
                case ViewsPart.TakingTheInventory:
                    return "taking the inventory of the tree";
                case ViewsPart.Removing:
                    return "removing the views of earlier runs";
                case ViewsPart.PuttingBack:
                    return "putting the document back";
                case ViewsPart.ReadingTheTree:
                    return "reading the tree for the VIEWS TREE block";
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>
        /// Half a thousandth of a second, the last place the line shows. Rounding in a sum of
        /// a thousand stretches can leave the rest a hair below zero, and that is not the
        /// parts holding more than the whole, so anything smaller than the line can show is
        /// read as nothing. A guard on rounding and not a number that shapes a run.
        /// </summary>
        private const double HalfOfTheLastPlace = 0.0005;

        private void Add(ViewsPart part, double from, double to)
        {
            double spent = to > from ? to - from : 0.0;
            double held;
            seconds.TryGetValue(part, out held);
            seconds[part] = held + spent;
        }

        private static string Show(double value)
        {
            return value.ToString("0.000", CultureInfo.InvariantCulture) + "s";
        }

        /// <summary>One stretch of one part. Ending it twice counts it once.</summary>
        private sealed class Stretch : IDisposable
        {
            private readonly ViewsSeconds owner;
            private readonly ViewsPart part;
            private readonly double from;
            private bool done;

            internal Stretch(ViewsSeconds owner, ViewsPart part, double from)
            {
                this.owner = owner;
                this.part = part;
                this.from = from;
            }

            public void Dispose()
            {
                if (done)
                {
                    return;
                }

                done = true;
                owner.Add(part, from, owner.clock());
            }
        }
    }
}
