using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;
using Federator.Core.Rerun;

namespace Federator.Addin.Engine
{
    /// <summary>One building's worth of work, fully decided before the run starts.</summary>
    public sealed class FederationJob
    {
        public FederationJob(string building, string outputName, string nwfPath, string nwdPath, IList<string> files)
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
        }

        public string Building { get; private set; }

        public string OutputName { get; private set; }

        public string NwfPath { get; private set; }

        public string NwdPath { get; private set; }

        /// <summary>Full paths, appended in this order.</summary>
        public IList<string> Files { get; private set; }
    }

    /// <summary>What actually happened to one group, checked against the disk.</summary>
    public sealed class JobOutcome
    {
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

        public string Error { get; set; }

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
            return new GroupFacts
            {
                Decision = Decision,
                NwfOnDisk = NwfOnDisk,
                NwdRequested = NwdRequested,
                NwdOnDisk = NwdOnDisk,
                NwdPublishReportedSuccess = NwdPublishReportedSuccess,
                AppendedCount = AppendedCount,
                FileCount = Job == null ? 0 : Job.Files.Count,
                FailedFileCount = FailedFiles.Count,
                Error = Error,
                NwfPath = Job == null ? null : Job.NwfPath,
                NwdPath = Job == null ? null : Job.NwdPath
            };
        }

        /// <summary>Which of the three rerun cases this group turned out to be.</summary>
        public RerunDecision Decision { get; set; }

        /// <summary>Size read back off the disk, or minus one when the NWF is not there.</summary>
        public long NwfSize { get; set; }

        /// <summary>Size read back off the disk, or minus one when the NWD is not there.</summary>
        public long NwdSize { get; set; }
    }
}
