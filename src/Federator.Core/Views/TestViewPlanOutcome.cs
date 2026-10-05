using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Clash;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>What the per test view plan wants and what it left out, F114.</summary>
    public sealed class TestViewPlanOutcome
    {
        private readonly List<PlannedTestView> views = new List<PlannedTestView>();
        private readonly Dictionary<ClashStatus, int> leftOut = new Dictionary<ClashStatus, int>();
        private readonly Dictionary<string, TeamPair> pairOfTest = new Dictionary<string, TeamPair>(StringComparer.Ordinal);
        private readonly List<string> sizeUnknown = new List<string>();
        private readonly List<string> unknownSets = new List<string>();

        internal TestViewPlanOutcome(IList<ClashStatus> inScope)
        {
            InScope = new ReadOnlyCollection<ClashStatus>(new List<ClashStatus>(inScope));
        }

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

        /// <summary>The tests whose every clash is at a status no view shows, so no view is made, point 15.</summary>
        public int TestsWithNoOpenClash { get; internal set; }

        /// <summary>Every clash in a pair carrying the size folder whose service size could not be read, by test and clash, FR-068.</summary>
        public ReadOnlyCollection<string> SizeUnknown
        {
            get { return new ReadOnlyCollection<string>(sizeUnknown); }
        }

        /// <summary>Every set name carrying no code this group knows, each once, FR-074.</summary>
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

        /// <summary>Every clash at a status no view shows.</summary>
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

        /// <summary>Whether the open clashes in views and the clashes left out make every clash looked at.</summary>
        public bool AddsUp
        {
            get { return InViews + LeftOut == Considered; }
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

        internal void Pair(string testName, TeamPair pair)
        {
            if (!pairOfTest.ContainsKey(testName))
            {
                pairOfTest[testName] = pair;
            }
        }

        internal void NameSizeUnknown(string testName, string clashName)
        {
            sizeUnknown.Add(testName + " / " + clashName);
        }

        internal void NameUnknownSet(string setName)
        {
            if (!unknownSets.Contains(setName))
            {
                unknownSets.Add(setName);
            }
        }

        /// <summary>
        /// The VIEWS block's plan lines: the counts, the clashes left out by status, every size
        /// that could not be read named, or as many as the settings say with the rest counted,
        /// and every set name with no code named. Nothing is guessed and nothing is silent.
        /// </summary>
        public IList<string> Lines(SizeSettings sizes)
        {
            List<string> lines = new List<string>();

            lines.Add("clashes looked at : " + Considered);
            lines.Add("open clashes in views : " + InViews + ", in " + views.Count + " views");

            foreach (ClashStatus status in Enum.GetValues(typeof(ClashStatus)))
            {
                if (!InScope.Contains(status))
                {
                    lines.Add("    " + LeftOutAt(status).ToString().PadLeft(5) + "  " + status
                        + ", left out because a view shows only " + StatusWords());
                }
            }

            lines.Add("tests with no open clash, so no view : " + TestsWithNoOpenClash);
            lines.Add(AddsUp
                ? "open clashes in views " + InViews + " and clashes left out " + LeftOut + " make the " + Considered + " looked at"
                : "DOES NOT ADD UP: open clashes in views " + InViews + " and clashes left out " + LeftOut
                    + " against " + Considered + " looked at");

            lines.Add("size could not be read, in a pair with a size folder : " + sizeUnknown.Count
                + ", every one in its pair view and none dropped");
            Named(lines, sizeUnknown, sizes);

            lines.Add("a set name with no code this group knows : " + unknownSets.Count
                + ", its side read as a team of UNKNOWN and none guessed at");
            Named(lines, unknownSets, null);

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

        private static void Named(List<string> lines, IList<string> names, SizeSettings sizes)
        {
            bool every = sizes == null || sizes.NameEveryUnknown;
            int shown = every ? names.Count : Math.Min(names.Count, Math.Max(0, sizes.ExamplesWhenNotNamingEvery));

            for (int i = 0; i < shown; i++)
            {
                lines.Add("    " + names[i]);
            }

            if (shown < names.Count)
            {
                lines.Add("    and " + (names.Count - shown) + " more, not named because the size settings name "
                    + shown + " and not every one");
            }
        }
    }
}
