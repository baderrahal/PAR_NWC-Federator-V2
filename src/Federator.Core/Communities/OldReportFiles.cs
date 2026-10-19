using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Federator.Core.Disciplines;
using Federator.Core.Health;
using Federator.Core.Report;
using Federator.Core.Rerun;

namespace Federator.Core.Communities
{
    /// <summary>
    /// The files earlier runs wrote under names this tool no longer writes, deleted at the start of a run, F243, Bader's order of
    /// 19 Oct 2026 point 7: "no more internal or dated named files". Inside Report\COMMUNITY\ of each community of the run and
    /// nowhere else, never a package's folder below it, and only a file whose name is one of these EXACTLY, every other name
    /// left alone:
    ///
    ///     Log\GROUP.xlsx, the clash workbook before it moved to the Clash report folder
    ///     Log\GROUP STAMP.xlsx and Log\GROUP STAMP N.xlsx
    ///     Log\GROUP Internal.xlsx, Log\GROUP STAMP Internal.xlsx and Log\GROUP STAMP N Internal.xlsx
    ///     Log\Models not on the same shared coordinates, run STAMP.txt
    ///     COMMUNITY DISC checks STAMP.xlsx and COMMUNITY DISC checks STAMP N.xlsx
    ///     COMMUNITY clash count check STAMP.xlsx and COMMUNITY clash count check STAMP N.xlsx
    ///     Backup\GROUP STAMP.nwf and Backup\GROUP STAMP (N).nwf, the newest renamed to Backup\GROUP.nwf where that is free
    ///
    /// GROUP is a group of the community in the run, DISC a discipline of the discipline settings, STAMP the moment the old
    /// names carried, yyyy-MM-dd HHmmss, read as a real moment, and N the counter the old names took, 2 or more. The names
    /// are matched Ordinal, as the tool wrote them. Where Backup\GROUP.nwf is already there the newest dated backup is left as
    /// it is and said, and the older ones go. The patterns live here and nowhere else.
    /// </summary>
    public static class OldReportFiles
    {
        /// <summary>The moment the old names carried after the name, ReportPaths' one format for it.</summary>
        private const string StampFormat = ReportPaths.DatedNameFormat;

        /// <summary>The word the internal workbook's name ended with, the default its suffix had while it was written.</summary>
        private const string InternalWord = "Internal";

        private const string Workbook = ReportPaths.WorkbookExtension;

        private const string Nwf = PackageTree.NwfExtension;

        /// <summary>
        /// Which old name a file of the community's own folder or its Log folder is, by its path below Report\COMMUNITY\, or
        /// null where it is none of them and stays.
        /// </summary>
        public static string KindOf(string relativePath, string community, ICollection<string> groups, DisciplineReportSettings disciplines)
        {
            if (string.IsNullOrEmpty(relativePath) || groups == null || disciplines == null)
            {
                return null;
            }

            string folder = Path.GetDirectoryName(relativePath) ?? string.Empty;
            string name = Path.GetFileName(relativePath);

            if (folder.Length == 0)
            {
                return TopLevelKind(name, community, disciplines);
            }

            return string.Equals(folder, ReportPaths.Subfolder, StringComparison.Ordinal) ? LogKind(name, groups) : null;
        }

        private static string TopLevelKind(string name, string community, DisciplineReportSettings disciplines)
        {
            if (string.IsNullOrEmpty(community))
            {
                return null;
            }

            foreach (string discipline in disciplines.Disciplines)
            {
                if (IsDated(name, Stem(DisciplineWorkbook.NameFor(community, discipline, disciplines)), Workbook))
                {
                    return "a dated discipline workbook";
                }
            }

            return IsDated(name, Stem(ClashCountWorkbook.NameFor(community)), Workbook) ? "a dated clash count check" : null;
        }

