using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
            "Item ID", "Layer", "Item Name", "Item Type",
            "Item ID", "Layer", "Item Name", "Item Type"
        };

        /// <summary>
        /// The four columns each item block carries, in their order. Measured on both
        /// exports in samples\client-report, which agree exactly.
        /// </summary>
        public static readonly string[] PerItemColumns =
        {
            "Item ID", "Layer", "Item Name", "Item Type"
        };

        /// <summary>Where the Item 1 block starts in <see cref="ClashColumns"/>, zero based.</summary>
        public const int FirstItemColumn = 7;

        /// <summary>How many columns each item block holds. Four, with Layer among them.</summary>
        public const int ItemColumns = 4;

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
        public const string GridSeparator = " : ";

        public static string GridLocation(string grid, string level)
        {
            bool hasGrid = !string.IsNullOrEmpty(grid);
            bool hasLevel = !string.IsNullOrEmpty(level);

            // Navisworks' own grid intersection name ALREADY carries the level, so it
            // arrives reading "D-8 : LGF". Joining the level onto that gave
            // "D-8 : LGF : LGF" on a real run. The level is only appended when the grid
            // does not already end with it.
            if (hasGrid && hasLevel && AlreadyEndsWith(grid, level))
            {
                return grid;
            }

            if (hasGrid && hasLevel)
            {
                return grid + GridSeparator + level;
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
            if (NoIdAtAll(value))
            {
                return string.Empty;
            }

            string name = string.IsNullOrEmpty(label) ? DefaultIdLabel : label;
            return name + ": " + value;
        }

        /// <summary>
        /// An all zero GUID is not an id, it is what Navisworks hands back when the item
        /// has none. A real run wrote
        /// "Instance GUID: 00000000-0000-0000-0000-000000000000" into all 426 item cells,
        /// which reads like an id and identifies nothing. An empty cell says the same
        /// thing honestly.
        /// </summary>
        public static bool NoIdAtAll(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            foreach (char c in value)
            {
                if (c != '0' && c != '-' && c != '{' && c != '}' && c != ' ')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The tolerance carrying its unit, "0.025m". The stylesheet writes the tolerance
        /// attribute and the units attribute one after the other with nothing between
        /// them, so there is no space and this is one field.
        /// </summary>
        public static string Tolerance(double value, string units)
        {
            return value.ToString(ToleranceFormat, CultureInfo.InvariantCulture)
                + (units ?? string.Empty);
        }

        /// <summary>
        /// Three decimals. Theirs reads 0.025m and ours read 0.2460629921ft on a real run,
        /// which is the same number of decimals the file happened to carry rather than a
        /// format. The units differ because the two documents do, and that is correct.
        /// </summary>
        public const string ToleranceFormat = "0.000";

        /// <summary>
        /// A distance or a coordinate the way theirs writes it.
        ///
        /// MEASURED on 2026-09-01 across all 387 coordinates and 129 distances in the two
        /// exports in samples\client-report. The rule is three decimals, trailing zeros
        /// kept, so 8.310 and -0.440 and 17.150 all appear as they are.
        ///
        /// With ONE exception, which is why this is not just a format string. A value that
        /// is not zero but would round to zero at three decimals is written to three
        /// significant figures instead, in plain decimal and never in exponent form. All
        /// nine of those in their files:
        ///
        ///     0.0000000000000373   0.0000000000000243   0.0000000000000365
        ///     0.0000000000000486   0.0000000684         -0.000432
        ///
        /// and 0.000 or -0.000 appears nowhere in either file. So a real value is never
        /// shown as zero.
        ///
        /// Whether theirs ROUNDS or TRUNCATES is UNKNOWN. Both files carry only the
        /// already formatted text and nothing in either carries the value behind it, so
        /// there is nothing to compare. This rounds, which is what three decimals
        /// ordinarily means.
        /// </summary>
        public static string Fixed(double value)
        {
            if (value == 0.0 || double.IsNaN(value) || double.IsInfinity(value))
            {
                return value.ToString(FixedFormat, CultureInfo.InvariantCulture);
            }

            string three = value.ToString(FixedFormat, CultureInfo.InvariantCulture);

            // Would it read as zero when it is not. Checked on the text rather than on the
            // number, because that is the thing a person would see.
            if (three.IndexOfAny(NotZero) >= 0)
            {
                return three;
            }

            return Significant(value);
        }

        /// <summary>The format three decimals means, kept here so nothing else spells it out.</summary>
        public const string FixedFormat = "0.000";

        /// <summary>How many significant figures a value too small for three decimals gets.</summary>
        public const int SmallValueFigures = 3;

        private static readonly char[] NotZero = { '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// Three significant figures, in plain decimal. 3.73e-14 comes out as
        /// 0.0000000000000373, which is what their file holds, and never as an exponent.
        /// </summary>
        private static string Significant(double value)
        {
            int magnitude = (int)Math.Floor(Math.Log10(Math.Abs(value)));
            int decimals = SmallValueFigures - 1 - magnitude;

            if (decimals < 0)
            {
                decimals = 0;
            }

            // A double cannot carry more than this, and a format string longer than it
            // would only print noise.
            if (decimals > 330)
            {
                decimals = 330;
            }

            return value.ToString("0." + new string('0', decimals), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The number format the Distance cell is given, so the cell holds the raw signed
        /// number and still sorts, while reading the way theirs does. Ours came out as
        /// -0.328083992004395 on a real run because the cell carried no format at all.
        /// </summary>
        public const string DistanceFormat = "0.000";

        private static bool AlreadyEndsWith(string grid, string level)
        {
            if (grid.EndsWith(GridSeparator + level, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(grid, level, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The test type in the words the client's report uses.
        ///
        /// One pair is MEASURED: the file carries test_type="hard_conservative" and the
        /// accepted report shows "Hard (Conservative)". The rule read off that pair is
        /// that the first word is the type and anything after it is a qualifier in
        /// brackets, and it is applied to the others rather than each being invented.
        /// Only the one pair has been seen against a client file. See docs\scan.md
        /// section 4k.
        ///
        /// Takes either the file's token, hard_conservative, or the enum name,
        /// HardConservative, because both reach this from different places.
        /// </summary>
        public static string TestTypeWording(string testType)
        {
            if (string.IsNullOrEmpty(testType))
            {
                return string.Empty;
            }

            // Already in their form. A bracket or a space means someone has written the
            // wording rather than a token, so it is left exactly as it is. Without this,
            // "Hard (Conservative)" came back as "Hard (( Conservative))".
            if (testType.IndexOf('(') >= 0 || testType.IndexOf(' ') >= 0)
            {
                return testType;
            }

            IList<string> words = Words(testType);

            if (words.Count == 0)
            {
                return testType;
            }

            if (words.Count == 1)
            {
                return words[0];
            }

            string[] rest = new string[words.Count - 1];

            for (int i = 1; i < words.Count; i++)
            {
                rest[i - 1] = words[i];
            }

            return words[0] + " (" + string.Join(" ", rest) + ")";
        }

        /// <summary>Splits on underscores and on the capitals of an enum name, and title cases each.</summary>
        private static IList<string> Words(string value)
        {
            List<string> words = new List<string>();
            StringBuilder current = new StringBuilder();

            foreach (char c in value)
            {
                if (c == '_' || c == ' ' || c == '-')
                {
                    Take(words, current);
                    continue;
                }

                if (char.IsUpper(c) && current.Length > 0)
                {
                    Take(words, current);
                }

                current.Append(c);
            }

            Take(words, current);
            return words;
        }

        private static void Take(List<string> words, StringBuilder current)
        {
            if (current.Length == 0)
            {
                return;
            }

            string word = current.ToString();
            words.Add(char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant());
            current.Length = 0;
        }
    }
}
