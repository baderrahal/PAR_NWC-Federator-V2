using System;
using System.Globalization;
using Federator.Core.Clash;

namespace Federator.Core.Report
{
    /// <summary>
    /// F172 and F175, Bader's answers to Q171. How many pictures a clash test gets, and where the pictures
    /// stop so the run cannot crash on the GDI limit. Two rules and nothing else:
    ///
    /// ONE RULE PER TEST BY ITS PRIORITY, F190, Bader's order of 15 Oct 2026. Every test, whatever its
    /// clash count, gets pictures for at most its first clashes in the order Navisworks lists them: priority A
    /// the first 150, priority B the first 100 and priority C the first 50. A test with fewer clashes than its
    /// cap gets a picture for each. A test the matrix does not name counts as C, F168, and so does a test whose
    /// priority is not known. It is the test that counts, not the run. The clashes stay in the HTML, the Excel
    /// and the viewpoints, only their Image cell is empty, and every clash keeps its viewpoint.
    ///
    /// ONE BACKSTOP FOR THE PROCESS. Every picture costs the Navisworks process one GDI object and a
    /// process may hold 10000. Past 9000 no more pictures are made, a NOTICE says at which picture and
    /// how many objects, and the run goes on with the rest of the group. The
    /// run 20261014-105308 crashed Navisworks at picture 8907 of 10170 at 9999 objects, in lcodwin.dll.
    ///
    /// The five numbers are settings on ImageOptions since F227, Bader's answer to Q210, and the consts here are
    /// their defaults.
    /// </summary>
    public static class PictureLimits
    {
        /// <summary>The most pictures a test of priority A gets by default, its first clashes in report order.</summary>
        public const int DefaultPicturesForA = 150;

        /// <summary>The most pictures a test of priority B gets by default.</summary>
        public const int DefaultPicturesForB = 100;

        /// <summary>The most pictures a test of priority C gets by default, and of one with no priority.</summary>
        public const int DefaultPicturesForC = 50;

        /// <summary>The GDI objects of the Navisworks process past which no picture is made, by default.</summary>
        public const long DefaultGdiStop = 9000;

        /// <summary>
        /// The GDI objects a Windows process may hold. A stop at it or past it never fires before the crash, which
        /// the run 20261014-105308 met at 9999 objects.
        /// </summary>
        public const long ProcessGdiLimit = 10000;

        /// <summary>
        /// F182, Bader's answer C to Q175. The private bytes of the Navisworks process past which no picture is made by
        /// default, 12 GB of 1024 cubed. The first of this and the GDI stop to fire ends the pictures.
        /// </summary>
        public const long DefaultMemoryStopBytes = 12L * 1024 * 1024 * 1024;

        /// <summary>
        /// The most pictures a test of that priority gets under the run's settings. None is C, because a test no
        /// matrix names is C by F168 and a priority not known is the least a test may get, never the most.
        /// </summary>
        public static int PicturesFor(ClashPriority priority, ImageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            switch (priority)
            {
                case ClashPriority.A: return options.PicturesForA;
                case ClashPriority.B: return options.PicturesForB;
                default: return options.PicturesForC;
            }
        }

        /// <summary>
        /// The most pictures a test may have, F190. The cap of its priority, whatever its clash count, and
        /// the least of it and the box's number, the run's CapPerTest, where the box is not zero. A test with
        /// fewer clashes than the limit gets a picture for each, because the pictures stop at the end of its clashes.
        /// </summary>
        public static int LimitFor(ClashPriority priority, ImageOptions options)
        {
            int limit = PicturesFor(priority, options);

            if (options.CapPerTest > 0 && options.CapPerTest < limit)
            {
                limit = options.CapPerTest;
            }

            return limit;
        }

        /// <summary>
        /// The one line a test whose clashes exceed its cap gets in the log: its name, its priority letter,
        /// its clashes, the pictures made and the clashes whose picture was skipped.
        /// </summary>
        public static string TestLine(string testName, ClashPriority priority, int clashes, int made, int skipped)
        {
            return "IMAGES   " + testName + " has priority " + Letter(priority) + " and "
                + clashes.ToString(CultureInfo.InvariantCulture)
                + " clashes, more than its cap, so " + made.ToString(CultureInfo.InvariantCulture) + " pictures were made and "
                + skipped.ToString(CultureInfo.InvariantCulture) + " were skipped. The clashes stay in the "
                + "HTML, the Excel and the viewpoints, only their Image cell is empty";
        }

        /// <summary>
        /// The letter the cap was read by. None is said as C, which is the cap it got, so a line never
        /// names a priority that was not the one used.
        /// </summary>
        public static string Letter(ClashPriority priority)
        {
            return priority == ClashPriority.None ? "C" : Priorities.Words(priority);
        }

        /// <summary>The rule said in words for the window and the log: the three caps of the run's settings.</summary>
        public static string RuleInWords(ImageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            return "every test gets pictures for at most its first " + options.PicturesForA.ToString(CultureInfo.InvariantCulture)
                + " clashes at priority A, " + options.PicturesForB.ToString(CultureInfo.InvariantCulture)
                + " at priority B and " + options.PicturesForC.ToString(CultureInfo.InvariantCulture) + " at priority C";
        }

    }

    /// <summary>
    /// The backstop of F172. It is asked before every picture with the GDI objects the process holds
    /// now and fires once, at the first picture that would be made with more than the limit. It never
    /// stops anything but pictures.
    /// </summary>
    public sealed class GdiBackstop
    {
        private readonly long limit;

