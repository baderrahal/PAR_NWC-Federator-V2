using System.Collections.Generic;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    [TestFixture]
    public class BuildingGroupingTests
    {
        private static BuildingGroupingResult GroupOf(params string[] names)
        {
            return BuildingGrouping.GroupNames(names, new ContainerNameSettings());
        }

        [Test]
        public void EveryDisciplineOfABuildingLandsInOneGroup()
        {
            BuildingGroupingResult result = GroupOf(
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001",
                "1104-PAR-1C07BC-ZZZ-ST-MOD-000001",
                "1104-PAR-1C07BC-ZZZ-ME-MOD-000001");

            Assert.That(result.GroupCount, Is.EqualTo(1));

            BuildingGroup group = result.Find("1C07BC");

            Assert.That(group, Is.Not.Null);
            Assert.That(group.FileCount, Is.EqualTo(3));
            Assert.That(group.Disciplines, Is.EqualTo(new[] { "AR", "ME", "ST" }));
        }

        [Test]
        public void TwoBuildingCodesStayTwoGroups()
        {
            BuildingGroupingResult result = GroupOf(
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001",
                "1104-PAR-1C07K1-ZZZ-AR-MOD-000001");

            Assert.That(result.GroupCount, Is.EqualTo(2));
            Assert.That(result.Find("1C07BC").FileCount, Is.EqualTo(1));
            Assert.That(result.Find("1C07K1").FileCount, Is.EqualTo(1));
        }

        [Test]
        public void DisciplineNeverSplitsAGroup()
        {
            BuildingGroupingResult result = GroupOf(
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001",
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000002",
                "1104-PAR-1C07BC-L01-EL-MOD-000001",
                "1104-PAR-1C07BC-L02-EL-MOD-000001");

            Assert.That(result.GroupCount, Is.EqualTo(1));

            BuildingGroup group = result.Find("1C07BC");

            Assert.That(group.FileCount, Is.EqualTo(4));
            Assert.That(group.Disciplines, Is.EqualTo(new[] { "AR", "EL" }));
        }

        [Test]
        public void AnUnreadableNameIsReportedAndNeverGrouped()
        {
            BuildingGroupingResult result = GroupOf(
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001",
                "not-a-container");

            Assert.That(result.GroupCount, Is.EqualTo(1));
            Assert.That(result.Unreadable.Count, Is.EqualTo(1));
            Assert.That(result.Unreadable[0].Stem, Is.EqualTo("not-a-container"));
            Assert.That(result.Unreadable[0].UnreadableReason, Is.Not.Null);
        }

        [Test]
        public void GroupsComeBackInBuildingOrder()
        {
            BuildingGroupingResult result = GroupOf(
                "1104-PAR-1C07K1-ZZZ-AR-MOD-000001",
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001",
                "1104-PAR-1A01AA-ZZZ-AR-MOD-000001");

            List<string> buildings = new List<string>();

            foreach (BuildingGroup group in result.Groups)
            {
                buildings.Add(group.Building);
            }

            Assert.That(buildings, Is.EqualTo(new[] { "1A01AA", "1C07BC", "1C07K1" }));
        }

        [Test]
        public void AnEmptyListGivesNoGroups()
        {
            BuildingGroupingResult result = GroupOf();

            Assert.That(result.GroupCount, Is.EqualTo(0));
            Assert.That(result.Skipped.Count, Is.EqualTo(0));
            Assert.That(result.Unreadable.Count, Is.EqualTo(0));
        }

        [Test]
        public void AGroupCarriesTheProjectCodeAndOriginatorItsFilesAgreedOn()
        {
            BuildingGroup group = GroupOf(
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001",
                "1104-PAR-1C07BC-L01-ST-MOD-000002").Find("1C07BC");

            Assert.That(group.Project, Is.EqualTo("1104"));
            Assert.That(group.Originator, Is.EqualTo("PAR"));
        }

        [Test]
        public void FilesThatDisagreeOnTheProjectCodeSkipTheWholeGroup()
        {
            BuildingGroupingResult result = GroupOf(
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001",
                "1105-PAR-1C07BC-ZZZ-ST-MOD-000001");

            Assert.That(result.GroupCount, Is.EqualTo(0));
            Assert.That(result.Skipped.Count, Is.EqualTo(1));

            SkippedBuildingGroup skipped = result.FindSkipped("1C07BC");

            Assert.That(skipped, Is.Not.Null);
            Assert.That(skipped.FileCount, Is.EqualTo(2));
            Assert.That(skipped.Reason, Does.Contain("project code"));
            Assert.That(skipped.Reason, Does.Contain("1104").And.Contains("1105"));
        }

        [Test]
        public void FilesThatDisagreeOnTheOriginatorSkipTheWholeGroup()
        {
            BuildingGroupingResult result = GroupOf(
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001",
                "1104-XYZ-1C07BC-ZZZ-ST-MOD-000001");

            Assert.That(result.GroupCount, Is.EqualTo(0));
            Assert.That(result.Skipped.Count, Is.EqualTo(1));
            Assert.That(result.FindSkipped("1C07BC").Reason, Does.Contain("originator"));
            Assert.That(result.FindSkipped("1C07BC").Reason, Does.Contain("PAR").And.Contains("XYZ"));
        }

        [Test]
        public void ASkippedGroupNamesTheFileThatBrokeRanks()
        {
            SkippedBuildingGroup skipped = GroupOf(
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001",
                "1104-PAR-1C07BC-ZZZ-ST-MOD-000002",
                "1105-PAR-1C07BC-ZZZ-ME-MOD-000003").FindSkipped("1C07BC");

            Assert.That(skipped.Reason, Does.Contain("1104-PAR-1C07BC-ZZZ-AR-MOD-000001"));
            Assert.That(skipped.Reason, Does.Contain("1105-PAR-1C07BC-ZZZ-ME-MOD-000003"));
        }

        [Test]
        public void OneBadGroupNeverStopsAGoodOne()
        {
            BuildingGroupingResult result = GroupOf(
                "1104-PAR-1C07BC-ZZZ-AR-MOD-000001",
                "1105-PAR-1C07BC-ZZZ-ST-MOD-000001",
                "1104-PAR-1C07K1-ZZZ-AR-MOD-000001",
                "1104-PAR-1C07K1-ZZZ-ST-MOD-000001");

            Assert.That(result.GroupCount, Is.EqualTo(1));
            Assert.That(result.Find("1C07K1"), Is.Not.Null);
            Assert.That(result.Find("1C07BC"), Is.Null);
            Assert.That(result.Skipped.Count, Is.EqualTo(1));
            Assert.That(result.Skipped[0].Building, Is.EqualTo("1C07BC"));
        }

        [Test]
        public void OneFileOnItsOwnCanNeverDisagreeWithItself()
        {
            BuildingGroupingResult result = GroupOf("1104-PAR-1C07BC-ZZZ-AR-MOD-000001");

            Assert.That(result.GroupCount, Is.EqualTo(1));
            Assert.That(result.Skipped.Count, Is.EqualTo(0));
        }
    }
}
