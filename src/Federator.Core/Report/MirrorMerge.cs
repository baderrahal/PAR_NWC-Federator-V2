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
    /// The add-in hands every clash of the kept test and of each mirror, each clash under a
    /// group on its own and never the group, then calls AddTo once with the kept test's report.
    /// </summary>
    public sealed class MirrorMerge
    {
        private readonly HashSet<string> keptPairs = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> addedPairs = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<MirrorCounts> mirrors = new List<MirrorCounts>();
        private readonly List<Handed> handed = new List<Handed>();
        private int keptFound;
        private int keptNotRead;
        private int held = -1;
        private bool added;

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

        /// <summary>The clashes of the mirrors the kept test found too, read once AddTo has run.</summary>
        public int FoundByBoth
        {
            get { return Sum(counts => counts.Both); }
        }

        /// <summary>The clashes added to the kept test, each once, read once AddTo has run.</summary>
        public int FoundByTheMirrorsOnly
        {
            get { return Sum(counts => counts.Only.Count); }
        }

        /// <summary>The clashes of the mirrors with an item not read, never added, read once AddTo has run.</summary>
        public int NotCompared
        {
            get { return Sum(counts => counts.NotRead); }
        }

        /// <summary>One clash of the kept test, by the keys of its two items, a clash under a group included.</summary>
        public void KeptFound(string firstItem, string secondItem)
        {
            StillOpen();
            keptFound++;

            string key = KeyOf(firstItem, secondItem);

            if (key == null)
            {
                keptNotRead++;
                return;
            }

            keptPairs.Add(key);
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
        /// Adds every clash only a mirror found to the kept test's report, once, marked with
        /// that mirror's name, so the report's rows and every count it gives are read off the
        /// merged list. Called once, with the kept test's own report.
        /// </summary>
        public void AddTo(TestReport kept)
        {
            if (kept == null)
            {
                throw new ArgumentNullException("kept");
            }

            if (!string.Equals(kept.Name, Kept.Name, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The clashes of the mirrors of " + Kept.Name + " go to " + Kept.Name + ", and " + kept.Name
                        + " was handed.",
                    "kept");
            }

            StillOpen();
            added = true;

            foreach (MirrorCounts counts in mirrors)
            {
                HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

                foreach (Handed clash in handed)
                {
                    if (!ReferenceEquals(clash.Counts, counts))
                    {
                        continue;
                    }

                    if (clash.Key == null)
                    {
                        counts.NotRead++;
                    }
                    else if (!seen.Add(clash.Key))
                    {
                        counts.Repeats++;
                    }
                    else if (keptPairs.Contains(clash.Key))
                    {
                        counts.Both++;
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

            held = kept.RawClashes;
        }

        /// <summary>
        /// The MIRROR lines for the log, once AddTo has run: for each mirror what it and the
        /// kept test found, then every clash only that mirror found, each named, his words,
        /// then once each what could not be compared and what it repeated. Last, what the
        /// report holds under the kept test.
        /// </summary>
        public IList<string> Lines()
        {
            string kept = Kept.Name;
            List<string> lines = new List<string>();

            foreach (MirrorCounts counts in mirrors)
            {
                string mirror = counts.Pair.MirrorName;

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
                                + " found it is UNKNOWN and it is not added"
                            : " clashes of " + mirror + " have an item that was not read, so whether " + kept
                                + " found them is UNKNOWN and they are not added"));
                }

                if (counts.Repeats > 0)
                {
                    lines.Add(MirrorRule.Prefix + "   " + counts.Repeats
                        + (counts.Repeats == 1
                            ? " clash of " + mirror + " repeats a pair of items it already gave, so it is counted once"
                            : " clashes of " + mirror + " repeat a pair of items it already gave, so each is counted once"));
                }
            }

            if (keptNotRead > 0)
            {
                lines.Add(MirrorRule.Prefix + "   " + keptNotRead + (keptNotRead == 1 ? " clash of " : " clashes of ")
                    + kept + (keptNotRead == 1 ? " has" : " have") + " an item that was not read, so up to " + keptNotRead
                    + " of the clashes found by a mirror only may be " + kept + "'s own as well, UNKNOWN");
            }

            lines.Add(MirrorRule.Prefix + "   the report holds "
                + (held < 0 ? "UNKNOWN, the merge has not run," : held.ToString(CultureInfo.InvariantCulture))
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
            }

            internal MirrorPair Pair { get; private set; }

            internal int Found { get; set; }

            internal int NotRead { get; set; }

            internal int Repeats { get; set; }

            internal int Both { get; set; }

            internal int Earlier { get; set; }

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
    }
}
