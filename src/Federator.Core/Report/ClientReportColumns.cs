using System;
using System.Collections.Generic;

namespace Federator.Core.Report
{
    /// <summary>
    /// The columns the client's own report carries, so a page this tool wrote can be
    /// checked against them.
    ///
    /// WHERE THIS CAME FROM, because it is not a list somebody typed. Two sources, read on
    /// 2026-09-01:
    ///
    /// 1. clash_report_html_tabular.xsl in the Navisworks install, which is what renders
    ///    both their reports and ours. Every column in it is a literal in one of the header
    ///    templates, and the complete vocabulary it can write is
    ///
    ///      Approved By, Assigned To, Clash Group, Clash Name, Clash Point, Comments,
    ///      Date Approved, Date Found, Description, Distance, Grid Location, Image,
    ///      Item ID, Layer, Path, Status, and the Task block of Name, Start and End
    ///
    ///    Item Name and Item Type are NOT in that list. They are quick properties, written
    ///    one column per smarttag with the heading taken from the data, which is why they
    ///    can be anything at all.
    ///
    /// 2. The two exports in samples\client-report, which say WHICH of that vocabulary the
    ///    client actually receives. Both carry exactly the same header, checked.
    ///
    /// So the stylesheet gives what is possible and the samples give what is theirs.
    /// ClientReportColumnsTests re-reads both sample files and fails if this set no longer
    /// matches them, so the list below is checked against the files rather than trusted.
    /// </summary>
    public static class ClientReportColumns
    {
        /// <summary>
        /// The seven that sit before the item blocks, in their order. All seven are
        /// stylesheet literals.
        /// </summary>
        public static readonly string[] General =
        {
            "Image", "Clash Name", "Status", "Distance", "Grid Location", "Description",
            "Clash Point"
        };

        /// <summary>
        /// The four each item block carries, in their order. Item ID and Layer are
        /// stylesheet literals. Item Name and Item Type are the two quick properties their
        /// export was configured with, and reach the page as smarttag headings.
        /// </summary>
        public static readonly string[] PerItem =
        {
            "Item ID", "Layer", "Item Name", "Item Type"
        };

        /// <summary>The two of PerItem that come from the data rather than the stylesheet.</summary>
        public static readonly string[] QuickProperties = { "Item Name", "Item Type" };

        /// <summary>The whole header, general then item one then item two.</summary>
        public static IList<string> All()
        {
            List<string> all = new List<string>(General);
            all.AddRange(PerItem);
            all.AddRange(PerItem);
            return all;
        }

        /// <summary>
        /// Every name that may appear without being one of ours. A column outside this is
        /// something this tool added and does not belong on a page going to a client.
        /// </summary>
        public static bool IsTheirs(string column)
        {
            if (string.IsNullOrEmpty(column))
            {
                return false;
            }

            foreach (string name in General)
            {
                if (string.Equals(name, column, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            foreach (string name in PerItem)
            {
                if (string.Equals(name, column, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>One line naming where the set came from, for the log.</summary>
        public static string ReadFrom()
        {
            return "The client column set was read from clash_report_html_tabular.xsl in the "
                + "Navisworks install and from the exports in samples\\client-report, never "
                + "from a list typed into this tool.";
        }
    }
}
