using System;
using System.Collections.Generic;
using System.Text;

namespace Federator.Core.Findings
{
    /// <summary>
    /// The findings as a table, so Copy findings puts something on the clipboard that
    /// pastes into Excel as rows and columns rather than as one blob of text.
    ///
    /// The code stays as its own short column, so a person can sort or filter on it, and
    /// the plain words sit next to it.
    /// </summary>
    public static class FindingsTable
    {
        /// <summary>What separates the columns. Tab, because that is what Excel splits on.</summary>
        public const char Separator = '\t';

        public static readonly string[] Columns =
        {
            "Code",
            "Buildings",
            "What it means",
            "Files"
        };

        public static string Header()
        {
            return string.Join(Separator.ToString(), Columns);
        }

        /// <summary>One row per finding, in the order they were found.</summary>
        public static IList<string> Rows(IEnumerable<ScanFinding> findings)
        {
            if (findings == null)
            {
                throw new ArgumentNullException("findings");
            }

            List<string> rows = new List<string>();

            foreach (ScanFinding finding in findings)
            {
                if (finding == null)
                {
                    continue;
                }

                string[] cells =
                {
                    Clean(finding.Label),
                    Clean(string.Join(", ", new List<string>(finding.Buildings).ToArray())),
                    Clean(finding.Sentence),
                    Clean(string.Join(", ", new List<string>(finding.Files).ToArray()))
                };

                rows.Add(string.Join(Separator.ToString(), cells));
            }

            return rows;
        }

        /// <summary>
        /// The header and every row, ready for the clipboard. A run with nothing odd still
        /// gets the header and one row saying so, rather than an empty clipboard that
        /// reads as a failed copy.
        /// </summary>
        public static string Tsv(IEnumerable<ScanFinding> findings)
        {
            IList<string> rows = Rows(findings);
            StringBuilder text = new StringBuilder();

            text.Append(Header()).Append(Environment.NewLine);

            if (rows.Count == 0)
            {
                text.Append(Clean("NOTHING ODD")).Append(Separator)
                    .Append(Separator)
                    .Append(Clean("Nothing was found worth a look in this run."))
                    .Append(Separator)
                    .Append(Environment.NewLine);

                return text.ToString();
            }

            foreach (string row in rows)
            {
                text.Append(row).Append(Environment.NewLine);
            }

            return text.ToString();
        }

        /// <summary>
        /// A tab or a newline inside a cell would break the table into the wrong shape, so
        /// both become a space. Nothing is dropped, only flattened.
        /// </summary>
        private static string Clean(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace('\t', ' ')
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Trim();
        }
    }
}
