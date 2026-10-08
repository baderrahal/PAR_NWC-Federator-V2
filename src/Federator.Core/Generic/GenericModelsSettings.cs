using System;
using Federator.Core.Report;
using Federator.Core.Sets;
using Federator.Core.Teams;

namespace Federator.Core.Generic
{
    /// <summary>
    /// What names the Generic Models of a model, F128 and FR-177, Bader's request 3 under Q112, a
    /// setting and never a constant. A value that cannot work is refused where it is set.
    ///
    /// THE CATEGORY VALUE IS THE TOOL'S OWN READING AND NOT YET MEASURED ON 1A02MM AND 1A04PK. The
    /// tool's list of 374 category values holds Generic Models, and the probe of 2026-09-20 counted
    /// 62 items of that category over all ten C02 NWFs, docs\history\scan.md 5i. That probe read the
    /// first property displayed as Category, Revit Category or Element Category on the item, so it is
    /// not shown to be the property the client's file asks, LcRevitPropertyElementCategory of the
    /// Element tab. Whether the items of the two wave buildings carry the value there is UNKNOWN until
    /// the probe of the laptop lane reads them, and a value that differs is changed here and nowhere else.
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

        /// <summary>The name of the sheet in the group's workbook, the folder's own words.</summary>
        public const string DefaultSheetName = "Generic Models";

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

        public GenericModelsSettings()
        {
            categoryValue = DefaultCategoryValue;
            folderName = DefaultFolderName;
            sheetName = DefaultSheetName;
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
