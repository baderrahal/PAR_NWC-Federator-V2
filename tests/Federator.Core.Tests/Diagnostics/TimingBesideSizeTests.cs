using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Q101, Bader's answer on 2026-10-04: the 45 minutes is not judged in this round, and
    /// each group's time is reported beside the sizes of its NWC files and its item counts,
    /// so the target can be set after the proof run. Every number in the block is one the
    /// run already read, and the block says where each comes from.
    /// </summary>
    [TestFixture]
    public class TimingBesideSizeTests
    {
        private static GroupSize G1()
        {
            return new GroupSize("1B06G1", 2056.835, 4, 7142410, 3539, 1511, 1165);
        }

        private static GroupSize M1()
        {
            return new GroupSize("1B06M1", 61.25, 2, 900000, 120, 20, 20);
        }

        private static string All(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        private static string RowOf(IList<string> lines, string building)
        {
            foreach (string line in lines)
            {
                if (line.StartsWith(building, StringComparison.Ordinal))
                {
                    return line;
                }
            }

            return null;
        }

        [Test]
        public void EachGroupsTimeStandsBesideWhatItHeld()
        {
            IList<string> lines = TimingBlock.BesideSize(new[] { G1() });
            string row = RowOf(lines, "1B06G1");

            Assert.That(row, Is.Not.Null);
            Assert.That(row, Does.Contain("2056.835s"));
            Assert.That(row, Does.Contain("4 NWC files"));
            Assert.That(row, Does.Contain("7,142,410 bytes"));
            Assert.That(row, Does.Contain("3,539 elements"));
            Assert.That(row, Does.Contain("1,511 clashes"));
            Assert.That(row, Does.Contain("1,165 viewpoints"));
        }

        [Test]
        public void TheGroupsAreListedSlowestFirst()
        {
            IList<string> lines = TimingBlock.BesideSize(new[] { M1(), G1() });
            string all = All(lines);

            Assert.That(lines[0], Is.EqualTo("by group, slowest first, with what each group held"));
            Assert.That(all.IndexOf("1B06G1", StringComparison.Ordinal),
                Is.LessThan(all.IndexOf("1B06M1", StringComparison.Ordinal)));
        }

        /// <summary>
        /// The break. A number the run could not read is UNKNOWN on the row and never a
        /// zero, because zero reads as a real count, and a group that ran no clash step is
        /// not a group with no clashes.
        /// </summary>
        [Test]
        public void ANumberThatCouldNotBeReadSaysUnknownAndNeverZero()
        {
            GroupSize unread = new GroupSize(
                "1B06PP", 693.6, 8, GroupSize.Unknown, GroupSize.Unknown, GroupSize.Unknown, GroupSize.Unknown);

            string row = RowOf(TimingBlock.BesideSize(new[] { unread }), "1B06PP");

            Assert.That(row, Does.Contain("8 NWC files"));
            Assert.That(row, Does.Contain("UNKNOWN bytes"));
            Assert.That(row, Does.Contain("UNKNOWN elements"));
            Assert.That(row, Does.Contain("UNKNOWN clashes"));
            Assert.That(row, Does.Contain("UNKNOWN viewpoints"));
            Assert.That(row, Does.Not.Contain(" 0 "));
        }

        [Test]
        public void OneOfAThingReadsInTheSingular()
        {
            GroupSize one = new GroupSize("1C06PK", 9.7, 1, 2048, 1, 1, 1);
            string row = RowOf(TimingBlock.BesideSize(new[] { one }), "1C06PK");

            Assert.That(row, Does.Contain("1 NWC file "));
            Assert.That(row, Does.Contain("1 element "));
            Assert.That(row, Does.Contain("1 clash "));
            Assert.That(row, Does.EndWith("1 viewpoint"));
        }

        [Test]
        public void TheBlockSaysWhereEachNumberComesFrom()
        {
            string all = All(TimingBlock.BesideSize(new[] { G1() }));

            Assert.That(all, Does.Contain("where each number comes from"));
            Assert.That(all, Does.Contain("seconds     the group's own clock, the number on its GROUP finished line"));
            Assert.That(all, Does.Contain("NWC         the files the group was handed, each sized on the disk when the group finished"));
            Assert.That(all, Does.Contain("elements    the Revit elements the EXPORT CHECK counted in the group's models"));
            Assert.That(all, Does.Contain("clashes     every clash of every test the group ran, as its CLASH finished line counts them"));
            Assert.That(all, Does.Contain("viewpoints  the viewpoints the VIEWS step created, as its VIEWS BUILT block counts them"));
            Assert.That(all, Does.Contain("UNKNOWN is a number that could not be read, and never a zero"));
        }

        /// <summary>
        /// Q101: the 45 minutes is not judged in this round. This block says nothing about
        /// it, and the line that does is left where it is, the last of the run block.
        /// </summary>
        [Test]
        public void TheBlockJudgesNoTargetAndTheFortyFiveMinuteLineStaysWhereItIs()
        {
            string all = All(TimingBlock.BesideSize(new[] { G1(), M1() }));

            Assert.That(all, Does.Not.Contain("OVER"));
            Assert.That(all, Does.Not.Contain("inside the"));
            Assert.That(all, Does.Not.Contain(TimingBlock.Clock(TimingBlock.UnattendedSeconds)));
        }

        [Test]
        public void NoGroupSaysSoRatherThanAnEmptyBlock()
        {
            Assert.That(TimingBlock.BesideSize(new GroupSize[0]), Does.Contain("no group ran"));
            Assert.That(TimingBlock.BesideSize(null), Does.Contain("no group ran"));
        }

        [Test]
        public void TheTitleSitsWithTheOtherTimingBlocks()
        {
            Assert.That(TimingBlock.SizeTitle, Does.StartWith(TimingBlock.GroupTitle));
        }

        // ---------- what the engine hands in ----------

        [Test]
        public void TheBytesAreTheFilesAddedUp()
        {
            Assert.That(GroupSize.BytesOf(new long[] { 525284, 6473674, 104247, 39205 }), Is.EqualTo(7142410L));
        }

        /// <summary>The break. One file not on the disk makes the total UNKNOWN rather than a smaller number that reads as real.</summary>
        [Test]
        public void OneFileNotOnTheDiskMakesTheBytesUnknown()
        {
            Assert.That(GroupSize.BytesOf(new long[] { 525284, -1, 104247 }), Is.EqualTo(GroupSize.Unknown));
        }

        [Test]
        public void NoFileIsNoBytes()
        {
            Assert.That(GroupSize.BytesOf(new long[0]), Is.EqualTo(0L));
            Assert.That(GroupSize.BytesOf(null), Is.EqualTo(0L));
        }

        [Test]
        public void TheElementsAreTheExportCheckCountsAddedUp()
        {
            IList<ModelExport> models = new List<ModelExport>
            {
                new ModelExport("AR.nwc", "AR", 429, 429, 429, new List<string>()),
                new ModelExport("ME.nwc", "ME", 2946, 2946, 2946, new List<string>()),
                new ModelExport("ST.nwc", "ST", 135, 135, 135, new List<string>()),
                new ModelExport("ST2.nwc", "ST", 29, 29, 29, new List<string>())
            };

            Assert.That(GroupSize.ElementsIn(models), Is.EqualTo(3539L));
        }

        /// <summary>The break. A model whose count could not be taken makes the group's UNKNOWN, never a total that leaves it out.</summary>
        [Test]
        public void AModelThatCouldNotBeCountedMakesTheElementsUnknown()
        {
            IList<ModelExport> models = new List<ModelExport>
            {
                new ModelExport("AR.nwc", "AR", 429, 429, 429, new List<string>()),
                new ModelExport("ME.nwc", "ME", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted, new List<string>())
            };

            Assert.That(GroupSize.ElementsIn(models), Is.EqualTo(GroupSize.Unknown));
        }

        [Test]
        public void NoModelReadMakesTheElementsUnknown()
        {
            Assert.That(GroupSize.ElementsIn(new List<ModelExport>()), Is.EqualTo(GroupSize.Unknown));
            Assert.That(GroupSize.ElementsIn(null), Is.EqualTo(GroupSize.Unknown));
        }

        /// <summary>The machine readable log carries what the block carries, the numbers as numbers.</summary>
        [Test]
        public void TheRowCarriesTheSameNumbersWithNoSeparators()
        {
            Assert.That(G1().RowText(),
                Is.EqualTo("4 NWC files, 7142410 bytes, 3539 elements, 1511 clashes, 1165 viewpoints"));

            GroupSize unread = new GroupSize(
                "x", 1.0, 2, GroupSize.Unknown, GroupSize.Unknown, GroupSize.Unknown, GroupSize.Unknown);

            Assert.That(unread.RowText(),
                Is.EqualTo("2 NWC files, UNKNOWN bytes, UNKNOWN elements, UNKNOWN clashes, UNKNOWN viewpoints"));
        }

        [Test]
        public void AGroupWithNoNameIsRefused()
        {
            Assert.That(() => new GroupSize(null, 1.0, 1, 1, 1, 1, 1), Throws.ArgumentNullException);
        }
    }
}
