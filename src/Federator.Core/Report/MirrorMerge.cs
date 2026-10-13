using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Clash;

namespace Federator.Core.Report
{
    /// <summary>
    /// The clashes of a test kept and of every mirror of it merged into the test kept, F132,
    /// Bader's answer D to Q133. Both tests of a pair are run. A clash both find is kept
    /// once, under the kept test, and the mirror's own copy of it is not reported. A clash
    /// only a mirror finds is added to the kept test's report, marked
    /// ClashRow.FoundOnlyByMirror, and named on a MIRROR line, so the report, the views and
    /// every count hold each clash once. A swap can find more than the test it mirrors:
    /// probe P1 measured 27 on a swap where the test found 25, all 25 among them,
    /// docs\history\scan.md 5z-k on the branch fix-F114-probes.
    ///
    /// ONLY THE PAIRS THE RULE MERGES, F132 attempt 12, the lead's Q142 A: Of takes the pairs
    /// MirrorRule says are merged, where this run created both tests from the clash matrix and
    /// no set of either drifted from it. Every other pair keeps its own clashes and has no
    /// merge here.
    ///
    /// ONE MERGE PER TEST KEPT. Since Q121 B a test can be kept over more than one mirror,
    /// and a clash two of its mirrors find and it does not would be added twice by two
    /// merges, so all the mirrors of one kept test go through one merge, in the rule's order,
    /// and such a clash is added once, under the first mirror that found it. ALL OR NONE:
    /// where one mirror of the kept test cannot be merged, none is, F132 attempt 12, since a
    /// clash it and another mirror find and the kept test does not would be added from the
    /// other and still held in its own block, the breaker's finding 5 on attempt 11.
    ///
    /// ONE CLASH IS ONE UNORDERED PAIR OF ITEMS. The swap holds each pair the other way
    /// round, P1's point 2, so the two items are keyed the same whichever comes first, on
    /// ByDesignPairs.KeyFor, the one unordered key of two strings. What an item's key is, an
    /// index path or anything else that names one item in the document, is the add-in's,
    /// and an empty or UNKNOWN key is an item not read. Two clashes of one test can share a
    /// key, one element carrying two pieces of geometry of one name, so a key is counted as
    /// many times as each test found it: a mirror's clash past the kept test's count of its
    /// key is found by the mirror only, F132-R3, where it was read as a repeat and left out.
    /// Whether two copies of one key in one test are two clashes or one clash read twice is
    /// UNKNOWN and was never measured. What stops a double count is the guard of AddTo: a
    /// mirror is merged only where the clashes handed are as many as its report holds, and
    /// the kept test only where every row it holds was handed with as many clashes as it
    /// stands for. Which copy of such a key is the twin of which is UNKNOWN too, so where the
    /// copies carry more than one status no status is carried across, F132-R3 attempt 2.
    ///
    /// FAIL CLOSED ON WHAT WAS NOT READ. A mirror is merged only where every clash on both
    /// sides, the kept test's and the mirror's, has both items read. A clash of a mirror with
    /// an item not read cannot be told from one the kept test holds, and added it could count
    /// one clash twice, so it is not added, and taken out with its mirror it would be in no
    /// block, F132 attempt 10. So that mirror is not merged and stays in the report as its own
    /// test with every clash it found, and the log says it is UNKNOWN whether the kept test
    /// found it. A clash of the kept test with an item not read cannot be matched to a
    /// mirror's copy of it, which would then be added as found by the mirror only and held
    /// twice, F132 attempt 11. So nothing is merged into that kept test, each of its mirrors
    /// stays in the report as its own test, and the log says why.
    ///
    /// WHAT WAS HANDED IS WHAT THE REPORT HOLDS, both ways, F132 attempt 8. Every row the kept
    /// test's clashes are handed with is held by its report, and every row it holds was
    /// handed with as many clashes as it stands for, one for the row of one clash and one for
    /// each clash under a group, or nothing is merged, since a clash of it not handed would
    /// read as found by a mirror only and be added a second time. A mirror whose clashes
    /// handed are not as many as its report holds is not merged and stays in the report as
    /// its own test, since taken out it would take the clashes not handed with it.
    ///
    /// RAN IS READ OFF THE REPORT, never off a count of calls. A kept test that did not run
    /// found nothing to compare, so nothing is merged into it and each mirror is reported as
    /// its own test. A mirror that did not run found UNKNOWN and never 0. Both are read off
    /// TestReport.State, so AddTo is called once the run has set every test's state.
    ///
    /// THE MIRROR'S OWN RESULTS ARE NOT REPORTED A SECOND TIME, Q133 D, and this is the one
    /// rule that carries it. AddTo takes each mirror that ran out of the group's report, its
    /// test and its rows, so every writer and every count that walks ClashReport.Tests holds
    /// each clash once without knowing about mirrors at all.
    ///
    /// THE STATUS OF A CLASH BOTH FIND, Bader's answer B to Q138. Its two copies can carry two
    /// statuses, and the report shows the one BothFoundStatus gives, set on the kept test's row
    /// with its count by status, TestReport.Restate, so the workbook and the clash XML read it.
    /// Each such clash is named on a MIRROR line with both statuses and the one the report
    /// shows, every one, his words. A clash under a group of the kept test is one row with the
    /// group's status, which stands for every clash under it, so the group is left as it is
    /// and the line says what the rule gives and what the report shows.
    ///
    /// The add-in hands every clash of the kept test with the row of the report that holds it,
    /// its own or its group's, and every clash of each mirror, each clash under a group on its
    /// own and never the group, then calls AddTo once with the group's report.
    /// </summary>
    public sealed class MirrorMerge
    {
        private readonly Dictionary<string, List<KeptClash>> keptPairs = new Dictionary<string, List<KeptClash>>(StringComparer.Ordinal);
        private readonly Dictionary<ClashRow, int> keptRows = new Dictionary<ClashRow, int>();
        private readonly Dictionary<string, List<AddedClash>> addedPairs = new Dictionary<string, List<AddedClash>>(StringComparer.Ordinal);
        private readonly List<MirrorCounts> mirrors = new List<MirrorCounts>();
        private readonly List<Handed> handed = new List<Handed>();
        private int keptFound;
        private int keptNotRead;
        private int held = -1;
        private bool added;
        private string notMerged;

