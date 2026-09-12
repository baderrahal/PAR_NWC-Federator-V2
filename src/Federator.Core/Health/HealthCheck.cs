using System;
using System.Collections.Generic;
using Federator.Core.Exchange;

namespace Federator.Core.Health
{
    /// <summary>
    /// Checks the tests against the sets before anything touches a model. Pure data in,
    /// pure data out.
    /// </summary>
    public static class HealthCheck
    {
        public static HealthCheckResult Run(ExchangeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            return Run(document.Tests, document.Sets);
        }

        /// <summary>
        /// Checks tests from one file against sets from another, which is how a tests
        /// export and a sets export get paired up.
        /// </summary>
        internal static HealthCheckResult Run(ExchangeDocument tests, ExchangeDocument sets)
        {
            if (tests == null)
            {
                throw new ArgumentNullException("tests");
            }

            if (sets == null)
            {
                throw new ArgumentNullException("sets");
            }

            return Run(tests.Tests, sets.Sets);
        }

        public static HealthCheckResult Run(
            IEnumerable<ClashTestDefinition> tests, IEnumerable<SelectionSetDefinition> sets)
        {
            if (tests == null)
            {
                throw new ArgumentNullException("tests");
            }

            if (sets == null)
            {
                throw new ArgumentNullException("sets");
            }

            List<SelectionSetDefinition> setList = new List<SelectionSetDefinition>(sets);
            List<ClashTestDefinition> testList = new List<ClashTestDefinition>(tests);

            HashSet<string> setPaths = new HashSet<string>(StringComparer.Ordinal);

            foreach (SelectionSetDefinition set in setList)
            {
                setPaths.Add(set.Path);
            }

            List<string> resolved = new List<string>();
            List<UnresolvedLocator> unresolved = new List<UnresolvedLocator>();
            List<string> testsWithUnresolvedSide = new List<string>();

            HashSet<string> seenLocators = new HashSet<string>(StringComparer.Ordinal);
            List<string> locatorOrder = new List<string>();
            Dictionary<string, List<string>> testsByLocator =
                new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (ClashTestDefinition test in testList)
            {
                bool leftOk = Note(test, test.Left, seenLocators, locatorOrder, testsByLocator);
                bool rightOk = Note(test, test.Right, seenLocators, locatorOrder, testsByLocator);

                if (!leftOk || !rightOk)
                {
                    testsWithUnresolvedSide.Add(test.Name);
                }
                else if (!setPaths.Contains(test.Left.Locator) || !setPaths.Contains(test.Right.Locator))
                {
                    testsWithUnresolvedSide.Add(test.Name);
                }
            }

            foreach (string locator in locatorOrder)
            {
                if (setPaths.Contains(locator))
                {
                    resolved.Add(locator);
                }
                else
                {
                    unresolved.Add(new UnresolvedLocator(locator, testsByLocator[locator]));
                }
            }

            int conditionCount;
            int distinctRuleCount;
            CountRules(setList, out conditionCount, out distinctRuleCount);

            bool allShareOneRule = setList.Count > 1 && conditionCount > 0 && distinctRuleCount == 1;

            return new HealthCheckResult(
                testList.Count,
                setList.Count,
                resolved,
                unresolved,
                testsWithUnresolvedSide,
                conditionCount,
                distinctRuleCount,
                allShareOneRule,
                FindDuplicateNames(setList),
                FindSetsAtRoot(setList));
        }

        /// <summary>
        /// Records the locator a side names. Returns false when the side is missing or
        /// carries no locator at all, which is a broken test rather than a missing set.
        /// </summary>
        private static bool Note(
            ClashTestDefinition test,
            ClashSideDefinition side,
            HashSet<string> seen,
            IList<string> order,
            IDictionary<string, List<string>> testsByLocator)
        {
            if (side == null || !side.HasLocator)
            {
                return false;
            }

            if (seen.Add(side.Locator))
            {
                order.Add(side.Locator);
                testsByLocator.Add(side.Locator, new List<string>());
            }

            List<string> names = testsByLocator[side.Locator];

            if (names.Count == 0 || !string.Equals(names[names.Count - 1], test.Name, StringComparison.Ordinal))
            {
                names.Add(test.Name);
            }

            return true;
        }

        private static void CountRules(
            IEnumerable<SelectionSetDefinition> sets, out int conditionCount, out int distinctRuleCount)
        {
            conditionCount = 0;
            HashSet<string> signatures = new HashSet<string>(StringComparer.Ordinal);

            foreach (SelectionSetDefinition set in sets)
            {
                foreach (SearchConditionDefinition condition in set.Conditions)
                {
                    conditionCount++;
                    signatures.Add(condition.RuleSignature);
                }
            }

            distinctRuleCount = signatures.Count;
        }

        private static IList<DuplicateSetName> FindDuplicateNames(IEnumerable<SelectionSetDefinition> sets)
        {
            List<string> order = new List<string>();
            Dictionary<string, List<SelectionSetDefinition>> byBaseName =
                new Dictionary<string, List<SelectionSetDefinition>>(StringComparer.Ordinal);

            foreach (SelectionSetDefinition set in sets)
            {
                string baseName = set.BaseName;
                List<SelectionSetDefinition> bucket;

                if (!byBaseName.TryGetValue(baseName, out bucket))
                {
                    bucket = new List<SelectionSetDefinition>();
                    byBaseName.Add(baseName, bucket);
                    order.Add(baseName);
                }

                bucket.Add(set);
            }

            List<DuplicateSetName> duplicates = new List<DuplicateSetName>();

            foreach (string baseName in order)
            {
                List<SelectionSetDefinition> bucket = byBaseName[baseName];

                if (bucket.Count < 2)
                {
                    continue;
                }

                duplicates.Add(new DuplicateSetName(baseName, bucket, AllNamesMatch(bucket)));
            }

            return duplicates;
        }

        private static bool AllNamesMatch(IList<SelectionSetDefinition> sets)
        {
            for (int i = 1; i < sets.Count; i++)
            {
                if (!string.Equals(sets[0].Name, sets[i].Name, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static IList<SelectionSetDefinition> FindSetsAtRoot(IEnumerable<SelectionSetDefinition> sets)
        {
            List<SelectionSetDefinition> atRoot = new List<SelectionSetDefinition>();

            foreach (SelectionSetDefinition set in sets)
            {
                if (set.IsAtRoot)
                {
                    atRoot.Add(set);
                }
            }

            return atRoot;
        }
    }
}
