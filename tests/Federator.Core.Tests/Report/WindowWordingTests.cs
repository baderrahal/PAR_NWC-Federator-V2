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

        // The one the brief asks for by name.
        [Test]
        public void BeforeAnythingIsPickedItSaysSoRatherThanUnknown()
        {
            string said = ReportPaths.WhereTheyGo(string.Empty, string.Empty, string.Empty);

            Assert.That(said, Is.EqualTo("beside the NWF folder, once one is picked on this step"));
            Assert.That(said, Does.Not.Contain("UNKNOWN"));
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
