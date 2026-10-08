using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Generic;
using Federator.Core.Report;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The Generic Models sheet, F128 and FR-177: each model of the group that holds Generic Models
    /// items with its count, a model with none left out, a count nobody took the word UNKNOWN. Every
    /// workbook here is a real xlsx the writer made, read back off the disk as a file.
    /// </summary>
    [TestFixture]
    public class GenericSheetTests
    {
        private const string ReportSheet = "1104-PAR-100000-ZZZ-BM-RPT-0000";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorGenericSheet");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static GenericModelsReport Report(params int[] counts)
        {
            List<GenericModelInput> models = new List<GenericModelInput>();

            for (int i = 0; i < counts.Length; i++)
            {
                models.Add(new GenericModelInput("model" + i + ".nwc"));
            }

            GenericModelsPlan plan = GenericModelsPlan.For(models, new GenericModelsSettings());
            SetBuildOutcome outcome = new SetBuildOutcome();

            for (int i = 0; i < counts.Length; i++)
            {
                outcome.AddAlreadyPresent(plan.Sets[i].Set.Path, "model" + i, 2, counts[i]);
            }

            return GenericModelsReport.From(plan, outcome.Results);
        }

        /// <summary>A workbook the client's sheet and this one were written into, saved and opened again off the disk.</summary>
        private XLWorkbook Written(GenericModelsReport report, string sheetName)
        {
            string path = Path.Combine(folder, "report.xlsx");

            using (XLWorkbook workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(ReportSheet);
                GenericSheet.Write(workbook, report, "100000", sheetName);
                workbook.SaveAs(path);
            }

            return new XLWorkbook(path);
        }

        [Test]
        public void TheSheetStandsAfterTheClientsSheetNamedAsAskedAndSaysItIsNotTheirs()
        {
            using (XLWorkbook workbook = Written(Report(12, 0, 3), "Generic Models"))
            {
                Assert.That(workbook.Worksheets.Count, Is.EqualTo(2));
                Assert.That(workbook.Worksheet(1).Name, Is.EqualTo(ReportSheet));
                Assert.That(workbook.Worksheet(2).Name, Is.EqualTo("Generic Models"));
                Assert.That(workbook.Worksheet(1).LastCellUsed(), Is.Null, "nothing of ours on the client's sheet");

                IXLWorksheet sheet = workbook.Worksheet(2);

                Assert.That(sheet.Cell(1, 1).GetString(), Is.EqualTo(GenericSheet.Title));
                Assert.That(sheet.Cell(2, 1).GetString(), Is.EqualTo("Group: 100000"));
                Assert.That(sheet.Cell(2, 2).GetString(), Does.StartWith("Category equals \"Generic Models\""));
                Assert.That(sheet.Cell(3, 1).GetString(), Does.Contain("3 models: 2 hold some, 1 hold none and are left out, 0 not counted. 15 items in all"));
            }
        }

        /// <summary>A model with items is a row with its count as a number, and a model with none is no row.</summary>
        [Test]
        public void AModelWithItemsIsARowAndACountIsANumberAndAModelWithNoneIsLeftOut()
        {
            using (XLWorkbook workbook = Written(Report(12, 0, 3), "Generic Models"))
            {
                IXLWorksheet sheet = workbook.Worksheet(2);

                for (int column = 0; column < GenericSheet.Headings.Length; column++)
                {
                    Assert.That(sheet.Cell(5, column + 1).GetString(), Is.EqualTo(GenericSheet.Headings[column]));
                }

                Assert.That(sheet.Cell(6, 1).GetString(), Is.EqualTo("model0"));
                Assert.That(sheet.Cell(6, 2).DataType, Is.EqualTo(XLDataType.Number));
                Assert.That(sheet.Cell(6, 2).GetDouble(), Is.EqualTo(12));
                Assert.That(sheet.Cell(7, 1).GetString(), Is.EqualTo("model2"));
                Assert.That(sheet.Cell(7, 2).GetDouble(), Is.EqualTo(3));
                Assert.That(sheet.Cell(8, 1).GetString(), Does.Not.Contain("model1"), "the model with none is left out");
            }
        }

        [Test]
        public void ACountNobodyTookIsTheWordUnknownWithWhyAndNeverNought()
        {
            GenericModelsPlan plan = GenericModelsPlan.For(
                new[] { new GenericModelInput("a.nwc"), new GenericModelInput("b.nwc") }, new GenericModelsSettings());
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent(plan.Sets[0].Set.Path, "a", 2, 7);
            GenericModelsReport report = GenericModelsReport.From(plan, outcome.Results);

            using (XLWorkbook workbook = Written(report, "Generic Models"))
            {
                IXLWorksheet sheet = workbook.Worksheet(2);

                Assert.That(sheet.Cell(3, 1).GetString(), Does.Contain("1 not counted. At least 7 items in all"));
                Assert.That(sheet.Cell(7, 1).GetString(), Is.EqualTo("b"));
                Assert.That(sheet.Cell(7, 2).GetString(), Is.EqualTo("UNKNOWN"));
                Assert.That(sheet.Cell(7, 3).GetString(), Does.Contain("not built, or no result of it was handed in"));
            }
        }

        [Test]
        public void ThePlansNotesAndTheLineThatNoClashTestIsMadeAreOnTheSheet()
        {
            GenericModelsPlan plan = GenericModelsPlan.For(
                new[] { new GenericModelInput("A-ME.nwc"), new GenericModelInput("A-ME2.nwc") }, new GenericModelsSettings());
            GenericModelsReport report = GenericModelsReport.From(plan, new List<SetResult>());

            using (XLWorkbook workbook = Written(report, "Generic Models"))
            {
                IXLWorksheet sheet = workbook.Worksheet(2);
                List<string> column = new List<string>();

                for (int row = 1; row <= sheet.LastRowUsed().RowNumber(); row++)
                {
                    column.Add(sheet.Cell(row, 1).GetString());
                }

                Assert.That(column, Has.Some.StartWith("note: the text A-ME that finds the model A-ME is also in the text A-ME2"));
                Assert.That(column[column.Count - 1], Is.EqualTo("No clash test is made for these sets"));
            }
        }

        [Test]
        public void ARunWithNoModelPlannedSaysSoAndWritesNoRow()
        {
            GenericModelsReport report = GenericModelsReport.From(
                GenericModelsPlan.For(new GenericModelInput[0], new GenericModelsSettings()), new List<SetResult>());

            using (XLWorkbook workbook = Written(report, "Generic Models"))
            {
                Assert.That(workbook.Worksheet(2).Cell(3, 1).GetString(), Is.EqualTo("No model of this group was planned, so nothing was counted"));
                Assert.That(GenericSheet.Rows(report), Is.Empty);
            }
        }

        // ---------- the name ----------

        /// <summary>A name Excel refuses would lose the whole workbook at the point of writing, so it is made acceptable.</summary>
        [Test]
        public void ANameExcelRefusesIsMadeAcceptableAndNeverLosesTheWorkbook()
        {
            using (XLWorkbook workbook = Written(Report(1), "Generic:Models"))
            {
                Assert.That(workbook.Worksheet(2).Name, Is.EqualTo("Generic Models"));
            }
        }

        [Test]
        public void TwoSheetsOfOneNameAreRefusedWhateverTheCase()
        {
            Assert.That(GenericSheet.WhyRefused(ReportSheet, "Coverage", "Generic Models"), Is.Null);
            Assert.That(GenericSheet.WhyRefused("generic models", "Coverage", "Generic Models"),
                Does.Contain("the report's own sheet is named generic models"));
            Assert.That(GenericSheet.WhyRefused(ReportSheet, "GENERIC MODELS", "Generic Models"),
                Does.Contain("the Coverage sheet is named GENERIC MODELS"));
        }

        [Test]
        public void NothingToWriteIsRefusedNotWrittenEmpty()
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {
                Assert.Throws<ArgumentNullException>(() => GenericSheet.Write(null, Report(1), "g", "x"));
                Assert.Throws<ArgumentNullException>(() => GenericSheet.Write(workbook, null, "g", "x"));
                Assert.Throws<ArgumentNullException>(() => GenericSheet.Rows(null));
                Assert.Throws<ArgumentNullException>(() => GenericSheet.Summary(null));
            }
        }
    }
}
