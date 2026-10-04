using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// The report files of one group already on the disk under exactly the names this run
    /// would have written, for a group whose clash was skipped and so wrote none, the
    /// breaker's third finding at c5d8aa8. A workbook, a page, an XML and the pictures an
    /// earlier run wrote keep their undated names and read as current, and the tests in the
    /// NWF keep that run's results, so the Excel and the panel agree and are both stale.
    ///
    /// WORDS AND NEVER DELETION. The tool reports and never acts: each path is read by its
    /// exact name, File.Exists or Directory.Exists on one full path, its last written time
    /// and its size taken, and nothing is opened, moved or removed. No wildcard and no
    /// listing, so the pictures folder is named as a folder and its files are not listed.
    /// </summary>
    public static class EarlierReports
    {
        /// <summary>
        /// One line per output of this group found at its name, each the way the note and
        /// the log carry it, or none. reportFolder null is a run with nowhere to report, and
        /// then nothing is looked for.
        /// </summary>
        public static IList<string> Lines(OutputPlan outputs, string reportFolder, string workbookName)
        {
            List<string> lines = new List<string>();

            if (outputs == null || string.IsNullOrEmpty(reportFolder))
            {
                return lines;
            }

            string workbook = ReportPaths.Workbook(reportFolder, workbookName);

            if (outputs.WriteWorkbook)
            {
                AddFile(lines, workbook);
            }

            if (outputs.WriteHtml)
            {
                AddFile(lines, HtmlTabularWriter.PathFor(workbook));
            }

            if (outputs.WriteXml)
            {
                AddFile(lines, ReportPaths.Xml(reportFolder, workbookName));
            }

            if (outputs.WriteImages)
            {
                string pictures = ImageNaming.FolderFor(workbook);

                if (Directory.Exists(pictures))
                {
                    lines.Add("   " + pictures + "  a folder of pictures, last written "
                        + Stamp(Directory.GetLastWriteTime(pictures)) + ", its files not listed");
                }
            }

            return lines;
        }

        private static void AddFile(List<string> lines, string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            FileInfo file = new FileInfo(path);
            lines.Add("   " + path + "  last written " + Stamp(file.LastWriteTime) + ", "
                + file.Length.ToString("#,##0", CultureInfo.InvariantCulture) + " bytes");
        }

        private static string Stamp(DateTime at)
        {
            return at.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
