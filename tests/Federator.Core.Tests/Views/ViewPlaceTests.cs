using System;
using Federator.Core.Clash;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F114 attempt 2, the reviewer's findings 0 and 1. The place of a view, its folders and its
    /// name, is written in one place and every type that names or compares a place reads it there:
    /// the plan's view, the walk's node, the mark, the block, the inventory and the checks. The
    /// key a place is compared by tells a folder from a view of the same name, and a folder whose
    /// name holds a slash from two folders. Names are sample data.
    /// </summary>
    [TestFixture]
    public class ViewPlaceTests
    {
        private const string Name = "BLD-ME-Ducts-vs-BLD-ST-Columns";

        private static readonly string[] Folders = { "A", "Structure vs Mechanical", "Over 150mm" };
        private static readonly ViewpointSettings Settings = new ViewpointSettings();
        private static readonly Point3 Camera = new Point3(1, 2, 3);

        [Test]
        public void EveryReaderOfAPlaceReadsTheOneWrittenPlace()
        {
            string place = ViewPlace.Of(Folders, Name);

            Assert.That(place, Is.EqualTo("A/Structure vs Mechanical/Over 150mm/" + Name));
            Assert.That(ViewPlace.FolderPath(Folders), Is.EqualTo("A/Structure vs Mechanical/Over 150mm"));

            ViewTeams teams = new ViewTeams(TeamMapTests.MapOf(TeamMapTests.BadersMap), new[] { "ME", "ST" }, Settings);
            ViewClash large = new ViewClash(Name, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New,
                ClashPriority.A, SizeVerdict.Large, null, null, null, null, null);
            PlannedTestView planned = TestViewPlan.For(new[] { large }, teams, null, Settings).Views[0];

            Assert.That(planned.ToString(), Is.EqualTo(place), "the plan's view");
            Assert.That(planned.Key, Is.EqualTo(ViewPlace.Key(Folders, Name, false)), "the key its read backs are kept under");

            ViewNode node = new ViewNode(Folders, Name, false, 0, null, 0, Camera, null, false);

            Assert.That(node.ToString(), Is.EqualTo(place), "the walk's node");
            Assert.That(node.FolderPath, Is.EqualTo(ViewPlace.FolderPath(Folders)));

            string stamp = ToolViewMark.StampOf(new DateTime(2026, 10, 5, 10, 0, 0, DateTimeKind.Utc));
            string body = ToolViewMark.Body(stamp, Folders, Name, Camera, null, Settings);

            Assert.That(ToolViewMark.Read(body, Settings).FolderPath, Is.EqualTo(ViewPlace.FolderPath(Folders)), "the mark");
            Assert.That(ToolViewMark.Judge(node.Folders, node.Name, Camera, new[] { body }, 0, null, Settings).Owner,
                Is.EqualTo(ViewOwner.Ours));
        }

        [Test]
        public void AtTheRootThePlaceIsTheNameAlone()
        {
            Assert.That(ViewPlace.Of(new string[0], "Level 1"), Is.EqualTo("Level 1"));
            Assert.That(ViewPlace.FolderPath(new string[0]), Is.EqualTo(string.Empty));
            Assert.That(new ViewNode(null, "Level 1", false, 0, null, 0, Camera, null, false).ToString(), Is.EqualTo("Level 1"));
        }

        /// <summary>The written place is for a person to read. The key is what a place is compared by.</summary>
        [Test]
        public void TheKeyTellsApartWhatTheWrittenPlaceCannot()
        {
            Assert.That(ViewPlace.Of(new[] { "A/B" }, "C"), Is.EqualTo(ViewPlace.Of(new[] { "A", "B" }, "C")));
            Assert.That(ViewPlace.Key(new[] { "A/B" }, "C", false), Is.Not.EqualTo(ViewPlace.Key(new[] { "A", "B" }, "C", false)));
            Assert.That(ViewPlace.Key(new[] { "A" }, "X", true), Is.Not.EqualTo(ViewPlace.Key(new[] { "A" }, "X", false)),
                "a folder and a view of one name side by side");
            Assert.That(ViewPlace.Key(new[] { "A" }, "X", false), Is.EqualTo(ViewPlace.Key(new[] { "A" }, "X", false)));
        }

        /// <summary>
        /// F114 attempt 3, the breaker's finding 5 of attempt 2. A test named Over 150mm/Pipes in a
        /// pair folder and a test named Pipes in that pair's size folder share one written place.
        /// Their read backs are kept under two keys, so one cannot answer for the other.
        /// </summary>
        [Test]
        public void TwoViewsOfOneWrittenPlaceKeepTheirReadBacksUnderTwoKeys()
        {
            ViewTeams teams = new ViewTeams(TeamMapTests.MapOf(TeamMapTests.BadersMap), new[] { "ME", "ST" }, Settings);
            ViewClash large = new ViewClash("Pipes", "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New,
                ClashPriority.A, SizeVerdict.Large, null, null, null, null, null);
            ViewClash small = new ViewClash("Over 150mm/Pipes", "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New,
                ClashPriority.A, SizeVerdict.Small, null, null, null, null, null);
            TestViewPlanOutcome plan = TestViewPlan.For(new[] { large, small }, teams, null, Settings);

            Assert.That(plan.Views.Count, Is.EqualTo(2));
            Assert.That(plan.Views[0].ToString(), Is.EqualTo(plan.Views[1].ToString()), "one written place");
            Assert.That(plan.Views[0].Key, Is.Not.EqualTo(plan.Views[1].Key));
        }

        [Test]
        public void TheParentKeyIsTheKeyOfTheFolderItSitsIn()
        {
            Assert.That(ViewPlace.ParentKey(Folders), Is.EqualTo(ViewPlace.Key(new[] { "A", "Structure vs Mechanical" }, "Over 150mm", true)));
            Assert.That(ViewPlace.ParentKey(new string[0]), Is.EqualTo(string.Empty));
        }
    }
}
