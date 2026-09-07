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
using Federator.Core.Report;
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
        private readonly ReportOptions reports;
        /// <summary>
        /// Where the reports go. Decided in the constructor for a scanned run, from the
        /// NWF folder, and in RunOpenDocument for the open file, from the file itself.
        /// </summary>
        private string reportFolder;
        private readonly List<SourcePair> sourcePairs = new List<SourcePair>();

        /// <summary>
        /// One guard for the whole run, not one per group. Every group of that nine hour
        /// run failed the same way, so a guard that reset between groups would have let
        /// all 24 of them through.
        /// </summary>
        private readonly RepeatedFailureGuard guard = new RepeatedFailureGuard();

        /// <summary>Why the run was abandoned, or null while it is still going.</summary>
        private string stopTheRun;

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
            : this(progress, log, republishNwd, exchange, null, null)
        {
        }

        /// <summary>
        /// The report folder is worked out once, from the picked folder or from beside the
        /// NWF folder, so every group in the run writes into the same place.
        /// </summary>
        public FederationEngine(
            Action<string> progress,
            RunLog log,
            bool republishNwd,
            ExchangeDocument exchange,
            ReportOptions reports,
            string nwfFolder)
            : this(progress, log, republishNwd, exchange, reports)
        {
            this.reportFolder = string.IsNullOrEmpty(nwfFolder)
                    && string.IsNullOrEmpty(this.reports.ExcelFolder)
                ? null
                : this.reports.ChooseFor(nwfFolder).Folder;
        }

        /// <summary>
        /// For the open file. No folder is handed in, because the report folder is read
        /// off the open file in RunOpenDocument through OpenDocumentJob.ReportFolder, the
        /// same rule the window uses for its line. Handing a folder in here is what once
        /// wrote to Clash Reports\Clash Reports: the window passed the report folder as
        /// the NWF folder and the constructor built Clash Reports beside it again.
        /// </summary>
        public FederationEngine(
            Action<string> progress,
            RunLog log,
            bool republishNwd,
            ExchangeDocument exchange,
            ReportOptions reports)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.progress = progress ?? delegate { };
            this.log = log;
            this.republishNwd = republishNwd;
            this.exchange = exchange;
            this.reports = reports ?? new ReportOptions();
            this.reportFolder = null;
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

                // Which of the two workflows this group took, read off the Decide result.
                // A group that threw before Decide carries the default, Build, and so
                // reads First run, the same default GroupJudgement judges it by.
                log.GroupFinished(
                    job.Building,
                    outcome.Result,
                    groupClock.Elapsed.TotalSeconds,
                    outcome.Reason,
                    RunPath.Label(outcome.Decision, exchange != null));

                // A run failing uniformly stops here rather than working through the rest.
                // One real run spent 8 hours 52 minutes over 24 groups with every test
                // failing the same way, and stopping the group would have saved none of it.
                if (stopTheRun != null)
                {
                    int notAttempted = jobs.Count - (i + 1);

                    log.Line("RUN      STOPPED after " + (i + 1)
                        + (i == 0 ? " group. " : " groups. ") + stopTheRun);

                    if (notAttempted > 0)
                    {
                        log.Line("RUN      " + notAttempted
                            + (notAttempted == 1 ? " group was" : " groups were")
                            + " not attempted. Everything already written is kept.");
                    }

                    progress("The run was stopped. " + stopTheRun);
                    break;
                }
            }

            return outcomes;
        }

        /// <summary>
        /// The whole job against the document somebody already has open, with no scan, no
        /// source folder and no grouping.
        ///
        /// WHY. Running a federation that already exists meant picking the folder its NWC
        /// files came from, waiting for a scan, choosing a grouping and ticking one group
        /// out of twenty two, all to arrive at a file that was open on the screen. In
        /// Navisworks a person opens a file, opens Clash Detective, presses Run and reads
        /// the results. This is the same shape.
        ///
        /// It is the SAME flow as a scanned group with the first step removed. There is no
        /// Decide, because there is nothing to compare a file list against: the document is
        /// the file list. Nothing is appended and nothing is cleared, so the clash results
        /// inside it survive, which is the rule that matters most here. Everything after
        /// that is what a scanned group does, in the same order, through the same methods.
        ///
        /// The clash file is optional. Without one, the tests saved in the document are
        /// run where they sit, which is the ordinary weekly case, and a document holding
        /// none means nothing runs and the log says so.
        /// </summary>
        public JobOutcome RunOpenDocument()
        {
            Document document = NavisworksApplication.ActiveDocument;
            string open = document == null ? string.Empty : Or(document.FileName);

            FederationJob job = OpenJob(open);
            JobOutcome outcome = new JobOutcome(job);
            outcome.NwfSize = -1;
            outcome.NwdSize = -1;
            outcome.NwdRequested = republishNwd;

            // The open file is one group, and it starts and finishes the way a scanned
            // group does, so GroupJudgement gives it DONE, PARTIAL or FAILED by the same
            // rule and the RESULT block counts it. It used to end with no GROUP line at
            // all, so the RESULT block that followed said no group had run.
            Stopwatch groupClock = Stopwatch.StartNew();
            log.GroupStarted(job.Building, FilesInsideTheOpenDocument(job.Building));

            try
            {
                if (document == null)
                {
                    outcome.AddError("There is no document open, so there is nothing to run.");
                    log.Line("OPEN     nothing is open");
                    return outcome;
                }

                if (!OpenDocumentJob.CanRun(open))
                {
                    outcome.AddError(OpenDocumentJob.WhyNot(open));
                    log.Line("OPEN     " + OpenDocumentJob.WhyNot(open));
                    return outcome;
                }

                // The one rule for where the reports go, shared with the window's line.
                // The open file's own folder stands where the NWF folder stands on a
                // scanned run, and no scan folder is applied because there was no scan.
                ReportFolderChoice where = OpenDocumentJob.ReportFolder(open, reports.ExcelFolder);
                reportFolder = where.Folder;

                log.Line("OPEN     running the document that is already open, no scan");
                log.Line("OPEN     file     " + open);
                log.Line("OPEN     NWD      " + job.NwdPath);
                log.Line("OPEN     report   " + reportFolder
                    + (string.IsNullOrEmpty(reports.ExcelFolder)
                        ? "  (beside the file, no Excel folder picked)"
                        : "  (the Excel folder picked on the Outputs step)"));

                try
                {
                    // Nothing is appended and nothing is cleared. The models in it are
                    // what somebody put there, and the clash history lives in the same
                    // file.
                    outcome.Decision = RerunDecision.Open;
                    outcome.AppendedCount = document.Models.Count;
                    outcome.NwfSize = SizeOnDiskOrMinusOne(job.NwfPath);
                    outcome.NwfOnDisk = outcome.NwfSize >= 0;

                    if (reports.SetDocumentUnits)
                    {
                        new DocumentUnits(log).Apply(document, WantedUnits());
                    }

                    if (ClashStep(document, job, outcome))
                    {
                        SaveTheNwfAgain(document, job, outcome);
                    }

                    WriteWorkbook(job, outcome);
                    WriteNwd(document, job, outcome);
                    ConfirmTheNwfSurvived(job, outcome);
                }
                catch (Exception error)
                {
                    outcome.AddError(error.Message);
                    log.Failure(
                        "running the open document " + job.Building,
                        error,
                        "stopped, everything already written is kept");

                    outcome.NwfSize = log.CheckOnDisk("NWF", job.NwfPath);
                    outcome.NwdSize = log.CheckOnDisk("NWD", job.NwdPath);
                    outcome.NwfOnDisk = outcome.NwfSize >= 0;
                    outcome.NwdOnDisk = outcome.NwdSize >= 0;
                }

                return outcome;
            }
            finally
            {
                groupClock.Stop();

                // The open file is always a Weekly run. There is no First run on this
                // path, because the NWF already exists and is the document.
                log.GroupFinished(
                    job.Building,
                    outcome.Result,
                    groupClock.Elapsed.TotalSeconds,
                    outcome.Reason,
                    RunPath.Label(RerunDecision.Open, exchange != null));

                // The same fields the scanned run's RUN SETTINGS and GROUPS blocks carry,
                // where they apply, written just before the window writes RESULT.
                log.Block(OpenDocumentJob.SummaryTitle, OpenDocumentJob.SummaryLines(
                    open,
                    exchange == null ? null : exchange.SourcePath,
                    job.NwdPath,
                    reportFolder,
                    outcome.Clash == null ? null : outcome.Clash.Summary(),
                    outcome.Result,
                    outcome.Reason));
            }
        }

        /// <summary>
        /// The job for the open document. Its name is read off the file rather than built
        /// from a pattern, because the name is already decided and is on the file.
        /// </summary>
        private static FederationJob OpenJob(string open)
        {
            string name = OpenDocumentJob.NameFrom(open);

            return new FederationJob(
                name,
                name,
                open,
                OpenDocumentJob.NwdBeside(open),
                new List<string>(),
                name);
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
                // BEFORE the clash step, so every tolerance and every distance is read in
                // the units the report is going out in. Converting afterwards would mean a
                // report whose numbers and whose unit label disagree.
                if (reports.SetDocumentUnits)
                {
                    new DocumentUnits(log).Apply(document, WantedUnits());
                }

                if (ClashStep(document, job, outcome))
                {
                    SaveTheNwfAgain(document, job, outcome);
                }

                // After the clash step and before the NWD, so the three outputs of a group
                // agree with each other rather than the workbook describing a state the
                // NWD does not carry.
                WriteWorkbook(job, outcome);

                WriteNwd(document, job, outcome);

                // The NWD is published last, and the NWF is the only record of what has
                // been fixed, so the NWF is looked at once more AFTER it. Nothing was
                // checking this, and a RESULT block reporting the size of the first save
                // made it read as though publishing the NWD had emptied the file.
                ConfirmTheNwfSurvived(job, outcome);
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
            if (outcome.Decision == RerunDecision.Changed)
            {
                // A CHANGED group is left alone entirely and the decision goes to Bader,
                // so nothing is built into it and nothing in it is run.
                log.Line("CLASH    " + job.Building
                    + " was left alone because its file list changed, so no set was built and no test created or run");
                return false;
            }

            // Three things can happen and the log names which, in the same words on the
            // scanned run and on the open file run: tests from the picked XML, the tests
            // already saved in the document when no XML was picked, or nothing. The
            // saved tests are only counted when there is no XML, because with one the
            // XML decides everything exactly as before.
            int savedTests = exchange == null ? SavedTests.Count(document) : 0;
            ClashSource source = ClashWork.SourceFor(exchange, savedTests);
            log.Line("CLASH    source   " + ClashWork.DescribeSource(source, exchange, savedTests));

            if (source == ClashSource.Nothing)
            {
                return false;
            }

            // Sets come from the XML and from nowhere else. With no XML the sets in the
            // document are left exactly as they are.
            bool changed = source == ClashSource.TestsFromXml && BuildTheSets(document, job, outcome);

            // Deliberately not short circuited. A file holding tests only is a normal
            // case, so the tests are created whether or not any set was built here.
            return CreateAndRunTheTests(document, job, outcome, source) || changed;
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

                // What decides the second NWF save is whether this build put anything
                // into the document. A set already there was left alone and put nothing
                // in, so on a rerun that finds sixty present and creates one, the one
                // still counts. Comparing created against already there said nothing
                // was built in exactly that case.
                log.Line("SETS     " + job.Building + " put into the document: "
                    + sets.CreatedCount + " created, "
                    + sets.AlreadyPresentCount + " already there and left alone");

                return sets.PutAnythingIn;
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

        private bool CreateAndRunTheTests(
            Document document, FederationJob job, JobOutcome outcome, ClashSource source)
        {
            try
            {
                // The units come from the document that is open right now, because every
                // tolerance in the file is converted into them. There is no global
                // tolerance setting in this tool, each test carries its own.
                string units = ClashRunner.DocumentUnits();
                ClashTestPlan plan;

                if (source == ClashSource.TestsFromXml)
                {
                    if (!ClashWork.CreatesTests(exchange))
                    {
                        log.Line("CLASH    the picked file holds no clash test, so none was created");
                        return false;
                    }

                    plan = ClashTestPlan.From(exchange, units);

                    foreach (string unknown in plan.UnknownTestTypes)
                    {
                        log.Line("CLASH    test type \"" + unknown
                            + "\" is not one this tool creates, every test using it is skipped by name");
                    }

                    log.Line("CLASH    " + job.Building + ", " + plan.TestsInFile + " in the file, "
                        + plan.Buildable.Count + " to create, " + plan.Skipped.Count + " skipped before the model");
                }
                else
                {
                    // No XML. The tests already saved in the document are the plan, by
                    // address, and every one is run where it sits. Nothing is created and
                    // nothing is compared, because there is no file to compare against.
                    plan = ClashTestPlan.FromDocument(SavedTests.Read(document), units);

                    foreach (string unknown in plan.UnknownTestTypes)
                    {
                        log.Line("CLASH    saved test " + unknown
                            + " is not one this tool runs, every test with it is skipped by name");
                    }

                    log.Line("CLASH    " + job.Building + ", " + plan.TestsInFile + " saved in the document, "
                        + plan.Buildable.Count + " to run, " + plan.Skipped.Count + " skipped before the model");
                }

                ClashRunner runner = new ClashRunner(progress, log, guard);
                runner.NameSettings = reports.Names;
                runner.SingleModelGroup = job.Files.Count == 1;
                runner.ApplyFileSettings = reports.ApplyFileSettings;
                runner.CompactResolved = reports.CompactResolved;

                if (runner.SingleModelGroup)
                {
                    log.Line("CLASH    " + job.Building + " holds one model, so every test is created "
                        + "and none is run. One model cannot clash with anything.");
                }

                if (reports.WriteWorkbook || reports.WriteXml)
                {
                    ClashReport report = new ClashReport(job.Building, job.OutputName);
                    report.SourceFile = exchange == null
                        ? "the tests saved in the open document"
                        : exchange.SourcePath;
                    report.DocumentUnits = units;
                    report.RunAt = DateTime.Now;
                    report.BuildStamp = BuildStamp.Of(typeof(FederationEngine).Assembly);
                    report.OpenCount = reports.OpenCount;
                    runner.Report = report;
                    outcome.Report = report;

                    // The pictures go in a folder named after the workbook and beside it,
                    // so where the workbook is going has to be known before the clash step
                    // rather than after it. Only when a workbook is actually being written,
                    // because pictures beside a file nobody writes are pictures nobody
                    // finds.
                    if (reports.WriteWorkbook && reportFolder != null && reports.Images.Write)
                    {
                        runner.WorkbookPath = ReportPaths.Workbook(reportFolder, job.WorkbookName);
                        runner.Images = new ClashImages(log, reports.Images);
                        log.Line("CLASH    " + reports.Images.Describe());
                    }
                    else if (!reports.Images.Write)
                    {
                        log.Line("CLASH    images are switched off for this run.");
                    }
                }

                ClashRunOutcome clash = runner.Run(plan);

                if (outcome.Report != null)
                {
                    outcome.Report.OpenDocument = clash.OpenDocument;
                    outcome.Report.ClashStepSeconds = clash.Seconds;
                    outcome.Report.CompactedAway = clash.Compacted;
                }
                outcome.Clash = clash;
                log.Block("CLASH " + job.Building, clash.Lines());

                if (outcome.Report != null && runner.Images != null)
                {
                    // Measured, never estimated. Every number anyone has given for what a
                    // clash image costs has been a guess until this line.
                    foreach (string line in outcome.Report.Images.Lines())
                    {
                        log.Line(line);
                    }

                    if (runner.Images.ShouldStopTheRun && stopTheRun == null)
                    {
                        stopTheRun = runner.Images.StopReason;
                        outcome.AddError(runner.Images.StopReason);
                    }
                }
                log.Line("CLASH    " + job.Building + " finished. " + clash.Summary());

                if (clash.StopTheRun)
                {
                    // Not this group's failure alone. The rest of the run is abandoned
                    // after this group finishes writing what it already has.
                    stopTheRun = clash.StopTheRunReason;
                    outcome.AddError(clash.StopTheRunReason);
                }

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
        /// Reads the workbook back off the disk and counts how many rows filled each of
        /// our own columns. A column filled zero times is named.
        ///
        /// This is the check that would have caught Source File and Discipline coming out
        /// empty on every row, without anyone opening the file to find out.
        /// </summary>
        private void CheckTheWorkbook(FederationJob job, JobOutcome outcome, string path)
        {
            WorkbookCheck check = WorkbookCheck.Of(path);

            log.Block("WORKBOOK CHECK " + job.Building, check.Lines());
            progress(job.Building + ". " + check.Summary());

            if (!check.Passed)
            {
                outcome.AddReportWarning(check.Summary());
            }
        }

        /// <summary>
        /// Reads the NWF one more time, after the NWD has been published, and says whether
        /// it is still the size it was when it was saved.
        ///
        /// This is the last thing a group does. It writes nothing and changes nothing, so
        /// it cannot itself be what breaks a group, and it only speaks about a file this
        /// run actually wrote.
        /// </summary>
        private void ConfirmTheNwfSurvived(FederationJob job, JobOutcome outcome)
        {
            if (!outcome.NwfOnDisk)
            {
                return;
            }

            long size = log.ConfirmStillWhole("NWF", job.NwfPath, "publishing the NWD");

            if (size < 0)
            {
                outcome.AddError(
                    "the NWF is not on disk after the NWD was published, so this group's "
                    + "clash results are gone");
                outcome.NwfOnDisk = false;
                outcome.NwfSize = -1;
                return;
            }

            if (size < outcome.NwfSize)
            {
                outcome.AddError(
                    "the NWF shrank from " + outcome.NwfSize.ToString("#,##0") + " to "
                    + size.ToString("#,##0") + " bytes while the NWD was published. The "
                    + "clash results live in that file.");
            }

            outcome.NwfSize = size;
        }

        /// <summary>
        /// Writes the report as HTML (Tabular), which is what the client actually
        /// receives, by handing our XML to Autodesk's own stylesheet.
        ///
        /// The stylesheet is read from the install at run time and no copy of it is in
        /// this repo. Where it is not there, this says every path it looked at and writes
        /// nothing. The workbook and the XML are untouched either way.
        /// </summary>
        private void WriteHtmlTabular(FederationJob job, JobOutcome outcome, ClashReport report)
        {
            if (!reports.WriteHtml)
            {
                return;
            }

            string stylesheet = StylesheetLocator.Find(
                NavisworksFacts.InstallFolder(), NavisworksFacts.Language());

            if (stylesheet.Length == 0)
            {
                foreach (string line in StylesheetLocator.WhyNotFound(
                    NavisworksFacts.InstallFolder(), NavisworksFacts.Language()))
                {
                    log.Line(line);
                }

                return;
            }

            string path = HtmlTabularWriter.PathFor(
                ReportPaths.Workbook(reportFolder, job.WorkbookName));

            progress("Writing the client report for " + job.Building);
            log.WriteAttempted("HTML", path);

            try
            {
                // No ClientColumnsOnly here any more. The page IS the client's report and
                // always carries only what theirs carries. Our extra columns live in the
                // workbook, where that tick box still governs them.
                ClashReportXml writer = new ClashReportXml();
                writer.LogoHref = CopyLogoIntoTheReportFolder(
                    ReportPaths.Workbook(reportFolder, job.WorkbookName));

                new HtmlTabularWriter().Write(writer.Build(report), stylesheet, path);
            }
            catch (Exception error)
            {
                outcome.AddError(
                    "writing the client report threw " + error.GetType().Name + ": " + error.Message);
                log.Failure(
                    "writing the client report for " + job.Building,
                    error,
                    "kept going, the workbook and the XML are unaffected");
            }

            outcome.HtmlSize = log.WriteFinished("HTML", path);
            outcome.HtmlOnDisk = outcome.HtmlSize >= 0;

            CheckThePage(job, outcome, path);
        }

        /// <summary>
        /// The units the report is going out in, read off the setting. A name nobody can
        /// read falls back to metres rather than stopping a run.
        /// </summary>
        private Autodesk.Navisworks.Api.Units WantedUnits()
        {
            try
            {
                return (Autodesk.Navisworks.Api.Units)Enum.Parse(
                    typeof(Autodesk.Navisworks.Api.Units), reports.UnitsName, true);
            }
            catch (Exception)
            {
                return Autodesk.Navisworks.Api.Units.Meters;
            }
        }

        /// <summary>
        /// Reads the page back off the disk and says what is in it, because Bader has been
        /// opening every report in Excel and searching it by hand and this tool wrote the
        /// file. The FILE is read, never the object that produced it, which is how the
        /// broken image link and the silently skipping test were both caught.
        ///
        /// Checking a report never fails a group. A check that cannot run says so.
        /// </summary>
        private void CheckThePage(FederationJob job, JobOutcome outcome, string path)
        {
            PageCheck check = PageCheck.Of(path);

            log.Block("REPORT CHECK " + job.Building, check.Lines());
            progress(job.Building + ". " + check.Summary());

            if (!check.Passed)
            {
                outcome.AddReportWarning(check.Summary());
            }
        }

        /// <summary>
        /// Copies the logo into the report's own _files folder, beside the clash pictures,
        /// and gives back the relative name to link it by.
        ///
        /// The report goes to a client, so the page cannot point at a path on the machine
        /// that wrote it. The accepted xlsx carries absolute file:/// links and that is
        /// exactly why its pictures break anywhere else. Copied and linked relatively, the
        /// whole folder works wherever it is sent, which is what Navisworks itself does
        /// and why both supplied reports have a logo.jpg sitting in their _files folder.
        ///
        /// Where nobody has changed it, the file copied is the install's own logo.jpg. No
        /// copy of it is in this repo or in the bundle. It is read off the machine that is
        /// running, every run.
        /// </summary>
        private string CopyLogoIntoTheReportFolder(string workbookPath)
        {
            string picked = reports.LogoPath == null ? string.Empty : reports.LogoPath.Trim();

            // Cleared on purpose means no logo, and that is a choice rather than a fault.
            if (picked.Length == 0)
            {
                log.Line("LOGO     the logo box is empty, so the page carries no logo");
                return string.Empty;
            }

            try
            {
                if (!System.IO.File.Exists(picked))
                {
                    foreach (string line in LogoLocator.WhyNotFound(
                        NavisworksFacts.InstallFolder(), NavisworksFacts.Language()))
                    {
                        log.Line(line);
                    }

                    log.Line("         and the picked " + picked + " is not there either");
                    return string.Empty;
                }

                string into = ImageNaming.LogoPathFor(workbookPath);
                string folder = System.IO.Path.GetDirectoryName(into);

                if (!string.IsNullOrEmpty(folder) && !System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }

                if (!string.Equals(picked, into, StringComparison.OrdinalIgnoreCase))
                {
                    System.IO.File.Copy(picked, into, true);
                }

                log.Line("LOGO     " + picked + " copied into " + folder);
                return ImageNaming.LogoLinkFor(workbookPath);
            }
            catch (Exception error)
            {
                log.Failure(
                    "copying the logo into the report folder",
                    error,
                    "kept going, the page was written with no logo");
                return string.Empty;
            }
        }

        /// <summary>
        /// Writes the workbook, and the XML beside it when that is switched on. Both are
        /// built from the same results in memory. Neither reads the other, so a fault in
        /// one cannot corrupt the other.
        ///
        /// A file is only recorded as written after it exists and its size has been read
        /// back off the disk, which is what WriteFinished does. Nothing here reports a
        /// size it did not read.
        /// </summary>
        private void WriteWorkbook(FederationJob job, JobOutcome outcome)
        {
            ClashReport report = outcome.Report;

            if (report == null || reportFolder == null)
            {
                return;
            }

            if (reports.WriteWorkbook)
            {
                string path = ReportPaths.Workbook(reportFolder, job.WorkbookName);
                progress("Writing the workbook for " + job.Building);
                log.WriteAttempted("XLSX", path);

                try
                {
                    new WorkbookWriter(reports).Write(report, path);
                }
                catch (Exception error)
                {
                    outcome.AddError(
                        "writing the workbook threw " + error.GetType().Name + ": " + error.Message);
                    log.Failure(
                        "writing the workbook for " + job.Building,
                        error,
                        "kept going, the disk is checked next to see whether anything landed");
                }

                outcome.WorkbookSize = log.WriteFinished("XLSX", path);
                outcome.WorkbookOnDisk = outcome.WorkbookSize >= 0;

                CheckTheWorkbook(job, outcome, path);
            }

            WriteHtmlTabular(job, outcome, report);

            if (!reports.WriteXml)
            {
                return;
            }

            string xmlPath = ReportPaths.Xml(reportFolder, job.WorkbookName);
            log.WriteAttempted("XML", xmlPath);

            try
            {
                new ClashReportXml().Write(report, xmlPath);
            }
            catch (Exception error)
            {
                outcome.AddError(
                    "writing the clash XML threw " + error.GetType().Name + ": " + error.Message);
                log.Failure(
                    "writing the clash XML for " + job.Building,
                    error,
                    "kept going, the workbook is unaffected because neither reads the other");
            }

            outcome.XmlSize = log.WriteFinished("XML", xmlPath);
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
                log.Line("NWD      not republished, this run was started without republishing");
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
