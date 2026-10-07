using System.IO;
using System.Xml.Linq;
using ClosedXML.Excel;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132 attempt 5 item 2, Bader's answer D to Q133: a clash only the mirror finds is
    /// named in the log, the workbook and the view as found by the mirror only. The log
    /// half is MirrorMerge.Lines. This is the workbook and the clash XML the HTML page is
    /// drawn from, both read off ClashRow.FoundOnlyByMirror through the one name a row is
    /// written under, ClashRow.WrittenName. The view is F114's, and WrittenName is the name
    /// it can read. The names in here are sample data.
    /// </summary>
    [TestFixture]
    public class MirrorRowNameTests
    {
        private const string Kept = "BLD-ME-Ducts-vs-BLD-ST-Columns";
        private const string Mirror = "BLD-ST-Columns-vs-BLD-ME-Ducts (mirror)";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorMirrorRowName");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static ClashRow Row(string name, string left, string right)
        {
            ClashRow row = new ClashRow();
            row.Name = name;
            row.Status = ClashStatus.New;
            row.Left.Name = left;
            row.Right.Name = right;
            return row;
        }

        /// <summary>
        /// A group's report after the merge: the kept test's own Clash1, and the mirror's own
        /// Clash1, which only the mirror found, added by the merge and marked by it. The mirror's
        /// Clash2 is the kept test's Clash1 found again, Approved under the mirror and New under
        /// the kept test, Q138.
        /// </summary>
        private static ClashReport Merged()
        {
            // An XML run over a new NWF, both tests created from the XML, the one case a mirror
            // is merged since F132 attempt 12.
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(Kept, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts));
            MirrorMerge merge = MirrorMerge.Of(MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked()))[0];

            Assert.That(merge.Pairs[0].MirrorName, Is.EqualTo(Mirror));

            ClashReport report = new ClashReport("1A02MM", "a report");
            TestReport kept = report.AddTest(Kept);
            kept.State = TestState.FoundClashes;
            kept.Add(Row("Clash1", "duct 1", "column 1"));
            merge.KeptFound("item 1", "item 101", ClashStatus.New, kept.Rows[0]);

            // The mirror's own report holds the two clashes handed, as the run leaves it.
            TestReport mirror = report.AddTest(Mirror);
            mirror.State = TestState.FoundClashes;
            mirror.Add(Row("Clash1", "column 8", "duct 7"));
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", mirror.Rows[0]);

            ClashRow approved = Row("Clash2", "column 1", "duct 1");

            approved.Status = ClashStatus.Approved;
            mirror.Add(approved);
            merge.MirrorFound(merge.Pairs[0], "item 101", "item 1", approved);
            merge.AddTo(report);

            Assert.That(kept.Rows.Count, Is.EqualTo(2));
            return report;
        }

        // Bader's answer B to Q138, the rule read by the workbook. The kept test's Clash1 is
        // New and the mirror's copy of it Approved, so the workbook writes Approved on its row,
        // and the block's one other row, the clash only the mirror found, is the only New.
        [Test]
        public void TheWorkbookShowsTheStatusTheRuleGivesAClashBothFound()
        {
            string path = new WorkbookWriter(new ReportOptions()).Write(Merged(), Path.Combine(folder, "status.xlsx"));
            string ownRow = null;
            string mirrorsRow = null;

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);
                int last = sheet.LastRowUsed().RowNumber();

                for (int row = 1; row <= last; row++)
                {
                    string name = sheet.Cell(row, WorkbookWriter.ColumnClashName).GetString();
                    string status = sheet.Cell(row, WorkbookWriter.ColumnStatus).GetString();

                    if (name == "Clash1")
                    {
                        ownRow = status;
                    }

                    if (name == "Clash1, found by the mirror only in " + Mirror)
                    {
                        mirrorsRow = status;
                    }
                }
            }

            Assert.That(ownRow, Is.EqualTo("Approved"));
            Assert.That(mirrorsRow, Is.EqualTo("New"));
        }

        // The same rule read by the clash XML the HTML page is drawn from: the row's status and
        // the test's summary count of Approved.
        [Test]
        public void TheClashXmlShowsTheStatusTheRuleGivesAClashBothFound()
        {
            XDocument xml = new ClashReportXml().Build(Merged());
            XElement test = null;

            foreach (XElement candidate in xml.Descendants("clashtest"))
            {
                if ((string)candidate.Attribute("name") == Kept)
                {
                    test = candidate;
                }
            }

            Assert.That(test, Is.Not.Null);
            Assert.That((string)test.Element("summary").Attribute("approved"), Is.EqualTo("1"));
            Assert.That((string)test.Element("summary").Attribute("new"), Is.EqualTo("1"), "the clash only the mirror found");

            string status = null;

            foreach (XElement result in test.Descendants("clashresult"))
            {
                if ((string)result.Attribute("name") == "Clash1")
                {
                    status = (string)result.Element("resultstatus");
                }
            }

            Assert.That(status, Is.EqualTo("Approved"));
        }

        [Test]
        public void ARowTheTestFoundIsWrittenUnderItsOwnName()
        {
            Assert.That(Row("Clash1", "a", "b").WrittenName(), Is.EqualTo("Clash1"));
        }

        // Both rows are Clash1 in Clash Detective, one under each test. Written as they are,
        // the block would hold two rows of one name, the breaker's finding, and nothing would
        // say which one the mirror alone found or where to find it in the panel.
        [Test]
        public void ARowOnlyTheMirrorFoundIsWrittenAsFoundByTheMirrorOnly()
        {
            TestReport kept = Merged().Tests[0];

            Assert.That(kept.Rows[0].WrittenName(), Is.EqualTo("Clash1"));
            Assert.That(kept.Rows[1].WrittenName(), Is.EqualTo("Clash1, found by the mirror only in " + Mirror));
        }

        [Test]
        public void TheWorkbookNamesAClashOnlyTheMirrorFound()
        {
            string path = new WorkbookWriter(new ReportOptions()).Write(Merged(), Path.Combine(folder, "merged.xlsx"));
            int found = 0;

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);
                int last = sheet.LastRowUsed().RowNumber();

                for (int row = 1; row <= last; row++)
                {
                    if (sheet.Cell(row, WorkbookWriter.ColumnClashName).GetString()
                        == "Clash1, found by the mirror only in " + Mirror)
                    {
                        found++;
                    }
                }
            }

            Assert.That(found, Is.EqualTo(1));
        }

        [Test]
        public void TheClashXmlThePageIsDrawnFromNamesAClashOnlyTheMirrorFound()
        {
            XDocument xml = new ClashReportXml().Build(Merged());
            int found = 0;

            foreach (XElement result in xml.Descendants("clashresult"))
            {
                if ((string)result.Attribute("name") == "Clash1, found by the mirror only in " + Mirror)
                {
                    found++;
                }
            }

            Assert.That(found, Is.EqualTo(1));
        }
    }
}
