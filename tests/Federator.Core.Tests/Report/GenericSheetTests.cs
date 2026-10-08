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
                outcome.AddAlreadyPresent(plan.Sets[i].Set.Path, "model" + i, 2, counts[i]).Asked = plan.Sets[i].Set.Describe();
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
                Assert.That(sheet.Cell(3, 1).GetString(), Does.Contain("3 models: 2 found some, 1 found none and are left out, 0 not counted. 15 items in all"));
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
            outcome.AddAlreadyPresent(plan.Sets[0].Set.Path, "a", 2, 7).Asked = plan.Sets[0].Set.Describe();
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

                Assert.That(column, Has.Some.StartWith("note: the text A-ME that finds the model A-ME is also in the text of the model A-ME2"));
                Assert.That(column[column.Count - 1], Is.EqualTo("No clash test is made for these sets"));
            }
        }

        private static List<string> FirstColumn(IXLWorksheet sheet)
        {
            List<string> column = new List<string>();

            for (int row = 1; row <= sheet.LastRowUsed().RowNumber(); row++)
            {
                column.Add(sheet.Cell(row, 1).GetString());
            }

            return column;
        }

        /// <summary>
        /// A nought is also what a text that no item carries gives, so the sheet says what it can mean, and says more
        /// where every model is at nought. The break: with every model holding items neither note is there.
        /// </summary>
        [Test]
        public void ANoughtIsSaidToMeanWhatItCanMeanAndNothingIsSaidWhereNoModelIsAtNought()
        {
            using (XLWorkbook workbook = Written(Report(0, 0), "Generic Models"))
            {
                List<string> column = FirstColumn(workbook.Worksheet(2));

                Assert.That(column, Has.Some.EqualTo("note: " + GenericModelsReport.NoughtMeans));
                Assert.That(column, Has.Some.EqualTo("note: " + GenericModelsReport.EveryModelAtNought));
                Assert.That(workbook.Worksheet(2).Cell(3, 1).GetString(), Does.Contain("2 found none and are left out"));
            }

            using (XLWorkbook workbook = Written(Report(4, 5), "Generic Models"))
            {
                Assert.That(FirstColumn(workbook.Worksheet(2)), Has.None.Contain("counted at nought"));
            }
        }

        [Test]
        public void ATotalOverNoCountIsUnknownAndOneOverSetsThatMeetIsNotACountOfItems()
        {
            GenericModelsPlan plan = GenericModelsPlan.For(
                new[] { new GenericModelInput("a.nwc"), new GenericModelInput("b.nwc") }, new GenericModelsSettings());

            Assert.That(GenericSheet.Summary(GenericModelsReport.From(plan, null)), Does.EndWith("2 not counted. UNKNOWN items, no model was counted"));

            GenericModelsPlan meet = GenericModelsPlan.For(
                new[] { new GenericModelInput("A-ME.nwc"), new GenericModelInput("A-ME2.nwc") }, new GenericModelsSettings());
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent(meet.Sets[0].Set.Path, "A-ME", 2, 1400).Asked = meet.Sets[0].Set.Describe();
            outcome.AddAlreadyPresent(meet.Sets[1].Set.Path, "A-ME2", 2, 400).Asked = meet.Sets[1].Set.Describe();

            string summary = GenericSheet.Summary(GenericModelsReport.From(meet, outcome.Results));

            Assert.That(summary, Does.EndWith("1,800 items added over the sets, which is not a count of items, because the texts of some sets meet"));
            Assert.That(summary, Does.Not.Contain("in all"));
        }

        /// <summary>A model name is text on the sheet exactly as the file carries it, whatever its first character.</summary>
        [Test]
        public void AModelNameIsWrittenAsTheFileCarriesItWhateverItStartsWith()
        {
            string[] names = { "'quoted", "=sum", "+plus", "007", "1-2" };
            List<GenericModelInput> models = new List<GenericModelInput>();

            foreach (string name in names)
            {
                models.Add(new GenericModelInput(name + ".nwc"));
            }

            GenericModelsPlan plan = GenericModelsPlan.For(models, new GenericModelsSettings());
            SetBuildOutcome outcome = new SetBuildOutcome();

            for (int i = 0; i < names.Length; i++)
            {
                outcome.AddAlreadyPresent(plan.Sets[i].Set.Path, names[i], 2, i + 1).Asked = plan.Sets[i].Set.Describe();
            }

            using (XLWorkbook workbook = Written(GenericModelsReport.From(plan, outcome.Results), "Generic Models"))
            {
                List<string> column = FirstColumn(workbook.Worksheet(2));

                foreach (string name in names)
                {
                    Assert.That(column, Has.Some.EqualTo(name), name);
                }
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

        /// <summary>
        /// A name with a space at its end is written without it, so it meets the sheet it matches, and is refused
        /// here and not met by the workbook as a duplicate. The break: the same name beside other sheets is not.
        /// </summary>
        [Test]
        public void AnEdgeSpaceDoesNotHideTwoSheetsOfOneName()
        {
            Assert.That(GenericSheet.WhyRefused(ReportSheet, "Coverage", "Coverage "), Does.Contain("the Coverage sheet is named Coverage"));
            Assert.That(GenericSheet.WhyRefused(ReportSheet, "Coverage", " Generic"), Is.Null);
            Assert.That(GenericSheet.WhyRefused("Generic ", "Coverage", "Generic"), Does.Contain("the report's own sheet is named Generic "));
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
