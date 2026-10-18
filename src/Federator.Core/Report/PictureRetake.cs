using System;
using System.Globalization;

namespace Federator.Core.Report
{
    /// <summary>
    /// F226, Bader's answer to Q208 of 18 Oct 2026: retake each picture that draws no item of its clash through the active
    /// view, from the clash's own camera moved back to 30 ft where it stands closer, its two items red and green, other models
    /// hidden, so no Image cell shows only sky. Every number is a setting, measured in scan.md 5z-zm on copies of 1B06BC and
    /// 1B06P1, where the rule's red and green found 76 and 25 pictures drawing no item, the audit's counts, and the retake
    /// drew the items of 72 and 25.
    /// </summary>
    public sealed class PictureRetakeSettings
    {
        /// <summary>30 ft in millimetres, the least distance from the clash's centre a retake's camera stands, Q208.</summary>
        public const double DefaultLeastMillimetres = 9144;

        /// <summary>A channel of a clashing item's colour reads above this, count-sky-pictures.ps1's measure.</summary>
        public const int DefaultBright = 180;

        /// <summary>The other two channels of a clashing item's colour read below this.</summary>
        public const int DefaultDark = 110;

        /// <summary>Every 2nd pixel of every 2nd row is read, as the measure read them.</summary>
        public const int DefaultEvery = 2;

        private double leastMillimetres = DefaultLeastMillimetres;
        private int every = DefaultEvery;

        public PictureRetakeSettings()
        {
            Retake = true;
            Bright = DefaultBright;
            Dark = DefaultDark;
            SkyRed = 136;
            SkyGreen = 169;
            SkyBlue = 238;
        }

        /// <summary>Whether a picture that draws no item of its clash is taken again, on by Q208.</summary>
        public bool Retake { get; set; }

        /// <summary>The least distance in millimetres. Refused at or below nought.</summary>
        public double LeastMillimetres
        {
            get { return leastMillimetres; }
            set
            {
                if (!(value > 0) || double.IsInfinity(value))
                {
                    throw new ArgumentOutOfRangeException("value", value, "the least distance of a retake must be above nought");
                }

                leastMillimetres = value;
            }
        }

        public int Bright { get; set; }

        public int Dark { get; set; }

        /// <summary>How many pixels apart a row and a column are read. Refused below one.</summary>
        public int Every
        {
            get { return every; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value", value, "a picture is read every one pixel at least");
                }

                every = value;
            }
        }

        /// <summary>The sky's colour at a picture's middle, 136,169,238, read off every empty picture of 5z-zk.</summary>
        public int SkyRed { get; set; }

        public int SkyGreen { get; set; }

