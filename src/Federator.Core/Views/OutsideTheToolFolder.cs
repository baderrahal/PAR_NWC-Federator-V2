using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Federator.Core.Views
{
    /// <summary>One item at the root of the saved viewpoint tree, as the add-in read it.</summary>
    public sealed class RootEntry
    {
        public RootEntry(string name, bool isFolder, int index, int viewsUnder, int foldersUnder, bool isThisRunsToolFolder)
        {
            Name = name ?? string.Empty;
            IsFolder = isFolder;
            Index = index;
            ViewsUnder = viewsUnder;
            FoldersUnder = foldersUnder;
            IsThisRunsToolFolder = isThisRunsToolFolder;
        }

        public string Name { get; private set; }

        public bool IsFolder { get; private set; }

        /// <summary>Its index among the root's children, which a removal is given by.</summary>
        public int Index { get; private set; }

        /// <summary>The viewpoints inside it at any depth, 0 for a viewpoint.</summary>
        public int ViewsUnder { get; private set; }

        /// <summary>The folders inside it at any depth, 0 for a viewpoint.</summary>
        public int FoldersUnder { get; private set; }

        /// <summary>A folder with the tool folder's name and the marker comment of THIS run.</summary>
        public bool IsThisRunsToolFolder { get; private set; }

        /// <summary>Itself and everything inside it.</summary>
        public int Items
        {
            get { return 1 + ViewsUnder + FoldersUnder; }
        }
    }

    /// <summary>What the clean up of the root will do, decided before anything is deleted.</summary>
    public sealed class OutsidePlan
    {
        private readonly List<RootEntry> delete = new List<RootEntry>();
        private readonly List<RootEntry> kept = new List<RootEntry>();

        internal OutsidePlan()
        {
        }

        /// <summary>The root items to delete, in tree order.</summary>
        public IList<RootEntry> Delete
        {
            get { return delete; }
        }

        /// <summary>The root folders named Comments, which are kept with everything inside them.</summary>
        public IList<RootEntry> KeptComments
        {
            get { return kept; }
        }

        /// <summary>True where nothing may be deleted, and WhyNot says why.</summary>
        public bool Refused { get; internal set; }

        public string WhyNot { get; internal set; }

        /// <summary>True where no Comments folder stands at the root, so an empty one is made.</summary>
        public bool CreateComments { get; internal set; }

        /// <summary>Every viewpoint and folder the plan deletes, at any depth.</summary>
        internal int ItemsDeleted
        {
            get
            {
                int total = 0;

                foreach (RootEntry entry in delete)
                {
                    total += entry.Items;
                }

                return total;
            }
        }

        /// <summary>Every viewpoint and folder inside the kept Comments folders, at any depth.</summary>
        public int ItemsKept
        {
            get
            {
                int total = 0;

                foreach (RootEntry entry in kept)
                {
                    total += entry.ViewsUnder + entry.FoldersUnder;
                }

                return total;
            }
        }
    }

    /// <summary>
    /// F191, Bader's order of 15 Oct 2026, task 2: "anything outside the tool folder is deleted". Everything at the
    /// root of the saved viewpoint tree is deleted except the tool folder this run built and a folder named
    /// Comments, whatever its case, which is kept with everything in it. A folder with any other name, "Comments old"
    /// included, is deleted, and so is a viewpoint named Comments, because only a folder is the place kept.
    ///
    /// This replaces the one time clean up of F186, which read the root folders A, B and C by their naming.
    ///
    /// WHEN IT RUNS. After the new tool folder is built and its marker is there, so a build that stopped or threw
    /// deletes nothing of a coordinator's. Where the run planned no views at all there is no tool folder to wait
    /// for. A tool folder that was expected and is not found once and once only refuses the whole clean up.
    /// </summary>
    public static class OutsideTheToolFolder
    {
        /// <summary>The one folder name kept, matched without case.</summary>
        public const string CommentsName = "Comments";

        /// <summary>The most names the log line lists, and then it says how many more.</summary>
        public const int NamesListed = 50;

        /// <summary>Whether a root entry is a folder named Comments, whatever its case.</summary>
        public static bool IsComments(RootEntry entry)
        {
            return entry != null && entry.IsFolder
                && string.Equals(entry.Name, CommentsName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The plan over the root as read. toolFolderExpected is true where this run planned views, so a tool folder
        /// has to be there. Nothing is deleted where it is expected and not found exactly once.
        /// </summary>
        public static OutsidePlan Plan(IList<RootEntry> root, bool toolFolderExpected)
        {
            OutsidePlan plan = new OutsidePlan();
            int toolFolders = 0;

            foreach (RootEntry entry in root ?? new RootEntry[0])
            {
                if (entry != null && entry.IsThisRunsToolFolder)
                {
                    toolFolders++;
                }
            }

            if (toolFolderExpected && toolFolders != 1)
            {
                plan.Refused = true;
                plan.WhyNot = (toolFolders == 0
                        ? "the tool folder of this run was not found at the root by its name and its marker"
                        : toolFolders.ToString(CultureInfo.InvariantCulture) + " tool folders of this run were found at the root")
                    + ", so nothing outside it was deleted";
                return plan;
            }

            foreach (RootEntry entry in root ?? new RootEntry[0])
            {
                if (entry == null || entry.IsThisRunsToolFolder)
                {
                    continue;
                }

                if (IsComments(entry))
                {
                    plan.KeptComments.Add(entry);
                    continue;
                }

                plan.Delete.Add(entry);
            }

            plan.CreateComments = plan.KeptComments.Count == 0;
            return plan;
        }

        /// <summary>
        /// The one line a run writes: the number of items deleted outside the tool folder, the top level names
        /// of what was deleted, at most 50 and then how many more, and what happened to the Comments folder.
        /// </summary>
        public static string Line(OutsidePlan plan, int itemsDeleted, bool commentsCreated)
        {
            if (plan.Refused)
            {
                return "VIEWS    OUTSIDE " + plan.WhyNot;
            }

            StringBuilder text = new StringBuilder();
            text.Append("VIEWS    OUTSIDE ").Append(itemsDeleted.ToString(CultureInfo.InvariantCulture))
                .Append(" items deleted outside the tool folder and the ").Append(CommentsName).Append(" folder");

            if (plan.Delete.Count == 0)
            {
                text.Append(", the root held nothing else");
            }
            else
            {
                text.Append(", ").Append(plan.Delete.Count.ToString(CultureInfo.InvariantCulture))
                    .Append(plan.Delete.Count == 1 ? " top level name: " : " top level names: ");

                for (int i = 0; i < plan.Delete.Count && i < NamesListed; i++)
                {
                    if (i > 0)
                    {
                        text.Append(", ");
                    }

                    text.Append(plan.Delete[i].IsFolder ? "folder " : "viewpoint ").Append('"').Append(plan.Delete[i].Name).Append('"');
                }

                if (plan.Delete.Count > NamesListed)
                {
                    text.Append(" and ").Append((plan.Delete.Count - NamesListed).ToString(CultureInfo.InvariantCulture)).Append(" more");
                }
            }

            text.Append(". ");

            if (commentsCreated)
            {
                text.Append("No ").Append(CommentsName).Append(" folder stood at the root, so an empty one was made");
            }
            else
            {
                text.Append("The ").Append(CommentsName).Append(" folder was kept with its ")
                    .Append(plan.ItemsKept.ToString(CultureInfo.InvariantCulture)).Append(" items");
            }

            return text.ToString();
        }

        /// <summary>
        /// Whether the clash tests and results are as many after as before. A count that was not read on either side
        /// is not a change, since it cannot be proved one, and the PROOF line says UNKNOWN for it.
        /// </summary>
        public static bool Unchanged(Federator.Core.Diagnostics.DocumentCensus before, Federator.Core.Diagnostics.DocumentCensus after)
        {
            if (before == null || after == null)
            {
                return true;
            }

            return Same(before.Of(Federator.Core.Diagnostics.CensusCount.Tests), after.Of(Federator.Core.Diagnostics.CensusCount.Tests))
                && Same(before.Of(Federator.Core.Diagnostics.CensusCount.Results), after.Of(Federator.Core.Diagnostics.CensusCount.Results));
        }

        private static bool Same(int before, int after)
        {
            return before < 0 || after < 0 || before == after;
        }

        /// <summary>The sentence the confirmations carry, the number being what the last run deleted or UNKNOWN.</summary>
        public static string ConfirmWords(int deletedAtTheLastRun)
        {
            return "ALL viewpoints and folders outside the tool folder " + ToolViewFolder.Name + " and the folder "
                + CommentsName + " will be deleted from the NWF. "
                + (deletedAtTheLastRun >= 0
                    ? "The last run deleted " + deletedAtTheLastRun.ToString(CultureInfo.InvariantCulture)
                        + " items there. The number now is counted when the NWF opens and written in the log."
                    : "How many is UNKNOWN until the NWF opens, and the log says it.")
                + " Put your own viewpoints in " + CommentsName + ".";
        }
    }
}
