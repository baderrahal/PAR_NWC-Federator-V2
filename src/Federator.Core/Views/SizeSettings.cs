using System;
using System.Collections.Generic;

namespace Federator.Core.Views
{
    /// <summary>
    /// Which items are big enough to go in a viewpoint, and where their size is read from.
    ///
    /// Pipes, ducts, cable trays and their fittings over 150 mm are in. Smaller ones are
    /// out. Every number here is a setting and none of them is a constant, which is the
    /// rule for every number that shapes a run.
    /// </summary>
    public sealed class SizeSettings
    {
        /// <summary>
        /// 150 mm. The threshold Bader gave, in millimetres, because a threshold in the
        /// document's units would mean a different rule per document.
        ///
        /// ONE NUMBER, READ TWO WAYS, AND THIS IS THE ONLY PLACE EITHER OF THEM IS WRITTEN.
        /// Two features turn on it and they read it in OPPOSITE directions, so a second
        /// copy of 150 anywhere would drift and nobody would notice until a report was
        /// wrong.
        ///
        ///   F53, the viewpoints. An item OVER the threshold goes in. Exactly 150 is OUT,
        ///   because over 150 is what was asked for and 150 itself is not over it.
        ///
        ///   F72, the penetrations. A service AT OR UNDER the threshold becomes Reviewed.
        ///   Exactly 150 is IN, because 150 or less is what was asked for. A service over
        ///   it stays New, because a large service through a wall is a real coordination
        ///   item and not a penetration somebody should stop looking at.
        ///
        /// So exactly 150 falls on a different side in each, which is not a contradiction:
        /// one rule is over and the other is at or under, and together they cover every
        /// size with no gap and no overlap.
        /// </summary>
        public const double DefaultThresholdMillimetres = 150.0;

        /// <summary>
        /// Where a size is looked for, in order, first one found wins.
        ///
        /// SIX AND NOT ONE, because which property carries the size differs per kind and
        /// per exporter. A round duct has a Diameter, a rectangular one has Width and
        /// Height, a cable tray usually has Width, and some exporters write only Size or
        /// Overall Size. Reading one name would silently drop every item that names it
        /// something else.
        /// </summary>
        public static readonly string[] DefaultPropertyNames =
        {
            "Diameter",
            "Width",
            "Height",
            "Size",
            "Nominal Diameter",
            "Overall Size"
        };

        public SizeSettings()
        {
            ThresholdMillimetres = DefaultThresholdMillimetres;
            PropertyNames = new List<string>(DefaultPropertyNames);
            NameEveryUnknown = true;
            ExamplesWhenNotNamingEvery = 5;
        }

        /// <summary>
        /// Over this, in millimetres, and the item is in. Exactly this is OUT, because
        /// over 150 is what was asked for and 150 itself is not over it.
        /// </summary>
        public double ThresholdMillimetres { get; set; }

        /// <summary>The property names a size is looked for under, in order.</summary>
        public IList<string> PropertyNames { get; set; }

        /// <summary>
        /// Whether every item whose size could not be read is named in the log.
        ///
        /// TRUE by default, and this is a deliberate departure from the rule that says to
        /// log a count and five examples. That rule is about many lines saying ONE thing,
        /// which is what made a run write 1830 near identical SKIPPED lines. These lines
        /// each say a DIFFERENT thing: every one names an item that may be wrongly in or
        /// out of a viewpoint, and reading five of them tells you nothing about the sixth.
        ///
        /// A project drowning in them can turn it off, and the block then SAYS it
        /// truncated, because a truncated list that does not say so is the fault both
        /// rules exist to prevent.
        /// </summary>
        public bool NameEveryUnknown { get; set; }

        /// <summary>How many are named when NameEveryUnknown is off.</summary>
        public int ExamplesWhenNotNamingEvery { get; set; }
    }
}
