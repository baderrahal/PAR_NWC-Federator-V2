using System;
using System.Collections.Generic;
using Federator.Core.Units;

namespace Federator.Core.Views
{
    /// <summary>What the rule decided about one item.</summary>
    public enum SizeVerdict
    {
        /// <summary>Over the threshold. In.</summary>
        Large = 0,

        /// <summary>At or under the threshold. Out.</summary>
        Small = 1,

        /// <summary>No size could be read at all. In, and said loudly.</summary>
        SizeUnknown = 2
    }

    /// <summary>
    /// Whether one item is big enough to go in a viewpoint.
    ///
    /// THE NUMBER IS NEVER COMPARED RAW. What a property hands back is in the document's
    /// units, so it goes through UnitTable before anything is compared. A document in feet
    /// reporting 0.5 is 152.4 mm and is IN, and comparing 0.5 against 150 would put it out
    /// while the same model in millimetres put it in. The same building would then produce
    /// two different sets of viewpoints depending on a setting nobody changed.
    ///
    /// A unit the table does not know FAILS rather than falling back, which is the rule
    /// F33 set and the one ReportUnits already follows: a number in the wrong unit reads
    /// as real and is not.
    ///
    /// No Navisworks type reaches this. The add-in reads the properties and hands in what
    /// it read, and makes no decision of its own.
    /// </summary>
    public static class SizeRule
    {
        /// <summary>
        /// The verdict for a size already reduced to millimetres, F85. The LARGEST size a
        /// clash side carries goes through here, because the views read a side the way F72a
        /// does, Q51. Over the threshold is Large, at or under is Small, exactly the
        /// threshold is Small, and no size at all is SizeUnknown, which stays in its view and
        /// is named. F53's reading of the FIRST property, Decide, had no caller in src and
        /// went with its tests in F114.
        /// </summary>
        public static SizeVerdict VerdictFor(double? millimetres, SizeSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (!millimetres.HasValue)
            {
                return SizeVerdict.SizeUnknown;
            }

            return millimetres.Value > settings.ThresholdMillimetres ? SizeVerdict.Large : SizeVerdict.Small;
        }

        /// <summary>
        /// The LARGEST of every size property read off one item, in millimetres, or null
        /// where none could be read. F72.
        ///
        /// A DUCT IS NOT ONE NUMBER. A 600 by 150 duct carries Width 600 and Height 150, and
        /// asking whether it fits through a wall as a small service, or whether it goes under
        /// Over 150mm, has to read the 600, because that is what has to fit. Taking the first
        /// would read whichever of the two the settings list happens to name earlier, which is
        /// Width today and would be Height if anybody reordered the list, and the answer would
        /// change with it.
        ///
        /// The conversion is HERE and not in the caller, for the reason written above the
        /// class: what a property hands back is in the document's units, and a raw double
        /// compared against 150 gives one building two different answers depending on a
        /// setting nobody changed. A unit the table does not know throws, the same way.
        /// </summary>
        public static double? LargestMillimetres(
            IDictionary<string, double> read, string unitEnumName, SizeSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            UnitRow unit = UnitTable.ByEnumName(unitEnumName);

            if (read == null || read.Count == 0 || settings.PropertyNames == null)
            {
                return null;
            }

            double? largest = null;

            // Every WANTED property, and never every property the item happens to carry.
            // The settings list is what this tool calls a size, so a value under some
            // other name is not one, whatever its number.
            for (int i = 0; i < settings.PropertyNames.Count; i++)
            {
                string name = settings.PropertyNames[i];

                if (string.IsNullOrEmpty(name) || !read.ContainsKey(name))
                {
                    continue;
                }

                double millimetres = read[name] * unit.MillimetresPerUnit;

                if (!largest.HasValue || millimetres > largest.Value)
                {
                    largest = millimetres;
                }
            }

            return largest;
        }

        /// <summary>Which property carried the largest size, or null where none did.</summary>
        public static string LargestProperty(
            IDictionary<string, double> read, string unitEnumName, SizeSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            UnitRow unit = UnitTable.ByEnumName(unitEnumName);

            if (read == null || read.Count == 0 || settings.PropertyNames == null)
            {
                return null;
            }

            string which = null;
            double largest = 0.0;

            for (int i = 0; i < settings.PropertyNames.Count; i++)
            {
                string name = settings.PropertyNames[i];

                if (string.IsNullOrEmpty(name) || !read.ContainsKey(name))
                {
                    continue;
                }

                double millimetres = read[name] * unit.MillimetresPerUnit;

                if (which == null || millimetres > largest)
                {
                    which = name;
                    largest = millimetres;
                }
            }

            return which;
        }
    }
}
