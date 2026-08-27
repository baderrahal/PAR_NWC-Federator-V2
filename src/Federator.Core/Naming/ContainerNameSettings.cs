using System;

namespace Federator.Core.Naming
{
    /// <summary>
    /// Where each field is read from inside a container name, and what the output name
    /// is built from. Part numbers are one based, matching how the naming standard is
    /// written down.
    /// </summary>
    public sealed class ContainerNameSettings
    {
        public const char DefaultSeparator = '-';
        public const int DefaultProjectPart = 1;
        public const int DefaultOriginatorPart = 2;
        public const int DefaultBuildingPart = 3;
        public const int DefaultDisciplinePart = 5;
        public const string DefaultOutputDisciplineCode = "BM";
        public const string DefaultForcedLevel = "ZZZ";
        public const string DefaultForcedTypeCode = "MOD";
        public const string DefaultForcedNumber = "000001";

        public ContainerNameSettings()
        {
            Separator = DefaultSeparator;
            ProjectPart = DefaultProjectPart;
            OriginatorPart = DefaultOriginatorPart;
            BuildingPart = DefaultBuildingPart;
            DisciplinePart = DefaultDisciplinePart;
            OutputDisciplineCode = DefaultOutputDisciplineCode;
            ForcedLevel = DefaultForcedLevel;
            ForcedTypeCode = DefaultForcedTypeCode;
            ForcedNumber = DefaultForcedNumber;
        }

        public char Separator { get; set; }

        /// <summary>The project code. Every file in a group has to agree on it.</summary>
        public int ProjectPart { get; set; }

        /// <summary>The originator. Every file in a group has to agree on it.</summary>
        public int OriginatorPart { get; set; }

        public int BuildingPart { get; set; }

        public int DisciplinePart { get; set; }

        /// <summary>What the discipline field of the output name carries.</summary>
        public string OutputDisciplineCode { get; set; }

        /// <summary>
        /// What the level field of the output name carries. Fixed because outputs
        /// overwrite, and because the files in one group may disagree on the level.
        /// </summary>
        public string ForcedLevel { get; set; }

        /// <summary>
        /// What the type field of the output name carries. Fixed for the same reason as
        /// the level.
        /// </summary>
        public string ForcedTypeCode { get; set; }

        /// <summary>
        /// What the number field of the output name carries. Fixed for the same reason as
        /// the level.
        /// </summary>
        public string ForcedNumber { get; set; }

        /// <summary>
        /// A name has to split into at least this many parts before every field the
        /// output name is built from can be read out of it. Only parts 1, 2, 3 and 5 are
        /// read, so the floor is 5. The level, the type code and the number are never
        /// read from the input, so they never raise it.
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

            RequireValue(OutputDisciplineCode, "OutputDisciplineCode");
            RequireValue(ForcedLevel, "ForcedLevel");
            RequireValue(ForcedTypeCode, "ForcedTypeCode");
            RequireValue(ForcedNumber, "ForcedNumber");
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
                ForcedLevel = ForcedLevel,
                ForcedTypeCode = ForcedTypeCode,
                ForcedNumber = ForcedNumber
            };
        }

        private static void RequireOneBased(int value, string name)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(name, value, "Part positions are one based.");
            }
        }

        private static void RequireValue(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(name + " cannot be empty.");
            }
        }
    }
}
