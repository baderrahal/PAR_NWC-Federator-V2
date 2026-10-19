using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ClosedXML.Excel;
using Federator.Core.Disciplines;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F209, F234 and F244. The six checks gathered into one workbook per discipline, every row in its own discipline's, sorted
    /// by priority, then building, model and check, the Summary counting models and buildings, a code outside the list named and
    /// never dropped, and the four sheets read back off the disk.
    /// </summary>
    [TestFixture]
    public class DisciplineReportTests
    {
        private string folder;
        private static readonly ContainerNameSettings Names = new ContainerNameSettings();
        private static readonly DateTime Now = new DateTime(2026, 10, 19, 9, 0, 0);

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("disciplines");
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

        private static ModelFacts Model(string file, double x, string site)
        {
            return new ModelFacts(file).WithPlacement(x, 0, 0).WithSite(site).WithCad(new List<ElementFact>()).WithGeneric(new List<ElementFact>());
        }

        /// <summary>
        /// 1B06BC with its ST 34 m from AR, 1B06PS of AR and ST missing EL and ME, 1B06PG of EL alone, not built, and an HV file
        /// whose discipline gets no workbook. Every file a day old but one ST file of 1B06PS, nine days old.
        /// </summary>
        private static DisciplineReport Report(string package = "C06 itself")
        {
            string[] bc = { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL"), Nwc("1B06BC", "ST"), Nwc("1B06BC", "HV") };
            string[] ps = { Nwc("1B06PS", "AR"), Nwc("1B06PS", "ST") };
            string[] pg = { Nwc("1B06PG", "EL") };
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06BC", bc, new[] { Model(bc[0], 0, "S"), Model(bc[1], 0, "S"), Model(bc[2], 34014.4, "T1"), Model(bc[3], 50000.0, "S") }, null),
                new DisciplineGroup("1B06PS", ps, ps.Select(f => Model(f, 0, "S")), null),
                new DisciplineGroup("1B06PG", pg, null, "this group did not run in this run").NotBuilt()
            };
            List<FileDate> dates = bc.Concat(ps).Concat(pg).Select(f => new FileDate(f, f == ps[1] ? Now.AddDays(-9) : Now.AddDays(-1), null)).ToList();

            return DisciplineReport.For("C06", package, groups, dates, Now, new DisciplineReportSettings(), Names);
        }

        [Test]
        public void EveryRowIsInItsOwnDisciplinesWorkbookSortedByPriorityThenBuildingThenModel()
        {
            DisciplineReport report = Report();

            Assert.That(report.Disciplines.ToArray(), Is.EqualTo(new[] { "AR", "ST", "ME", "EL" }));
            IList<DisciplineIssue> st = report.IssuesFor("ST");
            Assert.That(st.All(i => i.Discipline == "ST"), Is.True);
            Assert.That(st.Select(i => i.Priority + " " + i.Check + " " + i.Building).ToArray(), Is.EqualTo(new[] { "High Coordinates 1B06BC", "Medium File age 1B06PS" }));
            IList<DisciplineIssue> el = report.IssuesFor("EL");
            Assert.That(el.Select(i => i.Priority + " " + i.Check + " " + i.Building).ToArray(), Is.EqualTo(new[] { "High Missing model 1B06PS" }),
                "the EL alone in 1B06PG is no row, Bader's order of 19 Oct 2026");
            IList<DisciplineIssue> me = report.IssuesFor("ME");
            Assert.That(me.Select(i => i.Building).ToArray(), Is.EqualTo(new[] { "1B06BC", "1B06PS" }), "the buildings in order within one priority");
        }

        [Test]
        public void TheSummaryCountsTheModelsJudgedAndThoseWithARowAndItsStatusFollowsThePriority()
        {
            DisciplineReport report = Report();

            foreach (string discipline in report.Disciplines)
            {
                IList<SummaryRow> summary = report.SummaryFor(discipline);
                Assert.That(summary.Select(s => s.Check).ToArray(), Is.EqualTo(CheckWords.All.ToArray()), "the six checks in Bader's order");

                foreach (SummaryRow row in summary.Where(r => r.Checked.HasValue))
                {
                    Assert.That(row.WithIssue, Is.EqualTo(report.IssuesFor(discipline).Where(i => i.Check == row.Check).Select(i => i.Row).Distinct().Count()), discipline + " " + row.Check);
                    Assert.That(row.Status, Is.EqualTo(row.WithIssue == 0 ? "OK" : CheckWords.PriorityOf(row.Check) == "High" ? "FIX" : "REVIEW"));
                }
            }

            SummaryRow coordinates = report.SummaryFor("ST").Single(s => s.Check == CheckWords.Coordinates);
            Assert.That(coordinates.Checked, Is.EqualTo(2));
            Assert.That(coordinates.WithIssue, Is.EqualTo(1));
            Assert.That(coordinates.Status, Is.EqualTo("FIX"));
            Assert.That(report.SummaryFor("ST").Single(s => s.Check == CheckWords.FileAge).Status, Is.EqualTo("REVIEW"));
            SummaryRow arNaming = report.SummaryFor("AR").Single(s => s.Check == CheckWords.Naming);
            Assert.That(arNaming.Status, Is.EqualTo("NOT CHECKED"));
            Assert.That(arNaming.Checked, Is.Null);
            Assert.That(arNaming.Fix, Is.EqualTo("AR names set the pattern the other disciplines are checked against"));
            Assert.That(report.SummaryFor("ME").Single(s => s.Check == CheckWords.Coordinates).Status, Is.EqualTo("NOT CHECKED"), "no ME model, never a pass");
        }

        [Test]
        public void ACodeOutsideTheListIsNamedInTheLogAndNeverDroppedInSilence()
        {
            DisciplineReport report = Report();

            Assert.That(report.LeftOut.Count, Is.GreaterThan(0));
            Assert.That(report.LeftOut.All(r => r.Discipline == "HV"), Is.True);
            Assert.That(report.Lines(), Has.Some.StartsWith("left out  " + report.LeftOut.Count + " rows of HV, which gets no workbook"));
        }

        [Test]
        public void TheLinesSayEachDisciplinesRowsAndChecksAndWhatWasNotRead()
        {
            IList<string> lines = Report().Lines();

            Assert.That(lines.First(l => l.StartsWith("ST  ")), Does.StartWith("ST  2 rows to fix, 0 element IDs, Coordinates 2 models 1 with issue, Missing model 2 buildings 0 with issue"));
            Assert.That(lines.First(l => l.StartsWith("AR  ")), Does.Contain("Naming not checked"));
            Assert.That(lines, Has.Some.EqualTo("Coordinates  1B06PG not read, its models were not read, this group did not run in this run"));
            Assert.That(lines, Has.Some.StartsWith("Naming  learned from 2 AR names"));
        }

        [Test]
        public void TheWorkbookIsFourSheetsInTheMocksOrderReadBackOffTheDisk()
        {
            DisciplineReport report = Report();
            string path = Path.Combine(folder, DisciplineWorkbook.NameFor("C06", "ST", new DisciplineReportSettings()));

            DisciplineWorkbook.Write(path, report, "ST");

            Assert.That(Path.GetFileName(path), Is.EqualTo("C06 ST checks.xlsx"), "the same name as before F244");
            string why;
            DisciplineWorkbookRead read = DisciplineWorkbook.Read(path, out why);
            Assert.That(why, Is.Null);
            Assert.That(read.ToFixRows, Is.EqualTo(2));
            Assert.That(read.ElementIdRows, Is.EqualTo(0));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                Assert.That(workbook.Worksheets.Select(s => s.Name).ToArray(), Is.EqualTo(new[] { "Summary", "Models", "To fix", "Element IDs" }));
                IXLWorksheet summary = workbook.Worksheet("Summary");
                Assert.That(summary.Cell("B1").GetString(), Is.EqualTo("C06   ST model checks"), "never C06 C06 itself");
                Assert.That(summary.Cell("B2").GetString(), Is.EqualTo("Run 19 Oct 2026     Reference: the AR model of each building     2 ST models in 2 buildings"));
                Assert.That(summary.Cell("B5").GetString(), Is.EqualTo("1 of 2 ST models need a fix."));
                Assert.That(Enumerable.Range(2, 6).Select(c => summary.Cell(7, c).GetString()).ToArray(), Is.EqualTo(DisciplineWorkbook.SummaryColumns));
                Assert.That(workbook.Worksheet("Models").Cell("B1").GetString(), Is.EqualTo("C06   ST models"));
                Assert.That(Enumerable.Range(2, 10).Select(c => workbook.Worksheet("Models").Cell(5, c).GetString()).ToArray(), Is.EqualTo(DisciplineWorkbook.ModelsColumns));
                IXLWorksheet toFix = workbook.Worksheet("To fix");
                Assert.That(toFix.Cell("B1").GetString(), Is.EqualTo("C06   ST to fix"));
                Assert.That(Enumerable.Range(2, 9).Select(c => toFix.Cell(5, c).GetString()).ToArray(), Is.EqualTo(DisciplineWorkbook.ToFixColumns));
                Assert.That(toFix.Cell("C6").GetString(), Is.EqualTo("High"));
                Assert.That(toFix.Cell("E6").GetString(), Is.EqualTo("ST-000001"));
                Assert.That(toFix.Cell("G6").GetString(), Is.EqualTo("34.0 m from the AR model. ST on site T1, AR on S"));
                Assert.That(toFix.Cell("C7").GetString(), Is.EqualTo("Medium"));
                Assert.That(toFix.Cell("G7").GetString(), Is.EqualTo("Saved 10 Oct 2026"));
                Assert.That(toFix.Cell("H7").GetDouble(), Is.EqualTo(9));
                Assert.That(toFix.Cell("I7").GetString(), Is.EqualTo("days"));
                Assert.That(workbook.Worksheet("Element IDs").Cell("B1").GetString(), Is.EqualTo("C06   ST element IDs"));

                foreach (IXLWorksheet sheet in workbook.Worksheets)
                {
                    Assert.That(sheet.ShowGridLines, Is.False, sheet.Name);
                    Assert.That(sheet.PageSetup.PageOrientation, Is.EqualTo(XLPageOrientation.Landscape), sheet.Name);
                    Assert.That(sheet.PageSetup.PaperSize, Is.EqualTo(XLPaperSize.A3Paper), sheet.Name);
                    Assert.That(sheet.PageSetup.PagesWide, Is.EqualTo(1), sheet.Name);
                    Assert.That(sheet.PageSetup.Footer.Left.GetText(XLHFOccurrence.OddPages), Is.EqualTo("&A"), sheet.Name);
                    Assert.That(sheet.PageSetup.Footer.Right.GetText(XLHFOccurrence.OddPages), Is.EqualTo("Page &P of &N"), sheet.Name);
                }
            }

            using (ZipArchive zip = ZipFile.OpenRead(path))
            {
                string book = new StreamReader(zip.GetEntry("xl/workbook.xml").Open()).ReadToEnd();
                Assert.That(book, Does.Contain("name=\"To fix\""));
                Assert.That(book, Does.Contain("_xlnm.Print_Titles"), "the header row repeats on every printed page");
            }
        }

        [Test]
        public void TheTitleOfASubfolderPackageNamesItAndTheLeadSaysWhereAllPass()
        {
            string[] files = { Nwc("1B06PS", "AR"), Nwc("1B06PS", "ST") };
            DisciplineReport report = DisciplineReport.For("C06", "Infrastructure and Landscape",
                new List<DisciplineGroup> { new DisciplineGroup("1B06PS", files, files.Select(f => Model(f, 0, "S")), null) },
                files.Select(f => new FileDate(f, Now.AddDays(-1), null)).ToList(), Now, new DisciplineReportSettings(), Names);
            string path = Path.Combine(folder, "st.xlsx");

            DisciplineWorkbook.Write(path, report, "ST");

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet summary = workbook.Worksheet("Summary");
                Assert.That(summary.Cell("B1").GetString(), Is.EqualTo("C06 Infrastructure and Landscape   ST model checks"));
                Assert.That(summary.Cell("B5").GetString(), Is.EqualTo("All 1 ST models pass every check."));
                Assert.That(summary.Cell("B20").GetString(), Is.Empty, "every reference is AR, so no grey line");
            }

            Assert.That(CheckWords.Lead("ME", 0, 3, 0, 0, false), Is.EqualTo("0 of 3 ME models need a fix."), "a model not checked is never said to pass");
            Assert.That(CheckWords.Lead("ME", 5, 18, 2, 1, true), Is.EqualTo("5 of 18 ME models need a fix.  2 buildings have no ME model.  1 model has CAD imports or Generic Models to review."));
        }

        [Test]
        public void ALowRowIsAGreyChipAndAWorkbookWithNothingToFixHoldsItsHeadersAlone()
        {
            ModelFacts me = Model(Nwc("1B06PH", "ME"), 0, "S").WithCad(new[] { new ElementFact("5", "a.dwg", "f", "t", "l") });
            DisciplineReport report = DisciplineReport.For("C06", "C06 itself",
                new List<DisciplineGroup> { new DisciplineGroup("1B06PH", new[] { me.File, Nwc("1B06PH", "AR") }, new[] { me, Model(Nwc("1B06PH", "AR"), 0, "S") }, null) },
                new List<FileDate>(), Now, new DisciplineReportSettings(), Names);
            string me1 = Path.Combine(folder, "me.xlsx");
            string ar1 = Path.Combine(folder, "ar.xlsx");

            DisciplineWorkbook.Write(me1, report, "ME");
            DisciplineWorkbook.Write(ar1, report, "AR");

            using (XLWorkbook workbook = new XLWorkbook(me1))
            {
                IXLCell low = workbook.Worksheet("To fix").Cell("C6");
                Assert.That(low.GetString(), Is.EqualTo("Low"));
                Assert.That(WorkbookComparison.Fill(low), Is.EqualTo("D5DBE3"));
                IXLRow ids = workbook.Worksheet("Element IDs").Row(6);
                Assert.That(ids.Cell(4).GetString(), Is.EqualTo("CAD import"));
                Assert.That(ids.Cell(7).GetDouble(), Is.EqualTo(5));
                Assert.That(workbook.Worksheet("Models").Cell("H6").GetString(), Is.EqualTo("UNKNOWN"), "no date handed in");
            }

            string why;
            DisciplineWorkbookRead ar = DisciplineWorkbook.Read(ar1, out why);
            Assert.That(ar.ToFixRows, Is.EqualTo(0));
            Assert.That(ar.ElementIdRows, Is.EqualTo(0));

            using (XLWorkbook workbook = new XLWorkbook(ar1))
            {
                Assert.That(workbook.Worksheet("To fix").Cell("B5").GetString(), Is.EqualTo("No."));
                Assert.That(workbook.Worksheet("To fix").AutoFilter.Range.RangeAddress.ToStringRelative(), Is.EqualTo("B5:J5"));
            }
        }

        [Test]
        public void TheReadBackLineSaysInCapitalsWhereTheRowsDiffer()
        {
            DisciplineReport report = Report();
            string path = Path.Combine(folder, "st.xlsx");
            DisciplineWorkbook.Write(path, report, "ST");
            string why;
            DisciplineWorkbookRead read = DisciplineWorkbook.Read(path, out why);

            Assert.That(DisciplineWorkbookWords.ReadBackLine(read, 2, 0, "ST", "C06"),
                Is.EqualTo("CHECKS   read back 2 of 2 rows to fix and 0 of 0 element ID rows, ST of C06"));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                workbook.Worksheet("To fix").Row(7).Clear();
                workbook.Save();
            }

            DisciplineWorkbookRead broken = DisciplineWorkbook.Read(path, out why);
            Assert.That(broken.ToFixRows, Is.EqualTo(1));
            Assert.That(DisciplineWorkbookWords.ReadBackLine(broken, 2, 0, "ST", "C06"), Does.Contain("THE ROWS TO FIX DIFFER"));
            Assert.That(DisciplineWorkbookWords.ReadBackLine(broken, 1, 3, "ST", "C06"), Does.Contain("THE ELEMENT ID ROWS DIFFER"));
        }

        [Test]
        public void TheNameOfAPackagesWorkbookReadsItsPathInWords()
        {
            DisciplineReportSettings settings = new DisciplineReportSettings();

            Assert.That(DisciplineWorkbook.NameFor(Path.Combine("C06", "Infrastructure and Landscape"), "AR", settings),
                Is.EqualTo("C06 Infrastructure and Landscape AR checks.xlsx"));
            Assert.That(() => settings.WorkbookSuffix = "a/b", Throws.ArgumentException);
        }

        [Test]
        public void AWorkbookThatWillNotReadSaysWhyAndGivesNoRows()
        {
            string bad = Path.Combine(folder, "not a workbook.xlsx");
            File.WriteAllText(bad, "text");
            string why;

            Assert.That(DisciplineWorkbook.Read(bad, out why), Is.Null);
            Assert.That(why, Is.Not.Null);

            string old = Path.Combine(folder, "old.xlsx");

            using (XLWorkbook workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add("ST");
                workbook.SaveAs(old);
            }

            Assert.That(DisciplineWorkbook.Read(old, out why), Is.Null);
            Assert.That(why, Is.EqualTo("it holds no sheet named To fix"));
        }
    }
}
