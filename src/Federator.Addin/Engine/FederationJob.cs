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
        /// <summary>
        /// The workbook can carry a different name from the NWF, because the Outputs step
        /// holds one naming pattern for each of the three. The discipline count is what
        /// the scan found in this group, or null for the open file, where nothing was
        /// scanned and the count is UNKNOWN.
        /// </summary>
        public FederationJob(
            string building,
            string outputName,
            string nwfPath,
            string nwdPath,
            IList<string> files,
            string workbookName,
            int? disciplineCount,
            IList<string> disciplines = null)
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
            ClashReportName = WorkbookName;
            DisciplineCount = disciplineCount;
            Disciplines = disciplines ?? new List<string>();
        }

        public string Building { get; private set; }

        public string OutputName { get; private set; }

        public string NwfPath { get; private set; }

        public string NwdPath { get; private set; }

        /// <summary>Full paths, appended in this order.</summary>
        public IList<string> Files { get; private set; }

        /// <summary>What the workbook is called. The NWF name unless a pattern differs.</summary>
        public string WorkbookName { get; private set; }

        /// <summary>
        /// The plain name of the HTML and of its _files folder of pictures in the Clash report folder,
        /// F177, Bader's order of 14 Oct 2026. It is never dated: a file there under this name is
        /// overwritten. It is the name the group was given and NameTheReports never changes it.
        /// </summary>
        public string ClashReportName { get; private set; }

        /// <summary>
        /// The name the files of this group in the Log folder are written under, set once at the top of the
        /// group by the engine, F158, where a file already in the Log folder would otherwise be overwritten.
        /// The Excel, the Internal workbook and the XML read it. The HTML and the pictures folder do not,
        /// F177, they keep ClashReportName, and the links are made from each picture's own path.
        /// </summary>
        internal void NameTheReports(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                WorkbookName = name;
            }
        }

        /// <summary>
        /// How many disciplines the scan found in this group, or null where nothing was
        /// scanned. Fewer than two and the clash step creates every test and runs none,
        /// by BuildingGroup.CannotClashWith. D5.
        /// </summary>
        public int? DisciplineCount { get; private set; }

        /// <summary>
        /// The discipline codes in this group, which F52 makes one viewpoint folder from
        /// each of. Empty for the open file, where nothing was scanned, and empty is a real
        /// answer there rather than a gap.
        /// </summary>
        public IList<string> Disciplines { get; private set; }

        /// <summary>
        /// True where the scan found fewer than two disciplines. The open file is never
        /// judged here, because its count is UNKNOWN and a guess would stop real tests.
        /// </summary>
        public bool IsSingleDiscipline
        {
            get
            {
                return DisciplineCount.HasValue
                    && Federator.Core.Grouping.BuildingGroup.CannotClashWith(DisciplineCount.Value);
            }
        }
    }
}
