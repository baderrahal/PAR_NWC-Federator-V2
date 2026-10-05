using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Federator.Core.Exchange;
using Federator.Core.Health;

namespace Federator.Core.Teams
{
    /// <summary>
    /// Which discipline codes make one team, read off the map kept BESIDE the picked clash XML,
    /// Q114 points 1, 2, 11 and 12 decided by Bader on 2026-10-04 and Q115 answered A on
    /// 2026-10-05. The tool serves many projects, so no project's teams are inside it and nothing here names a
    /// team or a code. This project's map is in the exchange folder beside the corrected XML.
    ///
    /// THE MAP. One team a line, `team: name | code | code`, in the order a pair of teams is
    /// written, point 12, and `size-folder: team | team`, the teams whose pairs carry the size
    /// folder, point 11. Read by ListFile, the one way a list beside the XML is read, so a
    /// comment starts with #, a blank line is skipped and nothing is trimmed.
    ///
    /// ANY OTHER CODE IS A TEAM OF ITS OWN, named by its code, Bader's rule. A set name that
    /// carries no code takes the team a folder above it in the clash XML's set tree names, and
    /// where none does it reads as the UnknownTeam setting, UNKNOWN, never a guess, Q117 answered
    /// C, and A where the XML's set tree names no team, by Bader on 2026-10-05.
    ///
    /// A MAP THAT CANNOT BE READ IS SAID, NEVER HALF READ AND NEVER A THROW. A line this does not
    /// know, a code on two teams or twice on one, a team named twice, a team with no code, an
    /// empty team name or code, a code holding any space or a character a person cannot see, a
    /// team name with a space at its start or end or such a character anywhere, on a team line
    /// or a size-folder line, InvisibleDifference naming it, a size-folder line naming a team no
    /// team line names, or bytes that are not UTF-8, make the whole map Unread with its line and
    /// why. A map unread, missing or holding no team maps nothing: every code is a team of its
    /// own and no team carries the size folder, and the TEAMS lines say which of these it was.
    ///
    /// A RUN WITH NO CLASH XML reads the map the window kept from the last run with one, Q123
    /// answered B by Bader on 2026-10-05, TeamMapMemory, at its one full path, and its TEAMS lines
    /// say it is the kept map. With none kept it maps nothing and says so.
    ///
    /// CODES COMPARE ORDINAL, as they are read off a file name, so hv is not HV.
    ///
    /// WHERE IT APPLIES, Q116 answered A by Bader on 2026-10-05: the views, and the team written
    /// beside the code in the log, COVERAGE and the form. The grouping, the one-discipline judgement, the alignment
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

        /// <summary>Whether what could not be read is the memory of the kept map and not a map, TeamMapMemory, K26.</summary>
        private bool memoryUnread;

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

        /// <summary>Whether no clash XML was picked, so the map is the one kept from the last run with one, or none, Q123.</summary>
        public bool NoXmlPicked { get; private set; }

        /// <summary>Why the map could not be read, or null where it was read whole or is not there.</summary>
        public string Unread { get; private set; }

        /// <summary>The team of a name carrying no code, the UnknownTeam setting.</summary>
        public string UnknownTeam { get; private set; }

        /// <summary>The teams of the map, in the order of their lines, which is the order a pair is written in.</summary>
        public ReadOnlyCollection<string> Teams { get; private set; }

        /// <summary>Every code the map gives a team, in the order of the lines.</summary>
        public ReadOnlyCollection<string> Codes { get; private set; }

        /// <summary>Whether the map is there and was read whole, beside the picked XML or kept for a run with none.</summary>
        public bool IsRead
        {
            get { return ListPath != null && !Missing && Unread == null; }
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

            return At(settings.PathBeside(xmlPath), settings);
        }

