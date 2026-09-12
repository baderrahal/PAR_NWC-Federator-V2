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
        public void ReadsAnNwcFileNameThroughItsExtension()
        {
            ParsedContainerName parsed = ContainerName.Parse(Sample + ".nwc");

            Assert.That(parsed.IsReadable, Is.True, parsed.UnreadableReason);
            Assert.That(parsed.Stem, Is.EqualTo(Sample));
            Assert.That(parsed.Building, Is.EqualTo("1C07BC"));
        }

        /// <summary>
        /// The folder in front of the name is dropped and the name is read. The path used
        /// to be typed with backslashes, and off Windows that is one long file name whose
        /// parts after the first are still the right ones, so this passed while proving
        /// nothing. The Stem is asserted too, because that is the part that shows the
        /// folder was actually taken off.
        /// </summary>
        [Test]
        public void ReadsAFullPathThroughToTheName()
        {
            ParsedContainerName parsed = ContainerName.Parse(
                TestPaths.At("models", "incoming", Sample + ".nwc"));

            Assert.That(parsed.IsReadable, Is.True, parsed.UnreadableReason);
            Assert.That(parsed.Stem, Is.EqualTo(Sample), "the folder was not taken off the name");
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
        public void TheSeparatorIsASetting()
        {
            ContainerNameSettings settings = new ContainerNameSettings { Separator = '_' };
            ParsedContainerName parsed = ContainerName.Parse("1104_PAR_1C07BC_ZZZ_AR_MOD_000001", settings);

            Assert.That(parsed.IsReadable, Is.True, parsed.UnreadableReason);
            Assert.That(parsed.Building, Is.EqualTo("1C07BC"));
            Assert.That(parsed.Discipline, Is.EqualTo("AR"));
            Assert.That(parsed.Project, Is.EqualTo("1104"));
        }

        [Test]
        public void ThePartPositionsAreSettings()
        {
            ContainerNameSettings settings = new ContainerNameSettings
            {
                ProjectPart = 5,
                OriginatorPart = 3,
                BuildingPart = 2,
                DisciplinePart = 4
            };

            ParsedContainerName parsed = ContainerName.Parse("XX-1C07BC-YY-AR-ZZ", settings);

            Assert.That(parsed.IsReadable, Is.True, parsed.UnreadableReason);
            Assert.That(parsed.Building, Is.EqualTo("1C07BC"));
            Assert.That(parsed.Discipline, Is.EqualTo("AR"));
            Assert.That(parsed.Project, Is.EqualTo("ZZ"));
            Assert.That(parsed.Originator, Is.EqualTo("YY"));
        }







        [Test]
        public void ReadsTheProjectCodeAndTheOriginator()
        {
            ParsedContainerName parsed = ContainerName.Parse(Sample);

            Assert.That(parsed.Project, Is.EqualTo("1104"));
            Assert.That(parsed.Originator, Is.EqualTo("PAR"));
        }

        // The floor is five parts, because only parts 1, 2, 3 and 5 are read. A six part
        // name and a five part name both parse. The full seven field output name is the
        // pattern's job, proved in NamePatternTests.
        [Test]
        public void ASixPartNameParses()
        {
            ParsedContainerName parsed = ContainerName.Parse("1104-PAR-1C07BC-ZZZ-AR-MOD");

            Assert.That(parsed.IsReadable, Is.True, parsed.UnreadableReason);
            Assert.That(parsed.Building, Is.EqualTo("1C07BC"));
            Assert.That(parsed.Discipline, Is.EqualTo("AR"));
        }

        [Test]
        public void AFivePartNameParses()
        {
            ParsedContainerName parsed = ContainerName.Parse("1104-PAR-1C07BC-ZZZ-AR");

            Assert.That(parsed.IsReadable, Is.True, parsed.UnreadableReason);
            Assert.That(parsed.Building, Is.EqualTo("1C07BC"));
            Assert.That(parsed.Discipline, Is.EqualTo("AR"));
        }

        [Test]
        public void TheReadableNameFloorIsFiveParts()
        {
            Assert.That(new ContainerNameSettings().MinimumParts, Is.EqualTo(5));
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
