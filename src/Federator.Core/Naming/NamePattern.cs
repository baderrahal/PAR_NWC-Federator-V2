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
            // The characters are Windows' own, read off FileNames rather than off the
            // platform running this, which off Windows names only two of them.
            if (!FileNames.CanBeAName(written))
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
}
