using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Teams;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F114, Q114 point 19, the test that proves the tree: no team pair folder holds a test of
    /// another pair, no Over 150mm folder sits outside its own pair, no view shows a model of a
    /// third team, no clash is in two views and no mirrored test is run, with two checks more,
    /// that every view not this tool's before is there after and that no per clash viewpoint of
    /// an earlier run is left without a reason. A whole good tree passes, and each check broken
    /// once names what broke it. The VIEWS TREE block lists the tree and the seven lines. A
    /// failed check is a FAILED line, and the group keeps its own result. Names are sample data.
    /// </summary>
    [TestFixture]
    public class ViewsTreeCheckTests
    {
        private const string Ducts = "BLD-ME-Ducts-vs-BLD-ST-Columns";
        private const string Lights = "BLD-EL-Lighting Fixtures-vs-BLD-ST-Floors";
        private const string Walls = "BLD-AR-Walls-vs-BLD-ST-Columns";

        private static readonly ViewpointSettings Settings = new ViewpointSettings();
        private static readonly string Run = ToolViewMark.StampOf(new DateTime(2026, 10, 5, 10, 0, 0, DateTimeKind.Utc));
        private static readonly Point3 Camera = new Point3(1, 2, 3);
        private static readonly string[] Codes = { "AR", "EL", "ME", "ST" };

        private static TeamMap Map()
        {
            return TeamMapTests.MapOf(TeamMapTests.BadersMap);
        }

        private static ModelTeam Model(string code)
        {
            return new ModelTeam("1A02MM-" + code + ".nwc", code, Map().TeamOf(code));
        }

        private static List<ModelTeam> Models()
        {
            return new List<ModelTeam> { Model("AR"), Model("EL"), Model("ME"), Model("ST") };
        }

        private static ViewClash Clash(string test, string name, string left, string right, ClashPriority priority,
            SizeVerdict? size, int first, int second, string firstHome, string secondHome)
        {
            return new ViewClash(test, name, left, right, ClashStatus.New, priority, size,
                new ItemPath(new[] { 0, first }), new ItemPath(new[] { 1, second }), Camera,
                Model(firstHome).FileName, Model(secondHome).FileName);
        }

        private static TestViewPlanOutcome PlanOf(string lightsHome = "EL", ICollection<string> mirrors = null)
        {
            ViewClash[] clashes =
            {
                Clash(Ducts, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashPriority.A, SizeVerdict.Large, 1, 1, "ME", "ST"),
                Clash(Ducts, "Clash2", "BLD-ME-Ducts", "BLD-ST-Columns", ClashPriority.A, SizeVerdict.Small, 2, 2, "ME", "ST"),
                Clash(Lights, "Clash1", "BLD-EL-Lighting Fixtures", "BLD-ST-Floors", ClashPriority.A, null, 3, 3, lightsHome, "ST"),
                Clash(Walls, "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashPriority.B, null, 4, 4, "AR", "ST")
            };

            return TestViewPlan.For(clashes, new ViewTeams(Map(), Codes, Settings), mirrors, Settings);
        }

        private static string[] MarkOf(IList<string> folders, string name, Point3 camera)
        {
            return new[] { ToolViewMark.Body(Run, folders, name, camera, null, Settings) };
        }

        /// <summary>The tree as this run left it: a person's view at the root, then this run's marked folders and views.</summary>
        private static Tree Good(string lightsHome = "EL", ICollection<string> mirrors = null)
        {
            Tree tree = new Tree { Plan = PlanOf(lightsHome, mirrors) };
            Dictionary<string, int> next = new Dictionary<string, int>();
            HashSet<string> made = new HashSet<string>();

            tree.After.Add(new ViewNode(new string[0], "Level 1", false, Next(next, string.Empty), null, 0, Camera, null, false));

            foreach (PlannedTestView view in tree.Plan.Views)
            {
                List<string> above = new List<string>();

                foreach (string folder in view.Folders)
                {
                    string path = string.Join("/", above.ToArray()) + "|" + folder;

                    if (made.Add(path))
                    {
                        tree.After.Add(new ViewNode(above, folder, true, Next(next, string.Join("/", above.ToArray())),
                            MarkOf(above, folder, null), 0, null, null, false));
                    }

                    above.Add(folder);
                }

                int index = Next(next, view.FolderPath);
                tree.After.Add(new ViewNode(view.Folders, view.Name, false, index, MarkOf(view.Folders, view.Name, Camera), 0, Camera, null, false));
                tree.Written.Add(new WrittenView(view.Folders, view.Name, index, true, true));
                tree.Hidden[view.ToString()] = FileNames(ShownModels.For(view.Pair, Models(), HomesOf(view)).Hidden);
                tree.Painted[view.ToString()] = new List<ItemPath>(PaintPlan.For(view.Clashes).Solid);
            }

            return tree;
        }

        private static int Next(Dictionary<string, int> next, string parent)
        {
            int index = next.ContainsKey(parent) ? next[parent] : 0;
            next[parent] = index + 1;
            return index;
        }

        private static List<string> HomesOf(PlannedTestView view)
        {
            List<string> homes = new List<string>();

            foreach (ViewClash clash in view.Clashes)
            {
                homes.Add(clash.FirstHome);
                homes.Add(clash.SecondHome);
            }

            return homes;
        }

        private static IList<string> FileNames(IEnumerable<ModelTeam> models)
        {
            List<string> names = new List<string>();

            foreach (ModelTeam model in models)
            {
                names.Add(model.FileName);
            }

            return names;
        }

        private static ViewNode Legacy()
        {
            return new ViewNode(new[] { "ME vs ST" }, Ducts + "  Clash9", false, 0, null, 0, Camera, null, false);
        }

        private static ViewsTreeFacts FactsOf(Tree tree)
        {
            List<ViewNode> before = new List<ViewNode>(tree.After);
            before.Add(new ViewNode(new string[0], "ME vs ST", true, 9, null, 0, null, null, false));
            before.Add(Legacy());

            List<string> tests = new List<string> { Ducts, Lights, Walls };
            ViewsInventory inventory = ViewsInventory.Plan(before, tree.Plan, tests, tree.Written, Run, true, Codes, tests, Settings);

            return new ViewsTreeFacts
            {
                Group = "1A02MM",
                Map = Map(),
                Models = Models(),
                Plan = tree.Plan,
                Inventory = inventory,
                Written = tree.Written,
                After = tree.After,
                RunStamp = Run,
                HiddenReadBack = tree.Hidden,
                PaintedReadBack = tree.Painted,
                TestsRun = tests,
                Mirrors = new List<string>(),
                KnownCodes = Codes,
                TestNames = tests,
                Settings = Settings
            };
        }

        private static ViewsTreeCheck Check(ViewsTreeFacts facts, int number)
        {
            foreach (ViewsTreeCheck check in ViewsTreeCheck.Of(facts))
            {
                if (check.Number == number)
                {
                    return check;
                }
            }

            throw new InvalidOperationException("no check " + number);
        }

        private static string Joined(IEnumerable<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        private static int IndexOfView(Tree tree, string folderPath, string name)
        {
            return tree.After.FindIndex(node => !node.IsFolder && node.FolderPath == folderPath && node.Name == name);
        }

        // ---------- the whole good tree ----------

        [Test]
        public void AWholeGoodTreePassesEverySevenChecks()
        {
            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(FactsOf(Good()));

            Assert.That(checks.Count, Is.EqualTo(7));

            for (int i = 0; i < checks.Count; i++)
            {
                Assert.That(checks[i].Number, Is.EqualTo(i + 1));
                Assert.That(checks[i].Holds, Is.True, "check " + checks[i].Number + ": " + Joined(checks[i].Failures));
            }
        }

        // ---------- each check broken once ----------

        [Test]
        public void Check1NamesATestInAnotherPairsFolder()
        {
            Tree tree = Good();
            int at = IndexOfView(tree, "A/Structure vs Electrical", Lights);
            string[] wrong = { "A", "Structure vs Mechanical" };
            tree.After[at] = new ViewNode(wrong, Lights, false, 5, MarkOf(wrong, Lights, Camera), 0, Camera, null, false);

            ViewsTreeCheck check = Check(FactsOf(tree), 1);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain(Lights).And.Contain("Structure vs Electrical"));
        }

        [Test]
        public void Check2NamesAnOver150mmFolderAtThePriorityLevel()
        {
            Tree tree = Good();
            string[] priority = { "A" };
            string[] wrong = { "A", "Over 150mm" };
            int at = IndexOfView(tree, "A/Structure vs Mechanical/Over 150mm", Ducts);
            tree.After.Add(new ViewNode(priority, "Over 150mm", true, 7, MarkOf(priority, "Over 150mm", null), 0, null, null, false));
            tree.After[at] = new ViewNode(wrong, Ducts, false, 0, MarkOf(wrong, Ducts, Camera), 0, Camera, null, false);

            ViewsTreeCheck check = Check(FactsOf(tree), 2);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain("A/Over 150mm"));
        }

        [Test]
        public void Check3NamesAViewThatShowsAThirdTeamsModel()
        {
            Tree tree = Good();
            tree.Hidden["A/Structure vs Mechanical/" + Ducts] = new List<string> { Model("EL").FileName };

            ViewsTreeCheck check = Check(FactsOf(tree), 3);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain(Model("AR").FileName).And.Contain(Ducts));
        }

        /// <summary>Q118 A: a clashing item in a third team's model shows that model, and check 3 names it and holds.</summary>
        [Test]
        public void Check3NamesAnAllowedExceptionAndHolds()
        {
            ViewsTreeCheck check = Check(FactsOf(Good("AR")), 3);

            Assert.That(check.Holds, Is.True, Joined(check.Failures));
            Assert.That(Joined(check.Notes), Does.Contain(Model("AR").FileName).And.Contain(Lights));
        }

        [Test]
        public void Check3SaysWhereNothingWasReadBack()
        {
            Tree tree = Good();
            tree.Hidden.Clear();

            ViewsTreeCheck check = Check(FactsOf(tree), 3);

            Assert.That(check.Holds, Is.True);
            Assert.That(check.Basis, Does.Contain("plan"));
        }

        [Test]
        public void Check4NamesAClashInTwoViews()
        {
            Tree tree = Good();
            PlannedTestView lights = tree.Plan.Views[2];
            tree.Plan.Add(new PlannedTestView(lights.Priority, lights.PriorityFolder, lights.Pair, "Over 150mm", lights.Name, lights.Clashes));

            ViewsTreeCheck check = Check(FactsOf(tree), 4);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain(Lights).And.Contain("Clash1"));
        }

        [Test]
        public void Check4NamesAViewWhosePaintDoesNotReadBack()
        {
            Tree tree = Good();
            tree.Painted["B/Architecture vs Structure/" + Walls] = new List<ItemPath> { new ItemPath(new[] { 0, 4 }) };

            ViewsTreeCheck check = Check(FactsOf(tree), 4);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain(Walls));
        }

        [Test]
        public void Check5NamesAMirrorThatWasRun()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Mirrors = new List<string> { Walls };

            ViewsTreeCheck check = Check(facts, 5);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain(Walls));
        }

        /// <summary>A mirror rule that ran and found no mirror ran the check, and says it proves nothing here.</summary>
        [Test]
        public void Check5SaysItProvesNothingWithNoMirror()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            ViewsTreeCheck check = Check(facts, 5);

            Assert.That(check.Ran, Is.True);
            Assert.That(check.Holds, Is.True);
            Assert.That(Joined(check.Notes), Does.Contain("proves nothing"));
        }

        /// <summary>
        /// F114 attempt 2, the breaker's finding 0. With no mirror rule handed in check 5 did not
        /// run. It is never counted as holding: its line says it did not run and why, and the last
        /// line counts it apart, CLAUDE.md, never report a check that did not run.
        /// </summary>
        [Test]
        public void Check5DidNotRunWithNoMirrorRuleAndIsNotCountedAsHolding()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Mirrors = null;
            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);
            ViewsTreeCheck check = checks[4];
            string block = Joined(ViewsTree.Lines(facts, checks, 0));

            Assert.That(check.Ran, Is.False);
            Assert.That(check.Holds, Is.False);
            Assert.That(check.NotRunWhy, Does.Contain("no mirror rule"));
            Assert.That(block, Does.Contain("\nCHECK 5  no mirrored test is run or has a view  DID NOT RUN, no mirror rule"));
            Assert.That(block, Does.Contain("\n6 of 7 hold, 1 did not run"));
            Assert.That(block, Does.Not.Contain("7 of 7 hold"));
            Assert.That(block, Does.Not.Contain("FAILED"));
        }

        /// <summary>
        /// F114 attempt 2, the breaker's finding 1. A mirror not run this week whose old results
        /// the add-in read still got a view, and check 5 read only the tests run. It reads the tests
        /// the plan made views for.
        /// </summary>
        [Test]
        public void Check5NamesAMirrorThatHasAViewThoughItWasNotRun()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Mirrors = new List<string> { Walls };
            facts.TestsRun = new List<string> { Ducts, Lights };

            ViewsTreeCheck check = Check(facts, 5);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain(Walls + " is a mirror and has a view"));
        }

        /// <summary>The plan takes the mirrors, so no mirrored test gets a view, Bader's point that there are no mirrored tests.</summary>
        [Test]
        public void Check5HoldsWhenThePlanTookTheMirrors()
        {
            ViewsTreeFacts facts = FactsOf(Good("EL", new[] { Walls }));
            facts.Mirrors = new List<string> { Walls };
            facts.TestsRun = new List<string> { Ducts, Lights };

            ViewsTreeCheck check = Check(facts, 5);

            Assert.That(check.Ran, Is.True);
            Assert.That(check.Holds, Is.True, Joined(check.Failures));
            Assert.That(check.Basis, Does.Contain("1 mirrors against the 2 tests the plan made views for and the 2 tests the clash step ran"));
        }

        /// <summary>The breaker's finding 5. The checks that read the walk after did not run with no walk.</summary>
        [Test]
        public void TheChecksOfTheWalkDidNotRunWithNoWalk()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            facts.After = null;
            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);

            foreach (int number in new[] { 1, 2, 6, 7 })
            {
                Assert.That(checks[number - 1].Ran, Is.False, "check " + number);
                Assert.That(checks[number - 1].Holds, Is.False, "check " + number);
                Assert.That(checks[number - 1].NotRunWhy, Does.Contain("no walk of the tree"), "check " + number);
            }

            Assert.That(checks[2].Ran && checks[3].Ran && checks[4].Ran, Is.True, "3, 4 and 5 read the plan");
            Assert.That(Joined(ViewsTree.Lines(facts, checks, 0)), Does.Contain("\n3 of 7 hold, 4 did not run"));
        }

        [Test]
        public void ChecksOneAndTwoDidNotRunWithNoRunStamp()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            facts.RunStamp = null;

            Assert.That(Check(facts, 1).Ran, Is.False);
            Assert.That(Check(facts, 1).NotRunWhy, Does.Contain("no run stamp"));
            Assert.That(Check(facts, 2).Ran, Is.False);
        }

        /// <summary>The breaker's finding 5: a plan of four views and not one found marked by this run is not a check that held.</summary>
        [Test]
        public void ChecksOneAndTwoDidNotRunWhenNoPlannedViewIsFoundMarked()
        {
            Tree tree = Good();
            ViewsTreeFacts facts = FactsOf(tree);
            facts.After = tree.After.FindAll(node => node.Name == "Level 1");

            Assert.That(Check(facts, 1).Ran, Is.False);
            Assert.That(Check(facts, 1).NotRunWhy, Does.Contain("none of the 4 planned views"));
            Assert.That(Check(facts, 2).Ran, Is.False);
        }

        [Test]
        public void ChecksSixAndSevenDidNotRunWithNoInventory()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Inventory = null;

            Assert.That(Check(facts, 6).Ran, Is.False);
            Assert.That(Check(facts, 6).NotRunWhy, Does.Contain("no inventory"));
            Assert.That(Check(facts, 7).Ran, Is.False);
        }

        [Test]
        public void CheckSevenDidNotRunWithNoCodesOrTestNames()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            facts.KnownCodes = null;

            Assert.That(Check(facts, 7).Ran, Is.False);
            Assert.That(Check(facts, 7).NotRunWhy, Does.Contain("per clash viewpoint"));
        }

        [Test]
        public void CheckThreeDidNotRunWithNoModels()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Models = null;

            Assert.That(Check(facts, 3).Ran, Is.False);
            Assert.That(Check(facts, 3).NotRunWhy, Does.Contain("no models"));
        }

        /// <summary>
        /// The breaker's finding 2: a clashing item whose model could not be read, or whose model
        /// is no model of the group, is said, since whether its model is shown is UNKNOWN.
        /// </summary>
        [Test]
        public void Check3SaysAClashingItemWhoseModelIsUnknown()
        {
            ViewClash[] clashes =
            {
                new ViewClash(Walls, "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.B, null,
                    new ItemPath(new[] { 0, 4 }), new ItemPath(new[] { 1, 4 }), Camera, null, "1A02MM-XX.nwc")
            };
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Plan = TestViewPlan.For(clashes, new ViewTeams(Map(), Codes, Settings), new string[0], Settings);
            facts.HiddenReadBack = null;

            ViewsTreeCheck check = Check(facts, 3);

            Assert.That(check.Holds, Is.True, Joined(check.Failures));
            Assert.That(Joined(check.Notes), Does.Contain(Walls + " has 1 clashing items whose model could not be read"));
            Assert.That(Joined(check.Notes), Does.Contain("1A02MM-XX.nwc, which is no model of this group"));
        }

        [Test]
        public void Check6NamesAPersonsViewMissingAfter()
        {
            Tree tree = Good();
            ViewsTreeFacts facts = FactsOf(tree);
            facts.After = tree.After.FindAll(node => node.Name != "Level 1");

            ViewsTreeCheck check = Check(facts, 6);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain("Level 1"));
        }

        /// <summary>
        /// The reviewer's finding 7 and the breaker's 6: a person's view under two folders of one
        /// name is kept, and check 6 follows every item the inventory keeps, not two kinds of them.
        /// </summary>
        [Test]
        public void Check6NamesAPersonsViewUnderTwinFoldersMissingAfter()
        {
            Tree tree = Good();
            tree.After.Add(new ViewNode(new string[0], "Mine", true, 20, null, 0, null, null, false));
            tree.After.Add(new ViewNode(new string[0], "Mine", true, 21, null, 0, null, null, false));
            tree.After.Add(new ViewNode(new[] { "Mine" }, "Rami's view", false, 0, null, 0, Camera, null, false));
            ViewsTreeFacts facts = FactsOf(tree);
            facts.After = tree.After.FindAll(node => node.Name != "Rami's view");

            ViewsTreeCheck check = Check(facts, 6);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain("Mine/Rami's view was there before and is not there after"));
        }

        [Test]
        public void Check7NamesAPerClashViewpointLeftWithNoReason()
        {
            Tree tree = Good();
            ViewsTreeFacts facts = FactsOf(tree);
            facts.After = new List<ViewNode>(tree.After) { Legacy() };

            ViewsTreeCheck check = Check(facts, 7);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain(Ducts + "  Clash9"));
        }

        // ---------- the block ----------

        [Test]
        public void TheBlockListsTheTreeAndTheSevenChecks()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            string block = Joined(ViewsTree.Lines(facts, ViewsTreeCheck.Of(facts), 0));

            Assert.That(block, Does.StartWith("VIEWS TREE 1A02MM"));
            Assert.That(block, Does.Contain("\nA\n  Structure vs Mechanical\n    " + Ducts + "  1 open clashes, shows ME ST, hides AR EL, read back"));
            Assert.That(block, Does.Contain("\n    Over 150mm\n      " + Ducts + "  1 open clashes"));
            Assert.That(block, Does.Contain("\nB\n  Architecture vs Structure\n    " + Walls));
            Assert.That(block, Does.Contain("models     : AR Architecture, EL Electrical, ME Mechanical, ST Structure"));

            for (int number = 1; number <= 7; number++)
            {
                Assert.That(block, Does.Contain("\nCHECK " + number + "  "));
            }

            Assert.That(block, Does.Contain("7 of 7 hold"));
            Assert.That(block, Does.Not.Contain("FAILED"));
        }

        [Test]
        public void AFailedCheckIsAFailedLineNamingWhatBrokeIt()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Mirrors = new List<string> { Walls };
            string block = Joined(ViewsTree.Lines(facts, ViewsTreeCheck.Of(facts), 0));

            Assert.That(block, Does.Contain("FAILED CHECK 5"));
            Assert.That(block, Does.Contain(Walls));
            Assert.That(block, Does.Contain("6 of 7 hold"));
        }

        /// <summary>The .log cuts the tree at the TreeLinesInLog setting and says so, and the checks are never cut.</summary>
        [Test]
        public void TheTreeIsCutWhereTheSettingSaysAndTheChecksAreNot()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            IList<string> cut = ViewsTree.Lines(facts, ViewsTreeCheck.Of(facts), 3);
            IList<string> whole = ViewsTree.Lines(facts, ViewsTreeCheck.Of(facts), 0);
            string block = Joined(cut);

            Assert.That(cut.Count, Is.LessThan(whole.Count));
            Assert.That(block, Does.Contain("tree lines not written here"));
            Assert.That(block, Does.Contain("CHECK 7"));
        }

        /// <summary>
        /// The breaker's finding 3: a test with no clash at all never reaches the plan, so the
        /// tests that ran with no open clash are counted off the tests the clash step ran, and are
        /// UNKNOWN where those were not handed in.
        /// </summary>
        [Test]
        public void TheBlockCountsTheTestsThatRanWithNoOpenClashOffTheTestsRun()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            facts.TestsRun = new List<string> { Ducts, Lights, Walls, "BLD-AR-Doors-vs-BLD-ST-Columns" };

            Assert.That(Joined(ViewsTree.Lines(facts, ViewsTreeCheck.Of(facts), 0)),
                Does.Contain("4 tests ran, 3 with a view and 1 with no open clash"));

            facts.TestsRun = null;

            Assert.That(Joined(ViewsTree.Lines(facts, ViewsTreeCheck.Of(facts), 0)),
                Does.Contain("how many tests ran with no open clash is UNKNOWN, the tests the clash step ran were not handed in"));
        }

        private sealed class Tree
        {
            internal TestViewPlanOutcome Plan;
            internal readonly List<ViewNode> After = new List<ViewNode>();
            internal readonly List<WrittenView> Written = new List<WrittenView>();
            internal readonly Dictionary<string, IList<string>> Hidden = new Dictionary<string, IList<string>>();
            internal readonly Dictionary<string, IList<ItemPath>> Painted = new Dictionary<string, IList<ItemPath>>();
        }
    }
}
