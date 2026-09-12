using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The Building export holds both shapes in one file: 61 sets and 1830 tests.
    /// Every set carries the same rule, so the export is unusable.
    /// </summary>
    [TestFixture]
    public class BuildingSetsFileTests
    {
        private ExchangeDocument document;

        [OneTimeSetUp]
        public void ReadTheFile()
        {
            document = new ExchangeReader().ReadFile(Samples.Building());
        }

        [Test]
        public void HoldsSixtyOneSetsAndEighteenThirtyTestsInOneFile()
        {
            Assert.That(document.Sets.Count, Is.EqualTo(61));
            Assert.That(document.Tests.Count, Is.EqualTo(1830));
            Assert.That(document.HasSets, Is.True);
            Assert.That(document.HasTests, Is.True);
        }

        [Test]
        public void AllSixtyOneTestLocatorsResolveAgainstItsOwnSets()
        {
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.TotalLocatorCount, Is.EqualTo(61));
            Assert.That(health.ResolvedLocatorCount, Is.EqualTo(61));
            Assert.That(health.UnresolvedLocatorCount, Is.EqualTo(0));
            Assert.That(health.TestsWithUnresolvedSide.Count, Is.EqualTo(0));
        }

        [Test]
        public void EveryToleranceIsFiftyMillimetresAndSelfIntersectIsOn()
        {
            foreach (ClashTestDefinition test in document.Tests)
            {
                Assert.That(test.ToleranceInFileUnits, Is.EqualTo(0.1640419948).Within(1e-10), test.Name);
                Assert.That(test.FileUnits, Is.EqualTo("ft"), test.Name);
                Assert.That(ExchangeUnits.Convert(test.ToleranceInFileUnits, test.FileUnits, "mm"),
                    Is.EqualTo(50.0).Within(1e-6), test.Name);
                Assert.That(test.Left.SelfIntersect, Is.True, test.Name);
                Assert.That(test.Right.SelfIntersect, Is.True, test.Name);
            }
        }

        [Test]
        public void EverySetCarriesExactlyOneCondition()
        {
            foreach (SelectionSetDefinition set in document.Sets)
            {
                Assert.That(set.Conditions.Count, Is.EqualTo(1), set.Name);
            }
        }

        [Test]
        public void AllSixtyOneConditionsAreIdenticalSoTheExportIsUnusable()
        {
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.ConditionCount, Is.EqualTo(61));
            Assert.That(health.DistinctRuleCount, Is.EqualTo(1));
            Assert.That(health.AllSetsShareOneRule, Is.True);
            Assert.That(health.ExportUnusable, Is.True);
        }

        [Test]
        public void TheOneRuleAsksForCategoryNameEqualsFloors()
        {
            SearchConditionDefinition condition = document.Sets[0].Conditions[0];

            Assert.That(condition.Test, Is.EqualTo("equals"));
            Assert.That(condition.Category.InternalName, Is.EqualTo("Category"));
            Assert.That(condition.Category.DisplayName, Is.EqualTo("Category"));
            Assert.That(condition.Property.InternalName, Is.EqualTo("Name"));
            Assert.That(condition.Property.DisplayName, Is.EqualTo("Name"));
            Assert.That(condition.Value.DataType, Is.EqualTo("wstring"));
            Assert.That(condition.Value.Data, Is.EqualTo("Floors"));
        }

        // Two of the 61 set names end in a space, and the locators that point at them
        // carry that space too. Trimming either side breaks a match that is correct in
        // the file, so both are read exactly as written.
        [Test]
        public void TwoSetNamesEndInASpaceAndAreStillMatchedExactly()
        {
            List<string> trailing = new List<string>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                if (set.Name != set.Name.TrimEnd())
                {
                    trailing.Add(set.Name);
                }
            }

            Assert.That(trailing.Count, Is.EqualTo(2));
            Assert.That(trailing, Does.Contain("BLD-ME-FD_Flex Ducts "));
            Assert.That(trailing, Does.Contain("BLD-SD_Security Devices "));

            List<string> locators = new List<string>(document.DistinctTestLocators());

            Assert.That(
                locators,
                Does.Contain("lcop_selection_set_tree/Mechanical/Mechanical-HVAC/BLD-ME-FD_Flex Ducts "));
            Assert.That(
                locators,
                Does.Contain("lcop_selection_set_tree/Electrical/BLD-SD_Security Devices "));
        }

        [Test]
        public void ItsFoldersNestTwoDeep()
        {
            int deepest = 0;

            foreach (SelectionSetDefinition set in document.Sets)
            {
                if (set.Folders.Count > deepest)
                {
                    deepest = set.Folders.Count;
                }
            }

            Assert.That(deepest, Is.EqualTo(2));
        }
    }
}
