using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Federator.Core.Disciplines;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F244, the rules of Bader's order of 19 Oct 2026 that F245 items 3 to 7 name, each read off the written workbook as the
    /// file it is: a reference on Revit's internal origin, a group of one discipline, numbers stored as numbers, Missing model
    /// counted in buildings and every model's distance on Models.
    /// </summary>
    [TestFixture]
    public class DisciplineWorkbookSheetsTests
    {
        private string folder;
        private static readonly ContainerNameSettings Names = new ContainerNameSettings();
        private static readonly DateTime Now = new DateTime(2026, 10, 19, 9, 0, 0);
        private const string HighFill = "FDE2E1";
        private const string LowFill = "FFF4D6";
        private const string Grey = "6B7785";

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("discipline-sheets");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static string Nwc(string building, string discipline, string number = "000001")
        {
            return TestPaths.At("Parent", "C06", "1104-PAR-" + building + "-ZZZ-" + discipline + "-MOD-" + number + ".nwc");
        }

        private static ModelFacts Model(string file, double x, double y, string site)
        {
            return new ModelFacts(file).WithPlacement(x, y, 0).WithSite(site).WithCad(new List<ElementFact>()).WithGeneric(new List<ElementFact>());
        }

        private static DisciplineGroup Group(string building, params ModelFacts[] models)
        {
            return new DisciplineGroup(building, models.Select(m => m.File), models, null);
        }

        private static DisciplineReport Report(IList<DisciplineGroup> groups, int days = 1)
        {
            List<FileDate> dates = groups.SelectMany(g => g.Files).Select(f => new FileDate(f, Now.AddDays(-days), null)).ToList();
            return DisciplineReport.For("C06", "C06 itself", groups, dates, Now, new DisciplineReportSettings(), Names);
        }

        private XLWorkbook Written(DisciplineReport report, string discipline)
        {
            string path = Path.Combine(folder, discipline + ".xlsx");
            DisciplineWorkbook.Write(path, report, discipline);
            return new XLWorkbook(path);
        }

        private static List<IXLRow> Rows(IXLWorksheet sheet)
        {
            IXLRow last = sheet.LastRowUsed();
            return last == null || last.RowNumber() <= 5 ? new List<IXLRow>() : Enumerable.Range(6, last.RowNumber() - 5).Select(r => sheet.Row(r)).ToList();
        }

        /// <summary>The row of Models whose File name cell, column K, is the file's name.</summary>
        private static IXLRow ModelsRow(XLWorkbook workbook, string file)
        {
            return Rows(workbook.Worksheet("Models")).Single(r => r.Cell(11).GetString() == DisciplineOf.FileName(file));
        }

        [Test]
        public void F245Item3_AReferenceOnInternalOriginIsItsOwnHighRowAndEveryOtherModelOfItsBuildingAMediumRowNeverAHighOne()
        {
            DisciplineReport report = Report(new List<DisciplineGroup>
            {
                Group("1B06BC",
                    Model(Nwc("1B06BC", "AR"), 0, 0, "Internal"),
                    Model(Nwc("1B06BC", "ST"), -33975.57, -1624.92, "T1"),
                    Model(Nwc("1B06BC", "ME"), 500, 0, "16-S01"))
            });

            using (XLWorkbook ar = Written(report, "AR"))
            {
                IXLRow row = Rows(ar.Worksheet("To fix")).Single();
                Assert.That(row.Cell(3).GetString(), Is.EqualTo("High"));
                Assert.That(row.Cell(6).GetString(), Is.EqualTo("Coordinates"));
                Assert.That(row.Cell(7).GetString(), Is.EqualTo("On Revit's internal origin, not on a shared site"));
                Assert.That(row.Cell(10).GetString(), Is.EqualTo("Export on the project shared coordinates. The other models of this building are measured from this one"));

                IXLRow model = ModelsRow(ar, Nwc("1B06BC", "AR"));
                Assert.That(model.Cell(4).GetString(), Is.EqualTo("FIX"));
                Assert.That(model.Cell(6).GetString(), Is.EqualTo("internal origin"));
                Assert.That(WorkbookComparison.Fill(model.Cell(6)), Is.EqualTo(HighFill));
            }

            using (XLWorkbook st = Written(report, "ST"))
            {
                List<IXLRow> rows = Rows(st.Worksheet("To fix"));
                Assert.That(rows.Any(r => r.Cell(3).GetString() == "High"), Is.False, "a model measured from a reference on internal origin is never a High row");
                IXLRow row = rows.Single();
                Assert.That(row.Cell(3).GetString(), Is.EqualTo("Medium"));
                Assert.That(row.Cell(6).GetString(), Is.EqualTo("Coordinates"));
                Assert.That(row.Cell(7).GetString(), Is.EqualTo("AR of this building is on Revit's internal origin, so this distance cannot be judged"));
                Assert.That(Math.Round(row.Cell(8).GetDouble(), 3), Is.EqualTo(34.014));
                Assert.That(row.Cell(10).GetString(), Is.EqualTo("Fix AR first, then run again"));

                IXLRow model = ModelsRow(st, Nwc("1B06BC", "ST"));
                Assert.That(model.Cell(4).GetString(), Is.EqualTo("REVIEW"));
                Assert.That(Math.Round(model.Cell(6).GetDouble(), 3), Is.EqualTo(34.014));
                Assert.That(WorkbookComparison.Fill(model.Cell(6)), Is.EqualTo(LowFill));
            }

            using (XLWorkbook me = Written(report, "ME"))
            {
                IXLRow model = ModelsRow(me, Nwc("1B06BC", "ME"));
                Assert.That(model.Cell(4).GetString(), Is.EqualTo("REVIEW"), "under a metre, still not judged");
                Assert.That(model.Cell(6).GetString(), Is.EqualTo("under 1"));
                Assert.That(WorkbookComparison.Fill(model.Cell(6)), Is.EqualTo(LowFill));
            }
        }

        [Test]
        public void F245Item4_AGroupOfOneDisciplineIsNoRowOnToFixOrSummaryAndItsModelsAreAloneOnModels()
        {
            DisciplineReport report = Report(new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PG", new[] { Nwc("1B06PG", "EL"), Nwc("1B06PG", "EL", "000002") }, null, "this group did not run in this run").NotBuilt(),
                Group("1B06PS", Model(Nwc("1B06PS", "AR"), 0, 0, "S"), Model(Nwc("1B06PS", "EL"), 0, 0, "S"))
            }, 9);

            using (XLWorkbook el = Written(report, "EL"))
            {
                Assert.That(Rows(el.Worksheet("To fix")).Any(r => r.Cell(4).GetString() == "1B06PG"), Is.False);
                IXLWorksheet summary = el.Worksheet("Summary");
                Assert.That(Enumerable.Range(8, 6).Select(r => summary.Cell(r, 2).GetString()).ToArray(),
                    Is.EqualTo(new[] { "Coordinates", "Missing model", "Naming", "File age", "CAD imports", "Generic Models" }));
                Assert.That(summary.CellsUsed().Any(c => c.GetString() == "One discipline group"), Is.False);
                Assert.That(summary.Cell("D8").GetDouble(), Is.EqualTo(1), "the one EL model measured, never those of 1B06PG");
                Assert.That(summary.Cell("D11").GetDouble(), Is.EqualTo(1), "File age counts the EL model of 1B06PS alone");

                foreach (string file in new[] { Nwc("1B06PG", "EL"), Nwc("1B06PG", "EL", "000002") })
                {
                    IXLRow model = ModelsRow(el, file);
                    Assert.That(model.Cell(4).GetString(), Is.EqualTo("NOT CHECKED"));
                    Assert.That(model.Cell(5).GetString(), Is.EqualTo("none"));
                    Assert.That(WorkbookComparison.Hex(model.Cell(5).Style.Font.FontColor), Is.EqualTo(Grey));
                    Assert.That(model.Cell(6).GetString(), Is.EqualTo("alone"));
                    Assert.That(WorkbookComparison.Hex(model.Cell(6).Style.Font.FontColor), Is.EqualTo(Grey));
                    Assert.That(WorkbookComparison.Fill(model.Cell(8)), Is.Not.EqualTo(LowFill), "nine days old and not highlighted");
                }
            }
        }

        [Test]
        public void F245Item5_EveryCountDistanceAgeAndElementIdIsANumberCellNeverText()
        {
            ModelFacts far = Model(Nwc("1B06PH", "ME", "000002"), 118682, 0, "ARD")
                .WithCad(new[] { new ElementFact("1819523", "Chem-Feed Pkg-R0.dwg", null, null, null), new ElementFact("1819548", "Chem-Feed Pkg-R0.dwg", null, null, null) })
                .WithGeneric(new[] { new ElementFact("2116825", null, "MEC-GEN-Base-PAR", "SPAL- MECH EQ -Base", null) });
            DisciplineReport report = Report(new List<DisciplineGroup>
            {
                Group("1B06PH", Model(Nwc("1B06PH", "AR"), 0, 0, "PWPS"), Model(Nwc("1B06PH", "ME"), 8110, 0, "PWPS"), far),
                Group("1B06PS", Model(Nwc("1B06PS", "AR"), 0, 0, "S"), Model(Nwc("1B06PS", "EL"), 0, 0, "S"), Model(Nwc("1B06PS", "ST"), 0, 0, "S"))
            }, 9);

            using (XLWorkbook me = Written(report, "ME"))
            {
                IXLWorksheet summary = me.Worksheet("Summary");

                for (int r = 8; r <= 13; r++)
                {
                    Assert.That(summary.Cell(r, 4).DataType, Is.EqualTo(XLDataType.Number), "Summary D" + r);
                    Assert.That(summary.Cell(r, 5).DataType, Is.EqualTo(XLDataType.Number), "Summary E" + r);
                }

                foreach (IXLRow row in Rows(me.Worksheet("Models")).Where(r => r.Cell(4).GetString() != "MISSING"))
                {
                    foreach (int c in new[] { 8, 9, 10 })
                    {
                        Assert.That(row.Cell(c).DataType, Is.EqualTo(XLDataType.Number), "Models " + row.Cell(c).Address);
                    }
                }

                Assert.That(ModelsRow(me, Nwc("1B06PH", "ME")).Cell(6).DataType, Is.EqualTo(XLDataType.Number), "8.11 m");
                Assert.That(ModelsRow(me, far.File).Cell(6).DataType, Is.EqualTo(XLDataType.Number), "118.682 m");

                List<IXLRow> toFix = Rows(me.Worksheet("To fix"));
                Assert.That(toFix.Count, Is.EqualTo(6), "far, missing, two file ages, one CAD file and one family");

                foreach (IXLRow row in toFix)
                {
                    Assert.That(row.Cell(2).DataType, Is.EqualTo(XLDataType.Number), "To fix " + row.Cell(2).Address);

                    if (!row.Cell(8).IsEmpty())
                    {
                        Assert.That(row.Cell(8).DataType, Is.EqualTo(XLDataType.Number), "To fix " + row.Cell(8).Address);
                    }
                }

                List<IXLRow> ids = Rows(me.Worksheet("Element IDs"));
                Assert.That(ids.Count, Is.EqualTo(3));
                Assert.That(ids.All(r => r.Cell(7).DataType == XLDataType.Number), Is.True);

                foreach (IXLWorksheet sheet in me.Worksheets)
                {
                    Assert.That(sheet.CellsUsed().Any(c => c.HasFormula), Is.False, sheet.Name + " holds no formula");
                    Assert.That(sheet.MergedRanges.Count, Is.EqualTo(0), sheet.Name + " holds no merged cell");
                }
            }
        }

        [Test]
        public void F245Item6_MissingModelCountsTheBuildingsCheckedNeverTheirFiles()
        {
            DisciplineReport report = Report(new List<DisciplineGroup>
            {
                Group("1B06PS", Model(Nwc("1B06PS", "AR"), 0, 0, "S"), Model(Nwc("1B06PS", "AR", "000002"), 0, 0, "S"),
                    Model(Nwc("1B06PS", "ST"), 0, 0, "S"), Model(Nwc("1B06PS", "ST", "000002"), 0, 0, "S"), Model(Nwc("1B06PS", "EL"), 0, 0, "S")),
                Group("1B06PT", Model(Nwc("1B06PT", "AR"), 0, 0, "S"), Model(Nwc("1B06PT", "ST"), 0, 0, "S")),
                Group("1B06PU", Model(Nwc("1B06PU", "AR"), 0, 0, "S"), Model(Nwc("1B06PU", "ME"), 0, 0, "S"))
            });

            using (XLWorkbook me = Written(report, "ME"))
            {
                IXLWorksheet summary = me.Worksheet("Summary");
                Assert.That(summary.Cell("B9").GetString(), Is.EqualTo("Missing model"));
                Assert.That(summary.Cell("D9").GetDouble(), Is.EqualTo(3), "three buildings, where the files are nine");
                Assert.That(summary.Cell("D9").Style.NumberFormat.Format, Is.EqualTo("[=1]0\" building\";0\" buildings\""));
                Assert.That(summary.Cell("E9").GetDouble(), Is.EqualTo(2));
                Assert.That(summary.Cell("E9").Style.NumberFormat.Format, Is.EqualTo("[=1]0\" building\";[>1]0\" buildings\";\"none\""),
                    "Bader's decision of 20 Oct 2026, a count above 1 never printed none");
            }
        }

        [Test]
        public void F245Item7_EveryModelMeasuredHasItsDistanceOnModelsNotOnlyTheFarOnes()
        {
            DisciplineReport report = Report(new List<DisciplineGroup>
            {
                Group("1B06BC", Model(Nwc("1B06BC", "AR"), 0, 0, "S"), Model(Nwc("1B06BC", "ST"), 5000, 0, "S"),
                    Model(Nwc("1B06BC", "EL"), 500, 0, "S"), Model(Nwc("1B06BC", "ME"), 40000, 0, "S"))
            });

            using (XLWorkbook st = Written(report, "ST"))
            {
                IXLCell off = ModelsRow(st, Nwc("1B06BC", "ST")).Cell(6);
                Assert.That(off.GetDouble(), Is.EqualTo(5.0));
                Assert.That(off.Style.NumberFormat.Format, Is.EqualTo("#,##0.0"));
                Assert.That(WorkbookComparison.Fill(off), Is.EqualTo("none"), "within 30 m");
                Assert.That(ModelsRow(st, Nwc("1B06BC", "ST")).Cell(5).GetString(), Is.EqualTo("AR"));
            }

            using (XLWorkbook el = Written(report, "EL"))
            {
                Assert.That(ModelsRow(el, Nwc("1B06BC", "EL")).Cell(6).GetString(), Is.EqualTo("under 1"));
            }

            using (XLWorkbook me = Written(report, "ME"))
            {
                IXLCell off = ModelsRow(me, Nwc("1B06BC", "ME")).Cell(6);
                Assert.That(off.GetDouble(), Is.EqualTo(40.0));
                Assert.That(WorkbookComparison.Fill(off), Is.EqualTo(HighFill), "over 30 m");
            }

            using (XLWorkbook ar = Written(report, "AR"))
            {
                IXLRow row = ModelsRow(ar, Nwc("1B06BC", "AR"));
                Assert.That(row.Cell(5).GetString(), Is.EqualTo("itself"));
                Assert.That(row.Cell(6).GetString(), Is.EqualTo("reference"));
            }
        }
    }
}
