using System;
using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>
    /// How the views read a team, F114, Q114 points 10 to 12, with Q116 answered A: the team map
    /// applies to the views. A side's team is TeamMap.TeamOfSet, the one rule the TEAMS lines
    /// read too, Q117 answered C and A: the team of the code its set name carries, CodeOf.Set,
    /// and where it carries none the team a folder above it in the clash XML's set tree names,
    /// or UNKNOWN. The pair and its one order are TeamPair's, read here and never copied.
    ///
    /// A SIDE IS NAMED BY ITS SET NAME ALONE, ViewClash.LeftSet, so two sets of one name whose
    /// folders give two teams read UNKNOWN, never one of the two guessed.
    ///
    /// THE KNOWN CODES are the map's and the group's own models' codes, TeamMap.KnownCodes, so a
    /// code that is a team of its own is still read off a set name.
    ///
    /// A SET WITH NO NAME, which the reader gives a set or a set folder with no name attribute, is
    /// one no side can name, so it is left out and never throws. A side with no set name reads
    /// UNKNOWN, as ExportCheck and CodeOf treat a set with no name. A side read as UNKNOWN is said
    /// with why, UnknownWords, and the words say only what was read.
    /// </summary>
    public sealed class ViewTeams
    {
        private const string NoCode = ", no code this group knows, and ";

        private readonly IList<string> knownCodes;
        private readonly char setNameSeparator;
        private readonly string pairSeparator;
        private readonly bool treeRead;
        private readonly Dictionary<string, List<IList<string>>> foldersOf =
            new Dictionary<string, List<IList<string>>>(StringComparer.Ordinal);

        /// <summary>The teams of a group's views, with the clash XML's sets, or null where no set tree was read.</summary>
        public ViewTeams(TeamMap map, IEnumerable<SelectionSetDefinition> sets, IEnumerable<string> groupCodes, ViewpointSettings settings)
        {
            if (map == null)
            {
                throw new ArgumentNullException("map");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            Map = map;
            knownCodes = map.KnownCodes(groupCodes);
            setNameSeparator = settings.SetNameSeparator;
            pairSeparator = settings.PairSeparator;
            treeRead = sets != null;

            foreach (SelectionSetDefinition set in sets ?? new SelectionSetDefinition[0])
            {
                List<IList<string>> trees;

                if (string.IsNullOrEmpty(set.Name))
                {
                    continue;
                }

                if (!foldersOf.TryGetValue(set.Name, out trees))
                {
                    trees = new List<IList<string>>();
                    foldersOf[set.Name] = trees;
                }

                trees.Add(set.Folders);
            }
        }

        /// <summary>The team map these teams are read through.</summary>
        public TeamMap Map { get; private set; }

        /// <summary>The team of a set name, TeamMap.TeamOfSet over every set of that name, UNKNOWN where they give two.</summary>
        public string TeamOfSet(string setName)
        {
            string why;
            return Read(setName, out why);
        }

        /// <summary>The set name of a side read as UNKNOWN and why, FR-074, or null where the side reads a team.</summary>
        public string UnknownWords(string setName)
        {
            string why;
            string team = Read(setName, out why);

            return string.Equals(team, Map.UnknownTeam, StringComparison.Ordinal) ? why : null;
        }

        /// <summary>The team of a set name, and the words saying why, which are read only where the team is UNKNOWN.</summary>
        private string Read(string setName, out string why)
        {
            if (string.IsNullOrEmpty(setName))
            {
                why = "a side with no set name, so no code and no folder above it could be read";
                return Map.UnknownTeam;
            }

            string code = CodeOf.Set(setName, knownCodes, setNameSeparator);
            List<IList<string>> trees;

            if (code.Length > 0)
            {
                why = setName + ", its code " + code + " reads as the team " + Map.UnknownTeam;
                return Map.TeamOfSet(code, null);
            }

            if (!treeRead)
            {
                why = setName + NoCode + "no set tree was read, so whether a folder above it names a team is " + Map.UnknownTeam;
                return Map.TeamOfSet(code, null);
            }

            if (!foldersOf.TryGetValue(setName, out trees))
            {
                why = setName + NoCode + "no set of that name is in the clash XML's set tree";
                return Map.TeamOfSet(code, null);
            }

            List<string> teams = new List<string>();

            foreach (IList<string> folders in trees)
            {
                string team = Map.TeamOfSet(code, folders);

                if (!teams.Contains(team))
                {
                    teams.Add(team);
                }
            }

            if (teams.Count > 1)
            {
                why = setName + NoCode + "its " + trees.Count + " sets of that name in the clash XML's set tree give the teams "
                    + string.Join(", ", teams.ToArray());
                return Map.UnknownTeam;
            }

            why = setName + NoCode + "no folder above it in the clash XML's set tree names a team";
            return teams[0];
        }

        /// <summary>The team pair of a test's two sides, in the map's one order.</summary>
        public TeamPair PairOf(string leftSet, string rightSet)
        {
            return TeamPair.For(TeamOfSet(leftSet), TeamOfSet(rightSet), Map, pairSeparator);
        }
    }
}
