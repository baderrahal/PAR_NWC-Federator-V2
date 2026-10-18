using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Disciplines
{
    /// <summary>
    /// One row of the issues table of a discipline workbook, F234, Bader's round of 19 Oct 2026, fix 4, his columns in his
    /// order: No., Severity, Building, File, Check, What is wrong, Value, Action, Element IDs. A row is made only for what needs
    /// action and every word in it is made by a rule of this folder, nothing is free text. The number is given when the rows
    /// of a workbook are sorted, so it is not held here.
    /// </summary>
    public sealed class DisciplineIssue
    {
        public DisciplineIssue(
            string discipline, string check, string building, string file, string whatIsWrong, string value, string action, string elementIds)
        {
            Discipline = discipline ?? string.Empty;
            Check = check ?? string.Empty;
            Severity = CheckWords.SeverityOf(Check);
            Building = building ?? string.Empty;
            File = file ?? string.Empty;
            WhatIsWrong = whatIsWrong ?? string.Empty;
            Value = value ?? string.Empty;
            Action = action ?? string.Empty;
            ElementIds = elementIds ?? string.Empty;
        }

        /// <summary>The workbook the row goes in.</summary>
        public string Discipline { get; private set; }

        public string Severity { get; private set; }

        /// <summary>The building code read off the file name, such as 1B06BC, or the group's codes where the row names no file.</summary>
        public string Building { get; private set; }

        /// <summary>The file's name, or empty where the row is about a group and no one file.</summary>
        public string File { get; private set; }

        public string Check { get; private set; }

        public string WhatIsWrong { get; private set; }

        public string Value { get; private set; }

        public string Action { get; private set; }

        /// <summary>Every element id of the row, comma separated, or empty where the row is about no element.</summary>
        public string ElementIds { get; private set; }

        /// <summary>The cells in the order of the columns, the row's number first.</summary>
        public IList<string> Cells(int number)
        {
            return new[]
            {
                number.ToString(CultureInfo.InvariantCulture), Severity, Building, File, Check, WhatIsWrong, Value, Action, ElementIds
            };
        }
    }

    /// <summary>One row of the summary table of a discipline workbook, F234: a check, the files it read, its issues and its result.</summary>
    public sealed class CheckSummary
    {
        public CheckSummary(string check, int filesChecked, int issues)
        {
            Check = check ?? string.Empty;
            FilesChecked = filesChecked;
            Issues = issues;
        }

        public string Check { get; private set; }

        /// <summary>The files of this workbook's discipline the check read, CheckWords.FilesCheckedRule.</summary>
        public int FilesChecked { get; private set; }

        /// <summary>The rows of the check in the issues table of the same workbook.</summary>
        public int Issues { get; private set; }

        /// <summary>OK where the check found nothing, Issues otherwise, Bader's two words.</summary>
        public string Result
        {
            get { return Issues == 0 ? CheckWords.Ok : CheckWords.IssuesWord; }
        }
    }

    /// <summary>The words of the discipline workbooks, F234, one place for the workbooks, the log and the tests.</summary>
    public static class CheckWords
    {
        /// <summary>The columns of the issues table, Bader's in his order.</summary>
        public static readonly string[] IssueColumns = { "No.", "Severity", "Building", "File", "Check", "What is wrong", "Value", "Action", "Element IDs" };

        /// <summary>The columns of the summary table, Bader's in his order.</summary>
        public static readonly string[] SummaryColumns = { "Check", "Files checked", "Issues", "Result" };

        public const string Coordinates = "Coordinates";
        public const string Missing = "Missing model";
        public const string OneDiscipline = "One discipline group";
        public const string Naming = "Naming";
        public const string FileAge = "File age";
        public const string Cad = "CAD";
        public const string Generic = "Generic Models";

        /// <summary>The seven checks in Bader's order, the order of the summary table.</summary>
        public static readonly string[] All = { Coordinates, Missing, OneDiscipline, Naming, FileAge, Cad, Generic };

        public const string High = "High";
        public const string Medium = "Medium";
        public const string Low = "Low";

        /// <summary>The severities in the order the issues are sorted by.</summary>
        public static readonly string[] Severities = { High, Medium, Low };

        public const string Ok = "OK";
        public const string IssuesWord = "Issues";

        public const string ExportAgain = "Export again on the project shared coordinates";
        public const string ExportOnShared = "Export on the project shared coordinates";
        public const string OnInternal = "on Revit internal origin";
        public const string OnlyDiscipline = "only discipline in this group, not federated";

        /// <summary>Bader's severities: High for Coordinates and Missing model, Medium for Naming, File age and One discipline group, Low for CAD and Generic Models.</summary>
        public static string SeverityOf(string check)
        {
            switch (check)
            {
                case Coordinates:
                case Missing:
                    return High;
                case Naming:
                case FileAge:
                case OneDiscipline:
                    return Medium;
                case Cad:
                case Generic:
                    return Low;
                default:
                    throw new ArgumentException("the check " + check + " is not one of the seven", "check");
            }
        }

        /// <summary>The place of a severity in the sort, High first.</summary>
        public static int SeverityRank(string severity)
        {
            int at = Array.IndexOf(Severities, severity);
            return at < 0 ? Severities.Length : at;
        }

        /// <summary>Row 1 of a workbook.</summary>
        public static string Title(string community, string package, string discipline)
        {
            return community + " " + package + " " + discipline + " model checks";
        }

        /// <summary>Row 2 of a workbook.</summary>
        public static string RunDate(DateTime run)
        {
            return "Run date " + run.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>Row 3 of a workbook.</summary>
        public static string ReferenceLine(string reference)
        {
            return "Reference discipline " + reference;
        }

        public static string NoModel(string discipline)
        {
            return "no " + discipline + " model in this group";
        }

        public static string IssueTheModel(string discipline)
        {
            return "Issue the " + discipline + " model for this building";
        }

        /// <summary>What is wrong with a far model, the setting's metres and the reference's discipline.</summary>
        public static string FarFrom(double farMetres, string referenceDiscipline)
        {
            return "more than " + farMetres.ToString("0.###", CultureInfo.InvariantCulture) + " m from the " + referenceDiscipline + " model";
        }

        /// <summary>A far model's value: the distance in metres to three decimals and both shared sites.</summary>
        public static string FarValue(double millimetres, string discipline, string site, string referenceDiscipline, string referenceSite)
        {
            return (millimetres / 1000.0).ToString("0.000", CultureInfo.InvariantCulture) + " m, "
                + discipline + " on " + site + ", " + referenceDiscipline + " on " + referenceSite;
        }

        /// <summary>The value of a reference on Internal: the shared sites the other models of its group name.</summary>
        public static string OthersOn(IList<string> sites)
        {
            return "the other models on " + string.Join(", ", new List<string>(sites).ToArray());
        }

        public static string OlderThan(int days)
        {
            return "older than " + days.ToString(CultureInfo.InvariantCulture) + (days == 1 ? " day" : " days");
        }

        /// <summary>A file's age: its date and its age in whole days.</summary>
        public static string Age(DateTime written, int days)
        {
            return written.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ", "
                + days.ToString(CultureInfo.InvariantCulture) + (days == 1 ? " day" : " days");
        }

        public static string CadFile(string name)
        {
            return "CAD file " + name;
        }

        public static string GenericFamily(string family, string type)
        {
            return "Generic Models family " + family + ", type " + type;
        }

        public static string Count(int count)
        {
            return count.ToString(CultureInfo.InvariantCulture);
        }
    }
}
