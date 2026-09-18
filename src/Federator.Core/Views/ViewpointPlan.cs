using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Grouping;

namespace Federator.Core.Views
{
    /// <summary>
    /// One viewpoint this run means to put in the NWF: which folder it goes in, what it is
    /// called, the discipline it shows and the disciplines it hides.
    ///
    /// No Navisworks type reaches this. What a viewpoint IS on the API was never measured,
    /// see docs\history\scan.md section 5b, so the plan says what is wanted and the add-in
    /// works out how to say it.
    /// </summary>
    public sealed class PlannedViewpoint
    {
        internal PlannedViewpoint(string folder, string name, string shows, IList<string> hides)
        {
            Folder = folder;
            Name = name;
            Shows = shows;
            Hides = new ReadOnlyCollection<string>(hides);
        }

        /// <summary>The folder it sits in, which is the discipline code.</summary>
        public string Folder { get; private set; }

        /// <summary>What the viewpoint is called inside that folder.</summary>
        public string Name { get; private set; }

        /// <summary>The discipline code this viewpoint shows.</summary>
        public string Shows { get; private set; }

        /// <summary>
        /// Every other discipline in the group, hidden. Empty where the group holds one
        /// discipline, which is a real answer and not a reason to skip the viewpoint.
        /// </summary>
        public ReadOnlyCollection<string> Hides { get; private set; }

        /// <summary>
        /// Where it sits, folder then name. This is the address a rerun matches on, so it
        /// is built once here rather than joined by each caller in its own spelling.
        /// </summary>
        public string Path
        {
            get { return Folder + "/" + Name; }
        }

        public override string ToString()
        {
            return Path + ", shows " + Shows + ", hides " + Hides.Count;
        }
    }

    /// <summary>
    /// One folder per discipline in the group, with one viewpoint in each showing that
    /// discipline and hiding the others.
    ///
    /// A GROUP OF ONE DISCIPLINE STILL GETS ITS FOLDER AND ITS VIEWPOINT. It hides nothing,
    /// because there is nothing else in the group to hide, and that is not the same as
    /// having no viewpoint. A person opening any NWF this tool writes finds the same shape
    /// in it, and a group that quietly had no viewpoints would read as one where the step
    /// failed. This is the same reasoning F35 used for creating every clash test in a
    /// single discipline group and running none of them.
    ///
    /// The disciplines come from BuildingGroup.Disciplines, which the scan reads off part 5
    /// of the NWC name. Nothing here parses a file name, because that rule already lives in
    /// one place.
    /// </summary>
    public static class ViewpointPlan
    {
        /// <summary>
        /// The viewpoints wanted for one group, in the order the group's disciplines are
        /// held. An empty list where the group has no discipline at all, which nothing in a
        /// real scan produces but which is not worth throwing over.
        /// </summary>
        public static IList<PlannedViewpoint> For(BuildingGroup group, ViewpointSettings settings)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            List<PlannedViewpoint> planned = new List<PlannedViewpoint>();

            for (int i = 0; i < group.Disciplines.Count; i++)
            {
                string discipline = group.Disciplines[i];

                if (string.IsNullOrEmpty(discipline))
                {
                    continue;
                }

                List<string> hides = new List<string>();

                for (int other = 0; other < group.Disciplines.Count; other++)
                {
                    if (other != i && !string.IsNullOrEmpty(group.Disciplines[other]))
                    {
                        hides.Add(group.Disciplines[other]);
                    }
                }

                planned.Add(new PlannedViewpoint(
                    settings.FolderNameFor(discipline),
                    settings.ViewpointNameFor(discipline),
                    discipline,
                    hides));
            }

            return planned;
        }

        /// <summary>
        /// One line naming what the plan wants, for the log before anything is created, so
        /// a run that then fails still says what it was going to do.
        /// </summary>
        public static string Describe(IList<PlannedViewpoint> planned)
        {
            if (planned == null || planned.Count == 0)
            {
                return "no viewpoint planned, the group names no discipline";
            }

            List<string> names = new List<string>();

            for (int i = 0; i < planned.Count; i++)
            {
                names.Add(planned[i].Path);
            }

            return planned.Count + (planned.Count == 1 ? " viewpoint planned: " : " viewpoints planned: ")
                + string.Join(", ", names.ToArray());
        }
    }
}
