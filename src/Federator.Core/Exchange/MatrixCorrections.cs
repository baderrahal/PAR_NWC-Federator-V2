using System;
using System.Collections.Generic;
using System.Text;

namespace Federator.Core.Exchange
{
    /// <summary>One name that is wrong everywhere it appears in an exchange file.</summary>
    public sealed class SetRename
    {
        public SetRename(string from, string to)
        {
            if (string.IsNullOrEmpty(from))
            {
                throw new ArgumentException("A rename needs a name to look for.", "from");
            }

            if (string.IsNullOrEmpty(to))
            {
                throw new ArgumentException("A rename needs a name to put in its place.", "to");
            }

            // THE IDEMPOTENCY LAW, refused here rather than discovered on the second run.
            // A rename whose new name still holds the old one grows the file every time it
            // is applied, so BLD-X to BLD-BLD-X is not a correction, it is a loop.
            if (to.IndexOf(from, StringComparison.Ordinal) >= 0)
            {
                throw new ArgumentException(
                    "\"" + to + "\" still holds \"" + from + "\", so applying this twice would "
                        + "rename it again. A correction has to be safe to run on its own output.",
                    "to");
            }

            From = from;
            To = to;
        }

        public string From { get; private set; }

        public string To { get; private set; }
    }

    /// <summary>
    /// One set asking for the wrong category value. The set is named, so a value that
    /// other sets ask for legitimately is left alone in those sets.
    /// </summary>
    public sealed class CategoryRewrite
    {
        public CategoryRewrite(string setName, string from, string to)
            : this(setName, from, to, null)
        {
        }

        /// <summary>The same, with a note saying why this rewrite is what it is, A15, carried into the outcome line.</summary>
        public CategoryRewrite(string setName, string from, string to, string note)
        {
            if (string.IsNullOrEmpty(setName))
            {
                throw new ArgumentException("A rewrite has to say which set it is for.", "setName");
            }

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                throw new ArgumentException("A rewrite needs a value to look for and one to put in.", "from");
            }

            SetName = setName;
            From = from;
            To = to;
            Note = note ?? string.Empty;
        }

        public string SetName { get; private set; }

        public string From { get; private set; }

        public string To { get; private set; }

