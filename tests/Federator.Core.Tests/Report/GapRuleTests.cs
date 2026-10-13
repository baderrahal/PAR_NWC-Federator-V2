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

        private static ClashItem Item(string sourceFile, string idFrom)
        {
            ClashItem item = new ClashItem();
            item.SourceFile = sourceFile;
            item.IdFrom = idFrom;
            return item;
        }

        private static ClashItem Empty()
        {
            return new ClashItem();
        }

        private static ClashReport NoRows()
        {
            ClashReport report = new ClashReport("1B06PH", "name");
            report.AddTest("no rows");
            return report;
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
            ClashItem full = Item("ME.nwc", "Id");
            IList<ReportGap> gaps = GapRule.For(ReportWith(full, full));

            Assert.That(gaps.Count, Is.EqualTo(2));

            foreach (string name in new[] { "Source File", "Id From" })
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
            ClashItem full = Item("ME.nwc", "Id");
            ReportGap gap = Find(GapRule.For(ReportWith(full, full)), "Source File");

            Assert.That(gap.Name, Is.EqualTo("Source File"));
            Assert.That(gap.Value, Does.Contain("2 of 2 item cells carry it"));
            Assert.That(gap.WouldBelong, Does.Contain("Q38"));

            Assert.That(gap.Line(), Does.StartWith("Source File"));
            Assert.That(gap.Line(), Does.Contain("2 of 2"));
            Assert.That(gap.Line(), Does.Contain("Q38"));
        }

        /// <summary>
        /// A report with no item read nothing, so nothing was measured and nothing is a gap.
        /// Where items were read, Q76 says a property empty on every one, the test below.
        /// </summary>
        [Test]
        public void AReportWithNoItemHasNoGap()
        {
            Assert.That(GapRule.For(NoRows()), Is.Empty);
        }

        [Test]
        public void APropertySomeItemsCarriedIsCountedAndOneNoneCarriedIsSaidEmpty()
        {
            ClashItem some = Item("ME.nwc", null);
            IList<ReportGap> gaps = GapRule.For(ReportWith(some, Empty()));

            Assert.That(gaps.Count, Is.EqualTo(2));
            Assert.That(Find(gaps, "Source File").Value, Is.EqualTo("1 of 2 item cells carry it"));
            Assert.That(Find(gaps, "Id From").Value, Is.EqualTo("read on 2 item cells and empty on every one"));
        }

        [Test]
        public void OneItemCellReadAndEmptyReadsInTheSingular()
        {
            ClashReport report = new ClashReport("1B06PH", "name");
            TestReport test = report.AddTest("one sided");
            ClashRow row = new ClashRow();
            row.Left = Empty();
            row.Right = null;
            test.Add(row);

            Assert.That(Find(GapRule.For(report), "Source File").Value, Is.EqualTo("read on 1 item cell and empty on it"));
        }

        [Test]
        public void TheValueIsCountedOverEveryItemCellAndNotOverEveryRow()
        {
            ClashItem full = Item("ME.nwc", "Id");

            // Three rows, two item cells each, and only the left of each carries a source file.
            ClashReport report = ReportWith(full, Empty(), full, Empty(), full, Empty());
            ReportGap gap = Find(GapRule.For(report), "Source File");

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
            row.Left = Item("ME.nwc", null);
            row.Right = null;
            test.Add(row);

            ReportGap gap = Find(GapRule.For(report), "Source File");

            Assert.That(gap.Carried, Is.EqualTo(1));
            Assert.That(gap.OutOf, Is.EqualTo(1));
            Assert.That(gap.Value, Does.Contain("1 of 1 item cell carries it"));
        }

        /// <summary>
        /// Q76, FR-198. A property read on every item and empty every time is said, with its
        /// count, where the block left it out and read as an output carrying it.
        /// </summary>
        [Test]
        public void APropertyReadOnEveryItemAndEmptyEveryTimeIsSaidWithItsCount()
        {
            IList<ReportGap> gaps = GapRule.For(ReportWith(Empty(), Empty(), Empty(), Empty()));
            ReportGap gap = Find(gaps, "Source File");

            Assert.That(gap, Is.Not.Null, "a property read and empty on every item is left out");
            Assert.That(gap.Carried, Is.EqualTo(0));
            Assert.That(gap.OutOf, Is.EqualTo(4));
            Assert.That(gap.Value, Is.EqualTo("read on 4 item cells and empty on every one"));
            Assert.That(gap.Line(), Does.StartWith("Source File"));
        }

        /// <summary>
        /// Q35, Q36, Q37 and Q39, FR-198. Family, Type Name, Material and the per item
        /// discipline are deleted with their harvest, so the item holds none of them and the
        /// block names none of them.
        /// </summary>
        [Test]
        public void TheFourDeletedPropertiesAreNotOnTheItem()
        {
            foreach (string gone in new[] { "Family", "Type", "Material", "Discipline" })
            {
                Assert.That(typeof(ClashItem).GetProperty(gone), Is.Null, gone + " is still on the item");
            }
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
            IList<string> lines = GapRule.Lines(NoRows());

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Is.EqualTo(GapRule.NothingHeldBack()));
            Assert.That(lines[0], Does.Contain("nothing measured this group reaches no output"));
        }

        /// <summary>
        /// A group whose clash was skipped has no report, and its block said the report
        /// carries everything the run read, about a report that was never made. Where the
        /// engine says why there is no report, the block says that instead. A group with no
        /// report for any other reason is left as it was.
        /// </summary>
        [Test]
        public void AGroupWhoseClashWasSkippedSaysNoReportWasMade()
        {
            IList<string> lines = GapRule.Lines(null, "clash skipped, models not on the same shared coordinates");

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Is.EqualTo(
                "no clash report was made, clash skipped, models not on the same shared coordinates, so nothing was"
                + " measured that an output could hold back"));
            Assert.That(GapRule.Lines(null)[0], Is.EqualTo(GapRule.NothingHeldBack()));
        }

        [Test]
        public void TheBlockCountsWhatItHoldsAndSaysItIsNotAFault()
        {
            ClashItem full = Item("ME.nwc", "Id");
            IList<string> lines = GapRule.Lines(ReportWith(full, full));

            Assert.That(lines.Count, Is.EqualTo(3), "one heading and two gaps");
            Assert.That(lines[0], Does.Contain("2 numbers are"));
            Assert.That(lines[0], Does.Contain("nothing acts on it"));
            Assert.That(lines[1], Does.StartWith("Source File"));
            Assert.That(lines[2], Does.StartWith("Id From"));
        }

        // ---------- the run ----------

        [Test]
        public void TheRunCountsGapsByNameAndNotByLine()
        {
            ClashItem full = Item("ME.nwc", "Id");
            GapTally tally = new GapTally();

            // The same two on twenty two groups is still two things this tool does not
            // show, not 44.
            for (int i = 0; i < 22; i++)
            {
                tally.Add(GapRule.For(ReportWith(full, full)));
            }

            Assert.That(tally.Distinct, Is.EqualTo(2));
            Assert.That(tally.Groups, Is.EqualTo(22));
            Assert.That(tally.Line(), Does.Contain("2 things are"));
            Assert.That(tally.Line(), Does.Contain("over 22 groups"));
            Assert.That(tally.Line(), Does.Contain("Source File"));
            Assert.That(tally.Line(), Does.Contain("rather than a fault"));
        }

        [Test]
        public void ARunThatHeldNothingBackSaysSo()
        {
            GapTally tally = new GapTally();
            tally.Add(GapRule.For(NoRows()));

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
            ReportGap gap = new ReportGap("Source File", null, null);

            Assert.That(gap.Line(), Does.Contain("UNKNOWN"));
            Assert.That(gap.Line(), Does.Contain("UNKNOWN where it would belong"));
        }
    }
}
