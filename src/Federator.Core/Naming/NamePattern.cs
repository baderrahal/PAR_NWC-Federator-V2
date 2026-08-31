using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Grouping;

namespace Federator.Core.Naming
{
    /// <summary>
    /// The four fields of an output name this tool supplies rather than reads out of the
    /// input, plus the code used when a group spans more than one building.
    ///
    /// WHY THE DEFAULTS ARE WHAT THEY ARE. Outputs overwrite, and the files inside one
    /// group can disagree on the level, the type and the number, so those fields cannot be
    /// copied from any one input without picking a winner. They are supplied instead.
    /// ZZZ is the ISO 19650 code for all levels, BM is the discipline code for a federated
    /// building model, MOD is the type code for a model, and 000001 is the first in a
    /// sequence that never advances because the file is overwritten in place. ZZZZZZ
    /// follows the same all convention for a group that covers every building.
    ///
    /// They are defaults, not constants. Every one of them is shown in the Outputs step
    /// and can be changed there, and a project whose standard says something else changes
    /// them rather than the code.
    /// </summary>
    public sealed class NamePattern
    {
        public const string DefaultLevel = "ZZZ";
        public const string DefaultDiscipline = "BM";
        public const string DefaultTypeCode = "MOD";
        public const string DefaultNumber = "000001";
        public const string DefaultAllBuildings = "ZZZZZZ";

        /// <summary>Sorts by name the same way it sorts by date, which is the point of it.</summary>
        public const string DefaultDateFormat = "yyyyMMdd";

        public NamePattern()
        {
            Level = DefaultLevel;
            Discipline = DefaultDiscipline;
            TypeCode = DefaultTypeCode;
            Number = DefaultNumber;
            AllBuildings = DefaultAllBuildings;
            DateFormat = DefaultDateFormat;
        }

        /// <summary>ISO 19650 code for all levels. The files in a group can disagree on it.</summary>
        public string Level { get; set; }

        /// <summary>
        /// The discipline field, used when the group spans more than one discipline. A
        /// group that is one discipline carries that discipline instead.
        /// </summary>
        public string Discipline { get; set; }

        public string TypeCode { get; set; }

        /// <summary>The sequence number. It never advances, because the file is overwritten.</summary>
        public string Number { get; set; }

        /// <summary>
        /// The building field, used when the group spans more than one building. A group
        /// that is one building carries that building code instead.
        /// </summary>
        public string AllBuildings { get; set; }

        /// <summary>
        /// How a date is written into the number field when the NWD is being kept week
        /// by week. A setting, so a project that writes dates another way changes it here.
        /// </summary>
        public string DateFormat { get; set; }

        public NamePattern Copy()
        {
            return new NamePattern
            {
                Level = Level,
                Discipline = Discipline,
                TypeCode = TypeCode,
                Number = Number,
                AllBuildings = AllBuildings,
                DateFormat = DateFormat
            };
        }

        /// <summary>
        /// Refuses an empty field rather than writing a name with a hole in it. Returns
        /// the reason, or null when the pattern is usable.
        /// </summary>
        public string WhyUnusable()
        {
            string missing = FirstEmpty(
                new[] { Level, Discipline, TypeCode, Number, AllBuildings },
                new[] { "level", "discipline", "type code", "number", "all buildings code" });

            return missing;
        }

        private static string FirstEmpty(string[] values, string[] labels)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (string.IsNullOrEmpty(values[i]) || values[i].Trim().Length == 0)
                {
                    return "The " + labels[i] + " is empty, so the name would have a hole in it.";
                }
            }

