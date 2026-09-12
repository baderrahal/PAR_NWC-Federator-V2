using System;

namespace Federator.Core.Naming
{
    /// <summary>
    /// Where each field is read from inside a container name. Part numbers are one
    /// based, matching how the naming standard is written down. What the output name
    /// carries is NamePattern's, a pattern with defaults, and was never read from here
    /// once the patterns existed, F36.
    /// </summary>
    public sealed class ContainerNameSettings
    {
        public const char DefaultSeparator = '-';
        public const int DefaultProjectPart = 1;
        public const int DefaultOriginatorPart = 2;
        public const int DefaultBuildingPart = 3;
        public const int DefaultDisciplinePart = 5;

        public ContainerNameSettings()
        {
            Separator = DefaultSeparator;
            ProjectPart = DefaultProjectPart;
            OriginatorPart = DefaultOriginatorPart;
            BuildingPart = DefaultBuildingPart;
            DisciplinePart = DefaultDisciplinePart;
        }

        public char Separator { get; set; }

        /// <summary>The project code. Every file in a group has to agree on it.</summary>
        public int ProjectPart { get; set; }

        /// <summary>The originator. Every file in a group has to agree on it.</summary>
        public int OriginatorPart { get; set; }

        public int BuildingPart { get; set; }

        public int DisciplinePart { get; set; }

        /// <summary>
        /// A name has to split into at least this many parts before every field the
        /// output name is built from can be read out of it. Only parts 1, 2, 3 and 5 are
        /// read, so the floor is 5. The level, the type code and the number come from the
        /// pattern and are never read from the input, so they never raise it.
        /// </summary>
        public int MinimumParts
        {
            get
            {
                int minimum = Math.Max(BuildingPart, DisciplinePart);
                return Math.Max(minimum, Math.Max(ProjectPart, OriginatorPart));
            }
        }

        public void Validate()
        {
            RequireOneBased(ProjectPart, "ProjectPart");
            RequireOneBased(OriginatorPart, "OriginatorPart");
            RequireOneBased(BuildingPart, "BuildingPart");
            RequireOneBased(DisciplinePart, "DisciplinePart");

            if (BuildingPart == DisciplinePart)
            {
                throw new ArgumentException("BuildingPart and DisciplinePart cannot be the same position.");
            }

            if (ProjectPart == OriginatorPart)
            {
                throw new ArgumentException("ProjectPart and OriginatorPart cannot be the same position.");
            }
        }

        public ContainerNameSettings Copy()
        {
            return new ContainerNameSettings
            {
                Separator = Separator,
                ProjectPart = ProjectPart,
                OriginatorPart = OriginatorPart,
                BuildingPart = BuildingPart,
                DisciplinePart = DisciplinePart
            };
        }

        private static void RequireOneBased(int value, string name)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(name, value, "Part positions are one based.");
            }
        }
    }
}
