using System;
using System.Collections.Generic;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    [TestFixture]
    public class ContainerNameTests
    {
        private const string Sample = "1104-PAR-1C07BC-ZZZ-AR-MOD-000001";

        [Test]
        public void ReadsTheBuildingAndTheDiscipline()
        {
            ParsedContainerName parsed = ContainerName.Parse(Sample);

            Assert.That(parsed.IsReadable, Is.True, parsed.UnreadableReason);
            Assert.That(parsed.Building, Is.EqualTo("1C07BC"));
            Assert.That(parsed.Discipline, Is.EqualTo("AR"));
            Assert.That(parsed.Parts.Count, Is.EqualTo(7));
        }

        [Test]
        public void BuildsTheOutputNameBySwappingTheDisciplineForBm()
        {
            string output = ContainerName.BuildOutputName(ContainerName.Parse(Sample));

            Assert.That(output, Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
        }

        [Test]
        public void ReadsAnNwcFileNameThroughItsExtension()
        {
            ParsedContainerName parsed = ContainerName.Parse(Sample + ".nwc");

            Assert.That(parsed.IsReadable, Is.True, parsed.UnreadableReason);
            Assert.That(parsed.Stem, Is.EqualTo(Sample));
            Assert.That(parsed.Building, Is.EqualTo("1C07BC"));
        }

        [Test]
        public void ReadsAFullPathThroughToTheName()
        {
            ParsedContainerName parsed = ContainerName.Parse(@"C:\models\incoming\" + Sample + ".nwc");

            Assert.That(parsed.IsReadable, Is.True, parsed.UnreadableReason);
            Assert.That(parsed.Building, Is.EqualTo("1C07BC"));
            Assert.That(parsed.Discipline, Is.EqualTo("AR"));
        }

        [Test]
        public void TreatsTheFullSixCharacterCodeAsTheBuilding()
        {
            ParsedContainerName first = ContainerName.Parse("1104-PAR-1C07BC-ZZZ-AR-MOD-000001");
            ParsedContainerName second = ContainerName.Parse("1104-PAR-1C07K1-ZZZ-AR-MOD-000001");

            Assert.That(first.Building, Is.EqualTo("1C07BC"));
            Assert.That(second.Building, Is.EqualTo("1C07K1"));
            Assert.That(first.Building, Is.Not.EqualTo(second.Building));
        }

        [Test]
        public void AFourPartNameIsUnreadable()
        {
            ParsedContainerName parsed = ContainerName.Parse("1104-PAR-1C07BC-ZZZ");

            Assert.That(parsed.IsReadable, Is.False);
            Assert.That(parsed.Building, Is.Null);
            Assert.That(parsed.Discipline, Is.Null);
            Assert.That(parsed.UnreadableReason, Is.Not.Null.And.Contains("4 parts"));
        }

        [Test]
        public void AnUnreadableNameNeverProducesAnOutputName()
        {
            ParsedContainerName parsed = ContainerName.Parse("1104-PAR-1C07BC-ZZZ");

            Assert.Throws<InvalidOperationException>(() => ContainerName.BuildOutputName(parsed));
        }

        [Test]
        public void TheSeparatorIsASetting()
        {
            ContainerNameSettings settings = new ContainerNameSettings { Separator = '_' };
            ParsedContainerName parsed = ContainerName.Parse("1104_PAR_1C07BC_ZZZ_AR_MOD_000001", settings);

            Assert.That(parsed.IsReadable, Is.True, parsed.UnreadableReason);
            Assert.That(parsed.Building, Is.EqualTo("1C07BC"));
            Assert.That(parsed.Discipline, Is.EqualTo("AR"));
            Assert.That(
                ContainerName.BuildOutputName(parsed, settings),
                Is.EqualTo("1104_PAR_1C07BC_ZZZ_BM_MOD_000001"));
        }

        [Test]
        public void ThePartPositionsAreSettings()
        {
            ContainerNameSettings settings = ContainerNameSettings.WithoutFixedLevelAndNumber();
            settings.BuildingPart = 2;
            settings.DisciplinePart = 4;

            ParsedContainerName parsed = ContainerName.Parse("XX-1C07BC-YY-AR-ZZ", settings);

            Assert.That(parsed.IsReadable, Is.True, parsed.UnreadableReason);
            Assert.That(parsed.Building, Is.EqualTo("1C07BC"));
            Assert.That(parsed.Discipline, Is.EqualTo("AR"));
        }

        [Test]
        public void TheOutputDisciplineCodeIsASetting()
        {
            ContainerNameSettings settings = new ContainerNameSettings { OutputDisciplineCode = "FD" };

            Assert.That(
                ContainerName.BuildOutputName(ContainerName.Parse(Sample, settings), settings),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-FD-MOD-000001"));
        }

        // The output name reads one way only: project, originator, building, ZZZ, BM,
        // the type code, 000001. Level and number are fixed because outputs overwrite
        // and because the files in one group may disagree on both.
        [Test]
        public void TheOutputNameFixesTheLevelAndTheNumberWhateverTheInputCarried()
        {
            Assert.That(
                ContainerName.BuildOutputName(ContainerName.Parse("1104-PAR-1C07BC-L02-AR-MOD-000456")),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));

            Assert.That(
                ContainerName.BuildOutputName(ContainerName.Parse("1104-PAR-1C07BC-L07-EL-MOD-000912")),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
        }

        [Test]
        public void FourFilesOfOneBuildingAllGiveTheSameOutputName()
        {
            string[] inputs =
            {
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001",
                "1104-PAR-1C07BC-L01-ST-MOD-000004",
                "1104-PAR-1C07BC-L02-ME-MOD-000117",
                "1104-PAR-1C07BC-B01-EL-MOD-000999"
            };

            foreach (string input in inputs)
            {
                Assert.That(
                    ContainerName.BuildOutputName(ContainerName.Parse(input)),
                    Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"),
                    input);
            }
        }

        [Test]
        public void TheTypeCodeIsCarriedThroughRatherThanFixed()
        {
            Assert.That(
                ContainerName.BuildOutputName(ContainerName.Parse("1104-PAR-1C07BC-L02-AR-DOC-000456")),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-DOC-000001"));
        }

        [Test]
        public void TheFixedLevelAndNumberCanBeTurnedOff()
        {
            ContainerNameSettings settings = ContainerNameSettings.WithoutFixedLevelAndNumber();

            Assert.That(
                ContainerName.BuildOutputName("1104-PAR-1C07BC-L02-AR-MOD-000456", settings),
                Is.EqualTo("1104-PAR-1C07BC-L02-BM-MOD-000456"));
        }

        [Test]
        public void ReadsTheProjectCodeAndTheOriginator()
        {
            ParsedContainerName parsed = ContainerName.Parse(Sample);

            Assert.That(parsed.Project, Is.EqualTo("1104"));
            Assert.That(parsed.Originator, Is.EqualTo("PAR"));
        }

        [Test]
        public void ASixPartNameCannotProduceAnOutputNameSoItIsUnreadable()
        {
            ParsedContainerName parsed = ContainerName.Parse("1104-PAR-1C07BC-ZZZ-AR-MOD");

            Assert.That(parsed.IsReadable, Is.False);
            Assert.That(parsed.UnreadableReason, Does.Contain("6 parts").And.Contains("7 are needed"));
        }

        [Test]
        public void AnEmptyBuildingPartIsUnreadable()
        {
            ParsedContainerName parsed = ContainerName.Parse("1104-PAR--ZZZ-AR-MOD-000001");

            Assert.That(parsed.IsReadable, Is.False);
            Assert.That(parsed.UnreadableReason, Does.Contain("building"));
        }

        [Test]
        public void ParseAllKeepsReadableAndUnreadableTogetherInOrder()
        {
            IList<ParsedContainerName> parsed = ContainerName.ParseAll(
                new[] { Sample, "too-short-name", "1104-PAR-1C07K1-ZZZ-ST-MOD-000001" },
                new ContainerNameSettings());

            Assert.That(parsed.Count, Is.EqualTo(3));
            Assert.That(parsed[0].IsReadable, Is.True);
            Assert.That(parsed[1].IsReadable, Is.False);
            Assert.That(parsed[2].IsReadable, Is.True);
        }
    }
}
