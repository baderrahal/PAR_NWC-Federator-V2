using System;
using System.IO;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Where a workbook goes. One place builds the path, so the path written to and the
    /// path looked for cannot drift, the same way OutputPaths works for the NWF and NWD.
    /// </summary>
    [TestFixture]
    public class ReportPathsTests
    {
        private const string OutputName = "1104-PAR-1C07BC-ZZZ-BM-MOD-000001";

        [Test]
        public void APickedFolderIsUsedExactlyAsPicked()
        {
            Assert.That(ReportPaths.Folder(@"D:\reports", @"C:\out\nwf"), Is.EqualTo(@"D:\reports"));
        }

        // The one the brief asks for by name.
        [Test]
        public void AnEmptyPickGoesBesideTheNwfFolderInClashReports()
        {
            Assert.That(ReportPaths.Folder(string.Empty, @"C:\out\nwf"),
                Is.EqualTo(@"C:\out\nwf\Clash Reports"));
            Assert.That(ReportPaths.Folder(null, @"C:\out\nwf"),
                Is.EqualTo(@"C:\out\nwf\Clash Reports"));
            Assert.That(ReportPaths.Folder("   ", @"C:\out\nwf"),
                Is.EqualTo(@"C:\out\nwf\Clash Reports"));
        }

        [Test]
        public void TheSubfolderNameIsASettingRatherThanBuriedInTheCode()
        {
            Assert.That(ReportPaths.DefaultSubfolder, Is.EqualTo("Clash Reports"));
        }

        [Test]
        public void WithNeitherFolderItRefusesRatherThanWritingSomewhereOdd()
        {
            Assert.Throws<ArgumentException>(
                delegate { ReportPaths.Folder(string.Empty, string.Empty); });
            Assert.Throws<ArgumentException>(delegate { ReportPaths.Folder(null, null); });
        }

        // Named like the group's other outputs, so all three sit together.
        [Test]
        public void TheWorkbookIsNamedLikeTheNwfAndTheNwd()
        {
            Assert.That(ReportPaths.Workbook(@"C:\out\reports", OutputName),
                Is.EqualTo(@"C:\out\reports\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.xlsx"));
        }

        [Test]
        public void TheXmlSitsBesideItUnderTheSameName()
        {
            Assert.That(ReportPaths.Xml(@"C:\out\reports", OutputName),
                Is.EqualTo(@"C:\out\reports\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.xml"));
        }

        [Test]
        public void TheWorkbookAndTheXmlNeverLandOnTheSamePath()
        {
            Assert.That(ReportPaths.Workbook(@"C:\out", OutputName),
                Is.Not.EqualTo(ReportPaths.Xml(@"C:\out", OutputName)));
        }

        // Written twice, the same path both times, which is what overwriting means.
        [Test]
        public void TheSameGroupAlwaysGivesTheSamePath()
        {
            Assert.That(ReportPaths.Workbook(@"C:\out", OutputName),
                Is.EqualTo(ReportPaths.Workbook(@"C:\out", OutputName)));
        }

        [Test]
        public void AMissingNameOrExtensionIsRefusedRatherThanBuildingAJunkPath()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { ReportPaths.Workbook(null, OutputName); });
            Assert.Throws<ArgumentException>(delegate { ReportPaths.Workbook(@"C:\out", null); });
            Assert.Throws<ArgumentException>(
                delegate { ReportPaths.Workbook(@"C:\out", string.Empty); });
            Assert.Throws<ArgumentException>(
                delegate { ReportPaths.For(@"C:\out", OutputName, null); });
        }

        // ---------- the options object ----------

        [Test]
        public void TheXmlIsOffByDefaultAndTheWorkbookIsOn()
        {
            ReportOptions options = new ReportOptions();

            Assert.That(options.WriteXml, Is.False, "the XML must be off by default");
            Assert.That(options.WriteWorkbook, Is.True);
            Assert.That(options.ExcelFolder, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TheOptionsWorkOutTheSameFolderThePathsDo()
        {
            ReportOptions options = new ReportOptions();

            Assert.That(options.FolderFor(@"C:\out\nwf"), Is.EqualTo(@"C:\out\nwf\Clash Reports"));

            options.ExcelFolder = @"D:\reports";
            Assert.That(options.FolderFor(@"C:\out\nwf"), Is.EqualTo(@"D:\reports"));
        }

        // A real folder, written to and found again, which is the thing that has to hold.
        [Test]
        public void AWorkbookWrittenAtTheComputedPathIsFoundAtTheComputedPath()
        {
            string root = Path.Combine(
                Path.GetTempPath(), "FederatorReportPaths", Guid.NewGuid().ToString("N"));

            try
            {
                string folder = ReportPaths.Folder(string.Empty, root);
                Directory.CreateDirectory(folder);

                string written = ReportPaths.Workbook(folder, OutputName);
                File.WriteAllText(written, "pretend workbook");

                Assert.That(File.Exists(ReportPaths.Workbook(folder, OutputName)), Is.True,
                    "the two paths drifted apart");
                Assert.That(folder, Does.EndWith("Clash Reports"));
            }
            finally
            {
                try
                {
                    if (Directory.Exists(root))
                    {
                        Directory.Delete(root, true);
                    }
                }
                catch (IOException)
                {
                    // A leftover temp folder is not worth failing a test over.
                }
            }
        }
    }
}
