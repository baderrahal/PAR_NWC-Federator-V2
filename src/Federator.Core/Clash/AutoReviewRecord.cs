using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Clash
{
    /// <summary>Which rule moved a clash to Reviewed, F72c.</summary>
    public enum AutoReviewRule
    {
        /// <summary>A small service through a wall, a floor, a roof or a structural foundation. F72, Q63.</summary>
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
        /// The words every record of an undo starts with, FR-152. Not the marker of a move,
        /// so a record of an undo is never read as a move to Reviewed.
        /// </summary>
        public const string UndoneMarker = "Parsons NWC Federator put back";

        /// <summary>
        /// The comment an undo leaves when it puts this record's clash back, FR-152, so the
        /// next undo can tell a Reviewed this tool set from one a person set after the undo.
        /// </summary>
        public string UndoneText()
        {
            return UndoneMarker + " [" + Words(Rule) + "] [to " + WasAt + "] by " + UndoAutoReview.ButtonLabel;
        }

        /// <summary>Whether this comment is the record of an undo, FR-152.</summary>
        public static bool IsUndone(string comment)
        {
            return !string.IsNullOrEmpty(comment) && comment.StartsWith(UndoneMarker, StringComparison.Ordinal);
        }

        /// <summary>
        /// The last comment of the clash that is one of ours, a move or an undo, or an empty
        /// string. A comment a person wrote is never one of ours, and the last of ours is the
        /// most recent thing this tool did to the clash, FR-152. Read off the move alone, a
        /// clash an undo had put back still read as moved by this tool, so a Reviewed a person
        /// set after the undo went back by the next one.
        /// </summary>
        public static string LastOfOurs(IList<string> comments)
        {
            string ours = string.Empty;

            if (comments == null)
            {
                return ours;
            }

            foreach (string body in comments)
            {
                if (In(body) != null || IsUndone(body))
                {
                    ours = body;
                }
            }

            return ours;
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

        /// <summary>
        /// The status a record names, read only among the ones a record can carry, F113.
        /// The constructor refuses the other three, so a record naming one of them is a
        /// comment edited by hand and not one of ours. Read among all five, it reached the
        /// constructor and threw out of In, and the undo left every clash of its test alone.
        /// </summary>
        private static bool StatusFrom(string word, out ClashStatus status)
        {
            status = ClashStatus.New;

            foreach (ClashStatus one in StatusesThisToolMayMoveFrom.All())
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
