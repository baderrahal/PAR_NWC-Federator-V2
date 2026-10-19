using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Federator.Core.Report
{
    /// <summary>
    /// THE PICTURES OF THE WORKBOOK ARE LINKED PICTURES, F152, Bader's decision of 2026-10-13, Q162-2:
    /// pictures stay links, the same form as the client's own file. His by-hand client workbook,
    /// samples\client-report\1104-PAR-1A02WN-XXX-BM-RPT-000001.xlsx, holds xl\drawings\drawing1.xml with
    /// one twoCellAnchor, editAs oneCell, per picture, each blip carrying r:link and no r:embed, and
    /// xl\drawings\_rels\drawing1.xml.rels with one relationship of Type image and TargetMode External per
    /// picture, its target the absolute file:/// path of the jpg with a space written %20. Measured on
    /// 2026-10-13: 64 pictures, every one 906780 EMU square, anchored in column A of its row.
    ///
    /// ClosedXML can neither write a linked picture nor leave one alone, so this edits the xlsx after
    /// SaveAs as the zip it is: the drawing part, its rels, its content type, the sheet's rel to it and the
    /// drawing element of the sheet, put where the schema's order puts it. Every part is made and turned into bytes first, then
    /// written into a copy that replaces the workbook only once whole, so a fault leaves it as ClosedXML wrote
    /// it. No picture is embedded, so the pictures show only while the _files folder stays where it was
    /// written, which is what the client's own file does too.
    /// </summary>
    public static class LinkedPictures
    {
        /// <summary>The size of every picture in the client's drawing, read off it, in EMU, about 95 pixels.</summary>
        public const long PictureEmu = 906780;

        /// <summary>The relationship type of a linked picture, as the client's drawing rels write it.</summary>
        public const string ImageRelationship = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image";

        /// <summary>The relationship a click link on a picture goes through, F169.</summary>
        public const string HyperlinkRelationship = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink";

        internal const string DrawingRelationship = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing";

        internal const string DrawingContentType = "application/vnd.openxmlformats-officedocument.drawing+xml";

        private const long EmuPerPoint = 12700;

        private const long EmuPerPixel = 9525;

        /// <summary>The widest digit of Calibri 11 in pixels, the font ClosedXML writes, by which Excel turns a column width into pixels.</summary>
        private const int DigitPixels = 7;

        /// <summary>The width Excel gives a column the sheet says nothing about, 8.43 characters, in pixels.</summary>
        private const int DefaultColumnPixels = 64;

        private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace Rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PackageRel = "http://schemas.openxmlformats.org/package/2006/relationships";
        private static readonly XNamespace Types = "http://schemas.openxmlformats.org/package/2006/content-types";
        private static readonly XNamespace Xdr = "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing";
        private static readonly XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";

        /// <summary>
        /// What may follow the drawing element in a worksheet, in the order of the schema. The drawing goes
        /// before the first of these the sheet holds, and at the end where it holds none.
        /// </summary>
        private static readonly HashSet<string> AfterTheDrawing = new HashSet<string>(StringComparer.Ordinal)
        {
            "legacyDrawing", "legacyDrawingHF", "drawingHF", "picture", "oleObjects", "controls",
            "webPublishItems", "tableParts", "extLst",
        };

        /// <summary>
        /// The target a linked picture's relationship carries, the client's shape: file:/// and the absolute
        /// path with its backslashes kept, a space written %20, a per cent sign %25 and a hash %23, since
        /// either would otherwise be read as an escape or as the end of the path.
        /// </summary>
        public static string TargetFor(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                throw new ArgumentException("A linked picture needs the path of its jpg.", "imagePath");
            }

            string full = Path.GetFullPath(imagePath).Replace("%", "%25").Replace(" ", "%20").Replace("#", "%23");

            return (full.StartsWith("/", StringComparison.Ordinal) ? "file://" : "file:///") + full;
        }

        /// <summary>
        /// The click link of a picture, F169, Bader's answer A to Q163: the path of its jpg relative to the
        /// folder the workbook sits in, forward slashed, going up with .. where the jpg is not below it.
        /// A space, a per cent sign and a hash are escaped as the link is read as an address, and nothing
        /// is added after the file name, which the client's own workbook carried a stray %20 after.
        /// Where the jpg is on another drive no relative path exists, and the full path as an address is used.
        /// Since F243 the Excel sits beside its jpgs in the Clash report folder, so the link is the pictures folder and the jpg.
        /// </summary>
        public static string RelativeLinkFor(string workbookPath, string imagePath)
        {
            string plain = RelativePathFor(workbookPath, imagePath);

            if (plain == null)
            {
                return TargetFor(imagePath);
            }

            return plain.Replace("%", "%25").Replace(" ", "%20").Replace("#", "%23");
        }

        /// <summary>
        /// The same relative path with nothing escaped, forward slashed, for a cell link and for the XML
        /// where the reader of the file does its own escaping. Null where the jpg is on another drive.
        /// </summary>
        public static string RelativePathFor(string workbookPath, string imagePath)
        {
            if (string.IsNullOrEmpty(workbookPath) || string.IsNullOrEmpty(imagePath))
            {
                throw new ArgumentException("A click link needs the workbook and the jpg.");
            }

            string from = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(workbookPath)))
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string to = Path.GetFullPath(imagePath);
            string[] a = from.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string[] b = to.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            int same = 0;

            while (same < a.Length && same < b.Length - 1
                && string.Equals(a[same], b[same], StringComparison.OrdinalIgnoreCase))
            {
                same++;
            }

            if (same == 0)
            {
                return null;
            }

            List<string> parts = new List<string>();

            for (int i = same; i < a.Length; i++)
            {
                parts.Add("..");
            }

            for (int i = same; i < b.Length; i++)
            {
                parts.Add(b[i]);
            }

            return string.Join("/", parts.ToArray());
        }

        /// <summary>
        /// The log's line for one workbook: how many linked pictures it holds and where they point, and that
        /// they show only while that folder stays where it was written, since none is embedded.
        /// </summary>
        public static string Line(int count, string picturesFolder)
        {
            if (count <= 0)
            {
                return "XLSX     no picture linked in the workbook, no clash row of it carries one";
            }

            return "XLSX     " + count.ToString(CultureInfo.InvariantCulture) + " linked "
                + (count == 1 ? "picture" : "pictures") + " in the workbook, each pointing at its jpg in "
                + picturesFolder
                + ". None is embedded, so they show only while that folder stays where it was written";
        }

        /// <summary>
        /// Adds one linked picture per entry to the first sheet of the saved workbook, each anchored at column
        /// A of its row, and returns how many were added. None asked adds nothing and leaves the file as it
        /// is. A sheet that already holds a drawing is refused, since ClosedXML writes one only for a picture
        /// it embeds and this tool embeds none.
        /// </summary>
        internal static int AddTo(string workbookPath, IList<LinkedPicture> pictures)
        {
            if (pictures == null || pictures.Count == 0)
            {
                return 0;
            }

            Edit edit;

            using (ZipArchive zip = ZipFile.OpenRead(workbookPath))
            {
                edit = Plan(zip, pictures, workbookPath);
            }

            // Every part is turned into its bytes before anything is written, and the bytes go into a copy
            // that replaces the workbook only once the copy is whole, the reviewer's point on F152: a part
            // that threw inside the archive's update would still be saved by its Dispose, half edited.
            List<KeyValuePair<string, byte[]>> parts = new List<KeyValuePair<string, byte[]>>
            {
                new KeyValuePair<string, byte[]>("[Content_Types].xml", Bytes(edit.ContentTypes)),
                new KeyValuePair<string, byte[]>(edit.SheetPart, Bytes(edit.Sheet)),
                new KeyValuePair<string, byte[]>(edit.SheetRelsPart, Bytes(edit.SheetRels)),
                new KeyValuePair<string, byte[]>(edit.DrawingPart, Bytes(edit.Drawing)),
                new KeyValuePair<string, byte[]>(RelsPartOf(edit.DrawingPart), Bytes(edit.DrawingRels)),
            };

            string copy = workbookPath + ".linking";

            try
            {
                File.Copy(workbookPath, copy, true);

                using (ZipArchive zip = ZipFile.Open(copy, ZipArchiveMode.Update))
                {
                    foreach (KeyValuePair<string, byte[]> part in parts)
                    {
                        Put(zip, part.Key, part.Value);
                    }
                }

                File.Replace(copy, workbookPath, null);
            }
            finally
            {
                if (File.Exists(copy))
                {
                    File.Delete(copy);
                }
            }

            return pictures.Count;
        }

        private sealed class Edit
        {
            internal string SheetPart;
            internal string SheetRelsPart;
            internal string DrawingPart;
            internal XDocument ContentTypes;
            internal XDocument Sheet;
            internal XDocument SheetRels;
            internal XDocument Drawing;
            internal XDocument DrawingRels;
        }

        private static Edit Plan(ZipArchive zip, IList<LinkedPicture> pictures, string workbookPath)
        {
            Edit edit = new Edit();
            edit.SheetPart = FirstSheetPart(zip);
            edit.SheetRelsPart = RelsPartOf(edit.SheetPart);
            edit.Sheet = Read(zip, edit.SheetPart);

            XElement root = edit.Sheet.Root;

            if (root.Element(Main + "drawing") != null)
            {
                throw new InvalidDataException("The sheet " + edit.SheetPart + " already holds a drawing, so no linked picture was added to it.");
            }

            int number = 1;

            while (zip.GetEntry("xl/drawings/drawing" + number.ToString(CultureInfo.InvariantCulture) + ".xml") != null)
            {
                number++;
            }

            edit.DrawingPart = "xl/drawings/drawing" + number.ToString(CultureInfo.InvariantCulture) + ".xml";

            edit.SheetRels = zip.GetEntry(edit.SheetRelsPart) == null
                ? new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement(PackageRel + "Relationships"))
                : Read(zip, edit.SheetRelsPart);

            string drawingId = FreeId(edit.SheetRels);
            edit.SheetRels.Root.Add(new XElement(PackageRel + "Relationship",
                new XAttribute("Id", drawingId),
                new XAttribute("Type", DrawingRelationship),
                new XAttribute("Target", "../drawings/" + Path.GetFileName(edit.DrawingPart))));

            if (root.GetPrefixOfNamespace(Rel) == null)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + "r", Rel.NamespaceName));
            }

            XElement drawing = new XElement(Main + "drawing", new XAttribute(Rel + "id", drawingId));
            XElement before = null;

            foreach (XElement child in root.Elements())
            {
                if (child.Name.Namespace == Main && AfterTheDrawing.Contains(child.Name.LocalName))
                {
                    before = child;
                    break;
                }
            }

            if (before == null)
            {
                root.Add(drawing);
            }
            else
            {
                before.AddBeforeSelf(drawing);
            }

            edit.ContentTypes = Read(zip, "[Content_Types].xml");
            edit.ContentTypes.Root.Add(new XElement(Types + "Override",
                new XAttribute("PartName", "/" + edit.DrawingPart),
                new XAttribute("ContentType", DrawingContentType)));

            SheetSizes sizes = new SheetSizes(root);
            XElement drawingRoot = new XElement(Xdr + "wsDr",
                new XAttribute(XNamespace.Xmlns + "xdr", Xdr.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "a", A.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "r", Rel.NamespaceName));
            XElement rels = new XElement(PackageRel + "Relationships");

            for (int i = 0; i < pictures.Count; i++)
            {
                string id = "rId" + (i + 1).ToString(CultureInfo.InvariantCulture);
                rels.Add(new XElement(PackageRel + "Relationship",
                    new XAttribute("Id", id),
                    new XAttribute("Type", ImageRelationship),
                    new XAttribute("Target", TargetFor(pictures[i].ImagePath)),
                    new XAttribute("TargetMode", "External")));
                string linkId = "rId" + (pictures.Count + i + 1).ToString(CultureInfo.InvariantCulture);
                rels.Add(new XElement(PackageRel + "Relationship",
                    new XAttribute("Id", linkId),
                    new XAttribute("Type", HyperlinkRelationship),
                    new XAttribute("Target", RelativeLinkFor(workbookPath, pictures[i].ImagePath)),
                    new XAttribute("TargetMode", "External")));
                drawingRoot.Add(Anchor(sizes, pictures[i].Row, i + 2, id, linkId));
            }

            edit.Drawing = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), drawingRoot);
            edit.DrawingRels = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), rels);

            return edit;
        }

        /// <summary>
        /// One picture as the client's drawing writes it: anchored at column A of its row with no offset, its
        /// far corner where the picture's size reaches over the sheet's own widths and heights, the blip a
        /// link and never an embed, stretched to the frame.
        /// </summary>
        private static XElement Anchor(SheetSizes sizes, int row, int shapeId, string relationshipId, string clickLinkId)
        {
            int fromRow = row - 1;
            int toColumn;
            long toColumnOffset;
            sizes.ReachAcross(0, PictureEmu, out toColumn, out toColumnOffset);
            int toRow;
            long toRowOffset;
            sizes.ReachDown(fromRow, PictureEmu, out toRow, out toRowOffset);

            return new XElement(Xdr + "twoCellAnchor", new XAttribute("editAs", "oneCell"),
                Marker("from", 0, 0, fromRow, 0),
                Marker("to", toColumn, toColumnOffset, toRow, toRowOffset),
                new XElement(Xdr + "pic",
                    new XElement(Xdr + "nvPicPr",
                        new XElement(Xdr + "cNvPr",
                            new XAttribute("id", shapeId),
                            new XAttribute("name", "Picture " + (shapeId - 1).ToString(CultureInfo.InvariantCulture)),
                            new XElement(A + "hlinkClick", new XAttribute(Rel + "id", clickLinkId), new XAttribute("tgtFrame", "_blank"))),
                        new XElement(Xdr + "cNvPicPr",
                            new XElement(A + "picLocks", new XAttribute("noChangeAspect", "1"), new XAttribute("noChangeArrowheads", "1")))),
                    new XElement(Xdr + "blipFill",
                        new XElement(A + "blip", new XAttribute(Rel + "link", relationshipId)),
                        new XElement(A + "srcRect"),
                        new XElement(A + "stretch", new XElement(A + "fillRect"))),
                    new XElement(Xdr + "spPr", new XAttribute("bwMode", "auto"),
                        new XElement(A + "xfrm",
                            new XElement(A + "off", new XAttribute("x", 0), new XAttribute("y", sizes.Top(fromRow))),
                            new XElement(A + "ext", new XAttribute("cx", PictureEmu), new XAttribute("cy", PictureEmu))),
                        new XElement(A + "prstGeom", new XAttribute("prst", "rect"), new XElement(A + "avLst")),
                        new XElement(A + "noFill"))),
                new XElement(Xdr + "clientData"));
        }

        private static XElement Marker(string name, int column, long columnOffset, int row, long rowOffset)
        {
            return new XElement(Xdr + name,
                new XElement(Xdr + "col", column),
                new XElement(Xdr + "colOff", columnOffset),
                new XElement(Xdr + "row", row),
                new XElement(Xdr + "rowOff", rowOffset));
        }

        /// <summary>The widths and heights the sheet holds, in EMU, read off its own XML.</summary>
        private sealed class SheetSizes
        {
            private readonly Dictionary<int, long> rows = new Dictionary<int, long>();
            private readonly List<long[]> columns = new List<long[]>();
            private readonly long defaultRow;
            private readonly List<long> tops = new List<long> { 0 };

            internal SheetSizes(XElement sheet)
            {
                double defaultPoints = 15.0;
                XElement format = sheet.Element(Main + "sheetFormatPr");

                if (format != null && format.Attribute("defaultRowHeight") != null)
                {
                    defaultPoints = double.Parse(format.Attribute("defaultRowHeight").Value, CultureInfo.InvariantCulture);
                }

                defaultRow = Points(defaultPoints);

                XElement data = sheet.Element(Main + "sheetData");

                if (data != null)
                {
                    foreach (XElement row in data.Elements(Main + "row"))
                    {
                        XAttribute r = row.Attribute("r");
                        XAttribute ht = row.Attribute("ht");

                        if (r != null && ht != null)
                        {
                            rows[int.Parse(r.Value, CultureInfo.InvariantCulture) - 1] =
                                Points(double.Parse(ht.Value, CultureInfo.InvariantCulture));
                        }
                    }
                }

                XElement cols = sheet.Element(Main + "cols");

                if (cols != null)
                {
                    foreach (XElement col in cols.Elements(Main + "col"))
                    {
                        int min = int.Parse(col.Attribute("min").Value, CultureInfo.InvariantCulture);
                        int max = int.Parse(col.Attribute("max").Value, CultureInfo.InvariantCulture);
                        double width = double.Parse(col.Attribute("width").Value, CultureInfo.InvariantCulture);

                        columns.Add(new long[] { min - 1, max - 1, ColumnEmu(width) });
                    }
                }
            }

            /// <summary>Excel's own rule for a stored column width in pixels, for a font whose widest digit is 7 pixels.</summary>
            private static long ColumnEmu(double width)
            {
                double pixels = Math.Truncate(((256 * width + Math.Truncate(128.0 / DigitPixels)) / 256) * DigitPixels);
                return (long)pixels * EmuPerPixel;
            }

            private static long Points(double points)
            {
                return (long)Math.Round(points * EmuPerPoint);
            }

            private long Row(int index)
            {
                long height;
                return rows.TryGetValue(index, out height) ? height : defaultRow;
            }

            private long Column(int index)
            {
                foreach (long[] range in columns)
                {
                    if (index >= range[0] && index <= range[1])
                    {
                        return range[2];
                    }
                }

                return DefaultColumnPixels * EmuPerPixel;
            }

            /// <summary>The distance from the top of the sheet to the top of a row, counted from nought.</summary>
            internal long Top(int index)
            {
                while (tops.Count <= index)
                {
                    tops.Add(tops[tops.Count - 1] + Row(tops.Count - 1));
                }

                return tops[index];
            }

            internal void ReachAcross(int column, long length, out int toColumn, out long offset)
            {
                while (length > Column(column))
                {
                    length -= Column(column);
                    column++;
                }

                toColumn = column;
                offset = length;
            }

            internal void ReachDown(int row, long length, out int toRow, out long offset)
            {
                while (length > Row(row))
                {
                    length -= Row(row);
                    row++;
                }

                toRow = row;
                offset = length;
            }
        }

        private static string FirstSheetPart(ZipArchive zip)
        {
            XDocument workbook = Read(zip, "xl/workbook.xml");
            XElement sheets = workbook.Root.Element(Main + "sheets");
            XElement first = sheets == null ? null : sheets.Element(Main + "sheet");

            if (first == null)
            {
                throw new InvalidDataException("The workbook names no sheet.");
            }

            string id = (string)first.Attribute(Rel + "id");

            foreach (XElement relationship in Read(zip, "xl/_rels/workbook.xml.rels").Root.Elements(PackageRel + "Relationship"))
            {
                if ((string)relationship.Attribute("Id") == id)
                {
                    string target = (string)relationship.Attribute("Target");
                    return target.StartsWith("/", StringComparison.Ordinal) ? target.Substring(1) : "xl/" + target;
                }
            }

            throw new InvalidDataException("The workbook's first sheet, " + id + ", has no part.");
        }

        private static string RelsPartOf(string part)
        {
            int slash = part.LastIndexOf('/');
            return part.Substring(0, slash + 1) + "_rels/" + part.Substring(slash + 1) + ".rels";
        }

        private static string FreeId(XDocument rels)
        {
            int highest = 0;

            foreach (XElement relationship in rels.Root.Elements(PackageRel + "Relationship"))
            {
                string id = (string)relationship.Attribute("Id") ?? string.Empty;
                int number;

                if (id.StartsWith("rId", StringComparison.Ordinal)
                    && int.TryParse(id.Substring(3), NumberStyles.None, CultureInfo.InvariantCulture, out number)
                    && number > highest)
                {
                    highest = number;
                }
            }

            return "rId" + (highest + 1).ToString(CultureInfo.InvariantCulture);
        }

        private static XDocument Read(ZipArchive zip, string part)
        {
            ZipArchiveEntry entry = zip.GetEntry(part);

            if (entry == null)
            {
                throw new InvalidDataException("The workbook holds no " + part + ".");
            }

            using (Stream stream = entry.Open())
            {
                return XDocument.Load(stream);
            }
        }

        /// <summary>A part as the bytes the package holds, UTF-8 with no byte order mark.</summary>
        private static byte[] Bytes(XDocument document)
        {
            XmlWriterSettings settings = new XmlWriterSettings { Encoding = new UTF8Encoding(false) };

            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    document.Save(writer);
                }

                return stream.ToArray();
            }
        }

        private static void Put(ZipArchive zip, string part, byte[] bytes)
        {
            ZipArchiveEntry old = zip.GetEntry(part);

            if (old != null)
            {
                old.Delete();
            }

            ZipArchiveEntry entry = zip.CreateEntry(part, CompressionLevel.Optimal);

            using (Stream stream = entry.Open())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }

    /// <summary>One linked picture: the sheet row it sits on, counted from one, and the jpg it shows.</summary>
    internal sealed class LinkedPicture
    {
        internal LinkedPicture(int row, string imagePath)
        {
            Row = row;
            ImagePath = imagePath;
        }

        internal int Row { get; private set; }

        internal string ImagePath { get; private set; }
    }
}
