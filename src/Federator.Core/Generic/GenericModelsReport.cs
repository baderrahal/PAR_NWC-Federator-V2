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
            Counts = new ReadOnlyCollection<GenericModelCount>(counts);
        }

        /// <summary>What every set asked of the category.</summary>
        public string Asked { get; private set; }

        /// <summary>What the plan noticed, a line each.</summary>
        public ReadOnlyCollection<string> Notes { get; private set; }

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

        /// <summary>The items of the models that were counted. A lower bound where any model was not.</summary>
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
        /// that holds some with its count, each model not counted with why, the plan's notes, and that no
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
                lines.Add("models  : " + Counted(counts.Count, "model", "models") + " in this group, "
                    + WithItems + " hold some, " + WithNone + " hold none and are left out, "
                    + NotCounted + (NotCounted == 1 ? " was" : " were") + " not counted");
                lines.Add("items   : " + (NotCounted > 0 ? "at least " : string.Empty)
                    + Items.ToString("#,##0", CultureInfo.InvariantCulture) + " in all"
                    + (NotCounted > 0 ? ", which is the models counted and not the ones that were not" : string.Empty));
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

            lines.Add("no clash test is made for these sets");
            return lines;
        }

        private static string Counted(int count, string one, string many)
        {
            return count + " " + (count == 1 ? one : many);
        }
    }
}
