using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Diagnostics;
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
    /// value a list does not hold is one NO MODEL MEASURED SO FAR IN THIS PROJECT CARRIES only where the group's
    /// own models are of that project, read off the project part of their names with the naming
    /// settings the scan reads. For any other group, and for one whose project could not be
    /// read, the judge says it CANNOT TELL and why, because a list of one project's models says
    /// nothing about another's. The lists told another project's XML that no model carries
    /// values its models do carry.
    /// </summary>
    public sealed class EmptySetJudge
    {
        /// <summary>Why the project is not known where no model of the group was read at all.</summary>
        private const string ModelsNotRead = "this group's models were not read, so which project they are of is UNKNOWN";

        private readonly string whyNoProject;
        private readonly bool workListRead;
        private readonly int modelsDropped;

        /// <summary>
        /// workListRead is whether the list of workset names inside Core was read, which For takes off
        /// RevitWorksets.ResourceFound. It is a seam so the branch that says it was not is proved by a test,
        /// where the DLL's answer cannot be changed, F115 R10.
        /// </summary>
        internal EmptySetJudge(
            IList<string> worksets,
            string runProject,
            IList<string> groupWorksets,
            string whyNoProject = null,
            IList<string> modelsNotWalked = null,
            bool workListRead = true,
            int modelsDropped = 0)
        {
            Worksets = new ReadOnlyCollection<string>(new List<string>(worksets ?? new List<string>()));
            RunProject = runProject;
            GroupWorksets = new ReadOnlyCollection<string>(new List<string>(groupWorksets ?? new List<string>()));
            ModelsNotWalked = new ReadOnlyCollection<string>(new List<string>(modelsNotWalked ?? new List<string>()));
            this.whyNoProject = whyNoProject;
            this.workListRead = workListRead;
            this.modelsDropped = Math.Max(0, modelsDropped);
        }

        /// <summary>
        /// Every workset this group's models carry, as this run's EXPORT CHECK read them, FR-027.
        /// A value among them IS carried, measured now and whatever project the lists are of. A
        /// value not among them says nothing about the project, so it is never a reason on its
        /// own to call a value carried by no model.
        /// </summary>
        public ReadOnlyCollection<string> GroupWorksets { get; private set; }

        /// <summary>
        /// The models of this group whose counts were not all taken, so ModelExport hands back no worksets
        /// for them, F115 R14. The group's list above leaves them out, and the lists inside Core say nothing
        /// of what they carry, so a workset no other model carries may be carried by one of these and the
        /// judge cannot call a condition on it wrong. Each is named by its file name without the folder
        /// and the extension.
        /// </summary>
        internal ReadOnlyCollection<string> ModelsNotWalked { get; private set; }

        /// <summary>
        /// The workset spellings the clash matrix's corrections were chosen from,
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
        /// names agree on. modelsInTheDocument is how many models the document holds, or minus
        /// one where that was not read, so a model the EXPORT CHECK dropped is one whose worksets
        /// were not read, F115 R14.
        /// </summary>
        public static EmptySetJudge For(
            SetBuildPlan plan, IList<ModelExport> models, ContainerNameSettings names, int modelsInTheDocument)
        {
            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            // F115 R9. Naming settings that will not read a name are said as the reason the project
            // is UNKNOWN, and never thrown out of the judge, a diagnostic, into the group's catch,
            // where no set of the group was built. A report check never fails a group.
            string whyNames = WhyTheNamesWillNotRead(names);
            bool read = models != null && models.Count > 0;

            // A group whose models were not read, as on the Build sets button, is said as not read
            // and never as names that would not read, which reports a read that never ran. The
            // group's worksets by the EXPORT CHECK's own rule, one list for both blocks.
            return new EmptySetJudge(
                plan.Worksets,
                whyNames == null ? ProjectOf(models, names) : null,
                ExportCheck.WorksetsOf(models),
                !read ? ModelsNotRead : whyNames,
                NotWalked(models),
                RevitWorksets.ResourceFound,
                read && modelsInTheDocument > models.Count ? modelsInTheDocument - models.Count : 0);
        }

        /// <summary>Why those naming settings will not read a name, or null where they will.</summary>
        private static string WhyTheNamesWillNotRead(ContainerNameSettings names)
        {
            if (names == null)
            {
                return null;
            }

            try
            {
                names.Validate();
                return null;
            }
            catch (ArgumentException error)
            {
                // Carried, not swallowed: the settings' own words become the reason the judge gives.
                // On one line, since the reason reaches a row of the .tsv, the breaker's finding 9 on attempt 1.
                return "the naming settings will not read a model's name, " + Coverage.CoverageBlock.OneLine(error.Message).Trim().TrimEnd('.')
                    + ", so which project this group's models are of is UNKNOWN";
            }
        }

        /// <summary>
        /// The models whose counts were not all taken. ModelExport empties the worksets of such a model, which
        /// is what ExportCheck.WorksetsOf then reads, so the group's list and this one read the same test.
        /// </summary>
        private static IList<string> NotWalked(IList<ModelExport> models)
        {
            List<string> unread = new List<string>();

            if (models == null)
            {
                return unread;
            }

            foreach (ModelExport model in models)
            {
                if (model == null || model.Counted)
                {
                    continue;
                }

                string stem = string.IsNullOrEmpty(model.File) ? string.Empty : ContainerName.Stem(model.File);
                unread.Add(stem.Length == 0 ? "a model with no name" : stem);
            }

            return unread;
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
                if (!workListRead)
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

                if (ModelsNotWalked.Count > 0 || modelsDropped > 0)
                {
                    return Known.Unknown(WhyWorksetsNotAllRead(), carried);
                }

                return Known.Complete(carried);
            }

            return null;
        }

        /// <summary>Why a workset nobody here carries is not one no model carries, where some model's worksets were not read.</summary>
        private string WhyWorksetsNotAllRead()
        {
            // F115 R14. A model the EXPORT CHECK handed back nothing for is in no list, so the judge
            // is told how many the document holds and says them apart from the ones it can name.
            string dropped = modelsDropped == 1
                ? "1 model of this group was not read by the export check at all, so which worksets it carries is UNKNOWN"
                : modelsDropped + " models of this group were not read by the export check at all, so which worksets they carry is UNKNOWN";

            if (ModelsNotWalked.Count == 0)
            {
                return dropped;
            }

            return WhyWalksStopped() + (modelsDropped > 0 ? ", and " + dropped : string.Empty);
        }

        /// <summary>Why the worksets of the models whose walks stopped are UNKNOWN, naming them.</summary>
        private string WhyWalksStopped()
        {
            List<string> named = new List<string>();

            for (int i = 0; i < ModelsNotWalked.Count && i < RunLog.KeptOfARepeat; i++)
            {
                named.Add(ModelsNotWalked[i]);
            }

            int more = ModelsNotWalked.Count - named.Count;
            string models = named.Count == 1
                ? named[0]
                : string.Join(", ", named.GetRange(0, named.Count - 1).ToArray()) + (more > 0 ? ", " : " and ") + named[named.Count - 1];

            if (more > 0)
            {
                models += " and " + more + " more";
            }

            return ModelsNotWalked.Count == 1
                ? "the worksets of 1 model of this group were not read (" + models + "), so which worksets it carries is UNKNOWN"
                : "the worksets of " + ModelsNotWalked.Count + " models of this group were not read (" + models
                    + "), so which worksets they carry is UNKNOWN";
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
                return "the lists inside this tool were measured on project " + measuredOn + "'s models and "
                    + (whyNoProject ?? "which project this group's models are of could not be read off their names");
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
