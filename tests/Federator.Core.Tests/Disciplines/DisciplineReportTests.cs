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
                Assert.That(summary.Cell("B5").GetString(), Is.EqualTo("1 of 2 ST models needs a fix."), "singular where x is 1, the lead's reading of 20 Oct");
                Assert.That(Enumerable.Range(2, 6).Select(c => summary.Cell(7, c).GetString()).ToArray(), Is.EqualTo(CheckWords.SummaryColumns.ToArray()));
                Assert.That(workbook.Worksheet("Models").Cell("B1").GetString(), Is.EqualTo("C06   ST models"));
                Assert.That(Enumerable.Range(2, 10).Select(c => workbook.Worksheet("Models").Cell(5, c).GetString()).ToArray(), Is.EqualTo(CheckWords.ModelsColumns.ToArray()));
                IXLWorksheet toFix = workbook.Worksheet("To fix");
                Assert.That(toFix.Cell("B1").GetString(), Is.EqualTo("C06   ST to fix"));
                Assert.That(Enumerable.Range(2, 9).Select(c => toFix.Cell(5, c).GetString()).ToArray(), Is.EqualTo(CheckWords.ToFixColumns.ToArray()));
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
                Assert.That(summary.Cell("B5").GetString(), Is.EqualTo("All 1 ST model passes every check."), "singular for one, the lead's reading of 20 Oct");
                Assert.That(summary.Cell("B20").GetString(), Is.Empty, "every reference is AR, so no grey line");
            }

            Assert.That(CheckWords.Lead("ME", 0, 3, 0, 0, false), Is.EqualTo("0 of 3 ME models need a fix."), "a model not checked is never said to pass");
            Assert.That(CheckWords.Lead("ME", 5, 18, 2, 1, true), Is.EqualTo("5 of 18 ME models need a fix.  2 buildings have no ME model.  1 model has CAD imports or Generic Models to review."));
            Assert.That(CheckWords.Lead("ME", 1, 1, 0, 0, false), Is.EqualTo("1 of 1 ME model needs a fix."));
            Assert.That(CheckWords.Lead("ME", 1, 18, 0, 0, false), Is.EqualTo("1 of 18 ME models needs a fix."));
            Assert.That(CheckWords.Lead("ME", 0, 1, 0, 0, false), Is.EqualTo("0 of 1 ME model needs a fix."));
            Assert.That(CheckWords.Lead("ME", 0, 18, 0, 0, true), Is.EqualTo("All 18 ME models pass every check."));
        }

        /// <summary>F244 attempt 2, the breaker's S2: a row on To fix, here a file nine days old, is never under All pass.</summary>
        [Test]
        public void TheLeadNeverSaysAllPassWhileToFixHoldsARow()
        {
            string[] files = { Nwc("1B06PS", "AR"), Nwc("1B06PS", "ST") };
            DisciplineReport report = DisciplineReport.For("C06", "C06 itself",
                new List<DisciplineGroup> { new DisciplineGroup("1B06PS", files, files.Select(f => Model(f, 0, "S")), null) },
                files.Select(f => new FileDate(f, Now.AddDays(-9), null)).ToList(), Now, new DisciplineReportSettings(), Names);
            string path = Path.Combine(folder, "st.xlsx");

            DisciplineWorkbook.Write(path, report, "ST");

            Assert.That(report.IssuesFor("ST").Count, Is.EqualTo(1));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                Assert.That(workbook.Worksheet("Summary").Cell("B5").GetString(), Is.EqualTo("0 of 1 ST model needs a fix."));
            }
        }

        /// <summary>
        /// F244 attempt 2, the breaker's S1: a model with a value not read and nothing found reads NOT CHECKED and never OK: a
        /// CAD walk not read, a site not read, a file of the group with no facts. A row found still wins.
        /// </summary>
        [Test]
        public void AModelWithAValueNotReadAndNothingFoundIsNotCheckedAndARowFoundStillWins()
        {
            // The EL whose site was not read sits in a group of its own, since the guard pass leaves a group with a site not read
            // unjudged on Coordinates, which would hide the other cases here. Its name keeps 1B06PS, so the one AR name's pattern,
            // which holds 1B06PS alike, finds no break in it.
            string[] files = { Nwc("1B06PS", "AR"), Nwc("1B06PS", "ST"), Nwc("1B06PS", "EL"), Nwc("1B06PS", "ME"), Nwc("1B06PS", "ST", "000002") };
            ModelFacts[] models =
            {
                Model(files[0], 0, "S"),
                Model(files[1], 0, "S").CadNot("the walk threw"),
                Model(files[3], 40000, "S").GenericNot("the walk threw")
            };
            ModelFacts el = Model(files[2], 0, "S").SiteNot("the read threw");
            DisciplineReport report = DisciplineReport.For("C06", "C06 itself",
                new List<DisciplineGroup>
                {
                    new DisciplineGroup("1B06PS", files.Where(f => f != files[2]), models, null),
                    new DisciplineGroup("1B06PS EL", new[] { files[2] }, new[] { el }, null)
                },
                files.Select(f => new FileDate(f, Now.AddDays(-1), null)).ToList(), Now, new DisciplineReportSettings(), Names);

            Assert.That(report.ModelsFor("AR").Single().Status, Is.EqualTo("OK"), "every value read and nothing found");
            Assert.That(report.ModelsFor("ST").Single(r => r.Model == "ST-000001").Status, Is.EqualTo("NOT CHECKED"), "its CAD walk not read");
            Assert.That(report.ModelsFor("ST").Single(r => r.Model == "ST-000002").Status, Is.EqualTo("NOT CHECKED"), "no facts of it");
            Assert.That(report.ModelsFor("EL").Single(r => r.Model == "EL-000001").Status, Is.EqualTo("NOT CHECKED"), "its site not read");
            Assert.That(report.ModelsFor("ME").Single().Status, Is.EqualTo("FIX"), "40 m away, found, wins over its Generic walk not read");
        }

        /// <summary>F244 attempt 3, the reviewer's B1: a model whose file date was not read reads NOT CHECKED, never OK.</summary>
        [Test]
        public void AModelWhoseDateWasNotReadIsNotChecked()
        {
            string[] files = { Nwc("1B06PS", "AR"), Nwc("1B06PS", "ST") };
            DisciplineReport report = DisciplineReport.For("C06", "C06 itself",
                new List<DisciplineGroup> { new DisciplineGroup("1B06PS", files, files.Select(f => Model(f, 0, "S")), null) },
                new List<FileDate> { new FileDate(files[0], Now.AddDays(-1), null), new FileDate(files[1], null, "the file is gone") },
                Now, new DisciplineReportSettings(), Names);

            Assert.That(report.ModelsFor("AR").Single().Status, Is.EqualTo("OK"));
            Assert.That(report.ModelsFor("ST").Single().Status, Is.EqualTo("NOT CHECKED"), "its date not read");
        }

        /// <summary>F244 attempt 3, the reviewer's B1: in a package with no AR file Naming could judge no name, so no model reads OK.</summary>
        [Test]
        public void AModelNamingCouldNotJudgeInAPackageWithNoReferenceFileIsNotChecked()
        {
            string[] files = { Nwc("1B06PS", "EL"), Nwc("1B06PS", "ST") };
            DisciplineReport report = DisciplineReport.For("C06", "C06 itself",
                new List<DisciplineGroup> { new DisciplineGroup("1B06PS", files, files.Select(f => Model(f, 0, "S")), null) },
                files.Select(f => new FileDate(f, Now.AddDays(-1), null)).ToList(), Now, new DisciplineReportSettings(), Names);

            Assert.That(report.ModelsFor("EL").Single().Status, Is.EqualTo("NOT CHECKED"));
            Assert.That(report.ModelsFor("ST").Single().Status, Is.EqualTo("NOT CHECKED"));
        }

        /// <summary>
        /// F244 attempt 3, the breaker's 4 and the reviewer's B1: a model whose site was not read is not counted as judged by
        /// Coordinates, as it is not on Models, and a model of the facts that is not among the group's files reads NOT CHECKED.
        /// </summary>
        [Test]
        public void AModelWhoseSiteWasNotReadIsNotJudgedByCoordinatesAndOneNotAmongTheFilesIsNotChecked()
        {
            string[] files = { Nwc("1B06PS", "AR"), Nwc("1B06PS", "EL") };
            ModelFacts[] models = { Model(files[0], 0, "S"), Model(files[1], 0, "S").SiteNot("the read threw"), Model(Nwc("1B06PS", "ST"), 0, "S") };
            DisciplineReport report = DisciplineReport.For("C06", "C06 itself",
                new List<DisciplineGroup> { new DisciplineGroup("1B06PS", files, models, null) },
                models.Select(m => new FileDate(m.File, Now.AddDays(-1), null)).ToList(), Now, new DisciplineReportSettings(), Names);

            SummaryRow coordinates = report.SummaryFor("EL").Single(s => s.Check == CheckWords.Coordinates);
            Assert.That(coordinates.Checked, Is.Null, "the one EL model's site was not read");
            Assert.That(coordinates.Status, Is.EqualTo("NOT CHECKED"));
            Assert.That(report.SummaryFor("AR").Single(s => s.Check == CheckWords.Coordinates).Checked, Is.Null,
                "Bader's decision on the guard pass: a building with a site not read is not judged on Coordinates, its AR neither");
            Assert.That(report.ModelsFor("ST").Single(r => r.Model == "ST-000001").Status, Is.EqualTo("NOT CHECKED"), "in the facts and not among the group's files");
        }

        private DisciplineReport OneBuilding(params ModelFacts[] models)
        {
            return DisciplineReport.For("C06", "C06 itself",
                new List<DisciplineGroup> { new DisciplineGroup("1B06PS", models.Select(m => m.File), models, null) },
                models.Select(m => new FileDate(m.File, Now.AddDays(-1), null)).ToList(), Now, new DisciplineReportSettings(), Names);
        }

        /// <summary>
        /// F244 guard pass, Bader's decision: a building with a placed model whose site was not read is not judged on Coordinates.
        /// (a) Its reference's site not read and ST 12 m off: ST NOT CHECKED, no row, not counted, and B5 never says all pass.
        /// </summary>
        [Test]
        public void GuardA_AReferenceWhoseSiteWasNotReadLeavesItsBuildingNotJudgedOnCoordinates()
        {
            DisciplineReport report = OneBuilding(Model(Nwc("1B06PS", "AR"), 0, "S").SiteNot("the read threw"), Model(Nwc("1B06PS", "ST"), 12000, "S"));
            string path = Path.Combine(folder, "st.xlsx");

            DisciplineWorkbook.Write(path, report, "ST");

            Assert.That(report.ModelsFor("ST").Single().Status, Is.EqualTo("NOT CHECKED"));
            Assert.That(report.IssuesFor("ST").Any(i => i.Check == CheckWords.Coordinates), Is.False);
            Assert.That(report.SummaryFor("ST").Single(s => s.Check == CheckWords.Coordinates).Checked, Is.Null, "no model of it counted");
            Assert.That(report.ModelsFor("ST").Single().Metres, Is.EqualTo(12.0), "its distance still shows as measured");
            Assert.That(report.Lines(), Has.Some.EqualTo("Coordinates  1B06PS not judged, the site of 1104-PAR-1B06PS-ZZZ-AR-MOD-000001.nwc was not read, so no model of it is judged on Coordinates"));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                Assert.That(workbook.Worksheet("Summary").Cell("B5").GetString(), Does.Not.StartWith("All"));
            }
        }

        /// <summary>(b) AR on Internal as read, EL on a shared site and ST's site not read: no internal origin row, AR NOT CHECKED.</summary>
        [Test]
        public void GuardB_AReferenceOnInternalBesideASiteNotReadIsNoRowAndNotChecked()
        {
            DisciplineReport report = OneBuilding(
                Model(Nwc("1B06PS", "AR"), 0, "Internal"), Model(Nwc("1B06PS", "EL"), 0, "S"), Model(Nwc("1B06PS", "ST"), 0, "S").SiteNot("the read threw"));

            Assert.That(report.IssuesFor("AR"), Is.Empty);
            Assert.That(report.IssuesFor("EL"), Is.Empty, "no Medium row either");
            Assert.That(report.ModelsFor("AR").Single().Status, Is.EqualTo("NOT CHECKED"));
            Assert.That(report.ModelsFor("EL").Single().Status, Is.EqualTo("NOT CHECKED"));
        }

        /// <summary>(c) ST's site not read and 40 m off: no row, and Coordinates' With issue is never above its Checked.</summary>
        [Test]
        public void GuardC_AFarModelWhoseSiteWasNotReadIsNoRowAndTheSummaryNeverDisagrees()
        {
            DisciplineReport report = OneBuilding(Model(Nwc("1B06PS", "AR"), 0, "S"), Model(Nwc("1B06PS", "ST"), 40000, "S").SiteNot("the read threw"));

            Assert.That(report.IssuesFor("ST"), Is.Empty);
            SummaryRow coordinates = report.SummaryFor("ST").Single(s => s.Check == CheckWords.Coordinates);
            Assert.That(coordinates.WithIssue ?? 0, Is.LessThanOrEqualTo(coordinates.Checked ?? 0));
            Assert.That(coordinates.Checked, Is.Null);
            Assert.That(report.ModelsFor("ST").Single().Status, Is.EqualTo("NOT CHECKED"));
        }

        /// <summary>
        /// F244 attempt 2, the reviewer's 3 and the breaker's S8: the grey line says only what the files show. 1B06XX has an AR
        /// file not placed, so it is named in its own words. 1B06YY has no AR file and an EL file, so it is never under no AR or
        /// EL. 1B06WL has no AR and 1B06M1 neither AR nor EL, the mock's two sentences.
        /// </summary>
        [Test]
        public void TheGreyLineSaysOnlyWhatTheFilesShow()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                Built("1B06M1", Model(Nwc("1B06M1", "ME"), 0, "S"), Model(Nwc("1B06M1", "ST"), 0, "S")),
                Built("1B06WL", Model(Nwc("1B06WL", "EL"), 0, "S"), Model(Nwc("1B06WL", "ME"), 0, "S")),
                Built("1B06XX", new ModelFacts(Nwc("1B06XX", "AR")).PlacementNot("no transform").WithSite("S"), Model(Nwc("1B06XX", "EL"), 0, "S"), Model(Nwc("1B06XX", "ME"), 0, "S")),
                Built("1B06YY", Model(Nwc("1B06YY", "ME"), 0, "S"), Model(Nwc("1B06YY", "EL"), 0, "S"))
            };
            DisciplineReport report = DisciplineReport.For("C06", "C06 itself", groups, new List<FileDate>(), Now, new DisciplineReportSettings(), Names);

            Assert.That(report.ReferencesLineFor("ME"), Is.EqualTo(
                "Where a building has no AR model, the EL model is the reference (1B06WL). Where it has no AR or EL, the ME model is (1B06M1)."
                + " In 1B06XX the AR model was not placed, so it is measured from the EL model."
                + " In 1B06YY, which has no AR model, the ME model is the reference."));
        }

        private static DisciplineGroup Built(string building, params ModelFacts[] models)
        {
            return new DisciplineGroup(building, models.Select(m => m.File), models, null);
        }

        /// <summary>F244 attempt 2, the breaker's cosmetic: a package two levels below the community is said in words, never with a backslash.</summary>
        [Test]
        public void ATitleSaysAPackageTwoLevelsDownInWords()
        {
            Assert.That(CheckWords.Heading("C06", Path.Combine("Infrastructure", "Roads")), Is.EqualTo("C06 Infrastructure Roads"));
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
