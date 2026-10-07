using System.Collections.Generic;
using Federator.Core.Teams;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The pair of two teams, its one order and its size folder, Q114 points 10, 11 and 12 and
    /// Q114's B12 by its default A, built by F131 and carried into F114 with its tests by Bader's
    /// answer B to Q134: the map's line order, so one pair is always written the same way round
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

        /// <summary>
        /// Point 12. A pair is always written the same way round, the teams in the order of the
        /// map's lines, then any other team by its name, then UNKNOWN last. With no map every
        /// code is a team of its own, by its name, and UNKNOWN still last.
        /// </summary>
        [Test]
        public void TeamsAreOrderedByTheMapLinesThenOtherTeamsByNameThenUnknownLast()
        {
            TeamMap map = TeamMapTests.MapOf(TeamMapTests.BadersMap);
            List<string> teams = new List<string> { "UNKNOWN", "LS", "Electrical", "CV", "Mechanical", "Structure", "Architecture" };

            teams.Sort((x, y) => TeamPair.Compare(map, x, y));

            TeamMapTests.Same(teams, "Architecture", "Structure", "Mechanical", "Electrical", "CV", "LS", "UNKNOWN");
            Assert.That(TeamPair.Compare(map, "Mechanical", "Mechanical"), Is.EqualTo(0));

            TeamMap none = TeamMap.NoXml(new TeamMapSettings());
            List<string> codes = new List<string> { "UNKNOWN", "XX", "ST", "EL", "AR" };

            codes.Sort((x, y) => TeamPair.Compare(none, x, y));

            TeamMapTests.Same(codes, "AR", "EL", "ST", "XX", "UNKNOWN");
        }

        /// <summary>Point 11. Only a team the size-folder line names carries the size folder, and no team does with no map.</summary>
        [Test]
        public void OnlyTheTeamsTheMapNamesCarryTheSizeFolder()
        {
            TeamMap map = TeamMapTests.MapOf(TeamMapTests.BadersMap);

            Assert.That(TeamPair.TeamCarriesSizeFolder(map, "Mechanical"), Is.True);
            Assert.That(TeamPair.TeamCarriesSizeFolder(map, "Electrical"), Is.True);

            foreach (string team in new[] { "Architecture", "Structure", "LS", "UNKNOWN", "mechanical" })
            {
                Assert.That(TeamPair.TeamCarriesSizeFolder(map, team), Is.False, team);
            }

            Assert.That(TeamPair.TeamCarriesSizeFolder(TeamMapTests.MapOf("team: Mechanical | ME\n"), "Mechanical"), Is.False);
            Assert.That(TeamPair.TeamCarriesSizeFolder(TeamMap.NoXml(new TeamMapSettings()), "ME"), Is.False);
        }

        /// <summary>
        /// F114 attempt 7, the breaker's finding 1. The TEAMS lines on the order of a pair and the
        /// size folder are read off this rule and say what Compare and TeamCarriesSizeFolder do.
        /// A map naming a team by the UnknownTeam word orders it by its line, so its line does not
        /// say UNKNOWN comes last.
        /// </summary>
        [Test]
        public void TheTeamsLinesSayWhatThePairRuleDoes()
        {
            TeamMap map = TeamMapTests.MapOf(TeamMapTests.BadersMap);
            IList<string> lines = map.Lines();

            Assert.That(lines, Has.Member("TEAMS    a pair is written in the order Architecture, Structure, Mechanical, Electrical,"
                + " then any other team by its name, then UNKNOWN"));
            Assert.That(lines, Has.Member("TEAMS    a pair holding Mechanical or Electrical carries the size folder"));

            foreach (string team in map.Teams)
            {
                Assert.That(
                    TeamPair.TeamCarriesSizeFolder(map, team),
                    Is.EqualTo(lines[lines.Count - 1].Contains(" " + team + " ")),
                    team);
            }

            TeamMap odd = TeamMapTests.MapOf("team: Mechanical | ME\nteam: UNKNOWN | XX\n");
            List<string> teams = new List<string> { "Zed", "UNKNOWN", "Mechanical" };

            teams.Sort((x, y) => TeamPair.Compare(odd, x, y));

            TeamMapTests.Same(teams, "Mechanical", "UNKNOWN", "Zed");
            Assert.That(odd.Lines(), Has.Member("TEAMS    a pair is written in the order Mechanical, UNKNOWN, then any other team by its name"));
            Assert.That(odd.Lines(), Has.Member("TEAMS    no team carries the size folder"));
        }
    }
}
