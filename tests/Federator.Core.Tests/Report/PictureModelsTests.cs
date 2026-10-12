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
            Assert.That(rule.WhyEveryModel, Does.StartWith("1 side of the test holds no item"));
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
            tally.Hid("Walls", 9, 0.010);
            tally.SidesRead(0.040);
            tally.Hid("Floors", 8, 0.012);
            tally.SidesRead(0.003);
            tally.KeptEvery("Ramps");

            Assert.That(tally.TestsHidden, Is.EqualTo(2));
            Assert.That(tally.ModelsHidden, Is.EqualTo(17));
            Assert.That(tally.TestsKeptEvery, Is.EqualTo(1));
            Assert.That(tally.Line(true), Is.EqualTo(
                "IMAGES   only the models of each test shown for its pictures: 2 test(s) with models hidden, "
                + "8.5 model(s) hidden on average each time, 1 test(s) kept every model shown, why said above on an IMAGES or a failure line, "
                + "a repeated reason counted rather than written again, "
                + "reading the tests' sides took 0.103s and the hiding and the putting back took 0.022s"));
        }

        // F149, the second reading. The pictures are taken in stretches of one test's rows, so a test whose
        // rows sit in two stretches is hidden for twice and is still one test on the line, and the average
        // is over each time models were hidden.
        [Test]
        public void ATestHiddenForInTwoStretchesIsOneTest()
        {
            PictureModelsTally tally = new PictureModelsTally();
            tally.Hid("Walls", 9, 0.010);
            tally.Hid("Walls", 7, 0.010);
            tally.KeptEvery("Floors");
            tally.KeptEvery("Floors");

            Assert.That(tally.TestsHidden, Is.EqualTo(1));
            Assert.That(tally.ModelsHidden, Is.EqualTo(16));
            Assert.That(tally.TestsKeptEvery, Is.EqualTo(1));
            Assert.That(tally.Line(true), Does.Contain("1 test(s) with models hidden, 8 model(s) hidden on average each time"));
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

        // A clash of a status no picture is asked for takes none, so a stretch of such clashes is
        // passed over and its models are not hidden for nothing. F149 reads this row by row, the
        // rule that replaced F145's WouldTakeAPicture over a test's tally, since a stretch is passed
        // over row by row and the cap is read as its pictures are taken.
        [Test]
        public void APictureIsTakenOnlyWhereAClashIsOfAStatusAskedFor()
        {
            ImageOptions options = new ImageOptions();
            ClashReport report = new ClashReport("1C07BC", "out");
            TestReport test = report.AddTest("Walls");
            ClashRow resolved = new ClashRow();
            resolved.Status = ClashStatus.Resolved;
            ClashRow fresh = new ClashRow();
            fresh.Status = ClashStatus.New;
            test.Add(resolved);
            test.Add(fresh);

            Assert.That(options.TakesAPictureOf(report, test, resolved, null), Is.False);
            Assert.That(options.TakesAPictureOf(report, test, fresh, null), Is.True);
            Assert.That(options.TakesAPictureOf(null, test, fresh, null), Is.False);
            Assert.That(options.TakesAPictureOf(report, null, fresh, null), Is.False);
            Assert.That(options.TakesAPictureOf(report, test, null, null), Is.False);

            options.Write = false;

            Assert.That(options.TakesAPictureOf(report, test, fresh, null), Is.False);
        }

        // Each row passed over on its status or the cap is counted where it is passed over and nowhere
        // else, so the writer and the picture pass, which both read the rule, count it once.
        [Test]
        public void ARowPassedOverOnItsStatusOrTheCapIsCountedOnce()
        {
            ImageOptions options = new ImageOptions();
            options.CapPerTest = 1;
            ClashReport report = new ClashReport("1C07BC", "out");
            TestReport test = report.AddTest("Walls");
            ClashRow resolved = new ClashRow();
            resolved.Status = ClashStatus.Resolved;
            ClashRow pictured = new ClashRow();
            pictured.Status = ClashStatus.New;
            pictured.ImageFile = "cd000001.jpg";
            ClashRow pastTheCap = new ClashRow();
            pastTheCap.Status = ClashStatus.New;
            test.Add(resolved);
            test.Add(pictured);
            test.Add(pastTheCap);

            Assert.That(options.TakesAPictureOf(report, test, resolved, null), Is.False);
            Assert.That(options.TakesAPictureOf(report, test, pastTheCap, null), Is.False);

            Assert.That(report.Images.SkippedByStatus, Is.EqualTo(1));
            Assert.That(report.Images.SkippedByCap, Is.EqualTo(1));
        }

        // FR-076. Once the pictures' guard has fired no row takes a picture and none is counted, since
        // the run stops and the rest is not attempted.
        [Test]
        public void AFiredGuardTakesNoPictureAndCountsNothing()
        {
            ImageOptions options = new ImageOptions();
            ClashReport report = new ClashReport("1C07BC", "out");
            TestReport test = report.AddTest("Walls");
            ClashRow resolved = new ClashRow();
            resolved.Status = ClashStatus.Resolved;
            ClashRow fresh = new ClashRow();
            fresh.Status = ClashStatus.New;
            test.Add(resolved);
            test.Add(fresh);

            RepeatedFailureGuard guard = new RepeatedFailureGuard(2);
            guard.RecordFailure(ImageFailure.NoFileArrived);

            Assert.That(options.TakesAPictureOf(report, test, fresh, guard), Is.True, "one failure does not fire a guard of two");

            guard.RecordFailure(ImageFailure.NoFileArrived);

            Assert.That(options.TakesAPictureOf(report, test, fresh, guard), Is.False);
            Assert.That(options.TakesAPictureOf(report, test, resolved, guard), Is.False);
            Assert.That(report.Images.SkippedByStatus, Is.EqualTo(0));
            Assert.That(report.Images.SkippedByCap, Is.EqualTo(0));
        }
    }
}
