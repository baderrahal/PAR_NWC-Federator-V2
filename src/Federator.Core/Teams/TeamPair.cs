using System;

namespace Federator.Core.Teams
{
    /// <summary>
    /// The two teams of a clash test and the folder they share, F131, Q114 points 10, 11 and 12.
    ///
    /// ONE WAY ROUND, point 12 and Q114's B12 by its default A: the two are put in the team map's
    /// order, TeamMap.Compare, the map's lines first, then any other team by its name, then
    /// UNKNOWN. So Structure against Mechanical and Mechanical against Structure are one folder,
    /// Structure vs Mechanical, and one pair never becomes two folders.
    ///
    /// NO TEAMS MIXED, point 10. A pair is of teams and never of codes, so HV against PL is
    /// Mechanical vs Mechanical and Mechanical vs Electrical is its own folder.
    ///
    /// THE SIZE FOLDER, point 11, is carried by a pair holding a team the map's size-folder line
    /// names, and with no map by none.
    /// </summary>
    public sealed class TeamPair
    {
        private TeamPair(string first, string second, string folder, bool sameTeam, bool carriesSizeFolder)
        {
            First = first;
            Second = second;
            Folder = folder;
            SameTeam = sameTeam;
            CarriesSizeFolder = carriesSizeFolder;
        }

        /// <summary>The team written first.</summary>
        public string First { get; private set; }

        /// <summary>The team written second.</summary>
        public string Second { get; private set; }

        /// <summary>The pair folder's name, the first team, the separator, the second.</summary>
        public string Folder { get; private set; }

        /// <summary>Whether both sides are of one team, such as HV against PL in Mechanical vs Mechanical.</summary>
        public bool SameTeam { get; private set; }

        /// <summary>Whether this pair's folder carries the size folder, point 11.</summary>
        public bool CarriesSizeFolder { get; private set; }

        /// <summary>
        /// The pair of those two teams in that map's order, its folder named with that separator,
        /// ViewpointSettings.PairSeparator, a setting.
        /// </summary>
        public static TeamPair For(string teamA, string teamB, TeamMap map, string separator)
        {
            if (map == null)
            {
                throw new ArgumentNullException("map");
            }

            string first = string.IsNullOrEmpty(teamA) ? map.UnknownTeam : teamA;
            string second = string.IsNullOrEmpty(teamB) ? map.UnknownTeam : teamB;

            if (map.Compare(first, second) > 0)
            {
                string swap = first;
                first = second;
                second = swap;
            }

            return new TeamPair(
                first,
                second,
                first + (separator ?? string.Empty) + second,
                string.Equals(first, second, StringComparison.Ordinal),
                map.CarriesSizeFolder(first) || map.CarriesSizeFolder(second));
        }
    }
}
