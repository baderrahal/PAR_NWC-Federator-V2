using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Federator.Core.Exchange;

namespace Federator.Core.Teams
{
    /// <summary>
    /// Which discipline codes make one team, read off the map kept BESIDE the picked clash XML,
    /// Q114 points 1, 2, 11 and 12 decided by Bader on 2026-10-04 and Q115 by its default A. The
    /// tool serves many projects, so no project's teams are inside it and nothing here names a
    /// team or a code. This project's map is in the exchange folder beside the corrected XML.
    ///
    /// THE MAP. One team a line, `team: name | code | code`, in the order a pair of teams is
    /// written, point 12, and `size-folder: team | team`, the teams whose pairs carry the size
    /// folder, point 11. Read by ListFile, the one way a list beside the XML is read, so a
    /// comment starts with #, a blank line is skipped and nothing is trimmed.
    ///
    /// ANY OTHER CODE IS A TEAM OF ITS OWN, named by its code, Bader's rule. A name that carries
    /// no code reads as the UnknownTeam setting, UNKNOWN, Q117 by its default A, never a guess.
    ///
    /// A MAP THAT CANNOT BE READ IS SAID, NEVER HALF READ AND NEVER A THROW. A line this does not
    /// know, a code on two teams or twice on one, a team named twice, a team with no code, an
    /// empty team name or code, a code holding a space, a team name with a space at its start or
    /// end, a size-folder line naming a team no team line names, or bytes that are not UTF-8,
    /// make the whole map Unread with its line and why. A map unread, missing, holding no team or
    /// with no XML picked maps nothing: every code is a team of its own and no team carries the
    /// size folder, Q123 by its default A, and the TEAMS lines say which of these it was.
    ///
    /// CODES COMPARE ORDINAL, as they are read off a file name, so hv is not HV.
    ///
    /// WHERE IT APPLIES, Q116 by its default A: the views, and the team written beside the code in
    /// the log, COVERAGE and the form. The grouping, the one-discipline judgement, the alignment
    /// and export checks and the workbook keep the code.
    /// </summary>
    public sealed class TeamMap : IComparer<string>
    {
        /// <summary>A line naming one team and its codes.</summary>
        internal const string TeamMarker = "team:";

        /// <summary>A line naming the teams whose pairs carry the size folder.</summary>
        internal const string SizeFolderMarker = "size-folder:";

        private readonly Dictionary<string, string> teamOf;

        private readonly List<string> sizeFolder;

        private readonly List<List<string>> codesOf;

        private TeamMap(
            string listPath,
            bool missing,
            bool noXmlPicked,
            string unread,
            string unknownTeam,
            IList<string> teams,
            IList<List<string>> codes,
            IList<string> sizeFolderTeams)
        {
            ListPath = listPath;
            Missing = missing;
            NoXmlPicked = noXmlPicked;
            Unread = unread;
            UnknownTeam = string.IsNullOrEmpty(unknownTeam) ? TeamMapSettings.DefaultUnknownTeam : unknownTeam;
            Teams = new ReadOnlyCollection<string>(new List<string>(teams));
            codesOf = new List<List<string>>(codes);
            sizeFolder = new List<string>(sizeFolderTeams);
            teamOf = new Dictionary<string, string>(StringComparer.Ordinal);

            List<string> every = new List<string>();

            for (int i = 0; i < codesOf.Count; i++)
            {
                foreach (string code in codesOf[i])
                {
                    teamOf[code] = Teams[i];
                    every.Add(code);
                }
            }

            Codes = new ReadOnlyCollection<string>(every);
        }

        /// <summary>The full path the map was read from or looked for, or null where no XML was picked.</summary>
        public string ListPath { get; private set; }

        /// <summary>Whether no file is at that path.</summary>
        public bool Missing { get; private set; }

        /// <summary>Whether no clash XML was picked, so no map is beside one, Q123.</summary>
        public bool NoXmlPicked { get; private set; }

