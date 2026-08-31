using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Grouping;

namespace Federator.Core.Naming
{
    /// <summary>Which of a row's three names is being talked about.</summary>
    public enum OutputKind
    {
        Nwf,
        Nwd,
        Workbook
    }

    /// <summary>
    /// One group's three output names, each of which can be typed over.
    ///
    /// A name that was typed over is never refilled by a pattern change. That is the whole
    /// point of the table: the patterns are a starting point, and a name someone corrected
    /// by hand stays corrected.
    /// </summary>
    public sealed class OutputNameRow
    {
        private readonly string[] names = new string[3];
        private readonly bool[] byHand = new bool[3];

        internal OutputNameRow(string group, BuildingGroup source)
        {
            Group = group;
            Source = source;
        }

        /// <summary>The group this row names, as the Grouping step calls it.</summary>
        public string Group { get; private set; }

        /// <summary>The group itself, so a refill can rebuild from the pattern.</summary>
        public BuildingGroup Source { get; private set; }

        public string NwfName
        {
            get { return Get(OutputKind.Nwf); }
        }

        public string NwdName
        {
            get { return Get(OutputKind.Nwd); }
        }

        public string WorkbookName
        {
            get { return Get(OutputKind.Workbook); }
        }

        public string Get(OutputKind kind)
        {
            return names[(int)kind] ?? string.Empty;
        }

        /// <summary>True when this name was typed over rather than filled from a pattern.</summary>
        public bool IsByHand(OutputKind kind)
        {
            return byHand[(int)kind];
        }

        /// <summary>True when any of the three was typed over.</summary>
        public bool WasEdited
        {
            get { return byHand[0] || byHand[1] || byHand[2]; }
        }

        /// <summary>
        /// Types over one name. Setting it back to what the pattern produced still counts
        /// as by hand, because the person meant that value and a later pattern change must
        /// not take it away.
        /// </summary>
        public void SetByHand(OutputKind kind, string name)
        {
            names[(int)kind] = name == null ? string.Empty : name.Trim();
            byHand[(int)kind] = true;
        }

        /// <summary>Puts a name back under the pattern's control.</summary>
        public void ReleaseToPattern(OutputKind kind)
        {
            byHand[(int)kind] = false;
        }

        internal void Fill(OutputKind kind, string name)
        {
            names[(int)kind] = name;
        }

        public override string ToString()
        {
            return Group + "  " + NwfName;
        }
    }

    /// <summary>
    /// One row per group, filled from the patterns and editable in place.
    ///
    /// The global pattern fields stay as the starting point. Changing one refills every
    /// name that has not been typed over and leaves the rest exactly as they are, and it
    /// says how many it kept, so nobody has to guess whether their edit survived.
    /// </summary>
    public sealed class OutputNameTable
    {
        private readonly List<OutputNameRow> rows = new List<OutputNameRow>();

        public OutputNameTable()
        {
            Today = DateTime.Today;
        }

        public ReadOnlyCollection<OutputNameRow> Rows
        {
            get { return new ReadOnlyCollection<OutputNameRow>(rows); }
        }

        public int Count
        {
            get { return rows.Count; }
        }

        public static OutputKind[] AllKinds()
        {
            return new[] { OutputKind.Nwf, OutputKind.Nwd, OutputKind.Workbook };
        }

        /// <summary>The day the NWD is dated with. Handed in so a test can pin it.</summary>
        public DateTime Today { get; set; }

        public static OutputNameTable From(
            IEnumerable<BuildingGroup> groups, OutputNaming naming, ContainerNameSettings settings)
        {
            return From(groups, naming, settings, DateTime.Today);
        }

        public static OutputNameTable From(
            IEnumerable<BuildingGroup> groups,
            OutputNaming naming,
            ContainerNameSettings settings,
            DateTime today)
        {
            if (groups == null)
            {
                throw new ArgumentNullException("groups");
            }

            OutputNameTable table = new OutputNameTable();
            table.Today = today;

            foreach (BuildingGroup group in groups)
            {
                table.rows.Add(new OutputNameRow(group.Building, group));
            }

            table.Refill(naming, settings);
            return table;
        }

