using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132, Bader's answer B to Q138. A clash both tests of a mirrored pair find is kept once,
    /// under the kept test, and its two copies can carry two statuses. A status a person sets,
    /// Reviewed or Approved, wins over the other statuses, and otherwise New or Active wins
    /// over Resolved. Where his words do not choose, two statuses both a person's that differ,
    /// or both live that differ, the kept test's status stays, the lead's note under Q138.
    /// </summary>
    [TestFixture]
    public class BothFoundStatusTests
    {
        private static readonly ClashStatus[] APersons = { ClashStatus.Reviewed, ClashStatus.Approved };

        private static readonly ClashStatus[] Others = { ClashStatus.New, ClashStatus.Active, ClashStatus.Resolved };

        private static readonly ClashStatus[] Live = { ClashStatus.New, ClashStatus.Active };

        [Test]
        public void AStatusAPersonSetsWinsOverEveryOtherWhicheverTestCarriesIt()
        {
            foreach (ClashStatus persons in APersons)
            {
                foreach (ClashStatus other in Others)
                {
                    Assert.That(BothFoundStatus.Shown(other, persons), Is.EqualTo(persons), other + " kept, " + persons + " mirror");
                    Assert.That(BothFoundStatus.Shown(persons, other), Is.EqualTo(persons), persons + " kept, " + other + " mirror");
                    Assert.That(BothFoundStatus.Why(other, persons), Is.EqualTo("a status a person sets wins"));
                    Assert.That(BothFoundStatus.Why(persons, other), Is.EqualTo("a status a person sets wins"));
                }
            }
        }

        // The case the breaker named: Navisworks marks the kept test's copy Resolved when the
        // kept test no longer finds it, while the mirror still finds it live.
        [Test]
        public void ALiveStatusWinsOverResolvedWhicheverTestCarriesIt()
        {
            foreach (ClashStatus live in Live)
            {
                Assert.That(BothFoundStatus.Shown(ClashStatus.Resolved, live), Is.EqualTo(live));
                Assert.That(BothFoundStatus.Shown(live, ClashStatus.Resolved), Is.EqualTo(live));
                Assert.That(BothFoundStatus.Why(ClashStatus.Resolved, live), Is.EqualTo("a live status wins over Resolved"));
                Assert.That(BothFoundStatus.Why(live, ClashStatus.Resolved), Is.EqualTo("a live status wins over Resolved"));
            }
        }

        [Test]
        public void TwoStatusesBothAPersonsKeepTheKeptTestsStatus()
        {
            Assert.That(BothFoundStatus.Shown(ClashStatus.Reviewed, ClashStatus.Approved), Is.EqualTo(ClashStatus.Reviewed));
            Assert.That(BothFoundStatus.Shown(ClashStatus.Approved, ClashStatus.Reviewed), Is.EqualTo(ClashStatus.Approved));
            Assert.That(BothFoundStatus.Why(ClashStatus.Reviewed, ClashStatus.Approved),
                Is.EqualTo("both are a person's and the kept test's stays"));
        }

        [Test]
        public void TwoLiveStatusesKeepTheKeptTestsStatus()
        {
            Assert.That(BothFoundStatus.Shown(ClashStatus.New, ClashStatus.Active), Is.EqualTo(ClashStatus.New));
            Assert.That(BothFoundStatus.Shown(ClashStatus.Active, ClashStatus.New), Is.EqualTo(ClashStatus.Active));
            Assert.That(BothFoundStatus.Why(ClashStatus.Active, ClashStatus.New),
                Is.EqualTo("both are live and the kept test's stays"));
        }

        [Test]
        public void OneStatusOnBothIsThatStatus()
        {
            foreach (ClashStatus status in ClashTally.AllStatuses)
            {
                Assert.That(BothFoundStatus.Shown(status, status), Is.EqualTo(status));
                Assert.That(BothFoundStatus.Why(status, status), Is.EqualTo("both carry it"));
            }
        }

        // The add-in casts the Navisworks number, so a status this Core does not name can
        // arrive. Which one wins is then UNKNOWN, and the kept test's stays, either way round.
        [Test]
        public void AStatusNotAmongTheFiveLeavesTheKeptTestsStatus()
        {
            ClashStatus unnamed = (ClashStatus)7;

            Assert.That(BothFoundStatus.Shown(unnamed, ClashStatus.Approved), Is.EqualTo(unnamed));
            Assert.That(BothFoundStatus.Shown(ClashStatus.Resolved, unnamed), Is.EqualTo(ClashStatus.Resolved));
            Assert.That(BothFoundStatus.Why(ClashStatus.Resolved, unnamed),
                Is.EqualTo("a status not among the five is UNKNOWN to the rule and the kept test's stays"));
        }
    }
}
