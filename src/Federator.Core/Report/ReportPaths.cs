using System;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// Where a workbook goes. One place builds the path, so the path written to and the
    /// path looked for cannot drift, exactly as OutputPaths does for the NWF and the NWD.
    /// </summary>
    /// <summary>Where the reports go, and why, when a picked folder had to be refused.</summary>
    public sealed class ReportFolderChoice
    {
        internal ReportFolderChoice(string folder, string refusedReason)
        {
            Folder = folder;
            RefusedReason = refusedReason;
        }

        public string Folder { get; private set; }

        /// <summary>Null unless a picked folder was refused and the default used instead.</summary>
        public string RefusedReason { get; private set; }

        public bool WasRefused
        {
            get { return !string.IsNullOrEmpty(RefusedReason); }
        }
    }

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
        internal static string Folder(string pickedFolder, string nwfFolder)
        {
            return Choose(pickedFolder, nwfFolder, null).Folder;
        }

        /// <summary>
        /// Where the reports will go, in words a person would say, for the line under the
        /// Outputs step. Never throws and never shows a code identifier.
        ///
        /// It said "Workbooks go in UNKNOWN" before a scan, with
        /// "Parameter name: nwfFolder" behind it, because the empty NWF folder threw and
        /// the exception message went straight to the label. Nothing is wrong at that
        /// point. The folder has simply not been picked yet, and that is what it says.
        /// </summary>
        public static string WhereTheyGo(string pickedFolder, string nwfFolder, string sourceFolder)
        {
            string nwf = nwfFolder == null ? string.Empty : nwfFolder.Trim();
            string picked = pickedFolder == null ? string.Empty : pickedFolder.Trim();

            if (nwf.Length == 0)
            {
                return picked.Length > 0
                    ? picked
                    : "beside the NWF folder, once one is picked on this step";
            }

            try
            {
                ReportFolderChoice where = Choose(picked, nwf, sourceFolder);

                return where.WasRefused
                    ? where.Folder + ". " + where.RefusedReason
                    : where.Folder;
            }
            catch (Exception)
            {
                return "not worked out yet, the NWF folder on this step cannot be read";
            }
        }

        /// <summary>
        /// Where the reports go, and whether a picked folder had to be refused.
        ///
        /// The reports never go inside the folder being scanned. A real run put them in
        /// C:\00_NM\NWC Fed\NWC\test001, which is where the NWC files it was reading
        /// live. The Clash step picks an XML at run time, so a report written there is a
        /// file a later run can be pointed at as its own input, and even where it is not
        /// it fills the source folder with output.
        /// </summary>
        public static ReportFolderChoice Choose(
            string pickedFolder, string nwfFolder, string sourceFolder)
        {
            string picked = pickedFolder == null ? string.Empty : pickedFolder.Trim();
            string fallback = Beside(nwfFolder);

            if (picked.Length == 0)
            {
                return Refuse(fallback, sourceFolder, null);
            }

            if (!IsInside(picked, sourceFolder))
            {
                return new ReportFolderChoice(picked, null);
            }

            return Refuse(
                fallback,
                sourceFolder,
                "The Excel folder " + picked + " is inside the folder being scanned, "
                    + Trimmed(sourceFolder)
                    + ". Reports are not written into the folder they were read from, so they "
                    + "went to " + fallback + " instead.");
        }

        private static ReportFolderChoice Refuse(
            string fallback, string sourceFolder, string reason)
        {
            if (!IsInside(fallback, sourceFolder))
            {
                return new ReportFolderChoice(fallback, reason);
            }

            // The NWF folder is itself inside the source folder, so there is nowhere safe
            // to default to. Said rather than written somewhere surprising.
            throw new ArgumentException(
                "The NWF folder " + Trimmed(fallback) + " is inside the folder being scanned, "
                    + Trimmed(sourceFolder)
                    + ", so there is nowhere to put the reports that is not the folder they "
                    + "were read from. Pick an Excel folder outside it.",
                "nwfFolder");
        }

        private static string Beside(string nwfFolder)
        {
            if (string.IsNullOrEmpty(nwfFolder) || nwfFolder.Trim().Length == 0)
            {
                throw new ArgumentException(
                    "With no Excel folder picked, the NWF folder is what the default is built from.",
                    "nwfFolder");
            }

            return Path.Combine(nwfFolder.Trim(), DefaultSubfolder);
        }

        /// <summary>
        /// True when one folder is the other or sits under it. Compared on full paths
        /// without case, which is how Windows compares them, and with a separator on the
        /// end so C:\out\NWC does not read as inside C:\out\NWCFed.
        /// </summary>
        public static bool IsInside(string folder, string outer)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(outer)
                || outer.Trim().Length == 0)
            {
                return false;
            }

            string one = WithSeparator(folder);
            string two = WithSeparator(outer);

            return one.StartsWith(two, StringComparison.OrdinalIgnoreCase);
        }

        private static string WithSeparator(string folder)
        {
            string full;

            try
            {
                full = Path.GetFullPath(folder.Trim());
            }
            catch (Exception)
            {
                // A path Windows will not expand is compared as it came, which is still
                // better than treating it as safe.
                full = folder.Trim();
            }

            return full.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? full
                : full + Path.DirectorySeparatorChar;
        }

        private static string Trimmed(string value)
        {
            return value == null ? string.Empty : value.Trim();
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
