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
            Assert.That(PictureRetake.MiddleIsTheSky(sky, Width, Height, Stride, settings), Is.True);

            Paint(sky, 2, 2, 230, 40, 40);
            Paint(sky, 4, 4, 40, 230, 40);
            Paint(sky, 6, 6, 170, 40, 40);
            Paint(sky, 8, 8, 230, 120, 40);
            Paint(sky, 3, 3, 230, 40, 40);

            Assert.That(PictureRetake.ItemPixels(sky, Width, Height, Stride, settings), Is.EqualTo(2),
                "red and green above 180 with the other two below 110, every 2nd pixel of every 2nd row, as count-sky-pictures.ps1 read them");

            settings.Every = 1;
            Assert.That(PictureRetake.ItemPixels(sky, Width, Height, Stride, settings), Is.EqualTo(3), "the pixel at 3,3 is read every one pixel");

            byte[] grey = Filled(168, 168, 168);
            Assert.That(PictureRetake.MiddleIsTheSky(grey, Width, Height, Stride, settings), Is.False, "a wall at the middle is not the sky");
            Assert.That(PictureRetake.ItemPixels(null, Width, Height, Stride, settings), Is.EqualTo(0));
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
            Assert.That(PictureRetake.Choose(false, true, false, false, false), Is.EqualTo(RetakeChoice.NoPicture), "a sky picture whose retake threw is not kept");
            Assert.That(PictureRetake.Choose(false, false, true, false, true), Is.EqualTo(RetakeChoice.TheTool), "the tool's picture is kept where it is not the sky and the retake is");
            Assert.That(PictureRetake.Choose(false, false, false, false, false), Is.EqualTo(RetakeChoice.TheTool), "a result group, never taken again, keeps its picture");
        }

        [Test]
        public void TheLineSaysTheCountRetakenAndTheCountStillEmptyAndANoughtApart()
        {
            RetakeTally tally = new RetakeTally();
            Assert.That(tally.Line(PictureRetakeSettings.DefaultLeastMillimetres, true),
                Is.EqualTo("RETAKE   no picture drew none of its clash's items, so none was taken again, Bader's answer to Q208"));
            Assert.That(tally.Line(PictureRetakeSettings.DefaultLeastMillimetres, false), Does.Contain("is off"));

            tally.Add(true, true, RetakeChoice.TheRetake, 0.1);
            tally.Add(true, false, RetakeChoice.TheRetake, 0.1);
            tally.Add(false, false, RetakeChoice.NoPicture, 0);
            tally.Add(true, true, RetakeChoice.TheRetake, 0.1);

            Assert.That(tally.Line(PictureRetakeSettings.DefaultLeastMillimetres, true), Is.EqualTo(
                "RETAKE   4 pictures drew no item of the clash, 3 taken again through the active view from 9.144 m, 30 ft, at the nearest"
                + " with the two items red and green and the other models hidden, in 0.3 s, 2 drawing their items and 1 still drawing none,"
                + " 1 not taken again, a result group or a retake that threw, and 1 left with no picture as both showed only the sky,"
                + " Bader's answer to Q208"));
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
            Assert.That(new ImageOptions().Retake.Retake, Is.True, "the pictures' options carry it");
        }
    }
}
