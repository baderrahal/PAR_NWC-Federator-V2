using System;
using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>Why one clash was or was not put back, F72c.</summary>
    public enum UndoVerdict
    {
        /// <summary>Put back where this tool found it.</summary>
        PutBack = 0,

        /// <summary>This tool never moved it, so there is nothing of ours to undo.</summary>
        NotOurs = 1,

        /// <summary>This tool moved it and somebody has since moved it again, on or back. That stands.</summary>
        SomebodyMovedItOn = 2
    }

    /// <summary>
    /// The Undo auto Reviewed button, F72c. It puts back the clashes THIS TOOL moved to
    /// Reviewed, and nothing else.
    ///
    /// IT READS THE RECORD IN THE NWF AND NEVER A LIST OF ITS OWN. The record travels with
    /// the file, so an undo works on a machine that never saw the run that moved them, and
    /// a federation copied somewhere else still knows which of its clashes were moved by a
    /// rule and which by a person.
    ///
    /// IT PUTS EACH CLASH BACK WHERE IT WAS. New and Active are both statuses this tool
    /// may move from, so the record carries which one, and an undo that put everything
    /// back to New would destroy a real difference somebody had made.
    ///
    /// IT REFUSES A CLASH SOMEBODY HAS SINCE MOVED ON. A clash this tool set to Reviewed
    /// in week one and a person moved to Approved in week two still carries the record,
    /// and pulling it back to New would throw that person's decision away.
    ///
    /// IF NO RECORD CAN BE WRITTEN THERE IS NOTHING TO UNDO, and the button says that in
    /// one line rather than doing something that looks like an undo. That is scan.md 5h.
    /// </summary>
    public static class UndoAutoReview
    {
        /// <summary>The words on the button. Three, and a button is not a tick box.</summary>
        public const string ButtonLabel = "Undo auto Reviewed";

        /// <summary>The grey line under it. Twelve words, which is the limit.</summary>
        public const string HelpLine =
            "Puts back only the clashes this tool moved, each where it was";

        /// <summary>The words that begin every line this rule writes.</summary>
        public const string Prefix = "UNDO";

        /// <summary>
        /// What to do with one clash, and the status to put it back to. The status is only
        /// meaningful where the verdict is PutBack.
        /// </summary>
        public static UndoVerdict Judge(string comment, ClashStatus now, out ClashStatus putBackTo)
        {
            putBackTo = now;
            AutoReviewRecord record = AutoReviewRecord.In(comment);

            if (record == null)
            {
                return UndoVerdict.NotOurs;
            }

            if (now != ClashStatus.Reviewed)
            {
                return UndoVerdict.SomebodyMovedItOn;
            }

            putBackTo = record.WasAt;
            return UndoVerdict.PutBack;
        }

        /// <summary>The words for one verdict, so the block and nothing else spells them.</summary>
        public static string Describe(UndoVerdict verdict)
        {
            switch (verdict)
            {
                case UndoVerdict.PutBack:
                    return "put back where this tool found it";
                case UndoVerdict.NotOurs:
                    return "this tool never moved it, so there was nothing of ours to undo";
                case UndoVerdict.SomebodyMovedItOn:
                    return "this tool moved it and somebody has since moved it again, so that stands";
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>The three in the order the block lists them.</summary>
        public static UndoVerdict[] InOrder()
        {
            return new[] { UndoVerdict.PutBack, UndoVerdict.NotOurs, UndoVerdict.SomebodyMovedItOn };
        }

        /// <summary>
        /// The one line for a run that could not write a record in the first place, F72c.
        /// Said once, plainly, and NOTHING stands in for the comment: no side file, no
        /// encoded clash name, no second copy anywhere. A comment that could not be written
        /// is not a comment.
        /// </summary>
        public static string CannotLine(string whyTheApiCannot)
        {
            return Prefix + " no clash carries a record of this tool moving it, because "
                + (string.IsNullOrEmpty(whyTheApiCannot) ? "UNKNOWN" : whyTheApiCannot)
                + ". Nothing was put back and nothing stands in for the record";
        }
    }

    /// <summary>What one undo pass did, and the block that says so, F72c.</summary>
    public sealed class UndoTally
    {
        private readonly List<string> putBack = new List<string>();
        private readonly Dictionary<UndoVerdict, int> counts = new Dictionary<UndoVerdict, int>();

        public int Considered { get; private set; }

        public int Of(UndoVerdict verdict)
        {
            return counts.ContainsKey(verdict) ? counts[verdict] : 0;
        }

        public int PutBackCount
        {
            get { return Of(UndoVerdict.PutBack); }
        }

        public void Add(string testName, string clashName, UndoVerdict verdict, ClashStatus putBackTo)
        {
            Considered++;

            if (!counts.ContainsKey(verdict))
            {
                counts[verdict] = 0;
            }

            counts[verdict]++;

            if (verdict != UndoVerdict.PutBack)
            {
                return;
            }

            putBack.Add(UndoAutoReview.Prefix + " " + Words(clashName) + "  in " + Words(testName)
                + "  back to " + putBackTo);
        }

        /// <summary>
        /// The UNDO block. One line per clash put back, then the totals with one line per
        /// reason including the ones at zero, which is how every other block here reads.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>(putBack);

            lines.Add(string.Empty);
            lines.Add("clashes looked at : " + Considered);
            lines.Add("put back          : " + PutBackCount);

            foreach (UndoVerdict verdict in UndoAutoReview.InOrder())
            {
                if (verdict == UndoVerdict.PutBack)
                {
                    continue;
                }

                lines.Add("    " + Of(verdict).ToString().PadLeft(5) + "  "
                    + UndoAutoReview.Describe(verdict));
            }

            if (PutBackCount == 0)
            {
                lines.Add("Nothing was put back. Every clash is exactly as it was.");
            }

            return lines;
        }

        private static string Words(string value)
        {
            return string.IsNullOrEmpty(value) ? "UNKNOWN" : value;
        }
    }
}