        /// <summary>Why the map could not be read, or null where it was read whole or is not there.</summary>
        public string Unread { get; private set; }

        /// <summary>The team of a name carrying no code, the UnknownTeam setting.</summary>
        public string UnknownTeam { get; private set; }

        /// <summary>The teams of the map, in the order of their lines, which is the order a pair is written in.</summary>
        public ReadOnlyCollection<string> Teams { get; private set; }

        /// <summary>Every code the map gives a team, in the order of the lines.</summary>
        public ReadOnlyCollection<string> Codes { get; private set; }

        /// <summary>Whether the map is there and was read whole.</summary>
        public bool IsRead
        {
            get { return !NoXmlPicked && !Missing && Unread == null; }
        }

        /// <summary>Whether the map is there, was read and holds no team, a file of comments or blank lines.</summary>
        public bool HoldsNone
        {
            get { return IsRead && Teams.Count == 0; }
        }

        /// <summary>
        /// The map beside that XML, read when the XML is picked. Not there, it is Missing. There
        /// and unreadable, it is Unread with why. Never a throw.
        /// </summary>
        public static TeamMap Beside(string xmlPath, TeamMapSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            string path = settings.PathBeside(xmlPath);

            if (!File.Exists(path))
            {
                return Nothing(path, true, false, null, settings.UnknownTeam);
            }

            return ListFile.Read(
                path,
                reader => Read(reader, path, settings),
                why => Nothing(path, false, false, why, settings.UnknownTeam));
        }

        /// <summary>The map of a run with no clash XML picked, which has nothing beside which a map could sit, Q123 by its default A.</summary>
        public static TeamMap NoXml(TeamMapSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            return Nothing(null, false, true, null, settings.UnknownTeam);
        }

        /// <summary>
        /// A map read off that text, which came from that path. The first fault makes the whole
        /// map unread, saying which line, so a typo never maps a code in part.
        /// </summary>
        internal static TeamMap Read(TextReader text, string path, TeamMapSettings settings)
        {
            List<string> teams = new List<string>();
            List<List<string>> codes = new List<List<string>>();
            Dictionary<string, string> owner = new Dictionary<string, string>(StringComparer.Ordinal);
            List<string> sizeFolderTeams = new List<string>();
            List<string[]> sizeFolderLines = new List<string[]>();
            string line;
            int number = 0;

            while ((line = text.ReadLine()) != null)
            {
                number++;

                if (ListFile.Skipped(line))
                {
                    continue;
                }

                string at = "line " + number.ToString(CultureInfo.InvariantCulture) + ", \"" + line + "\", ";
                string[] parts = ListFile.PartsAfter(line, TeamMarker);

                if (parts != null)
                {
                    string fault = TeamLineFault(parts, teams, owner);

                    if (fault != null)
                    {
                        return Nothing(path, false, false, at + fault, settings.UnknownTeam);
                    }

                    List<string> its = new List<string>();

                    for (int i = 1; i < parts.Length; i++)
                    {
                        its.Add(parts[i]);
                        owner[parts[i]] = parts[0];
                    }

                    teams.Add(parts[0]);
                    codes.Add(its);
                    continue;
                }

                parts = ListFile.PartsAfter(line, SizeFolderMarker);

                if (parts != null)
                {
                    foreach (string team in parts)
                    {
                        if (team.Length == 0)
                        {
                            return Nothing(path, false, false, at + "names no team", settings.UnknownTeam);
                        }

                        sizeFolderLines.Add(new[] { team, at });

                        if (!sizeFolderTeams.Contains(team))
                        {
                            sizeFolderTeams.Add(team);
                        }
                    }

                    continue;
                }

                return Nothing(path, false, false, at + "is not a line of a team map this tool knows", settings.UnknownTeam);
            }

            // Checked once every team line is read, so a size-folder line may come first.
            foreach (string[] named in sizeFolderLines)
            {
                if (!teams.Contains(named[0]))
                {
                    return Nothing(path, false, false, named[1] + "names " + named[0] + ", which no team line names", settings.UnknownTeam);
                }
            }

            return new TeamMap(path, false, false, null, settings.UnknownTeam, teams, codes, sizeFolderTeams);
        }

