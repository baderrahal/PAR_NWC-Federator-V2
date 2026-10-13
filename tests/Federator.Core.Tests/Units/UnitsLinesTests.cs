using Federator.Core.Units;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The UNITS lines, FR-062. Where the document's units could not be read again after the
    /// models were set, the line named the units read before, which reads as a measurement
    /// nobody took. Whether Document.Units follows the models is UNKNOWN, so a read that
    /// threw has to say UNKNOWN and name no unit as what the document shows.
    /// </summary>
    [TestFixture]
    public class UnitsLinesTests
    {
        [Test]
        public void AnAfterReadThatThrewSaysUnknownAndNamesNoUnitAsShown()
        {
            string line = UnitsLines.AfterSetting(4, 0, "Feet", null);

            Assert.That(line, Does.Not.Contain("document shows Feet"),
                "the units read before the models were set were never read again");
            Assert.That(line, Does.Contain("UNKNOWN"));
            Assert.That(line, Does.Contain("4 models set, 0 that would not"));
        }

        [Test]
        public void AnAfterReadThatThrewStillSaysWhatTheDocumentWasBefore()
        {
            string line = UnitsLines.AfterSetting(4, 0, "Feet", null);

            Assert.That(line, Does.Contain("Feet (ft) before"),
                "what was read is still said, as the reading before");
        }

        [Test]
        public void AnAfterReadThatWorkedNamesWhatTheDocumentShows()
        {
            Assert.That(UnitsLines.AfterSetting(1, 2, "Feet", "Meters"),
                Does.StartWith("UNITS    1 model set, 2 that would not, document shows Meters (m)."));
        }

        [Test]
        public void TheReportIsSaidMetricWhateverTheDocumentShows()
        {
            Assert.That(UnitsLines.AfterSetting(1, 0, "Feet", null),
                Does.EndWith("so the report is metric whatever this says."));
        }

        [Test]
        public void TheLinesBeforeTheSettingNameBothUnits()
        {
            Assert.That(UnitsLines.AlreadyIn("Meters"),
                Is.EqualTo("UNITS    the document is already in Meters (m), so nothing was changed."));
            Assert.That(UnitsLines.Setting("Feet", "Meters"),
                Is.EqualTo("UNITS    the document is in Feet (ft) and the report needs Meters (m). Setting every model."));
        }

        [Test]
        public void AUnitTheTableDoesNotCarryIsWrittenByItsOwnName()
        {
            Assert.That(UnitsLines.Setting("Furlongs", "Meters"), Does.Contain("Furlongs (Furlongs)"));
        }
    }
}
