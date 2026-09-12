using System;
using System.Text.RegularExpressions;

namespace Federator.Core.Report
{
    /// <summary>
    /// What a cell of the client's report LOOKS like, so a check can compare shape and not
    /// only presence.
    ///
    /// The check before this one passed while the id label, the distance format and the
    /// coordinate format all differed from the samples, because presence is all it looked
    /// at. Every pattern here was read off the two exports in samples\client-report on
    /// 2026-09-01, and the examples are real cells out of them.
    /// </summary>
    public static class ClientShapes
    {
        /// <summary>A real Item ID cell out of 1104-PAR-1A04WN-XXX-BM-RPT-000001.</summary>
        public const string ExampleItemId = "Element ID: 707077";

        /// <summary>A real Clash Point cell out of the same file.</summary>
        public const string ExampleClashPoint = "x:33.399, y:8.310, z:-0.441";

        /// <summary>A real Tolerance cell out of the same file.</summary>
        public const string ExampleTolerance = "0.025m";

        /// <summary>A real Distance cell out of the same file.</summary>
        public const string ExampleDistance = "-0.116";

        private static readonly Regex ItemIdShape = new Regex(
            "^" + ClientFormat.DefaultIdLabel + @": \S", RegexOptions.Compiled);

        private static readonly Regex ClashPointShape = new Regex(
            @"^x:(-?\d+\.\d+), y:(-?\d+\.\d+), z:(-?\d+\.\d+)$", RegexOptions.Compiled);

        private static readonly Regex ToleranceShape = new Regex(
            @"^\d+\.\d{3}[A-Za-z]+$", RegexOptions.Compiled);

        /// <summary>
        /// "Element ID: 707077". The label is the one theirs uses and there is something
        /// after the colon. Ours read "Id: 990299", which is the same shape with the wrong
        /// label, so the label is part of the pattern on purpose.
        /// </summary>
        public static bool LooksLikeAnItemId(string cell)
        {
            return !string.IsNullOrEmpty(cell) && ItemIdShape.IsMatch(cell.Trim());
        }

        /// <summary>
        /// "x:33.399, y:8.310, z:-0.441". One field, three prefixes, two commas. Ours read
        /// x:33.170986, which is this shape carrying the whole double.
        /// </summary>
        public static bool LooksLikeAClashPoint(string cell)
        {
            if (string.IsNullOrEmpty(cell))
            {
                return false;
            }

            Match found = ClashPointShape.Match(cell.Trim());

            if (!found.Success)
            {
                return false;
            }

            // Each coordinate on its own, because the shape is not just three numbers with
            // commas between them. Ours read x:33.170986 while every column was present,
            // which is exactly what a presence check misses.
            for (int i = 1; i <= 3; i++)
            {
                if (!LooksLikeAMeasurement(found.Groups[i].Value))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// One distance or one coordinate, written the way theirs are.
        ///
        /// Three decimals, or, for a value too small for three decimals, more of them.
        /// Anything else carries precision theirs does not, which is what ours did.
        /// </summary>
        public static bool LooksLikeAMeasurement(string value)
        {
            if (string.IsNullOrEmpty(value) || !AnyDecimals.IsMatch(value.Trim()))
            {
                return false;
            }

            string tidied = value.Trim();
            int decimals = tidied.Length - tidied.IndexOf('.') - 1;

            if (decimals == 3)
            {
                return true;
            }

            // More than three is only right where three would have read as zero.
            double number;

            if (decimals < 3 || !double.TryParse(tidied,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out number))
            {
                return false;
            }

            return ClientFormat.Fixed(number) == tidied;
        }

        private static readonly Regex AnyDecimals = new Regex(
            @"^-?\d+\.\d+$", RegexOptions.Compiled);

        /// <summary>"0.025m", three decimals then the unit with no space.</summary>
        public static bool LooksLikeATolerance(string cell)
        {
            return !string.IsNullOrEmpty(cell) && ToleranceShape.IsMatch(cell.Trim());
        }

        /// <summary>
        /// "-0.116". Three decimals, which is what all 129 distances in the two exports
        /// carry. A value too small for three decimals is a separate case and is not a
        /// distance any of theirs holds.
        /// </summary>
        public static bool LooksLikeADistance(string cell)
        {
            return LooksLikeAMeasurement(cell);
        }
    }
}