        private static string LogKind(string name, ICollection<string> groups)
        {
            string list = Stem(OffCoordinatesAcrossTheRun.ListName);

            if (name.StartsWith(list + ", run ", StringComparison.Ordinal) && name.EndsWith(".txt", StringComparison.Ordinal))
            {
                DateTime moment;
                string stamp = name.Substring(list.Length + ", run ".Length, name.Length - list.Length - ", run ".Length - ".txt".Length);

                if (IsStamp(stamp, out moment))
                {
                    return "a dated list of models not on the same shared coordinates";
                }
            }

            if (!name.EndsWith(Workbook, StringComparison.Ordinal))
            {
                return null;
            }

            string stem = name.Substring(0, name.Length - Workbook.Length);

            foreach (string group in groups)
            {
                if (string.IsNullOrEmpty(group))
                {
                    continue;
                }

                if (string.Equals(stem, group, StringComparison.Ordinal))
                {
                    return "the clash workbook written in the Log folder before it moved to the Clash report folder";
                }

                if (!stem.StartsWith(group + " ", StringComparison.Ordinal))
                {
                    continue;
                }

                string rest = stem.Substring(group.Length + 1);

                if (string.Equals(rest, InternalWord, StringComparison.Ordinal))
                {
                    return "an internal workbook";
                }

                bool isInternal = rest.EndsWith(" " + InternalWord, StringComparison.Ordinal);

                if (isInternal)
                {
                    rest = rest.Substring(0, rest.Length - InternalWord.Length - 1);
                }

                if (IsStampAndCount(rest))
                {
                    return isInternal ? "a dated internal workbook" : "a dated clash workbook";
                }
            }

            return null;
        }

        /// <summary>The stem, the moment and a counter of 2 or more where there is one, and the extension, and nothing else.</summary>
        private static bool IsDated(string name, string stem, string extension)
        {
            if (!name.StartsWith(stem + " ", StringComparison.Ordinal) || !name.EndsWith(extension, StringComparison.Ordinal)
                || name.Length <= stem.Length + 1 + extension.Length)
            {
                return false;
            }

            return IsStampAndCount(name.Substring(stem.Length + 1, name.Length - stem.Length - 1 - extension.Length));
        }

        private static bool IsStampAndCount(string text)
        {
            DateTime moment;
            int count;

            if (text.Length == StampFormat.Length)
            {
                return IsStamp(text, out moment);
            }

            return text.Length > StampFormat.Length + 1 && text[StampFormat.Length] == ' '
                && IsStamp(text.Substring(0, StampFormat.Length), out moment)
                && IsCount(text.Substring(StampFormat.Length + 1), out count);
        }

        private static bool IsStamp(string text, out DateTime moment)
        {
            return DateTime.TryParseExact(text, StampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out moment);
        }

        /// <summary>A counter as the old names wrote it: digits only, no nought in front, 2 or more.</summary>
        private static bool IsCount(string text, out int count)
        {
            count = 0;

            if (string.IsNullOrEmpty(text) || text[0] == '0')
            {
                return false;
            }

            foreach (char c in text)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out count) && count >= 2;
        }

        /// <summary>
        /// A dated backup of that group: Backup\GROUP STAMP.nwf, its counter nought, or Backup\GROUP STAMP (N).nwf. False for
        /// any other name.
        /// </summary>
        public static bool IsDatedBackup(string fileName, string group, out DateTime moment, out int counter)
        {
            moment = DateTime.MinValue;
            counter = 0;

            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(group)
                || !fileName.StartsWith(group + " ", StringComparison.Ordinal) || !fileName.EndsWith(Nwf, StringComparison.Ordinal))
            {
                return false;
            }

            string rest = fileName.Substring(group.Length + 1, fileName.Length - group.Length - 1 - Nwf.Length);

            if (rest.Length == StampFormat.Length)
            {
                return IsStamp(rest, out moment);
            }

