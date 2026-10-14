using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Clash;

namespace Federator.Core.Views
{
    /// <summary>What the per test view plan wants and what it left out, F114.</summary>
    public sealed class TestViewPlanOutcome
    {
        private readonly List<PlannedTestView> views = new List<PlannedTestView>();
        private readonly Dictionary<ClashStatus, int> leftOut = new Dictionary<ClashStatus, int>();
        private readonly Dictionary<string, TeamPair> pairOfTest = new Dictionary<string, TeamPair>(StringComparer.Ordinal);
        private readonly List<string> unknownSets = new List<string>();
        private readonly List<string> unknownWords = new List<string>();
        private readonly List<string> mirrored = new List<string>();
        private readonly List<string> mirrorWords = new List<string>();
        private readonly List<string> mirrorRule = new List<string>();
        private int mirrorsOpen;
        private readonly List<string> unmapped = new List<string>();
        private readonly List<string> unmappedWords = new List<string>();
        private int unmappedOpen;

        internal TestViewPlanOutcome(IList<ClashStatus> inScope, ICollection<string> mirrorRule)
        {
            InScope = new ReadOnlyCollection<ClashStatus>(new List<ClashStatus>(inScope));
            MirrorRuleHandedIn = mirrorRule != null;

            if (mirrorRule != null)
            {
                this.mirrorRule.AddRange(mirrorRule);
            }
        }

        /// <summary>Whether the mirror rule's tests were handed to the plan, so a mirror can be known.</summary>
        public bool MirrorRuleHandedIn { get; private set; }

        /// <summary>
        /// Every test the mirror rule names, each once, as the plan was handed them, and empty where
        /// none was. The one mirror list of a run, F114 attempt 3: the VIEWS TREE check 5 reads it
        /// here, and nothing hands it a second copy.
        /// </summary>
        public ReadOnlyCollection<string> MirrorRule
        {
            get { return new ReadOnlyCollection<string>(mirrorRule); }
        }

        /// <summary>Each test the mirror rule names that the plan was handed clashes of, once, so it got no view.</summary>
        public ReadOnlyCollection<string> Mirrored
        {
            get { return new ReadOnlyCollection<string>(mirrored); }
        }

        /// <summary>The clashes of those mirrored tests, left out whatever their status.</summary>
        public int LeftOutAsMirrors { get; private set; }

        /// <summary>The statuses a view shows.</summary>
        public ReadOnlyCollection<ClashStatus> InScope { get; private set; }

        /// <summary>The views, in the order a person reads the tree: priority, pair, the pair's views, its size folder, test name.</summary>
        public ReadOnlyCollection<PlannedTestView> Views
        {
            get { return new ReadOnlyCollection<PlannedTestView>(views); }
        }

        /// <summary>Every clash the plan was handed.</summary>
        public int Considered { get; internal set; }

        /// <summary>The open clashes the views hold, each once.</summary>
        public int InViews { get; internal set; }

        /// <summary>
        /// The tests whose every clash is at a status no view shows, so no view is made, point 15.
        /// A test with no clash at all is never handed to the plan and is not counted here.
        /// </summary>
        public int TestsWithNoOpenClash { get; internal set; }

        /// <summary>Every side that reads UNKNOWN, each once by its whole locator, FR-074 and N5 of F114c, the empty side for a side with no set name. Lines says why, each reason written.</summary>
        public ReadOnlyCollection<string> UnknownSets
        {
            get { return new ReadOnlyCollection<string>(unknownSets); }
        }

        /// <summary>The clashes at that status no view shows.</summary>
        public int LeftOutAt(ClashStatus status)
        {
            int count;
            return leftOut.TryGetValue(status, out count) ? count : 0;
        }

        /// <summary>Every clash at a status no view shows, a mirrored test's not among them.</summary>
        public int LeftOut
        {
            get
            {
                int all = 0;

                foreach (int count in leftOut.Values)
                {
                    all += count;
                }

                return all;
            }
        }

