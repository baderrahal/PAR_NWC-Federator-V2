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
        /// What a set already in the document is asking, against what the clash matrix
        /// asks. Read off `SelectionSet.Search`, which is a getter nothing in this tool
        /// read until 5w measured that it works on every set of all ten of his groups.
        /// A search that will not read comes back as NOT READ and is never called
        /// drifted, the way a census count that could not be taken is never called a move.
        /// Why it would not read goes with it, and SetDrift.Lines says it in the run log.
        /// </summary>
        private static SetDrift DriftOf(PlannedSet planned, SelectionSet existing)
        {
            List<ReadCondition> asked = null;
            string whyNotRead = null;

            try
            {
                if (existing.HasSearch)
                {
                    asked = new List<ReadCondition>();

                    // This tool's wrapper, disposed, the reviewer's finding 6 on F115 attempt 1.
                    using (Search search = existing.Search)
                    {
                        if (search != null)
                        {
                            foreach (SearchCondition condition in search.SearchConditions)
                            {
                                asked.Add(Read(condition));
                            }
                        }
                    }
                }
                else
                {
                    whyNotRead = "it holds no search, it is a selection of items";
                }
            }
            catch (Exception error)
            {
                // Carried, not swallowed: the error's type and message go to Core with the set,
                // and BuildOne writes them in its SET NOT READ lines in the run log.
                asked = null;
                whyNotRead = error.GetType().Name + ": " + error.Message;
            }

            // The file's side is keyed in Core off the planned set, FR-015, so the two sides
            // are put in one shape by one rule.
            return SetDrift.Compare(asked, planned, whyNotRead);
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
            string whyNot;

            // FR-017. A value that would not read is UNKNOWN in Core, never an empty value.
            return ValueOf(condition.Value, out value, out whyNot)
                ? new ReadCondition(category, property, test, value, flags)
                : ReadCondition.Unread(category, property, test, flags, whyNot);
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
        /// would not read, with why, and the caller marks it unread in Core, which says UNKNOWN.
        /// It used to read every kind but an identifier as a display string and give an empty
        /// string for the throw, so the set was called drifted asking for "".
        /// </summary>
        private static bool ValueOf(VariantData value, out string text, out string whyNot)
        {
            text = string.Empty;
            whyNot = null;

            if (value == null)
            {
                return true;
            }

            try
            {
                text = ClashHarvest.Text(value);
                return true;
            }
            catch (Exception error)
            {
                // Carried, not swallowed: the error's type and message ride on the unread
                // condition, and BuildOne writes them in the set's SET NOT READ lines in the run
                // log, SetDrift.Lines. The set is never called drifted and never rebuilt.
                whyNot = error.GetType().Name + ": " + error.Message;
                return false;
            }
        }

        /// <summary>
        /// Replaces a drifted set in its own slot with one built from the clash matrix,
        /// Q72. `ReplaceWithCopy` and never a remove and an add, because 5v measured that
        /// the replace keeps the clash test pointing at it, its results, its statuses and
        /// its place in the tree, through a save and a reopen.
        ///
        /// The parent is the folder BuildOne resolved from a fresh root, with no mutator
        /// between, and the index is read off the tree at the moment of the call. AFTER the
        /// ReplaceWithCopy that handle has been held across a mutator, so BuildOne reads the
        /// set again through a parent resolved fresh, FR-019, the way the create path does
        /// after its AddCopy. This comment said the parent here was resolved fresh.
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
        /// rather than off the clash matrix, because the tests that point at the broken
        /// name were saved in the NWF by an earlier run and are in no file.
        /// </summary>
        private IList<DocumentSet> ReadEverySetInTheDocument(Document document, bool boxTicked)
        {
            List<DocumentSet> found = new List<DocumentSet>();
            Dictionary<string, int> sides = SidesBySetName(document, boxTicked);

            try
            {
                using (FolderItem root = document.SelectionSets.RootItem)
                {
                    WalkForLeftovers(root, TreeRoot, found, sides);
                }
            }
            catch (Exception error)
            {
                NotRead(boxTicked, "reading the sets already in this NWF", error, "no set is removed or renamed and the run goes on");
                return new List<DocumentSet>();
            }

            return found;
        }

        /// <summary>The first part of every set's path, as WalkForLeftovers writes it and IndexPathsAt reads it.</summary>
        private const string TreeRoot = "lcop_selection_set_tree";

        /// <summary>
        /// A read of the leftover walk that failed. With the box ticked it is a FAILURE, since sets
        /// may be removed. With it off nothing would be done, so it is one finding line and never a
        /// FAILURE, which turned RESULT to errors on every weekly run once the walk ran with the box
        /// off, the reviewer's finding 5 on F115 attempt 1. Core words it on one line.
        /// </summary>
        private void NotRead(bool boxTicked, string what, Exception error, string whatNext)
        {
            if (boxTicked)
            {
                log.Failure(what, error, whatNext);
                return;
            }

            log.Line("SET      " + SetLeftovers.NotReadWithTheBoxOff(
                what, error == null ? null : error.GetType().Name + ": " + error.Message));
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
                            if (set.HasSearch)
                            {
                                // Read once and disposed, the reviewer's finding 6 on F115 attempt 1,
                                // since this walk now runs on every weekly run.
                                using (Search search = set.Search)
                                {
                                    if (search != null)
                                    {
                                        List<ReadCondition> read = new List<ReadCondition>();

                                        foreach (SearchCondition condition in search.SearchConditions)
                                        {
                                            read.Add(Read(condition));
                                        }

                                        // FR-017. None where a value would not read, so it pairs with nothing.
                                        keys = ReadCondition.KeysOf(read);
                                    }
                                }
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
        private Dictionary<string, int> SidesBySetName(Document document, bool boxTicked)
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
                NotRead(
                    boxTicked,
                    "reading what the clash tests point at",
                    error,
                    "how many sides point at each set is UNKNOWN, so no set is removed or renamed and the run goes on");

                return null;
            }

            if (notRead > 0)
            {
                NotRead(
                    boxTicked,
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
        ///
        /// The side's selection and every source read out of it are this tool's wrappers and are
        /// disposed, F115 R1. Sources that read as none throw, so the caller counts the side as not
        /// read, F115 R21: it was counted as a side pointing at nothing, which makes a set look
        /// unused, and an unused set is removed with the box ticked. A source that resolves to a
        /// FOLDER of sets credits every set inside it, F115 R8, since removing one of them changes
        /// what that side finds. Whether Navisworks stores a folder as a clash source is UNKNOWN.
        /// </summary>
        private static void CountSide(
            Document document, Autodesk.Navisworks.Api.Clash.ClashSelection side, Dictionary<string, int> sides)
        {
            HashSet<string> named = new HashSet<string>(StringComparer.Ordinal);

            using (Selection selection = side.Selection)
            {
                SelectionSourceCollection sources = selection == null ? null : selection.SelectionSources;

                if (sources == null)
                {
                    throw new InvalidOperationException("the side's selection sources read as none, so what it points at is UNKNOWN");
                }

                for (int s = 0; s < sources.Count; s++)
                {
                    using (SelectionSource source = sources[s])
                    using (SavedItem pointed = document.SelectionSets.ResolveSelectionSource(source))
                    {
                        if (pointed is SelectionSet)
                        {
                            named.Add(pointed.DisplayName ?? string.Empty);
                            continue;
                        }

                        GroupItem folder = pointed as GroupItem;

                        if (folder != null)
                        {
                            NameEverySetIn(folder, named);
                        }
                    }
                }
            }

            foreach (string name in named)
            {
                sides[name] = sides.ContainsKey(name) ? sides[name] + 1 : 1;
            }
        }

        /// <summary>Every set under that folder, at any depth, each child disposed as it is read, F115 R8.</summary>
        private static void NameEverySetIn(GroupItem folder, HashSet<string> named)
        {
            SavedItemCollection children = folder.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    if (child is SelectionSet)
                    {
                        named.Add(child.DisplayName ?? string.Empty);
                        continue;
                    }

                    GroupItem inner = child as GroupItem;

                    if (inner != null)
                    {
                        NameEverySetIn(inner, named);
                    }
                }
            }
        }

        /// <summary>
        /// Takes the set at that index path out through the PARENT SCOPED form and READS THE TREE
        /// BACK at its set path. `Remove(SavedItem)` addresses the root collection and returns False
        /// without throwing for a nested set, so the return value is never what decides.
        ///
        /// BY PATH AND NEVER BY NAME, the breaker's finding 1 on F115 attempt 1. The first set of
        /// the name was taken and read back by name, so with two sets of one name the line said could
        /// not remove about a set that was gone, and a rename that happened read as one that did not
        /// and declared the document damaged, which stops the group.
        /// </summary>
        private bool RemoveSetAt(Document document, DocumentSelectionSets sets, int[] path, string setPath)
        {
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
            return IndexPathsAt(document, setPath).Count == 0;
        }

        /// <summary>
        /// Renames the set at that index path and reads the tree back at both set paths, for the
        /// same reason: none left where it was and exactly one where it lands.
        /// </summary>
        private bool RenameSetAt(
            Document document, DocumentSelectionSets sets, int[] path, string fromPath, string to, string toPath)
        {
            using (SavedItem found = sets.ResolveIndexPath(path))
            {
                if (found == null)
                {
                    return false;
                }

                sets.EditDisplayName(found, to);
            }

            return IndexPathsAt(document, toPath).Count == 1 && IndexPathsAt(document, fromPath).Count == 0;
        }

        /// <summary>
        /// The index paths, plain ints which survive across a mutator, of every set whose path, built as
        /// WalkForLeftovers builds it, is that path. One where the set is there alone, none where it has
        /// gone, and more where sets share the path, which no caller acts on.
        /// </summary>
        private static IList<int[]> IndexPathsAt(Document document, string setPath)
        {
            List<int[]> found = new List<int[]>();

            using (FolderItem root = document.SelectionSets.RootItem)
            {
                CollectAt(root, TreeRoot, setPath ?? string.Empty, new List<int>(), found);
            }

            return found;
        }

        private static void CollectAt(GroupItem parent, string path, string wanted, List<int> at, List<int[]> found)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    string here = path + "/" + child.DisplayName;

                    if (child is SelectionSet)
                    {
                        if (string.Equals(here, wanted, StringComparison.Ordinal))
                        {
                            at.Add(i);
                            found.Add(at.ToArray());
                            at.RemoveAt(at.Count - 1);
                        }

                        continue;
                    }

                    GroupItem folder = child as GroupItem;

                    if (folder != null && wanted.StartsWith(here + "/", StringComparison.Ordinal))
                    {
                        at.Add(i);
                        CollectAt(folder, here, wanted, at, found);
                        at.RemoveAt(at.Count - 1);
                    }
                }
            }
        }

        /// <summary>Why the set at that path cannot be acted on, or null where it is there alone.</summary>
        private static string WhyNotOneAt(IList<int[]> found, string setPath)
        {
            if (found.Count == 0)
            {
                return "no set is at " + setPath + " any more";
            }

            return found.Count > 1 ? found.Count + " sets are at " + setPath + ", so which one was decided is UNKNOWN" : null;
        }

        /// <summary>
        /// Where that set sits under that folder right now, or minus one. Read from the end, the
        /// order FindSelectionSet reads, F115 R3, so with two sets of one name in one folder the set
        /// the drift was read off is the set replaced. It read from the start, so the rebuild
        /// replaced the other one and the REBUILT count was the unreplaced set's.
        /// </summary>
        private static int IndexOfSet(GroupItem parent, string name)
        {
            SavedItemCollection children = parent.Children;

            for (int i = children.Count - 1; i >= 0; i--)
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

        /// <summary>
        /// Builds the plan's sets. The judge says why a set that found nothing found nothing,
        /// by what it knows of the values this group's models carry, FR-011, and no judge judges
        /// nothing, F128. walkLeftovers is whether the sets of the document this plan does not
        /// name are read and acted on, which only the clash matrix's build does, because a walk
        /// handed the Generic Models plan would remove every set of the file that no test points
        /// at. alsoWanted names the sets of the other plan that walk keeps, the Generic Models
        /// sets the last run saved in the NWF, F128, or null where there are none.
        /// </summary>
        public SetBuildOutcome Build(SetBuildPlan plan, EmptySetJudge judge, bool walkLeftovers, IList<string> alsoWanted)
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
                // Said in the summary and the lines too, the breaker's finding 4 on F115 attempt 1.
                outcome.StoppedBecause = "there is no active document";
                log.Line("SET      stopped, there is no active document");
                return outcome;
            }

            // Named before the first set, because a count means nothing without knowing
            // what it was counted against.
            string openDocument = NavisworksFacts.OpenDocument();
            outcome.OpenDocument = openDocument;
            outcome.RebuildsAsked = rebuilds.RebuildDriftedSets;
            log.Line("SET      ran against " + openDocument);

            DocumentSelectionSets sets = document.SelectionSets;

            for (int i = 0; i < plan.Buildable.Count; i++)
            {
                PlannedSet planned = plan.Buildable[i];
                progress("Set " + (i + 1) + " of " + plan.Buildable.Count + ": " + planned.Name);
                BuildOne(document, sets, planned, judge, outcome);
            }

            // Q74. EVERY SET IN THE NWF THE PICKED FILE NO LONGER NAMES, and what to do
            // about it. Last, because it reads the tree as the build left it and because
            // a leftover is only a leftover once everything the file DOES name is there.
            //
            // THE BOX HAS TO BE ON FOR ANYTHING TO BE DONE. It is off by default, it changes
            // the NWF, and the NWF is the only record of what has been fixed. With it off the
            // leftovers are decided and reported and nothing is done, F115 R20, where they
            // were neither.
            if (walkLeftovers)
            {
                HandleLeftovers(document, sets, plan, alsoWanted, outcome, rebuilds.RebuildDriftedSets);
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
            Document document, DocumentSelectionSets sets, SetBuildPlan plan, IList<string> alsoWanted, SetBuildOutcome outcome,
            bool boxTicked)
        {
            // Every set the file names, the sets the plan skipped among them, F115 R16.
            List<string> named = new List<string>(plan.NamesInTheFile());

            // The sets of the other plan are wanted too, F128, so a Generic Models set the
            // last run saved is not read as a set the file no longer names and removed.
            if (alsoWanted != null)
            {
                named.AddRange(alsoWanted);
            }

            IList<DocumentSet> inDocument = ReadEverySetInTheDocument(document, boxTicked);

            if (inDocument.Count == 0)
            {
                return;
            }

            IList<LeftoverSet> leftovers = SetLeftovers.For(inDocument, named);

            foreach (string line in SetLeftovers.Lines(leftovers, boxTicked))
            {
                log.Line("SET      " + line);
            }

            foreach (LeftoverSet leftover in leftovers)
            {
                if (!boxTicked || leftover.Action == LeftoverAction.Refuse)
                {
                    outcome.AddLeftover(leftover, false);
                }
                else if (!string.IsNullOrEmpty(outcome.TheDocumentIsDamaged))
                {
                    // NOTHING MORE IS CHANGED IN A DAMAGED DOCUMENT, the breaker's finding 5 on F115
                    // attempt 1. The leftovers after the damage were still removed and renamed.
                    outcome.AddLeftoverUntouched(leftover);
                }
                else
                {
                    CarryOut(document, sets, leftover, outcome);
                }
            }

            if (outcome.LeftoversUntouched > 0)
            {
                log.Line("SET      " + outcome.LeftoversUntouched
                    + " more set(s) the file no longer names were left untouched, because the document is damaged and nothing more is changed in it");
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
            // True once the twin is gone, so a throw after it is the window of the pair and never a
            // set left exactly as it was.
            bool twinGone = false;

            try
            {
                // EVERY SET IS FOUND AGAIN BY THE PATH SetLeftovers DECIDED IT BY, the breaker's
                // finding 1 on F115 attempt 1, and a path holding no set or more than one is acted on
                // not at all and said.
                IList<int[]> own = IndexPathsAt(document, leftover.Path);

                if (leftover.Action == LeftoverAction.Remove)
                {
                    string whyNot = WhyNotOneAt(own, leftover.Path);

                    if (whyNot != null)
                    {
                        outcome.AddLeftover(leftover, false);
                        log.Line("SET      nothing removed for " + leftover.Path + ", because " + whyNot);
                        return;
                    }

                    bool went = RemoveSetAt(document, sets, own[0], leftover.Path);
                    outcome.AddLeftover(leftover, went);

                    log.Line("SET      " + (went
                        ? "REMOVED " + leftover.Path + ", which nothing pointed at"
                        : "could not remove " + leftover.Path + ", so it is left exactly as it was"));

                    return;
                }

                // THE PAIR. Both ends found alone at their paths, and nothing else where the rename
                // lands but the twin itself, before anything is touched.
                IList<int[]> twin = IndexPathsAt(document, leftover.TwinPath);
                int landing = leftover.RenamedPath.Length == 0 ? -1 : IndexPathsAt(document, leftover.RenamedPath).Count
                    - (string.Equals(leftover.RenamedPath, leftover.TwinPath, StringComparison.Ordinal) ? 1 : 0);
                string whyNotPair = WhyNotOneAt(twin, leftover.TwinPath)
                    ?? WhyNotOneAt(own, leftover.Path)
                    ?? (landing != 0 ? "where it would land once renamed is not free, or UNKNOWN" : null);

                if (whyNotPair != null)
                {
                    outcome.AddLeftover(leftover, false);
                    log.Line("SET      nothing done for " + leftover.Path + " and its unused twin " + leftover.TwinPath
                        + ", because " + whyNotPair);
                    return;
                }

                // The twin first, then the rename into the freed name.
                if (!RemoveSetAt(document, sets, twin[0], leftover.TwinPath))
                {
                    outcome.AddLeftover(leftover, false);
                    log.Line("SET      the unused \"" + leftover.TwinPath
                        + "\" would not remove, so \"" + leftover.Path
                        + "\" is left exactly as it was and its "
                        + leftover.Sides + " test side(s) still work");
                    return;
                }

                twinGone = true;

                // Found again, since the removal may have moved it in the tree.
                own = IndexPathsAt(document, leftover.Path);

                if (own.Count != 1
                    || !RenameSetAt(document, sets, own[0], leftover.Path, leftover.TwinName, leftover.RenamedPath))
                {
                    // THE WINDOW. The twin is gone and the rename did not happen, so
                    // NEITHER set carries the corrected name and the document is worse
                    // than it started. Nothing may be saved from here.
                    Damaged(leftover, outcome, "would not take its name");
                    return;
                }

                outcome.AddLeftover(leftover, true);
                log.Line("SET      RENAMED \"" + leftover.Path + "\" to \"" + leftover.RenamedPath
                    + "\" and removed the unused set at \"" + leftover.TwinPath + "\". Its "
                    + leftover.Sides + " test side(s) keep working and now ask what the file asks");
            }
            catch (Exception error)
            {
                if (twinGone)
                {
                    log.Failure(
                        "renaming the set " + leftover.Path + " after its unused twin was removed",
                        error,
                        "the document is damaged, nothing more is changed in it and nothing is saved from it");
                    Damaged(leftover, outcome, "threw " + error.GetType().Name + " as it was renamed into its name");
                    return;
                }

                outcome.AddLeftover(leftover, false);
                log.Failure(
                    "bringing the set " + leftover.Path + " up to date",
                    error,
                    "the set is left exactly as it was and the run goes on");
            }
        }

        /// <summary>The pair failed between its two halves: recorded, the document declared damaged and said.</summary>
        private void Damaged(LeftoverSet leftover, SetBuildOutcome outcome, string what)
        {
            outcome.AddLeftover(leftover, false);
            outcome.TheDocumentIsDamaged = Federator.Core.Rerun.DamagedDocument.TheDocumentIsDamaged(
                "the unused \"" + leftover.TwinPath + "\" was removed and \"" + leftover.Path + "\" " + what);

            log.Line("SET      " + outcome.TheDocumentIsDamaged);
        }

        private void BuildOne(
            Document document, DocumentSelectionSets sets, PlannedSet planned, EmptySetJudge judge, SetBuildOutcome outcome)
        {
            // F115 R2 and R22. True from the moment the set is in the document, created or replaced,
            // so a throw after it is said as a set put in and then lost, which asks for the NWF save.
            bool putIn = false;

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

                        // UNKNOWN until it is counted, FR-018. A set not found again after a
                        // rebuild was recorded at 0 items and judged empty on the old question.
                        int found = SetResult.NotCounted;

                        // Released BEFORE the rebuild, FR-019, so no wrapper of the set it
                        // replaces is held across the ReplaceWithCopy.
                        existing.Dispose();

                        if (drift.Drifted && rebuilds.RebuildDriftedSets)
                        {
                            rebuilt = Rebuild(document, sets, planned, parent);
                            putIn = rebuilt;
                        }

                        // THE SET IS READ AGAIN AFTER THE REBUILD AND NEVER BEFORE IT.
                        // ReplaceWithCopy puts a new object in the slot, so the wrapper
                        // read before it is a borrowed handle over something that is no
                        // longer there, which is 4g's rule. Counting through it reported
                        // 0 items for every set this run rebuilt and said "left alone"
                        // about a set it had just replaced, and 3b then judged the OLD
                        // question and called a set wrong that had just been corrected.
                        IList<ReadCondition> asking = drift.CouldNotRead ? null : drift.Asked;
                        string askedNow = drift.AskedNow();
                        SetDrift after = null;

                        // AND AFTER A REBUILD THROUGH A PARENT RESOLVED FRESH, FR-019. The one
                        // from EnsureFolders was held across the ReplaceWithCopy, and a handle
                        // held across an AddCopy does not show the new child, EnsureFolders'
                        // own reading, so the count and the judgement could come off the old
                        // set. What ReplaceWithCopy does to a held parent is UNKNOWN. A folder
                        // that will not resolve again leaves the count UNKNOWN, FR-018.
                        using (GroupItem fresh = rebuilt ? ResolveFolders(sets, planned.Folders, planned.Folders.Count) : null)
                        {
                            GroupItem readFrom = rebuilt ? fresh : parent;

                            using (SelectionSet now = readFrom == null ? null : FindSelectionSet(readFrom, planned.Name))
                            {
                                if (now != null)
                                {
                                    found = CountOf(document, now);

                                    if (rebuilt)
                                    {
                                        after = DriftOf(planned, now);
                                        asking = after.CouldNotRead ? null : after.Asked;
                                        askedNow = after.AskedNow();
                                    }
                                }
                            }
                        }

                        // One call, so a present set is counted as present and never as
                        // created. It used to be added to both lists, which made every
                        // weekly run report sixty one created and save the NWF again.
                        SetResult present = outcome.AddAlreadyPresent(
                            planned.Path, planned.Name, planned.ConditionCount, found);

                        log.Line("SET      " + present.Line()
                            + (rebuilt ? ", and REBUILT from the clash matrix" : string.Empty));

                        // 3a. What the set in the DOCUMENT asks, read off the set and
                        // never off the clash matrix. SETS ACROSS THE RUN used to say
                        // "asked UNKNOWN, because it was already in the NWF and this
                        // run never read its question". 5w reads it.
                        present.Asked = askedNow;
                        present.AskedConditions = asking;

                        // 3b. A set that found NOTHING says which of three things is wrong,
                        // because his own report shows 1,677 of 1,830 tests touching a
                        // set that never produces a clash, and nothing told him which of
                        // those sets is wrong and which is a model with no such content.
                        // Judged on what it asks NOW, so a set this run corrected is not
                        // reported as asking the question it no longer asks, by what the judge
                        // knows of this group's models, F116 and FR-011. Core decides which sets
                        // are judged, never one whose count is UNKNOWN, FR-018.
                        outcome.JudgeIfEmpty(present, asking, judge);

                        // Every present set, so one whose search could not be read is counted
                        // and the lines never claim it asks what the file asks, FR-021. A rebuilt
                        // set is handed with what it was read back asking, F115 R19.
                        outcome.AddDrift(drift, rebuilt, after);

                        // Core decides which sets have lines and what they say: the old question
                        // and the new one for a drifted set, and for one whose search or a value
                        // in it would not read, what would not read and why. None for a set
                        // asking what the file asks.
                        foreach (string line in drift.Lines())
                        {
                            log.Line("SET      " + line);
                        }

                        // F115 R18. The box is named only where it is off. With it ticked a rebuild
                        // that failed has its FAILURE line above, and the box was blamed for it.
                        if (drift.Drifted)
                        {
                            log.Line("SET      " + (!rebuilt
                                ? rebuilds.RebuildDriftedSets
                                    ? "   NOT rebuilt, though \"" + SetRebuildSettings.TickLabel + "\" is ticked. Why is said where"
                                        + " the rebuild was tried, before this set's present line"
                                    : "   left alone. Tick \"" + SetRebuildSettings.TickLabel + "\" to rebuild it, Q72"
                                : found < 0
                                    ? "   REBUILT from the clash matrix, and it could not be found again to count, so what it finds is UNKNOWN"
                                    : "   REBUILT from the clash matrix, and it now finds " + found + " item(s). The clash tests pointing at it keep their results and their statuses, 5v"));

                            // F115 R19. Read back against the file, so a rebuild that did not take is said.
                            if (outcome.RebuiltNotTaken.Contains(drift))
                            {
                                log.Line("SET      "
                                    + "   read back after the rebuild it does NOT ask what the clash matrix asks, or could not be found or read again,"
                                    + " so the rebuild did not take or whether it took is UNKNOWN");
                            }
                        }

                        // A rebuilt set read again whose search would not read says why as well.
                        if (after != null && after.CouldNotRead)
                        {
                            foreach (string line in after.Lines())
                            {
                                log.Line("SET      " + line);
                            }
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
                            putIn = true;
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

                        // Judged where it found nothing, as a set already there is, FR-027.
                        outcome.AddCreated(planned, items, judge);
                        log.Line("SET      " + outcome.Results[outcome.Results.Count - 1].Line());
                    }
                }
            }
            catch (Exception error)
            {
                string why = error.GetType().Name + ": " + error.Message;

                if (putIn)
                {
                    outcome.AddFailedAfterPutIn(planned.Path, planned.Name, planned.ConditionCount, why);
                }
                else
                {
                    outcome.AddFailed(planned.Path, planned.Name, planned.ConditionCount, why);
                }

                log.Failure(
                    "building set " + planned.Path,
                    error,
                    putIn
                        ? "kept going with the next set, this one is reported as FAILED and it is in the document, so the NWF save is asked for"
                        : "kept going with the next set, this one is reported as FAILED");
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

        /// <summary>
        /// How many items a set that is already in the tree finds as it stands, or NotCounted where
        /// the read gave nothing back, which is UNKNOWN and never zero items, FR-018.
        /// </summary>
        private static int CountOf(Document document, SelectionSet set)
        {
            using (ModelItemCollection found = set.GetSelectedItems(document))
            {
                return found == null ? SetResult.NotCounted : found.Count;
            }
        }

        /// <summary>
        /// How many items the set finds in the model as it stands. Resolved through the set
        /// that is in the tree where possible, because that is the thing that has to work.
        /// Falls back to the search itself if the set cannot be found again. A read that gives
        /// nothing back is NotCounted, UNKNOWN and never zero items, FR-018.
        /// </summary>
        private int Resolve(Document document, SelectionSet created, Search search)
        {
            if (created != null)
            {
                using (ModelItemCollection found = created.GetSelectedItems(document))
                {
                    return found == null ? SetResult.NotCounted : found.Count;
                }
            }

            log.Line("SET      the created set could not be found again, resolving the search directly");

            using (ModelItemCollection direct = search.FindAll(document, false))
            {
                return direct == null ? SetResult.NotCounted : direct.Count;
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
