using System;
using System.Collections.Generic;

namespace Federator.Core.Teams
{
    /// <summary>
    /// The discipline code a side of a clash test carries in its set name, F131, Q116 by its
    /// default A. BLD-ME-Ducts is ME, BLD-FF-Sprinklers is FF. A side's team is the team map's team
    /// of that code. A model's code is part 5 of its file name, read by ContainerName.Parse as it
    /// always was, and its team is the map's team of that code, so no second reader of a file
    /// name is written here.
    ///
    /// THE CODE IS THE FIRST PART OF THE NAME THAT IS A KNOWN CODE, split on the set name
    /// separator, matched Ordinal and never trimmed, because a code is read off a name and two
    /// of the client's set names end in a space. The known codes are the team map's and those of
    /// the group's own models, TeamMap.KnownCodes, so a code that is a team of its own is still
    /// found. A name carrying none gives no code, which is UNKNOWN as a team, Q117 by its default
    /// A, counted and named and never guessed: the client's own matrix holds BLD-Security Devices.
    ///
    /// THE ONE PLACE A SET NAME'S CODE IS READ. The views' pair rule, DisciplinePairRule.CodeIn,
    /// reads it here with its own seven codes until F114 replaces it.
    /// </summary>
    public static class CodeOf
    {
        /// <summary>The first part of that set name that is one of those codes, or empty where none is.</summary>
        public static string Set(string setName, IEnumerable<string> knownCodes, char separator)
        {
            if (string.IsNullOrEmpty(setName) || knownCodes == null)
            {
                return string.Empty;
            }

            List<string> known = new List<string>(knownCodes);

            foreach (string part in setName.Split(separator))
            {
                if (part.Length > 0 && known.Exists(code => string.Equals(code, part, StringComparison.Ordinal)))
                {
                    return part;
                }
            }

            return string.Empty;
        }
    }
}
