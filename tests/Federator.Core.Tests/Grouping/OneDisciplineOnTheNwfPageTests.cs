using System.Collections.Generic;
using Federator.Core.Diagnostics;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F219, Bader's answer to Q203 of 18 Oct 2026: a group of one discipline runs its tests on neither page, and the log
    /// says why on both. The NWF page has no scan, so an NWF's disciplines are read off its models' file names when it opens,
    /// by the scan's own call, and a count that cannot be read is UNKNOWN and stops no test.
    /// </summary>
    [TestFixture]
    public class OneDisciplineOnTheNwfPageTests
    {
        private const string Federated = NamePattern.DefaultDiscipline;

        private static string Nwc(string discipline, string number = "000001")
        {
            return TestPaths.At("Parent", "C06", "1104-PAR-1B06BS-ZZZ-" + discipline + "-MOD-" + number + ".nwc");
        }

        private static IList<string> Read(IList<string> files, out string why)
        {
            return BuildingGroup.DisciplinesOf(files, new ContainerNameSettings(), Federated, out why);
        }

        [Test]
        public void AnNwfOfOneDisciplineReadsOneCodeWhateverItsModelCount()
        {
            string why;
            IList<string> codes = Read(new[] { Nwc("EL"), Nwc("EL", "000002") }, out why);

            Assert.That(why, Is.Null);
            Assert.That(codes.Count, Is.EqualTo(1));
            Assert.That(codes[0], Is.EqualTo("EL"));
            Assert.That(BuildingGroup.CannotClashWith(codes.Count), Is.True);
        }

        [Test]
        public void AnNwfOfTwoDisciplinesReadsBothSorted()
        {
            string why;
            IList<string> codes = Read(new[] { Nwc("ST"), Nwc("AR"), Nwc("ST", "000002") }, out why);

            Assert.That(codes.Count, Is.EqualTo(2));
            Assert.That(codes[0], Is.EqualTo("AR"));
            Assert.That(codes[1], Is.EqualTo("ST"));
            Assert.That(BuildingGroup.CannotClashWith(codes.Count), Is.False);
        }

        [Test]
        public void TheWholePathIsHandedToTheParserAsTheScanHandsIt()
        {
            // The reviewer's finding: the stem was taken a second time, so a dot in the building cut the name short and the
            // NWF page read UNKNOWN where the NWC page read the discipline.
            string dotted = TestPaths.At("Parent", "C06", "1104-PAR-1B06.A-ZZZ-EL-MOD-000001.nwc");
            string why;

            IList<string> codes = Read(new[] { dotted }, out why);

            Assert.That(codes.Count, Is.EqualTo(1));
            Assert.That(codes[0], Is.EqualTo(ContainerName.Parse(dotted, new ContainerNameSettings()).Discipline),
                "the same discipline the scan reads off the same path");
            Assert.That(codes[0], Is.EqualTo("EL"));
        }

        [Test]
        public void ANameThatWillNotReadOrAModelWithNoNameMakesTheCountUnknown()
        {
            string why;

            Assert.That(Read(new[] { Nwc("EL"), TestPaths.At("Parent", "C06", "pump.nwc") }, out why), Is.Null,
                "one name the parser cannot read and the count is never guessed");
            Assert.That(why, Does.Contain("pump.nwc").And.Not.EndWith("."));

            Assert.That(Read(new[] { Nwc("EL"), string.Empty }, out why), Is.Null, "a model with no file name could be of another discipline");
            Assert.That(why, Does.Contain("1 of the 2 models"));

            Assert.That(Read(null, out why), Is.Null);
        }

        [Test]
        public void AModelNamedAsTheToolNamesItsFederatedOutputsIsUnknownAndNeverOneDiscipline()
        {
            // A site NWF built from this tool's NWDs holds models named BM, each of several disciplines.
            string why;

            Assert.That(Read(new[] { Nwc(Federated), Nwc(Federated, "000002") }, out why), Is.Null);
            Assert.That(why, Does.Contain(Federated).And.Contain("federated"));
        }

        [Test]
        public void TheLinesSayWhyOnBothPagesAndClaimNothingTheRunDidNotDo()
        {
            string one = BuildingGroup.OneDisciplineLine("C06  1B06BS", new[] { "EL" });
            string unknown = BuildingGroup.DisciplinesNotReadLine("C06  1B06BS", "the name of pump.nwc does not read, it has 1 part.");

            Assert.That(one, Is.EqualTo("CLASH    C06  1B06BS holds one discipline, EL, so no test of it is run, Bader's answer to Q203"));
            Assert.That(BuildingGroup.OneDisciplineLine("G", null), Does.Contain("UNKNOWN which"));
            Assert.That(unknown, Does.Contain("UNKNOWN").And.Contain("1 part, so it is not judged"),
                "the parser's stop is taken off so the sentence reads on");
        }

        [Test]
        public void AReasonLosesItsStopAndNoneIsUnknown()
        {
            Assert.That(Words.TrimStop("it has 1 part. "), Is.EqualTo("it has 1 part"));
            Assert.That(Words.TrimStop(null), Is.EqualTo("UNKNOWN"));
            Assert.That(Words.TrimStop(" . "), Is.EqualTo("UNKNOWN"));
        }
    }
}
