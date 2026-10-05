using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.DocumentParts;
using Federator.Core.Diagnostics;
using Federator.Core.Sets;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Rebuilds the search sets from a plan into whatever document is open. Every
    /// Navisworks call here happens on the thread that calls Build, which is the plugin
    /// thread, exactly as the model side does.
    ///
    /// Nothing about any one project is in here. The names, the folder names, the count
    /// and the internal property names all come from the file that was picked.
    /// </summary>
    public sealed class SetBuilder
    {
        private readonly Action<string> progress;
        private readonly RunLog log;
        private readonly SetRebuildSettings rebuilds;

        public SetBuilder(Action<string> progress, RunLog log, SetRebuildSettings rebuilds)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.progress = progress ?? delegate { };
            this.log = log;
            this.rebuilds = rebuilds ?? new SetRebuildSettings();
        }

        /// <summary>
        /// What a set already in the document is asking, against what the picked file
        /// asks. Read off `SelectionSet.Search`, which is a getter nothing in this tool
        /// read until 5w measured that it works on every set of all ten of his groups.
        /// A search that will not read comes back as NOT READ and is never called
        /// drifted, the way a census count that could not be taken is never called a move.
        /// </summary>
        private static SetDrift DriftOf(PlannedSet planned, SelectionSet existing)
        {
            List<ReadCondition> asked = null;

            try
            {
                if (existing.HasSearch)
                {
                    asked = new List<ReadCondition>();
                    Search search = existing.Search;

                    if (search != null)
                    {
                        foreach (SearchCondition condition in search.SearchConditions)
                        {
                            asked.Add(Read(condition));
                        }
                    }
                }
            }
            catch (Exception)
            {
                asked = null;
            }

            // The file's side is keyed in Core off the planned set, FR-015, so the two sides
            // are put in one shape by one rule.
            return SetDrift.Compare(asked, planned);
        }

        /// <summary>
        /// One condition off a set in the document, in the plain strings Core compares, with its
        /// options as a number, because the negation and the start of an Or group are part of
        /// what it asks, FR-015.
        /// </summary>
        private static ReadCondition Read(SearchCondition condition)
        {
            string category = condition.CategoryCombinedName == null ? string.Empty : Words.Or(condition.CategoryCombinedName.Name, string.Empty);
            string property = condition.PropertyCombinedName == null ? string.Empty : Words.Or(condition.PropertyCombinedName.Name, string.Empty);
            string test = TestOf(condition.Comparison);
            int flags = (int)condition.Options;
            string value;

            // FR-017. A value that would not read is UNKNOWN in Core, never an empty value.
            return ValueOf(condition.Value, out value)
                ? new ReadCondition(category, property, test, value, flags)
                : ReadCondition.Unread(category, property, test, flags);
        }

        /// <summary>
        /// The comparison in the words the file writes for the two BuildCondition builds, and by
        /// its own name for any other, FR-015. Every comparison but contains read as equals, so a
        /// set asking NotEqual read as the same question as one asking Equal.
        /// </summary>
        private static string TestOf(SearchConditionComparison comparison)
        {
            if (comparison == SearchConditionComparison.DisplayStringContains)
            {
                return SetBuildPlan.ContainsTest;
            }

            return comparison == SearchConditionComparison.Equal ? SetBuildPlan.EqualsTest : comparison.ToString();
        }

        /// <summary>
        /// A condition's value as text, read BY ITS KIND through ClashHarvest.Text, addin.md, so a
        /// value that is not a string reads rather than throwing, FR-017. False where it still
        /// would not read, and the caller marks it unread in Core, which says UNKNOWN. It used to
        /// read every kind but an identifier as a display string and give an empty string for the
        /// throw, so the set was called drifted asking for "".
        /// </summary>
        private static bool ValueOf(VariantData value, out string text)
        {
            text = string.Empty;

            if (value == null)
            {
                return true;
            }

            try
            {
                text = ClashHarvest.Text(value);
                return true;
            }
            catch (Exception)
            {
                // Said, not swallowed: the condition is marked unread and its set reads UNKNOWN
                // in the SET DRIFT lines and is never called drifted or rebuilt.
                return false;
            }
        }

        /// <summary>
        /// Replaces a drifted set in its own slot with one built from the picked file,
        /// Q72. `ReplaceWithCopy` and never a remove and an add, because 5v measured that
        /// the replace keeps the clash test pointing at it, its results, its statuses and
        /// its place in the tree, through a save and a reopen.
        ///
        /// The parent is resolved FRESH and the index read off the tree at the moment of
        /// the call, because the walk that found the set released its own wrappers on the
        /// way out and a folder handed across that boundary is refused by name.
        /// </summary>
        private bool Rebuild(Document document, DocumentSelectionSets sets, PlannedSet planned, GroupItem parent)
        {
            try
            {
                int at = IndexOfSet(parent, planned.Name);

                if (at < 0)
                {
                    log.Line("SET      " + planned.Path + " drifted and could not be found again to rebuild");
                    return false;
                }

                using (Search search = new Search())
                {
                    search.Selection.SelectAll();
                    search.Locations = SearchLocations.DescendantsAndSelf;

                    foreach (PlannedCondition condition in planned.Conditions)
                    {
                        search.SearchConditions.Add(BuildCondition(condition));
                    }

                    using (SelectionSet made = new SelectionSet(search))
                    {
                        made.DisplayName = planned.Name;
                        sets.ReplaceWithCopy(parent, at, made);
                    }
                }

                return true;
            }
            catch (Exception error)
            {
                // A rebuild that threw leaves the set exactly as it was, which is the
                // safe end, and it is said rather than swallowed.
                log.Failure(
                    "rebuilding the set " + planned.Path,
                    error,
                    "the set is left exactly as it was and the run goes on");

                return false;
            }
        }

        /// <summary>
        /// Every set in the document, with its path, what it asks in the keys Core
        /// compares on, and HOW MANY CLASH TEST SIDES RESOLVE TO IT. That last number is
        /// what decides everything about a leftover, and it is read off the document
        /// rather than off the picked file, because the tests that point at the broken
        /// name were saved in the NWF by an earlier run and are in no file.
        /// </summary>
        private IList<DocumentSet> ReadEverySetInTheDocument(Document document)
        {
            List<DocumentSet> found = new List<DocumentSet>();
            Dictionary<string, int> sides = SidesBySetName(document);

            try
            {
                using (FolderItem root = document.SelectionSets.RootItem)
                {
                    WalkForLeftovers(root, "lcop_selection_set_tree", found, sides);
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading the sets already in this NWF",
                    error,
                    "no set is removed or renamed and the run goes on");

                return new List<DocumentSet>();
            }

            return found;
        }

        private static void WalkForLeftovers(
            GroupItem parent, string path, List<DocumentSet> found, Dictionary<string, int> sides)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    string here = path + "/" + child.DisplayName;
                    SelectionSet set = child as SelectionSet;

                    if (set != null)
                    {
                        IList<string> keys = new List<string>();

                        try
                        {
                            if (set.HasSearch && set.Search != null)
                            {
                                List<ReadCondition> read = new List<ReadCondition>();

                                foreach (SearchCondition condition in set.Search.SearchConditions)
                                {
                                    read.Add(Read(condition));
                                }

                                // FR-017. None where a value would not read, so it pairs with nothing.
                                keys = ReadCondition.KeysOf(read);
                            }
                        }
                        catch (Exception)
                        {
                            keys.Clear();
                        }

                        // FR-013. Sides that could not be counted are UNKNOWN for every set and
                        // never zero, and Core refuses every leftover of this document for it.
                        int pointing;
                        found.Add(new DocumentSet(
                            here,
                            child.DisplayName,
                            keys,
                            sides == null
                                ? DocumentSet.SidesNotCounted
                                : sides.TryGetValue(child.DisplayName, out pointing) ? pointing : 0));

                        continue;
                    }

                    GroupItem folder = child as GroupItem;

                    if (folder != null)
                    {
                        WalkForLeftovers(folder, here, found, sides);
                    }
                }
            }
        }

        /// <summary>
        /// How many clash test SIDES resolve to each set, by set name. Read once and not
        /// once per set, because walking 1830 tests per set is the O(n squared) shape that
        /// once built 1.7 million native handles in one group.
        ///
        /// NULL WHERE THE COUNT COULD NOT BE TAKEN, FR-013, the whole read or one side. A side
        /// left uncounted makes a set look UNUSED, which is what removes it, so an empty map
        /// here removed every set the file no longer names with the box on and orphaned the
        /// tests pointing at it. The comment over the old catch argued the opposite.
        /// </summary>
        private Dictionary<string, int> SidesBySetName(Document document)
        {
            Dictionary<string, int> sides = new Dictionary<string, int>(StringComparer.Ordinal);
            int notRead = 0;
            Exception first = null;

            try
            {
                CountSidesIn(document, document.GetClash().TestsData.Tests, sides, ref notRead, ref first);
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading what the clash tests point at",
                    error,
                    "how many sides point at each set is UNKNOWN, so no set is removed or renamed and the run goes on");

                return null;
            }

            if (notRead > 0)
            {
                log.Failure(
                    "reading what " + notRead + " clash test side(s) point at",
                    first,
                    "how many sides point at each set is UNKNOWN, so no set is removed or renamed and the run goes on");

                return null;
            }

            return sides;
        }

        /// <summary>
        /// Every test under those items, INTO EVERY CLASH DETECTIVE FOLDER, FR-014, the walk
        /// SavedTests and UndoAutoReviewed already make. Only the root tests were read, so a set
        /// pointed at only by tests a person moved into a folder counted 0 sides and was removed.
        /// Each item read out of the collection and each side is this tool's wrapper and is
        /// disposed, the way SavedTests disposes them. A side that throws is counted as not read.
        /// </summary>
        private static void CountSidesIn(
            Document document, SavedItemCollection items, Dictionary<string, int> sides, ref int notRead, ref Exception first)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    Autodesk.Navisworks.Api.Clash.ClashTest test = item as Autodesk.Navisworks.Api.Clash.ClashTest;

                    if (test != null)
                    {
                        using (Autodesk.Navisworks.Api.Clash.ClashSelection left = test.SelectionA)
                        using (Autodesk.Navisworks.Api.Clash.ClashSelection right = test.SelectionB)
                        {
                            foreach (Autodesk.Navisworks.Api.Clash.ClashSelection side in new[] { left, right })
                            {
                                try
                                {
                                    CountSide(document, side, sides);
                                }
                                catch (Exception error)
                                {
                                    notRead++;
                                    first = first ?? error;
                                }
                            }
                        }

                        continue;
                    }

                    GroupItem folder = item as GroupItem;

                    if (folder != null)
                    {
                        CountSidesIn(document, folder.Children, sides, ref notRead, ref first);
                    }
                }
            }
        }

        /// <summary>
        /// The sets one side points at, EVERY SOURCE OF IT, FR-014, and each set once per side.
        /// Only the first source was read, so a set named second in a side counted 0 sides from it.
        /// </summary>
        private static void CountSide(
            Document document, Autodesk.Navisworks.Api.Clash.ClashSelection side, Dictionary<string, int> sides)
        {
            SelectionSourceCollection sources = side.Selection.SelectionSources;

            if (sources == null)
            {
                return;
            }

            HashSet<string> named = new HashSet<string>(StringComparer.Ordinal);

            for (int s = 0; s < sources.Count; s++)
            {
                using (SavedItem pointed = document.SelectionSets.ResolveSelectionSource(sources[s]))
                {
                    if (pointed != null)
                    {
                        named.Add(pointed.DisplayName ?? string.Empty);
                    }
                }
            }

            foreach (string name in named)
            {
                sides[name] = sides.ContainsKey(name) ? sides[name] + 1 : 1;
            }
        }

        /// <summary>
        /// Takes that set out through the PARENT SCOPED form and READS THE TREE BACK.
        /// `Remove(SavedItem)` addresses the root collection and returns False without
        /// throwing for a nested set, so the return value is never what decides.
        /// </summary>
        private bool RemoveSet(Document document, DocumentSelectionSets sets, string name)
        {
            int[] path = PathToSetNamed(document, name);

            if (path == null)
            {
                return false;
            }

            int last = path[path.Length - 1];

            if (path.Length == 1)
            {
                using (FolderItem root = document.SelectionSets.RootItem)
                {
                    sets.RemoveAt(root, last);
                }
            }
            else
            {
                int[] parentPath = new int[path.Length - 1];
                Array.Copy(path, parentPath, parentPath.Length);

                using (SavedItem parentItem = sets.ResolveIndexPath(parentPath))
                {
                    GroupItem parent = parentItem as GroupItem;

                    if (parent == null)
                    {
                        return false;
                    }

                    sets.RemoveAt(parent, last);
                }
            }

            // READ THE TREE BACK. The return value of a remove is not evidence.
            return PathToSetNamed(document, name) == null;
        }

        /// <summary>Renames that set and reads the tree back, for the same reason.</summary>
        private bool RenameSet(Document document, DocumentSelectionSets sets, string from, string to)
        {
            int[] path = PathToSetNamed(document, from);

            if (path == null)
            {
                return false;
            }

            using (SavedItem found = sets.ResolveIndexPath(path))
            {
                if (found == null)
                {
                    return false;
                }

                sets.EditDisplayName(found, to);
            }

            return PathToSetNamed(document, to) != null && PathToSetNamed(document, from) == null;
        }

        /// <summary>The index path to that set as plain ints, which survive across a mutator.</summary>
        private static int[] PathToSetNamed(Document document, string name)
        {
            using (FolderItem root = document.SelectionSets.RootItem)
            {
                List<int> path = new List<int>();

                return WalkToNamedSet(root, name, path) ? path.ToArray() : null;
            }
        }

        private static bool WalkToNamedSet(GroupItem parent, string name, List<int> path)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];

                if (child is SelectionSet && string.Equals(child.DisplayName, name, StringComparison.Ordinal))
                {
                    path.Add(i);
                    child.Dispose();
                    return true;
                }

                GroupItem folder = child as GroupItem;

                if (folder != null)
                {
                    path.Add(i);

                    if (WalkToNamedSet(folder, name, path))
                    {
                        child.Dispose();
                        return true;
                    }

                    path.RemoveAt(path.Count - 1);
                }

                child.Dispose();
            }

            return false;
        }

        /// <summary>Where that set sits under that folder right now, or minus one.</summary>
        private static int IndexOfSet(GroupItem parent, string name)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    if (child is SelectionSet && string.Equals(child.DisplayName, name, StringComparison.Ordinal))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public SetBuildOutcome Build(SetBuildPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            SetBuildOutcome outcome = new SetBuildOutcome();

            foreach (SkippedSet skipped in plan.Skipped)
            {
                outcome.AddSkipped(skipped);
                log.Line("SET      SKIPPED  " + skipped.Path + "  " + skipped.Reason);
            }

            Document document = NavisworksApplication.ActiveDocument;

            if (document == null)
            {
                log.Line("SET      stopped, there is no active document");
                return outcome;
            }

            // Named before the first set, because a count means nothing without knowing
            // what it was counted against.
            string openDocument = NavisworksFacts.OpenDocument();
            outcome.OpenDocument = openDocument;
            log.Line("SET      ran against " + openDocument);

            DocumentSelectionSets sets = document.SelectionSets;

            for (int i = 0; i < plan.Buildable.Count; i++)
            {
                PlannedSet planned = plan.Buildable[i];
                progress("Set " + (i + 1) + " of " + plan.Buildable.Count + ": " + planned.Name);
                BuildOne(document, sets, planned, plan.Worksets, outcome);
            }

            // Q74. EVERY SET IN THE NWF THE PICKED FILE NO LONGER NAMES, and what to do
            // about it. Last, because it reads the tree as the build left it and because
            // a leftover is only a leftover once everything the file DOES name is there.
            //
            // THE BOX HAS TO BE ON. It is off by default, it changes the NWF, and the NWF
            // is the only record of what has been fixed.
            if (rebuilds.RebuildDriftedSets)
            {
                HandleLeftovers(document, sets, plan, outcome);
            }

            return outcome;
        }

        /// <summary>
        /// The leftovers, decided by `SetLeftovers` in Core and carried out here. The
        /// DECISION is a rule a test can prove and the Navisworks calls are this side of
        /// the line, which is the split this whole project keeps.
        ///
        /// THE ORDER IS LOAD BEARING AND THE PAIR IS ATOMIC AGAINST THE SAVE. The twin
        /// goes first and the rename takes its freed name, because renaming onto a name
        /// that still exists leaves two sets at one path, which is F28's prohibition and a
        /// clash locator resolving to whichever came first. Between the two there is a
        /// window where NEITHER set carries the corrected name, so a failure there leaves
        /// the NWF worse than it started and nothing may be saved from it.
        /// </summary>
        private void HandleLeftovers(
            Document document, DocumentSelectionSets sets, SetBuildPlan plan, SetBuildOutcome outcome)
        {
            List<string> named = new List<string>();

            foreach (PlannedSet planned in plan.Buildable)
            {
                named.Add(planned.Name);
            }

            IList<DocumentSet> inDocument = ReadEverySetInTheDocument(document);

            if (inDocument.Count == 0)
            {
                return;
            }

            IList<LeftoverSet> leftovers = SetLeftovers.For(inDocument, named);

            foreach (string line in SetLeftovers.Lines(leftovers))
            {
                log.Line("SET      " + line);
            }

            foreach (LeftoverSet leftover in leftovers)
            {
                CarryOut(document, sets, leftover, outcome);
            }
        }

        /// <summary>
        /// One leftover carried out, or refused. Every removal reads the tree back rather
        /// than trusting a return value, because `Remove(SavedItem)` on a NESTED set
        /// returns False and throws nothing, which a caller reads as "there was nothing to
        /// remove" when the truth is "the removal did not happen". A silent failed remove
        /// followed by a rename is two sets at one path.
        /// </summary>
        private void CarryOut(
            Document document, DocumentSelectionSets sets, LeftoverSet leftover, SetBuildOutcome outcome)
        {
            if (leftover.Action == LeftoverAction.Refuse)
            {
                outcome.AddLeftover(leftover, false);
                return;
            }

            try
            {
                if (leftover.Action == LeftoverAction.Remove)
                {
                    bool went = RemoveSet(document, sets, leftover.Name);
                    outcome.AddLeftover(leftover, went);

                    log.Line("SET      " + (went
                        ? "REMOVED " + leftover.Path + ", which nothing pointed at"
                        : "could not remove " + leftover.Path + ", so it is left exactly as it was"));

                    return;
                }

                // THE PAIR. The twin first, then the rename into the freed name.
                if (!RemoveSet(document, sets, leftover.TwinName))
                {
                    outcome.AddLeftover(leftover, false);
                    log.Line("SET      the unused \"" + leftover.TwinName
                        + "\" would not remove, so \"" + leftover.Name
                        + "\" is left exactly as it was and its "
                        + leftover.Sides + " test side(s) still work");
                    return;
                }

                if (!RenameSet(document, sets, leftover.Name, leftover.TwinName))
                {
                    // THE WINDOW. The twin is gone and the rename did not happen, so
                    // NEITHER set carries the corrected name and the document is worse
                    // than it started. Nothing may be saved from here.
                    outcome.AddLeftover(leftover, false);
                    outcome.TheDocumentIsDamaged = Federator.Core.Rerun.DamagedDocument.TheDocumentIsDamaged(
                        "the unused \"" + leftover.TwinName + "\" was removed and \""
                            + leftover.Name + "\" would not take its name");

                    log.Line("SET      " + outcome.TheDocumentIsDamaged);
                    return;
                }

                outcome.AddLeftover(leftover, true);
                log.Line("SET      RENAMED \"" + leftover.Name + "\" to \"" + leftover.TwinName
                    + "\" and removed the unused set that held that name. Its "
                    + leftover.Sides + " test side(s) keep working and now ask what the file asks");
            }
            catch (Exception error)
            {
                outcome.AddLeftover(leftover, false);
                log.Failure(
                    "bringing the set " + leftover.Path + " up to date",
                    error,
                    "the set is left exactly as it was and the run goes on");
            }
        }

        private void BuildOne(
            Document document, DocumentSelectionSets sets, PlannedSet planned, IList<string> worksets, SetBuildOutcome outcome)
        {
            try
            {
                using (GroupItem parent = EnsureFolders(sets, planned.Folders))
                {
                    // A reused NWF already holds last week's sets. Adding another copy would
                    // leave the tree with two sets at one path and a clash locator resolving
                    // to whichever came first, so an existing one is left exactly as it is.
                    SelectionSet existing = FindSelectionSet(parent, planned.Name);

                    if (existing != null)
                    {
                        // WHAT IT IS ACTUALLY ASKING, read off the set itself, 5w. Nothing
                        // in this tool read SelectionSet.Search until the drift round, so
                        // a value corrected in the file since the set was built reached
                        // the document nowhere and nothing said so. Q72.
                        SetDrift drift = DriftOf(planned, existing);
                        bool rebuilt = false;
                        int found = 0;

                        if (drift.Drifted && rebuilds.RebuildDriftedSets)
                        {
                            rebuilt = Rebuild(document, sets, planned, parent);
                        }

                        existing.Dispose();

                        // THE SET IS READ AGAIN AFTER THE REBUILD AND NEVER BEFORE IT.
                        // ReplaceWithCopy puts a new object in the slot, so the wrapper
                        // read before it is a borrowed handle over something that is no
                        // longer there, which is 4g's rule. Counting through it reported
                        // 0 items for every set this run rebuilt and said "left alone"
                        // about a set it had just replaced, and 3b then judged the OLD
                        // question and called a set wrong that had just been corrected.
                        IList<ReadCondition> asking = drift.Asked;
                        string askedNow = drift.AskedNow();

                        using (SelectionSet now = FindSelectionSet(parent, planned.Name))
                        {
                            if (now != null)
                            {
                                found = CountOf(document, now);

                                if (rebuilt)
                                {
                                    SetDrift after = DriftOf(planned, now);
                                    asking = after.Asked;
                                    askedNow = after.AskedNow();
                                }
                            }
                        }

                        // One call, so a present set is counted as present and never as
                        // created. It used to be added to both lists, which made every
                        // weekly run report sixty one created and save the NWF again.
                        SetResult present = outcome.AddAlreadyPresent(
                            planned.Path, planned.Name, planned.ConditionCount, found);

                        log.Line("SET      " + present.Line()
                            + (rebuilt ? ", and REBUILT from the picked file" : string.Empty));

                        // 3a. What the set in the DOCUMENT asks, read off the set and
                        // never off the picked file. SETS ACROSS THE RUN used to say
                        // "asked UNKNOWN, because it was already in the NWF and this
                        // run never read its question". 5w reads it.
                        present.Asked = askedNow;

                        // 3b. A set that found NOTHING says which of three things is wrong,
                        // because his own report shows 1,677 of 1,830 tests touching a
                        // set that never produces a clash, and nothing told him which of
                        // those sets is wrong and which is a model with no such content.
                        // Judged on what it asks NOW, so a set this run corrected is not
                        // reported as asking the question it no longer asks, and against the
                        // workset spellings the corrections were chosen from, F116.
                        if (found == 0 && !drift.CouldNotRead)
                        {
                            outcome.AddEmpty(EmptySets.Why(planned.Path, asking, worksets));
                        }

                        // Every present set, so one whose search could not be read is counted
                        // and the lines never claim it asks what the file asks, FR-021.
                        outcome.AddDrift(drift, rebuilt);

                        if (drift.Drifted)
                        {
                            foreach (string line in drift.Lines())
                            {
                                log.Line("SET      " + line);
                            }

                            log.Line("SET      " + (rebuilt
                                ? "   REBUILT from the picked file, and it now finds " + found + " item(s). The clash tests pointing at it keep their results and their statuses, 5v"
                                : "   left alone. Tick \"" + SetRebuildSettings.TickLabel + "\" to rebuild it, Q72"));
                        }

                        return;
                    }

                    // Both of these are this tool's own and both are disposed. They live to
                    // the end of the block because Resolve falls back to the search, and the
                    // set is disposed first so nothing it shares with the search is released
                    // while the search is still being read.
                    using (Search search = new Search())
                    {
                        search.Selection.SelectAll();
                        search.Locations = SearchLocations.DescendantsAndSelf;

                        foreach (PlannedCondition condition in planned.Conditions)
                        {
                            search.SearchConditions.Add(BuildCondition(condition));
                        }

                        int before = CountIn(parent);

                        using (SelectionSet set = new SelectionSet(search))
                        {
                            set.DisplayName = planned.Name;
                            sets.AddCopy(parent, set);
                        }

                        // AddCopy takes a copy, so the item in the tree is not the object
                        // above. Read the parent again from a fresh RootItem before looking,
                        // for the same reason the folders are resolved that way.
                        int items;

                        using (GroupItem fresh = ResolveFolders(sets, planned.Folders, planned.Folders.Count))
                        using (SelectionSet created = AddedAt(fresh ?? parent, before, planned.Name))
                        {
                            items = Resolve(document, created, search);
                        }

                        outcome.AddCreated(
                            planned.Path, planned.Name, planned.ConditionCount, items, planned.Describe());
                        log.Line("SET      " + outcome.Results[outcome.Results.Count - 1].Line());
                    }
                }
            }
            catch (Exception error)
            {
                outcome.AddFailed(
                    planned.Path, planned.Name, planned.ConditionCount,
                    error.GetType().Name + ": " + error.Message);

                log.Failure(
                    "building set " + planned.Path,
                    error,
                    "kept going with the next set, this one is reported as FAILED");
            }
        }

        /// <summary>
        /// Walks the folder path, creating any folder that is not already there. Folders
        /// nest to whatever depth the file used, and are never flattened into a name.
        /// </summary>
        private GroupItem EnsureFolders(DocumentSelectionSets sets, IList<string> folders)
        {
            for (int depth = 0; depth < folders.Count; depth++)
            {
                GroupItem already = ResolveFolders(sets, folders, depth + 1);

                // Every folder this walk hands back is a wrapper of this tool's, including
                // the ones read only to answer a question, so each one is released here.
                if (already != null)
                {
                    already.Dispose();
                    continue;
                }

                using (GroupItem parent = ResolveFolders(sets, folders, depth))
                {
                    if (parent == null)
                    {
                        throw new InvalidOperationException(
                            "The folder \"" + folders[depth] + "\" cannot be created because the path above it "
                                + "is not there.");
                    }

                    using (FolderItem folder = new FolderItem())
                    {
                        folder.DisplayName = folders[depth];
                        sets.AddCopy(parent, folder);
                    }
                }

                // Resolved again from a fresh RootItem rather than from the handle used
                // for the add. A handle held across an AddCopy does not show the new
                // child, which is what made the very first folder look like it had not
                // been created. See docs\history\scan.md.
                using (GroupItem added = ResolveFolders(sets, folders, depth + 1))
                {
                    if (added == null)
                    {
                        // AddCopy returns void, so there is no handle to hold on to and the
                        // only way back to the new folder is to read it again. If that read
                        // still does not show it, say what the level above does hold, so a
                        // repeat of this is diagnosable from the log alone.
                        using (GroupItem above = ResolveFolders(sets, folders, depth))
                        {
                            throw new InvalidOperationException(
                                "The folder \"" + folders[depth] + "\" was added and a fresh read still does not show it. "
                                    + "The level above holds: " + Describe(above) + ".");
                        }
                    }
                }

                log.Line("SET      folder   " + string.Join("/", Prefix(folders, depth + 1)));
            }

            return ResolveFolders(sets, folders, folders.Count);
        }

        /// <summary>
        /// Walks the folder path from a freshly read RootItem and returns the folder at
        /// that depth, or null when any level is missing. The root is read again on every
        /// call on purpose, because that is the read that has been seen to be current.
        /// </summary>
        private static GroupItem ResolveFolders(DocumentSelectionSets sets, IList<string> folders, int depth)
        {
            GroupItem current = sets.RootItem;

            for (int i = 0; i < depth; i++)
            {
                FolderItem next = FindFolder(current, folders[i]);

                // The level just walked past is not the one handed back, so its wrapper
                // is released here rather than left to a finalizer.
                current.Dispose();

                if (next == null)
                {
                    return null;
                }

                current = next;
            }

            return current;
        }

        /// <summary>What a group actually holds, for a failure message that diagnoses itself.</summary>
        private static string Describe(GroupItem group)
        {
            if (group == null)
            {
                return "nothing, the level above could not be read either";
            }

            SavedItemCollection children = group.Children;
            List<string> names = new List<string>();

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    names.Add((child is FolderItem ? "folder " : "set ") + child.DisplayName);
                }
            }

            return names.Count == 0 ? "nothing" : string.Join(", ", names.ToArray());
        }

        private static string[] Prefix(IList<string> folders, int depth)
        {
            string[] prefix = new string[depth];

            for (int i = 0; i < depth; i++)
            {
                prefix[i] = folders[i];
            }

            return prefix;
        }

        private static FolderItem FindFolder(GroupItem parent, string name)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];
                FolderItem folder = child as FolderItem;

                if (folder != null && string.Equals(folder.DisplayName, name, StringComparison.Ordinal))
                {
                    return folder;
                }

                // Every child read out of the collection is a wrapper of this tool's, so
                // the ones not handed back are released here.
                child.Dispose();
            }

            return null;
        }

        private static SelectionSet FindSelectionSet(GroupItem parent, string name)
        {
            SavedItemCollection children = parent.Children;

            // Backwards, because the one just added is at the end.
            for (int i = children.Count - 1; i >= 0; i--)
            {
                SavedItem child = children[i];
                SelectionSet set = child as SelectionSet;

                if (set != null && string.Equals(set.DisplayName, name, StringComparison.Ordinal))
                {
                    return set;
                }

                child.Dispose();
            }

            return null;
        }

        /// <summary>How many children a folder holds right now, or zero when it is null.</summary>
        private static int CountIn(GroupItem parent)
        {
            return parent == null ? 0 : parent.Children.Count;
        }

        /// <summary>
        /// The set just added, read at the index the count held before the add rather than
        /// by walking the collection. AddCopy appends, so that index is where it is, and
        /// the name is checked rather than assumed. Walking once per set is the shape the
        /// rules forbid, and it is what cost 1.7 million native handles a group over the
        /// clash tests before it was found there.
        ///
        /// A tree that is not the shape this expects says so and falls back to the walk,
        /// because a set reported missing when it is there is worse than a slow read.
        /// </summary>
        private SelectionSet AddedAt(GroupItem parent, int index, string name)
        {
            if (parent == null)
            {
                return null;
            }

            SavedItemCollection children = parent.Children;

            if (index >= 0 && index < children.Count)
            {
                SavedItem child = children[index];
                SelectionSet set = child as SelectionSet;

                if (set != null && string.Equals(set.DisplayName, name, StringComparison.Ordinal))
                {
                    return set;
                }

                child.Dispose();
                log.Line("SET      \"" + name + "\" was not at index " + index
                    + ", the index the count gave before the add, so it was looked for by name");
            }

            return FindSelectionSet(parent, name);
        }

        /// <summary>How many items a set that is already in the tree finds as it stands.</summary>
        private static int CountOf(Document document, SelectionSet set)
        {
            using (ModelItemCollection found = set.GetSelectedItems(document))
            {
                return found == null ? 0 : found.Count;
            }
        }

        /// <summary>
        /// How many items the set finds in the model as it stands. Resolved through the set
        /// that is in the tree where possible, because that is the thing that has to work.
        /// Falls back to the search itself if the set cannot be found again.
        /// </summary>
        private int Resolve(Document document, SelectionSet created, Search search)
        {
            if (created != null)
            {
                using (ModelItemCollection found = created.GetSelectedItems(document))
                {
                    return found == null ? 0 : found.Count;
                }
            }

            log.Line("SET      the created set could not be found again, resolving the search directly");

            using (ModelItemCollection direct = search.FindAll(document, false))
            {
                return direct == null ? 0 : direct.Count;
            }
        }

        /// <summary>
        /// One condition. The internal names are what the API matches on, so they go into
        /// NamedConstant as the name. The display words are carried alongside for
        /// reporting, and the two ignore options stop them affecting the match. That is the
        /// rule in .claude\rules\core.md that a search rebuilt through the API matches on
        /// the internal strings and never on the display words.
        ///
        /// The flags from the file are passed through as SearchConditionOptions. Read off
        /// the installed DLL on 2026-08-31, that enum's bits are the same numbers the file
        /// writes, and 64 is StartGroup. See docs\history\scan.md.
        /// </summary>
        private static SearchCondition BuildCondition(PlannedCondition condition)
        {
            SearchConditionOptions options =
                (SearchConditionOptions)condition.Flags
                | SearchConditionOptions.IgnoreCategoryDisplayName
                | SearchConditionOptions.IgnorePropertyDisplayName;

            // No overload takes a condition without a category, so a condition that
            // carried none passes null rather than having one invented for it.
            NamedConstant category = condition.HasCategory
                ? Named(condition.CategoryInternalName, condition.CategoryDisplayName)
                : null;

            NamedConstant property = Named(condition.PropertyInternalName, condition.PropertyDisplayName);

            SearchConditionComparison comparison = condition.Test == ConditionTest.Contains
                ? SearchConditionComparison.DisplayStringContains
                : SearchConditionComparison.Equal;

            return new SearchCondition(
                category,
                property,
                options,
                comparison,
                VariantData.FromDisplayString(condition.Value ?? string.Empty));
        }

        private static NamedConstant Named(string internalName, string displayName)
        {
            return string.IsNullOrEmpty(displayName)
                ? new NamedConstant(internalName ?? string.Empty)
                : new NamedConstant(internalName ?? string.Empty, displayName);
        }
    }
}
