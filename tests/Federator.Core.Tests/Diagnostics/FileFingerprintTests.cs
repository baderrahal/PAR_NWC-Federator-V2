using System;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The fingerprint of the picked XML, F127, turn5\f127-design.md section 1.7. Set 03's
    /// C06 run carries no hash of the XML it picked, so which bytes it read had to be
    /// worked out from its SET lines, which ask ME-PIPING where the exchange file asks
    /// ME-Piping. A SHA-256 named in the block and on the sheet says it outright.
    /// </summary>
    [TestFixture]
    public class FileFingerprintTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorFingerprint");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private string Written(string name, byte[] bytes)
        {
            string path = Path.Combine(folder, name);
            File.WriteAllBytes(path, bytes);
            return path;
        }

        // The standard's own example, FIPS 180-2 appendix B.1.
        [Test]
        public void TheBytesAbcGiveTheStandardsOwnFingerprint()
        {
            Assert.That(FileFingerprint.Sha256(Written("abc.xml", Encoding.ASCII.GetBytes("abc"))),
                Is.EqualTo("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"));
        }

        [Test]
        public void AnEmptyFileHasAFingerprintToo()
        {
            Assert.That(FileFingerprint.Sha256(Written("empty.xml", new byte[0])),
                Is.EqualTo("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"));
        }

        // The break: one letter's case is the whole difference between ME-PIPING and ME-Piping.
        [Test]
        public void OneByteChangedChangesTheFingerprint()
        {
            string upper = FileFingerprint.Sha256(Written("a.xml", Encoding.UTF8.GetBytes("ME-PIPING")));
            string lower = FileFingerprint.Sha256(Written("b.xml", Encoding.UTF8.GetBytes("ME-Piping")));

            Assert.That(upper, Is.Not.EqualTo(lower));
        }

        [Test]
        public void TheSameFileGivesTheSameFingerprintTwice()
        {
            string path = Samples.CorrectedMatrix();

            Assert.That(FileFingerprint.Sha256(path), Is.EqualTo(FileFingerprint.Sha256(path)));
            Assert.That(FileFingerprint.Sha256(path), Does.Match("^[0-9a-f]{64}$"));
        }

        [Test]
        public void AFileThatIsNotThereThrowsNamingItsPath()
        {
            string path = Path.Combine(folder, "gone.xml");

            FileNotFoundException error = Assert.Throws<FileNotFoundException>(() => FileFingerprint.Sha256(path));
            Assert.That(error.Message, Does.Contain(path));
        }

        [TestCase(null)]
        [TestCase("")]
        public void NoPathIsRefused(string path)
        {
            Assert.Throws<ArgumentException>(() => FileFingerprint.Sha256(path));
        }
    }
}
