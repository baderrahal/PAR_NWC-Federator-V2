using System.Collections.Generic;

namespace Federator.Addin.Ui
{
    /// <summary>
    /// One building. A group whose files disagree on the project code or the originator
    /// arrives blocked, showing both offending file names, and cannot be ticked.
    /// </summary>
    public sealed class GroupRow : ObservableObject
    {
        private bool include;

        private GroupRow(string building, IList<string> files, IList<string> disciplines)
        {
            Building = building;
            Files = files;
            Disciplines = string.Join(", ", new List<string>(disciplines).ToArray());
        }

        public static GroupRow Usable(
            string building,
            IList<string> files,
            IList<string> disciplines,
            string outputName)
        {
            GroupRow row = new GroupRow(building, files, disciplines);
            row.OutputName = outputName;
            row.include = true;
            return row;
        }

        public static GroupRow Blocked(string building, IList<string> files, string reason)
        {
            GroupRow row = new GroupRow(building, files, new List<string>());
            row.BlockedReason = reason;
            row.OutputName = string.Empty;
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

        public string OutputName { get; private set; }

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

        public string Status
        {
            get { return IsBlocked ? "BLOCKED: " + BlockedReason : "Ready"; }
        }

        public override string ToString()
        {
            return Building;
        }
    }
}