        /// <summary>
        /// The map of a run with no clash XML picked, the one the window kept from the last run
        /// with one, at that full path, Q123 answered B. Read as a map beside an XML is, and said
        /// as the kept map. Never a throw.
        /// </summary>
        public static TeamMap Kept(string path, TeamMapSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            TeamMap map = At(path, settings);
            map.NoXmlPicked = true;
            return map;
        }

        /// <summary>
        /// The map of a run with no clash XML picked whose memory of the kept map could not be
        /// read, with why, at the memory's path, TeamMapMemory.ForNoXml.
        /// </summary>
        internal static TeamMap MemoryUnread(string memoryPath, string unread, TeamMapSettings settings)
        {
            TeamMap map = Nothing(memoryPath, false, true, unread, settings.UnknownTeam);
            map.memoryUnread = true;
            return map;
        }

        /// <summary>The map at that one full path, tested with File.Exists, never a search.</summary>
        private static TeamMap At(string path, TeamMapSettings settings)
        {
            if (!File.Exists(path))
            {
                return Nothing(path, true, false, null, settings.UnknownTeam);
            }

            return ListFile.Read(
                path,
                reader => Read(reader, path, settings),
                why => Nothing(path, false, false, why, settings.UnknownTeam));
        }

        /// <summary>The map of a run with no clash XML picked and no map kept from a run with one.</summary>
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
                        string fault = team.Length == 0 ? "names no team" : TeamNameFault(team);

