using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Autodesk.Navisworks.Api;
using Federator.Core.Diagnostics;
using Federator.Core.Rerun;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Runs the model side. Every Navisworks call here happens on the thread that calls
    /// Run, which is the plugin thread. Nothing in this class touches a background
    /// thread, and nothing here may be moved onto one.
    /// </summary>
    public sealed class FederationEngine
    {
        private readonly Action<string> progress;
        private readonly RunLog log;
        private readonly bool republishNwd;

        public FederationEngine(Action<string> progress, RunLog log)
            : this(progress, log, true)
        {
        }

        public FederationEngine(Action<string> progress, RunLog log, bool republishNwd)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.progress = progress ?? delegate { };
            this.log = log;
            this.republishNwd = republishNwd;
        }

        /// <summary>
        /// Works through the jobs in order. Each group's NWF and NWD are written before
        /// the next group starts, so a failure part way through keeps everything already
        /// written.
        /// </summary>
        public IList<JobOutcome> Run(IList<FederationJob> jobs)
        {
            if (jobs == null)
            {
                throw new ArgumentNullException("jobs");
            }

            List<JobOutcome> outcomes = new List<JobOutcome>();

            for (int i = 0; i < jobs.Count; i++)
            {
                FederationJob job = jobs[i];
                progress(
                    "Group " + (i + 1) + " of " + jobs.Count + ": " + job.Building
                        + " (" + job.Files.Count + " files)");

                Stopwatch groupClock = Stopwatch.StartNew();
                log.GroupStarted(job.Building, job.Files);

                JobOutcome outcome = RunOne(job);
                groupClock.Stop();

                outcomes.Add(outcome);
                log.GroupFinished(
                    job.Building, outcome.Result, groupClock.Elapsed.TotalSeconds, outcome.Reason);
            }

            return outcomes;
        }

        private JobOutcome RunOne(FederationJob job)
        {
            JobOutcome outcome = new JobOutcome(job);
            outcome.NwfSize = -1;
            outcome.NwdSize = -1;
            outcome.NwdRequested = republishNwd;

            try
            {
                Document document = NavisworksApplication.ActiveDocument;

                if (document == null)
                {
                    outcome.Error = "There is no active document.";
                    log.Line("GROUP    " + job.Building + " stopped, there is no active document");
                    return outcome;
                }

                NwfComparison comparison = Decide(document, job);
                outcome.Decision = comparison.Decision;

                foreach (string line in comparison.Lines(job.NwfPath))
                {
                    log.Line(line);
                }

                if (comparison.Decision == RerunDecision.Build)
                {
                    if (!BuildFromScratch(document, job, outcome))
                    {
                        return outcome;
                    }
                }
                else
                {
                    // The NWF is already open, because reading its file list is what
                    // opened it. It is not cleared and nothing is re-appended, so the
                    // clash results inside it survive. A CHANGED group is left alone
                    // entirely and the decision goes to Bader.
                    outcome.AppendedCount = comparison.InNwf.Count;

                    // Verified, not recorded as written. This run did not write it, and
                    // the RESULT block's files written list says every size in it was read
                    // back after a write.
                    outcome.NwfSize = SizeOnDiskOrMinusOne(job.NwfPath);
                    outcome.NwfOnDisk = outcome.NwfSize >= 0;
                    log.Line("NWF      reused   " + job.NwfPath + "  "
                        + (outcome.NwfOnDisk ? outcome.NwfSize.ToString("#,##0") + " bytes" : "NOT ON DISK"));
                }

                WriteNwd(document, job, outcome);
            }
            catch (Exception error)
            {
                outcome.Error = error.Message;
                log.Failure(
                    "federating group " + job.Building,
                    error,
                    "stopped this group, carried on with the next one, everything already written is kept");

                // The outputs are still checked against the disk, because a throw after a
                // successful write must not report the file as missing. Checked, not
                // recorded as written. A group that threw may never have reached either
                // write, and whatever is sitting at these paths could be last week's,
                // which this run did not produce.
                outcome.NwfSize = log.CheckOnDisk("NWF", job.NwfPath);
                outcome.NwdSize = log.CheckOnDisk("NWD", job.NwdPath);
                outcome.NwfOnDisk = outcome.NwfSize >= 0;
                outcome.NwdOnDisk = outcome.NwdSize >= 0;
            }

            return outcome;
        }

        /// <summary>
        /// Works out which of the three cases this group is in. When an NWF is already
        /// there it is opened, because reading the file list out of it is the only way to
        /// compare, and the NWF is the record. No side file is kept.
        /// </summary>
        private NwfComparison Decide(Document document, FederationJob job)
        {
            if (!File.Exists(job.NwfPath))
            {
                return NwfComparison.NoNwfYet(job.Files);
            }

            progress("Opening the existing NWF for " + job.Building);
            log.Line("OPEN     reading the file list out of " + job.NwfPath);

            if (!document.TryOpenFile(job.NwfPath))
            {
                throw new InvalidOperationException(
                    "The NWF at " + job.NwfPath + " is there but would not open, so the group was left alone.");
            }

            return NwfComparison.Compare(FilesInsideTheOpenDocument(), job.Files);
        }

        /// <summary>
        /// The files the open document points at. SourceFileName is the file that was
        /// appended. FileName is also read, and used only when the source is empty, so a
        /// model that reports one and not the other is still counted.
        /// </summary>
        private IList<string> FilesInsideTheOpenDocument()
        {
            List<string> files = new List<string>();
            Document document = NavisworksApplication.ActiveDocument;

            if (document == null || document.Models == null)
            {
                return files;
            }

            foreach (Model model in document.Models)
            {
                string source = model.SourceFileName;
                string cached = model.FileName;
                string use = string.IsNullOrEmpty(source) ? cached : source;

                log.Line("         holds   " + (string.IsNullOrEmpty(use) ? "an unnamed model" : use)
                    + (string.Equals(source, cached, StringComparison.OrdinalIgnoreCase)
                        ? string.Empty
                        : "   [source " + Or(source) + ", file " + Or(cached) + "]"));

                if (!string.IsNullOrEmpty(use))
                {
                    files.Add(use);
                }
            }

            return files;
        }

        private static string Or(string value)
        {
            return string.IsNullOrEmpty(value) ? "none" : value;
        }

        /// <summary>
        /// The only path that clears the document, and it only runs when there is no NWF
        /// at the output path, so there is no clash history to lose. Returns false when
        /// nothing appended and nothing should be written.
        /// </summary>
        private bool BuildFromScratch(Document document, FederationJob job, JobOutcome outcome)
        {
            log.Line("CLEAR    the document, before building " + job.Building + " from scratch");
            document.Clear();

            foreach (string file in job.Files)
            {
                log.AppendAttempted(file);
                bool appended = AppendOne(document, file);
                log.AppendFinished(file, appended);

                if (appended)
                {
                    outcome.AppendedCount++;
                }
                else
                {
                    outcome.FailedFiles.Add(file);
                }
            }

            log.Line("APPEND   " + outcome.AppendedCount + " of " + job.Files.Count
                + " appended for " + job.Building);

            if (outcome.AppendedCount == 0)
            {
                outcome.Error = "No file in this group appended, so nothing was written.";
                log.Line("GROUP    " + job.Building + " wrote nothing, every append failed");
                return false;
            }

            WriteNwf(document, job, outcome);
            return true;
        }

        /// <summary>
        /// TryAppendFile returns false rather than throwing on a bad file, which is what
        /// keeps one broken NWC from stopping the group. It can still throw on something
        /// it did not expect, so that is caught too.
        /// </summary>
        private bool AppendOne(Document document, string file)
        {
            try
            {
                if (!File.Exists(file))
                {
                    log.Line("APPEND   missing on disk, never attempted: " + file);
                    return false;
                }

                return document.TryAppendFile(file);
            }
            catch (Exception error)
            {
                log.Failure(
                    "appending " + file,
                    error,
                    "kept going with the rest of the group, this group will be marked partial");
                return false;
            }
        }

        private void WriteNwf(Document document, FederationJob job, JobOutcome outcome)
        {
            progress("Saving NWF for " + job.Building);
            log.WriteAttempted("NWF", job.NwfPath);

            try
            {
                EnsureFolder(job.NwfPath);

                // The NWF path only runs when there was no NWF there, so a stale file
                // cannot mask a lost save the way it can for the NWD. The bool is still
                // read, because it says more in the log than an absent file does.
                if (!document.TrySaveFile(job.NwfPath))
                {
                    log.Line("NWF      the save returned false for " + job.Building);
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "saving the NWF for " + job.Building,
                    error,
                    "kept going, the disk is checked next to see whether anything landed");
            }

            // Never report a file as written without looking for it. WriteFinished reads
            // the size back off the disk itself.
            outcome.NwfSize = log.WriteFinished("NWF", job.NwfPath);
            outcome.NwfOnDisk = outcome.NwfSize >= 0;
        }

        private void WriteNwd(Document document, FederationJob job, JobOutcome outcome)
        {
            if (!republishNwd)
            {
                log.Line("NWD      not republished, the tick box is off");
                outcome.NwdSize = SizeOnDiskOrMinusOne(job.NwdPath);
                outcome.NwdOnDisk = outcome.NwdSize >= 0;
                return;
            }

            progress("Publishing NWD for " + job.Building);
            log.WriteAttempted("NWD", job.NwdPath);

            bool published = false;

            try
            {
                EnsureFolder(job.NwdPath);

                // PublishProperties is the 2025 way to write an NWD. NwdExportOptions is
                // a 2026 class and does not exist here.
                using (PublishProperties properties = new PublishProperties())
                {
                    properties.Title = job.OutputName;
                    properties.Publisher = "Parsons NWC Federator";
                    properties.Subject = "Federation of building " + job.Building;
                    properties.Author = Environment.UserName;

                    // TryPublishFile returns a bool. Discarding it and trusting
                    // File.Exists would call a stale NWD from last week a success.
                    published = document.TryPublishFile(job.NwdPath, properties);
                }

                if (!published)
                {
                    log.Line("NWD      the publish returned false for " + job.Building);
                }
            }
            catch (Exception error)
            {
                published = false;
                log.Failure(
                    "publishing the NWD for " + job.Building,
                    error,
                    "kept going, the disk is checked next to see whether anything landed");
            }

            outcome.NwdPublishReportedSuccess = published;

            outcome.NwdSize = log.WriteFinished("NWD", job.NwdPath);
            outcome.NwdOnDisk = outcome.NwdSize >= 0;
        }

        private static long SizeOnDiskOrMinusOne(string path)
        {
            try
            {
                return File.Exists(path) ? new FileInfo(path).Length : -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static void EnsureFolder(string filePath)
        {
            string folder = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public static string Describe(JobOutcome outcome)
        {
            StringBuilder line = new StringBuilder();
            line.Append(outcome.Job.Building).Append("  ").Append(outcome.Result.ToString().ToUpperInvariant());
            line.Append("  appended ").Append(outcome.AppendedCount)
                .Append(" of ").Append(outcome.Job.Files.Count);
            line.Append("  NWF ").Append(outcome.NwfOnDisk ? "on disk" : "MISSING");
            line.Append("  NWD ").Append(outcome.NwdOnDisk ? "on disk" : "MISSING");

            if (outcome.FailedFiles.Count > 0)
            {
                line.Append("  failed: ");

                for (int i = 0; i < outcome.FailedFiles.Count; i++)
                {
                    if (i > 0)
                    {
                        line.Append(", ");
                    }

                    line.Append(Path.GetFileName(outcome.FailedFiles[i]));
                }
            }

            if (!string.IsNullOrEmpty(outcome.Error))
            {
                line.Append("  error: ").Append(outcome.Error);
            }

            return line.ToString();
        }
    }
}
