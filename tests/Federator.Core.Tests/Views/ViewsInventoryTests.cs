using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F114, Q114 points 16 and 17, with the design's four safety rules. The views are made
    /// fresh every run, and only what this tool made is removed or replaced. A view this run
    /// wrote replaces the tool's earlier one only once it is marked and read back (S2), nothing
    /// goes when the clash step was not sound (S4), and a person's view, or one the tool made
    /// that a person changed, is never touched wherever it sits (S1). Each removal is a parent
    /// and an index, deepest first and latest index first, because a removal shifts the indexes
    /// after it, 5z. The names are sample data only.
    /// </summary>
    [TestFixture]
    public class ViewsInventoryTests
    {
        private const string T = "BLD-ME-Ducts-vs-BLD-ST-Columns";
        private const string Gone = "BLD-AR-Walls-vs-BLD-ST-Columns";
        private const string Pair = "Structure vs Mechanical";

        private static readonly ViewpointSettings Settings = new ViewpointSettings();
        private static readonly string Run = ToolViewMark.StampOf(new DateTime(2026, 10, 5, 10, 0, 0, DateTimeKind.Utc));
        private static readonly string Earlier = ToolViewMark.StampOf(new DateTime(2026, 9, 28, 10, 0, 0, DateTimeKind.Utc));
        private static readonly Point3 Camera = new Point3(1, 2, 3);
        private static readonly string[] Codes = { "AR", "EL", "ME", "ST" };
        private static readonly string[] Here = { "A", Pair };

        private static TestViewPlanOutcome PlanOf(params string[] openTests)
        {
            List<ViewClash> clashes = new List<ViewClash>();

            foreach (string test in openTests)
            {
                clashes.Add(new ViewClash(test, "Clash1", "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New,
                    ClashPriority.A, null, null, null, null, null, null));
            }

            ViewTeams teams = new ViewTeams(TeamMapTests.MapOf(TeamMapTests.BadersMap), Codes, Settings);
            return TestViewPlan.For(clashes, teams, Settings);
        }

        private static string[] Mark(string stamp, string[] folders, string name, Point3 camera)
        {
            return new[] { ToolViewMark.Body(stamp, string.Join("/", folders), name, camera, null, Settings) };
        }

        private static ViewNode View(string[] folders, string name, int index, string[] comments = null, Point3 camera = null)
        {
            return new ViewNode(folders, name, false, index, comments, 0, camera ?? Camera, null, false);
        }

        private static ViewNode Ours(string stamp, string[] folders, string name, int index)
        {
            return View(folders, name, index, Mark(stamp, folders, name, Camera));
        }

        private static ViewNode Folder(string[] folders, string name, int index, string[] comments = null, bool emptyBefore = false)
        {
            return new ViewNode(folders, name, true, index, comments, 0, null, null, emptyBefore);
        }

        private static ViewNode OurFolder(string stamp, string[] folders, string name, int index)
        {
            return Folder(folders, name, index, Mark(stamp, folders, name, null));
        }

        private static List<ViewNode> TreeWithTheNewView()
        {
            return new List<ViewNode>
            {
                OurFolder(Earlier, new string[0], "A", 0),
                OurFolder(Earlier, new[] { "A" }, Pair, 0),
                Ours(Earlier, Here, T, 0),
                Ours(Run, Here, T, 1)
            };
        }

        private static ViewsInventory Inventory(
            IList<ViewNode> tree,
            TestViewPlanOutcome plan,
            IList<WrittenView> written,
            ICollection<string> read = null,
            bool sound = true,
            ViewpointSettings settings = null)
        {
            return ViewsInventory.Plan(
                tree, plan, read ?? new List<string> { T, Gone }, written, Run, sound, Codes,
                new List<string> { T, Gone }, settings ?? Settings);
        }

        private static List<WrittenView> WrittenOk()
        {
            return new List<WrittenView> { new WrittenView(Here, T, 1, true, true) };
        }

        private static InventoryDecision DecisionOf(ViewsInventory inventory, ViewNode node)
        {
            foreach (InventoryItem item in inventory.Items)
            {
                if (ReferenceEquals(item.Node, node))
                {
                    return item.Decision;
                }
            }

            throw new InvalidOperationException("the inventory left out " + node);
        }

        // ---------- one test per decision ----------

        [Test]
        public void TheToolsEarlierViewReplacedThisRunIsRemovedAndTheNewOneKept()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            ViewsInventory inventory = Inventory(tree, PlanOf(T), WrittenOk());

            Assert.That(DecisionOf(inventory, tree[2]), Is.EqualTo(InventoryDecision.RemoveReplaced));
            Assert.That(DecisionOf(inventory, tree[3]), Is.EqualTo(InventoryDecision.KeepWrittenThisRun));
            Assert.That(DecisionOf(inventory, tree[1]), Is.EqualTo(InventoryDecision.KeepFolder), "it holds the new view");
            Assert.That(DecisionOf(inventory, tree[0]), Is.EqualTo(InventoryDecision.KeepFolder));
            Assert.That(inventory.Removals.Count, Is.EqualTo(1));
        }

        /// <summary>S2: a view this run wrote that did not read back is removed at once, and the earlier one it was to replace stays.</summary>
        [Test]
        public void AViewThisRunCouldNotReadBackGoesAtOnceAndTheEarlierOneStays()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            ViewsInventory inventory = Inventory(tree, PlanOf(T), new List<WrittenView> { new WrittenView(Here, T, 1, true, false) });

            Assert.That(DecisionOf(inventory, tree[3]), Is.EqualTo(InventoryDecision.RemoveAtOnce));
            Assert.That(DecisionOf(inventory, tree[2]), Is.EqualTo(InventoryDecision.KeepReplacementFailed));
        }

        /// <summary>A view this run wrote and could not mark carries no mark, and the run still knows it as its own by where it put it.</summary>
        [Test]
        public void AViewThisRunCouldNotMarkIsKnownByWhereItWentAndGoesAtOnce()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            tree[3] = View(Here, T, 1);
            ViewsInventory inventory = Inventory(tree, PlanOf(T), new List<WrittenView> { new WrittenView(Here, T, 1, false, true) });

            Assert.That(DecisionOf(inventory, tree[3]), Is.EqualTo(InventoryDecision.RemoveAtOnce));
            Assert.That(DecisionOf(inventory, tree[2]), Is.EqualTo(InventoryDecision.KeepReplacementFailed));
        }

        /// <summary>S4: a test whose results were not read this run keeps its old view.</summary>
        [Test]
        public void TheToolsViewOfATestNotReadThisRunIsKept()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            ViewsInventory inventory = Inventory(tree, PlanOf(), new List<WrittenView>(), new List<string> { Gone });

            Assert.That(DecisionOf(inventory, tree[2]), Is.EqualTo(InventoryDecision.KeepTestNotRead));
        }

        /// <summary>A test read this run whose view now sits elsewhere, or that needs none, loses the tool's view at the old place.</summary>
        [Test]
        public void TheToolsViewOfATestReadThatNeedsNoViewThereIsRemoved()
        {
            string[] there = { "B", Pair };
            List<ViewNode> tree = TreeWithTheNewView();
            tree.Add(OurFolder(Earlier, new string[0], "B", 1));
            tree.Add(OurFolder(Earlier, new[] { "B" }, Pair, 0));
            tree.Add(Ours(Earlier, there, T, 0));
            tree.Add(Ours(Earlier, there, Gone, 1));

            ViewsInventory inventory = Inventory(tree, PlanOf(T), WrittenOk());

            Assert.That(DecisionOf(inventory, tree[6]), Is.EqualTo(InventoryDecision.RemoveNoLongerNeeded), "its test's view moved to A");
            Assert.That(DecisionOf(inventory, tree[7]), Is.EqualTo(InventoryDecision.RemoveNoLongerNeeded), "its test has no open clash now");
            Assert.That(DecisionOf(inventory, tree[5]), Is.EqualTo(InventoryDecision.RemoveFolder));
            Assert.That(DecisionOf(inventory, tree[4]), Is.EqualTo(InventoryDecision.RemoveFolder));
        }

        [Test]
        public void APersonsViewAndTheViewsTheNwcsBroughtAreKept()
        {
            List<ViewNode> tree = new List<ViewNode>();

            for (int i = 0; i < 34; i++)
            {
                tree.Add(View(new string[0], "Level " + i, i));
            }

            tree.Add(View(new string[0], "Rami's view of the plant room", 34, new[] { "look here" }));

            ViewsInventory inventory = Inventory(tree, PlanOf(T), new List<WrittenView>());

            foreach (ViewNode node in tree)
            {
                Assert.That(DecisionOf(inventory, node), Is.EqualTo(InventoryDecision.KeepNotOurs), node.ToString());
            }

            Assert.That(inventory.Removals, Is.Empty);
        }

        [Test]
        public void AViewAPersonChangedIsKeptAndNamedWhereverItSits()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            ViewNode renamed = View(Here, T + " for Monday", 2, Mark(Earlier, Here, T, Camera));
            ViewNode moved = View(new[] { "My views" }, T, 0, Mark(Earlier, Here, T, Camera));
            tree.Add(renamed);
            tree.Add(moved);

            ViewsInventory inventory = Inventory(tree, PlanOf(T), WrittenOk());

            Assert.That(DecisionOf(inventory, renamed), Is.EqualTo(InventoryDecision.KeepChangedByAPerson));
            Assert.That(DecisionOf(inventory, moved), Is.EqualTo(InventoryDecision.KeepChangedByAPerson));
            Assert.That(string.Join("\n", new List<string>(inventory.Lines()).ToArray()), Does.Contain("My views/" + T));
        }

        // ---------- the per clash viewpoints of earlier runs ----------

        [Test]
        public void APerClashViewpointGoesOnceTheNewTreeIsWholeAndStaysOtherwise()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            ViewNode legacy = View(new[] { "ME vs ST" }, T + "  Clash4", 0);
            tree.Add(Folder(new string[0], "ME vs ST", 1));
            tree.Add(legacy);

            Assert.That(DecisionOf(Inventory(tree, PlanOf(T), WrittenOk()), legacy), Is.EqualTo(InventoryDecision.RemoveLegacy));
            Assert.That(
                DecisionOf(Inventory(tree, PlanOf(T), new List<WrittenView> { new WrittenView(Here, T, 1, true, false) }), legacy),
                Is.EqualTo(InventoryDecision.KeepReplacementFailed));
            Assert.That(
                DecisionOf(Inventory(tree, PlanOf(T), WrittenOk(), new List<string>()), legacy),
                Is.EqualTo(InventoryDecision.KeepTestNotRead));
        }

        /// <summary>The 2617 per clash viewpoints the baseline put in one folder go in one call when P14 says a folder goes with its views.</summary>
        [Test]
        public void AFolderOfPerClashViewpointsIsOneRemovalWhenAFolderGoesWithItsViews()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            tree.Add(Folder(new string[0], "ME vs ST", 1));

            for (int i = 0; i < 2617; i++)
            {
                tree.Add(View(new[] { "ME vs ST" }, T + "  Clash" + (i + 1), i));
            }

            ViewpointSettings oneCall = new ViewpointSettings { FolderGoesWithChildren = true };
            ViewsInventory together = Inventory(tree, PlanOf(T), WrittenOk(), settings: oneCall);
            ViewsInventory apart = Inventory(tree, PlanOf(T), WrittenOk());

            Assert.That(together.Removals.Count, Is.EqualTo(2), "the replaced view and the folder");
            Assert.That(together.Removals[1].Node.Name, Is.EqualTo("ME vs ST"));
            Assert.That(apart.Removals.Count, Is.EqualTo(2619));
            Assert.That(apart.Removals[0].Node.Name, Is.EqualTo(T), "the deepest first, two folders down");
            Assert.That(apart.Removals[1].Node.IndexInParent, Is.EqualTo(2616), "the latest index first");
            Assert.That(apart.Removals[2617].Node.IndexInParent, Is.EqualTo(0));
            Assert.That(apart.Removals[2618].Node.Name, Is.EqualTo("ME vs ST"), "the folder after its views");
        }

        // ---------- folders ----------

        [Test]
        public void APersonsViewInsideAToolFolderKeepsItselfAndItsFolder()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            ViewNode folder = Folder(new string[0], "ME vs ST", 1);
            ViewNode legacy = View(new[] { "ME vs ST" }, T + "  Clash4", 0);
            ViewNode persons = View(new[] { "ME vs ST" }, "the riser", 1);
            tree.AddRange(new[] { folder, legacy, persons });

            ViewsInventory inventory = Inventory(tree, PlanOf(T), WrittenOk());

            Assert.That(DecisionOf(inventory, legacy), Is.EqualTo(InventoryDecision.RemoveLegacy));
            Assert.That(DecisionOf(inventory, persons), Is.EqualTo(InventoryDecision.KeepNotOurs));
            Assert.That(DecisionOf(inventory, folder), Is.EqualTo(InventoryDecision.KeepFolder));
        }

        /// <summary>A person's folder that happens to be named A loses only the tool's views in it.</summary>
        [Test]
        public void APersonsFolderNamedAKeepsItselfAndLosesOnlyTheToolsViews()
        {
            List<ViewNode> tree = new List<ViewNode>
            {
                Folder(new string[0], "A", 0, new[] { "Rami's A list" }),
                OurFolder(Earlier, new[] { "A" }, Pair, 0),
                Ours(Earlier, Here, T, 0),
                Ours(Run, Here, T, 1),
                View(new[] { "A" }, "Rami's own", 1)
            };

            ViewsInventory inventory = Inventory(tree, PlanOf(T), WrittenOk());

            Assert.That(DecisionOf(inventory, tree[2]), Is.EqualTo(InventoryDecision.RemoveReplaced));
            Assert.That(DecisionOf(inventory, tree[4]), Is.EqualTo(InventoryDecision.KeepNotOurs));
            Assert.That(DecisionOf(inventory, tree[0]), Is.EqualTo(InventoryDecision.KeepFolder));
        }

        [Test]
        public void AFolderOfTheToolsThatWasEmptyBeforeTheRunIsKept()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            ViewNode empty = Folder(new string[0], "C", 1, Mark(Earlier, new string[0], "C", null), true);
            tree.Add(empty);

            Assert.That(DecisionOf(Inventory(tree, PlanOf(T), WrittenOk()), empty), Is.EqualTo(InventoryDecision.KeepFolder));
        }

        /// <summary>An unmarked folder holding anything but per clash viewpoints is not the tool's, and is never removed.</summary>
        [Test]
        public void AFolderTheToolDidNotMakeIsNeverRemoved()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            ViewNode folder = Folder(new string[0], "Old", 1);
            ViewNode view = Ours(Earlier, new[] { "Old" }, Gone, 0);
            tree.Add(folder);
            tree.Add(view);

            ViewsInventory inventory = Inventory(tree, PlanOf(T), WrittenOk());

            Assert.That(DecisionOf(inventory, view), Is.EqualTo(InventoryDecision.RemoveNoLongerNeeded));
            Assert.That(DecisionOf(inventory, folder), Is.EqualTo(InventoryDecision.KeepFolder));
        }

        /// <summary>Two folders of one name side by side cannot be told apart by a parent path, so nothing under that name is removed.</summary>
        [Test]
        public void NothingUnderTwoFoldersOfOneNameSideBySideIsRemoved()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            ViewNode twin = Folder(new string[0], "A", 1, new[] { "Rami's A list" });
            ViewNode inTwin = View(new[] { "A" }, "Rami's own", 1);
            tree.Add(twin);
            tree.Add(inTwin);

            ViewsInventory inventory = Inventory(tree, PlanOf(T), WrittenOk());

            Assert.That(DecisionOf(inventory, tree[2]), Is.EqualTo(InventoryDecision.KeepPlaceNotUnique), "even the replaced view");
            Assert.That(DecisionOf(inventory, twin), Is.EqualTo(InventoryDecision.KeepPlaceNotUnique));
            Assert.That(DecisionOf(inventory, inTwin), Is.EqualTo(InventoryDecision.KeepPlaceNotUnique));
            Assert.That(inventory.Removals, Is.Empty);
        }

        // ---------- S4 and the run on its own result ----------

        [Test]
        public void NothingIsRemovedWhenTheClashStepWasNotSound()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            tree.Add(Folder(new string[0], "ME vs ST", 1));
            tree.Add(View(new[] { "ME vs ST" }, T + "  Clash4", 0));

            ViewsInventory inventory = Inventory(tree, PlanOf(T), WrittenOk(), sound: false);

            Assert.That(inventory.Removals, Is.Empty);
            Assert.That(inventory.Items.Count, Is.EqualTo(tree.Count), "every item is still judged and said");

            foreach (InventoryItem item in inventory.Items)
            {
                Assert.That(item.Decision, Is.EqualTo(InventoryDecision.KeepClashStepNotSound), item.Node.ToString());
            }
        }

        [Test]
        public void ThePlanRunOnItsOwnResultRemovesNothing()
        {
            List<ViewNode> tree = TreeWithTheNewView();
            tree.Add(Folder(new string[0], "ME vs ST", 1));
            tree.Add(View(new[] { "ME vs ST" }, T + "  Clash4", 0));
            tree.Add(View(new[] { "ME vs ST" }, "the riser", 1));
            tree.Add(View(new string[0], "Level 1", 2));

            ViewsInventory first = Inventory(tree, PlanOf(T), WrittenOk());
            List<ViewNode> after = Without(tree, first.Removals);
            ViewsInventory second = Inventory(after, PlanOf(T), new List<WrittenView> { new WrittenView(Here, T, 0, true, true) });

            Assert.That(first.Removals.Count, Is.EqualTo(2));
            Assert.That(second.Removals, Is.Empty);
        }

        /// <summary>The tree with those items gone and every index moved down as Navisworks moves it.</summary>
        private static List<ViewNode> Without(IList<ViewNode> tree, IList<InventoryItem> removed)
        {
            HashSet<ViewNode> gone = new HashSet<ViewNode>();

            foreach (InventoryItem item in removed)
            {
                gone.Add(item.Node);
            }

            Dictionary<string, int> next = new Dictionary<string, int>();
            List<ViewNode> left = new List<ViewNode>();

            foreach (ViewNode node in tree)
            {
                if (gone.Contains(node))
                {
                    continue;
                }

                string parent = node.FolderPath;
                int index = next.ContainsKey(parent) ? next[parent] : 0;
                next[parent] = index + 1;

                left.Add(new ViewNode(node.Folders, node.Name, node.IsFolder, index, node.Comments, node.Redlines,
                    node.Camera, node.Guid, node.EmptyBeforeTheRun));
            }

            return left;
        }
    }
}
