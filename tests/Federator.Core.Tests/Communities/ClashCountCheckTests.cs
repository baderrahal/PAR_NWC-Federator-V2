using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Federator.Core.Clash;
using Federator.Core.Communities;
using Federator.Core.Coverage;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F236, Bader's round of 19 Oct 2026, fix 6, the clash count check: each number read off the files a group wrote, its
    /// .tsv, its clash workbook and its HTML page, the four counts set side by side, a red row where they differ, a count not
    /// read as UNKNOWN, the mirrors, the totals, and the groups not built or not read with the tests the two rules left out.
    /// Each part is broken one way and asserted to name it. The .tsv rows are made by EventRow and the coverage rows by
    /// CoverageBlock.RowText, the shapes the run writes.
    /// </summary>
    [TestFixture]
    public class ClashCountCheckTests
    {
        private const string Walls = "Walls-vs-Floors";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("clashcountcheck");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static string Row(string kind, string name, string number, string text)
        {
            return new EventRow("04:00:00.000", 1, "C06  1B06PK", string.Empty, kind, name, number, text).Line();
        }

        private static List<string> Tsv(params string[] rows)
        {
            List<string> lines = new List<string> { EventRow.Header() };
            lines.AddRange(rows);
            return lines;
        }

        private static string ViewLine(string test, int open, string state = "read back")
        {
            return "      " + test + "  " + open + " open clashes, shows AR ST, hides nothing, " + state;
        }

        private static string Coverage(string name, CoverageReason reason, bool ran)
        {
            return CoverageBlock.RowText(new TestCoverage(1, name, string.Empty, string.Empty, 2, 2, TestPresence.CreatedThisRun,
                ran, ran ? 1 : -1, reason, "both sides carry AR, see, the detail"));
        }

        // ---------- the views line ----------

        [Test]
        public void AViewsLineIsReadBackToItsTestItsOpenClashesAndWhetherItWasReadBackAndAFolderLineIsNot()
        {
            string test;
            int open;
            bool readBack;

            Assert.That(ViewsTree.ReadViewLine("      BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Columns  211 open clashes, shows ME ST, hides nothing, read back", out test, out open, out readBack), Is.True);
            Assert.That(test, Is.EqualTo("BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Columns"));
            Assert.That(open, Is.EqualTo(211));
            Assert.That(readBack, Is.True);
            Assert.That(ViewsTree.ReadViewLine("        BLD-ME-Ducts-vs-BLD-ST-Floors  7 open clashes, shows ME ST, hides nothing, NOT READ BACK", out test, out open, out readBack), Is.True);
            Assert.That(readBack, Is.False);
            Assert.That(ViewsTree.ReadViewLine("Clash Status  15 views, 904 open clashes", out test, out open, out readBack), Is.False);
            Assert.That(ViewsTree.ReadViewLine(null, out test, out open, out readBack), Is.False);
        }

        // ---------- the group's .tsv ----------

        [Test]
        public void TheTsvGivesClashDetectivesCountsTheFilesWrittenTheSheetTheViewsAndTheTestsTheRulesLeftOut()
        {
            GroupRunFiles read = GroupRunFiles.Read(Tsv(
                Row(RunEvents.TestFoundClashes, Walls, "5", "ran and found clashes"),
                Row(RunEvents.TestPassed, "Doors-vs-Walls", "0", "ran and found nothing"),
                Row(RunEvents.Written, RunEvents.WorkbookKind, "1000", @"C:\R\C06\Log\a" + "\t" + "b.xlsx"),
                Row(RunEvents.WorkbookSheet, RunEvents.WorkbookKind, string.Empty, "1104-PAR-1B06PK-ZZZ-BM-MOD-00"),
                Row(RunEvents.Written, RunEvents.PageKind, "1000", @"C:\R\C06\Clash report\a.html"),
                Row(RunEvents.ViewsTree, "1B06PK", "8", ViewLine(Walls, 3)),
                Row(RunEvents.ViewsTree, "1B06PK", "9", "  " + ViewLine(Walls, 1)),
                Row(RunEvents.CoverageTest, "AR-vs-AR", string.Empty, Coverage("AR-vs-AR", CoverageReason.SameSetCode, false)),
                Row(RunEvents.CoverageTest, Walls, "5", Coverage(Walls, CoverageReason.HasClashes, true))));

            Assert.That(read.Found[Walls], Is.EqualTo(5));
            Assert.That(read.Found["Doors-vs-Walls"], Is.EqualTo(0), "a test that ran and passed is a nought of Clash Detective, not UNKNOWN");
            Assert.That(read.Workbook, Is.EqualTo(@"C:\R\C06\Log\a" + "\t" + "b.xlsx"), "a path's backslashes and a tab read back whole");
            Assert.That(read.WorkbookSheet, Is.EqualTo("1104-PAR-1B06PK-ZZZ-BM-MOD-00"));
            Assert.That(read.Page, Is.EqualTo(@"C:\R\C06\Clash report\a.html"));
            Assert.That(read.ViewsOf(Walls), Is.EqualTo(4), "the views of one test added up, its size folder among them");
            Assert.That(read.ViewsOf("Doors-vs-Walls"), Is.EqualTo(0), "a test with no view where the block was written");
            Assert.That(read.LeftOutBySetCodes, Is.EqualTo(1));
            Assert.That(read.LeftOutByOneDiscipline, Is.EqualTo(0));
            Assert.That(GroupRunFiles.Read(null).Found, Is.Empty);
        }

        [Test]
        public void ARowOfAReasonInItsDetailAloneIsNotTheReason()
        {
            string row = CoverageBlock.RowText(new TestCoverage(1, Walls, string.Empty, string.Empty, 2, 2, TestPresence.CreatedThisRun,
                true, 1, CoverageReason.HasClashes, "an earlier note, " + CoverageWords.For(CoverageReason.SameSetCode)));

            Assert.That(CoverageBlock.RowGives(row, CoverageReason.HasClashes), Is.True);
            Assert.That(CoverageBlock.RowGives(row, CoverageReason.SameSetCode), Is.False);
        }

        [Test]
        public void TheViewsAreUnknownWhereNoBlockWasWrittenOrAViewOfTheTestWasNotReadBack()
        {
            GroupRunFiles none = GroupRunFiles.Read(Tsv(Row(RunEvents.TestFoundClashes, Walls, "5", string.Empty)));
            GroupRunFiles notBack = GroupRunFiles.Read(Tsv(
                Row(RunEvents.ViewsTree, "1B06PK", "1", ViewLine(Walls, 3)),
                Row(RunEvents.ViewsTree, "1B06PK", "2", ViewLine(Walls, 1, "NOT READ BACK")),
                Row(RunEvents.ViewsTree, "1B06PK", "3", ViewLine("Doors-vs-Walls", 2))));

            Assert.That(none.ViewsTreeWritten, Is.False);
            Assert.That(none.ViewsOf(Walls), Is.EqualTo(ClashCountRow.Unknown), "the views not asked for are never a nought");
            Assert.That(notBack.ViewsOf(Walls), Is.EqualTo(ClashCountRow.Unknown), "a view that failed never adds its clashes as agreeing");
            Assert.That(notBack.ViewsOf("Doors-vs-Walls"), Is.EqualTo(2));
        }

        // ---------- the page ----------

        private static string Block(string name, int rows)
        {
            string text = "<td rowspan=\"2\" colspan=\"2\" class=\"testName\">" + name.Replace("&", "&amp;") + "</td>\n"
                + "<tr class=\"contentRow\"><td class=\"contentCell\">0.025m</td><td class=\"contentCell\">" + rows + "</td></tr></table>"
                + "<table class=\"mainTable\"><tr class=\"headerRow\"></tr>";

            for (int i = 0; i < rows; i++)
            {
                text += "<tr class=\"contentRow\"><td colspan=\"2\" class=\"contentCell\"></td></tr>";
            }

            return text + "</table>";
        }

        [Test]
        public void ThePageIsReadTestByTestItsClashRowsAlone()
        {
            PageTests read = PageTests.Read("<html>" + Block("Ducts&Duct Fittings-vs-Walls", 3) + Block("None-vs-None", 0) + "</html>");

            Assert.That(read.Named("Ducts&Duct Fittings-vs-Walls").Single().Rows, Is.EqualTo(3), "the row of the summary table is not a clash row");
            Assert.That(read.Named("None-vs-None").Single().Rows, Is.EqualTo(0));
            Assert.That(PageTests.Read(null).Named(Walls), Is.Empty);
        }

        /// <summary>
        /// The client's own page, Navisworks' stylesheet's shape, read back: the rows of the test the page holds most clashes
        /// for, and of one holding none. The counts are the sample's, measured off it by a regular expression on 19 Oct 2026:
        /// 1830 tests and 64 clash rows, the 64 pictures its workbook links.
        /// </summary>
        [Test]
        public void TheClientsOwnPageIsReadInTheShapeNavisworksWritesIt()
        {
            PageTests read = PageTests.Read(File.ReadAllText(Path.Combine(Samples.Folder(), "client-report", "1104-PAR-1A02WN-XXX-BM-RPT-000001.html")));

            Assert.That(read.Named("BLD-EL-Conduits & Conduit Fittings-vs-BLD-ST-Columns").Single().Rows, Is.EqualTo(12), "a name with an ampersand read back to its words");
            Assert.That(read.Named("BLD-AR-Windows-vs-BLD-AR-Walls").Single().Rows, Is.EqualTo(6));
            Assert.That(read.Named("no such test"), Is.Empty);
        }

        // ---------- the workbook ----------

        /// <summary>A row with its picture, as the writer reads one: a file and the link its Image cell carries.</summary>
        private static void Picture(ClashRow row)
        {
            row.ImageFile = "cd000001.jpg";
            row.ImageLink = "x_files/" + row.ImageFile;
        }

        private string Workbook(Action<TestReport> walls)
        {
            ClashReport report = new ClashReport("1B06PK", "1104-PAR-1B06PK-ZZZ-BM-MOD-000001");
            report.SetTreeRoot = "lcop_selection_set_tree";
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 10, 19, 4, 0, 0);
            report.Priorities = PriorityMap.Read("test_name,left_set,right_set,priority\nWalls-vs-Floors,a,b,A\n", "map.csv");
            TestReport test = WorkbookTestsTests.AddTest(report, Walls, 5, 0);
            test.Priority = ClashPriority.A;
            walls(test);
            return WorkbookTestsTests.Write(report, folder);
        }

        [Test]
        public void TheWorkbookIsReadWithEachRowsStatusItsPictureAndItsPriority()
        {
            string path = Workbook(test =>
            {
                test.Rows[1].Status = ClashStatus.Reviewed;
                test.Rows[4].Status = ClashStatus.Resolved;
                Picture(test.Rows[0]);
                Picture(test.Rows[2]);
            });

            WorkbookTest read = WorkbookTestsTests.ReadBack(path).Named(Walls).Single();

            Assert.That(read.Rows, Is.EqualTo(5));
            Assert.That(read.Pictures, Is.EqualTo(2));
            Assert.That(read.RowsAt("New"), Is.EqualTo(3));
            Assert.That(read.RowsAt("Reviewed"), Is.EqualTo(1));
            Assert.That(read.RowsAt("Resolved"), Is.EqualTo(1));
            Assert.That(read.RowsAt("Approved"), Is.EqualTo(0));
            Assert.That(read.Priority, Is.EqualTo("A"));
        }

        // ---------- the check ----------

        private static GroupRunFiles Run(int found, int views, params string[] more)
        {
            List<string> rows = new List<string>
            {
                Row(RunEvents.TestFoundClashes, Walls, found.ToString(), "ran and found clashes"),
                Row(RunEvents.ViewsTree, "1B06PK", "1", ViewLine(Walls, views))
            };
            rows.AddRange(more);
            return GroupRunFiles.Read(Tsv(rows.ToArray()));
        }

        private ClashCountRow OneRow(GroupRunFiles run, WorkbookTests workbook, PageTests page, ClashCountCheck check = null)
        {
            return (check ?? new ClashCountCheck("C06")).AddGroup("1B06PK", run, workbook, page, null, ViewpointSettings.DefaultViewStatuses).Rows.Single();
        }

        [Test]
        public void TheFourCountsAgreeWhereTheViewsHoldTheOpenClashesAloneAndAReviewedClashIsInNoView()
        {
            string path = Workbook(test =>
            {
                test.Rows[1].Status = ClashStatus.Reviewed;
                test.Rows[4].Status = ClashStatus.Resolved;
            });
            ClashCountCheck check = new ClashCountCheck("C06");

            ClashCountRow row = OneRow(Run(5, 3), WorkbookTestsTests.ReadBack(path), PageTests.Read(Block(Walls, 5)), check);

            Assert.That(row.Found, Is.EqualTo(5));
            Assert.That(row.ExcelRows, Is.EqualTo(5));
            Assert.That(row.HtmlRows, Is.EqualTo(5));
            Assert.That(row.Views, Is.EqualTo(3));
            Assert.That(row.ViewsExpected, Is.EqualTo(3), "New and Active, the statuses a view shows");
            Assert.That(row.Differs, Is.False);
            Assert.That(row.HasUnknown, Is.False);
            Assert.That(row.Priority, Is.EqualTo("A"));
            Assert.That(check.Lines().Last(), Is.EqualTo("COUNTS   C06  1 group, 1 test with clashes, 0 red, the four counts differing, 0 with a count UNKNOWN, 0 not built for one discipline, 0 not read"));
        }

        [Test]
        public void TheViewsAreHeldToTheRunsViewStatuses()
        {
            string path = Workbook(test => test.Rows[1].Status = ClashStatus.Reviewed);

            ClashCountRow row = new ClashCountCheck("C06").AddGroup("1B06PK", Run(5, 5), WorkbookTestsTests.ReadBack(path),
                PageTests.Read(Block(Walls, 5)), null, OpenClashCount.NavisworksOpen).Rows.Single();

            Assert.That(row.ViewsExpected, Is.EqualTo(5), "New, Active and Reviewed under the other setting");
            Assert.That(row.Differs, Is.False);
        }

        [Test]
        public void EachOfTheFourCountsThatDiffersMakesTheRowRedAndALogLine()
        {
            WorkbookTests workbook = WorkbookTestsTests.ReadBack(Workbook(test => { }));

            ClashCountRow cd = OneRow(Run(6, 5), workbook, PageTests.Read(Block(Walls, 5)));
            ClashCountRow html = OneRow(Run(5, 5), workbook, PageTests.Read(Block(Walls, 4)));
            ClashCountCheck views = new ClashCountCheck("C06");
            ClashCountRow view = OneRow(Run(5, 4), workbook, PageTests.Read(Block(Walls, 5)), views);

            Assert.That(cd.Differs, Is.True, "Clash Detective against the Excel rows");
            Assert.That(html.Differs, Is.True, "the HTML rows");
            Assert.That(view.Differs, Is.True, "the views against the open clashes");
            Assert.That(views.Lines().First(), Is.EqualTo("COUNTS   DIFFER  C06  1B06PK  Walls-vs-Floors  Clash Detective 5, Excel rows 5, HTML rows 5, viewpoints 4 of 5 open"));
        }

        [Test]
        public void ACountNotReadIsUnknownNeverANoughtNeverRedAndNamedOnALine()
        {
            ClashCountCheck check = new ClashCountCheck("C06");
            ClashCountRow row = OneRow(GroupRunFiles.Read(Tsv(Row(RunEvents.TestFoundClashes, Walls, "5", string.Empty))), null, null, check);

            Assert.That(row.ExcelRows, Is.EqualTo(ClashCountRow.Unknown), "no workbook read");
            Assert.That(row.HtmlRows, Is.EqualTo(ClashCountRow.Unknown), "no page read");
            Assert.That(row.Views, Is.EqualTo(ClashCountRow.Unknown), "no views tree written");
            Assert.That(row.At(ClashStatus.New), Is.EqualTo(ClashCountRow.Unknown));
            Assert.That(row.Made, Is.EqualTo(ClashCountRow.Unknown));
            Assert.That(row.Differs, Is.False, "nothing read to judge against");
            Assert.That(row.HasUnknown, Is.True);
            Assert.That(row.Cells()[4], Is.EqualTo(ClashCountRow.UnknownWord));
            Assert.That(row.WhySkipped, Is.EqualTo("the workbook was not read"));
            Assert.That(check.Lines(), Has.Member("COUNTS   UNKNOWN  C06  1B06PK  Walls-vs-Floors  not judged on Excel rows, HTML rows, Clashes in viewpoints, which could not be read"));
        }

        [Test]
        public void ATestTheWorkbookReadHoldsNoBlockOfIsANoughtAndRed()
        {
            WorkbookTests other = WorkbookTestsTests.ReadBack(Workbook(test => { }));
            GroupRunFiles run = Run(5, 5, Row(RunEvents.TestFoundClashes, "Doors-vs-Walls", "2", string.Empty));

            ClashCountRow row = new ClashCountCheck("C06").AddGroup("1B06PK", run, other, PageTests.Read(Block(Walls, 5)), null,
                ViewpointSettings.DefaultViewStatuses).Rows.Single(r => r.Test == "Doors-vs-Walls");

            Assert.That(row.ExcelRows, Is.EqualTo(0));
            Assert.That(row.WhySkipped, Is.EqualTo("the workbook holds no block of this test"));
            Assert.That(row.Differs, Is.True);
        }

        [Test]
        public void AMergedMirrorIsNoRowAndItsKeptTestIsHeldToItsCountAndTheClashesOnlyItsMirrorsFound()
        {
            WorkbookTests workbook = WorkbookTestsTests.ReadBack(Workbook(test => { }));
            ClashCountCheck check = new ClashCountCheck("C06");
            GroupRunFiles run = Run(3, 5,
                Row(RunEvents.TestFoundClashes, "Floors-vs-Walls (mirror)", "4", string.Empty),
                Row(RunEvents.Mirror, "Floors-vs-Walls (mirror)", string.Empty, Walls),
                Row(RunEvents.MirrorMerged, "Floors-vs-Walls (mirror)", string.Empty, Walls),
                Row(RunEvents.AddedByMirrors, Walls, "2", string.Empty));

            IList<ClashCountRow> rows = check.AddGroup("1B06PK", run, workbook, PageTests.Read(Block(Walls, 5)), null, ViewpointSettings.DefaultViewStatuses).Rows;

            Assert.That(rows.Count, Is.EqualTo(1), "the mirror's clashes are the kept test's");
            Assert.That(rows[0].RowsExpected, Is.EqualTo(5));
            Assert.That(rows[0].Differs, Is.False);
            Assert.That(check.Lines(), Has.Some.StartsWith("COUNTS   MIRRORS  C06  1B06PK  Walls-vs-Floors  holds 2 clashes only its mirrors found"));
            Assert.That(OneRow(Run(3, 5), workbook, PageTests.Read(Block(Walls, 5))).Differs, Is.True, "without the merge the same counts differ");
        }

        [Test]
        public void AMirrorNotMergedKeepsItsRowsAndIsHeldToNoView()
        {
            const string mirror = "Floors-vs-Walls (mirror)";
            ClashReport report = new ClashReport("1B06PK", "1104-PAR-1B06PK-ZZZ-BM-MOD-000001");
            report.SetTreeRoot = "lcop_selection_set_tree";
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 10, 19, 4, 0, 0);
            WorkbookTestsTests.AddTest(report, mirror, 2, 0);
            WorkbookTests workbook = WorkbookTestsTests.ReadBack(WorkbookTestsTests.Write(report, folder));
            GroupRunFiles run = GroupRunFiles.Read(Tsv(
                Row(RunEvents.TestFoundClashes, mirror, "2", string.Empty),
                Row(RunEvents.Mirror, mirror, string.Empty, Walls),
                Row(RunEvents.ViewsTree, "1B06PK", "1", ViewLine(Walls, 0))));

            ClashCountRow row = OneRow(run, workbook, PageTests.Read(Block(mirror, 2)));

            Assert.That(row.UnmergedMirror, Is.True);
            Assert.That(row.Views, Is.EqualTo(0));
            Assert.That(row.ViewsExpected, Is.EqualTo(0), "no mirror gets a view");
            Assert.That(row.Differs, Is.False);
        }

        [Test]
        public void WhyARowHasNoPictureIsTheRunTakingNoneItsStatusTheCapOverTheSettingOrUnknown()
        {
            WorkbookTest read = WorkbookTestsTests.ReadBack(Workbook(test =>
            {
                Picture(test.Rows[0]);
                test.Rows[4].Status = ClashStatus.Resolved;
            })).Named(Walls).Single();

            Assert.That(ClashCountCheck.WhySkipped(read, new ImageOptions()),
                Is.EqualTo("1 at a status the pictures do not take, 3 UNKNOWN, a stop, a failure or a retake showing only the sky, which the group's IMAGES and RETAKE lines name"));
            Assert.That(ClashCountCheck.WhySkipped(read, new ImageOptions { Write = false }),
                Is.EqualTo("4 with no picture, the run took no pictures"), "pictures off never reads as a status not taken");
            Assert.That(ClashCountCheck.WhySkipped(WorkbookTestsTests.ReadBack(Workbook(test =>
            {
                foreach (ClashRow row in test.Rows)
                {
                    Picture(row);
                }
            })).Named(Walls).Single(), new ImageOptions()), Is.Empty, "every row with its picture");
        }

        [Test]
        public void TheCapIsReadOffTheTestsClashesCellAsThePicturesReadIt()
        {
            WorkbookTest read = WorkbookTestsTests.ReadBack(Workbook(test => Picture(test.Rows[0]))).Named(Walls).Single();

            Assert.That(ClashCountCheck.WhySkipped(read, new ImageOptions { CapOverClashes = 4, PicturesForA = 1 }),
                Is.EqualTo("4 past the cap of 1 of a test of more than 4 clashes"), "the Clashes cell, 5, is over 4");
            Assert.That(ClashCountCheck.WhySkipped(read, new ImageOptions { CapOverClashes = 5, PicturesForA = 1 }),
                Does.Not.Contain("past the cap"), "5 clashes is not more than 5");
        }

        [Test]
        public void TheTotalsAddUpEveryRowReadAndSayAtLeastOverACountNotRead()
        {
            WorkbookTests workbook = WorkbookTestsTests.ReadBack(Workbook(test => Picture(test.Rows[0])));
            ClashCountCheck check = new ClashCountCheck("C06");
            OneRow(Run(5, 5), workbook, PageTests.Read(Block(Walls, 5)), check);
            OneRow(Run(5, 4), workbook, null, check);

            ClashCountRow total = check.Total();
            Assert.That(total.Found, Is.EqualTo(10));
            Assert.That(total.HtmlRows, Is.EqualTo(5), "the HTML rows not read are left out");
            Assert.That(total.Made, Is.EqualTo(2));
            Assert.That(total.At(ClashStatus.New), Is.EqualTo(10));
            Assert.That(total.WhySkipped, Is.EqualTo("at least these, 1 count UNKNOWN is left out"));
            Assert.That(total.Test, Is.EqualTo("Total C06"));
            Assert.That(check.Groups[0].Total().Test, Is.EqualTo("Total 1B06PK"));
            Assert.That(check.Groups[0].Total().WhySkipped, Is.Empty);
        }

        [Test]
        public void TheWorkbookIsReadBackAgainstTheCheckItWasWrittenFrom()
        {
            WorkbookTests workbook = WorkbookTestsTests.ReadBack(Workbook(test => Picture(test.Rows[0])));
            ClashCountCheck check = new ClashCountCheck("C06");
            check.AddGroup("1B06PK", Run(5, 5), workbook, PageTests.Read(Block(Walls, 5)), null, ViewpointSettings.DefaultViewStatuses);
            check.AddGroup("1B06PS", Run(5, 4), workbook, PageTests.Read(Block(Walls, 5)), null, ViewpointSettings.DefaultViewStatuses);
            check.AddNotBuilt("1B06PG");
            check.AddNotRead("1B06PE", "its Navisworks wrote no result");
            string written = Path.Combine(folder, ClashCountWorkbook.NameFor("C06"));

            ClashCountWorkbook.Write(written, check, new DateTime(2026, 10, 19));

            Assert.That(Path.GetFileName(written), Is.EqualTo("C06 clash count check.xlsx"));
            Assert.That(ClashCountWorkbook.ReadBackLine(written, check), Is.EqualTo("COUNTS   read back 2 test rows, 1 red, as written, of C06"));

            using (XLWorkbook book = new XLWorkbook(written))
            {
                IXLWorksheet sheet = book.Worksheet(1);
                Assert.That(sheet.Name, Is.EqualTo("C06"));
                Assert.That(Enumerable.Range(1, 15).Select(c => sheet.Cell(ClashCountWorkbook.HeaderRow, c).GetString()).ToArray(), Is.EqualTo(ClashCountRow.Columns));
                Assert.That(sheet.Cell(ClashCountWorkbook.HeaderRow + 1, 4).GetDouble(), Is.EqualTo(5), "a number written as a number");
                Assert.That(sheet.Cell(ClashCountWorkbook.HeaderRow + 3, 1).Style.Fill.BackgroundColor, Is.EqualTo(ClashCountWorkbook.Red));
                Assert.That(sheet.Cell(ClashCountWorkbook.HeaderRow + 4, 1).Style.Fill.BackgroundColor, Is.Not.EqualTo(ClashCountWorkbook.Red), "a total is never red");
                IList<string> column = Enumerable.Range(1, 40).Select(r => sheet.Cell(r, 1).GetString()).ToList();
                Assert.That(column, Has.Member(ClashCountWorkbook.NotBuiltHeading));
                Assert.That(column, Has.Member("1B06PG"));
                Assert.That(column, Has.Member(ClashCountWorkbook.LeftOutHeading));
                Assert.That(column, Has.Member(ClashCountWorkbook.NotReadHeading));
                Assert.That(column, Has.Member("1B06PE"));
            }

            ClashCountCheck more = new ClashCountCheck("C06");
            more.AddGroup("1B06PK", Run(5, 4), workbook, PageTests.Read(Block(Walls, 5)), null, ViewpointSettings.DefaultViewStatuses);
            Assert.That(ClashCountWorkbook.ReadBackLine(written, more), Is.EqualTo("COUNTS   read back THE WORKBOOK DIFFERS  C06  read 2 test rows, 1 red, written 1 test row, 1 red"));
            Assert.That(ClashCountWorkbook.ReadBackLine(Path.Combine(folder, "none.xlsx"), check), Does.StartWith("COUNTS   read back FAILED"));
        }

        [Test]
        public void ACommunityNameExcelRefusesStillWritesItsSheet()
        {
            ClashCountCheck check = new ClashCountCheck("C06 [old]");
            string written = Path.Combine(folder, ClashCountWorkbook.NameFor("C06 old"));

            ClashCountWorkbook.Write(written, check, new DateTime(2026, 10, 19));

            using (XLWorkbook book = new XLWorkbook(written))
            {
                Assert.That(book.Worksheet(1).Name, Is.EqualTo(SheetNames.ForReport("C06 [old]")));
            }
        }

        [Test]
        public void TheCommunitysFolderIsUnderTheReportFolderTrimmedAsAPackagesIs()
        {
            string report = TestPaths.At("Report");

            Assert.That(PackageOutput.CommunityFolderFor(report + Path.DirectorySeparatorChar, "C06"), Is.EqualTo(Path.Combine(report, "C06")));
        }
    }
}
