using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;

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
    /// the XML's name, with the XML's two sets and the XML's settings. So a saved test is
    /// taken as this tool's only where its name is a mirror of the picked XML's and it is
    /// the very test the XML would create, both locators Ordinal and every setting the drift
    /// rule compares, TestDrift.Compare: the tolerance within its epsilon, the test type,
    /// merge composites and each side's self intersect and primitive types. A mirror a
    /// person tuned after this tool made it is not proved to be this tool's. Nothing else on
    /// a test says who made it, so with no XML picked nothing proves it and nothing is ever
    /// removed, and a swap the picked XML does not hold was made by a person or by an older
    /// XML and is left. A saved side that was not read, MirrorRule.BothSidesRead, proves
    /// nothing either, so a name the XML calls a mirror whose sides were not read is left
    /// and its line says UNKNOWN. By its sides alone such a test is never a mirror. The
    /// proof needs the XML's own rule, so Find refuses a rule holding a test read off the
    /// document as the picked XML's, and a test not read off the document as a saved one,
    /// since either would match itself. A saved test whose name another saved test carries
    /// too is left, because a removal that finds its test by name could take the other.
    ///
    /// WHOSE A STATUS IS, Q122's default A. Every status but New counts as a person's,
    /// `StatusesAPersonSet`, with one exception its record proves: a Reviewed carrying this
    /// tool's own record still reading as ours, judged by `UndoAutoReview.Judge`, the judge
    /// the Undo auto Reviewed button runs, so a result it would put back is the one taken as
    /// this tool's. So a mirror whose results a rerun moved to Active or Resolved is left. A
    /// result that could not be read is left as well, because a status nobody read is not a
    /// status nobody set. A test with no results carries no status. Where the record says a
    /// Reviewed of this tool's was Active before, the line counts how many, since a plain
    /// Active counts as a person's and Q122 is open on it.
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
        private readonly bool nameShared;
        private readonly ClashTally statuses = new ClashTally();
        private readonly List<string> notReadWhy = new List<string>();
        private readonly Dictionary<string, int> notReadTimes = new Dictionary<string, int>(StringComparer.Ordinal);
        private int notRead;
        private int setByAPerson;
        private int reviewedByThisTool;
        private int reviewedOffActive;
        private bool walkComplete;

        private MirrorInDocument(
            PlannedClashTest saved, string keptName, bool xmlPicked, MirrorPair ofThePickedXml, bool nameShared)
        {
            Saved = saved;
            this.keptName = keptName;
            this.xmlPicked = xmlPicked;
            this.ofThePickedXml = ofThePickedXml;
            this.nameShared = nameShared;
        }

        /// <summary>The test as it is saved in the document, carrying the address it is found at.</summary>
        public PlannedClashTest Saved { get; private set; }

        /// <summary>
        /// Every saved test that is a mirror, each to be judged once its results are added.
        /// The saved tests are the document's, ClashTestPlan.FromDocument's buildable ones,
        /// in the order the document holds them. The picked XML's rule is the one its own
        /// plan was built with, over ClashTestPlan.From's buildable tests, or null where no
        /// XML was picked.
        ///
        /// REFUSED, so the add-in cannot get it wrong. A rule holding a test read off the
        /// document is refused as the picked XML's, because each saved test in it would be
        /// found by its own name and match itself, and a person's test would be removed as
        /// one this tool created from an XML nobody picked. A test handed as a saved one that
        /// was not read off the document is refused the same way, because an XML test would
        /// match itself whatever the document holds under its name. A rule over no test at
        /// all cannot say where it came from, and it holds no pair, so it removes nothing.
        /// </summary>
        public static IList<MirrorInDocument> Find(
            IList<PlannedClashTest> saved, MirrorRule ofThePickedXml, PriorityMap priorities)
        {
            if (priorities == null)
            {
                throw new ArgumentNullException("priorities");
            }

            if (ofThePickedXml != null)
            {
                PlannedClashTest readOffTheDocument = ofThePickedXml.FirstReadOffTheDocument();

                if (readOffTheDocument != null)
                {
                    throw new ArgumentException(
                        "The rule handed as the picked XML's holds " + readOffTheDocument.Name
                            + ", a test read off the document, so it is no XML's rule and each saved test in it "
                            + "would match itself. With no XML picked the rule is null.",
                        "ofThePickedXml");
                }
            }

            List<MirrorInDocument> found = new List<MirrorInDocument>();
            List<PlannedClashTest> all = new List<PlannedClashTest>();

            if (saved != null)
            {
                foreach (PlannedClashTest test in saved)
                {
                    if (test == null)
                    {
                        continue;
                    }

                    if (!test.IsFromDocument)
                    {
                        throw new ArgumentException(
                            test.Name + " was handed as a saved test and was not read off the document, so it "
                                + "would match itself whatever the document holds under its name.",
                            "saved");
                    }

                    all.Add(test);
                }
            }

            HashSet<string> shared = NamesSaidTwice(all);

            if (ofThePickedXml == null)
            {
                foreach (MirrorPair pair in MirrorRule.Of(all, priorities).Pairs)
                {
                    found.Add(new MirrorInDocument(
                        pair.Mirror, pair.Kept.Name, false, null, shared.Contains(pair.Mirror.Name)));
                }

                return found;
            }

            List<PlannedClashTest> outsideTheXml = new List<PlannedClashTest>();

            foreach (PlannedClashTest test in all)
            {
                MirrorPair xmlPair = ofThePickedXml.PairWhoseMirrorIsNamed(test.Name);

                if (xmlPair != null)
                {
                    found.Add(new MirrorInDocument(test, xmlPair.Kept.Name, true, xmlPair, shared.Contains(test.Name)));
                    continue;
                }

                if (ofThePickedXml.Holds(test.Name))
                {
                    continue;
                }

                PlannedClashTest run = ofThePickedXml.RunTestSwappedFrom(test);

                if (run != null)
                {
                    found.Add(new MirrorInDocument(test, run.Name, true, null, shared.Contains(test.Name)));
                    continue;
                }

                outsideTheXml.Add(test);
            }

            foreach (MirrorPair pair in MirrorRule.Of(outsideTheXml, priorities).Pairs)
            {
                found.Add(new MirrorInDocument(
                    pair.Mirror, pair.Kept.Name, true, null, shared.Contains(pair.Mirror.Name)));
            }

            return found;
        }

        /// <summary>Every name more than one saved test carries, Ordinal and never trimmed.</summary>
        private static HashSet<string> NamesSaidTwice(List<PlannedClashTest> all)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> twice = new HashSet<string>(StringComparer.Ordinal);

            foreach (PlannedClashTest test in all)
            {
                if (!seen.Add(test.Name))
                {
                    twice.Add(test.Name);
                }
            }

            return twice;
        }

        /// <summary>One result of the saved test, its status and its comment as the document holds them.</summary>
        public void AddResult(ClashStatus status, string comment)
        {
            walkComplete = false;
            statuses.Add(status);

            ClashStatus putBackTo;

            if (UndoAutoReview.Judge(comment, status, out putBackTo) == UndoVerdict.PutBack)
            {
                reviewedByThisTool++;

                if (putBackTo == ClashStatus.Active)
                {
                    reviewedOffActive++;
                }
            }
            else if (StatusesAPersonSet.Counts(status))
            {
                setByAPerson++;
            }
        }

        /// <summary>
        /// A result, or the list of them, that could not be read, with what stopped it. The
        /// same reason handed again is counted and said once.
        /// </summary>
        public void ResultNotRead(string why)
        {
            walkComplete = false;
            notRead++;

            string reason = string.IsNullOrEmpty(why) ? "UNKNOWN" : why;
            int times;

            if (!notReadTimes.TryGetValue(reason, out times))
            {
                notReadWhy.Add(reason);
            }

            notReadTimes[reason] = times + 1;
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
            else
            {
                // The drift rule's own compare, so the saved test is the very test the XML
                // would create, in its sets and in every setting, and one a person tuned after
                // this tool made it is not proved to be this tool's.
                IList<TestDifference> differences = TestDrift.Compare(
                    Saved.Name, TestSettings.FromFile(ofThePickedXml.Mirror), TestSettings.FromFile(Saved));

                if (differences.Count > 0)
                {
                    reasons.Add("it is not the test the picked XML would create, " + Described(differences)
                        + ", so nothing proves this tool created it");
                }
            }

            if (nameShared)
            {
                reasons.Add("another saved test carries the same name, so which of them a removal by name "
                    + "would take is UNKNOWN");
            }

            if (!walkComplete)
            {
                reasons.Add("the walk of its results was not said to be complete, so whether a person "
                    + "set a status on one is UNKNOWN");
            }

            if (notRead > 0)
            {
                reasons.Add(notRead + (notRead == 1 ? " result" : " results") + " could not be read: " + WhyNotRead());
            }

            if (setByAPerson > 0)
            {
                reasons.Add(setByAPerson + (setByAPerson == 1 ? " result carries" : " results carry")
                    + " a status a person set");
            }

            return reasons;
        }

        private static string Described(IList<TestDifference> differences)
        {
            List<string> said = new List<string>();

            foreach (TestDifference difference in differences)
            {
                said.Add(difference.Field + " " + difference.InFile + " in the XML and " + difference.InDocument + " saved");
            }

            return string.Join(", ", said.ToArray());
        }

        /// <summary>Each reason once with how many times it came, five named and the rest counted.</summary>
        private string WhyNotRead()
        {
            int shown = Math.Min(notReadWhy.Count, RunLog.KeptOfARepeat);
            List<string> said = new List<string>();

            for (int i = 0; i < shown; i++)
            {
                int times = notReadTimes[notReadWhy[i]];
                said.Add(times == 1 ? notReadWhy[i] : notReadWhy[i] + " " + times + " times");
            }

            int more = notReadWhy.Count - shown;

            return string.Join(", ", said.ToArray())
                + (more > 0 ? ", and " + more + (more == 1 ? " more reason" : " more reasons") : string.Empty);
        }

        /// <summary>
        /// The statuses with their counts. Where the walk was not said complete, what it read
        /// is never given as all of them, and with nothing read the results are UNKNOWN.
        /// </summary>
        private string Results()
        {
            if (!walkComplete && statuses.Total == 0 && notRead == 0)
            {
                return "UNKNOWN";
            }

            string read = statuses.Total > 0 ? statuses.Describe() : notRead > 0 ? "none read" : "none";

            if (reviewedByThisTool > 0)
            {
                read += ", " + reviewedByThisTool + " of the Reviewed set by this tool"
                    + (reviewedOffActive > 0 ? ", " + reviewedOffActive + " of them Active before it moved them" : string.Empty);
            }

            if (notRead > 0)
            {
                read += ", " + notRead + " not read";
            }

            return walkComplete ? read : read + ", and whether that is all of them is UNKNOWN";
        }
    }
}
