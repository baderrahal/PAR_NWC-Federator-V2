using System;
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

        public GenericModelsSettings()
        {
            categoryValue = DefaultCategoryValue;
            folderName = DefaultFolderName;
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
