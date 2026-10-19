using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Communities;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F243 attempt 2, the lead's decision on item 1. Every group of a run of packages runs in its own Navisworks, so the one list
    /// of models not on the same shared coordinates is made by the window that started the run, over the facts each group's
    /// result carries back: the tally's own record of the group, OffCoordinatesGroup, written and read by the result file's own
    /// rules. A group with no result or no facts is not reached, never clean.
    /// </summary>
    [TestFixture]
    public class OffCoordinatesCarriedTests
    {
        private static readonly DateTime Started = new DateTime(2026, 10, 19, 21, 30, 0);

        private const string Far = "1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc  site T1  34.014 m off, X 30.002 Y -16.010 Z 0.000";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorOffCoordinatesCarried");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static OffCoordinates OneFar()
        {
            return new OffCoordinates("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc", new List<string> { Far }, 1, 4,
                new List<string> { "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc" }, new List<string> { "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc" });
        }

        private static OffCoordinates Clean()
        {
            return new OffCoordinates("1104-PAR-1B06G1-ZZZ-AR-MOD-000001.nwc", new List<string>(), 0, 4);
        }

        private static IList<PackageRunUnit> Units(params string[] groups)
        {
            List<PackageRunUnit> units = new List<PackageRunUnit>();

            foreach (string group in groups)
            {
                units.Add(new PackageRunUnit("C06", TestPaths.At("Report", "C06", "NWF", group + ".nwf"), group));
            }

            return units;
        }

        private PackageRunResult ReadBack(PackageRunResult written)
        {
            string path = Path.Combine(folder, Guid.NewGuid().ToString("N") + ".txt");
            written.Write(path);
            string why;
            PackageRunResult read = PackageRunResult.Read(path, out why);
            Assert.That(read, Is.Not.Null, why);
            return read;
        }

        [Test]
        public void TheFactsOfAGroupGoThroughTheResultFileWhole()
        {
            PackageRunResult result = new PackageRunResult("C06", null);
            result.OffCoordinates.Add(new OffCoordinatesGroup("C06  1B06BC", OneFar(), false));
            result.OffCoordinates.Add(new OffCoordinatesGroup("C06  1B06K1", null, true));
            result.OffCoordinates.Add(new OffCoordinatesGroup("C06  1B06G1", new OffCoordinates(null, new List<string>(), 2, 0), true));

            PackageRunResult read = ReadBack(result);

            Assert.That(read.OffCoordinates.Count, Is.EqualTo(3));

            OffCoordinatesGroup far = read.OffCoordinates[0];
            Assert.That(far.Building, Is.EqualTo("C06  1B06BC"));
            Assert.That(far.RunsATest, Is.False);
            Assert.That(far.Off.Reference, Is.EqualTo("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc"));
            Assert.That(far.Off.Models.Count, Is.EqualTo(1));
            Assert.That(far.Off.Models[0], Is.EqualTo(Far));
            Assert.That(far.Off.OnInternal[0], Is.EqualTo("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc"));
            Assert.That(far.Off.NoSite[0], Is.EqualTo("1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc"));
            Assert.That(far.Off.NotJudged, Is.EqualTo(1));
            Assert.That(far.Off.ModelsRead, Is.EqualTo(4));
            Assert.That(read.OffCoordinates[1].Off, Is.Null, "a read that threw stays not read");
            Assert.That(read.OffCoordinates[2].Off.Reference, Is.Null, "no model placed stays no reference");
            Assert.That(read.OffCoordinates[2].Off.NotJudged, Is.EqualTo(2));

            OffCoordinatesAcrossTheRun before = new OffCoordinatesAcrossTheRun(Started, 1);

            foreach (OffCoordinatesGroup group in result.OffCoordinates)
            {
                before.Add(group.Building, group.Off, group.RunsATest);
            }

            Assert.That(PackageRunResult.OffCoordinatesOf(Started, Units("1B06BC"), new[] { read }).ForModellers(), Is.EqualTo(before.ForModellers()),
                "the list made over the carried facts is the list the group's own tally makes");
        }

        /// <summary>
        /// F243 attempt 3, the reviewer's N7. A result of the shape before the coordinates record, 3, is refused with why, so a
        /// group's Navisworks of an older build never reads as one whose check found nothing.
        /// </summary>
        [Test]
        public void AResultOfTheShapeBeforeTheRecordIsRefusedWithWhy()
        {
            string path = Path.Combine(folder, "shape-3.txt");
            string old = "parsons nwc federator, the result of one group of a run of packages, 3";
            PackageRunFile.Write(path, old, new List<string>
            {
                PackageRunFile.Line("package", "C06"),
                PackageRunFile.Line("log", string.Empty),
                PackageRunFile.Line("end", "written whole")
            });

            string why;

            Assert.That(PackageRunResult.Header, Is.Not.EqualTo(old));
            Assert.That(PackageRunResult.Read(path, out why), Is.Null);
            Assert.That(why, Does.Contain("does not start with the line " + PackageRunResult.Header));
        }

        /// <summary>The break: a facts line of the wrong shape reads the whole result as null with why, as every other key does.</summary>
        [Test]
        public void AFactsLineThatDoesNotReadMakesTheWholeResultNoResult()
        {
            string[][] broken =
            {
                new[] { "offgroup", "C06  1B06BC", "yes", "read" },
                new[] { "offmodel", Far },
                new[] { "offgroup", "C06  1B06BC", "maybe", "read", "read", "AR.nwc", "0", "4" },
                new[] { "offgroup", "C06  1B06BC", "yes", "read", "read", "AR.nwc", "x", "4" }
            };

            foreach (string[] line in broken)
            {
                string path = Path.Combine(folder, Guid.NewGuid().ToString("N") + ".txt");
                List<string> lines = new List<string>
                {
                    PackageRunFile.Line("package", "C06"),
                    PackageRunFile.Line("log", string.Empty)
                };
                string[] values = new string[line.Length - 1];
                Array.Copy(line, 1, values, 0, values.Length);
                lines.Add(PackageRunFile.Line(line[0], values));
                lines.Add(PackageRunFile.Line("end", "written whole"));
                PackageRunFile.Write(path, PackageRunResult.Header, lines);

                string why;
                PackageRunResult read = PackageRunResult.Read(path, out why);

                Assert.That(read, Is.Null, string.Join(" ", line));
                Assert.That(why, Does.Contain("has a line that does not read"), string.Join(" ", line));
            }
        }

        /// <summary>
        /// A run of two groups in two Navisworks, one with a model 34 m off and one clean, gives ONE list that judged both and
        /// carries the NOTICE of the first. A group with no result is not reached and never clean.
        /// </summary>
        [Test]
        public void TwoGroupsGiveOneListHoldingBothAndTheNoticeOfTheFirst()
        {
            PackageRunResult first = new PackageRunResult("C06", null);
            first.OffCoordinates.Add(new OffCoordinatesGroup("C06  1B06BC", new OffCoordinates(
                "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc", new List<string> { Far }, 0, 4), true));
            PackageRunResult second = new PackageRunResult("C06", null);
            second.OffCoordinates.Add(new OffCoordinatesGroup("C06  1B06G1", Clean(), true));

            IList<string> list = PackageRunResult.OffCoordinatesOf(Started, Units("1B06BC", "1B06G1"), new[] { ReadBack(first), ReadBack(second) }).ForModellers();
            string all = string.Join("\n", list);

            Assert.That(list[0], Is.EqualTo("Models not on the same shared coordinates, from the run started 2026-10-19 21:30"));
            Assert.That(list[1], Is.EqualTo("The check judged 2 of 2 groups of this run."));
            Assert.That(all, Does.Contain("NOTICE. C06  1B06BC: 1 model is not on the same shared coordinates, measured from the reference"
                + " model 1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc"));
            Assert.That(all, Does.Contain("   " + Far));
            Assert.That(all, Does.Not.Contain("1B06G1:"), "the clean group is judged and not listed");

            IList<string> missing = PackageRunResult.OffCoordinatesOf(Started, Units("1B06BC", "1B06K1", "1B06PK"), new[] { ReadBack(first), null, new PackageRunResult("C06", null) })
                .ForModellers();

            Assert.That(missing[1], Is.EqualTo("The check judged 1 of 3 groups of this run. 2 were not judged here, no result or no"
                + " record of the check came back from C06  1B06K1, C06  1B06PK."),
                "F243 attempt 3, the breaker's 2 and 3: named by label in words true for the window, never clean");
            Assert.That(string.Join("\n", missing), Does.Not.Contain("the run stopped or the group ended"));
        }
    }
}
