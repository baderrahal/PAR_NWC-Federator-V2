using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Federator.Core.Communities;

namespace Federator.Core.Disciplines
{
    /// <summary>
    /// One row of the To fix sheet of a discipline workbook, F244, Bader's order of 19 Oct 2026: the check, its priority, the
    /// building, the model, the finding, the value as a number and its unit, and the words to send the modeller. A row of CAD
    /// imports or Generic Models carries its CAD file or family, its type and its element ids, which the Element IDs sheet
    /// lists one a row. Every word is made by a rule of this folder, nothing is free text.
    /// </summary>
    public sealed class DisciplineIssue
    {
        private readonly List<string> elementIds = new List<string>();

        public DisciplineIssue(
            string discipline, string check, string priority, string building, string finding, double? value, string unit, string fix)
        {
            Discipline = discipline ?? string.Empty;
            Check = check ?? string.Empty;
            Priority = priority ?? string.Empty;
            Building = building ?? string.Empty;
            Finding = finding ?? string.Empty;
            Value = value;
            Unit = unit ?? string.Empty;
            Fix = fix ?? string.Empty;
        }

        /// <summary>The workbook the row goes in.</summary>
        public string Discipline { get; private set; }

        /// <summary>The check of the Summary the row belongs to, one of CheckWords.All.</summary>
        public string Check { get; private set; }

        public string Priority { get; private set; }

        /// <summary>The building code read off the file name, such as 1B06BC, or the group's codes where the row is about a building.</summary>
        public string Building { get; private set; }

        public string Finding { get; private set; }

        /// <summary>The value as a number, metres, days or elements, or null where the row has none.</summary>
        public double? Value { get; private set; }

        public string Unit { get; private set; }

        public string Fix { get; private set; }

        /// <summary>The CAD file or the family of a row of CAD imports or Generic Models, or null.</summary>
        public string Source { get; private set; }

        /// <summary>The type of a row of Generic Models, or null.</summary>
        public string Type { get; private set; }

        /// <summary>Every element id of the row, each once, an id not read as null.</summary>
        public IList<string> ElementIds
        {
            get { return elementIds.AsReadOnly(); }
        }

        /// <summary>The model the row is about, set when its check adds it, or the building's row where no model is there.</summary>
        public ModelRow Row { get; internal set; }

        internal DisciplineIssue WithElements(string source, string type, IEnumerable<string> ids)
        {
            Source = source;
            Type = type;
            elementIds.AddRange(ids);
            return this;
        }
    }

    /// <summary>What a row of Models says of where its model sits, F244.</summary>
    public enum ModelMeasure
    {
        /// <summary>Not measured: its group's models, its placement or every placement of its group was not read.</summary>
        Unknown,

        /// <summary>The only discipline of its building, a group the run did not build.</summary>
        Alone,

        /// <summary>The reference model of its building.</summary>
        Reference,

        /// <summary>The reference model of its building, on Revit's internal origin while another model names a shared site.</summary>
        InternalOrigin,

        /// <summary>Measured from the reference model of its building.</summary>
        Measured
    }

    /// <summary>
    /// One row of the Models sheet, F244: one NWC of the package with what each check read of it, or one building that has
    /// other disciplines and no model of this one. A value a check did not read stays null and is written UNKNOWN, never a
    /// nought.
    /// </summary>
    public sealed class ModelRow
    {
        private readonly List<DisciplineIssue> issues = new List<DisciplineIssue>();

        internal ModelRow(string file, string discipline, string building, string model, bool isChecked, string present)
        {
            File = file ?? string.Empty;
            Discipline = discipline ?? string.Empty;
            Building = building ?? string.Empty;
            Model = model;
            Checked = isChecked;
            Present = present;
        }

        /// <summary>The NWC's name, or empty on the row of a building with no model of this discipline.</summary>
        public string File { get; private set; }

        public string Discipline { get; private set; }

        public string Building { get; private set; }

        /// <summary>The short name, the discipline and the number of the file, such as ME-000002, or null on a missing row.</summary>
        public string Model { get; private set; }

        /// <summary>Whether the checks judge the model, false for a group of one discipline the run did not build.</summary>
        public bool Checked { get; private set; }