        /// <summary>What is wrong with a team line, or null where nothing is.</summary>
        private static string TeamLineFault(string[] parts, IList<string> teams, IDictionary<string, string> owner)
        {
            string team = parts[0];

            if (team.Length == 0)
            {
                return "names no team";
            }

            if (team.Trim().Length != team.Length)
            {
                return "names the team \"" + team + "\" with a space at its start or end";
            }

            if (teams.Contains(team))
            {
                return "names the team " + team + " a second time";
            }

            if (parts.Length < 2)
            {
                return "names a team with no code";
            }

            List<string> seen = new List<string>();

            for (int i = 1; i < parts.Length; i++)
            {
                string code = parts[i];
                string already;

                if (code.Length == 0)
                {
                    return "holds an empty code";
                }

                if (code.IndexOf(' ') >= 0)
                {
                    return "holds the code \"" + code + "\", which has a space in it, and a code read off a file name has none";
                }

                if (seen.Contains(code))
                {
                    return "names the code " + code + " twice";
                }

                if (owner.TryGetValue(code, out already))
                {
                    return "gives the code " + code + " a second team, it is on the line of " + already + " already";
                }

                seen.Add(code);
            }

            return null;
        }

        private static TeamMap Nothing(string path, bool missing, bool noXml, string unread, string unknownTeam)
        {
            return new TeamMap(path, missing, noXml, unread, unknownTeam, new List<string>(), new List<List<string>>(), new List<string>());
        }

        /// <summary>
        /// The team of a code read off a set name or a file name: the map's team, or the code
        /// itself where no line names it, or the UnknownTeam setting where there is no code.
        /// </summary>
        public string TeamOf(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return UnknownTeam;
            }

            string team;

            return teamOf.TryGetValue(code, out team) ? team : code;
        }

        /// <summary>
        /// Point 12, the one order of two teams: the map's lines first, in their order, then any
        /// other team by its name, Ordinal, then the UnknownTeam setting last. So a pair is
        /// always written the same way round and one pair never becomes two folders.
        /// </summary>
        public int Compare(string x, string y)
        {
            string first = string.IsNullOrEmpty(x) ? UnknownTeam : x;
            string second = string.IsNullOrEmpty(y) ? UnknownTeam : y;

            if (string.Equals(first, second, StringComparison.Ordinal))
            {
                return 0;
            }

            int firstLine = LineOf(first);
            int secondLine = LineOf(second);

            if (firstLine >= 0 || secondLine >= 0)
            {
                if (firstLine < 0)
                {
                    return 1;
                }

                return secondLine < 0 ? -1 : firstLine.CompareTo(secondLine);
            }

            if (string.Equals(first, UnknownTeam, StringComparison.Ordinal))
            {
                return 1;
            }

            if (string.Equals(second, UnknownTeam, StringComparison.Ordinal))
            {
                return -1;
            }

            return Math.Sign(string.CompareOrdinal(first, second));
        }

