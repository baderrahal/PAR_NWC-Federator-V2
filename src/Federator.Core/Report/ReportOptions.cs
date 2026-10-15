using System;
using Federator.Core.Clash;
using Federator.Core.Coverage;
using Federator.Core.Generic;
using Federator.Core.Health;
using Federator.Core.Naming;
using Federator.Core.Sets;
using Federator.Core.Units;
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
            SourceFolder = string.Empty;
            ApplyFileSettings = false;
            RefreshOnOpen = true;
            RunTests = true;
            RebuildExisting = false;
            Tolerance = ToleranceChoice.Default();
            MarkByDesign = false;
            RebuildDriftedSets = SetRebuildSettings.DefaultRebuildDriftedSets;
            MarkPenetrations = false;
            Penetrations = new PenetrationSettings();
            Sizes = new SizeSettings();
            StopAfterFailures = RepeatedFailureGuard.DefaultThreshold;
            FarModelMillimetres = AlignmentCheck.DefaultFarModelMillimetres;

            MakeViewpoints = ViewpointRequest.DefaultMakeViewpoints;

            // Fixed on, and the window no longer sets it. A weekly run wants the page
            // every time, so it is not a decision any more. F34.
            WriteHtml = true;
            PlacementToleranceMillimetres = PlacementCheck.DefaultToleranceMillimetres;
            LogoPath = string.Empty;
            Images = new ImageOptions();
            Names = new ContainerNameSettings();
            GenericModels = new GenericModelsSettings();
            Coverage = new CoverageSettings();
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
        /// F201. A model whose box centre moved more than this many millimetres between the append and the NWD is
        /// named on its PLACE line. The units of the models are never set since F201, the setting and the Model units
        /// box having moved every model, and the report is in metres whatever the document is in, ReportUnits.
        /// </summary>
        public double PlacementToleranceMillimetres { get; set; }

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
        /// The folder being scanned. The reports never go inside it, whether it was
        /// picked or defaulted to, because the Clash step picks an XML at run time and
        /// a report written there is a file a later run can be pointed at as its own
        /// input.
        /// </summary>
        public string SourceFolder { get; set; }

        /// <summary>
        /// Put the file settings onto tests already in the document. Off by default,
        /// because it changes WHICH CLASHES a saved test finds the next time it runs. It
        /// resets nothing by itself, measured 5y.
        ///
        /// IT KEEPS EXACTLY THE MEANING IT HAD, F76, which is whether the FILE'S settings
        /// reach a saved test. A tolerance chosen on the Clash step wins over it, because
        /// a value a person typed beats a value read out of a file, and the two answer
        /// different questions.
        /// </summary>
        public bool ApplyFileSettings { get; set; }

        /// <summary>
        /// F187, task 7. A picked NWF is refreshed before its tests run again: backed up, its linked files compared
        /// with the last manifest, the ones that do not resolve named and the run stopped, and Document.UpdateFiles asked
        /// once. On by default, since this is the weekly run of the NWF page.
        /// </summary>
        public bool RefreshOnOpen { get; set; }

        /// <summary>
        /// F188. Run the tests of a picked NWF again where they sit. Off, the run is the refresh alone: the backup,
        /// the linked files compared and brought up to date and the clash results read, and the NWF and NWD are not
        /// written. The NWF page's Run tests tick box. On by default.
        /// </summary>
        public bool RunTests { get; set; }

        /// <summary>
        /// F188. A group whose NWF already exists is built again, into a new dated folder beside it and never over
        /// the NWF or NWD that is there. The NWC page's Rebuild tick box, confirmed with the clash history named.
        /// Off by default.
        /// </summary>
        public bool RebuildExisting { get; set; }

        /// <summary>
        /// The tolerance chosen on the Clash step, F76. Never null: the default is 25 mm,
        /// ToleranceChoice.Default, the drop down being the only source since F155. The
        /// value is set on every clash test in the run, created fresh or
        /// already saved in the NWF, and it beats both the built in matrix and the document.
        /// </summary>
        public ToleranceChoice Tolerance { get; set; }

        /// <summary>
        /// Move a small service through a wall, a floor, a roof or a structural foundation to
        /// Reviewed. F72 and Q63, and
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
        /// It reads the by design pairs of the built in matrix pack, F154, and where no pack
        /// can be read it moves nothing and says so, rather than inventing a list.
        /// </summary>
        public bool MarkByDesign { get; set; }

        /// <summary>
        /// Whether a set already in the NWF whose question no longer matches the picked
        /// file is rebuilt from it, Q72. Off by default, because it changes the NWF and
        /// the NWF is the record. It destroys nothing: 5v measured that the clash tests
        /// pointing at a replaced set keep their results and their statuses.
        /// </summary>
        public bool RebuildDriftedSets { get; set; }

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

        /// <summary>
        /// How far a model may sit from its group's reference model, in millimetres and in
        /// a straight line, before it is not on the same shared coordinates, Q98 B2 and Q99.
        /// A metre by default, the number Bader gave. A setting and not a constant, because
        /// it decides which groups clash.
        ///
        /// A value that is not a number is refused, because nothing is more than it and the
        /// rule would go off without a word, and so is one below zero, which would call a
        /// model sitting exactly on its reference far.
        /// </summary>
        public double FarModelMillimetres
        {
            get { return farModelMillimetres; }

            set
            {
                if (double.IsNaN(value) || value < 0.0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value", "A far model distance is a length of zero or more, in millimetres.");
                }

                farModelMillimetres = value;
            }
        }

        private double farModelMillimetres;



        /// <summary>
        /// Whether a group makes its saved views, F136, the box on the Clash step. ON by
        /// default since F114's add-in pass, Bader's answer B to Q131, off until F114 merged.
        /// Off, no group asks for one and none can fail it.
        /// </summary>
        public bool MakeViewpoints { get; set; }

        /// <summary>How a discipline is read off a source file name.</summary>
        public ContainerNameSettings Names { get; set; }

        /// <summary>
        /// The Generic Models sets, count, block and sheet of every group, F128 and FR-177, and the suffix that
        /// names the internal workbook the sheet goes in, Q146 answered B. Always on, by Bader's request, and
        /// its three values and the workbook suffix are read off Core here and never typed in the window.
        /// </summary>
        public GenericModelsSettings GenericModels { get; set; }

        /// <summary>
        /// The coverage of the picked tests of every group, F127: the Coverage sheet's name, the examples the
        /// COVERAGE block names for each reason and the FAILED lines RESULT lists. Read off Core here and never
        /// typed in the window.
        /// </summary>
        public CoverageSettings Coverage { get; set; }

        /// <summary>
        /// The Log folder for this run, given where the NWF and the NWD files are going, F158. It
        /// throws where the NWF folder is empty or the Log folder would be inside the folder
        /// being scanned, ReportPaths.LogFolder.
        /// </summary>
        public string LogFolderFor(string nwfFolder, string nwdFolder)
        {
            return ReportPaths.LogFolder(nwfFolder, nwdFolder, SourceFolder);
        }
    }
}
