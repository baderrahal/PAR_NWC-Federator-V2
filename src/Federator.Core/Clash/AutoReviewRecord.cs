using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Clash
{
    /// <summary>Which rule moved a clash to Reviewed, F72c.</summary>
    public enum AutoReviewRule
    {
        /// <summary>A small service through a wall, a floor or a roof. F72.</summary>
        Penetration = 0,

        /// <summary>Two sets named as a by design connection. F72b.</summary>
        ByDesign = 1
    }

    /// <summary>
    /// The record this tool leaves in the NWF when it moves a clash to Reviewed, F72c.
    ///
    /// WHY A RECORD AND NOT A LIST BESIDE THE FILE. The NWF is the record, it travels, it
    /// is opened on other machines, and a side file would be lost the first time somebody
    /// copied the federation somewhere. An undo that cannot tell which clashes this tool
    /// moved is an undo that either does nothing or undoes a person's own decisions, and
    /// both of those are worse than no undo at all.
    ///
    /// IT CARRIES THE STATUS THE CLASH WAS MOVED OFF, not just the fact that it moved. New
    /// and Active are both statuses this tool may move from, and putting an Active clash
    /// back to New would destroy a real difference that somebody had made.
    ///
    /// WHERE IT GOES IS THE MEASUREMENT IN scan.md 5h. If a comment can be written on a
    /// clash result and survives a save and a reopen, the record IS the comment and the
    /// text below is what it says, so a person reading the Clash Detective panel sees the
    /// reason without opening a log. IF IT CANNOT BE DONE, this tool says so in one line
    /// and carries on with the status alone, and NOTHING STANDS IN FOR IT: no side file,
    /// no encoded clash name, no second copy anywhere. An undo that cannot find its record
    /// refuses and says why, which is the honest answer.
    ///
    /// THE TEXT IS FIXED AND MACHINE READABLE ON PURPOSE. It has to be found again in a
    /// week by something that never saw this run, so it starts with a marker no person
    /// would type and carries the rule and the old status in a shape a reader can also
    /// understand at a glance.
    /// </summary>
    public sealed class AutoReviewRecord
    {
        /// <summary>
        /// The words every record starts with. Long enough that nothing a person types
        /// could collide with it, and readable enough that a person who finds one knows
        /// what it is.
        /// </summary>
        public const string Marker = "Parsons NWC Federator auto Reviewed";

        public AutoReviewRecord(AutoReviewRule rule, ClashStatus wasAt, string why)
        {
            if (!StatusesThisToolMayMoveFrom.Allows(wasAt))
            {
                throw new ArgumentOutOfRangeException(
                    "wasAt",
                    "A record can only be written for a status this tool was allowed to move "
                        + "from, which is New or Active. " + StatusesThisToolMayMoveFrom.WhyNot(wasAt));
            }

            Rule = rule;
            WasAt = wasAt;
            Why = why ?? string.Empty;
        }

        /// <summary>Which rule moved it.</summary>
        public AutoReviewRule Rule { get; private set; }

        /// <summary>What the clash was at before this tool moved it.</summary>
        public ClashStatus WasAt { get; private set; }

        /// <summary>Why, in the words the rule gave.</summary>
        public string Why { get; private set; }

        /// <summary>The words for one rule, so the record and the log cannot drift.</summary>
        public static string Words(AutoReviewRule rule)
        {
            switch (rule)
            {
                case AutoReviewRule.Penetration: return "penetration";
                case AutoReviewRule.ByDesign: return "by design";
                default: return "UNKNOWN";
            }
        }

        /// <summary>The rule that word names, or null where it names none.</summary>
        public static AutoReviewRule? RuleFrom(string word)
        {
            if (string.Equals(word, Words(AutoReviewRule.Penetration), StringComparison.Ordinal))
            {
                return AutoReviewRule.Penetration;
            }

            if (string.Equals(word, Words(AutoReviewRule.ByDesign), StringComparison.Ordinal))
            {
                return AutoReviewRule.ByDesign;
            }

            return null;
        }

        /// <summary>
        /// The comment text. The marker, the rule and the old status first, because those
        /// three are what the undo reads, then the reason in plain words for the person.
        /// </summary>
        public string Text()
        {
            return Marker + " [" + Words(Rule) + "] [was " + WasAt + "] " + Why;
        }

        /// <summary>
        /// The record inside that comment, or null where the comment is not one of ours.
        /// A comment a PERSON wrote is never one of ours, however it reads, because it
        /// will not carry the marker and the two brackets in that order.
        /// </summary>
        public static AutoReviewRecord In(string comment)
        {
            if (string.IsNullOrEmpty(comment)
                || !comment.StartsWith(Marker, StringComparison.Ordinal))
            {
                return null;
            }

            string rest = comment.Substring(Marker.Length);
            string ruleWord;
            string statusWord;
            int after;

            if (!Bracketed(rest, 0, out ruleWord, out after)
                || !Bracketed(rest, after, out statusWord, out after))
            {
                return null;
            }

            AutoReviewRule? rule = RuleFrom(ruleWord);

            if (rule == null || !statusWord.StartsWith("was ", StringComparison.Ordinal))
            {
                return null;
            }

            ClashStatus wasAt;

            if (!StatusFrom(statusWord.Substring(4), out wasAt))
            {
                return null;
            }

            return new AutoReviewRecord(rule.Value, wasAt, rest.Substring(after).TrimStart());
        }

        /// <summary>
        /// Whether an undo may touch this clash. It carries one of our records AND it is
        /// still at Reviewed.
        ///
        /// THE SECOND HALF IS THE ONE THAT MATTERS. A clash this tool set to Reviewed in
        /// week one and a person moved to Approved in week two still carries the record,
        /// and pulling it back to New would throw away that person's decision, which is
        /// the one thing this tool never does.
        /// </summary>
        public static bool MayUndo(string comment, ClashStatus now)
        {
            return now == ClashStatus.Reviewed && In(comment) != null;
        }

        /// <summary>
        /// Why an undo left a clash alone, or null where it may move it. Said in the
        /// words the block carries, so a person reading it knows whether to look.
        /// </summary>
        public static string WhyNotUndone(string comment, ClashStatus now)
        {
            if (In(comment) == null)
            {
                return "this tool did not move it, so there is nothing of ours to undo";
            }

            if (now != ClashStatus.Reviewed)
            {
                return "this tool moved it and somebody has since moved it to " + now
                    + ", so that decision stands";
            }

            return null;
        }

        private static bool Bracketed(string text, int from, out string inside, out int after)
        {
            inside = string.Empty;
            after = from;

            int open = text.IndexOf('[', from);

            if (open < 0)
            {
                return false;
            }

            int close = text.IndexOf(']', open + 1);

            if (close < 0)
            {
                return false;
            }

            inside = text.Substring(open + 1, close - open - 1);
            after = close + 1;
            return true;
        }

        private static bool StatusFrom(string word, out ClashStatus status)
        {
            status = ClashStatus.New;

            foreach (ClashStatus one in ClashTally.AllStatuses)
            {
                if (string.Equals(one.ToString(), word, StringComparison.Ordinal))
                {
                    status = one;
                    return true;
                }
            }

            return false;
        }
    }
}
