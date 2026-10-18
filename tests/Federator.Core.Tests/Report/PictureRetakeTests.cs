using System;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F226, Bader's answer to Q208 of 18 Oct 2026: a picture that draws no item of its clash is taken again through the active
    /// view, from the clash's own camera moved back to 30 ft where it stands closer, so no Image cell shows only sky. The
    /// reading of a picture's red and green and of its middle, the camera moved back, the picture kept and the one RETAKE
    /// line, each broken one thing at a time, on pictures made here pixel by pixel in the bytes LockBits hands the add-in.
    /// </summary>
    [TestFixture]
    public class PictureRetakeTests
    {
        private const int Width = 20;
        private const int Height = 10;
        private const int Stride = Width * 4;

        /// <summary>A picture of one colour, its bytes blue, green, red, alpha a pixel.</summary>
        private static byte[] Filled(byte r, byte g, byte b)
        {
            byte[] bytes = new byte[Stride * Height];

            for (int at = 0; at < bytes.Length; at += 4)
            {
                bytes[at] = b;
                bytes[at + 1] = g;
                bytes[at + 2] = r;
                bytes[at + 3] = 255;
            }

            return bytes;
        }

        private static void Paint(byte[] bytes, int x, int y, byte r, byte g, byte b)
        {
            int at = y * Stride + x * 4;
            bytes[at] = b;
            bytes[at + 1] = g;
            bytes[at + 2] = r;
        }

        [Test]
        public void ARedOrAGreenItemIsCountedAndTheSkyAndAShadedPixelAreNot()
        {
            PictureRetakeSettings settings = new PictureRetakeSettings();
            byte[] sky = Filled(136, 169, 238);

            Assert.That(PictureRetake.ItemPixels(sky, Width, Height, Stride, settings), Is.EqualTo(0));

            Paint(sky, 2, 2, 230, 40, 40);
            Paint(sky, 4, 4, 40, 230, 40);
            Paint(sky, 6, 6, 170, 40, 40);
            Paint(sky, 8, 8, 230, 120, 40);
            Paint(sky, 3, 3, 230, 40, 40);

            Assert.That(PictureRetake.ItemPixels(sky, Width, Height, Stride, settings), Is.EqualTo(2),
                "red and green above 180 with the other two below 110, every 2nd pixel of every 2nd row, as count-sky-pictures.ps1 read them");

            settings.Every = 1;
            Assert.That(PictureRetake.ItemPixels(sky, Width, Height, Stride, settings), Is.EqualTo(3), "the pixel at 3,3 is read every one pixel");

            Assert.That(PictureRetake.ItemPixels(null, Width, Height, Stride, settings), Is.EqualTo(0));
        }

        /// <summary>
        /// The sky runs from 108,144,230 at the top to 209,238,254 at the bottom, read off F226's first smoke run, whose 4 pictures
        /// of only the sky had 158,189,243 at their middle and so were not the 136,169,238 the first pass looked for there.
        /// </summary>
        [Test]
        public void ASkyOfAnyRowIsTheSkyAndAWallIsNever()
        {
            PictureRetakeSettings settings = new PictureRetakeSettings();
            byte[] gradient = Filled(108, 144, 230);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int r = 108 + (209 - 108) * y / (Height - 1);
                    int g = 144 + (238 - 144) * y / (Height - 1);
                    int b = 230 + (254 - 230) * y / (Height - 1);
                    Paint(gradient, x, y, (byte)r, (byte)g, (byte)b);
                }
            }

            Assert.That(PictureRetake.OnlyTheSky(gradient, Width, Height, Stride, settings), Is.True, "the top row, the middle and the palest bottom row");
            Assert.That(PictureRetake.OnlyTheSky(Filled(158, 189, 243), Width, Height, Stride, settings), Is.True);
            Assert.That(PictureRetake.OnlyTheSky(Filled(168, 168, 168), Width, Height, Stride, settings), Is.False, "a grey wall is never the sky");

            Paint(gradient, 0, 0, 230, 230, 230);
            Assert.That(PictureRetake.OnlyTheSky(gradient, Width, Height, Stride, settings), Is.True, "a label in a corner leaves the sky the sky");

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Paint(gradient, x, y, 168, 168, 168);
                }
            }

            Assert.That(PictureRetake.OnlyTheSky(gradient, Width, Height, Stride, settings), Is.False, "a wall across a fifth of the picture is something");
            Assert.That(PictureRetake.OnlyTheSky(null, Width, Height, Stride, settings), Is.False);
        }

        [Test]
        public void TheCameraMovesBackAlongItsLineOnlyWhereItStandsCloser()
        {
            double[] centre = { 100, 200, 10 };

            double[] near = PictureRetake.BackedOff(new double[] { 103, 204, 10 }, centre, 30);
            Assert.That(near[0], Is.EqualTo(118).Within(1e-9));
            Assert.That(near[1], Is.EqualTo(224).Within(1e-9), "a camera 5 away moved to 30 along the same line");
            Assert.That(near[2], Is.EqualTo(10).Within(1e-9));

            double[] far = PictureRetake.BackedOff(new double[] { 140, 200, 10 }, centre, 30);
            Assert.That(far[0], Is.EqualTo(140), "a camera 40 away stays where it stands");

            double[] onIt = PictureRetake.BackedOff(new double[] { 100, 200, 10 }, centre, 30);
            Assert.That(onIt[0], Is.EqualTo(100), "a camera on the centre has no line to move along");

            Assert.Throws<ArgumentException>(() => PictureRetake.BackedOff(new double[] { 1, 2 }, centre, 30));
        }

        [Test]
        public void ThePictureKeptDrawsTheClashAndNoneShowsOnlyTheSky()
        {
            Assert.That(PictureRetake.Choose(true, false, false, false, false), Is.EqualTo(RetakeChoice.TheTool), "a picture that draws an item is not taken again");
            Assert.That(PictureRetake.Choose(false, true, true, true, false), Is.EqualTo(RetakeChoice.TheRetake), "the retake draws the items, the 72 of 1B06BC");
            Assert.That(PictureRetake.Choose(false, true, true, false, false), Is.EqualTo(RetakeChoice.TheRetake),
                "the retake draws no item but is not the sky, the 4 of 1B06BC, so no Image cell shows only sky");
            Assert.That(PictureRetake.Choose(false, true, true, false, true), Is.EqualTo(RetakeChoice.NoPicture), "both only the sky, no picture");
            Assert.That(PictureRetake.Choose(false, true, false, false, false), Is.EqualTo(RetakeChoice.TheTool),
                "the breaker's finding: a sky picture whose retake threw or could not be taken keeps the tool's picture, never dropped");
            Assert.That(PictureRetake.Choose(false, false, true, false, true), Is.EqualTo(RetakeChoice.TheTool), "the tool's picture is kept where it is not the sky and the retake is");
            Assert.That(PictureRetake.Choose(false, false, false, false, false), Is.EqualTo(RetakeChoice.TheTool), "a result group, never taken again, keeps its picture");
        }

        [Test]
        public void TheLineSaysTheCountRetakenAndTheCountStillEmptyAndANoughtApart()
        {
            RetakeTally tally = new RetakeTally();

            for (int i = 0; i < 10; i++)
            {
                tally.Took();
            }

            Assert.That(tally.Line(PictureRetakeSettings.DefaultLeastMillimetres, true),
                Is.EqualTo("RETAKE   every one of the 10 pictures read drew an item of its clash, so none was taken again, Bader's answer to Q208"));
            Assert.That(tally.Line(PictureRetakeSettings.DefaultLeastMillimetres, false), Does.Contain("is off"));

            tally.Add(true, true, RetakeChoice.TheRetake, 0.1);
            tally.Add(true, false, RetakeChoice.TheRetake, 0.1);
            tally.Add(false, false, RetakeChoice.TheTool, 0);
            tally.Add(true, false, RetakeChoice.NoPicture, 0.1);

            Assert.That(tally.Line(PictureRetakeSettings.DefaultLeastMillimetres, true), Is.EqualTo(
                "RETAKE   4 of the 10 pictures read drew no item of the clash, 3 taken again through the active view from 9.144 m, 30 ft,"
                + " at the nearest with the two items red and green and the other models hidden, in 0.3 s, 1 drawing their items and 2"
                + " still drawing none, 1 not taken again and kept as Clash Detective drew them, a result group or a retake that could not"
                + " be taken, and 1 left with no picture as the tool's picture and its retake both showed only the sky, Bader's answer to Q208"),
                "the whole beside the part, so every picture drawing nothing reads N of N, the breaker's finding");
        }

        [Test]
        public void ANumberThatCannotWorkIsRefusedWhereItIsSet()
        {
            PictureRetakeSettings settings = new PictureRetakeSettings();
            Assert.That(settings.Retake, Is.True, "on by Q208");
            Assert.That(settings.LeastMillimetres, Is.EqualTo(9144), "30 ft");
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.LeastMillimetres = 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.LeastMillimetres = double.NaN);
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.Every = 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.Bright = 256);
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.Dark = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.SkyBlueAtLeast = 300);
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.SkyBlueOverRed = -5);
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.SkyShare = 0, "a share of nought reads every picture as the sky");
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.SkyShare = 1.5);
            Assert.That(PictureRetake.BackedOff(new[] { double.NaN, 0, 0 }, new double[] { 0, 0, 0 }, 30)[0], Is.NaN,
                "a camera that reads NaN is left as it reads and never moved by a NaN factor");
            Assert.That(new ImageOptions().Retake.Retake, Is.True, "the pictures' options carry it");
        }
    }
}
