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
    /// anyone using this.
    /// </summary>
    [TestFixture]
    public class WindowWordingTests
    {
        private const string Source = @"C:\00_NM\NWC Fed\NWC\test001";
        private const string Nwf = @"C:\00_NM\NWC Fed\NWF\test001";

        /// <summary>
        /// FR-131. With the NWF folder inside the scanned folder the label said the NWF folder cannot be
        /// read, which is not the reason. It names the scanned folder and carries no parameter name.
        /// </summary>
        [Test]
        public void AnNwfFolderInsideTheScannedFolderIsSaidAsThatAndNotAsUnreadable()
        {
            string scanned = TestPaths.At("nwc", "test001");
            string said = ReportPaths.WhereTheyGo(
                string.Empty, TestPaths.At("nwc", "test001", "NWF"), scanned);

            Assert.That(said, Does.Contain("is inside the folder being scanned"));
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

            Assert.That(said, Is.EqualTo("beside the NWF folder, once one is picked on this step"));
            Assert.That(said, Does.Not.Contain("UNKNOWN"));
        }

        /// <summary>
        /// A path the framework refuses to join throws its own ArgumentException, whose message is
        /// not the tool's to show. Only the refusal the tool throws on purpose is said as it is.
        /// </summary>
        [Test]
        public void AFrameworkMessageAboutAPathNeverReachesTheLine()
        {
            string said = ReportPaths.WhereTheyGo(string.Empty, "C:\\nwf\0folder", Source);

            Assert.That(said, Does.Not.Contain("Illegal"));
            Assert.That(said, Does.Not.Contain("path"));
            Assert.That(said, Is.EqualTo(
                "not worked out yet, the NWF folder on this step cannot be read"));
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

            foreach (string picked in awkward)
            {
                foreach (string nwf in awkward)
                {
                    string said = ReportPaths.WhereTheyGo(picked, nwf, Source);

                    Assert.That(said, Does.Not.Contain("Parameter name"), picked + " | " + nwf);
                    Assert.That(said, Does.Not.Contain("nwfFolder"), picked + " | " + nwf);
                    Assert.That(said, Does.Not.Contain("System."), picked + " | " + nwf);
                    Assert.That(said, Does.Not.Contain("Exception"), picked + " | " + nwf);
                    Assert.That(said, Does.Not.Contain("UNKNOWN"), picked + " | " + nwf);
                    Assert.That(said, Is.Not.Empty, picked + " | " + nwf);
                }
            }
        }

        [Test]
        public void APickedFolderIsShownEvenBeforeTheNwfFolderIs()
        {
            Assert.That(ReportPaths.WhereTheyGo(@"C:\out\reports", string.Empty, string.Empty),
                Is.EqualTo(@"C:\out\reports"));
        }

        [Test]
        public void OnceBothArePickedItSaysTheRealFolder()
        {
            Assert.That(ReportPaths.WhereTheyGo(string.Empty, Nwf, Source),
                Does.StartWith(Nwf));
            Assert.That(ReportPaths.WhereTheyGo(@"C:\out\reports", Nwf, Source),
                Is.EqualTo(@"C:\out\reports"));
        }

        // A folder inside the one being scanned is still refused, and the reason still
        // reaches the line, because that one is worth reading.
        [Test]
        public void AFolderInsideTheSourceStillSaysWhyItWasRefused()
        {
            string said = ReportPaths.WhereTheyGo(Source, Nwf, Source);

            Assert.That(said, Does.Contain("Reports are not written into the folder"));
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