        /// <summary>The team pair of a test the plan was handed, or null where it never saw that test.</summary>
        public TeamPair PairOfTest(string testName)
        {
            TeamPair pair;
            return testName != null && pairOfTest.TryGetValue(testName, out pair) ? pair : null;
        }

        internal void Add(PlannedTestView view)
        {
            views.Add(view);
            InViews = InViews + view.Clashes.Count;
        }

        internal void Sort(Comparison<PlannedTestView> order)
        {
            views.Sort(order);
        }

        internal void LeaveOut(ClashStatus status)
        {
            leftOut[status] = LeftOutAt(status) + 1;
        }

        /// <summary>
        /// One mirror's clashes left out, its open ones apart from the rest, rows F114-K34 and R1:
        /// the plan meets only a mirror not merged, so its open clashes are in no view.
        /// </summary>
        internal void LeaveOutMirror(string testName, int open, int atAnotherStatus)
        {
            mirrored.Add(testName);
            mirrorWords.Add(testName + " : " + open + " open and in no view, " + atAnotherStatus + " at a status no view shows");
            mirrorsOpen = mirrorsOpen + open;
            LeftOutAsMirrors = LeftOutAsMirrors + open + atAnotherStatus;
        }

        /// <summary>
        /// A test with a side on a team the team map does not name, or on UNKNOWN, F114-K29: no view,
        /// its open clashes counted apart from the rest and the test named with the two teams read.
        /// </summary>
        internal void LeaveOutUnmapped(string testName, int open, int atAnotherStatus, string leftTeam, string rightTeam)
        {
            unmapped.Add(testName);
            unmappedWords.Add(testName + " : " + open + " open and in no view, " + atAnotherStatus
                + " at a status no view shows, its sides read as the teams " + leftTeam + " and " + rightTeam);
            unmappedOpen = unmappedOpen + open;
        }

        /// <summary>Each test left without a view because a side is on a team the map does not name or on UNKNOWN, once.</summary>
        public ReadOnlyCollection<string> OutsideTheMap
        {
            get { return new ReadOnlyCollection<string>(unmapped); }
        }

        /// <summary>The open clashes of those tests, in no view.</summary>
        public int OpenOutsideTheMap
        {
            get { return unmappedOpen; }
        }

        internal void Pair(string testName, TeamPair pair)
        {
            if (!pairOfTest.ContainsKey(testName))
            {
                pairOfTest[testName] = pair;
            }
        }

        // ---------- F192, the Over 150mm folder under C only ----------

        private readonly List<string> overSizeKept = new List<string>();
        private readonly List<string> sizeUnknown = new List<string>();
        private int overSizeKeptClashes;
        private int overSizeKeptTestsA;
        private int overSizeKeptTestsB;

        /// <summary>
        /// A test of priority A or B in a pair that carries the size folder with clashes over the threshold. The
        /// earlier rule would have made an Over 150mm view of them. Bader's order of 15 Oct 2026 is that no such
        /// view is made outside C, so they stay in the test's one view, and they are counted for his decision.
        /// </summary>
        internal void OverSizeKeptWhole(string testName, string letter, int clashes)
        {
            overSizeKept.Add(testName + " : priority " + letter + ", " + clashes + " clashes over the threshold stay in the test's one view");
            overSizeKeptClashes = overSizeKeptClashes + clashes;

            if (letter == "A")
            {
                overSizeKeptTestsA++;
            }
            else
            {
                overSizeKeptTestsB++;
            }
        }

        internal void NameSizeUnknown(string testName, string clashName, string letter)
        {
            sizeUnknown.Add(testName + " / " + clashName + " (" + letter + ")");
        }

        /// <summary>The tests of priority A in a carrying pair that have clashes over the threshold, kept in their one view.</summary>
        public int OverSizeKeptTestsA
        {
            get { return overSizeKeptTestsA; }
        }

        /// <summary>The same for priority B.</summary>
        public int OverSizeKeptTestsB
        {
            get { return overSizeKeptTestsB; }
        }

        /// <summary>The clashes over the threshold of those tests of A and B.</summary>
        public int OverSizeKeptClashes
        {
            get { return overSizeKeptClashes; }
        }

