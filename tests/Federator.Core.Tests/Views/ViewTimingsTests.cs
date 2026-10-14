using System.Collections.Generic;
using System.IO;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F194, Bader's order of 15 Oct 2026, task 5. The seconds of each view are kept and said: the count, the total,
    /// the average, the five slowest, the first quarter against the last, and how well the seconds follow the clashes,
    /// the overrides and the tree. Names are sample data.
    /// </summary>
    [TestFixture]
    public class ViewTimingsTests
    {
        private static ViewTiming View(string name, double seconds, int clashes, int overrides, int tree)
        {
            return new ViewTiming
            {
                Name = name,
                Seconds = seconds,
                Clashes = clashes,
                Overrides = overrides,
                MakingSeconds = seconds * 0.01,
                AddingSeconds = seconds * 0.1,
                HidingAndDimmingSeconds = seconds * 0.02,
                MarkingSeconds = seconds * 0.001,
                ReadBackSeconds = seconds * 0.85,
                ReadBackHiddenSeconds = seconds * 0.05,
                ReadBackOverrideSeconds = seconds * 0.7,
                ReadBackColourSeconds = seconds * 0.1,
                PrivateMegabytes = 3000 + tree,
                TreeItems = tree
            };
        }

        [Test]
        public void NoViewSaysSoAndInventsNoSeconds()
        {
            ViewTimings timings = new ViewTimings();
            IList<string> lines = timings.Lines("1A04PK");

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.Contain("no view was written"));
            Assert.That(timings.Average, Is.EqualTo(0.0));
        }

        [Test]
        public void TheLinesSayTheCountTheTotalTheAverageAndTheFiveSlowest()
        {
            ViewTimings timings = new ViewTimings();
            double[] seconds = { 4, 8, 2, 40, 6, 90, 12, 30 };

            for (int i = 0; i < seconds.Length; i++)
            {
                timings.Add(View("V" + i, seconds[i], 10 * (i + 1), 1000 * (i + 1), 100 + i));
            }

            IList<string> lines = timings.Lines("1A04PK");
            string all = string.Join("\n", new List<string>(lines).ToArray());

            Assert.That(timings.Count, Is.EqualTo(8));
            Assert.That(timings.Total, Is.EqualTo(192.0).Within(1e-9));
            Assert.That(lines[0], Does.Contain("VIEWS    TIMING 1A04PK 8 views, 192 s in all, 24 s on average, the shortest 2 s and the longest 90 s"));
            Assert.That(all, Does.Contain("slowest 1: V5 90 s"));
            Assert.That(all, Does.Contain("slowest 2: V3 40 s"));
            Assert.That(all, Does.Contain("slowest 3: V7 30 s"));
            Assert.That(all, Does.Contain("slowest 5: V1 8 s"));
            Assert.That(all, Does.Not.Contain("slowest 6"));
            Assert.That(all, Does.Contain("the first 2 views took 6 s each on average and the last 2 took 21 s"));
        }

        [Test]
        public void TheReadBackAndItsThreePartsAreSaidSoTheCauseIsReadOffTheLog()
        {
            ViewTimings timings = new ViewTimings();
            timings.Add(View("V0", 100, 5, 1000, 10));

            string all = string.Join("\n", new List<string>(timings.Lines("G")).ToArray());

            Assert.That(all, Does.Contain("making the view 1 s, hiding and dimming 2 s, the add into its folder 10 s, the mark 0.1 s, the read back 85 s"));
            Assert.That(all, Does.Contain("of which the hidden items 5 s, the walk of the overrides 70 s and the colour of the clashing items 10 s"));
        }

        [Test]
        public void ThePearsonCorrelationIsOneForALineAndUnknownForAFlatSeries()
        {
            Assert.That(ViewTimings.Correlation(new double[] { 1, 2, 3, 4 }, new double[] { 2, 4, 6, 8 }), Is.EqualTo(1.0).Within(1e-9));
            Assert.That(ViewTimings.Correlation(new double[] { 1, 2, 3, 4 }, new double[] { 8, 6, 4, 2 }), Is.EqualTo(-1.0).Within(1e-9));
            Assert.That(ViewTimings.Correlation(new double[] { 1, 2, 3 }, new double[] { 5, 5, 5 }), Is.Null);
            Assert.That(ViewTimings.Correlation(new double[] { 1, 2 }, new double[] { 1, 2 }), Is.Null, "two points prove nothing");
        }

        [Test]
        public void TheCorrelationLineNamesTheClashesTheOverridesTheTreeAndTheOrder()
        {
            ViewTimings timings = new ViewTimings();

            for (int i = 0; i < 6; i++)
            {
                timings.Add(View("V" + i, 10.0 * (i + 1), 50, 1000 * (i + 1), 200));
            }

            string all = string.Join("\n", new List<string>(timings.Lines("G")).ToArray());

            Assert.That(all, Does.Contain("Pearson r over 6 views: its clashes UNKNOWN, its overrides 1.00, the tree at its start UNKNOWN, the order it was written 1.00"));
        }

        [Test]
        public void TheBuilderTimesEachViewAndLogsTheLinesOncePerGroup()
        {
            string path = Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Engine", "ViewpointBuilder.cs");
            string source = File.ReadAllText(path);

            Assert.That(source, Does.Contain("timings.Add(new ViewTiming"));
            Assert.That(source, Does.Contain("timings.Lines(timedGroup)"));
            Assert.That(source, Does.Contain("lastReadBack = read;"));
        }
    }
}
