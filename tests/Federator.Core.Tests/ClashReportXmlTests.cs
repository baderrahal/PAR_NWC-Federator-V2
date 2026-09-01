using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The optional XML. Off by default, built from the same model the workbook is built
    /// from, and never read by or reading the workbook.
    ///
    /// The shape came off the three stylesheets Navisworks ships, because there is no
    /// clash report schema anywhere in the install. See docs/scan.md section 4h.
    /// </summary>
    [TestFixture]
    public class ClashReportXmlTests
    {
        private const string Root = "lcop_selection_set_tree";
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(Path.GetTempPath(), "FederatorClashXml", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void RemoveFolder()
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (IOException)
            {
                // A leftover temp folder is not worth failing a test over.
            }
        }

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001");
            report.SetTreeRoot = Root;
            report.DocumentUnits = "m";
            return report;
        }

        private static TestReport WithRows(ClashReport report)
        {
            TestReport test = report.AddTest("BLD-AR-Floors v BLD-ME-Ducts");
            test.LeftLocator = Root + "/Architecture/BLD-AR-Floors";
            test.RightLocator = Root + "/Mechanical/BLD-ME-Ducts";
            test.TestTypeName = "hard_conservative";
            test.Tolerance = 0.075;
            test.State = TestState.FoundClashes;

            ClashRow group = new ClashRow();
            group.Name = "Group1";
            group.IsGroup = true;
            group.RawClashes = 14;
            group.Status = ClashStatus.New;
            group.Distance = -0.145;
            group.GridLocation = "C-4";
            group.Level = "Level 03";
            group.Found = new DateTime(2026, 8, 31, 10, 15, 0);
            group.X = 12.5;
            group.Y = -3.25;
            group.Z = 9.0;
            group.Left.Name = "Floor 200mm";
            group.Left.Family = "Floor";
            group.Left.Type = "Generic 200mm";
            group.Left.Material = "Concrete";
            group.Left.SourceFile = "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc";
            group.Left.Discipline = "AR";
            group.Left.ElementId = "884213";
            group.Right.Name = "Duct";
            group.Right.Discipline = "ME";
            test.Add(group);

            ClashRow single = new ClashRow();
            single.Name = "Clash2";
            single.IsGroup = false;
            single.RawClashes = 1;
            single.Status = ClashStatus.Active;
            single.Distance = -0.02;
            test.Add(single);

            return test;
        }

        // ---------- the shape came off the stylesheets ----------

        [Test]
        public void TheRootIsExchangeCarryingItsUnits()
        {
            XDocument document = new ClashReportXml().Build(Report());

            Assert.That(document.Root.Name.LocalName, Is.EqualTo("exchange"));
            Assert.That(document.Root.Attribute("units").Value, Is.EqualTo("m"));
        }

        [Test]
        public void TheNestingIsBatchtestThenClashtestsThenClashtest()
        {
            ClashReport report = Report();
            WithRows(report);

            XElement root = new ClashReportXml().Build(report).Root;
            XElement batch = root.Element("batchtest");

            Assert.That(batch, Is.Not.Null);
            Assert.That(batch.Attribute("name"), Is.Not.Null);
            Assert.That(batch.Attribute("internal_name"), Is.Not.Null);

            XElement tests = batch.Element("clashtests");
            Assert.That(tests, Is.Not.Null);
            Assert.That(tests.Element("clashtest"), Is.Not.Null);
        }

        [Test]
        public void ATestCarriesItsNameTypeStatusAndTolerance()
        {
            ClashReport report = Report();
            WithRows(report);

            XElement test = new ClashReportXml().Build(report)
                .Root.Element("batchtest").Element("clashtests").Element("clashtest");

            Assert.That(test.Attribute("name").Value, Is.EqualTo("BLD-AR-Floors v BLD-ME-Ducts"));
            Assert.That(test.Attribute("test_type").Value, Is.EqualTo("hard_conservative"));
            Assert.That(test.Attribute("status"), Is.Not.Null);
            Assert.That(test.Attribute("tolerance").Value, Is.EqualTo("0.075"));
        }

        [Test]
        public void TheSummaryCarriesEveryStatusAndTheTotal()
        {
            ClashReport report = Report();
            WithRows(report);

            XElement summary = new ClashReportXml().Build(report)
                .Root.Element("batchtest").Element("clashtests").Element("clashtest")
                .Element("summary");

            Assert.That(summary.Attribute("new").Value, Is.EqualTo("14"));
            Assert.That(summary.Attribute("active").Value, Is.EqualTo("1"));
            Assert.That(summary.Attribute("reviewed").Value, Is.EqualTo("0"));
            Assert.That(summary.Attribute("approved").Value, Is.EqualTo("0"));
            Assert.That(summary.Attribute("resolved").Value, Is.EqualTo("0"));
            Assert.That(summary.Attribute("total").Value, Is.EqualTo("15"));
        }

        // A group is a clashgroup and a plain clash is a clashresult, which is how the
        // stylesheets treat them.
        [Test]
        public void AGroupIsAClashgroupAndAPlainClashIsAClashresult()
        {
            ClashReport report = Report();
            WithRows(report);

            XElement results = new ClashReportXml().Build(report)
                .Root.Element("batchtest").Element("clashtests").Element("clashtest")
                .Element("clashresults");

            Assert.That(results.Element("clashgroup"), Is.Not.Null);
            Assert.That(results.Element("clashresult"), Is.Not.Null);
            Assert.That(results.Element("clashgroup").Attribute("name").Value, Is.EqualTo("Group1"));
            Assert.That(results.Element("clashresult").Attribute("name").Value, Is.EqualTo("Clash2"));
        }

        [Test]
        public void AResultCarriesItsStatusPointGridAndDateAndBothItems()
        {
            ClashReport report = Report();
            WithRows(report);

            XElement group = new ClashReportXml().Build(report)
                .Root.Element("batchtest").Element("clashtests").Element("clashtest")
                .Element("clashresults").Element("clashgroup");

            Assert.That(group.Attribute("distance").Value, Is.EqualTo("-0.145"));
            Assert.That(group.Element("resultstatus").Value, Is.EqualTo("New"));
            // The single joined field, the same one the workbook and the client's own
            // report carry. It used to write the grid without its level.
            Assert.That(group.Element("gridlocation").Value, Is.EqualTo("C-4 : Level 03"));

            XElement point = group.Element("clashpoint").Element("pos3f");
            Assert.That(point.Attribute("x").Value, Is.EqualTo("12.5"));
            Assert.That(point.Attribute("y").Value, Is.EqualTo("-3.25"));
            Assert.That(point.Attribute("z").Value, Is.EqualTo("9"));

            XElement date = group.Element("createddate").Element("date");
            Assert.That(date.Attribute("year").Value, Is.EqualTo("2026"));
            Assert.That(date.Attribute("month").Value, Is.EqualTo("8"));
            Assert.That(date.Attribute("day").Value, Is.EqualTo("31"));

            List<XElement> objects = new List<XElement>(
                group.Element("clashobjects").Elements("clashobject"));

            Assert.That(objects.Count, Is.EqualTo(2), "a clash has two sides");
            Assert.That(objects[0].Element("layer").Value, Is.EqualTo("Level 03"));
        }

        // Family, type and material go in as objectattribute pairs, which is the shape the
        // stylesheets read arbitrary item properties out of.
        [Test]
        public void TheIdIsTheOneObjectattributeAndEverythingElseIsAQuickProperty()
        {
            ClashReport report = Report();
            WithRows(report);

            XElement first = new ClashReportXml().Build(report)
                .Root.Element("batchtest").Element("clashtests").Element("clashtest")
                .Element("clashresults").Element("clashgroup")
                .Element("clashobjects").Element("clashobject");

            // Exactly one, because the stylesheet's Item ID cell is value-of over
            // ./objectattribute/name and takes the FIRST. Writing several put the item's
            // Name in the id column.
            Assert.That(first.Elements("objectattribute").Count, Is.EqualTo(1));
            Assert.That(first.Element("objectattribute").Element("name").Value,
                Is.EqualTo("Element ID"));
            Assert.That(first.Element("objectattribute").Element("value").Value,
                Is.EqualTo("884213"));

            Dictionary<string, string> read = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (XElement tag in first.Element("smarttags").Elements("smarttag"))
            {
                read[tag.Element("name").Value] = tag.Element("value").Value;
            }

            // Theirs first, then ours, which is the order they appear as columns.
            Assert.That(read["Item Name"], Is.EqualTo("Floor 200mm"));
            Assert.That(read["Family"], Is.EqualTo("Floor"));
            Assert.That(read["Type Name"], Is.EqualTo("Generic 200mm"));
            Assert.That(read["Material"], Is.EqualTo("Concrete"));
            Assert.That(read["Discipline"], Is.EqualTo("AR"));
        }

        // Left out rather than written empty, so a blank is never read as a measured blank.
        [Test]
        public void AnEmptyPropertyIsLeftOutRatherThanWrittenBlank()
        {
            ClashReport report = Report();
            WithRows(report);

            XElement second = new List<XElement>(new ClashReportXml().Build(report)
                .Root.Element("batchtest").Element("clashtests").Element("clashtest")
                .Element("clashresults").Element("clashgroup")
                .Element("clashobjects").Elements("clashobject"))[1];

            foreach (XElement attribute in second.Elements("objectattribute"))
            {
                Assert.That(attribute.Element("value").Value, Is.Not.Empty,
                    "an empty value was written, which reads as a measured blank");
            }
        }

        [Test]
        public void ASkippedTestStillAppearsAndCarriesNoResults()
        {
            ClashReport report = Report();
            TestReport skipped = report.AddTest("skipped one");
            skipped.State = TestState.Skipped;

            XElement test = new ClashReportXml().Build(report)
                .Root.Element("batchtest").Element("clashtests").Element("clashtest");

            Assert.That(test.Attribute("status").Value, Is.EqualTo("skipped"));
            Assert.That(test.Element("clashresults"), Is.Null,
                "a test that never ran must not carry a results element");
            Assert.That(test.Element("summary").Attribute("total").Value, Is.EqualTo("0"));
        }

        // ---------- it writes a real file, and says what it left out ----------

        [Test]
        public void ItWritesAFileThatReadsBackAsXml()
        {
            ClashReport report = Report();
            WithRows(report);

            string path = new ClashReportXml().Write(
                report, ReportPaths.Xml(folder, report.OutputName));

            Assert.That(File.Exists(path), Is.True);
            Assert.That(path, Does.EndWith(".xml"));

            XDocument reread = XDocument.Load(path);
            Assert.That(reread.Root.Name.LocalName, Is.EqualTo("exchange"));
        }

        [Test]
        public void ItSaysWhichPartsItFilledAndWhichItLeftOut()
        {
            string explained = string.Join("\n", new List<string>(ClashReportXml.Explain()).ToArray());

            Assert.That(explained, Does.Contain("clash_report_html.xsl"));
            Assert.That(explained, Does.Contain("no clash report schema"));
            Assert.That(explained, Does.Contain("filled  :"));
            Assert.That(explained, Does.Contain("left out:"));
            Assert.That(ClashReportXml.Filled, Does.Contain("clashgroup"));
            Assert.That(ClashReportXml.LeftOut, Does.Contain("clashtasklink"));
        }

        // Nothing here may read the workbook and nothing there may read this. The XML is
        // built from the model alone.
        [Test]
        public void TheXmlIsBuiltWithoutAnyWorkbookExisting()
        {
            ClashReport report = Report();
            WithRows(report);

            string path = new ClashReportXml().Write(
                report, ReportPaths.Xml(folder, report.OutputName));

            Assert.That(Directory.GetFiles(folder, "*.xlsx").Length, Is.EqualTo(0),
                "the XML writer needed a workbook, which it must never do");
            Assert.That(File.Exists(path), Is.True);
        }

        [Test]
        public void NoReportOrNoPathIsRefusedRatherThanWritten()
        {
            Assert.Throws<ArgumentNullException>(delegate { new ClashReportXml().Build(null); });
            Assert.Throws<ArgumentException>(
                delegate { new ClashReportXml().Write(Report(), null); });
        }
    }
}