        /// <summary>The disciplines the building holds, in words, on the row of a building with no model of this discipline, else null.</summary>
        public string Present { get; private set; }

        public bool IsMissing
        {
            get { return Present != null; }
        }

        public ModelMeasure Measure { get; internal set; }

        /// <summary>The discipline of the reference model of its building, its own on the reference, or null where not measured.</summary>
        public string ComparedWith { get; internal set; }

        /// <summary>The straight line from the reference model in metres, where Measure is Measured.</summary>
        public double Metres { get; internal set; }

        /// <summary>Whether the name follows the reference's pattern, or null where it was not judged.</summary>
        public bool? NameFollows { get; internal set; }

        /// <summary>The age of the file in whole days, or null where its date was not read.</summary>
        public int? AgeDays { get; internal set; }

        /// <summary>The distinct CAD files imported, or null where the model's items were not walked.</summary>
        public int? CadFiles { get; internal set; }

        /// <summary>The Generic Models elements, each id once, or null where the model's items were not walked.</summary>
        public int? GenericElements { get; internal set; }

        /// <summary>Whether Coordinates found the model's site not read.</summary>
        internal bool SiteNotRead { get; set; }

        /// <summary>The disciplines the files of the model's building read, what the grey line under Tabs may say it has.</summary>
        internal IList<string> BuildingHas { get; set; }

        /// <summary>Whether Naming was to judge the model's name, a model not of the reference discipline whose name reads.</summary>
        internal bool NamingDue { get; set; }

        /// <summary>Whether the model is in its group's facts and not among the group's files.</summary>
        internal bool NotInFiles { get; set; }

        /// <summary>Whether its building was not judged on Coordinates, a placed model of it with its site not read, the guard pass of F244.</summary>
        internal bool CoordinatesNotJudged { get; set; }

        /// <summary>
        /// Whether a value of the model was not read: its placement or the placement of every model of its building, its site,
        /// its CAD walk or its Generic walk, no facts of it at all, the breaker's S1 on a92838b, its file date, its name where
        /// Naming was to judge it and could not, such as in a package with no reference file, or the model not among its group's
        /// files, the reviewer's B1 on 4f5b5bb, or its building not judged on Coordinates since a placed model's site was not read.
        /// </summary>
        public bool SomethingNotRead
        {
            get
            {
                return Measure == ModelMeasure.Unknown || SiteNotRead || !CadFiles.HasValue || !GenericElements.HasValue
                    || !AgeDays.HasValue || (NamingDue && !NameFollows.HasValue) || NotInFiles || CoordinatesNotJudged;
            }
        }

        /// <summary>
        /// The chip of the Status cell: MISSING, NOT CHECKED for a group the run did not build, FIX where a High row is the
        /// model's, REVIEW where any row is, NOT CHECKED where nothing was found and a value was not read, else OK.
        /// </summary>
        public string Status
        {
            get
            {
                if (IsMissing)
                {
                    return CheckWords.MissingStatus;
                }

                if (!Checked)
                {
                    return CheckWords.NotChecked;
                }

                if (issues.Exists(i => i.Priority == CheckWords.High))
                {
                    return CheckWords.FixStatus;
                }

                if (issues.Count > 0)
                {
                    return CheckWords.Review;
                }

                return SomethingNotRead ? CheckWords.NotChecked : CheckWords.Ok;
            }
        }

        /// <summary>The priority of the model's row of a check, the highest where it has more than one, or null where it has none.</summary>
        public string PriorityOf(string check)
        {
            string found = null;

            foreach (DisciplineIssue issue in issues)
            {
                if (issue.Check == check && (found == null || CheckWords.SeverityRank(issue.Priority) < CheckWords.SeverityRank(found)))
                {
                    found = issue.Priority;
                }
            }

            return found;
        }

        internal void Add(DisciplineIssue issue)
        {
            issues.Add(issue);
            issue.Row = this;
        }
    }

    /// <summary>One row of the Summary sheet, F244: a check, what it checks, how many it checked, how many have an issue, its status and the words for the modeller.</summary>
    public sealed class SummaryRow
    {
        public SummaryRow(string check, string unit, int? checkedCount, int? withIssue, string status, string whatIsChecked, string fix)
        {
            Check = check;
            Unit = unit;
            Checked = checkedCount;
            WithIssue = withIssue;
            Status = status;
            WhatIsChecked = whatIsChecked;
            Fix = fix ?? string.Empty;
        }

