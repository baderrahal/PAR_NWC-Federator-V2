using Federator.Core.Teams;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The pair of two teams and its one order, F131, Q114 points 10, 11 and 12 and Q114's B12 by
    /// its default A: the map's line order, so one pair is always written the same way round
    /// and never becomes two folders. Two codes of one team pair as that team against itself.
    /// The size folder sits in a pair holding a team the map names for it.
    /// </summary>
    [TestFixture]
    public class TeamPairTests
    {
        private const string Vs = ViewpointSettings.DefaultPairSeparator;

        private static TeamPair PairOfCodes(string left, string right)
        {
            TeamMap map = TeamMapTests.MapOf(TeamMapTests.BadersMap);

            return TeamPair.For(map.TeamOf(left), map.TeamOf(right), map, Vs);
        }

        [Test]
        public void APairIsWrittenTheSameWayRoundWhicheverSideComesFirst()
        {
            Assert.That(PairOfCodes("ST", "HV").Folder, Is.EqualTo("Structure vs Mechanical"));
            Assert.That(PairOfCodes("HV", "ST").Folder, Is.EqualTo("Structure vs Mechanical"));
            Assert.That(PairOfCodes("EL", "ME").Folder, Is.EqualTo("Mechanical vs Electrical"));
            Assert.That(PairOfCodes("ME", "EL").Folder, Is.EqualTo("Mechanical vs Electrical"));
            Assert.That(PairOfCodes("XX", "AR").Folder, Is.EqualTo("Architecture vs XX"));
            Assert.That(PairOfCodes("LS", "CV").Folder, Is.EqualTo("CV vs LS"));
            Assert.That(PairOfCodes("EL", string.Empty).Folder, Is.EqualTo("Electrical vs UNKNOWN"));
            Assert.That(PairOfCodes(string.Empty, "EL").Folder, Is.EqualTo("Electrical vs UNKNOWN"));

            TeamPair pair = PairOfCodes("HV", "ST");

            Assert.That(pair.First, Is.EqualTo("Structure"));
            Assert.That(pair.Second, Is.EqualTo("Mechanical"));
        }

        /// <summary>Point 10: two codes of one team, such as HV against PL, go in that team against itself.</summary>
        [Test]
        public void TwoCodesOfOneTeamPairAsThatTeamAgainstItself()
        {
            TeamPair pair = PairOfCodes("HV", "PL");

            Assert.That(pair.Folder, Is.EqualTo("Mechanical vs Mechanical"));
            Assert.That(pair.SameTeam, Is.True);
            Assert.That(PairOfCodes("ST", "HV").SameTeam, Is.False);
        }

        /// <summary>Point 11: the size folder sits in a pair holding Mechanical or Electrical, and in no other.</summary>
        [Test]
        public void OnlyAPairHoldingASizeFolderTeamCarriesIt()
        {
            Assert.That(PairOfCodes("ST", "HV").CarriesSizeFolder, Is.True);
            Assert.That(PairOfCodes("EL", "AR").CarriesSizeFolder, Is.True);
            Assert.That(PairOfCodes("HV", "PL").CarriesSizeFolder, Is.True);
            Assert.That(PairOfCodes("AR", "ST").CarriesSizeFolder, Is.False);
            Assert.That(PairOfCodes("LS", string.Empty).CarriesSizeFolder, Is.False);
        }

        /// <summary>With no map every code is a team of its own, the pair is of codes by name, and none carries the size folder.</summary>
        [Test]
        public void WithNoMapAPairIsOfCodesAndCarriesNoSizeFolder()
        {
            TeamMap none = TeamMap.NoXml(new TeamMapSettings());
            TeamPair pair = TeamPair.For(none.TeamOf("EL"), none.TeamOf("AR"), none, Vs);

            Assert.That(pair.Folder, Is.EqualTo("AR vs EL"));
            Assert.That(pair.CarriesSizeFolder, Is.False);
            Assert.That(TeamPair.For(none.TeamOf("ME"), none.TeamOf("HV"), none, Vs).Folder, Is.EqualTo("HV vs ME"));
        }

        [Test]
        public void TheSeparatorIsASetting()
        {
            TeamMap map = TeamMapTests.MapOf(TeamMapTests.BadersMap);

            Assert.That(TeamPair.For("Mechanical", "Structure", map, " / ").Folder, Is.EqualTo("Structure / Mechanical"));
            Assert.That(() => TeamPair.For("Mechanical", "Structure", null, Vs), Throws.ArgumentNullException);
        }
    }
}
