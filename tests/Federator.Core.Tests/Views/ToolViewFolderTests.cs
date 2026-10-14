using System.Collections.Generic;
using System.IO;
using Federator.Core.Clash;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F186, Bader's answer 1 of Q174 and his order of 14 Oct 2026, task 6. Everything the tool makes goes in one top
    /// folder, found by its name and its marker. It is deleted with all it holds on every run and made again, and
    /// nothing outside it is read for removal. A tree of an earlier version is cleaned up once, and only where
    /// every item inside a root folder is the tool's own naming. Names are sample data.
    /// </summary>
    [TestFixture]
    public class ToolViewFolderTests
    {
        private const string Top = "NWC Federator (auto)";
        private const string TestA = "BLD-AR-Walls-vs-BLD-ST-Columns";
        private const string TestB = "BLD-ME-Ducts-vs-BLD-ST-Columns";

        private static readonly ViewpointSettings Settings = new ViewpointSettings();
        private static readonly string[] Teams = { "Architecture", "Structure", "Mechanical", "Electrical", "UNKNOWN" };
        private static readonly string[] Tests = { TestA, TestB };

        private static ViewNode Folder(string[] folders, string name, int index)
        {
            return new ViewNode(folders, name, true, index, null, 0, null, null, false);
        }

        private static ViewNode View(string[] folders, string name, int index)
        {
            return new ViewNode(folders, name, false, index, null, 0, new Point3(1, 2, 3), null, false);
        }

        // ---------- the letters and the marker ----------

        [Test]
        public void TheLettersAreAThroughCAndANoPriorityTestIsC()
        {
            Assert.That(ToolViewFolder.Letters, Is.EqualTo(new[] { "A", "B", "C" }));
            Assert.That(ToolViewFolder.LetterOf(ClashPriority.A), Is.EqualTo("A"));
            Assert.That(ToolViewFolder.LetterOf(ClashPriority.B), Is.EqualTo("B"));
            Assert.That(ToolViewFolder.LetterOf(ClashPriority.C), Is.EqualTo("C"));
            Assert.That(ToolViewFolder.LetterOf(ClashPriority.None), Is.EqualTo("C"));
        }

        [Test]
        public void TheNameIsTheOneBaderNamed()
        {
            Assert.That(ToolViewFolder.Name, Is.EqualTo(Top));
            Assert.That(ToolViewFolder.MarkerBody("r1"), Is.EqualTo("NWCF-AUTO run=r1"));
        }

        // ---------- nothing outside the folder is read ----------

        [Test]
        public void OnlyTheToolFolderAndWhatIsInsideItIsHandedToTheInventoryAndTheChecks()
        {
            List<ViewNode> walk = new List<ViewNode>
            {
                View(new string[0], "Level 1", 0),
                Folder(new string[0], "A", 1),
                View(new[] { "A" }, "Rami's own", 0),
                Folder(new string[0], Top, 2),
                Folder(new[] { Top }, "A", 0),
                View(new[] { Top, "A" }, TestA, 0),
                Folder(new[] { "My folder" }, Top, 0),
                View(new[] { "My folder", Top }, "Hers", 0)
            };

            List<ViewNode> mine = ToolViewFolder.NodesUnder(walk);

            Assert.That(mine.Count, Is.EqualTo(3));
            Assert.That(mine[0].Name, Is.EqualTo(Top));
            Assert.That(mine[1].ToString(), Is.EqualTo(Top + "/A"));
            Assert.That(mine[2].Name, Is.EqualTo(TestA));
            Assert.That(ToolViewFolder.NodesUnder(null), Is.Empty);
        }

        // ---------- the one time clean up ----------

        private static List<ViewNode> OldTree()
        {
            return new List<ViewNode>
            {
                Folder(new string[0], "A", 0),
                Folder(new[] { "A" }, "Structure vs Mechanical", 0),
                Folder(new[] { "A", "Structure vs Mechanical" }, "Over 150mm", 0),
                View(new[] { "A", "Structure vs Mechanical" }, TestB, 0),
                View(new[] { "A", "Structure vs Mechanical", "Over 150mm" }, TestB, 0),
                Folder(new string[0], "B", 1),
                Folder(new[] { "B" }, "Architecture vs Structure", 0),
                View(new[] { "B", "Architecture vs Structure" }, TestA, 0)
            };
        }

        [Test]
        public void ARootFolderOfOnlyTheToolsOwnNamingIsDeletedWithEverythingInIt()
        {
            LegacyViewCleanup plan = LegacyViewCleanup.Plan(OldTree(), Tests, Teams, Settings);

            Assert.That(plan.Roots.Count, Is.EqualTo(2));
            Assert.That(plan.ToDelete().Count, Is.EqualTo(2));
            Assert.That(plan.Roots[0].Delete, Is.True);
            Assert.That(plan.Roots[0].Views, Is.EqualTo(2));
            Assert.That(plan.Roots[0].Folders, Is.EqualTo(2));
            Assert.That(plan.Roots[1].Views, Is.EqualTo(1));
            Assert.That(plan.Lines()[0], Does.Contain("root folder A: every item is the tool's own naming, deleted with its 2 views and 2 folders"));
        }

        [Test]
        public void OneViewOfAPersonsKeepsTheWholeRootFolderAndNamesItAndWhy()
        {
            List<ViewNode> tree = OldTree();
            tree.Add(View(new[] { "A", "Structure vs Mechanical" }, "Rami's coordination view", 1));

            LegacyViewCleanup plan = LegacyViewCleanup.Plan(tree, Tests, Teams, Settings);

            Assert.That(plan.Roots[0].Delete, Is.False);
            Assert.That(plan.Roots[0].Why, Does.Contain("Rami's coordination view").And.Contain("no test of the built in matrix or the document"));
            Assert.That(plan.Roots[1].Delete, Is.True, "the other root folder is judged on its own");
            Assert.That(plan.ToDelete().Count, Is.EqualTo(1));
            Assert.That(plan.Lines()[0], Does.StartWith("root folder A: left as it is, "));
        }

        [Test]
        public void AFolderOfAPersonsKeepsTheRootFolder()
        {
            List<ViewNode> tree = OldTree();
            tree.Add(Folder(new[] { "B" }, "Site visit 12 Oct", 1));

            LegacyViewCleanup plan = LegacyViewCleanup.Plan(tree, Tests, Teams, Settings);

            Assert.That(plan.Roots[1].Delete, Is.False);
            Assert.That(plan.Roots[1].Why, Does.Contain("Site visit 12 Oct").And.Contain("not two team names"));
        }

        [Test]
        public void AViewDirectlyUnderTheRootOrTooDeepIsNotTheOldTrees()
        {
            List<ViewNode> direct = OldTree();
            direct.Add(View(new[] { "A" }, TestA, 5));
            Assert.That(LegacyViewCleanup.Plan(direct, Tests, Teams, Settings).Roots[0].Delete, Is.False);

            List<ViewNode> deep = OldTree();
            deep.Add(View(new[] { "A", "Structure vs Mechanical", "Over 150mm", "Extra" }, TestA, 5));
            Assert.That(LegacyViewCleanup.Plan(deep, Tests, Teams, Settings).Roots[0].Delete, Is.False);
        }

        [Test]
        public void ATeamPairOfNamesNobodyKnowsIsNotATeamPairFolder()
        {
            List<ViewNode> tree = new List<ViewNode>
            {
                Folder(new string[0], "C", 0),
                Folder(new[] { "C" }, "Coordination vs Programme", 0),
                View(new[] { "C", "Coordination vs Programme" }, TestA, 0)
            };

            LegacyViewCleanup plan = LegacyViewCleanup.Plan(tree, Tests, Teams, Settings);

            Assert.That(plan.Roots[0].Delete, Is.False);
        }

        [Test]
        public void APerClashViewpointOfTheFirstVersionCountsAsTheToolsNaming()
        {
            List<ViewNode> tree = new List<ViewNode>
            {
                Folder(new string[0], "A", 0),
                Folder(new[] { "A" }, "Structure vs Mechanical", 0),
                View(new[] { "A", "Structure vs Mechanical" }, TestB + "  Clash12", 0)
            };

            Assert.That(LegacyViewCleanup.Plan(tree, Tests, Teams, Settings).Roots[0].Delete, Is.True);

            tree[2] = View(new[] { "A", "Structure vs Mechanical" }, "Some test  Clash12", 0);
            Assert.That(LegacyViewCleanup.Plan(tree, Tests, Teams, Settings).Roots[0].Delete, Is.False);
        }

        [Test]
        public void AnEmptyRootFolderIsLeftBecauseNothingProvesItIsTheTools()
        {
            List<ViewNode> tree = new List<ViewNode> { Folder(new string[0], "A", 0) };

            LegacyViewCleanup plan = LegacyViewCleanup.Plan(tree, Tests, Teams, Settings);

            Assert.That(plan.Roots[0].Delete, Is.False);
            Assert.That(plan.Roots[0].Why, Does.Contain("holds nothing"));
        }

        [Test]
        public void TheNoPriorityRootFolderOfTheOldTreeIsJudgedLikeA()
        {
            List<ViewNode> tree = new List<ViewNode>
            {
                Folder(new string[0], "No priority", 0),
                Folder(new[] { "No priority" }, "Architecture vs Structure", 0),
                View(new[] { "No priority", "Architecture vs Structure" }, TestA, 0)
            };

            Assert.That(LegacyViewCleanup.Plan(tree, Tests, Teams, Settings).Roots[0].Delete, Is.True);
        }

        [Test]
        public void ARootFolderOfAnyOtherNameIsNeverLookedAt()
        {
            List<ViewNode> tree = new List<ViewNode>
            {
                Folder(new string[0], "My views", 0),
                View(new[] { "My views" }, TestA, 0),
                Folder(new string[0], Top, 1),
                Folder(new[] { Top }, "A", 0)
            };

            LegacyViewCleanup plan = LegacyViewCleanup.Plan(tree, Tests, Teams, Settings);

            Assert.That(plan.Roots, Is.Empty);
            Assert.That(plan.Lines()[0], Does.Contain("no tree of an earlier version to clean up"));
        }

        // ---------- the proof that deleting viewpoints does not change the results ----------

        [Test]
        public void TheProofLineSaysTheCountsAreTheSameChangedOrNotRead()
        {
            Assert.That(ToolViewFolder.DeletedWithoutChangingTheResultsLine(741, 10170, 741, 10170),
                Does.Contain("left the clash tests and results as they were: 741 tests and 10170 results before, 741 tests and 10170 results after"));
            Assert.That(ToolViewFolder.DeletedWithoutChangingTheResultsLine(741, 10170, 741, 10000),
                Does.Contain("THE DELETE CHANGED THE CLASH COUNTS"));
            Assert.That(ToolViewFolder.DeletedWithoutChangingTheResultsLine(741, -1, 741, 10170), Does.Contain("is UNKNOWN"));
        }

        // ---------- the delete and the rebuild, read off the add-in's source ----------

        private static string BuilderSource()
        {
            string path = Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Engine", "ViewpointBuilder.cs");
            return File.ReadAllText(path);
        }

        [Test]
        public void TheToolFolderIsDeletedBeforeTheViewsAreWrittenAndTheLegacyTreeIsCleanedAfter()
        {
            string source = BuilderSource();
            int body = source.IndexOf("public ViewpointBuildOutcome BuildForGroup(", System.StringComparison.Ordinal);
            int deleted = source.IndexOf("DeleteTheToolFolder(document);", body, System.StringComparison.Ordinal);
            int written = source.IndexOf("WriteTheViews(document, clashTests", body, System.StringComparison.Ordinal);
            int cleaned = source.IndexOf("CleanUpTheLegacyTree(document", body, System.StringComparison.Ordinal);

            Assert.That(deleted, Is.GreaterThan(body));
            Assert.That(written, Is.GreaterThan(deleted), "the old folder goes before the new views are made");
            Assert.That(cleaned, Is.GreaterThan(written));
        }

        [Test]
        public void TheRemovalFindsTheFolderByItsNameAndItsMarkerAndTouchesNothingElse()
        {
            string path = Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Engine", "SavedViewpoints.cs");
            string source = File.ReadAllText(path);
            int start = source.IndexOf("public static ToolFolderRemoval RemoveToolFolder(", System.StringComparison.Ordinal);
            int end = source.IndexOf("public static int MarkToolFolder(", System.StringComparison.Ordinal);
            string body = source.Substring(start, end - start);

            Assert.That(body, Does.Contain("string.Equals(child.DisplayName, ToolViewFolder.Name, StringComparison.Ordinal)"));
            Assert.That(body, Does.Contain("ToolViewFolder.IsMarker(body)"));
            Assert.That(body, Does.Contain("LeftWithoutAMarker"), "a folder of that name with no marker is left and counted");
            Assert.That(body, Does.Contain("document.SavedViewpoints.RemoveAt(root, at);"), "one RemoveAt on the root, a folder at the root only");
        }

        [Test]
        public void TheTopFolderIsMarkedWithTheNwcfAutoBodyAndTheLettersWithTheToolMark()
        {
            string source = BuilderSource();

            Assert.That(source, Does.Contain("depth == 0 ? ToolViewFolder.MarkerBody(stamp) : ToolViewMark.Body("));
        }
    }
}