        public string Check { get; private set; }

        /// <summary>What Checked and With issue count, model or building.</summary>
        public string Unit { get; private set; }

        /// <summary>The models or buildings the check judged, or null where it judged none and the cell is blank.</summary>
        public int? Checked { get; private set; }

        public int? WithIssue { get; private set; }

        public string Status { get; private set; }

        public string WhatIsChecked { get; private set; }

        /// <summary>The words for the modeller, or empty where nothing is to do.</summary>
        public string Fix { get; private set; }
    }

    /// <summary>One row of the Element IDs sheet, F244: one element of a CAD import or a Generic Models family of one model, each id once a model.</summary>
    public sealed class ElementIdRow
    {
        public ElementIdRow(string building, string model, string check, string source, string type, string id)
        {
            Building = building;
            Model = model;
            Check = check;
            Source = source;
            Type = type;
            Id = id;
        }

        public string Building { get; private set; }

        public string Model { get; private set; }

        /// <summary>The check as the row names it, CAD import or Generic Models.</summary>
        public string Check { get; private set; }

        public string Source { get; private set; }

        public string Type { get; private set; }

        /// <summary>The element id as read, or null where it was not read.</summary>
        public string Id { get; private set; }
    }

    /// <summary>The words of the discipline workbooks, F244, Bader's order of 19 Oct 2026, one place for the workbooks, the log and the tests.</summary>
    public static class CheckWords
    {
        public const string Coordinates = "Coordinates";
        public const string Missing = "Missing model";
        public const string Naming = "Naming";
        public const string FileAge = "File age";
        public const string Cad = "CAD imports";
        public const string Generic = "Generic Models";

        /// <summary>The six checks in Bader's order, the rows of the Summary.</summary>
        public static readonly ReadOnlyCollection<string> All = new ReadOnlyCollection<string>(new[] { Coordinates, Missing, Naming, FileAge, Cad, Generic });

        /// <summary>A row of CAD imports on To fix and Element IDs names one import, the mock's word.</summary>
        public const string CadRow = "CAD import";

        public const string High = "High";
        public const string Medium = "Medium";
        public const string Low = "Low";

        /// <summary>The priorities in the order To fix is sorted by.</summary>
        public static readonly ReadOnlyCollection<string> Severities = new ReadOnlyCollection<string>(new[] { High, Medium, Low });

        public const string FixStatus = "FIX";
        public const string MissingStatus = "MISSING";
        public const string Review = "REVIEW";
        public const string Ok = "OK";
        public const string NotChecked = "NOT CHECKED";

        public const string Model = "model";
        public const string Building = "building";

        public const string ModelsLine = "One row per model.  Red: fix before the next clash run.  Amber: review.  Grey: nothing to do.";
        public const string ToFixLine = "One row per thing to fix.  High first.  Send the Fix words to the modeller as they are.";
        public const string ElementIdsLine = "One element per row.  Filter on Model, copy the Element ID cells, then in Revit: Manage > Select by ID.";
        public const string NothingToDo = "Nothing to do";
        public const string Tabs = "Tabs";

        public const string Itself = "itself";
        public const string None = "none";
        public const string Under = "under 1";
        public const string ReferenceWord = "reference";
        public const string Alone = "alone";
        public const string InternalOrigin = "internal origin";
        public const string Unknown = "UNKNOWN";
        public const string NameOk = "OK";
        public const string NameWrong = "wrong";
        public const string NameNotChecked = "not checked";

        public const string OnInternal = "On Revit's internal origin, not on a shared site";
        public const string ExportOnShared = "Export on the project shared coordinates. The other models of this building are measured from this one";
        public const string ExportAgain = "Export the NWC again on the project shared coordinates";
        public const string Rename = "Rename the NWC to the project pattern";
        public const string ExportFresh = "Export a fresh NWC from the current model";
        public const string ReplaceImport = "Replace with a Revit family, or delete it";
        public const string MoveFamily = "Move the family to its right category";
        public const string Metres = "m";

