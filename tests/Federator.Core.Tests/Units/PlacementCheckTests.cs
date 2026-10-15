using System.Collections.Generic;
using System.IO;
using Federator.Core.Report;
using Federator.Core.Units;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F201, the alignment of 04 PK. Each model as it was appended against itself as the NWD is published, one PLACE line
    /// a file, held within 1 mm or named. The centres are the ones measured on the copy of 1A04PK on 15 Oct 2026, the
    /// NWCs appended alone and then set to Meters with their own transform as the tool did before F201. Sample data.
    /// </summary>
    [TestFixture]
    public class PlacementCheckTests
    {
        private const string Ar = "1104-PAR-1A04PK-ZZZ-AR-MOD-000001.nwc";
        private const string Me = "1104-PAR-1A04PK-ZZZ-ME-MOD-000001.nwc";

        private static ModelBox Box(string file, string units, double x, double y, double z)
        {
            return new ModelBox(file, units, x, y, z);
        }

        private static string Joined(IEnumerable<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        [Test]
        public void AModelThatDidNotMoveIsHeld()
        {
            IList<string> lines = PlacementCheck.Lines(
                new[] { Box(Ar, "Feet", 658349.9302, 2745901.1215, 700.6905) },
                new[] { Box(Ar, "Feet", 658349.9302, 2745901.1215, 700.6905) },
                "Feet", PlacementCheck.DefaultToleranceMillimetres);

            Assert.That(lines[1], Is.EqualTo("PLACE    " + Ar + "  Feet  centre E 658349.930 N 2745901.122 Z 700.691 m when appended, moved 0 mm before the NWD, held"));
            Assert.That(lines[2], Is.EqualTo("PLACE    1 of 1 models held, 0 moved, 0 not read, 0 in units that are not the document's"));
        }

        [Test]
        public void HeldIsOneMillimetreOrLessAndMoreIsNamed()
        {
            IList<string> justIn = PlacementCheck.Lines(new[] { Box(Ar, "Feet", 0, 0, 0) }, new[] { Box(Ar, "Feet", 0.001, 0, 0) }, "Feet", 1.0);
            IList<string> justOut = PlacementCheck.Lines(new[] { Box(Ar, "Feet", 0, 0, 0) }, new[] { Box(Ar, "Feet", 0.0011, 0, 0) }, "Feet", 1.0);

            Assert.That(justIn[1], Does.EndWith("moved 1 mm before the NWD, held"));
            Assert.That(justOut[1], Does.EndWith("moved 1.1 mm before the NWD, MOVED more than 1 mm"));
            Assert.That(justOut[2], Does.StartWith("PLACE    0 of 1 models held, 1 moved"));
        }

        /// <summary>The two centres of ME-000001 measured on 15 Oct 2026, appended alone and then set to Meters as before F201.</summary>
        [Test]
        public void TheMoveTheOldUnitsStepMadeOnMeIsNamedInMetresOfIt()
        {
            ModelBox appended = Box(Me, "Feet", 658346.8438, 2745904.4046, 686.4090);
            ModelBox setToMeters = Box(Me, "Meters", 657690.5152, 2746022.4472, 694.1296);

            Assert.That(PlacementCheck.DistanceMillimetres(appended, setToMeters), Is.EqualTo(666904).Within(1.0));

            IList<string> lines = PlacementCheck.Lines(new[] { appended }, new[] { setToMeters }, "Feet", 1.0);

            Assert.That(lines[1], Does.Contain("MOVED more than 1 mm"));
        }

        [Test]
        public void AModelWhoseUnitsAreNotTheDocumentsIsNamedWithTheRebuild()
        {
            IList<string> lines = PlacementCheck.Lines(
                new[] { Box(Me, "Meters", 1, 2, 3) }, new[] { Box(Me, "Meters", 1, 2, 3) }, "Feet", 1.0);

            Assert.That(Joined(lines), Does.Contain("PLACE    " + Me + "  its units Meters are not the document's Feet. A run of this tool before F201"));
            Assert.That(Joined(lines), Does.Contain("Rebuild the group on the NWC page"));
            Assert.That(lines[lines.Count - 1], Does.EndWith("1 in units that are not the document's"));
        }

        [Test]
        public void AModelNotReadOrNotFoundIsUnknownAndNeverHeld()
        {
            IList<string> lines = PlacementCheck.Lines(
                new[] { new ModelBox(Ar, "Feet", "its root item has no box"), Box(Me, "Feet", 1, 2, 3) },
                new[] { Box(Ar, "Feet", 1, 2, 3) },
                "Feet", 1.0);

            Assert.That(lines[1], Is.EqualTo("PLACE    " + Ar + "  Feet  UNKNOWN, its box was not read when it was appended, its root item has no box"));
            Assert.That(lines[2], Is.EqualTo("PLACE    " + Me + "  Feet  UNKNOWN, it was not found as the NWD is published"));
            Assert.That(lines[3], Is.EqualTo("PLACE    0 of 2 models held, 0 moved, 2 not read, 0 in units that are not the document's"));
        }

        [Test]
        public void AModelIsPairedByItsFileNameWhereTheOrderChanged()
        {
            IList<string> lines = PlacementCheck.Lines(
                new[] { Box(Ar, "Feet", 1, 2, 3), Box(Me, "Feet", 4, 5, 6) },
                new[] { Box(Me, "Feet", 4, 5, 6), Box(Ar, "Feet", 1, 2, 3) },
                "Feet", 1.0);

            Assert.That(lines[lines.Count - 1], Does.StartWith("PLACE    2 of 2 models held, 0 moved"));
        }

        [Test]
        public void TheToleranceIsASettingOfOneMillimetre()
        {
            Assert.That(new ReportOptions().PlacementToleranceMillimetres, Is.EqualTo(1.0));
        }

        /// <summary>The add-in sets no model's units and no model's transform, read off its source, so the fix cannot come back undone.</summary>
        [Test]
        public void TheAddInSetsNoModelsUnitsOrTransform()
        {
            foreach (string file in Directory.GetFiles(Path.Combine(Samples.Repo(), "src", "Federator.Addin"), "*.cs", SearchOption.AllDirectories))
            {
                string source = File.ReadAllText(file);

                Assert.That(source, Does.Not.Contain("SetModelUnitsAndTransform("), file);
                Assert.That(source, Does.Not.Contain("OverridePermanentTransform("), file);
            }

            string engine = File.ReadAllText(Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Engine", "FederationEngine.cs"));

            int first = engine.IndexOf("appended = ModelBoxReader.Read(document)", System.StringComparison.Ordinal);
            int second = engine.IndexOf("SayWhereTheModelsAre(document, appended);", System.StringComparison.Ordinal);

            Assert.That(first, Is.GreaterThan(0), "the models are read once the files are in");
            Assert.That(second, Is.GreaterThan(first), "and again later");
            Assert.That(engine.IndexOf("WriteNwd(document, job, outcome);", second, System.StringComparison.Ordinal), Is.GreaterThan(second),
                "the second read is made before the NWD is written");
        }
    }
}
