using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The Infra export is sets only. It carries the same rule 2715 times, holds a
    /// repeated name and a set with no folder above it.
    /// </summary>
    [TestFixture]
    public class InfraSetsFileTests
    {
        private ExchangeDocument document;

        [OneTimeSetUp]
        public void ReadTheFile()
        {
            document = new ExchangeReader().ReadFile(Samples.Infra());
        }

        [Test]
        public void HoldsTwentySixSetsAndNoTests()
        {
            Assert.That(document.Sets.Count, Is.EqualTo(26));
            Assert.That(document.Tests.Count, Is.EqualTo(0));
            Assert.That(document.HasTests, Is.False);
            Assert.That(document.BatchTests.Count, Is.EqualTo(0));
        }

        [Test]
        public void HoldsTwentySevenHundredAndFifteenConditions()
        {
            int conditions = 0;

            foreach (SelectionSetDefinition set in document.Sets)
            {
                conditions += set.Conditions.Count;
            }

            Assert.That(conditions, Is.EqualTo(2715));
        }

        [Test]
        public void EveryConditionAsksTheSameThingSoTheExportIsUnusable()
        {
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.ConditionCount, Is.EqualTo(2715));
            Assert.That(health.DistinctRuleCount, Is.EqualTo(1));
            Assert.That(health.AllSetsShareOneRule, Is.True);
            Assert.That(health.ExportUnusable, Is.True);
        }

        [Test]
        public void ConditionCountsPerSetAreOneThreeNineTwentySevenEightyOneAndThreeHundredAndTwentyFour()
        {
            SortedSet<int> counts = new SortedSet<int>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                counts.Add(set.Conditions.Count);
            }

            Assert.That(counts, Is.EqualTo(new[] { 1, 3, 9, 27, 81, 324 }));
        }

        [Test]
        public void ThreeSetsShareTheNameManholesWithCopySuffixes()
        {
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.DuplicateNames.Count, Is.EqualTo(1));

            DuplicateSetName duplicate = health.DuplicateNames[0];

            Assert.That(duplicate.BaseName, Is.EqualTo("INF-FS-MH_Manholes"));
            Assert.That(duplicate.Count, Is.EqualTo(3));
            Assert.That(duplicate.IsExact, Is.False);

            List<string> names = new List<string>();

            foreach (SelectionSetDefinition set in duplicate.Sets)
            {
                names.Add(set.Name);
            }

            Assert.That(names, Does.Contain("INF-FS-MH_Manholes"));
            Assert.That(names, Does.Contain("INF-FS-MH_Manholes (1)"));
            Assert.That(names, Does.Contain("INF-FS-MH_Manholes (2)"));
        }

        [Test]
        public void OneSetSitsAtTheRootWithNoFolder()
        {
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.SetsAtRoot.Count, Is.EqualTo(1));
            Assert.That(health.SetsAtRoot[0].Name, Is.EqualTo("Search Set"));
            Assert.That(health.SetsAtRoot[0].Path, Is.EqualTo("lcop_selection_set_tree/Search Set"));
            Assert.That(health.SetsAtRoot[0].Folders.Count, Is.EqualTo(0));
        }

        [Test]
        public void TheFoldersNestTwoDeepAndAreWalked()
        {
            SelectionSetDefinition manholes = null;

            foreach (SelectionSetDefinition set in document.Sets)
            {
                if (set.Name == "INF-TE-MH_Manholes")
                {
                    manholes = set;
                }
            }

            Assert.That(manholes, Is.Not.Null);
            Assert.That(manholes.Folders, Is.EqualTo(new[] { "Gravity Networks", "Treated Sewage" }));
            Assert.That(
                manholes.Path,
                Is.EqualTo("lcop_selection_set_tree/Gravity Networks/Treated Sewage/INF-TE-MH_Manholes"));
        }

        [Test]
        public void SetNamesHoldAmpersandsAndAreReadExactly()
        {
            List<string> names = new List<string>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                names.Add(set.Name);
            }

            Assert.That(names, Does.Contain("INF-DC-PF_Pipes&Fittings"));
            Assert.That(names, Does.Contain("INF-DC-EQ_Specialty&Equipment"));
        }

        [Test]
        public void TheFileUnitsAreStillFeetEvenWithNoTests()
        {
            Assert.That(document.Units, Is.EqualTo("ft"));
        }
    }
}
