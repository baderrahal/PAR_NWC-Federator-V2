using System;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Generic;

namespace Federator.Core.Report
{
    /// <summary>
    /// The Generic Models workbook of one group, F128 and FR-177: one workbook of its own beside the group's
    /// workbook, holding the Generic Models sheet alone. A workbook of its own because FR-200 makes the
    /// Coverage sheet the second and last sheet of the group's workbook and the workbook check allows nothing
    /// after it, and because the count does not depend on the clash step, so a group whose clash was skipped
    /// still gets it. Overwrites, as every output of a group does. The caller reads the size back off the disk,
    /// this never reports one.
    /// </summary>
    public static class GenericWorkbook
    {
        public static void Write(string path, GenericModelsReport report, string group, string sheetName)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("A workbook needs a path.", "path");
            }

            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            string folder = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (XLWorkbook workbook = new XLWorkbook())
            {
                GenericSheet.Write(workbook, report, group, sheetName);
                workbook.SaveAs(path);
            }
        }
    }
}
