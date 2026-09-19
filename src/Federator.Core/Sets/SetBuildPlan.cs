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
        /// over int whose bits are the same numbers, so 64 is StartGroup. See docs\history\scan.md.
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
        internal string ValueType { get; private set; }

        public string Value { get; private set; }

        /// <summary>The bit that says this condition starts a new group, F78. Measured.</summary>
        public const int StartGroupFlag = 64;

        /// <summary>
        /// Whether this condition STARTS a new group, F78. The first group of a set is
        /// implicit and carries no flag, so this is false on the first condition of every
        /// set and true on the first condition of every group after it.
        /// </summary>
        public bool StartsAGroup
        {
            get { return (Flags & StartGroupFlag) == StartGroupFlag; }
        }

        /// <summary>
        /// What this condition asks the model for, in internal names, so a set that finds
        /// nothing explains itself without anyone opening the XML again.
        ///
        /// THE FRIENDLY NAME RIDES BESIDE THE INTERNAL ONE AND NEVER IN PLACE OF IT, F78.
        /// lcldrevit_parameter_-1002053 tells a reader nothing and Workset tells them
        /// everything, and the internal name is what the API matches on, so both are
        /// printed. The friendly half is READ OFF THE FILE, which carries
        /// name internal="..." display="Workset" on every property, and there is no lookup
        /// table anywhere: a table would be a second copy of a mapping the file already
        /// supplies and it would go stale the moment a project used a different parameter.
        /// </summary>
        public string Describe()
        {
            return (HasCategory ? CategoryInternalName + "/" : string.Empty)
                + PropertyInternalName
                + Friendly()
                + (Test == ConditionTest.Contains ? " contains " : " equals ")
                + "\"" + Value + "\"";
        }

        /// <summary>
        /// The display name in brackets, or nothing. Nothing where the file gave none and
        /// nothing where it is the same word as the internal name, because a name printed
        /// twice is noise.
        /// </summary>
        private string Friendly()
        {
            if (string.IsNullOrEmpty(PropertyDisplayName)
                || string.Equals(PropertyDisplayName, PropertyInternalName, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            return " (" + PropertyDisplayName + ")";
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

        /// <summary>
        /// Every condition joined, which is the whole question the set asks, F78.
        ///
        /// IT USED TO SAY AND WHERE THE FILE SAYS OR. Every condition was joined with
        /// "and", so BLD-ME-Ducts&amp;Duct Fittings read as Category equals Ducts and
        /// Category equals Duct Fittings, which is a question no element can answer, and a
        /// set that found thousands of items was described as one that could find none.
        ///
        /// WHAT THE FILE ACTUALLY SAYS, measured on 2026-09-19 over all 102 conditions of
        /// the client's matrix. Five of them carry flags="64", which is StartGroup, and
        /// each of those five is the third condition of one of the five sets that carry
        /// four. A condition with that bit STARTS A NEW GROUP. The first group of a set is
        /// implicit and carries no flag. Inside a group the conditions are ANDed and the
        /// groups are ORed, so those four conditions read
        ///
        ///     (Category equals Ducts and Workset equals ME-DUCTWORK)
        ///      or (Category equals Duct Fittings and Workset equals ME-DUCTWORK)
        ///
        /// which is a question that has an answer.
        ///
        /// THE BRACKETS ONLY APPEAR WHERE THERE IS MORE THAN ONE GROUP. With one group
        /// there is nothing to bracket and the 56 ordinary sets read exactly as they did.
        /// </summary>
        public string Describe()
        {
            IList<IList<PlannedCondition>> groups = Groups();

            if (groups.Count == 0)
            {
                return "nothing";
            }

            List<string> said = new List<string>();

            foreach (IList<PlannedCondition> group in groups)
            {
                List<string> parts = new List<string>();

                foreach (PlannedCondition condition in group)
                {
                    parts.Add(condition.Describe());
                }

                string joined = string.Join(" and ", parts.ToArray());
                said.Add(groups.Count == 1 ? joined : "(" + joined + ")");
            }

            return string.Join(" or ", said.ToArray());
        }

        /// <summary>
        /// The conditions split into their groups, F78. One group per StartGroup, and the
        /// first group is the one before any of them.
        /// </summary>
        public IList<IList<PlannedCondition>> Groups()
        {
            List<IList<PlannedCondition>> groups = new List<IList<PlannedCondition>>();
            List<PlannedCondition> current = null;

            foreach (PlannedCondition condition in Conditions)
            {
                if (current == null || condition.StartsAGroup)
                {
                    current = new List<PlannedCondition>();
                    groups.Add(current);
                }

                current.Add(condition);
            }

            return groups;
        }

        /// <summary>How many groups this set asks about, which is how many ORs it holds plus one.</summary>
        public int GroupCount
        {
            get { return Groups().Count; }
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
