using System;
using System.Collections.Generic;
using System.IO;

namespace Federator.Core.Naming
{
    /// <summary>
    /// Reads a container name such as 1104-PAR-1C07BC-ZZZ-AR-MOD-000001 into its parts.
    /// The separator and the part positions come from <see cref="ContainerNameSettings"/>.
    /// The output name is NamePattern's, built from a pattern with defaults, never from
    /// here.
    /// </summary>
    public static class ContainerName
    {
        public static ParsedContainerName Parse(string name)
        {
            return Parse(name, new ContainerNameSettings());
        }

        public static ParsedContainerName Parse(string name, ContainerNameSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            settings.Validate();

            if (name == null)
            {
                return ParsedContainerName.Unreadable(null, null, null, "The name was null.");
            }

            string stem = Stem(name);

            if (stem.Length == 0)
            {
                return ParsedContainerName.Unreadable(name, stem, null, "The name was empty.");
            }

            List<string> parts = new List<string>(stem.Split(settings.Separator));

            if (parts.Count < settings.MinimumParts)
            {
                return ParsedContainerName.Unreadable(
                    name,
                    stem,
                    parts,
                    "Split on " + Quoted(settings.Separator) + " gave " + parts.Count + " parts, and "
                        + settings.MinimumParts + " are needed to read the building at part "
                        + settings.BuildingPart + " and the discipline at part "
                        + settings.DisciplinePart + ".");
            }

            string project = parts[settings.ProjectPart - 1];
            string originator = parts[settings.OriginatorPart - 1];
            string building = parts[settings.BuildingPart - 1];
            string discipline = parts[settings.DisciplinePart - 1];

            string empty = FirstEmpty(
                new[] { settings.ProjectPart, settings.OriginatorPart, settings.BuildingPart, settings.DisciplinePart },
                new[] { project, originator, building, discipline },
                new[] { "the project code", "the originator", "the building", "the discipline" });

            if (empty != null)
            {
                return ParsedContainerName.Unreadable(name, stem, parts, empty);
            }

            return ParsedContainerName.Readable(name, stem, parts, project, originator, building, discipline);
        }

        private static string FirstEmpty(int[] positions, string[] values, string[] labels)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].Length == 0)
                {
                    return "Part " + positions[i] + ", " + labels[i] + ", was empty.";
                }
            }

            return null;
        }

        /// <summary>
        /// Parses a run of names in one call. Readable and unreadable names both come
        /// back, in the order they were given.
        /// </summary>
        public static IList<ParsedContainerName> ParseAll(
            IEnumerable<string> names, ContainerNameSettings settings)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }

            List<ParsedContainerName> parsed = new List<ParsedContainerName>();

            foreach (string name in names)
            {
                parsed.Add(Parse(name, settings));
            }

            return parsed;
        }

        private static string Quoted(char separator)
        {
            return "\"" + separator + "\"";
        }

        private static string Stem(string name)
        {
            string trimmed = name.Trim();

            if (trimmed.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                string fileName = Path.GetFileName(trimmed);
                string withoutExtension = Path.GetFileNameWithoutExtension(fileName);
                return string.IsNullOrEmpty(withoutExtension) ? fileName : withoutExtension;
            }
            catch (ArgumentException)
            {
                // The string holds characters that are not legal in a path. Read it as a
                // bare name rather than refusing it here.
                return trimmed;
            }
        }
    }
}
