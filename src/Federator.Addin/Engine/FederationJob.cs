using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;

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

        public GroupOutcome Result
        {
            get
            {
                if (!NwfOnDisk || !NwdOnDisk || AppendedCount == 0)
                {
                    return GroupOutcome.Failed;
                }

                // A group left alone because its file list changed is not a failure and is
                // not a clean run either. It is partial, and the log names what differs.
                if (Decision == Federator.Core.Rerun.RerunDecision.Changed)
                {
                    return GroupOutcome.Partial;
                }

                return FailedFiles.Count > 0 ? GroupOutcome.Partial : GroupOutcome.Done;
            }
        }

        /// <summary>Which of the three rerun cases this group turned out to be.</summary>
        public Federator.Core.Rerun.RerunDecision Decision { get; set; }

        /// <summary>Size read back off the disk, or minus one when the NWF is not there.</summary>
        public long NwfSize { get; set; }

        /// <summary>Size read back off the disk, or minus one when the NWD is not there.</summary>
        public long NwdSize { get; set; }
    }
}
