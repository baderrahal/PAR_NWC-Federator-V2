using System;
using System.Collections.Generic;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>
    /// How the views read a team, F114, Q114 points 10 to 12, with Q116 by its default A: the
    /// team map applies to the views. A side's team is the map's team of the code its set name
    /// carries, CodeOf.Set, and a model's team the map's team of the code its file name
    /// carries. The pair and its one order are F131's TeamPair, read here and never copied.
    ///
    /// THE KNOWN CODES are the map's and the group's own models' codes, TeamMap.KnownCodes, so a
    /// code that is a team of its own is still read off a set name.
    /// </summary>
    public sealed class ViewTeams
    {
        private readonly IList<string> knownCodes;
        private readonly char setNameSeparator;
        private readonly string pairSeparator;

        public ViewTeams(TeamMap map, IEnumerable<string> groupCodes, ViewpointSettings settings)
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
        }

        /// <summary>The team map these teams are read through.</summary>
        public TeamMap Map { get; private set; }

        /// <summary>Whether that set name carries a code this group knows.</summary>
        public bool SetHasCode(string setName)
        {
            return CodeOf.Set(setName, knownCodes, setNameSeparator).Length > 0;
        }

        /// <summary>The team of a set name, UNKNOWN where it carries no code this group knows.</summary>
        public string TeamOfSet(string setName)
        {
            return Map.TeamOf(CodeOf.Set(setName, knownCodes, setNameSeparator));
        }

        /// <summary>The team pair of a test's two sides, in the map's one order.</summary>
        public TeamPair PairOf(string leftSet, string rightSet)
        {
            return TeamPair.For(TeamOfSet(leftSet), TeamOfSet(rightSet), Map, pairSeparator);
        }
    }
}
