using System;
using System.Collections.Generic;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Bader's standing rule built into the tool: when the code knows something the
    /// report does not show, it becomes a question.
    ///
    /// That rule has been worked by hand every time. Q25 is exactly this, and it took an
    /// audit of every file under src to find it. These tests pin the rule that makes the
    /// run say it itself.
    /// </summary>
    [TestFixture]
    public class GapRuleTests
    {
        private static ClashReport ReportWith(params ClashItem[] items)
        {
            ClashReport report = new ClashReport("1B06PH", "1104-PAR-1B06PH-ZZZ-BM-MOD-000001");
            TestReport test = report.AddTest("BLD-ME v BLD-EL");

            for (int i = 0; i + 1 < items.Length; i += 2)
            {
                ClashRow row = new ClashRow();
                row.Left = items[i];
                row.Right = items[i + 1];
                test.Add(row);
            }

            return report;
        }

        private static ClashItem Item(string family, string type, string material,
            string sourceFile, string discipline, string idFrom)
        {
            ClashItem item = new ClashItem();
            item.Family = family;
            item.Type = type;
            item.Material = material;
            item.SourceFile = sourceFile;
            item.Discipline = discipline;
            item.IdFrom = idFrom;
            return item;
        }

        private static ClashItem Empty()
        {
            return new ClashItem();
        }

        private static ReportGap Find(IList<ReportGap> gaps, string name)
        {
            foreach (ReportGap gap in gaps)
            {
                if (gap.Name == name)
                {
                    return gap;
                }
            }

            return null;
        }

        [Test]
        public void EveryPropertyMeasuredAndShownNowhereIsAGap()
        {
            ClashItem full = Item("Pipe Types", "Standard", "Steel", "ME.rvt", "ME", "Id");
            IList<ReportGap> gaps = GapRule.For(ReportWith(full, full));

            Assert.That(gaps.Count, Is.EqualTo(6));

            foreach (string name in new[]
            {
                "Family", "Type Name", "Material", "Source File", "Discipline", "Id From"
            })
            {
                Assert.That(Find(gaps, name), Is.Not.Null, name);
            }
        }

        /// <summary>
        /// A gap names what is missing, what it came to, and where it would go. All
        /// three, because a gap with no value is a complaint and one with no place to go
        /// is a shrug.
        /// </summary>
        [Test]
        public void AGapCarriesTheNameTheValueAndWhereItWouldBelong()
        {
            ClashItem full = Item("Pipe Types", "Standard", "Steel", "ME.rvt", "ME", "Id");
            ReportGap gap = Find(GapRule.For(ReportWith(full, full)), "Family");

            Assert.That(gap.Name, Is.EqualTo("Family"));
            Assert.That(gap.Value, Does.Contain("2 of 2 item cells carry it"));
            Assert.That(gap.WouldBelong, Does.Contain("Q25"));

            Assert.That(gap.Line(), Does.StartWith("Family"));
            Assert.That(gap.Line(), Does.Contain("2 of 2"));
            Assert.That(gap.Line(), Does.Contain("Q25"));
        }

        /// <summary>
        /// The break. A property nothing carried is NOT a gap. Reporting it would say the
        /// run is holding back something it never read, which is the opposite of true.
        /// </summary>
        [Test]
        public void APropertyNothingCarriedIsNotAGap()
        {
            IList<ReportGap> gaps = GapRule.For(ReportWith(Empty(), Empty()));

            Assert.That(gaps, Is.Empty);
        }

        [Test]
        public void OnlyThePropertiesThatWereReadAreReported()
        {
            ClashItem some = Item("Pipe Types", null, null, "ME.rvt", null, null);
            IList<ReportGap> gaps = GapRule.For(ReportWith(some, Empty()));

            Assert.That(gaps.Count, Is.EqualTo(2));
            Assert.That(Find(gaps, "Family"), Is.Not.Null);
            Assert.That(Find(gaps, "Source File"), Is.Not.Null);
            Assert.That(Find(gaps, "Material"), Is.Null);
        }

        [Test]
        public void TheValueIsCountedOverEveryItemCellAndNotOverEveryRow()
        {
            ClashItem full = Item("Pipe Types", "Standard", "Steel", "ME.rvt", "ME", "Id");

            // Three rows, two item cells each, and only the left of each carries a family.
            ClashReport report = ReportWith(full, Empty(), full, Empty(), full, Empty());
            ReportGap gap = Find(GapRule.For(report), "Family");

            Assert.That(gap.Carried, Is.EqualTo(3));
            Assert.That(gap.OutOf, Is.EqualTo(6));
            Assert.That(gap.Value, Does.Contain("3 of 6 item cells carry it"));
        }

        [Test]
        public void ARowWithOnlyOneSideIsCountedOnce()
        {
            ClashReport report = new ClashReport("1B06PH", "name");
            TestReport test = report.AddTest("one sided");
            ClashRow row = new ClashRow();
            row.Left = Item("Pipe Types", null, null, null, null, null);
            row.Right = null;
            test.Add(row);

            ReportGap gap = Find(GapRule.For(report), "Family");

            Assert.That(gap.Carried, Is.EqualTo(1));
            Assert.That(gap.OutOf, Is.EqualTo(1));
            Assert.That(gap.Value, Does.Contain("1 of 1 item cell carries it"));
        }

        [Test]
        public void NoReportAtAllIsNoGapAndNoThrow()
        {
            Assert.That(GapRule.For(null), Is.Empty);
        }

        // ---------- the block ----------

        /// <summary>
        /// The block is written even when it is empty. A missing block reads as a check
        /// that did not run, which is the one thing this tool must never look like.
        /// </summary>
        [Test]
        public void AnEmptyBlockStillSaysNothingWasHeldBack()
        {
            IList<string> lines = GapRule.Lines(ReportWith(Empty(), Empty()));

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Is.EqualTo(GapRule.NothingHeldBack()));
            Assert.That(lines[0], Does.Contain("nothing measured this group reaches no output"));
        }

        [Test]
        public void TheBlockCountsWhatItHoldsAndSaysItIsNotAFault()
        {
            ClashItem full = Item("Pipe Types", "Standard", "Steel", "ME.rvt", "ME", "Id");
            IList<string> lines = GapRule.Lines(ReportWith(full, full));

            Assert.That(lines.Count, Is.EqualTo(7), "one heading and six gaps");
            Assert.That(lines[0], Does.Contain("6 numbers are"));
            Assert.That(lines[0], Does.Contain("nothing acts on it"));
            Assert.That(lines[1], Does.StartWith("Family"));
        }

        [Test]
        public void OneGapReadsInTheSingular()
        {
            ClashItem one = Item("Pipe Types", null, null, null, null, null);

            Assert.That(GapRule.Lines(ReportWith(one, Empty()))[0], Does.Contain("1 number is"));
        }

        // ---------- the run ----------

        [Test]
        public void TheRunCountsGapsByNameAndNotByLine()
        {
            ClashItem full = Item("Pipe Types", "Standard", "Steel", "ME.rvt", "ME", "Id");
            GapTally tally = new GapTally();

            // The same six on twenty two groups is still six things this tool does not
            // show, not 132.
            for (int i = 0; i < 22; i++)
            {
                tally.Add(GapRule.For(ReportWith(full, full)));
            }

            Assert.That(tally.Distinct, Is.EqualTo(6));
            Assert.That(tally.Groups, Is.EqualTo(22));
            Assert.That(tally.Line(), Does.Contain("6 things are"));
            Assert.That(tally.Line(), Does.Contain("over 22 groups"));
            Assert.That(tally.Line(), Does.Contain("Family"));
            Assert.That(tally.Line(), Does.Contain("rather than a fault"));
        }

        [Test]
        public void ARunThatHeldNothingBackSaysSo()
        {
            GapTally tally = new GapTally();
            tally.Add(GapRule.For(ReportWith(Empty(), Empty())));

            Assert.That(tally.Distinct, Is.EqualTo(0));
            Assert.That(tally.Line(), Does.Contain("none over 1 group"));
            Assert.That(tally.Line(), Does.Contain("reached an output"));
        }

        [Test]
        public void ARunWithNoGroupAtAllSaysThatInsteadOfSayingNone()
        {
            Assert.That(new GapTally().Line(), Does.Contain("no group ran"));
        }

        [Test]
        public void AGroupWithNothingToReportStillCounts()
        {
            GapTally tally = new GapTally();
            tally.Add(null);

            Assert.That(tally.Groups, Is.EqualTo(1));
            Assert.That(tally.Distinct, Is.EqualTo(0));
        }

        [Test]
        public void AGapWithNoNameIsRefusedWhereItIsBuilt()
        {
            Assert.That(() => new ReportGap(null, "1", "somewhere"), Throws.ArgumentException);
            Assert.That(() => new ReportGap(string.Empty, "1", "somewhere"), Throws.ArgumentException);
        }

        [Test]
        public void AGapWithNoValueSaysUnknownRatherThanLeavingABlank()
        {
            ReportGap gap = new ReportGap("Family", null, null);

            Assert.That(gap.Line(), Does.Contain("UNKNOWN"));
            Assert.That(gap.Line(), Does.Contain("UNKNOWN where it would belong"));
        }
    }
}
