using System;
using System.Collections.Generic;
using System.Text;

namespace Federator.Core.Findings
{
    /// <summary>
    /// The plain count line at the top of the Source step: what was found, what could be
    /// read, how many groups it makes, and how many findings of each kind.
    /// </summary>
    public sealed class ScanCounts
    {
        private readonly Dictionary<FindingKind, int> byKind = new Dictionary<FindingKind, int>();

        public ScanCounts()
        {
            GroupingDescription = string.Empty;
        }

        public int FilesFound { get; set; }

        public int FilesReadable { get; set; }

        public int Groups { get; set; }

        public int BlockedGroups { get; set; }

        /// <summary>How the files were gathered, so the group count means something.</summary>
        public string GroupingDescription { get; set; }

        public int FilesUnreadable
        {
            get { return FilesFound - FilesReadable; }
        }

        public void Count(IEnumerable<ScanFinding> findings)
        {
            if (findings == null)
            {
                return;
            }

            foreach (ScanFinding finding in findings)
            {
                if (finding == null)
                {
                    continue;
                }

                int already;
                byKind.TryGetValue(finding.Kind, out already);
                byKind[finding.Kind] = already + 1;
            }
        }

        public int Of(FindingKind kind)
        {
            int count;
            return byKind.TryGetValue(kind, out count) ? count : 0;
        }

        public int TotalFindings
        {
            get
            {
                int total = 0;

                foreach (KeyValuePair<FindingKind, int> pair in byKind)
                {
                    total += pair.Value;
                }

                return total;
            }
        }

        /// <summary>The words for one kind, in the plain form the panel uses.</summary>
        public static string Describe(FindingKind kind)
        {
            switch (kind)
            {
                case FindingKind.OddShape:
                    return "written differently from the rest";
                case FindingKind.NearMatch:
                    return "two codes that look almost the same";
                case FindingKind.SingleDiscipline:
                    return "only one discipline, so nothing to clash";
                case FindingKind.MissingDisciplines:
                    return "missing a discipline others have";
                case FindingKind.SourceMismatch:
                    return "NWC and Revit names disagree";
                case FindingKind.SharedSourceBuilding:
                    return "two federations from one Revit building";
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>The order the kinds are listed in, so two runs read the same.</summary>
        public static FindingKind[] Kinds()
        {
            return new[]
            {
                FindingKind.OddShape,
                FindingKind.NearMatch,
                FindingKind.SingleDiscipline,
                FindingKind.MissingDisciplines,
                FindingKind.SourceMismatch,
                FindingKind.SharedSourceBuilding
            };
        }

        /// <summary>
        /// The count line. Plain numbers first, then the findings by kind, so a person can
        /// see the size of what they are looking at before reading any of it.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            lines.Add(FilesFound + (FilesFound == 1 ? " file found, " : " files found, ")
                + FilesReadable + " readable, "
                + FilesUnreadable + " that cannot be read.");

            lines.Add(Groups + (Groups == 1 ? " group" : " groups")
                + (GroupingDescription.Length == 0
                    ? string.Empty
                    : ", " + GroupingDescription.ToLowerInvariant())
                + (BlockedGroups > 0
                    ? ", and " + BlockedGroups + " blocked because their files disagree."
                    : "."));

            if (TotalFindings == 0)
            {
                lines.Add("Nothing found worth a look.");
                return lines;
            }

            lines.Add(TotalFindings + (TotalFindings == 1 ? " thing" : " things")
                + " worth a look. None of it stops a run.");

            foreach (FindingKind kind in Kinds())
            {
                int count = Of(kind);

                if (count > 0)
                {
                    lines.Add("    " + count.ToString().PadLeft(4) + "  " + Describe(kind));
                }
            }

            return lines;
        }
    }
}
