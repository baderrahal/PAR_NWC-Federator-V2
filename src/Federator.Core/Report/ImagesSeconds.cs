using System;
using Federator.Core.Diagnostics;

namespace Federator.Core.Report
{
    /// <summary>
    /// Where the IMAGES step's seconds went, call by call, and how many no part holds, FR-077.
    ///
    /// WHY IT EXISTS. Set 03 spent 643.150 s of a 6292.198 s run in IMAGES over 5679
    /// pictures, 0.113 s each, and ONE WATCH SAT AROUND THE RENDER AND THE SAVE, so how much
    /// of each picture was Navisworks rendering it and how much was the JPEG going to the
    /// disk was UNKNOWN, and that is the question the speed work has to answer before it
    /// changes anything. So every call a picture is made of is a part of its own.
    ///
    /// THE WHOLE IS THE VISITS. The step is entered once per picture asked for, and the
    /// whole is those visits added, so what no part holds is the bookkeeping inside a visit
    /// and never the harvest's walk between two pictures, which is HARVEST's and is read off
    /// the timing block as HARVEST less IMAGES. The stretches, the rest and the words are
    /// SecondsByPart, shared with the VIEWS line, and the parts and their words are this
    /// file's. One line per group, written beside the IMAGES tally.
    /// </summary>
    public sealed class ImagesSeconds
    {
        private readonly SecondsByPart<ImagesPart> parts;
        private double visited;
        private int visits;

        public ImagesSeconds(Func<double> clock)
        {
            parts = new SecondsByPart<ImagesPart>(clock);
        }

        /// <summary>
        /// One entry to the step, from now until the visit is disposed, which is the whole
        /// of one picture asked for. The writer reads it for the tally's seconds per
        /// picture, so the tally and this line come off one clock.
        /// </summary>
        public ImagesVisit Visit()
        {
            visits++;
            return new ImagesVisit(parts.Counted(spent => visited += spent));
        }

        /// <summary>Times one stretch of one part. Opened in a using block, so a call that throws still ends its stretch.</summary>
        public IDisposable In(ImagesPart part)
        {
            return parts.In(part);
        }

        /// <summary>Every visit added, which is the whole.</summary>
        internal double Total
        {
            get { return visited; }
        }

        /// <summary>What no part holds. Below zero only where the parts hold more than the whole, which Line says in words.</summary>
        internal double InNoPart
        {
            get { return visited - parts.InParts; }
        }

        /// <summary>How many times the step was entered.</summary>
        internal int Visits
        {
            get { return visits; }
        }

        /// <summary>The seconds one part holds, every stretch of it added.</summary>
        internal double Of(ImagesPart part)
        {
            return parts.Of(part);
        }

        /// <summary>
        /// The one line the log carries: every part the work entered, in the order a picture
        /// meets them, with its seconds, then what no part holds, then the whole and the visits.
        /// </summary>
        public string Line()
        {
            return "IMAGES   the step's seconds went: " + parts.Body(visited, Describe)
                + ", of " + SecondsByPart<ImagesPart>.Show(visited)
                + " over " + visits + (visits == 1 ? " visit" : " visits") + " to the step";
        }

        /// <summary>The words for one part, so the line and nothing else spells them.</summary>
        internal static string Describe(ImagesPart part)
        {
            switch (part)
            {
                case ImagesPart.Rendering:
                    return "rendering";
                case ImagesPart.Saving:
                    return "saving the JPEG";
                case ImagesPart.ReadingBack:
                    return "reading the file back";
                default:
                    return "UNKNOWN";
            }
        }
    }

    /// <summary>One visit to the IMAGES step, the whole of one picture asked for.</summary>
    public sealed class ImagesVisit : IDisposable
    {
        private readonly SecondsByPart<ImagesPart>.Stretch stretch;

        internal ImagesVisit(SecondsByPart<ImagesPart>.Stretch stretch)
        {
            this.stretch = stretch;
        }

        /// <summary>How long so far, or how long it took once ended.</summary>
        public double Elapsed
        {
            get { return stretch.Elapsed; }
        }

        public void Dispose()
        {
            stretch.Dispose();
        }
    }
}
