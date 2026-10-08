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
    /// The Generic Models workbook of a group, F128 and FR-177: a workbook of its own beside the group's
    /// workbook, holding the Generic Models sheet alone, written into a folder that may not be there yet.
    /// Every workbook here is a real xlsx read back off the disk.
    /// </summary>
    [TestFixture]
    public class GenericWorkbookTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorGenericWorkbook");
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
                outcome.AddCreated(plan.Sets[i].Set, counts[i], null);
            }

            return GenericModelsReport.From(plan, outcome.Results);
        }

        [Test]
        public void TheWorkbookHoldsTheOneSheetNamedAsAskedAndTheFolderIsMade()
        {
            GenericModelsSettings settings = new GenericModelsSettings();
            string path = ReportPaths.Workbook(Path.Combine(folder, "Clash Reports"), settings.WorkbookNameFor("1A04PK"));

            GenericWorkbook.Write(path, Report(528, 0, 3), "1A04PK", settings.SheetName);

            Assert.That(path, Does.EndWith(Path.Combine("Clash Reports", "1A04PK Generic Models.xlsx")));
            Assert.That(File.Exists(path), Is.True);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                Assert.That(workbook.Worksheets.Count, Is.EqualTo(1));
                Assert.That(workbook.Worksheet(1).Name, Is.EqualTo("Generic Models"));
                Assert.That(workbook.Worksheet(1).Cell(1, 1).GetString(), Is.EqualTo(GenericSheet.Title));
                Assert.That(workbook.Worksheet(1).Cell(2, 1).GetString(), Is.EqualTo("Group: 1A04PK"));
                Assert.That(workbook.Worksheet(1).Cell(6, 1).GetString(), Is.EqualTo("model0"));
                Assert.That(workbook.Worksheet(1).Cell(6, 2).GetDouble(), Is.EqualTo(528));
                Assert.That(workbook.Worksheet(1).Cell(7, 1).GetString(), Is.EqualTo("model2"));
            }
        }

        /// <summary>Overwrites, as every output of a group does, so last week's file never stands as this week's.</summary>
        [Test]
        public void ASecondWriteOverwritesTheFirst()
        {
            string path = Path.Combine(folder, "g.xlsx");

            GenericWorkbook.Write(path, Report(1, 2), "g", "Generic Models");
            GenericWorkbook.Write(path, Report(7), "g", "Generic Models");

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                Assert.That(workbook.Worksheet(1).Cell(6, 2).GetDouble(), Is.EqualTo(7));
                Assert.That(workbook.Worksheet(1).Cell(7, 1).GetString(), Is.Not.EqualTo("model1"));
            }
        }

        [Test]
        public void APathOrAReportMissingIsRefused()
        {
            Assert.Throws<ArgumentException>(() => GenericWorkbook.Write(string.Empty, Report(1), "g", "Generic Models"));
            Assert.Throws<ArgumentNullException>(() => GenericWorkbook.Write(Path.Combine(folder, "g.xlsx"), null, "g", "Generic Models"));
        }
    }
}