        private MirrorMerge(PlannedClashTest kept, List<MirrorPair> pairs)
        {
            Kept = kept;
            Pairs = new ReadOnlyCollection<MirrorPair>(pairs);

            foreach (MirrorPair pair in pairs)
            {
                mirrors.Add(new MirrorCounts(pair));
            }
        }

        /// <summary>
        /// One merge for each test kept whose mirrors the rule merges, MirrorPair.Merges, in the
        /// order the rule found its first pair. A test kept whose mirrors keep their own clashes
        /// has none.
        /// </summary>
        public static IList<MirrorMerge> Of(MirrorRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            List<PlannedClashTest> order = new List<PlannedClashTest>();
            Dictionary<PlannedClashTest, List<MirrorPair>> byKept = new Dictionary<PlannedClashTest, List<MirrorPair>>();

            foreach (MirrorPair pair in rule.Pairs)
            {
                if (!pair.Merges)
                {
                    continue;
                }

                List<MirrorPair> pairs;

                if (!byKept.TryGetValue(pair.Kept, out pairs))
                {
                    pairs = new List<MirrorPair>();
                    byKept.Add(pair.Kept, pairs);
                    order.Add(pair.Kept);
                }

                pairs.Add(pair);
            }

            List<MirrorMerge> merges = new List<MirrorMerge>();

            foreach (PlannedClashTest kept in order)
            {
                merges.Add(new MirrorMerge(kept, byKept[kept]));
            }

            return merges;
        }

        /// <summary>The test kept, whose report the clashes only a mirror found are added to.</summary>
        public PlannedClashTest Kept { get; private set; }

        /// <summary>Every pair of the test kept, one per mirror.</summary>
        public ReadOnlyCollection<MirrorPair> Pairs { get; private set; }

        /// <summary>
        /// The clashes of the mirrors the kept test found too, or null, UNKNOWN, where nothing
        /// was merged or a mirror was not merged, because it did not run, is not in the report
        /// once, was handed another number of clashes than its report holds, or was handed a
        /// clash with an item not read, since that mirror's clashes were never compared and a 0
        /// for them would read as a count taken. Refused before AddTo has run, for the same
        /// reason.
        /// </summary>
        public int? FoundByBoth
        {
            get { return Compared(counts => counts.Both); }
        }

