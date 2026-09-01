using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using Federator.Core.Rerun;
using Federator.Core.Sets;

namespace Federator.Addin.Engine
{
    /// <summary>One building's worth of work, fully decided before the run starts.</summary>
    public sealed class FederationJob
    {
        public FederationJob(string building, string outputName, string nwfPath, string nwdPath, IList<string> files)
            : this(building, outputName, nwfPath, nwdPath, files, outputName)
        {
        }

        /// <summary>
        /// The workbook can carry a different name from the NWF, because the Outputs step
        /// holds one naming pattern for each of the three.
        /// </summary>
        public FederationJob(
            string building,
            string outputName,
            string nwfPath,
            string nwdPath,
            IList<string> files,
            string workbookName)
        {
            if (building == null)
            {
                throw new ArgumentNullException("building");
            }

            if (files == null)
            {
                throw new ArgumentNullException("files");
            }

            Building = building;
            OutputName = outputName;
            NwfPath = nwfPath;
            NwdPath = nwdPath;
            Files = files;
            WorkbookName = string.IsNullOrEmpty(workbookName) ? outputName : workbookName;
        }

        public string Building { get; private set; }

        public string OutputName { get; private set; }

        public string NwfPath { get; private set; }

        public string NwdPath { get; private set; }

        /// <summary>Full paths, appended in this order.</summary>
        public IList<string> Files { get; private set; }

        /// <summary>What the workbook is called. The NWF name unless a pattern differs.</summary>
        public string WorkbookName { get; private set; }
    }

    /// <summary>What actually happened to one group, checked against the disk.</summary>
    public sealed class JobOutcome
    {
        private readonly List<string> errors = new List<string>();

        public JobOutcome(FederationJob job)
        {
            Job = job;
            FailedFiles = new List<string>();
        }

        public FederationJob Job { get; private set; }

        public int AppendedCount { get; set; }

        public IList<string> FailedFiles { get; private set; }

        /// <summary>Set only after File.Exists has been checked.</summary>
        public bool NwfOnDisk { get; set; }

        /// <summary>Set only after File.Exists has been checked.</summary>
        public bool NwdOnDisk { get; set; }

        /// <summary>
        /// Everything that threw for this group. A list, because the model side, the sets,
        /// the tests and the run can each throw on their own and one slot would keep only
        /// the last of them.
        /// </summary>
        public IList<string> Errors
        {
            get { return errors; }
        }

        public void AddError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                errors.Add(error);
            }
        }

        public bool HasErrors
        {
            get { return errors.Count > 0; }
        }

        private readonly List<string> reportWarnings = new List<string>();

        /// <summary>
        /// What the report checks found wrong, kept apart from the errors on purpose.
        ///
        /// A page with a column too many is worth saying loudly and is NOT a failed group.
        /// The NWF, the NWD and the workbook were all written. Judging the group on this
        /// would report a whole clean run as failed the way an earlier one did.
        /// </summary>
        public IList<string> ReportWarnings
        {
            get { return reportWarnings; }
        }

        public void AddReportWarning(string warning)
        {
            if (!string.IsNullOrEmpty(warning))
            {
                reportWarnings.Add(warning);
            }
        }

        /// <summary>
        /// Whether this run was asked to republish the NWD. When the tick box is off the
        /// NWD is not a requested step, so its absence is not a failure. Reporting it as
        /// one made every group of a clean 22 group run read FAILED.
        /// </summary>
        public bool NwdRequested { get; set; }

        /// <summary>
        /// Whether the publish call reported success. On a rerun last week's NWD sits at
        /// the same path, so File.Exists on its own cannot tell a fresh publish from a
        /// stale file.
        /// </summary>
        public bool NwdPublishReportedSuccess { get; set; }

        /// <summary>
        /// How this group ended. The rule itself lives in Federator.Core.Rerun so it can
        /// be tested without Navisworks. This only gathers the facts.
        /// </summary>
        public GroupOutcome Result
        {
            get { return GroupJudgement.Judge(Facts()); }
        }

        /// <summary>
        /// Why this group is not DONE, or null when it is. Comes out of the same pass that
        /// decides the outcome, so the two can never disagree and a FAILED group can never
        /// reach the log without a reason.
        /// </summary>
        public string Reason
        {
            get { return GroupJudgement.ReasonFor(Facts()); }
        }

        /// <summary>Everything the judgement needs, with no Navisworks types in it.</summary>
        public GroupFacts Facts()
        {
            GroupFacts facts = new GroupFacts
            {
                Decision = Decision,
                NwfOnDisk = NwfOnDisk,
                NwdRequested = NwdRequested,
                NwdOnDisk = NwdOnDisk,
                NwdPublishReportedSuccess = NwdPublishReportedSuccess,
                AppendedCount = AppendedCount,
                FileCount = Job == null ? 0 : Job.Files.Count,
                FailedFileCount = FailedFiles.Count,
                NwfPath = Job == null ? null : Job.NwfPath,
                NwdPath = Job == null ? null : Job.NwdPath
            };

            foreach (string error in errors)
            {
                facts.AddError(error);
            }

            return facts;
        }

        /// <summary>Which of the three rerun cases this group turned out to be.</summary>
        public RerunDecision Decision { get; set; }

        /// <summary>Size read back off the disk, or minus one when the NWF is not there.</summary>
        public long NwfSize { get; set; }

        /// <summary>The client report page, minus one until its size is read off the disk.</summary>
        public long HtmlSize { get; set; }

        public bool HtmlOnDisk { get; set; }

        /// <summary>Size read back off the disk, or minus one when the NWD is not there.</summary>
        public long NwdSize { get; set; }

        /// <summary>What the sets step did for this group, or null when it did not run.</summary>
        public SetBuildOutcome Sets { get; set; }

        /// <summary>What the clash step did for this group, or null when it did not run.</summary>
        public ClashRunOutcome Clash { get; set; }

        /// <summary>The workbook model for this group, or null when no report was wanted.</summary>
        public ClashReport Report { get; set; }

        /// <summary>Size read back off the disk, or minus one when the workbook is not there.</summary>
        public long WorkbookSize { get; set; }

        public bool WorkbookOnDisk { get; set; }

        /// <summary>Size read back off the disk, or minus one when no XML was asked for.</summary>
        public long XmlSize { get; set; }
    }
}
