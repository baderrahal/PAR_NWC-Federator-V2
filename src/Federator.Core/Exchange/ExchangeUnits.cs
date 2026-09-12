using System;
using System.Collections.Generic;
using Federator.Core.Units;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// Converting between the units an exchange file or a document names, by the codes
    /// the units attribute uses. The factors come from UnitTable, the one unit table in
    /// this repo, and nothing here holds a factor of its own. A tolerance is written in
    /// the file units, so it has to be converted before it reaches a document that
    /// measures in something else, and that happens in ClashTestPlan.Convert, the one
    /// place a file unit is judged.
    /// </summary>
    public static class ExchangeUnits
    {
        public static bool IsKnown(string units)
        {
            return UnitTable.FindByExchangeCode(units) != null;
        }

        /// <summary>
        /// One number from one unit into another. Throws on a unit the table has not been
        /// taught, rather than guessing at a factor.
        /// </summary>
        public static double Convert(double value, string fromUnits, string toUnits)
        {
            return value * FactorToMillimetres(fromUnits) / FactorToMillimetres(toUnits);
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

            UnitRow row = UnitTable.FindByExchangeCode(units);

            if (row == null)
            {
                throw new NotSupportedException(
                    "UNKNOWN exchange unit \"" + units + "\". Known units are "
                        + string.Join(", ", new List<string>(KnownUnits()).ToArray()) + ".");
            }

            return row.MillimetresPerUnit;
        }

        public static IList<string> KnownUnits()
        {
            return UnitTable.ExchangeCodes();
        }
    }
}
