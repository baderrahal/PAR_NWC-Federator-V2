using System;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Proves the check CATCHES each of the eight faults it used to report clean over.
    ///
    /// Bader's words were that this is the third time a check has passed with a difference
    /// sitting in the file. A check nobody has watched fail is not a check, so each of
    /// these writes a good workbook, breaks exactly one thing in it, and asserts the check
    /// names that thing. A test that only asserts the good file passes would have passed
    /// against every one of the eight.
    /// </summary>
    [TestFixture]
    public class WorkbookCellCheckTests
    {
        private const string OutputName = "1104-PAR-1C07BC-ZZZ-BM-MOD-000001";

        private string folder;

        [SetUp]
        public void Setup()
        {
            folder = Path.Combine(Path.GetTempPath(),
                "cellcheck-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void Teardown()
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (IOException)
            {
                // A leftover temp folder is not worth failing a test over.
            }
        }

        // ---------- a workbook to break ----------

        private string Good()
        {
            ClashReport report = new ClashReport("1C07BC", OutputName);
            report.DocumentUnits = "m";

            TestReport test = report.AddTest("BLD-EL-Conduits-vs-BLD-ST-Columns");
            test.Tolerance = 0.025;
            test.ToleranceUnits = "m";
            test.TestTypeName = "hard_conservative";
            test.StatusWord = "Complete";
            test.State = TestState.FoundClashes;

            for (int i = 1; i <= 2; i++)
            {
                ClashRow row = new ClashRow();
                row.Name = "Clash" + i;
                row.Status = ClashStatus.New;
                row.Distance = -0.116;
                row.GridLocation = "A-1";
                row.Level = "LGF";
                row.Description = "Hard (Conservative)";
                row.X = 33.399;
                row.Y = 8.310;
                row.Z = -0.441;
                row.Left.ElementId = "707077";
                row.Left.Name = "ELE-CND-Standard";
                row.Left.ItemType = "Solid";
                row.Right.ElementId = "1369872";
                row.Right.Name = "Concrete";
                row.Right.ItemType = "Solid";
                test.Add(row);
            }

            string path = Path.Combine(folder, OutputName + ".xlsx");
            new WorkbookWriter().Write(report, path);
            return path;
        }

        /// <summary>Opens the written file, lets the caller break one thing, saves it.</summary>
        private static void Break(string path, Action<IXLWorksheet> damage)
        {
            using (XLWorkbook book = new XLWorkbook(path))
            {
                damage(book.Worksheet(1));
                book.Save();
            }
        }

        private static WorkbookCheck After(string path, Action<IXLWorksheet> damage)
        {
            Break(path, damage);
            return WorkbookCheck.Of(path);
        }

        private static void Names(WorkbookCheck check, string what)
        {
            Assert.That(check.Passed, Is.False,
                "The check reported clean over " + what + ", which is the fault this "
                + "whole fixture exists to stop.");

            Assert.That(string.Join(" ", Text(check)), Does.Contain(what),
                "The check found something but did not say what. It said: "
                + string.Join(" | ", Text(check)));
        }

        private static string[] Text(WorkbookCheck check)
        {
            string[] lines = new string[check.Problems.Count];
            check.Problems.CopyTo(lines, 0);
            return lines;
        }

        /// <summary>The heading row, found the way the check finds it.</summary>
        private static int HeadingRow(IXLWorksheet sheet)
        {
            for (int row = 1; row <= 40; row++)
            {
                if (sheet.Cell(row, WorkbookWriter.ColumnClashName).GetString() == "Clash Name")
                {
                    return row;
                }
            }

            return 0;
        }

        // ---------- the workbook as written passes ----------

        [Test]
        public void TheWorkbookThisWritesPasses()
        {
            WorkbookCheck check = WorkbookCheck.Of(Good());

            Assert.That(check.Ran, Is.True, check.CouldNotRead);
            Assert.That(check.Passed, Is.True,
                "A workbook written by our own writer must pass our own check: "
                + string.Join(" | ", Text(check)));
        }

        // ---------- one fault at a time ----------

        [Test]
        public void ItCatchesAHeadingRowWithNoFill()
        {
            string path = Good();

            WorkbookCheck check = After(path, sheet =>
            {
                int row = HeadingRow(sheet);

                for (int column = 1; column <= WorkbookWriter.LastColumn; column++)
                {
                    sheet.Cell(row, column).Style.Fill.SetBackgroundColor(XLColor.NoColor);
                }
            });

            Names(check, "not filled");
        }

        [Test]
        public void ItCatchesAClashRowWithNoItemColours()
        {
            string path = Good();

            WorkbookCheck check = After(path, sheet =>
            {
                int row = HeadingRow(sheet) + 1;

                for (int column = WorkbookWriter.ColumnItem1;
                    column <= WorkbookWriter.LastColumn; column++)
                {
                    sheet.Cell(row, column).Style.Fill.SetBackgroundColor(XLColor.NoColor);
                }
            });

            Names(check, "not filled");
        }

        [Test]
        public void ItCatchesARowWithNoBorder()
        {
            string path = Good();

            WorkbookCheck check = After(path, sheet =>
            {
                int row = HeadingRow(sheet) + 1;

                for (int column = 1; column <= WorkbookWriter.LastColumn; column++)
                {
                    IXLStyle style = sheet.Cell(row, column).Style;
                    style.Border.TopBorder = XLBorderStyleValues.None;
                    style.Border.BottomBorder = XLBorderStyleValues.None;
                    style.Border.LeftBorder = XLBorderStyleValues.None;
                    style.Border.RightBorder = XLBorderStyleValues.None;
                }
            });

            Names(check, "carries no border");
        }

        [Test]
        public void ItCatchesAClashRowOfTheWrongHeight()
        {
            string path = Good();

            WorkbookCheck check = After(path,
                sheet => sheet.Row(HeadingRow(sheet) + 1).Height = 15.0);

            Names(check, "high and the client's report has it at 60");
        }

        [Test]
        public void ItCatchesTheTitleRowOfTheWrongHeight()
        {
            string path = Good();

            WorkbookCheck check = After(path, sheet => sheet.Row(1).Height = 15.0);

            Names(check, "Row 1 is 15 high");
        }

        [Test]
        public void ItCatchesAColumnOfTheWrongWidth()
        {
            string path = Good();

            WorkbookCheck check = After(path, sheet => sheet.Column(6).Width = 3.0);

            Names(check, "Column F is 3 wide");
        }

        [Test]
        public void ItCatchesTheRawDoubleBehindADisplayFormat()
        {
            string path = Good();

            WorkbookCheck check = After(path, sheet =>
            {
                IXLCell cell = sheet.Cell(HeadingRow(sheet) + 1, WorkbookWriter.ColumnDistance);
                cell.Value = -0.328083992004395;
                cell.Style.NumberFormat.Format = "0.000";
            });

            Names(check, "carries the number format");
        }

        [Test]
        public void ItCatchesADistanceThatWasNeverRounded()
        {
            string path = Good();

            WorkbookCheck check = After(path,
                sheet => sheet.Cell(HeadingRow(sheet) + 1, WorkbookWriter.ColumnDistance)
                    .Value = -0.328083992004395);

            Names(check, "holds it rounded");
        }

        [Test]
        public void ItCatchesADistanceWrittenAsText()
        {
            string path = Good();

            WorkbookCheck check = After(path,
                sheet => sheet.Cell(HeadingRow(sheet) + 1, WorkbookWriter.ColumnDistance)
                    .Value = "-0.116");

            Names(check, "as text");
        }

        [Test]
        public void ItCatchesAnEmptyLayerColumn()
        {
            string path = Good();

            WorkbookCheck check = After(path,
                sheet => sheet.Cell(HeadingRow(sheet) + 1, WorkbookWriter.ColumnItem1 + 1)
                    .Clear(XLClearOptions.Contents));

            Names(check, "Layer cell is empty");
        }

        [Test]
        public void ItCatchesAFileNameInTheImageCell()
        {
            string path = Good();

            WorkbookCheck check = After(path,
                sheet => sheet.Cell(HeadingRow(sheet) + 1, WorkbookWriter.ColumnImage)
                    .Value = "cd000001.jpg");

            Names(check, "The Image cell reads");
        }

        [Test]
        public void ItCatchesTheWrongWordsInTheTitle()
        {
            string path = Good();

            WorkbookCheck check = After(path, sheet => sheet.Cell(1, 4).Value = "Report");

            Names(check, "Row 1 reads");
        }

        // ---------- what it says when it passes ----------

        [Test]
        public void ThePassLineSaysWhatItDidNotLookAt()
        {
            WorkbookCheck check = WorkbookCheck.Of(Good());
            string said = string.Join(" ", Lines(check));

            Assert.That(said, Does.Contain("Not compared"),
                "A check that says nothing is wrong has to say what it did not look at, "
                + "or the next difference sitting in the file reads as approved.");
            Assert.That(said, Does.Contain("font"));
            Assert.That(said, Does.Contain("merged ranges"));
        }

        private static string[] Lines(WorkbookCheck check)
        {
            string[] lines = new string[check.Lines().Count];
            check.Lines().CopyTo(lines, 0);
            return lines;
        }
    }
}