        /// <summary>
        /// Rebuilds every name that has not been typed over, and returns how many rows were
        /// left alone because they had been.
        /// </summary>
        public int Refill(OutputNaming naming, ContainerNameSettings settings)
        {
            if (naming == null)
            {
                throw new ArgumentNullException("naming");
            }

            int kept = 0;

            foreach (OutputNameRow row in rows)
            {
                if (row.WasEdited)
                {
                    kept++;
                }

                foreach (OutputKind kind in AllKinds())
                {
                    if (row.IsByHand(kind))
                    {
                        continue;
                    }

                    row.Fill(kind, Build(
                        PatternFor(naming, kind), row.Source, settings, DateFor(naming, kind)));
                }
            }

            return kept;
        }

        /// <summary>
        /// What a pattern change would say. Worded here rather than in the window so the
        /// message and the count cannot drift apart.
        /// </summary>
        public static string DescribeRefill(int rowCount, int kept)
        {
            if (kept == 0)
            {
                return rowCount + (rowCount == 1 ? " name refilled." : " rows refilled.");
            }

            return (rowCount - kept) + " of " + rowCount + " rows refilled. "
                + kept + (kept == 1 ? " row was" : " rows were")
                + " typed over by hand and left alone.";
        }

        public static NamePattern PatternFor(OutputNaming naming, OutputKind kind)
        {
            switch (kind)
            {
                case OutputKind.Nwd:
                    return naming.Nwd;
                case OutputKind.Workbook:
                    return naming.Workbook;
                default:
                    return naming.Nwf;
            }
        }

        /// <summary>
        /// The date the NWD carries when it is being kept week by week, and nothing for the
        /// other two. The NWF always overwrites, because its clash results are the record.
        /// </summary>
        private DateTime? DateFor(OutputNaming naming, OutputKind kind)
        {
            return kind == OutputKind.Nwd ? naming.NwdDate(Today) : null;
        }

        public static string Build(
            NamePattern pattern, BuildingGroup group, ContainerNameSettings settings)
        {
            return Build(pattern, group, settings, null);
        }

        public static string Build(
            NamePattern pattern, BuildingGroup group, ContainerNameSettings settings, DateTime? on)
        {
            try
            {
                return pattern.NameFor(group, settings, on);
            }
            catch (InvalidOperationException error)
            {
                return "CANNOT BE NAMED: " + error.Message;
            }
        }

        public OutputNameRow Find(string group)
        {
            foreach (OutputNameRow row in rows)
            {
                if (string.Equals(row.Group, group, StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        /// <summary>
        /// Two rows landing on one name, for any of the three outputs. Outputs overwrite
        /// with no date suffix, so this is not a warning. The second silently destroys the
        /// first.
        /// </summary>
        public IList<NameCollision> Collisions()
        {
            List<NameCollision> found = new List<NameCollision>();
            IList<string> labels = OutputNaming.Labels();
            OutputKind[] kinds = AllKinds();

            for (int i = 0; i < kinds.Length; i++)
            {
                Dictionary<string, List<string>> byName =
                    new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                List<string> order = new List<string>();

                foreach (OutputNameRow row in rows)
                {
                    string name = row.Get(kinds[i]);
                    List<string> sharing;

                    if (!byName.TryGetValue(name, out sharing))
                    {
                        sharing = new List<string>();
                        byName.Add(name, sharing);
                        order.Add(name);
                    }

                    sharing.Add(row.Group);
                }

                foreach (string name in order)
                {
                    if (byName[name].Count > 1)
                    {
                        found.Add(NameCollisions.Make(labels[i], name, byName[name]));
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Why the run cannot start, or null when it can. Checked before anything is
        /// cleared or written.
        /// </summary>
        public string WhyTheRunCannotStart()
        {
            IList<NameCollision> collisions = Collisions();

            if (collisions.Count == 0)
            {
                return null;
            }

            List<string> lines = new List<string>();

            foreach (NameCollision collision in collisions)
            {
                lines.Add(collision.Sentence());
            }

            return string.Join(Environment.NewLine + Environment.NewLine, lines.ToArray());
        }

        /// <summary>Only the rows that will actually run, for the collision check.</summary>
        public OutputNameTable Only(IEnumerable<string> groups)
        {
            HashSet<string> wanted = new HashSet<string>(groups, StringComparer.Ordinal);
            OutputNameTable some = new OutputNameTable();
            some.Today = Today;

            foreach (OutputNameRow row in rows)
            {
                if (wanted.Contains(row.Group))
                {
                    some.rows.Add(row);
                }
            }

            return some;
        }
    }
}