        /// <summary>
        /// The tests whose clashes AddTo merged, the kept test and each mirror by the name it ran under,
        /// for F127's count check, which compares none of them, since the kept test's block in the clash
        /// workbook then carries its mirrors' clashes and no mirror has a block. None before AddTo and
        /// none where AddTo merged nothing, a pair the rule planned and AddTo refused at run time, so such
        /// a pair is compared like any other test, the breaker's first finding on F127b. AddTo merges
        /// every mirror of a kept test or none, so a merge part made names none.
        /// </summary>
        public IList<string> MergedNames
        {
            get
            {
                List<string> names = new List<string>();

                if (!added || mirrors.Count == 0 || !mirrors.TrueForAll(counts => counts.Merged))
                {
                    return names;
                }

                names.Add(Kept.Name);

                foreach (MirrorCounts counts in mirrors)
                {
                    names.Add(counts.Pair.MirrorName);
                }

                return names;
            }
        }

        /// <summary>
        /// The clashes the merge added to the kept test, each once, the number the kept test's
        /// ROWS line reads, ReportedCount.Line. A count taken once AddTo has run: a mirror
        /// that did not run adds nothing, so nothing of it is among them, and what it would
        /// have found is UNKNOWN, which Lines says and this number never stands for. Refused
        /// before AddTo has run.
        /// </summary>
        public int AddedToTheKeptTest
        {
            get { return Merged(counts => counts.Only.Count); }
        }

        /// <summary>
        /// One clash of the kept test, by the keys of its two items, with the status it
        /// carries and the row of the kept test's report that holds it: its own row, or the
        /// group's row for a clash under a group, each clash under a group handed. The row of
        /// one clash carries that clash's own status, so another status handed with it is
        /// refused rather than one of the two chosen.
        /// </summary>
        public void KeptFound(string firstItem, string secondItem, ClashStatus status, ClashRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            if (!row.IsGroup && row.Status != status)
            {
                throw new ArgumentException("The row of one clash carries that clash's own status, " + row.Status
                    + ", and " + status + " was handed with it.", "row");
            }

            StillOpen();
            keptFound++;

            int handedWith;

            keptRows.TryGetValue(row, out handedWith);
            keptRows[row] = handedWith + 1;

            string key = KeyOf(firstItem, secondItem);

            if (key == null)
            {
                keptNotRead++;
                return;
            }

            List<KeptClash> found;

            if (!keptPairs.TryGetValue(key, out found))
            {
                found = new List<KeptClash>();
                keptPairs.Add(key, found);
            }

            found.Add(new KeptClash(status, row));
        }

        /// <summary>
        /// One clash of a mirror of the kept test, by the keys of its two items, with the row
        /// the report would carry for it. A group is refused, because the merge is clash by
        /// clash and a group stands for many, and so is a pair of another kept test.
        /// </summary>
        public void MirrorFound(MirrorPair pair, string firstItem, string secondItem, ClashRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            if (row.IsGroup)
            {
                throw new ArgumentException(
                    "A group of the mirror was handed. The merge is clash by clash, so hand each clash under it.", "row");
            }

            MirrorCounts counts = CountsOf(pair);
            StillOpen();
            counts.Found++;

            string key = KeyOf(firstItem, secondItem);

            if (key == null)
            {
                counts.NotRead++;
                return;
            }

            handed.Add(new Handed(counts, key, row));
        }

