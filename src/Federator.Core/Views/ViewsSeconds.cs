using System;
using Federator.Core.Diagnostics;

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
    /// THE WHOLE IS THE STEP, from the first clash read to the document put back, on the
    /// clock handed in. The stretches, the rest no part holds and the words are
    /// SecondsByPart, shared with the IMAGES line since FR-077, and the parts and their
    /// words are this file's.
    /// </summary>
    public sealed class ViewsSeconds
    {
        private readonly SecondsByPart<ViewsPart> parts;
        private readonly double startedAt;
        private bool ended;
        private double endedAt;

        /// <summary>Starts the whole on the clock handed in, which is read for every stretch after it.</summary>
        public ViewsSeconds(Func<double> clock)
        {
            parts = new SecondsByPart<ViewsPart>(clock);
            startedAt = parts.Now;
        }

        /// <summary>
        /// Times one stretch of one part, from now until the stretch is disposed. Opened in a
        /// using block, so a call that throws still ends its stretch and the seconds it spent
        /// failing are counted, which is the rule every step keeps.
        /// </summary>
        public IDisposable In(ViewsPart part)
        {
            return parts.In(part);
        }

        /// <summary>Marks the end of the whole. The first call wins, so the whole is read once.</summary>
        public void Ended()
        {
            if (ended)
            {
                return;
            }

            ended = true;
            endedAt = parts.Now;
        }

        /// <summary>The whole, from the start to the end, or to now where it has not ended. Never less than nothing.</summary>
        internal double Total
        {
            get
            {
                double until = ended ? endedAt : parts.Now;
                return until > startedAt ? until - startedAt : 0.0;
            }
        }

        /// <summary>What no part holds. Below zero only where the parts hold more than the whole, which Line says in words.</summary>
        internal double InNoPart
        {
            get { return Total - parts.InParts; }
        }

        /// <summary>The seconds one part holds, every stretch of it added.</summary>
        internal double Of(ViewsPart part)
        {
            return parts.Of(part);
        }

        /// <summary>
        /// The one line the log carries: every part the work entered, in the order a viewpoint
        /// meets them, with its seconds, then what no part holds, then the whole.
        /// </summary>
        public string Line()
        {
            return "VIEWS    the step's seconds went: " + parts.Body(Total, Describe)
                + ", of " + SecondsByPart<ViewsPart>.Show(Total)
                + " from the first clash read to the document put back";
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
    }
}
