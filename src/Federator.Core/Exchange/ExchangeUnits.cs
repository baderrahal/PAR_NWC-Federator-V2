using System;
using System.Collections.Generic;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// The units attribute on an exchange file, and the conversion to millimetres.
    /// A tolerance is written in the file units, so it has to be converted before it
    /// reaches a document that measures in something else.
    /// </summary>
    public static class ExchangeUnits
    {
        private static readonly Dictionary<string, double> MillimetresPerUnit =
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "mm", 1.0 },
                { "cm", 10.0 },
                { "m", 1000.0 },
                { "km", 1000000.0 },
                { "in", 25.4 },
                { "ft", 304.8 },
                { "yd", 914.4 },
                { "mi", 1609344.0 },

                // The last three have never been seen in an exchange file. They are here
                // because a tolerance is converted into the units of the OPEN DOCUMENT,
                // and Autodesk.Navisworks.Api.Units carries Micrometers, Mils and
                // Microinches, so a document set to one of them would otherwise fail to
                // convert a tolerance that is written in perfectly ordinary feet.
                { "um", 0.001 },
                { "mil", 0.0254 },
                { "uin", 0.0000254 }
            };

        public static bool IsKnown(string units)
        {
            return units != null && MillimetresPerUnit.ContainsKey(units.Trim());
        }

        public static double ToMillimetres(double value, string units)
        {
            return value * FactorToMillimetres(units);
        }

        public static double FromMillimetres(double millimetres, string units)
        {
            return millimetres / FactorToMillimetres(units);
        }

        public static double Convert(double value, string fromUnits, string toUnits)
        {
            return FromMillimetres(ToMillimetres(value, fromUnits), toUnits);
        }

        /// <summary>
        /// How many millimetres one of <paramref name="units"/> is. Throws on a unit
        /// this tool has not been taught, rather than guessing at a factor.
        /// </summary>
        public static double FactorToMillimetres(string units)
        {
            if (units == null)
            {
                throw new ArgumentNullException("units");
            }

            double factor;

            if (!MillimetresPerUnit.TryGetValue(units.Trim(), out factor))
            {
                throw new NotSupportedException(
                    "UNKNOWN exchange unit \"" + units + "\". Known units are "
                        + string.Join(", ", new List<string>(KnownUnits()).ToArray()) + ".");
            }

            return factor;
        }

        public static IList<string> KnownUnits()
        {
            List<string> units = new List<string>(MillimetresPerUnit.Keys);
            units.Sort(StringComparer.OrdinalIgnoreCase);
            return units;
        }
    }
}