        /// <summary>Bader's priority of each check: High for Coordinates and Missing model, Medium for Naming and File age, Low for CAD imports and Generic Models.</summary>
        public static string PriorityOf(string check)
        {
            switch (check)
            {
                case Coordinates:
                case Missing:
                    return High;
                case Naming:
                case FileAge:
                    return Medium;
                case Cad:
                case Generic:
                    return Low;
                default:
                    throw new ArgumentException("the check " + check + " is not one of the six", "check");
            }
        }

        /// <summary>The place of a priority in the sort, High first.</summary>
        public static int SeverityRank(string priority)
        {
            int at = Severities.IndexOf(priority);
            return at < 0 ? Severities.Count : at;
        }

        /// <summary>The Check cell of To fix and Element IDs.</summary>
        public static string RowWord(string check)
        {
            return check == Cad ? CadRow : check;
        }

        /// <summary>What Checked and With issue count for a check: buildings for Missing model, models for the others.</summary>
        public static string UnitOf(string check)
        {
            return check == Missing ? Building : Model;
        }

        /// <summary>
        /// The words before the discipline in each title: the community, and the package's path in words where it is a
        /// subfolder, never C06 C06 itself and never a backslash.
        /// </summary>
        public static string Heading(string community, string package)
        {
            return string.IsNullOrEmpty(package) || package == PackageWords.ItselfName(community) ? community : community + " " + InWords(package);
        }

        /// <summary>A package's path with each folder separator a space, the one rule for a title and a workbook's name.</summary>
        public static string InWords(string path)
        {
            return (path ?? string.Empty).Replace(Path.DirectorySeparatorChar, ' ').Replace(Path.AltDirectorySeparatorChar, ' ').Trim();
        }

        public const string SummarySheet = "Summary";
        public const string ModelsSheet = "Models";
        public const string ToFixSheet = "To fix";
        public const string ElementIdsSheet = "Element IDs";

        /// <summary>The words after the discipline in B1 of each sheet.</summary>
        public const string SummaryTitle = "model checks";
        public const string ModelsTitle = "models";
        public const string ToFixTitle = "to fix";
        public const string ElementIdsTitle = "element IDs";

        public static readonly ReadOnlyCollection<string> SummaryColumns = new ReadOnlyCollection<string>(
            new[] { "Check", "What is checked", "Checked", "With issue", "Status", "What the modeller does" });

        public static readonly ReadOnlyCollection<string> ModelsColumns = new ReadOnlyCollection<string>(new[]
        {
            "Building", "Model", "Status", "Compared with", "Off the reference (m)", "Naming", "File age (days)", "CAD imports (files)",
            "Generic Models (elements)", "File name"
        });

        public static readonly ReadOnlyCollection<string> ToFixColumns = new ReadOnlyCollection<string>(
            new[] { "No.", "Priority", "Building", "Model", "Check", "Finding", "Value", "Unit", "Fix" });

        public static readonly ReadOnlyCollection<string> ElementIdsColumns = new ReadOnlyCollection<string>(
            new[] { "Building", "Model", "Check", "CAD file or family", "Type", "Element ID" });

        /// <summary>B1 of each sheet: the heading, three spaces, the discipline and the sheet's words.</summary>
        public static string Title(string heading, string discipline, string words)
        {
            return heading + "   " + discipline + " " + words;
        }

        /// <summary>B2 of the Summary, its three parts five spaces apart.</summary>
        public static string RunLine(DateTime run, string reference, string discipline, int models, int buildings)
        {
            return "Run " + Date(run) + "     Reference: the " + reference + " model of each building     "
                + Count(models, discipline + " model", discipline + " models") + " in " + Count(buildings, Building, "buildings");
        }

