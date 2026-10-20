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

        internal TestViewPlanOutcome(IList<ClashStatus> inScope, ICollection<string> mirrorRule, bool packHandedIn)
        {
            InScope = new ReadOnlyCollection<ClashStatus>(new List<ClashStatus>(inScope));
            MirrorRuleHandedIn = mirrorRule != null;
            PackHandedIn = packHandedIn;

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
        internal ReadOnlyCollection<string> UnknownSets
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
        internal TeamPair PairOfTest(string testName)
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
        internal ReadOnlyCollection<string> OutsideTheMap
        {
            get { return new ReadOnlyCollection<string>(unmapped); }
        }

        /// <summary>The open clashes of those tests, in no view.</summary>
        internal int OpenOutsideTheMap
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

        // ---------- F199, the discipline folders ----------

        private readonly List<string> notInThePack = new List<string>();

        /// <summary>Whether the saved tests the pack does not name were handed to the plan, so such a test can be known.</summary>
        public bool PackHandedIn { get; private set; }

        /// <summary>Each saved test the pack does not name that got a view, once, which is in Other.</summary>
        public ReadOnlyCollection<string> NotInThePack
        {
            get { return new ReadOnlyCollection<string>(notInThePack); }
        }

        internal void NameNotInThePack(string testName)
        {
            if (!notInThePack.Contains(testName))
            {
                notInThePack.Add(testName);
            }
        }

        // ---------- F192 and F199, the Over 150mm folder inside Mechanical and Electrical of C only ----------

        private readonly List<string> overSizeKept = new List<string>();
        private readonly List<string> overSizeKeptInC = new List<string>();
        private readonly List<string> sizeUnknown = new List<string>();
        private int overSizeKeptClashes;
        private int overSizeKeptTestsA;
        private int overSizeKeptTestsB;
        private int overSizeKeptClashesInC;

        /// <summary>
        /// A test in a pair that carries the size folder with clashes over the threshold, where no Over 150mm folder
        /// stands, so they stay in the test's one view and are counted. Of priority A or B, Bader's order of 15 Oct
        /// 2026, Q182. Of C in Architecture, Structure or Other, Bader's order of round 6, Q185.
        /// </summary>
        internal void OverSizeKeptWhole(string testName, string letter, string discipline, int clashes)
        {
            string words = testName + " : priority " + letter + ", " + discipline + ", " + clashes
                + " clashes over the threshold stay in the test's one view";

            if (letter == "C")
            {
                overSizeKeptInC.Add(words);
                overSizeKeptClashesInC = overSizeKeptClashesInC + clashes;
                return;
            }

            overSizeKept.Add(words);
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

        /// <summary>The tests of C in Architecture, Structure or Other with clashes over the threshold, kept in their one view, Q185.</summary>
        public int OverSizeKeptTestsC
        {
            get { return overSizeKeptInC.Count; }
        }

        /// <summary>The clashes over the threshold of those tests of C.</summary>
        public int OverSizeKeptClashesC
        {
            get { return overSizeKeptClashesInC; }
        }

        /// <summary>The views planned in that letter and discipline folder, the Over 150mm views inside it among them.</summary>
        public int ViewsIn(string letter, string discipline)
        {
            int count = 0;

            foreach (PlannedTestView view in views)
            {
                if (string.Equals(view.PriorityFolder, letter, StringComparison.Ordinal)
                    && string.Equals(view.DisciplineFolder, discipline, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        internal void NameSizeUnknown(string testName, string clashName, string letter)
        {
            sizeUnknown.Add(testName + " / " + clashName + " (" + letter + ")");
        }

        /// <summary>The tests of priority A in a carrying pair that have clashes over the threshold, kept in their one view.</summary>
        internal int OverSizeKeptTestsA
        {
            get { return overSizeKeptTestsA; }
        }

        /// <summary>The same for priority B.</summary>
        internal int OverSizeKeptTestsB
        {
            get { return overSizeKeptTestsB; }
        }

        /// <summary>The clashes over the threshold of those tests of A and B.</summary>
        internal int OverSizeKeptClashes
        {
            get { return overSizeKeptClashes; }
        }

        /// <summary>The views in an Over 150mm folder, all inside Mechanical or Electrical of C.</summary>
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

            lines.Add("discipline folders, views planned in each : " + DisciplineWords());
            lines.Add(PackHandedIn
                ? "saved tests the pack does not name, in Other : " + NotInThePack.Count
                : "saved tests the pack does not name : UNKNOWN, no matrix was read, so every test goes by its discipline folder rule, the code of its first side and AR with ST in Architecture");
            Named(lines, NotInThePack);
            lines.Add("Over 150mm folder, inside Mechanical and Electrical of C only : " + OverSizeViews
                + " views of tests of priority C in a pair that carries the size folder");
            lines.Add("tests of priority A or B with clashes over the threshold, no Over 150mm view made, all in the test's one view : "
                + (overSizeKeptTestsA + overSizeKeptTestsB) + " tests, " + overSizeKeptTestsA + " of A and " + overSizeKeptTestsB + " of B, "
                + overSizeKeptClashes + " clashes, for Bader's decision");
            Named(lines, overSizeKept);
            lines.Add("tests of priority C in Architecture, Structure or Other with clashes over the threshold, no Over 150mm view made, "
                + "all in the test's one view, Q185 : " + OverSizeKeptTestsC + " tests, " + OverSizeKeptClashesC + " clashes");
            Named(lines, overSizeKeptInC);
            lines.Add("clashes in a pair that carries the size folder whose size could not be read, so they stay in the test's main view : "
                + sizeUnknown.Count);
            Named(lines, sizeUnknown.Count > 20 ? sizeUnknown.GetRange(0, 20) : sizeUnknown);

            lines.Add("a side read as a team of UNKNOWN, by its set name : " + unknownSets.Count
                + ", each named with why and none guessed at");
            Named(lines, unknownWords);

            return lines;
        }

        /// <summary>Each letter with the views of each discipline folder that holds one, a folder holding none not named.</summary>
        private string DisciplineWords()
        {
            List<string> letters = new List<string>();

            foreach (string letter in ToolViewFolder.Letters)
            {
                List<string> held = new List<string>();

                foreach (string discipline in DisciplineFolders.Names)
                {
                    int count = ViewsIn(letter, discipline);

                    if (count > 0)
                    {
                        held.Add(discipline + " " + count);
                    }
                }

                letters.Add(letter + " " + (held.Count == 0 ? "none" : string.Join(", ", held.ToArray())));
            }

            return string.Join(", ", letters.ToArray());
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
