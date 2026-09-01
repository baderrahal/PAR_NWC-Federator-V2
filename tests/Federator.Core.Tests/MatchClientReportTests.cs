using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Clash;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The differences between the workbook this tool wrote on 2026-09-01 and the client's
    /// own report, measured cell by cell out of the two files in samples\, and pinned so
    /// they cannot come back.
    ///
    ///   samples\our-report\1104-PAR-1C07BC-ZZZ-BM-RPT-00001.xlsx
    ///   samples\client-report\1104-PAR-1A02WE-XXX-BM-RPT-000001.xlsx
    ///
    /// Every "theirs" value below was read out of their file and every "ours" value out of
    /// ours. Nothing here is an opinion about what the format ought to be.
    /// </summary>
    [TestFixture]
    public class MatchClientReportTests
    {
        private const string Root = "lcop_selection_set_tree";
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(Path.GetTempPath(), "FederatorMatch", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void RemoveFolder()
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

        // ---------- Grid Location, which had the level in it twice ----------

        // Ours read "D-8 : LGF : LGF" on every row of the real run. Navisworks' own grid
        // intersection name already carries the level, so joining the level onto it said
        // it twice.
        [Test]
        public void TheLevelIsNotSaidTwiceWhenTheGridAlreadyCarriesIt()
        {
            Assert.That(ClientFormat.GridLocation("D-8 : LGF", "LGF"), Is.EqualTo("D-8 : LGF"));
            Assert.That(ClientFormat.GridLocation("G-1 : LGF", "LGF"), Is.EqualTo("G-1 : LGF"));
        }

        [Test]
        public void AGridWithoutTheLevelStillGetsIt()
        {
            Assert.That(ClientFormat.GridLocation("B-1", "LGF"), Is.EqualTo("B-1 : LGF"));
        }

        [Test]
        public void ADifferentLevelOnTheEndIsNotMistakenForTheSameOne()
        {
            Assert.That(ClientFormat.GridLocation("D-8 : LGF", "ROF"),
                Is.EqualTo("D-8 : LGF : ROF"),
                "two different levels is odd but it is what the model said, so it is shown");
        }

        [Test]
        public void TheRowBuildsItWithoutDoublingToo()
        {
            ClashRow row = new ClashRow();
            row.GridLocation = "D-8 : LGF";
            row.Level = "LGF";

            Assert.That(row.ClientGridLocation(), Is.EqualTo("D-8 : LGF"));
        }

        // ---------- Type, which was the file's token rather than their words ----------

        // Ours read "hard_conservative". Theirs reads "Hard (Conservative)".
        [Test]
        public void TheTestTypeReadsInTheirWordsAndNotTheFilesToken()
        {
            Assert.That(ClientFormat.TestTypeWording("hard_conservative"),
                Is.EqualTo("Hard (Conservative)"));
            Assert.That(ClientFormat.TestTypeWording("HardConservative"),
                Is.EqualTo("Hard (Conservative)"));
        }

        [Test]
        public void ATypeOfOneWordIsJustThatWord()
        {
            Assert.That(ClientFormat.TestTypeWording("hard"), Is.EqualTo("Hard"));
            Assert.That(ClientFormat.TestTypeWording("clearance"), Is.EqualTo("Clearance"));
            Assert.That(ClientFormat.TestTypeWording("duplicate"), Is.EqualTo("Duplicate"));
            Assert.That(ClientFormat.TestTypeWording("custom"), Is.EqualTo("Custom"));
        }

        [Test]
        public void SomethingAlreadyInTheirWordsIsLeftExactlyAsItIs()
        {
            Assert.That(ClientFormat.TestTypeWording("Hard (Conservative)"),
                Is.EqualTo("Hard (Conservative)"));
            Assert.That(ClientFormat.TestTypeWording(string.Empty), Is.EqualTo(string.Empty));
            Assert.That(ClientFormat.TestTypeWording(null), Is.EqualTo(string.Empty));
        }

        // ---------- Item ID, which was an all zero GUID on all 426 item cells ----------

        [Test]
        public void AnAllZeroGuidIsNotAnIdAndLeavesTheCellEmpty()
        {
            Assert.That(ClientFormat.NoIdAtAll("00000000-0000-0000-0000-000000000000"), Is.True);
            Assert.That(ClientFormat.NoIdAtAll("{00000000-0000-0000-0000-000000000000}"), Is.True);
            Assert.That(ClientFormat.NoIdAtAll(string.Empty), Is.True);
            Assert.That(ClientFormat.NoIdAtAll(null), Is.True);

            Assert.That(ClientFormat.ItemId("Instance GUID", "00000000-0000-0000-0000-000000000000"),
                Is.EqualTo(string.Empty),
                "an empty cell is honest, a row of zeros reads like an id and identifies nothing");
        }

        [Test]
        public void ARealIdIsStillWritten()
        {
            Assert.That(ClientFormat.NoIdAtAll("2635048"), Is.False);
            Assert.That(ClientFormat.ItemId("Element ID", "2635048"),
                Is.EqualTo("Element ID: 2635048"));

            // A GUID that is not all zeros is a real id and is kept.
            Assert.That(ClientFormat.NoIdAtAll("3f2504e0-4f89-11d3-9a0c-0305e82c3301"), Is.False);
        }

        // ---------- Distance, which printed the whole double ----------

        // Ours read -0.328083992004395. Theirs reads -0.05. Both are the raw signed
        // number in their own document's units, so the value is right and the FORMAT was
        // not.
        [Test]
        public void TheDistanceCellIsANumberWithThreeDecimals()
        {
            ClashReport report = Report();
            TestReport test = OneTest(report, -0.328083992004395);

            using (XLWorkbook workbook = new XLWorkbook(Write(report, new ReportOptions())))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                IXLCell cell = sheet.Cell(HeaderRow(sheet) + 1, WorkbookWriter.ColumnDistance);

                // What this used to assert, that the raw double is kept behind a display
                // format, was measured against theirs and is wrong. Theirs stores -0.116
                // and carries no format at all, so anyone sorting, filtering or copying
                // a column of ours got -0.328083992004395 where theirs gives -0.328.
                Assert.That(cell.DataType, Is.EqualTo(XLDataType.Number),
                    "it has to stay a number so the column still sorts");
                Assert.That(cell.GetDouble(), Is.EqualTo(-0.328).Within(0.0000000001),
                    "the value itself is rounded, which is what theirs holds");
                Assert.That(cell.Style.NumberFormat.Format, Is.Empty,
                    "theirs carries no number format, so a format here would mean ours is "
                    + "hiding a longer number behind a shorter one");
                Assert.That(cell.GetFormattedString(), Is.EqualTo("-0.328"));
            }
        }

        // ---------- nothing of ours anywhere on the sheet ----------

        // What used to be here tested a client only MODE. There is no mode now. The
        // workbook is the client's report and nothing else, so the notes, the link back to
        // the Summary and the filter arrows are gone along with the Summary itself.
        [Test]
        public void TheSheetCarriesNoNotesNoBackLinkAndNoFilters()
        {
            ClashReport report = Report();
            OneTest(report, -0.05);

            using (XLWorkbook workbook = new XLWorkbook(Write(report, new ReportOptions())))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);

                foreach (IXLCell cell in sheet.CellsUsed())
                {
                    string text = cell.GetString();

                    Assert.That(text, Does.Not.Contain("Back to"));
                    Assert.That(text, Does.Not.Contain("raw clashes"));
                    Assert.That(text, Does.Not.Contain("carry a picture"));
                }

                Assert.That(sheet.AutoFilter.IsEnabled, Is.False,
                    "the filter arrows are ours and theirs has none");
                Assert.That(workbook.Worksheets.Count, Is.EqualTo(1));
            }
        }

        // ---------- the number kept all six digits ----------

        // The one the brief asks for by name. The inputs are MOD-000001 and a real run
        // wrote MOD-00001, five digits, because the window opened with every naming box
        // empty and all five fields had to be typed by hand.
        [Test]
        public void TheDefaultNumberHasSixDigits()
        {
            Assert.That(NamePattern.DefaultNumber, Is.EqualTo("000001"));
            Assert.That(NamePattern.DefaultNumber.Length, Is.EqualTo(6));
            Assert.That(ContainerNameSettings.DefaultForcedNumber.Length, Is.EqualTo(6));
            Assert.That(new NamePattern().Number.Length, Is.EqualTo(6));
        }

        [Test]
        public void TheDefaultOutputNameEndsInSixDigits()
        {
            BuildingGroupingResult result = BuildingGrouping.GroupNames(
                new[] { "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc" },
                new ContainerNameSettings(),
                GroupingMode.PerBuilding);

            string name = new NamePattern().NameFor(result.Groups[0], new ContainerNameSettings());

            Assert.That(name, Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
            Assert.That(name, Does.EndWith("-000001"));
            Assert.That(name.Substring(name.LastIndexOf('-') + 1).Length, Is.EqualTo(6),
                "the input files carry six digits and the output carried five");
        }

        [Test]
        public void EveryOneOfTheThreePatternsStartsWithSixDigits()
        {
            OutputNaming naming = new OutputNaming();

            Assert.That(naming.Nwf.Number.Length, Is.EqualTo(6));
            Assert.That(naming.Nwd.Number.Length, Is.EqualTo(6));
            Assert.That(naming.Workbook.Number.Length, Is.EqualTo(6));
        }

        // ---------- the thumbnails are off ----------

        // The one the brief asks for by name. Their report links to its pictures. Ours
        // pasted them in as well and came out at 51.7 MB where the same workbook without
        // them is about 0.3 MB, and the pictures are in the folder beside it either way.
        [Test]
        public void PastingThumbnailsIsOffByDefault()
        {
            Assert.That(new ImageOptions().EmbedThumbnail, Is.False);
            Assert.That(new ReportOptions().Images.EmbedThumbnail, Is.False);
            Assert.That(new ImageOptions().Describe(), Does.Contain("linked rather than pasted in"));
        }

        [Test]
        public void WithThumbnailsOffTheWorkbookHoldsNoPictures()
        {
            ClashReport report = Report();
            TestReport test = OneTest(report, -0.05);

            // A real picture beside the workbook, linked from the row.
            string images = ImageNaming.FolderFor(Path.Combine(folder, report.OutputName + ".xlsx"));
            Directory.CreateDirectory(images);
            string jpg = Path.Combine(images, ImageNaming.FileNameFor(0, 1));
            File.WriteAllBytes(jpg, Jpeg());

            test.Rows[0].ImageFile = ImageNaming.FileNameFor(0, 1);
            test.Rows[0].ImageLink = ImageNaming.LinkFor(
                Path.Combine(folder, report.OutputName + ".xlsx"), 0, 1);
            test.Rows[0].ImagePath = jpg;

            string path = Write(report, new ReportOptions());

            Assert.That(Pictures(path), Is.EqualTo(0),
                "the default pasted the pictures in and made the file 170 times bigger");

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                IXLCell cell = sheet.Cell(HeaderRow(sheet) + 1, WorkbookWriter.ColumnImage);

                Assert.That(cell.HasHyperlink, Is.True,
                    "the row still has to reach its picture, it is just not pasted in");
            }
        }

        [Test]
        public void TickingThumbnailsOnDoesPasteThem()
        {
            ClashReport report = Report();
            TestReport test = OneTest(report, -0.05);

            string images = ImageNaming.FolderFor(Path.Combine(folder, report.OutputName + ".xlsx"));
            Directory.CreateDirectory(images);
            string jpg = Path.Combine(images, ImageNaming.FileNameFor(0, 1));
            File.WriteAllBytes(jpg, Jpeg());

            test.Rows[0].ImageFile = ImageNaming.FileNameFor(0, 1);
            test.Rows[0].ImageLink = "x_files/cd000001.jpg";
            test.Rows[0].ImagePath = jpg;

            ReportOptions pasted = new ReportOptions();
            pasted.Images.EmbedThumbnail = true;

            Assert.That(Pictures(Write(report, pasted)), Is.EqualTo(1));
        }

        // ---------- helpers ----------

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-RPT-000001");
            report.SetTreeRoot = Root;
            report.DocumentUnits = "ft";
            report.RunAt = new DateTime(2026, 9, 1, 9, 44, 0);
            return report;
        }

        private static TestReport OneTest(ClashReport report, double distance)
        {
            TestReport test = report.AddTest("BLD-AR-Walls-vs-BLD-AR-Columns");
            test.LeftLocator = Root + "/Architecture/BLD-AR-Walls";
            test.RightLocator = Root + "/Architecture/BLD-AR-Columns";
            test.Tolerance = 0.2460629921;
            test.ToleranceUnits = "ft";
            test.TestTypeName = "hard_conservative";
            test.StatusWord = "New";
            test.State = TestState.FoundClashes;

            ClashRow row = new ClashRow();
            row.Name = "Clash1";
            row.Status = ClashStatus.New;
            row.Distance = distance;
            row.GridLocation = "D-8 : LGF";
            row.Level = "LGF";
            row.Description = "Hard (Conservative)";
            row.X = 33.171;
            row.Y = -10.410;
            row.Z = 0.328;
            row.Left.ElementId = "2635048";
            row.Left.Name = "PAR-CONC-FOUNDATION";
            row.Left.ItemType = "Solid";
            row.Left.Family = "Basic Wall";
            row.Right.ElementId = "814542";
            row.Right.Name = "Mx_Exterior_EW6_ Alum Sheet (Dark)";
            row.Right.ItemType = "Solid";
            row.Right.Family = "Structural Column";
            test.Add(row);

            return test;
        }

        private string Write(ClashReport report, ReportOptions options)
        {
            string path = Path.Combine(folder, report.OutputName + ".xlsx");
            return new WorkbookWriter(options).Write(report, path);
        }

        private static int HeaderRow(IXLWorksheet sheet)
        {
            for (int row = 1; row <= 40; row++)
            {
                if (sheet.Cell(row, WorkbookWriter.ColumnClashName).GetString() == "Clash Name")
                {
                    return row;
                }
            }

            Assert.Fail("no row carries the client's Clash Name header");
            return 0;
        }

        private static bool Anywhere(IXLWorksheet sheet, string text)
        {
            foreach (IXLCell cell in sheet.CellsUsed())
            {
                if (cell.GetString().IndexOf(text, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>How many pictures the written file actually holds, read out of the zip.</summary>
        private static int Pictures(string path)
        {
            int count = 0;

            using (System.IO.Compression.ZipArchive archive =
                System.IO.Compression.ZipFile.OpenRead(path))
            {
                foreach (System.IO.Compression.ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.IndexOf("media", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>The smallest thing ClosedXML will accept as a jpg.</summary>
        private static byte[] Jpeg()
        {
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(8, 8))
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }
    }
}
