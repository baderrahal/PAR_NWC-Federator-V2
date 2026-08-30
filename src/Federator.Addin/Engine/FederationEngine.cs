using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Autodesk.Navisworks.Api;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Findings;
using Federator.Core.Rerun;
using Federator.Core.Sets;
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
        private readonly ExchangeDocument exchange;
        private readonly List<SourcePair> sourcePairs = new List<SourcePair>();

        public FederationEngine(Action<string> progress, RunLog log)
            : this(progress, log, true, null)
        {
        }

        public FederationEngine(Action<string> progress, RunLog log, bool republishNwd)
            : this(progress, log, republishNwd, null)
        {
        }

        /// <summary>
        /// The exchange document is whatever was picked in the Clash step, read once. It
        /// can hold sets, tests, or both, and any of the three is a normal case. Null when
        /// nothing was picked, and then no set is built and no test is created.
        /// </summary>
        public FederationEngine(
            Action<string> progress, RunLog log, bool republishNwd, ExchangeDocument exchange)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.progress = progress ?? delegate { };
            this.log = log;
            this.republishNwd = republishNwd;
            this.exchange = exchange;
        }

        /// <summary>
        /// Every NWC and Revit source pair this run saw, in the order it saw them. Read
        /// after Run to report where the building code inside the Revit name is not the
        /// code on the NWC. Information only, nothing acts on it.
        /// </summary>
        public IList<SourcePair> SourcePairs
        {
            get { return sourcePairs; }
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
                    outcome.AddError("There is no active document.");
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

                // The sets, the tests and the results all live in the NWF, so the clash
                // work happens BEFORE the NWF is saved for the last time and long before
                // the NWD is published. The NWD used to go first, which shipped it with no
                // sets and no results in it.
                if (ClashStep(document, job, outcome))
                {
                    SaveTheNwfAgain(document, job, outcome);
                }

                WriteNwd(document, job, outcome);
            }
            catch (Exception error)
            {
                outcome.AddError(error.Message);
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

            return NwfComparison.Compare(FilesInsideTheOpenDocument(job.Building), job.Files);
        }

        /// <summary>
        /// The files the open document points at.
        ///
        /// Model.FileName is the NWC, and it is what the scan holds, so it is the one the
        /// comparison uses. Model.SourceFileName is the container the NWC was published
        /// from, which on these projects is a Revit file in Autodesk Docs, for example
        /// Autodesk Docs://KSA_New Murabba/1104-PAR-100000-ZZZ-AR-MOD-003000.rvt. That can
        /// never equal a scanned NWC path, and comparing it reported CHANGED for 22 of 22
        /// groups on a run where nothing had changed, so no set was built and no test ran.
        ///
        /// SourceFileName is still read, for two reasons. It is used when FileName is
        /// empty, so a model reporting one and not the other is still counted, and both
        /// are logged whenever they disagree, which is what made this findable in the
        /// first place.
        /// </summary>
        private IList<string> FilesInsideTheOpenDocument(string building)
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
                string use = ModelFileNames.PathOf(cached, source);

                log.Line("         holds   " + (string.IsNullOrEmpty(use) ? "an unnamed model" : use)
                    + (ModelFileNames.Disagree(cached, source)
                        ? "   [source " + Or(source) + ", file " + Or(cached) + "]"
                        : string.Empty));

                RecordSource(building, cached, source);

                if (!string.IsNullOrEmpty(use))
                {
                    files.Add(use);
                }
            }

            return files;
        }

        /// <summary>
        /// Keeps the NWC and the Revit container it came from, so the run can report where
        /// the building code inside the Revit name is not the code on the NWC. Reported
        /// only, never acted on.
        /// </summary>
        private void RecordSource(string building, string nwcPath, string sourceName)
        {
            if (string.IsNullOrEmpty(nwcPath) || string.IsNullOrEmpty(sourceName))
            {
                return;
            }

            sourcePairs.Add(new SourcePair(building, nwcPath, sourceName));
        }

        /// <summary>
        /// Reads the source name off every model in the document that was just built, so
        /// a first run reports the same mismatches a rerun would. Never fails the group.
        /// </summary>
        private void RecordSourcesAfterAppending(Document document, string building)
        {
            try
            {
                if (document.Models == null)
                {
                    return;
                }

                foreach (Model model in document.Models)
                {
                    string cached = model.FileName;
                    string source = model.SourceFileName;

                    if (ModelFileNames.Disagree(cached, source))
                    {
                        log.Line("         holds   " + Or(cached)
                            + "   [source " + Or(source) + ", file " + Or(cached) + "]");
                    }

                    RecordSource(building, cached, source);
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading the Revit source names for " + building,
                    error,
                    "kept going, this only costs the SOURCE MISMATCH report for this group");
            }
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
                outcome.AddError("No file in this group appended, so nothing was written.");
                log.Line("GROUP    " + job.Building + " wrote nothing, every append failed");
                return false;
            }

            RecordSourcesAfterAppending(document, job.Building);

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

        /// <summary>
        /// Builds the sets the picked file holds, creates the tests it holds and runs
        /// them, all into the document that is open for this group. Returns true when
        /// anything was put into the document, which is what decides whether the NWF is
        /// saved again.
        ///
        /// Any of the three shapes is a normal case: a file holding sets only, a file
        /// holding tests only where the sets already live in the model, or one file
        /// holding both.
        /// </summary>
        private bool ClashStep(Document document, FederationJob job, JobOutcome outcome)
        {
            if (!ClashWork.Any(exchange))
            {
                log.Line("CLASH    " + ClashWork.Describe(exchange));
                return false;
            }

            if (outcome.Decision == RerunDecision.Changed)
            {
                // A CHANGED group is left alone entirely and the decision goes to Bader,
                // so nothing is built into it either.
                log.Line("CLASH    " + job.Building
                    + " was left alone because its file list changed, so no set was built and no test created");
                return false;
            }

            bool changed = BuildTheSets(document, job, outcome);

            // Deliberately not short circuited. A file holding tests only is a normal
            // case, so the tests are created whether or not any set was built here.
            return CreateAndRunTheTests(document, job, outcome) || changed;
        }

        private bool BuildTheSets(Document document, FederationJob job, JobOutcome outcome)
        {
            try
            {
                SetBuildPlan plan = SetBuildPlan.From(exchange);

                foreach (string unknown in plan.UnknownTestValues)
                {
                    log.Line("SETS     condition test \"" + unknown
                        + "\" is not one this tool rebuilds, every set using it is skipped");
                }

                if (!plan.HasWork)
                {
                    log.Line("SETS     the picked file holds no set this tool rebuilds, "
                        + "so the tests will resolve against whatever sets the model already holds");
                    return false;
                }

                log.Line("SETS     " + job.Building + ", " + plan.Buildable.Count + " to build, "
                    + plan.Skipped.Count + " skipped");

                SetBuildOutcome sets = new SetBuilder(progress, log).Build(plan);
                outcome.Sets = sets;
                log.Block("SETS " + job.Building, sets.Lines());

                return sets.CreatedCount > sets.AlreadyPresentCount;
            }
            catch (Exception error)
            {
                outcome.AddError("building the sets threw " + error.GetType().Name + ": " + error.Message);
                log.Failure(
                    "building the sets for " + job.Building,
                    error,
                    "kept going, the tests will resolve against whatever sets did get built");
                return false;
            }
        }

        private bool CreateAndRunTheTests(Document document, FederationJob job, JobOutcome outcome)
        {
            try
            {
                if (!ClashWork.CreatesTests(exchange))
                {
                    log.Line("CLASH    the picked file holds no clash test, so none was created");
                    return false;
                }

                // The units come from the document that is open right now, because every
                // tolerance in the file is converted into them. There is no global
                // tolerance setting in this tool, each test carries its own.
                string units = ClashRunner.DocumentUnits();
                ClashTestPlan plan = ClashTestPlan.From(exchange, units);

                foreach (string unknown in plan.UnknownTestTypes)
                {
                    log.Line("CLASH    test type \"" + unknown
                        + "\" is not one this tool creates, every test using it is skipped by name");
                }

                log.Line("CLASH    " + job.Building + ", " + plan.TestsInFile + " in the file, "
                    + plan.Buildable.Count + " to create, " + plan.Skipped.Count + " skipped before the model");

                ClashRunOutcome clash = new ClashRunner(progress, log).Run(plan);
                outcome.Clash = clash;
                log.Block("CLASH " + job.Building, clash.Lines());
                log.Line("CLASH    " + job.Building + " finished. " + clash.Summary());

                return clash.CreatedCount > 0 || clash.RanCount > 0;
            }
            catch (Exception error)
            {
                outcome.AddError(
                    "creating or running the clash tests threw " + error.GetType().Name + ": " + error.Message);
                log.Failure(
                    "the clash tests for " + job.Building,
                    error,
                    "kept going, whatever was already created and run is kept in the NWF");
                return false;
            }
        }

        /// <summary>
        /// Saves the NWF after the clash work, which is what puts the sets, the tests and
        /// the results into it. This is a real write by this run, in all three rerun
        /// cases, so it goes through WriteFinished and its size is read back off the disk.
        /// </summary>
        private void SaveTheNwfAgain(Document document, FederationJob job, JobOutcome outcome)
        {
            progress("Saving the NWF for " + job.Building + " with its sets and results");
            log.WriteAttempted("NWF", job.NwfPath);

            try
            {
                EnsureFolder(job.NwfPath);

                if (!document.TrySaveFile(job.NwfPath))
                {
                    log.Line("NWF      the save after the clash work returned false for " + job.Building);
                }
            }
            catch (Exception error)
            {
                outcome.AddError(
                    "saving the NWF after the clash work threw " + error.GetType().Name + ": " + error.Message);
                log.Failure(
                    "saving the NWF after the clash work for " + job.Building,
                    error,
                    "kept going, the disk is checked next to see whether anything landed");
            }

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

            if (outcome.Clash != null)
            {
                line.Append("  clash ").Append(outcome.Clash.RanCount).Append(" run, ")
                    .Append(outcome.Clash.SkippedCount).Append(" skipped, ")
                    .Append(outcome.Clash.TotalClashes).Append(" clashes");
            }

            foreach (string error in outcome.Errors)
            {
                line.Append("  error: ").Append(error);
            }

            return line.ToString();
        }
    }
}
