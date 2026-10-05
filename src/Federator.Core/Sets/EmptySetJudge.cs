using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Exchange;
using Federator.Core.Health;
using Federator.Core.Naming;

namespace Federator.Core.Sets
{
    /// <summary>
    /// What the EMPTY SETS judge knows about the values the models of one group carry, FR-011.
    ///
    /// THE LISTS INSIDE THIS TOOL ARE ONE PROJECT'S. The categories and the workset names were
    /// measured off the models of one project, and each list names it on its project line. A
    /// value a list does not hold is one NO MODEL IN THIS PROJECT CARRIES only where the group's
    /// own models are of that project, read off the project part of their names with the naming
    /// settings the scan reads. For any other group, and for one whose project could not be
    /// read, the judge says it CANNOT TELL and why, because a list of one project's models says
    /// nothing about another's. The lists told another project's XML that no model carries
    /// values its models do carry.
    /// </summary>
    public sealed class EmptySetJudge
    {
        internal EmptySetJudge(IList<string> worksets, string runProject, IList<string> groupWorksets)
        {
            Worksets = new ReadOnlyCollection<string>(new List<string>(worksets ?? new List<string>()));
            RunProject = runProject;
            GroupWorksets = new ReadOnlyCollection<string>(new List<string>(groupWorksets ?? new List<string>()));
        }

        /// <summary>
        /// Every workset this group's models carry, as this run's EXPORT CHECK read them, FR-027.
        /// A value among them IS carried, measured now and whatever project the lists are of. A
        /// value not among them says nothing about the project, so it is never a reason on its
        /// own to call a value carried by no model.
        /// </summary>
        public ReadOnlyCollection<string> GroupWorksets { get; private set; }

        /// <summary>
        /// The workset spellings the picked file's corrections were chosen from,
        /// SetBuildPlan.Worksets, F116: the names inside Core and those of the list beside the
        /// picked file.
        /// </summary>
        public ReadOnlyCollection<string> Worksets { get; private set; }

        /// <summary>
        /// The project part every model of the group names, or null where one would not read,
        /// two differ or no model was read. Never a guess.
        /// </summary>
        public string RunProject { get; private set; }

        /// <summary>
        /// The judge for that plan and the models of the group as the EXPORT CHECK read them, or
        /// null where they were not read: the spellings of the plan and the project the models'
        /// names agree on.
        /// </summary>
        public static EmptySetJudge For(SetBuildPlan plan, IList<ModelExport> models, ContainerNameSettings names)
        {
            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            return new EmptySetJudge(plan.Worksets, ProjectOf(models, names), WorksetsOf(models));
        }

        /// <summary>
        /// Every workset those models carry, each once, in the order first seen, FR-027. A model
        /// whose walk stopped part way gives the ones it saw, which it does carry.
        /// </summary>
        internal static IList<string> WorksetsOf(IList<ModelExport> models)
        {
            List<string> carried = new List<string>();

            if (models == null)
            {
                return carried;
            }

            foreach (ModelExport model in models)
            {
                if (model == null)
                {
                    continue;
                }

                foreach (string workset in model.Worksets)
                {
                    if (!string.IsNullOrEmpty(workset) && !carried.Contains(workset))
                    {
                        carried.Add(workset);
                    }
                }
            }

            return carried;
        }

        /// <summary>
        /// The project part every one of those models' file names reads, or null where none was
        /// read, one would not read or two differ, FR-011.
        /// </summary>
        internal static string ProjectOf(IList<ModelExport> models, ContainerNameSettings names)
        {
            if (models == null || models.Count == 0 || names == null)
            {
                return null;
            }

            string project = null;

            foreach (ModelExport model in models)
            {
                ParsedContainerName parsed = ContainerName.Parse(model == null ? null : model.File, names);

                if (!parsed.IsReadable)
                {
                    return null;
                }

                if (project == null)
                {
                    project = parsed.Project;
                }
                else if (!string.Equals(project, parsed.Project, StringComparison.Ordinal))
                {
                    return null;
                }
            }

            return project;
        }

        /// <summary>
        /// What this judge knows about the values of that property, or null where it has no list
        /// for it and therefore no opinion. Never a guess: a property nobody measured is one the
        /// judge says it cannot tell about.
        /// </summary>
        internal Known KnownFor(string propertyInternalName)
        {
            if (string.Equals(propertyInternalName, EmptySets.CategoryProperty, StringComparison.Ordinal))
            {
                if (!RevitCategories.Measured)
                {
                    return null;
                }

                string why = WhyNotThisProjects(RevitCategories.Project);
                return why == null ? Known.Complete(RevitCategories.All()) : Known.Unknown(why, new List<string>());
            }

            if (string.Equals(propertyInternalName, EmptySets.WorksetProperty, StringComparison.Ordinal))
            {
                // FR-012. A list not read is said, never read as one holding no name. What this
                // group's models carry is carried whatever the lists say, FR-027.
                if (!RevitWorksets.ResourceFound)
                {
                    return Known.Unknown("the workset list inside Federator.Core.dll could not be read", GroupWorksets);
                }

                string why = WhyNotThisProjects(RevitWorksets.Project);

                if (why != null)
                {
                    return Known.Unknown(why, GroupWorksets);
                }

                List<string> carried = new List<string>(Worksets);

                foreach (string workset in GroupWorksets)
                {
                    if (!carried.Contains(workset))
                    {
                        carried.Add(workset);
                    }
                }

                return Known.Complete(carried);
            }

            return null;
        }

        /// <summary>Why a list measured on that project says nothing about this group's models, or null where it does.</summary>
        private string WhyNotThisProjects(string measuredOn)
        {
            if (measuredOn == null)
            {
                return "the list inside this tool names no project it was measured on";
            }

            if (RunProject == null)
            {
                return "the lists inside this tool were measured on project " + measuredOn
                    + "'s models and which project this group's models are of could not be read off their names";
            }

            if (!string.Equals(RunProject, measuredOn, StringComparison.Ordinal))
            {
                return "the lists inside this tool were measured on project " + measuredOn
                    + "'s models and this group's are of project " + RunProject;
            }

            return null;
        }

        /// <summary>
        /// The values known to be carried for one property, and whether the knowledge is
        /// COMPLETE, a list of this project's models, so a value it does not hold is one no
        /// model in this project carries. Where it is not, why.
        /// </summary>
        internal sealed class Known
        {
            private Known(IList<string> carried, bool complete, string whyNot)
            {
                Carried = new ReadOnlyCollection<string>(new List<string>(carried));
                IsComplete = complete;
                WhyNot = whyNot;
            }

            internal ReadOnlyCollection<string> Carried { get; private set; }

            internal bool IsComplete { get; private set; }

            internal string WhyNot { get; private set; }

            internal static Known Complete(IList<string> carried)
            {
                return new Known(carried, true, null);
            }

            /// <summary>Knowledge that is not this project's: only what is known carried, and why the rest is not known.</summary>
            internal static Known Unknown(string whyNot, IList<string> carried)
            {
                return new Known(carried, false, whyNot);
            }
        }
    }
}
