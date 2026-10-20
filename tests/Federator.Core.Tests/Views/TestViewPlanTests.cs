using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Exchange;
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
        private const string UnknownSetsLine =
            "a side read as a team of UNKNOWN, by its set name : ";

        private const string UnknownSetsTail = ", each named with why and none guessed at";

        private const string NoCode = ", no code this group knows, and ";

        private static readonly string[] GroupCodes = { "AR", "EL", "ME", "ST" };

        private static ViewTeams Teams()
        {
            return new ViewTeams(TeamMapTests.MapOf(TeamMapTests.BadersMap), null, GroupCodes, new ViewpointSettings());
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
            return TestViewPlan.For(clashes, Teams(), null, null, new ViewpointSettings());
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
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { "Clash Status", "A", "Electrical" }));
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

        // ---------- F186, Bader's answer 1 of Q174, the one tool folder, A B C, the tests directly ----------

        private const string Top = "Clash Status";

        /// <summary>No team pair level and, in A, no size folder: every open clash of a test is in its one view, whatever its size.</summary>
        [Test]
        public void EveryOpenClashOfATestIsInItsOneViewWhateverItsSizeAndPair()
        {
            const string T = "BLD-ME-Ducts-vs-BLD-ST-Columns";

            TestViewPlanOutcome plan = Plan(
                Clash(T, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.Large),
                Clash(T, "Clash2", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.Small),
                Clash(T, "Clash3", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.SizeUnknown),
                Clash(T, "Clash4", "BLD-ME-Ducts", "BLD-ST-Columns", size: null),
                Clash(T, "Clash5", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.Large));

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { Top, "A", "Mechanical" }));
            Assert.That(NamesOf(plan.Views[0]), Is.EqualTo(new[] { "Clash1", "Clash2", "Clash3", "Clash4", "Clash5" }));
            Assert.That(plan.Views[0].Name, Is.EqualTo(T));
        }

        // ---------- F192, Bader's order of 15 Oct 2026, Over 150mm only inside C ----------

        private const string Ducts = "BLD-ME-Ducts-vs-BLD-ST-Columns";

        [Test]
        public void InCTheClashesOverTheThresholdAreTheTestsViewUnderOver150mmAndTheRestStayInC()
        {
            TestViewPlanOutcome plan = Plan(
                Clash(Ducts, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C, SizeVerdict.Large),
                Clash(Ducts, "Clash2", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C, SizeVerdict.Small),
                Clash(Ducts, "Clash3", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.Active, ClashPriority.C, SizeVerdict.SizeUnknown),
                Clash(Ducts, "Clash4", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C, null),
                Clash(Ducts, "Clash5", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C, SizeVerdict.Large));

            Assert.That(plan.Views.Count, Is.EqualTo(2));
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { Top, "C", "Mechanical" }));
            Assert.That(NamesOf(plan.Views[0]), Is.EqualTo(new[] { "Clash2", "Clash3", "Clash4" }));
            Assert.That(plan.Views[1].Folders, Is.EqualTo(new[] { Top, "C", "Mechanical", "Over 150mm" }));
            Assert.That(NamesOf(plan.Views[1]), Is.EqualTo(new[] { "Clash1", "Clash5" }));
            Assert.That(plan.Views[0].Name, Is.EqualTo(Ducts));
            Assert.That(plan.Views[1].Name, Is.EqualTo(Ducts));
            Assert.That(plan.InViews, Is.EqualTo(5), "no clash is in two views and none is lost");
            Assert.That(plan.OverSizeViews, Is.EqualTo(1));
            Assert.That(plan.SizeUnknown.Count, Is.EqualTo(1));
            Assert.That(plan.SizeUnknown[0], Does.Contain("Clash3"));
        }

        [Test]
        public void ATestWithNoPriorityIsCAndSoGetsTheOver150mmFolderToo()
        {
            TestViewPlanOutcome plan = Plan(
                Clash(Ducts, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.None, SizeVerdict.Large));

            Assert.That(plan.Views.Count, Is.EqualTo(1), "a test with only large clashes has only the Over 150mm view, no empty half");
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { Top, "C", "Mechanical", "Over 150mm" }));
        }

        [Test]
        public void InAAndBThereIsNoOver150mmFolderAndEveryOpenClashStaysInTheTestsOneView()
        {
            const string TestB = "BLD-EL-Cable Trays-vs-BLD-AR-Walls";

            TestViewPlanOutcome plan = Plan(
                Clash(Ducts, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.A, SizeVerdict.Large),
                Clash(Ducts, "Clash2", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.A, SizeVerdict.Small),
                Clash(TestB, "Clash1", "BLD-EL-Cable Trays", "BLD-AR-Walls", ClashStatus.New, ClashPriority.B, SizeVerdict.Large),
                Clash(TestB, "Clash2", "BLD-EL-Cable Trays", "BLD-AR-Walls", ClashStatus.New, ClashPriority.B, SizeVerdict.Large));

            Assert.That(plan.Views.Count, Is.EqualTo(2));
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { Top, "A", "Mechanical" }));
            Assert.That(NamesOf(plan.Views[0]), Is.EqualTo(new[] { "Clash1", "Clash2" }));
            Assert.That(plan.Views[1].Folders, Is.EqualTo(new[] { Top, "B", "Electrical" }));
            Assert.That(NamesOf(plan.Views[1]), Is.EqualTo(new[] { "Clash1", "Clash2" }));
            Assert.That(plan.OverSizeViews, Is.EqualTo(0));
            Assert.That(plan.OverSizeKeptTestsA, Is.EqualTo(1));
            Assert.That(plan.OverSizeKeptTestsB, Is.EqualTo(1));
            Assert.That(plan.OverSizeKeptClashes, Is.EqualTo(3));
            Assert.That(string.Join("\n", new List<string>(plan.Lines()).ToArray()),
                Does.Contain("no Over 150mm view made, all in the test's one view : 2 tests, 1 of A and 1 of B, 3 clashes, for Bader's decision"));
        }

        [Test]
        public void APairThatDoesNotCarryTheSizeFolderHasNoOver150mmViewInCEither()
        {
            const string T = "BLD-AR-Walls-vs-BLD-ST-Columns";

            TestViewPlanOutcome plan = Plan(
                Clash(T, "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C, SizeVerdict.Large),
                Clash(T, "Clash2", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C, SizeVerdict.Small));

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { Top, "C", "Architecture" }));
            Assert.That(plan.OverSizeViews, Is.EqualTo(0));
        }

        [Test]
        public void TheOver150mmViewsComeAfterTheViewsOfCItself()
        {
            TestViewPlanOutcome plan = Plan(
                Clash(Ducts, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C, SizeVerdict.Large),
                Clash("BLD-AR-Walls-vs-BLD-ST-Columns", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C));

            Assert.That(plan.Views[0].SizeFolder, Is.Null);
            Assert.That(plan.Views[1].SizeFolder, Is.EqualTo("Over 150mm"));
        }

        [Test]
        public void TheFolderIsTheToolFolderThenTheLetterOfThePriorityThenTheDiscipline()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("TA", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.A),
                Clash("TB", "Clash1", "BLD-EL-Cable Trays", "BLD-AR-Walls", ClashStatus.New, ClashPriority.B),
                Clash("TC", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C));

            Assert.That(plan.Views.Count, Is.EqualTo(3));
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { Top, "A", "Architecture" }));
            Assert.That(plan.Views[1].Folders, Is.EqualTo(new[] { Top, "B", "Electrical" }));
            Assert.That(plan.Views[2].Folders, Is.EqualTo(new[] { Top, "C", "Architecture" }));

            foreach (PlannedTestView view in plan.Views)
            {
                Assert.That(view.Folders.Count, Is.EqualTo(3), "no team pair level, the discipline under the letter: " + view);
            }
        }

        [Test]
        public void ATestWithNoPriorityGoesUnderCAsEverywhereElseInTheTool()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("T", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.None));

            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { Top, "C", "Architecture" }));
        }

        // ---------- F199, Bader's order of round 6, the discipline folders ----------

        [Test]
        public void InCTheOver150mmFolderIsInsideElectricalAsInsideMechanical()
        {
            const string Trays = "BLD-EL-Cable Trays-vs-BLD-AR-Walls";

            TestViewPlanOutcome plan = Plan(
                Clash(Trays, "Clash1", "BLD-EL-Cable Trays", "BLD-AR-Walls", ClashStatus.New, ClashPriority.C, SizeVerdict.Large),
                Clash(Trays, "Clash2", "BLD-EL-Cable Trays", "BLD-AR-Walls", ClashStatus.New, ClashPriority.C, SizeVerdict.Small));

            Assert.That(plan.Views.Count, Is.EqualTo(2));
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { Top, "C", "Electrical" }));
            Assert.That(plan.Views[1].Folders, Is.EqualTo(new[] { Top, "C", "Electrical", "Over 150mm" }));
        }

        /// <summary>
        /// Q185. A test of C whose first side is Structure, in a pair that carries the size folder, has clashes over the
        /// threshold and no Over 150mm folder to put them in. They stay in its one view in C/Structure, and are counted.
        /// </summary>
        [Test]
        public void ATestOfCInStructureKeepsItsClashesOverTheThresholdInItsOneView()
        {
            const string Columns = "BLD-ST-Columns-vs-BLD-ME-Ducts";

            TestViewPlanOutcome plan = Plan(
                Clash(Columns, "Clash1", "BLD-ST-Columns", "BLD-ME-Ducts", ClashStatus.New, ClashPriority.C, SizeVerdict.Large),
                Clash(Columns, "Clash2", "BLD-ST-Columns", "BLD-ME-Ducts", ClashStatus.New, ClashPriority.C, SizeVerdict.Large),
                Clash(Columns, "Clash3", "BLD-ST-Columns", "BLD-ME-Ducts", ClashStatus.New, ClashPriority.C, SizeVerdict.Small));

            Assert.That(plan.Views.Count, Is.EqualTo(1), "no clash dropped and no Over 150mm folder outside Mechanical and Electrical");
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { Top, "C", "Structure" }));
            Assert.That(NamesOf(plan.Views[0]), Is.EqualTo(new[] { "Clash1", "Clash2", "Clash3" }));
            Assert.That(plan.OverSizeViews, Is.EqualTo(0));
            Assert.That(plan.OverSizeKeptTestsC, Is.EqualTo(1));
            Assert.That(plan.OverSizeKeptClashesC, Is.EqualTo(2));
            Assert.That(plan.OverSizeKeptTestsA + plan.OverSizeKeptTestsB, Is.EqualTo(0), "Q182 counts A and B alone");
            Assert.That(string.Join("\n", new List<string>(plan.Lines()).ToArray()), Does.Contain(
                "tests of priority C in Architecture, Structure or Other with clashes over the threshold, no Over 150mm view made, "
                + "all in the test's one view, Q185 : 1 tests, 2 clashes\n    " + Columns + " : priority C, Structure, 2 clashes over the threshold"));
        }

        [Test]
        public void ASavedTestThePackDoesNotNameGoesInOtherAndKeepsItsLargeClashesWhole()
        {
            const string Saved = "Rami ducts check";

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[]
                {
                    Clash(Saved, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.None, SizeVerdict.Large),
                    Clash(Ducts, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C, SizeVerdict.Small)
                },
                Teams(), null, new[] { Saved }, new ViewpointSettings());

            Assert.That(plan.Views.Count, Is.EqualTo(2));
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { Top, "C", "Mechanical" }));
            Assert.That(plan.Views[0].Name, Is.EqualTo(Ducts));
            Assert.That(plan.Views[1].Folders, Is.EqualTo(new[] { Top, "C", "Other" }));
            Assert.That(plan.Views[1].Name, Is.EqualTo(Saved));
            Assert.That(plan.NotInThePack, Is.EqualTo(new[] { Saved }));
            Assert.That(plan.OverSizeKeptTestsC, Is.EqualTo(1));
            Assert.That(plan.Lines(), Does.Contain("saved tests the pack does not name, in Other : 1"));
        }

        [Test]
        public void WithNoPackReadEveryTestGoesByItsFirstSideAndTheLinesSaySo()
        {
            TestViewPlanOutcome plan = Plan(Clash("Rami ducts check", "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns"));

            Assert.That(plan.PackHandedIn, Is.False);
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { Top, "A", "Mechanical" }));
            Assert.That(plan.Lines(), Does.Contain(
                "saved tests the pack does not name : UNKNOWN, no matrix was read, so every test goes by the code of its first side"));
        }

        /// <summary>A discipline folder is planned only where a test is in it, and the line names only those.</summary>
        [Test]
        public void ADisciplineFolderIsPlannedOnlyWhereATestIsInIt()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("BLD-AR-Walls-vs-BLD-ST-Columns", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.A),
                Clash(Ducts, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C, SizeVerdict.Large));

            List<string> paths = new List<string>();

            foreach (PlannedTestView view in plan.Views)
            {
                paths.Add(ViewPlace.FolderPath(view.Folders));
            }

            Assert.That(paths, Is.EqualTo(new[] { Top + "/A/Architecture", Top + "/C/Mechanical/Over 150mm" }));
            Assert.That(plan.ViewsIn("A", "Structure"), Is.EqualTo(0));
            Assert.That(plan.Lines(), Does.Contain(
                "discipline folders, views planned in each : A Architecture 1, B none, C Mechanical 1"));
        }

        /// <summary>Under a letter the folders come Architecture, Structure, Mechanical, Electrical, Other, each folder's views together.</summary>
        [Test]
        public void TheDisciplineFoldersComeInTheirOrderUnderEachLetter()
        {
            TestViewPlanOutcome plan = TestViewPlan.For(
                new[]
                {
                    Clash("Saved", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.C),
                    Clash("Lights", "Clash1", "BLD-EL-Lighting", "BLD-ST-Floors", ClashStatus.New, ClashPriority.None),
                    Clash("Columns", "Clash1", "BLD-ST-Columns", "BLD-ME-Ducts", ClashStatus.New, ClashPriority.C),
                    Clash("Pipes", "Clash1", "BLD-PL-Pipes", "BLD-ST-Floors", ClashStatus.New, ClashPriority.C),
                    Clash("Walls", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.None)
                },
                Teams(), null, new[] { "Saved" }, new ViewpointSettings());

            List<string> order = new List<string>();

            foreach (PlannedTestView view in plan.Views)
            {
                order.Add(view.ToString());
            }

            Assert.That(order, Is.EqualTo(new[]
            {
                Top + "/C/Architecture/Walls",
                Top + "/C/Structure/Columns",
                Top + "/C/Mechanical/Pipes",
                Top + "/C/Electrical/Lights",
                Top + "/C/Other/Saved"
            }), "a test with no priority is read with those of C");
        }

        [Test]
        public void ThePairStillDecidesTheModelsAViewShowsAndNoLongerWhereItSits()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("T", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.A));

            Assert.That(plan.Views[0].Pair.Folder, Is.EqualTo("Architecture vs Structure"));
            Assert.That(ViewPlace.FolderPath(plan.Views[0].Folders), Is.EqualTo(Top + "/A/Architecture"));
        }

        [Test]
        public void TheToolFolderMarkerIsWordsNoPersonTypesAndIsFoundByItsStart()
        {
            string body = ToolViewFolder.MarkerBody("2026-10-14T18:26:41Z");

            Assert.That(body, Is.EqualTo("NWCF-AUTO run=2026-10-14T18:26:41Z"));
            Assert.That(ToolViewFolder.IsMarker(body), Is.True);
            Assert.That(ToolViewFolder.RunIdOf(body), Is.EqualTo("2026-10-14T18:26:41Z"));
            Assert.That(ToolViewFolder.IsMarker("my own note, NWCF-AUTO run=1"), Is.False);
            Assert.That(ToolViewFolder.IsMarker("NWCF-AUTO"), Is.False);
            Assert.That(ToolViewFolder.IsMarker(null), Is.False);
            Assert.That(ToolViewFolder.IsMarker("NWCF-AUTO run=1\nsecond line"), Is.False, "a comment that is more than the marker is a person's");
            Assert.That(ToolViewFolder.RunIdOf("hello"), Is.Null);
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

        /// <summary>Point 10: two codes of one team, HV against PL, go in Mechanical vs Mechanical, and the group's own HV code is read.</summary>
        [Test]
        public void TwoCodesOfOneTeamGoInThatTeamAgainstItself()
        {
            ViewTeams teams = new ViewTeams(
                TeamMapTests.MapOf(TeamMapTests.BadersMap), null, new[] { "HV", "PL" }, new ViewpointSettings());

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[] { Clash("T", "Clash1", "BLD-HV-Ducts", "BLD-PL-Pipes") }, teams, null, null, new ViewpointSettings());

            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { "Clash Status", "A", "Mechanical" }));
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
                "Clash Status/A/Architecture/Wall",
                "Clash Status/A/Mechanical/Beam",
                "Clash Status/A/Mechanical/Duct",
                "Clash Status/B/Electrical/Alpha",
                "Clash Status/B/Electrical/Zed",
                "Clash Status/C/Architecture/Loose"
            }));
        }

        /// <summary>A test name ending in a space keeps it, as two set names of the client's matrix do.</summary>
        [Test]
        public void ATestNameEndingInASpaceKeepsIt()
        {
            TestViewPlanOutcome plan = Plan(Clash("BLD-AR-Walls-vs-BLD-ST-Columns ", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns "));

            Assert.That(plan.Views[0].Name, Is.EqualTo("BLD-AR-Walls-vs-BLD-ST-Columns "));
        }

        /// <summary>
        /// FR-074 and Q117 answered A where the XML's set tree names no team: a set name with no
        /// code and no set tree handed in reads as UNKNOWN, named once, never guessed.
        /// </summary>
        [Test]
        public void ASetNameWithNoCodeIsAnUnknownTeamAndIsNamedOnce()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("T", "Clash1", "BLD-EL-Lighting Fixtures", "BLD-Security Devices"),
                Clash("T", "Clash2", "BLD-EL-Lighting Fixtures", "BLD-Security Devices"),
                Clash("U", "Clash1", "BLD-Security Devices", "BLD-ST-Floors"));

            // F114-K29: a side on UNKNOWN is outside the map, so neither test gets a view, and
            // each is named with its open clashes counted.
            Assert.That(plan.Views.Count, Is.EqualTo(0));
            Assert.That(plan.OutsideTheMap, Is.EqualTo(new[] { "T", "U" }));
            Assert.That(plan.OpenOutsideTheMap, Is.EqualTo(3));
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
                Clash("U", "Clash1", "BLD-Security Devices", "BLD-ST-Floors")).Lines();

            int at = lines.IndexOf(UnknownSetsLine + "1" + UnknownSetsTail);

            Assert.That(at, Is.GreaterThanOrEqualTo(0), string.Join("\n", new List<string>(lines).ToArray()));
            Assert.That(
                lines[at + 1],
                Is.EqualTo("    BLD-Security Devices" + NoCode
                    + "no set tree was read, so whether a folder above it names a team is UNKNOWN"));
        }

        /// <summary>
        /// F114 attempt 8, the reviewer's finding 2 of attempt 7. The line under a side read as
        /// UNKNOWN says why, and says only what was read: a side with no set name, a set name not in
        /// the set tree read, a set whose folders name no team, two sets of one name giving two
        /// teams, and a code the map puts in a team named UNKNOWN, each named as it is.
        /// </summary>
        [Test]
        public void EachSideReadAsUnknownIsNamedWithWhy()
        {
            TeamMap map = TeamMapTests.MapOf(TeamMapTests.BadersMap + "team: UNKNOWN | XX\n");
            ViewTeams teams = new ViewTeams(
                map,
                new[]
                {
                    Set("BLD-Devices", "Electrical"),
                    Set("BLD-Devices", "Mechanical"),
                    Set("BLD-Signs", "Security")
                },
                GroupCodes,
                new ViewpointSettings());

            IList<string> lines = TestViewPlan.For(
                new[]
                {
                    Clash("T", "Clash1", "BLD-Devices", "BLD-ST-Floors"),
                    Clash("U", "Clash1", "BLD-Signs", "BLD-ST-Floors"),
                    Clash("V", "Clash1", "BLD-Nowhere", "BLD-ST-Floors"),
                    Clash("W", "Clash1", null, "BLD-ST-Floors"),
                    Clash("X", "Clash1", "BLD-XX-Things", "BLD-ST-Floors")
                },
                teams,
                null, null,
                new ViewpointSettings()).Lines();

            int at = lines.IndexOf(UnknownSetsLine + "5" + UnknownSetsTail);

            Assert.That(at, Is.GreaterThanOrEqualTo(0), string.Join("\n", new List<string>(lines).ToArray()));
            Assert.That(
                new List<string>(lines).GetRange(at + 1, 5),
                Is.EqualTo(new[]
                {
                    "    BLD-Devices" + NoCode + "its 2 sets of that name in the clash XML's set tree give the teams Electrical, Mechanical",
                    "    BLD-Signs" + NoCode + "no folder above it in the clash XML's set tree names a team",
                    "    BLD-Nowhere" + NoCode + "no set of that name is in the clash XML's set tree",
                    "    a side with no set name, so no code and no folder above it could be read",
                    "    BLD-XX-Things, its code XX reads as the team UNKNOWN"
                }));
        }

        /// <summary>
        /// F114 attempt 8, the reviewer's finding 1 of attempt 7. The reader gives a set or a set
        /// folder with no name attribute a null name, and the views' teams are built from every set
        /// it read. A set with no name never throws: no side can name it, and a side with no set
        /// name reads UNKNOWN, as ExportCheck and CodeOf already treat a set with no name. The set
        /// under a folder with no name still takes the team the folder above that names.
        /// </summary>
        [Test]
        public void ASetOrAFolderWithNoNameInTheClashXmlReadsUnknownAndNeverThrows()
        {
            ExchangeDocument document = new ExchangeReader().ReadText(
                "<exchange units=\"ft\"><selectionsets>"
                + "<viewfolder name=\"Electrical\">"
                + "<selectionset guid=\"g1\"/>"
                + "<selectionsetgroup guid=\"g2\"><selectionset name=\"BLD-Devices\" guid=\"g3\"/></selectionsetgroup>"
                + "</viewfolder>"
                + "</selectionsets></exchange>");

            Assert.That(document.Sets.Count, Is.EqualTo(3));
            Assert.That(document.Sets[0].Name, Is.Null, "the reader gives a set with no name attribute a null name");
            Assert.That(document.Sets[1].Name, Is.Null, "the reader gives a set folder with no name attribute a null name");

            ViewTeams teams = new ViewTeams(
                TeamMapTests.MapOf(TeamMapTests.BadersMap), document.Sets, GroupCodes, new ViewpointSettings());

            Assert.That(teams.TeamOfSet(null), Is.EqualTo("UNKNOWN"));
            Assert.That(teams.TeamOfSet(string.Empty), Is.EqualTo("UNKNOWN"));
            Assert.That(teams.TeamOfSet("BLD-Devices"), Is.EqualTo("Electrical"));

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[] { Clash("T", "Clash1", null, "BLD-ST-Floors") }, teams, null, null, new ViewpointSettings());

            Assert.That(plan.Views.Count, Is.EqualTo(0), "F114-K29, a side on UNKNOWN gets no view");
            Assert.That(plan.OutsideTheMap, Is.EqualTo(new[] { "T" }));
            Assert.That(plan.UnknownSets, Is.EqualTo(new[] { string.Empty }));
        }

        /// <summary>
        /// F114 attempt 7, the reviewer's finding 1. Q117 answered C by Bader on 2026-10-05: the
        /// client's one set with no code, BLD-Security Devices, takes the team Electrical its
        /// folder in the corrected XML names, in the views exactly as on the TEAMS line, one rule
        /// in one place, TeamMap.TeamOfSet. It is no UNKNOWN side and is not named as one.
        /// </summary>
        [Test]
        public void TheClientsSetWithNoCodeTakesTheTeamItsFolderNamesAsTheTeamsLineSays()
        {
            ExchangeDocument corrected = MatrixCorrections.ReadPicked(Samples.CorrectedMatrix());
            ViewTeams teams = new ViewTeams(corrected.Teams, corrected.Sets, GroupCodes, new ViewpointSettings());

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[] { Clash("T", "Clash1", "BLD-Security Devices", "BLD-ST-Floors") }, teams, null, null, new ViewpointSettings());

            Assert.That(
                corrected.Teams.SetLines(corrected.Sets, '-')[0],
                Does.Contain("BLD-Security Devices carries no discipline code the map lists, so its team is Electrical"));
            Assert.That(teams.TeamOfSet("BLD-Security Devices"), Is.EqualTo("Electrical"));
            Assert.That(plan.Views[0].Pair.Folder, Is.EqualTo("Structure vs Electrical"));
            Assert.That(plan.Views[0].Pair.CarriesSizeFolder, Is.True);
            Assert.That(plan.UnknownSets, Is.Empty);
            Assert.That(plan.Lines(), Has.Member(UnknownSetsLine + "0" + UnknownSetsTail));
        }

        /// <summary>
        /// Q117 answered C and A, the faults. A code in the set's name is its team whatever folder
        /// it sits in. Two sets of one name whose folders name two teams read UNKNOWN and are
        /// named, never one of the two guessed. A folder naming no team of the map reads UNKNOWN.
        /// </summary>
        [Test]
        public void ASetsFolderGivesItsTeamOnlyWhereItsNameCarriesNoCodeAndOneTeamIsNamed()
        {
            TeamMap map = TeamMapTests.MapOf(TeamMapTests.BadersMap);
            ViewTeams teams = new ViewTeams(
                map,
                new[]
                {
                    Set("BLD-ME-Ducts", "Electrical"),
                    Set("BLD-Doors", "Architecture"),
                    Set("BLD-Devices", "Electrical"),
                    Set("BLD-Devices", "Mechanical"),
                    Set("BLD-Signs", "Security")
                },
                GroupCodes,
                new ViewpointSettings());

            Assert.That(teams.TeamOfSet("BLD-ME-Ducts"), Is.EqualTo("Mechanical"));
            Assert.That(teams.TeamOfSet("BLD-Doors"), Is.EqualTo("Architecture"));
            Assert.That(teams.TeamOfSet("BLD-Devices"), Is.EqualTo("UNKNOWN"));
            Assert.That(teams.TeamOfSet("BLD-Signs"), Is.EqualTo("UNKNOWN"));
            Assert.That(teams.TeamOfSet("BLD-Nowhere"), Is.EqualTo("UNKNOWN"));

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[]
                {
                    Clash("T", "Clash1", "BLD-Doors", "BLD-Devices"),
                    Clash("U", "Clash1", "BLD-Signs", "BLD-ST-Floors")
                },
                teams,
                null, null,
                new ViewpointSettings());

            Assert.That(plan.Views.Count, Is.EqualTo(0), "F114-K29, a side on UNKNOWN gets no view");
            Assert.That(plan.OutsideTheMap, Is.EqualTo(new[] { "T", "U" }));
            Assert.That(plan.UnknownSets, Is.EqualTo(new[] { "BLD-Devices", "BLD-Signs" }));
        }

        /// <summary>
        /// Row F114-K30. A side is the test's locator whole, its folders and its set name, so two
        /// sets of one name in two folders naming two teams are told apart by the folders the
        /// locator holds, each reading the team its own folder names. A locator naming a path no
        /// set of the tree is at reads by the name alone, as before, UNKNOWN with why.
        /// </summary>
        [Test]
        public void TwoSetsOfOneNameAreToldApartByTheFoldersTheirLocatorHolds()
        {
            ViewTeams teams = new ViewTeams(
                TeamMapTests.MapOf(TeamMapTests.BadersMap),
                new[] { Set("BLD-Devices", "Electrical"), Set("BLD-Devices", "Mechanical", "Mechanical-HVAC") },
                GroupCodes,
                new ViewpointSettings());
            const string Electrical = "lcop_selection_set_tree/Electrical/BLD-Devices";
            const string Mechanical = "lcop_selection_set_tree/Mechanical/Mechanical-HVAC/BLD-Devices";
            const string Nowhere = "lcop_selection_set_tree/Security/BLD-Devices";

            Assert.That(teams.TeamOfSet(Electrical), Is.EqualTo("Electrical"));
            Assert.That(teams.TeamOfSet(Mechanical), Is.EqualTo("Mechanical"));
            Assert.That(teams.UnknownWords(Electrical), Is.Null);
            Assert.That(teams.TeamOfSet("BLD-Devices"), Is.EqualTo("UNKNOWN"), "a set name alone still reads both sets");
            Assert.That(teams.TeamOfSet(Nowhere), Is.EqualTo("UNKNOWN"));
            Assert.That(
                teams.UnknownWords(Nowhere),
                Is.EqualTo("BLD-Devices" + NoCode + "its 2 sets of that name in the clash XML's set tree give the teams Electrical, Mechanical"));

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[]
                {
                    Clash("T", "Clash1", Electrical, "lcop_selection_set_tree/Structure/BLD-ST-Floors"),
                    Clash("U", "Clash1", Mechanical, "lcop_selection_set_tree/Structure/BLD-ST-Floors"),
                    Clash("V", "Clash1", Nowhere, "lcop_selection_set_tree/Structure/BLD-ST-Floors")
                },
                teams,
                null, null,
                new ViewpointSettings());

            Assert.That(plan.Views.Count, Is.EqualTo(2));
            Assert.That(plan.Views[0].Pair.Folder, Is.EqualTo("Structure vs Mechanical"));
            Assert.That(plan.Views[1].Pair.Folder, Is.EqualTo("Structure vs Electrical"));
            Assert.That(plan.OutsideTheMap, Is.EqualTo(new[] { "V" }), "F114-K29, UNKNOWN is outside the map");
            Assert.That(plan.UnknownSets, Is.EqualTo(new[] { Nowhere }), "each side by its whole locator");
        }

        /// <summary>
        /// N5 of the second reading of F114c. Two sides of one set name in two folders can read
        /// UNKNOWN for two reasons, so the plan keys an UNKNOWN side on its whole locator and
        /// writes each reason. Keyed on the set name, the second reason was never written.
        /// </summary>
        [Test]
        public void EachUnknownSideOfOneSetNameIsNamedWithItsOwnReason()
        {
            ViewTeams teams = new ViewTeams(
                TeamMapTests.MapOf(TeamMapTests.BadersMap),
                new[] { Set("BLD-Devices", "Electrical"), Set("BLD-Devices", "Mechanical"), Set("BLD-Devices", "Security") },
                GroupCodes,
                new ViewpointSettings());
            const string Security = "lcop_selection_set_tree/Security/BLD-Devices";
            const string Nowhere = "lcop_selection_set_tree/Other/BLD-Devices";
            const string Floors = "lcop_selection_set_tree/Structure/BLD-ST-Floors";

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[] { Clash("T", "Clash1", Security, Floors), Clash("U", "Clash1", Nowhere, Floors) },
                teams,
                null, null,
                new ViewpointSettings());

            Assert.That(plan.UnknownSets, Is.EqualTo(new[] { Security, Nowhere }));
            IList<string> lines = plan.Lines();
            Assert.That(lines, Has.Member(UnknownSetsLine + "2" + UnknownSetsTail));
            Assert.That(lines, Has.Member("    BLD-Devices" + NoCode + "no folder above it in the clash XML's set tree names a team"));
            Assert.That(
                lines,
                Has.Member("    BLD-Devices" + NoCode + "its 3 sets of that name in the clash XML's set tree give the teams Electrical, Mechanical, UNKNOWN"));
        }

        /// <summary>
        /// The breaker's point 3 on attempt 1 of F114c. A weekly run with no clash matrix read hands
        /// the views no set tree, and a side handed as its whole locator then reads exactly the team
        /// its set name read before F114-K30: the code its name carries through the map, so its pair
        /// and its size folder stay, and UNKNOWN with why where its name carries no code.
        /// </summary>
        [Test]
        public void WithNoSetTreeALocatorReadsTheTeamItsSetNameReads()
        {
            ViewTeams teams = Teams();
            const string Ducts = "lcop_selection_set_tree/Mechanical/BLD-ME-Ducts";
            const string Columns = "lcop_selection_set_tree/Structure/BLD-ST-Columns";
            const string Devices = "lcop_selection_set_tree/Electrical/BLD-Devices";

            Assert.That(teams.TeamOfSet(Ducts), Is.EqualTo(teams.TeamOfSet("BLD-ME-Ducts")));
            Assert.That(teams.TeamOfSet(Ducts), Is.EqualTo("Mechanical"));
            Assert.That(teams.TeamOfSet(Devices), Is.EqualTo(teams.TeamOfSet("BLD-Devices")));
            Assert.That(teams.UnknownWords(Devices), Is.EqualTo(teams.UnknownWords("BLD-Devices")));

            TestViewPlanOutcome byLocator = TestViewPlan.For(
                new[] { Clash("T", "Clash1", Ducts, Columns, size: SizeVerdict.Large) }, teams, null, null, new ViewpointSettings());
            TestViewPlanOutcome byName = TestViewPlan.For(
                new[] { Clash("T", "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.Large) }, teams, null, null, new ViewpointSettings());

            Assert.That(byLocator.Views[0].Pair.Folder, Is.EqualTo("Structure vs Mechanical"));
            Assert.That(byLocator.Views[0].Pair.Folder, Is.EqualTo(byName.Views[0].Pair.Folder));
            Assert.That(byLocator.Views[0].Folders, Is.EqualTo(byName.Views[0].Folders), "the size folder kept");
        }

        private static SelectionSetDefinition Set(string name, params string[] folders)
        {
            return new SelectionSetDefinition(
                name, folders, "lcop_selection_set_tree/" + string.Join("/", folders) + "/" + name, "all", false, null, null);
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
                new[] { Mirror }, null,
                new ViewpointSettings());

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(plan.Views[0].Name, Is.EqualTo(Kept));
            Assert.That(plan.PairOfTest(Mirror), Is.Null, "a mirror is no test of this plan");
            Assert.That(plan.Mirrored, Is.EqualTo(new[] { Mirror }));
            Assert.That(plan.LeftOutAsMirrors, Is.EqualTo(2));
            Assert.That(plan.LeftOutAt(ClashStatus.Resolved), Is.EqualTo(0), "a mirror's clashes are counted once, as a mirror's");
            Assert.That(plan.InViews + plan.LeftOut + plan.LeftOutAsMirrors, Is.EqualTo(plan.Considered));

            IList<string> lines = plan.Lines();

            Assert.That(lines, Has.Member("mirrored tests, so no view : 1, their 2 clashes left out"));
            Assert.That(lines, Has.Member("    " + Mirror + " : 1 open and in no view, 1 at a status no view shows"));
            Assert.That(lines, Has.Member("clashes looked at : 3, open in views 1, left out by status 0, of mirrored tests 2"));
        }

        /// <summary>
        /// Rows F114-K34 and F114-R1. A mirror the rule paired whose clashes were not merged into the
        /// test it mirrors keeps its rows on the report under its own name, and gets no view by
        /// Bader's rule, so its open clashes are in no view of their own. The VIEWS block counts
        /// those open clashes apart from the closed ones, per mirror and in all, and says a clash
        /// only that mirror finds is then in no view, so the views can hold fewer clashes than the
        /// report. It never moves them into the kept test's view, which no rule says.
        /// </summary>
        [Test]
        public void TheOpenClashesOfAMirrorNotMergedAreCountedPerMirrorAndSaidToBeInNoView()
        {
            const string Kept = "BLD-ME-Ducts-vs-BLD-ST-Columns";
            const string Mirror = "BLD-ST-Columns-vs-BLD-ME-Ducts (mirror)";
            const string OtherKept = "BLD-EL-Devices-vs-BLD-AR-Walls";
            const string OtherMirror = "BLD-AR-Walls-vs-BLD-EL-Devices (mirror)";

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[]
                {
                    Clash(Kept, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns"),
                    Clash(Mirror, "Clash1", "BLD-ST-Columns", "BLD-ME-Ducts", ClashStatus.New),
                    Clash(Mirror, "Clash2", "BLD-ST-Columns", "BLD-ME-Ducts", ClashStatus.Active),
                    Clash(Mirror, "Clash3", "BLD-ST-Columns", "BLD-ME-Ducts", ClashStatus.Approved),
                    Clash(OtherKept, "Clash1", "BLD-EL-Devices", "BLD-AR-Walls"),
                    Clash(OtherMirror, "Clash1", "BLD-AR-Walls", "BLD-EL-Devices", ClashStatus.Resolved)
                },
                Teams(),
                new[] { Mirror, OtherMirror }, null,
                new ViewpointSettings());

            Assert.That(plan.Views.Count, Is.EqualTo(2), "each kept test keeps its own view and no mirror gets one");
            Assert.That(plan.LeftOutAsMirrors, Is.EqualTo(4));

            IList<string> lines = plan.Lines();

            Assert.That(lines, Has.Member("mirrored tests, so no view : 2, their 4 clashes left out"));
            Assert.That(lines, Has.Member(
                "open clashes of those mirrored tests, in no view : 2, each on the report under its mirror's own name because "
                + "its clashes were not merged into the test it mirrors, so a clash only that mirror finds is in no view "
                + "and how many of them it alone finds is UNKNOWN"));
            Assert.That(lines, Has.Member("    " + Mirror + " : 2 open and in no view, 1 at a status no view shows"));
            Assert.That(lines, Has.Member("    " + OtherMirror + " : 0 open and in no view, 1 at a status no view shows"));
        }

        /// <summary>With no mirror rule handed in the plan cannot know a mirror, and its lines say so.</summary>
        [Test]
        public void APlanHandedNoMirrorRuleSaysSo()
        {
            IList<string> lines = Plan(Clash("T", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns")).Lines();

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
                Clash("Open", "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New)).Lines();

            Assert.That(lines, Has.Member("tests whose every clash is at a status no view shows, so no view : 1, a test with no clash at all is not handed to the plan and not counted here"));
        }

        /// <summary>Q123 by its default A: with no team map every code is a team of its own and no pair carries the size folder.</summary>
        [Test]
        public void WithNoTeamMapThePairsAreCodesAndNoSizeFolderIsMade()
        {
            ViewTeams teams = new ViewTeams(TeamMap.NoXml(new TeamMapSettings()), null, GroupCodes, new ViewpointSettings());

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[] { Clash("T", "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", size: SizeVerdict.Large) },
                teams,
                null, null,
                new ViewpointSettings());

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(plan.Views[0].Folders, Is.EqualTo(new[] { "Clash Status", "A", "Mechanical" }));
        }

        // ---------- the counts and the block ----------

        /// <summary>
        /// F114-K29, Bader's words of 2026-10-13, "the idea is to create A B C and delete what come
        /// outside". Read as: only the teams the map names get pair folders, a test with a side on
        /// a team the map does not name, or on UNKNOWN, gets no view, and nothing else is removed.
        /// The plan names the test, counts its open clashes apart and the lines say how many.
        /// </summary>
        [Test]
        public void ATestWithASideOnATeamTheMapDoesNotNameGetsNoViewAndIsCountedAndNamed()
        {
            TestViewPlanOutcome plan = Plan(
                Clash("In", "Clash1", "BLD-EL-Lighting Fixtures", "BLD-ST-Floors", ClashStatus.New),
                Clash("Out", "Clash1", "BLD-QQ-Widgets", "BLD-ST-Floors", ClashStatus.New),
                Clash("Out", "Clash2", "BLD-QQ-Widgets", "BLD-ST-Floors", ClashStatus.Active),
                Clash("Out", "Clash3", "BLD-QQ-Widgets", "BLD-ST-Floors", ClashStatus.Approved));

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(plan.Views[0].Name, Is.EqualTo("In"));
            Assert.That(plan.OutsideTheMap, Is.EqualTo(new[] { "Out" }));
            Assert.That(plan.OpenOutsideTheMap, Is.EqualTo(2));
            Assert.That(plan.TestsWithNoOpenClash, Is.EqualTo(0));

            IList<string> lines = plan.Lines();

            Assert.That(
                lines,
                Does.Contain("tests with a side on a team the team map does not name, or on UNKNOWN, so no view : 1, their 2 open clashes in no view, "
                    + "and nothing deleted from the NWF, the clashes, the sets or the report"));
            Assert.That(
                lines,
                Does.Contain("    Out : 2 open and in no view, 1 at a status no view shows, its sides read as the teams UNKNOWN and Structure"));
        }

        [Test]
        public void WithNoTeamMapEveryTeamIsTakenAsNamedAndTheRuleIsNotApplied()
        {
            ViewTeams noMap = new ViewTeams(
                TeamMap.NoXml(new TeamMapSettings()), null, GroupCodes, new ViewpointSettings());

            Assert.That(noMap.NamedByTheMap("QQ"), Is.True);
            Assert.That(noMap.NamedByTheMap("UNKNOWN"), Is.True);

            TestViewPlanOutcome plan = TestViewPlan.For(
                new[] { Clash("T", "Clash1", "BLD-QQ-Widgets", "BLD-ST-Floors") }, noMap, null, null, new ViewpointSettings());

            Assert.That(plan.Views.Count, Is.EqualTo(1));
            Assert.That(plan.OutsideTheMap, Is.Empty);
        }

        [Test]
        public void ATeamTheMapNamesIsNamedAndOneNamedUnknownOrOutsideIsNot()
        {
            ViewTeams teams = Teams();

            Assert.That(teams.NamedByTheMap("Structure"), Is.True);
            Assert.That(teams.NamedByTheMap("Electrical"), Is.True);
            Assert.That(teams.NamedByTheMap("UNKNOWN"), Is.False);
            Assert.That(teams.NamedByTheMap("QQ"), Is.False);
            Assert.That(teams.NamedByTheMap(null), Is.False);
        }

        /// <summary>
        /// Every open clash is in exactly one view and no clash a view does not show is in any,
        /// over generated clashes of many tests, statuses and sizes, and the counts add up.
        /// </summary>
        [Test]
        public void EveryOpenClashIsInExactlyOneViewOverGeneratedClashes()
        {
            // Every set here has a code the map names, so none is outside the map, F114-K29, which
            // its own tests below cover.
            string[] sets = { "BLD-AR-Walls", "BLD-ST-Columns", "BLD-ME-Ducts", "BLD-EL-Cable Trays" };
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

                TestViewPlanOutcome plan = TestViewPlan.For(clashes, Teams(), null, null, new ViewpointSettings());
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
