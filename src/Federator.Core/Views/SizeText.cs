using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Units;

namespace Federator.Core.Views
{
    /// <summary>
    /// A size written as WORDS rather than as a number, read into millimetres, 5s.
    ///
    /// WHY THIS EXISTS. `ItemSizes` takes a size property only where its kind is
    /// DoubleLength or Double, and on 2026-09-20 that left 29 of 74 services in one
    /// group reported as having NO READABLE SIZE while every one of them carried a size
    /// the whole time. Revit writes a conduit's Size as the DisplayString "53 mmø" and a
    /// cable tray fitting's as "600 mmx100 mm-600 mmx100 mm", so the property was there,
    /// the number was in it, and the reader threw it away for being a string. That is
    /// the same shape as 5r: a reader returning nothing and looking like an answer.
    ///
    /// WHAT IT READS. Every number in the text that is FOLLOWED BY A UNIT this tool
    /// knows, converted through UnitTable, which is the one unit table in this repo.
    /// "600 mmx100 mm" is two measurements, 600 and 100, and the caller takes the
    /// largest, because a 600 by 100 tray has to fit a 600 through the wall.
    ///
    /// A NUMBER WITH NO UNIT IS REFUSED AND NEVER GUESSED AT. "300x300" carries no unit,
    /// so whether it is millimetres or inches is UNKNOWN, and this tool says UNKNOWN
    /// rather than filling the gap. A service whose size cannot be read is LEFT ALONE by
    /// the penetration rule, which is the safe answer, so refusing costs a clash a person
    /// looks at and guessing costs a hole in a wall nobody checked.
    /// </summary>
    public static class SizeText
    {
        /// <summary>
        /// Every measurement in that text, in millimetres, in the order it read them, or
        /// an empty list. Never throws: text is read off somebody else's model and a
        /// string this cannot parse is a string with no size in it.
        /// </summary>
        public static IList<double> Millimetres(string text)
        {
            List<double> found = new List<double>();

            if (string.IsNullOrEmpty(text))
            {
                return found;
            }

            int at = 0;

            while (at < text.Length)
            {
                if (!IsNumberStart(text, at))
                {
                    at++;
                    continue;
                }

                int numberFrom = at;

                while (at < text.Length && IsNumberPart(text[at]))
                {
                    at++;
                }

                string number = text.Substring(numberFrom, at - numberFrom);
                int afterNumber = at;

                // One optional space between the number and its unit, which is how Revit
                // writes "600 mm" and how a person writes "53mm". More than one space is
                // two separate things and not a measurement.
                if (at < text.Length && text[at] == ' ')
                {
                    at++;
                }

                double millimetres;

                if (TryUnitAt(text, ref at, number, out millimetres))
                {
                    found.Add(millimetres);
                }
                else
                {
                    // No unit on this number, so it is refused and the walk carries on
                    // from where the number ended rather than from where a unit would
                    // have been, or a bare number would swallow the letters after it.
                    at = afterNumber;
                }
            }

            return found;
        }

        /// <summary>
        /// The largest measurement in that text, in millimetres, or null where there is
        /// none. The largest because that is the one that has to fit through the wall,
        /// which is the reading `SizeRule.LargestMillimetres` already takes for a duct.
        /// </summary>
        public static double? LargestMillimetres(string text)
        {
            IList<double> all = Millimetres(text);

            if (all.Count == 0)
            {
                return null;
            }

            double largest = all[0];

            for (int i = 1; i < all.Count; i++)
            {
                if (all[i] > largest)
                {
                    largest = all[i];
                }
            }

            return largest;
        }

        /// <summary>
        /// Whether a unit this tool knows sits at that point, and what the number before
        /// it comes to in millimetres. The LONGEST label wins, so "mm" is read before
        /// "m" and "mil" before "mi", and a label followed by another letter is not a
        /// unit at all, so "min" is never read as "mi".
        /// </summary>
        private static bool TryUnitAt(string text, ref int at, string number, out double millimetres)
        {
            millimetres = 0.0;
            double value;

            if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return false;
            }

            string label = LongestLabelAt(text, at);

            if (label == null)
            {
                return false;
            }

            UnitRow row = UnitTable.FindByShortLabel(label);

            if (row == null)
            {
                return false;
            }

            at += label.Length;
            millimetres = value * row.MillimetresPerUnit;
            return true;
        }

        private static string LongestLabelAt(string text, int at)
        {
            string best = null;
            IList<string> labels = UnitTable.ShortLabels();

            for (int i = 0; i < labels.Count; i++)
            {
                string label = labels[i];

                if (at + label.Length > text.Length)
                {
                    continue;
                }

                if (string.Compare(text, at, label, 0, label.Length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }

                // A label with a letter straight after it is a longer word and not a
                // unit, so "mine" is not "mi" and "minutes" is not "mi". The DIMENSION
                // SEPARATOR is the exception and is not a letter for this purpose, or
                // "600 mmx100 mm" would read as no unit at all, which is what it did
                // until the client's own strings were put in front of it.
                int after = at + label.Length;

                if (after < text.Length && IsLetter(text[after]) && !IsSeparator(text[after]))
                {
                    continue;
                }

                if (best == null || label.Length > best.Length)
                {
                    best = label;
                }
            }

            return best;
        }

        private static bool IsNumberStart(string text, int at)
        {
            if (!IsDigit(text[at]))
            {
                return false;
            }

            // A digit straight after a letter is inside a word already being read, such
            // as the 70 in a cable's name. A digit after the DIMENSION SEPARATOR is the
            // second half of a measurement and is read, which is what "600 mmx100 mm" is.
            return at == 0 || !IsLetter(text[at - 1]) || IsSeparator(text[at - 1]);
        }

        /// <summary>
        /// What this project's models put BETWEEN the two halves of a measurement, read
        /// off the client's own strings on 2026-09-20: a cable tray fitting writes
        /// "600 mmx100 mm", so the x sits between a unit and the next number and is not
        /// part of either. No unit this tool knows contains an x, so reading it as a
        /// separator cannot swallow one.
        /// </summary>
        private static bool IsSeparator(char c)
        {
            return c == 'x' || c == 'X' || c == '×';
        }

        private static bool IsNumberPart(char c)
        {
            return IsDigit(c) || c == '.';
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool IsLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }
    }
}
