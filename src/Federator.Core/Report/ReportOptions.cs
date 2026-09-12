using System;
using Federator.Core.Clash;
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
            ApplyFileSettings = false;
            CompactResolved = false;
            StopAfterFailures = RepeatedFailureGuard.DefaultThreshold;

            // Fixed on, and the window no longer sets them. A weekly run wants the page
            // and the units every time, so neither is a decision any more. F34.
            WriteHtml = true;
            SetDocumentUnits = true;
            UnitsName = DefaultUnits;
            LogoPath = string.Empty;
            Images = new ImageOptions();
            Names = new ContainerNameSettings();
        }

        /// <summary>Clash pictures. On by default, because the accepted report has them.</summary>
        public ImageOptions Images { get; set; }

        /// <summary>
        /// Write the report as HTML (Tabular), which is the format the client actually
        /// receives. On, because Clash Detective cannot export an xlsx at all, so the file
        /// they accepted is an HTML page opened in Excel.
        ///
        /// It is rendered by Autodesk's own clash_report_html_tabular.xsl, read from the
        /// install at run time. No copy of that file is in this repo and none is written.
        /// A missing stylesheet turns this one output off and nothing else.
        /// </summary>
        public bool WriteHtml { get; set; }

        /// <summary>
        /// Put the document into <see cref="UnitsName"/> before the clash step.
        ///
        /// On, because this is a Saudi project and every report the team sends is metric,
        /// and a run on a document in feet wrote a tolerance of 0.246ft and distances in
        /// feet, which nobody can use. It is a mutation of the document, so it is logged
        /// with what the document was and what it became.
        /// </summary>
        public bool SetDocumentUnits { get; set; }

        /// <summary>
        /// Which units, by the name on the Navisworks enum. Metres by default.
        /// </summary>
        public string UnitsName { get; set; }

        /// <summary>Metres, because that is what the client receives.</summary>
        public const string DefaultUnits = "Meters";

        /// <summary>
        /// The logo the HTML page shows, or empty for none, which is the default.
        ///
        /// Autodesk's own logo.jpg sits in the Images folder of the install and is theirs.
        /// No copy of it is in this repo or in the bundle. This box starts filled with that
        /// file, read off the machine that is running, and whatever it points at is copied
        /// beside the report so the page and its picture travel together. Clear it for no
        /// logo at all.
        /// </summary>
        public string LogoPath { get; set; }

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

        /// <summary>
        /// Put the file's settings onto tests already in the document. Off by default,
        /// because it RESETS their results.
        /// </summary>
        public bool ApplyFileSettings { get; set; }

        /// <summary>
        /// Remove Resolved clashes after the tests run. Off by default, because it
        /// destroys the record of what was resolved.
        /// </summary>
        public bool CompactResolved { get; set; }

        /// <summary>
        /// How many clash tests failing in a row for the same reason stop the whole run.
        /// Fifty by default, the number the guard carries, and a setting rather than a
        /// constant because it shapes the run. One guard is built from this for the whole
        /// run, never one per group, since every group of that nine hour run failed the
        /// same way. The images have their own count of the same shape on ImageOptions.
        /// </summary>
        public int StopAfterFailures
        {
            get { return stopAfterFailures; }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(
                        "value", "The guard needs at least one failure before it can stop a run.");
                }

                stopAfterFailures = value;
            }
        }

        private int stopAfterFailures;

        /// <summary>How a discipline is read off a source file name.</summary>
        public ContainerNameSettings Names { get; set; }

        /// <summary>The folder for this run, given where the NWF files are going.</summary>
        internal string FolderFor(string nwfFolder)
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
