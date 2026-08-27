using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Exchange;

namespace Federator.Core.Health
{
    /// <summary>A locator named by a test that matches no set, and the tests that named it.</summary>
    public sealed class UnresolvedLocator
    {
        internal UnresolvedLocator(string locator, IList<string> testNames)
        {
            Locator = locator;
            TestNames = new ReadOnlyCollection<string>(testNames);
        }

        public string Locator { get; private set; }

        /// <summary>The tests that point at this locator, so they can be reported by name.</summary>
        public ReadOnlyCollection<string> TestNames { get; private set; }

        public override string ToString()
        {
            return Locator + " (" + TestNames.Count + " tests)";
        }
    }

    /// <summary>Sets whose names collide once a Navisworks copy suffix is stripped.</summary>
    public sealed class DuplicateSetName
    {
        internal DuplicateSetName(string baseName, IList<SelectionSetDefinition> sets, bool isExact)
        {
            BaseName = baseName;
            Sets = new ReadOnlyCollection<SelectionSetDefinition>(sets);
            IsExact = isExact;
        }

        public string BaseName { get; private set; }

        public ReadOnlyCollection<SelectionSetDefinition> Sets { get; private set; }

        /// <summary>True when the names match exactly, false when they differ only by a copy suffix.</summary>
        public bool IsExact { get; private set; }

        public int Count
        {
            get { return Sets.Count; }
        }

        public override string ToString()
        {
            return BaseName + " x" + Count + (IsExact ? " (exact)" : " (copy suffixes)");
        }
    }

    /// <summary>
    /// What the tests and the sets say about each other. Pure data, no model needed.
    /// </summary>
    public sealed class HealthCheckResult
    {
        internal HealthCheckResult(
            int testCount,
            int setCount,
            IList<string> resolvedLocators,
            IList<UnresolvedLocator> unresolvedLocators,
            IList<string> testsWithUnresolvedSide,
            int conditionCount,
            int distinctRuleCount,
            bool allSetsShareOneRule,
            IList<DuplicateSetName> duplicateNames,
            IList<SelectionSetDefinition> setsAtRoot)
        {
            TestCount = testCount;
            SetCount = setCount;
            ResolvedLocators = new ReadOnlyCollection<string>(resolvedLocators);
            UnresolvedLocators = new ReadOnlyCollection<UnresolvedLocator>(unresolvedLocators);
            TestsWithUnresolvedSide = new ReadOnlyCollection<string>(testsWithUnresolvedSide);
            ConditionCount = conditionCount;
            DistinctRuleCount = distinctRuleCount;
            AllSetsShareOneRule = allSetsShareOneRule;
            DuplicateNames = new ReadOnlyCollection<DuplicateSetName>(duplicateNames);
            SetsAtRoot = new ReadOnlyCollection<SelectionSetDefinition>(setsAtRoot);
        }

        public int TestCount { get; private set; }

        public int SetCount { get; private set; }

        /// <summary>Distinct test locators that matched a set path.</summary>
        public ReadOnlyCollection<string> ResolvedLocators { get; private set; }

        public ReadOnlyCollection<UnresolvedLocator> UnresolvedLocators { get; private set; }

        /// <summary>
        /// Tests with at least one side that does not resolve. These are reported by name
        /// and skipped, never imported with an empty side.
        /// </summary>
        public ReadOnlyCollection<string> TestsWithUnresolvedSide { get; private set; }

        public int ConditionCount { get; private set; }

        /// <summary>How many different rules the sets ask for, ignoring the flags that join them.</summary>
        public int DistinctRuleCount { get; private set; }

        /// <summary>
        /// True when more than one set exists and every one of them carries the same rule.
        /// That means the export came out of Navisworks wrong and cannot be used.
        /// </summary>
        public bool AllSetsShareOneRule { get; private set; }

        public ReadOnlyCollection<DuplicateSetName> DuplicateNames { get; private set; }

        /// <summary>Sets sitting at the root of the tree with no folder above them.</summary>
        public ReadOnlyCollection<SelectionSetDefinition> SetsAtRoot { get; private set; }

        public int ResolvedLocatorCount
        {
            get { return ResolvedLocators.Count; }
        }

        public int UnresolvedLocatorCount
        {
            get { return UnresolvedLocators.Count; }
        }

        public int TotalLocatorCount
        {
            get { return ResolvedLocatorCount + UnresolvedLocatorCount; }
        }

        /// <summary>An export nothing can be run against.</summary>
        public bool ExportUnusable
        {
            get { return AllSetsShareOneRule; }
        }

        public IList<string> Summary()
        {
            List<string> lines = new List<string>
            {
                "Tests: " + TestCount,
                "Sets: " + SetCount,
                "Locators resolved: " + ResolvedLocatorCount + " of " + TotalLocatorCount,
                "Tests with an unresolved side: " + TestsWithUnresolvedSide.Count,
                "Conditions: " + ConditionCount + " across " + DistinctRuleCount + " distinct rules",
                "Sets at the root with no folder: " + SetsAtRoot.Count,
                "Duplicate set names: " + DuplicateNames.Count
            };

            if (AllSetsShareOneRule)
            {
                lines.Add("UNUSABLE: every set carries the same rule.");
            }

            return lines;
        }
    }
}
