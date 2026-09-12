using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Federator.Core.Exchange
{
    /// <summary>One side of a clash test. Always a single clashselection in these files.</summary>
    public sealed class ClashSideDefinition
    {
        internal ClashSideDefinition(bool selfIntersect, int primitiveTypes, string locator)
        {
            SelfIntersect = selfIntersect;
            PrimitiveTypes = primitiveTypes;
            Locator = locator;
        }

        public bool SelfIntersect { get; private set; }

        /// <summary>The primtypes flag, kept as written because it is a bit mask.</summary>
        public int PrimitiveTypes { get; private set; }

        /// <summary>
        /// The name path this side points at, for example
        /// lcop_selection_set_tree/Mechanical/Mechanical-HVAC/BLD-ME-Air Terminals.
        /// Null when the side carried no locator.
        /// </summary>
        public string Locator { get; private set; }

        public bool HasLocator
        {
            get { return !string.IsNullOrEmpty(Locator); }
        }
    }

    /// <summary>One clashtest as it is written in the file, before any model is involved.</summary>
    public sealed class ClashTestDefinition
    {
        internal ClashTestDefinition(
            string name,
            string testType,
            string status,
            double toleranceInFileUnits,
            string fileUnits,
            bool mergeComposites,
            string linkageMode,
            IList<string> rules,
            ClashSideDefinition left,
            ClashSideDefinition right)
        {
            Name = name;
            TestType = testType;
            Status = status;
            ToleranceInFileUnits = toleranceInFileUnits;
            FileUnits = fileUnits;
            MergeComposites = mergeComposites;
            LinkageMode = linkageMode;
            Rules = new ReadOnlyCollection<string>(rules ?? new List<string>());
            Left = left;
            Right = right;
        }

        public string Name { get; private set; }

        /// <summary>The raw test_type string, for example hard_conservative.</summary>
        public string TestType { get; private set; }

        /// <summary>The raw status string, for example new.</summary>
        public string Status { get; private set; }

        /// <summary>The tolerance exactly as written, in <see cref="FileUnits"/>.</summary>
        public double ToleranceInFileUnits { get; private set; }

        public string FileUnits { get; private set; }

        public bool MergeComposites { get; private set; }

        /// <summary>The linkage mode, for example none. Read even though these files never use it.</summary>
        public string LinkageMode { get; private set; }

        /// <summary>The rule names under the rules element. Empty in these files, read anyway.</summary>
        public ReadOnlyCollection<string> Rules { get; private set; }

        public ClashSideDefinition Left { get; private set; }

        public ClashSideDefinition Right { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>A name element, carrying its internal key and its display value.</summary>
    public sealed class NameReference
    {
        internal NameReference(string internalName, string displayName)
        {
            InternalName = internalName;
            DisplayName = displayName;
        }

        public string InternalName { get; private set; }

        public string DisplayName { get; private set; }

        public override string ToString()
        {
            return DisplayName + " (" + InternalName + ")";
        }
    }

    /// <summary>A value element, carrying the data type and the data itself.</summary>
    public sealed class ValueReference
    {
        internal ValueReference(string dataType, string data)
        {
            DataType = dataType;
            Data = data;
        }

        public string DataType { get; private set; }

        public string Data { get; private set; }

        public override string ToString()
        {
            return Data + " (" + DataType + ")";
        }
    }

    /// <summary>
    /// One condition inside a findspec. Category can be absent, some conditions carry
    /// only a property.
    /// </summary>
    public sealed class SearchConditionDefinition
    {
        internal SearchConditionDefinition(
            string test, int flags, NameReference category, NameReference property, ValueReference value)
        {
            Test = test;
            Flags = flags;
            Category = category;
            Property = property;
            Value = value;
        }

        /// <summary>The test attribute, for example equals or contains.</summary>
        public string Test { get; private set; }

        /// <summary>The flags attribute. This joins conditions together, it is not part of the rule.</summary>
        public int Flags { get; private set; }

        /// <summary>Null when the condition carried no category element.</summary>
        public NameReference Category { get; private set; }

        public NameReference Property { get; private set; }

        public ValueReference Value { get; private set; }

        /// <summary>
        /// A separator that cannot turn up inside a category, property or value,
        /// so two different rules can never build the same signature.
        /// </summary>
        private const char FieldSeparator = '\u001F';

        /// <summary>
        /// What this condition actually asks of the model: the test, the category, the
        /// property and the value. Flags is left out on purpose, it only says how this
        /// condition joins to the next one.
        /// </summary>
        public string RuleSignature
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Test).Append(FieldSeparator);
                Append(builder, Category);
                Append(builder, Property);
                builder.Append(Value == null ? string.Empty : Value.DataType).Append(FieldSeparator);
                builder.Append(Value == null ? string.Empty : Value.Data);
                return builder.ToString();
            }
        }

        private static void Append(StringBuilder builder, NameReference reference)
        {
            builder.Append(reference == null ? string.Empty : reference.InternalName).Append(FieldSeparator);
            builder.Append(reference == null ? string.Empty : reference.DisplayName).Append(FieldSeparator);
        }

        public override string ToString()
        {
            return (Category == null ? string.Empty : Category.DisplayName + ".")
                + (Property == null ? string.Empty : Property.DisplayName)
                + " " + Test + " "
                + (Value == null ? string.Empty : Value.Data);
        }
    }

    /// <summary>One selectionset, with the folder path that leads to it.</summary>
    public sealed class SelectionSetDefinition
    {
        internal SelectionSetDefinition(
            string name,
            string guid,
            IList<string> folders,
            string path,
            string findSpecMode,
            bool disjoint,
            string findSpecLocator,
            IList<SearchConditionDefinition> conditions)
        {
            Name = name;
            Guid = guid;
            Folders = new ReadOnlyCollection<string>(folders ?? new List<string>());
            Path = path;
            FindSpecMode = findSpecMode;
            Disjoint = disjoint;
            FindSpecLocator = findSpecLocator;
            Conditions = new ReadOnlyCollection<SearchConditionDefinition>(
                conditions ?? new List<SearchConditionDefinition>());
        }

        public string Name { get; private set; }

        public string Guid { get; private set; }

        /// <summary>The folders above this set, outermost first. Empty for a set at the root.</summary>
        public ReadOnlyCollection<string> Folders { get; private set; }

        /// <summary>
        /// The full path, built as lcop_selection_set_tree/folder/folder/name so it can be
        /// compared to a test locator directly.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>The findspec mode attribute, for example all.</summary>
        public string FindSpecMode { get; private set; }

        public bool Disjoint { get; private set; }

        /// <summary>The locator inside the findspec, which says where the search starts.</summary>
        public string FindSpecLocator { get; private set; }

        public ReadOnlyCollection<SearchConditionDefinition> Conditions { get; private set; }

        public bool IsAtRoot
        {
            get { return Folders.Count == 0; }
        }

        /// <summary>
        /// The name with a Navisworks copy suffix removed, so INF-FS-MH_Manholes (2)
        /// reads as INF-FS-MH_Manholes.
        /// </summary>
        public string BaseName
        {
            get { return StripCopySuffix(Name); }
        }

        internal static string StripCopySuffix(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            string trimmed = name.TrimEnd();

            if (trimmed.Length < 4 || trimmed[trimmed.Length - 1] != ')')
            {
                return name;
            }

            int open = trimmed.LastIndexOf('(');

            if (open <= 0)
            {
                return name;
            }

            for (int i = open + 1; i < trimmed.Length - 1; i++)
            {
                if (!char.IsDigit(trimmed[i]))
                {
                    return name;
                }
            }

            if (open + 1 == trimmed.Length - 1)
            {
                return name;
            }

            return trimmed.Substring(0, open).TrimEnd();
        }

        public override string ToString()
        {
            return Path;
        }
    }

    /// <summary>A batchtest element, kept so the tests it held can be reported by batch.</summary>
    public sealed class BatchTestDefinition
    {
        internal BatchTestDefinition(string name, string internalName, string units, int testCount)
        {
            Name = name;
            InternalName = internalName;
            Units = units;
            TestCount = testCount;
        }

        public string Name { get; private set; }

        public string InternalName { get; private set; }

        public string Units { get; private set; }

        public int TestCount { get; private set; }

        public override string ToString()
        {
            return Name + " (" + TestCount + " tests)";
        }
    }

    /// <summary>
    /// One exchange file read into memory. A file can hold tests only, sets only, or
    /// both, so either list can be empty.
    /// </summary>
    public sealed class ExchangeDocument
    {
        internal ExchangeDocument(
            string sourcePath,
            string units,
            IList<BatchTestDefinition> batchTests,
            IList<ClashTestDefinition> tests,
            IList<SelectionSetDefinition> sets)
        {
            SourcePath = sourcePath;
            Units = units;
            BatchTests = new ReadOnlyCollection<BatchTestDefinition>(batchTests);
            Tests = new ReadOnlyCollection<ClashTestDefinition>(tests);
            Sets = new ReadOnlyCollection<SelectionSetDefinition>(sets);
        }

        /// <summary>Null when the document was read from a string or a stream.</summary>
        public string SourcePath { get; private set; }

        /// <summary>The units attribute on the exchange element.</summary>
        public string Units { get; private set; }

        public ReadOnlyCollection<BatchTestDefinition> BatchTests { get; private set; }

        public ReadOnlyCollection<ClashTestDefinition> Tests { get; private set; }

        public ReadOnlyCollection<SelectionSetDefinition> Sets { get; private set; }

        public bool HasTests
        {
            get { return Tests.Count > 0; }
        }

        public bool HasSets
        {
            get { return Sets.Count > 0; }
        }

        /// <summary>Every distinct locator named by a test side, in first seen order.</summary>
        public IList<string> DistinctTestLocators()
        {
            List<string> locators = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (ClashTestDefinition test in Tests)
            {
                AddLocator(test.Left, locators, seen);
                AddLocator(test.Right, locators, seen);
            }

            return locators;
        }

        private static void AddLocator(ClashSideDefinition side, IList<string> locators, HashSet<string> seen)
        {
            if (side != null && side.HasLocator && seen.Add(side.Locator))
            {
                locators.Add(side.Locator);
            }
        }
    }
}
