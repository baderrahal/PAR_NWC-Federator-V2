using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Where the logs and the reports go, F158. One place builds the path, so the path written to
    /// and the path looked for cannot drift, the same way OutputPaths works for the NWF and NWD.
    /// The NWF and the NWD folders hold the NWF and the NWD and nothing else. The logs and the
    /// reports go in one folder named Log beside them.
    /// </summary>
    [TestFixture]
    public class ReportPathsTests
    {
        private const string OutputName = "1104-PAR-1C07BC-ZZZ-BM-MOD-000001";

        // The layout of the proving folder, NM fed, one folder for each kind of file with a
        // folder for each group under it.
        private static readonly string Area = TestPaths.At("NM fed");
        private static readonly string Source = Path.Combine(Area, "NWC", "C04");
        private static readonly string Nwf = Path.Combine(Area, "NWF", "C04");
        private static readonly string Nwd = Path.Combine(Area, "NWD", "C04");
        private static readonly string Out = TestPaths.At("out");
        private static readonly string OutReports = TestPaths.At("out", "Log");

        // ---------- the Log folder ----------

        [Test]
        public void TheFolderIsCalledLog()
        {
            Assert.That(ReportPaths.DefaultSubfolder, Is.EqualTo("Log"));
            Assert.That(ReportPaths.Subfolder, Is.EqualTo("Log"));
        }

        [Test]
        public void TheLogFolderSitsInTheGroupAreaBesideTheNwfAndNwdFolders()
        {
            string log = ReportPaths.LogFolder(Nwf, Nwd, Source);

            Assert.That(log, Is.EqualTo(Path.Combine(Area, "Log")));
            Assert.That(ReportPaths.IsInside(log, Nwf), Is.False, "the Log folder is inside the NWF folder");
            Assert.That(ReportPaths.IsInside(log, Nwd), Is.False, "the Log folder is inside the NWD folder");
            Assert.That(ReportPaths.IsInside(log, Source), Is.False, "the Log folder is inside the folder being scanned");
        }

        [Test]
        public void AnNwfAndNwdFolderThatAreOneFolderGetTheLogFolderBesideIt()
        {
            string both = Path.Combine(Out, "Federations");

            Assert.That(ReportPaths.LogFolder(both, both, null), Is.EqualTo(Path.Combine(Out, "Log")));
            Assert.That(ReportPaths.LogFolder(both, string.Empty, null), Is.EqualTo(Path.Combine(Out, "Log")),
                "an empty NWD folder reads as the NWF folder");
            Assert.That(ReportPaths.LogFolder(both, null, null), Is.EqualTo(Path.Combine(Out, "Log")));
        }

        [Test]
        public void TwoSiblingFoldersGetTheLogFolderBesideBoth()
        {
            string nwf = Path.Combine(Out, "NWF");
            string nwd = Path.Combine(Out, "NWD");

            Assert.That(ReportPaths.LogFolder(nwf, nwd, null), Is.EqualTo(Path.Combine(Out, "Log")));
        }

        [Test]
        public void ANwdFolderInsideTheNwfFolderStillLeavesTheNwfFolderHoldingOnlyItsFiles()
        {
            string nwf = Path.Combine(Out, "NWF");
            string nwd = Path.Combine(nwf, "NWD");
            string log = ReportPaths.LogFolder(nwf, nwd, null);

            Assert.That(log, Is.EqualTo(Path.Combine(Out, "Log")));
            Assert.That(ReportPaths.IsInside(log, nwf), Is.False);
        }

        [Test]
        public void AFolderIsNeverTheLogFolderOfItself()
        {
            Assert.That(ReportPaths.LogFolder(Nwf, Nwd, Source), Is.Not.EqualTo(Nwf));
            Assert.That(ReportPaths.LogFolder(Nwf, Nwd, Source), Is.Not.EqualTo(Nwd));
        }

        [Test]
        public void AnNwfFolderWithNoFolderPickedIsRefusedRatherThanWritingSomewhereOdd()
        {
            Assert.Throws<ArgumentException>(delegate { ReportPaths.LogFolder(string.Empty, Nwd, null); });
            Assert.Throws<ArgumentException>(delegate { ReportPaths.LogFolder(null, null, null); });
            Assert.Throws<ArgumentException>(delegate { ReportPaths.LogFolder("   ", Nwd, null); });
        }

        // The Clash step picks an XML at run time, so a report written in the folder being scanned
        // is a file a later run can be handed as its own input.
        [Test]
        public void ALogFolderInsideTheSourceFolderIsRefusedRatherThanUsed()
        {
            string nwf = Path.Combine(Source, "nwf");

            ArgumentException thrown = Assert.Throws<ArgumentException>(
                delegate { ReportPaths.LogFolder(nwf, nwf, Source); });

            Assert.That(thrown.Message, Does.Contain("inside the folder being scanned"));
            Assert.That(thrown.Message, Does.Contain("Pick an NWF folder outside it"));
        }

        [Test]
        public void WithNoSourceFolderNothingIsRefused()
        {
            string nwf = Path.Combine(Source, "nwf");

            Assert.That(ReportPaths.LogFolder(nwf, nwf, null), Is.EqualTo(Path.Combine(Source, "Log")));
            Assert.That(ReportPaths.IsInside(TestPaths.At("anything"), null), Is.False);
            Assert.That(ReportPaths.IsInside(TestPaths.At("anything"), "   "), Is.False);
        }

        [Test]
        public void TheWordsForTheOutputsStepNameTheLogFolderAndNeverAnException()
        {
            Assert.That(ReportPaths.WhereTheyGo(Nwf, Nwd, Source), Is.EqualTo(Path.Combine(Area, "Log")));
            Assert.That(ReportPaths.WhereTheyGo(string.Empty, string.Empty, string.Empty),
                Does.Contain("once the NWF folder is picked"));
            Assert.That(ReportPaths.WhereTheyGo(Path.Combine(Source, "nwf"), null, Source),
                Does.Contain("inside the folder being scanned"));
        }

        [Test]
        public void TheFolderNameIsASettingAndRefusesAPathOrABlank()
        {
            string before = ReportPaths.Subfolder;

            try
            {
                Assert.Throws<ArgumentException>(delegate { ReportPaths.Subfolder = "  "; });
                Assert.Throws<ArgumentException>(delegate { ReportPaths.Subfolder = "a" + Path.DirectorySeparatorChar + "b"; });

                ReportPaths.Subfolder = "Logs";
                Assert.That(ReportPaths.LogFolder(Nwf, Nwd, Source), Is.EqualTo(Path.Combine(Area, "Logs")));
            }
            finally
            {
                ReportPaths.Subfolder = before;
            }
        }

        [Test]
        public void TheOptionsCarryTheSourceFolderThroughToTheChoice()
        {
            ReportOptions options = new ReportOptions();
            options.SourceFolder = Source;

            Assert.That(options.LogFolderFor(Nwf, Nwd), Is.EqualTo(Path.Combine(Area, "Log")));
            Assert.Throws<ArgumentException>(delegate { options.LogFolderFor(Path.Combine(Source, "x"), null); });
        }

        [Test]
        public void TheSourceFolderStartsEmptySoNothingIsRefusedByDefault()
        {
            Assert.That(new ReportOptions().SourceFolder, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TheXmlIsOffByDefaultAndTheWorkbookIsOn()
        {
            ReportOptions options = new ReportOptions();

            Assert.That(options.WriteXml, Is.False, "the XML must be off by default");
            Assert.That(options.WriteWorkbook, Is.True);
        }

        // ---------- the names ----------

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

        // ---------- nothing is overwritten ----------

        [Test]
        public void AFreeNameIsKeptAsItIs()
        {
            string name = ReportPaths.NameThatOverwritesNothing(
                OutReports, OutputName, "Internal", new DateTime(2026, 10, 14, 15, 30, 12), p => false);

            Assert.That(name, Is.EqualTo(OutputName));
        }

        [Test]
        public void ANameWhoseWorkbookIsAlreadyThereTakesTheMomentOfTheRun()
        {
            HashSet<string> there = new HashSet<string> { ReportPaths.Workbook(OutReports, OutputName) };

            string name = ReportPaths.NameThatOverwritesNothing(
                OutReports, OutputName, "Internal", new DateTime(2026, 10, 14, 15, 30, 12), there.Contains);

            Assert.That(name, Is.EqualTo(OutputName + " 2026-10-14 153012"));
        }

        [Test]
        public void EachOfTheGroupsFilesAloneForcesTheDatedName()
        {
            string workbook = ReportPaths.Workbook(OutReports, OutputName);
            string[] one =
            {
                ReportPaths.Xml(OutReports, OutputName),
                HtmlTabularWriter.PathFor(workbook),
                ImageNaming.FolderFor(workbook),
                Path.Combine(OutReports, OutputName + " Internal.xlsx")
            };

            foreach (string path in one)
            {
                HashSet<string> there = new HashSet<string> { path };
                string name = ReportPaths.NameThatOverwritesNothing(
                    OutReports, OutputName, "Internal", new DateTime(2026, 10, 14, 15, 30, 12), there.Contains);

                Assert.That(name, Is.EqualTo(OutputName + " 2026-10-14 153012"), path + " did not force a dated name");
            }
        }

        [Test]
        public void ADatedNameAlreadyThereGetsANumberAfterItNeverTheSameName()
        {
            string dated = OutputName + " 2026-10-14 153012";
            HashSet<string> there = new HashSet<string>
            {
                ReportPaths.Workbook(OutReports, OutputName),
                ReportPaths.Workbook(OutReports, dated)
            };

            string name = ReportPaths.NameThatOverwritesNothing(
                OutReports, OutputName, "Internal", new DateTime(2026, 10, 14, 15, 30, 12), there.Contains);

            Assert.That(name, Is.EqualTo(dated + " 2"));
        }

        [Test]
        public void AFileNameTakenInTheLogFolderIsNotReplacedTheMomentIsAddedAndThenANumber()
        {
            string plain = Path.Combine(OutReports, "run-1.log");
            string dated = Path.Combine(OutReports, "run-1 2026-10-14 153012.log");
            DateTime now = new DateTime(2026, 10, 14, 15, 30, 12);

            Assert.That(ReportPaths.FreeFilePath(OutReports, "run-1.log", now, p => false), Is.EqualTo(plain));
            Assert.That(ReportPaths.FreeFilePath(OutReports, "run-1.log", now, p => p == plain), Is.EqualTo(dated));
            Assert.That(
                ReportPaths.FreeFilePath(OutReports, "run-1.log", now, p => p == plain || p == dated),
                Is.EqualTo(Path.Combine(OutReports, "run-1 2026-10-14 153012 2.log")));
        }

        // A real folder, written to and found again, which is the thing that has to hold.
        [Test]
        public void AWorkbookWrittenAtTheComputedPathIsFoundAtTheComputedPathAndTheEarlierOneIsLeftAlone()
        {
            string root = TempFolder.Make("FederatorReportPaths");

            try
            {
                string nwf = Path.Combine(root, "NWF", "C04");
                string nwd = Path.Combine(root, "NWD", "C04");
                string folder = ReportPaths.LogFolder(nwf, nwd, null);
                Directory.CreateDirectory(folder);

                Assert.That(folder, Is.EqualTo(Path.Combine(root, "Log")));

                string first = ReportPaths.Workbook(folder, OutputName);
                File.WriteAllText(first, "the earlier run's workbook");

                string name = ReportPaths.NameThatOverwritesNothing(
                    folder, OutputName, "Internal", new DateTime(2026, 10, 14, 15, 30, 12), null);
                string second = ReportPaths.Workbook(folder, name);
                File.WriteAllText(second, "this run's workbook");

                Assert.That(second, Is.Not.EqualTo(first));
                Assert.That(File.ReadAllText(first), Is.EqualTo("the earlier run's workbook"),
                    "an earlier workbook was overwritten");
                Assert.That(File.ReadAllText(second), Is.EqualTo("this run's workbook"));
            }
            finally
            {
                TempFolder.Remove(root);
            }
        }
    }
}
