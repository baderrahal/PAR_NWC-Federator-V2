using System.Globalization;

namespace Federator.Core.Report
{
    /// <summary>
    /// The columns of the report the client has already accepted, in their order and in
    /// their words.
    ///
    /// Every name and every format in here was read off two things on 2026-08-31, never
    /// invented:
    ///
    /// - the accepted report itself,
    ///   C:\00_NM\Clash report\1104-PAR-1A02WO-XXX-BM-RPT-000001.html and its .xlsx
    /// - the stylesheet Navisworks used to write it,
    ///   en-US\stylesheets\clash_report_html_tabular.xsl
    ///
    /// The stylesheet matters because it says which of these are fields and which are
    /// joins. Grid Location and Clash Point look like they could be several columns and
    /// are one each, and Item ID carries its own label out of the model rather than a
    /// constant. See docs\scan.md section 4k.
    /// </summary>
    public static class ClientFormat
    {
        /// <summary>
        /// The per test header, their order and their words. From the clashtest template
        /// of the stylesheet, where these nine are written in this order whenever the
        /// summary is on.
        /// </summary>
        public static readonly string[] TestHeader =
        {
            "Tolerance", "Clashes", "New", "Active", "Reviewed", "Approved", "Resolved",
            "Type", "Status"
        };

        /// <summary>
        /// The per clash columns, their order and their words. Seven general ones, then
        /// three per item, which is what the accepted report carries.
        /// </summary>
        public static readonly string[] ClashColumns =
        {
            "Image", "Clash Name", "Status", "Distance", "Grid Location", "Description",
            "Clash Point",
            "Item ID", "Item Name", "Item Type",
            "Item ID", "Item Name", "Item Type"
        };

        /// <summary>Where the Item 1 block starts in <see cref="ClashColumns"/>, zero based.</summary>
        public const int FirstItemColumn = 7;

        /// <summary>How many columns each item block holds.</summary>
        public const int ItemColumns = 3;

        /// <summary>The merged labels sitting above the two item blocks.</summary>
        public const string ItemGroup1 = "Item 1";

        public const string ItemGroup2 = "Item 2";

        /// <summary>
        /// The label in front of an item's id. Navisworks writes the name of whatever
        /// property carried the id, so on a Revit sourced NWC it reads "Element ID". It
        /// is a default here rather than a constant, because a model from something other
        /// than Revit will carry a different one.
        /// </summary>
        public const string DefaultIdLabel = "Element ID";

        /// <summary>
        /// The clash point as one field. The stylesheet writes the three literal
        /// prefixes and two commas into a single cell, so this is one column and not
        /// three: x:31.643, y:-2.913, z:3.325
        ///
        /// Three decimals because that is what every one of the 60 rows in the accepted
        /// report carries, trailing zeros included, z:-0.050 among them.
        /// </summary>
        public static string ClashPoint(double x, double y, double z)
        {
            return "x:" + Fixed(x) + ", y:" + Fixed(y) + ", z:" + Fixed(z);
        }

        /// <summary>
        /// The grid location as one field, "B-1 : ROF", which is the grid intersection
        /// then the level with a spaced colon between them.
        ///
        /// Either half can be missing, and the separator goes with the half that is not
        /// there, so a model with no levels reads "B-1" rather than "B-1 : ".
        /// </summary>
        public static string GridLocation(string grid, string level)
        {
            bool hasGrid = !string.IsNullOrEmpty(grid);
            bool hasLevel = !string.IsNullOrEmpty(level);

            if (hasGrid && hasLevel)
            {
                return grid + " : " + level;
            }

            if (hasGrid)
            {
                return grid;
            }

            return hasLevel ? level : string.Empty;
        }

        /// <summary>
        /// An item id as one field, "Element ID: 1554240". The label is italic in their
        /// HTML and plain once it reaches a cell, which is what the accepted xlsx holds.
        /// </summary>
        public static string ItemId(string label, string value)
        {
            string name = string.IsNullOrEmpty(label) ? DefaultIdLabel : label;

            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return name + ": " + value;
        }

        /// <summary>
        /// The tolerance carrying its unit, "0.025m". The stylesheet writes the tolerance
        /// attribute and the units attribute one after the other with nothing between
        /// them, so there is no space and this is one field.
        /// </summary>
        public static string Tolerance(double value, string units)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture) + (units ?? string.Empty);
        }

        /// <summary>
        /// Three decimals, invariant, which is the form every distance and every
        /// coordinate in the accepted report is written in.
        /// </summary>
        public static string Fixed(double value)
        {
            return value.ToString("0.000", CultureInfo.InvariantCulture);
        }
    }
}