            return rest.Length > StampFormat.Length + 3 && rest[StampFormat.Length] == ' '
                && rest[StampFormat.Length + 1] == '(' && rest[rest.Length - 1] == ')'
                && IsStamp(rest.Substring(0, StampFormat.Length), out moment)
                && IsCount(rest.Substring(StampFormat.Length + 2, rest.Length - StampFormat.Length - 3), out counter);
        }

        /// <summary>
        /// Deletes the old names in one community's folder, its Log folder and its Backup folder, and renames the newest dated
        /// backup of each group to its one name where that is free. Each file on its own: one held open is named in InUse, one
        /// that fails otherwise in Failed with what it threw, and the rest go on. Nothing outside the three folders is read.
        /// </summary>
        public static OldReportFilesOutcome Clean(string communityFolder, string community, IEnumerable<string> groups, DisciplineReportSettings disciplines)
        {
            if (disciplines == null)
            {
                throw new ArgumentNullException("disciplines");
            }

            OldReportFilesOutcome outcome = new OldReportFilesOutcome(communityFolder);
            List<string> names = new List<string>();

            foreach (string group in groups ?? new List<string>())
            {
                if (!string.IsNullOrEmpty(group) && !names.Contains(group))
                {
                    names.Add(group);
                }
            }

            if (string.IsNullOrEmpty(communityFolder) || !Directory.Exists(communityFolder))
            {
                return outcome;
            }

            foreach (string file in Directory.GetFiles(communityFolder))
            {
                if (KindOf(Path.GetFileName(file), community, names, disciplines) != null)
                {
                    outcome.Delete(file);
                }
            }

            string log = Path.Combine(communityFolder, ReportPaths.Subfolder);

            if (Directory.Exists(log))
            {
                foreach (string file in Directory.GetFiles(log))
                {
                    if (KindOf(Path.Combine(ReportPaths.Subfolder, Path.GetFileName(file)), community, names, disciplines) != null)
                    {
                        outcome.Delete(file);
                    }
                }
            }

            string backup = Path.Combine(communityFolder, NwfBackupName.FolderName);

            if (Directory.Exists(backup))
            {
                string[] files = Directory.GetFiles(backup);

                foreach (string group in names)
                {
                    CleanTheBackupsOf(group, backup, files, outcome);
                }
            }

            return outcome;
        }

        private static void CleanTheBackupsOf(string group, string backup, string[] files, OldReportFilesOutcome outcome)
        {
            List<KeyValuePair<string, DateTime>> dated = new List<KeyValuePair<string, DateTime>>();
            string newest = null;
            DateTime newestMoment = DateTime.MinValue;
            int newestCounter = -1;

            foreach (string file in files)
            {
                DateTime moment;
                int counter;

                if (!IsDatedBackup(Path.GetFileName(file), group, out moment, out counter))
                {
                    continue;
                }

                dated.Add(new KeyValuePair<string, DateTime>(file, moment));

                if (newest == null || moment > newestMoment || (moment == newestMoment && counter > newestCounter))
                {
                    newest = file;
                    newestMoment = moment;
                    newestCounter = counter;
                }
            }

            if (newest == null)
            {
                return;
            }

            string one = Path.Combine(backup, group + Nwf);

            if (File.Exists(one))
            {
                outcome.Keep(newest, one);
            }
            else
            {
                outcome.Rename(newest, one);
            }

            foreach (KeyValuePair<string, DateTime> file in dated)
            {
                if (!string.Equals(file.Key, newest, StringComparison.Ordinal))
                {
                    outcome.Delete(file.Key);
                }
            }
        }

        private static string Stem(string fileName)
        {
            return fileName.Substring(0, fileName.Length - Path.GetExtension(fileName).Length);
        }
    }

    /// <summary>What the clean of one community's folder did, each file on a line of its own.</summary>
    public sealed class OldReportFilesOutcome
    {
        private readonly List<string> lines = new List<string>();
        private readonly List<string> inUse = new List<string>();
        private readonly List<KeyValuePair<string, Exception>> failed = new List<KeyValuePair<string, Exception>>();

        internal OldReportFilesOutcome(string folder)
        {
            Folder = folder ?? string.Empty;
        }

        /// <summary>The community's folder.</summary>
        public string Folder { get; private set; }

        /// <summary>A line for each file deleted, renamed or kept, in the order done.</summary>
        public IList<string> Lines
        {
            get { return lines.AsReadOnly(); }
        }

        /// <summary>Each file another program held, so it was neither deleted nor renamed.</summary>
        public IList<string> InUse
        {
            get { return inUse.AsReadOnly(); }
        }

        /// <summary>Each file whose delete or rename failed for another reason, with what it threw.</summary>
        public IList<KeyValuePair<string, Exception>> Failed
        {
            get { return failed.AsReadOnly(); }
        }

        /// <summary>How many files were deleted.</summary>
        public int DeletedCount { get; private set; }

        /// <summary>The one line where nothing in the folder bore an old name.</summary>
        public string NothingLine()
        {
            return lines.Count == 0 && inUse.Count == 0 && failed.Count == 0
                ? "OLD      " + Folder + " holds no file under a name this tool no longer writes"
                : null;
        }

        internal void Delete(string path)
        {
            try
            {
                File.Delete(path);
                DeletedCount++;
                lines.Add("OLD      deleted " + path + ", a name this tool no longer writes");
            }
            catch (Exception error)
            {
                Fail(path, error);
            }
        }

        internal void Rename(string from, string to)
        {
            try
            {
                File.Move(from, to);
                lines.Add("OLD      renamed " + from + " to " + to + ", the newest dated backup under the one name a backup has now");
            }
            catch (Exception error)
            {
                Fail(from, error);
            }
        }

        internal void Keep(string newest, string one)
        {
            lines.Add("OLD      kept " + newest + " as it is, the newest dated backup, because " + one + " is already there");
        }

        private void Fail(string path, Exception error)
        {
            if (FileInUse.IsInUse(error))
            {
                inUse.Add(path);
                return;
            }

            failed.Add(new KeyValuePair<string, Exception>(path, error));
        }
    }
}
