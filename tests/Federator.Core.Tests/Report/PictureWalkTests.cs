using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F149. Every picture is taken once every test has run and every merge is made, in the
    /// report's order, so each is written under its final name and the renumber renames none.
    /// PictureWalk is that order and its stretches, consecutive rows of one test of the report
    /// read from one test of the document, and ClashReport.NextPictureOf is the number each
    /// picture is written under. The sample is built in a MIXED order on purpose: the test with
    /// the fewest clashes is created first, so the order the tests ran and the report's order
    /// disagree on every test, and a walk in the wrong order would rename pictures.
    /// </summary>
    [TestFixture]
    public class PictureWalkTests
    {
        private string folder;
        private string workbook;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorPictureWalk");
            workbook = Path.Combine(folder, "1104-PAR-1C07BC-ZZZ-BM-MOD-000001.xlsx");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        // ---------- the sample ----------

        private static TestReport Found(ClashReport report, string name, params string[] clashes)
        {
            TestReport test = report.AddTest(name);
            test.State = TestState.FoundClashes;

            foreach (string clash in clashes)
            {
                test.Add(Row(clash));
            }

            return test;
        }

        private static ClashRow Row(string name)
        {
            ClashRow row = new ClashRow();
            row.Name = name;
            row.Status = ClashStatus.New;
            return row;
        }

        /// <summary>Floors ran first with one clash, Walls second with three, Columns third with two.</summary>
        private static ClashReport MixedOrder()
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001");
            Found(report, "Floors", "Floors 1");
            Found(report, "Walls", "Walls 1", "Walls 2", "Walls 3");
            Found(report, "Columns", "Columns 1", "Columns 2");
            return report;
        }

        /// <summary>Every row waits for its picture, read from the test of the document its test of the report is named after.</summary>
        private static Dictionary<ClashRow, string> EachFromItsOwnTest(ClashReport report)
        {
            Dictionary<ClashRow, string> from = new Dictionary<ClashRow, string>();

            foreach (TestReport test in report.Tests)
            {
                foreach (ClashRow row in test.Rows)
                {
                    from[row] = test.Name;
                }
            }

            return from;
        }

        private static PictureWalkOutcome Walk(ClashReport report, Dictionary<ClashRow, string> from)
        {
            return PictureWalk.Of(report, row =>
            {
                string key;
                return from.TryGetValue(row, out key) ? key : null;
            });
        }

        private static IList<string> Names(IEnumerable<ClashRow> rows)
        {
            List<string> names = new List<string>();

            foreach (ClashRow row in rows)
            {
                names.Add(row.Name);
            }

            return names;
        }

        private static void AssertNames(IList<string> actual, params string[] expected)
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Length), string.Join(", ", actual));

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(actual[i], Is.EqualTo(expected[i]), "at " + i);
            }
        }

        private static void AssertStretch(PictureStretch stretch, string into, string readFrom, params string[] rows)
        {
            Assert.That(stretch.Into.Name, Is.EqualTo(into));
            Assert.That(stretch.ReadFrom, Is.EqualTo(readFrom));
            AssertNames(Names(stretch.Rows), rows);
        }

        // ---------- the order ----------

        [Test]
        public void TheTestsAreWalkedInTheReportsOrderAndNotTheOrderTheyRan()
        {
            ClashReport report = MixedOrder();
            PictureWalkOutcome walk = Walk(report, EachFromItsOwnTest(report));

            Assert.That(walk.Stretches.Count, Is.EqualTo(3));
            AssertStretch(walk.Stretches[0], "Walls", "Walls", "Walls 1", "Walls 2", "Walls 3");
            AssertStretch(walk.Stretches[1], "Columns", "Columns", "Columns 1", "Columns 2");
            AssertStretch(walk.Stretches[2], "Floors", "Floors", "Floors 1");
            Assert.That(walk.Rows, Is.EqualTo(6));
            Assert.That(walk.MetTwice.Count, Is.EqualTo(0));
        }

        // A kept test holds the clashes only its mirror found after its own, read from the mirror,
        // so its rows make one stretch for each test of the document they were read from.
        [Test]
        public void RowsOfOneTestReadFromTwoTestsMakeAStretchEach()
        {
            ClashReport report = new ClashReport("1C07BC", "out");
            TestReport walls = Found(report, "Walls", "Clash1", "Clash2");
            ClashRow mirrorOnly = Row("Clash7");
            walls.Add(mirrorOnly);

            Dictionary<ClashRow, string> from = EachFromItsOwnTest(report);
            from[mirrorOnly] = "Walls (mirror)";

            PictureWalkOutcome walk = Walk(report, from);

            Assert.That(walk.Stretches.Count, Is.EqualTo(2));
            AssertStretch(walk.Stretches[0], "Walls", "Walls", "Clash1", "Clash2");
            AssertStretch(walk.Stretches[1], "Walls", "Walls (mirror)", "Clash7");
            Assert.That(walk.Rows, Is.EqualTo(3));
        }

        [Test]
        public void RowsReadFromTwoTestsInTurnMakeAStretchForEachRun()
        {
            ClashReport report = new ClashReport("1C07BC", "out");
            Found(report, "Walls", "Clash1", "Clash2", "Clash3");

            Dictionary<ClashRow, string> from = EachFromItsOwnTest(report);
            from[report.Tests[0].Rows[1]] = "Walls (mirror)";

            PictureWalkOutcome walk = Walk(report, from);

            Assert.That(walk.Stretches.Count, Is.EqualTo(3));
            AssertStretch(walk.Stretches[0], "Walls", "Walls", "Clash1");
            AssertStretch(walk.Stretches[1], "Walls", "Walls (mirror)", "Clash2");
            AssertStretch(walk.Stretches[2], "Walls", "Walls", "Clash3");
        }

        // A row waiting for no picture, a row of a mirror taken out of the report or of a test whose
        // rows were not recorded, is passed, and the rows either side of it stay one stretch.
        [Test]
        public void ARowWaitingForNoPictureIsPassedAndEndsNoStretch()
        {
            ClashReport report = new ClashReport("1C07BC", "out");
            Found(report, "Walls", "Clash1", "Clash2", "Clash3");

            Dictionary<ClashRow, string> from = EachFromItsOwnTest(report);
            from.Remove(report.Tests[0].Rows[1]);

            PictureWalkOutcome walk = Walk(report, from);

            Assert.That(walk.Stretches.Count, Is.EqualTo(1));
            AssertStretch(walk.Stretches[0], "Walls", "Walls", "Clash1", "Clash3");
            Assert.That(walk.Rows, Is.EqualTo(2));
        }

        // The break, the breaker's sixth point on attempt 1. One row under two tests of the report,
        // which only a merge that threw between two mirrors of one kept test could leave, is walked
        // once, under the test met first, and named, so its picture is taken once.
        [Test]
        public void TheSameRowUnderTwoTestsIsWalkedOnceAndNamed()
        {
            ClashReport report = MixedOrder();
            Dictionary<ClashRow, string> from = EachFromItsOwnTest(report);
            ClashRow twice = report.Tests[1].Rows[0];
            report.Tests[2].Add(twice);

            PictureWalkOutcome walk = Walk(report, from);

            Assert.That(walk.Stretches.Count, Is.EqualTo(3));
            AssertStretch(walk.Stretches[0], "Walls", "Walls", "Walls 1", "Walls 2", "Walls 3");
            AssertStretch(walk.Stretches[1], "Columns", "Columns", "Columns 1", "Columns 2");
            AssertStretch(walk.Stretches[2], "Floors", "Floors", "Floors 1");
            Assert.That(walk.Rows, Is.EqualTo(6), "the row is counted once");
            Assert.That(walk.MetTwice.Count, Is.EqualTo(1));
            Assert.That(walk.MetTwice[0], Does.Contain("Walls 1"));
            Assert.That(walk.MetTwice[0], Does.Contain("Walls"));
            Assert.That(walk.MetTwice[0], Does.Contain("Columns"));
        }

        [Test]
        public void AReportWithNothingWaitingHasNoStretch()
        {
            PictureWalkOutcome walk = Walk(MixedOrder(), new Dictionary<ClashRow, string>());

            Assert.That(walk.Stretches.Count, Is.EqualTo(0));
            Assert.That(walk.Rows, Is.EqualTo(0));
            Assert.That(walk.MetTwice.Count, Is.EqualTo(0));
        }

        [Test]
        public void NoReportOrNoReaderIsRefused()
        {
            Assert.That(() => PictureWalk.Of(null, row => "Walls"), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => PictureWalk.Of(MixedOrder(), null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void TheStretchesHandedOutCannotBeAddedTo()
        {
            ClashReport report = MixedOrder();
            PictureWalkOutcome walk = Walk(report, EachFromItsOwnTest(report));

            Assert.That(walk.Stretches.IsReadOnly, Is.True);
            Assert.That(walk.Stretches[0].Rows.IsReadOnly, Is.True);
            Assert.That(walk.MetTwice.IsReadOnly, Is.True);
        }

        // ---------- the numbers, the reviewer's fourth point on attempt 1 ----------

        /// <summary>
        /// Takes each picture the way the add-in's writer does, through the Core it reads: the
        /// choice, the number, the index handed back where a test's first render fails, and the
        /// row pointed at its file. A picture is a file holding its row's name, so a picture
        /// under another row's number reads as a wrong name.
        /// </summary>
        private void Take(ClashReport report, ImageOptions options, TestReport into, ClashRow row, ICollection<string> renderFails)
        {
            if (!options.TakesAPictureOf(report, into, row, null))
            {
                return;
            }

            PictureNumber number = report.NextPictureOf(into, row);

            if (renderFails.Contains(row.Name))
            {
                report.ReleaseImageIndex(into);
                return;
            }

            string path = ImageNaming.PathFor(workbook, number.TestIndex, number.ClashIndex);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, row.Name);
            number.PointTheRow(workbook);
        }

        /// <summary>
        /// Floors, Walls, Columns and Doors in the order they ran, one, three, two and two clashes,
        /// so the report's order is Walls, Columns, Doors, Floors. A cap of two passes Walls 3 over,
        /// Columns 1 is Resolved and passed over on its status, and the first render of Doors fails,
        /// so Doors hands its number back and takes it again at its second clash.
        /// </summary>
        private static ClashReport EveryKindOfPassOver()
        {
            ClashReport report = MixedOrder();
            Found(report, "Doors", "Doors 1", "Doors 2");
            report.Tests[2].Rows[0].Status = ClashStatus.Resolved;
            return report;
        }

        private static ImageOptions CapOfTwo()
        {
            ImageOptions options = new ImageOptions();
            options.CapPerTest = 2;
            return options;
        }

        [Test]
        public void TakenInTheWalksOrderEveryPictureIsWrittenUnderItsFinalName()
        {
            ClashReport report = EveryKindOfPassOver();
            ImageOptions options = CapOfTwo();
            PictureWalkOutcome walk = Walk(report, EachFromItsOwnTest(report));

            foreach (PictureStretch stretch in walk.Stretches)
            {
                foreach (ClashRow row in stretch.Rows)
                {
                    Take(report, options, stretch.Into, row, new[] { "Doors 1" });
                }
            }

            ImageRenumberingOutcome renumbered = ImageRenumbering.Apply(report, workbook);

            Assert.That(renumbered.Renamed, Is.EqualTo(0), "no picture is renamed");
            Assert.That(renumbered.Unchanged, Is.EqualTo(5));
            Assert.That(renumbered.Missing, Is.EqualTo(0));
            Assert.That(renumbered.Lines()[0],
                Is.EqualTo("IMAGES   numbered in report order: 0 renamed, 5 already right, 0 missing"));

            string[] pictured = { "Walls 1", "Walls 2", "Columns 2", "Doors 2", "Floors 1" };
            string[] named = { "cd000001.jpg", "cd000002.jpg", "cd010001.jpg", "cd020001.jpg", "cd030001.jpg" };
            List<ClashRow> withPictures = new List<ClashRow>();

            foreach (ClashRow row in ReportOrder.Rows(report))
            {
                if (row.HasImage)
                {
                    withPictures.Add(row);
                }
            }

            AssertNames(Names(withPictures), pictured);

            for (int i = 0; i < named.Length; i++)
            {
                Assert.That(withPictures[i].ImageFile, Is.EqualTo(named[i]), withPictures[i].Name);
                Assert.That(File.ReadAllText(withPictures[i].ImagePath), Is.EqualTo(withPictures[i].Name),
                    "the picture under " + named[i] + " is its own row's");
            }

            Assert.That(report.Images.SkippedByStatus, Is.EqualTo(1), "Columns 1 is Resolved");
            Assert.That(report.Images.SkippedByCap, Is.EqualTo(1), "Walls 3 is past the cap of two");
        }

        // The break. The same pictures taken in the order the tests ran, which is the order they
        // were taken in before F149, are under run order numbers, and the renumber renames them.
        [Test]
        public void TakenInTheOrderTheTestsRanThePicturesAreRenamed()
        {
            ClashReport report = EveryKindOfPassOver();
            ImageOptions options = CapOfTwo();

            foreach (TestReport test in report.Tests)
            {
                foreach (ClashRow row in test.Rows)
                {
                    Take(report, options, test, row, new[] { "Doors 1" });
                }
            }

            ImageRenumberingOutcome renumbered = ImageRenumbering.Apply(report, workbook);

            Assert.That(renumbered.Renamed, Is.GreaterThan(0), "a walk in the order the tests ran leaves pictures to rename");
            Assert.That(renumbered.Missing, Is.EqualTo(0));
        }
    }
}
