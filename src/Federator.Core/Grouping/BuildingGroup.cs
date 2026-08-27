using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Naming;

namespace Federator.Core.Grouping
{
    /// <summary>
    /// Every file that belongs to one building. Discipline is carried for reporting,
    /// it never splits a group.
    /// </summary>
    public sealed class BuildingGroup
    {
        internal BuildingGroup(string building, IList<ParsedContainerName> files, IList<string> disciplines)
        {
            Building = building;
            Files = new ReadOnlyCollection<ParsedContainerName>(files);
            Disciplines = new ReadOnlyCollection<string>(disciplines);
        }

        public string Building { get; private set; }

        public ReadOnlyCollection<ParsedContainerName> Files { get; private set; }

        /// <summary>Distinct disciplines present in this building, in sorted order.</summary>
        public ReadOnlyCollection<string> Disciplines { get; private set; }

        public int FileCount
        {
            get { return Files.Count; }
        }

        public override string ToString()
        {
            return Building + ": " + FileCount + " files, disciplines "
                + string.Join(", ", new List<string>(Disciplines).ToArray());
        }
    }

    /// <summary>
    /// The groups, plus every name that could not be read. Unreadable names are never
    /// folded into a group.
    /// </summary>
    public sealed class BuildingGroupingResult
    {
        internal BuildingGroupingResult(IList<BuildingGroup> groups, IList<ParsedContainerName> unreadable)
        {
            Groups = new ReadOnlyCollection<BuildingGroup>(groups);
            Unreadable = new ReadOnlyCollection<ParsedContainerName>(unreadable);
        }

        public ReadOnlyCollection<BuildingGroup> Groups { get; private set; }

        public ReadOnlyCollection<ParsedContainerName> Unreadable { get; private set; }

        public int GroupCount
        {
            get { return Groups.Count; }
        }

        public BuildingGroup Find(string building)
        {
            if (building == null)
            {
                throw new ArgumentNullException("building");
            }

            foreach (BuildingGroup group in Groups)
            {
                if (string.Equals(group.Building, building, StringComparison.Ordinal))
                {
                    return group;
                }
            }

            return null;
        }
    }
}
