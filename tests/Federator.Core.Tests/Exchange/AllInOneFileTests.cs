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
                Assert.That(ExchangeUnits.Convert(test.ToleranceInFileUnits, test.FileUnits, "mm"),
                    Is.EqualTo(75.0).Within(1e-6), test.Name);
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

        // This is the reference file. It holds both parts, 61 sets and 1830 tests, and
        // its own tests resolve against its own sets.
        [Test]
        public void TheFileAlsoHoldsSixtyOneSets()
        {
            Assert.That(document.Sets.Count, Is.EqualTo(61));
            Assert.That(document.HasSets, Is.True);
            Assert.That(document.HasTests, Is.True);
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

        // A condition can arrive with no category element. The reader must not assume one
        // is there, and reading the file must not throw over it.
        [Test]
        public void ConditionsWithNoCategoryAreReadWithoutThrowing()
        {
            ExchangeDocument reread = null;

            Assert.DoesNotThrow(() => reread = new ExchangeReader().ReadFile(Samples.AllInOne()));

            int withoutCategory = 0;

            foreach (SelectionSetDefinition set in reread.Sets)
            {
                foreach (SearchConditionDefinition condition in set.Conditions)
                {
                    if (condition.Category == null)
                    {
                        withoutCategory++;

                        // The ones with no category are the Source File searches, and they
                        // still carry a property and a value.
                        Assert.That(condition.Property.InternalName, Is.EqualTo("LcOaNodeSourceFile"));
                        Assert.That(condition.Value, Is.Not.Null);
                        Assert.DoesNotThrow(() => { string ignored = condition.RuleSignature; });
                        Assert.DoesNotThrow(() => { string ignored = condition.ToString(); });
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

        // HOW A DISTINCT RULE IS COUNTED.
        //
        // A rule is one condition, compared on the tuple
        //     test, category internal, category display,
        //     property internal, property display,
        //     value type, value data
        // with the flags attribute left out, because flags says how a condition joins to
        // the next one and not what it asks the model for. That definition gives 53 for
        // this file, and it is what HealthCheckResult.DistinctRuleCount reports.
        //
        // This definition is the one that has to be used for the damaged export check.
        // Search_Set_Infra holds 2715 conditions and exactly 1 distinct rule under it,
        // which is what makes the file readable as broken.
        //
        // Counting whole sets instead, by each set's full ordered list of conditions,
        // gives 59 for this file. Both numbers are right, they answer different
        // questions. 59 is not usable for the damaged export check: on Infra it gives 6,
        // because those sets differ only in how many copies of the one rule they carry.
        [Test]
        public void AnIndividualConditionIsTheUnitOfARuleAndThereAreFiftyThreeOfThem()
        {
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.ConditionCount, Is.EqualTo(102));
            Assert.That(health.DistinctRuleCount, Is.EqualTo(53));
            Assert.That(health.AllSetsShareOneRule, Is.False);
            Assert.That(health.ExportUnusable, Is.False);
        }

        [Test]
        public void CountingWholeSetsInsteadGivesFiftyNineBecauseTwoPairsMatch()
        {
            Dictionary<string, List<string>> byRuleList = new Dictionary<string, List<string>>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                List<string> signatures = new List<string>();

                foreach (SearchConditionDefinition condition in set.Conditions)
                {
                    signatures.Add(condition.RuleSignature);
                }

                string key = string.Join("||", signatures.ToArray());

                if (!byRuleList.ContainsKey(key))
                {
                    byRuleList.Add(key, new List<string>());
                }

                byRuleList[key].Add(set.Name);
            }

            Assert.That(document.Sets.Count, Is.EqualTo(61));
            Assert.That(byRuleList.Count, Is.EqualTo(59));

            List<string> shared = new List<string>();

            foreach (KeyValuePair<string, List<string>> entry in byRuleList)
            {
                if (entry.Value.Count > 1)
                {
                    entry.Value.Sort(System.StringComparer.Ordinal);
                    shared.Add(string.Join(" and ", entry.Value.ToArray()));
                }
            }

            shared.Sort(System.StringComparer.Ordinal);

            Assert.That(shared.Count, Is.EqualTo(2));
            Assert.That(shared, Does.Contain("BLD-EL-Devices and BLD-EL-Electrical Fixtures"));
            Assert.That(shared, Does.Contain("BLD-EL-Telecom Fixtures and BLD-EL-Telephone Devices"));
        }

        [Test]
        public void FlagsAreNotPartOfARuleSoBothValuesAppearAcrossTheFile()
        {
            HashSet<int> flags = new HashSet<int>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                foreach (SearchConditionDefinition condition in set.Conditions)
                {
                    flags.Add(condition.Flags);
                }
            }

            Assert.That(flags, Is.EquivalentTo(new[] { 0, 64 }));
        }

        [Test]
        public void BothEqualsAndContainsSurviveARead()
        {
            HashSet<string> tests = new HashSet<string>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                foreach (SearchConditionDefinition condition in set.Conditions)
                {
                    tests.Add(condition.Test);
                }
            }

            Assert.That(tests, Is.EquivalentTo(new[] { "equals", "contains" }));
        }

        // Rebuilding a search through the API matches on the internal string, never on
        // the display word, so the reader has to keep both and must not swap one for the
        // other.
        [Test]
        public void InternalNamesAreKeptAlongsideTheDisplayWords()
        {
            HashSet<string> categories = new HashSet<string>();
            HashSet<string> properties = new HashSet<string>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                foreach (SearchConditionDefinition condition in set.Conditions)
                {
                    if (condition.Category != null)
                    {
                        categories.Add(condition.Category.InternalName + " -> " + condition.Category.DisplayName);
                    }

                    properties.Add(condition.Property.InternalName + " -> " + condition.Property.DisplayName);
                }
            }

            Assert.That(categories, Is.EquivalentTo(new[] { "LcRevitData_Element -> Element" }));

            Assert.That(properties, Is.EquivalentTo(new[]
            {
                "LcRevitPropertyElementCategory -> Category",
                "lcldrevit_parameter_-1002053 -> Workset",
                "LcOaNodeSourceFile -> Source File"
            }));
        }

        [Test]
        public void EveryValueIsAWideStringAndItsDataIsKept()
        {
            HashSet<string> dataTypes = new HashSet<string>();
            int values = 0;

            foreach (SelectionSetDefinition set in document.Sets)
            {
                foreach (SearchConditionDefinition condition in set.Conditions)
                {
                    dataTypes.Add(condition.Value.DataType);
                    Assert.That(condition.Value.Data, Is.Not.Null.And.Not.Empty, set.Name);
                    values++;
                }
            }

            Assert.That(values, Is.EqualTo(102));
            Assert.That(dataTypes, Is.EquivalentTo(new[] { "wstring" }));
        }

        [Test]
        public void ThirtySetsCarryOneConditionTwentySixCarryTwoAndFiveCarryFour()
        {
            Dictionary<int, int> byCount = new Dictionary<int, int>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                int count = set.Conditions.Count;

                if (!byCount.ContainsKey(count))
                {
                    byCount.Add(count, 0);
                }

                byCount[count] = byCount[count] + 1;
            }

            Assert.That(byCount.Count, Is.EqualTo(3));
            Assert.That(byCount[1], Is.EqualTo(30));
            Assert.That(byCount[2], Is.EqualTo(26));
            Assert.That(byCount[4], Is.EqualTo(5));
            Assert.That((30 * 1) + (26 * 2) + (5 * 4), Is.EqualTo(102));
        }

        [Test]
        public void NoSetSitsAtTheRootAndNoNameIsRepeated()
        {
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.SetsAtRoot.Count, Is.EqualTo(0));
            Assert.That(health.DuplicateNames.Count, Is.EqualTo(0));
        }
        // D1. The one sentence the window puts under the picked file.
        [Test]
        public void TheHealthLineSaysEveryLocatorResolves()
        {
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.Line(document.HasSets),
                Is.EqualTo("Every locator resolves against its sets."));
        }
    }
}
