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
    /// .tsv, its clash workbook and its HTML page, the four counts set side by side, a red row where they differ, the totals,
    /// and the groups not built with the tests the two rules left out. Each part is broken one way and asserted to name it.
    /// </summary>
    [TestFixture]
    public class ClashCountCheckTests
    {
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
            return string.Join("\t", new[] { "04:00:00.000", "1.000", "C06  1B06PK", string.Empty, kind, name, number, text }.Select(EventRow.Escape));
        }

        private static List<string> Tsv(params string[] rows)
        {
            List<string> lines = new List<string> { "time\tseconds\tgroup\tstep\tevent\tname\tnumber\ttext" };
            lines.AddRange(rows);
            return lines;
        }

        // ---------- the views line ----------

        [Test]
        public void AViewsLineIsReadBackToItsTestAndItsOpenClashesAndAFolderLineIsNot()
        {
            string test;
            int open;

            Assert.That(ViewsTree.ReadViewLine("      BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Columns  211 open clashes, shows ME ST, hides nothing, read back", out test, out open), Is.True);
            Assert.That(test, Is.EqualTo("BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Columns"));
            Assert.That(open, Is.EqualTo(211));
            Assert.That(ViewsTree.ReadViewLine("        BLD-ME-Ducts&Duct Fittings-vs-BLD-ST-Floors  7 open clashes, shows ME ST, hides nothing, read back", out test, out open), Is.True);
            Assert.That(open, Is.EqualTo(7));
            Assert.That(ViewsTree.ReadViewLine("Clash Status  15 views, 904 open clashes", out test, out open), Is.False);
            Assert.That(ViewsTree.ReadViewLine("open clashes in views 904, tests with open clashes 15", out test, out open), Is.False);
            Assert.That(ViewsTree.ReadViewLine(null, out test, out open), Is.False);
        }

        // ---------- the group's .tsv ----------

        [Test]
        public void TheTsvGivesClashDetectivesCountsTheFilesWrittenTheViewsAndTheTestsTheRulesLeftOut()
        {
            GroupRunFiles read = GroupRunFiles.Read(Tsv(
                Row(RunEvents.TestFoundClashes, "Walls-vs-Floors", "5", "ran and found clashes"),
                Row(RunEvents.TestPassed, "Doors-vs-Walls", "0", "ran and found nothing"),
                Row(RunEvents.Written, RunEvents.WorkbookKind, "1000", @"C:\R\C06\Log\a.xlsx"),
                Row(RunEvents.Written, RunEvents.PageKind, "1000", @"C:\R\C06\Clash report\a.html"),
                Row(RunEvents.ViewsTree, "1B06PK", "8", "      Walls-vs-Floors  3 open clashes, shows AR ST, hides nothing, read back"),
                Row(RunEvents.ViewsTree, "1B06PK", "9", "        Walls-vs-Floors  1 open clashes, shows AR ST, hides nothing, read back"),
                Row(RunEvents.CoverageTest, "AR-vs-AR", string.Empty, "position 1, UNKNOWN, not run, " + CoverageWords.For(CoverageReason.SameSetCode) + ", both sides carry AR"),
                Row(RunEvents.CoverageTest, "Walls-vs-Floors", "5", "position 2, created this run, ran, has clashes")));

            Assert.That(read.Found["Walls-vs-Floors"], Is.EqualTo(5));
            Assert.That(read.Found.ContainsKey("Doors-vs-Walls"), Is.False, "a test that found nothing is no row of the check");
            Assert.That(read.Workbook, Is.EqualTo(@"C:\R\C06\Log\a.xlsx"), "a path's backslashes read back whole");
            Assert.That(read.Page, Is.EqualTo(@"C:\R\C06\Clash report\a.html"));
            Assert.That(read.Views["Walls-vs-Floors"], Is.EqualTo(4), "the views of one test added up, its size folder among them");
            Assert.That(read.LeftOutBySetCodes, Is.EqualTo(1));
            Assert.That(read.LeftOutByOneDiscipline, Is.EqualTo(0));
            Assert.That(GroupRunFiles.Read(null).Found, Is.Empty);
        }

        // ---------- the page ----------

        private static string Block(string name, int clashes, int rows, int pictures)
        {
            string text = "<td rowspan=\"2\" colspan=\"2\" class=\"testName\">" + name.Replace("&", "&amp;") + "</td>\n"
                + "<tr class=\"contentRow\"><td class=\"contentCell\">0.025m</td><td class=\"contentCell\">" + clashes + "</td></tr></table>"
                + "<table class=\"mainTable\"><tr class=\"headerRow\"></tr>";

            for (int i = 0; i < rows; i++)
            {
                text += "<tr class=\"contentRow\"><td colspan=\"2\" class=\"contentCell\">" + (i < pictures ? "<a><img border=\"0\" src=\"x.jpg\"></a>" : string.Empty) + "</td></tr>";
            }

            return text + "</table>";
        }

        [Test]
        public void ThePageIsReadTestByTestItsRowsItsPicturesAndItsClashesCell()
        {
            PageTests read = PageTests.Read("<html><img src=\"logo.jpg\">" + Block("Ducts&Duct Fittings-vs-Walls", 3, 3, 2) + Block("None-vs-None", 0, 0, 0) + "</html>");

            Assert.That(read.Tests.Count, Is.EqualTo(2));
            PageTest ducts = read.Named("Ducts&Duct Fittings-vs-Walls").Single();
            Assert.That(ducts.Clashes, Is.EqualTo(3));
            Assert.That(ducts.Rows, Is.EqualTo(3), "the row of the summary table is not a clash row");
            Assert.That(ducts.Pictures, Is.EqualTo(2), "the logo is no picture of a test");
            Assert.That(read.Named("None-vs-None").Single().Rows, Is.EqualTo(0));
            Assert.That(PageTests.Read(null).Tests, Is.Empty);
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
            TestReport test = WorkbookTestsTests.AddTest(report, "Walls-vs-Floors", 5, 0);
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

            WorkbookTest read = WorkbookTestsTests.ReadBack(path).Named("Walls-vs-Floors").Single();

            Assert.That(read.Rows, Is.EqualTo(5));
            Assert.That(read.Pictures, Is.EqualTo(2));
            Assert.That(read.RowsAt("New"), Is.EqualTo(3));
            Assert.That(read.RowsAt("Reviewed"), Is.EqualTo(1));
            Assert.That(read.RowsAt("Resolved"), Is.EqualTo(1));
            Assert.That(read.RowsAt("Approved"), Is.EqualTo(0));
            Assert.That(read.Priority, Is.EqualTo("A"));
        }

        // ---------- the check ----------

        private static GroupRunFiles Run(int found, int views)
        {
            return GroupRunFiles.Read(Tsv(
                Row(RunEvents.TestFoundClashes, "Walls-vs-Floors", found.ToString(), "ran and found clashes"),
                Row(RunEvents.ViewsTree, "1B06PK", "1", "      Walls-vs-Floors  " + views + " open clashes, shows AR ST, hides nothing, read back")));
        }

        [Test]
        public void TheFourCountsAgreeWhereTheViewsHoldTheOpenClashesAloneAndAReviewedClashIsInNoView()
        {
            string path = Workbook(test =>
            {
                test.Rows[1].Status = ClashStatus.Reviewed;
                test.Rows[4].Status = ClashStatus.Resolved;
            });
            WorkbookTests workbook = WorkbookTestsTests.ReadBack(path);
            PageTests page = PageTests.Read(Block("Walls-vs-Floors", 5, 5, 0));
            ClashCountCheck check = new ClashCountCheck("C06");

            ClashCountRow row = check.AddGroup("1B06PK", Run(5, 3), workbook, page, new ImageOptions()).Rows.Single();

            Assert.That(row.Found, Is.EqualTo(5));
            Assert.That(row.ExcelRows, Is.EqualTo(5));
            Assert.That(row.HtmlRows, Is.EqualTo(5));
            Assert.That(row.Views, Is.EqualTo(3));
            Assert.That(row.OpenForViews, Is.EqualTo(3), "New and Active, the statuses a view shows");
            Assert.That(row.Differs, Is.False);
            Assert.That(row.Priority, Is.EqualTo("A"));
            Assert.That(check.Differing(), Is.Empty);
            Assert.That(check.Lines().Last(), Is.EqualTo("COUNTS   C06  1 group, 1 test with clashes, 0 red, the four counts differing, 0 not built for one discipline"));
        }

        [Test]
        public void EachOfTheFourCountsThatDiffersMakesTheRowRedAndALogLine()
        {
            string path = Workbook(test => { });
            WorkbookTests workbook = WorkbookTestsTests.ReadBack(path);

            ClashCountRow cd = new ClashCountCheck("C06").AddGroup("1B06PK", Run(6, 5), workbook, PageTests.Read(Block("Walls-vs-Floors", 5, 5, 0)), null).Rows.Single();
            ClashCountRow html = new ClashCountCheck("C06").AddGroup("1B06PK", Run(5, 5), workbook, PageTests.Read(Block("Walls-vs-Floors", 5, 4, 0)), null).Rows.Single();
            ClashCountCheck views = new ClashCountCheck("C06");
            ClashCountRow view = views.AddGroup("1B06PK", Run(5, 4), workbook, PageTests.Read(Block("Walls-vs-Floors", 5, 5, 0)), null).Rows.Single();

            Assert.That(cd.Differs, Is.True, "Clash Detective against the Excel rows");
            Assert.That(html.Differs, Is.True, "the HTML rows");
            Assert.That(view.Differs, Is.True, "the views against the open clashes");
            Assert.That(views.Lines().First(), Is.EqualTo("COUNTS   DIFFER  C06  1B06PK  Walls-vs-Floors  Clash Detective 5, Excel rows 5, HTML rows 5, viewpoints 4 of 5 open"));
        }

        [Test]
        public void WhyARowHasNoPictureIsItsStatusTheCapOverTheSettingOrUnknown()
        {
            WorkbookTest read = WorkbookTestsTests.ReadBack(Workbook(test =>
            {
                Picture(test.Rows[0]);
                test.Rows[4].Status = ClashStatus.Resolved;
            })).Named("Walls-vs-Floors").Single();

            Assert.That(ClashCountCheck.WhySkipped(read, 5, new ImageOptions()),
                Is.EqualTo("1 at a status the pictures do not take, 3 UNKNOWN, a stop or a failure the group's IMAGES lines name"));
            Assert.That(ClashCountCheck.WhySkipped(read, 900, new ImageOptions { PicturesForA = 1 }),
                Is.EqualTo("1 at a status the pictures do not take, 3 past the cap of 1 of a test of more than 750 clashes"));
            Assert.That(ClashCountCheck.WhySkipped(WorkbookTestsTests.ReadBack(Workbook(test =>
            {
                foreach (ClashRow row in test.Rows)
                {
                    Picture(row);
                }
            })).Named("Walls-vs-Floors").Single(), 5, new ImageOptions()), Is.Empty, "every row with its picture");
        }

        [Test]
        public void TheTotalsAddUpEveryRowAndTheWorkbookIsReadBackWithItsRedRowsAndItsGroupsNotBuilt()
        {
            string path = Workbook(test => Picture(test.Rows[0]));
            WorkbookTests workbook = WorkbookTestsTests.ReadBack(path);
            ClashCountCheck check = new ClashCountCheck("C06");
            check.AddGroup("1B06PK", Run(5, 5), workbook, PageTests.Read(Block("Walls-vs-Floors", 5, 5, 1)), null);
            check.AddGroup("1B06PS", Run(5, 4), workbook, PageTests.Read(Block("Walls-vs-Floors", 5, 5, 1)), null);
            check.AddNotBuilt("1B06PG");

            ClashCountRow total = check.Total();
            Assert.That(total.Found, Is.EqualTo(10));
            Assert.That(total.Made, Is.EqualTo(2));
            Assert.That(total.At(ClashStatus.New), Is.EqualTo(10));
            Assert.That(total.Test, Is.EqualTo("Total C06"));
            Assert.That(check.Groups[0].Total().Test, Is.EqualTo("Total 1B06PK"));

            string written = Path.Combine(folder, ClashCountWorkbook.NameFor("C06"));
            ClashCountWorkbook.Write(written, check, new DateTime(2026, 10, 19));

            Assert.That(Path.GetFileName(written), Is.EqualTo("C06 clash count check.xlsx"));
            Assert.That(ClashCountWorkbook.ReadBackLine(written, "C06"), Is.EqualTo("COUNTS   read back 2 test rows, 1 red, of C06"));

            using (XLWorkbook book = new XLWorkbook(written))
            {
                IXLWorksheet sheet = book.Worksheet(1);
                Assert.That(Enumerable.Range(1, 15).Select(c => sheet.Cell(ClashCountWorkbook.HeaderRow, c).GetString()).ToArray(), Is.EqualTo(ClashCountRow.Columns));
                Assert.That(sheet.Cell(ClashCountWorkbook.HeaderRow + 1, 4).GetDouble(), Is.EqualTo(5), "a number written as a number");
                Assert.That(sheet.Cell(ClashCountWorkbook.HeaderRow + 3, 1).Style.Fill.BackgroundColor, Is.EqualTo(ClashCountWorkbook.Red));
                IList<string> column = Enumerable.Range(1, 30).Select(r => sheet.Cell(r, 1).GetString()).ToList();
                Assert.That(column, Has.Member(ClashCountWorkbook.NotBuiltHeading));
                Assert.That(column, Has.Member("1B06PG"));
                Assert.That(column, Has.Member(ClashCountWorkbook.LeftOutHeading));
            }

            string missing = Path.Combine(folder, "none.xlsx");
            Assert.That(ClashCountWorkbook.ReadBackLine(missing, "C06"), Does.StartWith("COUNTS   read back FAILED"));
        }

        [Test]
        public void ATestTheWorkbookHoldsNoBlockOfSaysSoInWhySkipped()
        {
            ClashCountRow row = new ClashCountCheck("C06").AddGroup("1B06PK", Run(5, 5), null, null, null).Rows.Single();

            Assert.That(row.ExcelRows, Is.EqualTo(0));
            Assert.That(row.WhySkipped, Is.EqualTo("the workbook holds no block of this test"));
            Assert.That(row.Differs, Is.True);
        }
    }
}
