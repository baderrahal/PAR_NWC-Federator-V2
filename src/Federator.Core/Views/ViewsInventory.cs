using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Views
{
    /// <summary>
    /// What happens to every item of the saved viewpoints tree after this run wrote its views,
    /// F114, Q114 points 16 and 17: the views are made fresh every run, only what this tool made
    /// is removed or replaced, the per clash viewpoints of earlier runs included, and a person's
    /// is never touched wherever it sits. Read off a fresh walk taken after this run's views
    /// were written, marked and read back, the design's order, so a run that stops part way
    /// leaves the old picture or a checked new one and never neither.
    ///
    /// THE SAFETY RULES. S1, a view is removed only when its mark proves it the tool's and
    /// unchanged, or it is a per clash viewpoint of an earlier run by LegacyClashView's strict
    /// shape. S2, the tool's earlier view of a test goes only once every view of that test this
    /// run planned was written, marked and read back, and a per clash viewpoint only once the
    /// whole new tree was, while a view this run wrote and could not mark or read back goes at
    /// once, known by where it was put. S4, nothing goes when the clash step threw or its report
    /// is missing, and a test not read this run keeps its views.
    ///
    /// FOLDERS. A folder goes only when it is the tool's, marked, or unmarked and holding only
    /// per clash viewpoints of earlier runs, was not empty before the run, and every item under
    /// it goes. A person's folder, one holding anything kept, and the root never go. Two folders
    /// of one name side by side cannot be told apart by a parent path, so nothing under that
    /// name goes.
    ///
    /// THE ORDER. Each removal is a parent and an index, deepest first and the latest index
    /// first within a parent, because a removal shifts the indexes after it, 5z. Where the
    /// FolderGoesWithChildren setting says a folder takes its views in one call, probe P14,
    /// what is under a removed folder is not listed apart.
    /// </summary>
    public sealed class ViewsInventory
    {
        private readonly List<InventoryItem> items = new List<InventoryItem>();
        private readonly List<InventoryItem> removals = new List<InventoryItem>();

        private ViewsInventory()
        {
        }

        /// <summary>Every item, in the order of the walk, with its decision.</summary>
        public ReadOnlyCollection<InventoryItem> Items
        {
            get { return new ReadOnlyCollection<InventoryItem>(items); }
        }

        /// <summary>What is removed, in the order it is removed: deepest first, latest index first within a parent.</summary>
        public ReadOnlyCollection<InventoryItem> Removals
        {
            get { return new ReadOnlyCollection<InventoryItem>(removals); }
        }

        /// <summary>The words for one decision, so the block and nothing else spells them.</summary>
        public static string Describe(InventoryDecision decision)
        {
            switch (decision)
            {
                case InventoryDecision.KeepNotOurs: return "kept, not this tool's";
                case InventoryDecision.KeepChangedByAPerson: return "kept, this tool's and changed by a person or not provable";
                case InventoryDecision.KeepTestNotRead: return "kept, its test was not read this run";
                case InventoryDecision.KeepReplacementFailed: return "kept, what replaces it was not written or did not read back";
                case InventoryDecision.KeepWrittenThisRun: return "kept, written, marked and read back by this run";
                case InventoryDecision.KeepFolder: return "kept, a folder";
                case InventoryDecision.KeepClashStepNotSound: return "kept, the clash step threw or its report is missing";
                case InventoryDecision.KeepPlaceNotUnique: return "kept, a folder beside it on its path has the same name";
                case InventoryDecision.RemoveReplaced: return "removed, this tool's earlier view replaced at its place";
                case InventoryDecision.RemoveNoLongerNeeded: return "removed, this tool's earlier view of a test that needs no view there";
                case InventoryDecision.RemoveLegacy: return "removed, a per clash viewpoint of an earlier run";
                case InventoryDecision.RemoveAtOnce: return "removed at once, written this run and not marked or not read back";
                case InventoryDecision.RemoveFolder: return "removed, a folder of this tool's that this run empties";
                default: return "UNKNOWN";
            }
        }

        /// <summary>The decisions for the tree after this run wrote its views.</summary>
        public static ViewsInventory Plan(
            IList<ViewNode> tree,
            TestViewPlanOutcome plan,
            ICollection<string> testsRead,
            IList<WrittenView> written,
            string runStamp,
            bool clashStepSound,
            ICollection<string> knownCodes,
            ICollection<string> testNames,
            ViewpointSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            ViewsInventory inventory = new ViewsInventory();
            List<ViewNode> nodes = new List<ViewNode>();

            if (tree != null)
            {
                foreach (ViewNode node in tree)
                {
                    if (node != null)
                    {
                        nodes.Add(node);
                    }
                }
            }

            if (!clashStepSound)
            {
                foreach (ViewNode node in nodes)
                {
                    inventory.items.Add(new InventoryItem(node, InventoryDecision.KeepClashStepNotSound,
                        "the clash step threw or its report is missing, so nothing is removed"));
                }

                return inventory;
            }

            Facts facts = new Facts(plan, testsRead, written, runStamp, knownCodes, testNames, settings);
            HashSet<string> twinPaths = TwinPaths(nodes);
            Dictionary<ViewNode, InventoryItem> decided = new Dictionary<ViewNode, InventoryItem>();
            HashSet<ViewNode> legacy = new HashSet<ViewNode>();
            HashSet<ViewNode> toolFolders = new HashSet<ViewNode>();

            foreach (ViewNode node in nodes)
            {
                if (!node.IsFolder)
                {
                    decided[node] = OnATwinPath(node, twinPaths)
                        ? Twin(node)
                        : Leaf(node, facts, legacy);
                }
            }

            List<ViewNode> folders = nodes.FindAll(node => node.IsFolder);
            folders.Sort((a, b) => b.Folders.Count.CompareTo(a.Folders.Count));

            foreach (ViewNode folder in folders)
            {
                decided[folder] = OnATwinPath(folder, twinPaths) || twinPaths.Contains(ViewPlace.Key(folder.Folders, folder.Name, true))
                    ? Twin(folder)
                    : Folder(folder, nodes, decided, legacy, toolFolders, settings);
            }

            foreach (ViewNode node in nodes)
            {
                inventory.items.Add(decided[node]);
            }

            inventory.removals.AddRange(Ordered(inventory.items, settings.FolderGoesWithChildren));
            return inventory;
        }

        /// <summary>
        /// The inventory's lines for the VIEWS block: how many of each decision, then every item
        /// kept for a reason a person may want to act on, named with why, then the removals.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            int views = 0;

            foreach (InventoryItem item in items)
            {
                if (!item.Node.IsFolder)
                {
                    views++;
                }
            }

            lines.Add("inventory : " + views + " views and " + (items.Count - views)
                + " folders, read after this run wrote its own");

            foreach (InventoryDecision decision in Enum.GetValues(typeof(InventoryDecision)))
            {
                int count = items.FindAll(item => item.Decision == decision).Count;

                if (count > 0)
                {
                    lines.Add("    " + count.ToString().PadLeft(5) + "  " + Describe(decision));
                }
            }

            foreach (InventoryItem item in items)
            {
                if (item.Decision == InventoryDecision.KeepChangedByAPerson
                    || item.Decision == InventoryDecision.KeepTestNotRead
                    || item.Decision == InventoryDecision.KeepReplacementFailed
                    || item.Decision == InventoryDecision.KeepPlaceNotUnique
                    || item.Decision == InventoryDecision.RemoveAtOnce)
                {
                    lines.Add("    " + item.Node + " : " + item.Why);
                }
            }

            lines.Add("removals : " + removals.Count + ", deepest first and the latest index first within a folder");
            return lines;
        }

        private static InventoryItem Leaf(ViewNode node, Facts facts, HashSet<ViewNode> legacy)
        {
            WrittenView mine = facts.WrittenAt(node);

            if (mine != null)
            {
                if (mine.Sound)
                {
                    return new InventoryItem(node, InventoryDecision.KeepWrittenThisRun, "written, marked and read back by this run");
                }

                return new InventoryItem(node, InventoryDecision.RemoveAtOnce, mine.Marked
                    ? "written this run and it did not read back"
                    : "written this run and it could not be marked");
            }

            MarkJudgement judged = ToolViewMark.Judge(
                node.Folders, node.Name, node.Camera, node.Comments, node.Redlines, node.Guid, facts.Settings);

            if (judged.Owner == ViewOwner.ChangedByAPerson)
            {
                return new InventoryItem(node, InventoryDecision.KeepChangedByAPerson, judged.Why);
            }

            if (judged.Owner == ViewOwner.Ours)
            {
                return Earlier(node, judged.Mark, facts);
            }

            string legacyTest = LegacyClashView.TestOf(
                node.Folders, node.Name, false, node.Comments.Count, node.Redlines, facts.KnownCodes, facts.TestNames, facts.Settings);

            if (legacyTest == null)
            {
                return new InventoryItem(node, InventoryDecision.KeepNotOurs, judged.Why);
            }

            legacy.Add(node);

            if (!facts.Read.Contains(legacyTest))
            {
                return new InventoryItem(node, InventoryDecision.KeepTestNotRead, "its test " + legacyTest + " was not read this run");
            }

            if (!facts.NewTreeWhole)
            {
                return new InventoryItem(node, InventoryDecision.KeepReplacementFailed,
                    "a view this run planned was not written or did not read back, so the per clash viewpoints stay");
            }

            return new InventoryItem(node, InventoryDecision.RemoveLegacy, "a per clash viewpoint of an earlier run of " + legacyTest);
        }

        private static InventoryItem Earlier(ViewNode node, ToolViewMark mark, Facts facts)
        {
            if (string.Equals(mark.Stamp, facts.RunStamp, StringComparison.Ordinal))
            {
                return new InventoryItem(node, InventoryDecision.KeepWrittenThisRun, "marked by this run");
            }

            if (!facts.Read.Contains(node.Name))
            {
                return new InventoryItem(node, InventoryDecision.KeepTestNotRead, "its test was not read this run");
            }

            if (!facts.TestWrittenWhole(node.Name))
            {
                return new InventoryItem(node, InventoryDecision.KeepReplacementFailed,
                    "a view of its test this run planned was not written or did not read back");
            }

            return facts.PlannedAt(node)
                ? new InventoryItem(node, InventoryDecision.RemoveReplaced, "replaced by this run's view at its place")
                : new InventoryItem(node, InventoryDecision.RemoveNoLongerNeeded, "its test was read and needs no view at its place this run");
        }

        private static InventoryItem Folder(
            ViewNode folder,
            List<ViewNode> nodes,
            Dictionary<ViewNode, InventoryItem> decided,
            HashSet<ViewNode> legacy,
            HashSet<ViewNode> toolFolders,
            ViewpointSettings settings)
        {
            MarkJudgement judged = ToolViewMark.Judge(
                folder.Folders, folder.Name, null, folder.Comments, folder.Redlines, folder.Guid, settings);

            if (judged.Owner == ViewOwner.ChangedByAPerson)
            {
                return new InventoryItem(folder, InventoryDecision.KeepChangedByAPerson, judged.Why);
            }

            List<ViewNode> under = nodes.FindAll(node => IsUnder(node, folder));
            bool onlyLegacy = under.Exists(node => !node.IsFolder)
                && under.TrueForAll(node => node.IsFolder ? toolFolders.Contains(node) : legacy.Contains(node));

            if (judged.Owner == ViewOwner.Ours || (folder.Comments.Count == 0 && onlyLegacy))
            {
                toolFolders.Add(folder);
            }
            else
            {
                return new InventoryItem(folder, InventoryDecision.KeepFolder, "not a folder this tool made");
            }

            if (folder.EmptyBeforeTheRun)
            {
                return new InventoryItem(folder, InventoryDecision.KeepFolder, "it was empty before this run");
            }

            if (under.Count == 0)
            {
                return new InventoryItem(folder, InventoryDecision.KeepFolder, "it holds nothing");
            }

            if (!under.TrueForAll(node => decided[node].Removes))
            {
                return new InventoryItem(folder, InventoryDecision.KeepFolder, "it holds something kept");
            }

            return new InventoryItem(folder, InventoryDecision.RemoveFolder, "this tool's, and this run empties it");
        }

        private static InventoryItem Twin(ViewNode node)
        {
            return new InventoryItem(node, InventoryDecision.KeepPlaceNotUnique,
                "a folder on its path shares its name with a folder beside it, so it cannot be found again for certain");
        }

        private static List<InventoryItem> Ordered(IList<InventoryItem> all, bool folderGoesWithChildren)
        {
            List<ViewNode> removedFolders = new List<ViewNode>();

            foreach (InventoryItem item in all)
            {
                if (item.Removes && item.Node.IsFolder)
                {
                    removedFolders.Add(item.Node);
                }
            }

            List<InventoryItem> ordered = new List<InventoryItem>();

            foreach (InventoryItem item in all)
            {
                if (item.Removes && !(folderGoesWithChildren && removedFolders.Exists(folder => IsUnder(item.Node, folder))))
                {
                    ordered.Add(item);
                }
            }

            ordered.Sort((a, b) =>
            {
                int byDepth = b.Node.Folders.Count.CompareTo(a.Node.Folders.Count);

                if (byDepth != 0)
                {
                    return byDepth;
                }

                int byParent = string.CompareOrdinal(ViewPlace.ParentKey(a.Node.Folders), ViewPlace.ParentKey(b.Node.Folders));

                return byParent != 0 ? byParent : b.Node.IndexInParent.CompareTo(a.Node.IndexInParent);
            });

            return ordered;
        }

        private static bool IsUnder(ViewNode node, ViewNode folder)
        {
            int depth = folder.Folders.Count;

            if (node.Folders.Count <= depth || !string.Equals(node.Folders[depth], folder.Name, StringComparison.Ordinal))
            {
                return false;
            }

            for (int i = 0; i < depth; i++)
            {
                if (!string.Equals(node.Folders[i], folder.Folders[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The paths of every folder that has a sibling folder of the same name.</summary>
        private static HashSet<string> TwinPaths(IEnumerable<ViewNode> nodes)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> twins = new HashSet<string>(StringComparer.Ordinal);

            foreach (ViewNode node in nodes)
            {
                if (node.IsFolder && !seen.Add(ViewPlace.Key(node.Folders, node.Name, true)))
                {
                    twins.Add(ViewPlace.Key(node.Folders, node.Name, true));
                }
            }

            return twins;
        }

        private static bool OnATwinPath(ViewNode node, HashSet<string> twinPaths)
        {
            for (int depth = 1; depth <= node.Folders.Count; depth++)
            {
                List<string> above = new List<string>(node.Folders).GetRange(0, depth - 1);

                if (twinPaths.Contains(ViewPlace.Key(above, node.Folders[depth - 1], true)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Whether two views sit at one place, by ViewPlace's one key.</summary>
        private static bool SamePlace(IList<string> foldersA, string nameA, IList<string> foldersB, string nameB)
        {
            return string.Equals(ViewPlace.Key(foldersA, nameA, false), ViewPlace.Key(foldersB, nameB, false), StringComparison.Ordinal);
        }

        /// <summary>What every decision of one inventory reads, gathered once.</summary>
        private sealed class Facts
        {
            private readonly List<PlannedTestView> planned;
            private readonly List<WrittenView> written;

            internal Facts(
                TestViewPlanOutcome plan,
                ICollection<string> testsRead,
                IList<WrittenView> written,
                string runStamp,
                ICollection<string> knownCodes,
                ICollection<string> testNames,
                ViewpointSettings settings)
            {
                planned = plan == null ? new List<PlannedTestView>() : new List<PlannedTestView>(plan.Views);
                this.written = written == null ? new List<WrittenView>() : new List<WrittenView>(written);
                Read = new HashSet<string>(testsRead ?? new string[0], StringComparer.Ordinal);
                RunStamp = runStamp ?? string.Empty;
                KnownCodes = knownCodes ?? new string[0];
                TestNames = testNames ?? new string[0];
                Settings = settings;
                NewTreeWhole = planned.TrueForAll(Written);
            }

            internal HashSet<string> Read { get; private set; }

            internal string RunStamp { get; private set; }

            internal ICollection<string> KnownCodes { get; private set; }

            internal ICollection<string> TestNames { get; private set; }

            internal ViewpointSettings Settings { get; private set; }

            /// <summary>Whether every view this run planned was written, marked and read back.</summary>
            internal bool NewTreeWhole { get; private set; }

            internal WrittenView WrittenAt(ViewNode node)
            {
                return written.Find(view => view.IndexInParent == node.IndexInParent
                    && SamePlace(view.Folders, view.Name, node.Folders, node.Name));
            }

            internal bool TestWrittenWhole(string test)
            {
                return planned.TrueForAll(view => !string.Equals(view.Name, test, StringComparison.Ordinal) || Written(view));
            }

            internal bool PlannedAt(ViewNode node)
            {
                return planned.Exists(view => SamePlace(view.Folders, view.Name, node.Folders, node.Name));
            }

            private bool Written(PlannedTestView view)
            {
                return written.Exists(done => done.Sound && SamePlace(done.Folders, done.Name, view.Folders, view.Name));
            }
        }
    }
}
