using System;
using System.Collections.Generic;
using System.Globalization;
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

    /// <summary>One item, judged.</summary>
    public sealed class SizeDecision
    {
        internal SizeDecision(SizeVerdict verdict, string matchedProperty, double? millimetres, string reason)
        {
            Verdict = verdict;
            MatchedProperty = matchedProperty;
            Millimetres = millimetres;
            Reason = reason;
        }

        public SizeVerdict Verdict { get; private set; }

        /// <summary>
        /// Whether it goes in the viewpoint. Large and SizeUnknown both do, which is the
        /// whole point of the rule: nothing disappears because nobody could measure it.
        /// </summary>
        public bool Included
        {
            get { return Verdict != SizeVerdict.Small; }
        }

        /// <summary>Which property supplied the size, or null where none did.</summary>
        public string MatchedProperty { get; private set; }

        /// <summary>The size in millimetres, or null where none could be read.</summary>
        public double? Millimetres { get; private set; }

        /// <summary>Why, in the words a person would use.</summary>
        public string Reason { get; private set; }
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
        /// Decides one item from the properties read off it. The lookup is property name to
        /// value IN THE DOCUMENT'S UNITS, and the unit is the Navisworks enum name of those
        /// units, which is what UnitTable keys on.
        ///
        /// Throws where the unit is not one the table knows, because carrying on would mean
        /// guessing the factor, and a viewpoint built on a guessed factor holds the wrong
        /// items and looks right.
        /// </summary>
        public static SizeDecision Decide(
            IDictionary<string, double> read, string unitEnumName, SizeSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            UnitRow unit = UnitTable.ByEnumName(unitEnumName);

            if (read == null || read.Count == 0 || settings.PropertyNames == null)
            {
                return new SizeDecision(
                    SizeVerdict.SizeUnknown, null, null,
                    "no size property could be read off it, so it is IN");
            }

            for (int i = 0; i < settings.PropertyNames.Count; i++)
            {
                string name = settings.PropertyNames[i];

                if (string.IsNullOrEmpty(name) || !read.ContainsKey(name))
                {
                    continue;
                }

                double inDocumentUnits = read[name];
                double millimetres = inDocumentUnits * unit.MillimetresPerUnit;

                string measured = name + " " + Round(inDocumentUnits) + unit.Short
                    + ", which is " + Round(millimetres) + "mm";

                if (millimetres > settings.ThresholdMillimetres)
                {
                    return new SizeDecision(
                        SizeVerdict.Large, name, millimetres,
                        measured + ", over " + Round(settings.ThresholdMillimetres) + "mm, so it is IN");
                }

                return new SizeDecision(
                    SizeVerdict.Small, name, millimetres,
                    measured + ", not over " + Round(settings.ThresholdMillimetres) + "mm, so it is OUT");
            }

            return new SizeDecision(
                SizeVerdict.SizeUnknown, null, null,
                "none of " + string.Join(", ", Names(settings)) + " is on it, so it is IN");
        }

        /// <summary>
        /// The LARGEST of every size property read off one item, in millimetres, or null
        /// where none could be read. F72.
        ///
        /// A DUCT IS NOT ONE NUMBER, which is why this exists beside Decide rather than
        /// inside it. Decide takes the FIRST property on the settings list that is there,
        /// which is what a viewpoint wants: one representative size, cheaply, in a rule
        /// nobody has to argue about. A penetration wants the opposite. A 600 by 150 duct
        /// carries Width 600 and Height 150, and asking whether it fits through a wall as
        /// a small service has to read the 600, because that is what has to fit. Taking
        /// the first would read whichever of the two the settings list happens to name
        /// earlier, which is Width today and would be Height if anybody reordered the
        /// list, and the answer would change with it.
        ///
        /// The conversion is HERE and not in the caller, for the reason written above
        /// Decide: what a property hands back is in the document's units, and a raw double
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

        private static string[] Names(SizeSettings settings)
        {
            return new List<string>(settings.PropertyNames).ToArray();
        }

        /// <summary>
        /// Three decimals with the trailing zeros dropped, which is how a size reads in a
        /// sentence rather than in a report column.
        /// </summary>
        private static string Round(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
