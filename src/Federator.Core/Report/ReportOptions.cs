using System;
using Federator.Core.Clash;
using Federator.Core.Naming;
using Federator.Core.Views;

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
            Tolerance = ToleranceChoice.FromTheFile();
            MarkByDesign = false;
            PriorityPath = string.Empty;
            ByDesignPath = string.Empty;
            CompactResolved = false;
            MarkPenetrations = false;
            Penetrations = new PenetrationSettings();
            Sizes = new SizeSettings();
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
        ///
        /// IT KEEPS EXACTLY THE MEANING IT HAD, F76, which is whether the FILE'S settings
        /// reach a saved test. A tolerance chosen on the Clash step wins over it, because
        /// a value a person typed beats a value read out of a file, and the two answer
        /// different questions.
        /// </summary>
        public bool ApplyFileSettings { get; set; }

        /// <summary>
        /// The clash priority CSV picked on the Clash step, F83, or empty. Empty means
        /// none and NOTHING CHANGES: no Priority column, no extra RESULT line, and the
        /// measured block order. The same shape LogoPath has, which is the other optional
        /// picked file, because one optional picker is enough of a pattern.
        /// </summary>
        public string PriorityPath { get; set; }

        /// <summary>
        /// The by design pairs CSV picked on the Clash step, F72b, or empty. Empty means
        /// none. It is read only when MarkByDesign is on, because a file picked with the
        /// box off would be read and then ignored, which reads as a file that did nothing.
        /// </summary>
        public string ByDesignPath { get; set; }

        /// <summary>
        /// The tolerance chosen on the Clash step, F76. Never null: the default is
        /// ToleranceChoice.FromTheFile, which is what this tool has always done. When a
        /// value is chosen it is set on every clash test in the run, created fresh or
        /// already saved in the NWF, and it beats both the XML and the document.
        /// </summary>
        public ToleranceChoice Tolerance { get; set; }

        /// <summary>
        /// Remove Resolved clashes after the tests run. Off by default, because it
        /// destroys the record of what was resolved.
        /// </summary>
        public bool CompactResolved { get; set; }

        /// <summary>
        /// Move a small service through a wall, a floor or a roof to Reviewed. F72, and
        /// the answer to Q33. OFF by default, because it writes into the NWF, which is the
        /// only record of what has been fixed, and a project has to say it wants this
        /// before a run starts changing statuses in it.
        ///
        /// Nothing is destroyed either way: a person moves a clash back in one click and
        /// only New and Active ever move, so a decision somebody made is never overwritten.
        /// That is why it is an ordinary box and not one of the two under Things that
        /// destroy data.
        /// </summary>
        public bool MarkPenetrations { get; set; }

        /// <summary>
        /// Move a clash between two sets named as a by design connection to Reviewed,
        /// F72b. OFF by default, for the same reason the penetration box is: it writes
        /// into the NWF, which is the only record of what has been fixed.
        ///
        /// It reads ByDesignPath, and with the box on and no file picked it moves nothing
        /// and says so, rather than inventing a list.
        /// </summary>
        public bool MarkByDesign { get; set; }

        /// <summary>
        /// Which categories are a service and which are a solid. F72. The SIZE is not here
        /// and is Federator.Core.Views.SizeSettings, which F53 reads too.
        /// </summary>
        public PenetrationSettings Penetrations { get; set; }

        /// <summary>
        /// The one threshold, read by F53 for the viewpoints and by F72 for the
        /// penetrations, in opposite directions. The comment at
        /// SizeSettings.DefaultThresholdMillimetres says which way round each reads it.
        /// </summary>
        public SizeSettings Sizes { get; set; }

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
