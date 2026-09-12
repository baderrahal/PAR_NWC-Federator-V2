using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Pins ClientLayout to the client's own file.
    ///
    /// WHY THIS IS THE IMPORTANT ONE. WorkbookCheck compares what we wrote against
    /// ClientLayout, and WorkbookWriter paints from ClientLayout, so a number wrong in the
    /// table is wrong in both and the check reports clean. That has now happened three
    /// times. These open the accepted export and read every number back off it, so the
    /// table answers to their file rather than to whoever typed it.
    ///
    /// A missing sample SKIPS rather than fails, because the exports are Bader's and a
    /// fresh clone without them must still build.
    /// </summary>
    [TestFixture]
    public class ClientLayoutTests
    {
        private const string Sample = "1104-PAR-1A04WN-XXX-BM-RPT-000001.xlsx";

        private static string Find()
        {
            string path = Path.Combine(Samples.Folder(), "client-report", Sample);

            return File.Exists(path) ? path : null;
        }

        private static IXLWorksheet Theirs(out XLWorkbook book)
        {
            string path = Find();

            if (path == null)
            {
                book = null;
                Assert.Ignore("The client's export is not in this checkout, so there is "
                    + "nothing to pin the layout to. Commit samples\\client-report\\"
                    + Sample + " to run this.");
            }

            book = new XLWorkbook(path);
            return book.Worksheet(1);
        }

        /// <summary>
        /// Their block starts at row 4 and the heading row carrying Clash Name is row 8,
        /// which is what KindOf is built on. Read rather than assumed.
        /// </summary>
        private static int HeadingRow(IXLWorksheet sheet)
        {
            for (int row = 1; row <= 40; row++)
            {
                if (sheet.Cell(row, WorkbookWriter.ColumnClashName).GetString() == "Clash Name")
                {
                    return row;
                }
            }

            Assert.Fail("Their export has no row holding Clash Name, so the layout cannot "
                + "be read off it.");
            return 0;
        }

        [Test]
        public void EveryRowKindIsAsHighAsTheirs()
        {
            XLWorkbook book;
            IXLWorksheet sheet = Theirs(out book);

            using (book)
            {
                int heading = HeadingRow(sheet);
                List<string> wrong = new List<string>();

                foreach (ClientLayout.RowKind kind in ClientLayout.AllKinds())
                {
                    int row = kind == ClientLayout.RowKind.Title
                        ? 1
                        : heading + Offset(kind);

                    double theirs = sheet.Row(row).Height;
                    double ours = ClientLayout.Height(kind);

                    if (Math.Abs(theirs - ours) > ClientLayout.Epsilon)
                    {
                        wrong.Add(kind + " row " + row + ": theirs " + theirs
                            + ", the table says " + ours);
                    }
                }

                Assert.That(wrong, Is.Empty, "The row heights in ClientLayout disagree "
                    + "with the client's own export: " + string.Join("; ", wrong.ToArray()));
            }
        }

        private static int Offset(ClientLayout.RowKind kind)
        {
            switch (kind)
            {
                case ClientLayout.RowKind.TestHeader: return -4;
                case ClientLayout.RowKind.TestValues: return -3;
                case ClientLayout.RowKind.Gap: return -2;
                case ClientLayout.RowKind.ItemLabels: return -1;
                case ClientLayout.RowKind.Headings: return 0;
                default: return 1;
            }
        }

        [Test]
        public void EveryCellIsFilledTheColourTheirsIs()
        {
            XLWorkbook book;
            IXLWorksheet sheet = Theirs(out book);

            using (book)
            {
                int heading = HeadingRow(sheet);
                List<string> wrong = new List<string>();

                foreach (ClientLayout.RowKind kind in ClientLayout.AllKinds())
                {
                    if (kind == ClientLayout.RowKind.Title || kind == ClientLayout.RowKind.Gap)
                    {
                        continue;
                    }

                    int row = heading + Offset(kind);

                    for (int column = 1; column <= ClientLayout.LastColumnOf(kind); column++)
                    {
                        string theirs = Hex(sheet.Cell(row, column));
                        string ours = ClientLayout.Fill(kind, column);

                        if (!string.Equals(theirs, ours, StringComparison.OrdinalIgnoreCase))
                        {
                            wrong.Add(kind + " " + Letter(column) + row + ": theirs "
                                + Say(theirs) + ", the table says " + Say(ours));
                        }
                    }
                }

                Assert.That(wrong, Is.Empty, "The fills in ClientLayout disagree with the "
                    + "client's own export: " + string.Join("; ", wrong.ToArray()));
            }
        }

        [Test]
        public void EveryColumnIsAsWideAsTheirs()
        {
            XLWorkbook book;
            IXLWorksheet sheet = Theirs(out book);

            using (book)
            {
                List<string> wrong = new List<string>();

                for (int column = 1; column <= WorkbookWriter.LastColumn; column++)
                {
                    // Their file was written by Excel, so its stored width carries the
                    // padding that ClosedXML takes off when it reports one.
                    double theirs = sheet.Column(column).Width
                        + WorkbookWriter.ClosedXmlWidthPadding;
                    double ours = ClientLayout.Width(column);

                    if (Math.Abs(theirs - ours) > 0.001)
                    {
                        wrong.Add(Letter(column) + ": theirs " + theirs
                            + ", the table says " + ours);
                    }
                }

                Assert.That(wrong, Is.Empty, "The column widths in ClientLayout disagree "
                    + "with the client's own export: " + string.Join("; ", wrong.ToArray()));
            }
        }

        [Test]
        public void TheirDistanceCellCarriesNoNumberFormat()
        {
            XLWorkbook book;
            IXLWorksheet sheet = Theirs(out book);

            using (book)
            {
                int row = HeadingRow(sheet) + 1;
                IXLCell distance = sheet.Cell(row, WorkbookWriter.ColumnDistance);

                Assert.That(distance.DataType, Is.EqualTo(XLDataType.Number),
                    "Their Distance cell holds a number, so ours has to as well.");

                Assert.That(distance.Style.NumberFormat.Format,
                    Is.EqualTo(ClientLayout.DistanceNumberFormat),
                    "Their Distance cell carries no number format, so the table must not "
                    + "expect one.");
            }
        }

        [Test]
        public void TheirImageCellIsEmpty()
        {
            XLWorkbook book;
            IXLWorksheet sheet = Theirs(out book);

            using (book)
            {
                int row = HeadingRow(sheet) + 1;

                Assert.That(sheet.Cell(row, WorkbookWriter.ColumnImage).GetString(),
                    Is.Empty, "Their Image cell holds nothing at all. Ours held the "
                    + "picture's file name, which is what this pins.");
            }
        }

        [Test]
        public void TheirTitleIsTheWordsWeWrite()
        {
            XLWorkbook book;
            IXLWorksheet sheet = Theirs(out book);

            using (book)
            {
                Assert.That(sheet.Cell(1, 4).GetString(),
                    Is.EqualTo(WorkbookWriter.TitleText));
            }
        }

        private static string Hex(IXLCell cell)
        {
            if (cell.Style.Fill.PatternType == XLFillPatternValues.None)
            {
                return string.Empty;
            }

            System.Drawing.Color colour = cell.Style.Fill.BackgroundColor.Color;

            return colour.R.ToString("X2") + colour.G.ToString("X2") + colour.B.ToString("X2");
        }

        private static string Say(string colour)
        {
            return colour.Length == 0 ? "no fill" : colour;
        }

        private static string Letter(int column)
        {
            string name = string.Empty;
            int at = column;

            while (at > 0)
            {
                int part = (at - 1) % 26;
                name = (char)(65 + part) + name;
                at = (at - 1 - part) / 26;
            }

            return name;
        }
    }
}
