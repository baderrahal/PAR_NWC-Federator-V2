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
                GroupItem parent = EnsureFolders(sets, planned.Folders);

                Search search = new Search();
                search.Selection.SelectAll();
                search.Locations = SearchLocations.DescendantsAndSelf;

                foreach (PlannedCondition condition in planned.Conditions)
                {
                    search.SearchConditions.Add(BuildCondition(condition));
                }

                SelectionSet set = new SelectionSet(search);
                set.DisplayName = planned.Name;

                sets.AddCopy(parent, set);

                // AddCopy takes a copy, so the item in the tree is not the object above.
                // Find it again to resolve it, which is what proves the set actually works
                // rather than that it was merely added.
                SelectionSet created = FindSelectionSet(parent, planned.Name);
                int items = Resolve(document, created, search);

                outcome.AddCreated(planned.Path, planned.Name, planned.ConditionCount, items);
                log.Line("SET      " + outcome.Results[outcome.Results.Count - 1].Line());
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
            GroupItem parent = sets.RootItem;

            foreach (string name in folders)
            {
                FolderItem existing = FindFolder(parent, name);

                if (existing != null)
                {
                    parent = existing;
                    continue;
                }

                FolderItem folder = new FolderItem();
                folder.DisplayName = name;
                sets.AddCopy(parent, folder);

                FolderItem created = FindFolder(parent, name);

                if (created == null)
                {
                    throw new InvalidOperationException(
                        "The folder \"" + name + "\" was added but could not be found again.");
                }

                log.Line("SET      folder   " + name);
                parent = created;
            }

            return parent;
        }

        private static FolderItem FindFolder(GroupItem parent, string name)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                FolderItem folder = children[i] as FolderItem;

                if (folder != null && string.Equals(folder.DisplayName, name, StringComparison.Ordinal))
                {
                    return folder;
                }
            }

            return null;
        }

        private static SelectionSet FindSelectionSet(GroupItem parent, string name)
        {
            SavedItemCollection children = parent.Children;

            // Backwards, because the one just added is at the end.
            for (int i = children.Count - 1; i >= 0; i--)
            {
                SelectionSet set = children[i] as SelectionSet;

                if (set != null && string.Equals(set.DisplayName, name, StringComparison.Ordinal))
                {
                    return set;
                }
            }

            return null;
        }

        /// <summary>
        /// How many items the set finds in the model as it stands. Resolved through the
        /// set that is in the tree where possible, because that is the thing that has to
        /// work. Falls back to the search itself if the set cannot be found again.
        /// </summary>
        private int Resolve(Document document, SelectionSet created, Search search)
        {
            if (created != null)
            {
                ModelItemCollection found = created.GetSelectedItems(document);
                return found == null ? 0 : found.Count;
            }

            log.Line("SET      the created set could not be found again, resolving the search directly");
            ModelItemCollection direct = search.FindAll(document, false);
            return direct == null ? 0 : direct.Count;
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
