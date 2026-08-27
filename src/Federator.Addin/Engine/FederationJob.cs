using System;
using System.Collections.Generic;

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

    public enum JobResult
    {
        /// <summary>Every file appended and both outputs are on disk.</summary>
        Written,

        /// <summary>At least one file failed to append, but the outputs are on disk.</summary>
        Partial,

        /// <summary>Nothing usable came out of this group.</summary>
        Failed
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

        public JobResult Result
        {
            get
            {
                if (!NwfOnDisk || !NwdOnDisk || AppendedCount == 0)
                {
                    return JobResult.Failed;
                }

                return FailedFiles.Count > 0 ? JobResult.Partial : JobResult.Written;
            }
        }
    }
}
