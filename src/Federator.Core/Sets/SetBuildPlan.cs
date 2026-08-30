using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Sets
{
    /// <summary>
    /// The condition tests this tool knows how to rebuild. Anything else is reported by
    /// name and its set is skipped, never approximated into one of these.
    /// </summary>
    public enum ConditionTest
    {
        /// <summary>test="equals" in the file.</summary>
        Equals,

        /// <summary>test="contains" in the file.</summary>
        Contains
    }

    /// <summary>
    /// One condition, ready to hand to the API. The internal names are what the API
    /// matches on. The display words are carried for reporting and nothing else.
    /// </summary>
    public sealed class PlannedCondition
    {
        internal PlannedCondition(
            ConditionTest test,
            int flags,
            string categoryInternalName,
            string categoryDisplayName,
            string propertyInternalName,
            string propertyDisplayName,
            string valueType,
            string value)
        {
            Test = test;
            Flags = flags;
            CategoryInternalName = categoryInternalName;
            CategoryDisplayName = categoryDisplayName;
            PropertyInternalName = propertyInternalName;
            PropertyDisplayName = propertyDisplayName;
            ValueType = valueType;
            Value = value;
        }

        public ConditionTest Test { get; private set; }

        /// <summary>
        /// The flags attribute exactly as the file carried it. Read off the installed DLL
        /// on 2026-08-31, Autodesk.Navisworks.Api.SearchConditionOptions is a Flags enum
        /// over int whose bits are the same numbers, so 64 is StartGroup. See docs\scan.md.
        /// This is kept as an int here because Core never references the Navisworks API.
        /// </summary>
        public int Flags { get; private set; }

        /// <summary>Null when the condition carried no category element.</summary>
        public string CategoryInternalName { get; private set; }

        /// <summary>Null when the condition carried no category element.</summary>
        public string CategoryDisplayName { get; private set; }

        public bool HasCategory
        {
            get { return CategoryInternalName != null; }
        }

        public string PropertyInternalName { get; private set; }

        public string PropertyDisplayName { get; private set; }

        /// <summary>The data type attribute, for example wstring.</summary>
        public string ValueType { get; private set; }

        public string Value { get; private set; }

        /// <summary>
        /// What this condition asks the model for, in internal names, so a set that finds
        /// nothing explains itself without anyone opening the XML again.
        /// </summary>
        public string Describe()
        {
            return (HasCategory ? CategoryInternalName + "/" : string.Empty)
                + PropertyInternalName
                + (Test == ConditionTest.Contains ? " contains " : " equals ")
                + "\"" + Value + "\"";
        }

        public override string ToString()
        {
            return Describe();
        }
    }

    /// <summary>A set that can be rebuilt, with the folders that have to exist above it.</summary>
    public sealed class PlannedSet
    {
        internal PlannedSet(string name, string path, IList<string> folders, IList<PlannedCondition> conditions)
        {
            Name = name;
            Path = path;
            Folders = new ReadOnlyCollection<string>(folders);
            Conditions = new ReadOnlyCollection<PlannedCondition>(conditions);
        }

        public string Name { get; private set; }

        /// <summary>The full path as the file wrote it, used for reporting.</summary>
        public string Path { get; private set; }

        /// <summary>The folders above this set, outermost first. Empty for a set at the root.</summary>
        public ReadOnlyCollection<string> Folders { get; private set; }

        public ReadOnlyCollection<PlannedCondition> Conditions { get; private set; }

        public int ConditionCount
        {
            get { return Conditions.Count; }
        }

        /// <summary>Every condition joined, which is the whole question the set asks.</summary>
        public string Describe()
        {
            List<string> parts = new List<string>();

            foreach (PlannedCondition condition in Conditions)
            {
                parts.Add(condition.Describe());
            }

            return parts.Count == 0 ? "nothing" : string.Join(" and ", parts.ToArray());
        }

        public override string ToString()
        {
            return Path;
        }
    }

    /// <summary>A set that will not be rebuilt, and why. Always reported, never silent.</summary>
    public sealed class SkippedSet
    {
        internal SkippedSet(string name, string path, string reason)
        {
            Name = name;
            Path = path;
            Reason = reason;
        }

        public string Name { get; private set; }

        public string Path { get; private set; }

        public string Reason { get; private set; }

        public override string ToString()
        {
            return Path + ": " + Reason;
        }
    }
}
