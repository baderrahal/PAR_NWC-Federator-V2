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
    /// F209 and F234. The seven checks gathered into one workbook per discipline, every row in its own discipline's, sorted by
    /// severity, then building, then file, each summary count the rows of its check, a code outside the list named and never
    /// dropped, and the workbook read back off the disk in Bader's layout.
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
        private static DisciplineReport Report()
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

            return DisciplineReport.For("C06", "C06 itself", groups, dates, Now, new DisciplineReportSettings(), Names);
        }

        [Test]
        public void EveryRowIsInItsOwnDisciplinesWorkbookSortedBySeverityThenBuildingThenFile()
        {
            DisciplineReport report = Report();

            Assert.That(report.Disciplines.ToArray(), Is.EqualTo(new[] { "AR", "ST", "ME", "EL" }));
            IList<DisciplineIssue> st = report.IssuesFor("ST");
            Assert.That(st.All(i => i.Discipline == "ST"), Is.True);
            Assert.That(st.Select(i => i.Check + " " + i.Building).ToArray(), Is.EqualTo(new[] { "Coordinates 1B06BC", "File age 1B06PS" }));
            IList<DisciplineIssue> el = report.IssuesFor("EL");
            Assert.That(el.Select(i => i.Severity + " " + i.Check + " " + i.Building).ToArray(),
                Is.EqualTo(new[] { "High Missing model 1B06PS", "Medium One discipline group 1B06PG" }));
            IList<DisciplineIssue> me = report.IssuesFor("ME");
            Assert.That(me.Select(i => i.Building).ToArray(), Is.EqualTo(new[] { "1B06BC", "1B06PS" }), "the buildings in order within one severity");
        }

        [Test]
        public void EachSummaryCountIsTheRowsOfItsCheckInTheSameWorkbookAndTheResultSaysOkOrIssues()
        {
            DisciplineReport report = Report();

            foreach (string discipline in report.Disciplines)
            {
                IList<CheckSummary> summary = report.SummaryFor(discipline);
                Assert.That(summary.Select(s => s.Check).ToArray(), Is.EqualTo(CheckWords.All), "the seven checks in Bader's order");

                foreach (CheckSummary row in summary)
                {
                    Assert.That(row.Issues, Is.EqualTo(report.IssuesFor(discipline).Count(i => i.Check == row.Check)), discipline + " " + row.Check);
                    Assert.That(row.Result, Is.EqualTo(row.Issues == 0 ? "OK" : "Issues"));
                }
            }

            CheckSummary coordinates = report.SummaryFor("ST").Single(s => s.Check == CheckWords.Coordinates);
            Assert.That(coordinates.FilesChecked, Is.EqualTo(2));
            Assert.That(coordinates.Issues, Is.EqualTo(1));
            Assert.That(report.SummaryFor("AR").Single(s => s.Check == CheckWords.Naming).Result, Is.EqualTo("OK"));
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
        public void TheLinesSayEachDisciplinesFilesAndIssuesAndWhatWasNotRead()
        {
            IList<string> lines = Report().Lines();

            Assert.That(lines.First(l => l.StartsWith("ST  ")), Does.StartWith("ST  2 issue rows, Coordinates 2 files 1 issues"));
            Assert.That(lines, Has.Some.EqualTo("Coordinates  1B06PG not read, its models were not read, this group did not run in this run"));
            Assert.That(lines, Has.Some.StartsWith("Naming  learned from 2 AR names"));
        }

        [Test]
        public void TheWorkbookIsOneSheetInBadersLayoutReadBackOffTheDisk()
        {
            DisciplineReport report = Report();
            string path = Path.Combine(folder, DisciplineWorkbook.NameFor("C06", "ST", new DisciplineReportSettings()));

            DisciplineWorkbook.Write(path, report, "ST");

            Assert.That(Path.GetFileName(path), Is.EqualTo("C06 ST checks.xlsx"), "the same name as before F234");
            string why;
            DisciplineWorkbookRead read = DisciplineWorkbook.Read(path, out why);
            Assert.That(why, Is.Null);
            Assert.That(read.Title, Is.EqualTo("C06 C06 itself ST model checks"));
            Assert.That(read.Issues.Count, Is.EqualTo(2));
            Assert.That(read.SummaryIssues, Is.EqualTo(2));
            Assert.That(read.SummaryMatchesTheRows, Is.True);
            Assert.That(read.Issues[0].ToArray(), Is.EqualTo(report.IssuesFor("ST")[0].Cells(1).ToArray()));
            Assert.That(read.Issues[0][6], Is.EqualTo("34.014 m, ST on T1, AR on S"));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);
                int header = DisciplineWorkbook.IssuesHeaderRow;
                Assert.That(sheet.Cell(2, 1).GetString(), Is.EqualTo("Run date 2026-10-19"));
                Assert.That(sheet.Cell(3, 1).GetString(), Is.EqualTo("Reference discipline AR"));
                Assert.That(sheet.Cell(DisciplineWorkbook.SummaryHeaderRow, 1).GetString(), Is.EqualTo("Check"));
                Assert.That(sheet.Cell(header - 1, 1).GetString(), Is.Empty, "one blank row between the two tables");
                Assert.That(Enumerable.Range(1, 9).Select(c => sheet.Cell(header, c).GetString()).ToArray(), Is.EqualTo(CheckWords.IssueColumns));
                Assert.That(sheet.Cell(header, 1).Style.Font.Bold, Is.True);
                Assert.That(sheet.Cell(header, 1).Style.Fill.BackgroundColor, Is.EqualTo(DisciplineWorkbook.HeaderFill));
                Assert.That(sheet.SheetView.SplitRow, Is.EqualTo(header), "the header row frozen");
                Assert.That(sheet.AutoFilter.Range.FirstRow().RowNumber(), Is.EqualTo(header), "the filter on the header row");
                Assert.That(sheet.Cell(header + 1, 2).GetString(), Is.EqualTo("High"));
                Assert.That(sheet.Cell(header + 1, 1).Style.Fill.BackgroundColor, Is.EqualTo(DisciplineWorkbook.HighFill));
                Assert.That(sheet.Cell(header + 2, 2).GetString(), Is.EqualTo("Medium"));
                Assert.That(sheet.Cell(header + 2, 1).Style.Fill.BackgroundColor, Is.EqualTo(DisciplineWorkbook.MediumFill));
                Assert.That(sheet.Cell(header + 1, 6).Style.Alignment.WrapText, Is.True);
                Assert.That(sheet.Column(4).Width, Is.EqualTo(DisciplineWorkbook.Widths[3]));
            }

            using (ZipArchive zip = ZipFile.OpenRead(path))
            {
                Assert.That(zip.Entries.Count(e => e.FullName.StartsWith("xl/worksheets/sheet", StringComparison.Ordinal)), Is.EqualTo(1), "one sheet, never many");
                string book = new StreamReader(zip.GetEntry("xl/workbook.xml").Open()).ReadToEnd();
                Assert.That(book, Does.Contain("name=\"ST\""));
            }
        }

        [Test]
        public void ALowRowHasNoFillAndAWorkbookWithNoIssueHoldsItsHeaderAndSummaryAlone()
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
                IXLCell low = workbook.Worksheet(1).Cell(DisciplineWorkbook.IssuesHeaderRow + 1, 1);
                Assert.That(workbook.Worksheet(1).Cell(DisciplineWorkbook.IssuesHeaderRow + 1, 2).GetString(), Is.EqualTo("Low"));
                Assert.That(low.Style.Fill.BackgroundColor, Is.Not.EqualTo(DisciplineWorkbook.HighFill));
                Assert.That(low.Style.Fill.BackgroundColor, Is.Not.EqualTo(DisciplineWorkbook.MediumFill));
            }

            string why;
            DisciplineWorkbookRead ar = DisciplineWorkbook.Read(ar1, out why);
            Assert.That(ar.Issues.Count, Is.EqualTo(0));
            Assert.That(ar.SummaryMatchesTheRows, Is.True);
            Assert.That(ar.Summary.Count, Is.EqualTo(7));
        }

        [Test]
        public void TheReadBackLineSaysInCapitalsWhereTheSummaryAndTheRowsDiffer()
        {
            DisciplineReport report = Report();
            string path = Path.Combine(folder, "st.xlsx");
            DisciplineWorkbook.Write(path, report, "ST");
            string why;
            DisciplineWorkbookRead read = DisciplineWorkbook.Read(path, out why);

            Assert.That(DisciplineWorkbookWords.ReadBackLine(read, 2, "ST", "C06"),
                Is.EqualTo("CHECKS   read back 2 of 2 issue rows, the summary counts 2 issues, each check's issues the rows of that check, ST of C06"));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                workbook.Worksheet(1).Cell(DisciplineWorkbook.SummaryHeaderRow + 1, 3).SetValue(5);
                workbook.Save();
            }

            DisciplineWorkbookRead broken = DisciplineWorkbook.Read(path, out why);
            Assert.That(broken.SummaryMatchesTheRows, Is.False);
            Assert.That(DisciplineWorkbookWords.ReadBackLine(broken, 2, "ST", "C06"), Does.Contain("THE SUMMARY AND THE ROWS DIFFER"));
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
        }
    }
}
