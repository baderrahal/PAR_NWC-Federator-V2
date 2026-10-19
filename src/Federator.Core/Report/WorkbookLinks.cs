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
    /// </summary>
    public sealed class WorkbookLinks
    {
        private readonly List<string> missing = new List<string>();

        private WorkbookLinks()
        {
        }

        /// <summary>How many hyperlinks were read and looked for.</summary>
        public int Checked { get; private set; }

        /// <summary>The full path each link that reaches no file names, in the order read.</summary>
        public IList<string> Missing
        {
            get { return missing.AsReadOnly(); }
        }

        /// <summary>Why the workbook could not be read back, or null where it was.</summary>
        public string NotRead { get; private set; }

        /// <summary>Reads every hyperlink of the workbook at that path and looks for its target. Never throws.</summary>
        public static WorkbookLinks Check(string workbookPath)
        {
            WorkbookLinks links = new WorkbookLinks();

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
                            links.Checked++;

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

        /// <summary>The log's line: the count read and the count missing, and the first missing one named.</summary>
        public string Line()
        {
            if (NotRead != null)
            {
                return "XLSX     links    UNKNOWN, the workbook could not be read back to look for its links, " + NotRead;
            }

            return "XLSX     links    " + Checked.ToString(CultureInfo.InvariantCulture) + " checked, "
                + missing.Count.ToString(CultureInfo.InvariantCulture) + " missing"
                + (missing.Count == 0 ? string.Empty : ", the first " + missing[0]);
        }

        /// <summary>The group's error where a link reaches no file, or null where none is missing or the links were not read.</summary>
        public string Error()
        {
            return missing.Count == 0
                ? null
                : "the clash workbook holds " + missing.Count.ToString(CultureInfo.InvariantCulture)
                    + (missing.Count == 1 ? " link" : " links") + " to a picture not on the disk, the first " + missing[0];
        }
    }
}
