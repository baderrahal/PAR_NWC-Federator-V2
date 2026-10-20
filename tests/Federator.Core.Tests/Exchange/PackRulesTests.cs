using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F247, Bader's answer Q230 of 20 Oct 2026. The rules of a matrix pack, read in the team map's format, and every fault
    /// that makes the whole file unread, each broken one at a time and named on the PACK line.
    /// </summary>
    [TestFixture]
    public class PackRulesTests
    {
        [Test]
        public void ARuleReadsItsPartsInTheirOrderAndCommentsAndBlankLinesAreSkipped()
        {
            PackRules rules = PackRules.Read("# a comment\n\nsame-code-left-out: AR | ST\n", "the file");

            Assert.That(rules.IsRead, Is.True, rules.Unread);
            Assert.That(rules.SameCodeLeftOut, Is.EqualTo(new[] { "AR", "ST" }));
            Assert.That(PackRules.Read("same-code-left-out: ST | AR\r\n", "f").SameCodeLeftOut, Is.EqualTo(new[] { "ST", "AR" }));
        }

        [Test]
        public void ARuleWithNoPartOrNoRuleAtAllLeavesOutNone()
        {
            PackRules empty = PackRules.Read("same-code-left-out:\n", "f");
            PackRules nothing = PackRules.Read("# only a comment\n", "f");

            Assert.That(empty.IsRead, Is.True, empty.Unread);
            Assert.That(empty.SameCodeLeftOut, Is.Empty);
            Assert.That(empty.Line(), Does.Contain("no set code is left out against itself, so every test of one set code runs"));
            Assert.That(nothing.IsRead, Is.True);
            Assert.That(nothing.SameCodeLeftOut, Is.Empty);
        }

        [TestCase("same-code-leftout: AR", "names a rule this tool does not know, same-code-leftout")]
        [TestCase("AR | ST", "is no rule")]
        [TestCase("same-code-left-out: AR\nsame-code-left-out: ST", "line 3, \"same-code-left-out: ST\", writes the rule same-code-left-out a second time")]
        [TestCase("same-code-left-out:AR", "has no space after its colon")]
        [TestCase("same-code-left-out: AR |ST", "has an empty part or a bar with no space either side")]
        [TestCase("same-code-left-out: AR | ", "has an empty part or a bar with no space either side")]
        [TestCase("same-code-left-out: AR | AR", "names AR twice")]
        [TestCase(" same-code-left-out: AR", "names a rule this tool does not know")]
        public void AFaultMakesTheWholeFileUnreadAndIsNamed(string text, string named)
        {
            PackRules rules = PackRules.Read("# rules\n" + text + "\n", "the file");

            Assert.That(rules.IsRead, Is.False);
            Assert.That(rules.SameCodeLeftOut, Is.Empty, "never half a file");
            Assert.That(rules.Unread, Does.Contain(named));
            Assert.That(rules.Line(), Does.StartWith("PACK    the pack's rules were not read, ").And.Contain(named)
                .And.EndWith("so no test of one set code is left out and every one runs, Bader's answer to Q230"));
        }

        [Test]
        public void NoTextAndNoFileReadNone()
        {
            Assert.That(PackRules.Read(null, "f").IsRead, Is.False);
            Assert.That(PackRules.NoneRead("f", "the pack holds no pack-rules.txt").Line(), Does.Contain("the pack holds no pack-rules.txt"));
            Assert.That(PackRules.Joined(new[] { "AR", "ST", "EL" }), Is.EqualTo("AR, ST and EL"));
            Assert.That(PackRules.Joined(new string[0]), Is.EqualTo("none"));
        }

        /// <summary>
        /// The BY DESIGN block says how many pairs the file holds and how many of the clashes moved came from a pair whose two
        /// sets carry one set code, a nought said, and UNKNOWN where no code was known to read a pair against.
        /// </summary>
        [Test]
        public void TheByDesignBlockSaysThePairsAndTheClashesMovedFromPairsOfOneCode()
        {
            ByDesignPairs pairs = ByDesignPairs.Read(
                "left_set,right_set,reason\nBLD-ME-Ducts,BLD-ME-Air Terminals,diffuser\nBLD-EL-Lighting Fixtures,BLD-AR-Ceilings,light\n", "f");
            ByDesignTally tally = new ByDesignTally();
            tally.ReadsThePairs(pairs.Count, new[] { "ME", "EL", "AR" }, '-');

            Assert.That(tally.Lines(), Does.Contain("pairs in the file : 2"));
            Assert.That(tally.Lines(), Does.Contain("moved from pairs of one set code : 0 of the 0 moved, from pairs whose two sets carry one set code"),
                "a nought is said");

            ByDesignPair one = null;
            ByDesignPair two = null;

            foreach (ByDesignPair pair in pairs.All)
            {
                if (pair.Left == "BLD-ME-Ducts")
                {
                    one = pair;
                }
                else
                {
                    two = pair;
                }
            }

            tally.Add("T1", "Clash1", ByDesignVerdict.Reviewed, one);
            tally.Add("T1", "Clash2", ByDesignVerdict.Reviewed, one);
            tally.Add("T2", "Clash1", ByDesignVerdict.Reviewed, two);
            tally.Add("T2", "Clash2", ByDesignVerdict.SomebodyDecided, one);

            Assert.That(tally.MovedFromOneCode, Is.EqualTo(2), "a pair of two codes and a clash not moved are not counted");
            Assert.That(tally.Lines(), Does.Contain("moved from pairs of one set code : 2 of the 3 moved, from pairs whose two sets carry one set code"));

            ByDesignTally run = new ByDesignTally();
            run.Add(tally);
            Assert.That(run.MovedFromOneCode, Is.EqualTo(2), "the run total adds each group's");

            ByDesignTally noCodes = new ByDesignTally();
            noCodes.ReadsThePairs(2, null, '-');
            noCodes.Add("T1", "Clash1", ByDesignVerdict.Reviewed, one);
            Assert.That(noCodes.Lines(), Does.Contain("moved from pairs of one set code : UNKNOWN, no set code was known to read the pairs against"));
            Assert.That(new ByDesignTally().Lines(), Has.None.StartsWith("pairs in the file"), "a tally never told the pairs says nothing of them");
        }
    }
}
