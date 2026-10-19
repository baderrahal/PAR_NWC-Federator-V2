using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Federator.Core.Grouping;
using Federator.Core.Health;
using Federator.Core.Naming;

namespace Federator.Core.Disciplines
{
    /// <summary>
    /// What one check of the discipline reports found over the groups of a package, F234 and F244: its rows, the models or
    /// buildings of each discipline it judged for the Summary's Checked, and the lines of what it could not read, which go in
    /// the log, since a value not read is never called a pass.
    /// </summary>
    public sealed class CheckFindings
    {
        private readonly List<DisciplineIssue> issues = new List<DisciplineIssue>();
        private readonly Dictionary<string, int> judged = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<string> notes = new List<string>();

        public CheckFindings(string check)
        {
            Check = check ?? string.Empty;
        }

        public string Check { get; private set; }

        public IList<DisciplineIssue> Issues
        {
            get { return issues.AsReadOnly(); }
        }

        /// <summary>The lines the log carries: what was not read and why, and what a check learned, such as the naming pattern.</summary>
        public IList<string> Notes
        {
            get { return notes.AsReadOnly(); }
        }

        /// <summary>How many models of a discipline the check judged, or for Missing model how many buildings.</summary>
        public int CheckedFor(string discipline)
        {
            int count;
            return judged.TryGetValue(discipline ?? string.Empty, out count) ? count : 0;
        }

        internal void Judged(string discipline)
        {
            string key = discipline ?? string.Empty;
            int was;
            judged[key] = (judged.TryGetValue(key, out was) ? was : 0) + 1;
        }

        internal void Add(DisciplineIssue issue, ModelRow row)
        {
            issues.Add(issue);
            row.Add(issue);
        }

        internal void Note(string line)
        {
            notes.Add(Check + "  " + line);
        }
    }

    /// <summary>
    /// The rows of the Models sheet of a package, F244: one per NWC of every group in the groups' order, made before any check
    /// runs, each holding the facts read off its model where they were read, and the rows of the buildings that miss a
    /// discipline, which Missing model adds. Each check writes what it read into the row of the model it read.
    /// </summary>
    public sealed class ModelRows
    {
        private readonly List<ModelRow> all = new List<ModelRow>();
        private readonly Dictionary<DisciplineGroup, Dictionary<string, ModelRow>> byGroup = new Dictionary<DisciplineGroup, Dictionary<string, ModelRow>>();
        private readonly ContainerNameSettings names;

        private ModelRows(ContainerNameSettings names)
        {
            this.names = names;
        }

        public IList<ModelRow> All
        {
            get { return all.AsReadOnly(); }
        }

        public static ModelRows Of(IList<DisciplineGroup> groups, ContainerNameSettings names)
        {
            ModelRows rows = new ModelRows(names);

            foreach (DisciplineGroup group in groups)
            {
                foreach (string file in group.Files)
                {
                    rows.In(group, file);
                }

                foreach (ModelFacts model in group.Models ?? new List<ModelFacts>().AsReadOnly())
                {
                    rows.In(group, model.File);
                }
            }

            return rows;
        }

        /// <summary>The row of a file of a group, by its name with no case, made where the group's facts name a file its list does not.</summary>
        public ModelRow In(DisciplineGroup group, string file)
        {
            Dictionary<string, ModelRow> rows;

            if (!byGroup.TryGetValue(group, out rows))
            {
                rows = new Dictionary<string, ModelRow>(StringComparer.OrdinalIgnoreCase);
                byGroup.Add(group, rows);
            }

            string name = DisciplineOf.FileName(file);
            ModelRow row;

            if (!rows.TryGetValue(name, out row))
            {
                ParsedContainerName parsed = ContainerName.Parse(file, names);
                string code = parsed.IsReadable ? parsed.Discipline : null;
                row = new ModelRow(name, code, DisciplineOf.Building(file, names), ShortName(name, parsed), !group.NotBuiltForOneDiscipline, null);
                row.BuildingHas = CodesOf(group);
                rows.Add(name, row);
                all.Add(row);
            }

            return row;
        }

