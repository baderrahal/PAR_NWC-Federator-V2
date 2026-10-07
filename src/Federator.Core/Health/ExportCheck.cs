using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Sets;

namespace Federator.Core.Health
{
    /// <summary>
    /// What ONE model carries of the two things a set and a report column need, as plain
    /// numbers and names, read off the Revit elements in it.
    /// </summary>
    public sealed class ModelExport
    {
        /// <summary>A count that could not be taken. Never zero, because zero reads as a real count.</summary>
        public const int NotCounted = -1;

        public ModelExport(string file, string discipline, int elements, int withWorkset, int withElementId, IList<string> worksets)
        {
            File = file ?? string.Empty;
            Discipline = discipline ?? string.Empty;
            Elements = elements;
            WithWorkset = withWorkset;
            WithElementId = withElementId;

            // T1-S49. A walk that threw hands on the names it gathered before it threw,
            // and they were listed as seen and compared for typos. Part of a list is not a
            // list, the same as part of a count is not a count, so a model that was not
            // counted carries no names.
            Worksets = worksets == null || !Counted ? new List<string>() : worksets;
        }

        /// <summary>The NWC file name.</summary>
        public string File { get; private set; }

        /// <summary>Part 5 of the file name, the discipline code.</summary>
        public string Discipline { get; private set; }

        /// <summary>
        /// How many Revit ELEMENTS the model holds, which is not how many items it holds
        /// and not how many of them have geometry. MEASURED on 2026-09-20, scan.md 5q: a
        /// Revit element reaches Navisworks as a composite item carrying the Element tab,
        /// and the geometry solids under it carry neither the workset nor the id. A first
        /// count taken over the geometry read zero against a real run reading 860 of 1,052.
        /// </summary>
        public int Elements { get; private set; }

        /// <summary>How many of them carry a Workset, or NotCounted.</summary>
        public int WithWorkset { get; private set; }

        /// <summary>How many of them carry an Element ID, or NotCounted.</summary>
        public int WithElementId { get; private set; }

        /// <summary>The distinct workset names, in the order they were first seen. None where the model was not counted.</summary>
        public IList<string> Worksets { get; private set; }

        /// <summary>
        /// Whether the walk over the model's elements finished. A walk that threw part way
        /// counts nothing, every count NotCounted, and the worksets it hands back are only
        /// those seen before it stopped, F116.
        /// </summary>
        public bool WalkFinished
        {
            get { return Elements != NotCounted; }
        }

        /// <summary>Whether any element in this model carries a workset at all.</summary>
        public bool CarriesAWorkset
        {
            get { return WithWorkset > 0; }
        }

        /// <summary>
        /// Whether all three counts were taken. A walk that threw part way gives back none
        /// of them, because part of a count is not a count, and a model nobody counted is
        /// never called whole and never called empty.
        /// </summary>
        public bool Counted
        {
            get { return Elements != NotCounted && WithWorkset != NotCounted && WithElementId != NotCounted; }
        }

        /// <summary>
        /// Whether the model was counted and holds no Revit element at all, so it has no
        /// workset and no element id to check. The one rule the block and the run line both
        /// count by, the breaker's fifth finding at c5d8aa8.
        /// </summary>
        public bool HoldsNoElement
        {
            get { return Counted && Elements == 0; }
        }

        /// <summary>Whether the model holds elements and not one of them carries a workset. False where nothing was counted.</summary>
        public bool CarriesNoWorkset
        {
            get { return Counted && Elements > 0 && WithWorkset == 0; }
        }

        /// <summary>
        /// Whether some of its elements carry a workset and some do not, T1-S50. One
        /// element of a thousand on a workset passed as every element, and a set that
        /// filters on workset finds none of the other 999.
        /// </summary>
        public bool CarriesAWorksetOnSomeElements
        {
            get { return Counted && WithWorkset > 0 && WithWorkset < Elements; }
        }

