using System;

namespace Federator.Core.Naming
{
    /// <summary>
    /// Where the building and discipline sit inside a container name, and what the
    /// output name carries. Part numbers are one based, matching how the naming
    /// standard is written down.
    /// </summary>
    public sealed class ContainerNameSettings
    {
        public const char DefaultSeparator = '-';
        public const int DefaultBuildingPart = 3;
        public const int DefaultDisciplinePart = 5;
        public const string DefaultOutputDisciplineCode = "BM";

        public ContainerNameSettings()
        {
            Separator = DefaultSeparator;
            BuildingPart = DefaultBuildingPart;
            DisciplinePart = DefaultDisciplinePart;
            OutputDisciplineCode = DefaultOutputDisciplineCode;
        }

        public char Separator { get; set; }

        public int BuildingPart { get; set; }

        public int DisciplinePart { get; set; }

        public string OutputDisciplineCode { get; set; }

        /// <summary>
        /// One based position of the level part, paired with <see cref="ForcedLevel"/>.
        /// Both null means the output name keeps whatever level the input carried.
        /// </summary>
        public int? LevelPart { get; set; }

        public string ForcedLevel { get; set; }

        /// <summary>
        /// One based position of the sequence number, paired with <see cref="ForcedNumber"/>.
        /// Both null means the output name keeps whatever number the input carried.
        /// </summary>
        public int? NumberPart { get; set; }

        public string ForcedNumber { get; set; }

        /// <summary>
        /// A name has to split into at least this many parts before both the building
        /// and the discipline can be read out of it.
        /// </summary>
        public int MinimumParts
        {
            get { return Math.Max(BuildingPart, DisciplinePart); }
        }

        public void Validate()
        {
            if (BuildingPart < 1)
            {
                throw new ArgumentOutOfRangeException("BuildingPart", BuildingPart, "Part positions are one based.");
            }

            if (DisciplinePart < 1)
            {
                throw new ArgumentOutOfRangeException("DisciplinePart", DisciplinePart, "Part positions are one based.");
            }

            if (BuildingPart == DisciplinePart)
            {
                throw new ArgumentException("BuildingPart and DisciplinePart cannot be the same position.");
            }

            if (string.IsNullOrEmpty(OutputDisciplineCode))
            {
                throw new ArgumentException("OutputDisciplineCode cannot be empty.");
            }
        }

        public ContainerNameSettings Copy()
        {
            return new ContainerNameSettings
            {
                Separator = Separator,
                BuildingPart = BuildingPart,
                DisciplinePart = DisciplinePart,
                OutputDisciplineCode = OutputDisciplineCode,
                LevelPart = LevelPart,
                ForcedLevel = ForcedLevel,
                NumberPart = NumberPart,
                ForcedNumber = ForcedNumber
            };
        }
    }
}
