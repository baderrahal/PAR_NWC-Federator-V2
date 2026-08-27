using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Numbers measured from the real clash export. A change in any of them means
    /// something broke.
    /// </summary>
    [TestFixture]
    public class AllInOneFileTests
    {
        private ExchangeDocument document;

        [OneTimeSetUp]
        public void ReadTheFile()
        {
            document = new ExchangeReader().ReadFile(Samples.AllInOne());
        }

        [Test]
        public void HoldsOneThousandEightHundredAndThirtyTests()
        {
            Assert.That(document.Tests.Count, Is.EqualTo(1830));
        }

        [Test]
        public void EveryTestNameIsUnique()
        {
            HashSet<string> names = new HashSet<string>();

            foreach (ClashTestDefinition test in document.Tests)
            {
                Assert.That(names.Add(test.Name), Is.True, "Repeated test name " + test.Name);
            }

            Assert.That(names.Count, Is.EqualTo(1830));
        }

        [Test]
        public void HoldsSixtyOneUniqueLocators()
        {
            Assert.That(document.DistinctTestLocators().Count, Is.EqualTo(61));
        }

        [Test]
        public void HoldsNoSelfPairs()
        {
            int selfPairs = 0;

            foreach (ClashTestDefinition test in document.Tests)
            {
                if (test.Left.Locator == test.Right.Locator)
                {
                    selfPairs++;
                }
            }

            Assert.That(selfPairs, Is.EqualTo(0));
        }

        [Test]
        public void TheTestsAreEveryPairOfSixtyOneSets()
        {
            HashSet<string> pairs = new HashSet<string>();

            foreach (ClashTestDefinition test in document.Tests)
            {
                string a = test.Left.Locator;
                string b = test.Right.Locator;
                pairs.Add(string.CompareOrdinal(a, b) <= 0 ? a + "|" + b : b + "|" + a);
            }

            Assert.That(pairs.Count, Is.EqualTo(1830));
            Assert.That(pairs.Count, Is.EqualTo(61 * 60 / 2));
        }

        [Test]
        public void EveryTestIsHardConservativeAndNewAndMergesComposites()
        {
            foreach (ClashTestDefinition test in document.Tests)
            {
                Assert.That(test.TestType, Is.EqualTo("hard_conservative"), test.Name);
                Assert.That(test.Status, Is.EqualTo("new"), test.Name);
                Assert.That(test.MergeComposites, Is.True, test.Name);
            }
        }

        [Test]
        public void TheFileUnitsAreFeet()
        {
            Assert.That(document.Units, Is.EqualTo("ft"));
        }

        [Test]
        public void EveryToleranceIsSeventyFiveMillimetres()
        {
            foreach (ClashTestDefinition test in document.Tests)
            {
                Assert.That(test.ToleranceInFileUnits, Is.EqualTo(0.2460629921).Within(1e-10), test.Name);
                Assert.That(test.FileUnits, Is.EqualTo("ft"), test.Name);
                Assert.That(test.ToleranceMillimetres, Is.EqualTo(75.0).Within(1e-6), test.Name);
            }
        }

        [Test]
        public void LinkageAndRulesAreReadEvenThoughTheyAreEmptyHere()
        {
            foreach (ClashTestDefinition test in document.Tests)
            {
                Assert.That(test.LinkageMode, Is.EqualTo("none"), test.Name);
                Assert.That(test.Rules.Count, Is.EqualTo(0), test.Name);
            }
        }

        [Test]
        public void EverySideCarriesItsFlagsAndOneLocator()
        {
            foreach (ClashTestDefinition test in document.Tests)
            {
                Assert.That(test.Left.SelfIntersect, Is.False, test.Name);
                Assert.That(test.Right.SelfIntersect, Is.False, test.Name);
                Assert.That(test.Left.PrimitiveTypes, Is.EqualTo(1), test.Name);
                Assert.That(test.Right.PrimitiveTypes, Is.EqualTo(1), test.Name);
                Assert.That(test.Left.HasLocator, Is.True, test.Name);
                Assert.That(test.Right.HasLocator, Is.True, test.Name);
            }
        }

        [Test]
        public void OneBatchTestHoldsEveryTest()
        {
            Assert.That(document.BatchTests.Count, Is.EqualTo(1));
            Assert.That(document.BatchTests[0].Name, Is.EqualTo("Clash Test Building"));
            Assert.That(document.BatchTests[0].TestCount, Is.EqualTo(1830));
        }

        // The brief said this file holds no sets. It holds 61. Measured this session and
        // recorded in docs\scan.md.
        [Test]
        public void TheFileAlsoHoldsSixtyOneSets()
        {
            Assert.That(document.Sets.Count, Is.EqualTo(61));
        }

        [Test]
        public void TheSetsTreeNestsSoTheFoldersHaveToBeWalked()
        {
            int deepest = 0;

            foreach (SelectionSetDefinition set in document.Sets)
            {
                if (set.Folders.Count > deepest)
                {
                    deepest = set.Folders.Count;
                }
            }

            Assert.That(deepest, Is.EqualTo(2), "Sets nest two folders deep in this file.");
        }

        [Test]
        public void MechanicalHoldsFourSubfolders()
        {
            HashSet<string> subfolders = new HashSet<string>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                if (set.Folders.Count == 2 && set.Folders[0] == "Mechanical")
                {
                    subfolders.Add(set.Folders[1]);
                }
            }

            Assert.That(subfolders.Count, Is.EqualTo(4));
        }

        [Test]
        public void SetPathsAreBuiltSoATestLocatorCanBeComparedDirectly()
        {
            SelectionSetDefinition airTerminals = null;

            foreach (SelectionSetDefinition set in document.Sets)
            {
                if (set.Name == "BLD-ME-Air Terminals")
                {
                    airTerminals = set;
                }
            }

            Assert.That(airTerminals, Is.Not.Null);
            Assert.That(
                airTerminals.Path,
                Is.EqualTo("lcop_selection_set_tree/Mechanical/Mechanical-HVAC/BLD-ME-Air Terminals"));
        }

        [Test]
        public void SetNamesHoldSpacesAndAmpersandsAndAreMatchedExactly()
        {
            List<string> paths = new List<string>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                paths.Add(set.Path);
            }

            Assert.That(paths, Does.Contain("lcop_selection_set_tree/Mechanical/Mechanical-HVAC/BLD-ME-Ducts&Duct Fittings"));
            Assert.That(paths, Does.Contain("lcop_selection_set_tree/Mechanical/Mechanical-Fire Fighting/BLD-FF-Pipes & Pipe Fittings"));
        }

        [Test]
        public void TheSetsCarryOneHundredAndTwoConditions()
        {
            int conditions = 0;

            foreach (SelectionSetDefinition set in document.Sets)
            {
                conditions += set.Conditions.Count;
            }

            Assert.That(conditions, Is.EqualTo(102));
        }

        [Test]
        public void SomeConditionsCarryNoCategory()
        {
            int withoutCategory = 0;

            foreach (SelectionSetDefinition set in document.Sets)
            {
                foreach (SearchConditionDefinition condition in set.Conditions)
                {
                    if (condition.Category == null)
                    {
                        withoutCategory++;
                    }

                    Assert.That(condition.Property, Is.Not.Null, set.Name);
                    Assert.That(condition.Value, Is.Not.Null, set.Name);
                }
            }

            Assert.That(withoutCategory, Is.EqualTo(6));
        }

        [Test]
        public void EveryFindSpecIsReadAllAndNotDisjointAndStartsAtTheRoot()
        {
            foreach (SelectionSetDefinition set in document.Sets)
            {
                Assert.That(set.FindSpecMode, Is.EqualTo("all"), set.Name);
                Assert.That(set.Disjoint, Is.False, set.Name);
                Assert.That(set.FindSpecLocator, Is.EqualTo("/"), set.Name);
            }
        }

        [Test]
        public void EveryTestLocatorResolvesAgainstThisFileOwnSets()
        {
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.ResolvedLocatorCount, Is.EqualTo(61));
            Assert.That(health.UnresolvedLocatorCount, Is.EqualTo(0));
            Assert.That(health.TestsWithUnresolvedSide.Count, Is.EqualTo(0));
        }

        [Test]
        public void TheSetsAskFiftyThreeDifferentThingsSoTheExportIsUsable()
        {
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.ConditionCount, Is.EqualTo(102));
            Assert.That(health.DistinctRuleCount, Is.EqualTo(53));
            Assert.That(health.AllSetsShareOneRule, Is.False);
            Assert.That(health.ExportUnusable, Is.False);
        }

        [Test]
        public void NoSetSitsAtTheRootAndNoNameIsRepeated()
        {
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.SetsAtRoot.Count, Is.EqualTo(0));
            Assert.That(health.DuplicateNames.Count, Is.EqualTo(0));
        }
    }
}
