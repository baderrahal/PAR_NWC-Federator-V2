using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace Federator.Core.Report
{
    /// <summary>
    /// The clash workbook's links read back off the disk, F243, Bader's order of 19 Oct 2026 point 3: the workbook sits beside
    /// its pictures in the Clash report folder, and after it is written every hyperlink target is looked for on the disk, the
    /// count checked and the count missing logged, and a missing one is a failure of the group.
    ///
    /// EVERY HYPERLINK OF THE PACKAGE, read as the zip it is and never through the object that wrote it: each relationship
    /// of the hyperlink type in every .rels part, the Image cell's link of the sheet and the click link of each linked picture
    /// of F169 alike. A relative target is resolved against the folder the workbook sits in, its escapes undone, and a
    /// file:/// one is read as the path it names. The linked picture's own image relationship, F152, is not a hyperlink and
    /// is not read here.
    ///
    /// HELD TO WHAT THE WRITER MADE, attempt 2 on the breaker's S5: the count of links the writer made, WorkbookWriter.LinksWritten,
    /// is the count expected, and a link expected and not read back is missing as much as one whose picture is not there, so a
    /// workbook whose links went is never read as clean.
    /// </summary>
    public sealed class WorkbookLinks
    {
        private readonly List<string> missing = new List<string>();

        private WorkbookLinks(int picturesLinked, int expected)
        {
            PicturesLinked = picturesLinked;
            Expected = expected;
        }

        /// <summary>The pictures the writer linked.</summary>
        internal int PicturesLinked { get; private set; }

        /// <summary>The links the writer made, a cell link and a click link a picture.</summary>
        internal int Expected { get; private set; }

        /// <summary>How many hyperlinks were read back and looked for.</summary>
        internal int Read { get; private set; }

        /// <summary>The full path each link read back names that reaches no file, in the order read, one .tsv row each.</summary>
        public IList<string> Missing
        {
            get { return missing.AsReadOnly(); }
        }

        /// <summary>The links read that reach no file, and the links expected that were not read.</summary>
        internal int MissingCount
        {
            get { return missing.Count + NotInTheWorkbook; }
        }

        private int NotInTheWorkbook
        {
            get { return Math.Max(0, Expected - Read); }
        }

        /// <summary>Why the workbook could not be read back, or null where it was.</summary>
        internal string NotRead { get; private set; }

        /// <summary>Reads every hyperlink of the workbook at that path, looks for its target and holds the count to the writer's. Never throws.</summary>
        public static WorkbookLinks Check(string workbookPath, int picturesLinked, int linksExpected)
        {
            WorkbookLinks links = new WorkbookLinks(picturesLinked, linksExpected);

            try
            {
                string folder = Path.GetDirectoryName(Path.GetFullPath(workbookPath));

                using (ZipArchive zip = ZipFile.OpenRead(workbookPath))
                {
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (!entry.FullName.EndsWith(".rels", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        XDocument rels;

                        using (Stream stream = entry.Open())
                        {
                            rels = XDocument.Load(stream);
                        }

                        foreach (XElement relationship in rels.Root.Elements())
                        {
                            if ((string)relationship.Attribute("Type") != LinkedPictures.HyperlinkRelationship)
                            {
                                continue;
                            }

                            string target = PathOf(folder, (string)relationship.Attribute("Target") ?? string.Empty);
                            links.Read++;

                            if (!File.Exists(target))
                            {
                                links.missing.Add(target);
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                links.NotRead = error.GetType().Name + ": " + error.Message.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
            }

            return links;
        }

        /// <summary>The file a target names: a file:/// address as its path, anything else relative to the workbook's folder.</summary>
        private static string PathOf(string folder, string target)
        {
            if (target.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                return new Uri(target).LocalPath;
            }

            string relative = Uri.UnescapeDataString(target).Replace('/', Path.DirectorySeparatorChar);

            return Path.GetFullPath(Path.Combine(folder, relative));
        }

        /// <summary>The log's line: the pictures linked, the links expected, read and missing, and the first missing named.</summary>
        public string Line()
        {
            string counts = Number(PicturesLinked) + (PicturesLinked == 1 ? " picture" : " pictures") + " linked, "
                + Number(Expected) + " links expected, ";

            if (NotRead != null)
            {
                return "XLSX     links    " + counts + "read UNKNOWN, the workbook could not be read back to look for its links, " + NotRead;
            }

            return "XLSX     links    " + counts + Number(Read) + " read, " + Number(MissingCount) + " missing"
                + (missing.Count > 0
                    ? ", the first " + missing[0]
                    : NotInTheWorkbook > 0 ? ", " + Number(NotInTheWorkbook) + " links expected and not in the workbook" : string.Empty);
        }

        /// <summary>The group's error where a link is missing, or null where none is or the links were not read.</summary>
        public string Error()
        {
            if (NotRead != null || MissingCount == 0)
            {
                return null;
            }

            return "the clash workbook should hold " + Number(Expected) + " links to its pictures, and " + Number(MissingCount)
                + " of the " + Number(Expected) + " links reach no picture on the disk or are not in it, the first "
                + (missing.Count > 0 ? missing[0] : "of them not in the workbook");
        }

        private static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