        /// <summary>
        /// Merges the clashes of every mirror that ran into the kept test of the group's
        /// report, once. Each clash only a mirror found is added to the kept test, marked with
        /// that mirror's name, ClashRow.FoundOnlyByMirror, and each mirror that ran is taken out
        /// of the report, so its own results are not reported a second time. Nothing is merged
        /// where the kept test did not run or is not in the report exactly once, and a mirror
        /// that did not run, or is not in the report exactly once, keeps its place and merges
        /// nothing. Nothing is merged either where a clash of the kept test was handed with a
        /// row the report does not hold under it, since the status the rule gives could then
        /// reach no row, or where a row it holds was handed with another number of clashes
        /// than it stands for, or where a clash of it was handed with an item not read, and a
        /// mirror handed another number of clashes than its report holds, or a clash with an
        /// item not read, keeps its place and merges nothing. Where one mirror merges nothing,
        /// no mirror of the kept test is merged, F132 attempt 12. Every one of those is said by
        /// Lines.
        /// </summary>
        public void AddTo(ClashReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            StillOpen();
            added = true;

            IList<TestReport> keptReports = Named(report, Kept.Name);

            if (keptReports.Count != 1)
            {
                notMerged = Kept.Name + " is in the report " + keptReports.Count + (keptReports.Count == 1 ? " time" : " times")
                    + ", so which one its mirrors' clashes go to is UNKNOWN and nothing is merged";
                return;
            }

            TestReport kept = keptReports[0];

            if (kept.State == TestState.Skipped)
            {
                notMerged = Kept.Name + " did not run, so nothing is merged into it and each of its mirrors is reported "
                    + "as its own test";
                return;
            }

            notMerged = Unmatched(kept) ?? KeptNotRead();

            if (notMerged != null)
            {
                return;
            }

            List<TestReport> taken = new List<TestReport>();

            foreach (MirrorCounts counts in mirrors)
            {
                IList<TestReport> mirrorReports = Named(report, counts.Pair.MirrorName);

                counts.InTheReport = mirrorReports.Count;

                if (mirrorReports.Count != 1 || mirrorReports[0].State == TestState.Skipped)
                {
                    continue;
                }

                counts.Ran = true;
                counts.Holds = mirrorReports[0].RawClashes;
                taken.Add(mirrorReports[0]);
            }

            if (mirrors.Exists(counts => !counts.Ran || counts.Found != counts.Holds || counts.NotRead > 0))
            {
                held = kept.RawClashes;
                return;
            }

            for (int i = 0; i < mirrors.Count; i++)
            {
                mirrors[i].Merged = true;
                Merge(mirrors[i], kept);
                report.TakeOut(taken[i]);
            }

            if (kept.State == TestState.Passed && Sum(counts => counts.Only.Count) > 0)
            {
                kept.State = TestState.FoundClashes;
            }

            held = kept.RawClashes;
        }

        /// <summary>
        /// Every clash one mirror handed, each counted once under the first thing it is. The mirror's
        /// clash of a key past as many as the kept test found of it, F132-R3, is one the kept test did not
        /// find, and past as many as earlier mirrors added of it too is one no earlier mirror added. A key
        /// with more than one copy under the kept test or under this mirror whose copies carry more than
        /// one status restates nothing, since which copy is the twin of which is UNKNOWN, and is named once.
        /// A copy an earlier mirror added whose status differs from this mirror's is named, its status
        /// kept as added.
        /// </summary>
        private void Merge(MirrorCounts counts, TestReport kept)
        {
            string mirror = counts.Pair.MirrorName;
            Dictionary<string, List<ClashRow>> copies = new Dictionary<string, List<ClashRow>>(StringComparer.Ordinal);

            foreach (Handed clash in handed)
            {
                if (!ReferenceEquals(clash.Counts, counts))
                {
                    continue;
                }

                List<ClashRow> ofTheKey;

                if (!copies.TryGetValue(clash.Key, out ofTheKey))
                {
                    ofTheKey = new List<ClashRow>();
                    copies.Add(clash.Key, ofTheKey);
                }

                ofTheKey.Add(clash.Row);
            }

            Dictionary<string, int> seen = new Dictionary<string, int>(StringComparer.Ordinal);
            HashSet<string> named = new HashSet<string>(StringComparer.Ordinal);

            foreach (Handed clash in handed)
            {
                if (!ReferenceEquals(clash.Counts, counts))
                {
                    continue;
                }

                int before;
                seen.TryGetValue(clash.Key, out before);
                seen[clash.Key] = before + 1;

                List<KeptClash> keptClashes;
                int keptOfTheKey = keptPairs.TryGetValue(clash.Key, out keptClashes) ? keptClashes.Count : 0;
                List<AddedClash> addedOfTheKey;
                int addedCount = addedPairs.TryGetValue(clash.Key, out addedOfTheKey) ? addedOfTheKey.Count : 0;

                if (before < keptOfTheKey)
                {
                    KeptClash keptClash = keptClashes[before];
                    counts.Both++;

                    if (TwinUnknown(keptClashes, copies[clash.Key]))
                    {
                        if (named.Add(clash.Key))
                        {
                            counts.NotCarried.Add(NotCarried(keptClashes, copies[clash.Key], clash.Row, mirror));
                        }
                    }
                    else if (keptClash.Status != clash.Row.Status)
                    {
                        counts.StatusesDiffer.Add(Weighed(kept, keptClash, clash.Row, mirror));
                    }
                }
                else if (before - keptOfTheKey < addedCount)
                {
                    counts.Earlier++;

                    AddedClash first = addedOfTheKey[before - keptOfTheKey];

                    if (first.Row.Status != clash.Row.Status)
                    {
                        counts.NotCarried.Add(Said(first.Row.Name) + " of " + Kept.Name + ", " + Said(clash.Row.Left.Name)
                            + " against " + Said(clash.Row.Right.Name) + ", added from " + first.Mirror + " at "
                            + first.Row.Status + ", is " + clash.Row.Status + " under " + mirror + ", and the report keeps "
                            + first.Row.Status + " as added, since which copy " + first.Mirror + " gave is the twin of "
                            + "this one is not weighed");
                    }
                }
                else
                {
                    if (addedOfTheKey == null)
                    {
                        addedOfTheKey = new List<AddedClash>();
                        addedPairs.Add(clash.Key, addedOfTheKey);
                    }

                    addedOfTheKey.Add(new AddedClash(clash.Row, mirror));
                    clash.Row.FoundOnlyByMirror = counts.Pair.MirrorName;
                    counts.Only.Add(clash.Row);
                    kept.Add(clash.Row);
                }
            }
        }

