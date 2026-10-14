using System;
using System.Globalization;

namespace Federator.Core.Report
{
    /// <summary>
    /// F172, Bader's answer to Q171. How many pictures a clash test gets, and where the pictures stop so
    /// the run cannot crash on the GDI limit. Two rules and nothing else:
    ///
    /// ONE FIXED RULE PER TEST. A test with more than 1000 clashes gets pictures for its first 100
    /// clashes and the rest are skipped. It is the test that counts, not the run. The clashes stay in the
    /// HTML, the Excel and the viewpoints, only their Image cell is empty.
    ///
    /// ONE BACKSTOP FOR THE PROCESS. Every picture costs the Navisworks process one GDI object and a
    /// process may hold 10000. Past 9000 no more pictures are made, a NOTICE says at which picture and
    /// how many objects, and the run goes on to the NWD, the HTML, the Excel and the viewpoints. The
    /// run 20261014-105308 crashed Navisworks at picture 8907 of 10170 at 9999 objects, in lcodwin.dll.
    /// </summary>
    public static class PictureLimits
    {
        /// <summary>A test with more clashes than this is a many clash test.</summary>
        public const int ManyClashes = 1000;

        /// <summary>The pictures a many clash test gets, its first clashes in report order.</summary>
        public const int PicturesForAManyClashTest = 100;

        /// <summary>The GDI objects of the Navisworks process past which no picture is made.</summary>
        public const long GdiStop = 9000;

        /// <summary>
        /// The most pictures a test may have. The fixed rule for a many clash test, the least of it and
        /// the box's number where the box is not zero, and no limit at all (int.MaxValue) otherwise.
        /// </summary>
        public static int LimitFor(int clashesInTheTest, int boxCapPerTest)
        {
            int limit = clashesInTheTest > ManyClashes ? PicturesForAManyClashTest : int.MaxValue;

            if (boxCapPerTest > 0 && boxCapPerTest < limit)
            {
                limit = boxCapPerTest;
            }

            return limit;
        }

        /// <summary>
        /// The one line a many clash test gets in the log: its name, its clashes, the pictures made and
        /// the clashes whose picture was skipped.
        /// </summary>
        public static string TestLine(string testName, int clashes, int made, int skipped)
        {
            return "IMAGES   " + testName + " has " + clashes.ToString(CultureInfo.InvariantCulture)
                + " clashes, more than " + ManyClashes.ToString(CultureInfo.InvariantCulture)
                + ", so " + made.ToString(CultureInfo.InvariantCulture) + " pictures were made and "
                + skipped.ToString(CultureInfo.InvariantCulture) + " were skipped. The clashes stay in the "
                + "HTML, the Excel and the viewpoints, only their Image cell is empty";
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
