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
        private int bright;
        private int dark;
        private double skyShare;
        private int skyBlueAtLeast;
        private int skyBlueOverRed;

        public PictureRetakeSettings()
        {
            Retake = true;
            Bright = DefaultBright;
            Dark = DefaultDark;
            SkyShare = 0.95;
            SkyBlueAtLeast = 200;
            SkyBlueOverRed = 40;
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

        public int Bright { get { return bright; } set { bright = Channel(value); } }

        public int Dark { get { return dark; } set { dark = Channel(value); } }

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

        /// <summary>The share of the pixels read that must be the sky's blue for a picture to show only the sky, 0.95.</summary>
        public double SkyShare
        {
            get { return skyShare; }
            set
            {
                if (!(value > 0) || value > 1)
                {
                    throw new ArgumentOutOfRangeException("value", value, "the share of the sky is above nought and at most one");
                }

                skyShare = value;
            }
        }

        /// <summary>A channel of a colour, nought to 255, refused otherwise.</summary>
        private static int Channel(int value)
        {
            if (value < 0 || value > 255)
            {
                throw new ArgumentOutOfRangeException("value", value, "a colour channel is nought to 255");
            }

            return value;
        }

        /// <summary>The least blue of the sky, 200, under the 230 of its darkest row.</summary>
        public int SkyBlueAtLeast { get { return skyBlueAtLeast; } set { skyBlueAtLeast = Channel(value); } }

        /// <summary>The least the sky's blue is over its red, 40, under the 45 of its palest row.</summary>
        public int SkyBlueOverRed { get { return skyBlueOverRed; } set { skyBlueOverRed = Channel(value); } }
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

        /// <summary>
        /// Whether a picture shows only the sky: at least the setting's share of the pixels read are the sky's blue, blue the
        /// largest channel, green over red, blue at the setting's least and over red by the setting's gap. The sky runs from
        /// 108,144,230 at the top to 209,238,254 at the bottom of a picture of 1024 by 1024, read off F226's first smoke run, so
        /// no one colour names it, and a grey wall, 168,168,168 at the middle of 5z-zm's 4 retakes, is never the sky.
        /// </summary>
        public static bool OnlyTheSky(byte[] bgra, int width, int height, int stride, PictureRetakeSettings settings)
        {
            if (bgra == null || settings == null || width <= 0 || height <= 0 || stride < width * 4)
            {
                return false;
            }

            int read = 0;
            int sky = 0;

            for (int y = 0; y < height; y += settings.Every)
            {
                for (int x = 0; x < width; x += settings.Every)
                {
                    int at = y * stride + x * 4;

                    if (at + 2 >= bgra.Length)
                    {
                        break;
                    }

                    int b = bgra[at];
                    int g = bgra[at + 1];
                    int r = bgra[at + 2];
                    read++;

                    if (b >= g && g >= r && b >= settings.SkyBlueAtLeast && b - r >= settings.SkyBlueOverRed)
                    {
                        sky++;
                    }
                }
            }

            return read > 0 && sky >= settings.SkyShare * read;
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

            if (double.IsNaN(length) || double.IsNaN(least) || length <= 0 || length >= least)
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

            // A retake not taken, a result group or one that threw, keeps the tool's picture as before F226, the breaker's
            // finding: a throw never drops a picture, whatever it shows, and the line counts it apart.
            if (!retaken)
            {
                return RetakeChoice.TheTool;
            }

            if (retakeDrew)
            {
                return RetakeChoice.TheRetake;
            }

            if (!toolSky)
            {
                return RetakeChoice.TheTool;
            }

            return retakeSky ? RetakeChoice.NoPicture : RetakeChoice.TheRetake;
        }
    }

    /// <summary>The retakes of one group's picture pass, written as one IMAGES line, Q208.</summary>
    public sealed class RetakeTally
    {
        /// <summary>Every picture the tool's way read for its items, so a count drawing nothing is set beside the whole.</summary>
        public int Read { get; private set; }

        public int DrewNothing { get; private set; }

        public int Retaken { get; private set; }

        public int RetakeDrew { get; private set; }

        public int NotRetaken { get; private set; }

        public int NoPicture { get; private set; }

        public double Seconds { get; private set; }

        /// <summary>One picture taken the tool's way and read, whatever it drew.</summary>
        public void Took()
        {
            Read++;
        }

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
                return "RETAKE   every one of the " + Read + " pictures read drew an item of its clash, so none was taken again,"
                    + " Bader's answer to Q208";
            }

            return "RETAKE   " + DrewNothing + " of the " + Read + " pictures read drew no item of the clash, "
                + Retaken + " taken again through the active view from " + Metres(leastMillimetres) + " at the nearest with the two items"
                + " red and green and the other models hidden, in " + Seconds.ToString("0.0", CultureInfo.InvariantCulture) + " s, "
                + RetakeDrew + " drawing their items and " + (Retaken - RetakeDrew) + " still drawing none, "
                + NotRetaken + " not taken again and kept as Clash Detective drew them, a result group or a retake that could not be"
                + " taken, and " + NoPicture + " left with no picture as the tool's picture and its retake both showed only the sky,"
                + " Bader's answer to Q208";
        }

        private static string Metres(double millimetres)
        {
            return (millimetres / 1000).ToString("0.###", CultureInfo.InvariantCulture) + " m, "
                + (millimetres / 304.8).ToString("0.#", CultureInfo.InvariantCulture) + " ft,";
        }
    }
}
