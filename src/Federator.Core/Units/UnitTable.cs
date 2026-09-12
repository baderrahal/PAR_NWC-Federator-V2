using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Units
{
    /// <summary>
    /// One row per unit this tool knows. Every other place that named a unit, the
    /// exchange file's units attribute, the short label the report writes, the name on
    /// the Navisworks enum, the words the window shows, had its own list, and the lists
    /// had drifted: the report wrote "micrometers" where the clash step wrote "um" for the
    /// same unit. F33.
    /// </summary>
    public sealed class UnitRow
    {
        internal UnitRow(
            string enumName, string displayName, string shortLabel, string exchangeCode,
            double millimetresPerUnit)
        {
            EnumName = enumName;
            DisplayName = displayName;
            Short = shortLabel;
            ExchangeCode = exchangeCode;
            MillimetresPerUnit = millimetresPerUnit;
        }

        /// <summary>
        /// The member name on Autodesk.Navisworks.Api.Units, as text, because nothing in
        /// Core references Navisworks. The add-in parses it onto the enum and the values
        /// were read off the installed DLL, see docs\scan.md.
        /// </summary>
        public string EnumName { get; private set; }

        /// <summary>What the window shows. Metres, not Meters, because the team writes it that way.</summary>
        public string DisplayName { get; private set; }

        /// <summary>The short label the report and the log write, "m", "ft".</summary>
        public string Short { get; private set; }

        /// <summary>The value of the units attribute on an exchange file, "ft" on the reference file.</summary>
        public string ExchangeCode { get; private set; }

        /// <summary>How many millimetres one of this unit is. The one conversion factor.</summary>
        public double MillimetresPerUnit { get; private set; }

        public override string ToString()
        {
            return EnumName + " (" + Short + ")";
        }
    }

    /// <summary>
    /// The one unit table in this repo. ExchangeUnits converts through it, the clash step
    /// and the units step name units through it, and the window offers units from it.
    /// A unit that is not in here is not one this tool knows, anywhere.
    /// </summary>
    public static class UnitTable
    {
        private static readonly ReadOnlyCollection<UnitRow> Rows = new ReadOnlyCollection<UnitRow>(
            new List<UnitRow>
            {
                new UnitRow("Millimeters", "Millimetres", "mm", "mm", 1.0),
                new UnitRow("Centimeters", "Centimetres", "cm", "cm", 10.0),
                new UnitRow("Meters", "Metres", "m", "m", 1000.0),
                new UnitRow("Kilometers", "Kilometres", "km", "km", 1000000.0),
                new UnitRow("Inches", "Inches", "in", "in", 25.4),
                new UnitRow("Feet", "Feet", "ft", "ft", 304.8),
                new UnitRow("Yards", "Yards", "yd", "yd", 914.4),
                new UnitRow("Miles", "Miles", "mi", "mi", 1609344.0),

                // The last three have never been seen in an exchange file. They are here
                // because Autodesk.Navisworks.Api.Units carries them, so a document set
                // to one of them would otherwise fail to convert a tolerance written in
                // perfectly ordinary feet.
                new UnitRow("Micrometers", "Micrometres", "um", "um", 0.001),
                new UnitRow("Mils", "Mils", "mil", "mil", 0.0254),
                new UnitRow("Microinches", "Microinches", "uin", "uin", 0.0000254),
            });

        /// <summary>
        /// The units the Outputs step offers, in the order it shows them. Five of the
        /// eleven, because a model on this project is only ever in one of these. The
        /// first is the default.
        /// </summary>
        private static readonly string[] OfferedNames =
        {
            "Meters", "Millimeters", "Centimeters", "Feet", "Inches"
        };

        /// <summary>Every unit this tool knows.</summary>
        internal static IList<UnitRow> All
        {
            get { return Rows; }
        }

        /// <summary>The rows the Outputs step offers, in order, the default first.</summary>
        public static IList<UnitRow> Offered()
        {
            List<UnitRow> offered = new List<UnitRow>();

            foreach (string name in OfferedNames)
            {
                offered.Add(ByEnumName(name));
            }

            return offered;
        }

        /// <summary>The row whose exchange code this is, case blind and trimmed, or null.</summary>
        public static UnitRow FindByExchangeCode(string code)
        {
            if (code == null)
            {
                return null;
            }

            string wanted = code.Trim();

            foreach (UnitRow row in Rows)
            {
                if (string.Equals(row.ExchangeCode, wanted, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }

            return null;
        }

        /// <summary>The row whose Navisworks enum name this is, case blind and trimmed, or null.</summary>
        public static UnitRow FindByEnumName(string enumName)
        {
            if (enumName == null)
            {
                return null;
            }

            string wanted = enumName.Trim();

            foreach (UnitRow row in Rows)
            {
                if (string.Equals(row.EnumName, wanted, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }

            return null;
        }

        /// <summary>
        /// The row for an enum name the caller knows is in the table. Throws on one that is
        /// not, naming it and every name that is, rather than guessing at a unit.
        /// </summary>
        public static UnitRow ByEnumName(string enumName)
        {
            UnitRow row = FindByEnumName(enumName);

            if (row == null)
            {
                throw new NotSupportedException(
                    "UNKNOWN unit \"" + enumName + "\". Known units are "
                        + string.Join(", ", new List<string>(EnumNames()).ToArray()) + ".");
            }

            return row;
        }

        /// <summary>Every exchange code, sorted, for a message that lists what is known.</summary>
        public static IList<string> ExchangeCodes()
        {
            List<string> codes = new List<string>();

            foreach (UnitRow row in Rows)
            {
                codes.Add(row.ExchangeCode);
            }

            codes.Sort(StringComparer.OrdinalIgnoreCase);
            return codes;
        }

        /// <summary>Every enum name, in table order, for a message that lists what is known.</summary>
        public static IList<string> EnumNames()
        {
            List<string> names = new List<string>();

            foreach (UnitRow row in Rows)
            {
                names.Add(row.EnumName);
            }

            return names;
        }
    }
}
