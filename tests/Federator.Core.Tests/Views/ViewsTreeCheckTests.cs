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
            return TestViewPlan.For(ClashesOf(lightsHome), new ViewTeams(Map(), null, Codes, Settings), mirrors ?? new string[0], Settings);
        }

        private static ViewClash[] ClashesOf(string lightsHome)
        {
            return new[]
            {
                Clash(Ducts, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashPriority.A, SizeVerdict.Large, 1, 1, "ME", "ST"),
                Clash(Ducts, "Clash2", "BLD-ME-Ducts", "BLD-ST-Columns", ClashPriority.A, SizeVerdict.Small, 2, 2, "ME", "ST"),
                Clash(Lights, "Clash1", "BLD-EL-Lighting Fixtures", "BLD-ST-Floors", ClashPriority.A, null, 3, 3, lightsHome, "ST"),
                Clash(Walls, "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashPriority.B, null, 4, 4, "AR", "ST")
            };
        }

        /// <summary>The same plan with no mirror rule handed to it.</summary>
        private static TestViewPlanOutcome PlanWithNoMirrorRule()
        {
            return TestViewPlan.For(ClashesOf("EL"), new ViewTeams(Map(), null, Codes, Settings), null, Settings);
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

                int index = Next(next, ViewPlace.FolderPath(view.Folders));
                tree.After.Add(new ViewNode(view.Folders, view.Name, false, index, MarkOf(view.Folders, view.Name, Camera), 0, Camera, null, false));
                tree.Written.Add(new WrittenView(view.Folders, view.Name, index, true, true));
                tree.Hidden[ReadBackKey(view)] = FileNames(ShownModels.For(view.Pair, Models(), view.Homes).Hidden);
                tree.Painted[ReadBackKey(view)] = new List<ItemPath>(PaintPlan.For(view.Clashes).Solid);
            }

            return tree;
        }

        /// <summary>The key the add-in keeps a view's read backs under, the one a planned view gives.</summary>
        private static string ReadBackKey(PlannedTestView view)
        {
            return view.Key;
        }

        /// <summary>The planned view at that written place.</summary>
        private static PlannedTestView Planned(Tree tree, string place)
        {
            foreach (PlannedTestView view in tree.Plan.Views)
            {
                if (view.ToString() == place)
                {
                    return view;
                }
            }

            throw new InvalidOperationException("no planned view at " + place);
        }

        private static int Next(Dictionary<string, int> next, string parent)
        {
            int index = next.ContainsKey(parent) ? next[parent] : 0;
            next[parent] = index + 1;
            return index;
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
            tree.Hidden[ReadBackKey(Planned(tree, "A/Structure vs Mechanical/" + Ducts))] = new List<string> { Model("EL").FileName };

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

        /// <summary>
        /// F114 attempt 3, the breaker's finding 0 of attempt 2. This test asserted the fault: with
        /// nothing read back check 3 tested the plan against the plan and was counted as holding.
        /// It did not run, and says so.
        /// </summary>
        [Test]
        public void Check3DidNotRunWhereNothingWasReadBack()
        {
            Tree tree = Good();
            tree.Hidden.Clear();
            ViewsTreeFacts facts = FactsOf(tree);
            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);
            string block = Joined(ViewsTree.Lines(facts, checks, 0));

            Assert.That(checks[2].Ran, Is.False);
            Assert.That(checks[2].Holds, Is.False);
            Assert.That(block, Does.Contain("\nCHECK 3  no view shows a model of a third team  DID NOT RUN, the hidden models of none of the 4 views were read back"));
            Assert.That(block, Does.Contain("\n6 of 7 hold, 1 did not run and is not counted as holding"));
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
            tree.Painted[ReadBackKey(Planned(tree, "B/Architecture vs Structure/" + Walls))] = new List<ItemPath> { new ItemPath(new[] { 0, 4 }) };

            ViewsTreeCheck check = Check(FactsOf(tree), 4);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain(Walls));
        }

        [Test]
        public void Check5NamesAMirrorThatWasRun()
        {
            ViewsTreeFacts facts = FactsOf(Good("EL", new[] { Walls }));

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
            facts.Plan = PlanWithNoMirrorRule();
            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);
            ViewsTreeCheck check = checks[4];
            string block = Joined(ViewsTree.Lines(facts, checks, 0));

            Assert.That(check.Ran, Is.False);
            Assert.That(check.Holds, Is.False);
            Assert.That(check.NotRunWhy, Is.EqualTo("no mirror rule was handed to the plan, so whether a mirrored test ran or has a view is UNKNOWN"));
            Assert.That(block, Does.Contain("\nCHECK 5  no mirrored test is run or has a view  DID NOT RUN, no mirror rule"));
            Assert.That(block, Does.Contain("\n6 of 7 hold, 1 did not run"));
            Assert.That(block, Does.Not.Contain("7 of 7 hold"));
            Assert.That(block, Does.Not.Contain("FAILED"));
        }

        /// <summary>
        /// F114 attempt 3, the breaker's finding 3 of attempt 2. This replaces attempt 2's test of a
        /// view the plan made for a mirror: the plan now takes the one mirror list, so it cannot
        /// make one, and reading its views tested the plan against itself. A mirror not run this
        /// week keeps this tool's view of an earlier run in the tree, and check 5 reads the walk.
        /// </summary>
        [Test]
        public void Check5NamesAMirrorWhoseViewOfAnEarlierRunIsStillInTheTree()
        {
            Tree tree = Good("EL", new[] { Walls });
            string[] folders = { "B", "Architecture vs Structure" };
            string earlier = ToolViewMark.StampOf(new DateTime(2026, 9, 28, 10, 0, 0, DateTimeKind.Utc));
            tree.After.Add(new ViewNode(folders, Walls, false, 0,
                new[] { ToolViewMark.Body(earlier, folders, Walls, Camera, null, Settings) }, 0, Camera, null, false));
            ViewsTreeFacts facts = FactsOf(tree);
            facts.TestsRun = new List<string> { Ducts, Lights };

            ViewsTreeCheck check = Check(facts, 5);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Failures), Does.Contain("B/Architecture vs Structure/" + Walls + " is a view this tool made of a mirror"));
        }

        /// <summary>The plan takes the mirrors, so no mirrored test gets a view, Bader's point that there are no mirrored tests.</summary>
        [Test]
        public void Check5HoldsWhenThePlanTookTheMirrors()
        {
            ViewsTreeFacts facts = FactsOf(Good("EL", new[] { Walls }));
            facts.TestsRun = new List<string> { Ducts, Lights };

            ViewsTreeCheck check = Check(facts, 5);

            Assert.That(check.Ran, Is.True);
            Assert.That(check.Holds, Is.True, Joined(check.Failures));
            Assert.That(check.Basis, Is.EqualTo("1 mirrors against the 2 tests the clash step ran and the 4 views of the walk after"));
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
        /// The breaker's finding 2 of attempt 2, and its blocking finding of attempt 3. A clashing
        /// item whose model could not be read, or whose home names no model of the group, leaves
        /// what the view shows UNKNOWN, so check 3 did not run for that view. This test asserted
        /// that such a view holds, which counted UNKNOWN as holding.
        /// </summary>
        [Test]
        public void Check3RanInPartForAClashingItemWhoseModelIsUnknown()
        {
            ViewClash[] clashes =
            {
                new ViewClash(Walls, "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.B, null,
                    new ItemPath(new[] { 0, 4 }), new ItemPath(new[] { 1, 4 }), Camera, null, "1A02MM-XX.nwc")
            };
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Plan = TestViewPlan.For(clashes, new ViewTeams(Map(), null, Codes, Settings), new string[0], Settings);

            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);
            ViewsTreeCheck check = checks[2];
            string place = "B/Architecture vs Structure/" + Walls;

            Assert.That(check.Ran, Is.True);
            Assert.That(check.Holds, Is.False);
            Assert.That(check.Failures, Is.Empty);
            Assert.That(check.Basis, Does.Contain("1 views, their hidden models read back off the document"));
            Assert.That(check.NotRead, Is.EqualTo(new[]
            {
                place + ", the model of 1 of its clashing items could not be read, so whether that model is shown is UNKNOWN",
                place + ", a clashing item lives in 1A02MM-XX.nwc, whose file name is no model of this group, so whether that model is shown is UNKNOWN"
            }));
            Assert.That(Joined(ViewsTree.Lines(facts, checks, 0)),
                Does.Contain("\nCHECK 3  no view shows a model of a third team  RAN IN PART, 0 broke it"));
        }

        /// <summary>
        /// The breaker's blocking finding of attempt 3, the input it named: every home naming no
        /// model of the group, so every view hides every model. The read back agrees with the plan
        /// and nothing is a third team, and check 3 held over a tree of blank views. Each home that
        /// matches no model is one check 3 did not run for.
        /// </summary>
        [Test]
        public void Check3DoesNotHoldOverBlankViewsWhoseHomesMatchNoModel()
        {
            Tree tree = Good();
            ViewsTreeFacts facts = FactsOf(tree);
            List<ModelTeam> renamed = new List<ModelTeam>();

            foreach (ModelTeam model in Models())
            {
                renamed.Add(new ModelTeam("Federated " + model.FileName, model.Code, model.Team));
            }

            facts.Models = renamed;

            foreach (PlannedTestView view in tree.Plan.Views)
            {
                tree.Hidden[ReadBackKey(view)] = FileNames(renamed);
            }

            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);
            string block = Joined(ViewsTree.Lines(facts, checks, 0));

            Assert.That(checks[2].Holds, Is.False);
            Assert.That(checks[2].NotRead.Count, Is.EqualTo(8), "two homes in each of the four views: " + Joined(checks[2].NotRead));
            Assert.That(block, Does.Not.Contain("7 of 7 hold"));
            Assert.That(block, Does.Contain("\n6 of 7 hold, 1 ran in part and is not counted as holding"));
        }

        /// <summary>
        /// The reviewer's blocking finding of attempt 3: the homes of a view were gathered in the
        /// tree line and again in check 3. They are the view's own, PlannedTestView.Homes, each
        /// clash's first home then its second, in the order the clashes were read.
        /// </summary>
        [Test]
        public void TheHomesOfAViewAreEachClashsFirstThenSecondHome()
        {
            PlannedTestView ducts = Planned(Good(), "A/Structure vs Mechanical/" + Ducts);

            Assert.That(ducts.Homes, Is.EqualTo(new[] { Model("ME").FileName, Model("ST").FileName }));
        }

        /// <summary>
        /// The breaker's blocking finding of attempt 3, one rule for how a name is matched to a
        /// model: homes and hidden read backs written as paths match their models by file name, in
        /// the tree line and in check 3 alike, so a view of ME and ST shows ME and ST in both.
        /// </summary>
        [Test]
        public void HomesAndReadBacksWrittenAsPathsAreMatchedByFileNameInTheTreeAndCheck3()
        {
            string folder = "C:/Projects/1A02MM/";
            ViewClash[] clashes =
            {
                new ViewClash(Ducts, "Clash2", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.A, SizeVerdict.Small,
                    new ItemPath(new[] { 0, 2 }), new ItemPath(new[] { 1, 2 }), Camera, folder + Model("ME").FileName, folder + Model("ST").FileName)
            };
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Plan = TestViewPlan.For(clashes, new ViewTeams(Map(), null, Codes, Settings), new string[0], Settings);
            string key = ReadBackKey(facts.Plan.Views[0]);

            facts.HiddenReadBack = new Dictionary<string, IList<string>>();
            string plannedLine = Joined(ViewsTree.Lines(facts, ViewsTreeCheck.Of(facts), 0));

            facts.HiddenReadBack[key] = new List<string> { folder + Model("AR").FileName, folder + Model("EL").FileName };
            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);
            string readLine = Joined(ViewsTree.Lines(facts, checks, 0));

            Assert.That(plannedLine, Does.Contain(Ducts + "  1 open clashes, shows ME ST, hides AR EL"));
            Assert.That(readLine, Does.Contain(Ducts + "  1 open clashes, shows ME ST, hides AR EL"));
            Assert.That(checks[2].Holds, Is.True, Joined(checks[2].Failures) + Joined(checks[2].NotRead));
            Assert.That(checks[2].Notes, Is.Empty);
        }

        /// <summary>
        /// F114 attempt 5, the breaker's blocking finding of attempt 4, the hidden side of the
        /// homes finding of attempt 3. A hidden model read back whose name is no model of the group
        /// was read as nothing hidden, so every model counted as shown, and where none was of a
        /// third team check 3 held. Where one was, it failed on a model the read back may well
        /// hide. Such a view is one check 3 did not run for, named, and is not judged.
        /// </summary>
        [Test]
        public void Check3RanInPartWhereAHiddenModelReadBackIsNoModelOfTheGroup()
        {
            ViewClash[] clashes = { Clash(Ducts, "Clash2", "BLD-ME-Ducts", "BLD-ST-Columns", ClashPriority.A, SizeVerdict.Small, 2, 2, "ME", "ST") };
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Plan = TestViewPlan.For(clashes, new ViewTeams(Map(), null, Codes, Settings), new string[0], Settings);
            facts.Models = new List<ModelTeam> { Model("ME"), Model("ST"), new ModelTeam("1A02MM-ME2.nwc", "ME", Map().TeamOf("ME")) };
            facts.HiddenReadBack = new Dictionary<string, IList<string>> { { ReadBackKey(facts.Plan.Views[0]), new List<string> { "Level 1.5 ME2" } } };
            string place = facts.Plan.Views[0].ToString();

            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);
            string block = Joined(ViewsTree.Lines(facts, checks, 0));

            Assert.That(checks[2].Holds, Is.False, "the read back hid a name of no model, and check 3 held");
            Assert.That(checks[2].NotRead, Is.EqualTo(new[]
            {
                place + ", it reads back Level 1.5 ME2 as hidden, whose file name is no model of this group, so what it hides is UNKNOWN"
            }));
            Assert.That(block, Does.Contain(Ducts + "  1 open clashes, shows ME ST, hides nothing, 1 name read back as hidden tied to no one model"));

            facts.Models = Models();
            ViewsTreeCheck withAThirdTeam = Check(facts, 3);

            Assert.That(withAThirdTeam.Failures, Is.Empty, "a model the read back may hide is not named as shown");
            Assert.That(withAThirdTeam.Holds, Is.False);
        }

        /// <summary>F114 attempt 5: a hidden model read back with no name was passed over, so what the view hides is UNKNOWN.</summary>
        [Test]
        public void Check3RanInPartWhereAHiddenModelReadBackHasNoName()
        {
            Tree tree = Good();
            string walls = "B/Architecture vs Structure/" + Walls;
            tree.Hidden[ReadBackKey(Planned(tree, walls))] = new List<string> { string.Empty, Model("EL").FileName, null, Model("ME").FileName };

            ViewsTreeCheck check = Check(FactsOf(tree), 3);

            Assert.That(check.Holds, Is.False);
            Assert.That(check.Failures, Is.Empty);
            Assert.That(check.NotRead, Is.EqualTo(new[] { walls + ", 2 of the hidden models it reads back have no name, so what it hides is UNKNOWN" }));
        }

        /// <summary>
        /// F114 attempt 5, the breaker's finding 2 of attempt 4. With subfolders ticked a group can
        /// hold one file name in two folders, and the one rule of what a name is, its stem, ties a
        /// home or a hidden name to both. Which model is meant is UNKNOWN, so a view with such a
        /// home or such a hidden name is one check 3 did not run for, named.
        /// </summary>
        [Test]
        public void Check3RanInPartWhereAHomeOrAHiddenNameIsTheNameOfTwoModels()
        {
            List<ModelTeam> models = new List<ModelTeam>
            {
                Model("AR"), Model("EL"), Model("ST"),
                new ModelTeam("Current/" + Model("ME").FileName, "ME", Map().TeamOf("ME")),
                new ModelTeam("Old/" + Model("ME").FileName, "ME", Map().TeamOf("ME"))
            };
            Tree tree = Good();

            foreach (PlannedTestView view in tree.Plan.Views)
            {
                tree.Hidden[ReadBackKey(view)] = FileNames(ShownModels.For(view.Pair, models, view.Homes).Hidden);
            }

            ViewsTreeFacts facts = FactsOf(tree);
            facts.Models = models;
            string twice = ", whose file name is the name of more than one model of this group, so ";

            ViewsTreeCheck check = Check(facts, 3);

            Assert.That(check.Holds, Is.False, "a name of two models was tied to both and check 3 held");
            Assert.That(check.Failures, Is.Empty);
            Assert.That(check.NotRead, Is.EqualTo(new[]
            {
                "A/Structure vs Mechanical/" + Ducts + ", a clashing item lives in " + Model("ME").FileName + twice + "which one it lives in is UNKNOWN",
                "A/Structure vs Mechanical/Over 150mm/" + Ducts + ", a clashing item lives in " + Model("ME").FileName + twice + "which one it lives in is UNKNOWN",
                "A/Structure vs Electrical/" + Lights + ", it reads back Current/" + Model("ME").FileName + " as hidden" + twice + "which one it hides is UNKNOWN",
                "B/Architecture vs Structure/" + Walls + ", it reads back Current/" + Model("ME").FileName + " as hidden" + twice + "which one it hides is UNKNOWN"
            }));
        }

        /// <summary>
        /// F114 attempt 5, the breaker's finding 4 of attempt 4. A model whose file name is blank
        /// can be reached by no home and no hidden name, so the read back always counted it as
        /// shown and judged it by its team. It is named once, and check 3 does not hold.
        /// </summary>
        [Test]
        public void Check3NamesAModelWithNoFileNameOnce()
        {
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Models = new List<ModelTeam>(Models()) { new ModelTeam(" ", "ST", Map().TeamOf("ST")) };

            ViewsTreeCheck check = Check(facts, 3);

            Assert.That(check.Holds, Is.False, "a model no name can reach was counted as shown and held");
            Assert.That(check.Failures, Is.Empty);
            Assert.That(check.NotRead, Is.EqualTo(new[]
            {
                "a model of this group of code ST has no file name, so no clashing item and no hidden model read back can be tied to it"
            }));
        }

        /// <summary>
        /// F114 attempt 5, the painted side of the same class. A painted read back holds item paths
        /// and no name, so it is tied to a clash by the path. A clashing item that could not be
        /// pointed at is in no paint plan, so check 4 held over a view whose paint of that item is
        /// UNKNOWN. That view is one check 4 did not run for, named.
        /// </summary>
        [Test]
        public void Check4RanInPartWhereAClashingItemCouldNotBePointedAt()
        {
            ViewClash[] clashes =
            {
                new ViewClash(Walls, "Clash1", "BLD-AR-Walls", "BLD-ST-Columns", ClashStatus.New, ClashPriority.B, null,
                    new ItemPath(new[] { 0, 4 }), null, Camera, Model("AR").FileName, Model("ST").FileName)
            };
            ViewsTreeFacts facts = FactsOf(Good());
            facts.Plan = TestViewPlan.For(clashes, new ViewTeams(Map(), null, Codes, Settings), new string[0], Settings);
            PlannedTestView walls = facts.Plan.Views[0];
            facts.PaintedReadBack = new Dictionary<string, IList<ItemPath>> { { ReadBackKey(walls), new List<ItemPath>(PaintPlan.For(walls.Clashes).Solid) } };

            ViewsTreeCheck check = Check(facts, 4);

            Assert.That(check.Holds, Is.False, "an item not pointed at was left out of the paint and check 4 held");
            Assert.That(check.Failures, Is.Empty);
            Assert.That(check.NotRead, Is.EqualTo(new[]
            {
                walls + ", 1 of its clashes has an item that could not be pointed at, so whether it is painted is UNKNOWN"
            }));
        }

        /// <summary>
        /// F114 attempt 5: an item read back as painted that could not be pointed at, a null in the
        /// list, was counted as painted and not of the view's clashes, a failure the read back does
        /// not show. Which item it is is UNKNOWN, so the view is one check 4 did not run for.
        /// </summary>
        [Test]
        public void Check4NamesAPaintedItemReadBackWithNoPathAsNotRead()
        {
            Tree tree = Good();
            string walls = "B/Architecture vs Structure/" + Walls;
            tree.Painted[ReadBackKey(Planned(tree, walls))].Add(null);

            ViewsTreeCheck check = Check(FactsOf(tree), 4);

            Assert.That(check.Holds, Is.False);
            Assert.That(check.Failures, Is.Empty, "a null read back is not an item painted by mistake");
            Assert.That(check.NotRead, Is.EqualTo(new[]
            {
                walls + ", 1 of the items it reads back as painted could not be pointed at, so whether its paint is right is UNKNOWN"
            }));
        }

        /// <summary>
        /// The breaker's finding 2 of attempt 3, a crash on a documented input. ViewsTreeFacts says
        /// a read back is null where not read, and the tree line read a null one as read and threw,
        /// and a null model threw in the models line. A null read back is one not read, in the tree
        /// line and the checks alike, and a null model is left out as ShownModels leaves it out.
        /// </summary>
        [Test]
        public void ANullReadBackIsNotReadAndANullModelIsLeftOutAndNeitherStopsTheBlock()
        {
            Tree tree = Good();
            string walls = "B/Architecture vs Structure/" + Walls;
            tree.Hidden[ReadBackKey(Planned(tree, walls))] = null;
            ViewsTreeFacts facts = FactsOf(tree);
            facts.Models = new List<ModelTeam>(Models()) { null };

            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);
            string block = Joined(ViewsTree.Lines(facts, checks, 0));

            Assert.That(Joined(checks[2].NotRead), Is.EqualTo(walls + ", its hidden models were not read back"));
            Assert.That(block, Does.Contain("models     : AR Architecture, EL Electrical, ME Mechanical, ST Structure\n"));
            Assert.That(block, Does.Contain("\n    " + Walls + "  1 open clashes, shows AR ST, hides EL ME"));
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

        // ---------- F114 attempt 3, a read back missing for a view is a check that did not run for it ----------

        /// <summary>
        /// The breaker's finding 0 of attempt 2. A view whose hidden models were not read back was
        /// tested against the plan's own list, which cannot fail. Check 3 did not run for it.
        /// </summary>
        [Test]
        public void Check3RanInPartWhereOneViewWasNotReadBackAndIsNotCountedAsHolding()
        {
            Tree tree = Good();
            tree.Hidden.Remove(ReadBackKey(Planned(tree, "A/Structure vs Electrical/" + Lights)));
            ViewsTreeFacts facts = FactsOf(tree);
            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);
            string block = Joined(ViewsTree.Lines(facts, checks, 0));

            Assert.That(checks[2].Holds, Is.False);
            Assert.That(block, Does.Contain("\nCHECK 3  no view shows a model of a third team  RAN IN PART, 0 broke it, 3 of 4 views"));
            Assert.That(block, Does.Contain("\n    not read: A/Structure vs Electrical/" + Lights + ", its hidden models were not read back"));
            Assert.That(block, Does.Contain("\n6 of 7 hold, 1 ran in part and is not counted as holding"));
        }

        /// <summary>
        /// The breaker's finding 0 of attempt 2, its second input, with its finding 5: read backs
        /// kept under the written place, which two views can share, or any key but the one a
        /// planned view gives, all miss. Check 3 did not run and says how many missed.
        /// </summary>
        [Test]
        public void ReadBacksKeptUnderTheWrittenPlaceAreNotReadAndAreCounted()
        {
            Tree tree = Good();
            ViewsTreeFacts facts = FactsOf(tree);
            Dictionary<string, IList<string>> byWrittenPlace = new Dictionary<string, IList<string>>();

            foreach (PlannedTestView view in tree.Plan.Views)
            {
                byWrittenPlace[view.ToString()] = FileNames(ShownModels.For(view.Pair, Models(), view.Homes).Hidden);
            }

            facts.HiddenReadBack = byWrittenPlace;
            ViewsTreeCheck check = Check(facts, 3);

            Assert.That(check.Ran, Is.False);
            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.Notes), Does.Contain("4 hidden read backs were handed in under a key no planned view gives, so none of them was read"));
        }

        /// <summary>The breaker's finding 0 of attempt 2, check 4: the painted half did not run for a view not read back.</summary>
        [Test]
        public void Check4RanInPartWhereNoPaintWasReadBack()
        {
            Tree tree = Good();
            tree.Painted.Clear();
            ViewsTreeFacts facts = FactsOf(tree);
            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);
            string block = Joined(ViewsTree.Lines(facts, checks, 0));

            Assert.That(checks[3].Holds, Is.False);
            Assert.That(block, Does.Contain("\nCHECK 4  no clash is in two views  RAN IN PART, 0 broke it"));
            Assert.That(block, Does.Contain("\n    not read: the painted items of all 4 views, none was read back"));
            Assert.That(block, Does.Contain("\n6 of 7 hold, 1 ran in part"));
        }

        [Test]
        public void Check4NamesTheViewWhosePaintWasNotReadBack()
        {
            Tree tree = Good();
            tree.Painted.Remove(ReadBackKey(Planned(tree, "B/Architecture vs Structure/" + Walls)));

            ViewsTreeCheck check = Check(FactsOf(tree), 4);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.NotRead), Is.EqualTo("B/Architecture vs Structure/" + Walls + ", its painted items were not read back"));
        }

        /// <summary>
        /// The reviewer's finding 2 and the breaker's finding 1 of attempt 2: checks 1 and 2 held
        /// once one planned view of many was found marked. A planned view not found is one they
        /// did not run for.
        /// </summary>
        [Test]
        public void ChecksOneAndTwoRanInPartWhereAPlannedViewIsNotFoundMarked()
        {
            Tree tree = Good();
            ViewsTreeFacts facts = FactsOf(tree);
            facts.After = tree.After.FindAll(node => node.Name != Walls);
            IList<ViewsTreeCheck> checks = ViewsTreeCheck.Of(facts);

            foreach (int number in new[] { 1, 2 })
            {
                Assert.That(checks[number - 1].Ran, Is.True, "check " + number);
                Assert.That(checks[number - 1].Holds, Is.False, "check " + number);
                Assert.That(Joined(checks[number - 1].NotRead),
                    Is.EqualTo("B/Architecture vs Structure/" + Walls + ", not found marked by this run in the walk after"), "check " + number);
            }
        }

        /// <summary>With a mirror to look for, check 5 run without the tests run or the walk after is a check that did not run for them.</summary>
        [Test]
        public void Check5RanInPartWithAMirrorAndNoTestsRunOrNoWalk()
        {
            ViewsTreeFacts facts = FactsOf(Good("EL", new[] { Walls }));
            facts.TestsRun = null;
            facts.After = null;

            ViewsTreeCheck check = Check(facts, 5);

            Assert.That(check.Ran, Is.True);
            Assert.That(check.Holds, Is.False);
            Assert.That(check.NotRead, Is.EqualTo(new[]
            {
                "the tests the clash step ran, which were not handed in",
                "the walk of the tree after, which was not handed in"
            }));
        }

        /// <summary>Q119 B with Q118 A: a view shows only its homes, so a model with no code it shows is one whose team is UNKNOWN.</summary>
        [Test]
        public void Check3DidNotRunForAViewShowingAModelWhoseCodeWillNotRead()
        {
            Tree tree = Good(string.Empty);
            List<ModelTeam> models = new List<ModelTeam>(Models()) { Model(string.Empty) };

            foreach (PlannedTestView view in tree.Plan.Views)
            {
                tree.Hidden[ReadBackKey(view)] = FileNames(ShownModels.For(view.Pair, models, view.Homes).Hidden);
            }

            ViewsTreeFacts facts = FactsOf(tree);
            facts.Models = models;

            ViewsTreeCheck check = Check(facts, 3);

            Assert.That(check.Holds, Is.False);
            Assert.That(Joined(check.NotRead), Is.EqualTo("A/Structure vs Electrical/" + Lights + ", it shows "
                + Model(string.Empty).FileName + " whose code will not read, so whether that is a third team is UNKNOWN"));
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
            ViewsTreeFacts facts = FactsOf(Good("EL", new[] { Walls }));
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
