using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Teams;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F114, Bader's Q114 points 9 to 15. One view per clash test of its open clashes, New and
    /// Active, in a folder by priority and then by team pair, the pair written in the team map's
    /// one order. In a pair holding Mechanical or Electrical the clashes over 150 mm go in the
    /// test's view under Over 150mm and the rest in its view in the pair folder, no clash in two
    /// views. A test with no open clash gets no view. The set names here are sample data only.
    /// </summary>
    [TestFixture]
    public class TestViewPlanTests
    {
        private const string Over150 = "Over 150mm";

        private static readonly string[] GroupCodes = { "AR", "EL", "ME", "ST" };

        private static ViewTeams Teams()
        {
            return new ViewTeams(TeamMapTests.MapOf(TeamMapTests.BadersMap), GroupCodes, new ViewpointSettings());
        }

        private static ViewClash Clash(
            string test, string name, string left, string right,
            ClashStatus status = ClashStatus.New,
            ClashPriority priority = ClashPriority.A,
            SizeVerdict? size = null)
        {
            return new ViewClash(test, name, left, right, status, priority, size, null, null, null, null, null);
        }

        private static TestViewPlanOutcome Plan(params ViewClash[] clashes)
        {
            return TestViewPlan.For(clashes, Teams(), null, new ViewpointSettings());
        }

        private static List<string> NamesOf(PlannedTestView view)
        {
            List<string> names = new List<string>();

            foreach (ViewClash clash in view.Clashes)
            {
                names.Add(clash.ClashName);
            }

            return names;
        }

        // ---------- point 13, what a view shows ----------

        [Test]
        public void ATestsNewAndActiveClashesAreOneViewAndTheRestAreLeftOutByStatus()
        {
            const string T = "BLD-EL-Lighting Fixtures-vs-BLD-ST-Floors";

            TestViewPlanOutcome plan = Plan(
                Clash(T, "Clash1", "BLD-EL-Lighting Fixtures", "BLD-ST-Floors", ClashStatus.New),
                Clash(T, "Clash2", "BLD-EL-Lighting Fixtures", "BLD-ST-Floors", ClashStatus.Active),
                Clash(T, "Clash3", "BLD-EL-Lighting Fixtures", "BLD-ST-Floors", ClashStatus.Reviewed),
                Clash(T, "Clash4", "BLD-EL-Lighting Fixtures", "BLD-ST-Floors", ClashStatus.Approved),
                Clash(T, "Clash5", "BLD-EL-Lighting Fixtures", "BLD-ST-Floors", ClashStatus.Resolved));

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(NamesOf(plan.Views[0]), Is.EqualTo(new[] { "Clash1", "Clash2" }));
            Assert.That(plan.Views[0].Name, Is.EqualTo(T));
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { "A", "Structure vs Electrical" }));
            Assert.That(plan.LeftOutAt(ClashStatus.Reviewed), Is.EqualTo(1));
            Assert.That(plan.LeftOutAt(ClashStatus.Approved), Is.EqualTo(1));
            Assert.That(plan.LeftOutAt(ClashStatus.Resolved), Is.EqualTo(1));
            Assert.That(plan.LeftOutAt(ClashStatus.New), Is.EqualTo(0));
            Assert.That(plan.Considered, Is.EqualTo(5));
            Assert.That(plan.InViews, Is.EqualTo(2));
            Assert.That(plan.InViews + plan.LeftOut, Is.EqualTo(plan.Considered));
        }

        /// <summary>The view's camera is its first open clash in read order, one camera per view.</summary>
        [Test]
        public void TheCameraClashIsTheFirstOpenClashInReadOrder()
        {
            const string T = "BLD-AR-Walls-vs-BLD-ST-Columns";

            TestViewPlanOutcome plan = Plan(
                Clash(T, "Clash7", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.Reviewed),
                Clash(T, "Clash3", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.Active),
                Clash(T, "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New));

            Assert.That(plan.Views[0].CameraClash.ClashName, Is.EqualTo("Clash3"));
        }

        // ---------- point 14, the size split ----------

        [Test]
        public void InAPairWithMechanicalTheLargeServicesGoUnderOver150mmAndTheRestInThePairView()
        {
            const string T = "BLD-ME-Ducts-vs-BLD-ST-Columns";

            TestViewPlanOutcome plan = Plan(
                Clash(T, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.Large),
                Clash(T, "Clash2", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.Small),
                Clash(T, "Clash3", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.SizeUnknown),
                Clash(T, "Clash4", "BLD-ME-Ducts", "BLD-ST-Columns", size: null),
                Clash(T, "Clash5", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.Large));

            Assert.That(plan.Views.Count, Is.EqualTo(2));

            PlannedTestView pairView = plan.Views[0];
            PlannedTestView sizeView = plan.Views[1];

            Assert.That(pairView.Folders, Is.EqualTo(new[] { "A", "Structure vs Mechanical" }));
            Assert.That(NamesOf(pairView), Is.EqualTo(new[] { "Clash2", "Clash3", "Clash4" }));
            Assert.That(sizeView.Folders, Is.EqualTo(new[] { "A", "Structure vs Mechanical", Over150 }));
            Assert.That(NamesOf(sizeView), Is.EqualTo(new[] { "Clash1", "Clash5" }));
            Assert.That(sizeView.Name, Is.EqualTo(T), "both views are named by their test");
            Assert.That(plan.SizeUnknown, Is.EqualTo(new[] { T + " / Clash3" }), "FR-068, the size not read is named");
        }

        /// <summary>A rectangular service by its larger side: a 600 by 150 duct is 600 and over.</summary>
        [Test]
        public void ADuct600By150IsLargeAndGoesUnderOver150mm()
        {
            SizeSettings sizes = new SizeSettings();
            Dictionary<string, double> read = new Dictionary<string, double> { { "Width", 600.0 }, { "Height", 150.0 } };
            SizeVerdict verdict = SizeRule.VerdictFor(SizeRule.LargestMillimetres(read, "Millimeters", sizes), sizes);

            TestViewPlanOutcome plan = Plan(Clash("T", "Clash1", "BLD-ME-Ducts", "BLD-AR-Walls", size: verdict));

            Assert.That(plan.Views[0].SizeFolder, Is.EqualTo(Over150));
        }

        /// <summary>Over means over: a service of exactly 150 mm goes in the pair view.</summary>
        [Test]
        public void ExactlyTheThresholdGoesInThePairView()
        {
            SizeSettings sizes = new SizeSettings();
            Dictionary<string, double> read = new Dictionary<string, double> { { "Diameter", 150.0 } };
            SizeVerdict verdict = SizeRule.VerdictFor(SizeRule.LargestMillimetres(read, "Millimeters", sizes), sizes);

            TestViewPlanOutcome plan = Plan(Clash("T", "Clash1", "BLD-EL-Cable Trays", "BLD-ST-Beams", size: verdict));

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(plan.Views[0].SizeFolder, Is.Null);
        }

        /// <summary>Point 11: Architecture vs Structure carries no size folder, so a large service there stays in its one view.</summary>
        [Test]
        public void APairWithNoSizeFolderTeamGivesOneViewWhateverTheSize()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("T", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", size: SizeVerdict.Large),
                Clash("T", "Clash2", "BLD-AR-Walls", "BLD-ST-Columns", size: SizeVerdict.Small));

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { "A", "Architecture vs Structure" }));
            Assert.That(plan.Views[0].Clashes.Count, Is.EqualTo(2));
            Assert.That(plan.SizeUnknown, Is.Empty);
        }

        /// <summary>The folder's words come off the threshold in SizeSettings, the one place it is set.</summary>
        [Test]
        public void ADifferentThresholdRenamesTheSizeFolder()
        {
            ViewpointSettings settings = new ViewpointSettings();
            settings.Sizes.ThresholdMillimetres = 250.0;

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[] { Clash("T", "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.Large) },
                Teams(),
                null,
                settings);

            Assert.That(plan.Views[0].SizeFolder, Is.EqualTo("Over 250mm"));
        }

        /// <summary>Over 150mm sits only directly under its own pair folder, never under a priority folder.</summary>
        [Test]
        public void TheSizeFolderSitsOnlyUnderItsOwnPair()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("T1", "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.Large),
                Clash("T2", "Clash1", "BLD-EL-Cable Trays", "BLD-AR-Walls", ClashStatus.New, ClashPriority.B, SizeVerdict.Large),
                Clash("T3", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C, SizeVerdict.Large));

            foreach (PlannedTestView view in plan.Views)
            {
                IList<string> folders = view.Folders;

                for (int i = 0; i < folders.Count; i++)
                {
                    if (folders[i] == Over150)
                    {
                        Assert.That(i, Is.EqualTo(2), view.ToString());
                        Assert.That(view.Pair.CarriesSizeFolder, Is.True, view.ToString());
                        Assert.That(folders[1], Is.EqualTo(plan.PairOfTest(view.Name).Folder), view.ToString());
                    }
                }
            }

            Assert.That(plan.Views.Count, Is.EqualTo(3));
            Assert.That(plan.Views[2].Folders, Is.EqualTo(new[] { "C", "Architecture vs Structure" }));
        }

        // ---------- point 15 ----------

        [Test]
        public void ATestWithNoOpenClashGetsNoView()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("Shut", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.Resolved),
                Clash("Shut", "Clash2", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.Approved),
                Clash("Open", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New));

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(plan.Views[0].Name, Is.EqualTo("Open"));
            Assert.That(plan.TestsWithNoOpenClash, Is.EqualTo(1));
            Assert.That(plan.InViews + plan.LeftOut, Is.EqualTo(plan.Considered));
        }

        [Test]
        public void NoClashesGiveNoViews()
        {
            TestViewPlanOutcome plan = Plan();

            Assert.That(plan.Views, Is.Empty);
            Assert.That(plan.Considered, Is.EqualTo(0));
            Assert.That(plan.InViews + plan.LeftOut, Is.EqualTo(plan.Considered));
        }

        // ---------- points 9, 10 and 12, the tree ----------

        /// <summary>Point 9: the priority layer is always there, and a test the file does not name goes under No priority.</summary>
        [Test]
        public void ATestWithNoPriorityGoesUnderNoPriority()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("T", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.None));

            Assert.That(plan.Views[0].PriorityFolder, Is.EqualTo("No priority"));
            Assert.That(plan.Views[0].FolderPath, Is.EqualTo("No priority/Architecture vs Structure"));
        }

        /// <summary>Point 10: two codes of one team, HV against PL, go in Mechanical vs Mechanical, and the group's own HV code is read.</summary>
        [Test]
        public void TwoCodesOfOneTeamGoInThatTeamAgainstItself()
        {
            ViewTeams teams = new ViewTeams(
                TeamMapTests.MapOf(TeamMapTests.BadersMap), new[] { "HV", "PL" }, new ViewpointSettings());

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[] { Clash("T", "Clash1", "BLD-HV-Ducts", "BLD-PL-Pipes") }, teams, null, new ViewpointSettings());

            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { "A", "Mechanical vs Mechanical" }));
        }

        /// <summary>Point 12: Mechanical vs Structure is written Structure vs Mechanical, whichever side the test puts first.</summary>
        [Test]
        public void APairIsWrittenOneWayRoundWhicheverSideTheTestPutsFirst()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("ME first", "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns"),
                Clash("ST first", "Clash1", "BLD-ST-Columns", "BLD-ME-Ducts"));

            Assert.That(plan.Views[0].Pair.Folder, Is.EqualTo("Structure vs Mechanical"));
            Assert.That(plan.Views[1].Pair.Folder, Is.EqualTo("Structure vs Mechanical"));
        }

        /// <summary>
        /// The order a person reads the tree in: priority, then the pair in the map's order,
        /// then the pair's views before its size folder, then the test name, Ordinal.
        /// </summary>
        [Test]
        public void TheViewsComeInTheOrderOfTheTree()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("Zed", "Clash1", "BLD-EL-Lighting", "BLD-ST-Floors", ClashStatus.New, ClashPriority.B),
                Clash("Duct", "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.A, SizeVerdict.Large),
                Clash("Duct", "Clash2", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.A, SizeVerdict.Small),
                Clash("Beam", "Clash1", "BLD-ME-Ducts", "BLD-ST-Beams", ClashStatus.New, ClashPriority.A, SizeVerdict.Small),
                Clash("Wall", "Clash1", "BLD-AR-Walls", "BLD-ME-Ducts", ClashStatus.New, ClashPriority.A),
                Clash("Alpha", "Clash1", "BLD-EL-Lighting", "BLD-ST-Floors", ClashStatus.New, ClashPriority.B),
                Clash("Loose", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.None));

            List<string> order = new List<string>();

            foreach (PlannedTestView view in plan.Views)
            {
                order.Add(view.ToString());
            }

            Assert.That(order, Is.EqualTo(new[]
            {
                "A/Architecture vs Mechanical/Wall",
                "A/Structure vs Mechanical/Beam",
                "A/Structure vs Mechanical/Duct",
                "A/Structure vs Mechanical/Over 150mm/Duct",
                "B/Structure vs Electrical/Alpha",
                "B/Structure vs Electrical/Zed",
                "No priority/Architecture vs Structure/Loose"
            }));
        }

        /// <summary>A test name ending in a space keeps it, as two set names of the client's matrix do.</summary>
        [Test]
        public void ATestNameEndingInASpaceKeepsIt()
        {
            TestViewPlanOutcome plan = Plan(Clash("BLD-AR-Walls-vs-BLD-ST-Columns ", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns "));

            Assert.That(plan.Views[0].Name, Is.EqualTo("BLD-AR-Walls-vs-BLD-ST-Columns "));
        }

        /// <summary>FR-074 and Q117 by its default A: a set name with no code reads as UNKNOWN, named once, never guessed.</summary>
        [Test]
        public void ASetNameWithNoCodeIsAnUnknownTeamAndIsNamedOnce()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("T", "Clash1", "BLD-EL-Lighting Fixtures", "BLD-Security Devices"),
                Clash("T", "Clash2", "BLD-EL-Lighting Fixtures", "BLD-Security Devices"),
                Clash("U", "Clash1", "BLD-Security Devices", "BLD-ST-Floors"));

            Assert.That(plan.Views[0].Pair.Folder, Is.EqualTo("Structure vs UNKNOWN"));
            Assert.That(plan.Views[1].Pair.Folder, Is.EqualTo("Electrical vs UNKNOWN"));
            Assert.That(plan.UnknownSets, Is.EqualTo(new[] { "BLD-Security Devices" }));
        }

        /// <summary>
        /// The reviewer's finding 6: the set half of the plan's lines names the set with no code
        /// under its count, as the size half does.
        /// </summary>
        [Test]
        public void ASetNameWithNoCodeIsNamedInTheLines()
        {
            IList<string> lines = Plan(
                Clash("T", "Clash1", "BLD-EL-Lighting Fixtures", "BLD-Security Devices"),
                Clash("U", "Clash1", "BLD-Security Devices", "BLD-ST-Floors")).Lines(new SizeSettings());

            int at = lines.IndexOf("a set name with no code this group knows : 1, its side read as a team of UNKNOWN and none guessed at");

            Assert.That(at, Is.GreaterThanOrEqualTo(0), string.Join("\n", new List<string>(lines).ToArray()));
            Assert.That(lines[at + 1], Is.EqualTo("    BLD-Security Devices"));
        }

        // ---------- the mirrors, F132, Bader's point that there are no mirrored tests ----------

        /// <summary>
        /// F114 attempt 2, the breaker's finding 1. The plan takes the mirror rule's tests, so a
        /// mirrored test gets no view even where its old results are in the document, and it is
        /// named with its clashes counted apart.
        /// </summary>
        [Test]
        public void AMirroredTestGetsNoViewAndIsNamed()
        {
            const string Kept = "BLD-ME-Ducts-vs-BLD-ST-Columns";
            const string Mirror = "BLD-ST-Columns-vs-BLD-ME-Ducts";

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[]
                {
                    Clash(Kept, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns"),
                    Clash(Mirror, "Clash1", "BLD-ST-Columns", "BLD-ME-Ducts"),
                    Clash(Mirror, "Clash2", "BLD-ST-Columns", "BLD-ME-Ducts", ClashStatus.Resolved)
                },
                Teams(),
                new[] { Mirror },
                new ViewpointSettings());

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(plan.Views[0].Name, Is.EqualTo(Kept));
            Assert.That(plan.PairOfTest(Mirror), Is.Null, "a mirror is no test of this plan");
            Assert.That(plan.Mirrored, Is.EqualTo(new[] { Mirror }));
            Assert.That(plan.LeftOutAsMirrors, Is.EqualTo(2));
            Assert.That(plan.LeftOutAt(ClashStatus.Resolved), Is.EqualTo(0), "a mirror's clashes are counted once, as a mirror's");
            Assert.That(plan.InViews + plan.LeftOut + plan.LeftOutAsMirrors, Is.EqualTo(plan.Considered));

            IList<string> lines = plan.Lines(new SizeSettings());

            Assert.That(lines, Has.Member("mirrored tests, so no view : 1, their 2 clashes left out"));
            Assert.That(lines, Has.Member("    " + Mirror));
            Assert.That(lines, Has.Member("clashes looked at : 3, open in views 1, left out by status 0, of mirrored tests 2"));
        }

        /// <summary>With no mirror rule handed in the plan cannot know a mirror, and its lines say so.</summary>
        [Test]
        public void APlanHandedNoMirrorRuleSaysSo()
        {
            IList<string> lines = Plan(Clash("T", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns")).Lines(new SizeSettings());

            Assert.That(lines, Has.Member("mirrored tests : UNKNOWN, no mirror rule was handed to the plan, so a mirrored test gets a view as any test does"));
        }

        /// <summary>
        /// The breaker's finding 3: a test with no clash at all is never handed to the plan, so the
        /// plan's count is of the tests whose every clash is at a status no view shows, and says so.
        /// </summary>
        [Test]
        public void ThePlanSaysWhichTestsWithNoOpenClashItCounts()
        {
            IList<string> lines = Plan(
                Clash("Shut", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.Resolved),
                Clash("Open", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New)).Lines(new SizeSettings());

            Assert.That(lines, Has.Member("tests whose every clash is at a status no view shows, so no view : 1, a test with no clash at all is not handed to the plan and not counted here"));
        }

        /// <summary>Q123 by its default A: with no team map every code is a team of its own and no pair carries the size folder.</summary>
        [Test]
        public void WithNoTeamMapThePairsAreCodesAndNoSizeFolderIsMade()
        {
            ViewTeams teams = new ViewTeams(TeamMap.NoXml(new TeamMapSettings()), GroupCodes, new ViewpointSettings());

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[] { Clash("T", "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.Large) },
                teams,
                null,
                new ViewpointSettings());

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { "A", "ME vs ST" }));
        }

        // ---------- the counts and the block ----------

        [Test]
        public void ASizeNotReadIsNamedOrCountedAsTheSizeSettingsSay()
        {
            ViewpointSettings settings = new ViewpointSettings();
            List<ViewClash> clashes = new List<ViewClash>();

            for (int i = 1; i <= 7; i++)
            {
                clashes.Add(Clash("T", "Clash" + i, "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.SizeUnknown));
            }

            IList<string> every = TestViewPlan.For(clashes, Teams(), null, settings).Lines(settings.Sizes);

            Assert.That(every, Has.Member("size could not be read, in a pair with a size folder : 7, every one in its pair view and none dropped"));
            Assert.That(every, Has.Member("    T / Clash7"));

            settings.Sizes.NameEveryUnknown = false;
            settings.Sizes.ExamplesWhenNotNamingEvery = 5;
            IList<string> some = TestViewPlan.For(clashes, Teams(), null, settings).Lines(settings.Sizes);

            Assert.That(some, Has.Member("    T / Clash5"));
            Assert.That(some, Has.No.Member("    T / Clash6"));
            Assert.That(some, Has.Member("    and 2 more, not named because the size settings name 5 and not every one"));
        }

        /// <summary>
        /// Every open clash is in exactly one view and no clash a view does not show is in any,
        /// over generated clashes of many tests, statuses and sizes, and the counts add up.
        /// </summary>
        [Test]
        public void EveryOpenClashIsInExactlyOneViewOverGeneratedClashes()
        {
            string[] sets = { "BLD-AR-Walls", "BLD-ST-Columns", "BLD-ME-Ducts", "BLD-EL-Cable Trays", "BLD-Security Devices" };
            ClashStatus[] statuses = (ClashStatus[])Enum.GetValues(typeof(ClashStatus));
            SizeVerdict?[] sizes = { null, SizeVerdict.Large, SizeVerdict.Small, SizeVerdict.SizeUnknown };
            ClashPriority[] priorities = Priorities.AllWithNone;
            Random random = new Random(114);

            for (int round = 0; round < 50; round++)
            {
                List<ViewClash> clashes = new List<ViewClash>();
                Dictionary<string, string[]> sides = new Dictionary<string, string[]>();
                int open = 0;

                for (int i = 0; i < 200; i++)
                {
                    string test = "T" + random.Next(12);

                    if (!sides.ContainsKey(test))
                    {
                        sides[test] = new[] { sets[random.Next(sets.Length)], sets[random.Next(sets.Length)] };
                    }

                    ClashStatus status = statuses[random.Next(statuses.Length)];

                    if (status == ClashStatus.New || status == ClashStatus.Active)
                    {
                        open++;
                    }

                    clashes.Add(new ViewClash(
                        test, "Clash" + i, sides[test][0], sides[test][1], status,
                        priorities[random.Next(priorities.Length)], sizes[random.Next(sizes.Length)],
                        null, null, null, null, null));
                }

                TestViewPlanOutcome plan = TestViewPlan.For(clashes, Teams(), null, new ViewpointSettings());
                Dictionary<string, int> seen = new Dictionary<string, int>();

                foreach (PlannedTestView view in plan.Views)
                {
                    Assert.That(view.Clashes, Is.Not.Empty, "an empty half is never planned");

                    foreach (ViewClash clash in view.Clashes)
                    {
                        Assert.That(clash.Status, Is.EqualTo(ClashStatus.New).Or.EqualTo(ClashStatus.Active));
                        seen[clash.Key] = seen.ContainsKey(clash.Key) ? seen[clash.Key] + 1 : 1;
                    }
                }

                Assert.That(seen.Count, Is.EqualTo(open), "round " + round);
                Assert.That(new List<int>(seen.Values).TrueForAll(n => n == 1), Is.True, "round " + round);
                Assert.That(plan.InViews + plan.LeftOut, Is.EqualTo(plan.Considered), "round " + round);
            }
        }
    }
}
