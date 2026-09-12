using System;
using System.IO;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// A real run rebuilt all 22 groups, logging "no NWF at" for every one, at paths where
    /// an earlier run had written NWFs. The check itself is File.Exists on the computed
    /// output path, so what has to hold is that the path looked at and the path written to
    /// are the same string. These build a real file on disk and look for it the way the
    /// engine does.
    /// </summary>
    [TestFixture]
    public class OutputPathsTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorOutputPaths");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private const string OutputName = "1104-PAR-1C07BC-ZZZ-BM-MOD-000001";

        [Test]
        public void TheNwfPathIsTheNameWithAnNwfExtension()
        {
            string folder = TestPaths.At("out", "nwf");

            Assert.That(OutputPaths.Nwf(folder, OutputName),
                Is.EqualTo(Path.Combine(folder, OutputName + ".nwf")));
        }

        [Test]
        public void TheNwdPathIsTheNameWithAnNwdExtension()
        {
            string folder = TestPaths.At("out", "nwd");

            Assert.That(OutputPaths.Nwd(folder, OutputName),
                Is.EqualTo(Path.Combine(folder, OutputName + ".nwd")));
        }

        // This is the one that matters. An NWF written at the computed path on one run has
        // to be found at the computed path on the next.
        [Test]
        public void AnNwfWrittenAtTheComputedPathIsFoundAtTheComputedPath()
        {
            string written = OutputPaths.Nwf(folder, OutputName);
            File.WriteAllText(written, "pretend NWF");

            string lookedFor = OutputPaths.Nwf(folder, OutputName);

            Assert.That(lookedFor, Is.EqualTo(written), "the two paths drifted apart");
            Assert.That(File.Exists(lookedFor), Is.True,
                "an NWF at the computed output path was not detected");
        }

        [Test]
        public void AnEmptyFolderReportsNoNwf()
        {
            Assert.That(File.Exists(OutputPaths.Nwf(folder, OutputName)), Is.False);
        }

        // The two outputs must never collide, whatever the name.
        [Test]
        public void TheNwfAndTheNwdNeverLandOnTheSamePath()
        {
            Assert.That(OutputPaths.Nwf(folder, OutputName),
                Is.Not.EqualTo(OutputPaths.Nwd(folder, OutputName)));
        }

        [Test]
        public void ANwdAtThePathIsNotMistakenForAnNwf()
        {
            File.WriteAllText(OutputPaths.Nwd(folder, OutputName), "pretend NWD");

            Assert.That(File.Exists(OutputPaths.Nwf(folder, OutputName)), Is.False,
                "an NWD was mistaken for an NWF");
        }

        [Test]
        public void ADifferentOutputNameLooksAtADifferentPath()
        {
            File.WriteAllText(OutputPaths.Nwf(folder, OutputName), "pretend NWF");

            // An output name that changed between runs, for example because the naming
            // rule changed, leaves the old NWF stranded under its old name.
            Assert.That(File.Exists(OutputPaths.Nwf(folder, "1104-PAR-1C07K1-ZZZ-BM-MOD-000001")),
                Is.False);
        }

        [Test]
        public void AFolderWithATrailingSlashStillGivesTheSamePath()
        {
            string withSlash = OutputPaths.Nwf(folder + Path.DirectorySeparatorChar, OutputName);
            string without = OutputPaths.Nwf(folder, OutputName);

            File.WriteAllText(without, "pretend NWF");

            Assert.That(File.Exists(withSlash), Is.True,
                "a trailing slash on the folder changed where the NWF is looked for");
        }

        [Test]
        public void ANameWithSpacesAndAmpersandsStillRoundTrips()
        {
            string odd = "1104-PAR-1C07BC-ZZZ-BM-MOD-000001 & copy";
            string written = OutputPaths.Nwf(folder, odd);
            File.WriteAllText(written, "pretend NWF");

            Assert.That(File.Exists(OutputPaths.Nwf(folder, odd)), Is.True);
        }

        [Test]
        public void AMissingNameOrExtensionIsRefusedRatherThanBuildingAJunkPath()
        {
            Assert.Throws<ArgumentNullException>(delegate { OutputPaths.Nwf(null, OutputName); });
            Assert.Throws<ArgumentException>(delegate { OutputPaths.Nwf(folder, null); });
            Assert.Throws<ArgumentException>(delegate { OutputPaths.Nwf(folder, string.Empty); });
            Assert.Throws<ArgumentException>(delegate { OutputPaths.For(folder, OutputName, null); });
        }
    }
}
