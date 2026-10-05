using System;
using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// A mirror already saved in the NWF, F132, FR-183, Bader's Q114 point 7: in an NWF that
    /// already holds a test and its mirror, the mirror is not run. One this tool created
    /// whose results carry no status a person set is removed from the NWF, the one place
    /// this tool removes a test, by his word. One that carries a person's status is left,
    /// not run, and named for Bader.
    ///
    /// WHAT PROVES THIS TOOL CREATED IT. This tool creates a test off the picked XML, under
    /// the XML's name and with the XML's two sets. So a saved test is taken as this tool's
    /// only where its name and both its locators equal a mirror of the picked XML exactly,
    /// Ordinal. Nothing else on a test says who made it, so with no XML picked nothing
    /// proves it and nothing is ever removed, and a swap the picked XML does not hold was
    /// made by a person or by an older XML and is left. A saved side that was not read,
    /// MirrorRule.BothSidesRead, proves nothing either, so a name the XML calls a mirror
    /// whose sides were not read is left and its line says UNKNOWN. By its sides alone such
    /// a test is never a mirror.
    ///
    /// WHOSE A STATUS IS, Q122's default A. Every status but New counts as a person's,
    /// `StatusesAPersonSet`, with one exception its record proves: a Reviewed carrying this
    /// tool's own record still reading as ours, the undo's own judge,
    /// `AutoReviewRecord.MayUndo`. So a mirror whose results a rerun moved to Active or
    /// Resolved is left. A result that could not be read is left as well, because a status
    /// nobody read is not a status nobody set. A test with no results carries no status.
    ///
    /// FAIL CLOSED. Core cannot tell a test with no results from a walk of its results that
    /// never ran or stopped part way, so nothing is removed until the add-in says the walk
    /// reached its end, AllResultsAdded, after the last result.
    ///
    /// WHICH SAVED TESTS ARE MIRRORS. With an XML picked, the XML decides what this run
    /// creates and runs, so a saved test is a mirror where its name is a mirror of the
    /// XML's, and a test the XML holds and keeps is never one, whatever order the NWF saved
    /// them in. A saved test the XML does not hold is a mirror where its two sets are a test
    /// of the XML that runs swapped, that test then being the one kept whatever the
    /// priorities, since only the XML's tests run, or where they are another such saved
    /// test's swapped and the rule keeps the other. With no XML the rule over the saved
    /// tests decides, in the order the document holds them.
    /// </summary>
    public sealed class MirrorInDocument
    {
        private readonly string keptName;
        private readonly bool xmlPicked;
        private readonly MirrorPair ofThePickedXml;
        private readonly ClashTally statuses = new ClashTally();
        private readonly List<string> notRead = new List<string>();
        private int setByAPerson;
        private int reviewedByThisTool;
        private bool walkComplete;

        private MirrorInDocument(PlannedClashTest saved, string keptName, bool xmlPicked, MirrorPair ofThePickedXml)
        {
            Saved = saved;
            this.keptName = keptName;
            this.xmlPicked = xmlPicked;
            this.ofThePickedXml = ofThePickedXml;
        }

        /// <summary>The test as it is saved in the document, carrying the address it is found at.</summary>
        public PlannedClashTest Saved { get; private set; }

        /// <summary>
        /// Every saved test that is a mirror, each to be judged once its results are added.
        /// The saved tests are the document's, ClashTestPlan.FromDocument's buildable ones,
        /// in the order the document holds them. The picked XML's rule is the one its own
        /// plan was built with, or null where no XML was picked.
        /// </summary>
        public static IList<MirrorInDocument> Find(
            IList<PlannedClashTest> saved, MirrorRule ofThePickedXml, PriorityMap priorities)
        {
            if (priorities == null)
            {
                throw new ArgumentNullException("priorities");
            }

            List<MirrorInDocument> found = new List<MirrorInDocument>();
            List<PlannedClashTest> all = new List<PlannedClashTest>();

            if (saved != null)
            {
                foreach (PlannedClashTest test in saved)
                {
                    if (test != null)
                    {
                        all.Add(test);
                    }
                }
            }

            if (ofThePickedXml == null)
            {
                foreach (MirrorPair pair in MirrorRule.Of(all, priorities).Pairs)
                {
                    found.Add(new MirrorInDocument(pair.Mirror, pair.Kept.Name, false, null));
                }

                return found;
            }

            List<PlannedClashTest> outsideTheXml = new List<PlannedClashTest>();

            foreach (PlannedClashTest test in all)
            {
                MirrorPair xmlPair = ofThePickedXml.PairWhoseMirrorIsNamed(test.Name);

                if (xmlPair != null)
                {
                    found.Add(new MirrorInDocument(test, xmlPair.Kept.Name, true, xmlPair));
                    continue;
                }

                if (ofThePickedXml.Holds(test.Name))
                {
                    continue;
                }

                PlannedClashTest run = ofThePickedXml.RunTestSwappedFrom(test);

                if (run != null)
                {
                    found.Add(new MirrorInDocument(test, run.Name, true, null));
                    continue;
                }

                outsideTheXml.Add(test);
            }

            foreach (MirrorPair pair in MirrorRule.Of(outsideTheXml, priorities).Pairs)
            {
                found.Add(new MirrorInDocument(pair.Mirror, pair.Kept.Name, true, null));
            }

            return found;
        }

        /// <summary>One result of the saved test, its status and its comment as the document holds them.</summary>
        public void AddResult(ClashStatus status, string comment)
        {
            walkComplete = false;
            statuses.Add(status);

            if (AutoReviewRecord.MayUndo(comment, status))
            {
                reviewedByThisTool++;
            }
            else if (StatusesAPersonSet.Counts(status))
            {
                setByAPerson++;
            }
        }

        /// <summary>A result, or the list of them, that could not be read, with what stopped it.</summary>
        public void ResultNotRead(string why)
        {
            walkComplete = false;
            notRead.Add(string.IsNullOrEmpty(why) ? "UNKNOWN" : why);
        }

        /// <summary>
        /// Says the walk of the saved test's results reached its end, every result handed to
        /// AddResult or ResultNotRead, called once after the last of them. FAIL CLOSED: Core
        /// cannot tell a test with no results from a walk that never ran, threw or stopped
        /// part way, so until this is said Removes is false. A result handed after it means
        /// the walk went on, and it must be said again.
        /// </summary>
        public void AllResultsAdded()
        {
            walkComplete = true;
        }

        /// <summary>Whether it is removed from the NWF. Only when nothing at all says leave it.</summary>
        public bool Removes
        {
            get { return Reasons().Count == 0; }
        }

        /// <summary>
        /// The MIRROR line for the log and the form. A mirror removed is written once the
        /// removal is read back off the document, and one left names why and its statuses
        /// with their counts, so Bader can decide.
        /// </summary>
        public string Line()
        {
            IList<string> reasons = Reasons();

            if (reasons.Count == 0)
            {
                return MirrorRule.Prefix + "   " + Saved.Name + " is a mirror of " + keptName
                    + " that this tool created from the picked XML, and no result carries a status a person set, "
                    + "so it is removed from the NWF. Its results: " + Results();
            }

            return MirrorRule.Prefix + "   left in the NWF and not run: " + Saved.Name + ", a mirror of " + keptName
                + ", because " + string.Join(", and ", new List<string>(reasons).ToArray())
                + ". Its results: " + Results();
        }

        private IList<string> Reasons()
        {
            List<string> reasons = new List<string>();

            if (!xmlPicked)
            {
                reasons.Add("no XML was picked, so nothing proves this tool created it");
            }
            else if (ofThePickedXml == null)
            {
                reasons.Add("the picked XML does not hold it, so nothing proves this tool created it");
            }
            else if (!MirrorRule.BothSidesRead(Saved))
            {
                reasons.Add("its name is a mirror of the picked XML and its sides were not read, UNKNOWN, "
                    + "so nothing proves this tool created it");
            }
            else if (!SameSides(Saved, ofThePickedXml.Mirror))
            {
                reasons.Add("its name is a test of the picked XML and its sides are not, left \""
                    + Saved.Left.Locator + "\" and right \"" + Saved.Right.Locator + "\" where the XML has \""
                    + ofThePickedXml.Mirror.Left.Locator + "\" and \"" + ofThePickedXml.Mirror.Right.Locator
                    + "\", so nothing proves this tool created it");
            }

            if (!walkComplete)
            {
                reasons.Add("the walk of its results was not said to be complete, so whether a person "
                    + "set a status on one is UNKNOWN");
            }

            if (notRead.Count > 0)
            {
                reasons.Add(notRead.Count + (notRead.Count == 1 ? " result" : " results")
                    + " could not be read, " + string.Join(", ", notRead.ToArray()));
            }

            if (setByAPerson > 0)
            {
                reasons.Add(setByAPerson + (setByAPerson == 1 ? " result carries" : " results carry")
                    + " a status a person set");
            }

            return reasons;
        }

        private string Results()
        {
            return statuses.Describe()
                + (reviewedByThisTool > 0 ? ", " + reviewedByThisTool + " of the Reviewed set by this tool" : string.Empty)
                + (notRead.Count > 0 ? ", " + notRead.Count + " not read" : string.Empty);
        }

        private static bool SameSides(PlannedClashTest saved, PlannedClashTest inTheXml)
        {
            return string.Equals(saved.Left.Locator, inTheXml.Left.Locator, StringComparison.Ordinal)
                && string.Equals(saved.Right.Locator, inTheXml.Right.Locator, StringComparison.Ordinal);
        }
    }
}
