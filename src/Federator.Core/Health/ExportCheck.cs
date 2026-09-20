using System;
using System.Collections.Generic;
using System.Globalization;

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
            Worksets = worksets ?? new List<string>();
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

        /// <summary>The distinct workset names, in the order they were first seen.</summary>
        public IList<string> Worksets { get; private set; }

        /// <summary>Whether any element in this model carries a workset at all.</summary>
        public bool CarriesAWorkset
        {
            get { return WithWorkset > 0; }
        }

        /// <summary>The share of elements carrying an id, as whole per cent, or minus one where it could not be worked out.</summary>
        public int IdShare
        {
            get
            {
                if (Elements <= 0 || WithElementId == NotCounted)
                {
                    return NotCounted;
                }

                return (int)Math.Round(100.0 * WithElementId / Elements, MidpointRounding.AwayFromZero);
            }
        }
    }

    /// <summary>
    /// Two faults that are invisible until a set finds nothing or a report column comes
    /// out blank, put in front of a person on every run instead of behind a button.
    ///
    /// THE WORKSET. Every mechanical set in the client's matrix filters on the Workset
    /// parameter, and 33 of the 61 sets found nothing in every group of the run of
    /// 2026-09-20. So this says, per model, whether any element carries a workset at all
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
        /// How many workset names are listed before the rest are counted. Ten, the same
        /// number SetsAcrossTheRun names, and the block SAYS it truncated rather than
        /// leaving a reader to wonder.
        /// </summary>
        public const int NamesShown = 10;

        /// <summary>The block, written even when everything is right, because a missing block reads as a check that did not run.</summary>
        public static IList<string> Lines(IList<ModelExport> models)
        {
            return Lines(models, NamesShown);
        }

        public static IList<string> Lines(IList<ModelExport> models, int namesShown)
        {
            List<string> lines = new List<string>();

            if (models == null || models.Count == 0)
            {
                lines.Add("no model was read, so nothing could be checked");
                return lines;
            }

            int withoutAnyWorkset = 0;
            int withoutEveryId = 0;
            List<string> everyWorkset = new List<string>();

            for (int i = 0; i < models.Count; i++)
            {
                ModelExport model = models[i];

                lines.Add("   " + Named(model)
                    + "   elements " + Count(model.Elements)
                    + "   worksets " + (model.CarriesAWorkset ? model.Worksets.Count.ToString(CultureInfo.InvariantCulture) : "NONE")
                    + "   element id " + Share(model.IdShare));

                if (!model.CarriesAWorkset && model.Elements > 0)
                {
                    withoutAnyWorkset++;
                    lines.Add("      no element carries a workset. Every set that filters on workset will find nothing here");
                }

                if (model.IdShare != ModelExport.NotCounted && model.IdShare < 100)
                {
                    withoutEveryId++;
                    lines.Add("      re-export with Convert element Ids switched on, or "
                        + Count(model.Elements - model.WithElementId) + " element(s) reach the report with an empty id cell");
                }

                Gather(everyWorkset, model.Worksets);
            }

            lines.Add(Sentence(models.Count, withoutAnyWorkset, withoutEveryId));
            AddWorksets(lines, everyWorkset, namesShown);
            AddDisagreements(lines, models);
            return lines;
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
            lines.Add("   check these against the workset each set in the matrix asks for. The match is CASE SENSITIVE,"
                + " so ME-Ductwork does not match ME-DUCTWORK and that set finds nothing");
        }

        private static string Sentence(int models, int withoutAnyWorkset, int withoutEveryId)
        {
            if (withoutAnyWorkset == 0 && withoutEveryId == 0)
            {
                return "all " + models + " model(s) carry a workset on every element and an element id on every element";
            }

            string said = withoutAnyWorkset + " of " + models + " model(s) carry no workset at all";

            if (withoutEveryId > 0)
            {
                said += ", and " + withoutEveryId + " do not carry an element id on every element";
            }

            return said + ". Nothing is changed and the run goes on.";
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

        private static string Named(ModelExport model)
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