        /// <summary>
        /// B5 of the Summary. Where To fix holds no row and every model was judged by every check, they all pass. Otherwise one
        /// sentence a part, the first kept even at nought and the others left out at nought, F244 attempt 2, the breaker's S2.
        /// The words bent to one where a count is 1.
        /// </summary>
        public static string Lead(string discipline, int fix, int models, int missing, int review, bool allPass)
        {
            if (allPass && models > 0 && fix == 0 && missing == 0 && review == 0)
            {
                return "All " + models + " " + discipline + (models == 1 ? " model passes" : " models pass") + " every check.";
            }

            List<string> parts = new List<string>
            {
                fix + " of " + Count(models, discipline + " model", discipline + " models") + (fix == 1 || models == 1 ? " needs" : " need") + " a fix."
            };

            if (missing > 0)
            {
                parts.Add(missing + (missing == 1 ? " building has no " : " buildings have no ") + discipline + " model.");
            }

            if (review > 0)
            {
                parts.Add(Count(review, "model has", "models have") + " CAD imports or Generic Models to review.");
            }

            return string.Join("  ", parts.ToArray());
        }

        /// <summary>What is checked, the second cell of a Summary row.</summary>
        public static string WhatIsChecked(string check, string discipline, double farMetres, int olderThanDays, string pattern)
        {
            switch (check)
            {
                case Coordinates:
                    return "Within " + farMetres.ToString("0.###", CultureInfo.InvariantCulture) + " m of the reference model of its building, on the shared site";
                case Missing:
                    return "Every building that has other models has an " + discipline + " model";
                case Naming:
                    return pattern;
                case FileAge:
                    return "NWC saved in the last " + Count(olderThanDays, "day", "days");
                case Cad:
                    return "No DWG, IPT or IAM imported into the model";
                default:
                    return "No family left in the Generic Models category";
            }
        }

        /// <summary>What the modeller does, the last cell of a Summary row with an issue.</summary>
        public static string SummaryFix(string check, string discipline)
        {
            switch (check)
            {
                case Coordinates:
                    return "Export the NWC again from Revit on the project shared coordinates";
                case Missing:
                    return "Issue the " + discipline + " model for the building";
                case Naming:
                    return Rename;
                case FileAge:
                    return ExportFresh;
                case Cad:
                    return "Replace each import with a Revit family, or delete it";
                default:
                    return "Move each family to its right category, for example Mechanical Equipment";
            }
        }

        public static string NameIs(string pattern)
        {
            return "Name is " + pattern;
        }

        public static string NameShould(string pattern)
        {
            return "Name should be " + pattern;
        }

        /// <summary>The Naming row of the reference discipline's own workbook, whose names are the pattern.</summary>
        public static string SetThePattern(string reference)
        {
            return reference + " names set the pattern the other disciplines are checked against";
        }

        /// <summary>The finding of a model more than the setting from its reference, the metres to one decimal and both shared sites.</summary>
        public static string FarFinding(double metres, string model, string site, string reference, string referenceSite)
        {
            return metres.ToString("#,##0.0", CultureInfo.InvariantCulture) + " m from the " + reference + " model. "
                + model + " on site " + site + ", " + reference + " on " + referenceSite;
        }

        public static string FromInternal(string reference)
        {
            return reference + " of this building is on Revit's internal origin, so this distance cannot be judged";
        }

        public static string FixFirst(string reference)
        {
            return "Fix " + reference + " first, then run again";
        }

        /// <summary>The finding of a building with no model of the discipline.</summary>
        public static string NoModelIssued(string discipline, IList<string> present)
        {
            return "No " + discipline + " model. " + And(present) + " are issued";
        }

        /// <summary>The File name cell of the Models row of a building with no model of the discipline.</summary>
        public static string ThisBuildingHas(string discipline, string present)
        {
            return "No " + discipline + " model. This building has " + present + ".";
        }

        public static string IssueTheModel(string discipline)
        {
            return "Issue the " + discipline + " model for this building";
        }

        public static string Saved(DateTime written)
        {
            return "Saved " + Date(written);
        }

        /// <summary>The finding of a model's CAD import, Bader's words of 19 Oct 2026 where its name carries no file name, F245 item 1.</summary>
        public const string ImportedNameNotRead = "Imported CAD file, name not readable";

        /// <summary>The finding of a model's imports of one CAD file, or ImportedNameNotRead where the name is null.</summary>
        public static string Imported(string name)
        {
            return name == null ? ImportedNameNotRead : "Imported " + name;
        }

        public static string Family(string family, string type)
        {
            return "Family " + family + ", type " + type;
        }

        public static string Elements(int count)
        {
            return count == 1 ? "element" : "elements";
        }

