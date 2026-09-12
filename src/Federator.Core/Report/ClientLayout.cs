using System;
using System.Collections.Generic;

namespace Federator.Core.Report
{
    /// <summary>
    /// What every cell of the client's report LOOKS like, row kind by row kind, measured
    /// off samples\client-report on 2026-09-01.
    ///
    /// WHY IT IS A TABLE AND NOT CODE IN TWO PLACES. The writer paints the sheet and the
    /// check reads it back, and if each carried its own idea of the layout the two would
    /// drift apart quietly, which is the fault this whole session is about. So the layout
    /// is stated ONCE, here, the writer paints from it and the check compares against it.
    ///
    /// WHAT THAT CANNOT CATCH, said plainly because it matters. A check reading the same
    /// table the writer wrote from cannot tell that the TABLE is wrong. It catches the
    /// writer failing to apply it, a cell missed, a row of the wrong height, a fill left
    /// off. It does not catch this file disagreeing with the client's report. That is what
    /// ClientLayoutTests is for: it opens the sample xlsx and asserts every number below
    /// against it, so the table is pinned to their file rather than to an opinion.
    /// </summary>
    public static class ClientLayout
    {
        /// <summary>The kinds of row a block is made of, in the order they appear.</summary>
        public enum RowKind
        {
            /// <summary>Row 1, Clash Report, with the logo space beside it.</summary>
            Title,

            /// <summary>The test name and the nine headings.</summary>
            TestHeader,

            /// <summary>The nine values.</summary>
            TestValues,

            /// <summary>The blank between the test header and the table.</summary>
            Gap,

            /// <summary>Item 1 and Item 2 over their blocks.</summary>
            ItemLabels,

            /// <summary>The fifteen column headings.</summary>
            Headings,

            /// <summary>One clash.</summary>
            Clash
        }

        /// <summary>Their height for each kind of row. Measured.</summary>
        public static double Height(RowKind kind)
        {
            switch (kind)
            {
                case RowKind.Title: return WorkbookWriter.TitleRowHeight;
                case RowKind.TestHeader: return ClientStyle.TestHeaderRowHeight;
                case RowKind.TestValues: return ClientStyle.TestValuesRowHeight;
                case RowKind.Gap: return ClientStyle.GapRowHeight;
                case RowKind.ItemLabels: return ClientStyle.HeadingRowHeight;
                case RowKind.Headings: return ClientStyle.HeadingRowHeight;
                default: return WorkbookWriter.ClashRowHeight;
            }
        }

        /// <summary>
        /// The fill behind one cell, as six hex digits, or empty for none. Their banding:
        /// grey over every heading, blue over Item 1 and pink over Item 2, with the two
        /// item blocks paler on the clash rows than on the headings.
        /// </summary>
        public static string Fill(RowKind kind, int column)
        {
            bool item1 = column >= WorkbookWriter.ColumnItem1 && column < WorkbookWriter.ColumnItem2;
            bool item2 = column >= WorkbookWriter.ColumnItem2 && column <= WorkbookWriter.LastColumn;

            switch (kind)
            {
                case RowKind.TestHeader:
                case RowKind.TestValues:
                    return column <= WorkbookWriter.LastTestHeaderColumn
                        ? ClientStyle.HeaderGrey
                        : string.Empty;

                case RowKind.ItemLabels:
                case RowKind.Headings:
                    if (item1)
                    {
                        return ClientStyle.Item1Heading;
                    }

                    if (item2)
                    {
                        return ClientStyle.Item2Heading;
                    }

                    return ClientStyle.HeaderGrey;

                case RowKind.Clash:
                    if (item1)
                    {
                        return ClientStyle.Item1Body;
                    }

                    return item2 ? ClientStyle.Item2Body : string.Empty;

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// The last column a row of this kind reaches. Their test header is a table nine
        /// columns wide that stops at Status, so L to S on those two rows are not cells of
        /// theirs at all and are not expected to carry anything.
        /// </summary>
        public static int LastColumnOf(RowKind kind)
        {
            switch (kind)
            {
                case RowKind.TestHeader:
                case RowKind.TestValues:
                    return WorkbookWriter.LastTestHeaderColumn;

                case RowKind.Title:
                case RowKind.Gap:
                    return 0;

                default:
                    return WorkbookWriter.LastColumn;
            }
        }

        /// <summary>Their column widths, one per column from A.</summary>
        public static double Width(int column)
        {
            return column >= 1 && column <= WorkbookWriter.TheirWidths.Length
                ? WorkbookWriter.TheirWidths[column - 1]
                : 0.0;
        }

        /// <summary>
        /// Which kind a row is, given where it sits relative to the block's heading row.
        /// The heading row is the one carrying Clash Name, which is the only row the check
        /// can find without trusting our own numbering.
        /// </summary>
        public static RowKind KindOf(int row, int headingRow)
        {
            int from = row - headingRow;

            switch (from)
            {
                case -4: return RowKind.TestHeader;
                case -3: return RowKind.TestValues;
                case -2: return RowKind.Gap;
                case -1: return RowKind.ItemLabels;
                case 0: return RowKind.Headings;
                default: return RowKind.Clash;
            }
        }

        /// <summary>Every kind, for a test that wants to walk them.</summary>
        public static IList<RowKind> AllKinds()
        {
            return (RowKind[])Enum.GetValues(typeof(RowKind));
        }

        /// <summary>
        /// How close two heights or widths have to be. Excel writes a width to seven
        /// decimals and ClosedXML to six, so an exact comparison reports drift on all
        /// nineteen columns of a file that is right.
        /// </summary>
        public const double Epsilon = 0.0001;

        /// <summary>
        /// The number format the Distance cell carries, which is none at all. Theirs holds
        /// the rounded number under General, so a format here would mean ours is storing
        /// the raw double and hiding it behind a display, which is what it used to do.
        /// </summary>
        public const string DistanceNumberFormat = "";
    }
}
