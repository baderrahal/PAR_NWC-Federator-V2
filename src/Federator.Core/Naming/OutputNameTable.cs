using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Diagnostics;
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
    /// One group's three output names, each made by its pattern. Since F232 no name is typed over: the Outputs step
    /// holds the naming boxes and the logo only, Bader's round of 19 Oct 2026, fix 2.
    /// </summary>
    public sealed class OutputNameRow
    {
        private readonly string[] names = new string[3];

        internal OutputNameRow(string group, BuildingGroup source)
        {
            Group = group;
            Source = source;
        }

        /// <summary>The group this row names, as the Grouping step calls it.</summary>
        public string Group { get; private set; }

        /// <summary>The group itself, which the patterns name.</summary>
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
    /// One row per group, filled from the patterns, and the guard that refuses a run where two groups would share a name.
    /// </summary>
    public sealed class OutputNameTable
    {
        private readonly List<OutputNameRow> rows = new List<OutputNameRow>();

        public OutputNameTable()
        {
            Today = DateTime.Today;
        }

        internal ReadOnlyCollection<OutputNameRow> Rows
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

            table.FillFromThePatterns(naming, settings);
            return table;
        }

        /// <summary>Builds every name of every row from its pattern.</summary>
        private void FillFromThePatterns(OutputNaming naming, ContainerNameSettings settings)
        {
            if (naming == null)
            {
                throw new ArgumentNullException("naming");
            }

            foreach (OutputNameRow row in rows)
            {
                foreach (OutputKind kind in AllKinds())
                {
                    row.Fill(kind, Build(
                        PatternFor(naming, kind), row.Source, settings, DateFor(naming, kind)));
                }
            }
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

        /// <summary>What a name that could not be built starts with, so one rule tells it from a name.</summary>
        public const string CannotBeNamed = "CANNOT BE NAMED: ";

        public static string Build(
            NamePattern pattern, BuildingGroup group, ContainerNameSettings settings, DateTime? on)
        {
            try
            {
                return pattern.NameFor(group, settings, on);
            }
            catch (InvalidOperationException error)
            {
                return CannotBeNamed + error.Message;
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
            List<string> lines = new List<string>(Unusable());

            foreach (NameCollision collision in Collisions())
            {
                // Rows that cannot be named share the sentence, and rows with the cell cleared share the
                // empty name, so neither is told as a collision, they are told as unusable above.
                if (WhyUnusable(collision.Name) != null)
                {
                    continue;
                }

                lines.Add(collision.Sentence());
            }

            return lines.Count == 0
                ? null
                : string.Join(Environment.NewLine + Environment.NewLine, lines.ToArray());
        }

        /// <summary>
        /// The names that cannot be used for a file, one sentence each, FR-159 and FR-165: a name the
        /// pattern could not build, such as one with an emptied field, and a name cell cleared to
        /// nothing. Neither is a collision, and the check before a run looked at collisions only, so
        /// the refusal sentence went on as the file name and a cleared cell threw out of the click.
        /// </summary>
        public IList<string> Unusable()
        {
            List<string> found = new List<string>();
            IList<string> labels = OutputNaming.Labels();
            OutputKind[] kinds = AllKinds();

            // One sentence per output and cause, with the count and the first few groups, so one emptied
            // box is one paragraph and not one for every group of the run.
            for (int i = 0; i < kinds.Length; i++)
            {
                List<string> causes = new List<string>();
                Dictionary<string, List<string>> groupsByCause = new Dictionary<string, List<string>>(StringComparer.Ordinal);

                foreach (OutputNameRow row in rows)
                {
                    string cause = WhyUnusable(row.Get(kinds[i]));

                    if (cause == null)
                    {
                        continue;
                    }

                    List<string> groups;

                    if (!groupsByCause.TryGetValue(cause, out groups))
                    {
                        groups = new List<string>();
                        groupsByCause.Add(cause, groups);
                        causes.Add(cause);
                    }

                    groups.Add(row.Group);
                }

                foreach (string cause in causes)
                {
                    List<string> groups = groupsByCause[cause];
                    int shown = Math.Min(groups.Count, RunLog.KeptOfARepeat);
                    string named = string.Join(", ", groups.GetRange(0, shown).ToArray());

                    found.Add(groups.Count == 1
                        ? "The " + labels[i] + " name of " + groups[0] + " cannot be used" + cause
                            + " The run does not start."
                        : "The " + labels[i] + " name of " + groups.Count + " groups cannot be used" + cause
                            + " They are " + named
                            + (groups.Count > shown ? " and " + (groups.Count - shown) + " more." : ".")
                            + " The run does not start.");
                }
            }

            return found;
        }

        /// <summary>
        /// Why a name cannot be used, as the words that follow cannot be used, or null where it can. The
        /// one rule for an empty name and for a name the pattern could not build.
        /// </summary>
        private static string WhyUnusable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ", it is empty.";
            }

            if (name.StartsWith(CannotBeNamed, StringComparison.Ordinal))
            {
                return ". " + name.Substring(CannotBeNamed.Length);
            }

            // A name can hold what Windows refuses in a file name, a character typed into a naming field.
            // FileNames names the characters, not the running platform.
            int at = name.IndexOfAny(FileNames.Refused);

            if (at < 0)
            {
                return null;
            }

            return ", it holds " + FileNames.Name(name[at]) + ", which Windows does not allow in a file name.";
        }

    }
}
