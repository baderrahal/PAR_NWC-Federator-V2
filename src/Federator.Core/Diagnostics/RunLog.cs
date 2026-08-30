using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The diagnostic log. Every line is written and flushed to the disk as it happens,
    /// so a hard crash inside a Navisworks call still leaves everything up to that moment
    /// on disk. Nothing is held back until the end.
    /// </summary>
    public sealed class RunLog : IDisposable
    {
        public const string AppFolderName = "ParsonsNwcFederator";
        public const string LogsFolderName = "logs";
        public const string FileNamePrefix = "run-";
        public const string FileNameExtension = ".log";

        /// <summary>
        /// How many log files the folder is left holding, the file being written
        /// included. A setting, passed to the Start overload that takes it.
        /// </summary>
        public const int DefaultKeepLogs = 30;

        /// <summary>
        /// Only files matching this are ever considered for deletion, so nothing else
        /// that happens to be sitting in the folder is touched.
        /// </summary>
        public const string LogFileSearchPattern = FileNamePrefix + "*" + FileNameExtension;

        private const string TimeFormat = "HH:mm:ss.fff";
        private const string Indent = "                          ";

        private readonly object gate = new object();
        private readonly FileStream stream;
        private readonly StreamWriter writer;
        private readonly Stopwatch clock;
        private readonly StringBuilder mirror = new StringBuilder();
        private readonly List<WrittenFile> written = new List<WrittenFile>();
        private readonly List<LoggedFailure> failures = new List<LoggedFailure>();
        private readonly Dictionary<GroupOutcome, int> groupCounts = new Dictionary<GroupOutcome, int>();
        private bool closed;

        private RunLog(string path, DateTime startedAt, FileStream stream, string disabledReason)
        {
            Path = path;
            StartedAt = startedAt;
            DisabledReason = disabledReason;
            this.stream = stream;

            if (stream != null)
            {
                writer = new StreamWriter(stream, new UTF8Encoding(true));
            }

            clock = Stopwatch.StartNew();

            groupCounts[GroupOutcome.Done] = 0;
            groupCounts[GroupOutcome.Partial] = 0;
            groupCounts[GroupOutcome.Failed] = 0;
        }

        /// <summary>False when no file could be opened anywhere. Lines still reach the window.</summary>
        public bool IsWritingToDisk
        {
            get { return writer != null; }
        }

        /// <summary>Null while the log is writing to disk, the reason when it is not.</summary>
        public string DisabledReason { get; private set; }

        /// <summary>
        /// The fixed folder. It never depends on any folder the user picked, so a bad
        /// output path still leaves a log somewhere findable.
        /// </summary>
        public static string DefaultLogFolder()
        {
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return System.IO.Path.Combine(System.IO.Path.Combine(local, AppFolderName), LogsFolderName);
        }

        public static RunLog Start()
        {
            return Start(DefaultLogFolder(), DateTime.Now);
        }

        /// <summary>
        /// Opens the log and never throws. The fixed folder is tried first, then the temp
        /// folder. If neither can be opened the log carries on with nothing behind it, so
        /// the lines still reach the window and logging is never the thing that stops a
        /// run. Check <see cref="IsWritingToDisk"/> to find out which happened.
        /// </summary>
        public static RunLog StartOrDisabled()
        {
            return StartOrDisabled(DefaultLogFolder(), DateTime.Now, DefaultKeepLogs);
        }

        public static RunLog StartOrDisabled(string preferredFolder, DateTime startedAt)
        {
            return StartOrDisabled(preferredFolder, startedAt, DefaultKeepLogs);
        }

        public static RunLog StartOrDisabled(string preferredFolder, DateTime startedAt, int keepLogs)
        {
            List<string> tried = new List<string>();

            foreach (string folder in new[] { preferredFolder, System.IO.Path.GetTempPath() })
            {
                if (string.IsNullOrEmpty(folder))
                {
                    continue;
                }

                try
                {
                    return Start(folder, startedAt, keepLogs);
                }
                catch (Exception error)
                {
                    tried.Add(folder + " (" + error.GetType().Name + ": " + error.Message + ")");
                }
            }

            RunLog disabled = new RunLog(
                null, startedAt, null,
                "No log file could be opened. Tried: " + string.Join("; ", tried.ToArray()));

            disabled.Line("LOG DISABLED. " + disabled.DisabledReason);
            disabled.Line("The run carries on. These lines are in the window only, not on disk.");
            return disabled;
        }

        /// <summary>
        /// Opens the file straight away, before any work is done, so a run that dies at
        /// startup still produces one.
        /// </summary>
        public static RunLog Start(string folder, DateTime startedAt)
        {
            return Start(folder, startedAt, DefaultKeepLogs);
        }

        /// <summary>
        /// Opens the file straight away, then prunes the folder back to
        /// <paramref name="keepLogs"/> files, the new one included.
        /// </summary>
        public static RunLog Start(string folder, DateTime startedAt, int keepLogs)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            Directory.CreateDirectory(folder);

            string path = null;
            FileStream stream = null;

            for (int attempt = 0; attempt < 100 && stream == null; attempt++)
            {
                path = System.IO.Path.Combine(folder, FileName(startedAt, attempt));

                try
                {
                    // FileShare.ReadWrite so the file can be read, tailed or copied while
                    // the run is still going.
                    stream = new FileStream(
                        path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite, 1024, false);
                }
                catch (IOException)
                {
                    // Two runs inside the same second. Try the next suffix.
                    stream = null;
                }
            }

            if (stream == null)
            {
                throw new IOException("Could not open a log file in " + folder + ".");
            }

            RunLog log = new RunLog(path, startedAt, stream, null);
            log.Line("Log opened at " + path);

            // After the new file is open, so the live file is in the list and can be held
            // back from deletion by name rather than by hoping it sorts newest.
            log.PruneOldLogs(folder, keepLogs);
            return log;
        }

        /// <summary>
        /// Deletes the oldest logs until <paramref name="keepLogs"/> are left, the file
        /// being written included. The live file is never a candidate. A delete that
        /// fails gets one line saying which file and why, and the run carries on.
        /// Nothing in here is allowed to throw.
        /// </summary>
        private void PruneOldLogs(string folder, int keepLogs)
        {
            try
            {
                FileInfo[] all;

                try
                {
                    all = new DirectoryInfo(folder).GetFiles(LogFileSearchPattern);
                }
                catch (Exception error)
                {
                    Line("RETAIN   could not list " + folder + ", nothing was deleted: "
                        + error.GetType().Name + ": " + error.Message);
                    return;
                }

                string live = FullPathOrSelf(Path);
                List<FileInfo> others = new List<FileInfo>();

                foreach (FileInfo candidate in all)
                {
                    if (!string.Equals(FullPathOrSelf(candidate.FullName), live, StringComparison.OrdinalIgnoreCase))
                    {
                        others.Add(candidate);
                    }
                }

                // Newest first. The name carries the timestamp, so it breaks ties between
                // files written inside the same clock tick.
                others.Sort(NewestFirst);

                // The live file counts towards the total, so one fewer of the rest is kept.
                int keepOthers = Math.Max(0, keepLogs - 1);

                if (others.Count <= keepOthers)
                {
                    return;
                }

                int deleted = 0;
                int refused = 0;

                for (int i = keepOthers; i < others.Count; i++)
                {
                    try
                    {
                        others[i].Delete();
                        deleted++;
                    }
                    catch (Exception error)
                    {
                        refused++;
                        Line("RETAIN   could not delete " + others[i].Name + ": "
                            + error.GetType().Name + ": " + error.Message);
                    }
                }

                Line("RETAIN   keeping " + keepLogs + " logs, deleted " + deleted
                    + ", could not delete " + refused);
            }
            catch (Exception error)
            {
                // Retention is housekeeping. It never stops a run.
                Line("RETAIN   skipped, " + error.GetType().Name + ": " + error.Message);
            }
        }

        private static int NewestFirst(FileInfo left, FileInfo right)
        {
            int byTime = right.LastWriteTimeUtc.CompareTo(left.LastWriteTimeUtc);
            return byTime != 0 ? byTime : string.CompareOrdinal(right.Name, left.Name);
        }

        private static string FullPathOrSelf(string path)
        {
            try
            {
                return string.IsNullOrEmpty(path) ? string.Empty : System.IO.Path.GetFullPath(path);
            }
            catch (Exception)
            {
                return path ?? string.Empty;
            }
        }

        private static string FileName(DateTime startedAt, int attempt)
        {
            string stamp = startedAt.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string suffix = attempt == 0 ? string.Empty : "-" + attempt.ToString(CultureInfo.InvariantCulture);
            return FileNamePrefix + stamp + suffix + FileNameExtension;
        }

        public string Path { get; private set; }

        public DateTime StartedAt { get; private set; }

        public double ElapsedSeconds
        {
            get { return clock.Elapsed.TotalSeconds; }
        }

        /// <summary>Raised for every line, so the window can show it as it is written.</summary>
        public event Action<string> LineWritten;

        // ---------- the primitive ----------

        /// <summary>
        /// One line, stamped and flushed all the way to the disk before returning.
        /// </summary>
        public void Line(string message)
        {
            string stamped = DateTime.Now.ToString(TimeFormat, CultureInfo.InvariantCulture)
                + "  +" + ElapsedSeconds.ToString("0000.000", CultureInfo.InvariantCulture) + "s  "
                + (message ?? string.Empty);

            WriteRaw(stamped);
        }

        /// <summary>A continuation line, indented under the line above and not stamped.</summary>
        public void Detail(string message)
        {
            WriteRaw(Indent + (message ?? string.Empty));
        }

        public void Blank()
        {
            WriteRaw(string.Empty);
        }

        public void Section(string title)
        {
            WriteRaw(string.Empty);
            WriteRaw("================================================================");
            WriteRaw(title);
            WriteRaw("================================================================");
        }

        private void WriteRaw(string line)
        {
            lock (gate)
            {
                mirror.Append(line).Append(Environment.NewLine);

                if (!closed && writer != null)
                {
                    writer.WriteLine(line);
                    writer.Flush();

                    // Flush(true) pushes the operating system buffers to the disk. Without
                    // it a hard crash loses whatever was still in flight, which is exactly
                    // the case this log exists for.
                    stream.Flush(true);
                }
            }

            Action<string> handler = LineWritten;

            if (handler != null)
            {
                handler(line);
            }
        }

        // ---------- header ----------

        /// <summary>
        /// Written as soon as the button is pressed. The folders and the counts are not
        /// known yet, so they go in the run settings block once the user has picked them.
        /// </summary>
        public void Session(string pluginVersion, string navisworksVersion, string openDocument)
        {
            Section("SESSION");
            Line("started        : " + StartedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            Line("plugin version : " + Or(pluginVersion, "UNKNOWN"));
            Line("navisworks     : " + Or(navisworksVersion, "UNKNOWN"));
            Line("open document  : " + Or(openDocument, "none"));
            Line("log file       : " + Path);
        }

        public void RunSettings(
            string sourceFolder,
            bool includeSubfolders,
            string nwfFolder,
            string nwdFolder,
            int filesFound,
            int filesTicked,
            int groupsBuilt)
        {
            Section("RUN SETTINGS");
            Line("source folder     : " + Or(sourceFolder, "none"));
            Line("include subfolders: " + (includeSubfolders ? "yes" : "no"));
            Line("NWF folder        : " + Or(nwfFolder, "none"));
            Line("NWD folder        : " + Or(nwdFolder, "none"));
            Line("files found       : " + filesFound);
            Line("files ticked      : " + filesTicked);
            Line("groups built      : " + groupsBuilt);
        }

        // ---------- one line per action ----------

        public const string GroupsSectionTitle = "GROUPS";
        public const string FindingsSectionTitle = "FINDINGS";

        /// <summary>
        /// A titled block of lines, written in order. Used for the group list and then
        /// the findings, which go after it.
        /// </summary>
        public void Block(string title, IEnumerable<string> lines)
        {
            Section(title);

            if (lines == null)
            {
                return;
            }

            foreach (string line in lines)
            {
                Line(line);
            }
        }

        public void ScanStarted(string folder, bool includeSubfolders)
        {
            Line("SCAN     started  " + folder + (includeSubfolders ? "  including subfolders" : "  top folder only"));
        }

        public void ScanFinished(int found, int readable, int unreadable)
        {
            Line("SCAN     finished " + found + " found, " + readable + " readable, "
                + unreadable + " that cannot be read");
        }

        public void UnreadableFile(string fileName, string reason)
        {
            Line("UNREAD   " + fileName);
            Detail("reason   : " + Or(reason, "UNKNOWN"));
        }

        public void GroupStarted(string building, IList<string> files)
        {
            Line("GROUP    started  " + building + "  " + (files == null ? 0 : files.Count) + " files");

            if (files != null)
            {
                foreach (string file in files)
                {
                    Detail("file     : " + file);
                }
            }
        }

        public void GroupFinished(string building, GroupOutcome outcome, double seconds)
        {
            lock (gate)
            {
                groupCounts[outcome] = groupCounts[outcome] + 1;
            }

            Line("GROUP    finished " + building + "  " + outcome.ToString().ToUpperInvariant()
                + "  " + seconds.ToString("0.000", CultureInfo.InvariantCulture) + "s");
        }

        public void AppendAttempted(string file)
        {
            Line("APPEND   attempt  " + file);
        }

        /// <summary>
        /// The size is read back off the disk here rather than taken from the caller, so
        /// a size that was never read can never reach the log.
        /// </summary>
        public void AppendFinished(string file, bool succeeded)
        {
            long size = SizeOnDisk(file);
            Line("APPEND   " + (succeeded ? "ok      " : "FAILED  ") + file
                + "  " + DescribeSize(size));
        }

        public void WriteAttempted(string kind, string path)
        {
            Line(kind.PadRight(8) + " attempt  " + path);
        }

        /// <summary>
        /// Checks the file is really there, reads its real size, records it for the result
        /// block and logs it. Returns minus one when the file is not on disk, and in that
        /// case nothing is recorded as written.
        /// </summary>
        public long WriteFinished(string kind, string path)
        {
            long size = SizeOnDisk(path);

            if (size < 0)
            {
                Line(kind.PadRight(8) + " MISSING  " + path + "  not on disk after the write");
                return -1;
            }

            lock (gate)
            {
                written.Add(new WrittenFile(kind, path, size));
            }

            Line(kind.PadRight(8) + " written  " + path + "  " + DescribeSize(size));
            return size;
        }

        // ---------- failures ----------

        /// <summary>
        /// A failure with its full type name, message, inner exception and stack trace,
        /// then what the tool did next. A swallowed failure is the one bug this log
        /// exists to prevent, so nothing calls this without saying what happened after.
        /// </summary>
        public void Failure(string what, Exception error, string whatNext)
        {
            string detail = Describe(error);

            lock (gate)
            {
                failures.Add(new LoggedFailure(what, detail, whatNext));
            }

            Line("FAILURE  " + Or(what, "unnamed failure"));

            foreach (string line in detail.Split('\n'))
            {
                Detail(line.TrimEnd('\r'));
            }

            Detail("next     : " + Or(whatNext, "UNKNOWN"));
        }

        private static string Describe(Exception error)
        {
            if (error == null)
            {
                return "type     : none, no exception was supplied";
            }

            StringBuilder text = new StringBuilder();
            text.Append("type     : ").Append(error.GetType().FullName).Append('\n');
            text.Append("message  : ").Append(error.Message).Append('\n');

            Exception inner = error.InnerException;
            int depth = 1;

            while (inner != null && depth <= 5)
            {
                text.Append("inner ").Append(depth).Append("  : ")
                    .Append(inner.GetType().FullName).Append(": ").Append(inner.Message).Append('\n');
                inner = inner.InnerException;
                depth++;
            }

            if (error.InnerException == null)
            {
                text.Append("inner    : none\n");
            }

            text.Append("stack    :\n");
            text.Append(error.StackTrace ?? "   no stack trace was captured");
            return text.ToString();
        }

        // ---------- result block ----------

        public int CountOf(GroupOutcome outcome)
        {
            lock (gate)
            {
                return groupCounts[outcome];
            }
        }

        public IList<WrittenFile> WrittenFiles
        {
            get
            {
                lock (gate)
                {
                    return new List<WrittenFile>(written);
                }
            }
        }

        public IList<LoggedFailure> Failures
        {
            get
            {
                lock (gate)
                {
                    return new List<LoggedFailure>(failures);
                }
            }
        }

        /// <summary>
        /// The last section, so the summary is at the bottom and does not have to be
        /// scrolled for.
        /// </summary>
        public void WriteResultBlock()
        {
            Section("RESULT");

            Line("groups done    : " + CountOf(GroupOutcome.Done));
            Line("groups partial : " + CountOf(GroupOutcome.Partial));
            Line("groups failed  : " + CountOf(GroupOutcome.Failed));

            IList<WrittenFile> files = WrittenFiles;
            Blank();

            if (files.Count == 0)
            {
                Line("files written  : none");
            }
            else
            {
                Line("files written  : " + files.Count + ", every size read back off the disk");

                foreach (WrittenFile file in files)
                {
                    Detail(file.Kind.PadRight(5) + " " + file.Path + "  " + DescribeSize(file.SizeInBytes));
                }
            }

            IList<LoggedFailure> errors = Failures;
            Blank();

            if (errors.Count == 0)
            {
                Line("Nothing failed.");
            }
            else
            {
                Line("errors         : " + errors.Count + ", repeated here in full");

                for (int i = 0; i < errors.Count; i++)
                {
                    Blank();
                    Line("  [" + (i + 1) + "] " + errors[i].What);

                    foreach (string line in errors[i].Detail.Split('\n'))
                    {
                        Detail(line.TrimEnd('\r'));
                    }

                    Detail("next     : " + Or(errors[i].WhatNext, "UNKNOWN"));
                }
            }

            Blank();
            Line("total elapsed  : " + ElapsedSeconds.ToString("0.000", CultureInfo.InvariantCulture) + "s");
        }

        // ---------- the copy next to the outputs ----------

        /// <summary>
        /// Copies the log next to the NWF folder. Returns false and writes the reason into
        /// the fixed log rather than throwing, because logging must never be the thing
        /// that stops a run.
        /// </summary>
        public bool TryCopyTo(string folder, out string copiedPath)
        {
            copiedPath = null;

            try
            {
                if (string.IsNullOrEmpty(folder))
                {
                    Line("COPY     skipped, no folder was given for the second copy");
                    return false;
                }

                if (writer == null)
                {
                    Line("COPY     skipped, there is no log file to copy");
                    return false;
                }

                Directory.CreateDirectory(folder);
                string target = System.IO.Path.Combine(folder, System.IO.Path.GetFileName(Path));

                lock (gate)
                {
                    writer.Flush();
                    stream.Flush(true);
                }

                // Copied through a read rather than File.Copy, because the source is still
                // open for writing.
                using (FileStream from = new FileStream(
                           Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (FileStream to = new FileStream(
                           target, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    from.CopyTo(to);
                }

                copiedPath = target;
                Line("COPY     written  " + target + "  " + DescribeSize(SizeOnDisk(target)));
                return true;
            }
            catch (Exception error)
            {
                Failure(
                    "copying the log next to the NWF folder " + folder,
                    error,
                    "kept going, the log at " + Path + " is the one that matters");
                return false;
            }
        }

        /// <summary>
        /// The whole log as text, for the clipboard. Read back off the disk when there is
        /// a file, so what gets pasted is what was really written. Falls back to what was
        /// held in memory if the file cannot be read for any reason.
        /// </summary>
        public string ReadAll()
        {
            lock (gate)
            {
                if (!closed && writer != null)
                {
                    writer.Flush();
                    stream.Flush(true);
                }

                if (writer == null)
                {
                    return mirror.ToString();
                }
            }

            try
            {
                using (FileStream from = new FileStream(
                           Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(from, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                lock (gate)
                {
                    return mirror.ToString();
                }
            }
        }

        // ---------- helpers ----------

        private static long SizeOnDisk(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    return -1;
                }

                return new FileInfo(path).Length;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static string DescribeSize(long size)
        {
            if (size < 0)
            {
                return "NOT ON DISK";
            }

            return size.ToString("#,##0", CultureInfo.InvariantCulture) + " bytes";
        }

        private static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        public void Dispose()
        {
            lock (gate)
            {
                if (closed)
                {
                    return;
                }

                closed = true;

                try
                {
                    if (writer != null)
                    {
                        writer.Flush();
                        stream.Flush(true);
                        writer.Dispose();
                    }
                }
                catch (Exception)
                {
                    // Closing the log is never worth throwing over.
                }
            }
        }
    }
}
