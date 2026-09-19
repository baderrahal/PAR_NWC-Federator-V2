using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The one line that lets criterion 3 be checked off the log. What the workbook got
    /// and what the document holds, beside each other, per test.
    /// </summary>
    [TestFixture]
    public class ReportedCountTests
    {
        [Test]
        public void EqualCountsSayTheyAgree()
        {
            string line = ReportedCount.Line("BLD-ME v BLD-EL", 12, 12);

            Assert.That(line, Does.StartWith("ROWS     BLD-ME v BLD-EL"));
            Assert.That(line, Does.Contain("12 rows for the workbook"));
            Assert.That(line, Does.Contain("12 clashes in the document"));
            Assert.That(line, Does.EndWith("they agree"));
            Assert.That(ReportedCount.NothingExplainsIt(12, 12), Is.False);
        }

        /// <summary>
        /// Fewer rows than clashes is the ordinary case for any test holding result
        /// groups, because the harvest writes one row per group and the tally counts the
        /// clashes inside it. Both numbers are right and the line says why they differ.
        /// </summary>
        [Test]
        public void FewerRowsThanClashesIsTheGroupingAndSaysSo()
        {
            string line = ReportedCount.Line("BLD-ME v BLD-EL", 7, 19);

            Assert.That(line, Does.Contain("7 rows"));
            Assert.That(line, Does.Contain("19 clashes"));
            Assert.That(line, Does.Contain("result groups"));
            Assert.That(line, Does.Not.Contain("MORE ROWS"));
            Assert.That(ReportedCount.NothingExplainsIt(7, 19), Is.False);
        }

        /// <summary>
        /// The break. More rows than clashes is the one shape nothing in this tool
        /// produces, so it is said in capitals with the difference named, and nothing
        /// acts on it.
        /// </summary>
        [Test]
        public void MoreRowsThanClashesIsCalledOutWithTheDifference()
        {
            string line = ReportedCount.Line("BLD-ME v BLD-EL", 20, 19);

            Assert.That(line, Does.Contain("THERE ARE MORE ROWS THAN CLASHES"));
            Assert.That(line, Does.Contain("by 1"));
            Assert.That(line, Does.Contain("will not match the panel"));
            Assert.That(ReportedCount.NothingExplainsIt(20, 19), Is.True);
        }

        [Test]
        public void OneOfEachReadsInTheSingular()
        {
            string line = ReportedCount.Line("one test", 1, 1);

            Assert.That(line, Does.Contain("1 row for the workbook"));
            Assert.That(line, Does.Contain("1 clash in the document"));
        }

        [Test]
        public void NoneAtAllStillWritesItsLine()
        {
            string line = ReportedCount.Line("a test that passed", 0, 0);

            Assert.That(line, Does.Contain("0 rows for the workbook"));
            Assert.That(line, Does.Contain("0 clashes in the document"));
            Assert.That(line, Does.EndWith("they agree"));
        }

        [Test]
        public void ATestWithNoNameSaysUnknownRatherThanLeavingAGap()
        {
            Assert.That(ReportedCount.Line(null, 1, 1), Does.Contain("UNKNOWN test"));
            Assert.That(ReportedCount.Line(string.Empty, 1, 1), Does.Contain("UNKNOWN test"));
        }
    }
}