        /// <summary>
        /// Whether which copy of a key under the kept test is the twin of which under the mirror is
        /// UNKNOWN in a way that matters: more than one copy on either side, and more than one status
        /// among them all, F132-R3 attempt 2. A key with one copy on each side is weighed as before.
        /// </summary>
        private static bool TwinUnknown(List<KeptClash> keptClashes, List<ClashRow> mirrorCopies)
        {
            if (keptClashes.Count < 2 && mirrorCopies.Count < 2)
            {
                return false;
            }

            ClashStatus first = keptClashes[0].Status;

            return keptClashes.Exists(copy => copy.Status != first) || mirrorCopies.Exists(copy => copy.Status != first);
        }

        /// <summary>The words for a key whose twins are UNKNOWN, named once, no status carried across.</summary>
        private string NotCarried(List<KeptClash> keptClashes, List<ClashRow> mirrorCopies, ClashRow mirrorRow, string mirror)
        {
            List<string> keptStatuses = new List<string>();
            List<string> mirrorStatuses = new List<string>();

            foreach (KeptClash copy in keptClashes)
            {
                keptStatuses.Add(copy.Status.ToString());
            }

            foreach (ClashRow copy in mirrorCopies)
            {
                mirrorStatuses.Add(copy.Status.ToString());
            }

            return keptClashes.Count + (keptClashes.Count == 1 ? " clash" : " clashes") + " under " + Kept.Name + " and "
                + mirrorCopies.Count + " under " + mirror + " share one pair of items, " + Said(mirrorRow.Left.Name)
                + " against " + Said(mirrorRow.Right.Name) + ", at " + string.Join(", ", keptStatuses.ToArray()) + " under "
                + Kept.Name + " and " + string.Join(", ", mirrorStatuses.ToArray()) + " under " + mirror
                + ". Which copy is the twin of which is UNKNOWN, so no status was carried across and the report keeps "
                + Kept.Name + "'s";
        }

        /// <summary>
        /// One clash both found whose two copies carry two statuses, weighed by Bader's answer B
        /// to Q138, BothFoundStatus, against the status the report holds for it under the kept
        /// test when this mirror comes, and set on the kept test's row where that row is the
        /// clash's own. The words naming the clash, both statuses and the one the report shows.
        /// </summary>
        private string Weighed(TestReport kept, KeptClash keptClash, ClashRow mirrorRow, string mirror)
        {
            ClashStatus before = keptClash.Status;
            ClashStatus shown = BothFoundStatus.Shown(before, mirrorRow.Status);
            string why = BothFoundStatus.Why(before, mirrorRow.Status);
            ClashRow row = keptClash.Row;

            keptClash.Status = shown;

            if (row.IsGroup)
            {
                return "a clash both found under the group " + Said(row.Name) + " of " + Kept.Name + ", "
                    + Said(mirrorRow.Left.Name) + " against " + Said(mirrorRow.Right.Name) + ", is " + before + " under "
                    + Kept.Name + " and " + mirrorRow.Status + " under " + mirror + ", " + why + ", so the rule gives "
                    + shown + ", and the report shows the group's " + row.Status + " for its " + row.RawClashes
                    + (row.RawClashes == 1 ? " clash" : " clashes") + ", not changed for one of them";
            }

            if (shown != row.Status)
            {
                kept.Restate(row, shown);
            }

            return Said(row.Name) + " of " + Kept.Name + ", " + Said(row.Left.Name) + " against " + Said(row.Right.Name)
                + ", is " + before + " in the report under " + Kept.Name + " and " + mirrorRow.Status + " under " + mirror
                + ", " + why + ", so the report shows " + shown;
        }

