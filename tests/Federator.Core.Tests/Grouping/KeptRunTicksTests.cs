using System.Collections.Generic;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// FR-153. Unticking one file regrouped the list and every group came back with its Run box
    /// ticked, so a group the person had unticked in another building ran. Each test breaks one
    /// thing, the box, the key, a blocked row or a group new to the list, and asserts which
    /// groups start ticked after the regroup.
    /// </summary>
    [TestFixture]
    public class KeptRunTicksTests
    {
        private static readonly ContainerNameSettings Settings = new ContainerNameSettings();

        private static readonly string[] Folder =
        {
            "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1C07BC-ZZZ-ST-MOD-000001.nwc",
            "1104-PAR-1C07K1-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1B06BS-ZZZ-EL-MOD-000001.nwc"
        };

        private static IList<BuildingGroup> GroupsOf(params string[] files)
        {
            return BuildingGrouping.GroupNames(files, Settings, GroupingMode.PerBuilding).Groups;
        }

        /// <summary>
        /// The row's own case. 1C07K1 unticked, then the ST file of 1C07BC unticked, which regroups.
        /// 1C07K1 stays unticked, 1C07BC, a group whose files changed, keeps its tick, and so does 1B06BS.
        /// </summary>
        [Test]
        public void AGroupUntickedInOneBuildingStaysUntickedWhenAFileOfAnotherIsUnticked()
        {
            KeptRunTicks ticks = new KeptRunTicks();

            foreach (BuildingGroup group in GroupsOf(Folder))
            {
                ticks.Read(group.Building, false, group.Building != "1C07K1");
            }

            Dictionary<string, bool> after = new Dictionary<string, bool>();

            foreach (BuildingGroup group in GroupsOf(Folder[0], Folder[2], Folder[3]))
            {
                after.Add(group.Building, ticks.StartsTicked(group.Building));
            }

            Assert.That(after["1C07K1"], Is.False);
            Assert.That(after["1C07BC"], Is.True);
            Assert.That(after["1B06BS"], Is.True);
        }

        [Test]
        public void AGroupTickedBeforeStartsTicked()
        {
            KeptRunTicks ticks = new KeptRunTicks();
            ticks.Read("1C07BC", false, true);

            Assert.That(ticks.StartsTicked("1C07BC"), Is.True);
        }

        /// <summary>A group the list did not hold before starts ticked, as every group always has.</summary>
        [Test]
        public void AGroupNewToTheListStartsTicked()
        {
            KeptRunTicks ticks = new KeptRunTicks();
            ticks.Read("1C07BC", false, false);

            Assert.That(ticks.StartsTicked("1B06BS"), Is.True);
        }

        /// <summary>A blocked group's empty box was never the person's choice, so once its files agree it starts ticked.</summary>
        [Test]
        public void ABlockedGroupsEmptyBoxIsNotKept()
        {
            KeptRunTicks ticks = new KeptRunTicks();
            ticks.Read("1C07BC", true, false);

            Assert.That(ticks.StartsTicked("1C07BC"), Is.True);
        }

        /// <summary>The group is matched by its key exactly, as the name table finds a row.</summary>
        [Test]
        public void TheGroupIsMatchedByItsKeyExactly()
        {
            KeptRunTicks ticks = new KeptRunTicks();
            ticks.Read("1C07BC", false, false);

            Assert.That(ticks.StartsTicked("1C07BC"), Is.False);
            Assert.That(ticks.StartsTicked("1c07bc"), Is.True);
            Assert.That(ticks.StartsTicked("1C07BC-AR"), Is.True);
        }
    }
}
