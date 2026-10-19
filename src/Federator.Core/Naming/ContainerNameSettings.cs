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

        /// <summary>The number, part 7 of 1104-PAR-1B06BC-ZZZ-AR-MOD-000001, F244, which the discipline workbooks' short name reads.</summary>
        public const int DefaultNumberPart = 7;

        public ContainerNameSettings()
        {
            Separator = DefaultSeparator;
            ProjectPart = DefaultProjectPart;
            OriginatorPart = DefaultOriginatorPart;
            BuildingPart = DefaultBuildingPart;
            DisciplinePart = DefaultDisciplinePart;
            NumberPart = DefaultNumberPart;
        }

        public char Separator { get; set; }

        /// <summary>Which part of the split name carries the project code.</summary>
        public int ProjectPart { get; set; }

        /// <summary>Which part carries the originator.</summary>
        public int OriginatorPart { get; set; }

        public int BuildingPart { get; set; }

        public int DisciplinePart { get; set; }

        /// <summary>
        /// Which part carries the number. Read where the name has that many parts and never needed to read a name, so it does
        /// not raise MinimumParts and a name of five or six parts still reads, with no number.
        /// </summary>
        public int NumberPart { get; set; }

        /// <summary>
        /// A name has to split into at least this many parts before every field the
        /// output name is built from can be read out of it. Only parts 1, 2, 3 and 5 are
        /// needed, so the floor is 5. The level and the type code come from the pattern and
        /// are never read from the input. The number of the output name comes from the
        /// pattern too, and the input's number, NumberPart, is read only where the name has
        /// that part, for the discipline workbooks' short name, F244, so none of them raises it.
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
            RequireOneBased(NumberPart, "NumberPart");

            if (BuildingPart == DisciplinePart)
            {
                throw new ArgumentException("BuildingPart and DisciplinePart cannot be the same position.");
            }

            if (ProjectPart == OriginatorPart)
            {
                throw new ArgumentException("ProjectPart and OriginatorPart cannot be the same position.");
            }
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