        /// <summary>The disciplines a group's files read, each once, what the grey line under Tabs may say a building has.</summary>
        private IList<string> CodesOf(DisciplineGroup group)
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

            return codes.AsReadOnly();
        }

        internal ModelRow Missing(string discipline, string building, IList<string> present)
        {
            ModelRow row = new ModelRow(null, discipline, building, null, true, CheckWords.And(present));
            all.Add(row);
            return row;
        }

        /// <summary>
        /// The short name of a file, its discipline and its number at the naming settings' NumberPart, such as ME-000002, or the
        /// file's name where the name does not read or has no number part, never the last part of the name.
        /// </summary>
        private string ShortName(string name, ParsedContainerName parsed)
        {
            return parsed.IsReadable && parsed.Number != null ? parsed.Discipline + names.Separator + parsed.Number : name;
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

        /// <summary>The building code of a file by the naming settings, part 3 by default, or empty where the name does not read.</summary>
        public static string Building(string file, ContainerNameSettings names)
        {
            if (string.IsNullOrEmpty(file) || names == null)
            {
                return string.Empty;
            }

            ParsedContainerName parsed = ContainerName.Parse(file, names);
            return parsed.IsReadable ? parsed.Building : string.Empty;
        }

        /// <summary>The building codes of a group's files, each once in file order, joined by a comma, for a row about the group.</summary>
        public static string BuildingsOf(DisciplineGroup group, ContainerNameSettings names)
        {
            List<string> codes = new List<string>();

            foreach (string file in group.Files)
            {
                string code = Building(file, names);

                if (code.Length > 0 && !codes.Contains(code))
                {
                    codes.Add(code);
                }
            }

            return codes.Count == 0 ? group.Name : string.Join(", ", codes.ToArray());
        }

        /// <summary>The name after the last slash or backslash, read as text so no path is ever a throw.</summary>
        public static string FileName(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return string.Empty;
            }

            int at = file.LastIndexOfAny(new[] { '\\', '/' });
            return at < 0 ? file : file.Substring(at + 1);
        }
    }

    /// <summary>
    /// Coordinates, F234 and F244, Bader's order of 19 Oct 2026: ALIGNMENT's rule and numbers, never the box gap. Each model of
    /// a group is placed by the translation of its transform, ModelPlacement, and measured from the group's reference model,
    /// AlignmentCheck.ReferenceIn, by the straight line of the differences, AlignmentCheck.StraightLineMillimetres, and EVERY
    /// distance measured is carried to its row of Models. A model more than the setting from it, 30 m by Bader's answer to
    /// Q202, is a High row. A reference model on Internal while another model of its group names a shared site that is not
    /// Internal is a High row of its own, and every other model measured from it a Medium row, since its distance cannot be
    /// judged. ST of 1B06BC sits 34.014 m from AR, read off its ALIGNMENT block of 19 Oct 2026.
    /// </summary>
    public static class CoordinatesCheck
    {
        public static CheckFindings Of(IList<DisciplineGroup> groups, ModelRows rows, DisciplineReportSettings settings, ContainerNameSettings names)
        {
            CheckFindings found = new CheckFindings(CheckWords.Coordinates);

            foreach (DisciplineGroup group in groups)
            {
                if (group.Models == null)
                {
                    foreach (string file in group.Files)
                    {
                        rows.In(group, file).Measure = group.NotBuiltForOneDiscipline ? ModelMeasure.Alone : ModelMeasure.Unknown;
                    }

                    if (group.Files.Count > 0)
                    {
                        found.Note(group.Name + " not read, its models were not read, " + group.WhyNoModels);
                    }

                    continue;
                }

                List<ModelPlacement> placements = new List<ModelPlacement>();

                foreach (ModelFacts model in group.Models)
                {
                    placements.Add(PlacementOf(model, names));
                }

                ModelPlacement reference = AlignmentCheck.ReferenceIn(placements, settings.Reference);

                if (reference == null)
                {
                    found.Note(group.Name + " not read, no model of it was placed, so nothing is measured");
                    continue;
                }

                bool onInternal = ReferenceOnInternal(placements, reference);

                for (int i = 0; i < placements.Count; i++)
                {
                    ModelPlacement model = placements[i];
                    ModelRow row = rows.In(group, group.Models[i].File);
                    row.SiteNotRead = !model.SiteRead;

                    if (!model.Placed)
                    {
                        found.Note(group.Name + "  " + model.File + " not read, its placement was not read, " + group.Models[i].PlacementNotRead);
                        continue;
                    }

                    found.Judged(model.Discipline);
                    row.ComparedWith = NameOf(reference);

                    if (model == reference)
                    {
                        row.Measure = onInternal ? ModelMeasure.InternalOrigin : ModelMeasure.Reference;

                        if (onInternal)
                        {
                            found.Add(Row(model, names, CheckWords.High, CheckWords.OnInternal, null, CheckWords.ExportOnShared), row);
                        }

                        continue;
                    }

                    double metres = AlignmentCheck.StraightLineMillimetres(model, reference) / 1000.0;
                    row.Measure = ModelMeasure.Measured;
                    row.Metres = metres;

                    if (onInternal)
                    {
                        found.Add(Row(model, names, CheckWords.Medium, CheckWords.FromInternal(NameOf(reference)), metres, CheckWords.FixFirst(NameOf(reference))), row);
                    }
                    else if (metres > settings.FarMetres)
                    {
                        found.Add(Row(model, names, CheckWords.High,
                            CheckWords.FarFinding(metres, NameOf(model), AlignmentCheck.SiteName(model), NameOf(reference), AlignmentCheck.SiteName(reference)),
                            metres, CheckWords.ExportAgain), row);
                    }
                }
            }

            return found;
        }

        /// <summary>The reference names Internal while another model of its group names a shared site that is not Internal, F234's rule.</summary>
        private static bool ReferenceOnInternal(IList<ModelPlacement> placements, ModelPlacement reference)
        {
            if (!AlignmentCheck.NamesInternal(reference, AlignmentCheck.DefaultInternalName))
            {
                return false;
            }

            foreach (ModelPlacement model in placements)
            {
                if (model != reference && model.NamesASharedCoordinate && !AlignmentCheck.NamesInternal(model, AlignmentCheck.DefaultInternalName))
                {
                    return true;
                }
            }

            return false;
        }

        private static DisciplineIssue Row(ModelPlacement model, ContainerNameSettings names, string priority, string finding, double? metres, string fix)
        {
            return new DisciplineIssue(
                model.Discipline, CheckWords.Coordinates, priority, DisciplineOf.Building(model.File, names), finding, metres,
                metres.HasValue ? CheckWords.Metres : null, fix);
        }

        /// <summary>A model as a row names it, its discipline, or its file's name where the discipline does not read.</summary>
        private static string NameOf(ModelPlacement model)
        {
            return model.Discipline.Length > 0 ? model.Discipline : model.File;
        }

        /// <summary>A model's facts as ALIGNMENT's placement, its name, its discipline, its site or not read and its translation or not read.</summary>
        private static ModelPlacement PlacementOf(ModelFacts model, ContainerNameSettings names)
        {
            string file = DisciplineOf.FileName(model.File);
            return new ModelPlacement(
                file,
                DisciplineOf.File(model.File, names) ?? string.Empty,
                model.SiteNotRead == null ? model.Site : ModelPlacement.SiteNotRead,
                model.IsPlaced ? model.X : ModelPlacement.NotRead,
                model.IsPlaced ? model.Y : ModelPlacement.NotRead,
                model.IsPlaced ? model.Z : ModelPlacement.NotRead);
        }
    }

    /// <summary>
    /// Missing model, F234 and F244. A group of two disciplines or more, read off its file names by F219's one rule,
    /// BuildingGroup.DisciplinesOf, is one building checked in the workbook of each discipline of the setting, and one row in
    /// the workbook of each it holds no file of, with its row on Models. A group F233 did not build for one discipline is not
    /// processed, Bader's order of 19 Oct 2026. A group whose files were not read, or whose disciplines that rule says are
    /// UNKNOWN, is said in the log and never counted as missing a discipline, F222.
    /// </summary>
    public static class GroupDisciplinesCheck
    {
        public static CheckFindings Missing(IList<DisciplineGroup> groups, ModelRows rows, DisciplineReportSettings settings, ContainerNameSettings names)
        {
            CheckFindings found = new CheckFindings(CheckWords.Missing);

            foreach (DisciplineGroup group in groups)
            {
                if (group.Files.Count == 0)
                {
                    found.Note(group.Name + " not read, whether it misses a discipline is UNKNOWN, its files were not read, " + (group.WhyNoModels ?? "UNKNOWN why"));
                    continue;
                }

                if (group.NotBuiltForOneDiscipline)
                {
                    continue;
                }

                // The one rule of a group's disciplines off its file names, F219's, which says UNKNOWN for a name that does not
                // read or a federated model, so no group is said to miss a discipline on a guess.
                string why;
                IList<string> codes = BuildingGroup.DisciplinesOf(group.Files, names, NamePattern.DefaultDiscipline, out why);

                if (codes == null)
                {
                    found.Note(group.Name + " not read, whether it misses a discipline is UNKNOWN, " + why);
                    continue;
                }

                if (BuildingGroup.CannotClashWith(codes.Count))
                {
                    continue;
                }

                foreach (string discipline in settings.Disciplines)
                {
                    found.Judged(discipline);

                    if (codes.Contains(discipline))
                    {
                        continue;
                    }

                    string building = DisciplineOf.BuildingsOf(group, names);
                    found.Add(
                        new DisciplineIssue(discipline, CheckWords.Missing, CheckWords.High, building, CheckWords.NoModelIssued(discipline, codes), null, null, CheckWords.IssueTheModel(discipline)),
                        rows.Missing(discipline, building, codes));
                }
            }

            return found;
        }
    }

    /// <summary>One part of a file name that breaks the reference's pattern: what the file holds there and what the reference has.</summary>
    public sealed class NamingBreak
    {
        public NamingBreak(string whatIsWrong, string referenceHas)
        {
            WhatIsWrong = whatIsWrong;
            ReferenceHas = referenceHas;
        }

        public string WhatIsWrong { get; private set; }

        public string ReferenceHas { get; private set; }
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
        private readonly List<int> shapeOnly = new List<int>();

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

        public int BuildingPart { get; private set; }

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
            naming.BuildingPart = names.BuildingPart;
            naming.shapeOnly.AddRange(settings.ShapeOnlyParts);
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

        /// <summary>The pattern in words, for the one line of the log that says what was learned.</summary>
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

        /// <summary>
        /// The pattern as the Summary and To fix say it for a discipline, F244: each part the reference names hold alike as it
        /// reads, the discipline's part the discipline, the building part &lt;building&gt;, a part judged by its shape alone
        /// &lt;number&gt;, any other &lt;part N&gt;, such as 1104-PAR-&lt;building&gt;-ZZZ-ME-MOD-&lt;number&gt;. The words of
        /// Describe where no pattern was learned.
        /// </summary>
        public string Pattern(string discipline)
        {
            if (!WasLearned)
            {
                return Describe();
            }

            List<string> words = new List<string>();

            for (int i = 0; i < PartCount; i++)
            {
                words.Add(i + 1 == DisciplinePart ? discipline
                    : fixedValues[i] != null ? fixedValues[i]
                    : i + 1 == BuildingPart ? "<building>"
                    : shapeOnly.Contains(i + 1) ? "<number>"
                    : "<part " + (i + 1) + ">");
            }

            return string.Join(Separator.ToString(), words.ToArray());
        }

        /// <summary>Every part of a file's name that breaks the pattern, what it holds and what the reference has there, or none where it follows it.</summary>
        public IList<NamingBreak> Breaks(string file)
        {
            List<NamingBreak> breaks = new List<NamingBreak>();

            if (!WasLearned)
            {
                return breaks;
            }

            string[] split = Stem(file).Split(Separator);

            if (split.Length != PartCount)
            {
                breaks.Add(new NamingBreak(
                    "it has " + split.Length + " parts split on " + Separator, Reference + " names have " + PartCount));
            }

            for (int i = 0; i < Math.Min(split.Length, PartCount); i++)
            {
                if (i + 1 == DisciplinePart)
                {
                    continue;
                }

                if (fixedValues[i] != null && !string.Equals(split[i], fixedValues[i], StringComparison.Ordinal))
                {
                    breaks.Add(new NamingBreak("part " + (i + 1) + " reads " + split[i], Reference + " has " + fixedValues[i] + " there"));
                }
                else if (fixedValues[i] == null && shapes[i] != null && !Fits(split[i], shapes[i]))
                {
                    breaks.Add(new NamingBreak("part " + (i + 1) + " reads " + split[i], Reference + " has a part shaped like " + examples[i] + " there"));
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
            string name = DisciplineOf.FileName(file);
            int dot = name.LastIndexOf('.');
            return dot > 0 ? name.Substring(0, dot) : name;
        }

        /// <summary>
        /// The naming check of a package, F234 and F244: the pattern learned off the reference's files, every file of another
        /// discipline read against it and its row of Models told whether it follows, and a model of a group the run built
        /// that breaks it one Medium row, each part it breaks a line of the log with what the reference has there. The pattern
        /// is a line of the log too, and the reference's own files are the pattern and are not judged.
        /// </summary>
        public static CheckFindings Of(
            IList<DisciplineGroup> groups, ModelRows rows, DisciplineReportSettings settings, ContainerNameSettings names, out ReferenceNaming naming)
        {
            CheckFindings found = new CheckFindings(CheckWords.Naming);
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

            naming = Learn(referenceFiles, settings, names);
            found.Note(naming.Describe());

            if (!naming.WasLearned)
            {
                return found;
            }

            foreach (DisciplineGroup group in groups)
            {
                foreach (string file in group.Files)
                {
                    string code = DisciplineOf.File(file, names);

                    if (code == null)
                    {
                        found.Note(DisciplineOf.FileName(file) + " not judged, its name does not read");
                        continue;
                    }

                    if (string.Equals(code, settings.Reference, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    ModelRow row = rows.In(group, file);
                    IList<NamingBreak> breaks = naming.Breaks(file);
                    row.NameFollows = breaks.Count == 0;

                    if (group.NotBuiltForOneDiscipline)
                    {
                        continue;
                    }

                    found.Judged(code);

                    if (breaks.Count == 0)
                    {
                        continue;
                    }

                    foreach (NamingBreak broken in breaks)
                    {
                        found.Note(row.File + "  " + broken.WhatIsWrong + ", " + broken.ReferenceHas);
                    }

                    found.Add(
                        new DisciplineIssue(code, CheckWords.Naming, CheckWords.Medium, row.Building, CheckWords.NameShould(naming.Pattern(code)), null, null, CheckWords.Rename),
                        row);
                }
            }

            return found;
        }
    }

    /// <summary>File age, F234 and F244: each NWC's age in whole days off the date the disk gives, on its row of Models, and a model of a group the run built whose whole days are over the setting, 7, one Medium row.</summary>
    public static class FileAgeCheck
    {
        public static CheckFindings Of(
            IList<DisciplineGroup> groups, ModelRows rows, IList<FileDate> dates, DateTime now, DisciplineReportSettings settings, ContainerNameSettings names)
        {
            CheckFindings found = new CheckFindings(CheckWords.FileAge);
            Dictionary<string, FileDate> byFile = new Dictionary<string, FileDate>(StringComparer.OrdinalIgnoreCase);

            foreach (FileDate date in dates ?? new List<FileDate>())
            {
                byFile[date.File] = date;
            }

            foreach (DisciplineGroup group in groups)
            {
                foreach (string file in group.Files)
                {
                    FileDate date;

                    if (!byFile.TryGetValue(file, out date) || !date.Written.HasValue)
                    {
                        found.Note(DisciplineOf.FileName(file) + " not read, its date was not read, " + (date == null ? "UNKNOWN why, no date was handed in" : date.WhyNotRead));
                        continue;
                    }

                    ModelRow row = rows.In(group, file);
                    int days = (int)Math.Floor((now - date.Written.Value).TotalDays);
                    row.AgeDays = days;

                    if (group.NotBuiltForOneDiscipline)
                    {
                        continue;
                    }

                    found.Judged(row.Discipline);

                    // Judged on the whole days the cell writes, so a cell of 7 is never over 7, the breaker's reading of a92838b.
                    if (days > settings.OlderThanDays)
                    {
                        found.Add(
                            new DisciplineIssue(row.Discipline, CheckWords.FileAge, CheckWords.Medium, row.Building, CheckWords.Saved(date.Written.Value), days, CheckWords.Days(days), CheckWords.ExportFresh),
                            row);
                    }
                }
            }

            return found;
        }
    }

    /// <summary>
    /// CAD imports and Generic Models, F234 and F244: one Low row per model and CAD file, and one per model, family and type,
    /// each with its elements counted and every element id once, an id not read counted on its own. The row of Models holds
    /// the distinct CAD files and the Generic Models elements. A model whose items were not walked is said in the log.
    /// </summary>
    public static class ElementChecks
    {
        public static CheckFindings Cad(IList<DisciplineGroup> groups, ModelRows rows, DisciplineReportSettings settings)
        {
            return Of(groups, rows, CheckWords.Cad, m => m.CadNotRead, m => m.Cad,
                e => new KeyValuePair<string, string>(e.Name ?? settings.NotReadable, null),
                (row, kinds, elements) => row.CadFiles = kinds);
        }

        public static CheckFindings Generic(IList<DisciplineGroup> groups, ModelRows rows, DisciplineReportSettings settings)
        {
            return Of(groups, rows, CheckWords.Generic, m => m.GenericNotRead, m => m.Generic,
                e => new KeyValuePair<string, string>(e.Family ?? settings.NotReadable, e.Type ?? settings.NotReadable),
                (row, kinds, elements) => row.GenericElements = elements);
        }

        private static CheckFindings Of(
            IList<DisciplineGroup> groups, ModelRows rows, string check, Func<ModelFacts, string> notRead, Func<ModelFacts, IList<ElementFact>> elements,
            Func<ElementFact, KeyValuePair<string, string>> keyOf, Action<ModelRow, int, int> counted)
        {
            CheckFindings found = new CheckFindings(check);

            foreach (DisciplineGroup group in groups)
            {
                if (group.Models == null)
                {
                    if (group.Files.Count > 0)
                    {
                        found.Note(group.Name + " not read, its models were not read, " + group.WhyNoModels);
                    }

                    continue;
                }

                foreach (ModelFacts model in group.Models)
                {
                    ModelRow row = rows.In(group, model.File);

                    if (notRead(model) != null)
                    {
                        found.Note(row.File + " not read, its items were not walked, " + notRead(model));
                        continue;
                    }

                    List<KeyValuePair<string, string>> order = new List<KeyValuePair<string, string>>();
                    Dictionary<KeyValuePair<string, string>, List<string>> ids = new Dictionary<KeyValuePair<string, string>, List<string>>();
                    HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
                    int count = 0;

                    foreach (ElementFact element in elements(model))
                    {
                        KeyValuePair<string, string> key = keyOf(element);
                        List<string> into;

                        if (!ids.TryGetValue(key, out into))
                        {
                            into = new List<string>();
                            ids.Add(key, into);
                            order.Add(key);
                        }

                        if (element.ElementId == null || !into.Contains(element.ElementId))
                        {
                            into.Add(element.ElementId);
                        }

                        count += element.ElementId == null || seen.Add(element.ElementId) ? 1 : 0;
                    }

                    counted(row, order.Count, count);

                    if (group.NotBuiltForOneDiscipline)
                    {
                        continue;
                    }

                    found.Judged(row.Discipline);

                    foreach (KeyValuePair<string, string> key in order)
                    {
                        int elementCount = ids[key].Count;
                        DisciplineIssue issue = new DisciplineIssue(
                            row.Discipline, check, CheckWords.Low, row.Building,
                            key.Value == null ? CheckWords.Imported(key.Key) : CheckWords.Family(key.Key, key.Value),
                            elementCount, CheckWords.Elements(elementCount),
                            key.Value == null ? CheckWords.ReplaceImport : CheckWords.MoveFamily);
                        found.Add(issue.WithElements(key.Key, key.Value, ids[key]), row);
                    }
                }
            }

            return found;
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
