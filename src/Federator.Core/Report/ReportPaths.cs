using System;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// Where a workbook goes. One place builds the path, so the path written to and the
    /// path looked for cannot drift, exactly as OutputPaths does for the NWF and the NWD.
    /// </summary>
    public static class ReportPaths
    {
        /// <summary>
        /// The subfolder used when no Excel folder was picked. A setting, not a constant.
        /// </summary>
        public const string DefaultSubfolder = "Clash Reports";

        public const string WorkbookExtension = ".xlsx";

        public const string XmlExtension = ".xml";

        /// <summary>
        /// The folder the workbooks go in. Whatever was picked, and when nothing was
        /// picked, a subfolder beside the NWF folder so the reports sit next to the models
        /// they came from rather than somewhere nobody looks.
        /// </summary>
        public static string Folder(string pickedFolder, string nwfFolder)
        {
            if (!string.IsNullOrEmpty(pickedFolder) && pickedFolder.Trim().Length > 0)
            {
                return pickedFolder.Trim();
            }

            if (string.IsNullOrEmpty(nwfFolder))
            {
                throw new ArgumentException(
                    "With no Excel folder picked, the NWF folder is what the default is built from.",
                    "nwfFolder");
            }

            return Path.Combine(nwfFolder, DefaultSubfolder);
        }

        /// <summary>
        /// The workbook for one group, named like the group's other outputs so all three
        /// sit together under one name. Overwrites, the same as the NWF and the NWD.
        /// </summary>
        public static string Workbook(string folder, string outputName)
        {
            return For(folder, outputName, WorkbookExtension);
        }

        /// <summary>The optional XML, beside the workbook and under the same name.</summary>
        public static string Xml(string folder, string outputName)
        {
            return For(folder, outputName, XmlExtension);
        }

        public static string For(string folder, string outputName, string extension)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            if (string.IsNullOrEmpty(outputName))
            {
                throw new ArgumentException("A report needs an output name.", "outputName");
            }

            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("A report needs an extension.", "extension");
            }

            return Path.Combine(folder, outputName + extension);
        }
    }
}
