using System;
using System.Collections.Generic;
using Federator.Core.Naming;

namespace Federator.Core.Grouping
{
    /// <summary>
    /// Groups parsed container names on the full building code. Every discipline of a
    /// building lands in the same group.
    /// </summary>
    public static class BuildingGrouping
    {
        public static BuildingGroupingResult Group(IEnumerable<ParsedContainerName> names)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }

            List<ParsedContainerName> unreadable = new List<ParsedContainerName>();
            List<string> order = new List<string>();
            Dictionary<string, List<ParsedContainerName>> byBuilding =
                new Dictionary<string, List<ParsedContainerName>>(StringComparer.Ordinal);

            foreach (ParsedContainerName name in names)
            {
                if (name == null)
                {
                    throw new ArgumentException("The list held a null name.", "names");
                }

                if (!name.IsReadable)
                {
                    unreadable.Add(name);
                    continue;
                }

                List<ParsedContainerName> bucket;

                if (!byBuilding.TryGetValue(name.Building, out bucket))
                {
                    bucket = new List<ParsedContainerName>();
                    byBuilding.Add(name.Building, bucket);
                    order.Add(name.Building);
                }

                bucket.Add(name);
            }

            order.Sort(StringComparer.Ordinal);

            List<BuildingGroup> groups = new List<BuildingGroup>();

            foreach (string building in order)
            {
                List<ParsedContainerName> files = byBuilding[building];
                groups.Add(new BuildingGroup(building, files, DistinctDisciplines(files)));
            }

            return new BuildingGroupingResult(groups, unreadable);
        }

        public static BuildingGroupingResult GroupNames(
            IEnumerable<string> names, ContainerNameSettings settings)
        {
            return Group(ContainerName.ParseAll(names, settings));
        }

        private static List<string> DistinctDisciplines(IEnumerable<ParsedContainerName> files)
        {
            List<string> disciplines = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (ParsedContainerName file in files)
            {
                if (seen.Add(file.Discipline))
                {
                    disciplines.Add(file.Discipline);
                }
            }

            disciplines.Sort(StringComparer.Ordinal);
            return disciplines;
        }
    }
}
