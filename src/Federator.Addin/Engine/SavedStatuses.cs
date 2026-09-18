using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Clash;
using CoreClashStatus = Federator.Core.Clash.ClashStatus;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// What every clash result in the open document is sitting at, counted by status.
    ///
    /// F50. This is the part of the NWF with no second copy anywhere. A result at New is
    /// what running a test produces and the next run makes it again, so losing one costs
    /// nothing. A result somebody moved to Active, Reviewed, Approved or Resolved is a
    /// decision made while looking at the model, and this tool runs weekly, so a rebuild
    /// that lost them would throw away however many weeks of review the file has collected.
    /// The rebuild counts them out and counts them back, the same as the sets and the tests.
    ///
    /// WHY THIS IS NOT PART OF SavedTests.Read. Reading the tests walks the tests. Counting
    /// the statuses walks every RESULT of every test, which on the reference file is 1830
    /// tests worth, so the two are separate calls and a caller that wants the cheap one
    /// does not pay for the expensive one.
    ///
    /// A ClashTest is itself a GroupItem holding its results, so it is tested for before a
    /// folder is, or the walk would descend into the results twice. Every wrapper is
    /// disposed on the way out, because a SavedItem is an eEXTERNAL handle onto something
    /// the document owns.
    /// </summary>
    public static class SavedStatuses
    {
        /// <summary>
        /// Every result in the document counted by status. An empty tally where there is no
        /// document, because a count of nothing is a real answer and a throw here would stop
        /// a rebuild over a diagnostic.
        /// </summary>
        public static ClashTally In(Document document)
        {
            ClashTally tally = new ClashTally();

            if (document == null)
            {
                return tally;
            }

            DocumentClashTests clashTests = document.GetClash().TestsData;
            Walk(clashTests.Tests, tally);
            return tally;
        }

        /// <summary>
        /// How many results carry a status somebody chose. Which statuses those are is
        /// Federator.Core.Clash.StatusesAPersonSet, so the rule is testable and lives in
        /// one place rather than as a comparison written here.
        /// </summary>
        public static int SetByAPerson(Document document)
        {
            return StatusesAPersonSet.In(In(document));
        }

        private static void Walk(SavedItemCollection items, ClashTally tally)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    ClashTest test = item as ClashTest;

                    if (test != null)
                    {
                        CountResults(test.Children, tally);
                        continue;
                    }

                    GroupItem folder = item as GroupItem;

                    if (folder != null)
                    {
                        Walk(folder.Children, tally);
                    }
                }
            }
        }

        /// <summary>
        /// The results of one test. A result group is one row in the panel holding several
        /// clashes, so the leaves are counted rather than the group counting as one, which
        /// is what keeps this number agreeing with the Clash Detective panel.
        /// </summary>
        private static void CountResults(SavedItemCollection items, ClashTally tally)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    ClashResultGroup group = item as ClashResultGroup;

                    if (group != null)
                    {
                        CountResults(group.Children, tally);
                        continue;
                    }

                    ClashResult result = item as ClashResult;

                    if (result != null)
                    {
                        tally.Add((CoreClashStatus)(int)result.Status);
                    }
                }
            }
        }
    }
}
