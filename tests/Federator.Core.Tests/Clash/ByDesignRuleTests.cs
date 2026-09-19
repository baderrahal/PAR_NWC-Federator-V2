using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F72b. A column sitting on its foundation, a door in a wall, a valve in a pipe run.
    /// Every one of those is a clash and none of them is a problem, and a person looks at
    /// the same several hundred of them every week.
    ///
    /// This rule is a LIST and not a judgement. It knows nothing about items, categories,
    /// sizes or disciplines. It reads two set names off a file and matches them against
    /// the two sides of a test.
    /// </summary>
    [TestFixture]
    public class ByDesignRuleTests
    {
        private static ByDesignPairs TheFile()
        {
            return ByDesignPairs.Read(File.ReadAllText(Samples.ByDesign()), Samples.ByDesign());
        }

        private static ByDesignPairs From(string text)
        {
            return ByDesignPairs.Read(text, "a.csv");
        }

        // ---------- the file ----------

        [Test]
        public void TheFileIsReadOffTheDiskAndNotACopy()
        {
            ByDesignPairs pairs = TheFile();

            Assert.That(pairs.Picked, Is.True);
            Assert.That(pairs.Count, Is.EqualTo(41), "forty one pairs, read off the file");
            Assert.That(pairs.Problems, Is.Empty);
        }

        [Test]
        public void TheHeaderIsTheThreeColumnsAndIsNotReadAsAPair()
        {
            Assert.That(ByDesignPairs.Columns,
                Is.EqualTo(new[] { "left_set", "right_set", "reason" }).AsCollection);
            Assert.That(TheFile().Holds("left_set", "right_set"), Is.False);
        }

        [Test]
        public void EveryPairCarriesAReasonInWordsAPersonWouldSay()
        {
            foreach (ByDesignPair pair in TheFile().All)
            {
                Assert.That(pair.Reason, Is.Not.Empty, pair.Left + " and " + pair.Right);
                Assert.That(pair.Reason, Does.Not.Contain("\r"), "a carriage return reached a reason");
            }
        }

        /// <summary>
        /// The pairs file already carries F87's corrected name. Against the uncorrected
        /// matrix one pair matches no set at all, and that is the rename being visible
        /// rather than a fault in this reader. Nothing here papers over the hyphen.
        /// </summary>
        [Test]
        public void EverySetNameInTheFileIsASetInTheCorrectedMatrix()
        {
            HashSet<string> sets = SetNamesIn(Samples.CorrectedMatrix());
            List<string> missing = new List<string>();

            foreach (ByDesignPair pair in TheFile().All)
            {
                if (!sets.Contains(pair.Left))
                {
                    missing.Add(pair.Left);
                }

                if (!sets.Contains(pair.Right))
                {
                    missing.Add(pair.Right);
                }
            }

            Assert.That(missing, Is.Empty,
                "a set the pairs file names that the matrix does not hold: "
                    + string.Join("; ", missing.ToArray()));
        }

        [Test]
        public void AgainstTheUncorrectedMatrixOneSetNameIsMissing()
        {
            HashSet<string> sets = SetNamesIn(Samples.Matrix());
            int missing = 0;

            foreach (ByDesignPair pair in TheFile().All)
            {
                if (!sets.Contains(pair.Left))
                {
                    missing++;
                }

                if (!sets.Contains(pair.Right))
                {
                    missing++;
                }
            }

            Assert.That(missing, Is.EqualTo(1),
                "BLD-DR-Pipe Accessories, which F87 renames");
        }

        // ---------- how a pair is matched ----------

        /// <summary>
        /// The two names are sorted before matching, so a pair written one way round
        /// matches a test written the other way round. A column on a foundation is the
        /// same connection whichever side the matrix put first.
        /// </summary>
        [Test]
        public void APairMatchesWhicheverWayRoundTheTestNamesThem()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\nA,B,a sits on b\n");

            Assert.That(pairs.Holds("A", "B"), Is.True);
            Assert.That(pairs.Holds("B", "A"), Is.True);
            Assert.That(ByDesignPairs.KeyFor("A", "B"), Is.EqualTo(ByDesignPairs.KeyFor("B", "A")));
        }

        /// <summary>
        /// The opposite of how a CATEGORY is matched. A category is trimmed and compared
        /// without case, because it comes off a model property. A set name is compared
        /// Ordinal and never trimmed, because two of them in the reference file end in a
        /// space and this file carries two different ampersand spellings, both real.
        /// </summary>
        [Test]
        public void ASetNameIsOrdinalAndNeverTrimmedOrLowered()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\nBLD-AR-Doors,BLD-AR-Walls ,door in wall\n");

            Assert.That(pairs.Holds("BLD-AR-Doors", "BLD-AR-Walls "), Is.True);
            Assert.That(pairs.Holds("BLD-AR-Doors", "BLD-AR-Walls"), Is.False,
                "a name with the space trimmed off is a different set");
            Assert.That(pairs.Holds("bld-ar-doors", "BLD-AR-Walls "), Is.False);
        }

        [Test]
        public void BothAmpersandSpellingsInTheRealFileAreRealAndBothMatch()
        {
            ByDesignPairs pairs = TheFile();
            int withNoSpaces = 0;
            int withSpaces = 0;

            foreach (ByDesignPair pair in pairs.All)
            {
                string both = pair.Left + pair.Right;

                if (both.Contains(" & "))
                {
                    withSpaces++;
                }
                else if (both.Contains("&"))
                {
                    withNoSpaces++;
                }
            }

            Assert.That(withSpaces, Is.GreaterThan(0));
            Assert.That(withNoSpaces, Is.GreaterThan(0));
        }

        [Test]
        public void TheSetNameIsTheLastSegmentOfTheLocator()
        {
            Assert.That(ByDesignRule.SetNameIn("lcop_selection_set_tree/Mechanical/BLD-ME-Ducts"),
                Is.EqualTo("BLD-ME-Ducts"));
            Assert.That(ByDesignRule.SetNameIn("BLD-ME-Ducts"), Is.EqualTo("BLD-ME-Ducts"));
            Assert.That(ByDesignRule.SetNameIn("a/b/Name With A Space "),
                Is.EqualTo("Name With A Space "), "nothing is trimmed");
            Assert.That(ByDesignRule.SetNameIn(null), Is.EqualTo(string.Empty));
        }

        // ---------- what the rule decides ----------

        private static ByDesignVerdict Judge(
            ByDesignPairs pairs, string left, string right, ClashStatus status, bool penetration)
        {
            ByDesignPair pair;
            return ByDesignRule.Judge(pairs, left, right, status, penetration, out pair);
        }

        [Test]
        public void APairAtNewOrActiveMovesToReviewed()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\nA,B,a sits on b\n");

            Assert.That(Judge(pairs, "A", "B", ClashStatus.New, false),
                Is.EqualTo(ByDesignVerdict.Reviewed));
            Assert.That(Judge(pairs, "B", "A", ClashStatus.Active, false),
                Is.EqualTo(ByDesignVerdict.Reviewed));
        }

        /// <summary>
        /// Never overwrite a decision. Reviewed, Approved and Resolved are all somebody's
        /// statement about a clash and the NWF is the only record that they made it.
        /// </summary>
        [Test]
        public void ADecisionSomebodyMadeIsNeverOverwritten()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\nA,B,a sits on b\n");

            Assert.That(Judge(pairs, "A", "B", ClashStatus.Reviewed, false),
                Is.EqualTo(ByDesignVerdict.SomebodyDecided));
            Assert.That(Judge(pairs, "A", "B", ClashStatus.Approved, false),
                Is.EqualTo(ByDesignVerdict.SomebodyDecided));
            Assert.That(Judge(pairs, "A", "B", ClashStatus.Resolved, false),
                Is.EqualTo(ByDesignVerdict.SomebodyDecided));
        }

        [Test]
        public void TwoSetsThatAreNotAPairAreLeftExactlyAsTheyAre()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\nA,B,a sits on b\n");

            Assert.That(Judge(pairs, "A", "C", ClashStatus.New, false),
                Is.EqualTo(ByDesignVerdict.NotAPair));
        }

        /// <summary>
        /// The order of the questions. A clash that is not a pair reads as NotAPair even
        /// when somebody has decided about it, because the rule was never going to touch
        /// it and a block saying otherwise reads as though the guard did all the work.
        /// </summary>
        [Test]
        public void NotAPairBeatsSomebodyDecided()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\nA,B,a sits on b\n");

            Assert.That(Judge(pairs, "A", "C", ClashStatus.Approved, false),
                Is.EqualTo(ByDesignVerdict.NotAPair));
        }

        /// <summary>
        /// A clash both rules want is moved once, by the penetration rule, so the two
        /// blocks add up to the number of clashes that moved rather than to twice it.
        /// </summary>
        [Test]
        public void ThePenetrationRuleOwnsAClashTheyBothWant()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\nA,B,a sits on b\n");

            Assert.That(Judge(pairs, "A", "B", ClashStatus.New, true),
                Is.EqualTo(ByDesignVerdict.ThePenetrationRuleHasIt));
        }

        [Test]
        public void ASideNamingNoSetIsSaidAndNotGuessedAt()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\nA,B,a sits on b\n");

            Assert.That(Judge(pairs, string.Empty, "B", ClashStatus.New, false),
                Is.EqualTo(ByDesignVerdict.NoSetName));
            Assert.That(Judge(pairs, "A", null, ClashStatus.New, false),
                Is.EqualTo(ByDesignVerdict.NoSetName));
        }

        [Test]
        public void NothingPickedMovesNothingAndIsNotAFault()
        {
            Assert.That(Judge(ByDesignPairs.NothingPicked(), "A", "B", ClashStatus.New, false),
                Is.EqualTo(ByDesignVerdict.NotAPair));
            Assert.That(Judge(null, "A", "B", ClashStatus.New, false),
                Is.EqualTo(ByDesignVerdict.NotAPair));
        }

        // ---------- what the file refuses ----------

        [Test]
        public void ARowWithTooFewCellsIsNamedAndLeftOut()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\nA,B,ok\nbroken\n");

            Assert.That(pairs.Count, Is.EqualTo(1));
            Assert.That(pairs.Problems[0], Does.Contain("line 3"));
            Assert.That(pairs.Problems[0], Does.Contain("needs three"));
        }

        [Test]
        public void TheSamePairTwiceIsNamedAndKeptOnce()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\nA,B,first\nB,A,second\n");

            Assert.That(pairs.Count, Is.EqualTo(1));
            Assert.That(pairs.Problems[0], Does.Contain("same pair as an earlier line"));
            Assert.That(pairs.For("A", "B").Reason, Is.EqualTo("first"));
        }

        /// <summary>
        /// A file a person points at will not be the one in samples, which .gitattributes
        /// pins to LF. A file saved out of Excel is CRLF, and a carriage return left on the
        /// reason would reach the log.
        /// </summary>
        [Test]
        public void ACarriageReturnNeverReachesASetNameOrAReason()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\r\nA,B,a sits on b\r\n");

            Assert.That(pairs.Count, Is.EqualTo(1));
            Assert.That(pairs.Holds("A", "B"), Is.True);
            Assert.That(pairs.For("A", "B").Reason, Is.EqualTo("a sits on b"));
        }

        [Test]
        public void AnEmptyFileIsAProblemAndNotAThrow()
        {
            ByDesignPairs pairs = From(string.Empty);

            Assert.That(pairs.Count, Is.EqualTo(0));
            Assert.That(pairs.Problems[0], Does.Contain("empty"));
        }

        // ---------- the block and the line ----------

        [Test]
        public void TheRunLineSaysHowManyMovedAndHowManyPairsMatchedNothing()
        {
            Assert.That(ByDesignTally.RunLine(47, 3),
                Is.EqualTo("REVIEWED rule B 47 set, 3 pairs in the file matched no test in this run"));
            Assert.That(ByDesignTally.RunLine(1, 1), Does.Contain("1 pair in the file"));
        }

        [Test]
        public void APairThatMatchedNoTestIsAFindingAndNothingMore()
        {
            ByDesignPairs pairs = From("left_set,right_set,reason\nA,B,one\nC,D,two\n");
            ByDesignTally tally = new ByDesignTally();

            tally.Add("T", "clash 1", ByDesignVerdict.Reviewed, pairs.For("A", "B"));

            IList<ByDesignPair> missed = pairs.NotMatched(tally.PairsSeen);

            Assert.That(missed.Count, Is.EqualTo(1));
            Assert.That(missed[0].Left, Is.EqualTo("C"));
        }

        [Test]
        public void EveryReasonIsInTheBlockIncludingTheOnesAtZero()
        {
            ByDesignTally tally = new ByDesignTally();
            tally.Add("T", "clash 1", ByDesignVerdict.Reviewed,
                From("left_set,right_set,reason\nA,B,a sits on b\n").For("A", "B"));

            string all = string.Join("\n", new List<string>(tally.Lines()).ToArray());

            Assert.That(all, Does.StartWith("REVIEWED clash 1  in T  A and B, a sits on b"));
            Assert.That(all, Does.Contain("clashes looked at : 1"));
            Assert.That(all, Does.Contain("moved to Reviewed : 1"));

            foreach (ByDesignVerdict verdict in ByDesignTally.InOrder())
            {
                if (verdict == ByDesignVerdict.Reviewed)
                {
                    Assert.That(all, Does.Not.Contain("0  " + ByDesignTally.Describe(verdict)));
                    continue;
                }

                Assert.That(all, Does.Contain(ByDesignTally.Describe(verdict)),
                    "a reason missing from the block reads as one nobody thought of");
            }
        }

        [Test]
        public void ARunThatMovedNothingSaysSoRatherThanShowingAnEmptyBlock()
        {
            string all = string.Join("\n", new List<string>(new ByDesignTally().Lines()).ToArray());

            Assert.That(all, Does.Contain("Nothing moved. Every clash is exactly as it was."));
        }

        /// <summary>
        /// One rule lives in one place. Both rules write the same kind of REVIEWED line
        /// and only the WHY differs, so a person scanning a log for REVIEWED finds both.
        /// </summary>
        [Test]
        public void BothRulesWriteTheSameShapeOfReviewedLine()
        {
            string line = ReviewedLine.For("clash 1", "T", "why");

            Assert.That(line, Is.EqualTo("REVIEWED clash 1  in T  why"));
            Assert.That(ReviewedLine.For(null, null, null),
                Is.EqualTo("REVIEWED UNKNOWN  in UNKNOWN  UNKNOWN"));
        }

        [Test]
        public void TheResultLineIsNullWhenTheBoxWasNeverTicked()
        {
            Assert.That(ByDesignTally.ResultLine(false, 47), Is.Null);
            Assert.That(ByDesignTally.ResultLine(true, 47),
                Is.EqualTo("by design     : 47 clashes moved to Reviewed"));
            Assert.That(ByDesignTally.ResultLine(true, 1), Does.Contain("1 clash moved"));
        }

        [Test]
        public void OneGroupRollsIntoARunTotal()
        {
            ByDesignTally run = new ByDesignTally();
            ByDesignTally group = new ByDesignTally();
            group.Add("T", "c", ByDesignVerdict.Reviewed, null);
            group.Add("T", "d", ByDesignVerdict.NotAPair, null);

            run.Add(group);
            run.Add(group);

            Assert.That(run.MovedCount, Is.EqualTo(2));
            Assert.That(run.Of(ByDesignVerdict.NotAPair), Is.EqualTo(2));
            Assert.That(run.Considered, Is.EqualTo(4));
        }

        // ---------- the tick box ----------

        [Test]
        public void TheLabelAndTheGreyLineFitTheirLimits()
        {
            Assert.That(ByDesignPairs.TickLabel, Is.EqualTo("Mark by design connections as Reviewed"));
            Assert.That(ByDesignPairs.TickLabel.Split(' ').Length, Is.LessThanOrEqualTo(8));
            // The brief wrote "From a list file", which is thirteen words. A grey line is
            // twelve at most, so the word that carries least came off.
            Assert.That(ByDesignPairs.HelpLine,
                Is.EqualTo("Column on foundation, door in wall, valve in pipe. From a file"));
            Assert.That(ByDesignPairs.HelpLine.Split(' ').Length, Is.LessThanOrEqualTo(12));
        }

        [Test]
        public void TheBoxIsOffByDefaultBecauseItWritesIntoTheNwf()
        {
            Assert.That(new ReportOptions().MarkByDesign, Is.False);
            Assert.That(new ReportOptions().ByDesignPath, Is.EqualTo(string.Empty));
        }

        private static HashSet<string> SetNamesIn(string path)
        {
            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
            System.Xml.Linq.XDocument document = System.Xml.Linq.XDocument.Load(path);

            foreach (System.Xml.Linq.XElement set in document.Descendants("selectionset"))
            {
                System.Xml.Linq.XAttribute name = set.Attribute("name");

                if (name != null)
                {
                    names.Add(name.Value);
                }
            }

            return names;
        }
    }
}
