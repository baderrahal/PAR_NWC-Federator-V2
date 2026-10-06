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
    /// ONE MERGE PER TEST KEPT. Since Q121 B a test can be kept over more than one mirror,
    /// and a clash two of its mirrors find and it does not would be added twice by two
    /// merges, so all the mirrors of one kept test go through one merge, in the rule's order,
    /// and such a clash is added once, under the first mirror that found it.
    ///
    /// ONE CLASH IS ONE UNORDERED PAIR OF ITEMS. The swap holds each pair the other way
    /// round, P1's point 2, so the two items are keyed the same whichever comes first, on
    /// ByDesignPairs.KeyFor, the one unordered key of two strings. What an item's key is, an
    /// index path or anything else that names one item in the document, is the add-in's,
    /// and an empty or UNKNOWN key is an item not read.
    ///
    /// FAIL CLOSED ON WHAT WAS NOT READ. A clash of a mirror with an item not read cannot be
    /// told from one the kept test holds, and added it could count one clash twice, so it is
    /// not added and the log says it is UNKNOWN whether the kept test found it. A clash of the
    /// kept test with an item not read could match a clash of a mirror, so the log says how
    /// many of those said to be found by a mirror only may be the kept test's own.
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
        private readonly Dictionary<string, KeptClash> keptPairs = new Dictionary<string, KeptClash>(StringComparer.Ordinal);
        private readonly HashSet<string> addedPairs = new HashSet<string>(StringComparer.Ordinal);
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

        /// <summary>One merge for each test kept, in the order the rule found its first pair.</summary>
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
        /// was merged or a mirror did not run or is not in the report once, because that
        /// mirror's clashes were never compared and a 0 for them would read as a count taken.
        /// Refused before AddTo has run, for the same reason.
        /// </summary>
        public int? FoundByBoth
        {
            get { return Compared(counts => counts.Both); }
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
        /// The clashes of the mirrors with an item not read, never added, or null, UNKNOWN,
        /// where FoundByBoth is. Refused before AddTo has run.
        /// </summary>
        public int? NotCompared
        {
            get { return Compared(counts => counts.NotRead); }
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

            string key = KeyOf(firstItem, secondItem);

            if (key == null)
            {
                keptNotRead++;
                return;
            }

            if (!keptPairs.ContainsKey(key))
            {
                keptPairs.Add(key, new KeptClash(status, row));
            }
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
            handed.Add(new Handed(counts, KeyOf(firstItem, secondItem), row));
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
        /// reach no row. Every one of those is said by Lines.
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

            int notHeld = NotHeldBy(kept);

            if (notHeld > 0)
            {
                notMerged = notHeld + (notHeld == 1 ? " clash of " : " clashes of ") + Kept.Name
                    + (notHeld == 1 ? " was" : " were") + " handed with a row the report does not hold under it, so which "
                    + "row carries " + (notHeld == 1 ? "its" : "their") + " status is UNKNOWN and nothing is merged";
                return;
            }

            foreach (MirrorCounts counts in mirrors)
            {
                IList<TestReport> mirrorReports = Named(report, counts.Pair.MirrorName);

                counts.InTheReport = mirrorReports.Count;

                if (mirrorReports.Count != 1 || mirrorReports[0].State == TestState.Skipped)
                {
                    continue;
                }

                counts.Ran = true;
                Merge(counts, kept);
                report.TakeOut(mirrorReports[0]);
            }

            if (kept.State == TestState.Passed && Sum(counts => counts.Only.Count) > 0)
            {
                kept.State = TestState.FoundClashes;
            }

            held = kept.RawClashes;
        }

        /// <summary>Every clash one mirror handed, each counted once under the first thing it is.</summary>
        private void Merge(MirrorCounts counts, TestReport kept)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (Handed clash in handed)
            {
                if (!ReferenceEquals(clash.Counts, counts))
                {
                    continue;
                }

                KeptClash keptClash;

                if (clash.Key == null)
                {
                    counts.NotRead++;
                }
                else if (!seen.Add(clash.Key))
                {
                    counts.Repeats++;
                }
                else if (keptPairs.TryGetValue(clash.Key, out keptClash))
                {
                    counts.Both++;

                    if (keptClash.Status != clash.Row.Status)
                    {
                        counts.StatusesDiffer.Add(Weighed(kept, keptClash, clash.Row, counts.Pair.MirrorName));
                    }
                }
                else if (!addedPairs.Add(clash.Key))
                {
                    counts.Earlier++;
                }
                else
                {
                    clash.Row.FoundOnlyByMirror = counts.Pair.MirrorName;
                    counts.Only.Add(clash.Row);
                    kept.Add(clash.Row);
                }
            }
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

        /// <summary>How many of the kept test's clashes handed name a row the kept test's report does not hold.</summary>
        private int NotHeldBy(TestReport kept)
        {
            HashSet<ClashRow> holds = new HashSet<ClashRow>(kept.Rows);
            int notHeld = 0;

            foreach (KeptClash clash in keptPairs.Values)
            {
                if (!holds.Contains(clash.Row))
                {
                    notHeld++;
                }
            }

            return notHeld;
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
        /// line saying why. Otherwise for each mirror that ran what it and the kept test found,
        /// then every clash only that mirror found, each named, his words, then once each what
        /// could not be compared and what it repeated, then how many clashes both found carry
        /// another status under it and every one of them named with both statuses and the one
        /// the report shows, his words to Q138, and that it was taken out of the report. A mirror that did not run
        /// is one line saying so, UNKNOWN and never 0. Last, what the report holds under the
        /// kept test. Before AddTo has run, one line saying what each found is UNKNOWN.
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

                if (counts.NotRead > 0)
                {
                    lines.Add(MirrorRule.Prefix + "   " + counts.NotRead
                        + (counts.NotRead == 1
                            ? " clash of " + mirror + " has an item that was not read, so whether " + kept
                                + " found it is UNKNOWN and it is not added, and with " + mirror
                                + " taken out of the report it is in no block of it"
                            : " clashes of " + mirror + " have an item that was not read, so whether " + kept
                                + " found them is UNKNOWN and they are not added, and with " + mirror
                                + " taken out of the report they are in no block of it"));
                }

                if (counts.Repeats > 0)
                {
                    lines.Add(MirrorRule.Prefix + "   " + counts.Repeats
                        + (counts.Repeats == 1
                            ? " clash of " + mirror + " repeats a pair of items it already gave, so it is counted once"
                            : " clashes of " + mirror + " repeat a pair of items it already gave, so each is counted once"));
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

                if (counts.Ran)
                {
                    lines.Add(MirrorRule.Prefix + "   " + mirror
                        + " is taken out of the report, so its own results are not reported a second time");
                }
            }

            if (keptNotRead > 0)
            {
                lines.Add(MirrorRule.Prefix + "   " + keptNotRead + (keptNotRead == 1 ? " clash of " : " clashes of ")
                    + kept + (keptNotRead == 1 ? " has" : " have") + " an item that was not read, so up to " + keptNotRead
                    + " of the clashes found by a mirror only may be " + kept + "'s own as well, UNKNOWN");
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

            return notMerged != null || mirrors.Exists(counts => !counts.Ran) ? (int?)null : sum;
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
            }

            internal MirrorPair Pair { get; private set; }

            internal int Found { get; set; }

            internal int NotRead { get; set; }

            internal int Repeats { get; set; }

            internal int Both { get; set; }

            internal int Earlier { get; set; }

            /// <summary>Each clash both found whose two statuses differ, in words, Q138.</summary>
            internal List<string> StatusesDiffer { get; private set; }

            /// <summary>Whether the mirror is in the report once and ran, read by AddTo off its state.</summary>
            internal bool Ran { get; set; }

            /// <summary>How many tests of the mirror's name the report held.</summary>
            internal int InTheReport { get; set; }

            internal List<ClashRow> Only { get; private set; }
        }

        /// <summary>One clash a mirror handed, kept until AddTo, since the kept test's clashes may come after it.</summary>
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
