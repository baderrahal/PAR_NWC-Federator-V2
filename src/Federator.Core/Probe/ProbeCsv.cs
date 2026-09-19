using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Federator.Core.Probe
{
    /// <summary>
    /// The probe's CSV, F86. Five columns and a header, written the way a spreadsheet
    /// reads one.
    ///
    /// COMMA AND NOT TAB, which is the opposite of the machine readable log. That file is
    /// full of Windows paths and clash names, both of which carry commas and neither of
    /// which carries a tab. This one carries property values off a model, which carry
    /// tabs and newlines as often as they carry commas, so neither separator is safe on
    /// its own and the values are QUOTED, which is what a CSV does and what Excel expects
    /// from a file named .csv.
    /// </summary>
    public static class ProbeCsv
    {
        /// <summary>The header, in the order the brief asks for.</summary>
        public static readonly string[] Columns =
        {
            "category",
            "property tab",
            "property name",
            "distinct value",
            "how many elements"
        };

        /// <summary>The header line.</summary>
        public static string Header()
        {
            return Line(Columns);
        }

        /// <summary>The whole file as text, header first.</summary>
        public static string Text(IEnumerable<ProbeRow> rows)
        {
            StringBuilder text = new StringBuilder();
            text.Append(Header());
            text.Append("\r\n");

            if (rows != null)
            {
                foreach (ProbeRow row in rows)
                {
                    text.Append(Line(new[]
                    {
                        row.Category,
                        row.Tab,
                        row.Property,
                        row.Value,
                        row.Elements.ToString(CultureInfo.InvariantCulture)
                    }));
                    text.Append("\r\n");
                }
            }

            return text.ToString();
        }

        /// <summary>
        /// One line. Every cell is quoted, whether it needs it or not, because a rule
        /// that only quotes sometimes is a rule that gets the sometimes wrong.
        /// </summary>
        public static string Line(IList<string> cells)
        {
            List<string> quoted = new List<string>();

            if (cells != null)
            {
                foreach (string cell in cells)
                {
                    quoted.Add(Quote(cell));
                }
            }

            return string.Join(",", quoted.ToArray());
        }

        /// <summary>
        /// One cell. A quote inside a value is doubled, which is what a CSV does, and a
        /// newline is left where it is INSIDE the quotes rather than stripped, because
        /// stripping it would change the value the probe exists to report.
        /// </summary>
        public static string Quote(string value)
        {
            string said = value == null ? string.Empty : value;
            return "\"" + said.Replace("\"", "\"\"") + "\"";
        }
    }
}
