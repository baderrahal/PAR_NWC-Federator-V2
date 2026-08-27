using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Autodesk.Navisworks.Api;
using Federator.Core.Diagnostics;
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

        public FederationEngine(Action<string> progress, RunLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.progress = progress ?? delegate { };
            this.log = log;
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
                log.GroupFinished(job.Building, outcome.Result, groupClock.Elapsed.TotalSeconds);
            }

            return outcomes;
        }

        private JobOutcome RunOne(FederationJob job)
        {
            JobOutcome outcome = new JobOutcome(job);
            outcome.NwfSize = -1;
            outcome.NwdSize = -1;

            try
            {
                Document document = NavisworksApplication.ActiveDocument;

                if (document == null)
                {
                    outcome.Error = "There is no active document.";
                    log.Line("GROUP    " + job.Building + " stopped, there is no active document");
                    return outcome;
                }

                log.Line("CLEAR    the document, before group " + job.Building);
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
                    return outcome;
                }

                WriteNwf(document, job, outcome);
                WriteNwd(document, job, outcome);
            }
            catch (Exception error)
            {
                outcome.Error = error.Message;
                log.Failure(
                    "federating group " + job.Building,
                    error,
                    "stopped this group, carried on with the next one, everything already written is kept");

                // The outputs are still checked against the disk below, because a throw
                // after a successful write must not report the file as missing.
                outcome.NwfSize = log.WriteFinished("NWF", job.NwfPath);
                outcome.NwdSize = log.WriteFinished("NWD", job.NwdPath);
                outcome.NwfOnDisk = outcome.NwfSize >= 0;
                outcome.NwdOnDisk = outcome.NwdSize >= 0;
            }

            return outcome;
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
                document.TrySaveFile(job.NwfPath);
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
            progress("Publishing NWD for " + job.Building);
            log.WriteAttempted("NWD", job.NwdPath);

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

                    document.TryPublishFile(job.NwdPath, properties);
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "publishing the NWD for " + job.Building,
                    error,
                    "kept going, the disk is checked next to see whether anything landed");
            }

            outcome.NwdSize = log.WriteFinished("NWD", job.NwdPath);
            outcome.NwdOnDisk = outcome.NwdSize >= 0;
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
