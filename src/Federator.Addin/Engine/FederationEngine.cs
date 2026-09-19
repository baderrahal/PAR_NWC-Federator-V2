using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.DocumentParts;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Findings;
using Federator.Core.Report;
using Federator.Core.Rerun;
using Federator.Core.Sets;
using Federator.Core.Views;
using Federator.Core.Units;
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

        /// <summary>
        /// What the window shows while the run works. F62. The callback is the one that
        /// was always there and this widens WHAT it carries rather than adding a second
        /// way out, so everything stays on the thread the run is on.
        /// </summary>
        private readonly LiveLine live;
        private readonly RunLog log;
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
        /// all 24 of them through. Its count comes from the run's own options, so the
        /// number can be changed without a recompile.
        /// </summary>
        private readonly RepeatedFailureGuard guard;

        /// <summary>Why the run was abandoned, or null while it is still going.</summary>
        private string stopTheRun;

        /// <summary>
        /// The group being worked on, so the live line can find what the SAME step took
        /// on the group before this one. Null outside a group.
        /// </summary>
        private string currentBuilding;

        /// <summary>
        /// How many distinct things this run measured and no output carries. F63. Counted
        /// by name across the run, because the same ones are held back on every group and
        /// adding them up would say nothing except how many groups there were.
        /// </summary>
        private readonly GapTally gaps = new GapTally();

        /// <summary>
        /// For a scanned run. The exchange document is whatever was picked in the Clash
        /// step, read once. It can hold sets, tests, or both, and any of the three is a
        /// normal case. Null when nothing was picked, and then no set is built and no test
        /// is created. The report folder is worked out once, from the picked folder or
        /// from beside the NWF folder, so every group in the run writes into the same
        /// place.
        /// </summary>
        public FederationEngine(
            Action<string> progress,
            RunLog log,
            ExchangeDocument exchange,
            ReportOptions reports,
            string nwfFolder)
            : this(progress, log, exchange, reports)
        {
            this.reportFolder = string.IsNullOrEmpty(nwfFolder)
                    && string.IsNullOrEmpty(this.reports.ExcelFolder)
                ? null
                : this.reports.ChooseFor(nwfFolder).Folder;
        }

        /// <summary>
        /// For the open file and for the two hand buttons on the Clash step. No folder is
        /// handed in, because the report folder is read off the open file in
        /// RunOpenDocument through OpenDocumentJob.ReportFolder, the same rule the window
        /// uses for its line, and the hand buttons write no file at all. Handing a folder
        /// in here is what once wrote to Clash Reports\Clash Reports: the window passed the
        /// report folder as the NWF folder and the constructor built Clash Reports beside
        /// it again.
        /// </summary>
        public FederationEngine(
            Action<string> progress,
            RunLog log,
            ExchangeDocument exchange,
            ReportOptions reports)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.progress = progress ?? delegate { };
            this.log = log;
            this.exchange = exchange;
            this.reports = reports ?? new ReportOptions();
            this.guard = new RepeatedFailureGuard(this.reports.StopAfterFailures);
            this.reportFolder = null;
            this.live = new LiveLine(() => log.ElapsedSeconds);
            this.live.PaceReader = OnTheGroupBefore;

            // F61. The log decides WHEN to count and Core decides what a move means. This
            // is the only line that tells it HOW, and it reads the document that is open
            // at that moment rather than one captured here, because a group opening its
            // NWF replaces the open document.
            log.CensusReader = () => DocumentCensusReader.Read(NavisworksApplication.ActiveDocument);
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

                // The live line carries the group from here on, so the sentence beside it
                // says what is NEW rather than repeating what the line already holds.
                currentBuilding = job.Building;
                live.Groups(i + 1, jobs.Count, job.Building);
                Say(job.Files.Count + (job.Files.Count == 1 ? " file" : " files"));

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

                    Say("The run was stopped. " + stopTheRun);
                    break;
                }
            }

            // F63. One line for the whole run, so the standing rule has a number on it.
            log.Line(gaps.Line());

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
            string open = document == null ? string.Empty : Words.Or(document.FileName, "none");

            FederationJob job = OpenJob(open);

            // One group of one. The open file run has no scan and no grouping, and the
            // live line says so rather than reading as the first of an unknown number.
            currentBuilding = job.Building;
            live.Groups(1, 1, job.Building);
            JobOutcome outcome = new JobOutcome(job);
            outcome.NwfSize = -1;
            outcome.NwdSize = -1;

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

                    FinishTheGroup(document, job, outcome);
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

                // F61, the same as the scanned run. Before the return, so the outcome the
                // finally below reports already carries them.
                foreach (string reason in log.CensusFaults)
                {
                    outcome.AddError(reason);
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

                // F63. One group, so the run line and the group block say the same thing,
                // and it is still written, because a run that held nothing back saying so
                // is a check that ran and silence is not.
                log.Line(gaps.Line());

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
                name,
                null);
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
                    outcome.AddError("There is no active document.");
                    log.Line("GROUP    " + job.Building + " stopped, there is no active document");
                    return outcome;
                }

                NwfComparison comparison = null;

                InStep(
                    RunSteps.Decide,
                    () => comparison = Decide(document, job),
                    () => RunPath.Label(comparison.Decision, exchange != null));

                outcome.Decision = comparison.Decision;

                foreach (string line in comparison.Lines(job.NwfPath))
                {
                    log.Line(line);
                }

                if (comparison.Decision == RerunDecision.Changed)
                {
                    // Rebuilt from the scan folder, keeping the tests saved inside it.
                    // Bader decided this on 2026-09-07, Q22, after six groups in one run
                    // had an NWF built from an older folder with fewer files, were left
                    // alone, and had NWDs published off them with models missing. The
                    // rebuild happens BEFORE the units change and the clash step, and
                    // from here the group is treated exactly as an opened one.
                    if (!RebuildFromScan(document, job, outcome, comparison))
                    {
                        return outcome;
                    }
                }
                else if (comparison.Decision == RerunDecision.Build)
                {
                    if (!BuildFromScratch(document, job, outcome))
                    {
                        return outcome;
                    }
                }
                else
                {
                    // OPENED. The NWF is already open, because reading its file list is
                    // what opened it. It is not cleared and nothing is re-appended, so the
                    // clash results inside it survive.
                    outcome.AppendedCount = comparison.InNwf.Count;

                    // Verified, not recorded as written. This run did not write it, and
                    // the RESULT block's files written list says every size in it was read
                    // back after a write.
                    outcome.NwfSize = SizeOnDiskOrMinusOne(job.NwfPath);
                    outcome.NwfOnDisk = outcome.NwfSize >= 0;
                    log.Line("NWF      reused   " + job.NwfPath + "  "
                        + (outcome.NwfOnDisk ? outcome.NwfSize.ToString("#,##0") + " bytes" : "NOT ON DISK"));
                }

                FinishTheGroup(document, job, outcome);
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

            // F61. Outside the try, so a group that threw still carries whatever the
            // census saw before it did. Nothing was undone and nothing was skipped: the
            // count moved where the rule says it may not, the log said so, and the group
            // is not reported DONE.
            foreach (string reason in log.CensusFaults)
            {
                outcome.AddError(reason);
            }

            return outcome;
        }

        /// <summary>
        /// The tail every group runs once its document holds the models, whether it came
        /// from a scan through RunOne or is the open file through RunOpenDocument: the
        /// units, the clash step, the NWF saved again, the workbook, the NWD, and the NWF
        /// looked at once more. One method, so the two paths cannot drift apart in what
        /// they do or in what order. The two used to carry the same six calls each, and
        /// the comments sat on one of them only.
        /// </summary>
        private void FinishTheGroup(Document document, FederationJob job, JobOutcome outcome)
        {
            // The sets, the tests and the results all live in the NWF, so the clash
            // work happens BEFORE the NWF is saved for the last time and long before
            // the NWD is published. The NWD used to go first, which shipped it with no
            // sets and no results in it.
            // BEFORE the clash step, so every tolerance and every distance is read in
            // the units the report is going out in. Converting afterwards would mean a
            // report whose numbers and whose unit label disagree.
            if (reports.SetDocumentUnits)
            {
                Autodesk.Navisworks.Api.Units wanted;

                if (!TryWantedUnits(out wanted))
                {
                    // A unit name nobody can act on. The group stops here with nothing
                    // converted and nothing written, rather than running in units it
                    // was not asked for. It used to fall back to Meters without a word.
                    string name = string.IsNullOrEmpty(reports.UnitsName)
                        ? "an empty unit name"
                        : reports.UnitsName;

                    log.Line("UNITS    " + name + " is not one this tool knows");
                    outcome.AddError(
                        "the model units setting, " + name + ", is not one this tool knows, "
                            + "so nothing was converted and the group did not run. Known units are "
                            + string.Join(", ", new List<string>(UnitTable.EnumNames()).ToArray()));
                    return;
                }

                InStep(
                    RunSteps.Units,
                    () => new DocumentUnits(log).Apply(document, wanted),
                    () => "the document was asked for " + wanted);
            }

            bool clashPutSomethingIn = ClashStep(document, job, outcome);

            // F52. After the clash run and before the NWF is saved again, so a viewpoint
            // this run made is inside the file the NWD is published from.
            bool viewsPutSomethingIn = BuildViewpoints(document, job, outcome);

            if (clashPutSomethingIn || viewsPutSomethingIn)
            {
                InStep(
                    RunSteps.NwfSave,
                    () => SaveTheNwfAgain(document, job, outcome),
                    () => outcome.NwfOnDisk ? "saved " + outcome.NwfSize + " bytes" : "not saved");
            }

            // After the clash step and before the NWD, so the three outputs of a group
            // agree with each other rather than the workbook describing a state the
            // NWD does not carry.
            WriteWorkbook(job, outcome);

            InStep(
                RunSteps.Nwd,
                () => WriteNwd(document, job, outcome),
                () => outcome.NwdOnDisk ? "published " + outcome.NwdSize + " bytes" : "nothing published");

            // The NWD is published last, and the NWF is the only record of what has
            // been fixed, so the NWF is looked at once more AFTER it. Nothing was
            // checking this, and a RESULT block reporting the size of the first save
            // made it read as though publishing the NWD had emptied the file.
            InStep(
                RunSteps.Confirm,
                () => ConfirmTheNwfSurvived(job, outcome),
                () => outcome.NwfOnDisk ? "the NWF is " + outcome.NwfSize + " bytes" : "the NWF is NOT ON DISK");

            // F63. Last thing the group does, after every output is written, because the
            // question it answers is what the outputs DO NOT carry. Written even when it
            // is empty, because a missing block reads as a check that did not run.
            gaps.Add(GapRule.For(outcome.Report));
            log.Block(
                GapRule.BlockTitle + " " + Words.Or(job.Building, "this group"),
                GapRule.Lines(outcome.Report));
        }

        // ---------- the live line, F62 ----------

        /// <summary>
        /// Says something, with the group, the building, the step and both clocks in
        /// front of it. Everything the run used to hand to the callback goes through
        /// here, so there is still exactly one route out and it now carries the whole
        /// live line instead of a bare sentence.
        /// </summary>
        private void Say(string sentence)
        {
            progress(live.Line(sentence));
        }

        /// <summary>
        /// The same, for the places that say something once per test, once per set and
        /// once per viewpoint. It renders at most once a second, so a loop over 1830
        /// tests repaints the window about as often as a person can read it rather than
        /// 1830 times, and the step changing always shows.
        ///
        /// Anything said through here is in the log as well, so a message the throttle
        /// skips is never a message that was lost.
        /// </summary>
        private void Tick(string sentence)
        {
            if (live.ShouldSay())
            {
                progress(live.Line(sentence));
            }
        }

        /// <summary>
        /// What the same step took on the group before, off the records the log already
        /// keeps, so the pace on the live line and the seconds in the timing block are
        /// the same numbers.
        /// </summary>
        private double OnTheGroupBefore(string step)
        {
            IList<StepRecord> records = log.StepRecords;
            return LiveLine.OnTheGroupBefore(
                records, LiveLine.TheGroupBefore(records, currentBuilding), step);
        }

        // ---------- the steps, F59 ----------

        /// <summary>
        /// One named step around one piece of work.
        ///
        /// WHY A HELPER AND NOT A USING BLOCK AT EVERY STEP. The try, the Failed and the
        /// phrase are the same three lines at all fourteen of them, and fourteen copies of
        /// three lines is fourteen places for one of them to be left out. The one that
        /// matters is Failed: a step whose work threw has to say so and still carry its
        /// seconds, because time spent failing is time the run spent.
        ///
        /// THE STEP NEVER CHANGES WHAT THE RUN DOES. It opens, the work runs exactly as it
        /// did before, and it closes. Nothing is skipped, reordered or waited for, and a
        /// throw goes straight on up to the caller that already handled it.
        /// </summary>
        private void InStep(string name, Action work, Func<string> phrase)
        {
            using (RunStep step = log.Step(name))
            {
                live.StepStarted(name);
                Say(null);

                try
                {
                    work();
                }
                catch (Exception)
                {
                    step.Failed();
                    throw;
                }
                finally
                {
                    live.StepEnded();
                }

                step.Changed(phrase == null ? null : phrase());
            }
        }

        /// <summary>The same, for work that answers whether it changed the document.</summary>
        private bool InStepReturning(string name, Func<bool> work, Func<string> phrase)
        {
            using (RunStep step = log.Step(name))
            {
                live.StepStarted(name);
                Say(null);

                bool answer;

                try
                {
                    answer = work();
                }
                catch (Exception)
                {
                    step.Failed();
                    throw;
                }
                finally
                {
                    live.StepEnded();
                }

                step.Changed(phrase == null ? null : phrase());
                return answer;
            }
        }

        /// <summary>
        /// Works out which of the three cases this group is in. When an NWF is already
        /// there it is opened, because reading the file list out of it is the only way to
        /// compare, and the NWF is the record. No side file is kept.
        ///
        /// The DECIDE step is around the work itself, in RunOne, because the comparison it
        /// answers with is what the phrase on the finish line says.
        /// </summary>
        private NwfComparison Decide(Document document, FederationJob job)
        {
            if (!File.Exists(job.NwfPath))
            {
                return NwfComparison.NoNwfYet(job.Files);
            }

            Say("Opening the existing NWF for " + job.Building);
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
                        ? "   [source " + Words.Or(source, "none") + ", file " + Words.Or(cached, "none") + "]"
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
                        log.Line("         holds   " + Words.Or(cached, "none")
                            + "   [source " + Words.Or(source, "none") + ", file " + Words.Or(cached, "none") + "]");
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


        /// <summary>
        /// Clears the document and builds the group from nothing. Runs when there is no
        /// NWF at the output path, so there is no clash history to lose. The other clear
        /// is RebuildFromScan, which keeps the history. Returns false when nothing
        /// appended and nothing should be written.
        /// </summary>
        private bool BuildFromScratch(Document document, FederationJob job, JobOutcome outcome)
        {
            log.Line("CLEAR    the document, before building " + job.Building + " from scratch");
            document.Clear();

            if (!AppendAll(document, job, outcome))
            {
                return false;
            }

            WriteNwf(document, job, outcome);
            return true;
        }

        /// <summary>
        /// Appends every file of the group, in the group's order, into the document as it
        /// is. Shared by the first build and the rebuild, so the two cannot drift. Returns
        /// false when nothing appended, which is a group that produced nothing.
        /// </summary>
        private bool AppendAll(Document document, FederationJob job, JobOutcome outcome)
        {
            // The step is here and not at the two call sites, because both the first build
            // and the rebuild put the models in and a reader asking where the time went
            // wants one number for that.
            return InStepReturning(
                RunSteps.Append,
                () => AppendEveryFile(document, job, outcome),
                () => outcome.AppendedCount + " of " + job.Files.Count + " appended");
        }

        private bool AppendEveryFile(Document document, FederationJob job, JobOutcome outcome)
        {
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
            return true;
        }

        /// <summary>
        /// Clears the opened NWF and rebuilds it from the scan folder, keeping the clash
        /// tests saved inside it. F24, bug B13, Q22.
        ///
        /// WHY. Six groups in the run of 2026-09-07 had an NWF built from an older folder
        /// holding fewer files than the scan, 1B06BC 4 of 5 with EL missing, 1B06K1 1 of
        /// 4. CHANGED left every one alone, so nothing rebuilt them and the NWDs went out
        /// missing models. Bader decided: rebuild from the scan by itself, keep the saved
        /// tests, and say what was added, what moved and what was removed.
        ///
        /// HOW THE TESTS ARE KEPT. What the NWF holds is read BEFORE the clear, in two
        /// forms. SavedTests.Read gives the count and the names as Core sees them, which
        /// is what the log and the check use. DocumentClashTests.CreateCopy gives the
        /// document's own value copy of the tests, and DocumentSelectionSets.CreateCopy
        /// the same for the sets, which are what can be put back with CopyFrom. Whether
        /// Document.Clear keeps either is UNKNOWN until a run: the count is read off the
        /// document after the appends, the copies are put back only where the count
        /// dropped, and the count is read again. The log line says which happened.
        /// CreateCopy and CopyFrom on DocumentClashTests were read off the DLL on
        /// 2026-08-27, docs\history\scan.md section 4. The same pair on DocumentSelectionSets was
        /// NOT measured and is used on the strength of the Navisworks pattern every
        /// document part follows, so a build error there names exactly this.
        ///
        /// WHAT IS NOT DONE. The NWF on disk is only saved over once every test read
        /// before the clear is back in the document. Where they cannot be put back the
        /// group fails, says so, and the NWF on disk keeps its file list and its tests.
        /// </summary>
        private bool RebuildFromScan(
            Document document, FederationJob job, JobOutcome outcome, NwfComparison comparison)
        {
            NwfRebuildPlan plan = NwfRebuildPlan.From(comparison);

            foreach (string line in plan.Lines(job.NwfPath))
            {
                log.Line(line);
            }

            Say("Rebuilding the NWF for " + job.Building + " from the scan folder");

            // What the NWF holds, read BEFORE the clear. The tests as Core sees them and
            // the sets as a count off the tree, which is what the log and the check use,
            // plus the document's own value copies of both, which are what can be put
            // back. F29: the sets are counted and put back on their own, because a test
            // side points at a set, and a document that came back with its tests and
            // without its sets would run every test against nothing.
            IList<SavedClashTest> saved = SavedTests.Read(document);

            // F50. Four things, one rule. The sets and the tests were always counted out
            // and counted back. The viewpoints and the clash result statuses are counted
            // the same way now, because the NWF carries those too and neither has a second
            // copy anywhere. Any one of the four not coming back leaves the NWF on disk
            // alone. The keep rule itself is Federator.Core.Rerun.RebuildTally, written
            // once, where it used to be written twice in the same words.
            RebuildTally tally = new RebuildTally();
            RebuiltThing sets = tally.Count("SETS", "selection sets");
            RebuiltThing tests = tally.Count("TESTS", "saved clash tests");
            RebuiltThing views = tally.Count("VIEWS", "saved viewpoints");
            RebuiltThing statuses = tally.Count("RESULTS", "clash results carrying a status a person set");

            sets.Before = DocumentCensusReader.Sets(document.SelectionSets);
            tests.Before = saved.Count;
            views.Before = SavedViewpoints.Count(document);
            statuses.Before = SavedStatuses.SetByAPerson(document);

            // What kind of decision is at stake, read before the clear, so a rebuild that
            // loses something says which kind went and not only how many.
            string statusesBefore = StatusesAPersonSet.Describe(SavedStatuses.In(document));

            ClashTestsData testsCopy = null;
            SavedItemCollection setsCopy = null;

            try
            {
                testsCopy = document.GetClash().TestsData.CreateCopy();
                setsCopy = document.SelectionSets.CreateCopy();
            }
            catch (Exception error)
            {
                log.Failure(
                    "copying the saved sets and tests of " + job.Building + " before the clear",
                    error,
                    "kept going, whether the clear keeps them is read off the document after the appends");
            }

            try
            {
                log.Line("CLEAR    the document, before rebuilding " + job.Building + " from the scan folder");
                document.Clear();

                if (!AppendAll(document, job, outcome))
                {
                    log.Line("NWF      NOT saved over, the NWF on disk keeps its file list, its sets and its tests");
                    return false;
                }

                // Sets first, on their own count, whatever the tests did. A test side
                // points at a set, so the tests go back into a document that already
                // holds what they point at.
                sets.AfterAppends = DocumentCensusReader.Sets(document.SelectionSets);
                sets.AfterRestore = sets.AfterAppends;

                if (sets.NeedsRestoring && setsCopy != null)
                {
                    try
                    {
                        document.SelectionSets.CopyFrom(setsCopy);
                        sets.AfterRestore = DocumentCensusReader.Sets(document.SelectionSets);
                    }
                    catch (Exception error)
                    {
                        log.Failure(
                            "putting the saved sets back into " + job.Building,
                            error,
                            "the SETS line below carries what the document holds now");
                    }
                }

                tests.AfterAppends = SavedTests.Count(document);
                tests.AfterRestore = tests.AfterAppends;

                if (tests.NeedsRestoring && testsCopy != null)
                {
                    try
                    {
                        document.GetClash().TestsData.CopyFrom(testsCopy);
                        tests.AfterRestore = SavedTests.Count(document);
                    }
                    catch (Exception error)
                    {
                        log.Failure(
                            "putting the saved tests back into " + job.Building,
                            error,
                            "the TESTS line below carries what the document holds now");
                    }
                }

                // The viewpoints and the statuses ride back inside the two copies above and
                // nothing puts them back on their own. They are counted because a count is
                // the only way to know they came, and because counting them is what turns a
                // silent loss into a group that fails with the NWF left alone. Both are read
                // after everything else has been put back, so what they report is the final
                // state of the document and not a stage of it.
                views.AfterAppends = SavedViewpoints.Count(document);
                views.AfterRestore = views.AfterAppends;

                statuses.AfterAppends = SavedStatuses.SetByAPerson(document);
                statuses.AfterRestore = statuses.AfterAppends;

                foreach (string line in tally.Lines())
                {
                    log.Line(line);
                }

                if (statuses.Before > 0)
                {
                    log.Line("RESULTS  before the clear that was " + statusesBefore
                        + ", and now " + StatusesAPersonSet.Describe(SavedStatuses.In(document)));
                }

                foreach (string reason in tally.LostReasons())
                {
                    outcome.AddError(reason);
                }

                if (!tally.EverythingKept)
                {
                    log.Line("NWF      NOT saved over, the NWF on disk keeps its file list, its sets, its tests, "
                        + "its viewpoints and every status a person set");
                    return false;
                }

                WriteNwf(document, job, outcome);
                outcome.Decision = RerunDecision.Rebuilt;
                return true;
            }
            finally
            {
                if (testsCopy != null)
                {
                    testsCopy.Dispose();
                }

                // SavedItemCollection was not IDisposable on the DLL measured on 2026-08-31,
                // docs/history/scan.md section 4g, so the copy is disposed only where the type
                // turns out to be. This compiles either way.
                IDisposable setsHandle = setsCopy as IDisposable;

                if (setsHandle != null)
                {
                    setsHandle.Dispose();
                }
            }
        }

        /// <summary>
        /// The label each group will take, worked out by opening each NWF that is on disk
        /// and comparing its file list with the scan, BEFORE the confirm dialog, so the
        /// dialog can count the Rebuilt groups and the list can show them. The window only
        /// calls this when nothing open would be lost, because opening an NWF replaces the
        /// open document and the person has not yet said yes.
        ///
        /// Every NWF is opened again by Decide in the run itself, and that is the read
        /// that counts. This changes what the person is told and nothing else, so nothing
        /// here records a Revit source or writes a holds line. A group that will not open
        /// reads Unknown here and the run decides.
        /// </summary>
        public static IList<string> PreviewRunPaths(
            IList<FederationJob> jobs, bool xmlPicked, RunLog log, Action<string> progress)
        {
            if (jobs == null)
            {
                throw new ArgumentNullException("jobs");
            }

            List<string> labels = new List<string>();
            Stopwatch clock = Stopwatch.StartNew();
            int opened = 0;

            for (int i = 0; i < jobs.Count; i++)
            {
                FederationJob job = jobs[i];
                string label;

                try
                {
                    if (!File.Exists(job.NwfPath))
                    {
                        label = RunPath.FirstRun;
                    }
                    else
                    {
                        if (progress != null)
                        {
                            progress("Checking NWF " + (i + 1) + " of " + jobs.Count + ": " + job.Building);
                        }

                        Document document = NavisworksApplication.ActiveDocument;

                        if (document == null || !document.TryOpenFile(job.NwfPath))
                        {
                            label = RunPath.Unknown;
                        }
                        else
                        {
                            opened++;
                            NwfComparison comparison = NwfComparison.Compare(ModelFileList(document), job.Files);
                            label = RunPath.AfterOpening(comparison.Decision, xmlPicked);
                        }
                    }
                }
                catch (Exception error)
                {
                    label = RunPath.Unknown;

                    if (log != null)
                    {
                        log.Failure(
                            "checking the NWF of " + job.Building + " before the run",
                            error,
                            "the group reads Unknown in the list and the run decides");
                    }
                }

                labels.Add(label);

                if (log != null)
                {
                    log.Line("PREVIEW  " + job.Building + "  " + label);
                }
            }

            clock.Stop();

            if (log != null)
            {
                log.Line("PREVIEW  opened " + opened + (opened == 1 ? " NWF" : " NWFs")
                    + " before the confirm dialog in "
                    + clock.Elapsed.TotalSeconds.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "s");
            }

            return labels;
        }

        /// <summary>The NWC each model points at, with no logging and no recording. See FilesInsideTheOpenDocument.</summary>
        private static IList<string> ModelFileList(Document document)
        {
            List<string> files = new List<string>();

            if (document == null || document.Models == null)
            {
                return files;
            }

            foreach (Model model in document.Models)
            {
                string use = ModelFileNames.PathOf(model.FileName, model.SourceFileName);

                if (!string.IsNullOrEmpty(use))
                {
                    files.Add(use);
                }
            }

            return files;
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
            // Inside the method, because the first build and the rebuild both call it and
            // one step name belongs in one place.
            InStep(
                RunSteps.NwfSave,
                () => SaveTheNwf(document, job, outcome),
                () => outcome.NwfOnDisk ? "saved " + outcome.NwfSize + " bytes" : "not saved");
        }

        private void SaveTheNwf(Document document, FederationJob job, JobOutcome outcome)
        {
            Say("Saving NWF for " + job.Building);
            log.WriteAttempted("NWF", job.NwfPath);

            try
            {
                EnsureFolder(job.NwfPath);

                // This also runs on a rebuild, where an NWF IS already at the path, so
                // the bool is what says the save happened rather than the file being
                // there. It says more in the log than an absent file does either way.
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
            // F54. A status written into the document is a change, so it asks for the NWF
            // the same way a test that ran does.
            return CreateAndRunTheTests(document, job, outcome, source) || changed;
        }

        /// <summary>
        /// The Build sets button on the Clash step: the picked file's sets into the open
        /// document, nothing else. No scan, no NWF saved, no test created. It is the same
        /// BuildTheSets the run calls per group, so the SETS lines in the log read the
        /// same whichever way the sets were built. The open file stands as the group. D4.
        /// </summary>
        public SetBuildOutcome BuildSetsByHand()
        {
            Document document = NavisworksApplication.ActiveDocument;

            if (document == null)
            {
                log.Line("SETS     stopped, there is no active document");
                return new SetBuildOutcome();
            }

            FederationJob job = OpenJob(Words.Or(document.FileName, "none"));
            JobOutcome outcome = new JobOutcome(job);

            BuildTheSets(document, job, outcome);
            return outcome.Sets ?? new SetBuildOutcome();
        }

        /// <summary>
        /// The Run tests button on the Clash step: the picked file's tests created into
        /// and run against the open document, nothing federated and no NWF saved. It is
        /// the same CreateAndRunTheTests the run calls per group, so the CLASH lines in
        /// the log read the same either way, and the report is built in memory the way a
        /// run's is. With no report folder no picture and no file is written. Null when
        /// nothing is open, when the file holds no test, or when the step threw, and the
        /// log says which. D4.
        /// </summary>
        public ClashRunOutcome RunTestsByHand()
        {
            Document document = NavisworksApplication.ActiveDocument;

            if (document == null)
            {
                log.Line("CLASH    stopped, there is no active document");
                return null;
            }

            FederationJob job = OpenJob(Words.Or(document.FileName, "none"));
            JobOutcome outcome = new JobOutcome(job);

            CreateAndRunTheTests(document, job, outcome, ClashSource.TestsFromXml);
            return outcome.Clash;
        }

        private bool BuildTheSets(Document document, FederationJob job, JobOutcome outcome)
        {
            // Inside the method, because the run and the Sets into open model button both
            // call it and the SETS lines have to read the same whichever way they were built.
            return InStepReturning(
                RunSteps.Sets,
                () => BuildTheSetsFromTheFile(document, job, outcome),
                () => outcome.Sets == null ? "UNKNOWN, no set outcome was recorded" : outcome.Sets.Summary());
        }

        private bool BuildTheSetsFromTheFile(Document document, FederationJob job, JobOutcome outcome)
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
                    // Nothing to build is still an outcome, so the window's Build sets
                    // button has lines and a summary to show, and the skipped sets are in
                    // them by name rather than lost.
                    SetBuildOutcome nothing = new SetBuildOutcome();

                    foreach (SkippedSet skipped in plan.Skipped)
                    {
                        nothing.AddSkipped(skipped);
                    }

                    outcome.Sets = nothing;
                    log.Line("SETS     " + nothing.Summary()
                        + " The tests will resolve against whatever sets the model already holds.");
                    return false;
                }

                log.Line("SETS     " + job.Building + ", " + plan.Buildable.Count + " to build, "
                    + plan.Skipped.Count + " skipped");

                SetBuildOutcome sets = new SetBuilder(Tick, log).Build(plan);
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

                ClashRunner runner = new ClashRunner(Tick, log, guard);

                // The three steps that run per test are opened in there, so the live line
                // is handed over rather than the engine guessing at what it is doing.
                runner.Live = live;
                runner.NameSettings = reports.Names;
                runner.SingleDisciplineGroup = job.IsSingleDiscipline;
                runner.ApplyFileSettings = reports.ApplyFileSettings;
                runner.CompactResolved = reports.CompactResolved;

                if (runner.SingleDisciplineGroup)
                {
                    log.Line("CLASH    " + job.Building + " holds one discipline, so every test is created "
                        + "and none is run. One discipline cannot clash with itself.");
                }

                // Each output answers to its own flag. The report is built when the
                // workbook, the XML or the page is wanted, because all three are made
                // from it. It used to be built only for the workbook or the XML, so the
                // page rode on the workbook flag and would have gone with it.
                OutputPlan outputs = OutputPlan.From(reports);
                log.Line("OUTPUTS  " + outputs);

                if (outputs.BuildReport)
                {
                    ClashReport report = new ClashReport(job.Building, job.OutputName);
                    report.SourceFile = exchange == null
                        ? "the tests saved in the open document"
                        : exchange.SourcePath;
                    report.DocumentUnits = units;
                    report.RunAt = DateTime.Now;
                    report.BuildStamp = BuildStamp.Of(typeof(FederationEngine).Assembly);
                    runner.Report = report;
                    outcome.Report = report;

                    // The pictures go in a folder named after the workbook and beside it,
                    // which is also where the page and the XML sit, so where the workbook
                    // would go has to be known before the clash step rather than after it.
                    // Rendered when pictures are wanted and any report is wanted to link
                    // them from. They used to ride on the workbook flag alone.
                    if (outputs.WriteImages && reportFolder != null)
                    {
                        runner.WorkbookPath = ReportPaths.Workbook(reportFolder, job.WorkbookName);
                        runner.Images = new ClashImages(log, reports.Images);
                        log.Line("CLASH    " + reports.Images.Describe());
                    }
                    else
                    {
                        log.WriteSkipped("IMAGES", outputs.ImagesSkipReason ?? "no report folder");
                    }
                }
                else
                {
                    log.WriteSkipped("IMAGES", outputs.ImagesSkipReason);
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

                if (outcome.Report != null)
                {
                    // The Item ID label is always Element ID and this tool chooses it, so
                    // which property actually supplied each id is said here. Counted per
                    // property, never a line per item.
                    IList<string> idSources = outcome.Report.IdSourceLines();

                    if (idSources.Count > 0)
                    {
                        log.Block("ITEM IDS " + job.Building, idSources);
                    }
                }

                if (outcome.Report != null && runner.Images != null)
                {
                    // The pictures were rendered under run order numbers while the tests
                    // ran, because the report order is only known when the last test has
                    // run. Renamed once here, before any report is written, so every
                    // picture carries the number of its row. A rename that throws is a
                    // warning on the report and never fails the group, since the NWF, the
                    // pictures and the workbook are all still written.
                    RenumberThePictures(job, outcome, runner.WorkbookPath);
                }

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

                // F54. A status written into the document is a write like any other, so it
                // asks for the NWF to be saved again even where nothing was created and
                // nothing ran.
                return clash.CreatedCount > 0 || clash.RanCount > 0 || runner.ChangedAStatus;
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
        /// Renames the pictures into report order. The rule and the two pass move live in
        /// Federator.Core.Report.ImageRenumbering so they can be tested without Navisworks.
        /// </summary>
        private void RenumberThePictures(FederationJob job, JobOutcome outcome, string workbookPath)
        {
            try
            {
                ImageRenumberingOutcome renumbered = ImageRenumbering.Apply(outcome.Report, workbookPath);

                foreach (string line in renumbered.Lines())
                {
                    log.Line(line);
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "renumbering the pictures for " + job.Building,
                    error,
                    "kept going, the pictures that were not yet renamed keep their run order numbers");
            }
        }

        /// <summary>
        /// Reads the workbook back off the disk and counts how many rows filled each of
        /// our own columns. A column filled zero times is named.
        ///
        /// This is the check that would have caught Source File and Discipline coming out
        /// empty on every row, without anyone opening the file to find out.
        /// </summary>
        private void CheckTheWorkbook(FederationJob job, string path)
        {
            WorkbookCheck check = WorkbookCheck.Of(path);

            log.Block("WORKBOOK CHECK " + job.Building, check.Lines());
            Say(job.Building + ". " + check.Summary());
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
                log.WriteSkipped("HTML", OutputPlan.NotWanted);
                return;
            }

            string stylesheet = InstallFiles.FindStylesheet(
                NavisworksFacts.InstallFolder(), NavisworksFacts.Language());

            if (stylesheet.Length == 0)
            {
                foreach (string line in InstallFiles.WhyNoStylesheet(
                    NavisworksFacts.InstallFolder(), NavisworksFacts.Language()))
                {
                    log.Line(line);
                }

                log.WriteSkipped("HTML", "the stylesheet was not found in the install, see the lines above");
                return;
            }

            string path = HtmlTabularWriter.PathFor(
                ReportPaths.Workbook(reportFolder, job.WorkbookName));

            Say("Writing the client report for " + job.Building);
            log.WriteAttempted("HTML", path);

            try
            {
                // The page IS the client's report and always carries only what theirs
                // carries. Our extra columns live on the clash XML and nowhere else.
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

            CheckThePage(job, path);
        }

        /// <summary>
        /// The units the Outputs step asked for, by the name on the Navisworks enum, read
        /// through UnitTable so the name is one this tool knows before Enum.Parse sees it.
        /// False is a name the table does not know, or one the installed enum does not
        /// carry, and the caller fails the group. It used to fall back to Meters on any
        /// name it could not parse, silently, which is a setting replaced by a guess.
        /// </summary>
        private bool TryWantedUnits(out Autodesk.Navisworks.Api.Units wanted)
        {
            wanted = Autodesk.Navisworks.Api.Units.Meters;

            UnitRow row = UnitTable.FindByEnumName(reports.UnitsName);

            if (row == null)
            {
                return false;
            }

            try
            {
                wanted = (Autodesk.Navisworks.Api.Units)Enum.Parse(
                    typeof(Autodesk.Navisworks.Api.Units), row.EnumName, false);
                return true;
            }
            catch (ArgumentException)
            {
                log.Line("UNITS    " + row.EnumName + " is in this tool's table and not on the installed enum");
                return false;
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
        private void CheckThePage(FederationJob job, string path)
        {
            PageCheck check = PageCheck.Of(path);

            log.Block("REPORT CHECK " + job.Building, check.Lines());
            Say(job.Building + ". " + check.Summary());
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
                    foreach (string line in InstallFiles.WhyNoLogo(
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
            OutputPlan outputs = OutputPlan.From(reports);

            if (report == null || reportFolder == null)
            {
                // Nothing to write from, or nowhere to write to. Each output still gets
                // its line, so a log never leaves an output unaccounted for.
                string why = reportFolder == null
                    ? "no report folder"
                    : outputs.BuildReport
                        ? "no clash step ran for this group, so there is nothing to report"
                        : OutputPlan.NotWanted;

                log.WriteSkipped("XLSX", outputs.WriteWorkbook ? why : OutputPlan.NotWanted);
                log.WriteSkipped("HTML", outputs.WriteHtml ? why : OutputPlan.NotWanted);
                log.WriteSkipped("XML", outputs.WriteXml ? why : OutputPlan.NotWanted);
                return;
            }

            // METERS, always. F26, Q23. Every number came out of the document in the
            // document's units, and this converts the whole report once, before the
            // workbook, the page and the XML are written, so all three read the same
            // converted numbers and the same label. A unit the table has not been taught
            // fails the group rather than writing a report in the wrong unit.
            ReportUnitsOutcome units = ReportUnits.ToMeters(report);
            log.Line(units.Line());

            if (units.Refused)
            {
                outcome.AddError(
                    "the report was not written because its numbers could not be converted to "
                    + ReportUnits.Name + ": " + units.Problem);
                log.WriteSkipped("XLSX", "the numbers are not in " + ReportUnits.Name);
                log.WriteSkipped("HTML", "the numbers are not in " + ReportUnits.Name);
                log.WriteSkipped("XML", "the numbers are not in " + ReportUnits.Name);
                return;
            }

            if (!outputs.WriteWorkbook)

            {
                log.WriteSkipped("XLSX", OutputPlan.NotWanted);
            }
            else
            {
                string path = ReportPaths.Workbook(reportFolder, job.WorkbookName);

                InStep(
                    RunSteps.Workbook,
                    () => WriteTheWorkbook(job, outcome, report, path),
                    () => outcome.WorkbookOnDisk
                        ? "wrote " + outcome.WorkbookSize + " bytes"
                        : "nothing on disk");
            }

            InStep(
                RunSteps.Html,
                () => WriteHtmlTabular(job, outcome, report),
                () => outcome.HtmlOnDisk ? "wrote " + outcome.HtmlSize + " bytes" : "nothing on disk");

            if (!outputs.WriteXml)
            {
                log.WriteSkipped("XML", OutputPlan.NotWanted);
                return;
            }

            string xmlPath = ReportPaths.Xml(reportFolder, job.WorkbookName);

            InStep(
                RunSteps.Xml,
                () => WriteTheClashXml(job, outcome, report, xmlPath),
                () => outcome.XmlSize >= 0 ? "wrote " + outcome.XmlSize + " bytes" : "nothing on disk");
        }

        /// <summary>
        /// The workbook itself. Split out of WriteWorkbook so the WORKBOOK step is around
        /// the write, the read back and the check, and around nothing else.
        /// </summary>
        private void WriteTheWorkbook(
            FederationJob job, JobOutcome outcome, ClashReport report, string path)
        {
            Say("Writing the workbook for " + job.Building);
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

            CheckTheWorkbook(job, path);
        }

        /// <summary>The clash XML itself, split out for the same reason.</summary>
        private void WriteTheClashXml(
            FederationJob job, JobOutcome outcome, ClashReport report, string xmlPath)
        {
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
            Say("Saving the NWF for " + job.Building + " with its sets and results");
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

        /// <summary>
        /// Publishes the NWD, every run. It used to be a tick box, on by default, and a
        /// weekly run wanted it every time, so it is fixed on and there is no branch here
        /// for a run that does not want it.
        /// </summary>
        /// <summary>
        /// One folder per discipline with one viewpoint in each, F52. Returns whether
        /// anything new went into the document, which is what asks for the second NWF save.
        ///
        /// WHILE THE API IS UNMEASURED this plans the viewpoints, says in the log what it
        /// would have made, and attempts nothing. SavedViewpoints.CanBuild is the one
        /// switch, and it is false until tools\probes\probe-viewpoints.ps1 has been run.
        /// The group is told the viewpoints were NOT REQUESTED, because a step this tool
        /// cannot do is not a step that failed, and reporting every group FAILED over a
        /// feature that was never attempted is the fault that once called a clean 22 group
        /// run failed over a missing NWD nobody had asked for.
        /// </summary>
        private bool BuildViewpoints(Document document, FederationJob job, JobOutcome outcome)
        {
            ViewpointSettings views = new ViewpointSettings();
            IList<PlannedViewpoint> planned = ViewpointPlan.For(job.Disciplines, views);

            log.Line("VIEWS    " + ViewpointPlan.Describe(planned));

            if (planned.Count == 0)
            {
                return false;
            }

            if (!SavedViewpoints.CanBuild)
            {
                log.Line(SavedViewpoints.WhyNotYet());
                outcome.ViewpointsRequested = false;
                return false;
            }

            outcome.ViewpointsRequested = true;

            // BUILT and not views. The settings above are already called views, and a
            // second local of that name in the same scope is CS0128, which is why the
            // add-in did not build between F52 and F58.
            ViewpointBuildOutcome built = new ViewpointBuilder(Tick, log, views.Sizes)
                .Build(document, planned);

            log.Block("VIEWS", built.Lines());
            outcome.FailedViewpointCount = built.FailedCount;

            return built.PutAnythingIn;
        }

        private void WriteNwd(Document document, FederationJob job, JobOutcome outcome)
        {
            Say("Publishing NWD for " + job.Building);
            log.WriteAttempted("NWD", job.NwdPath);

            bool published = false;

            try
            {
                EnsureFolder(job.NwdPath);

                // PublishProperties is the 2025 way to write an NWD. NwdExportOptions is
                // a 2026 class and does not exist here.
                using (PublishProperties properties = new PublishProperties())
                {
                    // Every value below is written once, into a local, and then used
                    // twice: once to set the property and once to record it. Reading a
                    // value back off the handle to record it would make the line mean two
                    // different things row by row, what we asked for on some and what the
                    // native object holds on others, and this line is read months later by
                    // someone who cannot ask which.
                    PublishedProperties whatWasSet = new PublishedProperties();

                    string title = job.OutputName;
                    string publisher = "Parsons NWC Federator";
                    string subject = "Federation of building " + job.Building;
                    string author = Environment.UserName;

                    properties.Title = title;
                    whatWasSet.Set("Title", title);

                    properties.Publisher = publisher;
                    whatWasSet.Set("Publisher", publisher);

                    properties.Subject = subject;
                    whatWasSet.Set("Subject", subject);

                    properties.Author = author;
                    whatWasSet.Set("Author", author);

                    // F51. An NWD published without May be re-saved cannot be translated by
                    // the ACC and Forma viewer, which is why every NWD this tool has
                    // published so far shows a processing error beside it up there. All
                    // three names below are on the list read off the installed DLL on
                    // 2026-08-29, docs\history\scan.md section 4b, each with a getter and a
                    // setter, so none of them is assumed.
                    properties.AllowResave = true;
                    whatWasSet.Set("AllowResave", true);

                    // The second Autodesk article: an NWD reaching ACC with no properties,
                    // every object showing as solid. These two are what carry the object
                    // properties into the published file.
                    properties.EmbedDatabaseProperties = true;
                    whatWasSet.Set("EmbedDatabaseProperties", true);

                    properties.PreventObjectPropertyExport = false;
                    whatWasSet.Set("PreventObjectPropertyExport", false);

                    // Written BEFORE the publish, so an NWD whose publish throws still
                    // leaves behind what it was asked to carry.
                    log.Line(whatWasSet.Line());

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

            // How many, never what they say. This line is a label, and the errors carry
            // the type name and the message of whatever threw.
            line.Append(RunLog.ErrorsAreInTheLog(outcome.Errors.Count));

            return line.ToString();
        }
    }
}
