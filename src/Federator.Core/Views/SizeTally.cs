using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Views
{
    /// <summary>
    /// What the size rule decided across one group, and the block that says so.
    ///
    /// The count of items INCLUDED BECAUSE THEIR SIZE COULD NOT BE READ is the number this
    /// whole block exists for. A fitting usually carries no size property at all, so that
    /// number is expected to be large, and it is the one most likely to say the rule is
    /// wrong on the first run. It is never folded into the other totals and it is never
    /// left to be worked out by subtraction.
    /// </summary>
    public sealed class SizeTally
    {
        private readonly List<string> unknownNames = new List<string>();
        private int large;
        private int small;

        /// <summary>Records one item. The name is what the log will show, so it is the item's own.</summary>
        public void Add(string itemName, SizeDecision decision)
        {
            if (decision == null)
            {
                throw new ArgumentNullException("decision");
            }

            switch (decision.Verdict)
            {
                case SizeVerdict.Large:
                    large++;
                    break;

                case SizeVerdict.Small:
                    small++;
                    break;

                default:
                    unknownNames.Add(string.IsNullOrEmpty(itemName) ? "an item with no name" : itemName);
                    break;
            }
        }

        /// <summary>Over the threshold, read off a property.</summary>
        public int LargeCount
        {
            get { return large; }
        }

        /// <summary>At or under the threshold, read off a property. The only ones left out.</summary>
        public int SmallCount
        {
            get { return small; }
        }

        /// <summary>In because no size could be read. The number this block exists for.</summary>
        public int SizeUnknownCount
        {
            get { return unknownNames.Count; }
        }

        /// <summary>Everything that goes in the viewpoint, which is the large plus the unmeasurable.</summary>
        public int IncludedCount
        {
            get { return large + unknownNames.Count; }
        }

        /// <summary>Every item whose size could not be read, in the order they were judged.</summary>
        public ReadOnlyCollection<string> SizeUnknownNames
        {
            get { return new ReadOnlyCollection<string>(unknownNames); }
        }

        /// <summary>
        /// The SIZE block. The totals first, then the loud part: every item that is in
        /// because nobody could measure it.
        /// </summary>
        public IList<string> Lines(SizeSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            List<string> lines = new List<string>();

            lines.Add("SIZE     over " + settings.ThresholdMillimetres + "mm: " + large
                + ", not over: " + small
                + ", size could not be read: " + unknownNames.Count);

            lines.Add("SIZE     " + IncludedCount + " in the viewpoint, "
                + small + " left out");

            if (unknownNames.Count == 0)
            {
                lines.Add("SIZE     every item carried a size this tool could read");
                return lines;
            }

            // The loud line. It says the count and what it means, so a reader who stops
            // here still knows that these items were included rather than dropped.
            lines.Add("SIZE     " + unknownNames.Count
                + (unknownNames.Count == 1 ? " item is" : " items are")
                + " in the viewpoint because no size could be read off "
                + (unknownNames.Count == 1 ? "it" : "them")
                + ". Nothing was dropped. A fitting usually carries no size property, so this "
                + "number is expected to be large, and it is named in full below");

            bool everyOne = settings.NameEveryUnknown;
            int cap = everyOne
                ? unknownNames.Count
                : Math.Max(0, Math.Min(settings.ExamplesWhenNotNamingEvery, unknownNames.Count));

            for (int i = 0; i < cap; i++)
            {
                lines.Add("         " + unknownNames[i]);
            }

            if (cap < unknownNames.Count)
            {
                // A truncated list that does not say so is the fault this guards against.
                lines.Add("         and " + (unknownNames.Count - cap)
                    + " more, not named because naming every one is switched off");
            }

            return lines;
        }
    }
}
