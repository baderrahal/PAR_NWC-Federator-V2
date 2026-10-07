using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Views
{
    /// <summary>
    /// The red, the green and the solid items of one view of many clashes, F114, Q114 point 13,
    /// red and green as Clash Detective paints them and as F85 painted one clash.
    /// </summary>
    public sealed class PaintPlan
    {
        private readonly List<ItemPath> red = new List<ItemPath>();
        private readonly List<ItemPath> green = new List<ItemPath>();

        private PaintPlan()
        {
        }

        /// <summary>Every first item, each once.</summary>
        public ReadOnlyCollection<ItemPath> Red
        {
            get { return new ReadOnlyCollection<ItemPath>(red); }
        }

        /// <summary>Every second item that is not already red, each once.</summary>
        public ReadOnlyCollection<ItemPath> Green
        {
            get { return new ReadOnlyCollection<ItemPath>(green); }
        }

        /// <summary>Every clashing item, kept solid while the rest of the shown models are dimmed.</summary>
        public ReadOnlyCollection<ItemPath> Solid
        {
            get
            {
                List<ItemPath> all = new List<ItemPath>(red);
                all.AddRange(green);
                return new ReadOnlyCollection<ItemPath>(all);
            }
        }

        /// <summary>The items first in one clash and second in another, painted red and counted.</summary>
        public int FirstAndSecond { get; private set; }

        /// <summary>The clashes with an item that could not be pointed at, so not painted.</summary>
        public int NotPointedAt { get; private set; }

        /// <summary>The paint of a view of those clashes.</summary>
        public static PaintPlan For(IEnumerable<ViewClash> clashes)
        {
            PaintPlan paint = new PaintPlan();

            if (clashes == null)
            {
                return paint;
            }

            HashSet<ItemPath> firsts = new HashSet<ItemPath>();
            HashSet<ItemPath> seconds = new HashSet<ItemPath>();
            List<ItemPath> secondsInOrder = new List<ItemPath>();

            foreach (ViewClash clash in clashes)
            {
                if (clash == null)
                {
                    continue;
                }

                if (clash.FirstItem == null || clash.SecondItem == null)
                {
                    paint.NotPointedAt = paint.NotPointedAt + 1;
                }

                if (clash.FirstItem != null && firsts.Add(clash.FirstItem))
                {
                    paint.red.Add(clash.FirstItem);
                }

                if (clash.SecondItem != null && seconds.Add(clash.SecondItem))
                {
                    secondsInOrder.Add(clash.SecondItem);
                }
            }

            foreach (ItemPath item in secondsInOrder)
            {
                if (firsts.Contains(item))
                {
                    paint.FirstAndSecond = paint.FirstAndSecond + 1;
                }
                else
                {
                    paint.green.Add(item);
                }
            }

            return paint;
        }
    }
}
