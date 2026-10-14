using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Federator.Core.Naming;

namespace Federator.Core.Report
{
    /// <summary>
    /// Where the logs and the reports go. One place builds the path, so the path written to and
    /// the path looked for cannot drift, exactly as OutputPaths does for the NWF and the NWD.
    ///
    /// F158, changed by F170 (Bader's answer to Q169). The NWF and the NWD folders hold the NWF and
    /// the NWD and nothing else. The Log folder sits beside them and holds the text type files and
    /// the Excel: the run log and its .tsv, the notes on the shared coordinates, the Excel, the
    /// Internal workbook and the XML. The Clash report folder sits beside the Log folder and holds
    /// the HTML with its _files folder of pictures, so the picture links of the HTML work. The Excel
    /// reaches the pictures by a link that goes up one folder.
    /// </summary>
    public static class ReportPaths
    {
        /// <summary>The name of the folder that holds the HTML and its pictures, beside the Log folder.</summary>
        public const string ClashReportSubfolder = "Clash report";

        /// <summary>
        /// The Clash report folder for a Log folder: its sibling, never inside it. Empty where there is
        /// no Log folder.
        /// </summary>
        public static string ClashReportFolder(string logFolder)
        {
            if (string.IsNullOrEmpty(logFolder))
            {
                return string.Empty;
            }

            string trimmed = logFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string parent = Path.GetDirectoryName(trimmed);

            return string.IsNullOrEmpty(parent)
                ? Path.Combine(trimmed, ClashReportSubfolder)
                : Path.Combine(parent, ClashReportSubfolder);
        }
        /// <summary>The name of the folder the logs and the reports go in.</summary>
        public const string DefaultSubfolder = "Log";

        private static string subfolder = DefaultSubfolder;

        /// <summary>
        /// The name of the folder the logs and the reports go in. A setting and not a constant,
        /// because the name shapes where every report of every run lands. It is static because the
        /// open file run and the line under the Outputs step both work the folder out with no
        /// options object in front of them. A name that is not one folder is refused where it is set.
        /// </summary>
        public static string Subfolder
        {
            get { return subfolder; }

            set
            {
                string wanted = value == null ? string.Empty : value.Trim();

                if (wanted.Length == 0)
                {
                    throw new ArgumentException(
                        "The logs and the reports need a folder name to go in.",
                        "value");
                }

                if (wanted.IndexOfAny(NotInAFolderName) >= 0)
                {
                    throw new ArgumentException(
                        "The folder is one folder name and not a path, so \"" + wanted
                            + "\" cannot be used.",
                        "value");
                }

                subfolder = wanted;
            }
        }

        private static readonly char[] NotInAFolderName = BuildNotInAFolderName();

        /// <summary>
        /// Windows' own list, which carries both slashes, read off FileNames rather than
        /// off whichever platform is running. Asking the platform passed a backslash
        /// straight through in the container.
        /// </summary>
        private static char[] BuildNotInAFolderName()
        {
            return FileNames.Refused;
        }

        public const string WorkbookExtension = ".xlsx";

        public const string XmlExtension = ".xml";

        /// <summary>
        /// Where the logs and the reports will go, in words a person would say, for the line under
        /// the Outputs step. Never throws and never shows a code identifier.
        ///
        /// It said "Workbooks go in UNKNOWN" before a scan, with "Parameter name: nwfFolder" behind
        /// it, because the empty NWF folder threw and the exception message went straight to the
        /// label. Nothing is wrong at that point. The folder has simply not been picked yet.
        /// </summary>
        public static string WhereTheyGo(string nwfFolder, string nwdFolder, string sourceFolder)
        {
            string nwf = nwfFolder == null ? string.Empty : nwfFolder.Trim();

            if (nwf.Length == 0)
            {
                return "beside the NWF and NWD folders, in a folder called " + Subfolder
                    + ", once the NWF folder is picked on this step";
            }

            try
            {
                return LogFolder(nwf, nwdFolder, sourceFolder);
            }
            catch (ArgumentException error) when (error.ParamName == "nwfFolder")
            {
                // The refusal LogFolder throws on purpose, its first line only, since the rest is
                // the framework's parameter name and a label carries none.
                string first = error.Message.Split(new[] { '\r', '\n' }, 2)[0];

                return first.Length > 0
                    ? first
                    : "not worked out yet, the NWF folder on this step cannot be read";
            }
            catch (Exception)
            {
                // A label carries no framework message, so what it threw is not shown.
                return "not worked out yet, the NWF folder on this step cannot be read";
            }
        }

