using System;
using System.Collections.Generic;
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
    /// The number a picture is written under here is ClashReport.NextPictureOf's. Since F149
    /// every picture is asked for once every test has run, in the order PictureWalk walks the
    /// report, so that number is already the one the Navisworks export gives it, and
    /// ImageRenumbering in Federator.Core, which renamed every picture once after the run
    /// while the pictures were taken in the order the tests ran, finds each already right.
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
        private readonly HideOtherAcrossTheRun hideOther;
        private readonly ImagesSeconds seconds;
        private readonly ImageCodecInfo jpeg;
        private readonly PictureNameWatch names = new PictureNameWatch();
        private readonly GdiSplit gdiSplit = new GdiSplit();

        /// <summary>
        /// The line saying where the GDI objects went since the last one, F161, or null where no
        /// picture was taken since. The runner writes it under each MEMORY line.
        /// </summary>
        public string TakeTheGdiSplitLine()
        {
            return gdiSplit.TakeLine();
        }

        /// <summary>
        /// The guard is handed in so it lives for the whole run rather than for one group,
        /// FR-076, the way ClashRunner takes the tests' guard. Built new per group it never
        /// fired across groups, and read only after the group's clash step it let the rest
        /// of the group render once it had. Null switches it off.
        /// The user's Clash Detective Hide Other is the run's too, F150, kept by the engine for
        /// the run and handed in the same way, null giving this group one of its own.
        /// </summary>
        public ClashImages(RunLog log, ImageOptions options, RepeatedFailureGuard guard, HideOtherAcrossTheRun hideOther)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
            this.options = options ?? new ImageOptions();
            this.guard = guard;
            this.hideOther = hideOther ?? new HideOtherAcrossTheRun();
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

        /// <summary>
        /// The seconds from which one render is named on a CALL line, F143, the runner's
        /// ProgressSettings.LongCallSeconds handed down. The default where none is handed.
        /// </summary>
        public double LongCallSeconds { get; set; } = ProgressSettings.DefaultLongCallSeconds;

        /// <summary>
        /// Whether each test's pictures are taken with only its models shown, F145, the setting
        /// on ImageOptions, and only where pictures are written at all.
        /// </summary>
        public bool OnlyTheTestsModels
        {
            get { return options.Write && options.OnlyTheTestsModels; }
        }

        /// <summary>How many pictures between two MEMORY and RATE lines, F157, the setting on ImageOptions.</summary>
        public int LogEveryPictures
        {
            get { return options.LogEveryPictures; }
        }

        /// <summary>The size past which the texture cache is cleared between two tests' pictures, F147, the setting on ImageOptions.</summary>
        public long TextureCacheLimitBytes
        {
            get { return options.TextureCacheLimitBytes; }
        }

        /// <summary>The least seconds between two measurements of the texture cache, F147, the setting on ImageOptions.</summary>
        public int TextureCacheCheckEverySeconds
        {
            get { return options.TextureCacheCheckEverySeconds; }
        }

        /// <summary>
        /// Whether the other items are left as Clash Detective draws them in the pictures, F150, the
        /// setting on ImageOptions. Off, PicturesHideOther sets Hide Other on for the picture pass.
        /// </summary>
        public bool GhostTheOtherItems
        {
            get { return options.GhostTheOtherItems; }
        }

        /// <summary>The user's Hide Other as the run's first group read it, and what the run owes, F150.</summary>
        public HideOtherAcrossTheRun HideOther
        {
            get { return hideOther; }
        }

        /// <summary>
        /// Whether a picture of this row is written now, with a workbook to sit beside, by the
        /// options' rule, ImageOptions.TakesAPictureOf, which counts a row passed over on its
        /// status or the cap. Write reads it, and the picture pass reads it before it resolves a
        /// row's result, F149, so a row that takes no picture costs no walk to its result.
        /// </summary>
        public bool Wanted(ClashReport report, TestReport test, ClashRow row, string workbookPath)
        {
            return !backstop.Fired && !string.IsNullOrEmpty(workbookPath)
                && options.TakesAPictureOf(report, test, row, guard);
        }

        /// <summary>
        /// F172. Whether the GDI backstop has fired: no picture is made after it, the run goes on, and the
        /// picture pass stops asking for rows. Not the guard's stop, which ends the run.
        /// </summary>
        public bool GdiBackstopFired
        {
            get { return backstop.Fired; }
        }

        private readonly GdiBackstop backstop = new GdiBackstop();

        private int picturesMade;

        /// <summary>How many of this test's clashes have a status that gets a picture.</summary>
        public int RowsThatWantAPicture(TestReport test)
        {
            int count = 0;

            foreach (ClashRow row in test.Rows)
            {
                if (options.Wants(row.Status))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>The most pictures this test may have, the fixed rule of F172 and the box's cap, the least of the two.</summary>
        public int PictureLimitOf(TestReport test)
        {
            return PictureLimits.LimitFor(test.Rows.Count, options.CapPerTest);
        }

        /// <summary>The clashes of the whole report with a status that gets a picture and no picture, said once the pass ends.</summary>
        public int RowsLeftWithoutAPicture(ClashReport report)
        {
            int count = 0;

            foreach (TestReport test in report.Tests)
            {
                foreach (ClashRow row in test.Rows)
                {
                    if (!row.HasImage && options.Wants(row.Status))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

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
                // FR-076. The guard is read in Wanted, where a picture is written, so nothing is
                // rendered, counted or logged once it has fired. Since F149 every picture is taken
                // in the picture pass, once every test of the group has run, so the run stops when
                // that pass ends, and the pass asks for no picture after the guard has fired.
                if (clashTests == null || result == null || !Wanted(report, test, row, workbookPath))
                {
                    return false;
                }

                // F172, Bader's answer to Q171. The GDI objects of the process are read before every
                // picture. Past the limit no more pictures are made, the NOTICE is said once and the run
                // goes on, so it never reaches the 10000 a process may hold and crashes on.
                if (backstop.Check(ProcessMemory.GdiObjectsNow(), picturesMade + 1))
                {
                    log.Line("IMAGES   " + backstop.Notice());
                    return false;
                }

                PictureNumber number = report.NextPictureOf(test, row);
                string path = ImageNaming.PathFor(workbookPath, number.TestIndex, number.ClashIndex);

                // F157. Two clashes that would write one file are said before the second is written,
                // by name, so a run that stops can be read for whether one picture replaced another.
                string twice = names.WouldReplace(path, test.Name, Name(row));

                if (twice != null)
                {
                    log.Line("IMAGE    " + twice);
                }

                try
                {
                    string folder = Path.GetDirectoryName(path);

                    if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    Render(clashTests, result, path, test.Name + " " + row.Name);

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
                        Missed(report, test, row, visit.Elapsed, ImageFailure.BecauseNoFileArrived(path), path);
                        return false;
                    }

                    names.Wrote(path, Name(row));
                    picturesMade++;
                    report.Images.Wrote(visit.Elapsed, length);
                    number.PointTheRow(workbookPath);

                    if (guard != null)
                    {
                        guard.RecordSuccess();
                    }

                    return true;
                }
                catch (Exception error)
                {
                    Missed(report, test, row, visit.Elapsed, ImageFailure.BecauseItThrewWriting(error, path), path);
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
        private void Render(DocumentClashTests clashTests, IClashResult result, string path, string on)
        {
            Bitmap bitmap;
            long gdiBefore = ProcessMemory.GdiObjectsNow();

            using (seconds.In(ImagesPart.Rendering))
            {
                // F143. The call alone is timed, and named on a CALL line with its .tsv row where
                // it took the setting's seconds or more, in the finally, so a call that throws
                // after a long wait is named too.
                double from = log.ElapsedSeconds;

                try
                {
                    bitmap = clashTests.TestsImageForResult(result, Style, options.Width, options.Height);
                }
                finally
                {
                    LongCall.Say(log, "TestsImageForResult", on, log.ElapsedSeconds - from, LongCallSeconds);
                }
            }

            long gdiAfterRender = ProcessMemory.GdiObjectsNow();

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

            gdiSplit.Add(gdiBefore, gdiAfterRender, ProcessMemory.GdiObjectsNow());
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
            ClashReport report, TestReport test, ClashRow row, double tookSeconds, ImageFailure why, string path)
        {
            report.ReleaseImageIndex(test);
            Failed(report, test, row, tookSeconds, why, path);
        }

        /// <summary>
        /// The pictures one fault left the picture pass unable to ask for, F149: the rows from one
        /// on, whose test was not found again once every test had run, whose stretch a throw ended,
        /// or one clash not found again in its test. Counted by Core's rule,
        /// ImageOptions.CountTheRowsLeft, as the writer would have counted them: a row of a status
        /// not asked for passed over on its status, as many as the room under the cap counted as
        /// failed and the rest passed over on the cap, nothing once the guard has fired, and the
        /// guard told ONCE for the fault, the second reading's blocking point, where told once a
        /// row one throw fired the fifty in a row guard with no render tried. Each row counted as
        /// failed is named on a line, the first with the fault's own words and the rest with the
        /// words for a row it left, so an empty Image cell never reads as a picture nobody asked for.
        /// </summary>
        public RowsLeftCount NotReached(
            ClashReport report, TestReport test, IList<ClashRow> rows, int from, string workbookPath, ImageFailure first, ImageFailure rest)
        {
            if (first == null || rest == null)
            {
                throw new ArgumentNullException(first == null ? "first" : "rest");
            }

            // With no workbook to sit beside no picture is written, as Wanted says, so nothing is counted.
            ClashReport countOn = string.IsNullOrEmpty(workbookPath) ? null : report;
            RowsLeftCount counted = options.CountTheRowsLeft(countOn, test, rows, from, first.Reason, guard);
            ClashRow firstRow = rows != null && from >= 0 && from < rows.Count ? rows[from] : null;

            foreach (ClashRow row in counted.Failed)
            {
                ImageFailure why = ReferenceEquals(row, firstRow) ? first : rest;
                SayTheFailure(test, row, why, null);
            }

            if (counted.Failed.Count > 0)
            {
                // Once for the fault and not once a row, so a stretch of 5000 rows left is one line.
                SayTheMemory("on an image failure, " + counted.Failed.Count + " rows counted as failed from " + (firstRow == null ? "the rows left" : Name(firstRow)));
            }

            return counted;
        }

        private void Failed(ClashReport report, TestReport test, ClashRow row, double tookSeconds, ImageFailure why, string path)
        {
            report.Images.RenderFailed(row.Name, tookSeconds);
            SayTheFailure(test, row, why, path);
            SayTheMemory("on an image failure, " + Name(row));

            if (guard != null)
            {
                guard.RecordFailure(why.Reason);
            }
        }

        /// <summary>
        /// One failed picture, F157: a stamped line with the clash, its test and the reason, and under
        /// it the full path and the whole exception with its call trace. The line was an unstamped
        /// continuation line before, which is how a run that died had two failures and no time.
        /// </summary>
        private void SayTheFailure(TestReport test, ClashRow row, ImageFailure why, string path)
        {
            log.Line("IMAGE    failed for " + Name(row) + " in "
                + (test == null || string.IsNullOrEmpty(test.Name) ? "an unnamed test" : test.Name)
                + ". " + why.Line());

            if (!string.IsNullOrEmpty(path))
            {
                log.Detail("file     " + path);
            }

            if (!string.IsNullOrEmpty(why.Full))
            {
                foreach (string line in why.Full.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                {
                    log.Detail("         " + line);
                }
            }
        }

        /// <summary>A MEMORY line with the GDI and USER counts, F157, on its own stamped line.</summary>
        private void SayTheMemory(string when)
        {
            log.Line(ProcessMemory.PicturesLine(when, ProcessMemory.Read()));
        }

        private static string Name(ClashRow row)
        {
            return string.IsNullOrEmpty(row.Name) ? "an unnamed clash" : row.Name;
        }
    }
}
