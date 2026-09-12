using System;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F26. The report goes out in meters whatever the document measures in. The numbers
    /// here are the ones from the run of 2026-09-07, where every model was set to meters,
    /// the document stayed in feet, and the report was written in feet.
    /// </summary>
    [TestFixture]
    public class ReportUnitsTests
    {
        private static ClashReport InFeet()
        {
            ClashReport report = new ClashReport("1B06BC", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001");
            report.DocumentUnits = "ft";

            TestReport test = report.AddTest("BLD-AR-Floors v BLD-ME-Ducts");
            test.Tolerance = 0.2460629921;
            test.ToleranceUnits = "ft";
            test.State = TestState.FoundClashes;

            ClashRow row = new ClashRow();
            row.Name = "Clash1";
            row.Status = ClashStatus.New;
            row.Distance = -0.328083989501312;
            row.X = 3.28083989501312;
            row.Y = -6.56167979002625;
            row.Z = 1.0;
            test.Add(row);

            return report;
        }

        // ---------- the table ----------

        // One unit table in Core, UnitTable, read through ExchangeUnits. These are the five
        // units a document on this project can realistically be in.
        [TestCase("m", 1.0)]
        [TestCase("ft", 0.3048)]
        [TestCase("in", 0.0254)]
        [TestCase("mm", 0.001)]
        [TestCase("cm", 0.01)]
        public void EveryKnownUnitConvertsToMeters(string units, double expected)
        {
            Assert.That(ReportUnits.ToMeters(1.0, units), Is.EqualTo(expected).Within(1e-12));
            Assert.That(ReportUnits.IsKnown(units), Is.True);
        }

        [Test]
        public void TheReportUnitIsMetersAndSaysSoInBothForms()
        {
            Assert.That(ReportUnits.Short, Is.EqualTo("m"));
            Assert.That(ReportUnits.Name, Is.EqualTo("Meters (m)"));
        }

        // ---------- the tolerance ----------

        // The clash file tolerance is 75 mm, which reads 0.2460629921 in a document
        // measuring feet. In meters it is 0.075, which is what the client's report shows.
        [Test]
        public void TheToleranceIsConvertedAndItsLabelWithIt()
        {
            ClashReport report = InFeet();

            ReportUnitsOutcome outcome = ReportUnits.ToMeters(report);

            Assert.That(outcome.Refused, Is.False);
            Assert.That(report.Tests[0].Tolerance, Is.EqualTo(0.075).Within(1e-9));
            Assert.That(report.Tests[0].ToleranceUnits, Is.EqualTo("m"));
            Assert.That(report.Tests[0].ClientTolerance(), Is.EqualTo("0.075m"),
                "the cell carries the number and the unit with no space, and both are metric");
        }

        // ---------- one row ----------

        [Test]
        public void ARowsDistanceAndClashPointAreConverted()
        {
            ClashReport report = InFeet();

            ReportUnits.ToMeters(report);
            ClashRow row = report.Tests[0].Rows[0];

            Assert.That(row.Distance, Is.EqualTo(-0.1).Within(1e-9), "a hard clash keeps its sign");
            Assert.That(row.X, Is.EqualTo(1.0).Within(1e-9));
            Assert.That(row.Y, Is.EqualTo(-2.0).Within(1e-9));
            Assert.That(row.Z, Is.EqualTo(0.3048).Within(1e-9));
            Assert.That(row.ClientClashPoint(), Is.EqualTo("x:1.000, y:-2.000, z:0.305"));
        }

        [Test]
        public void NothingButTheMeasuredNumbersIsTouched()
        {
            ClashReport report = InFeet();
            ClashRow row = report.Tests[0].Rows[0];
            row.GridLocation = "B-1";
            row.Level = "ROF";
            row.RawClashes = 14;

            ReportUnits.ToMeters(report);

            Assert.That(row.GridLocation, Is.EqualTo("B-1"));
            Assert.That(row.Level, Is.EqualTo("ROF"));
            Assert.That(row.RawClashes, Is.EqualTo(14));
            Assert.That(row.Status, Is.EqualTo(ClashStatus.New));
            Assert.That(row.Name, Is.EqualTo("Clash1"));
        }

        // ---------- the report label ----------

        [Test]
        public void TheReportSaysMetersAfterwards()
        {
            ClashReport report = InFeet();

            ReportUnits.ToMeters(report);

            Assert.That(report.DocumentUnits, Is.EqualTo("m"),
                "the XML units attribute and the page read this");
        }

        [Test]
        public void ADocumentAlreadyInMetersConvertsNothingAndStillCarriesTheLabel()
        {
            ClashReport report = InFeet();
            report.DocumentUnits = "m";
            report.Tests[0].Tolerance = 0.075;

            ReportUnitsOutcome outcome = ReportUnits.ToMeters(report);

            Assert.That(outcome.Refused, Is.False);
            Assert.That(outcome.Changed, Is.False);
            Assert.That(report.Tests[0].Tolerance, Is.EqualTo(0.075).Within(1e-9), "not multiplied twice");
            Assert.That(report.Tests[0].ToleranceUnits, Is.EqualTo("m"));
            Assert.That(outcome.Line(), Does.Contain("already"));
        }

        // ---------- an unknown unit ----------

        [Test]
        public void AnUnknownUnitIsRefusedAndNoNumberIsChanged()
        {
            ClashReport report = InFeet();
            report.DocumentUnits = "furlong";

            ReportUnitsOutcome outcome = ReportUnits.ToMeters(report);

            Assert.That(outcome.Refused, Is.True);
            Assert.That(outcome.Problem, Does.Contain("furlong"));
            Assert.That(outcome.Problem, Does.Contain("not a unit this tool has been taught"));
            Assert.That(report.Tests[0].Tolerance, Is.EqualTo(0.2460629921).Within(1e-9),
                "nothing is half converted");
            Assert.That(report.DocumentUnits, Is.EqualTo("furlong"), "the label is not faked to meters");
            Assert.That(outcome.Line(), Does.Contain("NOT written"));
        }

        [Test]
        public void ADocumentThatSaidNothingAboutItsUnitsIsRefusedToo()
        {
            ClashReport report = InFeet();
            report.DocumentUnits = string.Empty;

            ReportUnitsOutcome outcome = ReportUnits.ToMeters(report);

            Assert.That(outcome.Refused, Is.True);
            Assert.That(outcome.Problem, Does.Contain("did not say what units"));
        }

        [Test]
        public void NoReportIsRefusedRatherThanConverted()
        {
            Assert.Throws<ArgumentNullException>(delegate { ReportUnits.ToMeters((ClashReport)null); });
        }

        // ---------- the log line ----------

        [Test]
        public void TheLineNamesTheUnitTheFactorAndTheCounts()
        {
            ClashReport report = InFeet();

            string line = ReportUnits.ToMeters(report).Line();

            Assert.That(line, Does.StartWith("UNITS    every number converted from ft to m"));
            Assert.That(line, Does.Contain("one ft is 0.3048 m"));
            Assert.That(line, Does.Contain("1 test and 1 row"));
            Assert.That(line, Does.Contain("The report is written in Meters (m)"));
            Assert.That(line, Does.Not.Contain("DID NOT FOLLOW"));
        }

        [Test]
        public void EveryTestAndEveryRowIsCounted()
        {
            ClashReport report = InFeet();
            TestReport second = report.AddTest("second");
            second.Tolerance = 0.2460629921;

            for (int i = 0; i < 3; i++)
            {
                ClashRow row = new ClashRow();
                row.Distance = -1.0;
                second.Add(row);
            }

            ReportUnitsOutcome outcome = ReportUnits.ToMeters(report);

            Assert.That(outcome.Tests, Is.EqualTo(2));
            Assert.That(outcome.Rows, Is.EqualTo(4));
            Assert.That(second.Rows[0].Distance, Is.EqualTo(-0.3048).Within(1e-9));
        }
    }
}
