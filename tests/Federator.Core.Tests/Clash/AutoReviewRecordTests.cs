using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F72c. Say why inside Navisworks and be able to undo it.
    ///
    /// The record goes in the NWF, because the NWF travels and a side file would be lost
    /// the first time somebody copied the federation. Whether a comment can be written on
    /// a clash result at all is scan.md 5h and is not measured. If it cannot be done, this
    /// tool says so in one line and carries on with the status alone, and NOTHING stands
    /// in for it.
    /// </summary>
    [TestFixture]
    public class AutoReviewRecordTests
    {
        private static AutoReviewRecord APenetration()
        {
            return new AutoReviewRecord(
                AutoReviewRule.Penetration, ClashStatus.New, "Pipes 100mm through Walls");
        }

        // ---------- what the record says ----------

        [Test]
        public void TheRecordCarriesTheRuleTheOldStatusAndTheReason()
        {
            string text = APenetration().Text();

            Assert.That(text, Does.StartWith(AutoReviewRecord.Marker));
            Assert.That(text, Does.Contain("[penetration]"));
            Assert.That(text, Does.Contain("[was New]"));
            Assert.That(text, Does.Contain("Pipes 100mm through Walls"));
        }

        /// <summary>
        /// New and Active are both statuses this tool may move from, so the record has to
        /// carry which one. An undo that put everything back to New would destroy a real
        /// difference somebody had made.
        /// </summary>
        [Test]
        public void TheRecordTellsNewApartFromActive()
        {
            AutoReviewRecord fromNew = new AutoReviewRecord(AutoReviewRule.ByDesign, ClashStatus.New, "why");
            AutoReviewRecord fromActive = new AutoReviewRecord(AutoReviewRule.ByDesign, ClashStatus.Active, "why");

            Assert.That(AutoReviewRecord.In(fromNew.Text()).WasAt, Is.EqualTo(ClashStatus.New));
            Assert.That(AutoReviewRecord.In(fromActive.Text()).WasAt, Is.EqualTo(ClashStatus.Active));
        }

        [Test]
        public void ARecordCannotBeWrittenForAStatusThisToolMayNotMoveFrom()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { new AutoReviewRecord(AutoReviewRule.ByDesign, ClashStatus.Approved, "why"); });
            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { new AutoReviewRecord(AutoReviewRule.ByDesign, ClashStatus.Resolved, "why"); });
            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { new AutoReviewRecord(AutoReviewRule.ByDesign, ClashStatus.Reviewed, "why"); });
        }

        [Test]
        public void ARecordReadsBackAsExactlyWhatWentIn()
        {
            foreach (AutoReviewRule rule in new[] { AutoReviewRule.Penetration, AutoReviewRule.ByDesign })
            {
                foreach (ClashStatus was in StatusesThisToolMayMoveFrom.All())
                {
                    AutoReviewRecord went = new AutoReviewRecord(rule, was, "a reason [with] brackets");
                    AutoReviewRecord back = AutoReviewRecord.In(went.Text());

                    Assert.That(back, Is.Not.Null, went.Text());
                    Assert.That(back.Rule, Is.EqualTo(rule));
                    Assert.That(back.WasAt, Is.EqualTo(was));
                    Assert.That(back.Why, Is.EqualTo("a reason [with] brackets"));
                }
            }
        }

        // ---------- what is not one of ours ----------

        /// <summary>
        /// The break. A comment a PERSON wrote is never one of ours, however it reads,
        /// because an undo that reads a person's comment as its own record would put back
        /// a decision that person made.
        /// </summary>
        [Test]
        public void ACommentAPersonWroteIsNeverOneOfOurs()
        {
            string[] theirs =
            {
                "reviewed this, it is fine",
                "auto Reviewed",
                "[penetration] [was New] looks like ours and is not",
                "Parsons NWC Federator auto reviewed [penetration] [was New] wrong case",
                string.Empty,
                null
            };

            foreach (string comment in theirs)
            {
                Assert.That(AutoReviewRecord.In(comment), Is.Null, "\"" + comment + "\"");
            }
        }

        [Test]
        public void ARecordWithARuleOrAStatusNobodyKnowsIsNotOneOfOurs()
        {
            Assert.That(AutoReviewRecord.In(
                AutoReviewRecord.Marker + " [something else] [was New] why"), Is.Null);
            Assert.That(AutoReviewRecord.In(
                AutoReviewRecord.Marker + " [penetration] [was Nonsense] why"), Is.Null);
            Assert.That(AutoReviewRecord.In(
                AutoReviewRecord.Marker + " [penetration] why"), Is.Null);
            Assert.That(AutoReviewRecord.In(AutoReviewRecord.Marker), Is.Null);
        }

        // ---------- the undo ----------

        [Test]
        public void AClashThisToolMovedAndNobodyTouchedIsPutBack()
        {
            ClashStatus back;

            Assert.That(UndoAutoReview.Judge(APenetration().Text(), ClashStatus.Reviewed, out back),
                Is.EqualTo(UndoVerdict.PutBack));
            Assert.That(back, Is.EqualTo(ClashStatus.New));
            Assert.That(AutoReviewRecord.MayUndo(APenetration().Text(), ClashStatus.Reviewed), Is.True);
        }

        [Test]
        public void AClashThisToolNeverMovedIsLeftAlone()
        {
            ClashStatus back;

            Assert.That(UndoAutoReview.Judge("somebody's own note", ClashStatus.Reviewed, out back),
                Is.EqualTo(UndoVerdict.NotOurs));
            Assert.That(back, Is.EqualTo(ClashStatus.Reviewed), "nothing is moved");
            Assert.That(AutoReviewRecord.WhyNotUndone("somebody's own note", ClashStatus.Reviewed),
                Does.Contain("nothing of ours to undo"));
        }

        /// <summary>
        /// The one that matters. A clash this tool set to Reviewed in week one and a
        /// person moved to Approved in week two still carries the record, and pulling it
        /// back to New would throw that person's decision away.
        /// </summary>
        [Test]
        public void AClashSomebodyHasSinceMovedOnIsLeftAlone()
        {
            foreach (ClashStatus now in new[] { ClashStatus.Approved, ClashStatus.Resolved,
                ClashStatus.New, ClashStatus.Active })
            {
                ClashStatus back;

                Assert.That(UndoAutoReview.Judge(APenetration().Text(), now, out back),
                    Is.EqualTo(UndoVerdict.SomebodyMovedItOn), now.ToString());
                Assert.That(AutoReviewRecord.MayUndo(APenetration().Text(), now), Is.False);
            }

            Assert.That(AutoReviewRecord.WhyNotUndone(APenetration().Text(), ClashStatus.Approved),
                Does.Contain("that decision stands"));
        }

        // ---------- what an undo may set ----------

        /// <summary>
        /// An undo invents nothing. It puts a clash back to the exact status one of this
        /// tool's own records names, and refuses anything else, so the rule that Reviewed
        /// is the only status this tool sets is still true of everything that is not an
        /// undo.
        /// </summary>
        [Test]
        public void AnUndoMaySetOnlyTheStatusItsOwnRecordNames()
        {
            AutoReviewRecord record = new AutoReviewRecord(
                AutoReviewRule.ByDesign, ClashStatus.Active, "why");

            Assert.That(StatusesThisToolMaySet.AllowsAsUndo(ClashStatus.Active, record), Is.True);
            Assert.That(StatusesThisToolMaySet.AllowsAsUndo(ClashStatus.New, record), Is.False);
            Assert.That(StatusesThisToolMaySet.AllowsAsUndo(ClashStatus.Approved, record), Is.False);
            Assert.That(StatusesThisToolMaySet.AllowsAsUndo(ClashStatus.Active, null), Is.False);

            Assert.That(StatusesThisToolMaySet.WhyNotAsUndo(ClashStatus.New, record),
                Does.Contain("puts a clash back where it was"));
            Assert.That(StatusesThisToolMaySet.WhyNotAsUndo(ClashStatus.New, null),
                Does.Contain("nothing to undo"));
        }

        [Test]
        public void TheOrdinaryRuleIsUnchangedAndReviewedIsStillTheOnlyStatusSet()
        {
            Assert.That(StatusesThisToolMaySet.All(),
                Is.EqualTo(new[] { ClashStatus.Reviewed }).AsCollection);
            Assert.That(StatusesThisToolMaySet.Allows(ClashStatus.New), Is.False);
            Assert.That(StatusesThisToolMaySet.Allows(ClashStatus.Active), Is.False);
            Assert.That(StatusesThisToolMaySet.Allows(ClashStatus.Approved), Is.False);
        }

        // ---------- the block ----------

        [Test]
        public void EveryReasonIsInTheBlockIncludingTheOnesAtZero()
        {
            UndoTally tally = new UndoTally();
            tally.Add("T", "clash 1", UndoVerdict.PutBack, ClashStatus.Active);

            string all = string.Join("\n", new List<string>(tally.Lines()).ToArray());

            Assert.That(all, Does.StartWith("UNDO clash 1  in T  back to Active"));
            Assert.That(all, Does.Contain("clashes looked at : 1"));
            Assert.That(all, Does.Contain("put back          : 1"));

            foreach (UndoVerdict verdict in UndoAutoReview.InOrder())
            {
                if (verdict == UndoVerdict.PutBack)
                {
                    continue;
                }

                Assert.That(all, Does.Contain(UndoAutoReview.Describe(verdict)));
            }
        }

        [Test]
        public void AnUndoThatPutNothingBackSaysSo()
        {
            string all = string.Join("\n", new List<string>(new UndoTally().Lines()).ToArray());

            Assert.That(all, Does.Contain("Nothing was put back. Every clash is exactly as it was."));
        }

        /// <summary>
        /// If a comment cannot be written there is no record, so there is nothing to undo.
        /// The button says that in one line and NOTHING stands in for the record: no side
        /// file, no encoded clash name, no second copy anywhere.
        /// </summary>
        [Test]
        public void WithNoRecordAnywhereTheButtonSaysSoInOneLineAndFakesNothing()
        {
            string line = UndoAutoReview.CannotLine(
                "the Navisworks clash API has no writable comment on a result");

            Assert.That(line, Does.StartWith("UNDO no clash carries a record"));
            Assert.That(line, Does.Contain("no writable comment"));
            Assert.That(line, Does.Contain("nothing stands in for the record"));
            Assert.That(UndoAutoReview.CannotLine(null), Does.Contain("UNKNOWN"));
        }

        [Test]
        public void TheButtonLabelAndGreyLineFitTheirLimits()
        {
            Assert.That(UndoAutoReview.ButtonLabel, Is.EqualTo("Undo auto Reviewed"));
            Assert.That(UndoAutoReview.HelpLine.Split(' ').Length, Is.LessThanOrEqualTo(12));
            Assert.That(UndoAutoReview.HelpLine, Does.Not.Contain("AutoReviewRecord"),
                "no code identifier ever reaches a label");
        }
    }
}
