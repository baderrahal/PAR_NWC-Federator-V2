using System;
using System.Globalization;
using Federator.Core.Clash;

namespace Federator.Core.Report
{
    /// <summary>
    /// F172 and F175, Bader's answers to Q171. How many pictures a clash test gets, and where the pictures
    /// stop so the run cannot crash on the GDI limit. Two rules and nothing else:
    ///
    /// ONE RULE PER TEST BY ITS PRIORITY. A test with more than 750 clashes gets pictures for its first
    /// clashes only: priority A the first 150, priority B the first 100 and priority C the first 50. A test
    /// the matrix does not name counts as C, F168, and so does a test whose priority is not known. It is the
    /// test that counts, not the run. The clashes stay in the HTML, the Excel and the viewpoints, only
    /// their Image cell is empty, and every clash keeps its viewpoint.
    ///
    /// ONE BACKSTOP FOR THE PROCESS. Every picture costs the Navisworks process one GDI object and a
    /// process may hold 10000. Past 9000 no more pictures are made, a NOTICE says at which picture and
    /// how many objects, and the run goes on to the NWD, the HTML, the Excel and the viewpoints. The
    /// run 20261014-105308 crashed Navisworks at picture 8907 of 10170 at 9999 objects, in lcodwin.dll.
    /// </summary>
    public static class PictureLimits
    {
        /// <summary>A test with more clashes than this is a many clash test. 750 since F181, Bader's answer to Q176, it was 1000.</summary>
        public const int ManyClashes = 750;

        /// <summary>The pictures a many clash test of priority A gets, its first clashes in report order.</summary>
        public const int PicturesForA = 150;

        /// <summary>The pictures a many clash test of priority B gets.</summary>
        public const int PicturesForB = 100;

        /// <summary>The pictures a many clash test of priority C gets, and of one with no priority.</summary>
        public const int PicturesForC = 50;

        /// <summary>The GDI objects of the Navisworks process past which no picture is made.</summary>
        public const long GdiStop = 9000;

        /// <summary>
        /// The pictures a many clash test of that priority gets. None is C, because a test no matrix
        /// names is C by F168 and a priority not known is the least a test may get, never the most.
        /// </summary>
        public static int PicturesFor(ClashPriority priority)
        {
            switch (priority)
            {
                case ClashPriority.A: return PicturesForA;
                case ClashPriority.B: return PicturesForB;
                default: return PicturesForC;
            }
        }

        /// <summary>
        /// The most pictures a test may have. The rule of its priority for a many clash test, the least of
        /// it and the box's number where the box is not zero, and no limit at all (int.MaxValue) otherwise.
        /// </summary>
        public static int LimitFor(int clashesInTheTest, ClashPriority priority, int boxCapPerTest)
        {
            int limit = clashesInTheTest > ManyClashes ? PicturesFor(priority) : int.MaxValue;

            if (boxCapPerTest > 0 && boxCapPerTest < limit)
            {
                limit = boxCapPerTest;
            }

            return limit;
        }

        /// <summary>
        /// The one line a many clash test gets in the log: its name, its priority letter, its clashes,
        /// the pictures made and the clashes whose picture was skipped.
        /// </summary>
        public static string TestLine(string testName, ClashPriority priority, int clashes, int made, int skipped)
        {
            return "IMAGES   " + testName + " has priority " + Letter(priority) + " and "
                + clashes.ToString(CultureInfo.InvariantCulture)
                + " clashes, more than " + ManyClashes.ToString(CultureInfo.InvariantCulture)
                + ", so " + made.ToString(CultureInfo.InvariantCulture) + " pictures were made and "
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

        /// <summary>The rule said in words for the window and the log: the trigger and the three caps.</summary>
        public static string RuleInWords()
        {
            return "a test with more than " + ManyClashes.ToString(CultureInfo.InvariantCulture)
                + " clashes gets pictures for its first " + PicturesForA.ToString(CultureInfo.InvariantCulture)
                + " clashes at priority A, " + PicturesForB.ToString(CultureInfo.InvariantCulture)
                + " at priority B and " + PicturesForC.ToString(CultureInfo.InvariantCulture) + " at priority C";
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

        public GdiBackstop()
            : this(PictureLimits.GdiStop)
        {
        }

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
                + " set so the run does not crash on the limit of 10000. "
                + "The run goes on to the NWD, the HTML, the Excel and the viewpoints";
        }

        /// <summary>The line said once the pass ends: how many clashes keep an empty Image cell.</summary>
        public static string LeftLine(int clashesLeftWithoutAPicture)
        {
            return "NOTICE   " + clashesLeftWithoutAPicture.ToString(CultureInfo.InvariantCulture)
                + " clashes with a status that gets a picture keep an empty Image cell, "
                + "and so show no picture in the HTML and the Excel. They are still in both and in the viewpoints";
        }
    }
}
