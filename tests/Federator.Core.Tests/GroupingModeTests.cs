using System;
using System.Collections.Generic;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Per building is the default, because that is what this project runs weekly, but it
    /// is a choice. The names in here are the real shape off the project and are sample
    /// data, not settings.
    /// </summary>
    [TestFixture]
    public class GroupingModeTests
    {
        private static readonly string[] Folder =
        {
            "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1C07BC-ZZZ-ST-MOD-000001.nwc",
            "1104-PAR-1C07BC-ZZZ-ME-MOD-000001.nwc",
            "1104-PAR-1C07K1-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1C07K1-ZZZ-ME-MOD-000001.nwc"
        };

        private static BuildingGroupingResult Group(GroupingMode mode)
        {
            return BuildingGrouping.GroupNames(Folder, new ContainerNameSettings(), mode);
        }

        private static List<string> Keys(BuildingGroupingResult result)
        {
            List<string> keys = new List<string>();

            foreach (BuildingGroup group in result.Groups)
            {
                keys.Add(group.Building);
            }

            return keys;
        }

        // ---------- the choice changes the groups ----------

        [Test]
        public void PerBuildingIsTheDefault()
        {
            Assert.That(GroupingModes.Default, Is.EqualTo(GroupingMode.PerBuilding));

            BuildingGroupingResult byDefault = BuildingGrouping.GroupNames(
                Folder, new ContainerNameSettings());

            Assert.That(Keys(byDefault), Is.EqualTo(Keys(Group(GroupingMode.PerBuilding))));
        }

        [Test]
        public void PerBuildingPutsEveryDisciplineOfABuildingTogether()
        {
            BuildingGroupingResult result = Group(GroupingMode.PerBuilding);

            Assert.That(Keys(result), Is.EqualTo(new[] { "1C07BC", "1C07K1" }));
            Assert.That(result.Find("1C07BC").FileCount, Is.EqualTo(3));
            Assert.That(result.Find("1C07K1").FileCount, Is.EqualTo(2));
        }

        [Test]
        public void PerBuildingAndDisciplineSplitsEachBuildingByDiscipline()
        {
            BuildingGroupingResult result = Group(GroupingMode.PerBuildingAndDiscipline);

            Assert.That(Keys(result), Is.EqualTo(new[]
            {
                "1C07BC-AR", "1C07BC-ME", "1C07BC-ST", "1C07K1-AR", "1C07K1-ME"
            }));

            foreach (BuildingGroup group in result.Groups)
            {
                Assert.That(group.FileCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void PerDisciplineGathersOneDisciplineAcrossEveryBuilding()
        {
            BuildingGroupingResult result = Group(GroupingMode.PerDiscipline);

            Assert.That(Keys(result), Is.EqualTo(new[] { "AR", "ME", "ST" }));
            Assert.That(result.Find("AR").FileCount, Is.EqualTo(2));
            Assert.That(result.Find("ST").FileCount, Is.EqualTo(1));
        }

        [Test]
        public void EverythingIsOneGroup()
        {
            BuildingGroupingResult result = Group(GroupingMode.Everything);

            Assert.That(result.Groups.Count, Is.EqualTo(1));
            Assert.That(result.Groups[0].Building, Is.EqualTo(GroupingModes.EverythingKey));
            Assert.That(result.Groups[0].FileCount, Is.EqualTo(5));
        }

        // ---------- and it changes the output names ----------

        private static string NameOf(BuildingGroup group)
        {
            return new NamePattern().NameFor(group, new ContainerNameSettings());
        }

        [Test]
        public void PerBuildingNamesTheBuildingAndTheFederatedDisciplineCode()
        {
            Assert.That(NameOf(Group(GroupingMode.PerBuilding).Find("1C07BC")),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
        }

        [Test]
        public void PerBuildingAndDisciplineCarriesTheRealDisciplineInTheName()
        {
            BuildingGroupingResult result = Group(GroupingMode.PerBuildingAndDiscipline);

            Assert.That(NameOf(result.Find("1C07BC-AR")),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-AR-MOD-000001"));
            Assert.That(NameOf(result.Find("1C07BC-ST")),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-ST-MOD-000001"));
        }

        // A group spanning several buildings cannot carry any one of their codes, so it
        // carries the all buildings code, which is a setting on the pattern.
        [Test]
        public void PerDisciplineCarriesTheAllBuildingsCodeAndTheDiscipline()
        {
            Assert.That(NameOf(Group(GroupingMode.PerDiscipline).Find("AR")),
                Is.EqualTo("1104-PAR-ZZZZZZ-ZZZ-AR-MOD-000001"));
        }

        [Test]
        public void EverythingCarriesTheAllBuildingsCodeAndTheFederatedDisciplineCode()
        {
            Assert.That(NameOf(Group(GroupingMode.Everything).Groups[0]),
                Is.EqualTo("1104-PAR-ZZZZZZ-ZZZ-BM-MOD-000001"));
        }

        [Test]
        public void EveryModeGivesADifferentNameForTheSameFolder()
        {
            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);

            foreach (GroupingMode mode in GroupingModes.All())
            {
                foreach (BuildingGroup group in Group(mode).Groups)
                {
                    names.Add(NameOf(group));
                }
            }

            // 2 per building, 5 per building and discipline, 3 per discipline, 1 for all,
            // with 1C07BC-AR and the per discipline AR names differing on the building.
            Assert.That(names.Count, Is.EqualTo(11));
        }

        // ---------- what the group knows about itself ----------

        [Test]
        public void AGroupSpanningBuildingsSaysSoRatherThanNamingOne()
        {
            Assert.That(Group(GroupingMode.PerBuilding).Find("1C07BC").BuildingCode,
                Is.EqualTo("1C07BC"));
            Assert.That(Group(GroupingMode.PerDiscipline).Find("AR").BuildingCode,
                Is.EqualTo(string.Empty), "a group over two buildings cannot name one of them");
        }

        [Test]
        public void AGroupSpanningDisciplinesSaysSoRatherThanNamingOne()
        {
            Assert.That(Group(GroupingMode.PerBuildingAndDiscipline).Find("1C07BC-AR").DisciplineCode,
                Is.EqualTo("AR"));
            Assert.That(Group(GroupingMode.PerBuilding).Find("1C07BC").DisciplineCode,
                Is.EqualTo(string.Empty));
        }

        // ---------- a single discipline group, whatever the mode ----------

        [Test]
        public void AGroupHoldingOneDisciplineKnowsItCannotClash()
        {
            Assert.That(Group(GroupingMode.PerBuildingAndDiscipline).Find("1C07BC-ST").IsSingleDiscipline,
                Is.True);
            Assert.That(Group(GroupingMode.PerBuilding).Find("1C07BC").IsSingleDiscipline, Is.False);
        }

        // ---------- the files still have to agree, whatever the mode ----------

        [Test]
        public void FilesThatDisagreeOnTheProjectCodeStillBlockTheirGroup()
        {
            string[] mixed =
            {
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc",
                "1105-PAR-1C07BC-ZZZ-ST-MOD-000001.nwc"
            };

            BuildingGroupingResult result = BuildingGrouping.GroupNames(
                mixed, new ContainerNameSettings(), GroupingMode.PerBuilding);

            Assert.That(result.Groups.Count, Is.EqualTo(0));
            Assert.That(result.Skipped.Count, Is.EqualTo(1));
            Assert.That(result.Skipped[0].Reason, Does.Contain("project code"));
        }

        // Grouping everything puts files from different buildings in one group, so a
        // project code disagreement blocks the one group there is.
        [Test]
        public void GroupingEverythingStillRefusesFilesThatDisagree()
        {
            string[] mixed =
            {
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc",
                "1105-PAR-1C07K1-ZZZ-ST-MOD-000001.nwc"
            };

            BuildingGroupingResult result = BuildingGrouping.GroupNames(
                mixed, new ContainerNameSettings(), GroupingMode.Everything);

            Assert.That(result.Groups.Count, Is.EqualTo(0));
            Assert.That(result.Skipped.Count, Is.EqualTo(1));
        }

        [Test]
        public void AnUnreadableNameIsNeverFoldedIntoAGroupWhateverTheMode()
        {
            foreach (GroupingMode mode in GroupingModes.All())
            {
                BuildingGroupingResult result = BuildingGrouping.GroupNames(
                    new[] { "rubbish.nwc", "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc" },
                    new ContainerNameSettings(),
                    mode);

                Assert.That(result.Unreadable.Count, Is.EqualTo(1), mode.ToString());
            }
        }

        [Test]
        public void EveryModeHasWordsForItAndTheyAreAllDifferent()
        {
            HashSet<string> words = new HashSet<string>(StringComparer.Ordinal);

            foreach (GroupingMode mode in GroupingModes.All())
            {
                string described = GroupingModes.Describe(mode);

                Assert.That(described, Is.Not.EqualTo("UNKNOWN"), mode.ToString());
                Assert.That(words.Add(described), Is.True, "two modes read the same");
            }

            Assert.That(GroupingModes.All().Length, Is.EqualTo(4));
        }
    }
}
