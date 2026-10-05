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
        private const string Mirror = Kept + " (mirror)";

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
        /// Clash1, which only the mirror found, added by the merge and marked by it.
        /// </summary>
        private static ClashReport Merged()
        {
            ClashTestPlan plan = MirrorRuleTests.SavedPlan(Kept, Mirror);
            MirrorMerge merge = MirrorMerge.Of(
                MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings()))[0];

            ClashReport report = new ClashReport("1A02MM", "a report");
            TestReport kept = report.AddTest(Kept);
            kept.State = TestState.FoundClashes;
            kept.Add(Row("Clash1", "duct 1", "column 1"));
            merge.KeptFound("item 1", "item 101", ClashStatus.New);

            report.AddTest(Mirror).State = TestState.FoundClashes;
            merge.MirrorFound(merge.Pairs[0], "item 7", "item 8", Row("Clash1", "column 8", "duct 7"));
            merge.AddTo(report);

            Assert.That(kept.Rows.Count, Is.EqualTo(2));
            return report;
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
