using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Exchange;

namespace Federator.Core.Sets
{
    /// <summary>
    /// Turns the sets an ExchangeReader produced into a plan the builder can follow.
    /// Nothing about any one project is written in here. The names, the folder names, the
    /// count and the internal property names all come from whichever file was picked at
    /// run time. An internal name never seen before is passed through as it is.
    /// </summary>
    public sealed class SetBuildPlan
    {
        /// <summary>The condition test values in the files seen so far, not a permitted list.</summary>
        public const string EqualsTest = "equals";

        public const string ContainsTest = "contains";

        private SetBuildPlan(IList<PlannedSet> buildable, IList<SkippedSet> skipped, IList<string> unknownTests)
        {
            Buildable = new ReadOnlyCollection<PlannedSet>(buildable);
            Skipped = new ReadOnlyCollection<SkippedSet>(skipped);
            UnknownTestValues = new ReadOnlyCollection<string>(unknownTests);
        }

        public ReadOnlyCollection<PlannedSet> Buildable { get; private set; }

        /// <summary>Sets that will not be rebuilt, each carrying the reason.</summary>
        public ReadOnlyCollection<SkippedSet> Skipped { get; private set; }

        /// <summary>Every distinct test value that stopped a set, in the order first seen.</summary>
        public ReadOnlyCollection<string> UnknownTestValues { get; private set; }

        internal int TotalSets
        {
            get { return Buildable.Count + Skipped.Count; }
        }

        public bool HasWork
        {
            get { return Buildable.Count > 0; }
        }

        /// <summary>
        /// Every folder that has to exist, outermost first, parents always before their
        /// children, with no repeats. A folder tree of any depth comes back in an order
        /// that can be created straight through.
        /// </summary>
        internal IList<IList<string>> FolderPaths()
        {
            List<IList<string>> paths = new List<IList<string>>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (PlannedSet set in Buildable)
            {
                for (int depth = 1; depth <= set.Folders.Count; depth++)
                {
                    List<string> prefix = new List<string>();

                    for (int i = 0; i < depth; i++)
                    {
                        prefix.Add(set.Folders[i]);
                    }

                    string key = string.Join("/", prefix.ToArray());

                    if (seen.Add(key))
                    {
                        paths.Add(new ReadOnlyCollection<string>(prefix));
                    }
                }
            }

            return paths;
        }

        /// <summary>The deepest folder nesting in the plan. Zero when every set is at the root.</summary>
        internal int DeepestFolderDepth()
        {
            int deepest = 0;

            foreach (PlannedSet set in Buildable)
            {
                if (set.Folders.Count > deepest)
                {
                    deepest = set.Folders.Count;
                }
            }

            return deepest;
        }

        public static SetBuildPlan From(ExchangeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            return From(document.Sets);
        }

        /// <summary>
        /// A file holding no sets is accepted and gives an empty plan. A project that
        /// keeps its sets in the model and supplies only tests is a normal case, not an
        /// error.
        /// </summary>
        public static SetBuildPlan From(IEnumerable<SelectionSetDefinition> sets)
        {
            if (sets == null)
            {
                throw new ArgumentNullException("sets");
            }

            List<PlannedSet> buildable = new List<PlannedSet>();
            List<SkippedSet> skipped = new List<SkippedSet>();
            List<string> unknownTests = new List<string>();
            HashSet<string> unknownSeen = new HashSet<string>(StringComparer.Ordinal);

            foreach (SelectionSetDefinition set in sets)
            {
                string offending;
                List<PlannedCondition> conditions = Plan(set, out offending);

                if (conditions == null)
                {
                    if (unknownSeen.Add(offending ?? string.Empty))
                    {
                        unknownTests.Add(offending);
                    }

                    skipped.Add(new SkippedSet(
                        set.Name,
                        set.Path,
                        "condition test \"" + offending
                            + "\" is not one this tool knows how to rebuild. Known values are \""
                            + EqualsTest + "\" and \"" + ContainsTest
                            + "\". The set is skipped rather than approximated."));
                    continue;
                }

                buildable.Add(new PlannedSet(
                    set.Name, set.Path, new List<string>(set.Folders), conditions));
            }

            return new SetBuildPlan(buildable, skipped, unknownTests);
        }

        /// <summary>
        /// Returns null and names the offending test when the set cannot be rebuilt. Only
        /// an unknown test value stops a set. An internal name never seen before is not a
        /// reason to stop, it is passed to the API as it is.
        /// </summary>
        private static List<PlannedCondition> Plan(SelectionSetDefinition set, out string offendingTest)
        {
            offendingTest = null;
            List<PlannedCondition> planned = new List<PlannedCondition>();

            foreach (SearchConditionDefinition condition in set.Conditions)
            {
                ConditionTest test;

                if (!TryReadTest(condition.Test, out test))
                {
                    offendingTest = condition.Test;
                    return null;
                }

                planned.Add(new PlannedCondition(
                    test,
                    condition.Flags,
                    condition.Category == null ? null : condition.Category.InternalName,
                    condition.Category == null ? null : condition.Category.DisplayName,
                    condition.Property == null ? null : condition.Property.InternalName,
                    condition.Property == null ? null : condition.Property.DisplayName,
                    condition.Value == null ? null : condition.Value.DataType,
                    condition.Value == null ? null : condition.Value.Data));
            }

            return planned;
        }

        public static bool TryReadTest(string test, out ConditionTest result)
        {
            if (string.Equals(test, EqualsTest, StringComparison.Ordinal))
            {
                result = ConditionTest.Equals;
                return true;
            }

            if (string.Equals(test, ContainsTest, StringComparison.Ordinal))
            {
                result = ConditionTest.Contains;
                return true;
            }

            result = ConditionTest.Equals;
            return false;
        }
    }
}
