using System.IO;
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
            Assert.That(
                OpenDocumentJob.ReportFolderBeside(Open, ReportPaths.DefaultSubfolder),
                Is.EqualTo(@"D:\Federations\Clash Reports"),
                "the same building run either way has to write to one place");
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
            string said = OpenDocumentJob.Describe(Open, ReportPaths.DefaultSubfolder);

            Assert.That(said, Does.Contain("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
            Assert.That(said, Does.Contain(@"D:\Federations\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd"));
            Assert.That(said, Does.Contain(@"D:\Federations\Clash Reports"));
        }

        [Test]
        public void AnUnsavedDocumentDescribesTheRefusalRatherThanAPath()
        {
            string said = OpenDocumentJob.Describe(string.Empty, ReportPaths.DefaultSubfolder);

            Assert.That(said, Is.EqualTo(OpenDocumentJob.WhyNot(string.Empty)));
        }

        [Test]
        public void ANameWithNoFolderBehindItGivesNoPathRatherThanAWrongOne()
        {
            // A relative name has no folder, and guessing one would put a client's report
            // in whatever directory the process happened to start in.
            Assert.That(OpenDocumentJob.NwdBeside("model.nwf"), Is.Empty);
            Assert.That(
                OpenDocumentJob.ReportFolderBeside("model.nwf", ReportPaths.DefaultSubfolder),
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
                OpenDocumentJob.ReportFolderBeside(Open, ReportPaths.DefaultSubfolder),
                Is.EqualTo(Path.Combine(folder, ReportPaths.DefaultSubfolder)));
        }
    }
}
