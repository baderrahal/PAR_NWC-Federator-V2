using System.Collections.Generic;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>
    /// Everything the VIEWS TREE block and its seven checks read, F114, Q114 point 19, gathered by
    /// the add-in after the VIEWS step: the plan, the inventory, a fresh walk of the tree after
    /// the removals, and what was read back off each view. A view is keyed by its written place,
    /// ViewPlace.Of, which PlannedTestView.ToString gives. A read back that is null was not read,
    /// and the check then rests on the plan and says so. A part the checks need that is null makes
    /// those checks say they did not run, and they are never counted as holding.
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

        /// <summary>Per view, the file names of the models it reads back as hiding, probe P19, or null where not read.</summary>
        public IDictionary<string, IList<string>> HiddenReadBack { get; set; }

        /// <summary>Per view, the items it reads back as painted, or null where not read.</summary>
        public IDictionary<string, IList<ItemPath>> PaintedReadBack { get; set; }

        /// <summary>The tests the clash step ran, or null where not handed in, which makes the tests with no open clash UNKNOWN.</summary>
        public ICollection<string> TestsRun { get; set; }

        /// <summary>
        /// The tests the mirror rule says are mirrors and not run, F132, the same list the plan was
        /// handed. A plain list of test names until fix-F132 is merged, and null where no mirror
        /// rule ran, so check 5 did not run.
        /// </summary>
        public ICollection<string> Mirrors { get; set; }

        /// <summary>The codes a per clash viewpoint's pair folder is read against.</summary>
        public ICollection<string> KnownCodes { get; set; }

        /// <summary>The test names of the document and the picked XML.</summary>
        public ICollection<string> TestNames { get; set; }

        public ViewpointSettings Settings { get; set; }
    }
}
