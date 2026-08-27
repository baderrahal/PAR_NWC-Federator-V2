using System;
using System.Collections.Generic;
using System.IO;

namespace Federator.Core.Naming
{
    /// <summary>
    /// Reads a container name such as 1104-PAR-1C07BC-ZZZ-AR-MOD-000001 and builds the
    /// matching output name. The separator and the part positions come from
    /// <see cref="ContainerNameSettings"/>.
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

            string building = parts[settings.BuildingPart - 1];
            string discipline = parts[settings.DisciplinePart - 1];

            if (building.Length == 0)
            {
                return ParsedContainerName.Unreadable(
                    name, stem, parts, "Part " + settings.BuildingPart + ", the building, was empty.");
            }

            if (discipline.Length == 0)
            {
                return ParsedContainerName.Unreadable(
                    name, stem, parts, "Part " + settings.DisciplinePart + ", the discipline, was empty.");
            }

            return ParsedContainerName.Readable(name, stem, parts, building, discipline);
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

        /// <summary>
        /// The output name for a federation. The discipline becomes the output code and
        /// every other part is kept. When the settings name a level or a number position
        /// together with a forced value, those parts are overwritten as well.
        /// </summary>
        public static string BuildOutputName(ParsedContainerName parsed, ContainerNameSettings settings)
        {
            if (parsed == null)
            {
                throw new ArgumentNullException("parsed");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (!parsed.IsReadable)
            {
                throw new InvalidOperationException(
                    "An output name cannot be built from an unreadable name: " + parsed.UnreadableReason);
            }

            settings.Validate();

            List<string> parts = new List<string>(parsed.Parts);
            parts[settings.DisciplinePart - 1] = settings.OutputDisciplineCode;

            ApplyForcedPart(parts, settings.LevelPart, settings.ForcedLevel, "LevelPart");
            ApplyForcedPart(parts, settings.NumberPart, settings.ForcedNumber, "NumberPart");

            return string.Join(settings.Separator.ToString(), parts.ToArray());
        }

        public static string BuildOutputName(ParsedContainerName parsed)
        {
            return BuildOutputName(parsed, new ContainerNameSettings());
        }

        public static string BuildOutputName(string name, ContainerNameSettings settings)
        {
            return BuildOutputName(Parse(name, settings), settings);
        }

        private static void ApplyForcedPart(
            IList<string> parts, int? position, string value, string settingName)
        {
            if (!position.HasValue && value == null)
            {
                return;
            }

            if (!position.HasValue || value == null)
            {
                throw new ArgumentException(
                    settingName + " and its forced value have to be set together, or both left unset.");
            }

            if (position.Value < 1 || position.Value > parts.Count)
            {
                throw new ArgumentOutOfRangeException(
                    settingName, position.Value, "The name has " + parts.Count + " parts.");
            }

            parts[position.Value - 1] = value;
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
