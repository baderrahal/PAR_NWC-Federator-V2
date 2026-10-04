using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Federator.Core.Sets;

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


    /// <summary>
    /// One property VALUE in the matrix corrected to the spelling the models actually
    /// carry, Q68 answered a on 2026-09-20.
    ///
    /// WHY THIS EXISTS. The client's matrix asks for a workset called `ME-DUCTWORK` and
    /// every model in the project carries `ME-Ductwork`. A search condition compares a
    /// value CASE SENSITIVELY unless `IgnoreDisplayStringValueCase` is set, nothing sets
    /// it, and so three mechanical sets found nothing in every group of every run since
    /// the tool was first pointed at this project, 5q.
    ///
    /// IT IS NOT AN IGNORE CASE FLAG AND IT NEVER SETS ONE. Bader refused that on
    /// 2026-09-20 and the reason stands: the flag would also make two genuinely different
    /// worksets match, quietly, on every set in the file, and `AR-EXTERIOR` against
    /// `AR-INTERIOR` and `ST-SUB` against `ST-SUP` are two pairs of real worksets in this
    /// very project that are one and two letters apart, 5t.
    ///
    /// SO IT CORRECTS ONE VALUE TO ONE SPELLING WHERE THE MODELS CARRY EXACTLY ONE. The
    /// candidates are the workset names READ OFF THE MODELS, never a list in the code.
    ///
    /// AND WHERE THE MODELS CARRY TWO OR MORE IT ASKS FOR EVERY ONE OF THEM, Q102 answered
    /// on 2026-10-04. Until then a value with two case only spellings was refused and left
    /// as it was, because a rule that guesses between two real worksets is worse than a set
    /// that finds nothing. It no longer guesses: the C06 buildings write ME-DUCTWORK and
    /// ME-Ductwork, so a set asks both as Or groups built the way Q69's are, each group
    /// copied whole, FR-025, and finds the items of every building whichever it carries.
    /// A spelling no model was measured carrying is never asked.
    /// </summary>
    public sealed class ValueRewrite
    {
        public ValueRewrite(string from, IList<string> candidates)
        {
            From = from ?? string.Empty;
            Candidates = candidates ?? new List<string>();
        }

        /// <summary>The value as the matrix writes it.</summary>
        public string From { get; private set; }

        /// <summary>
        /// Every spelling the MODELS carry that differs from it by case alone. None means
        /// nothing to correct, one is the correction, and two or more are all asked, Q102.
        /// </summary>
        public IList<string> Candidates { get; private set; }

        /// <summary>The one spelling to write, or null where there is not exactly one.</summary>
        public string To
        {
            get { return Candidates.Count == 1 ? Candidates[0] : null; }
        }

        /// <summary>
        /// Whether this is a correction to one spelling. A value the models spell exactly as
        /// the matrix does needs none, and one with two candidates is asked in both instead.
        /// </summary>
        public bool Corrects
        {
            get
            {
                return To != null
                    && !string.Equals(To, From, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Every value the matrix asks for, matched against every workset name the models
        /// carry, case blind. The one place a candidate list is built, so the rule and
        /// the log cannot disagree about what was on offer.
        /// </summary>
        public static IList<ValueRewrite> For(IEnumerable<string> matrixValues, IEnumerable<string> modelWorksets)
        {
            List<ValueRewrite> rewrites = new List<ValueRewrite>();

            if (matrixValues == null)
            {
                return rewrites;
            }

            List<string> models = new List<string>();

            if (modelWorksets != null)
            {
                foreach (string workset in modelWorksets)
                {
                    if (!string.IsNullOrEmpty(workset) && !models.Contains(workset))
                    {
                        models.Add(workset);
                    }
                }
            }

            List<string> seen = new List<string>();

            foreach (string value in matrixValues)
            {
                if (string.IsNullOrEmpty(value) || seen.Contains(value))
                {
                    continue;
                }

                seen.Add(value);
                List<string> candidates = new List<string>();

                for (int i = 0; i < models.Count; i++)
                {
                    // CASE ALONE and nothing wider. A name differing by a letter is a
                    // different word, which 5t proved twice over on this project.
                    if (string.Equals(models[i], value, StringComparison.OrdinalIgnoreCase))
                    {
                        candidates.Add(models[i]);
                    }
                }

                rewrites.Add(new ValueRewrite(value, candidates));
            }

            return rewrites;
        }
    }
    /// <summary>
    /// One value the matrix asks for, carrying a SECOND spelling beside it as an Or row,
    /// Q69 answered b on 2026-09-20.
    ///
    /// WHY. Where this project's own models disagree about the name of a workset, a set
    /// asking for one spelling finds only the models that used it. Carrying both means
    /// the set finds everything it was meant to find while the models are still wrong.
    ///
    /// THE OR ROW IS THE WHOLE GROUP COPIED, FR-025. `flags="64"` is StartGroup, which this
    /// repo already measured, F78: a condition with that bit starts a new group, the
    /// conditions inside a group are ANDed and the groups are ORed. A set of Category X and
    /// Workset V becomes (X and V) or (X and the other spelling), so every group still asks
    /// for its category. One condition with that bit put straight after the workset, which
    /// is what this wrote until F116, started a group holding the workset alone, and that
    /// group took every element on the other spelling whatever its category.
    ///
    /// IT IS BUILT FROM WHAT WAS MEASURED IN THE MODELS AND NEVER FROM A LIST IN THE
    /// CODE, and it never runs on a spelling no model carries, because the whole reason
    /// it exists is that a real model somewhere used the other word.
    ///
    /// AND THE EXPORT CHECK STILL NAMES THE PAIR AS MISSPELLED. Absorbing a typo and
    /// saying nothing would mean nobody ever fixes the model and the next building
    /// repeats it, which is `Federator.Core.Health.WorksetDisagreements`.
    /// </summary>
    public sealed class ValueOrRow
    {
        public ValueOrRow(string value, string alsoAccept)
        {
            Value = value ?? string.Empty;
            AlsoAccept = alsoAccept ?? string.Empty;
        }

        /// <summary>The value the matrix already asks for.</summary>
        public string Value { get; private set; }

        /// <summary>The other spelling a model carries, added beside it as an Or row.</summary>
        public string AlsoAccept { get; private set; }

        /// <summary>Whether there is anything to do. Two identical spellings are not a disagreement.</summary>
        public bool Adds
        {
            get
            {
                return Value.Length > 0
                    && AlsoAccept.Length > 0
                    && !string.Equals(Value, AlsoAccept, StringComparison.Ordinal);
            }
        }
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
            return Apply(xml, renames, rewrites, conditions, null);
        }

        /// <summary>
        /// The same, plus the VALUE corrections of Q68 and Q102: a workset the matrix spells
        /// one way and every model spells another is corrected to theirs, and one the models
        /// spell two or more ways is asked in every one of them. Pass null for the last and
        /// it is the four argument form exactly.
        /// </summary>
        public static CorrectionOutcome Apply(
            string xml,
            IList<SetRename> renames,
            IList<CategoryRewrite> rewrites,
            IList<ConditionsRewrite> conditions,
            IList<ValueRewrite> values)
        {
            return Apply(xml, renames, rewrites, conditions, values, null);
        }

        /// <summary>
        /// The same, plus the Or rows of Q69: a second spelling a model carries, accepted
        /// beside the one the matrix asks for. Pass null and it is the five argument form.
        /// </summary>
        public static CorrectionOutcome Apply(
            string xml,
            IList<SetRename> renames,
            IList<CategoryRewrite> rewrites,
            IList<ConditionsRewrite> conditions,
            IList<ValueRewrite> values,
            IList<ValueOrRow> orRows)
        {
            return Apply(xml, renames, rewrites, conditions, values, orRows, null);
        }

        /// <summary>
        /// The same, plus the Source File rule of Q103: a set beside the ones asking that
        /// Source File condition asks it too where another discipline also uses its
        /// category. Applied last, so it reads the sets as every other correction left them.
        /// Pass null and it is the six argument form.
        /// </summary>
        public static CorrectionOutcome Apply(
            string xml,
            IList<SetRename> renames,
            IList<CategoryRewrite> rewrites,
            IList<ConditionsRewrite> conditions,
            IList<ValueRewrite> values,
            IList<ValueOrRow> orRows,
            IList<SourceFileRule> sourceFiles)
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

            if (values != null)
            {
                List<string> askedInEverySpelling = new List<string>();

                foreach (ValueRewrite value in values)
                {
                    if (value == null)
                    {
                        continue;
                    }

                    if (value.Candidates.Count > 1)
                    {
                        // Q102. Every spelling, and once for each workset whatever spelling
                        // the file asked first, because the first pass already asked them all.
                        if (askedInEverySpelling.Exists(
                            done => string.Equals(done, value.From, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        askedInEverySpelling.Add(value.From);

                        List<string> spellings = new List<string>(value.Candidates);
                        spellings.Sort(StringComparer.Ordinal);

                        int asking;
                        int widened;
                        text = AskEverySpelling(
                            text,
                            one => string.Equals(one, value.From, StringComparison.OrdinalIgnoreCase),
                            spellings,
                            out asking,
                            out widened);

                        outcome.Add(
                            "the value " + value.From + " is asked as " + string.Join(" or ", spellings.ToArray())
                                + ", every spelling the models in this project carry",
                            widened,
                            widened > 0
                                ? null
                                : asking > 0
                                    ? "every set asking for it already asks every spelling"
                                    : "this file does not ask for that value");
                        continue;
                    }

                    if (!value.Corrects)
                    {
                        outcome.Add(
                            "the value " + value.From + " is left alone",
                            0,
                            value.Candidates.Count == 0
                                ? "no model in this run carries a workset spelled that way but for its case"
                                : "the models spell it exactly as the matrix does");
                        continue;
                    }

                    int changed;
                    text = RewriteValue(text, value.From, value.To, out changed);

                    outcome.Add(
                        "the value " + value.From + " becomes " + value.To + ", which is how the models spell it",
                        changed,
                        changed == 0 ? "this file does not ask for that value" : null);
                }
            }

            if (orRows != null)
            {
                foreach (ValueOrRow row in orRows)
                {
                    if (row == null)
                    {
                        continue;
                    }

                    if (!row.Adds)
                    {
                        outcome.Add(
                            "the value " + row.Value + " gains no Or row",
                            0,
                            "no model carries a second spelling of it that this tool would accept");
                        continue;
                    }

                    int asked;
                    int added;
                    List<string> both = new List<string> { row.Value, row.AlsoAccept };
                    both.Sort(StringComparer.Ordinal);

                    text = AskEverySpelling(
                        text,
                        value => string.Equals(value, row.Value, StringComparison.Ordinal)
                            || string.Equals(value, row.AlsoAccept, StringComparison.Ordinal),
                        both,
                        out asked,
                        out added);

                    outcome.Add(
                        "the value " + row.Value + " also accepts " + row.AlsoAccept
                            + ", which a model in this run spells that way",
                        added,
                        added > 0
                            ? null
                            : asked > 0
                                ? "every set asking for it already asks for both"
                                : "this file holds no condition asking for that value");
                }
            }

            if (sourceFiles != null)
            {
                foreach (SourceFileRule rule in sourceFiles)
                {
                    if (rule != null && rule.Asks.Length > 0)
                    {
                        text = AskSourceFile(text, rule, outcome);
                    }
                }
            }

            outcome.Text = text;
            return outcome;
        }

        /// <summary>
        /// Q103. A set in the folder of the sets already asking that Source File condition
        /// asks it too, where its category is one another discipline also uses: one another
        /// folder's set asks, read off this file, or one another discipline's models were
        /// measured carrying, handed in. The condition is the file's own, copied off the
        /// first set asking it, and goes at the end of every group of the set that lacks
        /// it, so a set that is an Or keeps asking it in each group.
        ///
        /// NOTHING HERE NAMES A DISCIPLINE, A FOLDER OR A CATEGORY. The value it asks, -AR-
        /// on this project, is handed in, and the folder and the category property are those
        /// of the set already asking it. One line per set it changes, and one saying so where
        /// none needed it, so the log names every correction.
        /// </summary>
        private static string AskSourceFile(string xml, SourceFileRule rule, CorrectionOutcome outcome)
        {
            ExchangeDocument read = new ExchangeReader().ReadText(xml);
            SelectionSetDefinition template = null;
            SearchConditionDefinition asking = null;

            foreach (SelectionSetDefinition set in read.Sets)
            {
                asking = AskingCondition(set, rule.Asks);

                if (asking != null)
                {
                    template = set;
                    break;
                }
            }

            if (template == null)
            {
                outcome.Add(
                    "no set asks for " + rule.Asks + ", so no set beside one is given it",
                    0,
                    "there is no condition in this file to copy");
                return xml;
            }

            string condition = (string.IsNullOrEmpty(asking.Property.DisplayName)
                ? asking.Property.InternalName
                : asking.Property.DisplayName) + " contains " + rule.Asks;

            string categoryProperty = null;

            foreach (SearchConditionDefinition other in template.Conditions)
            {
                if (!ReferenceEquals(other, asking) && other.Property != null)
                {
                    categoryProperty = other.Property.InternalName;
                    break;
                }
            }

            if (categoryProperty == null)
            {
                outcome.Add(
                    "no set beside those asking " + condition + " is given it",
                    0,
                    template.Name + " asks for nothing else, so no category could be read");
                return xml;
            }

            HashSet<string> askedElsewhere = new HashSet<string>(StringComparer.Ordinal);

            foreach (SelectionSetDefinition set in read.Sets)
            {
                if (!SameFolder(set, template))
                {
                    askedElsewhere.UnionWith(CategoriesOf(set, categoryProperty));
                }
            }

            // The sets to give it, keyed by the name as the file writes it, in file order.
            Dictionary<string, string> because = new Dictionary<string, string>(StringComparer.Ordinal);
            List<SelectionSetDefinition> given = new List<SelectionSetDefinition>();

            foreach (SelectionSetDefinition set in read.Sets)
            {
                if (!SameFolder(set, template) || because.ContainsKey(AttributeText(set.Name)))
                {
                    continue;
                }

                foreach (string category in CategoriesOf(set, categoryProperty))
                {
                    string reason = askedElsewhere.Contains(category)
                        ? "a set in another folder also asks for " + category
                        : rule.MeasuredElsewhere.Contains(category)
                            ? "another discipline's models were measured carrying " + category
                            : null;

                    if (reason != null)
                    {
                        because[AttributeText(set.Name)] = reason;
                        given.Add(set);
                        break;
                    }
                }
            }

            WrittenCondition copy = WrittenAsking(xml, template.Name, rule.Asks);

            if (copy == null)
            {
                outcome.Add(
                    "no set beside those asking " + condition + " is given it",
                    0,
                    template.Name + " could not be read in the file's text, so there is nothing to copy");
                return xml;
            }

            copy = copy.WithFlags(copy.Flags & ~StartGroup);
            Dictionary<string, int> groupsGiven = new Dictionary<string, int>(StringComparer.Ordinal);

            string text = EachSet(xml, block =>
            {
                string name = NameIn(block);

                if (name == null || !because.ContainsKey(name) || groupsGiven.ContainsKey(name))
                {
                    return block;
                }

                SetConditionsText set = SetConditionsText.Read(block);

                if (set == null)
                {
                    return block;
                }

                List<WrittenCondition> written = new List<WrittenCondition>();
                int added = 0;

                foreach (IList<WrittenCondition> group in set.Groups())
                {
                    written.AddRange(group);

                    if (!Asks(group, rule.Asks))
                    {
                        written.Add(copy.WithLead(group[group.Count - 1].Lead));
                        added++;
                    }
                }

                groupsGiven[name] = added;
                return added == 0 ? block : set.With(written).Write();
            });

            int changedSets = 0;

            foreach (SelectionSetDefinition set in given)
            {
                string name = AttributeText(set.Name);
                int added;

                if (!groupsGiven.TryGetValue(name, out added))
                {
                    outcome.Add(
                        set.Name + " is to ask " + condition + " as well",
                        0,
                        "because " + because[name] + ", and its conditions could not be read in the file's text, so it was NOT changed");
                    continue;
                }

                if (added > 0)
                {
                    changedSets++;
                    outcome.Add(set.Name + " asks " + condition + " as well, because " + because[name], added, null);
                }
            }

            if (changedSets == 0)
            {
                outcome.Add(
                    "every set beside those asking " + condition + " whose category another discipline also uses asks it already",
                    0,
                    null);
            }

            return text;
        }

        /// <summary>The condition of that set asking for that value with contains, and not negated, or null.</summary>
        private static SearchConditionDefinition AskingCondition(SelectionSetDefinition set, string asks)
        {
            foreach (SearchConditionDefinition condition in set.Conditions)
            {
                if (condition.Property != null
                    && condition.Value != null
                    && string.Equals(condition.Test, SetBuildPlan.ContainsTest, StringComparison.Ordinal)
                    && (condition.Flags & NegateCondition) == 0
                    && string.Equals(condition.Value.Data, asks, StringComparison.Ordinal))
                {
                    return condition;
                }
            }

            return null;
        }

        /// <summary>Whether a group of the file's text already asks for that value with contains.</summary>
        private static bool Asks(IList<WrittenCondition> group, string asks)
        {
            foreach (WrittenCondition condition in group)
            {
                if (string.Equals(condition.Test, SetBuildPlan.ContainsTest, StringComparison.Ordinal)
                    && string.Equals(condition.Value, asks, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The values a set asks on that property and not negated, in the order it asks them.</summary>
        private static IList<string> CategoriesOf(SelectionSetDefinition set, string property)
        {
            List<string> categories = new List<string>();

            foreach (SearchConditionDefinition condition in set.Conditions)
            {
                if (condition.Property != null
                    && condition.Value != null
                    && (condition.Flags & NegateCondition) == 0
                    && string.Equals(condition.Property.InternalName, property, StringComparison.Ordinal)
                    && !categories.Contains(condition.Value.Data))
                {
                    categories.Add(condition.Value.Data);
                }
            }

            return categories;
        }

        private static bool SameFolder(SelectionSetDefinition one, SelectionSetDefinition other)
        {
            if (one.Folders.Count != other.Folders.Count)
            {
                return false;
            }

            for (int i = 0; i < one.Folders.Count; i++)
            {
                if (!string.Equals(one.Folders[i], other.Folders[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>That set's condition asking for that value, as the file's text writes it, or null where it will not read.</summary>
        private static WrittenCondition WrittenAsking(string xml, string setName, string asks)
        {
            int at = xml.IndexOf(SetOpens + AttributeText(setName) + "\"", StringComparison.Ordinal);
            int ends = at < 0 ? -1 : xml.IndexOf(SetCloses, at, StringComparison.Ordinal);
            SetConditionsText set = ends < 0 ? null : SetConditionsText.Read(xml.Substring(at, ends - at + SetCloses.Length));

            if (set == null)
            {
                return null;
            }

            foreach (WrittenCondition condition in set.Conditions)
            {
                if (string.Equals(condition.Test, SetBuildPlan.ContainsTest, StringComparison.Ordinal)
                    && string.Equals(condition.Value, asks, StringComparison.Ordinal))
                {
                    return condition;
                }
            }

            return null;
        }

        /// <summary>The name attribute a set block opens with, as the file writes it, or null.</summary>
        private static string NameIn(string block)
        {
            if (!block.StartsWith(SetOpens, StringComparison.Ordinal))
            {
                return null;
            }

            int ends = block.IndexOf('"', SetOpens.Length);
            return ends < 0 ? null : block.Substring(SetOpens.Length, ends - SetOpens.Length);
        }

        /// <summary>A name the way an attribute in the file writes it, its four special characters escaped.</summary>
        private static string AttributeText(string name)
        {
            return (name ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        /// <summary>
        /// Every group of every set that asks for one of those spellings, written once per
        /// spelling with the rest of the group copied into it, FR-025. A group of Ducts on
        /// ME-Ductwork that is also to accept ME-DUCTWORK becomes (Ducts and ME-DUCTWORK) or
        /// (Ducts and ME-Ductwork), so each group still asks for its own category. The
        /// category and the property are copied off the file itself, so nothing here has to
        /// know what either is called on this project.
        ///
        /// THE SPELLINGS ARE WRITTEN IN THE ORDER GIVEN, whichever one the file asked, and a
        /// group already written once per spelling is left as it is. So a file asking one
        /// spelling and a file asking the other come out the same, condition for condition,
        /// and a second run over its own output changes nothing and counts zero.
        /// </summary>
        /// <param name="asks">Whether a condition's value is one of the spellings.</param>
        /// <param name="asked">How many conditions in the file asked for one of them.</param>
        /// <param name="changed">How many of those were in a set this changed.</param>
        private static string AskEverySpelling(
            string xml, Func<string, bool> asks, IList<string> spellings, out int asked, out int changed)
        {
            int askedInAll = 0;
            int changedInAll = 0;

            string text = EachSet(xml, block =>
            {
                SetConditionsText set = SetConditionsText.Read(block);

                if (set == null)
                {
                    return block;
                }

                int inSet = 0;
                List<WrittenCondition> written = new List<WrittenCondition>();
                HashSet<string> done = new HashSet<string>(StringComparer.Ordinal);

                foreach (IList<WrittenCondition> group in set.Groups())
                {
                    int inGroup = Asking(group, asks);

                    if (inGroup == 0)
                    {
                        written.AddRange(group);
                        continue;
                    }

                    inSet += inGroup;

                    // A group that differs from one already written only in its spelling is
                    // one of that one's copies, so it is not written a second time.
                    if (!done.Add(Shape(group, asks)))
                    {
                        continue;
                    }

                    for (int i = 0; i < spellings.Count; i++)
                    {
                        for (int c = 0; c < group.Count; c++)
                        {
                            WrittenCondition one = asks(group[c].Value) ? group[c].WithValue(spellings[i]) : group[c];
                            written.Add(c == 0 && i > 0 ? one.WithFlags(one.Flags | StartGroup) : one);
                        }
                    }
                }

                askedInAll += inSet;

                if (inSet == 0)
                {
                    return block;
                }

                string rewritten = set.With(written).Write();

                if (!string.Equals(rewritten, block, StringComparison.Ordinal))
                {
                    changedInAll += inSet;
                }

                return rewritten;
            });

            asked = askedInAll;
            changed = changedInAll;
            return text;
        }

        /// <summary>How many conditions of that group ask for one of the spellings.</summary>
        private static int Asking(IList<WrittenCondition> group, Func<string, bool> asks)
        {
            int count = 0;

            foreach (WrittenCondition condition in group)
            {
                if (asks(condition.Value))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// The group with its spelling and its StartGroup bit taken out, which is what two
        /// copies of one group have in common and two different groups do not.
        /// </summary>
        private static string Shape(IList<WrittenCondition> group, Func<string, bool> asks)
        {
            StringBuilder shape = new StringBuilder();

            for (int c = 0; c < group.Count; c++)
            {
                WrittenCondition one = asks(group[c].Value) ? group[c].WithValue(string.Empty) : group[c];
                shape.Append(c == 0 ? one.WithFlags(one.Flags & ~StartGroup).Element : one.Element).Append('\n');
            }

            return shape.ToString();
        }

        /// <summary>Every set block of the file in turn, handed to the change and written back as it comes out.</summary>
        private static string EachSet(string xml, Func<string, string> change)
        {
            StringBuilder written = new StringBuilder(xml.Length);
            int at = 0;

            while (true)
            {
                int opens = xml.IndexOf(SetOpens, at, StringComparison.Ordinal);
                int closes = opens < 0 ? -1 : xml.IndexOf(SetCloses, opens, StringComparison.Ordinal);

                if (closes < 0)
                {
                    break;
                }

                closes += SetCloses.Length;
                written.Append(xml, at, opens - at).Append(change(xml.Substring(opens, closes - opens)));
                at = closes;
            }

            return written.Append(xml, at, xml.Length - at).ToString();
        }

        /// <summary>A whole wstring value element, which is what a workset value is written as.</summary>
        private const string ValueOpens = "<data type=\"wstring\">";

        private const string ValueCloses = "</data>";

        /// <summary>
        /// The bit that starts a new condition group, which is what makes an Or, F78.
        /// Measured over all 102 conditions of the reference file on 2026-09-19, and the
        /// plan's own constant, so the two cannot disagree.
        /// </summary>
        public const int StartGroup = PlannedCondition.StartGroupFlag;

        /// <summary>
        /// One value rewritten wherever it is the whole text of a data element, and
        /// NOWHERE ELSE. Scoped to `&lt;data type="wstring"&gt;VALUE&lt;/data&gt;` rather
        /// than replaced across the file, because a workset name can be a substring of a
        /// set name, of a category or of another workset, and a blind replace would
        /// rewrite all of them. Matched Ordinal, because the whole point is that the
        /// spelling differs.
        /// </summary>
        private static string RewriteValue(string xml, string from, string to, out int changed)
        {
            string was = ValueOpens + from + ValueCloses;
            string now = ValueOpens + to + ValueCloses;

            changed = Occurrences(xml, was);
            return changed == 0 ? xml : xml.Replace(was, now);
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

        /// <summary>How a set's conditions open and close, and one condition, read by SetConditionsText as well.</summary>
        internal const string ConditionsOpen = "<conditions>";

        internal const string ConditionsClose = "</conditions>";

        internal const string ConditionOpens = "<condition ";

        internal const string ConditionCloses = "</condition>";

        private const string DataOpens = "<data ";

        /// <summary>
        /// The value inside one named set, and inside no other. Where the set is not there
        /// at all, nothing changes and the count is zero.
        ///
        /// ONLY A WHOLE VALUE, FR-026. The block opens with the set's own name, and replacing
        /// the text across it renamed a set whose name held the value, and found an old value
        /// again inside a new one that held it, so a second run grew it. A whole value element
        /// is neither, so the name is never touched and a second run counts zero.
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
            string block = RewriteValue(text.Substring(at, ends - at), rewrite.From, rewrite.To, out changed);

            return changed == 0 ? text : text.Substring(0, at) + block + text.Substring(ends);
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
