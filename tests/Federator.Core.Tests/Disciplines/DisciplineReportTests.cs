using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Federator.Core.Disciplines;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F209. The six checks gathered into one workbook per discipline, every line in its own discipline's, the checks in
    /// Bader's order, a code outside the list named and never dropped, and the workbook read back off the disk.
    /// </summary>
    [TestFixture]
    public class DisciplineReportTests
    {
        private string folder;
        private static readonly ContainerNameSettings Names = new ContainerNameSettings();

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

        private static string Nwc(string building, string discipline)
        {
            return TestPaths.At("Parent", "C06", "1104-PAR-" + building + "-ZZZ-" + discipline + "-MOD-000001.nwc");
        }

        private static ModelFacts Read(string file)
        {
            return new ModelFacts(file).WithBox(0, 0, 0, 10, 10, 10).WithSite("S").WithCad(new List<ElementFact>()).WithGeneric(new List<ElementFact>());
        }

        private static DisciplineReport Report()
        {
            string[] bc = { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL"), Nwc("1B06BC", "HV") };
            string[] bs = { Nwc("1B06BS", "EL") };
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06BC", bc, bc.Select(Read), null),
                new DisciplineGroup("1B06BS", bs, null, "this group did not run in this run")
            };
            DateTime now = new DateTime(2026, 10, 18, 1, 0, 0);
            List<FileDate> dates = bc.Concat(bs).Select(f => new FileDate(f, now.AddDays(-1), null)).ToList();

            return DisciplineReport.For("C06", "C06", groups, dates, now, new DisciplineReportSettings(), Names);
        }

        [Test]
        public void EveryLineIsInItsOwnDisciplinesWorkbookAndTheChecksComeInBadersOrder()
        {
            DisciplineReport report = Report();

            Assert.That(report.Disciplines.ToArray(), Is.EqualTo(new[] { "AR", "ST", "ME", "EL" }));
            Assert.That(report.RowsFor("EL").All(r => r.Discipline == "EL"), Is.True);
            IList<string> checks = report.RowsFor("EL").Select(r => r.Check).Distinct().ToList();
            Assert.That(checks.ToArray(), Is.EqualTo(CheckWords.All), "all six checks, in order");
            Assert.That(report.RowsFor("AR").Single(r => r.Check == CheckWords.Missing).Value,
                Is.EqualTo("AR is in 1 of 2 groups of this package and not in 1B06BS, which holds EL"));
            Assert.That(report.RowsFor("ST").Single(r => r.Check == CheckWords.Missing).Result, Is.EqualTo(CheckWords.NotInThePackage),
                "a workbook for a discipline the package lacks still says what was checked");
        }

        [Test]
        public void ACodeOutsideTheListIsNamedInTheLogAndNeverDroppedInSilence()
        {
            DisciplineReport report = Report();

            Assert.That(report.LeftOut.Count, Is.GreaterThan(0));
            Assert.That(report.LeftOut.All(r => r.Discipline == "HV"), Is.True);
            Assert.That(report.Lines(), Has.Some.StartsWith("left out  " + report.LeftOut.Count + " lines of HV, which gets no workbook"));
        }

        [Test]
        public void TheLinesCountEachChecksResultsAndTheIdsNotReadable()
        {
            ModelFacts me = Read(Nwc("1B06PH", "ME")).WithGeneric(new[] { new ElementFact(null, "n", "f", "t", "l"), new ElementFact("5", "n", "f", "t", "l") });
            DisciplineReport report = DisciplineReport.For(
                "C06", "C06", new List<DisciplineGroup> { new DisciplineGroup("1B06PH", new[] { me.File }, new[] { me }, null) },
                new List<FileDate>(), DateTime.Now, new DisciplineReportSettings(), Names);

            Assert.That(report.IdsNotReadable("ME"), Is.EqualTo(1));
            Assert.That(report.Lines(), Has.Some.StartsWith("ME  "));
            Assert.That(report.Lines(), Has.Some.EqualTo("    Generic Models     2 found"));
            Assert.That(report.Lines().First(l => l.StartsWith("ME  ")), Does.EndWith("element ids not readable 1"));
        }

        [Test]
        public void TheWorkbookIsOneSheetWithBadersColumnsAndEveryLineReadBackOffTheDisk()
        {
            DisciplineReport report = Report();
            string path = Path.Combine(folder, DisciplineWorkbook.NameFor("C06", "EL", new DisciplineReportSettings()));

            DisciplineWorkbook.Write(path, "EL", report.RowsFor("EL"));

            Assert.That(Path.GetFileName(path), Is.EqualTo("C06 EL checks.xlsx"));
            string why;
            IList<IList<string>> rows = DisciplineWorkbook.Read(path, out why);
            Assert.That(why, Is.Null);
            Assert.That(rows.Count, Is.EqualTo(report.RowsFor("EL").Count));
            Assert.That(rows[0].ToArray(), Is.EqualTo(report.RowsFor("EL")[0].Cells().ToArray()));

            using (ZipArchive zip = ZipFile.OpenRead(path))
            {
                Assert.That(zip.Entries.Count(e => e.FullName.StartsWith("xl/worksheets/sheet", StringComparison.Ordinal)), Is.EqualTo(1), "one sheet, never many");
                string book = new StreamReader(zip.GetEntry("xl/workbook.xml").Open()).ReadToEnd();
                Assert.That(book, Does.Contain("name=\"EL\""));
                string strings = new StreamReader(zip.GetEntry("xl/sharedStrings.xml").Open()).ReadToEnd();

                foreach (string column in CheckWords.Columns)
                {
                    Assert.That(strings, Does.Contain(column), column);
                }
            }
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
