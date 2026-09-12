using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// What counts as still outstanding.
    ///
    /// The clash API has no open against closed notion at all, see docs\history\scan.md
    /// section 4h, so neither reading is off it. Both are stated rules. Since the Summary
    /// and Matrix sheets went and the window stopped offering the choice, the only reader
    /// left is the image status filter, which asks for what Navisworks counts as open.
    /// </summary>
    [TestFixture]
    public class OpenClashesTests
    {
        [Test]
        public void NewAndActiveCountsTwoStatuses()
        {
            Assert.That(OpenClashes.StatusesFor(OpenClashCount.NewAndActive).Length, Is.EqualTo(2));
        }

        [Test]
        public void NavisworksOpenAddsReviewed()
        {
            Assert.That(OpenClashes.StatusesFor(OpenClashCount.NavisworksOpen).Length, Is.EqualTo(3));
        }

        [Test]
        public void ApprovedAndResolvedAreClosedUnderBoth()
        {
            foreach (OpenClashCount which in new[] { OpenClashCount.NewAndActive, OpenClashCount.NavisworksOpen })
            {
                Assert.That(OpenClashes.StatusesFor(which), Does.Not.Contain(ClashStatus.Approved), which.ToString());
                Assert.That(OpenClashes.StatusesFor(which), Does.Not.Contain(ClashStatus.Resolved), which.ToString());
            }
        }

        [Test]
        public void EachTestReportsHowManyAreResolved()
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001");
            TestReport test = report.AddTest("AR v ME");
            test.State = TestState.FoundClashes;

            ClashRow row = new ClashRow();
            row.Name = "Resolved";
            row.Status = ClashStatus.Resolved;
            row.RawClashes = 40;
            row.IsGroup = true;
            test.Add(row);

            Assert.That(test.Resolved, Is.EqualTo(40),
                "resolved clashes stay in the file and this is how the growth is seen");
            Assert.That(test.RawClashes, Is.EqualTo(40));
        }

        [Test]
        public void NothingCompactedIsMinusOneRatherThanZero()
        {
            Assert.That(new ClashReport("b", "n").CompactedAway, Is.EqualTo(-1),
                "zero removed and never asked to remove are different things");
            Assert.That(new ClashRunOutcome().Compacted, Is.EqualTo(-1));
        }
    }
}
