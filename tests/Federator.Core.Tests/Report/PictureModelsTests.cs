using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F145, Bader's choice of Q150: a test's pictures are taken with only the models its own sides
    /// live in shown. 1A04PK's Walls-vs-Floors lives in its AR model, one of ten, so nine are hidden.
    /// A picture never loses a model it may need, so every doubt keeps every model shown and says
    /// why, and each doubt has a test that breaks it.
    /// </summary>
    [TestFixture]
    public class PictureModelsTests
    {
        private static PictureSide Side(params int[] models)
        {
            return new PictureSide(models, models.Length == 0 ? 0 : 10, 0, 0);
        }

        [Test]
        public void OnlyTheModelsTheSidesLiveInStayShown()
        {
            PictureModels rule = PictureModels.For(10, new[] { Side(0), Side(0) });

            Assert.That(rule.HidesAny, Is.True);
            Assert.That(rule.Shown, Is.EqualTo(new[] { 0 }));
            Assert.That(rule.WhyEveryModel, Is.Null);
        }

        [Test]
        public void TwoSidesInTwoModelsShowBothOnceInTheDocumentsOrder()
        {
            PictureModels rule = PictureModels.For(10, new[] { Side(8, 0), Side(8, 3) });

            Assert.That(rule.Shown, Is.EqualTo(new[] { 0, 3, 8 }));
        }

        [Test]
        public void AnItemWithNoModelReadKeepsEveryModelShownAndSaysHowMany()
        {
            PictureModels rule = PictureModels.For(10, new[] { new PictureSide(new[] { 0 }, 10, 3, 0), Side(0) });

            Assert.That(rule.HidesAny, Is.False);
            Assert.That(rule.Shown, Is.Empty);
            Assert.That(rule.WhyEveryModel, Is.EqualTo(
                "3 items of its sides live in no model that could be read, so every model stays shown for its pictures"));
            Assert.That(PictureModels.For(10, new[] { new PictureSide(new[] { 0 }, 10, 1, 0), Side(0) }).WhyEveryModel,
                Does.StartWith("1 item of its sides lives in no model"));
        }

        [Test]
        public void AnItemInAModelWhoseFileNameIsSharedKeepsEveryModelShown()
        {
            PictureModels rule = PictureModels.For(10, new[] { new PictureSide(new[] { 0 }, 10, 0, 2), Side(1) });

            Assert.That(rule.HidesAny, Is.False);
            Assert.That(rule.WhyEveryModel, Does.StartWith(
                "2 items of its sides live in a model whose file name another model of the document shares"));
        }

        [Test]
        public void ASideNotReadKeepsEveryModelShown()
        {
            PictureModels rule = PictureModels.For(10, new[] { Side(0), PictureSide.NotRead() });

            Assert.That(rule.HidesAny, Is.False);
            Assert.That(rule.WhyEveryModel, Does.StartWith("1 side of the test could not be read"));
            Assert.That(PictureModels.For(10, new PictureSide[] { Side(0), null }).HidesAny, Is.False);
        }

        // The breaker's point: one side read with no item while the other read models would
        // otherwise show the other side's models alone.
        [Test]
        public void ASideHoldingNoItemKeepsEveryModelShown()
        {
            PictureModels rule = PictureModels.For(10, new[] { Side(0), new PictureSide(new int[0], 0, 0, 0) });

            Assert.That(rule.HidesAny, Is.False);
            Assert.That(rule.WhyEveryModel, Does.StartWith("1 side of the test hold no item"));
        }

        [Test]
        public void NoSideHandedInKeepsEveryModelShown()
        {
            Assert.That(PictureModels.For(10, null).WhyEveryModel, Does.StartWith("no side of the test was handed in"));
            Assert.That(PictureModels.For(10, new PictureSide[0]).HidesAny, Is.False);
        }

        [Test]
        public void NoModelReadKeepsEveryModelShown()
        {
            PictureModels rule = PictureModels.For(10, new[] { new PictureSide(new int[0], 5, 0, 0), new PictureSide(new int[0], 5, 0, 0) });

            Assert.That(rule.HidesAny, Is.False);
            Assert.That(rule.WhyEveryModel, Does.StartWith("no item of its sides was read to a model"));
        }

        [Test]
        public void AModelOutsideTheDocumentKeepsEveryModelShownAndNamesIt()
        {
            PictureModels rule = PictureModels.For(4, new[] { Side(0), Side(7) });

            Assert.That(rule.HidesAny, Is.False);
            Assert.That(rule.WhyEveryModel, Does.StartWith("a model read for its sides, 7, is not one of the document's 4"));
            Assert.That(PictureModels.For(4, new[] { Side(-1), Side(0) }).HidesAny, Is.False);
        }

        [Test]
        public void SidesInEveryModelHideNothing()
        {
            PictureModels rule = PictureModels.For(3, new[] { Side(2, 1), Side(0) });

            Assert.That(rule.HidesAny, Is.False);
            Assert.That(rule.WhyEveryModel, Does.StartWith("its sides live in every model of the document"));
        }

        [Test]
        public void ADocumentOfNoModelHidesNothing()
        {
            Assert.That(PictureModels.For(0, new[] { Side(0), Side(0) }).WhyEveryModel, Does.StartWith("the document holds no model"));
        }

        // ---------- the one line ----------

        [Test]
        public void TheLineCountsTheTestsTheModelsAndBothSeconds()
        {
            PictureModelsTally tally = new PictureModelsTally();
            tally.SidesRead(0.060);
            tally.Hid(9, 0.010);
            tally.SidesRead(0.040);
            tally.Hid(8, 0.012);
            tally.SidesRead(0.003);
            tally.KeptEvery();

            Assert.That(tally.TestsHidden, Is.EqualTo(2));
            Assert.That(tally.ModelsHidden, Is.EqualTo(17));
            Assert.That(tally.TestsKeptEvery, Is.EqualTo(1));
            Assert.That(tally.Line(true), Is.EqualTo(
                "IMAGES   only the models of each test shown for its pictures: 2 test(s) with models hidden, "
                + "8.5 model(s) hidden on average, 1 test(s) kept every model shown, why on the IMAGES lines above, "
                + "reading the tests' sides took 0.103s and the hiding and the putting back took 0.022s"));
        }

        [Test]
        public void TheLineSaysTheSettingWasOff()
        {
            Assert.That(new PictureModelsTally().Line(false), Is.EqualTo(
                "IMAGES   every model shown for the pictures, the setting asks for it"));
        }

        [Test]
        public void TheLineSaysNoTestHadAPicture()
        {
            Assert.That(new PictureModelsTally().Line(true), Does.EndWith("and no test had a picture to take"));
        }

        // ---------- the setting and when a picture is taken ----------

        [Test]
        public void TheSettingIsOnByDefaultAndSaysSo()
        {
            ImageOptions options = new ImageOptions();

            Assert.That(options.OnlyTheTestsModels, Is.True);
            Assert.That(options.Describe(), Does.Contain("only the models of each test shown"));

            options.OnlyTheTestsModels = false;

            Assert.That(options.Describe(), Does.Contain("every model shown"));
        }

        // A test whose clashes are all of a status no picture is asked for takes none, so its
        // models are not hidden for nothing.
        [Test]
        public void APictureIsTakenOnlyWhereAClashIsOfAStatusAskedFor()
        {
            ImageOptions options = new ImageOptions();
            ClashTally resolvedOnly = new ClashTally();
            resolvedOnly.Add(ClashStatus.Resolved, 4);
            ClashTally oneNew = new ClashTally();
            oneNew.Add(ClashStatus.Resolved, 4);
            oneNew.Add(ClashStatus.New, 1);

            Assert.That(options.WouldTakeAPicture(resolvedOnly), Is.False);
            Assert.That(options.WouldTakeAPicture(oneNew), Is.True);
            Assert.That(options.WouldTakeAPicture(new ClashTally()), Is.False);
            Assert.That(options.WouldTakeAPicture(null), Is.False);

            options.Write = false;

            Assert.That(options.WouldTakeAPicture(oneNew), Is.False);
        }
    }
}
