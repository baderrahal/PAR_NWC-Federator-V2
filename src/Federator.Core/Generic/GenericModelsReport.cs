using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Sets;

namespace Federator.Core.Generic
{
    /// <summary>One model's Generic Models count, F128. A count nobody took is UNKNOWN and never nought.</summary>
    public sealed class GenericModelCount
    {
        /// <summary>The count of a model whose set was not counted, UNKNOWN and not zero.</summary>
        public const int NotCountedItems = SetResult.NotCounted;

        internal GenericModelCount(string modelName, int items, string whyNotCounted)
        {
            ModelName = modelName;
            Items = items;
            WhyNotCounted = whyNotCounted ?? string.Empty;
        }

        public string ModelName { get; private set; }

        /// <summary>How many items of the category the model holds, or NotCountedItems.</summary>
        public int Items { get; private set; }

        public bool Counted
        {
            get { return Items >= 0; }
        }

        /// <summary>Why no count was taken, empty where one was.</summary>
        public string WhyNotCounted { get; private set; }
    }

    /// <summary>
    /// How many Generic Models items each model of a group holds, F128 and FR-177, Bader's request 3
    /// under Q112: each model file with its count, and a model with none left out. It is read off the
    /// results of the sets the plan built, one set a model, so the count of a model is the items its
    /// set found and nothing else counts them a second time.
    ///
    /// A COUNT NOBODY TOOK IS UNKNOWN AND IS NEVER NOUGHT, and a model it belongs to is neither left
    /// out nor called empty. A set that was not built, a count that was not taken, a set the results do
    /// not hold and two sets of one path are each a model not counted with the reason, and the total says
    /// at least where any model was not counted, because a sum beside a count nobody took is a lower
    /// bound. Only a model counted at nought is left out of the list, and it is counted in the line above.
    ///
    /// THE LINES ARE THE GENERIC BLOCK, with the plan's notes carried through, and the rows are the
    /// ones the sheet writes, so the two cannot say one model two ways. No clash test is made for any
    /// of these sets, and the block says so.
    /// </summary>
    public sealed class GenericModelsReport
    {
        /// <summary>The title of a group's block, which the add-in puts the building after.</summary>
        public const string BlockTitle = "GENERIC MODELS";

        /// <summary>The word for a number nobody took.</summary>
        public const string Unknown = "UNKNOWN";

        private readonly List<GenericModelCount> counts = new List<GenericModelCount>();

        private GenericModelsReport(GenericModelsPlan plan)
        {
            Asked = plan.Asked;
            Notes = plan.Notes;
            SetsOverlap = plan.TextsMeet;
            Counts = new ReadOnlyCollection<GenericModelCount>(counts);
        }

        /// <summary>What every set asked of the category.</summary>
        public string Asked { get; private set; }

        /// <summary>What the plan noticed, a line each.</summary>
        public ReadOnlyCollection<string> Notes { get; private set; }

        /// <summary>
        /// True where the text of one set also finds the items another set finds, so an item can be in two
        /// counts and the total over the sets is not a count of items.
        /// </summary>
        public bool SetsOverlap { get; private set; }

        /// <summary>
        /// What a count of nought means, said under the models line wherever one model was counted at nought, because
        /// a model left out of the list is also one whose items carry another text than the one looked for, and the
        /// output cannot tell the two apart.
        /// </summary>
        public const string NoughtMeans =
            "a model counted at nought holds no item of the category whose Source File contains the text its set looks for, "
            + "which is its file name without the extension unless another text was handed in";

        /// <summary>Said beside NoughtMeans where every model was counted at nought, because that is also the count a wrong question gives.</summary>
        public const string EveryModelAtNought =
            "every model was counted at nought, which is also the count a category value or a Source File text that no item carries gives";

        /// <summary>Every model of the group, in the plan's order, counted or not.</summary>
        public ReadOnlyCollection<GenericModelCount> Counts { get; private set; }

        public int Models
        {
            get { return counts.Count; }
        }

        /// <summary>The models counted at one item or more.</summary>
        public int WithItems { get; private set; }

        /// <summary>The models counted at nought, which the list leaves out.</summary>
        public int WithNone { get; private set; }

        /// <summary>The models whose count nobody took.</summary>
        public int NotCounted { get; private set; }

        /// <summary>
        /// The items of the models that were counted, added over the sets. A lower bound where any model was not,
        /// and not a count of items where SetsOverlap, because an item two sets find is in both counts.
        /// </summary>
        public long Items { get; private set; }

