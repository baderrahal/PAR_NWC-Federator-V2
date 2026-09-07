using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Report;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Renders one picture per clash and writes it where the client already expects it.
    ///
    /// How this is done, read off the installed DLLs on 2026-08-31 and recorded in
    /// docs\scan.md section 4k:
    ///
    ///   public System.Drawing.Bitmap DocumentClashTests.TestsImageForResult(
    ///       IClashResult result, ImageGenerationStyle style, int width, int height)
    ///
    /// That is the only method in either Autodesk.Navisworks.Api.dll or
    /// Autodesk.Navisworks.Clash.dll that takes a clash result and gives back an image, so
    /// it is the one used. It takes no camera, so the view it renders can only have come
    /// from the result itself. Whether it applies the clash's own viewpoint internally is
    /// UNKNOWN from the DLL, and the explicit route exists if it turns out not to:
    ///
    ///   public Viewpoint DocumentClashTests.TestsViewpointForResult(IClashResult result)
    ///   public void Document.CurrentViewpoint.CopyFrom(Viewpoint viewpoint)
    ///   public void View.CopyViewpointFrom(Viewpoint viewpoint, ViewChange change)
    ///   public Bitmap View.GenerateImage(ImageGenerationStyle, int, int, bool)
    ///
    /// Nothing in the Navisworks API writes a clash image to a file. The bitmap is written
    /// with System.Drawing, Image.Save(string, ImageFormat), which is why jpg is reachable
    /// at all.
    ///
    /// No clash is saved as a viewpoint in the NWF. Nothing here touches SavedViewpoints.
    ///
    /// The number a picture is written under here is the RUN order, because the report
    /// order is only known once every test has run. ImageRenumbering in Federator.Core
    /// renames every picture once after the run, so what the client receives is numbered
    /// the way the Navisworks export numbers it.
    /// </summary>
    public sealed class ClashImages
    {
        private readonly RunLog log;
        private readonly ImageOptions options;
        private readonly RepeatedFailureGuard guard;
        private readonly List<string> failures = new List<string>();

        public ClashImages(RunLog log, ImageOptions options)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
            this.options = options ?? new ImageOptions();

            this.guard = this.options.StopAfterFailures > 0
                ? new RepeatedFailureGuard(this.options.StopAfterFailures)
                : null;

            Style = ImageGenerationStyle.ScenePlusOverlay;
        }

        /// <summary>
        /// Which of the three the renderer is asked for. ScenePlusOverlay, because the
        /// overlay is where a clash highlight would live and the accepted report shows the
        /// two clashing items picked out. WHICH style Navisworks itself uses for its own
        /// report is UNKNOWN, so this is a setting and not a fact.
        /// </summary>
        public ImageGenerationStyle Style { get; set; }

        /// <summary>Fires once fifty renders in a row have failed the same way.</summary>
        public bool ShouldStopTheRun
        {
            get { return guard != null && guard.ShouldStopTheRun; }
        }

        /// <summary>
        /// Null until the guard has fired. Worded for images rather than borrowed from
        /// the clash step, because a line saying tests failed when what failed was
        /// pictures sends the next person looking in the wrong place.
        /// </summary>
        public string StopReason
        {
            get
            {
                if (guard == null || !guard.ShouldStopTheRun)
                {
                    return null;
                }

                return "the last " + guard.Consecutive
                    + " clash images all failed for the same reason, so the rest of the run "
                    + "was not attempted. Switch images off to run without them. "
                    + First();
            }
        }

        private string First()
        {
            return failures.Count == 0 ? string.Empty : failures[0];
        }

        /// <summary>
        /// One picture for one clash, or nothing at all, which is an ordinary answer
        /// rather than a fault.
        ///
        /// Returns true only when a file was actually written and read back off the disk.
        /// A failure leaves the row's cell empty, is counted, and never stops the run by
        /// itself.
        /// </summary>
        public bool Write(
            DocumentClashTests clashTests,
            IClashResult result,
            ClashReport report,
            TestReport test,
            ClashRow row,
            string workbookPath)
        {
            if (report == null || test == null || row == null)
            {
                return false;
            }

            if (!options.Write || clashTests == null || result == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(workbookPath))
            {
                return false;
            }

            if (!options.Wants(row.Status))
            {
                report.Images.WrongStatus();
                return false;
            }

            if (!options.RoomFor(test.ImageCount))
            {
                report.Images.CappedOne();
                return false;
            }

            int testIndex = report.ImageIndexFor(test);
            int clashIndex = test.ImageCount + 1;
            string path = ImageNaming.PathFor(workbookPath, testIndex, clashIndex);

            Stopwatch watch = Stopwatch.StartNew();

            try
            {
                string folder = Path.GetDirectoryName(path);

                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                Render(clashTests, result, path);
                watch.Stop();

                // The size is read back off the disk. A render that returned without
                // writing anything must not be counted as a picture.
                FileInfo written = new FileInfo(path);

                if (!written.Exists || written.Length == 0)
                {
                    Missed(report, test, row, watch.Elapsed.TotalSeconds,
                        "the render finished but no file arrived at " + path);
                    return false;
                }

                report.Images.Wrote(watch.Elapsed.TotalSeconds, written.Length);

                row.ImageFile = ImageNaming.FileNameFor(testIndex, clashIndex);
                row.ImageLink = ImageNaming.LinkFor(workbookPath, testIndex, clashIndex);
                row.ImagePath = path;

                if (guard != null)
                {
                    guard.RecordSuccess();
                }

                return true;
            }
            catch (Exception error)
            {
                watch.Stop();
                Missed(report, test, row, watch.Elapsed.TotalSeconds,
                    error.GetType().Name + ": " + error.Message);
                return false;
            }
        }

        /// <summary>
        /// The render itself, kept apart so what talks to Navisworks is three lines and
        /// everything around it is bookkeeping.
        ///
        /// The bitmap is disposed whatever happens. It is a native image handle and one
        /// per clash across 1830 tests is how a run runs a machine out of memory.
        /// </summary>
        private void Render(DocumentClashTests clashTests, IClashResult result, string path)
        {
            using (Bitmap bitmap = clashTests.TestsImageForResult(
                result, Style, options.Width, options.Height))
            {
                if (bitmap == null)
                {
                    throw new InvalidOperationException(
                        "TestsImageForResult returned nothing for this clash.");
                }

                bitmap.Save(path, ImageFormat.Jpeg);
            }
        }

        /// <summary>
        /// One picture that did not happen. Logged by name, counted, and the cell left
        /// empty. The index is handed back where this was the test's first attempt, so
        /// the numbering carries no gap.
        /// </summary>
        private void Missed(
            ClashReport report, TestReport test, ClashRow row, double seconds, string why)
        {
            report.Images.RenderFailed(row.Name, seconds);
            report.ReleaseImageIndex(test);

            log.Detail("IMAGE    failed for " + Name(row) + ". " + why);

            if (failures.Count == 0)
            {
                failures.Add(why);
            }

            if (guard != null)
            {
                guard.RecordFailure(why);
            }
        }

        private static string Name(ClashRow row)
        {
            return string.IsNullOrEmpty(row.Name) ? "an unnamed clash" : row.Name;
        }
    }
}
