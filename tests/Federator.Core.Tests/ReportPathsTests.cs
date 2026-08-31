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


        // ---------- the reports never go inside the folder being scanned ----------

        // A real run put the workbooks in C:\00_NM\NWC Fed\NWC\test001, which is the
        // folder it was reading its NWC files out of. The Clash step picks an XML at run
        // time, so a report written there is a file a later run can be handed as its own
        // input.
        private const string Source = @"C:\\00_NM\\NWC Fed\\NWC\\test001";
        private const string Nwf = @"C:\\00_NM\\NWC Fed\\NWF\\test001";

        // The one the brief asks for by name.
        [Test]
        public void TheDefaultLandsBesideTheNwfFolderAndNotInTheSourceFolder()
        {
            ReportFolderChoice where = ReportPaths.Choose(string.Empty, Nwf, Source);

            Assert.That(where.Folder, Is.EqualTo(Nwf + @"\Clash Reports"));
            Assert.That(where.WasRefused, Is.False);
            Assert.That(ReportPaths.IsInside(where.Folder, Source), Is.False,
                "the default landed inside the folder being scanned");
        }

        // The other half. A folder picked by hand that is inside the source is refused.
        [Test]
        public void APickedFolderInsideTheSourceFolderIsRefusedAndSaysWhy()
        {
            ReportFolderChoice where = ReportPaths.Choose(Source, Nwf, Source);

            Assert.That(where.WasRefused, Is.True);
            Assert.That(where.Folder, Is.EqualTo(Nwf + @"\Clash Reports"),
                "it should fall back to beside the NWF folder");
            Assert.That(where.RefusedReason, Does.Contain("inside the folder being scanned"));
            Assert.That(where.RefusedReason, Does.Contain(Source));
        }

        [Test]
        public void AFolderUnderTheSourceFolderIsRefusedToo()
        {
            ReportFolderChoice where = ReportPaths.Choose(
                Source + @"\reports", Nwf, Source);

            Assert.That(where.WasRefused, Is.True);
            Assert.That(ReportPaths.IsInside(where.Folder, Source), Is.False);
        }

        [Test]
        public void AFolderOutsideTheSourceFolderIsUsedExactlyAsPicked()
        {
            ReportFolderChoice where = ReportPaths.Choose(@"D:\reports", Nwf, Source);

            Assert.That(where.WasRefused, Is.False);
            Assert.That(where.Folder, Is.EqualTo(@"D:\reports"));
        }

        // A folder whose name merely starts with the source folder's name is a different
        // folder, not one inside it.
        [Test]
        public void AFolderThatOnlySharesAPrefixIsNotInsideIt()
        {
            Assert.That(ReportPaths.IsInside(@"C:\out\NWCFed", @"C:\out\NWC"), Is.False);
            Assert.That(ReportPaths.IsInside(@"C:\out\NWC\sub", @"C:\out\NWC"), Is.True);
            Assert.That(ReportPaths.IsInside(@"C:\out\NWC", @"C:\out\NWC"), Is.True,
                "the folder itself counts as inside itself");
        }

        [Test]
        public void CaseAndTrailingSlashesDoNotChangeTheAnswer()
        {
            Assert.That(ReportPaths.IsInside(@"c:\OUT\nwc\sub", @"C:\out\NWC\"), Is.True);
        }

        [Test]
        public void WithNoSourceFolderNothingIsRefused()
        {
            Assert.That(ReportPaths.Choose(Source, Nwf, null).WasRefused, Is.False,
                "with nothing being scanned there is nothing to keep out of");
            Assert.That(ReportPaths.IsInside(@"C:\anything", null), Is.False);
            Assert.That(ReportPaths.IsInside(@"C:\anything", "   "), Is.False);
        }

        // If the NWF folder is itself inside the source folder there is nowhere safe, and
        // that is said rather than written somewhere surprising.
        [Test]
        public void AnNwfFolderInsideTheSourceFolderIsRefusedRatherThanUsed()
        {
            ArgumentException thrown = Assert.Throws<ArgumentException>(
                delegate { ReportPaths.Choose(string.Empty, Source + @"\nwf", Source); });

            Assert.That(thrown.Message, Does.Contain("nowhere to put the reports"));
            Assert.That(thrown.Message, Does.Contain("Pick an Excel folder outside it"));
        }

        [Test]
        public void TheOptionsCarryTheSourceFolderThroughToTheChoice()
        {
            ReportOptions options = new ReportOptions();
            options.SourceFolder = Source;

            Assert.That(options.ChooseFor(Nwf).Folder, Is.EqualTo(Nwf + @"\Clash Reports"));

            options.ExcelFolder = Source;
            Assert.That(options.ChooseFor(Nwf).WasRefused, Is.True);
            Assert.That(options.FolderFor(Nwf), Is.EqualTo(Nwf + @"\Clash Reports"));
        }

        [Test]
        public void TheSourceFolderStartsEmptySoNothingIsRefusedByDefault()
        {
            Assert.That(new ReportOptions().SourceFolder, Is.EqualTo(string.Empty));
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
