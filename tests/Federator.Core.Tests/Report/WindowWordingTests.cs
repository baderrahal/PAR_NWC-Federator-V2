using System;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// What the window says before a run.
    ///
    /// Two things were leaking into it. It said "Workbooks go in UNKNOWN" before a scan,
    /// which reads as a fault when nothing is wrong yet, and behind that it showed
    /// "Parameter name: nwfFolder", which is a code identifier and means nothing to
    /// anyone using this. Since F158 the line names the Log folder, whose place is worked
    /// out from the NWF and the NWD folders alone.
    /// </summary>
    [TestFixture]
    public class WindowWordingTests
    {
        private const string Source = @"C:\00_NM\NWC Fed\NWC\test001";
        private const string Nwf = @"C:\00_NM\NWC Fed\NWF\test001";
        private const string Nwd = @"C:\00_NM\NWC Fed\NWD\test001";

        /// <summary>
        /// FR-131. With the NWF folder inside the scanned folder the label said the NWF folder cannot be
        /// read, which is not the reason. It names the scanned folder and carries no parameter name.
        /// </summary>
        [Test]
        public void AnNwfFolderInsideTheScannedFolderIsSaidAsThatAndNotAsUnreadable()
        {
            string scanned = TestPaths.At("nwc", "test001");
            string said = ReportPaths.WhereTheyGo(
                TestPaths.At("nwc", "test001", "NWF"), null, scanned);

            Assert.That(said, Does.Contain("inside the folder being scanned"));
            Assert.That(said, Does.Contain(scanned));
            Assert.That(said, Does.Not.Contain("Parameter name"));
            Assert.That(said, Does.Not.Contain("cannot be read"));
            Assert.That(said, Does.Not.Contain("\n"));
        }

        // The one the brief asks for by name.
        [Test]
        public void BeforeAnythingIsPickedItSaysSoRatherThanUnknown()
        {
            string said = ReportPaths.WhereTheyGo(string.Empty, string.Empty, string.Empty);

            Assert.That(said, Does.Contain("once the NWF folder is picked on this step"));
            Assert.That(said, Does.Contain("Log"));
            Assert.That(said, Does.Not.Contain("UNKNOWN"));
        }

        /// <summary>
        /// A path the framework refuses to join throws its own ArgumentException, whose message is
        /// not the tool's to show. Only the refusal the tool throws on purpose is said as it is.
        /// </summary>
        [Test]
        public void AFrameworkMessageAboutAPathNeverReachesTheLine()
        {
            string said = ReportPaths.WhereTheyGo("C:\\nwf\0folder", Nwd, Source);

            Assert.That(said, Does.Not.Contain("Illegal"));
            Assert.That(said, Does.Not.Contain("path"));
        }

        // The one the brief asks for by name.
        [Test]
        public void NoCodeIdentifierEverReachesTheLine()
        {
            string[] awkward =
            {
                string.Empty, "   ", "not a folder at all", "|||", @"C:\", "?",
                @"\\server\share", "con", @"C:\a\b\c"
            };

            foreach (string nwf in awkward)
            {
                foreach (string nwd in awkward)
                {
                    string said = ReportPaths.WhereTheyGo(nwf, nwd, Source);

                    Assert.That(said, Does.Not.Contain("Parameter name"), nwf + " | " + nwd);
                    Assert.That(said, Does.Not.Contain("nwfFolder"), nwf + " | " + nwd);
                    Assert.That(said, Does.Not.Contain("System."), nwf + " | " + nwd);
                    Assert.That(said, Does.Not.Contain("Exception"), nwf + " | " + nwd);
                    Assert.That(said, Does.Not.Contain("UNKNOWN"), nwf + " | " + nwd);
                    Assert.That(said, Is.Not.Empty, nwf + " | " + nwd);
                }
            }
        }

        [Test]
        public void OnceTheFoldersArePickedItSaysTheRealFolderBesideBoth()
        {
            TestPaths.OnWindowsOnly("a Windows path");

            Assert.That(ReportPaths.WhereTheyGo(Nwf, Nwd, Source),
                Is.EqualTo(@"C:\00_NM\NWC Fed\Log"));
            Assert.That(ReportPaths.WhereTheyGo(Nwf, string.Empty, Source),
                Is.EqualTo(@"C:\00_NM\NWC Fed\NWF\Log"));
        }

        // A Log folder inside the one being scanned is still refused, and the reason still
        // reaches the line, because that one is worth reading.
        [Test]
        public void ALogFolderInsideTheSourceStillSaysWhyItWasRefused()
        {
            string said = ReportPaths.WhereTheyGo(Source + @"\out", null, Source);

            Assert.That(said, Does.Contain("not written into the folder"));
            Assert.That(said, Does.Not.Contain("Parameter name"));
        }

        [Test]
        public void ItNeverThrowsWhateverIsInTheBoxes()
        {
            Assert.That(delegate { ReportPaths.WhereTheyGo(null, null, null); }, Throws.Nothing);
            Assert.That(delegate { ReportPaths.WhereTheyGo("|", "|", "|"); }, Throws.Nothing);
        }
    }
}
