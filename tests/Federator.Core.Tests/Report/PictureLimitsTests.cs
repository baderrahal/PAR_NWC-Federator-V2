using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F172, F175 and F190, Bader's answers to Q171 and his order of 15 Oct 2026. Every test, whatever its clash
    /// count, gets pictures for at most its first 150, 100 or 50 clashes by its priority A, B or C, per test and not
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

        /// <summary>The run's settings at their defaults with the box's cap per test set.</summary>
        private static ImageOptions Box(int capPerTest)
        {
            return new ImageOptions { CapPerTest = capPerTest };
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
        public void ATestWithFewerClashesThanItsCapGetsAPictureForEach()
        {
            ClashReport report = Report();
            TestReport a120 = AddTest(report, "A-120", 120, ClashPriority.A);
            TestReport a400 = AddTest(report, "A-400", 400, ClashPriority.A);
            TestReport b90 = AddTest(report, "B-90", 90, ClashPriority.B);
            TestReport b300 = AddTest(report, "B-300", 300, ClashPriority.B);
            TestReport c80 = AddTest(report, "C-80", 80, ClashPriority.C);
            TestReport c20 = AddTest(report, "C-20", 20, ClashPriority.C);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(120 + 150 + 90 + 100 + 50 + 20));
            Assert.That(a120.ImageCount, Is.EqualTo(120));
            Assert.That(a400.ImageCount, Is.EqualTo(150));
            Assert.That(b90.ImageCount, Is.EqualTo(90));
            Assert.That(b300.ImageCount, Is.EqualTo(100));
            Assert.That(c80.ImageCount, Is.EqualTo(50));
            Assert.That(c20.ImageCount, Is.EqualTo(20));
        }

        [Test]
        public void ATestWithExactlyItsCapOfClashesGetsAllOfThemAndOneMoreGetsTheCap()
        {
            ClashReport report = Report();
            TestReport exact = AddTest(report, "EXACT-A", 150, ClashPriority.A);
            TestReport one = AddTest(report, "ONE-MORE-A", 151, ClashPriority.A);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(300));
            AssertTheFirstOnesHavePicturesAndNoOtherDoes(exact, 150);
            AssertTheFirstOnesHavePicturesAndNoOtherDoes(one, 150);
        }

        [Test]
        public void APriorityATestOf800ClashesGives150Pictures()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "EIGHT-A", 800, ClashPriority.A);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(150));
            AssertTheFirstOnesHavePicturesAndNoOtherDoes(test, 150);
        }

        [Test]
        public void APriorityBTestOf800ClashesGives100Pictures()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "EIGHT-B", 800, ClashPriority.B);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(100));
            AssertTheFirstOnesHavePicturesAndNoOtherDoes(test, 100);
        }

        [Test]
        public void APriorityCTestOf800ClashesGives50Pictures()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "EIGHT-C", 800, ClashPriority.C);
            int gdi;

            Assert.That(Pass(report, new ImageOptions(), null, 100, out gdi), Is.EqualTo(50));
            AssertTheFirstOnesHavePicturesAndNoOtherDoes(test, 50);
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
        public void TheCapsAreTheOnesBaderAnswered()
        {
            Assert.That(PictureLimits.PicturesFor(ClashPriority.A, new ImageOptions()), Is.EqualTo(150));
            Assert.That(PictureLimits.PicturesFor(ClashPriority.B, new ImageOptions()), Is.EqualTo(100));
            Assert.That(PictureLimits.PicturesFor(ClashPriority.C, new ImageOptions()), Is.EqualTo(50));
            Assert.That(PictureLimits.PicturesFor(ClashPriority.None, new ImageOptions()), Is.EqualTo(50));
            Assert.That(PictureLimits.DefaultGdiStop, Is.EqualTo(9000));
        }

        /// <summary>
        /// F227, Bader's answer to Q210. The five numbers are the run's settings on ImageOptions, his numbers of
        /// Q171 and Q175 their defaults.
        /// </summary>
        [Test]
        public void TheFiveLimitsAreSettingsOfTheRunWithBadersNumbersAsDefaults()
        {
            ImageOptions options = new ImageOptions();

            Assert.That(options.PicturesForA, Is.EqualTo(150));
            Assert.That(options.PicturesForB, Is.EqualTo(100));
            Assert.That(options.PicturesForC, Is.EqualTo(50));
            Assert.That(options.GdiStop, Is.EqualTo(9000));
            Assert.That(options.MemoryStopBytes, Is.EqualTo(12L * 1024 * 1024 * 1024));
            Assert.That(new ReportOptions().Images.PicturesForA, Is.EqualTo(PictureLimits.DefaultPicturesForA));
        }

        /// <summary>
        /// F227. Caps set on the run are the ones the pass, the limit of a test and the words follow, so no reader
        /// keeps a number of its own.
        /// </summary>
        [Test]
        public void CapsSetOnTheRunAreTheOnesThePicturesAndTheWordsFollow()
        {
            ImageOptions options = new ImageOptions { PicturesForA = 20, PicturesForB = 10, PicturesForC = 5 };

            Assert.That(PictureLimits.PicturesFor(ClashPriority.A, options), Is.EqualTo(20));
            Assert.That(PictureLimits.PicturesFor(ClashPriority.B, options), Is.EqualTo(10));
            Assert.That(PictureLimits.PicturesFor(ClashPriority.C, options), Is.EqualTo(5));
            Assert.That(PictureLimits.PicturesFor(ClashPriority.None, options), Is.EqualTo(5), "no priority is C");
            Assert.That(PictureLimits.RuleInWords(options), Does.Contain("first 20 clashes at priority A, 10 at priority B and 5 at priority C"));
            Assert.That(options.Describe(), Does.Contain("first 20 clashes at priority A"));

            ClashReport report = Report();
            TestReport a = AddTest(report, "A", 200, ClashPriority.A);
            TestReport c = AddTest(report, "C", 200, ClashPriority.C);
            int gdi;

            Assert.That(Pass(report, options, null, 100, out gdi), Is.EqualTo(25));
            Assert.That(a.ImageCount, Is.EqualTo(20));
            Assert.That(c.ImageCount, Is.EqualTo(5));
            Assert.That(options.LimitOf(a), Is.EqualTo(20));

            options.CapPerTest = 8;
            Assert.That(PictureLimits.LimitFor(ClashPriority.A, options), Is.EqualTo(8), "the box's cap still holds where it is the least");
            Assert.That(PictureLimits.LimitFor(ClashPriority.C, options), Is.EqualTo(5));

            options.PicturesForC = 0;
            Assert.That(options.LimitOf(c), Is.EqualTo(0), "nought gives a test of C no picture");
        }

        /// <summary>F227. The two stops the add-in builds off the run's settings fire past the numbers set there.</summary>
        [Test]
        public void StopsSetOnTheRunAreTheOnesTheBackstopsFireAt()
        {
            ImageOptions options = new ImageOptions { GdiStop = 600, MemoryStopBytes = 2L * 1024 * 1024 * 1024 };
            GdiBackstop gdi = new GdiBackstop(options.GdiStop);
            MemoryBackstop memory = new MemoryBackstop(options.MemoryStopBytes);

            Assert.That(gdi.Check(600, 1), Is.False);
            Assert.That(gdi.Check(601, 2), Is.True);
            Assert.That(gdi.Notice(), Does.Contain("past the 600 set").And.Contain("the limit of 10000"));
            Assert.That(memory.Check(options.MemoryStopBytes, 1), Is.False);
            Assert.That(memory.Check(options.MemoryStopBytes + 1, 2), Is.True);
            Assert.That(memory.FiredLine(), Does.Contain("past the 2.0 GB set"));
        }

        /// <summary>
        /// F227. Each of the five refuses where it is set what cannot work, and a refused number leaves the setting as
        /// it was: a cap below nought, a GDI stop at nought or at the 10000 a process may hold, and a memory stop of no
        /// bytes. A number one inside each edge is taken.
        /// </summary>
        [Test]
        public void ALimitThatCannotWorkIsRefusedWhereItIsSet()
        {
            ImageOptions options = new ImageOptions();

            Assert.Throws<ArgumentOutOfRangeException>(() => options.PicturesForA = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => options.PicturesForB = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => options.PicturesForC = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => options.GdiStop = 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => options.GdiStop = PictureLimits.ProcessGdiLimit);
            Assert.Throws<ArgumentOutOfRangeException>(() => options.MemoryStopBytes = 0);

            Assert.That(options.PicturesForA, Is.EqualTo(150));
            Assert.That(options.PicturesForB, Is.EqualTo(100));
            Assert.That(options.PicturesForC, Is.EqualTo(50));
            Assert.That(options.GdiStop, Is.EqualTo(9000));
            Assert.That(options.MemoryStopBytes, Is.EqualTo(PictureLimits.DefaultMemoryStopBytes));

            options.PicturesForA = 0;
            options.GdiStop = 1;
            options.GdiStop = PictureLimits.ProcessGdiLimit - 1;
            options.MemoryStopBytes = 1;

            Assert.That(options.PicturesForA, Is.EqualTo(0));
            Assert.That(options.GdiStop, Is.EqualTo(9999));
            Assert.That(options.MemoryStopBytes, Is.EqualTo(1));
        }

        [Test]
        public void TheBoxKeepsItsOwnJobAndTheLeastOfTheTwoHolds()
        {
            Assert.That(PictureLimits.LimitFor(ClashPriority.A, Box(0)), Is.EqualTo(150));
            Assert.That(PictureLimits.LimitFor(ClashPriority.A, Box(300)), Is.EqualTo(150), "the cap is there whatever the box says");
            Assert.That(PictureLimits.LimitFor(ClashPriority.A, Box(120)), Is.EqualTo(120));
            Assert.That(PictureLimits.LimitFor(ClashPriority.B, Box(0)), Is.EqualTo(100));
            Assert.That(PictureLimits.LimitFor(ClashPriority.C, Box(80)), Is.EqualTo(50));
            Assert.That(PictureLimits.LimitFor(ClashPriority.C, Box(20)), Is.EqualTo(20));
            Assert.That(PictureLimits.LimitFor(ClashPriority.None, Box(0)), Is.EqualTo(50), "a test with no priority is C");
            Assert.That(PictureLimits.LimitFor(ClashPriority.None, Box(30)), Is.EqualTo(30));

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
            // Many tests at the cap, as the C04 run had: 100 tests of priority A with 160 clashes, 150 pictures each.
            ClashReport report = Report();

            for (int t = 0; t < 100; t++)
            {
                AddTest(report, "T" + t, 160, ClashPriority.A);
            }

            GdiBackstop backstop = new GdiBackstop(PictureLimits.DefaultGdiStop);
            int gdiAtTheEnd;
            int made = Pass(report, new ImageOptions(), backstop, 500, out gdiAtTheEnd);

            Assert.That(backstop.Fired, Is.True);
            Assert.That(backstop.GdiObjects, Is.EqualTo(9001));
            Assert.That(made, Is.EqualTo(8501), "500 objects at the start, one a picture, and the 8502nd is the first past 9000");
            Assert.That(backstop.PictureNumber, Is.EqualTo(8502));
            Assert.That(gdiAtTheEnd, Is.LessThan(10000), "it never nears the 10000 a process may hold");
            Assert.That(backstop.Notice(), Does.Contain("picture 8502"));
            Assert.That(backstop.Notice(), Does.Contain("9001 GDI objects"));
            Assert.That(backstop.Notice(), Does.Contain("The run goes on without the rest of the pictures"));

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
            GdiBackstop backstop = new GdiBackstop(PictureLimits.DefaultGdiStop);

            Assert.That(backstop.Check(-1, 1), Is.False);
            Assert.That(backstop.Check(9000, 2), Is.False, "9000 is the limit and not past it");
            Assert.That(backstop.Check(9001, 3), Is.True);
            Assert.That(backstop.Check(9500, 4), Is.False, "said once");
            Assert.That(backstop.PictureNumber, Is.EqualTo(3));
            Assert.That(backstop.GdiObjects, Is.EqualTo(9001));
        }

        private const long Megabyte = 1024L * 1024;

        /// <summary>
        /// The picture pass with both stops, as ClashImages.Write asks them: each is read before the picture
        /// and again after it is saved, the GDI count rising one and the private bytes rising a fixed amount
        /// for every picture, as the C04 run measured it, 3.3 MB. The first stop to fire ends the pass.
        /// </summary>
        private static int PassWithBothStops(
            ClashReport report, GdiBackstop gdi, MemoryBackstop memory, long gdiAtTheStart, long bytesAtTheStart, long bytesForAPicture)
        {
            int made = 0;
            long gdiNow = gdiAtTheStart;
            long bytesNow = bytesAtTheStart;
            ImageOptions options = new ImageOptions();

            foreach (TestReport test in report.Tests)
            {
                foreach (ClashRow row in test.Rows)
                {
                    if (gdi.Fired || memory.Fired)
                    {
                        return made;
                    }

                    if (!options.TakesAPictureOf(report, test, row, null))
                    {
                        continue;
                    }

                    if (gdi.Check(gdiNow, made + 1) || memory.Check(bytesNow, made + 1))
                    {
                        return made;
                    }

                    row.ImageFile = "cd" + made + ".jpg";
                    made++;
                    gdiNow++;
                    bytesNow += bytesForAPicture;

                    gdi.Check(gdiNow, made + 1);
                    memory.Check(bytesNow, made + 1);
                }
            }

            return made;
        }

        private static ClashReport ManySmallTests()
        {
            ClashReport report = Report();

            for (int t = 0; t < 100; t++)
            {
                AddTest(report, "T" + t, 160, ClashPriority.A);
            }

            return report;
        }

        [Test]
        public void TheMemoryStopIs12GigabytesOfPrivateBytesAndReadsNothingItCouldNotRead()
        {
            Assert.That(PictureLimits.DefaultMemoryStopBytes, Is.EqualTo(12L * 1024 * 1024 * 1024));

            MemoryBackstop stop = new MemoryBackstop(PictureLimits.DefaultMemoryStopBytes);

            Assert.That(stop.Check(-1, 1), Is.False, "a count that could not be read fires nothing");
            Assert.That(stop.Check(PictureLimits.DefaultMemoryStopBytes, 2), Is.False, "12 GB is the limit and not past it");
            Assert.That(stop.Check(PictureLimits.DefaultMemoryStopBytes + 1, 3), Is.True);
            Assert.That(stop.Check(PictureLimits.DefaultMemoryStopBytes * 2, 4), Is.False, "said once");
            Assert.That(stop.PictureNumber, Is.EqualTo(3));
            Assert.That(stop.PrivateBytes, Is.EqualTo(PictureLimits.DefaultMemoryStopBytes + 1));
        }

        [Test]
        public void TheMemoryStopFiresAtPastTwelveGigabytesAndTheRunFinishesWithTheRestLeftEmpty()
        {
            // The C04 run: 0.8 GB at the start and 3.3 MB of private memory for each picture. 16000 pictures
            // are wanted, so without the stop it would reach 53 GB. The GDI count starts low, so only the
            // memory stop can fire.
            ClashReport report = ManySmallTests();
            GdiBackstop gdi = new GdiBackstop(PictureLimits.DefaultGdiStop);
            MemoryBackstop memory = new MemoryBackstop(PictureLimits.DefaultMemoryStopBytes);
            long perPicture = 3300 * 1024;

            int made = PassWithBothStops(report, gdi, memory, 500, 800 * Megabyte, perPicture);

            Assert.That(memory.Fired, Is.True);
            Assert.That(gdi.Fired, Is.False);
            Assert.That(made, Is.InRange(3400, 3600), "about 3500 pictures, as the run's own arithmetic gave");
            Assert.That(memory.PictureNumber, Is.EqualTo(made + 1), "the picture that was not made");
            Assert.That(memory.PrivateBytes, Is.GreaterThan(PictureLimits.DefaultMemoryStopBytes));
            Assert.That(memory.PrivateBytes - perPicture, Is.LessThanOrEqualTo(PictureLimits.DefaultMemoryStopBytes),
                "it fired on the first reading past the limit");

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

            string notice = memory.Notice(empty);

            Assert.That(notice, Does.StartWith("NOTICE   the pictures stopped at picture " + memory.PictureNumber));
            Assert.That(notice, Does.Contain("GB of private memory, past the 12.0 GB set"));
            Assert.That(notice, Does.Contain(empty + " pictures were skipped"));
            Assert.That(notice, Does.Contain("The run goes on without them"));
            Assert.That(memory.FiredLine(), Does.Contain("picture " + memory.PictureNumber));
        }

        [Test]
        public void TheFirstOfTheTwoStopsToFireEndsThePicturesAndTheOtherStaysQuiet()
        {
            // Memory is low and the GDI count is high: the GDI stop fires first, as on the run of 14 Oct.
            ClashReport first = ManySmallTests();
            GdiBackstop gdiFirst = new GdiBackstop(PictureLimits.DefaultGdiStop);
            MemoryBackstop memoryQuiet = new MemoryBackstop(PictureLimits.DefaultMemoryStopBytes);
            int madeByGdi = PassWithBothStops(first, gdiFirst, memoryQuiet, 8000, 800 * Megabyte, 1024);

            Assert.That(gdiFirst.Fired, Is.True);
            Assert.That(memoryQuiet.Fired, Is.False);
            Assert.That(madeByGdi, Is.EqualTo(1001), "the 1001st picture takes the count to 9001, the one that crosses the limit");

            // Memory is high and the GDI count low: the memory stop fires first.
            ClashReport second = ManySmallTests();
            GdiBackstop gdiQuiet = new GdiBackstop(PictureLimits.DefaultGdiStop);
            MemoryBackstop memoryFirst = new MemoryBackstop(PictureLimits.DefaultMemoryStopBytes);
            int madeByMemory = PassWithBothStops(second, gdiQuiet, memoryFirst, 500, 11 * 1024 * Megabyte, 3300 * 1024);

            Assert.That(memoryFirst.Fired, Is.True);
            Assert.That(gdiQuiet.Fired, Is.False);
            Assert.That(madeByMemory, Is.LessThan(400));
        }

        [Test]
        public void ATestOf400ClashesIsStillCappedByItsPriorityWhenTheMemoryStopDoesNotFire()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "MANY-A", 400, ClashPriority.A);
            GdiBackstop gdi = new GdiBackstop(PictureLimits.DefaultGdiStop);
            MemoryBackstop memory = new MemoryBackstop(PictureLimits.DefaultMemoryStopBytes);

            int made = PassWithBothStops(report, gdi, memory, 500, 800 * Megabyte, 1024);

            Assert.That(made, Is.EqualTo(150));
            Assert.That(test.ImageCount, Is.EqualTo(150));
            Assert.That(memory.Fired, Is.False);
            Assert.That(gdi.Fired, Is.False);
        }
    }
}
