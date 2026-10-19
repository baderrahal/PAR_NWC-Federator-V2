using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ClosedXML.Excel;
using Federator.Core.Disciplines;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F244, Bader's order of 19 Oct 2026: the discipline workbook copied from the mock. The workbook written from a report of
    /// C06 ME built off the mock's own cells is opened beside docs\design\discipline-workbook-mock.xlsx and held to it sheet by
    /// sheet, and the comparison is broken one thing at a time to prove it names the cell.
    /// </summary>
    [TestFixture]
    public class DisciplineWorkbookMockTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("discipline-mock");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private string WriteMe()
        {
            string path = Path.Combine(folder, "C06 ME checks.xlsx");
            DisciplineWorkbook.Write(path, DisciplineMock.Report(), "ME");
            return path;
        }

        [Test]
        public void TheWorkbookOfC06MeMatchesTheMockSheetBySheet()
        {
            IList<string> differences = WorkbookComparison.Differences(WriteMe(), Samples.DisciplineWorkbookMock(), WorkbookComparison.OpenpyxlWidthPadding);

            Assert.That(differences, Is.Empty, string.Join("\n", differences));
        }

        /// <summary>The measurement the width comparison stands on: the mock's XML holds the script's 18 and ClosedXML reads it less its padding.</summary>
        [Test]
        public void TheMocksXmlHoldsTheWidthsAsItsScriptTypedThem()
        {
            using (ZipArchive zip = ZipFile.OpenRead(Samples.DisciplineWorkbookMock()))
            {
                string summary = new StreamReader(zip.GetEntry("xl/worksheets/sheet1.xml").Open()).ReadToEnd();
                Assert.That(summary, Does.Contain("<col width=\"18\" customWidth=\"1\" min=\"2\" max=\"2\""));
            }

            using (XLWorkbook mock = new XLWorkbook(Samples.DisciplineWorkbookMock()))
            {
                Assert.That(mock.Worksheet("Summary").Column(2).Width + WorkbookComparison.OpenpyxlWidthPadding, Is.EqualTo(18).Within(1e-9));
            }
        }

        [Test]
        public void TheComparisonNamesTheCellOfAWidthAFillAndAFormatBroken()
        {
            string written = WriteMe();
            string broken = Path.Combine(folder, "broken.xlsx");
            File.Copy(written, broken);

            using (XLWorkbook workbook = new XLWorkbook(broken))
            {
                workbook.Worksheet("Models").Column(3).Width = 20;
                workbook.Worksheet("Summary").Cell("F8").Style.Fill.BackgroundColor = XLColor.White;
                workbook.Worksheet("Models").Cell("F11").Style.NumberFormat.Format = "0.00";
                workbook.Save();
            }

            IList<string> differences = WorkbookComparison.Differences(broken, written, 0);

            Assert.That(differences.Count, Is.EqualTo(3), string.Join("\n", differences));
            Assert.That(differences, Has.Some.StartsWith("Models!C width"));
            Assert.That(differences, Has.Some.StartsWith("Summary!F8 chip fill"));
            Assert.That(differences, Has.Some.StartsWith("Models!F11 number format"));
        }
    }
}
