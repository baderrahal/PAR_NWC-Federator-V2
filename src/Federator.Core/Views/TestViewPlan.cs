using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>
    /// One view per clash test of its open clashes, in folders by priority and team pair, F114,
    /// Bader's Q114 points 9 to 15. It replaces the per clash plan of F85, whose viewpoint per
    /// clash took 94.5 percent of the 2 h 12 min baseline on 1A02MM.
    ///
    /// THE TREE, points 9 to 12: the priority folder, A, B, C or No priority, always there, then
    /// the team pair folder in the team map's one order, TeamPair, then the view named
    /// exactly by its test. Over 150mm sits inside a pair folder whose pair carries it, never
    /// shared and never at the priority level.
    ///
    /// WHAT A VIEW HOLDS, points 13 to 15: the test's clashes at the ViewStatuses setting, New
    /// and Active, all at once. In a pair carrying the size folder a clash whose larger service
    /// is over the threshold goes in the test's view under Over 150mm, and every other one, a
    /// small service, a size that could not be read or no service at all, in its view in the
    /// pair folder, so no clash is in two views. A size not read there is named, FR-068. A test
    /// with no open clash gets no view, and an empty half is never planned.
    ///
    /// THE SIZE IS F72a'S READING, SizeRule.LargestMillimetres: a rectangular service by its
    /// larger side and the larger of two services deciding, read by the add-in.
    ///
    /// NO MIRRORED TEST GETS A VIEW, Bader's point that there are no mirrored tests, F114 attempt
    /// 2. The plan takes the tests F132's mirror rule names, since a mirror not run this week can
    /// still hold the results of an earlier run in the document. A mirrored test's clashes are
    /// left out and counted, and the test is named. Until fix-F132 is merged the add-in hands in
    /// a plain list of test names, and null where no mirror rule ran, which the lines say.
    /// A mirror whose clashes the merge took is out of the report, so a mirror the plan meets
    /// was not merged, rows F114-K34 and F114-R1: its open clashes are counted apart from the
    /// rest, per mirror and in all, since a clash only it finds is in no view.
    /// </summary>
    public static class TestViewPlan
    {
        /// <summary>
        /// The plan for one group, the clashes in the order they were read, the mirror rule's
        /// tests left out, or null where no mirror rule ran.
        /// </summary>
        public static TestViewPlanOutcome For(
            IEnumerable<ViewClash> clashes, ViewTeams teams, ICollection<string> mirrors, ViewpointSettings settings)
        {
            if (teams == null)
            {
                throw new ArgumentNullException("teams");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            IList<ClashStatus> inScope = OpenClashes.StatusesFor(settings.ViewStatuses);
            HashSet<string> mirrored = new HashSet<string>(StringComparer.Ordinal);
            List<string> mirrorRule = mirrors == null ? null : new List<string>();

            foreach (string mirror in mirrors ?? new string[0])
            {
                if (mirror != null && mirrored.Add(mirror))
                {
                    mirrorRule.Add(mirror);
                }
            }

            TestViewPlanOutcome outcome = new TestViewPlanOutcome(inScope, mirrorRule);

            List<string> order = new List<string>();
            Dictionary<string, List<ViewClash>> byTest = new Dictionary<string, List<ViewClash>>(StringComparer.Ordinal);

            if (clashes != null)
            {
                foreach (ViewClash clash in clashes)
                {
                    if (clash == null)
                    {
                        continue;
                    }

                    outcome.Considered = outcome.Considered + 1;

                    if (!byTest.ContainsKey(clash.TestName))
                    {
                        byTest[clash.TestName] = new List<ViewClash>();
                        order.Add(clash.TestName);
                    }

                    byTest[clash.TestName].Add(clash);
                }
            }

            foreach (string test in order)
            {
                List<ViewClash> all = byTest[test];

                if (mirrored.Contains(test))
                {
                    int open = 0;

                    foreach (ViewClash clash in all)
                    {
                        if (inScope.Contains(clash.Status))
                        {
                            open++;
                        }
                    }

                    outcome.LeaveOutMirror(test, open, all.Count - open);
                    continue;
                }

                ViewClash first = all[0];
                TeamPair pair = teams.PairOf(first.LeftSide, first.RightSide);

                outcome.Pair(test, pair);
                NameAnUnknownSide(outcome, teams, first.LeftSide);
                NameAnUnknownSide(outcome, teams, first.RightSide);

                // F114-K29, Bader's words "the idea is to create A B C and delete what come outside":
                // only the teams the map names get pair folders, so a test with a side on a team the
                // map does not name, or on UNKNOWN, gets no view. Nothing is deleted from the NWF, the
                // clashes, the sets or the report, and the open clashes it holds are counted and named.
                string leftTeam = teams.TeamOfSet(first.LeftSide);
                string rightTeam = teams.TeamOfSet(first.RightSide);

                if (!teams.NamedByTheMap(leftTeam) || !teams.NamedByTheMap(rightTeam))
                {
                    int openOutside = 0;

                    foreach (ViewClash clash in all)
                    {
                        if (inScope.Contains(clash.Status))
                        {
                            openOutside++;
                        }
                    }

                    outcome.LeaveOutUnmapped(test, openOutside, all.Count - openOutside, leftTeam, rightTeam);
                    continue;
                }

                List<ViewClash> shown = new List<ViewClash>();

                foreach (ViewClash clash in all)
                {
                    if (!inScope.Contains(clash.Status))
                    {
                        outcome.LeaveOut(clash.Status);
                        continue;
                    }

                    shown.Add(clash);
                }

                if (shown.Count == 0)
                {
                    outcome.TestsWithNoOpenClash = outcome.TestsWithNoOpenClash + 1;
                    continue;
                }

                // F186, Bader's answer 1 of Q174. The folder is the priority's letter, a test with no priority
                // being C as everywhere else, and every open clash of the test is in its one view. The team pair
                // and the size folder no longer make a level of the tree.
                outcome.Add(new PlannedTestView(
                    first.Priority, ToolViewFolder.LetterOf(first.Priority), pair, test, shown));
            }

            outcome.Sort((a, b) => Compare(a, b, teams.Map));
            return outcome;
        }

        private static void NameAnUnknownSide(TestViewPlanOutcome outcome, ViewTeams teams, string side)
        {
            string words = teams.UnknownWords(side);

            if (words != null)
            {
                outcome.NameUnknownSet(side, words);
            }
        }

        /// <summary>Priority, then the pair in the map's order, then the test name, Ordinal.</summary>
        private static int Compare(PlannedTestView a, PlannedTestView b, TeamMap map)
        {
            int byPriority = Priorities.Order(a.Priority).CompareTo(Priorities.Order(b.Priority));

            if (byPriority != 0)
            {
                return byPriority;
            }

            int byFirst = TeamPair.Compare(map, a.Pair.First, b.Pair.First);

            if (byFirst != 0)
            {
                return byFirst;
            }

            int bySecond = TeamPair.Compare(map, a.Pair.Second, b.Pair.Second);

            if (bySecond != 0)
            {
                return bySecond;
            }

            return string.CompareOrdinal(a.Name, b.Name);
        }
    }
}