        /// <summary>
        /// The Log folder for a run: beside the NWF and the NWD folders, inside the same group area,
        /// which is the deepest folder that holds both. Where the two are one folder, or one holds
        /// the other, it is the folder above, so the Log folder is never inside the folder the NWF
        /// and the NWD sit in and that folder holds the NWF and the NWD alone. Where the two are on
        /// different drives it is beside the NWF folder. An empty NWD folder reads as the NWF's.
        ///
        /// The logs and the reports never go inside the folder being scanned, because the Clash step
        /// picks an XML at run time and a report written there is a file a later run can be pointed
        /// at as its own input. Said rather than written somewhere surprising.
        /// </summary>
        public static string LogFolder(string nwfFolder, string nwdFolder, string sourceFolder)
        {
            if (string.IsNullOrEmpty(nwfFolder) || nwfFolder.Trim().Length == 0)
            {
                throw new ArgumentException(
                    "The logs and the reports go beside the NWF folder, so it has to be picked first.",
                    "nwfFolder");
            }

            string nwf = Full(nwfFolder);
            string nwd = string.IsNullOrEmpty(nwdFolder) || nwdFolder.Trim().Length == 0 ? nwf : Full(nwdFolder);
            string area = Common(nwf, nwd) ?? Path.GetDirectoryName(nwf);

            if (area == null)
            {
                // A drive root. Nothing is above it, so the Log folder is inside it.
                area = nwf;
            }
            else if (Same(area, nwf) || Same(area, nwd))
            {
                area = Path.GetDirectoryName(area) ?? area;
            }

            string folder = Path.Combine(area, Subfolder);

            if (IsInside(folder, sourceFolder))
            {
                throw new ArgumentException(
                    "The Log folder would be " + folder + ", which is inside the folder being scanned, "
                        + Trimmed(sourceFolder) + ". Logs and reports are not written into the folder they "
                        + "were read from. Pick an NWF folder outside it.",
                    "nwfFolder");
            }

            return folder;
        }

        /// <summary>The deepest folder holding both, or null where they share none, as on two drives.</summary>
        private static string Common(string one, string two)
        {
            string[] a = one.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string[] b = two.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            List<string> shared = new List<string>();

            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                if (!string.Equals(a[i], b[i], StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                shared.Add(a[i]);
            }

            if (shared.Count == 0 || (shared.Count == 1 && shared[0].Length == 0))
            {
                return null;
            }

            string joined = string.Join(Path.DirectorySeparatorChar.ToString(), shared.ToArray());

            // A drive on its own reads C: and needs its separator to be a folder.
            return joined.EndsWith(":", StringComparison.Ordinal)
                ? joined + Path.DirectorySeparatorChar
                : joined;
        }

        private static bool Same(string one, string two)
        {
            return string.Equals(
                one.TrimEnd(Path.DirectorySeparatorChar),
                two.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string Full(string folder)
        {
            try
            {
                return Path.GetFullPath(folder.Trim()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch (Exception)
            {
                return folder.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
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
        /// The workbook for one group, named like the group's other outputs so all of them
        /// sit together under one name.
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

        /// <summary>
        /// The name a group's reports are written under, so that nothing already in the Log folder is
        /// overwritten, F158. The name as it is where none of its files is there: the workbook, the
        /// Internal workbook and the XML in the Log folder, the HTML and the _files folder in the Clash
        /// report folder. Where any is, the name with the moment of the run after it, and a number after
        /// that where even that is taken. Nothing is
        /// deleted or touched, and the name is worked out once per group so the Excel, the HTML and the
        /// pictures folder all carry the same one and their links keep working.
        /// </summary>
        public static string NameThatOverwritesNothing(
            string folder, string clashReportFolder, string workbookName, string internalSuffix, DateTime now,
            Func<string, bool> exists)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(workbookName))
            {
                return workbookName;
            }

            if (exists == null)
            {
                exists = path => File.Exists(path) || Directory.Exists(path);
            }

            if (!AnyOfThemExist(folder, clashReportFolder, workbookName, internalSuffix, exists))
            {
                return workbookName;
            }

            string dated = workbookName + " " + now.ToString("yyyy-MM-dd HHmmss", CultureInfo.InvariantCulture);
            string candidate = dated;

            for (int number = 2; AnyOfThemExist(folder, clashReportFolder, candidate, internalSuffix, exists); number++)
            {
                candidate = dated + " " + number.ToString(CultureInfo.InvariantCulture);
            }

            return candidate;
        }

        private static bool AnyOfThemExist(
            string folder, string clashReportFolder, string name, string internalSuffix, Func<string, bool> exists)
        {
            string workbook = Path.Combine(folder, name + WorkbookExtension);

            if (exists(workbook) || exists(Path.Combine(folder, name + XmlExtension)))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(clashReportFolder))
            {
                string page = Path.Combine(clashReportFolder, name + WorkbookExtension);

                if (exists(HtmlTabularWriter.PathFor(page)) || exists(ImageNaming.FolderFor(page)))
                {
                    return true;
                }
            }

            return !string.IsNullOrEmpty(internalSuffix)
                && exists(Path.Combine(folder, name + " " + internalSuffix + WorkbookExtension));
        }

        /// <summary>
        /// The path for something written into the Log folder that must not replace a file there,
        /// F158: the plain name where it is free, and the name with the moment after it where not.
        /// </summary>
        public static string FreeFilePath(string folder, string fileName, DateTime now, Func<string, bool> exists)
        {
            if (exists == null)
            {
                exists = path => File.Exists(path) || Directory.Exists(path);
            }

            string plain = Path.Combine(folder, fileName);

            if (!exists(plain))
            {
                return plain;
            }

            string stem = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string dated = stem + " " + now.ToString("yyyy-MM-dd HHmmss", CultureInfo.InvariantCulture);
            string candidate = Path.Combine(folder, dated + extension);

            for (int number = 2; exists(candidate); number++)
            {
                candidate = Path.Combine(folder, dated + " " + number.ToString(CultureInfo.InvariantCulture) + extension);
            }

            return candidate;
        }
    }
}
