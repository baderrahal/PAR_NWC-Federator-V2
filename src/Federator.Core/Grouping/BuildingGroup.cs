using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Naming;

namespace Federator.Core.Grouping
{
    /// <summary>
    /// Every file that belongs to one group. What makes a group is the chosen
    /// <see cref="GroupingMode"/>, and per building is the default.
    /// </summary>
    public sealed class BuildingGroup
    {
        internal BuildingGroup(
            string building,
            string project,
            string originator,
            IList<ParsedContainerName> files,
            IList<string> disciplines,
            string buildingCode,
            string disciplineCode)
        {
            Building = building;
            Project = project;
            Originator = originator;
            Files = new ReadOnlyCollection<ParsedContainerName>(files);
            Disciplines = new ReadOnlyCollection<string>(disciplines);
            BuildingCode = buildingCode;
            DisciplineCode = disciplineCode;
        }

        /// <summary>
        /// What this group is called in the table and the log. The building code when
        /// grouping per building, and the building and discipline together, or the
        /// discipline alone, under the other modes.
        /// </summary>
        public string Building { get; private set; }

        /// <summary>
        /// The building code for the output name, or empty when the group spans more than
        /// one building and the name has to carry the all buildings code instead.
        /// </summary>
        public string BuildingCode { get; private set; }

        /// <summary>
        /// The discipline for the output name, or empty when the group spans more than one
        /// discipline and the name has to carry the pattern's discipline instead.
        /// </summary>
        public string DisciplineCode { get; private set; }

        /// <summary>
        /// A group with fewer than two disciplines cannot clash, whatever the tests say,
        /// because every clash test here is one discipline against another. One NWC is
        /// the plain case, and two NWCs of the same discipline are the same case, which
        /// is why the count is of disciplines and not of files. D5, 2026-09-12. The
        /// clash step still creates every test so the NWF matches the others, and then
        /// records them as not run for this reason rather than as a side finding nothing.
        /// </summary>
        public bool IsSingleDiscipline
        {
            get { return CannotClashWith(Disciplines.Count); }
        }

        /// <summary>
        /// The one rule, so the window row and the engine read the same answer off the
        /// same count: fewer than two disciplines and nothing here can clash.
        /// </summary>
        public static bool CannotClashWith(int disciplineCount)
        {
            return disciplineCount < 2;
        }

        /// <summary>The project code every file in this group agreed on.</summary>
        public string Project { get; private set; }

        /// <summary>The originator every file in this group agreed on.</summary>
        public string Originator { get; private set; }

        public ReadOnlyCollection<ParsedContainerName> Files { get; private set; }

        /// <summary>Distinct disciplines present in this building, in sorted order.</summary>
        public ReadOnlyCollection<string> Disciplines { get; private set; }

        public int FileCount
        {
            get { return Files.Count; }
        }

        public override string ToString()
        {
            return Building + ": " + FileCount + " files, disciplines "
                + string.Join(", ", new List<string>(Disciplines).ToArray());
        }
    }

    /// <summary>
    /// A building whose files disagree about something the output name is built from.
    /// It is reported and skipped. Picking one of the two values would put a wrong
    /// name on a federation.
    /// </summary>
    public sealed class SkippedBuildingGroup
    {
        internal SkippedBuildingGroup(string building, string reason, IList<ParsedContainerName> files)
        {
            Building = building;
            Reason = reason;
            Files = new ReadOnlyCollection<ParsedContainerName>(files);
        }

        public string Building { get; private set; }

        public string Reason { get; private set; }

        public ReadOnlyCollection<ParsedContainerName> Files { get; private set; }

        internal int FileCount
        {
            get { return Files.Count; }
        }

        public override string ToString()
        {
            return Building + " skipped: " + Reason;
        }
    }

    /// <summary>
    /// The groups that can be federated, the ones that were skipped, and every name
    /// that could not be read. Unreadable names are never folded into a group.
    /// </summary>
    public sealed class BuildingGroupingResult
    {
        internal BuildingGroupingResult(
            IList<BuildingGroup> groups,
            IList<SkippedBuildingGroup> skipped,
            IList<ParsedContainerName> unreadable)
        {
            Groups = new ReadOnlyCollection<BuildingGroup>(groups);
            Skipped = new ReadOnlyCollection<SkippedBuildingGroup>(skipped);
            Unreadable = new ReadOnlyCollection<ParsedContainerName>(unreadable);
        }

        public ReadOnlyCollection<BuildingGroup> Groups { get; private set; }

        /// <summary>Buildings whose files disagreed, reported by name with the reason.</summary>
        public ReadOnlyCollection<SkippedBuildingGroup> Skipped { get; private set; }

        internal ReadOnlyCollection<ParsedContainerName> Unreadable { get; private set; }

        internal int GroupCount
        {
            get { return Groups.Count; }
        }

        internal BuildingGroup Find(string building)
        {
            if (building == null)
            {
                throw new ArgumentNullException("building");
            }

            foreach (BuildingGroup group in Groups)
            {
                if (string.Equals(group.Building, building, StringComparison.Ordinal))
                {
                    return group;
                }
            }

            return null;
        }

        internal SkippedBuildingGroup FindSkipped(string building)
        {
            if (building == null)
            {
                throw new ArgumentNullException("building");
            }

            foreach (SkippedBuildingGroup skipped in Skipped)
            {
                if (string.Equals(skipped.Building, building, StringComparison.Ordinal))
                {
                    return skipped;
                }
            }

            return null;
        }
    }
}
