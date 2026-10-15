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
    /// nothing inside the folders it leaves is read for removal. What stands outside it is deleted by F191, and
    /// the Comments folder is kept. Names are sample data.
    /// </summary>
    [TestFixture]
    public class ToolViewFolderTests
    {
        private const string Top = "Clash Status";
        private const string TestA = "BLD-AR-Walls-vs-BLD-ST-Columns";
        private const string TestB = "BLD-ME-Ducts-vs-BLD-ST-Columns";

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
        public void TheToolFolderIsDeletedBeforeTheViewsAreWrittenAndTheItemsOutsideItAfter()
        {
            string source = BuilderSource();
            int body = source.IndexOf("public ViewpointBuildOutcome BuildForGroup(", System.StringComparison.Ordinal);
            int deleted = source.IndexOf("DeleteTheToolFolder(document);", body, System.StringComparison.Ordinal);
            int written = source.IndexOf("WriteTheViews(document, clashTests", body, System.StringComparison.Ordinal);
            int cleaned = source.IndexOf("DeleteOutsideTheToolFolder(document, outcome);", body, System.StringComparison.Ordinal);

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
