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
        /// <summary>The codes read, compared count first and then one by one, as tests.md asks of a list.</summary>
        internal static void AreTheCodes(System.Collections.Generic.IList<string> actual, params string[] expected)
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Count, Is.EqualTo(expected.Length), "how many codes");

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(actual[i], Is.EqualTo(expected[i]), "code " + (i + 1));
            }
        }

        [Test]
        public void ARuleReadsItsPartsInTheirOrderAndCommentsAndBlankLinesAreSkipped()
        {
            PackRules rules = PackRules.Read("# a comment\n\nsame-code-left-out: AR | ST\n", "the file");

            Assert.That(rules.IsRead, Is.True, rules.Unread);
            AreTheCodes(rules.SameCodeLeftOut, "AR", "ST");
            AreTheCodes(PackRules.Read("same-code-left-out: ST | AR\r\n", "f").SameCodeLeftOut, "ST", "AR");
        }

        /// <summary>A pack that holds no rules file reads none and says so, read through the pack and not only the reader.</summary>
        [Test]
        public void APackWithNoRulesFileReadsNoneAndSaysWhy()
        {
            System.Collections.Generic.IList<MatrixPack> packs = MatrixPack.All(
                new[] { MatrixPack.Prefix + "/Other/" + MatrixPack.ByDesignFile }, typeof(MatrixPack).Assembly);

            Assert.That(packs.Count, Is.EqualTo(1));
            PackRules rules = packs[0].Rules();

            Assert.That(rules.IsRead, Is.False);
            Assert.That(rules.SameCodeLeftOut, Is.Empty);
            Assert.That(rules.Line(), Is.EqualTo("PACK    the pack's rules were not read, the pack holds no pack-rules.txt, so no test of one"
                + " set code is left out and every one runs, Bader's answer to Q230"));
        }

        /// <summary>The NWF page's confirm names the codes the rules leave out, or that every test of one set code runs.</summary>
        [Test]
        public void TheConfirmNamesTheCodesTheRulesLeaveOut()
        {
            Assert.That(SetCodeRule.ConfirmWords(new[] { "AR", "ST" }), Is.EqualTo("A test whose two sides carry one set code is not run"
                + " where that code is AR or ST, as the project's rules say, and one saved in the NWF keeps its earlier results there. "));
            Assert.That(SetCodeRule.ConfirmWords(new string[0]), Is.EqualTo(
                "A test whose two sides carry one set code runs, the project's rules leaving out none. "));
            Assert.That(SetCodeRule.ConfirmWords(null), Is.EqualTo(SetCodeRule.ConfirmWords(new string[0])));
            Assert.That(Federator.Core.Rerun.RefreshConfirm.Text("A.nwf", "Backup", null, "steps", 10, 0, 0, 0, "Clash Status", false,
                new[] { "AR", "ST" }), Does.Contain("is not run where that code is AR or ST"));
            Assert.That(Federator.Core.Rerun.RefreshConfirm.Text("A.nwf", "Backup", null, "steps", 10, 0, 0, 0, "Clash Status"),
                Does.Contain("runs, the project's rules leaving out none"));
        }

        /// <summary>The CLASH line tells rules not read from rules that leave out none, as the PACK line does.</summary>
        [Test]
        public void TheClashLineTellsRulesNotReadFromRulesLeavingOutNone()
        {
            System.Collections.Generic.Dictionary<string, int> none = new System.Collections.Generic.Dictionary<string, int>();

            Assert.That(SetCodeRule.Line(0, 10, none, true, 0, null, none), Does.Contain("the pack's rules not read, so no set code is left out against itself"));
            Assert.That(SetCodeRule.Line(0, 10, none, true, 0, new string[0], none), Does.Contain("the pack's rules leaving out no set code against itself"));
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
        [TestCase("same-code-left-out: ", "has an empty part or a bar with no space either side")]
        [TestCase("same-code-left-out: AR | ST ", "names \"ST \", which has")]
        [TestCase("same-code-left-out: AR | S T", "names \"S T\", which has")]
        [TestCase("same-code-left-out: AR | ST\t", "names \"ST\t\", which has")]
        [TestCase("same-code-left-out: AR |  ST", "which has")]
        [TestCase("same-code-left-out: AR | S​T", "which has")]
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

            ByDesignTally noCodes = new ByDesignTally();
            noCodes.ReadsThePairs(2, null, '-');
            noCodes.Add("T1", "Clash1", ByDesignVerdict.Reviewed, one);
            Assert.That(noCodes.Lines(), Does.Contain("moved from pairs of one set code : UNKNOWN, no set code was known to read the pairs against"));
            Assert.That(new ByDesignTally().Lines(), Has.None.StartsWith("pairs in the file"), "a tally never told the pairs says nothing of them");
        }
    }
}