        /// <summary>The backstop at the run's ImageOptions.GdiStop, where a limit that cannot work is refused.</summary>
        public GdiBackstop(long limit)
        {
            this.limit = limit;
        }

        /// <summary>Whether it has fired, after which no picture is made.</summary>
        public bool Fired { get; private set; }

        /// <summary>The number of the picture that was not made, one based, or 0 before it fires.</summary>
        public int PictureNumber { get; private set; }

        /// <summary>The GDI objects it read when it fired, or 0 before it fires.</summary>
        public long GdiObjects { get; private set; }

        /// <summary>
        /// Reads the count for the picture about to be made. True only on the call that fires it, so the
        /// caller says the NOTICE once. A count that could not be read (negative) fires nothing.
        /// </summary>
        public bool Check(long gdiObjectsNow, int pictureNumber)
        {
            if (Fired || gdiObjectsNow <= limit)
            {
                return false;
            }

            Fired = true;
            PictureNumber = pictureNumber;
            GdiObjects = gdiObjectsNow;
            return true;
        }

        /// <summary>
        /// The NOTICE for the log and the window: at which picture, how many GDI objects and that the run
        /// goes on.
        /// </summary>
        public string Notice()
        {
            return "NOTICE   the pictures stopped at picture " + PictureNumber.ToString(CultureInfo.InvariantCulture)
                + " because Navisworks held " + GdiObjects.ToString(CultureInfo.InvariantCulture)
                + " GDI objects, past the " + limit.ToString(CultureInfo.InvariantCulture)
                + " set so the run does not crash on the limit of "
                + PictureLimits.ProcessGdiLimit.ToString(CultureInfo.InvariantCulture) + ". "
                + "The run goes on without the rest of the pictures";
        }

        /// <summary>The line said once the pass ends: how many clashes keep an empty Image cell.</summary>
        public static string LeftLine(int clashesLeftWithoutAPicture)
        {
            return "NOTICE   " + clashesLeftWithoutAPicture.ToString(CultureInfo.InvariantCulture)
                + " clashes with a status that gets a picture keep an empty Image cell, "
                + "and so show no picture in the HTML and the Excel. They are still in both and in the viewpoints";
        }
    }

    /// <summary>
    /// F182, Bader's answer C to Q175. The second stop of the picture step, beside the GDI one. Navisworks's
    /// private memory grew about 3.3 MB for each picture, to 26 GB on a PC with 15.8 GB of RAM, so past
    /// MemoryStopBytes no more pictures are made, a NOTICE says at which picture, how much memory and how
    /// many pictures were skipped, and the run goes on. It is asked before every picture and again after each
    /// one is saved, as the GDI count is, and fires once. The first of the two stops to fire ends the pictures.
    /// </summary>
    public sealed class MemoryBackstop
    {
        private readonly long limit;

        /// <summary>The stop at the run's ImageOptions.MemoryStopBytes, where a limit that cannot work is refused.</summary>
        public MemoryBackstop(long limit)
        {
            this.limit = limit;
        }

        /// <summary>Whether it has fired, after which no picture is made.</summary>
        public bool Fired { get; private set; }

        /// <summary>The number of the picture that was not made, one based, or 0 before it fires.</summary>
        public int PictureNumber { get; private set; }

        /// <summary>The private bytes it read when it fired, or 0 before it fires.</summary>
        public long PrivateBytes { get; private set; }

        /// <summary>
        /// Reads the private bytes for the picture about to be made. True only on the call that fires it.
        /// A count that could not be read (negative) fires nothing.
        /// </summary>
        public bool Check(long privateBytesNow, int pictureNumber)
        {
            if (Fired || privateBytesNow < 0 || privateBytesNow <= limit)
            {
                return false;
            }

            Fired = true;
            PictureNumber = pictureNumber;
            PrivateBytes = privateBytesNow;
            return true;
        }

        /// <summary>The line said at once when it fires, so a crash after it leaves the number to read.</summary>
        public string FiredLine()
        {
            return "NOTICE   the memory stop fired at picture " + PictureNumber.ToString(CultureInfo.InvariantCulture)
                + ", Navisworks held " + Gigabytes(PrivateBytes) + " of private memory, past the "
                + Gigabytes(limit) + " set. No more pictures are made";
        }

        /// <summary>
        /// The NOTICE for the log and the window, said once the pass ends: at which picture, how much
        /// private memory, how many pictures were skipped and that the run goes on.
        /// </summary>
        public string Notice(int skippedPictures)
        {
            return "NOTICE   the pictures stopped at picture " + PictureNumber.ToString(CultureInfo.InvariantCulture)
                + " because Navisworks held " + Gigabytes(PrivateBytes) + " of private memory, past the "
                + Gigabytes(limit) + " set so it does not run out of memory. "
                + skippedPictures.ToString(CultureInfo.InvariantCulture)
                + " pictures were skipped and their Image cells are empty. The run goes on without them";
        }

        /// <summary>Bytes in gigabytes of 1024 cubed to one place, the unit the memory lines use for megabytes.</summary>
        public static string Gigabytes(long bytes)
        {
            return (bytes / (1024d * 1024d * 1024d)).ToString("0.0", CultureInfo.InvariantCulture) + " GB";
        }
    }
}
