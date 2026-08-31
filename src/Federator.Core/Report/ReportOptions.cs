using Federator.Core.Naming;

namespace Federator.Core.Report
{
    /// <summary>
    /// What the run does about reports. One object rather than four more arguments on the
    /// engine, so the next thing the reports need does not change the constructor again.
    /// </summary>
    public sealed class ReportOptions
    {
        public ReportOptions()
        {
            WriteWorkbook = true;
            WriteXml = false;
            ExcelFolder = string.Empty;
            Names = new ContainerNameSettings();
        }

        /// <summary>On. The workbook is the point of the run.</summary>
        public bool WriteWorkbook { get; set; }

        /// <summary>
        /// Off by default. Written from the same results the workbook is built from, never
        /// by reading the workbook and never read by it, so a fault in one cannot corrupt
        /// the other.
        /// </summary>
        public bool WriteXml { get; set; }

        /// <summary>
        /// Where the workbooks go. Empty means beside the NWF folder, in a subfolder named
        /// by <see cref="ReportPaths.DefaultSubfolder"/>.
        /// </summary>
        public string ExcelFolder { get; set; }

        /// <summary>How a discipline is read off a source file name.</summary>
        public ContainerNameSettings Names { get; set; }

        /// <summary>The folder for this run, given where the NWF files are going.</summary>
        public string FolderFor(string nwfFolder)
        {
            return ReportPaths.Folder(ExcelFolder, nwfFolder);
        }
    }
}
