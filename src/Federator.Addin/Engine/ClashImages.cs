using System;
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
    /// docs\history\scan.md section 4k:
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
    /// with System.Drawing, Image.Save, which is why jpg is reachable at all.
    ///
    /// NOTHING HERE TOUCHES SavedViewpoints, and that half is still true. The other half
    /// of this sentence said no clash is ever saved as a viewpoint, and F85 reversed it, then
    /// F114 made it one view per clash test of its open clashes beside a picture per clash,
    /// planned by TestViewPlan and written by ViewpointBuilder. Neither is this file.
    ///
    /// The number a picture is written under here is the RUN order, because the report
    /// order is only known once every test has run. ImageRenumbering in Federator.Core
    /// renames every picture once after the run, so what the client receives is numbered
    /// the way the Navisworks export numbers it.
    ///
    /// WHERE THE SECONDS GO IS MEASURED CALL BY CALL, FR-077. One watch sat around the
    /// render and the save, so of the 0.113 s a picture cost in set 03, which was Navisworks
    /// rendering it and which was the JPEG going to the disk was UNKNOWN. The rule is
    /// Federator.Core.Report.ImagesSeconds on the log's clock, one line per group.
    /// </summary>
    public sealed class ClashImages
    {
        private readonly RunLog log;
        private readonly ImageOptions options;
        private readonly RepeatedFailureGuard guard;
        private readonly ImagesSeconds seconds;
        private readonly ImageCodecInfo jpeg;

        /// <summary>
        /// The guard is handed in so it lives for the whole run rather than for one group,
        /// FR-076, the way ClashRunner takes the tests' guard. Built new per group it never
        /// fired across groups, and read only after the group's clash step it let the rest
        /// of the group render once it had. Null switches it off.
        /// </summary>
        public ClashImages(RunLog log, ImageOptions options, RepeatedFailureGuard guard)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
            this.options = options ?? new ImageOptions();
            this.guard = guard;
            this.seconds = new ImagesSeconds(() => log.ElapsedSeconds);
            this.jpeg = JpegEncoder();

            Style = ImageGenerationStyle.ScenePlusOverlay;
        }

        /// <summary>
        /// Which of the three the renderer is asked for. ScenePlusOverlay, because the
        /// overlay is where a clash highlight would live and the accepted report shows the
        /// two clashing items picked out. WHICH style Navisworks itself uses for its own
        /// report is UNKNOWN, so this is a setting and not a fact.
        /// </summary>
        public ImageGenerationStyle Style { get; set; }

        /// <summary>Where this group's IMAGES seconds went, call by call, FR-077.</summary>
        public ImagesSeconds Seconds
        {
            get { return seconds; }
        }

        /// <summary>
        /// Fires once StopAfterFailures renders in a row have failed the same way, which
        /// is fifty by default and is a setting on ImageOptions.
        /// </summary>
        public bool ShouldStopTheRun
        {
            get { return guard != null && guard.ShouldStopTheRun; }
        }

        /// <summary>
        /// Null until the guard has fired. Worded for images rather than borrowed from
        /// the clash step, because a line saying tests failed when what failed was
        /// pictures sends the next person looking in the wrong place. The words are
        /// Federator.Core.Report.ImageFailure's.
        /// </summary>
        public string StopReason
        {
            get { return ShouldStopTheRun ? ImageFailure.RunStopped(guard.Consecutive, guard.FirstReason) : null; }
        }

        /// <summary>The same for the label, which carries no type name and no framework message.</summary>
        public string StopReasonInPlainWords
        {
            get { return ShouldStopTheRun ? ImageFailure.RunStoppedInPlainWords(guard.Consecutive) : null; }
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
            using (ImagesVisit visit = seconds.Visit())
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

                // FR-076. Read here, where a picture is written, so the run stops at the
                // fiftieth failure in a row and not at the end of the group. Nothing is
                // rendered, counted or logged once it has fired, and the runner stops the
                // run after the test it fell in.
                if (ShouldStopTheRun)
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

                try
                {
                    string folder = Path.GetDirectoryName(path);

                    if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    Render(clashTests, result, path);

                    // The size is read back off the disk. A render that returned without
                    // writing anything must not be counted as a picture.
                    bool arrived;
                    long length;

                    using (seconds.In(ImagesPart.ReadingBack))
                    {
                        FileInfo written = new FileInfo(path);
                        arrived = written.Exists && written.Length > 0;
                        length = arrived ? written.Length : 0;
                    }

                    if (!arrived)
                    {
                        Missed(report, test, row, visit.Elapsed, ImageFailure.BecauseNoFileArrived(path));
                        return false;
                    }

                    report.Images.Wrote(visit.Elapsed, length);

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
                    Missed(report, test, row, visit.Elapsed, ImageFailure.BecauseItThrew(error));
                    return false;
                }
            }
        }

        /// <summary>
        /// The render itself, kept apart so what talks to Navisworks is one line and
        /// everything around it is bookkeeping. The render and the save are timed apart.
        ///
        /// The bitmap is disposed whatever happens, as soon as the save returns. It is a
        /// native image handle and one per clash across 1830 tests is how a run runs a
        /// machine out of memory. No copy of it is made on the way to the disk.
        /// </summary>
        private void Render(DocumentClashTests clashTests, IClashResult result, string path)
        {
            Bitmap bitmap;

            using (seconds.In(ImagesPart.Rendering))
            {
                bitmap = clashTests.TestsImageForResult(result, Style, options.Width, options.Height);
            }

            using (bitmap)
            {
                if (bitmap == null)
                {
                    throw new InvalidOperationException(
                        "TestsImageForResult returned nothing for this clash.");
                }

                using (seconds.In(ImagesPart.Saving))
                {
                    Save(bitmap, path);
                }
            }
        }

        /// <summary>
        /// The JPEG written through the encoder found once per group, FR-077.
        /// Image.Save(string, ImageFormat) looks the encoder up on every call, walking the
        /// GDI+ encoder list, and then calls this same overload with no parameters, so the
        /// bytes on the disk are the same and the lookup is paid once rather than once per
        /// picture. The size, the format and the quality are untouched.
        /// </summary>
        private void Save(Bitmap bitmap, string path)
        {
            if (jpeg == null)
            {
                throw new InvalidOperationException(
                    "No JPEG encoder is installed on this machine, so no picture can be written.");
            }

            bitmap.Save(path, jpeg, null);
        }

        private static ImageCodecInfo JpegEncoder()
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>
        /// One picture that did not happen. Logged by name with the reason and its detail,
        /// counted, and the cell left empty. The index is handed back where this was the
        /// test's first attempt, so the numbering carries no gap. The guard reads the
        /// reason alone, so the same fault at fifty paths is one reason, FR-076.
        /// </summary>
        private void Missed(
            ClashReport report, TestReport test, ClashRow row, double tookSeconds, ImageFailure why)
        {
            report.Images.RenderFailed(row.Name, tookSeconds);
            report.ReleaseImageIndex(test);

            log.Detail("IMAGE    failed for " + Name(row) + ". " + why.Line());

            if (guard != null)
            {
                guard.RecordFailure(why.Reason);
            }
        }

        private static string Name(ClashRow row)
        {
            return string.IsNullOrEmpty(row.Name) ? "an unnamed clash" : row.Name;
        }
    }
}
