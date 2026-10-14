using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Federator.Core.Views
{
    /// <summary>One written view's own seconds and what it carried, F194, Bader's order of 15 Oct 2026, task 5.</summary>
    public sealed class ViewTiming
    {
        public string Name { get; set; }

        /// <summary>The open clashes the view shows.</summary>
        public int Clashes { get; set; }

        /// <summary>The material overrides the saved view carries, read back, or minus one where it was not read.</summary>
        public int Overrides { get; set; }

        /// <summary>The whole of the view, from the first call for it to its read back done.</summary>
        public double Seconds { get; set; }

        /// <summary>Making the COM view with its flags and camera, and the ApplyHideAttribs of it.</summary>
        public double MakingSeconds { get; set; }

        /// <summary>Finding its folder, the add into it and the move out of the root where that route is used.</summary>
        public double AddingSeconds { get; set; }

        /// <summary>Hiding and showing the models, dimming and painting before the add.</summary>
        public double HidingAndDimmingSeconds { get; set; }

        /// <summary>The mark comment written on it and on the folders it made.</summary>
        public double MarkingSeconds { get; set; }

        /// <summary>The read back of the saved view, all of it.</summary>
        public double ReadBackSeconds { get; set; }

        /// <summary>Inside the read back, the hidden items.</summary>
        public double ReadBackHiddenSeconds { get; set; }

        /// <summary>Inside the read back, walking the material overrides one by one.</summary>
        public double ReadBackOverrideSeconds { get; set; }

        /// <summary>F196. Inside the walk of the overrides, getting the list from the saved view.</summary>
        public double ReadBackGetSeconds { get; set; }

        /// <summary>F196. Inside the walk, reading the colour of each override.</summary>
        public double ReadBackOverrideColourSeconds { get; set; }

        /// <summary>F196. Inside the walk, reading the item of each coloured override.</summary>
        public double ReadBackOverrideItemSeconds { get; set; }

        /// <summary>F196. Inside the walk, making the index path of each coloured item.</summary>
        public double ReadBackOverridePathSeconds { get; set; }

        /// <summary>Inside the read back, asking the colour each clashing item will show.</summary>
        public double ReadBackColourSeconds { get; set; }

        /// <summary>The Navisworks process's private bytes when the view was done, in MB, or minus one.</summary>
        public double PrivateMegabytes { get; set; }

        /// <summary>The viewpoints and folders the tree held when the view was started, an estimate from the walk before and the views written.</summary>
        public int TreeItems { get; set; }
    }

    /// <summary>
    /// The seconds of every view of a group, F194, and what they say: the total, the average, the five slowest, the first
    /// quarter against the last, and how well the seconds follow the clashes, the overrides and the tree. The one place
    /// those lines are made, so the log and the tests read the same words.
    /// </summary>
    public sealed class ViewTimings
    {
        private const int Slowest = 5;

        private readonly List<ViewTiming> views = new List<ViewTiming>();

        public void Add(ViewTiming view)
        {
            if (view != null)
            {
                views.Add(view);
            }
        }

        public int Count
        {
            get { return views.Count; }
        }

        public double Total
        {
            get { return views.Sum(v => v.Seconds); }
        }

        public double Average
        {
            get { return views.Count == 0 ? 0.0 : Total / views.Count; }
        }

        /// <summary>
        /// The Pearson correlation of two series, from minus one to one, or null where either does not vary or there
        /// are fewer than three points. A number, never a verdict: Lines says what it was read over.
        /// </summary>
        public static double? Correlation(IList<double> x, IList<double> y)
        {
            if (x == null || y == null || x.Count != y.Count || x.Count < 3)
            {
                return null;
            }

            double mx = x.Average();
            double my = y.Average();
            double sxy = 0.0;
            double sxx = 0.0;
            double syy = 0.0;

            for (int i = 0; i < x.Count; i++)
            {
                double dx = x[i] - mx;
                double dy = y[i] - my;
                sxy += dx * dy;
                sxx += dx * dx;
                syy += dy * dy;
            }

            if (sxx <= 0.0 || syy <= 0.0)
            {
                return null;
            }

            return sxy / Math.Sqrt(sxx * syy);
        }

        public IList<string> Lines(string group)
        {
            List<string> lines = new List<string>();

            if (views.Count == 0)
            {
                lines.Add("VIEWS    TIMING " + group + " no view was written, so no view has seconds");
                return lines;
            }

            List<ViewTiming> byTime = views.OrderByDescending(v => v.Seconds).ToList();

            lines.Add("VIEWS    TIMING " + group + " " + views.Count + " views, " + Show(Total) + " s in all, " + Show(Average)
                + " s on average, the shortest " + Show(byTime[byTime.Count - 1].Seconds) + " s and the longest " + Show(byTime[0].Seconds) + " s");
            lines.Add("VIEWS    TIMING " + group + " where the seconds of the views went: making the view "
                + Show(views.Sum(v => v.MakingSeconds)) + " s, hiding and dimming " + Show(views.Sum(v => v.HidingAndDimmingSeconds))
                + " s, the add into its folder " + Show(views.Sum(v => v.AddingSeconds)) + " s, the mark " + Show(views.Sum(v => v.MarkingSeconds))
                + " s, the read back " + Show(views.Sum(v => v.ReadBackSeconds)) + " s, of which the hidden items "
                + Show(views.Sum(v => v.ReadBackHiddenSeconds)) + " s, the walk of the overrides " + Show(views.Sum(v => v.ReadBackOverrideSeconds))
                + " s (getting the list " + Show(views.Sum(v => v.ReadBackGetSeconds)) + " s, the colour of each override "
                + Show(views.Sum(v => v.ReadBackOverrideColourSeconds)) + " s, the item of each coloured one "
                + Show(views.Sum(v => v.ReadBackOverrideItemSeconds)) + " s, its path "
                + Show(views.Sum(v => v.ReadBackOverridePathSeconds)) + " s)"
                + ", and the colour of the clashing items " + Show(views.Sum(v => v.ReadBackColourSeconds)) + " s");

            int quarter = Math.Max(1, views.Count / 4);
            double first = views.Take(quarter).Average(v => v.Seconds);
            double last = views.Skip(views.Count - quarter).Average(v => v.Seconds);

            lines.Add("VIEWS    TIMING " + group + " in the order they were written, the first " + quarter + " views took "
                + Show(first) + " s each on average and the last " + quarter + " took " + Show(last) + " s, with the tree at "
                + views[0].TreeItems + " items at the start and " + views[views.Count - 1].TreeItems + " at the end");

            double mbFirst = views.Where(v => v.PrivateMegabytes >= 0).Select(v => v.PrivateMegabytes).DefaultIfEmpty(-1.0).First();
            double mbLast = views.Where(v => v.PrivateMegabytes >= 0).Select(v => v.PrivateMegabytes).DefaultIfEmpty(-1.0).Last();

            lines.Add("VIEWS    TIMING " + group + " Navisworks private bytes " + (mbFirst < 0 ? "UNKNOWN" : Show(mbFirst) + " MB after the first view")
                + (mbLast < 0 ? string.Empty : " and " + Show(mbLast) + " MB after the last"));

            List<double> seconds = views.Select(v => v.Seconds).ToList();
            lines.Add("VIEWS    TIMING " + group + " how the seconds of a view follow, Pearson r over " + views.Count + " views: its clashes "
                + R(Correlation(seconds, views.Select(v => (double)v.Clashes).ToList())) + ", its overrides "
                + R(Correlation(seconds, views.Select(v => (double)v.Overrides).ToList())) + ", the tree at its start "
                + R(Correlation(seconds, views.Select(v => (double)v.TreeItems).ToList())) + ", the order it was written "
                + R(Correlation(seconds, Enumerable.Range(0, views.Count).Select(i => (double)i).ToList())));

            int shown = Math.Min(Slowest, byTime.Count);

            for (int i = 0; i < shown; i++)
            {
                ViewTiming v = byTime[i];
                lines.Add("VIEWS    TIMING " + group + " slowest " + (i + 1) + ": " + v.Name + " " + Show(v.Seconds) + " s, "
                    + v.Clashes + " clashes, " + (v.Overrides < 0 ? "UNKNOWN" : v.Overrides.ToString(CultureInfo.InvariantCulture))
                    + " overrides, made " + Show(v.MakingSeconds) + " s, hidden and dimmed " + Show(v.HidingAndDimmingSeconds)
                    + " s, added " + Show(v.AddingSeconds) + " s, marked " + Show(v.MarkingSeconds) + " s, read back "
                    + Show(v.ReadBackSeconds) + " s");
            }

            return lines;
        }

        private static string Show(double seconds)
        {
            return seconds.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string R(double? r)
        {
            return r.HasValue ? r.Value.ToString("0.00", CultureInfo.InvariantCulture) : "UNKNOWN";
        }
    }
}
