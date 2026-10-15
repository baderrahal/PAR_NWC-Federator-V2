using System.Collections.Generic;
using System.IO;
using Federator.Core.Diagnostics;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F191, Bader's order of 15 Oct 2026, task 2. Everything at the root of the saved viewpoint tree is deleted
    /// except this run's tool folder and a folder named Comments, which is kept with all inside it. Names are
    /// sample data.
    /// </summary>
    [TestFixture]
    public class OutsideTheToolFolderTests
    {
        private static RootEntry View(string name, int index)
        {
            return new RootEntry(name, false, index, 0, 0, false);
        }

        private static RootEntry Folder(string name, int index, int views, int folders)
        {
            return new RootEntry(name, true, index, views, folders, false);
        }

        private static RootEntry Tool(int index)
        {
            return new RootEntry("Clash Status", true, index, 187, 4, true);
        }

        private static List<string> DeletedNames(OutsidePlan plan)
        {
            List<string> names = new List<string>();

            foreach (RootEntry entry in plan.Delete)
            {
                names.Add(entry.Name);
            }

            return names;
        }

        [Test]
        public void ACoordinatorViewpointAtTheRootIsDeleted()
        {
            OutsidePlan plan = OutsideTheToolFolder.Plan(
                new List<RootEntry> { View("Rami site visit", 0), Tool(1) }, true);

            Assert.That(plan.Refused, Is.False);
            Assert.That(DeletedNames(plan), Is.EqualTo(new[] { "Rami site visit" }));
        }

        [Test]
        public void ARootFolderAnNwcBroughtIsDeletedWithEverythingInIt()
        {
            OutsidePlan plan = OutsideTheToolFolder.Plan(
                new List<RootEntry> { Folder("Revit Views", 0, 40, 3), Tool(1) }, true);

            Assert.That(DeletedNames(plan), Is.EqualTo(new[] { "Revit Views" }));
            Assert.That(plan.ItemsDeleted, Is.EqualTo(1 + 40 + 3));
        }

        [Test]
        public void ARootFolderOfTheOldTreeNamedAOrBOrCIsDeletedToo()
        {
            OutsidePlan plan = OutsideTheToolFolder.Plan(
                new List<RootEntry> { Folder("A", 0, 5, 1), Folder("B", 1, 2, 0), Folder("C", 2, 0, 0), Tool(3) }, true);

            Assert.That(DeletedNames(plan), Is.EqualTo(new[] { "A", "B", "C" }));
        }

        [Test]
        public void AFolderNamedCommentsIsKeptWithEverythingInIt()
        {
            OutsidePlan plan = OutsideTheToolFolder.Plan(
                new List<RootEntry> { Folder("Comments", 0, 6, 1), Tool(1) }, true);

            Assert.That(plan.Delete, Is.Empty);
            Assert.That(plan.KeptComments.Count, Is.EqualTo(1));
            Assert.That(plan.ItemsKept, Is.EqualTo(7));
            Assert.That(plan.CreateComments, Is.False);
        }

        [Test]
        public void TheCaseOfCommentsDoesNotMatter()
        {
            OutsidePlan plan = OutsideTheToolFolder.Plan(
                new List<RootEntry> { Folder("COMMENTS", 0, 1, 0), Tool(1) }, true);

            Assert.That(plan.Delete, Is.Empty);
            Assert.That(plan.KeptComments.Count, Is.EqualTo(1));
        }

        [Test]
        public void ASecondFolderNamedLikeCommentsButNotExactlyItIsDeleted()
        {
            OutsidePlan plan = OutsideTheToolFolder.Plan(
                new List<RootEntry>
                {
                    Folder("Comments", 0, 2, 0),
                    Folder("Comments old", 1, 1, 0),
                    Folder(" Comments", 2, 1, 0),
                    Folder("Comment", 3, 1, 0),
                    Tool(4)
                }, true);

            Assert.That(DeletedNames(plan), Is.EqualTo(new[] { "Comments old", " Comments", "Comment" }));
            Assert.That(plan.KeptComments.Count, Is.EqualTo(1));
        }

        [Test]
        public void AViewpointNamedCommentsIsNotTheFolderAndIsDeleted()
        {
            OutsidePlan plan = OutsideTheToolFolder.Plan(
                new List<RootEntry> { View("Comments", 0), Tool(1) }, true);

            Assert.That(DeletedNames(plan), Is.EqualTo(new[] { "Comments" }));
            Assert.That(plan.CreateComments, Is.True, "no folder named Comments stands there, so an empty one is made");
        }

        [Test]
        public void AnEmptyCommentsFolderIsMadeWhenThereIsNone()
        {
            OutsidePlan plan = OutsideTheToolFolder.Plan(new List<RootEntry> { Tool(0) }, true);

            Assert.That(plan.CreateComments, Is.True);
            Assert.That(plan.Delete, Is.Empty);
            Assert.That(OutsideTheToolFolder.Line(plan, 0, true), Does.Contain("an empty one was made"));
        }

        [Test]
        public void AToolFolderOfThatNameWithoutThisRunsMarkerIsOutsideAndDeleted()
        {
            RootEntry old = Folder("Clash Status", 0, 12, 3);
            OutsidePlan plan = OutsideTheToolFolder.Plan(new List<RootEntry> { old, Tool(1) }, true);

            Assert.That(DeletedNames(plan), Is.EqualTo(new[] { "Clash Status" }));
            Assert.That(plan.Delete[0].IsThisRunsToolFolder, Is.False);
        }

        [Test]
        public void TheFolderOfTheOldNameAnEarlierRunMadeIsOutsideAndDeleted()
        {
            // F198. The tool folder was NWC Federator (auto) until it was named Clash Status, so the folder an
            // earlier run made under the old name is outside this run's tool folder and goes with the rest.
            RootEntry old = Folder("NWC Federator (auto)", 0, 190, 4);
            OutsidePlan plan = OutsideTheToolFolder.Plan(new List<RootEntry> { old, Folder("Comments", 1, 0, 0), Tool(2) }, true);

            Assert.That(ToolViewFolder.Name, Is.EqualTo("Clash Status"));
            Assert.That(DeletedNames(plan), Is.EqualTo(new[] { "NWC Federator (auto)" }));
            Assert.That(plan.ItemsDeleted, Is.EqualTo(1 + 190 + 4));
        }

        [Test]
        public void AnEmptyCommentsFolderThatStandsIsKeptAndNoSecondIsMade()
        {
            OutsidePlan plan = OutsideTheToolFolder.Plan(new List<RootEntry> { Folder("Comments", 0, 0, 0), Tool(1) }, true);

            Assert.That(plan.Delete, Is.Empty);
            Assert.That(plan.KeptComments.Count, Is.EqualTo(1));
            Assert.That(plan.ItemsKept, Is.EqualTo(0));
            Assert.That(plan.CreateComments, Is.False, "the empty folder that stands is the one kept");
            Assert.That(OutsideTheToolFolder.Line(plan, 0, false), Does.EndWith("The Comments folder was kept with its 0 items"));
        }

        [Test]
        public void NothingIsDeletedWhenTheToolFolderWasExpectedAndIsNotThere()
        {
            OutsidePlan plan = OutsideTheToolFolder.Plan(new List<RootEntry> { View("Rami site visit", 0) }, true);

            Assert.That(plan.Refused, Is.True);
            Assert.That(plan.Delete, Is.Empty);
            Assert.That(plan.WhyNot, Does.Contain("not found"));
            Assert.That(OutsideTheToolFolder.Line(plan, 0, false), Does.StartWith("VIEWS    OUTSIDE the tool folder of this run was not found"));
        }

        [Test]
        public void WhereNoViewWasPlannedThereIsNoToolFolderToWaitFor()
        {
            OutsidePlan plan = OutsideTheToolFolder.Plan(new List<RootEntry> { View("Rami site visit", 0) }, false);

            Assert.That(plan.Refused, Is.False);
            Assert.That(DeletedNames(plan), Is.EqualTo(new[] { "Rami site visit" }));
        }

        [Test]
        public void TheLineNamesAtMost50AndSaysHowManyMore()
        {
            List<RootEntry> root = new List<RootEntry>();

            for (int i = 0; i < 53; i++)
            {
                root.Add(View("V" + i, i));
            }

            root.Add(Tool(53));
            OutsidePlan plan = OutsideTheToolFolder.Plan(root, true);
            string line = OutsideTheToolFolder.Line(plan, 53, false);

            Assert.That(line, Does.StartWith("VIEWS    OUTSIDE 53 items deleted outside the tool folder and the Comments folder, 53 top level names: "));
            Assert.That(line, Does.Contain("viewpoint \"V49\""));
            Assert.That(line, Does.Not.Contain("\"V50\""));
            Assert.That(line, Does.Contain(" and 3 more"));
        }

        [Test]
        public void TheDeleteIsUnchangedWhenTheTestAndResultCountsAreEqual()
        {
            DocumentCensus same = new DocumentCensus(36, 12, 741, 10170, 400);
            DocumentCensus fewerViews = new DocumentCensus(36, 12, 741, 10170, 3);
            DocumentCensus fewerResults = new DocumentCensus(36, 12, 741, 10100, 3);
            DocumentCensus notRead = new DocumentCensus(36, 12, 741, DocumentCensus.NotCounted, 3);

            Assert.That(OutsideTheToolFolder.Unchanged(same, fewerViews), Is.True, "fewer viewpoints is what the delete is for");
            Assert.That(OutsideTheToolFolder.Unchanged(same, fewerResults), Is.False);
            Assert.That(OutsideTheToolFolder.Unchanged(same, notRead), Is.True, "a count not read is UNKNOWN and not a change");
        }

        [Test]
        public void TheConfirmationWordsSayAllOutsideIsDeletedAndWhatIsKept()
        {
            string said = OutsideTheToolFolder.ConfirmWords(37);

            Assert.That(said, Does.Contain("ALL viewpoints and folders outside the tool folder Clash Status and the folder Comments will be deleted"));
            Assert.That(said, Does.Contain("The last run deleted 37 items there"));
            Assert.That(OutsideTheToolFolder.ConfirmWords(-1), Does.Contain("UNKNOWN until the NWF opens"));
        }

        // ---------- the add-in, read off its source ----------

        private static string Source(string name)
        {
            return File.ReadAllText(Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Engine", name));
        }

        [Test]
        public void TheItemsOutsideAreDeletedAfterTheViewsAreWrittenAndTheProofIsReadBeforeAndAfter()
        {
            string source = Source("ViewpointBuilder.cs");
            int start = source.IndexOf("private void DeleteOutsideTheToolFolder(", System.StringComparison.Ordinal);
            int end = source.IndexOf("// ---------- walk one ----------", start, System.StringComparison.Ordinal);
            string body = source.Substring(start, end - start);

            Assert.That(body, Does.Contain("DocumentCensusReader.Read(document)"));
            Assert.That(body, Does.Contain("OutsideTheToolFolder.Unchanged(before, afterFirst)"));
            Assert.That(body, Does.Contain("OutsideTheToolFolder.Unchanged(before, after)"));
            Assert.That(body, Does.Contain("ToolViewFolder.DeletedWithoutChangingTheResultsLine("));
            Assert.That(body, Does.Contain("EnsureFolders(document, new List<string> { OutsideTheToolFolder.CommentsName })"));
            Assert.That(body, Does.Contain("outcome.AddFailed("));

            int written = source.IndexOf("WriteTheViews(document, clashTests", System.StringComparison.Ordinal);
            int deleted = source.IndexOf("DeleteOutsideTheToolFolder(document, outcome);", System.StringComparison.Ordinal);

            Assert.That(deleted, Is.GreaterThan(written));
            Assert.That(source, Does.Not.Contain("CleanUpTheLegacyTree"));
        }

        [Test]
        public void ARootItemIsRemovedOnlyWhereItsNameAndKindAreStillAtItsIndex()
        {
            string source = Source("SavedViewpoints.cs");
            int start = source.IndexOf("public static string DeleteRootItem(", System.StringComparison.Ordinal);
            string body = source.Substring(start, 1800);

            Assert.That(body, Does.Contain("isFolder != entry.IsFolder"));
            Assert.That(body, Does.Contain("string.Equals(child.DisplayName, entry.Name, StringComparison.Ordinal)"));
            Assert.That(body, Does.Contain("document.SavedViewpoints.RemoveAt(root, entry.Index);"));
        }
    }
}
