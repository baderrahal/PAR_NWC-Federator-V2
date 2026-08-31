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
            SourceFolder = string.Empty;
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

        /// <summary>
        /// The folder being scanned. The reports never go inside it, whether it was
        /// picked or defaulted to, because the Clash step picks an XML at run time and
        /// a report written there is a file a later run can be pointed at as its own
        /// input.
        /// </summary>
        public string SourceFolder { get; set; }

        /// <summary>How a discipline is read off a source file name.</summary>
        public ContainerNameSettings Names { get; set; }

        /// <summary>The folder for this run, given where the NWF files are going.</summary>
        public string FolderFor(string nwfFolder)
        {
            return ChooseFor(nwfFolder).Folder;
        }

        /// <summary>
        /// The folder and, when a picked one had to be refused for being inside the folder
        /// being scanned, the reason why.
        /// </summary>
        public ReportFolderChoice ChooseFor(string nwfFolder)
        {
            return ReportPaths.Choose(ExcelFolder, nwfFolder, SourceFolder);
        }
    }
}
