using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using Federator.Core.Teams;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F199, Bader's order of round 6, Q183 and Q184. A test's discipline folder is the code of its first side: the words
    /// of the order first, then the pack's team line for a code they do not name, else Other, and a saved test the pack
    /// does not name is Other. The set names are sample data, and the last tests read the pack itself.
    /// </summary>
    [TestFixture]
    public class DisciplineFoldersTests
    {
        private static TeamMap Map()
        {
            return TeamMapTests.MapOf(TeamMapTests.BadersMap);
        }

        private static ViewTeams Teams(TeamMap map, params SelectionSetDefinition[] sets)
        {
            return new ViewTeams(map, sets.Length == 0 ? null : sets, null, new ViewpointSettings());
        }

        private static SelectionSetDefinition Set(string name, params string[] folders)
        {
            return new SelectionSetDefinition(
                name, folders, "lcop_selection_set_tree/" + string.Join("/", folders) + "/" + name, "all", false, null, null);
        }

        private static ViewClash Test(string left, string right)
        {
            return new ViewClash(left + "-vs-" + right, "Clash1", "lcop_selection_set_tree/Folder/" + left, right,
                ClashStatus.New, ClashPriority.C, null, null, null, null, null, null);
        }

        private static string FolderOf(string left, string right = "BLD-ST-Columns")
        {
            return DisciplineFolders.DisciplineFolderFor(Test(left, right), true, Teams(Map()), '-');
        }

        [TestCase("BLD-AR-Walls", "Architecture")]
        [TestCase("BLD-Architecture-Walls", "Architecture")]
        [TestCase("BLD-ST-Columns", "Structure")]
        [TestCase("BLD-Structure-Columns", "Structure")]
        [TestCase("BLD-ME-Ducts", "Mechanical")]
        [TestCase("BLD-MEP-Ducts", "Mechanical")]
        [TestCase("BLD-Mechanical-Ducts", "Mechanical")]
        [TestCase("BLD-PL-Pipes", "Mechanical")]
        [TestCase("BLD-FP-Sprinklers", "Mechanical")]
        [TestCase("BLD-FF-Sprinklers", "Mechanical")]
        [TestCase("BLD-EL-Cable Trays", "Electrical")]
        [TestCase("BLD-Electrical-Cable Trays", "Electrical")]
        public void EveryWordOfTheOrderGoesInItsFolder(string left, string folder)
        {
            Assert.That(FolderOf(left), Is.EqualTo(folder));
        }

        /// <summary>Q184. The codes the order does not name go by the pack's team line, which names them, never guessed.</summary>
        [TestCase("BLD-HV-Ducts", "Mechanical")]
        [TestCase("BLD-DR-Pipe Accessories", "Mechanical")]
        [TestCase("BLD-EV-CCTV", "Electrical")]
        public void ACodeTheOrderDoesNotNameGoesByThePacksTeamLine(string left, string folder)
        {
            Assert.That(FolderOf(left), Is.EqualTo(folder));
            Assert.That(DisciplineFolders.DisciplineFolderFor(Test(left, "BLD-ST-Columns"), true, Teams(TeamMap.NoXml(new TeamMapSettings())), '-'),
                Is.EqualTo("Other"), "with no team map read there is no line to read it off");
        }

        [TestCase("BLD-Security Devices")]
        [TestCase("BLD-LS-Lamps")]
        [TestCase("BLD-ar-Walls")]
        [TestCase("BLD-ARX-Walls")]
        [TestCase("Walls")]
        [TestCase("")]
        public void AnythingElseIsOther(string left)
        {
            Assert.That(FolderOf(left), Is.EqualTo("Other"));
        }

        [Test]
        public void ASavedTestThePackDoesNotNameIsOtherWhateverItsFirstSide()
        {
            Assert.That(DisciplineFolders.DisciplineFolderFor(Test("BLD-ME-Ducts", "BLD-ST-Columns"), false, Teams(Map()), '-'), Is.EqualTo("Other"));
            Assert.That(DisciplineFolders.DisciplineFolderFor(null, true, Teams(Map()), '-'), Is.EqualTo("Other"));
        }

        [Test]
        public void TheFirstSideDecidesAndNeverTheSecond()
        {
            Assert.That(FolderOf("BLD-ST-Columns", "BLD-ME-Ducts"), Is.EqualTo("Structure"));
            Assert.That(FolderOf("BLD-ME-Ducts", "BLD-ST-Columns"), Is.EqualTo("Mechanical"));
            Assert.That(FolderOf("BLD-Security Devices", "BLD-EL-Cable Trays"), Is.EqualTo("Other"),
                "a first side with no code and no set tree read is Other even where the second carries one");
        }

        /// <summary>
        /// F246, Bader's answer Q229 of 20 Oct 2026. AR and ST in either order are Architecture, and every other test keeps
        /// its first side, so AR against a service stays with the service where the service is first.
        /// </summary>
        [TestCase("BLD-ST-Walls", "BLD-AR-Walls", "Architecture")]
        [TestCase("BLD-AR-Walls", "BLD-ST-Walls", "Architecture")]
        [TestCase("BLD-Structure-Walls", "BLD-Architecture-Walls", "Architecture")]
        [TestCase("BLD-ST-Walls", "BLD-ST-Columns", "Structure")]
        [TestCase("BLD-ST-Walls", "BLD-ME-Ducts", "Structure")]
        [TestCase("BLD-ME-Ducts", "BLD-AR-Walls", "Mechanical")]
        [TestCase("BLD-EL-Cable Trays", "BLD-AR-Ceilings", "Electrical")]
        [TestCase("BLD-HV-Ducts", "BLD-AR-Ceilings", "Mechanical")]
        [TestCase("BLD-AR-Walls", "BLD-ME-Ducts", "Architecture")]
        public void ArchitectureAgainstStructureIsArchitectureInEitherOrder(string left, string right, string folder)
        {
            Assert.That(FolderOf(left, right), Is.EqualTo(folder));
        }

        /// <summary>
        /// F246, Bader's answer Q231 of 20 Oct 2026. A side whose set name carries no code takes the team the folder above it
        /// in the clash XML's set tree names, the reader the TEAMS lines use, and a folder naming no team, or two sets of
        /// that name giving two teams, leaves it Other.
        /// </summary>
        [Test]
        public void ASideWithNoCodeTakesTheTeamItsSetTreeFolderNames()
        {
            ViewTeams teams = Teams(Map(), Set("BLD-Security Devices", "Electrical"), Set("BLD-Signs", "Security"),
                Set("BLD-Devices", "Electrical"), Set("BLD-Devices", "Mechanical"), Set("BLD-Pumps", "Mechanical", "Mechanical-Water"));

            Assert.That(DisciplineFolders.DisciplineFolderFor(Test("BLD-Security Devices", "BLD-ST-Columns"), true, teams, '-'),
                Is.EqualTo("Electrical"));
            Assert.That(DisciplineFolders.DisciplineFolderFor(Test("BLD-Pumps", "BLD-ST-Columns"), true, teams, '-'),
                Is.EqualTo("Mechanical"), "the folder nearest the set naming a team is read");
            Assert.That(DisciplineFolders.DisciplineFolderFor(Test("BLD-Signs", "BLD-ST-Columns"), true, teams, '-'),
                Is.EqualTo("Other"), "a folder naming no team gives no folder");
            Assert.That(DisciplineFolders.DisciplineFolderFor(Test("BLD-Devices", "BLD-ST-Columns"), true, teams, '-'),
                Is.EqualTo("Other"), "two sets of one name in two teams read UNKNOWN, never one of them guessed");
            Assert.That(DisciplineFolders.DisciplineFolderFor(Test("BLD-Security Devices", "BLD-ST-Columns"), true,
                Teams(TeamMap.NoXml(new TeamMapSettings()), Set("BLD-Security Devices", "Electrical")), '-'),
                Is.EqualTo("Other"), "with no team map read no folder names a team");
            Assert.That(DisciplineFolders.DisciplineFolderFor(Test("BLD-Security Devices", "BLD-ST-Columns"), false, teams, '-'),
                Is.EqualTo("Other"), "a saved test the pack does not name is Other whatever its side");
        }

        [Test]
        public void OnlyMechanicalAndElectricalHoldTheSizeFolder()
        {
            List<string> holding = new List<string>();

            foreach (string folder in DisciplineFolders.Names)
            {
                if (DisciplineFolders.HoldsTheSizeFolder(folder))
                {
                    holding.Add(folder);
                }
            }

            Assert.That(DisciplineFolders.Names, Is.EqualTo(new[] { "Architecture", "Structure", "Mechanical", "Electrical", "Other" }));
            Assert.That(holding, Is.EqualTo(new[] { "Mechanical", "Electrical" }));
        }

        /// <summary>The NWF page's rules line names the tree as it is built, the folder names read off their one place.</summary>
        [Test]
        public void TheNwfPagesRulesLineNamesTheTreeAsItIsBuilt()
        {
            string line = Federator.Core.Rerun.NwfPageWords.RulesLine;

            Assert.That(line, Does.Contain("The tool viewpoint folder " + ToolViewFolder.Name + " is deleted and made again on each run."));
            Assert.That(line, Does.Contain("Inside it A, B and C, then " + string.Join(", ", new List<string>(DisciplineFolders.Names).GetRange(0, 4).ToArray())
                + " and " + DisciplineFolders.Other + ", each made only where a test is in it"));
            Assert.That(line, Does.Contain("Over 150mm inside Mechanical and Electrical of C only"));
        }

        // ---------- the pack itself ----------

        private static string PackFile(string name)
        {
            return System.IO.Path.Combine(Samples.Repo(), "src", "Federator.Core", "Packs", "1104-PAR", name);
        }

        /// <summary>Every code a team line of the pack names, and the folder each goes in, read off the pack's own file.</summary>
        [Test]
        public void EveryCodeOfThePacksTeamLinesGoesInTheFolderOfItsTeam()
        {
            TeamMap map = TeamMap.Beside(PackFile("1104-PAR_CLASH_AllInOne_25mm_FIXED.xml"), new TeamMapSettings());
            Dictionary<string, string> found = new Dictionary<string, string>();

            Assert.That(map.IsRead, Is.True, map.Unread);

            foreach (string code in map.Codes)
            {
                found[code] = DisciplineFolders.DisciplineFolderFor(Test("BLD-" + code + "-Thing", "BLD-ME-Ducts"), true, Teams(map), '-');
            }

            Assert.That(found, Is.EqualTo(new Dictionary<string, string>
            {
                { "AR", "Architecture" },
                { "ST", "Structure" },
                { "HV", "Mechanical" },
                { "PL", "Mechanical" },
                { "FP", "Mechanical" },
                { "ME", "Mechanical" },
                { "DR", "Mechanical" },
                { "FF", "Mechanical" },
                { "EL", "Electrical" },
                { "EV", "Electrical" }
            }));
        }

        /// <summary>
        /// Every test of the pack: its name is its left set, -vs-, its right set, so the left set is the first side of its
        /// name, and the folders it goes in, counted, F246. The 57 tests whose first side is BLD-Security Devices carry no
        /// code and go in Electrical by the set tree folder above it, Q231, and the 96 of ST against AR go in Architecture,
        /// Q229, so Structure keeps the 15 of ST against ST alone and Other holds none, read off the pack on 20 Oct 2026.
        /// </summary>
        [Test]
        public void EveryTestOfThePackGoesInTheFolderOfItsFirstSide()
        {
            string xml = PackFile("1104-PAR_CLASH_AllInOne_25mm_FIXED.xml");
            TeamMap map = TeamMap.Beside(xml, new TeamMapSettings());
            ExchangeDocument document = new ExchangeReader().ReadFile(xml);
            ViewTeams teams = new ViewTeams(map, document.Sets, null, new ViewpointSettings());
            Dictionary<string, int> folders = new Dictionary<string, int>();
            Dictionary<string, int> codes = new Dictionary<string, int>();
            List<string> known = new List<string>(map.Codes);
            int structureAgainstArchitecture = 0;

            foreach (ClashTestDefinition test in document.Tests)
            {
                string left = ByDesignRule.SetNameIn(test.Left.Locator);
                Assert.That(test.Name.StartsWith(left + "-vs-", StringComparison.Ordinal), Is.True, test.Name);

                ViewClash clash = new ViewClash(test.Name, "Clash1", test.Left.Locator, test.Right.Locator,
                    ClashStatus.New, ClashPriority.C, null, null, null, null, null, null);
                string folder = DisciplineFolders.DisciplineFolderFor(clash, true, teams, '-');
                string code = CodeOf.Set(left, known, '-');

                folders[folder] = (folders.ContainsKey(folder) ? folders[folder] : 0) + 1;
                codes[code] = (codes.ContainsKey(code) ? codes[code] : 0) + 1;

                if (code == "ST" && CodeOf.Set(ByDesignRule.SetNameIn(test.Right.Locator), known, '-') == "AR")
                {
                    structureAgainstArchitecture++;
                    Assert.That(folder, Is.EqualTo("Architecture"), test.Name);
                }
            }

            Assert.That(structureAgainstArchitecture, Is.EqualTo(96));

            Assert.That(document.Tests.Count, Is.EqualTo(1830));
            Assert.That(codes, Is.EqualTo(new Dictionary<string, int>
            {
                { "AR", 120 }, { "ST", 111 }, { "ME", 234 }, { "PL", 195 }, { "FF", 201 }, { "DR", 220 }, { "EL", 692 }, { string.Empty, 57 }
            }));
            Assert.That(folders, Is.EqualTo(new Dictionary<string, int>
            {
                { "Architecture", 216 }, { "Structure", 15 }, { "Mechanical", 850 }, { "Electrical", 749 }
            }));
        }
    }
}
