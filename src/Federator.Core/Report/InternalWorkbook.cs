using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Coverage;
using Federator.Core.Diagnostics;
using Federator.Core.Generic;

namespace Federator.Core.Report
{
    /// <summary>
    /// The internal workbook of one group, Bader's answer B to Q146 of 11 Oct 2026: a workbook of its own
    /// beside the clash workbook in the Clash Reports folder, named by GenericModelsSettings.WorkbookNameFor,
    /// holding the Generic Models sheet of F128 and the Coverage sheet of F127, in one save. The clash
    /// workbook goes to the client and keeps the Navisworks export's layout, so nothing of ours goes in it.
    ///
    /// A GROUP WRITES WHAT IT HAS. A Generic Models count and no coverage writes the Generic Models sheet
    /// alone, a coverage and no count writes the Coverage sheet alone, and neither is refused here, so the
    /// caller says why no workbook was written. Where the two sheets would carry one name to Excel, the
    /// Generic Models sheet is left out by GenericSheet.WhyRefused, the one rule for it, and the caller logs
    /// that rule's words.
    ///
    /// Overwrites, as every output of a group does. The caller reads the size back off the disk, this never
    /// reports one.
    /// </summary>
    public static class InternalWorkbook
    {
        /// <summary>
        /// Writes the sheets handed in, the Generic Models sheet first and then the Coverage sheet, and returns
        /// the names of the sheets written in that order, the names Excel holds after the floor every sheet
        /// name goes through.
        /// </summary>
        public static IList<string> Write(
            string path,
            string group,
            GenericModelsReport generic,
            string genericSheetName,
            CoverageSheetData coverage,
            string coverageSheetName)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("A workbook needs a path.", "path");
            }

            if (generic == null && coverage == null)
            {
                throw new ArgumentException(
                    "The internal workbook needs a Generic Models count or a coverage, and neither was handed in.", "generic");
            }

            string folder = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            bool writesGeneric = generic != null && WhyNoGenericSheet(coverage, genericSheetName, coverageSheetName) == null;
            List<string> written = new List<string>();

            using (XLWorkbook workbook = new XLWorkbook())
            {
                if (writesGeneric)
                {
                    GenericSheet.Write(workbook, generic, group, genericSheetName);
                }

                if (coverage != null)
                {
                    CoverageSheet.Write(workbook, coverage, coverageSheetName);
                }

                foreach (IXLWorksheet sheet in workbook.Worksheets)
                {
                    written.Add(sheet.Name);
                }

                workbook.SaveAs(path);
            }

            return written;
        }

        /// <summary>
        /// Why the Generic Models sheet is left out of a workbook holding that coverage, or null where it goes
        /// in: the two would carry one name to Excel. GenericSheet.WhyRefused's words, with no Coverage sheet to
        /// meet where no coverage is written.
        /// </summary>
        public static string WhyNoGenericSheet(CoverageSheetData coverage, string genericSheetName, string coverageSheetName)
        {
            return GenericSheet.WhyRefused(
                coverage == null ? null : SheetNames.Sanitise(coverageSheetName, CoverageSettings.DefaultSheetName),
                genericSheetName);
        }

        /// <summary>
        /// The internal workbook read back off the disk by its sheets' names, against the names Write returned.
        /// One line saying it holds them where it does, and otherwise a line for each sheet written and not
        /// found, each sheet found and not written, or the file that would not open with what it threw. Never
        /// throws, because a report check never fails a group.
        /// </summary>
        public static IList<string> Check(string path, IList<string> written)
        {
            List<string> lines = new List<string>();
            List<string> held = new List<string>();
            List<string> wanted = new List<string>(written ?? new List<string>());

            try
            {
                using (XLWorkbook workbook = new XLWorkbook(path))
                {
                    foreach (IXLWorksheet sheet in workbook.Worksheets)
                    {
                        held.Add(sheet.Name);
                    }
                }
            }
            catch (Exception error)
            {
                lines.Add("the internal workbook could not be read back, " + error.GetType().Name + ": "
                    + error.Message.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " "));
                return lines;
            }

            foreach (string name in wanted)
            {
                if (!held.Contains(name))
                {
                    lines.Add("the sheet " + name + " was written and the workbook read back holds no sheet of that name");
                }
            }

            foreach (string name in held)
            {
                if (!wanted.Contains(name))
                {
                    lines.Add("the workbook read back holds a sheet " + name + " this run did not write");
                }
            }

            if (lines.Count == 0)
            {
                lines.Add("read back, it holds " + Words.Counted(held.Count, "sheet", "sheets") + ", "
                    + string.Join(", ", held.ToArray()) + ", each found by the name it was written under");
            }

            return lines;
        }
    }
}
