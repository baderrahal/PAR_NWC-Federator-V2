using Federator.Core.Exchange;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The two exports name their sets differently, so pairing tests from one against
    /// sets from the other resolves nothing. This is what the health check exists to
    /// catch before a run starts.
    /// </summary>
    [TestFixture]
    public class CrossFileTests
    {
        private ExchangeDocument allInOne;
        private ExchangeDocument building;

        [OneTimeSetUp]
        public void ReadTheFiles()
        {
            ExchangeReader reader = new ExchangeReader();
            allInOne = reader.ReadFile(Samples.AllInOne());
            building = reader.ReadFile(Samples.Building());
        }

        [Test]
        public void PairingTheClashTestsWithTheBuildingSetsResolvesNoneOfTheSixtyOne()
        {
            HealthCheckResult health = HealthCheck.Run(allInOne, building);

            Assert.That(health.TestCount, Is.EqualTo(1830));
            Assert.That(health.SetCount, Is.EqualTo(61));
            Assert.That(health.TotalLocatorCount, Is.EqualTo(61));
            Assert.That(health.ResolvedLocatorCount, Is.EqualTo(0));
            Assert.That(health.UnresolvedLocatorCount, Is.EqualTo(61));
        }

        [Test]
        public void EveryTestIsReportedByNameAndSkipped()
        {
            HealthCheckResult health = HealthCheck.Run(allInOne, building);

            Assert.That(health.TestsWithUnresolvedSide.Count, Is.EqualTo(1830));
            Assert.That(
                health.TestsWithUnresolvedSide[0],
                Is.EqualTo(allInOne.Tests[0].Name));
        }

        [Test]
        public void EachUnresolvedLocatorNamesTheTestsThatPointedAtIt()
        {
            HealthCheckResult health = HealthCheck.Run(allInOne, building);

            foreach (UnresolvedLocator unresolved in health.UnresolvedLocators)
            {
                Assert.That(unresolved.TestNames.Count, Is.GreaterThan(0), unresolved.Locator);
                Assert.That(unresolved.Locator, Does.StartWith("lcop_selection_set_tree/"));
            }

            // Every one of the 61 sets pairs with the other 60.
            Assert.That(health.UnresolvedLocators[0].TestNames.Count, Is.EqualTo(60));
        }

        [Test]
        public void ReadingBothFilesTogetherGivesOneDocument()
        {
            ExchangeDocument combined = new ExchangeReader().ReadFiles(
                new[] { Samples.AllInOne(), Samples.Infra() });

            Assert.That(combined.Tests.Count, Is.EqualTo(1830));
            Assert.That(combined.Sets.Count, Is.EqualTo(61 + 26));
            Assert.That(combined.Units, Is.EqualTo("ft"));
        }
    }
}