        /// <summary>The views in an Over 150mm folder, all under C.</summary>
        public int OverSizeViews
        {
            get
            {
                int count = 0;

                foreach (PlannedTestView view in views)
                {
                    if (view.SizeFolder != null)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>The clashes read with no size in a carrying pair, each once, by test and clash name.</summary>
        public ReadOnlyCollection<string> SizeUnknown
        {
            get { return new ReadOnlyCollection<string>(sizeUnknown); }
        }

        internal void NameUnknownSet(string side, string words)
        {
            if (!unknownSets.Contains(side))
            {
                unknownSets.Add(side);
                unknownWords.Add(words);
            }
        }

        /// <summary>
        /// The VIEWS block's plan lines: the counts, the clashes left out by status, the mirrored
        /// tests named, each with its open clashes in no view counted, or said UNKNOWN, and
        /// every set whose side reads UNKNOWN named with
        /// why. Nothing is guessed and nothing is silent.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            lines.Add("clashes looked at : " + Considered + ", open in views " + InViews + ", left out by status " + LeftOut
                + ", of mirrored tests " + LeftOutAsMirrors);
            lines.Add("open clashes in views : " + InViews + ", in " + views.Count + " views");

            foreach (ClashStatus status in Enum.GetValues(typeof(ClashStatus)))
            {
                if (!InScope.Contains(status))
                {
                    lines.Add("    " + LeftOutAt(status).ToString().PadLeft(5) + "  " + status
                        + ", left out because a view shows only " + StatusWords());
                }
            }

            lines.Add("tests whose every clash is at a status no view shows, so no view : " + TestsWithNoOpenClash
                + ", a test with no clash at all is not handed to the plan and not counted here");

            if (MirrorRuleHandedIn)
            {
                lines.Add("mirrored tests, so no view : " + mirrored.Count + ", their " + LeftOutAsMirrors + " clashes left out");

                if (mirrored.Count > 0)
                {
                    lines.Add("open clashes of those mirrored tests, in no view : " + mirrorsOpen
                        + ", each on the report under its mirror's own name because its clashes were not merged into the test it mirrors, "
                        + "so a clash only that mirror finds is in no view and how many of them it alone finds is UNKNOWN");
                }

                Named(lines, mirrorWords);
            }
            else
            {
                lines.Add("mirrored tests : UNKNOWN, no mirror rule was handed to the plan, so a mirrored test gets a view as any test does");
            }

            lines.Add("tests with a side on a team the team map does not name, or on UNKNOWN, so no view : " + unmapped.Count
                + ", their " + unmappedOpen + " open clashes in no view, and nothing deleted from the NWF, the clashes, the sets or the report");
            Named(lines, unmappedWords);

            lines.Add("Over 150mm folder, under C only : " + OverSizeViews + " views of tests of priority C in a pair that carries the size folder");
            lines.Add("tests of priority A or B with clashes over the threshold, no Over 150mm view made, all in the test's one view : "
                + (overSizeKeptTestsA + overSizeKeptTestsB) + " tests, " + overSizeKeptTestsA + " of A and " + overSizeKeptTestsB + " of B, "
                + overSizeKeptClashes + " clashes, for Bader's decision");
            Named(lines, overSizeKept);
            lines.Add("clashes in a pair that carries the size folder whose size could not be read, so they stay in the test's main view : "
                + sizeUnknown.Count);
            Named(lines, sizeUnknown.Count > 20 ? sizeUnknown.GetRange(0, 20) : sizeUnknown);

            lines.Add("a side read as a team of UNKNOWN, by its set name : " + unknownSets.Count
                + ", each named with why and none guessed at");
            Named(lines, unknownWords);

            return lines;
        }

        private string StatusWords()
        {
            List<string> words = new List<string>();

            foreach (ClashStatus status in InScope)
            {
                words.Add(status.ToString());
            }

            return string.Join(" and ", words.ToArray());
        }

        private static void Named(List<string> lines, IList<string> names)
        {
            for (int i = 0; i < names.Count; i++)
            {
                lines.Add("    " + names[i]);
            }
        }
    }
}