                        if (fault != null)
                        {
                            return Nothing(path, false, false, at + fault, settings.UnknownTeam);
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

            string nameFault = TeamNameFault(team);

            if (nameFault != null)
            {
                return nameFault;
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

                int unseen = Unseen(code, true);

                if (unseen >= 0)
                {
                    return "holds the code \"" + code + "\", which has "
                        + (code[unseen] == ' ' ? "a space" : InvisibleDifference.Describe(code[unseen]))
                        + " in it, and a code read off a file name has none";
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

        /// <summary>
        /// What is wrong with a team's name on a team line or a size-folder line, or null where
        /// nothing is: a space at its start or end, or a character a person cannot see as what
        /// it is anywhere in it, so two names that look the same are never two teams. An
        /// ordinary space inside a name is part of it.
        /// </summary>
        private static string TeamNameFault(string team)
        {
            if (team.Trim().Length != team.Length)
            {
                return "names the team \"" + team + "\" with a space at its start or end";
            }

            int unseen = Unseen(team, false);

            return unseen < 0
                ? null
                : "names the team \"" + team + "\", which has " + InvisibleDifference.Describe(team[unseen]) + " in it";
        }

        /// <summary>
        /// Where the first character of that name stands that a person cannot see as what it is,
        /// any space but the ordinary one or a character InvisibleDifference names, and the
        /// ordinary space too where asked, or -1. A code read off a file name holds none of
        /// them, and one that did would never equal it and would make a team of its own.
        /// </summary>
        private static int Unseen(string name, bool ordinarySpaceToo)
        {
            for (int i = 0; i < name.Length; i++)
            {
                char one = name[i];

                if (one == ' ' ? ordinarySpaceToo : (char.IsWhiteSpace(one) || InvisibleDifference.IsInvisible(one)))
                {
                    return i;
                }
            }

            return -1;
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
        /// The team of a side of a clash test, Q117 answered C, and A where the XML's set tree
        /// names no team: the team of the code its set name carries, CodeOf.Set, and where it
        /// carries none, the team a folder above the set names, the folder nearest the set first,
        /// a folder naming a team when its whole name is a team of this map, Ordinal. Where no
        /// folder does, the UnknownTeam setting. With no map no folder names a team.
        /// </summary>
        public string TeamOfSet(string code, IList<string> folders)
        {
            if (!string.IsNullOrEmpty(code))
            {
                return TeamOf(code);
            }

            return FolderNamingATeam(folders) ?? UnknownTeam;
        }

        /// <summary>The folder nearest the set whose whole name is a team of this map, or null where none is.</summary>
        private string FolderNamingATeam(IList<string> folders)
        {
            if (folders == null)
            {
                return null;
            }

            for (int i = folders.Count - 1; i >= 0; i--)
            {
                if (LineOf(folders[i]) >= 0)
                {
                    return folders[i];
                }
            }

            return null;
        }

        /// <summary>
        /// The TEAMS lines naming each set of the picked XML whose name carries no code of this
        /// map, with the team a folder above it names or UNKNOWN, Q117 answered C and A, said
        /// after the map's own lines. A model of a group carrying a code the set's name holds
        /// gives it that code's team in that group, CodeOf.Set, which the lines say. None where
        /// this map maps no team, its first TEAMS line saying every code is a team of its own.
        /// </summary>
        public IList<string> SetLines(IEnumerable<SelectionSetDefinition> sets, char separator)
        {
            const string Unless = ", unless a model of its group carries a code its name holds";
            List<string> lines = new List<string>();

            if (sets == null || Teams.Count == 0)
            {
                return lines;
            }

            foreach (SelectionSetDefinition set in sets)
            {
                if (CodeOf.Set(set.Name, Codes, separator).Length > 0)
                {
                    continue;
                }

                string folder = FolderNamingATeam(set.Folders);

                lines.Add(folder == null
                    ? "TEAMS    " + set.Name + " carries no discipline code the map lists and no folder above it in the clash XML's set"
                        + " tree names a team, so its team is " + UnknownTeam + Unless
                    : "TEAMS    " + set.Name + " carries no discipline code the map lists, so its team is " + folder
                        + ", which a folder above it in the clash XML's set tree names" + Unless);
            }

            return lines;
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
        /// The code with its team beside it, for the log, COVERAGE and the form, Q116 answered
        /// A: HV in Mechanical, or LS in a team of its own. The code alone where the map
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

            const string Kept = "the team map kept from the last run with one";

            if (NoXmlPicked && ListPath == null)
            {
                lines.Add(Prefix + "no clash XML was picked and no team map is kept from a run with one" + NothingApplies);
                return lines;
            }

            if (Missing)
            {
                lines.Add(Prefix + (NoXmlPicked
                    ? "no clash XML was picked, and " + Kept + ", " + ListPath + ", is not there"
                    : "no team map is beside this file: " + ListPath + " was looked for and is not there") + NothingApplies);
                return lines;
            }

            if (Unread != null)
            {
                lines.Add(Prefix + (NoXmlPicked
                    ? "no clash XML was picked, and " + (memoryUnread ? "the memory of " + Kept : Kept).ToUpperInvariant() + ", " + ListPath + ", COULD NOT BE READ: "
                    : "THE TEAM MAP BESIDE THIS FILE, " + ListPath + ", COULD NOT BE READ: ") + Unread + NothingApplies);
                return lines;
            }

            if (HoldsNone)
            {
                lines.Add(Prefix + (NoXmlPicked
                    ? "no clash XML was picked, and " + Kept + ", " + ListPath + ", holds no team"
                    : "the team map beside this file, " + ListPath + ", holds no team") + NothingApplies);
                return lines;
            }

            lines.Add(Prefix + (NoXmlPicked ? "no clash XML was picked, so the teams are read from " : "the teams are read from ")
                + ListPath + ", " + (NoXmlPicked ? Kept : "the team map beside this file") + ". It holds "
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
                return ListPath == null ? "Teams: no XML picked, no map kept yet"
                    : Missing ? "Teams: no XML picked, the kept map is gone"
                    : Unread != null ? (memoryUnread ? "Teams: no XML picked, the kept map's memory could not be read" : "Teams: no XML picked, the kept map could not be read")
                    : HoldsNone ? "Teams: no XML picked, the kept map holds no team"
                    : "Teams: no XML picked, " + Teams.Count.ToString(CultureInfo.InvariantCulture) + " read from the kept map";
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
