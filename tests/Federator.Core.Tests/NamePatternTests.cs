using System;
using System.Collections.Generic;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The four fields the tool supplies rather than reads out of a file name, and the
    /// check that stops two groups being written to one name.
    ///
    /// Outputs overwrite with no date suffix, so two groups sharing a name is not a
    /// warning. The second silently destroys the first and only shows up later as a
    /// federation nobody can find.
    /// </summary>
    [TestFixture]
    public class NamePatternTests
    {
        private static readonly ContainerNameSettings Settings = new ContainerNameSettings();

        private static readonly string[] TwoBuildings =
        {
            "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1C07BC-ZZZ-ST-MOD-000001.nwc",
            "1104-PAR-1C07K1-ZZZ-AR-MOD-000001.nwc"
        };

        private static BuildingGroupingResult Group(GroupingMode mode)
        {
            return BuildingGrouping.GroupNames(TwoBuildings, Settings, mode);
        }

        // ---------- the defaults ----------

        [Test]
        public void TheDefaultsAreTheOnesTheProjectHasBeenUsing()
        {
            NamePattern pattern = new NamePattern();

            Assert.That(pattern.Level, Is.EqualTo("ZZZ"));
            Assert.That(pattern.Discipline, Is.EqualTo("BM"));
            Assert.That(pattern.TypeCode, Is.EqualTo("MOD"));
            Assert.That(pattern.Number, Is.EqualTo("000001"));
            Assert.That(pattern.AllBuildings, Is.EqualTo("ZZZZZZ"));
        }

        [Test]
        public void TheThreePatternsStartIdenticalAndAreSeparateObjects()
        {
            OutputNaming naming = new OutputNaming();

            Assert.That(naming.Nwf.Level, Is.EqualTo(naming.Nwd.Level));
            Assert.That(naming.Nwf.Level, Is.EqualTo(naming.Workbook.Level));

            naming.Workbook.TypeCode = "RPT";

            Assert.That(naming.Nwf.TypeCode, Is.EqualTo("MOD"),
                "changing one pattern changed another");
            Assert.That(naming.Workbook.TypeCode, Is.EqualTo("RPT"));
        }

        [Test]
        public void EachFieldIsASettingAndReachesTheName()
        {
            NamePattern pattern = new NamePattern();
            pattern.Level = "L01";
            pattern.Discipline = "XX";
            pattern.TypeCode = "FED";
            pattern.Number = "000042";

            Assert.That(
                pattern.NameFor(Group(GroupingMode.PerBuilding).Find("1C07BC"), Settings),
                Is.EqualTo("1104-PAR-1C07BC-L01-XX-FED-000042"));
        }

        [Test]
        public void TheAllBuildingsCodeIsASettingToo()
        {
            NamePattern pattern = new NamePattern();
            pattern.AllBuildings = "SITE01";

            Assert.That(
                pattern.NameFor(Group(GroupingMode.Everything).Groups[0], Settings),
                Is.EqualTo("1104-PAR-SITE01-ZZZ-BM-MOD-000001"));
        }

        [Test]
        public void ThreePatternsCanGiveThreeDifferentNamesForOneGroup()
        {
            BuildingGroup group = Group(GroupingMode.PerBuilding).Find("1C07BC");

            OutputNaming naming = new OutputNaming();
            naming.Nwd.TypeCode = "PUB";
            naming.Workbook.TypeCode = "RPT";

            Assert.That(naming.Nwf.NameFor(group, Settings),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
            Assert.That(naming.Nwd.NameFor(group, Settings),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-PUB-000001"));
            Assert.That(naming.Workbook.NameFor(group, Settings),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-RPT-000001"));
        }

        [Test]
        public void TheSeparatorStillComesFromTheParserSettings()
        {
            ContainerNameSettings underscores = new ContainerNameSettings();
            underscores.Separator = '_';

            BuildingGroupingResult result = BuildingGrouping.GroupNames(
                new[] { "1104_PAR_1C07BC_ZZZ_AR_MOD_000001.nwc" }, underscores, GroupingMode.PerBuilding);

            Assert.That(new NamePattern().NameFor(result.Find("1C07BC"), underscores),
                Is.EqualTo("1104_PAR_1C07BC_ZZZ_BM_MOD_000001"));
        }

        // ---------- an empty field is refused rather than leaving a hole ----------

        [Test]
        public void AnEmptyFieldIsRefusedRatherThanWritingAHoleInTheName()
        {
            string[] fields = { "Level", "Discipline", "TypeCode", "Number", "AllBuildings" };

            foreach (string field in fields)
            {
                NamePattern pattern = new NamePattern();

                if (field == "Level") { pattern.Level = string.Empty; }
                if (field == "Discipline") { pattern.Discipline = string.Empty; }
                if (field == "TypeCode") { pattern.TypeCode = string.Empty; }
                if (field == "Number") { pattern.Number = string.Empty; }
                if (field == "AllBuildings") { pattern.AllBuildings = string.Empty; }

                Assert.That(pattern.WhyUnusable(), Is.Not.Null, field + " was accepted empty");
                Assert.Throws<InvalidOperationException>(
                    delegate { pattern.NameFor("1104", "PAR", string.Empty, string.Empty, Settings); },
                    field);
            }
        }

        [Test]
        public void AFieldOfNothingButSpacesIsAlsoRefused()
        {
            NamePattern pattern = new NamePattern();
            pattern.Number = "   ";

            Assert.That(pattern.WhyUnusable(), Does.Contain("number is empty"));
        }

        [Test]
        public void AUsablePatternSaysSoRatherThanGivingAReason()
        {
            Assert.That(new NamePattern().WhyUnusable(), Is.Null);
        }

        // ---------- the collision guard ----------

        /// <summary>
        /// Two groups that would be written to one name. The four grouping modes cannot
        /// currently produce this pair, because a group always carries either its own
        /// building code or its own discipline, so the guard is built and tested against
        /// groups made here rather than left untested until a fifth mode arrives.
        /// </summary>
        private static IList<BuildingGroup> TwoGroupsOnOneName()
        {
            List<ParsedContainerName> one = new List<ParsedContainerName>
            {
                ContainerName.Parse("1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc", Settings)
            };

            List<ParsedContainerName> two = new List<ParsedContainerName>
            {
                ContainerName.Parse("1104-PAR-1C07BC-ZZZ-ST-MOD-000001.nwc", Settings)
            };

            return new List<BuildingGroup>
            {
                new BuildingGroup("first", "1104", "PAR", one, new List<string> { "AR" },
                    "1C07BC", string.Empty),
                new BuildingGroup("second", "1104", "PAR", two, new List<string> { "ST" },
                    "1C07BC", string.Empty)
            };
        }

        // The one the brief asks for by name.
        [Test]
        public void APatternThatWouldNameTwoGroupsTheSameIsCaughtBeforeTheRun()
        {
            IList<BuildingGroup> colliding = TwoGroupsOnOneName();

            string why = OutputNameCheck.WhyTheRunCannotStart(colliding, new OutputNaming(), Settings);

            Assert.That(why, Is.Not.Null, "a collision went unnoticed");
            Assert.That(why, Does.Contain("would be written to the same"));
            Assert.That(why, Does.Contain("the run does not start"));
        }

        [Test]
        public void TheCollisionNamesTheGroupsAndTheNameTheyShare()
        {
            IList<NameCollision> collisions =
                OutputNameCheck.Collisions(TwoGroupsOnOneName(), new OutputNaming(), Settings);

            Assert.That(collisions.Count, Is.EqualTo(3), "one per pattern, NWF, NWD and workbook");

            NameCollision first = collisions[0];

            Assert.That(first.Kind, Is.EqualTo("NWF"));
            Assert.That(first.Name, Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
            Assert.That(first.Groups, Does.Contain("first"));
            Assert.That(first.Groups, Does.Contain("second"));
            Assert.That(first.Sentence(), Does.Contain("One would overwrite the other"));
        }

        // A collision in only one of the three still stops the run, because that one
        // output would still be destroyed.
        [Test]
        public void ACollisionInOnlyOnePatternStillStopsTheRun()
        {
            OutputNaming naming = new OutputNaming();

            // The workbook alone loses what told the two apart.
            naming.Nwf.Number = "000001";
            naming.Nwd.Number = "000002";

            IList<BuildingGroup> colliding = TwoGroupsOnOneName();

            Assert.That(
                OutputNameCheck.WhyTheRunCannotStart(colliding, naming, Settings), Is.Not.Null);
        }

        [Test]
        public void ChangingAPatternCanClearACollision()
        {
            IList<BuildingGroup> colliding = TwoGroupsOnOneName();

            Assert.That(
                OutputNameCheck.WhyTheRunCannotStart(colliding, new OutputNaming(), Settings),
                Is.Not.Null);

            // Grouping them by discipline instead would give each its own name, which is
            // what a person would do about it.
            List<BuildingGroup> apart = new List<BuildingGroup>();

            foreach (BuildingGroup group in colliding)
            {
                apart.Add(new BuildingGroup(
                    group.Building, group.Project, group.Originator,
                    new List<ParsedContainerName>(group.Files),
                    new List<string>(group.Disciplines),
                    group.BuildingCode,
                    group.Disciplines[0]));
            }

            Assert.That(
                OutputNameCheck.WhyTheRunCannotStart(apart, new OutputNaming(), Settings),
                Is.Null);
        }

        // ---------- no collision under any real mode ----------

        [Test]
        public void NoGroupingModeCollidesWithItself()
        {
            foreach (GroupingMode mode in GroupingModes.All())
            {
                Assert.That(
                    OutputNameCheck.WhyTheRunCannotStart(
                        Group(mode).Groups, new OutputNaming(), Settings),
                    Is.Null,
                    mode + " produced two groups on one name");
            }
        }

        [Test]
        public void AnUnusablePatternStopsTheRunAndSaysWhich()
        {
            OutputNaming naming = new OutputNaming();
            naming.Nwd.Number = string.Empty;

            string why = OutputNameCheck.WhyTheRunCannotStart(
                Group(GroupingMode.PerBuilding).Groups, naming, Settings);

            Assert.That(why, Is.Not.Null);
            Assert.That(why, Does.Contain("NWD"));
            Assert.That(why, Does.Contain("number is empty"));
        }

        [Test]
        public void NothingAtAllIsRefusedRatherThanChecked()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { OutputNameCheck.Collisions(null, new OutputNaming(), Settings); });
            Assert.Throws<ArgumentNullException>(
                delegate { OutputNameCheck.Collisions(new List<BuildingGroup>(), null, Settings); });
            Assert.Throws<ArgumentNullException>(
                delegate { new NamePattern().NameFor((BuildingGroup)null, Settings); });
        }
    }
}
