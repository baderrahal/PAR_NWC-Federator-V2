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
    /// </summary>
    public sealed class ViewTeams
    {
        private readonly IList<string> knownCodes;
        private readonly char setNameSeparator;
        private readonly string pairSeparator;
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

            foreach (SelectionSetDefinition set in sets ?? new SelectionSetDefinition[0])
            {
                List<IList<string>> trees;

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
            string code = CodeOf.Set(setName, knownCodes, setNameSeparator);
            List<IList<string>> trees;

            if (setName == null || !foldersOf.TryGetValue(setName, out trees))
            {
                return Map.TeamOfSet(code, null);
            }

            string team = Map.TeamOfSet(code, trees[0]);

            for (int i = 1; i < trees.Count; i++)
            {
                if (!string.Equals(Map.TeamOfSet(code, trees[i]), team, StringComparison.Ordinal))
                {
                    return Map.UnknownTeam;
                }
            }

            return team;
        }

        /// <summary>The team pair of a test's two sides, in the map's one order.</summary>
        public TeamPair PairOf(string leftSet, string rightSet)
        {
            return TeamPair.For(TeamOfSet(leftSet), TeamOfSet(rightSet), Map, pairSeparator);
        }
    }
}