        private int LineOf(string team)
        {
            for (int i = 0; i < Teams.Count; i++)
            {
                if (string.Equals(Teams[i], team, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Point 11. Whether a pair holding that team carries the size folder, only where the map's size-folder line names it.</summary>
        public bool CarriesSizeFolder(string team)
        {
            return !string.IsNullOrEmpty(team) && sizeFolder.Contains(team);
        }

        /// <summary>
        /// The codes a set name is read against: the map's, in the order of its lines, then the
        /// group's own models' codes, each once. So a code that is a team of its own is still
        /// found in a set name when a model of the group carries it.
        /// </summary>
        public IList<string> KnownCodes(IEnumerable<string> groupCodes)
        {
            List<string> known = new List<string>(Codes);

            if (groupCodes != null)
            {
                foreach (string code in groupCodes)
                {
                    if (!string.IsNullOrEmpty(code) && !known.Contains(code))
                    {
                        known.Add(code);
                    }
                }
            }

            return known;
        }

        /// <summary>
        /// The code with its team beside it, for the log, COVERAGE and the form, Q116 by its
        /// default A: HV in Mechanical, or LS in a team of its own. The code alone where the map
        /// maps nothing, the TEAMS line saying every code is a team of its own.
        /// </summary>
        public string CodeWithTeam(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return UnknownTeam;
            }

            if (Teams.Count == 0)
            {
                return code;
            }

            string team;

            return teamOf.TryGetValue(code, out team) ? code + " in " + team : code + " in a team of its own";
        }

        /// <summary>
        /// The TEAMS lines, said before the MATRIX lines: where the map was read from and what it
        /// holds, or that none is there, could not be read, holds no team or that no XML was
        /// picked, with the full path looked for. Never silent.
        /// </summary>
        public IList<string> Lines()
        {
            const string Prefix = "TEAMS    ";
            const string NothingApplies = ". Every discipline code is a team of its own and no pair carries the size folder";
            List<string> lines = new List<string>();

            if (NoXmlPicked)
            {
                lines.Add(Prefix + "no clash XML was picked, so no team map is beside one" + NothingApplies);
                return lines;
            }

            if (Missing)
            {
                lines.Add(Prefix + "no team map is beside this file: " + ListPath + " was looked for and is not there" + NothingApplies);
                return lines;
            }

            if (Unread != null)
            {
                lines.Add(Prefix + "THE TEAM MAP BESIDE THIS FILE, " + ListPath + ", COULD NOT BE READ: " + Unread + NothingApplies);
                return lines;
            }

            if (HoldsNone)
            {
                lines.Add(Prefix + "the team map beside this file, " + ListPath + ", holds no team" + NothingApplies);
                return lines;
            }

            lines.Add(Prefix + "the teams are read from " + ListPath + ", the team map beside this file. It holds "
                + Counted(Teams.Count, "team", "teams") + " and " + Counted(Codes.Count, "code", "codes")
                + ", and a code on no line is a team of its own named by its code");

            for (int i = 0; i < Teams.Count; i++)
            {
                lines.Add(Prefix + Teams[i] + " is " + Listed(codesOf[i], " and "));
            }

            lines.Add(Prefix + "a pair is written in the order " + string.Join(", ", new List<string>(Teams).ToArray())
                + ", then any other team by its name, then " + UnknownTeam);

            lines.Add(Prefix + (sizeFolder.Count == 0
                ? "no team carries the size folder"
                : "a pair holding " + Listed(sizeFolder, " or ") + " carries the size folder"));

            return lines;
        }

        /// <summary>The window's one grey line at the pick, plain words and twelve at most.</summary>
        public string WindowLine()
        {
            if (NoXmlPicked)
            {
                return "Teams: no XML picked, each code is its own team";
            }

            if (Missing)
            {
                return "Teams: no map beside the XML, each code is its own team";
            }

            if (Unread != null)
            {
                return "Teams: the map beside the XML could not be read";
            }

            if (HoldsNone)
            {
                return "Teams: the map beside the XML holds no team";
            }

            return "Teams: " + Teams.Count.ToString(CultureInfo.InvariantCulture) + " read from the map beside the XML";
        }

        private static string Counted(int count, string one, string many)
        {
            return count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);
        }

        /// <summary>Parts read as a list: one alone, two joined by the last word, more with commas before it.</summary>
        private static string Listed(IList<string> parts, string last)
        {
            if (parts.Count == 1)
            {
                return parts[0];
            }

            string[] head = new string[parts.Count - 1];

            for (int i = 0; i < head.Length; i++)
            {
                head[i] = parts[i];
            }

            return string.Join(", ", head) + last + parts[parts.Count - 1];
        }
    }
}
