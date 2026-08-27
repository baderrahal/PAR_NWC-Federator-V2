using System;

namespace Federator.Core.Naming
{
    /// <summary>
    /// Where each field sits inside a container name, and what the output name carries.
    /// Part numbers are one based, matching how the naming standard is written down.
    /// </summary>
    public sealed class ContainerNameSettings
    {
        public const char DefaultSeparator = '-';
        public const int DefaultProjectPart = 1;
        public const int DefaultOriginatorPart = 2;
        public const int DefaultBuildingPart = 3;
        public const int DefaultLevelPart = 4;
        public const int DefaultDisciplinePart = 5;
        public const int DefaultNumberPart = 7;
        public const string DefaultOutputDisciplineCode = "BM";
        public const string DefaultForcedLevel = "ZZZ";
        public const string DefaultForcedNumber = "000001";

        public ContainerNameSettings()
        {
            Separator = DefaultSeparator;
            ProjectPart = DefaultProjectPart;
            OriginatorPart = DefaultOriginatorPart;
            BuildingPart = DefaultBuildingPart;
            DisciplinePart = DefaultDisciplinePart;
            OutputDisciplineCode = DefaultOutputDisciplineCode;
            LevelPart = DefaultLevelPart;
            ForcedLevel = DefaultForcedLevel;
            NumberPart = DefaultNumberPart;
            ForcedNumber = DefaultForcedNumber;
        }

        public char Separator { get; set; }

        /// <summary>The project code. Every file in a group has to agree on it.</summary>
        public int ProjectPart { get; set; }

        /// <summary>The originator. Every file in a group has to agree on it.</summary>
        public int OriginatorPart { get; set; }

        public int BuildingPart { get; set; }

        public int DisciplinePart { get; set; }

        public string OutputDisciplineCode { get; set; }

        /// <summary>
        /// One based position of the level part, paired with <see cref="ForcedLevel"/>.
        /// The level is fixed on output because outputs overwrite, and because the files
        /// in one group may disagree on it. Set both to null to carry the input level
        /// through instead.
        /// </summary>
        public int? LevelPart { get; set; }

        public string ForcedLevel { get; set; }

        /// <summary>
        /// One based position of the sequence number, paired with <see cref="ForcedNumber"/>.
        /// Fixed on output for the same reason as the level.
        /// </summary>
        public int? NumberPart { get; set; }

        public string ForcedNumber { get; set; }

        /// <summary>
        /// A name has to split into at least this many parts before every field the
        /// settings ask for can be read, and before an output name can be built.
        /// </summary>
        public int MinimumParts
        {
            get
            {
                int minimum = Math.Max(BuildingPart, DisciplinePart);
                minimum = Math.Max(minimum, Math.Max(ProjectPart, OriginatorPart));

                if (LevelPart.HasValue)
                {
                    minimum = Math.Max(minimum, LevelPart.Value);
                }

                if (NumberPart.HasValue)
                {
                    minimum = Math.Max(minimum, NumberPart.Value);
                }

                return minimum;
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

            if (string.IsNullOrEmpty(OutputDisciplineCode))
            {
                throw new ArgumentException("OutputDisciplineCode cannot be empty.");
            }

            RequirePaired(LevelPart, ForcedLevel, "LevelPart", "ForcedLevel");
            RequirePaired(NumberPart, ForcedNumber, "NumberPart", "ForcedNumber");
        }

        public ContainerNameSettings Copy()
        {
            return new ContainerNameSettings
            {
                Separator = Separator,
                ProjectPart = ProjectPart,
                OriginatorPart = OriginatorPart,
                BuildingPart = BuildingPart,
                DisciplinePart = DisciplinePart,
                OutputDisciplineCode = OutputDisciplineCode,
                LevelPart = LevelPart,
                ForcedLevel = ForcedLevel,
                NumberPart = NumberPart,
                ForcedNumber = ForcedNumber
            };
        }

        /// <summary>
        /// Settings that carry the input level and number through to the output name
        /// instead of fixing them. Outputs would then no longer overwrite each other,
        /// so this is for reading a name apart, not for naming a federation.
        /// </summary>
        public static ContainerNameSettings WithoutFixedLevelAndNumber()
        {
            return new ContainerNameSettings
            {
                LevelPart = null,
                ForcedLevel = null,
                NumberPart = null,
                ForcedNumber = null
            };
        }

        private static void RequireOneBased(int value, string name)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(name, value, "Part positions are one based.");
            }
        }

        private static void RequirePaired(int? position, string value, string positionName, string valueName)
        {
            if (position.HasValue == (value != null))
            {
                if (position.HasValue && position.Value < 1)
                {
                    throw new ArgumentOutOfRangeException(
                        positionName, position.Value, "Part positions are one based.");
                }

                return;
            }

            throw new ArgumentException(
                positionName + " and " + valueName + " have to be set together, or both left unset.");
        }
    }
}
