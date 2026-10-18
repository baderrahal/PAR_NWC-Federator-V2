using System.Collections.Generic;
using Federator.Core.Communities;
using Federator.Core.Grouping;
using Federator.Core.Rerun;

namespace Federator.Addin.Ui
{
    /// <summary>
    /// One group of a ticked package on the Grouping step, F232, Bader's round of 19 Oct 2026, fix 2: its community, its
    /// package, its building, its files, its disciplines, how it is expected to run and its status. A group whose files
    /// disagree on the project code or the originator arrives blocked, and a group of one discipline is not built, and
    /// neither can be ticked, NwcPageRule.CanTick. The names, the paths and the files are the package plan's, PackagePlan,
    /// so the row holds no second copy of them.
    /// </summary>
    public sealed class GroupRow : ObservableObject
    {
        private bool include;

        private GroupRow(Package package, string building, IList<string> files, int fileCount, IList<string> disciplines, PackageGroup group)
        {
            Package = package;
            Building = building ?? string.Empty;
            Files = files ?? new List<string>();
            FileCount = fileCount;
            DisciplineCodes = new List<string>(disciplines ?? new List<string>());
            Disciplines = string.Join(", ", new List<string>(DisciplineCodes).ToArray());
            Group = group;
        }

        /// <summary>A group the plan runs. It starts ticked where it can be.</summary>
        public static GroupRow Usable(PackageGroup group)
        {
            GroupRow row = new GroupRow(group.Package, group.Building, group.Files, group.Files.Count, group.Disciplines, group);
            row.include = row.CanInclude;
            return row;
        }

        /// <summary>A group the grouping blocked, or one of a package the name guard refuses, with why.</summary>
        public static GroupRow Blocked(Package package, string building, int fileCount, IList<string> disciplines, string reason)
        {
            GroupRow row = new GroupRow(package, building, new List<string>(), fileCount, disciplines, null);
            row.BlockedReason = reason ?? string.Empty;
            return row;
        }

        public Package Package { get; private set; }

        /// <summary>The plan's group, or null for a blocked one.</summary>
        public PackageGroup Group { get; private set; }

        public string Community
        {
            get { return Package == null ? string.Empty : Package.Community; }
        }

        /// <summary>The package's name in the tree, its path below the community.</summary>
        public string PackageName
        {
            get { return Package == null ? string.Empty : Package.Name; }
        }

        public string Building { get; private set; }

        /// <summary>The group named with its package, the key its tick is kept by across a regroup and the run reads.</summary>
        public string Label
        {
            get { return PackagePlan.LabelFor(Package == null ? string.Empty : Package.RelativePath, Building); }
        }

        /// <summary>Full paths, in the order the files will be appended.</summary>
        public IList<string> Files { get; private set; }

        public int FileCount { get; private set; }

        public string Disciplines { get; private set; }

        public IList<string> DisciplineCodes { get; private set; }

        public int DisciplineCount
        {
            get { return DisciplineCodes.Count; }
        }

        public string NwfName
        {
            get { return Group == null ? string.Empty : Group.NwfName; }
        }

        public string NwfPath
        {
            get { return Group == null ? string.Empty : Group.NwfPath; }
        }

        /// <summary>Null when the group is usable.</summary>
        public string BlockedReason { get; private set; }

        public bool IsBlocked
        {
            get { return BlockedReason != null; }
        }

        /// <summary>
        /// F188. The NWF of this group is already there, so the NWC page does not run it, the NWF page does. Only the
        /// Rebuild tick box lets it be built again, into a new dated folder, and the clash history is not carried.
        /// </summary>
        public bool NwfExists { get; set; }

        /// <summary>F188. The Rebuild tick box of the NWC page, set on every row, confirmed with the history named.</summary>
        public bool RebuildAllowed { get; set; }

        public bool CanInclude
        {
            get { return NwcPageRule.CanTick(IsBlocked, DisciplineCount, NwfExists, RebuildAllowed); }
        }

        /// <summary>Fewer than two disciplines cannot clash, by the one rule in BuildingGroup, and is not built, F232.</summary>
        public bool IsSingleDiscipline
        {
            get { return !IsBlocked && BuildingGroup.CannotClashWith(DisciplineCount); }
        }

        public bool Include
        {
            get { return include; }
            set
            {
                bool now = value && CanInclude;

                if (include == now)
                {
                    Raise("Include");
                    return;
                }

                include = now;
                Raise("Include");
            }
        }

        private string runAs = RunPath.Unknown;

        /// <summary>
        /// Which of the workflows this group is expected to take, in the words of Federator.Core.Rerun.RunPath, worked out
        /// before Run from whether the NWF is already at its output path.
        /// </summary>
        public string RunAs
        {
            get { return IsBlocked ? string.Empty : runAs; }
            set
            {
                string tidied = value ?? RunPath.Unknown;

                if (string.Equals(runAs, tidied, System.StringComparison.Ordinal))
                {
                    return;
                }

                runAs = tidied;
                Raise("RunAs");
                Raise("RunAsShown");
            }
        }

        private string nearbyNwf = string.Empty;

        /// <summary>
        /// The sentence about a file in the NWF folder whose name is nearly the one this group would write, or empty, F71,
        /// kept apart from RunAs, which is what the run counts by.
        /// </summary>
        public string NearbyNwf
        {
            get { return IsBlocked ? string.Empty : nearbyNwf; }
            set
            {
                string tidied = value ?? string.Empty;

                if (string.Equals(nearbyNwf, tidied, System.StringComparison.Ordinal))
                {
                    return;
                }

                nearbyNwf = tidied;
                Raise("NearbyNwf");
                Raise("RunAsShown");
            }
        }

        /// <summary>What the Run as column shows: the label, and the near miss sentence after it where there is one.</summary>
        public string RunAsShown
        {
            get { return IsBlocked ? string.Empty : RunPath.Shown(runAs, nearbyNwf); }
        }

        public string Status
        {
            get { return NwcPageRule.StatusOf(BlockedReason, DisciplineCount); }
        }

        /// <summary>Tells the grid the box may have moved, after the NWF or the Rebuild box changed what it allows.</summary>
        public void AllowedMoved()
        {
            if (include && !CanInclude)
            {
                include = false;
            }

            Raise("Include");
            Raise("CanInclude");
        }

        public override string ToString()
        {
            return Label;
        }
    }
}