        public int SkyBlue { get; set; }
    }

    /// <summary>Which picture a clash keeps once its retake was taken or not.</summary>
    public enum RetakeChoice
    {
        /// <summary>Clash Detective's own picture, which drew an item, or which is not the sky where the retake drew none.</summary>
        TheTool,

        /// <summary>The picture taken again, which drew an item, or which is not the sky where the tool's was.</summary>
        TheRetake,

        /// <summary>No picture, since both showed only the sky, so no Image cell shows only sky.</summary>
        NoPicture
    }

    /// <summary>The rule of a picture that draws no item of its clash, one place for the add-in and its tests.</summary>
    public static class PictureRetake
    {
        /// <summary>
        /// The pixels that read as a clashing item's red or green in a 32 bit picture, its bytes blue, green, red, alpha a
        /// pixel, as LockBits hands them, every setting's pixel of every setting's row.
        /// </summary>
        public static int ItemPixels(byte[] bgra, int width, int height, int stride, PictureRetakeSettings settings)
        {
            if (bgra == null || settings == null || width <= 0 || height <= 0 || stride < width * 4)
            {
                return 0;
            }

            int items = 0;

            for (int y = 0; y < height; y += settings.Every)
            {
                int row = y * stride;

                for (int x = 0; x < width; x += settings.Every)
                {
                    int at = row + x * 4;

                    if (at + 2 >= bgra.Length)
                    {
                        return items;
                    }

                    int b = bgra[at];
                    int g = bgra[at + 1];
                    int r = bgra[at + 2];

                    if ((r > settings.Bright && g < settings.Dark && b < settings.Dark)
                        || (g > settings.Bright && r < settings.Dark && b < settings.Dark))
                    {
                        items++;
                    }
                }
            }

            return items;
        }

        /// <summary>Whether a picture's middle pixel is the sky's colour, read in the same bytes.</summary>
        public static bool MiddleIsTheSky(byte[] bgra, int width, int height, int stride, PictureRetakeSettings settings)
        {
            if (bgra == null || settings == null || width <= 0 || height <= 0)
            {
                return false;
            }

            int at = (height / 2) * stride + (width / 2) * 4;

            return at + 2 < bgra.Length && bgra[at + 2] == settings.SkyRed && bgra[at + 1] == settings.SkyGreen && bgra[at] == settings.SkyBlue;
        }

        /// <summary>
        /// The camera of the retake: the clash's own camera, moved back along the line from the clash's centre through it to
        /// the least distance where it stands closer, and left where it stands otherwise or where it stands on the centre.
        /// </summary>
        public static double[] BackedOff(double[] camera, double[] centre, double least)
        {
            if (camera == null || centre == null || camera.Length != 3 || centre.Length != 3)
            {
                throw new ArgumentException("a camera and a centre are three numbers each");
            }

            double dx = camera[0] - centre[0];
            double dy = camera[1] - centre[1];
            double dz = camera[2] - centre[2];
            double length = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (length <= 0 || length >= least)
            {
                return new[] { camera[0], camera[1], camera[2] };
            }

            double k = least / length;
            return new[] { centre[0] + dx * k, centre[1] + dy * k, centre[2] + dz * k };
        }

        /// <summary>The picture a clash keeps. A retake not taken leaves the tool's, unless it is only the sky.</summary>
        public static RetakeChoice Choose(bool toolDrew, bool toolSky, bool retaken, bool retakeDrew, bool retakeSky)
        {
            if (toolDrew)
            {
                return RetakeChoice.TheTool;
            }

            if (retaken && retakeDrew)
            {
                return RetakeChoice.TheRetake;
            }

            if (!toolSky)
            {
                return RetakeChoice.TheTool;
            }

            return retaken && !retakeSky ? RetakeChoice.TheRetake : RetakeChoice.NoPicture;
        }
    }

    /// <summary>The retakes of one group's picture pass, written as one IMAGES line, Q208.</summary>
    public sealed class RetakeTally
    {
        public int DrewNothing { get; private set; }

        public int Retaken { get; private set; }

        public int RetakeDrew { get; private set; }

        public int NotRetaken { get; private set; }

        public int NoPicture { get; private set; }

        public double Seconds { get; private set; }

        /// <summary>A picture taken the tool's way that drew no item of its clash, and what came of it.</summary>
        public void Add(bool retaken, bool retakeDrew, RetakeChoice choice, double seconds)
        {
            DrewNothing++;

            if (retaken)
            {
                Retaken++;
                Seconds += seconds;

                if (retakeDrew)
                {
                    RetakeDrew++;
                }
            }
            else
            {
                NotRetaken++;
            }

            if (choice == RetakeChoice.NoPicture)
            {
                NoPicture++;
            }
        }

        /// <summary>The one line, the count retaken and the count still empty, and a nought said.</summary>
        public string Line(double leastMillimetres, bool on)
        {
            if (!on)
            {
                return "RETAKE   the retake of a picture that draws no item of its clash is off, so none was taken again";
            }

            if (DrewNothing == 0)
            {
                return "RETAKE   no picture drew none of its clash's items, so none was taken again, Bader's answer to Q208";
            }

            return "RETAKE   " + DrewNothing + (DrewNothing == 1 ? " picture" : " pictures") + " drew no item of the clash, "
                + Retaken + " taken again through the active view from " + Metres(leastMillimetres) + " at the nearest with the two items"
                + " red and green and the other models hidden, in " + Seconds.ToString("0.0", CultureInfo.InvariantCulture) + " s, "
                + RetakeDrew + " drawing their items and " + (Retaken - RetakeDrew) + " still drawing none, "
                + NotRetaken + " not taken again, a result group or a retake that threw, and " + NoPicture
                + " left with no picture as both showed only the sky, Bader's answer to Q208";
        }

        private static string Metres(double millimetres)
        {
            return (millimetres / 1000).ToString("0.###", CultureInfo.InvariantCulture) + " m, "
                + (millimetres / 304.8).ToString("0.#", CultureInfo.InvariantCulture) + " ft,";
        }
    }
}
