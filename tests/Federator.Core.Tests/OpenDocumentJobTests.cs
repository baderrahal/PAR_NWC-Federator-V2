using System.Collections.Generic;
using System.IO;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The second way to run: the file somebody already has open, with no scan behind it.
    ///
    /// The naming rule is the whole of what can be tested without Navisworks, and it is
    /// also the part that matters: the outputs of a document run this way must land on
    /// exactly the paths the scanned run uses, or the same building written both ways
    /// leaves two sets of files.
    /// </summary>
    [TestFixture]
    public class OpenDocumentJobTests
    {
        private const string Open =
            @"D:\Federations\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf";

        /// <summary>
        /// The same file on a path the running system can take apart. D:\ has no folder
        /// in it anywhere but Windows, and the folder rule is what these tests are about.
        /// </summary>
        private static readonly string OpenHere = Path.Combine(
            Path.GetTempPath(), "Federations", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf");

        [Test]
        public void TheNameIsReadOffTheOpenFile()
        {
            Assert.That(OpenDocumentJob.NameFrom(Open),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
        }

        [Test]
        public void TheNwdSitsBesideItWithTheExtensionSwapped()
        {
            Assert.That(OpenDocumentJob.NwdBeside(Open),
                Is.EqualTo(@"D:\Federations\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd"));
        }

        [Test]
        public void TheReportGoesInTheSameSubfolderTheScannedRunUses()
        {
            string folder = Path.GetDirectoryName(Open);

            Assert.That(
                OpenDocumentJob.ReportFolder(Open, string.Empty).Folder,
                Is.EqualTo(Path.Combine(folder, ReportPaths.DefaultSubfolder)),
                "the same building run either way has to write to one place");
        }

        /// <summary>
        /// B2. The window built Clash Reports beside the file and handed it to the engine
        /// as the NWF folder, and the engine built Clash Reports beside that again. The
        /// full path is asserted and the subfolder is counted, so a second copy anywhere
        /// in it fails here.
        /// </summary>
        [Test]
        public void TheReportFolderHoldsClashReportsOnceAndNotTwice()
        {
            string folder = Path.GetDirectoryName(OpenHere);
            string report = OpenDocumentJob.ReportFolder(OpenHere, string.Empty).Folder;

            Assert.That(report, Is.EqualTo(Path.Combine(folder, ReportPaths.DefaultSubfolder)));
            Assert.That(Count(report, ReportPaths.DefaultSubfolder), Is.EqualTo(1),
                "the subfolder appears once: " + report);
            Assert.That(report, Does.Not.Contain(
                ReportPaths.DefaultSubfolder + Path.DirectorySeparatorChar + ReportPaths.DefaultSubfolder));
        }

        /// <summary>
        /// The trap itself, pinned so it is not walked into again: hand the report folder
        /// back into the scanned run's rule as if it were the NWF folder and the subfolder
        /// doubles. That is what the old code did across two files.
        /// </summary>
        [Test]
        public void HandingTheReportFolderBackInAsTheNwfFolderDoublesIt()
        {
            string report = OpenDocumentJob.ReportFolder(OpenHere, string.Empty).Folder;
            string doubled = ReportPaths.Choose(string.Empty, report, null).Folder;

            Assert.That(Count(doubled, ReportPaths.DefaultSubfolder), Is.EqualTo(2));
            Assert.That(doubled, Is.Not.EqualTo(report));
        }

        [Test]
        public void APickedExcelFolderWinsForTheOpenFileToo()
        {
            // The scanned run honours the Excel folder on the Outputs step, so the open
            // file does the same, or the same building written both ways lands in two
            // places.
            string picked = Path.Combine(Path.GetTempPath(), "picked-reports");

            Assert.That(OpenDocumentJob.ReportFolder(OpenHere, picked).Folder, Is.EqualTo(picked));
        }

        [Test]
        public void TheScanFolderNeverRefusesAnOpenFile()
        {
            // An open file inside a folder that was scanned earlier in the same window is
            // still run beside itself. There was no scan behind this run, so the refusal
            // that guards a scan does not apply and nothing throws.
            string open = Path.Combine(Path.GetTempPath(), "NWC", "fed.nwf");
            string report = OpenDocumentJob.ReportFolder(open, string.Empty).Folder;

            Assert.That(report, Is.EqualTo(
                Path.Combine(Path.GetTempPath(), "NWC", ReportPaths.DefaultSubfolder)));
        }

        [Test]
        public void TheFolderOfTheOpenFileIsWhereTheLogCopyGoes()
        {
            Assert.That(OpenDocumentJob.FolderOf(OpenHere),
                Is.EqualTo(Path.GetDirectoryName(OpenHere)));
            Assert.That(OpenDocumentJob.FolderOf("model.nwf"), Is.Empty);
            Assert.That(OpenDocumentJob.FolderOf(string.Empty), Is.Empty);
            Assert.That(OpenDocumentJob.FolderOf(null), Is.Empty);
        }

        /// <summary>
        /// B11. The open file run ended with no block naming what it did, so the RESULT
        /// block after it counted a group nobody could see. These lines carry the same
        /// fields the scanned run's blocks carry, where they apply.
        /// </summary>
        [Test]
        public void TheSummaryNamesTheFileTheDecisionTheClashTheNwdAndTheReportFolder()
        {
            IList<string> lines = OpenDocumentJob.SummaryLines(
                OpenHere,
                Path.Combine(Path.GetTempPath(), "tests.xml"),
                OpenDocumentJob.NwdBeside(OpenHere),
                OpenDocumentJob.ReportFolder(OpenHere, string.Empty).Folder,
                "1830 created, 48 with clashes",
                GroupOutcome.Done,
                null);

            string all = string.Join("\n", new List<string>(lines).ToArray());

            Assert.That(all, Does.Contain(OpenHere));
            Assert.That(all, Does.Contain("opened file, no Decide"));
            Assert.That(all, Does.Contain("tests.xml"));
            Assert.That(all, Does.Contain("1830 created, 48 with clashes"));
            Assert.That(all, Does.Contain(OpenDocumentJob.NwdBeside(OpenHere)));
            Assert.That(all, Does.Contain(OpenDocumentJob.ReportFolder(OpenHere, string.Empty).Folder));
            Assert.That(all, Does.Contain("outcome       : DONE"));
        }

        [Test]
        public void TheFieldsAScannedRunHasAndAnOpenFileDoesNotSayNotApplicable()
        {
            IList<string> lines = OpenDocumentJob.SummaryLines(
                OpenHere, null, OpenDocumentJob.NwdBeside(OpenHere), null, null,
                GroupOutcome.Failed, "the NWD was requested and is not on disk");

            Assert.That(lines, Has.Some.StartsWith("source folder : not applicable"));
            Assert.That(lines, Has.Some.StartsWith("grouping      : not applicable"));
            Assert.That(lines, Has.Some.StartsWith("files ticked  : not applicable"));
            Assert.That(lines, Has.Some.StartsWith("clash file    : none picked"));
            Assert.That(lines, Has.Some.StartsWith("clash         : no clash step ran"));
            Assert.That(lines, Has.Some.Contains("FAILED, the NWD was requested and is not on disk"));

            foreach (string line in lines)
            {
                Assert.That(line.TrimEnd(), Does.Not.EndWith(":"),
                    "a field is never left blank: " + line);
            }
        }

        private static int Count(string text, string part)
        {
            int count = 0;
            int at = text.IndexOf(part, System.StringComparison.Ordinal);

            while (at >= 0)
            {
                count++;
                at = text.IndexOf(part, at + part.Length, System.StringComparison.Ordinal);
            }

            return count;
        }

        [Test]
        public void AnNwdOpenedDirectlyStillNamesItsOutputs()
        {
            string open = @"D:\Federations\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd";

            Assert.That(OpenDocumentJob.NameFrom(open),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
            Assert.That(OpenDocumentJob.CanRun(open), Is.True);
        }

        [Test]
        public void AnUnsavedDocumentIsRefusedAndSaysWhy()
        {
            Assert.That(OpenDocumentJob.CanRun(string.Empty), Is.False);
            Assert.That(OpenDocumentJob.CanRun(null), Is.False);

            string why = OpenDocumentJob.WhyNot(string.Empty);

            Assert.That(why, Does.Contain("has not been saved"));
            Assert.That(why, Does.Contain("Save it first"));
        }

        [Test]
        public void TheRefusalCarriesNoCodeIdentifier()
        {
            // A label may never carry a framework message or a parameter name. The rule
            // that put "Parameter name: nwfFolder" in front of a user applies here too.
            string why = OpenDocumentJob.WhyNot(null);

            Assert.That(why, Does.Not.Contain("Parameter"));
            Assert.That(why, Does.Not.Contain("null"));
            Assert.That(why, Does.Not.Contain("Exception"));
        }

        [Test]
        public void TheLineSaysWhatItWillDoAndWhereBeforeAnyonePressesIt()
        {
            string said = OpenDocumentJob.Describe(Open, string.Empty);

            Assert.That(said, Does.Contain("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
            Assert.That(said, Does.Contain(@"D:\Federations\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd"));
            Assert.That(said, Does.Contain(@"D:\Federations\Clash Reports"));
        }

        [Test]
        public void AnUnsavedDocumentDescribesTheRefusalRatherThanAPath()
        {
            string said = OpenDocumentJob.Describe(string.Empty, string.Empty);

            Assert.That(said, Is.EqualTo(OpenDocumentJob.WhyNot(string.Empty)));
        }

        [Test]
        public void ANameWithNoFolderBehindItGivesNoPathRatherThanAWrongOne()
        {
            // A relative name has no folder, and guessing one would put a client's report
            // in whatever directory the process happened to start in.
            Assert.That(OpenDocumentJob.NwdBeside("model.nwf"), Is.Empty);
            Assert.That(
                OpenDocumentJob.ReportFolder("model.nwf", string.Empty).Folder,
                Is.Empty);
        }

        [Test]
        public void TheOutputsMatchWhatTheScannedRunWouldWriteForTheSameName()
        {
            // The point of the whole path. A building federated by the scan and the same
            // building rerun off its open NWF must write the same three files.
            string folder = Path.GetDirectoryName(Open);
            string name = OpenDocumentJob.NameFrom(Open);

            Assert.That(OpenDocumentJob.NwdBeside(Open),
                Is.EqualTo(Path.Combine(folder, name + ".nwd")));
            Assert.That(
                OpenDocumentJob.ReportFolder(Open, string.Empty).Folder,
                Is.EqualTo(Path.Combine(folder, ReportPaths.DefaultSubfolder)));
        }
    }
}
