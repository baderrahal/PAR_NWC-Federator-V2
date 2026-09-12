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

        public override string ToString()
        {
            return Locator;
        }
    }
}
