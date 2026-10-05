using System.Collections.Generic;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>
    /// Everything the VIEWS TREE block and its seven checks read, F114, Q114 point 19, gathered by
    /// the add-in after the VIEWS step: the plan, the inventory, a fresh walk of the tree after
    /// the removals, and what was read back off each view. A read back is kept under one key,
    /// PlannedTestView.Key, and under no other shape. A view whose read back is missing, because
    /// the dictionary is null, holds no entry for it or a null one, or is kept under another key,
    /// is a view the check did not run for, F114 attempt 3, and a check with such a view is never
    /// counted as holding. A part the checks need that is null makes those checks say they did not run.
    /// </summary>
    public sealed class ViewsTreeFacts
    {
        /// <summary>The group's name, on the block's first line.</summary>
        public string Group { get; set; }

        /// <summary>The team map the run read.</summary>
        public TeamMap Map { get; set; }

        /// <summary>The group's models with their codes and teams.</summary>
        public IList<ModelTeam> Models { get; set; }

        /// <summary>The per test view plan.</summary>
        public TestViewPlanOutcome Plan { get; set; }

        /// <summary>The inventory taken after the new views were written and before anything was removed.</summary>
        public ViewsInventory Inventory { get; set; }

        /// <summary>What this run wrote.</summary>
        public IList<WrittenView> Written { get; set; }

        /// <summary>A fresh walk of the tree after the removals, S3.</summary>
        public IList<ViewNode> After { get; set; }

        /// <summary>This run's stamp, as its marks carry it.</summary>
        public string RunStamp { get; set; }

        /// <summary>Per view by PlannedTestView.Key, the file names of the models it reads back as hiding, probe P19, or null where not read.</summary>
        public IDictionary<string, IList<string>> HiddenReadBack { get; set; }

        /// <summary>Per view by PlannedTestView.Key, the items it reads back as painted, or null where not read.</summary>
        public IDictionary<string, IList<ItemPath>> PaintedReadBack { get; set; }

        /// <summary>The tests the clash step ran, or null where not handed in, which makes the tests with no open clash UNKNOWN.</summary>
        public ICollection<string> TestsRun { get; set; }

        /// <summary>The codes a per clash viewpoint's pair folder is read against.</summary>
        public ICollection<string> KnownCodes { get; set; }

        /// <summary>The test names of the document and the picked XML.</summary>
        public ICollection<string> TestNames { get; set; }

        public ViewpointSettings Settings { get; set; }

        /// <summary>
        /// The models handed in, a null among them left out as ShownModels leaves it out, the one
        /// list the block's lines and its checks read, F114 attempt 4.
        /// </summary>
        internal List<ModelTeam> ModelsHandedIn()
        {
            return ShownModels.GroupOf(Models);
        }

        /// <summary>
        /// The names of the models the view reads back as hiding, or null where not read: the
        /// dictionary null, no entry under the view's key, or a null entry. The one lookup the
        /// tree line and check 3 read, F114 attempt 4.
        /// </summary>
        internal IList<string> HiddenOf(PlannedTestView view)
        {
            IList<string> hidden;
            return HiddenReadBack != null && HiddenReadBack.TryGetValue(view.Key, out hidden) ? hidden : null;
        }
    }
}
