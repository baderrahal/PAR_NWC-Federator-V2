using System;
using Federator.Core.Naming;
using Federator.Core.Report;
using Federator.Core.Sets;
using Federator.Core.Teams;

namespace Federator.Core.Generic
{
    /// <summary>
    /// What names the Generic Models of a model, F128 and FR-177, Bader's request 3 under Q112, a
    /// setting and never a constant. A value that cannot work is refused where it is set.
    ///
    /// THE CATEGORY VALUE AND THE TAB WERE MEASURED ON 1A02MM AND 1A04PK ON 2026-10-08, docs\history\scan.md
    /// 5z-zb. On 1A04PK the value Generic Models sits on the Element tab, LcRevitData_Element, under
    /// LcRevitPropertyElementCategory, exactly 14 characters, no trim or case variant, on 572 items in five
    /// of ten models, which is the tab and the property the client's file asks, so the default stands. On
    /// 1A02MM no item of 19,028 carries the value on any tab, so every set there is at nought and the block's
    /// nought line is the true reading. A value that differs on a later project is changed here and nowhere else.
    ///
    /// The two property names are not typed here: the category property is EmptySets.CategoryProperty
    /// and the Source File property is the one SilentMisses reads, each typed once under src. What this
    /// class types is the Element tab's own name, which the add-in's ModelFactsReader also types, and
    /// the words Navisworks shows for the three, the way the client's matrix writes them.
    /// </summary>
    public sealed class GenericModelsSettings
    {
        /// <summary>The value of the Category property that names a Generic Models item, as the tool's own list holds it.</summary>
        public const string DefaultCategoryValue = "Generic Models";

        /// <summary>The folder of the saved sets tree the sets go in, Bader's own words, request 3 under Q112.</summary>
        public const string DefaultFolderName = "Generic Models";

        /// <summary>The name of the sheet in the Generic Models workbook, the folder's own words.</summary>
        public const string DefaultSheetName = "Generic Models";

        /// <summary>
        /// What is put after the group's workbook name to name the Generic Models workbook, the folder's own
        /// words, so the two workbooks of a group sit together in the Clash Reports folder. A workbook of its
        /// own because FR-200 makes the Coverage sheet the second and last sheet of the group's workbook and
        /// the workbook check allows nothing after it, the lead's choice until Bader answers.
        /// </summary>
        public const string DefaultWorkbookSuffix = "Generic Models";

        /// <summary>
        /// The Element tab, the category every Revit property of an item sits under, as the client's matrix writes it.
        /// Public so the add-in can read it where ModelFactsReader types the same word as ElementTab.
        /// </summary>
        public const string ElementCategoryInternalName = "LcRevitData_Element";

        internal const string ElementCategoryDisplayName = "Element";

        /// <summary>What Navisworks shows for the Category property.</summary>
        internal const string CategoryPropertyDisplayName = "Category";

        /// <summary>What Navisworks shows for the Source File property.</summary>
        internal const string SourceFilePropertyDisplayName = "Source File";

        /// <summary>The data type of both values, as the client's matrix writes them.</summary>
        internal const string ValueType = "wstring";

        private string categoryValue;
        private string folderName;
        private string sheetName;
        private string workbookSuffix;

        public GenericModelsSettings()
        {
            categoryValue = DefaultCategoryValue;
            folderName = DefaultFolderName;
            sheetName = DefaultSheetName;
            workbookSuffix = DefaultWorkbookSuffix;
        }

        /// <summary>
        /// What the Category property of an item reads when it is a Generic Models item. Compared exactly
        /// and never trimmed, the way every other value of a set is. Not blank, and a value of spaces alone is blank.
        /// </summary>
        public string CategoryValue
        {
            get
            {
                return categoryValue;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("The category that names a Generic Models item cannot be blank.", "value");
                }

                categoryValue = value;
            }
        }

        /// <summary>
        /// The folder the sets go in, directly under the tree's root. Not blank, and not holding the
        /// slash that separates one folder from the next, which would make two folders of one.
        /// </summary>
        public string FolderName
        {
            get
            {
                return folderName;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.IndexOf('/') >= 0)
                {
                    throw new ArgumentException(
                        "The folder of the Generic Models sets needs a name that is not blank and holds no slash.", "value");
                }

                folderName = value;
            }
        }

        /// <summary>The sheet's name. One Excel refuses is refused here, by SheetNames' own rule.</summary>
        public string SheetName
        {
            get
            {
                return sheetName;
            }

            set
            {
                if (!SheetNames.IsAcceptable(value))
                {
                    throw new ArgumentException(
                        "The Generic Models sheet needs a name Excel accepts: 1 to " + SheetNames.MaxLength
                            + " characters, none of " + new string(SheetNames.Refused)
                            + ", no apostrophe at either end and not blank.",
                        "value");
                }

                sheetName = value;
            }
        }

        /// <summary>
        /// What follows the group's workbook name, with one space between, to name the Generic Models workbook.
        /// Refused where Windows refuses it in a file name, by FileNames' own rule, so a suffix nobody can write
        /// never loses the workbook.
        /// </summary>
        public string WorkbookSuffix
        {
            get
            {
                return workbookSuffix;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value) || !FileNames.CanBeAName(value))
                {
                    throw new ArgumentException(
                        "The Generic Models workbook needs a suffix Windows accepts in a file name: not blank and none of "
                            + FileNames.RefusedPrintable + ".",
                        "value");
                }

                workbookSuffix = value;
            }
        }

        /// <summary>
        /// The name of the group's Generic Models workbook, without its extension: the group's workbook name,
        /// one space and the suffix, so the two sit together in the Clash Reports folder under one name.
        /// </summary>
        public string WorkbookNameFor(string groupWorkbookName)
        {
            if (string.IsNullOrEmpty(groupWorkbookName))
            {
                throw new ArgumentException("The Generic Models workbook is named after the group's workbook, which needs a name.", "groupWorkbookName");
            }

            return groupWorkbookName + " " + workbookSuffix;
        }

        /// <summary>The Category property's internal name, typed once under src in EmptySets.</summary>
        internal static string CategoryPropertyInternalName
        {
            get { return EmptySets.CategoryProperty; }
        }

        /// <summary>The Source File property's internal name, typed once under src where the silent miss judge reads it.</summary>
        internal static string SourceFilePropertyInternalName
        {
            get { return SilentMisses.SourceFileProperty; }
        }
    }
}
