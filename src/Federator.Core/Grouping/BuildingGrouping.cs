using System;
using System.Collections.Generic;
using Federator.Core.Naming;

namespace Federator.Core.Grouping
{
    /// <summary>
    /// Groups parsed container names on the full building code. Every discipline of a
    /// building lands in the same group. A building whose files disagree on the project
    /// code or the originator is reported and skipped.
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
            List<SkippedBuildingGroup> skipped = new List<SkippedBuildingGroup>();

            foreach (string building in order)
            {
                List<ParsedContainerName> files = byBuilding[building];
                string disagreement = FirstDisagreement(files);

                if (disagreement != null)
                {
                    skipped.Add(new SkippedBuildingGroup(building, disagreement, files));
                    continue;
                }

                groups.Add(new BuildingGroup(
                    building,
                    files[0].Project,
                    files[0].Originator,
                    files,
                    DistinctDisciplines(files)));
            }

            return new BuildingGroupingResult(groups, skipped, unreadable);
        }

        public static BuildingGroupingResult GroupNames(
            IEnumerable<string> names, ContainerNameSettings settings)
        {
            return Group(ContainerName.ParseAll(names, settings));
        }

        /// <summary>
        /// The project code and the originator go into the output name, so every file in
        /// a group has to agree on both. Reports the first disagreement found, naming the
        /// two values and the file that broke ranks, or null when the files agree.
        /// </summary>
        private static string FirstDisagreement(IList<ParsedContainerName> files)
        {
            string reason = Disagreement(files, "project code", true);

            return reason ?? Disagreement(files, "originator", false);
        }

        private static string Disagreement(IList<ParsedContainerName> files, string label, bool project)
        {
            string expected = project ? files[0].Project : files[0].Originator;

            for (int i = 1; i < files.Count; i++)
            {
                string actual = project ? files[i].Project : files[i].Originator;

                if (!string.Equals(expected, actual, StringComparison.Ordinal))
                {
                    return "The files disagree on the " + label + ". "
                        + files[0].Stem + " says " + expected + " and "
                        + files[i].Stem + " says " + actual + ".";
                }
            }

            return null;
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
