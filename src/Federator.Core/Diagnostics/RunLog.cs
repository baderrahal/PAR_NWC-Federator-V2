using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

using Federator.Core.Rerun;

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

        /// <summary>
        /// Every distinct failure seen so far, so a repeat is recognised rather than
        /// written out again. Keyed on what was being done plus the whole laid out
        /// detail, which is the type, the message, the inner exceptions and the stack.
        /// </summary>
        private readonly Dictionary<string, LoggedFailure> repeats =
            new Dictionary<string, LoggedFailure>(StringComparer.Ordinal);

        /// <summary>A separator that cannot turn up inside a message or a stack.</summary>
        private const string FailureSeparator = "\u001F";

        private static readonly char[] SplitOnNewLine = { '\n' };

        private static readonly char[] TrimCarriageReturn = { '\r' };

        // One list, not a count on one side and a reason list on the other. The RESULT
        // block once printed "groups failed: 22" and "Nothing failed." together, because
        // the count came from a dictionary and the errors came from a separate list that
        // only exceptions reached. Everything about group outcomes now derives from here.
        private readonly List<GroupRecord> groupRecords = new List<GroupRecord>();
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
            Line("plugin version : " + Words.Or(pluginVersion, "UNKNOWN"));
            Line("navisworks     : " + Words.Or(navisworksVersion, "UNKNOWN"));
            Line("open document  : " + Words.Or(openDocument, "none"));
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
            Line("source folder     : " + Words.Or(sourceFolder, "none"));
            Line("include subfolders: " + (includeSubfolders ? "yes" : "no"));
            Line("NWF folder        : " + Words.Or(nwfFolder, "none"));
            Line("NWD folder        : " + Words.Or(nwdFolder, "none"));
            Line("files found       : " + filesFound);
            Line("files ticked      : " + filesTicked);
            Line("groups built      : " + groupsBuilt);
        }

        // ---------- one line per action ----------

        public const string GroupsSectionTitle = "GROUPS";
        public const string FindingsSectionTitle = "FINDINGS";

        /// <summary>
        /// The one line after the group list saying how many groups were unticked in the
        /// Run column. F27. The list used to print skipped for an unticked group, and the
        /// run log of 2026-09-07 was read as a hidden rule dropping 12 of 26 groups. There
        /// is no such rule: nothing in the code unticks a group except the Run column and
        /// a blocked group, so the line says so.
        /// </summary>
        public static string UntickedGroupsLine(int unticked)
        {
            return unticked + (unticked == 1 ? " group" : " groups")
                + " unticked in the Run column, nothing else drops a group";
        }

        /// <summary>
        /// Where the Revit container inside an NWC is a different building from the NWC.
        /// Only knowable once a document is open, so it goes in after the run rather than
        /// with the scan findings.
        /// </summary>
        public const string SourceFindingsSectionTitle = "SOURCE FINDINGS";

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
            Detail("reason   : " + Words.Or(reason, "UNKNOWN"));
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

        /// <summary>
        /// Records how one group ended, with the reason when it did not end cleanly, and
        /// the GROUP finished line that carries which of the two workflows it took, in the
        /// words of Federator.Core.Rerun.RunPath. The label goes on the line and into the
        /// RESULT block totals, so a log alone shows how many groups took each path. Null
        /// where the caller does not know, and then no path is written.
        ///
        /// A group recorded as Failed always carries a reason. When the caller gives none
        /// one is substituted rather than thrown over, because logging must never be the
        /// thing that stops a run, and a substituted reason still keeps the RESULT block
        /// honest by naming the group and saying the reason is missing.
        /// </summary>
        public void GroupFinished(
            string building, GroupOutcome outcome, double seconds, string reason, string runPath)
        {
            string recorded = reason;

            if (outcome == GroupOutcome.Failed && string.IsNullOrEmpty(recorded))
            {
                recorded = "UNKNOWN, the group was recorded as failed and no reason was given";
            }

            lock (gate)
            {
                groupRecords.Add(new GroupRecord(building, outcome, recorded, runPath));
            }

            Line("GROUP    finished " + building + "  " + outcome.ToString().ToUpperInvariant()
                + "  " + seconds.ToString("0.000", CultureInfo.InvariantCulture) + "s"
                + (string.IsNullOrEmpty(runPath) ? string.Empty : "  " + runPath)
                + (string.IsNullOrEmpty(recorded) ? string.Empty : "  " + recorded));
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
        /// An output this run did not write, with why, in the same shape as the written
        /// ones so every output has one line whichever way it went. Records nothing,
        /// because nothing was written.
        /// </summary>
        public void WriteSkipped(string kind, string reason)
        {
            Line(kind.PadRight(8) + " skipped  " + (string.IsNullOrEmpty(reason) ? "UNKNOWN" : reason));
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
                // One entry per file, so "files written" stays a count of files rather
                // than of writes. The SIZE is the latest one read, because a file written
                // twice is bigger the second time and the first number is then a lie.
                //
                // The NWF is the case this exists for. It is saved once after the models
                // are appended and again after the clash step, and a run on 2026-09-01
                // reported it at 4,141 bytes in the RESULT block, the size of the empty
                // federation, while the second save had read 165,844 bytes off the disk
                // and printed it four minutes earlier. Nothing had shrunk. The entry was
                // simply never updated, which reads exactly like an NWF that lost every
                // clash result it held.
                bool already = false;

                foreach (WrittenFile seen in written)
                {
                    if (string.Equals(seen.Path, path, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(seen.Kind, kind, StringComparison.Ordinal))
                    {
                        seen.SizeInBytes = size;
                        already = true;
                        break;
                    }
                }

                if (!already)
                {
                    written.Add(new WrittenFile(kind, path, size));
                }
            }

            Line(kind.PadRight(8) + " written  " + path + "  " + DescribeSize(size));
            return size;
        }

        /// <summary>
        /// Reads a file this run already wrote, one more time, and says whether it is
        /// still the size it was. Updates the recorded entry so the RESULT block carries
        /// the last state rather than the first.
        ///
        /// This exists for the NWF. The clash tests and every clash result live inside it
        /// and are the only record of what has been fixed, and the NWD is published after
        /// it. Nothing was checking that the NWF was still whole once that had happened,
        /// so a run could have destroyed a week of review and said nothing. Now it looks.
        ///
        /// Returns the size, or minus one when the file has gone.
        /// </summary>
        public long ConfirmStillWhole(string kind, string path, string after)
        {
            long size = SizeOnDisk(path);
            long before = -1;

            lock (gate)
            {
                foreach (WrittenFile seen in written)
                {
                    if (string.Equals(seen.Path, path, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(seen.Kind, kind, StringComparison.Ordinal))
                    {
                        before = seen.SizeInBytes;

                        if (size >= 0)
                        {
                            seen.SizeInBytes = size;
                        }

                        break;
                    }
                }
            }

            if (size < 0)
            {
                Line(kind.PadRight(8) + " GONE     " + path
                    + "  it is not on disk after " + after);
                return -1;
            }

            if (before < 0)
            {
                Line(kind.PadRight(8) + " checked  " + path + "  " + DescribeSize(size)
                    + " after " + after);
                return size;
            }

            if (size == before)
            {
                Line(kind.PadRight(8) + " intact   " + path + "  " + DescribeSize(size)
                    + ", unchanged by " + after);
                return size;
            }

            Line(kind.PadRight(8) + " CHANGED  " + path + "  was " + DescribeSize(before)
                + ", is now " + DescribeSize(size) + " after " + after
                + (size < before
                    ? ". It got SMALLER. The clash results live in this file."
                    : "."));

            return size;
        }

        /// <summary>
        /// Looks at a file and reports its size WITHOUT recording it as written by this
        /// run. Used where the outputs are checked after something threw, because a file
        /// that was already sitting at the path last week is not one this run produced,
        /// and the result block's files written list says every size in it was read back
        /// after a write.
        /// </summary>
        public long CheckOnDisk(string kind, string path)
        {
            long size = SizeOnDisk(path);

            Line(kind.PadRight(8) + " checked  " + path + "  "
                + (size < 0 ? "NOT ON DISK" : DescribeSize(size) + ", not written by this run"));

            return size;
        }

        // ---------- failures ----------

        /// <summary>
        /// One failure, with its type, message, inner exceptions and stack trace, then
        /// what the tool did next. A swallowed failure is the one bug this log exists to
        /// prevent, so nothing calls this without saying what happened after.
        ///
        /// The same failure repeating is written out in full once and counted after
        /// that. One run threw the same ObjectDisposedException tens of thousands of
        /// times and left a 17.8 MB log that was almost entirely one stack trace, which
        /// buries every line that says what actually happened. Nothing is lost, because
        /// every repeat is counted and the total goes in the RESULT block beside the
        /// one trace.
        /// </summary>
        public void Failure(string what, Exception error, string whatNext)
        {
            string detail = Describe(error);
            bool firstTime;
            int times;

            lock (gate)
            {
                LoggedFailure already;
                string signature = Words.Or(what, string.Empty) + FailureSeparator + detail;

                if (repeats.TryGetValue(signature, out already))
                {
                    already.AgainOnce();
                    times = already.Times;
                    firstTime = false;
                }
                else
                {
                    LoggedFailure first = new LoggedFailure(what, detail, whatNext);
                    failures.Add(first);
                    repeats.Add(signature, first);
                    times = 1;
                    firstTime = true;
                }
            }

            if (firstTime)
            {
                Line("FAILURE  " + Words.Or(what, "unnamed failure"));

                foreach (string line in detail.Split(SplitOnNewLine))
                {
                    Detail(line.TrimEnd(TrimCarriageReturn));
                }

                Detail("next     : " + Words.Or(whatNext, "UNKNOWN"));
                return;
            }

            // One line saying the repeats are being counted rather than written out, then
            // silence. The RESULT block carries the total beside the one trace.
            if (times == 2)
            {
                Line("FAILURE  the same failure again for " + Words.Or(what, "unnamed failure")
                    + ". Every further repeat of this exact trace is counted, not written out.");
            }
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
                int count = 0;

                foreach (GroupRecord record in groupRecords)
                {
                    if (record.Outcome == outcome)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Every group that ended Failed, each carrying its reason. This is the same list
        /// CountOf(Failed) counts, so a failed count without a matching entry here cannot
        /// happen.
        /// </summary>
        public IList<GroupRecord> FailedGroups
        {
            get
            {
                lock (gate)
                {
                    List<GroupRecord> failed = new List<GroupRecord>();

                    foreach (GroupRecord record in groupRecords)
                    {
                        if (record.Outcome == GroupOutcome.Failed)
                        {
                            failed.Add(record);
                        }
                    }

                    return failed;
                }
            }
        }

        /// <summary>
        /// A copy of the group records, taken under the lock, for this class alone.
        /// Private, because the RESULT block is the only reader and a test reads the
        /// block off the disk rather than the list behind it.
        /// </summary>
        private IList<GroupRecord> GroupRecords
        {
            get
            {
                lock (gate)
                {
                    return new List<GroupRecord>(groupRecords);
                }
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

            // Which of the two workflows each group took, so a log alone answers how many
            // were a First run and how many a Weekly run. Only where the groups carried a
            // path at all, so a log from a caller that never said stays as it was.
            List<string> paths = new List<string>();

            foreach (GroupRecord record in GroupRecords)
            {
                if (!string.IsNullOrEmpty(record.RunPath))
                {
                    paths.Add(record.RunPath);
                }
            }

            if (paths.Count > 0)
            {
                Blank();

                foreach (string line in RunPath.ResultLines(paths))
                {
                    Line(line);
                }
            }

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

            // Both halves of this come from the lists the counts above were taken from,
            // so "groups failed: 22" and "Nothing failed." can no longer both be true.
            IList<GroupRecord> failedGroups = FailedGroups;
            IList<LoggedFailure> errors = Failures;
            int total = failedGroups.Count + errors.Count;
            Blank();

            if (total == 0)
            {
                Line("Nothing failed.");
            }
            else
            {
                Line("errors         : " + total + ", repeated here in full");

                int numbered = 0;

                foreach (GroupRecord record in failedGroups)
                {
                    numbered++;
                    Blank();
                    Line("  [" + numbered + "] group " + record.Building + " ended FAILED");
                    Detail("reason   : " + Words.Or(record.Reason, "UNKNOWN"));
                }

                for (int i = 0; i < errors.Count; i++)
                {
                    numbered++;
                    Blank();
                    // The trace is written once however many times it happened, so the
                    // count has to be on the line beside it or the log understates what
                    // went wrong by tens of thousands.
                    Line("  [" + numbered + "] " + errors[i].What
                        + (errors[i].Times > 1
                            ? "   THIS HAPPENED " + errors[i].Times
                                + " TIMES, the trace is written once"
                            : string.Empty));

                    foreach (string line in errors[i].Detail.Split('\n'))
                    {
                        Detail(line.TrimEnd('\r'));
                    }

                    Detail("next     : " + Words.Or(errors[i].WhatNext, "UNKNOWN"));
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