        /// <summary>
        /// The report for a plan and the results of its sets. A result is found by the full path of the
        /// set, compared exactly. A null results list is a group whose sets were not built, and counts
        /// every model as not counted.
        /// </summary>
        public static GenericModelsReport From(GenericModelsPlan plan, IEnumerable<SetResult> results)
        {
            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            GenericModelsReport report = new GenericModelsReport(plan);
            Dictionary<string, List<SetResult>> byPath = new Dictionary<string, List<SetResult>>(StringComparer.Ordinal);

            if (results != null)
            {
                foreach (SetResult result in results)
                {
                    if (result == null || result.Path == null)
                    {
                        continue;
                    }

                    List<SetResult> held;

                    if (!byPath.TryGetValue(result.Path, out held))
                    {
                        held = new List<SetResult>();
                        byPath.Add(result.Path, held);
                    }

                    held.Add(result);
                }
            }

            foreach (GenericModelSet model in plan.Sets)
            {
                List<SetResult> found;
                GenericModelCount count;

                if (!byPath.TryGetValue(model.Set.Path, out found))
                {
                    count = new GenericModelCount(model.ModelName, GenericModelCount.NotCountedItems,
                        "its set was not built, or no result of it was handed in");
                }
                else if (found.Count > 1)
                {
                    count = new GenericModelCount(model.ModelName, GenericModelCount.NotCountedItems,
                        "the path of its set holds " + found.Count + " results, so which one is its count is not known");
                }
                else if (!string.IsNullOrEmpty(found[0].Error))
                {
                    count = new GenericModelCount(model.ModelName, GenericModelCount.NotCountedItems, "its set could not be built");
                }
                else if (found[0].ItemCount < 0)
                {
                    count = new GenericModelCount(model.ModelName, GenericModelCount.NotCountedItems, "the count of its set was not taken");
                }
                else if (found[0].Present)
                {
                    // BY THE ONE DRIFT RULE, SetDrift.Compare on the keys, and never the two prose strings:
                    // the question read off a set carries no display names and the plan's carries them,
                    // so comparing the words called every present set on a weekly rerun a set asking
                    // another question, the readers' finding on attempt 1.
                    SetDrift drift = SetDrift.Compare(found[0].AskedConditions, model.Set);

                    if (drift.CouldNotRead)
                    {
                        count = new GenericModelCount(model.ModelName, GenericModelCount.NotCountedItems,
                            "its set was already in the document and the question it asks was not read, so its count may answer another question");
                    }
                    else if (drift.Drifted)
                    {
                        count = new GenericModelCount(model.ModelName, GenericModelCount.NotCountedItems,
                            "its set was already in the document and asks something other than the plan asks (it asks: "
                            + drift.AskedNow() + ". the plan asks: " + drift.WantedNow() + "), so its count answers another question");
                    }
                    else
                    {
                        count = new GenericModelCount(model.ModelName, found[0].ItemCount, null);
                    }
                }
                else
                {
                    count = new GenericModelCount(model.ModelName, found[0].ItemCount, null);
                }

                report.Add(count);
            }

            return report;
        }

        private void Add(GenericModelCount count)
        {
            counts.Add(count);

            if (!count.Counted)
            {
                NotCounted++;
            }
            else if (count.Items == 0)
            {
                WithNone++;
            }
            else
            {
                WithItems++;
                Items += count.Items;
            }
        }

        /// <summary>
        /// The GENERIC block: what was asked, how many models of each kind, the items in all, each model
        /// that found some with its count, each model not counted with why, the plan's notes, and that no
        /// clash test is made for these sets. A model counted at nought is in the line above and not in the list.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            lines.Add("asked   : " + Asked + ", found in each model by its Source File");

            if (counts.Count == 0)
            {
                lines.Add("models  : none, the plan holds no model of this group, so nothing was counted");
            }
            else
            {
                lines.Add("models  : " + counts.Count + (counts.Count == 1 ? " model" : " models") + " in this group, "
                    + WithItems + " found some, " + WithNone + " found none and are left out, "
                    + NotCounted + (NotCounted == 1 ? " was" : " were") + " not counted");
                lines.Add("items   : " + ItemsPhrase());

                if (WithNone > 0)
                {
                    lines.Add("nought  : " + NoughtMeans);
                }

                if (WithNone == counts.Count)
                {
                    lines.Add("nought  : " + EveryModelAtNought);
                }
            }

            foreach (GenericModelCount count in counts)
            {
                if (count.Counted && count.Items > 0)
                {
                    lines.Add("    " + count.ModelName + "  " + count.Items.ToString("#,##0", CultureInfo.InvariantCulture));
                }
            }

            foreach (GenericModelCount count in counts)
            {
                if (!count.Counted)
                {
                    lines.Add("    " + count.ModelName + "  " + Unknown + ", " + count.WhyNotCounted);
                }
            }

            foreach (string note in Notes)
            {
                lines.Add("note    : " + note);
            }

            lines.Add(NoClashTest);
            return lines;
        }

        /// <summary>The last line of every block, so a reader sees these sets are counted and never clashed.</summary>
        public const string NoClashTest = "no clash test is made for these sets";

        /// <summary>
        /// The block of a group whose plan, build or count threw, F128 attempt 2, the lead's decision: a
        /// FAILED line in the block, the count UNKNOWN, and the group keeps its own result, as a report
        /// check never fails a group. The line says what threw and what happens next.
        /// </summary>
        public static IList<string> FailedLines(string what, string error)
        {
            return new List<string>
            {
                "FAILED  " + (string.IsNullOrEmpty(what) ? "the Generic Models count" : what) + " threw "
                    + (string.IsNullOrEmpty(error) ? Unknown : error)
                    + ", so no model of this group is counted, its count is " + Unknown
                    + " and the group keeps its own result",
                NoClashTest
            };
        }

        /// <summary>
        /// The total said as what it is. Where the texts of some sets meet it is not a count of items and not a
        /// lower bound, because an item two sets find is in both counts.
        /// </summary>
        private string ItemsPhrase()
        {
            string total = Items.ToString("#,##0", CultureInfo.InvariantCulture);

            if (NotCounted == counts.Count)
            {
                return Unknown + ", no model was counted";
            }

            if (SetsOverlap)
            {
                return total + " added over the sets, which is not a count of items, because the texts of some sets meet and an item"
                    + " both find is in both counts" + (NotCounted > 0 ? ", and the models not counted are not in it" : string.Empty);
            }

            return (NotCounted > 0 ? "at least " : string.Empty) + total + " in all"
                + (NotCounted > 0 ? ", which is the models counted and not the ones that were not" : string.Empty);
        }
    }
}
