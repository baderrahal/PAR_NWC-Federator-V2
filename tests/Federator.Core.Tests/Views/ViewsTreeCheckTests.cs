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

        private static TestViewPlanOutcome PlanOf(string lightsHome = "EL")
        {
            ViewClash[] clashes =
            {
                Clash(Ducts, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashPriority.A, SizeVerdict.Large, 1, 1, "ME", "ST"),
                Clash(Ducts, "Clash2", "BLD-ME-Ducts", "BLD-ST-Columns", ClashPriority.A, SizeVerdict.Small, 2, 2, "ME", "ST"),
                Clash(Lights, "Clash1", "BLD-EL-Lighting Fixtures", "BLD-ST-Floors", ClashPriority.A, null, 3, 3, lightsHome, "ST"),
                Clash(Walls, "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashPriority.B, null, 4, 4, "AR", "ST")
            };

            return TestViewPlan.For(clashes, new ViewTeams(Map(), Codes, Settings), Settings);
        }

        private static string[] MarkOf(IList<string> folders, string name, Point3 camera)
        {
            return new[] { ToolViewMark.Body(Run, string.Join("/", new List<string>(folders).ToArray()), name, camera, null, Settings) };
        }

        /// <summary>The tree as this run left it: a person's view at the root, then this run's marked folders and views.</summary>
        private static Tree Good(string lightsHome = "EL")
        {
            Tree tree = new Tree { Plan = PlanOf(lightsHome) };
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

        [Test]
        public void Check5SaysItProvesNothingWithNoMirrorRuleOrNoMirror()
        {
            ViewsTreeFacts facts = FactsOf(Good());

            Assert.That(Joined(Check(facts, 5).Notes), Does.Contain("proves nothing"));

            facts.Mirrors = null;
            Assert.That(Joined(Check(facts, 5).Notes), Does.Contain("UNKNOWN"));
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
