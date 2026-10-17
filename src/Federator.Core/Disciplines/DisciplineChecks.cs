using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Federator.Core.Naming;

namespace Federator.Core.Disciplines
{
    /// <summary>The place a row is about, so each check writes its rows with the community and the package once.</summary>
    internal sealed class RowPlace
    {
        public RowPlace(string community, string package)
        {
            Community = community ?? string.Empty;
            Package = package ?? string.Empty;
        }

        public string Community { get; private set; }

        public string Package { get; private set; }

        public CheckRow Row(string discipline, string group, string file, string check, string result, string value, string elementId)
        {
            return new CheckRow(discipline, Community, Package, group, FileName(file), check, result, value, elementId);
        }

        public static string FileName(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return string.Empty;
            }

            try
            {
                return Path.GetFileName(file);
            }
            catch (ArgumentException)
            {
                return file;
            }
        }
    }

    /// <summary>The discipline of a file by the naming settings, part 5 by default, or null where the name does not read.</summary>
    public static class DisciplineOf
    {
        public static string File(string file, ContainerNameSettings names)
        {
            if (string.IsNullOrEmpty(file) || names == null)
            {
                return null;
            }

            ParsedContainerName parsed = ContainerName.Parse(file, names);
            return parsed.IsReadable ? parsed.Discipline : null;
        }
    }

    /// <summary>
    /// Check 1, coordinates, F209. Each model's box is measured from the boxes of the reference discipline's models of its
    /// group, AR by default, as the gap between the two boxes, nought where they overlap and the straight line of the gaps
    /// on the three axes where they do not, and a model whose gap is over the setting is far. Each line names the distance
    /// and both shared sites. A group with no reference model takes the discipline of its first model with a box, and the
    /// line says so. Measured on C02 on 18 Oct 2026, scan.md 5z-zg: AR and ME near Revit's origin, EL and ST 2,823 km away.
    /// </summary>
    public static class CoordinatesCheck
    {
        public static IList<CheckRow> Rows(
            string community, string package, DisciplineGroup group, DisciplineReportSettings settings, ContainerNameSettings names)
        {
            RowPlace place = new RowPlace(community, package);
            List<CheckRow> rows = new List<CheckRow>();

            if (group.Models == null)
            {
                foreach (string file in group.Files)
                {
                    rows.Add(place.Row(DisciplineOf.File(file, names), group.Name, file, CheckWords.Coordinates, CheckWords.NotRead,
                        "the models of this group were not read, " + group.WhyNoModels, null));
                }

                return rows;
            }

            string reference = null;
            bool fellBack = false;

            foreach (ModelFacts model in group.Models)
            {
                if (model.HasBox && string.Equals(DisciplineOf.File(model.File, names), settings.Reference, StringComparison.Ordinal))
                {
                    reference = settings.Reference;
                    break;
                }
            }

            if (reference == null)
            {
                foreach (ModelFacts model in group.Models)
                {
                    string code = DisciplineOf.File(model.File, names);

                    if (model.HasBox && code != null)
                    {
                        reference = code;
                        fellBack = true;
                        break;
                    }
                }
            }

            List<ModelFacts> referenceModels = group.Models.Count == 0
                ? new List<ModelFacts>()
                : new List<ModelFacts>(group.Models).FindAll(m => m.HasBox && string.Equals(DisciplineOf.File(m.File, names), reference, StringComparison.Ordinal));

            foreach (ModelFacts model in group.Models)
            {
                string code = DisciplineOf.File(model.File, names);

                if (!model.HasBox)
                {
                    rows.Add(place.Row(code, group.Name, model.File, CheckWords.Coordinates, CheckWords.NotRead, "its box was not read, " + model.BoxNotRead, null));
                    continue;
                }

                if (reference == null)
                {
                    rows.Add(place.Row(code, group.Name, model.File, CheckWords.Coordinates, CheckWords.NotRead,
                        "no model of this group has a box and a discipline that reads, so nothing is measured from", null));
                    continue;
                }

                if (string.Equals(code, reference, StringComparison.Ordinal))
                {
                    rows.Add(place.Row(code, group.Name, model.File, CheckWords.Coordinates, CheckWords.Reference,
                        ReferenceWords(model, reference, fellBack, settings) + FarFromIt(group, reference, referenceModels, settings, names), null));
                    continue;
                }

                double gap = GapToAll(model, referenceModels);
                rows.Add(place.Row(code, group.Name, model.File, CheckWords.Coordinates,
                    gap > settings.FarMetres ? CheckWords.Far : CheckWords.Near,
                    Distance(gap) + " from " + reference + (fellBack ? ", the first discipline of the group with a box, since it holds no " + settings.Reference : string.Empty)
                        + ", " + SiteWords(model) + ", " + reference + " " + Sites(referenceModels),
                    null));
            }

            return rows;
        }

        /// <summary>The gap between two boxes in metres, nought where they touch or overlap.</summary>
        public static double Gap(ModelFacts a, ModelFacts b)
        {
            double dx = Math.Max(0.0, Math.Max(a.MinX - b.MaxX, b.MinX - a.MaxX));
            double dy = Math.Max(0.0, Math.Max(a.MinY - b.MaxY, b.MinY - a.MaxY));
            double dz = Math.Max(0.0, Math.Max(a.MinZ - b.MaxZ, b.MinZ - a.MaxZ));
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>The distance in words, metres to three decimals and kilometres too from a thousand metres on.</summary>
        public static string Distance(double metres)
        {
            string m = metres.ToString("0.000", CultureInfo.InvariantCulture) + " m";
            return metres >= 1000.0 ? (metres / 1000.0).ToString("0.000", CultureInfo.InvariantCulture) + " km, " + m : m;
        }

        private static double GapToAll(ModelFacts model, IList<ModelFacts> references)
        {
            double nearest = double.MaxValue;

            foreach (ModelFacts reference in references)
            {
                nearest = Math.Min(nearest, Gap(model, reference));
            }

            return nearest == double.MaxValue ? 0.0 : nearest;
        }

        private static string ReferenceWords(ModelFacts model, string reference, bool fellBack, DisciplineReportSettings settings)
        {
            return reference + " is the discipline the others are measured from"
                + (fellBack ? ", the first of the group with a box, since it holds no " + settings.Reference : string.Empty)
                + ", " + SiteWords(model);
        }

        private static string FarFromIt(
            DisciplineGroup group, string reference, IList<ModelFacts> referenceModels, DisciplineReportSettings settings, ContainerNameSettings names)
        {
            Dictionary<string, double> far = new Dictionary<string, double>(StringComparer.Ordinal);
            List<string> order = new List<string>();

            foreach (ModelFacts model in group.Models)
            {
                string code = DisciplineOf.File(model.File, names);

                if (!model.HasBox || code == null || string.Equals(code, reference, StringComparison.Ordinal))
                {
                    continue;
                }

                double gap = GapToAll(model, referenceModels);

                if (gap <= settings.FarMetres)
                {
                    continue;
                }

                double was;

                if (!far.TryGetValue(code, out was))
                {
                    order.Add(code);
                    far[code] = gap;
                }
                else if (gap < was)
                {
                    far[code] = gap;
                }
            }

            if (order.Count == 0)
            {
                return ", no discipline of the group is far from it";
            }

            List<string> words = order.ConvertAll(code => code + " " + Distance(far[code]));
            return ", far from it: " + string.Join(", ", words.ToArray());
        }

        private static string SiteWords(ModelFacts model)
        {
            return "its site " + (model.SiteNotRead == null ? Quoted(model.Site) : "not read, " + model.SiteNotRead);
        }

        private static string Sites(IList<ModelFacts> models)
        {
            List<string> sites = new List<string>();

            foreach (ModelFacts model in models)
            {
                string site = model.SiteNotRead == null ? Quoted(model.Site) : "not read";

                if (!sites.Contains(site))
                {
                    sites.Add(site);
                }
            }

            return (sites.Count == 1 ? "site " : "sites ") + string.Join(", ", sites.ToArray());
        }

        private static string Quoted(string site)
        {
            return string.IsNullOrEmpty(site) ? "named none" : site;
        }
    }

    /// <summary>
    /// Check 2, a missing discipline, F209: a discipline found in other groups of the package but not in this one, listed
    /// by group in its own workbook, read off the file names. Bader's example: ME, EL and ST present and AR not there is
    /// AR missing. A discipline in every group and one in no group of the package each get one line saying so.
    /// </summary>
    public static class MissingDisciplineCheck
    {
        public static IList<CheckRow> Rows(
            string community, string package, IList<DisciplineGroup> groups, DisciplineReportSettings settings, ContainerNameSettings names)
        {
            RowPlace place = new RowPlace(community, package);
            List<CheckRow> rows = new List<CheckRow>();
            Dictionary<DisciplineGroup, List<string>> codesOf = new Dictionary<DisciplineGroup, List<string>>();

            foreach (DisciplineGroup group in groups)
            {
                List<string> codes = new List<string>();

                foreach (string file in group.Files)
                {
                    string code = DisciplineOf.File(file, names);

                    if (code != null && !codes.Contains(code))
                    {
                        codes.Add(code);
                    }
                }

                codesOf[group] = codes;
            }

            foreach (string discipline in settings.Disciplines)
            {
                List<DisciplineGroup> with = new List<DisciplineGroup>(groups).FindAll(g => codesOf[g].Contains(discipline));

                if (with.Count == 0)
                {
                    rows.Add(place.Row(discipline, string.Empty, string.Empty, CheckWords.Missing, CheckWords.NotInThePackage,
                        discipline + " is in no group of this package, so no group misses it against the others", null));
                    continue;
                }

                if (with.Count == groups.Count)
                {
                    rows.Add(place.Row(discipline, string.Empty, string.Empty, CheckWords.Missing, CheckWords.NoneMissing,
                        discipline + " is in every group of this package, " + PackageCount(groups.Count), null));
                    continue;
                }

                foreach (DisciplineGroup group in groups)
                {
                    if (codesOf[group].Contains(discipline))
                    {
                        continue;
                    }

                    rows.Add(place.Row(discipline, group.Name, string.Empty, CheckWords.Missing, CheckWords.MissingHere,
                        discipline + " is in " + with.Count + " of " + groups.Count + " groups of this package and not in " + group.Name
                            + ", which holds " + (codesOf[group].Count == 0 ? "no discipline that reads" : string.Join(", ", codesOf[group].ToArray())),
                        null));
                }
            }

            return rows;
        }

        private static string PackageCount(int groups)
        {
            return groups + (groups == 1 ? " group" : " groups");
        }
    }

    /// <summary>
    /// Check 3, naming, F209: not against ISO but against the reference discipline's own names, AR by default. The names
    /// of its files are split on the separator of the naming settings and their shared pattern learned: the count of parts
    /// most of them have, the parts every one holds alike, and the shape every one gives a part, its letters and digits.
    /// The discipline's own part is left out, since it differs by its nature. Every file of another discipline is then
    /// read against it, and a part it breaks is named with what it holds and what the reference holds there. A shape is
    /// said by naming a real part of the reference that has it, never as a code of letters, core.md.
    /// </summary>
    public sealed class ReferenceNaming
    {
        private readonly List<string> fixedValues = new List<string>();
        private readonly List<string> shapes = new List<string>();
        private readonly List<string> examples = new List<string>();

        private ReferenceNaming()
        {
        }

        public string Reference { get; private set; }

        public int NameCount { get; private set; }

        public int PartCount { get; private set; }

        /// <summary>How many of the reference names have the part count learned.</summary>
        public int WithThatCount { get; private set; }

        public char Separator { get; private set; }

        public int DisciplinePart { get; private set; }

        public bool WasLearned
        {
            get { return NameCount > 0; }
        }

        public static ReferenceNaming Learn(IEnumerable<string> referenceFiles, DisciplineReportSettings settings, ContainerNameSettings names)
        {
            ReferenceNaming naming = new ReferenceNaming();
            naming.Reference = settings.Reference;
            naming.Separator = names.Separator;
            naming.DisciplinePart = names.DisciplinePart;
            List<string[]> parts = new List<string[]>();

            foreach (string file in referenceFiles)
            {
                parts.Add(Stem(file).Split(names.Separator));
            }

            naming.NameCount = parts.Count;

            if (parts.Count == 0)
            {
                return naming;
            }

            Dictionary<int, int> byCount = new Dictionary<int, int>();

            foreach (string[] split in parts)
            {
                int was;
                byCount[split.Length] = byCount.TryGetValue(split.Length, out was) ? was + 1 : 1;
            }

            int count = 0;
            int most = -1;

            foreach (KeyValuePair<int, int> pair in byCount)
            {
                if (pair.Value > most || (pair.Value == most && pair.Key < count))
                {
                    count = pair.Key;
                    most = pair.Value;
                }
            }

            naming.PartCount = count;
            naming.WithThatCount = most;
            List<string[]> alike = parts.FindAll(p => p.Length == count);

            for (int i = 0; i < count; i++)
            {
                string first = alike[0][i];
                bool same = alike.TrueForAll(p => string.Equals(p[i], first, StringComparison.Ordinal));
                string shape = MergedShape(alike.ConvertAll(p => p[i]));
                naming.fixedValues.Add(same && i + 1 != names.DisciplinePart && !settings.ShapeOnlyParts.Contains(i + 1) ? first : null);
                naming.shapes.Add(i + 1 != names.DisciplinePart ? shape : null);
                naming.examples.Add(first);
            }

            return naming;
        }

        /// <summary>
        /// The shape the reference names share at one part: as long as each of them, each place a letter where every one
        /// holds a letter there, a digit where every one holds a digit, either where they hold one or the other, and the
        /// same mark where every one holds that mark. Null where the lengths differ or a mark differs, so the part is free.
        /// C06's building codes, 1B06BC and 1B06G1, share a shape of six whose last place is either.
        /// </summary>
        public static string MergedShape(IList<string> parts)
        {
            if (parts == null || parts.Count == 0)
            {
                return null;
            }

            int length = parts[0].Length;
            StringBuilder shape = new StringBuilder();

            foreach (string part in parts)
            {
                if (part.Length != length)
                {
                    return null;
                }
            }

            for (int i = 0; i < length; i++)
            {
                bool letters = true;
                bool digits = true;
                bool marksAlike = true;
                char mark = parts[0][i];

                foreach (string part in parts)
                {
                    char c = part[i];
                    letters = letters && char.IsLetter(c);
                    digits = digits && char.IsDigit(c);
                    marksAlike = marksAlike && c == mark;
                }

                if (letters)
                {
                    shape.Append('A');
                }
                else if (digits)
                {
                    shape.Append('9');
                }
                else if (AllLettersOrDigits(parts, i))
                {
                    shape.Append('#');
                }
                else if (marksAlike)
                {
                    shape.Append(mark);
                }
                else
                {
                    return null;
                }
            }

            return shape.ToString();
        }

        /// <summary>Whether a part fits a merged shape, MergedShape's places read as it writes them.</summary>
        public static bool Fits(string part, string shape)
        {
            if (part == null || shape == null || part.Length != shape.Length)
            {
                return false;
            }

            for (int i = 0; i < part.Length; i++)
            {
                char c = part[i];
                char s = shape[i];
                bool ok = s == 'A' ? char.IsLetter(c) : s == '9' ? char.IsDigit(c) : s == '#' ? char.IsLetterOrDigit(c) : c == s;

                if (!ok)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The pattern in words, for the one line of each workbook that says what was learned.</summary>
        public string Describe()
        {
            if (!WasLearned)
            {
                return "no " + Reference + " file in this package, so no pattern was learned and no name was checked";
            }

            List<string> words = new List<string>();

            for (int i = 0; i < PartCount; i++)
            {
                string part = "part " + (i + 1) + " ";

                if (i + 1 == DisciplinePart)
                {
                    words.Add(part + "the discipline");
                }
                else if (fixedValues[i] != null)
                {
                    words.Add(part + fixedValues[i]);
                }
                else if (shapes[i] != null)
                {
                    words.Add(part + "shaped like " + examples[i]);
                }
                else
                {
                    words.Add(part + "free");
                }
            }

            return "learned from " + NameCount + " " + Reference + " names, " + PartCount + " parts split on " + Separator
                + (WithThatCount < NameCount ? " in " + WithThatCount + " of them" : string.Empty) + ": " + string.Join(", ", words.ToArray());
        }

        /// <summary>Every part of a file's name that breaks the pattern, in words, or none where it follows it.</summary>
        public IList<string> Breaks(string file)
        {
            List<string> breaks = new List<string>();

            if (!WasLearned)
            {
                return breaks;
            }

            string[] split = Stem(file).Split(Separator);

            if (split.Length != PartCount)
            {
                breaks.Add("it has " + split.Length + " parts split on " + Separator + ", " + Reference + " names have " + PartCount);
            }

            for (int i = 0; i < Math.Min(split.Length, PartCount); i++)
            {
                if (i + 1 == DisciplinePart)
                {
                    continue;
                }

                if (fixedValues[i] != null && !string.Equals(split[i], fixedValues[i], StringComparison.Ordinal))
                {
                    breaks.Add("part " + (i + 1) + " reads " + split[i] + ", " + Reference + " has " + fixedValues[i] + " there");
                }
                else if (fixedValues[i] == null && shapes[i] != null && !Fits(split[i], shapes[i]))
                {
                    breaks.Add("part " + (i + 1) + " reads " + split[i] + ", " + Reference + " has a part shaped like " + examples[i] + " there");
                }
            }

            return breaks;
        }

        private static bool AllLettersOrDigits(IList<string> parts, int at)
        {
            foreach (string part in parts)
            {
                if (!char.IsLetterOrDigit(part[at]))
                {
                    return false;
                }
            }

            return true;
        }

        private static string Stem(string file)
        {
            string name = RowPlace.FileName(file);
            int dot = name.LastIndexOf('.');
            return dot > 0 ? name.Substring(0, dot) : name;
        }

        public static IList<CheckRow> Rows(
            string community, string package, IList<DisciplineGroup> groups, DisciplineReportSettings settings, ContainerNameSettings names)
        {
            RowPlace place = new RowPlace(community, package);
            List<string> referenceFiles = new List<string>();

            foreach (DisciplineGroup group in groups)
            {
                foreach (string file in group.Files)
                {
                    if (string.Equals(DisciplineOf.File(file, names), settings.Reference, StringComparison.Ordinal))
                    {
                        referenceFiles.Add(file);
                    }
                }
            }

            ReferenceNaming naming = Learn(referenceFiles, settings, names);
            List<CheckRow> rows = new List<CheckRow>();

            foreach (string discipline in settings.Disciplines)
            {
                rows.Add(place.Row(discipline, string.Empty, string.Empty, CheckWords.Naming,
                    naming.WasLearned ? CheckWords.Pattern : CheckWords.NoPattern, naming.Describe(), null));
            }

            if (!naming.WasLearned)
            {
                return rows;
            }

            foreach (DisciplineGroup group in groups)
            {
                foreach (string file in group.Files)
                {
                    string code = DisciplineOf.File(file, names);

                    if (code == null || string.Equals(code, settings.Reference, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    IList<string> breaks = naming.Breaks(file);
                    rows.Add(place.Row(code, group.Name, file, CheckWords.Naming, breaks.Count == 0 ? CheckWords.Follows : CheckWords.Breaks,
                        breaks.Count == 0 ? "every part as " + settings.Reference + " names have it" : string.Join(", and ", new List<string>(breaks).ToArray()), null));
                }
            }

            return rows;
        }
    }

    /// <summary>Check 4, file age, F209: each NWC with the date the disk gives it, flagged when older than the setting, 7 days.</summary>
    public static class FileAgeCheck
    {
        public static IList<CheckRow> Rows(
            string community, string package, IList<DisciplineGroup> groups, IList<FileDate> dates, DateTime now,
            DisciplineReportSettings settings, ContainerNameSettings names)
        {
            RowPlace place = new RowPlace(community, package);
            List<CheckRow> rows = new List<CheckRow>();
            Dictionary<string, FileDate> byFile = new Dictionary<string, FileDate>(StringComparer.OrdinalIgnoreCase);

            foreach (FileDate date in dates ?? new List<FileDate>())
            {
                byFile[date.File] = date;
            }

            foreach (DisciplineGroup group in groups)
            {
                foreach (string file in group.Files)
                {
                    string code = DisciplineOf.File(file, names);
                    FileDate date;

                    if (!byFile.TryGetValue(file, out date) || !date.Written.HasValue)
                    {
                        rows.Add(place.Row(code, group.Name, file, CheckWords.FileAge, CheckWords.NotRead,
                            "its date was not read, " + (date == null ? "UNKNOWN why, no date was handed in" : date.WhyNotRead), null));
                        continue;
                    }

                    TimeSpan age = now - date.Written.Value;
                    int days = (int)Math.Floor(age.TotalDays);
                    bool older = age > TimeSpan.FromDays(settings.OlderThanDays);
                    rows.Add(place.Row(code, group.Name, file, CheckWords.FileAge, older ? CheckWords.Older : CheckWords.Within,
                        "written " + date.Written.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + ", " + days
                            + (days == 1 ? " day" : " days") + " old, the limit " + settings.OlderThanDays + " days", null));
                }
            }

            return rows;
        }
    }

    /// <summary>
    /// Checks 5 and 6, the CAD files inside the NWC and the Generic Models elements, F209: one line per element the add-in
    /// read, with its element id, and the setting's words where the id was not read, so no element is dropped in silence.
    /// A model with none found says so, and a model whose items were not walked says not read with why.
    /// </summary>
    public static class ElementChecks
    {
        public static IList<CheckRow> Rows(
            string community, string package, DisciplineGroup group, DisciplineReportSettings settings, ContainerNameSettings names)
        {
            RowPlace place = new RowPlace(community, package);
            List<CheckRow> rows = new List<CheckRow>();

            if (group.Models == null)
            {
                foreach (string file in group.Files)
                {
                    string code = DisciplineOf.File(file, names);
                    rows.Add(place.Row(code, group.Name, file, CheckWords.Cad, CheckWords.NotRead, "the models of this group were not read, " + group.WhyNoModels, null));
                    rows.Add(place.Row(code, group.Name, file, CheckWords.Generic, CheckWords.NotRead, "the models of this group were not read, " + group.WhyNoModels, null));
                }

                return rows;
            }

            foreach (ModelFacts model in group.Models)
            {
                string code = DisciplineOf.File(model.File, names);
                Add(rows, place, code, group.Name, model, CheckWords.Cad, model.CadNotRead, model.Cad, settings, Cad,
                    "no imported or linked CAD file found in it");
                Add(rows, place, code, group.Name, model, CheckWords.Generic, model.GenericNotRead, model.Generic, settings, Generic,
                    "no element of the category " + settings.GenericCategory + " found in it");
            }

            return rows;
        }

        private static void Add(
            List<CheckRow> rows, RowPlace place, string code, string group, ModelFacts model, string check, string notRead,
            IList<ElementFact> found, DisciplineReportSettings settings, Func<ElementFact, DisciplineReportSettings, string> words, string none)
        {
            if (notRead != null)
            {
                rows.Add(place.Row(code, group, model.File, check, CheckWords.NotRead, "its items were not walked, " + notRead, null));
                return;
            }

            if (found.Count == 0)
            {
                rows.Add(place.Row(code, group, model.File, check, CheckWords.NoneFound, none, null));
                return;
            }

            foreach (ElementFact element in found)
            {
                rows.Add(place.Row(code, group, model.File, check, CheckWords.Found, words(element, settings), element.ElementId ?? settings.NotReadable));
            }
        }

        private static string Cad(ElementFact element, DisciplineReportSettings settings)
        {
            return "CAD file " + (element.Name ?? settings.NotReadable) + ", in " + (element.Family ?? settings.NotReadable)
                + " of type " + (element.Type ?? settings.NotReadable) + ", level " + (element.Level ?? settings.NotReadable);
        }

        private static string Generic(ElementFact element, DisciplineReportSettings settings)
        {
            return "family " + (element.Family ?? settings.NotReadable) + ", type " + (element.Type ?? settings.NotReadable)
                + ", level " + (element.Level ?? settings.NotReadable);
        }

        /// <summary>
        /// The level's name out of a Revit level read off a property, which Navisworks gives as Level "GRF", #151434, the
        /// name between the first and the last double quote, measured on C06 on 18 Oct 2026, or the text as it is where it
        /// holds no pair of quotes. Null for nothing.
        /// </summary>
        public static string LevelName(string read)
        {
            if (string.IsNullOrEmpty(read) || read.Trim().Length == 0)
            {
                return null;
            }

            int first = read.IndexOf('"');
            int last = read.LastIndexOf('"');
            return first >= 0 && last > first + 1 ? read.Substring(first + 1, last - first - 1) : read.Trim();
        }

        /// <summary>The CAD file name an import carries in its own name, its name where it ends with a dot and letters, else null.</summary>
        public static string CadFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            string trimmed = name.Trim();
            int dot = trimmed.LastIndexOf('.');

            if (dot <= 0 || dot == trimmed.Length - 1 || trimmed.Length - dot - 1 > 5)
            {
                return null;
            }

            for (int i = dot + 1; i < trimmed.Length; i++)
            {
                if (!char.IsLetter(trimmed[i]))
                {
                    return null;
                }
            }

            return trimmed;
        }
    }
}
