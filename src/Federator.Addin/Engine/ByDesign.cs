using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using CoreClashStatus = Federator.Core.Clash.ClashStatus;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Reads the name and the status of every clash in one test and asks Core whether the
    /// two sets the test names are a by design connection. F72b.
    ///
    /// IT READS NO ITEM. The pairs file names SETS, and both set names are on the test
    /// itself, off its two locators, so nothing here touches a clash side, a category or
    /// a size. Reading every item once per clash is the walk shape that once built 1.7
    /// million native handles in one group, and this rule needs none of it.
    ///
    /// IT ONLY WALKS AND NEVER WRITES. What it hands back is a list of clash names to
    /// move, which joins the penetration rule's list and goes through the ONE
    /// ClashStatusEditor, exactly as F54 built it. The penetration rule runs first and
    /// OWNS a clash they both want, which Core counts under ThePenetrationRuleHasIt, so
    /// the two blocks add up to the number of clashes that moved rather than to twice it.
    ///
    /// IT IS OFF BY DEFAULT and a run with the box off never calls this at all.
    /// </summary>
    public sealed class ByDesign
    {
        private readonly RunLog log;
        private readonly ByDesignPairs pairs;

        public ByDesign(RunLog log, ByDesignPairs pairs)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
            this.pairs = pairs ?? ByDesignPairs.NothingPicked();
        }

        /// <summary>The file this run judges by, so the run line can say which pairs matched nothing.</summary>
        public ByDesignPairs Pairs
        {
            get { return pairs; }
        }

        /// <summary>
        /// The clashes of one test that should become Reviewed under the pairs file, and
        /// every decision behind the ones left alone, added to the tally handed in. The
        /// names the penetration rule already wants are handed in so that rule keeps them.
        ///
        /// Never throws. A test whose results will not read leaves every clash exactly as
        /// it was and says so.
        /// </summary>
        public IList<WantedStatus> WantedFor(
            ClashTest test,
            string leftLocator,
            string rightLocator,
            IList<WantedStatus> thePenetrationRuleWants,
            ByDesignTally tally)
        {
            List<WantedStatus> wanted = new List<WantedStatus>();

            if (test == null || tally == null)
            {
                return wanted;
            }

            string testName = Words.Or(test.DisplayName, "UNKNOWN test");
            string leftSet = ByDesignRule.SetNameIn(leftLocator);
            string rightSet = ByDesignRule.SetNameIn(rightLocator);
            HashSet<string> penetration = new HashSet<string>(StringComparer.Ordinal);

            if (thePenetrationRuleWants != null)
            {
                foreach (WantedStatus one in thePenetrationRuleWants)
                {
                    if (one != null && !string.IsNullOrEmpty(one.ClashName))
                    {
                        penetration.Add(one.ClashName);
                    }
                }
            }

            try
            {
                Walk(test.Children, testName, leftSet, rightSet, penetration, tally, wanted);
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading the clashes in " + testName + " for the by design pairs",
                    error,
                    "kept going, every clash this test holds is left exactly as it was");
            }

            return wanted;
        }

        /// <summary>
        /// Every result under this test, descending result groups, the same way the editor
        /// and the tally walk. A group is one row in the panel holding several clashes and
        /// the leaves are what carry a status.
        /// </summary>
        private void Walk(
            SavedItemCollection items,
            string testName,
            string leftSet,
            string rightSet,
            HashSet<string> penetration,
            ByDesignTally tally,
            List<WantedStatus> wanted)
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
                        Walk(group.Children, testName, leftSet, rightSet, penetration, tally, wanted);
                        continue;
                    }

                    ClashResult result = item as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    string clashName = Words.Or(result.DisplayName, string.Empty);
                    CoreClashStatus status = (CoreClashStatus)(int)result.Status;
                    ByDesignPair pair;

                    ByDesignVerdict verdict = ByDesignRule.Judge(
                        pairs, leftSet, rightSet, status, penetration.Contains(clashName), out pair);

                    tally.Add(testName, clashName.Length == 0 ? "an unnamed clash" : clashName, verdict, pair);

                    if (verdict == ByDesignVerdict.Reviewed && clashName.Length > 0)
                    {
                        wanted.Add(new WantedStatus(clashName, CoreClashStatus.Reviewed));
                    }
                }
            }
        }
    }
}
