using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Exchange;
using Federator.Core.Teams;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The team map of a picked clash XML, F131, Q114 points 1, 2, 11 and 12 and Q115 answered
    /// A: a plain file beside the XML named after it with .teams.txt, read the way the
    /// list of corrections is, one team a line in the order a pair is written, and the teams
    /// whose pairs carry the size folder.
    ///
    /// The teams and codes below are Bader's map of 2026-10-04, SAMPLE DATA for the rule, which
    /// is CLAUDE.md's rule. Nothing in src names a team or a code.
    /// </summary>
    [TestFixture]
    public class TeamMapTests
    {
        /// <summary>Bader's map of Q114 point 1, with his order of point 12 and the size folder of point 11.</summary>
        internal const string BadersMap =
            "# a comment\n"
            + "team: Architecture | AR\n"
            + "team: Structure | ST\n"
            + "team: Mechanical | HV | PL | FP | ME | DR | FF\n"
            + "team: Electrical | EL | EV\n"
            + "\n"
            + "size-folder: Mechanical | Electrical\n";

        private const string InATest = "a map in a test";

        private const string NothingApplies =
            ". Every discipline code is a team of its own and no pair carries the size folder";

        internal static TeamMap MapOf(string text)
        {
            return TeamMap.Read(new StringReader(text), InATest, new TeamMapSettings());
        }

        /// <summary>A list compared the way tests.md asks, its count first and then each one.</summary>
        internal static void Same(IList<string> actual, params string[] expected)
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Length), string.Join(" / ", new List<string>(actual).ToArray()));

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(actual[i], Is.EqualTo(expected[i]), "at " + i);
            }
        }

        [Test]
        public void TheMapIsFoundBesideTheXmlByItsNameAndTheSuffixSetting()
        {
            Assert.That(TeamMapSettings.DefaultSuffix, Is.EqualTo(".teams.txt"));
            Assert.That(() => new TeamMapSettings().PathBeside(string.Empty), Throws.ArgumentException);

            string folder = TempFolder.Make("f131-suffix");

            try
            {
                string xml = Path.Combine(folder, "a.xml");
                File.WriteAllText(xml, "<exchange/>");
                File.WriteAllText(Path.Combine(folder, "a.crews.txt"), "team: Crew | AR\n");
                File.WriteAllText(Path.Combine(folder, "a.teams.txt"), "not a team line\n");

                Assert.That(new TeamMapSettings().PathBeside(xml), Is.EqualTo(Path.Combine(folder, "a.teams.txt")));

                TeamMap map = TeamMap.Beside(xml, new TeamMapSettings { Suffix = ".crews.txt" });

                Assert.That(map.ListPath, Is.EqualTo(Path.Combine(folder, "a.crews.txt")));
                Assert.That(map.IsRead, Is.True);
                Assert.That(map.TeamOf("AR"), Is.EqualTo("Crew"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        [Test]
        public void BadersMapReadsEveryCodeToItsTeamInHisOrder()
        {
            TeamMap map = MapOf(BadersMap);

            Assert.That(map.Unread, Is.Null);
            Assert.That(map.IsRead, Is.True);
            Assert.That(map.HoldsNone, Is.False);
            Same(map.Teams, "Architecture", "Structure", "Mechanical", "Electrical");
            Same(map.Codes, "AR", "ST", "HV", "PL", "FP", "ME", "DR", "FF", "EL", "EV");

            foreach (string code in new[] { "HV", "PL", "FP", "ME", "DR", "FF" })
            {
                Assert.That(map.TeamOf(code), Is.EqualTo("Mechanical"), code);
            }

            Assert.That(map.TeamOf("EL"), Is.EqualTo("Electrical"));
            Assert.That(map.TeamOf("EV"), Is.EqualTo("Electrical"));
            Assert.That(map.TeamOf("AR"), Is.EqualTo("Architecture"));
            Assert.That(map.TeamOf("ST"), Is.EqualTo("Structure"));
        }

        /// <summary>
        /// Bader's rule: any other code is a team of its own, named by its code. A name carrying
        /// no code, TeamOf of an empty code, is UNKNOWN, and that word is a setting. Codes compare
        /// Ordinal, as they are read off a file name.
        /// </summary>
        [Test]
        public void ACodeOnNoLineIsATeamOfItsOwnAndNoCodeIsUnknown()
        {
            TeamMap map = MapOf(BadersMap);

            Assert.That(map.TeamOf("LS"), Is.EqualTo("LS"));
            Assert.That(map.TeamOf(string.Empty), Is.EqualTo("UNKNOWN"));
            Assert.That(map.TeamOf(null), Is.EqualTo("UNKNOWN"));
            Assert.That(map.TeamOf("hv"), Is.EqualTo("hv"), "hv is not HV");
            Assert.That(map.TeamOf("HV "), Is.EqualTo("HV "), "nothing is trimmed");

            TeamMap otherWord = TeamMap.Read(new StringReader(BadersMap), InATest, new TeamMapSettings { UnknownTeam = "NO CODE" });

            Assert.That(otherWord.TeamOf(string.Empty), Is.EqualTo("NO CODE"));
        }

        /// <summary>
        /// Every fault makes the whole map unread with its line and why, never half read and never
        /// a throw, and an unread map maps nothing: every code is a team of its own and no team
        /// carries the size folder.
        /// </summary>
        [Test]
        public void EachFaultUnreadsTheWholeMapAndNamesItsLine()
        {
            string[][] faults =
            {
                new[] { "team: A | AR\nteams: B | ST\n", "line 2, \"teams: B | ST\", is not a line of a team map this tool knows" },
                new[] { "team: A | AR\nteam: B | ST | AR\n", "line 2, \"team: B | ST | AR\", gives the code AR a second team, it is on the line of A already" },
                new[] { "team: A | AR\nteam: B | ST | ST\n", "line 2, \"team: B | ST | ST\", names the code ST twice" },
                new[] { "team: A | AR\nteam: A | ST\n", "line 2, \"team: A | ST\", names the team A a second time" },
                new[] { "team: Mechanical\n", "line 1, \"team: Mechanical\", names a team with no code" },
                new[] { "team:  | AR\n", "line 1, \"team:  | AR\", names no team" },
                new[] { "team: A | AR |  | ST\n", "line 1, \"team: A | AR |  | ST\", holds an empty code" },
                new[] { "team: A | AR ST\n", "line 1, \"team: A | AR ST\", holds the code \"AR ST\", which has a space in it, and a code read off a file name has none" },
                new[] { "team: A | AR \n", "line 1, \"team: A | AR \", holds the code \"AR \", which has a space in it, and a code read off a file name has none" },
                new[] { "team: Mechanical  | ME\n", "line 1, \"team: Mechanical  | ME\", names the team \"Mechanical \" with a space at its start or end" },
                new[] { "team: A | AR\nsize-folder: B\n", "line 2, \"size-folder: B\", names B, which no team line names" },
                new[] { "team: A | AR\nsize-folder: \n", "line 2, \"size-folder: \", names no team" }
            };

            foreach (string[] fault in faults)
            {
                TeamMap map = null;

                Assert.That(() => map = MapOf(fault[0]), Throws.Nothing, fault[0]);
                Assert.That(map.Unread, Is.EqualTo(fault[1]), fault[0]);
                Assert.That(map.IsRead, Is.False, fault[0]);
                Assert.That(map.Teams.Count, Is.EqualTo(0), fault[0]);
                Assert.That(map.TeamOf("AR"), Is.EqualTo("AR"), fault[0]);
                Assert.That(map.SizeFolderTeams.Count, Is.EqualTo(0), fault[0]);
                Assert.That(map.Lines().Count, Is.EqualTo(1), fault[0]);
                Assert.That(map.Lines()[0], Does.EndWith(NothingApplies), fault[0]);
            }
        }

        /// <summary>
        /// The breaker's finding on attempt 1. A code holding a tab, a no-break space, a thin
        /// space, a zero width space or a byte order mark never equals the code read off a file
        /// name, so it made a team of its own while the map read as whole and the TEAMS line
        /// looked normal. Each is refused with its line, the character named the way
        /// InvisibleDifference names it. A team name holding one of the characters a person cannot
        /// see is refused too, on a team line and on a size-folder line, because two names that
        /// look the same would be two teams.
        /// </summary>
        [Test]
        public void ACharacterNobodyCanSeeInACodeOrATeamUnreadsTheMapAndIsNamed()
        {
            const string HasNone = ", and a code read off a file name has none";

            string[][] faults =
            {
                new[] { "team: Electrical | EL | EV\t\n", "line 1, \"team: Electrical | EL | EV\t\", holds the code \"EV\t\", which has TAB (U+0009) in it" + HasNone },
                new[] { "team: Electrical | EL | E\u00A0V\n", "line 1, \"team: Electrical | EL | E\u00A0V\", holds the code \"E\u00A0V\", which has NON-BREAKING SPACE (U+00A0) in it" + HasNone },
                new[] { "team: Electrical | EL | EV\u2009\n", "line 1, \"team: Electrical | EL | EV\u2009\", holds the code \"EV\u2009\", which has \"\u2009\" (U+2009) in it" + HasNone },
                new[] { "team: Electrical | EL | EV\u200B\n", "line 1, \"team: Electrical | EL | EV\u200B\", holds the code \"EV\u200B\", which has ZERO WIDTH SPACE (U+200B) in it" + HasNone },
                new[] { "team: Electrical | \uFEFFEL\n", "line 1, \"team: Electrical | \uFEFFEL\", holds the code \"\uFEFFEL\", which has ZERO WIDTH NO-BREAK SPACE (U+FEFF) in it" + HasNone },
                new[] { "team: Elec\u00A0trical | EL\n", "line 1, \"team: Elec\u00A0trical | EL\", names the team \"Elec\u00A0trical\", which has NON-BREAKING SPACE (U+00A0) in it" },
                new[] { "team: Electrical\u200B | EL\n", "line 1, \"team: Electrical\u200B | EL\", names the team \"Electrical\u200B\", which has ZERO WIDTH SPACE (U+200B) in it" },
                new[] { "team: Electrical | EL\nsize-folder: Elec\u00A0trical\n", "line 2, \"size-folder: Elec\u00A0trical\", names the team \"Elec\u00A0trical\", which has NON-BREAKING SPACE (U+00A0) in it" },
                new[] { "team: Electrical | EL\nsize-folder: Electrical\t\n", "line 2, \"size-folder: Electrical\t\", names the team \"Electrical\t\" with a space at its start or end" }
            };

            foreach (string[] fault in faults)
            {
                TeamMap map = MapOf(fault[0]);

                Assert.That(map.Unread, Is.EqualTo(fault[1]), fault[0]);
                Assert.That(map.Teams.Count, Is.EqualTo(0), fault[0]);
                Assert.That(map.TeamOf("EV"), Is.EqualTo("EV"), fault[0]);
            }

            TeamMap spaced = MapOf("team: Fire Protection | FP\nsize-folder: Fire Protection\n");

            Assert.That(spaced.Unread, Is.Null, "an ordinary space inside a team name is a team name");
            Assert.That(spaced.TeamOf("FP"), Is.EqualTo("Fire Protection"));
        }

        [Test]
        public void ASizeFolderLineBeforeItsTeamLineIsRead()
        {
            TeamMap map = MapOf("size-folder: A\nteam: A | AR\n");

            Assert.That(map.Unread, Is.Null);
            Same(map.SizeFolderTeams, "A");
            Assert.That(map.Lines()[map.Lines().Count - 1], Is.EqualTo("TEAMS    a pair holding A carries the size folder"));
        }

        [Test]
        public void AMapThatIsNotUtf8IsUnreadAndSaysSo()
        {
            string folder = TempFolder.Make("f131-not-utf8");

            try
            {
                string xml = Path.Combine(folder, "a.xml");
                File.WriteAllText(xml, "<exchange/>");
                File.WriteAllBytes(
                    Path.Combine(folder, "a.teams.txt"),
                    Encoding.GetEncoding(1252).GetBytes("team: " + (char)0xC9 + "lectricit" + (char)0xE9 + " | EL\n"));

                TeamMap map = TeamMap.Beside(xml, new TeamMapSettings());

                Assert.That(map.Missing, Is.False);
                Assert.That(map.Unread, Is.EqualTo("it is not UTF-8 text"));
                Assert.That(map.TeamOf("EL"), Is.EqualTo("EL"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// No map beside the XML is said with the one full path looked for, and a near name is
        /// never read, because the map is one path tested with File.Exists and never a search.
        /// </summary>
        [Test]
        public void AMissingMapNamesThePathLookedForAndANearNameIsNotRead()
        {
            string folder = TempFolder.Make("f131-missing");

            try
            {
                string xml = Path.Combine(folder, "a.xml");
                File.WriteAllText(xml, "<exchange/>");

                foreach (string near in new[] { "a.team.txt", "a.xml.teams.txt", "another.teams.txt", "a.teams.txt.bak", "a-teams.txt" })
                {
                    File.WriteAllText(Path.Combine(folder, near), BadersMap);
                }

                string path = Path.Combine(folder, "a.teams.txt");
                TeamMap map = TeamMap.Beside(xml, new TeamMapSettings());

                Assert.That(map.Missing, Is.True);
                Assert.That(map.IsRead, Is.False);
                Assert.That(map.ListPath, Is.EqualTo(path));
                Assert.That(map.TeamOf("HV"), Is.EqualTo("HV"));
                Assert.That(map.SizeFolderTeams.Count, Is.EqualTo(0));
                Same(map.Lines(), "TEAMS    no team map is beside this file: " + path + " was looked for and is not there" + NothingApplies);
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        [Test]
        public void TheTeamsLinesSayWhatWasReadOrWhyNothingWas()
        {
            Same(
                MapOf(BadersMap).Lines(),
                "TEAMS    the teams are read from a map in a test, the team map beside this file. It holds 4 teams and 10 codes,"
                    + " and a code on no line is a team of its own named by its code",
                "TEAMS    Architecture is AR",
                "TEAMS    Structure is ST",
                "TEAMS    Mechanical is HV, PL, FP, ME, DR and FF",
                "TEAMS    Electrical is EL and EV",
                "TEAMS    a pair is written in the order Architecture, Structure, Mechanical, Electrical, then any other team by its name, then UNKNOWN",
                "TEAMS    a pair holding Mechanical or Electrical carries the size folder");

            Same(
                MapOf("team: Mechanical | ME\n").Lines(),
                "TEAMS    the teams are read from a map in a test, the team map beside this file. It holds 1 team and 1 code,"
                    + " and a code on no line is a team of its own named by its code",
                "TEAMS    Mechanical is ME",
                "TEAMS    a pair is written in the order Mechanical, then any other team by its name, then UNKNOWN",
                "TEAMS    no team carries the size folder");

            Same(
                MapOf("team: A | AR\nteams: B\n").Lines(),
                "TEAMS    THE TEAM MAP BESIDE THIS FILE, a map in a test, COULD NOT BE READ: line 2, \"teams: B\","
                    + " is not a line of a team map this tool knows" + NothingApplies);

            TeamMap none = MapOf("# a comment alone\n\n");

            Assert.That(none.HoldsNone, Is.True);
            Same(none.Lines(), "TEAMS    the team map beside this file, a map in a test, holds no team" + NothingApplies);

            TeamMap noXml = TeamMap.NoXml(new TeamMapSettings());

            Assert.That(noXml.NoXmlPicked, Is.True);
            Assert.That(noXml.ListPath, Is.Null);
            Same(noXml.Lines(), "TEAMS    no clash XML was picked and no team map is kept from a run with one" + NothingApplies);
        }

        /// <summary>The window's grey line at the pick, in plain words held in Core and twelve words at most.</summary>
        [Test]
        public void TheWindowLineIsPlainWordsOfTwelveAtMost()
        {
            string[][] lines =
            {
                new[] { MapOf(BadersMap).WindowLine(), "Teams: 4 read from the map beside the XML" },
                new[] { TeamMap.Read(new StringReader("team: A | AR\nteams: B\n"), InATest, new TeamMapSettings()).WindowLine(), "Teams: the map beside the XML could not be read" },
                new[] { MapOf("# nothing\n").WindowLine(), "Teams: the map beside the XML holds no team" },
                new[] { TeamMap.NoXml(new TeamMapSettings()).WindowLine(), "Teams: no XML picked, no map kept yet" }
            };

            foreach (string[] line in lines)
            {
                Assert.That(line[0], Is.EqualTo(line[1]));
                Assert.That(line[0].Split(' ').Length, Is.LessThanOrEqualTo(12), line[0]);
            }

            string folder = TempFolder.Make("f131-window");

            try
            {
                string xml = Path.Combine(folder, "a.xml");
                File.WriteAllText(xml, "<exchange/>");

                Assert.That(TeamMap.Beside(xml, new TeamMapSettings()).WindowLine(), Is.EqualTo("Teams: no map beside the XML, each code is its own team"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// Q116 answered A: the team is written beside the code wherever the log, COVERAGE
        /// and the form name a model, and the code stays what it is. With no map the code is
        /// written alone, the TEAMS line saying every code is a team of its own.
        /// </summary>
        [Test]
        public void ACodeIsWrittenWithItsTeamBesideIt()
        {
            TeamMap map = MapOf(BadersMap);

            Assert.That(map.CodeWithTeam("HV"), Is.EqualTo("HV in Mechanical"));
            Assert.That(map.CodeWithTeam("AR"), Is.EqualTo("AR in Architecture"));
            Assert.That(map.CodeWithTeam("LS"), Is.EqualTo("LS in a team of its own"));
            Assert.That(map.CodeWithTeam(string.Empty), Is.EqualTo("UNKNOWN"));
            Assert.That(TeamMap.NoXml(new TeamMapSettings()).CodeWithTeam("HV"), Is.EqualTo("HV"));
        }

        /// <summary>
        /// The codes a set name is read against are the map's and those of the group's own
        /// models, each once, so a code that is a team of its own is still found in a set name.
        /// </summary>
        [Test]
        public void TheKnownCodesAreTheMapsThenTheGroupsOwn()
        {
            TeamMap map = MapOf(BadersMap);

            Same(
                map.KnownCodes(new[] { "AR", "LS", string.Empty, null, "LS", "CV" }),
                "AR", "ST", "HV", "PL", "FP", "ME", "DR", "FF", "EL", "EV", "LS", "CV");
            Same(map.KnownCodes(null), "AR", "ST", "HV", "PL", "FP", "ME", "DR", "FF", "EL", "EV");
            Same(TeamMap.NoXml(new TeamMapSettings()).KnownCodes(new[] { "AR", "EL" }), "AR", "EL");
        }

        // ---------- the add-in pass, Bader's answers of 2026-10-05 to Q117 and Q123 ----------

        /// <summary>
        /// Q117 answered C, and A where the XML's set tree names no team: a set whose name carries
        /// no code takes the team its folder names, the folder nearest the set first, a folder
        /// naming a team when its whole name is a team of the map, and UNKNOWN where none does.
        /// A code in the name comes first. With no map no folder names a team.
        /// </summary>
        [Test]
        public void ASetWithNoCodeTakesTheTeamItsFolderNamesAndUnknownWhereNone()
        {
            TeamMap map = MapOf(BadersMap);

            Assert.That(map.TeamOfSet(string.Empty, new[] { "Electrical" }), Is.EqualTo("Electrical"));
            Assert.That(map.TeamOfSet(string.Empty, new[] { "Mechanical", "Mechanical-HVAC" }), Is.EqualTo("Mechanical"));
            Assert.That(map.TeamOfSet(string.Empty, new[] { "Electrical", "Mechanical" }), Is.EqualTo("Mechanical"));
            Assert.That(map.TeamOfSet(string.Empty, new[] { "Security" }), Is.EqualTo("UNKNOWN"));
            Assert.That(map.TeamOfSet(string.Empty, new[] { "electrical" }), Is.EqualTo("UNKNOWN"));
            Assert.That(map.TeamOfSet(string.Empty, new string[0]), Is.EqualTo("UNKNOWN"));
            Assert.That(map.TeamOfSet(string.Empty, null), Is.EqualTo("UNKNOWN"));
            Assert.That(map.TeamOfSet("ME", new[] { "Electrical" }), Is.EqualTo("Mechanical"));
            Assert.That(map.TeamOfSet("LS", new[] { "Electrical" }), Is.EqualTo("LS"));
            Assert.That(TeamMap.NoXml(new TeamMapSettings()).TeamOfSet(string.Empty, new[] { "Electrical" }), Is.EqualTo("UNKNOWN"));
        }

        /// <summary>
        /// Q117's set with no code is NAMED on a TEAMS line of the pick, with the team its folder
        /// names or UNKNOWN, and a set carrying a code of the map gets no line. With no map that
        /// maps a team no line is written, the first TEAMS line saying every code is its own team.
        /// </summary>
        [Test]
        public void EachSetWithNoCodeIsNamedWithItsTeamOrUnknown()
        {
            const string Element = "<category><name internal=\"LcRevitData_Element\">Element</name></category>";
            const string Ducts = "<findspec mode=\"all\" disjoint=\"0\"><conditions><condition test=\"equals\" flags=\"0\">" + Element
                + "<property><name internal=\"LcRevitPropertyElementCategory\">Category</name></property>"
                + "<value><data type=\"wstring\">Ducts</data></value></condition></conditions><locator>/</locator></findspec>";
            string xml = "<?xml version='1.0' encoding='UTF-8'?>\n<exchange units=\"ft\"><selectionsets>"
                + "<viewfolder name=\"Mechanical\"><viewfolder name=\"Mechanical-HVAC\">"
                + "<selectionset name=\"BLD-ME-Ducts\" guid=\"a\">" + Ducts + "</selectionset>"
                + "<selectionset name=\"BLD-Ducts\" guid=\"b\">" + Ducts + "</selectionset>"
                + "</viewfolder></viewfolder>"
                + "<viewfolder name=\"Security\"><selectionset name=\"BLD-Cameras\" guid=\"c\">" + Ducts + "</selectionset></viewfolder>"
                + "<selectionset name=\"Loose\" guid=\"d\">" + Ducts + "</selectionset>"
                + "</selectionsets></exchange>\n";

            IList<SelectionSetDefinition> sets = new ExchangeReader().ReadText(xml).Sets;

            Same(
                MapOf(BadersMap).SetLines(sets, '-'),
                "TEAMS    BLD-Ducts carries no discipline code the map lists, so its team is Mechanical, which a folder above it in the"
                    + " clash XML's set tree names, unless a model of its group carries a code its name holds",
                "TEAMS    BLD-Cameras carries no discipline code the map lists and no folder above it in the clash XML's set tree"
                    + " names a team, so its team is UNKNOWN, unless a model of its group carries a code its name holds",
                "TEAMS    Loose carries no discipline code the map lists and no folder above it in the clash XML's set tree"
                    + " names a team, so its team is UNKNOWN, unless a model of its group carries a code its name holds");

            Same(TeamMap.NoXml(new TeamMapSettings()).SetLines(sets, '-'));
            Same(MapOf("# nothing\n").SetLines(sets, '-'));
            Same(MapOf(BadersMap).SetLines(null, '-'));
        }

        /// <summary>
        /// Q123 answered B: a run with no clash XML reads the team map the window kept, by its
        /// one full path, and every TEAMS line and the window line say it is the kept map. Gone,
        /// unread or holding no team it maps nothing and says which, as a map beside an XML does.
        /// </summary>
        [Test]
        public void AKeptMapIsReadForARunWithNoXmlAndSaysSo()
        {
            string folder = TempFolder.Make("f131-kept");

            try
            {
                string path = Path.Combine(folder, "a.teams.txt");
                File.WriteAllText(path, BadersMap);

                TeamMap kept = TeamMap.Kept(path, new TeamMapSettings());

                Assert.That(kept.NoXmlPicked, Is.True);
                Assert.That(kept.IsRead, Is.True);
                Assert.That(kept.ListPath, Is.EqualTo(path));
                Assert.That(kept.TeamOf("HV"), Is.EqualTo("Mechanical"));
                Same(kept.SizeFolderTeams, "Mechanical", "Electrical");
                Same(
                    kept.Lines(),
                    "TEAMS    no clash XML was picked, so the teams are read from " + path + ", the team map kept from the last run with"
                        + " one. It holds 4 teams and 10 codes, and a code on no line is a team of its own named by its code",
                    "TEAMS    Architecture is AR",
                    "TEAMS    Structure is ST",
                    "TEAMS    Mechanical is HV, PL, FP, ME, DR and FF",
                    "TEAMS    Electrical is EL and EV",
                    "TEAMS    a pair is written in the order Architecture, Structure, Mechanical, Electrical, then any other team by its name, then UNKNOWN",
                    "TEAMS    a pair holding Mechanical or Electrical carries the size folder");
                Assert.That(kept.WindowLine(), Is.EqualTo("Teams: no XML picked, 4 read from the kept map"));

                string gone = Path.Combine(folder, "gone.teams.txt");
                TeamMap missing = TeamMap.Kept(gone, new TeamMapSettings());

                Assert.That(missing.Missing, Is.True);
                Assert.That(missing.IsRead, Is.False);
                Assert.That(missing.Teams.Count, Is.EqualTo(0));
                Same(missing.Lines(), "TEAMS    no clash XML was picked, and the team map kept from the last run with one, " + gone + ", is not there" + NothingApplies);
                Assert.That(missing.WindowLine(), Is.EqualTo("Teams: no XML picked, the kept map is gone"));

                string bad = Path.Combine(folder, "bad.teams.txt");
                File.WriteAllText(bad, "teams: B\n");
                TeamMap unread = TeamMap.Kept(bad, new TeamMapSettings());

                Assert.That(unread.IsRead, Is.False);
                Same(
                    unread.Lines(),
                    "TEAMS    no clash XML was picked, and THE TEAM MAP KEPT FROM THE LAST RUN WITH ONE, " + bad
                        + ", COULD NOT BE READ: line 1, \"teams: B\", is not a line of a team map this tool knows" + NothingApplies);
                Assert.That(unread.WindowLine(), Is.EqualTo("Teams: no XML picked, the kept map could not be read"));

                string empty = Path.Combine(folder, "empty.teams.txt");
                File.WriteAllText(empty, "# nothing\n");
                TeamMap none = TeamMap.Kept(empty, new TeamMapSettings());

                Assert.That(none.HoldsNone, Is.True);
                Same(none.Lines(), "TEAMS    no clash XML was picked, and the team map kept from the last run with one, " + empty + ", holds no team" + NothingApplies);
                Assert.That(none.WindowLine(), Is.EqualTo("Teams: no XML picked, the kept map holds no team"));

                foreach (TeamMap one in new[] { kept, missing, unread, none })
                {
                    Assert.That(one.WindowLine().Split(' ').Length, Is.LessThanOrEqualTo(12), one.WindowLine());
                }
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }
    }
}
