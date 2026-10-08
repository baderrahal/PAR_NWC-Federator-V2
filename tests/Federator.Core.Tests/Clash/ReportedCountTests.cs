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

        // ---------- a kept test holding its mirror's extra clashes, F132 attempt 5 item 3 ----------

        // Bader's answer D to Q133 adds the clashes only the mirror found to the kept test, and
        // the panel shows them under the mirror. So the kept test's rows are the panel's own
        // count plus the mirror's extra, which this tool explains, never MORE ROWS THAN CLASHES.
        [Test]
        public void AKeptTestHoldingItsMirrorsExtraClashesIsExplained()
        {
            string line = ReportedCount.Line("BLD-ME v BLD-ST", 27, 25, 2);

            Assert.That(line, Does.Contain("27 rows for the workbook"));
            Assert.That(line, Does.Contain("25 clashes in the document"));
            Assert.That(line, Does.Contain("2 of the rows are clashes only its mirror found, which the panel shows "
                + "under the mirror"));
            Assert.That(line, Does.EndWith("they agree"));
            Assert.That(line, Does.Not.Contain("MORE ROWS"));
        }

        [Test]
        public void OneExtraClashReadsInTheSingular()
        {
            Assert.That(ReportedCount.Line("one test", 2, 1, 1), Does.Contain(
                "1 of the rows is a clash only its mirror found, which the panel shows under the mirror"));
        }

        // The mirror's extra explains its own rows and no more. A row past them is still the
        // finding nothing in this tool explains.
        [Test]
        public void ARowPastTheMirrorsExtraIsStillCalledOut()
        {
            string line = ReportedCount.Line("BLD-ME v BLD-ST", 28, 25, 2);

            Assert.That(line, Does.Contain("THERE ARE MORE ROWS THAN CLASHES, by 1"));
        }

        // Result groups and the mirror's extra together: the rows of the test's own are fewer
        // than its clashes, and the line says the grouping, as without a mirror.
        [Test]
        public void TheGroupingIsStillSaidBesideTheMirrorsExtra()
        {
            string line = ReportedCount.Line("BLD-ME v BLD-ST", 9, 19, 2);

            Assert.That(line, Does.Contain("result groups"));
            Assert.That(line, Does.Not.Contain("MORE ROWS"));
        }

        // No extra is the line as it always was, word for word.
        [Test]
        public void NoExtraIsTheLineAsItWas()
        {
            Assert.That(ReportedCount.Line("a test", 12, 12, 0), Is.EqualTo(ReportedCount.Line("a test", 12, 12)));
            Assert.That(ReportedCount.Line("a test", 20, 19, 0), Is.EqualTo(ReportedCount.Line("a test", 20, 19)));
        }
    }
}
