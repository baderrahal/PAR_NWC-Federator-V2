using System;
using System.Collections.Generic;
using Federator.Core.Naming;

namespace Federator.Core.Grouping
{
    /// <summary>
    /// Gathers parsed container names into federations.
    ///
    /// Per building is the default and is what this project runs weekly, with every
    /// discipline of a building landing in the same group. It is a default rather than a
    /// rule, because a coordination meeting about one discipline across a whole site wants
    /// a different split from a building handover. See <see cref="GroupingMode"/>.
    ///
    /// Whatever the mode, a group whose files disagree on the project code or the
    /// originator is reported and skipped, because both go into the output name and
    /// picking one of two values would put a wrong name on a federation.
    /// </summary>
    public static class BuildingGrouping
    {
        public static BuildingGroupingResult Group(IEnumerable<ParsedContainerName> names)
        {
            return Group(names, GroupingModes.Default, new ContainerNameSettings());
        }

        public static BuildingGroupingResult Group(
            IEnumerable<ParsedContainerName> names, GroupingMode mode)
        {
            return Group(names, mode, new ContainerNameSettings());
        }

        public static BuildingGroupingResult Group(
            IEnumerable<ParsedContainerName> names, GroupingMode mode, ContainerNameSettings settings)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            List<ParsedContainerName> unreadable = new List<ParsedContainerName>();
            List<string> order = new List<string>();
            Dictionary<string, List<ParsedContainerName>> byKey =
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

                string key = KeyFor(name, mode, settings);
                List<ParsedContainerName> bucket;

                if (!byKey.TryGetValue(key, out bucket))
                {
                    bucket = new List<ParsedContainerName>();
                    byKey.Add(key, bucket);
                    order.Add(key);
                }

                bucket.Add(name);
            }

            order.Sort(StringComparer.Ordinal);

            List<BuildingGroup> groups = new List<BuildingGroup>();
            List<SkippedBuildingGroup> skipped = new List<SkippedBuildingGroup>();

            foreach (string key in order)
            {
                List<ParsedContainerName> files = byKey[key];
                string disagreement = FirstDisagreement(files);

                if (disagreement != null)
                {
                    skipped.Add(new SkippedBuildingGroup(key, disagreement, files));
                    continue;
                }

                groups.Add(new BuildingGroup(
                    key,
                    files[0].Project,
                    files[0].Originator,
                    files,
                    DistinctDisciplines(files),
                    GroupingModes.OneBuildingPerGroup(mode) ? files[0].Building : string.Empty,
                    GroupingModes.OneDisciplinePerGroup(mode) ? files[0].Discipline : string.Empty));
            }

            return new BuildingGroupingResult(groups, skipped, unreadable);
        }

        /// <summary>
        /// What gathers files into one group, and what that group is called.
        /// </summary>
        public static string KeyFor(
            ParsedContainerName name, GroupingMode mode, ContainerNameSettings settings)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            switch (mode)
            {
                case GroupingMode.PerBuildingAndDiscipline:
                    return name.Building + settings.Separator + name.Discipline;
                case GroupingMode.PerDiscipline:
                    return name.Discipline;
                case GroupingMode.Everything:
                    return GroupingModes.EverythingKey;
                default:
                    return name.Building;
            }
        }

        public static BuildingGroupingResult GroupNames(
            IEnumerable<string> names, ContainerNameSettings settings)
        {
            return GroupNames(names, settings, GroupingModes.Default);
        }

        public static BuildingGroupingResult GroupNames(
            IEnumerable<string> names, ContainerNameSettings settings, GroupingMode mode)
        {
            return Group(ContainerName.ParseAll(names, settings), mode, settings);
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
