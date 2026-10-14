using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F172, Bader's answer to Q171. A test with more than 1000 clashes gets pictures for its first 100
    /// and the rest are skipped, per test and not for the run. The GDI objects of the process are the
    /// backstop: past 9000 no more pictures are made and the run goes on.
    /// </summary>
    [TestFixture]
    public class PictureLimitsTests
    {
        private static TestReport AddTest(ClashReport report, string name, int clashes)
        {
            TestReport test = report.AddTest(name);

            for (int i = 0; i < clashes; i++)
            {
                ClashRow row = new ClashRow();
                row.Name = "Clash" + (i + 1);
                row.Status = ClashStatus.New;
                test.Add(row);
            }

            return test;
        }

        /// <summary>The picture pass as the runner walks it: each row asked, and a picture written where it is.</summary>
        private static int Pass(ClashReport report, ImageOptions options, GdiBackstop backstop, long gdiAtTheStart, out int gdiAtTheEnd)
        {
            int made = 0;
            long gdi = gdiAtTheStart;

            foreach (TestReport test in report.Tests)
            {
                foreach (ClashRow row in test.Rows)
                {
                    if (backstop != null && backstop.Fired)
                    {
                        break;
                    }

                    if (!options.TakesAPictureOf(report, test, row, null))
                    {
                        continue;
                    }

                    if (backstop != null && backstop.Check(gdi, made + 1))
                    {
                        break;
                    }

                    row.ImageFile = "cd" + made + ".jpg";
                    made++;
                    gdi++;
                }
            }

            gdiAtTheEnd = (int)gdi;
            return made;
        }

        private static ClashReport Report()
        {
            return new ClashReport("1A04PK", "1104-PAR-1A04PK-ZZZ-BM-MOD-000001");
        }

        [Test]
        public void ATestWith1500ClashesGivesPicturesForTheFirst100()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "BIG", 1500);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(100));
            Assert.That(test.ImageCount, Is.EqualTo(100));

            for (int i = 0; i < 100; i++)
            {
                Assert.That(test.Rows[i].HasImage, Is.True, "clash " + (i + 1));
            }

            Assert.That(test.Rows[100].HasImage, Is.False);
            Assert.That(test.Rows[1499].HasImage, Is.False);
        }

        [Test]
        public void ATestWith1000ClashesOrLessGivesAllOfThem()
        {
            ClashReport report = Report();
            TestReport exactly = AddTest(report, "EXACTLY", 1000);
            TestReport small = AddTest(report, "SMALL", 40);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(1040));
            Assert.That(exactly.ImageCount, Is.EqualTo(1000));
            Assert.That(small.ImageCount, Is.EqualTo(40));
        }

        [Test]
        public void TheLimitIsPerTestSoTwoTestsOf1200ClashesGive200InTotal()
        {
            ClashReport report = Report();
            TestReport a = AddTest(report, "A", 1200);
            TestReport b = AddTest(report, "B", 1200);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(200));
            Assert.That(a.ImageCount, Is.EqualTo(100));
            Assert.That(b.ImageCount, Is.EqualTo(100));
        }

        [Test]
        public void TheBoxKeepsItsOwnJobAndTheLeastOfTheTwoHolds()
        {
            Assert.That(PictureLimits.LimitFor(1500, 0), Is.EqualTo(100));
            Assert.That(PictureLimits.LimitFor(1500, 300), Is.EqualTo(100), "the fixed rule is there whatever the box says");
            Assert.That(PictureLimits.LimitFor(1500, 50), Is.EqualTo(50));
            Assert.That(PictureLimits.LimitFor(200, 0), Is.EqualTo(int.MaxValue));
            Assert.That(PictureLimits.LimitFor(200, 30), Is.EqualTo(30));
            Assert.That(PictureLimits.LimitFor(1000, 0), Is.EqualTo(int.MaxValue));
            Assert.That(PictureLimits.LimitFor(1001, 0), Is.EqualTo(100));

            ClashReport report = Report();
            TestReport small = AddTest(report, "SMALL", 200);
            ImageOptions options = new ImageOptions { CapPerTest = 30 };
            int gdi;

            Assert.That(Pass(report, options, null, 100, out gdi), Is.EqualTo(30));
            Assert.That(small.ImageCount, Is.EqualTo(30));
        }

        [Test]
        public void TheTestLineNamesTheTestItsClashesThePicturesMadeAndTheSkipped()
        {
            string line = PictureLimits.TestLine("BLD-AR-Walls-vs-BLD-ST-Columns", 1500, 100, 1400);

            Assert.That(line, Does.StartWith("IMAGES   BLD-AR-Walls-vs-BLD-ST-Columns has 1500 clashes"));
            Assert.That(line, Does.Contain("100 pictures were made and 1400 were skipped"));
        }

        [Test]
        public void TheGdiBackstopFiresAt9000AndTheRunFinishesWithTheRestLeftEmpty()
        {
            // Many small tests, as the C04 run had, so the fixed rule does not decide it: 40 tests of 400.
            ClashReport report = Report();

            for (int t = 0; t < 40; t++)
            {
                AddTest(report, "T" + t, 400);
            }

            GdiBackstop backstop = new GdiBackstop();
            int gdiAtTheEnd;
            int made = Pass(report, new ImageOptions(), backstop, 500, out gdiAtTheEnd);

            Assert.That(backstop.Fired, Is.True);
            Assert.That(backstop.GdiObjects, Is.EqualTo(9001));
            Assert.That(made, Is.EqualTo(8501), "500 objects at the start, one a picture, and the 8502nd is the first past 9000");
            Assert.That(backstop.PictureNumber, Is.EqualTo(8502));
            Assert.That(gdiAtTheEnd, Is.LessThan(10000), "it never nears the 10000 a process may hold");
            Assert.That(backstop.Notice(), Does.Contain("picture 8502"));
            Assert.That(backstop.Notice(), Does.Contain("9001 GDI objects"));
            Assert.That(backstop.Notice(), Does.Contain("The run goes on to the NWD, the HTML, the Excel and the viewpoints"));

            int empty = 0;

            foreach (TestReport test in report.Tests)
            {
                foreach (ClashRow row in test.Rows)
                {
                    if (!row.HasImage)
                    {
                        empty++;
                    }
                }
            }

            Assert.That(empty, Is.EqualTo(16000 - made));
            Assert.That(GdiBackstop.LeftLine(empty), Does.Contain(empty + " clashes"));
        }

        [Test]
        public void TheBackstopFiresOnceAndANeverReadCountFiresNothing()
        {
            GdiBackstop backstop = new GdiBackstop();

            Assert.That(backstop.Check(-1, 1), Is.False);
            Assert.That(backstop.Check(9000, 2), Is.False, "9000 is the limit and not past it");
            Assert.That(backstop.Check(9001, 3), Is.True);
            Assert.That(backstop.Check(9500, 4), Is.False, "said once");
            Assert.That(backstop.PictureNumber, Is.EqualTo(3));
            Assert.That(backstop.GdiObjects, Is.EqualTo(9001));
        }
    }
}