        /// <summary>
        /// The words for the kept test's clashes handed against the rows its report holds, both
        /// ways, or null where every row handed is held and every row held was handed with as
        /// many clashes as it stands for.
        /// </summary>
        private string Unmatched(TestReport kept)
        {
            HashSet<ClashRow> holds = new HashSet<ClashRow>(kept.Rows);
            int notHeld = 0;

            foreach (KeyValuePair<ClashRow, int> row in keptRows)
            {
                if (!holds.Contains(row.Key))
                {
                    notHeld += row.Value;
                }
            }

            if (notHeld > 0)
            {
                return notHeld + (notHeld == 1 ? " clash of " : " clashes of ") + Kept.Name
                    + (notHeld == 1 ? " was" : " were") + " handed with a row the report does not hold under it, so which "
                    + "row carries " + (notHeld == 1 ? "its" : "their") + " status is UNKNOWN and nothing is merged";
            }

            int rowsOff = 0;

            foreach (ClashRow row in kept.Rows)
            {
                int handedWith;

                keptRows.TryGetValue(row, out handedWith);

                if (handedWith != row.RawClashes)
                {
                    rowsOff++;
                }
            }

            if (rowsOff == 0)
            {
                return null;
            }

            int holdsClashes = kept.RawClashes;

            return Kept.Name + " holds " + holdsClashes + (holdsClashes == 1 ? " clash" : " clashes") + " in the report and "
                + keptFound + (keptFound == 1 ? " was" : " were") + " handed, " + rowsOff + (rowsOff == 1
                    ? " row holding another number of clashes than was handed with it"
                    : " rows holding another number of clashes than were handed with them")
                + ", so which of its clashes a mirror found too is UNKNOWN and nothing is merged";
        }

        /// <summary>
        /// The words for the kept test's clashes handed with an item not read, F132 attempt 11, or
        /// null where every one was read. Such a clash cannot be matched to a mirror's, so a
        /// mirror's copy of it would be added as found by the mirror only and the kept test
        /// would hold it twice.
        /// </summary>
        private string KeptNotRead()
        {
            if (keptNotRead == 0)
            {
                return null;
            }

            return keptNotRead + (keptNotRead == 1 ? " clash of " : " clashes of ") + Kept.Name
                + (keptNotRead == 1 ? " has" : " have") + " an item that was not read, so which clashes of its mirrors it "
                + "found too is UNKNOWN, nothing is merged into " + Kept.Name + ", and each of its mirrors stays in the "
                + "report as its own test, where a clash both find is counted twice";
        }

        private static IList<TestReport> Named(ClashReport report, string name)
        {
            List<TestReport> named = new List<TestReport>();

            foreach (TestReport test in report.Tests)
            {
                if (string.Equals(test.Name, name, StringComparison.Ordinal))
                {
                    named.Add(test);
                }
            }

            return named;
        }

