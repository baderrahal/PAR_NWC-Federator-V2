using System.Collections.Generic;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F219, Bader's answer to Q203 of 18 Oct 2026: a group of one discipline runs its tests on neither page, and the log
    /// says why on both. The NWF page has no scan, so an NWF's disciplines are read off its models' file names when it opens,
    /// and a count that cannot be read is UNKNOWN and stops no test.
    /// </summary>
    [TestFixture]
    public class OneDisciplineOnTheNwfPageTests
    {
        private static string Nwc(string discipline, string number = "000001")
        {
            return TestPaths.At("Parent", "C06", "1104-PAR-1B06BS-ZZZ-" + discipline + "-MOD-" + number + ".nwc");
        }

        [Test]
        public void AnNwfOfOneDisciplineReadsOneCodeWhateverItsModelCount()
        {
            string why;
            IList<string> codes = BuildingGroup.DisciplinesOf(new[] { Nwc("EL"), Nwc("EL", "000002") }, 2, new ContainerNameSettings(), out why);

            Assert.That(why, Is.Null);
            Assert.That(codes.Count, Is.EqualTo(1));
            Assert.That(codes[0], Is.EqualTo("EL"));
            Assert.That(BuildingGroup.CannotClashWith(codes.Count), Is.True);
        }

        [Test]
        public void AnNwfOfTwoDisciplinesReadsBothSorted()
        {
            string why;
            IList<string> codes = BuildingGroup.DisciplinesOf(new[] { Nwc("ST"), Nwc("AR"), Nwc("ST", "000002") }, 3, new ContainerNameSettings(), out why);

            Assert.That(codes.Count, Is.EqualTo(2));
            Assert.That(codes[0], Is.EqualTo("AR"));
            Assert.That(codes[1], Is.EqualTo("ST"));
            Assert.That(BuildingGroup.CannotClashWith(codes.Count), Is.False);
        }

        [Test]
        public void ANameThatWillNotReadOrAModelWithNoNameMakesTheCountUnknown()
        {
            string why;

            Assert.That(BuildingGroup.DisciplinesOf(new[] { Nwc("EL"), TestPaths.At("Parent", "C06", "pump.nwc") }, 2, new ContainerNameSettings(), out why),
                Is.Null, "one name the parser cannot read and the count is never guessed");
            Assert.That(why, Does.Contain("pump.nwc"));

            Assert.That(BuildingGroup.DisciplinesOf(new[] { Nwc("EL") }, 2, new ContainerNameSettings(), out why), Is.Null,
                "a model with no file name could be of another discipline");
            Assert.That(why, Does.Contain("1 of the 2 models"));

            Assert.That(BuildingGroup.DisciplinesOf(null, 0, new ContainerNameSettings(), out why), Is.Null);
        }

        [Test]
        public void TheLinesSayWhyOnBothPages()
        {
            string one = BuildingGroup.OneDisciplineLine("C06  1B06BS", new[] { "EL" });
            string unknown = BuildingGroup.DisciplinesNotReadLine("C06  1B06BS", "the name of pump.nwc does not read");

            Assert.That(one, Does.StartWith("CLASH    C06  1B06BS holds one discipline, EL, so no test is run"));
            Assert.That(one, Does.Contain("Q203"));
            Assert.That(BuildingGroup.OneDisciplineLine("G", null), Does.Contain("UNKNOWN which"));
            Assert.That(unknown, Does.Contain("UNKNOWN").And.Contain("pump.nwc").And.Contain("its tests run"));
        }
    }
}