        public static string Days(int count)
        {
            return count == 1 ? "day" : "days";
        }

        /// <summary>The three lines under Tabs, the sheet and what it holds.</summary>
        public static IList<KeyValuePair<string, string>> TabLines(string discipline)
        {
            return new[]
            {
                new KeyValuePair<string, string>(ModelsSheet, "one row per " + discipline + " model, red where a check failed"),
                new KeyValuePair<string, string>(ToFixSheet, "one row per thing to fix, High first, with the words to send the modeller"),
                new KeyValuePair<string, string>(ElementIdsSheet, "one element per row. Filter on Model, copy the IDs, then Revit Manage > Select by ID")
            };
        }

        /// <summary>
        /// The grey line under Tabs, F244 attempt 2, the reviewer's 3 and the breaker's S8: each building of these models whose
        /// reference is not of the setting's discipline, saying only what its files show. One with a file of the setting's
        /// discipline, which was not placed since the reference moved, is said in its own words. The others are grouped by the
        /// discipline that is their reference, in Ordinal order, in the mock's words, each named under a sentence only where its
        /// files hold none of the disciplines that sentence says it has not, and in its own words otherwise. Null where every
        /// building's reference is the setting's.
        /// </summary>
        internal static string ReferencesLine(string reference, IList<ModelRow> models)
        {
            List<string> seen = new List<string>();
            List<string> notPlaced = new List<string>();
            List<string> noReference = new List<string>();
            SortedDictionary<string, List<ModelRow>> byDiscipline = new SortedDictionary<string, List<ModelRow>>(StringComparer.Ordinal);

            foreach (ModelRow row in models)
            {
                if (row.ComparedWith == null || row.ComparedWith == reference || seen.Contains(row.Building))
                {
                    continue;
                }

                seen.Add(row.Building);

                if (row.BuildingHas.Contains(reference))
                {
                    notPlaced.Add("In " + row.Building + " the " + reference + " model was not placed, so it is measured from the " + row.ComparedWith + " model.");
                    continue;
                }

                List<ModelRow> into;

                if (!byDiscipline.TryGetValue(row.ComparedWith, out into))
                {
                    into = new List<ModelRow>();
                    byDiscipline.Add(row.ComparedWith, into);
                }

                into.Add(row);
            }

            List<string> sentences = new List<string>();
            List<string> before = new List<string> { reference };

            foreach (KeyValuePair<string, List<ModelRow>> pair in byDiscipline)
            {
                List<string> buildings = new List<string>();

                foreach (ModelRow row in pair.Value)
                {
                    if (before.Exists(d => row.BuildingHas.Contains(d)))
                    {
                        noReference.Add("In " + row.Building + ", which has no " + reference + " model, the " + pair.Key + " model is the reference.");
                    }
                    else
                    {
                        buildings.Add(row.Building);
                    }
                }

                if (buildings.Count == 0)
                {
                    continue;
                }

                string named = "(" + string.Join(", ", buildings.ToArray()) + ")";
                sentences.Add(sentences.Count == 0
                    ? "Where a building has no " + reference + " model, the " + pair.Key + " model is the reference " + named + "."
                    : "Where it has no " + Or(before) + ", the " + pair.Key + " model is " + named + ".");
                before.Add(pair.Key);
            }

            sentences.AddRange(notPlaced);
            sentences.AddRange(noReference);
            return sentences.Count == 0 ? null : string.Join(" ", sentences.ToArray());
        }

        /// <summary>A date the way the workbooks write it, 19 Oct 2026.</summary>
        public static string Date(DateTime value)
        {
            return value.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
        }

        public static string Count(int count, string one, string many)
        {
            return count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);
        }

        /// <summary>A list in words, AR, EL and ST.</summary>
        public static string And(IList<string> items)
        {
            return Joined(items, " and ");
        }

        private static string Or(IList<string> items)
        {
            return Joined(items, " or ");
        }

        private static string Joined(IList<string> items, string last)
        {
            if (items.Count < 2)
            {
                return items.Count == 0 ? string.Empty : items[0];
            }

            List<string> head = new List<string>(items);
            head.RemoveAt(head.Count - 1);
            return string.Join(", ", head.ToArray()) + last + items[items.Count - 1];
        }
    }
}
