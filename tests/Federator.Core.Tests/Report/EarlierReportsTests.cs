using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The breaker's third finding at c5d8aa8. A group whose clash is skipped writes no
    /// report, and the workbook, the page, the XML and the pictures an earlier run wrote
    /// under the same undated names stay in the Clash Report folder and read as current. The
    /// tool reports and never acts, so the answer is words: each one is named, with when it
    /// was last written and its size read off the disk by its exact path, and none is
    /// touched.
    /// </summary>
    [TestFixture]
    public class EarlierReportsTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorEarlierReports");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static OutputPlan Wanted(bool workbook, bool xml, bool html, bool images)
        {
            ReportOptions options = new ReportOptions();
            options.WriteWorkbook = workbook;
            options.WriteXml = xml;
            options.WriteHtml = html;
            options.Images.Write = images;
            return OutputPlan.From(options);
        }

        private static string Written(string path, string text, DateTime at)
        {
            File.WriteAllText(path, text);
            File.SetLastWriteTime(path, at);
            return path;
        }

        [Test]
        public void EachReportOfAnEarlierRunAtTheNamesThisRunWouldWriteIsNamedWithItsTimeAndSize()
        {
            string workbook = Written(ReportPaths.Workbook(folder, "1B06K1"), "12345", new DateTime(2026, 9, 28, 14, 2, 0));
            string xml = Written(ReportPaths.Xml(folder, "1B06K1"), "abc", new DateTime(2026, 9, 28, 14, 3, 0));
            string pictures = ImageNaming.FolderFor(workbook);
            Directory.CreateDirectory(pictures);
            Directory.SetLastWriteTime(pictures, new DateTime(2026, 9, 28, 14, 4, 0));

            IList<string> lines = EarlierReports.Lines(Wanted(true, true, true, true), folder, "1B06K1");

            Assert.That(lines, Does.Contain("   " + workbook + "  last written 2026-09-28 14:02, 5 bytes"));
            Assert.That(lines, Does.Contain("   " + xml + "  last written 2026-09-28 14:03, 3 bytes"));
            Assert.That(lines, Does.Contain(
                "   " + pictures + "  a folder of pictures, last written 2026-09-28 14:04, its files not listed"));
            Assert.That(lines.Count, Is.EqualTo(3), "the page is not on the disk, so it is not named");
        }

        /// <summary>Exactly the names this run would have written, so an output switched off is not looked for.</summary>
        [Test]
        public void AReportThisRunWouldNotWriteIsNotNamed()
        {
            string workbook = Written(ReportPaths.Workbook(folder, "1B06K1"), "12345", new DateTime(2026, 9, 28, 14, 2, 0));
            Written(ReportPaths.Xml(folder, "1B06K1"), "abc", new DateTime(2026, 9, 28, 14, 3, 0));

            IList<string> lines = EarlierReports.Lines(Wanted(true, false, false, false), folder, "1B06K1");

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.StartWith("   " + workbook + "  last written"));
        }

        /// <summary>Another group's report in the same folder is never named, because only exact paths are read.</summary>
        [Test]
        public void AnotherGroupsReportIsNeverNamed()
        {
            Written(ReportPaths.Workbook(folder, "1B06K2"), "12345", new DateTime(2026, 9, 28, 14, 2, 0));

            Assert.That(EarlierReports.Lines(Wanted(true, true, true, true), folder, "1B06K1"), Is.Empty);
        }

        [Test]
        public void WithNoReportFolderNothingIsLookedFor()
        {
            Assert.That(EarlierReports.Lines(Wanted(true, true, true, true), null, "1B06K1"), Is.Empty);
        }

        /// <summary>Read and never touched: the same bytes and the same time after it as before it.</summary>
        [Test]
        public void TheFilesAreNeverTouched()
        {
            DateTime at = new DateTime(2026, 9, 28, 14, 2, 0);
            string workbook = Written(ReportPaths.Workbook(folder, "1B06K1"), "12345", at);

            EarlierReports.Lines(Wanted(true, true, true, true), folder, "1B06K1");

            Assert.That(File.ReadAllText(workbook), Is.EqualTo("12345"));
            Assert.That(File.GetLastWriteTime(workbook), Is.EqualTo(at));
        }
    }
}
