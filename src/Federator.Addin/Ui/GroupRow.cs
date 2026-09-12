using System.Collections.Generic;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using Federator.Core.Rerun;

namespace Federator.Addin.Ui
{
    /// <summary>
    /// One group in the tables. A group whose files disagree on the project code or the
    /// originator arrives blocked, showing both offending file names, and cannot be ticked.
    ///
    /// The three output names are a view onto the row in the name table, so typing one in
    /// the grid marks that one name as typed over and a later pattern change leaves it
    /// alone. Nothing here holds a second copy of a name.
    /// </summary>
    public sealed class GroupRow : ObservableObject
    {
        private bool include;
        private readonly OutputNameRow names;

        private GroupRow(
            string building, IList<string> files, IList<string> disciplines, OutputNameRow names)
        {
            Building = building;
            Files = files;
            Disciplines = string.Join(", ", new List<string>(disciplines).ToArray());
            DisciplineCount = disciplines.Count;
            this.names = names;
        }

        public static GroupRow Usable(
            string building,
            IList<string> files,
            IList<string> disciplines,
            OutputNameRow names)
        {
            GroupRow row = new GroupRow(building, files, disciplines, names);
            row.include = true;
            return row;
        }

        public static GroupRow Blocked(string building, IList<string> files, string reason)
        {
            GroupRow row = new GroupRow(building, files, new List<string>(), null);
            row.BlockedReason = reason;
            row.include = false;
            return row;
        }

        public string Building { get; private set; }

        /// <summary>Full paths, in the order the files will be appended.</summary>
        public IList<string> Files { get; private set; }

        public int FileCount
        {
            get { return Files.Count; }
        }

        public string Disciplines { get; private set; }

        /// <summary>How many disciplines the scan found in this group, handed on to the job.</summary>
        public int DisciplineCount { get; private set; }

        public string NwfName
        {
            get { return NameOf(OutputKind.Nwf); }
            set { TypeOver(OutputKind.Nwf, value, "NwfName"); }
        }

        public string NwdName
        {
            get { return NameOf(OutputKind.Nwd); }
            set { TypeOver(OutputKind.Nwd, value, "NwdName"); }
        }

        public string WorkbookName
        {
            get { return NameOf(OutputKind.Workbook); }
            set { TypeOver(OutputKind.Workbook, value, "WorkbookName"); }
        }

        /// <summary>
        /// The name this group is known by in the log and the group list. The NWF is the
        /// one that carries the clash history, so it is the one that names the group.
        /// </summary>
        public string OutputName
        {
            get { return NwfName; }
        }

        /// <summary>Says at a glance which rows a pattern change will leave alone.</summary>
        public string EditedMark
        {
            get { return names != null && names.WasEdited ? "by hand" : string.Empty; }
        }

        private string NameOf(OutputKind kind)
        {
            return names == null ? string.Empty : names.Get(kind);
        }

        private void TypeOver(OutputKind kind, string value, string property)
        {
            if (names == null)
            {
                return;
            }

            string tidied = value == null ? string.Empty : value.Trim();

            if (string.Equals(names.Get(kind), tidied, System.StringComparison.Ordinal)
                && names.IsByHand(kind))
            {
                return;
            }

            names.SetByHand(kind, tidied);
            Raise(property);
            Raise("EditedMark");
        }

        /// <summary>Tells the grid the names underneath it have been refilled.</summary>
        public void NamesRefilled()
        {
            Raise("NwfName");
            Raise("NwdName");
            Raise("WorkbookName");
            Raise("EditedMark");
        }

        /// <summary>Null when the group is usable.</summary>
        public string BlockedReason { get; private set; }

        public bool IsBlocked
        {
            get { return BlockedReason != null; }
        }

        public bool CanInclude
        {
            get { return !IsBlocked; }
        }

        /// <summary>
        /// Fewer than two disciplines cannot clash, by the one rule in BuildingGroup. The
        /// tests are still created so the NWF matches the others, and none of them is run.
        /// </summary>
        public bool IsSingleDiscipline
        {
            get { return BuildingGroup.CannotClashWith(DisciplineCount); }
        }

        public bool Include
        {
            get { return include; }
            set
            {
                if (include == value)
                {
                    return;
                }

                include = value && !IsBlocked;
                Raise("Include");
            }
        }

        private string runAs = RunPath.Unknown;

        /// <summary>
        /// Which of the two workflows this group is expected to take, in the words of
        /// Federator.Core.Rerun.RunPath, worked out before Run from whether the NWF is
        /// already at its output path and whether an XML is picked. Shown in the group
        /// list so the person knows before pressing anything.
        /// </summary>
        public string RunAs
        {
            get { return IsBlocked ? string.Empty : runAs; }
            set
            {
                string tidied = value ?? RunPath.Unknown;

                if (string.Equals(runAs, tidied, System.StringComparison.Ordinal))
                {
                    return;
                }

                runAs = tidied;
                Raise("RunAs");
            }
        }

        public string Status
        {
            get
            {
                if (IsBlocked)
                {
                    return "BLOCKED: " + BlockedReason;
                }

                return IsSingleDiscipline
                    ? "Ready. One discipline, so every test is created and none is run."
                    : "Ready";
            }
        }

        public override string ToString()
        {
            return Building;
        }
    }
}