            return null;
        }

        /// <summary>
        /// The output name for one group. Seven fields: project, originator, building,
        /// level, discipline, type, number.
        ///
        /// The building comes from the group where the group is one building, and from
        /// AllBuildings otherwise. The discipline comes from the group where the group is
        /// one discipline, and from Discipline otherwise. Nothing else is read from the
        /// input, so a five part input still gives a full seven field name.
        /// </summary>
        public string NameFor(BuildingGroup group, ContainerNameSettings settings)
        {
            return NameFor(group, settings, null);
        }

        /// <summary>
        /// The name for one group, with a date in place of the number when one is given.
        ///
        /// The number field exists to tell revisions of one container apart, and it never
        /// advances here because the file is overwritten in place. When the NWD is being
        /// kept week by week it stops being overwritten, so the date goes exactly where the
        /// number was rather than as an eighth field the naming standard does not have.
        /// </summary>
        public string NameFor(BuildingGroup group, ContainerNameSettings settings, DateTime? on)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            return NameFor(
                group.Project, group.Originator, group.BuildingCode, group.DisciplineCode,
                settings, on);
        }

        /// <summary>The number field, or the date when the output is being kept week by week.</summary>
        public string NumberOrDate(DateTime? on)
        {
            if (!on.HasValue)
            {
                return Number;
            }

            string format = string.IsNullOrEmpty(DateFormat) ? DefaultDateFormat : DateFormat;
            string written;

            try
            {
                written = on.Value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return Number;
            }

            // DateTime.ToString does NOT throw on a format string nobody can read. It
            // treats what it does not recognise as literal text, so "not a real format"
            // comes back as "noA a real 0or0aA". Measured on 2026-08-31. What matters is
            // whether the result can be part of a file name, so that is what is checked
            // rather than an exception that never arrives.
            if (written.Length == 0 || written.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                return Number;
            }

            return written;
        }

        public string NameFor(
            string project,
            string originator,
            string buildingCode,
            string disciplineCode,
            ContainerNameSettings settings)
        {
            return NameFor(project, originator, buildingCode, disciplineCode, settings, null);
        }

        public string NameFor(
            string project,
            string originator,
            string buildingCode,
            string disciplineCode,
            ContainerNameSettings settings,
            DateTime? on)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            string unusable = WhyUnusable();

            if (unusable != null)
            {
                throw new InvalidOperationException(unusable);
            }

            string[] fields =
            {
                project,
                originator,
                string.IsNullOrEmpty(buildingCode) ? AllBuildings : buildingCode,
                Level,
                string.IsNullOrEmpty(disciplineCode) ? Discipline : disciplineCode,
                TypeCode,
                NumberOrDate(on)
            };

            return string.Join(settings.Separator.ToString(), fields);
        }
    }

    /// <summary>
    /// One pattern each for the NWF, the NWD and the workbook, because a project may want
    /// them to differ. They start identical.
    /// </summary>
    public sealed class OutputNaming
    {
        public OutputNaming()
        {
            Nwf = new NamePattern();
            Nwd = new NamePattern();
            Workbook = new NamePattern();
        }

        public NamePattern Nwf { get; set; }

        public NamePattern Nwd { get; set; }

        public NamePattern Workbook { get; set; }

        /// <summary>
        /// Write the NWD with a date in its name, so an earlier week still exists. Off by
        /// default.
        ///
        /// The NWF always overwrites and always will. Its clash results live inside it and
        /// are the record of what has been fixed, so it has to be the same file week after
        /// week or that record starts again. The NWD carries no results, it is the picture
        /// of the models as they were, so keeping one per week costs only disk and is the
        /// only way an earlier week still exists.
        /// </summary>
        public bool DateTheNwd { get; set; }

        /// <summary>The date the NWD carries, or null when it is overwriting as usual.</summary>
        public DateTime? NwdDate(DateTime today)
        {
            return DateTheNwd ? (DateTime?)today : null;
        }

        /// <summary>The three, in the order the window shows them.</summary>
        public IList<NamePattern> All()
        {
            return new List<NamePattern> { Nwf, Nwd, Workbook };
        }

        public static IList<string> Labels()
        {
            return new List<string> { "NWF", "NWD", "Workbook" };
        }

        public OutputNaming Copy()
        {
            return new OutputNaming
            {
                Nwf = Nwf.Copy(),
                Nwd = Nwd.Copy(),
                Workbook = Workbook.Copy(),
                DateTheNwd = DateTheNwd
            };
        }
    }

    /// <summary>Builds a collision from outside this file, for the name table.</summary>
    public static class NameCollisions
    {
        public static NameCollision Make(string kind, string name, IList<string> groups)
        {
            return new NameCollision(kind, name, groups);
        }
    }

    /// <summary>Two groups that would be written to the same name.</summary>
    public sealed class NameCollision
    {
        internal NameCollision(string kind, string name, IList<string> groups)
        {
            Kind = kind;
            Name = name;
            Groups = new ReadOnlyCollection<string>(groups);
        }

        /// <summary>NWF, NWD or Workbook.</summary>
        public string Kind { get; private set; }

        public string Name { get; private set; }

        /// <summary>Every group that would be written to that one name.</summary>
        public ReadOnlyCollection<string> Groups { get; private set; }

        public string Sentence()
        {
            return Groups.Count + " groups would be written to the same "
                + Kind + " name, " + Name + ". They are "
                + string.Join(", ", new List<string>(Groups).ToArray())
                + ". One would overwrite the other, so the run does not start.";
        }

        public override string ToString()
        {
            return Sentence();
        }
    }

    /// <summary>
    /// Works out the name every group would be written to, and refuses a set of patterns
    /// that would put two groups on one name.
    ///
    /// Outputs overwrite with no date suffix, so two groups sharing a name is not a
    /// warning. The second one silently destroys the first, and it only shows up as a
    /// missing federation nobody notices. It is caught before the run starts.
    /// </summary>
    public static class OutputNameCheck
    {
        public static IList<NameCollision> Collisions(
            IEnumerable<BuildingGroup> groups, OutputNaming naming, ContainerNameSettings settings)
        {
            if (groups == null)
            {
                throw new ArgumentNullException("groups");
            }

            if (naming == null)
            {
                throw new ArgumentNullException("naming");
            }

            List<NameCollision> found = new List<NameCollision>();
            IList<NamePattern> patterns = naming.All();
            IList<string> labels = OutputNaming.Labels();

            for (int i = 0; i < patterns.Count; i++)
            {
                Collect(groups, patterns[i], labels[i], settings, found);
            }

            return found;
        }

        private static void Collect(
            IEnumerable<BuildingGroup> groups,
            NamePattern pattern,
            string label,
            ContainerNameSettings settings,
            IList<NameCollision> into)
        {
            Dictionary<string, List<string>> byName =
                new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            List<string> order = new List<string>();

            foreach (BuildingGroup group in groups)
            {
                string name = pattern.NameFor(group, settings);
                List<string> sharing;

                if (!byName.TryGetValue(name, out sharing))
                {
                    sharing = new List<string>();
                    byName.Add(name, sharing);
                    order.Add(name);
                }

                sharing.Add(group.Building);
            }

            foreach (string name in order)
            {
                if (byName[name].Count > 1)
                {
                    into.Add(new NameCollision(label, name, byName[name]));
                }
            }
        }

        /// <summary>
        /// Why the run cannot start, or null when the patterns are usable. Checked before
        /// anything is cleared or written.
        /// </summary>
        public static string WhyTheRunCannotStart(
            IEnumerable<BuildingGroup> groups, OutputNaming naming, ContainerNameSettings settings)
        {
            if (naming == null)
            {
                throw new ArgumentNullException("naming");
            }

            IList<NamePattern> patterns = naming.All();
            IList<string> labels = OutputNaming.Labels();

            for (int i = 0; i < patterns.Count; i++)
            {
                string unusable = patterns[i].WhyUnusable();

                if (unusable != null)
                {
                    return "The " + labels[i] + " name pattern cannot be used. " + unusable;
                }
            }

            IList<NameCollision> collisions = Collisions(groups, naming, settings);

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
    }
}
