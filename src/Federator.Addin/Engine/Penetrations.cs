using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Views;
using CoreClashStatus = Federator.Core.Clash.ClashStatus;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Reads both sides of every clash in one test and asks Core whether it is a
    /// penetration. F72, and the answer to Q33.
    ///
    /// THE ADD-IN READS AND CORE DECIDES. Nothing here knows what 150 is, which categories
    /// are a service, or which statuses may move. It reads a category off each side, reads
    /// the sizes off the service, hands the two sides and the status to
    /// Federator.Core.Clash.PenetrationRule, and collects what comes back. Every one of
    /// those rules is tested without Navisworks anywhere near it.
    ///
    /// IT ONLY WALKS AND NEVER WRITES. What it hands back is a list of clash names to move,
    /// which ClashStatusEditor applies through the one measured mutator, exactly as F54
    /// built it. Two walks rather than one is the price of keeping one place that calls
    /// TestsEditResultStatus, and it is worth paying: that mutator is a copy form which
    /// kills the handle given to it, and a walk that both read and wrote would be holding
    /// a handle across it.
    ///
    /// WHY THE NAME IS WHAT IS HANDED BACK. Within one test a clash name is unique, which
    /// is how Clash Detective numbers them, and the editor applies per test. Nothing here
    /// matches a name across two tests.
    ///
    /// IT IS OFF BY DEFAULT and a run with the box off never calls this at all, so a
    /// weekly run costs exactly what it cost before F72.
    /// </summary>
    public sealed class Penetrations
    {
        private readonly RunLog log;
        private readonly PenetrationSettings settings;
        private readonly SizeSettings sizes;

        public Penetrations(RunLog log, PenetrationSettings settings, SizeSettings sizes)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
            this.settings = settings ?? new PenetrationSettings();
            this.sizes = sizes ?? new SizeSettings();
        }

        /// <summary>
        /// Everything this run has decided, across every group. One tally for the whole
        /// run so the RESULT block can say how many statuses the run moved in total.
        /// </summary>
        public PenetrationTally Tally { get; private set; }

        /// <summary>
        /// The clashes of one test that should become Reviewed, and the decisions behind
        /// every one it left alone, added to the tally handed in.
        ///
        /// Never throws. A side that cannot be read is a side with no category, which the
        /// rule answers as not a penetration, so a property that will not read leaves a
        /// clash at New rather than taking the test down.
        /// </summary>
        public IList<WantedStatus> WantedFor(
            Document document, ClashTest test, PenetrationTally tally)
        {
            List<WantedStatus> wanted = new List<WantedStatus>();

            if (document == null || test == null || tally == null)
            {
                return wanted;
            }

            string unitEnumName = UnitEnumName(document);
            string testName = Words.Or(test.DisplayName, "UNKNOWN test");

            try
            {
                Walk(test.Children, testName, unitEnumName, tally, wanted);
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading the categories of the clashes in " + testName,
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
            string unitEnumName,
            PenetrationTally tally,
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
                        Walk(group.Children, testName, unitEnumName, tally, wanted);
                        continue;
                    }

                    ClashResult result = item as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    One(result, testName, unitEnumName, tally, wanted);
                }
            }
        }

        private void One(
            ClashResult result,
            string testName,
            string unitEnumName,
            PenetrationTally tally,
            List<WantedStatus> wanted)
        {
            string clashName = Words.Or(result.DisplayName, string.Empty);

            if (clashName.Length == 0)
            {
                // Nothing to address it by, so nothing could be applied to it even if the
                // rule said yes. Counted as looked at, because it was.
                tally.Add(testName, "an unnamed clash", NotAPenetration());
                return;
            }

            PenetrationSide first = SideOf(result.Selection1, unitEnumName);
            PenetrationSide second = SideOf(result.Selection2, unitEnumName);
            CoreClashStatus status = (CoreClashStatus)(int)result.Status;

            PenetrationDecision decision =
                PenetrationRule.Decide(first, second, status, settings, sizes);

            tally.Add(testName, clashName, decision);

            if (decision.Moves)
            {
                wanted.Add(new WantedStatus(clashName, CoreClashStatus.Reviewed));
            }
        }

        /// <summary>
        /// One side of a clash, read off the first item it holds.
        ///
        /// THE FIRST ITEM AND NOT EVERY ITEM. A clash side is one geometry item on a
        /// Revit sourced NWC, and reading every item of a ModelItemCollection once per
        /// clash over 1830 tests is the shape of walk that once built 1.7 million handles
        /// in a group. Where a side somehow holds several, the first is the one the panel
        /// shows, which is the one a person looking at this clash sees.
        /// </summary>
        private PenetrationSide SideOf(ModelItemCollection selection, string unitEnumName)
        {
            if (selection == null || selection.Count == 0)
            {
                return new PenetrationSide(string.Empty, string.Empty, null);
            }

            ModelItem item = selection[0];

            if (item == null)
            {
                return new PenetrationSide(string.Empty, string.Empty, null);
            }

            string name = Words.Or(item.DisplayName, string.Empty);
            string category = CategoryOf(item);
            double? largest = LargestOf(item, unitEnumName);

            return new PenetrationSide(name, category, largest);
        }

        /// <summary>
        /// The item category, looked for on the item and then up its ancestors, which is
        /// the same walk the harvest uses and for the same reason: a clash gives back the
        /// geometry leaf, and on a Revit sourced NWC that leaf carries a material name and
        /// no Revit properties at all.
        ///
        /// Its own try, because one property that will not read must not cost the size
        /// read after it. That rule was written after a single throw lost three columns of
        /// every row on one run.
        /// </summary>
        private string CategoryOf(ModelItem item)
        {
            try
            {
                IList<ModelItem> lookIn = Upwards(item);

                for (int i = 0; i < lookIn.Count; i++)
                {
                    string found = ClashHarvest.FirstPropertyOn(lookIn[i], settings.CategoryNames);

                    if (found.Length > 0)
                    {
                        return found;
                    }
                }
            }
            catch (Exception)
            {
                // No category is a real answer meaning not a penetration, so a property
                // that will not read leaves the clash exactly as it is.
            }

            return string.Empty;
        }

        /// <summary>
        /// The largest size property on the item, in millimetres, or null.
        ///
        /// The reading is ItemSizes, which F53 already wrote, and the reduction and the
        /// unit conversion are SizeRule, which F53 already wrote too. Nothing here does
        /// either, so the two features cannot disagree about what a size is.
        /// </summary>
        private double? LargestOf(ModelItem item, string unitEnumName)
        {
            try
            {
                IList<ModelItem> lookIn = Upwards(item);

                for (int i = 0; i < lookIn.Count; i++)
                {
                    IDictionary<string, double> read = ItemSizes.Read(lookIn[i], sizes);

                    if (read.Count == 0)
                    {
                        continue;
                    }

                    return SizeRule.LargestMillimetres(read, unitEnumName, sizes);
                }
            }
            catch (Exception)
            {
                // A unit the table does not know throws out of SizeRule, and so does a
                // property that will not read. Either way no size could be read, which the
                // rule answers by LEAVING THE CLASH ALONE. That is the opposite of F53,
                // which INCLUDES an item it could not measure, and the two are deliberately
                // different: the safe mistake there is showing something unnecessary in a
                // viewpoint, and the safe mistake here is leaving a clash at New for a
                // person to look at. Do not make them agree.
            }

            return null;
        }

        /// <summary>
        /// The item, then the element it belongs to, then up the tree, capped so a deep
        /// tree cannot turn one clash into a long walk. The same shape the harvest uses.
        /// </summary>
        private static IList<ModelItem> Upwards(ModelItem item)
        {
            List<ModelItem> chain = new List<ModelItem>();

            if (item == null)
            {
                return chain;
            }

            chain.Add(item);

            ModelItem walker = item;

            for (int level = 0; level < LookUpLevels && walker != null; level++)
            {
                walker = walker.Parent;

                if (walker != null)
                {
                    chain.Add(walker);
                }
            }

            return chain;
        }

        /// <summary>
        /// How far up a category or a size is looked for. The same depth the harvest uses
        /// for an element id, because it is the same tree and the same reason.
        /// </summary>
        private const int LookUpLevels = 4;

        private PenetrationDecision NotAPenetration()
        {
            return PenetrationRule.Decide(
                new PenetrationSide(string.Empty, string.Empty, null),
                new PenetrationSide(string.Empty, string.Empty, null),
                CoreClashStatus.New,
                settings,
                sizes);
        }

        /// <summary>
        /// The Navisworks units enum NAME, which is what UnitTable keys on.
        ///
        /// NOT ClashRunner.UnitName, which hands back the exchange code, m or mm, for the
        /// tolerance conversion. Feeding that to UnitTable.ByEnumName would throw on every
        /// clash, so the enum's own name is read here and nowhere else.
        /// </summary>
        private static string UnitEnumName(Document document)
        {
            return document == null ? string.Empty : document.Units.ToString();
        }
    }
}
