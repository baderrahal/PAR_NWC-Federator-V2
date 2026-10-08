using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Generic
{
    /// <summary>
    /// The Generic Models counts added up over every group of a run, F128 and FR-177, for the one line of
    /// the RESULT block, in the shape the other run lines keep. Each group's report is added as the group
    /// finishes its sets step, so the line and the per group blocks come from the same additions.
    ///
    /// A COUNT NOBODY TOOK IS UNKNOWN AND NEVER NOUGHT, here as in the block: a group that reached no count,
    /// because its sets step threw or there was no document, is counted as a group not counted and never as
    /// nought items, and the items are said as at least where any model of any group was not counted. Where
    /// the texts of some sets met in any group the total is not a count of items, and the line says so,
    /// because an item two sets find is in both counts.
    /// </summary>
    public sealed class GenericModelsAcrossTheRun
    {
        /// <summary>The label of the RESULT line, padded as the other labels of the block are.</summary>
        public const string Label = "generic models : ";

        private readonly List<string> groupsNotCounted = new List<string>();

        /// <summary>The groups whose report was handed in.</summary>
        public int Groups { get; private set; }

        /// <summary>The models of those groups, counted or not.</summary>
        public int Models { get; private set; }

        /// <summary>The models counted at one item or more.</summary>
        public int WithItems { get; private set; }

        /// <summary>The models counted at nought.</summary>
        public int WithNone { get; private set; }

        /// <summary>The models whose count nobody took.</summary>
        public int NotCounted { get; private set; }

        /// <summary>The items over the models counted, a lower bound where any model was not counted.</summary>
        public long Items { get; private set; }

        /// <summary>The groups whose sets' texts met, so an item can be in two counts.</summary>
        public int GroupsWhoseTextsMeet { get; private set; }

        /// <summary>The groups that reached no count at all, by name.</summary>
        public IList<string> GroupsNotCounted
        {
            get { return new List<string>(groupsNotCounted); }
        }

        /// <summary>One group's report, as its sets step made it.</summary>
        public void Add(GenericModelsReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            Groups++;
            Models += report.Models;
            WithItems += report.WithItems;
            WithNone += report.WithNone;
            NotCounted += report.NotCounted;
            Items += report.Items;

            if (report.SetsOverlap)
            {
                GroupsWhoseTextsMeet++;
            }
        }

        /// <summary>A group that reached no count, so nothing of it is in the numbers and the line names it.</summary>
        public void AddNotCounted(string group)
        {
            groupsNotCounted.Add(string.IsNullOrEmpty(group) ? GenericModelsReport.Unknown : group);
        }

        /// <summary>The one line of RESULT, or the line for a run whose sets step reached no group.</summary>
        public string ResultLine()
        {
            if (Groups == 0 && groupsNotCounted.Count == 0)
            {
                return Label + GenericModelsReport.Unknown + ", no group reached the sets step, so nothing was counted";
            }

            string line;

            if (Groups == 0)
            {
                line = Label + GenericModelsReport.Unknown + ", no group was counted";
            }
            else if (NotCounted == Models)
            {
                line = Label + GenericModelsReport.Unknown + ", " + Count(Models, "model") + " in " + Count(Groups, "group")
                    + " and none was counted";
            }
            else
            {
                string items = Items.ToString("#,##0", CultureInfo.InvariantCulture);
                string total = GroupsWhoseTextsMeet > 0
                    ? items + " added over the sets, which is not a count of items because the texts of some sets meet in "
                        + Count(GroupsWhoseTextsMeet, "group")
                    : (NotCounted > 0 ? "at least " : string.Empty) + items + " items";

                line = Label + total + " in " + Count(WithItems, "model") + " of " + Models + " over " + Count(Groups, "group")
                    + ", " + WithNone + " at nought, " + NotCounted + " not counted";
            }

            if (groupsNotCounted.Count > 0)
            {
                line += ", and no count was taken in " + Count(groupsNotCounted.Count, "group") + ": "
                    + string.Join(", ", groupsNotCounted.ToArray());
            }

            return line;
        }

        /// <summary>The line RESULT writes where no tally was handed in, because a missing line reads as a count that did not run.</summary>
        public static string NoneTaken()
        {
            return Label + GenericModelsReport.Unknown + ", no Generic Models count was handed in";
        }

        private static string Count(int number, string one)
        {
            return number + " " + one + (number == 1 ? string.Empty : "s");
        }
    }
}
