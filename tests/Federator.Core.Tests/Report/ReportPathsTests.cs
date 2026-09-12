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
            Assert.That(ReportPaths.Folder(Elsewhere, OutNwf), Is.EqualTo(Elsewhere));
        }

        // The one the brief asks for by name.
        [Test]
        public void AnEmptyPickGoesBesideTheNwfFolderInClashReports()
        {
            string beside = Path.Combine(OutNwf, "Clash Reports");

            Assert.That(ReportPaths.Folder(string.Empty, OutNwf), Is.EqualTo(beside));
            Assert.That(ReportPaths.Folder(null, OutNwf), Is.EqualTo(beside));
            Assert.That(ReportPaths.Folder("   ", OutNwf), Is.EqualTo(beside));
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
            Assert.That(ReportPaths.Workbook(OutReports, OutputName),
                Is.EqualTo(Path.Combine(OutReports, OutputName + ".xlsx")));
        }

        [Test]
        public void TheXmlSitsBesideItUnderTheSameName()
        {
            Assert.That(ReportPaths.Xml(OutReports, OutputName),
                Is.EqualTo(Path.Combine(OutReports, OutputName + ".xml")));
        }

        [Test]
        public void TheWorkbookAndTheXmlNeverLandOnTheSamePath()
        {
            Assert.That(ReportPaths.Workbook(Out, OutputName),
                Is.Not.EqualTo(ReportPaths.Xml(Out, OutputName)));
        }

        // Written twice, the same path both times, which is what overwriting means.
        [Test]
        public void TheSameGroupAlwaysGivesTheSamePath()
        {
            Assert.That(ReportPaths.Workbook(Out, OutputName),
                Is.EqualTo(ReportPaths.Workbook(Out, OutputName)));
        }

        [Test]
        public void AMissingNameOrExtensionIsRefusedRatherThanBuildingAJunkPath()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { ReportPaths.Workbook(null, OutputName); });
            Assert.Throws<ArgumentException>(delegate { ReportPaths.Workbook(Out, null); });
            Assert.Throws<ArgumentException>(
                delegate { ReportPaths.Workbook(Out, string.Empty); });
            Assert.Throws<ArgumentException>(
                delegate { ReportPaths.For(Out, OutputName, null); });
        }


        // ---------- the reports never go inside the folder being scanned ----------

        // A real run put the workbooks in C:\00_NM\NWC Fed\NWC\test001, which is the
        // folder it was reading its NWC files out of. The Clash step picks an XML at run
        // time, so a report written there is a file a later run can be handed as its own
        // input.
        private static readonly string Source = TestPaths.At("00_NM", "NWC Fed", "NWC", "test001");
        private static readonly string Nwf = TestPaths.At("00_NM", "NWC Fed", "NWF", "test001");
        private static readonly string Elsewhere = TestPaths.At("reports");
        private static readonly string Out = TestPaths.At("out");
        private static readonly string OutNwf = TestPaths.At("out", "nwf");
        private static readonly string OutReports = TestPaths.At("out", "reports");

        // The one the brief asks for by name.
        [Test]
        public void TheDefaultLandsBesideTheNwfFolderAndNotInTheSourceFolder()
        {
            ReportFolderChoice where = ReportPaths.Choose(string.Empty, Nwf, Source);

            Assert.That(where.Folder, Is.EqualTo(Path.Combine(Nwf, "Clash Reports")));
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
            Assert.That(where.Folder, Is.EqualTo(Path.Combine(Nwf, "Clash Reports")),
                "it should fall back to beside the NWF folder");
            Assert.That(where.RefusedReason, Does.Contain("inside the folder being scanned"));
            Assert.That(where.RefusedReason, Does.Contain(Source));
        }

        [Test]
        public void AFolderUnderTheSourceFolderIsRefusedToo()
        {
            ReportFolderChoice where = ReportPaths.Choose(
                Path.Combine(Source, "reports"), Nwf, Source);

            Assert.That(where.WasRefused, Is.True);
            Assert.That(ReportPaths.IsInside(where.Folder, Source), Is.False);
        }

        [Test]
        public void AFolderOutsideTheSourceFolderIsUsedExactlyAsPicked()
        {
            ReportFolderChoice where = ReportPaths.Choose(Elsewhere, Nwf, Source);

            Assert.That(where.WasRefused, Is.False);
            Assert.That(where.Folder, Is.EqualTo(Elsewhere));
        }

        // A folder whose name merely starts with the source folder's name is a different
        // folder, not one inside it.
        [Test]
        public void AFolderThatOnlySharesAPrefixIsNotInsideIt()
        {
            Assert.That(
                ReportPaths.IsInside(TestPaths.At("out", "NWCFed"), TestPaths.At("out", "NWC")),
                Is.False);
            Assert.That(
                ReportPaths.IsInside(TestPaths.At("out", "NWC", "sub"), TestPaths.At("out", "NWC")),
                Is.True);
            Assert.That(
                ReportPaths.IsInside(TestPaths.At("out", "NWC"), TestPaths.At("out", "NWC")),
                Is.True,
                "the folder itself counts as inside itself");
        }

        [Test]
        public void ATrailingSeparatorDoesNotChangeTheAnswer()
        {
            Assert.That(
                ReportPaths.IsInside(
                    TestPaths.At("out", "nwc", "sub"),
                    TestPaths.At("out", "nwc") + Path.DirectorySeparatorChar),
                Is.True);
        }

        [Test]
        public void CaseDoesNotChangeTheAnswerEither()
        {
            TestPaths.OnWindowsOnly("comparing two paths without case");

            Assert.That(
                ReportPaths.IsInside(
                    TestPaths.At("OUT", "nwc", "sub"),
                    TestPaths.At("out", "NWC") + Path.DirectorySeparatorChar),
                Is.True);
        }

        [Test]
        public void WithNoSourceFolderNothingIsRefused()
        {
            Assert.That(ReportPaths.Choose(Source, Nwf, null).WasRefused, Is.False,
                "with nothing being scanned there is nothing to keep out of");
            Assert.That(ReportPaths.IsInside(TestPaths.At("anything"), null), Is.False);
            Assert.That(ReportPaths.IsInside(TestPaths.At("anything"), "   "), Is.False);
        }

        // If the NWF folder is itself inside the source folder there is nowhere safe, and
        // that is said rather than written somewhere surprising.
        [Test]
        public void AnNwfFolderInsideTheSourceFolderIsRefusedRatherThanUsed()
        {
            ArgumentException thrown = Assert.Throws<ArgumentException>(
                delegate { ReportPaths.Choose(string.Empty, Path.Combine(Source, "nwf"), Source); });

            Assert.That(thrown.Message, Does.Contain("nowhere to put the reports"));
            Assert.That(thrown.Message, Does.Contain("Pick an Excel folder outside it"));
        }

        [Test]
        public void TheOptionsCarryTheSourceFolderThroughToTheChoice()
        {
            ReportOptions options = new ReportOptions();
            options.SourceFolder = Source;

            Assert.That(
                options.ChooseFor(Nwf).Folder, Is.EqualTo(Path.Combine(Nwf, "Clash Reports")));

            options.ExcelFolder = Source;
            Assert.That(options.ChooseFor(Nwf).WasRefused, Is.True);
            Assert.That(options.FolderFor(Nwf), Is.EqualTo(Path.Combine(Nwf, "Clash Reports")));
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

            Assert.That(
                options.FolderFor(OutNwf), Is.EqualTo(Path.Combine(OutNwf, "Clash Reports")));

            options.ExcelFolder = Elsewhere;
            Assert.That(options.FolderFor(OutNwf), Is.EqualTo(Elsewhere));
        }

        // A real folder, written to and found again, which is the thing that has to hold.
        [Test]
        public void AWorkbookWrittenAtTheComputedPathIsFoundAtTheComputedPath()
        {
            string root = TempFolder.Make("FederatorReportPaths");

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
                TempFolder.Remove(root);
            }
        }
    }
}
