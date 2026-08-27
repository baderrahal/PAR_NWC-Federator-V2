using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Autodesk.Navisworks.Api;
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
        public const string RunLogFileName = "ParsonsNwcFederator-run.log";

        private readonly Action<string> progress;
        private readonly Action<string> log;

        public FederationEngine(Action<string> progress, Action<string> log)
        {
            this.progress = progress ?? delegate { };
            this.log = log ?? delegate { };
        }

        /// <summary>
        /// Works through the jobs in order. Each group's NWF and NWD are written before
        /// the next group starts, so a failure part way through keeps everything already
        /// written.
        /// </summary>
        public IList<JobOutcome> Run(IList<FederationJob> jobs, string runLogPath)
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

                JobOutcome outcome = RunOne(job);
                outcomes.Add(outcome);

                AppendRunLog(runLogPath, outcome);
                log(Describe(outcome));
            }

            return outcomes;
        }

        private JobOutcome RunOne(FederationJob job)
        {
            JobOutcome outcome = new JobOutcome(job);

            try
            {
                Document document = NavisworksApplication.ActiveDocument;

                if (document == null)
                {
                    outcome.Error = "There is no active document.";
                    return outcome;
                }

                document.Clear();

                foreach (string file in job.Files)
                {
                    if (AppendOne(document, file))
                    {
                        outcome.AppendedCount++;
                    }
                    else
                    {
                        outcome.FailedFiles.Add(file);
                        log("    could not append " + Path.GetFileName(file));
                    }
                }

                if (outcome.AppendedCount == 0)
                {
                    outcome.Error = "No file in this group appended, so nothing was written.";
                    return outcome;
                }

                WriteNwf(document, job, outcome);
                WriteNwd(document, job, outcome);
            }
            catch (Exception error)
            {
                outcome.Error = error.Message;

                // The outputs are still checked against the disk below, because a throw
                // after a successful write must not report the file as missing.
                outcome.NwfOnDisk = File.Exists(job.NwfPath);
                outcome.NwdOnDisk = File.Exists(job.NwdPath);
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
                    log("    missing on disk: " + file);
                    return false;
                }

                return document.TryAppendFile(file);
            }
            catch (Exception error)
            {
                log("    append threw on " + Path.GetFileName(file) + ": " + error.Message);
                return false;
            }
        }

        private void WriteNwf(Document document, FederationJob job, JobOutcome outcome)
        {
            progress("Saving NWF for " + job.Building);
            EnsureFolder(job.NwfPath);

            try
            {
                document.TrySaveFile(job.NwfPath);
            }
            catch (Exception error)
            {
                log("    NWF save threw: " + error.Message);
            }

            // Never report a file as written without looking for it.
            outcome.NwfOnDisk = File.Exists(job.NwfPath);

            if (!outcome.NwfOnDisk)
            {
                log("    NWF is not on disk after the save: " + job.NwfPath);
            }
        }

        private void WriteNwd(Document document, FederationJob job, JobOutcome outcome)
        {
            progress("Publishing NWD for " + job.Building);
            EnsureFolder(job.NwdPath);

            try
            {
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
                log("    NWD publish threw: " + error.Message);
            }

            outcome.NwdOnDisk = File.Exists(job.NwdPath);

            if (!outcome.NwdOnDisk)
            {
                log("    NWD is not on disk after the publish: " + job.NwdPath);
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

        /// <summary>
        /// One line per group, appended as the group finishes rather than at the end, so
        /// a run that stops half way still leaves a readable log.
        /// </summary>
        private void AppendRunLog(string runLogPath, JobOutcome outcome)
        {
            if (string.IsNullOrEmpty(runLogPath))
            {
                return;
            }

            try
            {
                EnsureFolder(runLogPath);
                string stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                File.AppendAllText(runLogPath, stamp + "  " + Describe(outcome) + Environment.NewLine);
            }
            catch (Exception error)
            {
                log("    could not write the run log: " + error.Message);
            }
        }
    }
}