        /// <summary>
        /// Whether some of its elements reach the report with an empty id cell, T1-S48.
        /// Judged on the COUNTS and never on IdShare, because 1051 of 1052 is 99.9 per cent
        /// and once read as 100, which wrote no re-export line and called every id there.
        /// </summary>
        public bool MissesAnId
        {
            get { return Counted && Elements > 0 && WithElementId < Elements; }
        }

        /// <summary>
        /// The share of elements carrying an id, as whole per cent for a person to read, or
        /// minus one where it could not be worked out. It reads 100 only when every element
        /// carries one and 0 only when none does, so it never sits at 100 beside a line
        /// about missing ids. Nothing is judged on it, MissesAnId is.
        /// </summary>
        public int IdShare
        {
            get
            {
                if (Elements <= 0 || WithElementId == NotCounted)
                {
                    return NotCounted;
                }

                int share = (int)Math.Round(100.0 * WithElementId / Elements, MidpointRounding.AwayFromZero);

                if (WithElementId < Elements)
                {
                    share = Math.Min(share, 99);
                }

                if (WithElementId > 0)
                {
                    share = Math.Max(share, 1);
                }

                return share;
            }
        }
    }

    /// <summary>
    /// Two faults that are invisible until a set finds nothing or a report column comes
    /// out blank, put in front of a person on every run instead of behind a button.
    ///
    /// THE WORKSET. Every mechanical set in the client's matrix filters on the Workset
    /// parameter, and 33 of the 61 sets found nothing in every group of the run of
    /// 2026-09-20. So this says, per model, how many of its elements carry a workset
    /// and WHAT THE NAMES ARE, because the names are what a person has to put beside the
    /// matrix. On 2026-09-20 that comparison was the answer: the models carry
    /// ME-Ductwork and the matrix asks for ME-DUCTWORK, the condition carries no
    /// ignore case flag, and case sensitive is why those sets find nothing, scan.md 5q.
    ///
    /// THE ELEMENT ID. Per model, what share of elements carry one. A model at zero was
    /// exported with Convert element Ids switched off, and every id cell of every row it
    /// touches comes out blank.
    ///
    /// Nothing here fails a group, Q65 answered: report it and run anyway.
    /// </summary>
    public static class ExportCheck
    {
        /// <summary>The block's title, which the add-in puts the building after.</summary>
        public const string BlockTitle = "EXPORT CHECK";

        /// <summary>
        /// Every workset name of one model, in the order the model gave them, for the row
        /// file, F116. The block lists ten a group and counts the rest, and which spelling
        /// each building's models carry is what the matrix corrections act on, Q102, so the
        /// row file carries them all. A bar splits them, because a name can hold a comma.
        /// UNKNOWN where the walk over the model did not finish, never the names it saw before
        /// it stopped as if they were all.
        /// </summary>
        public static string EveryWorkset(ModelExport model)
        {
            if (!model.WalkFinished)
            {
                return "UNKNOWN, the walk over this model's elements stopped part way, so the "
                    + model.Worksets.Count + " workset name(s) it saw before stopping are not every workset the model carries";
            }

            return string.Join(" | ", new List<string>(model.Worksets).ToArray());
        }