        /// <summary>Why this rewrite is what it is, or empty. Said on the outcome line so a person reading a run knows which form a set carries.</summary>
        public string Note { get; private set; }
    }

    /// <summary>What one correction changed, counted rather than assumed.</summary>
    public sealed class CorrectionCount
    {
        public CorrectionCount(string what, int count, string note)
        {
            What = what;
            Count = count;
            Note = note;
        }

        public string What { get; private set; }

        public int Count { get; private set; }

        /// <summary>Why a count is zero, where zero needs explaining. Null otherwise.</summary>
        public string Note { get; private set; }

        public string Line()
        {
            return "MATRIX   " + What + "  " + Count
                + (Count == 1 ? " occurrence" : " occurrences")
                + (string.IsNullOrEmpty(Note) ? string.Empty : ". " + Note);
        }
    }

    /// <summary>The corrected text and what it took to get there.</summary>
    public sealed class CorrectionOutcome
    {
        private readonly List<CorrectionCount> counts = new List<CorrectionCount>();

        internal CorrectionOutcome(string text)
        {
            Text = text;
        }

        public string Text { get; internal set; }

        public IList<CorrectionCount> Counts
        {
            get { return new List<CorrectionCount>(counts); }
        }

        internal void Add(string what, int count, string note)
        {
            counts.Add(new CorrectionCount(what, count, note));
        }

        /// <summary>
        /// Everything that changed, added up. ZERO is the answer that proves a correction
        /// has already been applied, which is why it is worth reporting rather than hiding.
        /// </summary>
        public int TotalChanged
        {
            get
            {
                int total = 0;

                foreach (CorrectionCount one in counts)
                {
                    total += one.Count;
                }

                return total;
            }
        }

        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            foreach (CorrectionCount one in counts)
            {
                lines.Add(one.Line());
            }

            lines.Add(TotalChanged == 0
                ? "MATRIX   nothing changed, so this file already carries every correction"
                : "MATRIX   " + TotalChanged + " changes in all");

            return lines;
        }
    }

    /// <summary>
    /// Corrections applied to an exchange file before it is used.
    ///
    /// WHY THIS IS GENERIC AND NAMES NO SET. CLAUDE.md says nothing in the code names any
    /// one project's file, and that a name off the clash XML appears in tests as sample
    /// data only. The corrections this project needs are therefore DATA handed in, and
    /// what lives here is only the rule for applying them safely. A second project with a
    /// different matrix hands in a different list and needs no code change.
    ///
    /// SAFE TO RUN TWICE IS THE WHOLE POINT. A correction that is applied to its own
    /// output must change nothing the second time, and TotalChanged coming back zero is
    /// how that is proved rather than argued. A rename whose new name still holds the old
    /// one is refused where it is built, because that one grows the file on every pass.
    ///
    /// A REWRITE IS SCOPED TO ITS SET. The same category value is asked for legitimately by
    /// other sets, so a rewrite that replaced it everywhere would break the sets that were
    /// right. It finds the named set and changes only what is inside it.
    /// </summary>
    public static class MatrixCorrections
    {
        /// <summary>How a set opens in an exchange file.</summary>
        private const string SetOpens = "<selectionset name=\"";

        private const string SetCloses = "</selectionset>";

        /// <summary>
        /// The corrected text, with one count per correction. Never throws over a
        /// correction that finds nothing: a set that is not in this file is reported and
        /// the rest are still applied, which is the rule this tool keeps everywhere.
        /// </summary>
        public static CorrectionOutcome Apply(
            string xml, IList<SetRename> renames, IList<CategoryRewrite> rewrites)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            CorrectionOutcome outcome = new CorrectionOutcome(xml);
            string text = xml;

            if (renames != null)
            {
                foreach (SetRename rename in renames)
                {
                    if (rename == null)
                    {
                        continue;
                    }

                    int found = Occurrences(text, rename.From);
                    text = text.Replace(rename.From, rename.To);

                    outcome.Add(
                        rename.From + " to " + rename.To,
                        found,
                        found == 0 ? "this file does not hold that name" : null);
                }
            }

            if (rewrites != null)
            {
                foreach (CategoryRewrite rewrite in rewrites)
                {
                    if (rewrite == null)
                    {
                        continue;
                    }

                    int changed;
                    text = Rewrite(text, rewrite, out changed);

                    outcome.Add(
                        rewrite.SetName + " asks for " + rewrite.To + " and not " + rewrite.From,
                        changed,
                        Noted(Why(changed, text, rewrite), rewrite));
                }
            }

            outcome.Text = text;
            return outcome;
        }

        /// <summary>
        /// The value inside one named set, and inside no other. Where the set is not there
        /// at all, nothing changes and the count is zero.
        /// </summary>
        private static string Rewrite(string text, CategoryRewrite rewrite, out int changed)
        {
            changed = 0;

            int at = text.IndexOf(SetOpens + rewrite.SetName + "\"", StringComparison.Ordinal);

            if (at < 0)
            {
                return text;
            }

            int ends = text.IndexOf(SetCloses, at, StringComparison.Ordinal);

            if (ends < 0)
            {
                return text;
            }

            ends += SetCloses.Length;
            string block = text.Substring(at, ends - at);
            changed = Occurrences(block, rewrite.From);

            if (changed == 0)
            {
                return text;
            }

            return text.Substring(0, at) + block.Replace(rewrite.From, rewrite.To) + text.Substring(ends);
        }

        /// <summary>
        /// The why and the rewrite's own note together, A15, either on its own where the
        /// other is empty, so the outcome line says both what happened and which form the
        /// set carries.
        /// </summary>
        private static string Noted(string why, CategoryRewrite rewrite)
        {
            string note = rewrite == null ? string.Empty : rewrite.Note;

            if (string.IsNullOrEmpty(note))
            {
                return why;
            }

            return string.IsNullOrEmpty(why) ? note : why + ". " + note;
        }

        /// <summary>
        /// Why a rewrite changed nothing, in words, so a zero is never left to be guessed
        /// at. Null where it changed something.
        /// </summary>
        private static string Why(int changed, string text, CategoryRewrite rewrite)
        {
            if (changed > 0)
            {
                return null;
            }

            if (text.IndexOf(SetOpens + rewrite.SetName + "\"", StringComparison.Ordinal) < 0)
            {
                return "there is no set of that name in this file";
            }

            return "the set is there and already asks for something else";
        }

        private static int Occurrences(string text, string what)
        {
            int count = 0;
            int at = text.IndexOf(what, StringComparison.Ordinal);

            while (at >= 0)
            {
                count++;
                at = text.IndexOf(what, at + what.Length, StringComparison.Ordinal);
            }

            return count;
        }
    }
}
