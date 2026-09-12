using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
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

        public SetBuilder(Action<string> progress, RunLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.progress = progress ?? delegate { };
            this.log = log;
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
                BuildOne(document, sets, planned, outcome);
            }

            return outcome;
        }

        private void BuildOne(
            Document document, DocumentSelectionSets sets, PlannedSet planned, SetBuildOutcome outcome)
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
                        using (existing)
                        {
                            // One call, so a present set is counted as present and never as
                            // created. It used to be added to both lists, which made every
                            // weekly run report sixty one created and save the NWF again.
                            SetResult present = outcome.AddAlreadyPresent(
                                planned.Path, planned.Name, planned.ConditionCount,
                                CountOf(document, existing));
                            log.Line("SET      " + present.Line());
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
                // been created. See docs\scan.md.
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
        /// How many items the set finds in the model as it stands. Resolved through the
        /// set that is in the tree where possible, because that is the thing that has to
        /// work. Falls back to the search itself if the set cannot be found again.
        /// </summary>
        /// <summary>How many items a set that is already in the tree finds as it stands.</summary>
        private static int CountOf(Document document, SelectionSet set)
        {
            using (ModelItemCollection found = set.GetSelectedItems(document))
            {
                return found == null ? 0 : found.Count;
            }
        }

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
        /// reporting, and the two ignore options stop them affecting the match, which is
        /// what CLAUDE.md asks for.
        ///
        /// The flags from the file are passed through as SearchConditionOptions. Read off
        /// the installed DLL on 2026-08-31, that enum's bits are the same numbers the file
        /// writes, and 64 is StartGroup. See docs\scan.md.
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
