using System;
using System.Collections.Generic;
using System.Globalization;
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

    /// <summary>
    /// One set whose whole question is replaced with a catch all: its category CONTAINS
    /// one value and is NOT each of the named ones. That is how a set is written where
    /// its siblings already claim the named values and it is meant to hold the rest.
    ///
    /// WHY THE NEGATION IS WRITTEN AT ALL, and why it was not until now. The exchange
    /// file's flags attribute is Navisworks' SearchConditionOptions and NegateCondition
    /// is 32, F78, but whether a negated condition actually finds the right items and
    /// survives the file was not measured until 2026-09-20, docs\history\scan.md 5g. It
    /// does, exactly: on the 1A02MM federation category contains Devices found 67 items,
    /// and with one negated equals beside it the count came down by exactly the number
    /// that named category holds, for all six. The fallback this replaces asked for one
    /// named value and found ZERO items in that building.
    ///
    /// A NEGATION NEEDS A POSITIVE BESIDE IT. 5g measured a negated condition ON ITS OWN
    /// matching the four model roots and nothing under them, because a root carries no
    /// category at all and the search prunes below a match. So this writes the contains
    /// first and the negations after it, and never a negation alone.
    /// </summary>
    public sealed class ConditionsRewrite
    {
        public ConditionsRewrite(string setName, string contains, IList<string> excluded)
            : this(setName, contains, excluded, null)
        {
        }

        public ConditionsRewrite(string setName, string contains, IList<string> excluded, string note)
        {
            SetName = setName;
            Contains = contains;
            Excluded = excluded ?? new List<string>();
            Note = note;
        }

        /// <summary>The set whose conditions are replaced, and no other.</summary>
        public string SetName { get; private set; }

        /// <summary>The value the category must contain.</summary>
        public string Contains { get; private set; }

        /// <summary>The values the category must not equal, one negated condition each.</summary>
        public IList<string> Excluded { get; private set; }

        /// <summary>What a reader should know about the change, carried into the outcome line.</summary>
        public string Note { get; private set; }
    }

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
            return Apply(xml, renames, rewrites, null);
        }

        public static CorrectionOutcome Apply(
            string xml,
            IList<SetRename> renames,
            IList<CategoryRewrite> rewrites,
            IList<ConditionsRewrite> conditions)
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

            if (conditions != null)
            {
                foreach (ConditionsRewrite rewrite in conditions)
                {
                    if (rewrite == null)
                    {
                        continue;
                    }

                    string why;
                    int changed;
                    text = RewriteConditions(text, rewrite, out changed, out why);

                    outcome.Add(
                        rewrite.SetName + " asks for a category holding " + rewrite.Contains
                            + " and none of the " + rewrite.Excluded.Count + " its siblings claim",
                        changed,
                        Both(why, rewrite.Note));
                }
            }

            outcome.Text = text;
            return outcome;
        }

        /// <summary>
        /// Replaces one named set's conditions with a contains and one negated equals per
        /// excluded value, built from the set's OWN first condition as the template, so
        /// nothing here has to know what a category or a property is called on this
        /// project. Safe to run twice: a set already carrying the built block is left
        /// alone and counted as zero.
        /// </summary>
        private static string RewriteConditions(string text, ConditionsRewrite rewrite, out int changed, out string why)
        {
            changed = 0;
            why = null;

            int at = text.IndexOf(SetOpens + rewrite.SetName + "\"", StringComparison.Ordinal);

            if (at < 0)
            {
                why = "this file holds no set of that name";
                return text;
            }

            int ends = text.IndexOf(SetCloses, at, StringComparison.Ordinal);

            if (ends < 0)
            {
                why = "that set never closes, so it was left alone";
                return text;
            }

            ends += SetCloses.Length;
            string block = text.Substring(at, ends - at);

            int open = block.IndexOf(ConditionsOpen, StringComparison.Ordinal);
            int close = block.IndexOf(ConditionsClose, StringComparison.Ordinal);

            if (open < 0 || close < 0 || close < open)
            {
                why = "that set carries no conditions to replace";
                return text;
            }

            open += ConditionsOpen.Length;
            string inner = block.Substring(open, close - open);
            string template = FirstCondition(inner);

            if (template == null)
            {
                why = "that set carries no condition to copy the category and property from";
                return text;
            }

            System.Text.StringBuilder built = new System.Text.StringBuilder();
            built.Append(OneCondition(template, "contains", 0, rewrite.Contains));

            foreach (string excluded in rewrite.Excluded)
            {
                built.Append(OneCondition(template, "equals", NegateCondition, excluded));
            }

            string want = built.ToString();

            if (string.Equals(inner, want, StringComparison.Ordinal))
            {
                why = "that set already asks exactly this, so it was left alone";
                return text;
            }

            changed = 1 + rewrite.Excluded.Count;
            string fixedBlock = block.Substring(0, open) + want + block.Substring(close);
            return text.Substring(0, at) + fixedBlock + text.Substring(ends);
        }

        /// <summary>The first whole condition element inside a conditions block, with the newline that precedes it.</summary>
        private static string FirstCondition(string inner)
        {
            int opens = inner.IndexOf(ConditionOpens, StringComparison.Ordinal);

            if (opens < 0)
            {
                return null;
            }

            int closes = inner.IndexOf(ConditionCloses, opens, StringComparison.Ordinal);

            if (closes < 0)
            {
                return null;
            }

            closes += ConditionCloses.Length;

            // Back to the start of the line the condition opens on, so the indentation
            // this file uses is carried rather than invented, and the block it builds
            // reads the way a person wrote the rest of the file.
            int line = inner.LastIndexOf('\n', opens);
            int from = line < 0 ? 0 : line;

            return inner.Substring(from, closes - from);
        }

        /// <summary>One condition built off the template, differing in the test, the flags and the value.</summary>
        private static string OneCondition(string template, string test, int flags, string value)
        {
            string one = ReplaceOpeningTag(
                template,
                ConditionOpens,
                "<condition test=\"" + test + "\" flags=\"" + flags.ToString(CultureInfo.InvariantCulture) + "\">");

            return ReplaceInnerText(one, DataOpens, "</data>", value);
        }

        /// <summary>The whole of the tag that starts with that opening, replaced.</summary>
        private static string ReplaceOpeningTag(string text, string opens, string with)
        {
            int at = text.IndexOf(opens, StringComparison.Ordinal);

            if (at < 0)
            {
                return text;
            }

            int shut = text.IndexOf('>', at);

            if (shut < 0)
            {
                return text;
            }

            return text.Substring(0, at) + with + text.Substring(shut + 1);
        }

        /// <summary>What sits between that opening tag and its closing tag, replaced.</summary>
        private static string ReplaceInnerText(string text, string opens, string closes, string with)
        {
            int at = text.IndexOf(opens, StringComparison.Ordinal);

            if (at < 0)
            {
                return text;
            }

            int shut = text.IndexOf('>', at);
            int end = text.IndexOf(closes, at, StringComparison.Ordinal);

            if (shut < 0 || end < 0 || end < shut)
            {
                return text;
            }

            return text.Substring(0, shut + 1) + with + text.Substring(end);
        }

        /// <summary>Two sentences where there are two, one where there is one, and null where there are none.</summary>
        private static string Both(string why, string note)
        {
            if (string.IsNullOrEmpty(note))
            {
                return why;
            }

            return string.IsNullOrEmpty(why) ? note : why + ". " + note;
        }

        /// <summary>NegateCondition in Navisworks' SearchConditionOptions, which the file's flags attribute is, F78 and 5g.</summary>
        public const int NegateCondition = 32;

        private const string ConditionsOpen = "<conditions>";
        private const string ConditionsClose = "</conditions>";
        private const string ConditionOpens = "<condition ";
        private const string ConditionCloses = "</condition>";
        private const string DataOpens = "<data ";

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
