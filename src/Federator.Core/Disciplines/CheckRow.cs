using System.Collections.Generic;

namespace Federator.Core.Disciplines
{
    /// <summary>
    /// One line of a discipline workbook, F209, Bader's columns in his order: community, package, group, file, check,
    /// result, value, element id. Every word in it is made by a rule of this folder and nothing is free text. The
    /// element id is empty on a line that is about no element, and the setting's words where an element's id could not
    /// be read.
    /// </summary>
    public sealed class CheckRow
    {
        public CheckRow(
            string discipline, string community, string package, string group, string file, string check, string result, string value, string elementId)
        {
            Discipline = discipline ?? string.Empty;
            Community = community ?? string.Empty;
            Package = package ?? string.Empty;
            Group = group ?? string.Empty;
            File = file ?? string.Empty;
            Check = check ?? string.Empty;
            Result = result ?? string.Empty;
            Value = value ?? string.Empty;
            ElementId = elementId ?? string.Empty;
        }

        /// <summary>The workbook the line goes in.</summary>
        public string Discipline { get; private set; }

        public string Community { get; private set; }

        public string Package { get; private set; }

        public string Group { get; private set; }

        public string File { get; private set; }

        public string Check { get; private set; }

        public string Result { get; private set; }

        public string Value { get; private set; }

        public string ElementId { get; private set; }

        /// <summary>The cells in the order of the columns.</summary>
        public IList<string> Cells()
        {
            return new[] { Community, Package, Group, File, Check, Result, Value, ElementId };
        }
    }

    /// <summary>The words of the checks and their results, one place for the workbooks, the log and the tests.</summary>
    public static class CheckWords
    {
        public static readonly string[] Columns = { "Community", "Package", "Group", "File", "Check", "Result", "Value", "Element ID" };

        public const string Coordinates = "Coordinates";
        public const string Missing = "Missing discipline";
        public const string Naming = "Naming";
        public const string FileAge = "File age";
        public const string Cad = "CAD";
        public const string Generic = "Generic Models";

        /// <summary>The six checks in the order each workbook carries them.</summary>
        public static readonly string[] All = { Coordinates, Missing, Naming, FileAge, Cad, Generic };

        public const string Far = "far";
        public const string Near = "near";
        public const string Reference = "reference";
        public const string NotRead = "not read";
        public const string MissingHere = "missing";
        public const string NoneMissing = "none missing";
        public const string NotInThePackage = "not in this package";
        public const string Breaks = "breaks the pattern";
        public const string Follows = "follows the pattern";
        public const string Pattern = "the pattern";
        public const string NoPattern = "no pattern";
        public const string Found = "found";
        public const string NoneFound = "none found";
        public const string Older = "older than the limit";
        public const string Within = "within the limit";
    }
}
