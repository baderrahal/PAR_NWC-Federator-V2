using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Federator.Core.Disciplines;
using Federator.Core.Naming;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F244. The report of C06 ME as the mock of 19 Oct 2026 shows it, built from the mock's own cells: its 18 ME models with
    /// their buildings, the model each was compared with and its distance, the building 1B06PS with AR, EL and ST and no ME,
    /// the CAD files and Generic families with their element ids, every file 2 days old on the run of 19 Oct 2026. A reference
    /// model of AR or EL sits at the origin of its building, an ME model that is its own reference has an ST model beside it, so
    /// the building is a group of two disciplines, and every model sits on the X axis at the distance the mock reads.
    /// </summary>
    internal static class DisciplineMock
    {
        public static readonly DateTime Run = new DateTime(2026, 10, 19, 9, 0, 0);

        private const string Shared = "S";

        public static DisciplineReport Report()
        {
            Dictionary<string, List<string>> files = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            Dictionary<string, List<ModelFacts>> models = new Dictionary<string, List<ModelFacts>>(StringComparer.Ordinal);
            Dictionary<string, string> fileOf = new Dictionary<string, string>(StringComparer.Ordinal);
            Dictionary<string, KeyValuePair<string, string>> sites = new Dictionary<string, KeyValuePair<string, string>>(StringComparer.Ordinal);
            Dictionary<string, List<ElementFact>> cad = new Dictionary<string, List<ElementFact>>(StringComparer.Ordinal);
            Dictionary<string, List<ElementFact>> generic = new Dictionary<string, List<ElementFact>>(StringComparer.Ordinal);

            using (XLWorkbook mock = new XLWorkbook(Samples.DisciplineWorkbookMock()))
            {
                IXLWorksheet toFix = mock.Worksheet("To fix");

                for (int r = 6; r <= toFix.LastRowUsed().RowNumber(); r++)
                {
                    if (toFix.Cell(r, 6).GetString() != "Coordinates")
                    {
                        continue;
                    }

                    Match words = Regex.Match(toFix.Cell(r, 7).GetString(), @"ME on site (.+), (\w+) on (.+)$");
                    sites[toFix.Cell(r, 4).GetString() + " " + toFix.Cell(r, 5).GetString()] =
                        new KeyValuePair<string, string>(words.Groups[1].Value, words.Groups[3].Value);
                }

                IXLWorksheet sheet = mock.Worksheet("Models");
                Dictionary<string, string> referenceOf = new Dictionary<string, string>(StringComparer.Ordinal);

                for (int r = 6; r <= sheet.LastRowUsed().RowNumber(); r++)
                {
                    string building = sheet.Cell(r, 2).GetString();
                    string model = sheet.Cell(r, 3).GetString();
                    string compared = sheet.Cell(r, 5).GetString();
                    string name = sheet.Cell(r, 11).GetString();

                    if (!files.ContainsKey(building))
                    {
                        files[building] = new List<string>();
                        models[building] = new List<ModelFacts>();
                    }

                    if (sheet.Cell(r, 4).GetString() == "MISSING")
                    {
                        foreach (string code in Regex.Match(name, @"This building has (.+)\.$").Groups[1].Value.Replace(" and ", ", ").Split(new[] { ", " }, StringSplitOptions.None))
                        {
                            Add(files, models, building, Nwc(building, code), 0, Shared);
                        }

                        continue;
                    }

                    fileOf[building + " " + model] = name;
                    IXLCell off = sheet.Cell(r, 6);
                    double metres = off.DataType == XLDataType.Number ? off.GetDouble() : 0.0;
                    KeyValuePair<string, string> site;
                    bool far = sites.TryGetValue(building + " " + model, out site);
                    string referenceSite = far ? site.Value : sites.Where(s => s.Key.StartsWith(building + " ", StringComparison.Ordinal)).Select(s => s.Value.Value).FirstOrDefault() ?? Shared;

                    if (compared != "itself" && !referenceOf.ContainsKey(building))
                    {
                        referenceOf[building] = compared;
                        Add(files, models, building, Nwc(building, compared), 0, referenceSite);
                    }

                    Add(files, models, building, name, metres * 1000.0, far ? site.Key : referenceSite);

                    if (compared == "itself")
                    {
                        Add(files, models, building, Nwc(building, "ST"), 0, referenceSite);
                    }
                }

                IXLWorksheet ids = mock.Worksheet("Element IDs");

                for (int r = 6; r <= ids.LastRowUsed().RowNumber(); r++)
                {
                    string name = fileOf[ids.Cell(r, 2).GetString() + " " + ids.Cell(r, 3).GetString()];
                    string id = ((long)ids.Cell(r, 7).GetDouble()).ToString(CultureInfo.InvariantCulture);
                    string source = ids.Cell(r, 5).GetString();

                    if (ids.Cell(r, 4).GetString() == "CAD import")
                    {
                        Into(cad, name).Add(new ElementFact(id, source, null, null, null));
                    }
                    else
                    {
                        Into(generic, name).Add(new ElementFact(id, null, source, ids.Cell(r, 6).GetString(), null));
                    }
                }
            }

            List<DisciplineGroup> groups = new List<DisciplineGroup>();
            List<FileDate> dates = new List<FileDate>();

            foreach (string building in files.Keys.OrderBy(b => b, StringComparer.Ordinal))
            {
                foreach (ModelFacts model in models[building])
                {
                    string name = DisciplineOf.FileName(model.File);
                    model.WithCad(cad.ContainsKey(name) ? cad[name] : new List<ElementFact>());
                    model.WithGeneric(generic.ContainsKey(name) ? generic[name] : new List<ElementFact>());
                }

                groups.Add(new DisciplineGroup(building, files[building], models[building], null));
                dates.AddRange(files[building].Select(f => new FileDate(f, Run.AddDays(-2), null)));
            }

            return DisciplineReport.For("C06", "C06 itself", groups, dates, Run, new DisciplineReportSettings(), new ContainerNameSettings());
        }

        private static string Nwc(string building, string discipline)
        {
            return "1104-PAR-" + building + "-ZZZ-" + discipline + "-MOD-000001.nwc";
        }

        private static void Add(Dictionary<string, List<string>> files, Dictionary<string, List<ModelFacts>> models, string building, string name, double x, string site)
        {
            string path = TestPaths.At("Parent", "C06", name);
            files[building].Add(path);
            models[building].Add(new ModelFacts(path).WithPlacement(x, 0, 0).WithSite(site));
        }

        private static List<ElementFact> Into(Dictionary<string, List<ElementFact>> lists, string name)
        {
            List<ElementFact> list;

            if (!lists.TryGetValue(name, out list))
            {
                list = new List<ElementFact>();
                lists.Add(name, list);
            }

            return list;
        }
    }

    /// <summary>
    /// F244. Two workbooks compared sheet by sheet the way Bader's order lists it: the sheet names and their order, every header
    /// text, every column width, the band's fills and heights, the header's font and fill, the chip fills of the Status column,
    /// the number formats and the values of every cell of the table, the freeze cell and the filter range. Each difference is
    /// one line naming the sheet and the cell.
    /// </summary>
    internal static class WorkbookComparison
    {
        /// <summary>
        /// MEASURED on 20 Oct 2026: openpyxl stores a column width as typed, 18 in the mock's XML, while ClosedXML stores 18 as
        /// 18.710625 and reads every stored width less 0.710625, so it reads the mock's 18 as 17.289375. The mock's widths are
        /// compared as its script typed them, the stored number, which is the order's.
        /// </summary>
        public const double OpenpyxlWidthPadding = 0.710625;

        /// <summary>The header row of each sheet and the column of its Status chips, 0 where it has none.</summary>
        private static readonly Dictionary<string, int[]> Layout = new Dictionary<string, int[]>(StringComparer.Ordinal)
        {
            { "Summary", new[] { 7, 6 } },
            { "Models", new[] { 5, 4 } },
            { "To fix", new[] { 5, 0 } },
            { "Element IDs", new[] { 5, 0 } }
        };

        /// <summary>The differences of a written workbook from another, whose widths are read plus the padding given, nought for one ClosedXML wrote.</summary>
        public static IList<string> Differences(string written, string mock, double mockWidthPadding)
        {
            List<string> found = new List<string>();

            using (XLWorkbook a = new XLWorkbook(written))
            using (XLWorkbook b = new XLWorkbook(mock))
            {
                string[] namesA = a.Worksheets.Select(s => s.Name).ToArray();
                string[] namesB = b.Worksheets.Select(s => s.Name).ToArray();

                if (!namesA.SequenceEqual(namesB))
                {
                    found.Add("the sheets: written " + string.Join(", ", namesA) + ", mock " + string.Join(", ", namesB));
                }

                foreach (IXLWorksheet mine in a.Worksheets)
                {
                    IXLWorksheet theirs;

                    if (!b.TryGetWorksheet(mine.Name, out theirs) || !Layout.ContainsKey(mine.Name))
                    {
                        continue;
                    }

                    Compare(mine, theirs, Layout[mine.Name][0], Layout[mine.Name][1], mockWidthPadding, found);
                }
            }

            return found;
        }

        private static void Compare(IXLWorksheet mine, IXLWorksheet theirs, int header, int status, double padding, List<string> found)
        {
            string sheet = mine.Name + "!";
            int lastColumn = Math.Max(Last(mine.Row(header)), Last(theirs.Row(header)));
            int lastRow = Math.Max(mine.LastRowUsed().RowNumber(), theirs.LastRowUsed().RowNumber());

            for (int c = 1; c <= lastColumn; c++)
            {
                Same(found, sheet + Letter(c) + " width", Math.Round(mine.Column(c).Width, 6), Math.Round(theirs.Column(c).Width + padding, 6));
            }

            for (int r = 1; r <= 3; r++)
            {
                Same(found, sheet + "row " + r + " height", mine.Row(r).Height, theirs.Row(r).Height);

                for (int c = 1; c <= lastColumn + 1; c++)
                {
                    Same(found, sheet + Letter(c) + r + " band fill", Fill(mine.Cell(r, c)), Fill(theirs.Cell(r, c)));
                }
            }

            Same(found, sheet + "row " + header + " height", mine.Row(header).Height, theirs.Row(header).Height);

            for (int c = 2; c <= lastColumn; c++)
            {
                IXLCell x = mine.Cell(header, c);
                IXLCell y = theirs.Cell(header, c);
                string at = sheet + x.Address.ToStringRelative();
                Same(found, at + " header text", x.GetString(), y.GetString());
                Same(found, at + " header font", Font(x), Font(y));
                Same(found, at + " header fill", Fill(x), Fill(y));
            }

            for (int r = header + 1; r <= lastRow; r++)
            {
                for (int c = 2; c <= lastColumn; c++)
                {
                    IXLCell x = mine.Cell(r, c);
                    IXLCell y = theirs.Cell(r, c);
                    string at = sheet + x.Address.ToStringRelative();
                    Same(found, at + " value", Value(x), Value(y));
                    Same(found, at + " number format", Format(x), Format(y));

                    if (c == status)
                    {
                        Same(found, at + " chip fill", Fill(x), Fill(y));
                    }
                }
            }

            Same(found, sheet + " freeze", Freeze(mine), Freeze(theirs));
            Same(found, sheet + " filter", Filter(mine), Filter(theirs));
        }

        private static int Last(IXLRow row)
        {
            IXLCell last = row.LastCellUsed();
            return last == null ? 1 : last.Address.ColumnNumber;
        }

        private static void Same<T>(List<string> found, string what, T written, T mock)
        {
            if (!Equals(written, mock))
            {
                found.Add(what + ": written " + written + ", mock " + mock);
            }
        }

        private static string Letter(int column)
        {
            return XLHelper.GetColumnLetterFromNumber(column);
        }

        /// <summary>A colour as its six hex digits, the alpha left out since openpyxl writes 00 where ClosedXML writes FF.</summary>
        public static string Hex(XLColor color)
        {
            if (color == null)
            {
                return "none";
            }

            switch (color.ColorType)
            {
                case XLColorType.Color:
                    return (color.Color.ToArgb() & 0xFFFFFF).ToString("X6", CultureInfo.InvariantCulture);
                case XLColorType.Theme:
                    return "theme " + color.ThemeColor;
                default:
                    return "indexed " + color.Indexed;
            }
        }

        public static string Fill(IXLCell cell)
        {
            return cell.Style.Fill.PatternType == XLFillPatternValues.None ? "none" : Hex(cell.Style.Fill.BackgroundColor);
        }

        private static string Font(IXLCell cell)
        {
            IXLFont font = cell.Style.Font;
            return font.FontName + " " + font.FontSize.ToString(CultureInfo.InvariantCulture) + " " + Hex(font.FontColor);
        }

        /// <summary>A number format as its words, a built in one by the words Excel gives its number.</summary>
        public static string Format(IXLCell cell)
        {
            string format = cell.Style.NumberFormat.Format;

            if (!string.IsNullOrEmpty(format))
            {
                return format;
            }

            switch (cell.Style.NumberFormat.NumberFormatId)
            {
                case 0:
                    return "General";
                case 1:
                    return "0";
                default:
                    return "built in " + cell.Style.NumberFormat.NumberFormatId;
            }
        }

        /// <summary>A number to the micrometre of a metre, since the mock holds 8.109999999999999 where the distance read is 8.11.</summary>
        private static string Value(IXLCell cell)
        {
            return cell.DataType == XLDataType.Number
                ? "number " + Math.Round(cell.GetDouble(), 6).ToString("R", CultureInfo.InvariantCulture)
                : "text " + cell.GetString();
        }

        private static string Freeze(IXLWorksheet sheet)
        {
            return sheet.SheetView.SplitRow + " rows, " + sheet.SheetView.SplitColumn + " columns";
        }

        private static string Filter(IXLWorksheet sheet)
        {
            return sheet.AutoFilter == null || sheet.AutoFilter.Range == null ? "none" : sheet.AutoFilter.Range.RangeAddress.ToStringRelative();
        }
    }
}