        /// <summary>
        /// The MIRROR lines for the log, once AddTo has run. Where nothing was merged, the one
        /// line saying why. Otherwise for each mirror merged what it and the kept test found,
        /// then every clash only that mirror found, each named, his words, then how many
        /// clashes both found carry another status under it and every
        /// one of them named with both statuses and the one the report shows, his words to
        /// Q138, then every pair of items whose status was not carried across, and that it was
        /// taken out of the report. A mirror that did not run, was handed
        /// another number of clashes than its report holds, or was handed a clash with an item
        /// not read, is one line saying so, UNKNOWN and never 0, and where the kept test has more
        /// than one mirror one line then says none of them is merged, F132 attempt 12. Last, what
        /// the report holds under the kept test. Before AddTo has run, one line saying what each
        /// found is UNKNOWN. A kept test with a clash of an item not read merges nothing, so it is
        /// the one line of why.
        /// </summary>
        public IList<string> Lines()
        {
            string kept = Kept.Name;
            List<string> lines = new List<string>();

            if (!added)
            {
                lines.Add(MirrorRule.Prefix + "   the clashes of the mirrors of " + kept + " are not merged into it yet, "
                    + "so what each found is UNKNOWN");
                return lines;
            }

            if (notMerged != null)
            {
                lines.Add(MirrorRule.Prefix + "   " + notMerged);
                return lines;
            }

            foreach (MirrorCounts counts in mirrors)
            {
                string mirror = counts.Pair.MirrorName;

                if (!counts.Ran)
                {
                    lines.Add(MirrorRule.Prefix + "   " + mirror + (counts.InTheReport == 1
                        ? " did not run, so what it finds is UNKNOWN and nothing of it is merged into " + kept
                        : " is in the report " + counts.InTheReport + " times, so whether it ran is UNKNOWN and "
                            + "nothing of it is merged into " + kept));
                    continue;
                }

                if (!counts.Merged && counts.Found != counts.Holds)
                {
                    lines.Add(MirrorRule.Prefix + "   " + counts.Found + (counts.Found == 1 ? " clash of " : " clashes of ")
                        + mirror + (counts.Found == 1 ? " was" : " were") + " handed to the merge and its report holds "
                        + counts.Holds + ", so which clashes it found is UNKNOWN, nothing of it is merged into " + kept
                        + ", and it stays in the report as its own test, where a clash both find is counted twice");
                    continue;
                }

                if (!counts.Merged && counts.NotRead > 0)
                {
                    lines.Add(MirrorRule.Prefix + "   " + counts.NotRead + (counts.NotRead == 1
                            ? " clash of " + mirror + " has an item that was not read, so whether " + kept + " found it is "
                                + "UNKNOWN"
                            : " clashes of " + mirror + " have an item that was not read, so whether " + kept + " found "
                                + "them is UNKNOWN")
                        + ", nothing of " + mirror + " is merged into " + kept + ", and " + mirror + " stays in the report "
                        + "as its own test, where a clash both find is counted twice");
                    continue;
                }

                // Read whole and not merged only because another mirror of the kept test is not,
                // which the line after the loop says.
                if (!counts.Merged)
                {
                    continue;
                }

                lines.Add(MirrorRule.Prefix + "   " + kept + " and its mirror " + mirror + ": " + kept + " found "
                    + keptFound + ", the mirror " + counts.Found + ", " + counts.Both + " by both and "
                    + counts.Only.Count + " by the mirror only, added to " + kept
                    + (counts.Earlier == 0
                        ? string.Empty
                        : ", and " + counts.Earlier + " found by an earlier mirror of " + kept + " as well, added once"));

                foreach (ClashRow row in counts.Only)
                {
                    lines.Add(MirrorRule.Prefix + "   found by the mirror only, added to " + kept + ": " + Said(row.Name)
                        + " of " + mirror + ", " + Said(row.Left.Name) + " against " + Said(row.Right.Name));
                }

                int differ = counts.StatusesDiffer.Count;

                if (differ > 0)
                {
                    lines.Add(MirrorRule.Prefix + "   " + differ
                        + (differ == 1 ? " clash both found carries" : " clashes both found carry")
                        + " another status under " + mirror + " than under " + kept
                        + ", each named with the status the report shows");

                    foreach (string clash in counts.StatusesDiffer)
                    {
                        lines.Add(MirrorRule.Prefix + "   " + clash);
                    }
                }

                foreach (string clash in counts.NotCarried)
                {
                    lines.Add(MirrorRule.Prefix + "   " + clash);
                }

                lines.Add(MirrorRule.Prefix + "   " + mirror
                    + " is taken out of the report, so its own results are not reported a second time");
            }

            if (mirrors.Count > 1 && mirrors.Exists(counts => !counts.Merged))
            {
                lines.Add(MirrorRule.Prefix + "   so no mirror of " + kept + " is merged into it: each of its " + mirrors.Count
                    + " mirrors stays in the report as its own test, since a clash one of them finds and " + kept
                    + " does not would otherwise be added from another and held in two blocks, and a clash both find "
                    + "is counted twice");
            }

            lines.Add(MirrorRule.Prefix + "   the report holds " + held.ToString(CultureInfo.InvariantCulture)
                + " under " + kept);

            return lines;
        }

        private MirrorCounts CountsOf(MirrorPair pair)
        {
            if (pair == null)
            {
                throw new ArgumentNullException("pair");
            }

            foreach (MirrorCounts counts in mirrors)
            {
                if (ReferenceEquals(counts.Pair, pair))
                {
                    return counts;
                }
            }

            throw new ArgumentException(
                pair.MirrorName + " is not a mirror of " + Kept.Name + ", so its clashes do not go to it.", "pair");
        }

