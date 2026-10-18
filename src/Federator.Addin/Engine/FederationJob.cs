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
        /// scanned and the count is UNKNOWN, until F219 reads a picked NWF's at its open.
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

        /// <summary>
        /// The package this group belongs to, its path from the parent folder such as C06\Infrastructure, F208. Empty for
        /// a run of one folder, the open file and a picked NWF, where there is no package.
        /// </summary>
        public string Package { get; private set; } = string.Empty;

        /// <summary>
        /// The Log folder of the group's package, under its report folder, F208. Empty where the engine's own Log folder,
        /// worked out once for the run, is the group's.
        /// </summary>
        public string LogFolder { get; private set; } = string.Empty;

        /// <summary>The group named with its package, the words every GROUP line and RESULT carry, Core's PackagePlan.LabelFor.</summary>
        public string Label
        {
            get { return Federator.Core.Communities.PackagePlan.LabelFor(Package, Building); }
        }

        /// <summary>Puts the group in its package with the package's Log folder, F208, and hands the job back.</summary>
        public FederationJob InPackage(string package, string logFolder)
        {
            Package = package ?? string.Empty;
            LogFolder = logFolder ?? string.Empty;
            return this;
        }

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
        /// How many disciplines the scan found in this group, or for a picked NWF its models'
        /// names gave at its open, F219, or null where neither did. Fewer than two and the
        /// clash step runs no test, by BuildingGroup.CannotClashWith. D5.
        /// </summary>
        public int? DisciplineCount { get; private set; }

        /// <summary>
        /// The discipline codes in this group, from the scan or, for a picked NWF, from its
        /// models' names at its open, F219. Empty for the document already open, where
        /// nothing was read, and empty is a real answer there rather than a gap.
        /// </summary>
        public IList<string> Disciplines { get; private set; }

        /// <summary>
        /// F219, Bader's answer to Q203. The disciplines of a picked NWF, read off its models' names once it opened, where
        /// no scan gave them. Set once and never over a scan's count.
        /// </summary>
        public void ReadTheDisciplinesAtTheOpen(IList<string> codes)
        {
            if (DisciplineCount.HasValue || codes == null)
            {
                return;
            }

            DisciplineCount = codes.Count;
            Disciplines = codes;
        }

        /// <summary>
        /// True where the scan, or since F219 the models of a picked NWF read at its open, found fewer than two
        /// disciplines. The document already open is never judged here, because its count is UNKNOWN and a guess would
        /// stop real tests.
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
