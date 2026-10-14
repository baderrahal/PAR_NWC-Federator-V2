using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F172 and F175, Bader's answers to Q171. A test with more than 1000 clashes gets pictures for its
    /// first 150, 100 or 50 clashes by its priority A, B or C, and the rest are skipped, per test and not
    /// for the run. A test the matrix does not name is C. The GDI objects of the process are the
    /// backstop: past 9000 no more pictures are made and the run goes on.
    /// </summary>
    [TestFixture]
    public class PictureLimitsTests
    {
        private static TestReport AddTest(ClashReport report, string name, int clashes, ClashPriority priority)
        {
            TestReport test = report.AddTest(name);
            test.PicturePriority = priority;

            for (int i = 0; i < clashes; i++)
            {
                ClashRow row = new ClashRow();
                row.Name = "Clash" + (i + 1);
                row.Status = ClashStatus.New;
                test.Add(row);
            }

            return test;
        }

        private static TestReport AddTest(ClashReport report, string name, int clashes)
        {
            return AddTest(report, name, clashes, ClashPriority.C);
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

        private static void AssertTheFirstOnesHavePicturesAndNoOtherDoes(TestReport test, int expected)
        {
            Assert.That(test.ImageCount, Is.EqualTo(expected));

            for (int i = 0; i < test.Rows.Count; i++)
            {
                Assert.That(test.Rows[i].HasImage, Is.EqualTo(i < expected), "clash " + (i + 1));
            }
        }

        [Test]
        public void APriorityATestOf1500ClashesGives150Pictures()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "BIG-A", 1500, ClashPriority.A);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(150));
            AssertTheFirstOnesHavePicturesAndNoOtherDoes(test, 150);
        }

        [Test]
        public void APriorityBTestOf1500ClashesGives100Pictures()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "BIG-B", 1500, ClashPriority.B);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(100));
            AssertTheFirstOnesHavePicturesAndNoOtherDoes(test, 100);
        }

        [Test]
        public void APriorityCTestOf1500ClashesGives50Pictures()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "BIG-C", 1500, ClashPriority.C);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(50));
            AssertTheFirstOnesHavePicturesAndNoOtherDoes(test, 50);
        }

        [Test]
        public void ASavedTestTheMatrixDoesNotNameCountsAsCAndGives50Pictures()
        {
            // The priority the runner puts on the test is the map's answer for a saved test, F168.
            PriorityMap map = PriorityMap.Read(
                "test_name,left_set,right_set,priority\nBLD-AR-Walls-vs-BLD-ST-Columns,a,b,A\n", "built in");
            Assert.That(map.Names("MY-HAND-MADE-TEST"), Is.False);

            ClashReport report = Report();
            TestReport saved = AddTest(report, "MY-HAND-MADE-TEST", 1500, map.OfASavedTest("MY-HAND-MADE-TEST"));
            TestReport named = AddTest(report, "BLD-AR-Walls-vs-BLD-ST-Columns", 1500, map.OfASavedTest("BLD-AR-Walls-vs-BLD-ST-Columns"));
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(200));
            Assert.That(saved.ImageCount, Is.EqualTo(50));
            Assert.That(named.ImageCount, Is.EqualTo(150));
        }

        [Test]
        public void ATestWithNoPriorityReadAtAllIsCappedAsCAndNeverAsTheMost()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "NO-PRIORITY", 1500, ClashPriority.None);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(50));
            Assert.That(test.ImageCount, Is.EqualTo(50));
            Assert.That(PictureLimits.Letter(ClashPriority.None), Is.EqualTo("C"));
        }

        [Test]
        public void ATestWith1000ClashesOrLessGivesAllOfThemWhateverItsPriority()
        {
            ClashReport report = Report();
            TestReport a = AddTest(report, "EXACTLY-A", 1000, ClashPriority.A);
            TestReport b = AddTest(report, "EXACTLY-B", 1000, ClashPriority.B);
            TestReport c = AddTest(report, "EXACTLY-C", 1000, ClashPriority.C);
            TestReport small = AddTest(report, "SMALL", 40, ClashPriority.C);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(3040));
            Assert.That(a.ImageCount, Is.EqualTo(1000));
            Assert.That(b.ImageCount, Is.EqualTo(1000));
            Assert.That(c.ImageCount, Is.EqualTo(1000));
            Assert.That(small.ImageCount, Is.EqualTo(40));
        }

        [Test]
        public void TheLimitIsPerTestSoThreeTestsOf1200ClashesGive150And100And50()
        {
            ClashReport report = Report();
            TestReport a = AddTest(report, "A", 1200, ClashPriority.A);
            TestReport b = AddTest(report, "B", 1200, ClashPriority.B);
            TestReport c = AddTest(report, "C", 1200, ClashPriority.C);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(300));
            Assert.That(a.ImageCount, Is.EqualTo(150));
            Assert.That(b.ImageCount, Is.EqualTo(100));
            Assert.That(c.ImageCount, Is.EqualTo(50));
        }

        [Test]
        public void TheCapsAndTheTriggerAreTheOnesBaderAnswered()
        {
            Assert.That(PictureLimits.ManyClashes, Is.EqualTo(1000));
            Assert.That(PictureLimits.PicturesFor(ClashPriority.A), Is.EqualTo(150));
            Assert.That(PictureLimits.PicturesFor(ClashPriority.B), Is.EqualTo(100));
            Assert.That(PictureLimits.PicturesFor(ClashPriority.C), Is.EqualTo(50));
            Assert.That(PictureLimits.PicturesFor(ClashPriority.None), Is.EqualTo(50));
            Assert.That(PictureLimits.GdiStop, Is.EqualTo(9000));
        }

        [Test]
        public void TheBoxKeepsItsOwnJobAndTheLeastOfTheTwoHolds()
        {
            Assert.That(PictureLimits.LimitFor(1500, ClashPriority.A, 0), Is.EqualTo(150));
            Assert.That(PictureLimits.LimitFor(1500, ClashPriority.A, 300), Is.EqualTo(150), "the rule is there whatever the box says");
            Assert.That(PictureLimits.LimitFor(1500, ClashPriority.A, 120), Is.EqualTo(120));
            Assert.That(PictureLimits.LimitFor(1500, ClashPriority.B, 0), Is.EqualTo(100));
            Assert.That(PictureLimits.LimitFor(1500, ClashPriority.C, 80), Is.EqualTo(50));
            Assert.That(PictureLimits.LimitFor(1500, ClashPriority.C, 20), Is.EqualTo(20));
            Assert.That(PictureLimits.LimitFor(200, ClashPriority.A, 0), Is.EqualTo(int.MaxValue));
            Assert.That(PictureLimits.LimitFor(200, ClashPriority.C, 30), Is.EqualTo(30));
            Assert.That(PictureLimits.LimitFor(1000, ClashPriority.C, 0), Is.EqualTo(int.MaxValue));
            Assert.That(PictureLimits.LimitFor(1001, ClashPriority.C, 0), Is.EqualTo(50));

            ClashReport report = Report();
            TestReport small = AddTest(report, "SMALL", 200, ClashPriority.A);
            ImageOptions options = new ImageOptions { CapPerTest = 30 };
            int gdi;

            Assert.That(Pass(report, options, null, 100, out gdi), Is.EqualTo(30));
            Assert.That(small.ImageCount, Is.EqualTo(30));
        }

        [Test]
        public void TheTestLineNamesTheTestItsPriorityItsClashesThePicturesMadeAndTheSkipped()
        {
            string line = PictureLimits.TestLine("BLD-AR-Walls-vs-BLD-ST-Columns", ClashPriority.B, 1500, 100, 1400);

            Assert.That(line, Does.StartWith("IMAGES   BLD-AR-Walls-vs-BLD-ST-Columns has priority B and 1500 clashes"));
            Assert.That(line, Does.Contain("100 pictures were made and 1400 were skipped"));
            Assert.That(PictureLimits.TestLine("T", ClashPriority.A, 1200, 150, 1050), Does.Contain("has priority A and 1200 clashes"));
            Assert.That(PictureLimits.TestLine("T", ClashPriority.None, 1200, 50, 1150), Does.Contain("has priority C and 1200 clashes"));
        }

        [Test]
        public void TheGdiBackstopFiresAt9000AndTheRunFinishesWithTheRestLeftEmpty()
        {
            // Many small tests, as the C04 run had, so the priority rule does not decide it: 40 tests of 400.
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