        private int Merged(Func<MirrorCounts, int> of)
        {
            if (!added)
            {
                throw new InvalidOperationException(
                    "The clashes of the mirrors of " + Kept.Name + " are not merged into it yet, so how many each "
                        + "found is UNKNOWN. Read the counts once AddTo has run.");
            }

            return Sum(of);
        }

        /// <summary>A count over what the mirrors' clashes were compared to, null where any of them was never compared.</summary>
        private int? Compared(Func<MirrorCounts, int> of)
        {
            int sum = Merged(of);

            return notMerged != null || mirrors.Exists(counts => !counts.Merged) ? (int?)null : sum;
        }

        private int Sum(Func<MirrorCounts, int> of)
        {
            int sum = 0;

            foreach (MirrorCounts counts in mirrors)
            {
                sum += of(counts);
            }

            return sum;
        }

        private void StillOpen()
        {
            if (added)
            {
                throw new InvalidOperationException(
                    "The clashes of the mirrors of " + Kept.Name + " were already merged into it, so a clash handed "
                        + "now or a second merge would count a clash twice or leave one out.");
            }
        }

        /// <summary>The unordered key of the two items, or null where either was not read.</summary>
        private static string KeyOf(string firstItem, string secondItem)
        {
            return Read(firstItem) && Read(secondItem) ? ByDesignPairs.KeyFor(firstItem, secondItem) : null;
        }

        private static bool Read(string item)
        {
            return !string.IsNullOrEmpty(item) && TestDrift.WasRead(item);
        }

        private static string Said(string name)
        {
            return string.IsNullOrEmpty(name) ? "UNKNOWN" : name;
        }

        /// <summary>What one mirror found and what became of it.</summary>
        private sealed class MirrorCounts
        {
            internal MirrorCounts(MirrorPair pair)
            {
                Pair = pair;
                Only = new List<ClashRow>();
                StatusesDiffer = new List<string>();
                NotCarried = new List<string>();
            }

            internal MirrorPair Pair { get; private set; }

            internal int Found { get; set; }

            internal int NotRead { get; set; }

            internal int Both { get; set; }

            internal int Earlier { get; set; }

            /// <summary>Each clash both found whose two statuses differ, in words, Q138.</summary>
            internal List<string> StatusesDiffer { get; private set; }

            /// <summary>
            /// Each pair of items whose status was not carried across, in words, F132-R3 attempt 2: a key
            /// whose twins are UNKNOWN, named once, and a copy an earlier mirror added at another status.
            /// </summary>
            internal List<string> NotCarried { get; private set; }

            /// <summary>Whether the mirror is in the report once and ran, read by AddTo off its state.</summary>
            internal bool Ran { get; set; }

            /// <summary>The clashes its report holds, read by AddTo where it ran.</summary>
            internal int Holds { get; set; }

            /// <summary>
            /// Whether it ran and was handed as many clashes as its report holds, each with both
            /// items read, and so was merged.
            /// </summary>
            internal bool Merged { get; set; }

            /// <summary>How many tests of the mirror's name the report held.</summary>
            internal int InTheReport { get; set; }

            internal List<ClashRow> Only { get; private set; }
        }

        /// <summary>
        /// One clash a mirror handed with both items read, kept until AddTo, since the kept
        /// test's clashes may come after it.
        /// </summary>
        private sealed class Handed
        {
            internal Handed(MirrorCounts counts, string key, ClashRow row)
            {
                Counts = counts;
                Key = key;
                Row = row;
            }

            internal MirrorCounts Counts { get; private set; }

            internal string Key { get; private set; }

            internal ClashRow Row { get; private set; }
        }

        /// <summary>A clash a mirror added to the kept test, with the mirror it came from.</summary>
        private sealed class AddedClash
        {
            internal AddedClash(ClashRow row, string mirror)
            {
                Row = row;
                Mirror = mirror;
            }

            internal ClashRow Row { get; private set; }

            internal string Mirror { get; private set; }
        }

        /// <summary>
        /// One clash of the kept test: the status the report holds for it, which the rule of
        /// Q138 can change as each mirror comes, and the row of the kept test's report that
        /// holds it, its own or its group's.
        /// </summary>
        private sealed class KeptClash
        {
            internal KeptClash(ClashStatus status, ClashRow row)
            {
                Status = status;
                Row = row;
            }

            internal ClashStatus Status { get; set; }

            internal ClashRow Row { get; private set; }
        }
    }
}
