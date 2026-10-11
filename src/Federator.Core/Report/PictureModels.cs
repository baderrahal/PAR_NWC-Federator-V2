using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Report
{
    /// <summary>
    /// What one side of a test was read to, F145: the models its items live in by their index in
    /// the document's own order, how many items it holds, how many live in no model that could be
    /// read, and how many live in a model whose file name another model of the document shares.
    /// </summary>
    public sealed class PictureSide
    {
        public PictureSide(IEnumerable<int> models, int itemsRead, int itemsWithNoModel, int itemsInASharedFile)
        {
            Read = true;
            Models = new List<int>(models ?? new int[0]);
            ItemsRead = Math.Max(0, itemsRead);
            ItemsWithNoModel = Math.Max(0, itemsWithNoModel);
            ItemsInASharedFile = Math.Max(0, itemsInASharedFile);
        }

        private PictureSide()
        {
            Models = new List<int>();
        }

        /// <summary>A side whose items could not be read at all.</summary>
        public static PictureSide NotRead()
        {
            return new PictureSide();
        }

        public bool Read { get; private set; }

        public IList<int> Models { get; private set; }

        public int ItemsRead { get; private set; }

        public int ItemsWithNoModel { get; private set; }

        public int ItemsInASharedFile { get; private set; }
    }

    /// <summary>
    /// Which models stay shown while one test's pictures are taken, F145, Bader's choice of Q150
    /// on 2026-10-11: only the models the test's own two sides live in, every other model hidden
    /// once before that test's pictures and the document's hidden state put back after, at the same
    /// size and style. Measured on the first 20 clashes of BLD-AR-Walls-vs-BLD-AR-Floors in 1A04PK,
    /// docs\history\scan.md 5z-zc: 7.76 s a picture with every model shown and 1.78 s with only the
    /// test's, the export's layout kept.
    ///
    /// A PICTURE NEVER LOSES A MODEL IT MAY NEED. The sides are the ones the test itself holds, read
    /// off the test and not off the file's plan, and every doubt keeps every model shown and says
    /// why: no side handed in, a side not read, a side holding no item, an item in a model whose file
    /// name another model shares, an item whose model was not read, no model read, or a model read
    /// outside the document's. Where the sides live in every model there is nothing to hide.
    /// </summary>
    public sealed class PictureModels
    {
        private PictureModels(IList<int> shown, string whyEveryModel)
        {
            Shown = shown;
            WhyEveryModel = whyEveryModel;
        }

        /// <summary>The indexes of the models left shown, in order, or empty where nothing is hidden.</summary>
        public IList<int> Shown { get; private set; }

        /// <summary>Null where models are hidden, otherwise why every model stays shown.</summary>
        public string WhyEveryModel { get; private set; }

        public bool HidesAny
        {
            get { return WhyEveryModel == null; }
        }

        /// <summary>The rule for one test, its sides as read off the test itself.</summary>
        public static PictureModels For(int modelCount, IList<PictureSide> sides)
        {
            if (modelCount <= 0)
            {
                return Every("the document holds no model");
            }

            if (sides == null || sides.Count == 0)
            {
                return Every("no side of the test was handed in");
            }

            int notRead = 0;
            int empty = 0;
            int noModel = 0;
            int shared = 0;
            SortedSet<int> shown = new SortedSet<int>();

            foreach (PictureSide side in sides)
            {
                if (side == null || !side.Read)
                {
                    notRead++;
                    continue;
                }

                if (side.ItemsRead == 0)
                {
                    empty++;
                }

                noModel += side.ItemsWithNoModel;
                shared += side.ItemsInASharedFile;

                foreach (int model in side.Models)
                {
                    if (model < 0 || model >= modelCount)
                    {
                        return Every("a model read for its sides, " + model.ToString(CultureInfo.InvariantCulture)
                            + ", is not one of the document's " + modelCount.ToString(CultureInfo.InvariantCulture));
                    }

                    shown.Add(model);
                }
            }

            if (notRead > 0)
            {
                return Every(notRead + (notRead == 1 ? " side" : " sides") + " of the test could not be read");
            }

            if (empty > 0)
            {
                return Every(empty + (empty == 1 ? " side" : " sides") + " of the test hold no item");
            }

            if (shared > 0)
            {
                return Every(shared + (shared == 1 ? " item of its sides lives" : " items of its sides live")
                    + " in a model whose file name another model of the document shares");
            }

            if (noModel > 0)
            {
                return Every(noModel + (noModel == 1 ? " item of its sides lives" : " items of its sides live")
                    + " in no model that could be read");
            }

            if (shown.Count == 0)
            {
                return Every("no item of its sides was read to a model");
            }

            if (shown.Count == modelCount)
            {
                return Every("its sides live in every model of the document");
            }

            return new PictureModels(new List<int>(shown), null);
        }

        private static PictureModels Every(string why)
        {
            return new PictureModels(new List<int>(), why + ", so every model stays shown for its pictures");
        }
    }

    /// <summary>
    /// What showing only each test's models did over one clash step, F145, for its one IMAGES
    /// line: the tests whose pictures were taken with models hidden, how many models each hid, the
    /// tests that kept every model shown, the seconds reading the tests' sides took and the seconds
    /// the hiding and the putting back took, apart, so the cost of the rule is read beside what it
    /// saves.
    /// </summary>
    public sealed class PictureModelsTally
    {
        public int TestsHidden { get; private set; }

        public int ModelsHidden { get; private set; }

        public int TestsKeptEvery { get; private set; }

        public double ReadingSeconds { get; private set; }

        public double HidingSeconds { get; private set; }

        public void SidesRead(double seconds)
        {
            ReadingSeconds += Math.Max(0.0, seconds);
        }

        public void Hid(int models, double seconds)
        {
            TestsHidden++;
            ModelsHidden += Math.Max(0, models);
            HidingSeconds += Math.Max(0.0, seconds);
        }

        public void KeptEvery()
        {
            TestsKeptEvery++;
        }

        /// <summary>The one line, which says the setting was off where it was.</summary>
        public string Line(bool on)
        {
            if (!on)
            {
                return "IMAGES   every model shown for the pictures, the setting asks for it";
            }

            if (TestsHidden == 0 && TestsKeptEvery == 0)
            {
                return "IMAGES   only the models of each test shown for its pictures, and no test had a picture to take";
            }

            string each = TestsHidden == 0
                ? string.Empty
                : ", " + (ModelsHidden / (double)TestsHidden).ToString("0.#", CultureInfo.InvariantCulture) + " model(s) hidden on average";

            return "IMAGES   only the models of each test shown for its pictures: " + TestsHidden + " test(s) with models hidden"
                + each + ", " + TestsKeptEvery + " test(s) kept every model shown, why on the IMAGES lines above, reading the tests' sides took "
                + ReadingSeconds.ToString("0.000", CultureInfo.InvariantCulture) + "s and the hiding and the putting back took "
                + HidingSeconds.ToString("0.000", CultureInfo.InvariantCulture) + "s";
        }
    }
}
