using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// A real run wrote 1830 near identical SKIPPED lines and a 1 MB log, then finished
    /// with 0 created and 0 run because the document held no sets at all. Two separate
    /// faults: it said one thing 1830 times, and it went ahead when it could not possibly
    /// clash anything.
    /// </summary>
    [TestFixture]
    public class ClashSkipSummaryTests
    {
        private static ClashRunOutcome WithSkips(int howMany, ClashSkipReason reason)
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.TestsInFile = howMany;

            for (int i = 1; i <= howMany; i++)
            {
                outcome.AddSkipped(
                    "T" + i.ToString("0000"), reason, "the left side finds nothing in this model");
            }

            return outcome;
        }

        private static string Block(ClashRunOutcome outcome)
        {
            return string.Join("\n", new List<string>(outcome.Lines()).ToArray());
        }

        // ---------- one thing said once, not 1830 times ----------

        [Test]
        public void EighteenHundredSkipsForOneReasonDoNotBecomeEighteenHundredLines()
        {
            ClashRunOutcome outcome = WithSkips(1830, ClashSkipReason.EmptySide);
            IList<string> skipLines = outcome.SkipLines();

            // One heading, five examples, one line saying how many are not listed.
            Assert.That(skipLines.Count, Is.EqualTo(7),
                "the skip block grew with the number of skips");

            Assert.That(outcome.SkippedCount, Is.EqualTo(1830),
                "the count itself must still be all of them");
        }

        [Test]
        public void AtMostFiveExamplesAreNamedForEachReason()
        {
            IList<string> lines = WithSkips(1830, ClashSkipReason.EmptySide).SkipLines();
            int named = 0;

            foreach (string line in lines)
            {
                if (line.StartsWith("        T"))
                {
                    named++;
                }
            }

            Assert.That(named, Is.EqualTo(ClashRunOutcome.MaxSkipExamples));
            Assert.That(ClashRunOutcome.MaxSkipExamples, Is.EqualTo(5));
        }

        [Test]
        public void TheCountAndTheRemainderAreBothStated()
        {
            string block = Block(WithSkips(1830, ClashSkipReason.EmptySide));

            Assert.That(block, Does.Contain("SKIPPED 1830 tests, "
                + ClashTestPlan.Describe(ClashSkipReason.EmptySide)));
            Assert.That(block, Does.Contain("and 1825 more skipped for the same reason"));
            Assert.That(block, Does.Contain("counted and not listed"));
            Assert.That(block, Does.Contain("tests skipped     : 1830"));
        }

        // The examples plus the remainder must always account for every skip.
        [Test]
        public void TheExamplesAndTheRemainderAlwaysAddUpToTheTotal()
        {
            foreach (int howMany in new[] { 1, 2, 5, 6, 7, 100, 1830 })
            {
                ClashRunOutcome outcome = WithSkips(howMany, ClashSkipReason.EmptySide);
                IList<string> lines = outcome.SkipLines();

                int named = 0;

                foreach (string line in lines)
                {
                    if (line.StartsWith("        T"))
                    {
                        named++;
                    }
                }

                int expectedNamed = howMany < ClashRunOutcome.MaxSkipExamples
                    ? howMany
                    : ClashRunOutcome.MaxSkipExamples;

                Assert.That(named, Is.EqualTo(expectedNamed), howMany + " skips");

                string block = Block(outcome);

                if (howMany > expectedNamed)
                {
                    Assert.That(block, Does.Contain("and " + (howMany - expectedNamed) + " more"),
                        howMany + " skips");
                }
                else
                {
                    Assert.That(block, Does.Not.Contain(" more skipped for the same reason"),
                        howMany + " skips");
                }
            }
        }

        [Test]
        public void FiveOrFewerAreAllNamedWithNoRemainderLine()
        {
            string block = Block(WithSkips(5, ClashSkipReason.EmptySide));

            Assert.That(block, Does.Contain("T0005"));
            Assert.That(block, Does.Not.Contain(" more skipped for the same reason"));
        }

        [Test]
        public void EachReasonIsCountedAndExemplifiedOnItsOwn()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();

            for (int i = 0; i < 20; i++)
            {
                outcome.AddSkipped("empty" + i, ClashSkipReason.EmptySide, "a side finds nothing");
            }

            for (int i = 0; i < 9; i++)
            {
                outcome.AddSkipped("missing" + i, ClashSkipReason.LocatorNotResolved, "not in the document");
            }

            string block = Block(outcome);

            Assert.That(block, Does.Contain("SKIPPED 20 tests, "
                + ClashTestPlan.Describe(ClashSkipReason.EmptySide)));
            Assert.That(block, Does.Contain("SKIPPED 9 tests, "
                + ClashTestPlan.Describe(ClashSkipReason.LocatorNotResolved)));
            Assert.That(block, Does.Contain("and 15 more"));
            Assert.That(block, Does.Contain("and 4 more"));
            Assert.That(outcome.SkippedCount, Is.EqualTo(29));
        }

        [Test]
        public void OneSkipReadsAsOneTestRatherThanOneTests()
        {
            Assert.That(Block(WithSkips(1, ClashSkipReason.EmptySide)),
                Does.Contain("SKIPPED 1 test, "));
        }

        // ---------- per test detail stays for tests that did something ----------

        [Test]
        public void EveryTestThatRanStillGetsItsOwnLine()
        {
            ClashRunOutcome outcome = WithSkips(1830, ClashSkipReason.EmptySide);

            ClashTally tally = new ClashTally();
            tally.Add(ClashStatus.New, 4);

            for (int i = 1; i <= 12; i++)
            {
                outcome.AddRan("ran" + i, 100, 50, tally, 1.0);
            }

            string block = Block(outcome);

            for (int i = 1; i <= 12; i++)
            {
                Assert.That(block, Does.Contain("ran" + i),
                    "a test that ran lost its own line to the summarising");
            }

            Assert.That(outcome.RanCount, Is.EqualTo(12));
        }

        [Test]
        public void NoSkipsAtAllProducesNoSkipBlock()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddRan("only one", 10, 10, new ClashTally(), 0.5);

            Assert.That(outcome.SkipLines().Count, Is.EqualTo(0));
            Assert.That(Block(outcome), Does.Not.Contain("SKIPPED"));
        }

        // ---------- the zero sets guard ----------

        [Test]
        public void AStoppedRunSaysBothNumbersAndCreatedNothing()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.TestsInFile = 1830;
            outcome.OpenDocument = "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwf";

            string why = outcome.StopBecauseNoSetResolves(0, 61);

            Assert.That(outcome.Stopped, Is.True);
            Assert.That(why, Does.Contain("the document holds 0 sets"));
            Assert.That(why, Does.Contain("the tests name 61 sets"));
            Assert.That(why, Does.Contain("Nothing was created and nothing was run."));

            string block = Block(outcome);

            Assert.That(block, Does.Contain("STOPPED"));
            Assert.That(block, Does.Contain("tests in the file : 1830"));
            Assert.That(block, Does.Contain("tests created     : 0"));
            Assert.That(block, Does.Contain("tests run         : 0"));
        }

        // The guard exists so that nothing is walked. A stopped run must not then print
        // the 1830 line skip block it was meant to avoid.
        [Test]
        public void AStoppedRunDoesNotAlsoListEveryTest()
        {
            ClashRunOutcome outcome = WithSkips(1830, ClashSkipReason.EmptySide);
            outcome.StopBecauseNoSetResolves(0, 61);

            IList<string> lines = outcome.Lines();

            Assert.That(lines.Count, Is.LessThan(12),
                "a stopped run still walked the tests it was meant to skip walking");
            Assert.That(Block(outcome), Does.Not.Contain("T0001"));
        }

        [Test]
        public void TheStoppedReasonReachesTheOneLineSummary()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.StopBecauseNoSetResolves(0, 61);

            Assert.That(outcome.Summary(), Does.StartWith("Stopped before creating anything."));
            Assert.That(outcome.Summary(), Does.Contain("0 sets"));
        }

        [Test]
        public void ARunThatWasNotStoppedSummarisesNormally()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("one");
            outcome.AddRan("one", 10, 10, new ClashTally(), 0.5);

            Assert.That(outcome.Stopped, Is.False);
            Assert.That(outcome.StoppedReason, Is.Null);
            Assert.That(outcome.Summary(), Does.Contain("1 created"));
        }

        [Test]
        public void OneSetEachReadsAsSetRatherThanSets()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            string why = outcome.StopBecauseNoSetResolves(1, 1);

            Assert.That(why, Does.Contain("holds 1 set "));
            Assert.That(why, Does.Contain("name 1 set,"));
        }

        // ---------- what the guard is deciding on ----------

        // The plan can resolve nothing against a document holding no sets, which is the
        // condition the runner reads before it creates anything.
        [Test]
        public void NoSetInTheDocumentMeansNoTestResolves()
        {
            ClashTestPlan plan = ClashTestPlan.From(
                new ExchangeReader().ReadFile(Samples.AllInOne()), "m");

            Assert.That(plan.Buildable.Count, Is.GreaterThan(0), "the plan should start with work");

            ClashTestPlan resolved = plan.ResolveAgainst(new string[0]);

            Assert.That(resolved.Buildable.Count, Is.EqualTo(0),
                "this is the condition the guard reads");
            Assert.That(plan.DistinctLocators().Count, Is.EqualTo(61),
                "and this is the number of sets the tests name");
        }
    }
}
