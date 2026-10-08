using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Exchange;

namespace Federator.Core.Clash
{
    /// <summary>One side of a test as the file wrote it, before any model is involved.</summary>
    public sealed class PlannedClashSide
    {
        internal PlannedClashSide(bool selfIntersect, int primitiveTypes, string locator)
        {
            SelfIntersect = selfIntersect;
            PrimitiveTypes = primitiveTypes;
            Locator = locator;
        }

        public bool SelfIntersect { get; private set; }

        /// <summary>
        /// The primtypes flag exactly as the file wrote it. Autodesk.Navisworks.Api.PrimitiveTypes
        /// is a Flags enum over int whose bits are these numbers, so 1 is Triangles.
        /// Kept as an int here because Core never references the Navisworks API.
        /// </summary>
        public int PrimitiveTypes { get; private set; }

        /// <summary>The set path this side names, for example lcop_selection_set_tree/A/B/Name.</summary>
        public string Locator { get; private set; }

        /// <summary>
        /// Whether the set this side names was counted, and how many items it held. A side with no locator, or a
        /// locator nobody counted, is not counted, and the number is then nought, which is not a count of nothing.
        /// </summary>
        internal static bool Counted(IDictionary<string, int> itemsByLocator, PlannedClashSide side, out int items)
        {
            items = 0;

            if (itemsByLocator == null || side == null || string.IsNullOrEmpty(side.Locator))
            {
                return false;
            }

            return itemsByLocator.TryGetValue(side.Locator, out items);
        }

        public override string ToString()
        {
            return Locator;
        }
    }
}
