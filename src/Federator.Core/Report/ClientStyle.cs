using ClosedXML.Excel;

namespace Federator.Core.Report
{
    /// <summary>
    /// How the client's report LOOKS, measured cell by cell off
    /// samples\client-report\1104-PAR-1A04WN-XXX-BM-RPT-000001.xlsx on 2026-09-01 and
    /// checked against 1104-PAR-1A04WE-XXX-BM-RPT-000001.xlsx.
    ///
    /// WHY THIS EXISTS. Every VALUE in our workbook already matched theirs, and the check
    /// said so, while the two files looked nothing alike side by side. Theirs is banded:
    /// grey headers, a blue block for Item 1 and a pink block for Item 2, every cell boxed
    /// in a medium border, and the test header ruled off with a thick one. Ours was plain
    /// white with no border anywhere. A person comparing the two sees that before they see
    /// a single number, so a report matching theirs has to carry it.
    ///
    /// The colours are theirs. They are the fills Navisworks' own HTML Tabular export
    /// writes, which arrive in the xlsx when the page is opened in Excel, so they are read
    /// off the accepted report rather than chosen here.
    ///
    /// WHAT IS NOT COPIED, and why, so a later session does not read these as misses:
    ///
    ///   the 53 columns   theirs runs to BA because an HTML table declares that many. Ours
    ///                    stops at S, which is the last column either report fills
    ///   the font colour  theirs is theme 1, ours is an explicit black. They render the
    ///                    same under every stock theme and ClosedXML has no theme colour
    ///   pageSetup        theirs has none because an imported page carries none. ClosedXML
    ///                    writes one whatever we do
    ///   hyperlinks       theirs has none at all. Ours links the picture, which is a rule
    ///                    of its own and is worth more than the match
    /// </summary>
    public static class ClientStyle
    {
        /// <summary>The grey behind every heading, theirs.</summary>
        public const string HeaderGrey = "EEEEEE";

        /// <summary>The blue over the Item 1 headings, theirs.</summary>
        public const string Item1Heading = "99CCFF";

        /// <summary>The pink over the Item 2 headings, theirs.</summary>
        public const string Item2Heading = "FFCCCC";

        /// <summary>The pale blue behind every Item 1 value, theirs.</summary>
        public const string Item1Body = "DDEEFF";

        /// <summary>The pale pink behind every Item 2 value, theirs.</summary>
        public const string Item2Body = "FFEEEE";

        // Their row heights, measured. The clash rows are 60 and row 1 is 45, which live
        // on WorkbookWriter because the writer sets them as it goes.

        /// <summary>Their row 4, the test name and the nine headings.</summary>
        public const double TestHeaderRowHeight = 15.6;

        /// <summary>Their row 5, the nine values.</summary>
        public const double TestValuesRowHeight = 15.0;

        /// <summary>Their row 6, the blank between the test header and the table.</summary>
        public const double GapRowHeight = 15.6;

        /// <summary>Their rows 7 and 8, the Item 1 and Item 2 labels and the headings.</summary>
        public const double HeadingRowHeight = 15.0;

        /// <summary>
        /// Every cell of theirs wraps. Set once per cell we write, because a cell we never
        /// touch is not in the file at all and inherits nothing.
        /// </summary>
        public static void Cell(IXLStyle style, XLAlignmentVerticalValues vertical)
        {
            style.Alignment.WrapText = true;
            style.Alignment.Vertical = vertical;
        }

        /// <summary>
        /// A run of columns boxed the way theirs boxes one merged cell: the left edge on
        /// the first column, the right edge on the last, and the top and bottom on all of
        /// them. Reproducing it this way rather than with an outside border is what keeps
        /// our cells identical to theirs where a run is merged, because Excel draws a
        /// merged region's edges off its corner cells and theirs leaves the inside bare.
        /// </summary>
        public static void Box(
            IXLWorksheet sheet, int row, int first, int last, XLBorderStyleValues weight)
        {
            for (int column = first; column <= last; column++)
            {
                IXLStyle style = sheet.Cell(row, column).Style;

                style.Border.TopBorder = weight;
                style.Border.BottomBorder = weight;

                if (column == first)
                {
                    style.Border.LeftBorder = weight;
                }

                if (column == last)
                {
                    style.Border.RightBorder = weight;
                }
            }
        }

        /// <summary>Fills a run of one row.</summary>
        public static void Fill(IXLWorksheet sheet, int row, int first, int last, string colour)
        {
            for (int column = first; column <= last; column++)
            {
                sheet.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromHtml("#" + colour);
            }
        }

        /// <summary>
        /// The two rows of the test header, ruled thick on the outside and medium within,
        /// which is exactly what theirs carries. The name sits in a cell merged over both
        /// rows, so its bottom edge is on the second row and never on the first.
        /// </summary>
        public static void TestHeader(IXLWorksheet sheet, int top, int nameLast, int last)
        {
            Fill(sheet, top, 1, last, HeaderGrey);
            Fill(sheet, top + 1, 1, last, HeaderGrey);

            for (int row = top; row <= top + 1; row++)
            {
                for (int column = 1; column <= last; column++)
                {
                    IXLStyle style = sheet.Cell(row, column).Style;
                    bool inTheName = column <= nameLast;

                    // Only the name is centred down. Their nine value columns carry no
                    // vertical alignment at all, so they sit on the bottom of the row.
                    style.Alignment.WrapText = true;

                    if (inTheName)
                    {
                        style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    style.Border.LeftBorder = column == 1
                        ? XLBorderStyleValues.Thick
                        : (inTheName ? XLBorderStyleValues.None : XLBorderStyleValues.Medium);

                    style.Border.RightBorder = column == last
                        ? XLBorderStyleValues.Thick
                        : (column == nameLast || !inTheName
                            ? XLBorderStyleValues.Medium
                            : XLBorderStyleValues.None);

                    // The name is one merged cell over both rows, so it is ruled only at
                    // the top of the first and the bottom of the second.
                    if (row == top)
                    {
                        style.Border.TopBorder = XLBorderStyleValues.Thick;
                        style.Border.BottomBorder = inTheName
                            ? XLBorderStyleValues.None
                            : XLBorderStyleValues.Medium;
                    }
                    else
                    {
                        style.Border.TopBorder = inTheName
                            ? XLBorderStyleValues.None
                            : XLBorderStyleValues.Medium;
                        style.Border.BottomBorder = XLBorderStyleValues.Thick;
                    }
                }
            }
        }
    }
}