        /// <summary>
        /// The number column of the same row: how many worksets the model carries, or empty
        /// where its element walk did not finish, because a count of the names seen before it
        /// stopped is not a count of the model's worksets, F116.
        /// </summary>
        public static string WorksetCount(ModelExport model)
        {
            return model.WalkFinished ? model.Worksets.Count.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        /// <summary>
        /// How many workset names are listed before the rest are counted, and the block SAYS
        /// it truncated rather than leaving a reader to wonder. Ten, taken when the block was
        /// written from the ten SetsAcrossTheRun named then. That number is no longer shared:
        /// since F127 SetsAcrossTheRun names every set that found nothing, through
        /// CoverageSettings.SetsAtZeroNamedInTheRun, and this one stays ten.
        /// </summary>
        public const int NamesShown = 10;

        /// <summary>
        /// The block, written even when everything is right, because a missing block reads
        /// as a check that did not run. The sets are those of the picked clash file, or
        /// null where none was picked, and they are what the workset names are compared
        /// with by letter case.
        /// </summary>
        public static IList<string> Lines(IList<ModelExport> models, IEnumerable<SelectionSetDefinition> sets)
        {
            return Lines(models, sets, NamesShown);
        }

        /// <summary>
        /// What one model carries, in the words of its line in the block, which the row
        /// file carries too so the two cannot disagree. A count nobody took is UNKNOWN,
        /// never zero and never NONE, T1-S49: a model whose walk threw read worksets NONE
        /// in the block and 0 workset(s) in the row file.
        /// </summary>
        public static string Counts(ModelExport model)
        {
            string worksets = !model.Counted
                ? "UNKNOWN"
                : model.CarriesAWorkset ? model.Worksets.Count.ToString(CultureInfo.InvariantCulture) : "NONE";

            return "elements " + Count(model.Elements)
                + "   with a workset " + Count(model.WithWorkset)
                + "   worksets " + worksets
                + "   element id " + Share(model.IdShare);
        }

        /// <summary>
        /// The elements of one model for the Number column of the row file, or empty where
        /// they could not be counted, which is what the row file carries for an unknown
        /// elsewhere: the model placement row leaves its Z empty for a placement not read. It
        /// carried -1, and a sum over a run took -1 for each model, the breaker's ninth
        /// finding at c5d8aa8.
        /// </summary>
        public static string ElementsNumber(ModelExport model)
        {
            return model == null || !model.Counted ? string.Empty : EventRow.Count(model.Elements);
        }

        public static IList<string> Lines(
            IList<ModelExport> models, IEnumerable<SelectionSetDefinition> sets, int namesShown)
        {
            List<string> lines = new List<string>();

            if (models == null || models.Count == 0)
            {
                lines.Add("no model was read, so nothing could be checked");
                return lines;
            }

            int withoutAnyWorkset = 0;
            int withSomeWorkset = 0;
            int withoutEveryId = 0;
            int withNoElement = 0;
            int notCounted = 0;
            List<string> everyWorkset = new List<string>();

            for (int i = 0; i < models.Count; i++)
            {
                ModelExport model = models[i];

                lines.Add("   " + Named(model) + "   " + Counts(model));

                // EVERY ELEMENT IS A CLAIM THAT NEEDS COUNTING, T1-S50. A model nobody
                // counted, one holding no Revit element and one with a workset on only
                // some of its elements were all once called whole by the sentence below.
                if (!model.Counted)
                {
                    notCounted++;
                    lines.Add("      its elements could not be counted, so it is not called whole and none of its workset names is listed");
                }
                else if (model.HoldsNoElement)
                {
                    withNoElement++;
                    lines.Add("      no item in this model is a Revit element, so it has no workset and no element id to check");
                }
                else if (model.CarriesNoWorkset)
                {
                    withoutAnyWorkset++;
                    lines.Add("      no element carries a workset. Every set that filters on workset will find nothing here");
                }
                else if (model.CarriesAWorksetOnSomeElements)
                {
                    withSomeWorkset++;
                    lines.Add("      only " + Count(model.WithWorkset) + " of " + Count(model.Elements)
                        + " element(s) carry a workset. A set that filters on workset finds none of the other "
                        + Count(model.Elements - model.WithWorkset) + " here");
                }

                if (model.MissesAnId)
                {
                    withoutEveryId++;
                    lines.Add("      re-export with Convert element Ids switched on, or "
                        + Count(model.Elements - model.WithElementId) + " element(s) reach the report with an empty id cell");
                }

                Gather(everyWorkset, model.Worksets);
            }

            lines.Add(Sentence(models.Count, withoutAnyWorkset, withSomeWorkset, withoutEveryId, withNoElement, notCounted));
            AddWorksets(lines, everyWorkset, namesShown);
            AddCaseDifferences(lines, everyWorkset, sets, namesShown);
            AddDisagreements(lines, models);
            return lines;
        }

        /// <summary>
        /// Where a set of the picked file asks for a workset that a model here carries in
        /// another letter case, S03-2. The match is case sensitive, and Bader refused an
        /// ignore case flag on 2026-09-20, so a condition asking for one spelling finds
        /// none of the items carrying the other. Each pair is named with the sets that ask,
        /// and a group where no name differs says so in one line.
        ///
        /// IT REPLACES ONE FIXED SENTENCE. 'The match is CASE SENSITIVE, so ME-Ductwork does
        /// not match ME-DUCTWORK' was written in all 22 groups of set 03, whether any name
        /// differed or not, with one project's two spellings typed into it, log line 210.
        ///
        /// A SET THAT ALSO ASKS THE CARRIED SPELLING EXACTLY IS LEFT OUT, F116. The corrections
        /// ask a workset in every spelling measured, Q102, so a set asks ME-DUCTWORK or
        /// ME-Ductwork and finds the items carrying either. Naming it as missing one spelling
        /// would be the overclaim this check is there to end. A set asking the carried spelling
        /// only negated finds none of those items and is still named.
        /// </summary>
        private static void AddCaseDifferences(
            IList<string> lines, IList<string> carried, IEnumerable<SelectionSetDefinition> sets, int namesShown)
        {
            List<WorksetAsk> asks = new List<WorksetAsk>();
            Dictionary<string, List<WorksetAsk>> askedBy = new Dictionary<string, List<WorksetAsk>>(StringComparer.Ordinal);
            bool anySet = false;

            if (sets != null)
            {
                foreach (SelectionSetDefinition set in sets)
                {
                    if (set == null)
                    {
                        continue;
                    }

                    anySet = true;

                    foreach (SearchConditionDefinition condition in set.Conditions)
                    {
                        string value = condition.Value == null ? null : condition.Value.Data;

                        if (condition.Property == null
                            || !string.Equals(condition.Property.InternalName, EmptySets.WorksetProperty, StringComparison.Ordinal)
                            || string.IsNullOrEmpty(value))
                        {
                            continue;
                        }

                        bool contains = string.Equals(condition.Test, SetWarnings.ContainsTest, StringComparison.OrdinalIgnoreCase);
                        WorksetAsk ask = asks.Find(a => a.Contains == contains && string.Equals(a.Value, value, StringComparison.Ordinal));

                        if (ask == null)
                        {
                            ask = new WorksetAsk(value, contains);
                            asks.Add(ask);
                        }

                        string setName = string.IsNullOrEmpty(set.Name) ? "a set with no name" : set.Name;

                        if (!ask.Sets.Contains(setName))
                        {
                            ask.Sets.Add(setName);
                        }

                        if ((condition.Flags & MatrixCorrections.NegateCondition) == 0)
                        {
                            if (!askedBy.ContainsKey(setName))
                            {
                                askedBy[setName] = new List<WorksetAsk>();
                            }

                            askedBy[setName].Add(ask);
                        }
                    }
                }
            }

            if (!anySet)
            {
                lines.Add("no set was read from a picked file, so no workset name was compared with what a set asks");
                return;
            }

            if (asks.Count == 0)
            {
                lines.Add("no set of the picked file asks for a workset, so there was nothing to compare");
                return;
            }

            List<string> pairs = new List<string>();

            foreach (WorksetAsk ask in asks)
            {
                foreach (string name in carried)
                {
                    if (!ask.MissesByCaseAlone(name))
                    {
                        continue;
                    }

                    List<string> missing = ask.Sets.FindAll(setName => !AlsoFinds(askedBy, setName, name));

                    if (missing.Count > 0)
                    {
                        pairs.Add("   the file asks for \"" + ask.Value + "\" in " + SetNames(missing, namesShown)
                            + ", and a model here carries \"" + name + "\"");
                    }
                }
            }

            if (pairs.Count == 0)
            {
                lines.Add("no workset name a set of the picked file asks for differs by letter case alone from one a model here carries");
                return;
            }

            lines.Add(pairs.Count + " pair(s) of workset names differ by letter case alone, one asked by a set of the"
                + " picked file and one carried by a model here. The match is CASE SENSITIVE, so a condition asking"
                + " for one spelling finds none of the items that carry the other:");

            for (int i = 0; i < pairs.Count; i++)
            {
                lines.Add(pairs[i]);
            }
        }

        /// <summary>
        /// Whether a workset asked that way finds that carried name as it is spelled, the whole name
        /// or, for contains, a part of it. The one place this is said, read by this check and by the
        /// judge of a set that cannot reach a model of its own team, SilentMisses, F131.
        /// </summary>
        internal static bool WorksetFinds(string asked, bool contains, string carried)
        {
            return contains
                ? carried.IndexOf(asked, StringComparison.Ordinal) >= 0
                : string.Equals(carried, asked, StringComparison.Ordinal);
        }

        /// <summary>Whether that set also asks, not negated, for a workset that finds the carried name exactly, F116.</summary>
        private static bool AlsoFinds(Dictionary<string, List<WorksetAsk>> askedBy, string setName, string carried)
        {
            List<WorksetAsk> asked;

            return askedBy.TryGetValue(setName, out asked) && asked.Exists(one => WorksetFinds(one.Value, one.Contains, carried));
        }

        /// <summary>The sets that ask, as a list, with the rest counted past namesShown the way the names are.</summary>
        private static string SetNames(IList<string> names, int namesShown)
        {
            if (namesShown <= 0 || names.Count <= namesShown)
            {
                return Listed(names);
            }

            List<string> shown = new List<string>();

            for (int i = 0; i < namesShown; i++)
            {
                shown.Add(names[i]);
            }

            shown.Add((names.Count - namesShown) + " more");
            return Listed(shown);
        }

        /// <summary>One workset value the picked file asks for, the way it asks, and the sets asking it in file order.</summary>
        private sealed class WorksetAsk
        {
            internal WorksetAsk(string value, bool contains)
            {
                Value = value;
                Contains = contains;
                Sets = new List<string>();
            }

            internal string Value { get; private set; }

            /// <summary>Whether the file asks for part of a name, test contains, rather than the whole name.</summary>
            internal bool Contains { get; private set; }

            internal List<string> Sets { get; private set; }

            /// <summary>
            /// Whether this ask misses that carried name by letter case alone, and finds it once case
            /// is set aside. The miss is WorksetFinds, the one place a find is said.
            /// </summary>
            internal bool MissesByCaseAlone(string carried)
            {
                bool findsCaseBlind = Contains
                    ? carried.IndexOf(Value, StringComparison.OrdinalIgnoreCase) >= 0
                    : string.Equals(carried, Value, StringComparison.OrdinalIgnoreCase);

                return findsCaseBlind && !WorksetFinds(Value, Contains, carried);
            }
        }

        /// <summary>
        /// Where these models disagree with each other about the name of a workset, Q69,
        /// one line per pair saying which model carries which.
        ///
        /// THIS IS THE HALF THAT MATTERS, and it is why the tool never merges a typo
        /// silently. `EL-Lightining Protection` is a misspelling of `EL-Lightning
        /// Protection` and `AR-EXTERIOR` against `AR-INTERIOR` is two real worksets, and
        /// no rule can tell them apart, 5t. If this tool absorbed the first kind quietly
        /// nobody would ever fix the models and the next building would repeat it.
        /// </summary>
        private static void AddDisagreements(IList<string> lines, IList<ModelExport> models)
        {
            Dictionary<string, IList<string>> byWorkset = new Dictionary<string, IList<string>>(StringComparer.Ordinal);

            for (int i = 0; i < models.Count; i++)
            {
                for (int w = 0; w < models[i].Worksets.Count; w++)
                {
                    string workset = models[i].Worksets[w];

                    if (!byWorkset.ContainsKey(workset))
                    {
                        byWorkset[workset] = new List<string>();
                    }

                    if (!byWorkset[workset].Contains(models[i].File))
                    {
                        byWorkset[workset].Add(models[i].File);
                    }
                }
            }

            IList<WorksetDisagreement> found = WorksetDisagreements.In(byWorkset);

            string alreadyDecided = WorksetDisagreements.DecidedInLastRead == 0
                ? string.Empty
                : " " + WorksetDisagreements.DecidedInLastRead
                    + " more pair(s) are close enough too and are NOT listed, because a person has already"
                    + " decided they are two different worksets.";

            if (found.Count == 0)
            {
                lines.Add("no two workset names in this group are close enough to be one word typed twice."
                    + alreadyDecided);
                return;
            }

            lines.Add(found.Count + " pair(s) of workset names are close enough to be one word typed twice."
                + " NOTHING IS MERGED: a person reads these and fixes the models, and this tool never"
                + " decides which of two spellings is the right one." + alreadyDecided);

            for (int i = 0; i < found.Count; i++)
            {
                lines.Add("   " + found[i].Line());
            }
        }

        /// <summary>
        /// The workset names across the group, in one line a person can hold beside the
        /// matrix. This is the whole point of the block, so it is written even when there
        /// is exactly one, and it says how many were left out when it truncates.
        /// </summary>
        private static void AddWorksets(IList<string> lines, IList<string> worksets, int namesShown)
        {
            if (worksets.Count == 0)
            {
                lines.Add("worksets seen: NONE in any model of this group");
                return;
            }

            int shown = namesShown <= 0 || namesShown > worksets.Count ? worksets.Count : namesShown;
            string[] named = new string[shown];

            for (int i = 0; i < shown; i++)
            {
                named[i] = worksets[i];
            }

            lines.Add("worksets seen: " + string.Join(", ", named)
                + (shown < worksets.Count
                    ? ", and " + (worksets.Count - shown) + " more, counted and not listed"
                    : string.Empty));
        }

        /// <summary>
        /// The closing sentence. EVERY ELEMENT is said only when every model was counted,
        /// holds elements, and carries both on each of them, and otherwise every shape that
        /// kept it from being said is counted in the sentence.
        /// </summary>
        private static string Sentence(
            int models, int withoutAnyWorkset, int withSomeWorkset, int withoutEveryId, int withNoElement, int notCounted)
        {
            if (withoutAnyWorkset == 0 && withSomeWorkset == 0 && withoutEveryId == 0 && withNoElement == 0 && notCounted == 0)
            {
                return "all " + models + " model(s) carry a workset on every element and an element id on every element";
            }

            List<string> said = new List<string>();
            said.Add(withoutAnyWorkset + " of " + models + " model(s) carry no workset at all");

            if (withSomeWorkset > 0)
            {
                said.Add(withSomeWorkset + " carry a workset on only some of their elements");
            }

            if (withoutEveryId > 0)
            {
                said.Add(withoutEveryId + " do not carry an element id on every element");
            }

            if (withNoElement > 0)
            {
                said.Add(withNoElement + " hold no Revit element");
            }

            if (notCounted > 0)
            {
                said.Add(notCounted + " could not be counted");
            }

            return Listed(said) + ". Nothing is changed and the run goes on.";
        }

        /// <summary>Parts read as a list: one alone, two joined by and, more with commas and an and before the last.</summary>
        private static string Listed(IList<string> parts)
        {
            if (parts.Count == 0)
            {
                return string.Empty;
            }

            if (parts.Count == 1)
            {
                return parts[0];
            }

            string[] head = new string[parts.Count - 1];

            for (int i = 0; i < head.Length; i++)
            {
                head[i] = parts[i];
            }

            return string.Join(", ", head) + ", and " + parts[parts.Count - 1];
        }

        private static void Gather(IList<string> into, IList<string> worksets)
        {
            for (int i = 0; i < worksets.Count; i++)
            {
                if (!into.Contains(worksets[i]))
                {
                    into.Add(worksets[i]);
                }
            }
        }

        internal static string Named(ModelExport model)
        {
            string discipline = string.IsNullOrEmpty(model.Discipline) ? "??" : model.Discipline;
            return discipline + "  " + (string.IsNullOrEmpty(model.File) ? "a model with no name" : model.File);
        }

        private static string Count(int value)
        {
            return value == ModelExport.NotCounted ? "UNKNOWN" : value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Share(int share)
        {
            return share == ModelExport.NotCounted ? "UNKNOWN" : share.ToString(CultureInfo.InvariantCulture) + "%";
        }
    }
}
