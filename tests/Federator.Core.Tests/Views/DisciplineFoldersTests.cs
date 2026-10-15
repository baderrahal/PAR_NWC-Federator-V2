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

        private static ViewClash Test(string left, string right)
        {
            return new ViewClash(left + "-vs-" + right, "Clash1", "lcop_selection_set_tree/Folder/" + left, right,
                ClashStatus.New, ClashPriority.C, null, null, null, null, null, null);
        }

        private static string FolderOf(string left, string right = "BLD-ST-Columns")
        {
            return DisciplineFolders.DisciplineFolderFor(Test(left, right), true, Map(), '-');
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
            Assert.That(DisciplineFolders.DisciplineFolderFor(Test(left, "BLD-ST-Columns"), true, TeamMap.NoXml(new TeamMapSettings()), '-'),
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
            Assert.That(DisciplineFolders.DisciplineFolderFor(Test("BLD-ME-Ducts", "BLD-ST-Columns"), false, Map(), '-'), Is.EqualTo("Other"));
            Assert.That(DisciplineFolders.DisciplineFolderFor(null, true, Map(), '-'), Is.EqualTo("Other"));
        }

        [Test]
        public void TheFirstSideDecidesAndNeverTheSecond()
        {
            Assert.That(FolderOf("BLD-ST-Columns", "BLD-ME-Ducts"), Is.EqualTo("Structure"));
            Assert.That(FolderOf("BLD-ME-Ducts", "BLD-ST-Columns"), Is.EqualTo("Mechanical"));
            Assert.That(FolderOf("BLD-Security Devices", "BLD-EL-Cable Trays"), Is.EqualTo("Other"),
                "a first side with no code is Other even where the second carries one");
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
                found[code] = DisciplineFolders.DisciplineFolderFor(Test("BLD-" + code + "-Thing", "BLD-ST-Columns"), true, map, '-');
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
        /// name, and the folders it goes in, counted. 57 tests whose first side is BLD-Security Devices carry no code.
        /// </summary>
        [Test]
        public void EveryTestOfThePackGoesInTheFolderOfItsFirstSide()
        {
            string xml = PackFile("1104-PAR_CLASH_AllInOne_25mm_FIXED.xml");
            TeamMap map = TeamMap.Beside(xml, new TeamMapSettings());
            ExchangeDocument document = new ExchangeReader().ReadFile(xml);
            Dictionary<string, int> folders = new Dictionary<string, int>();
            Dictionary<string, int> codes = new Dictionary<string, int>();
            List<string> known = new List<string>(map.Codes);

            foreach (ClashTestDefinition test in document.Tests)
            {
                string left = ByDesignRule.SetNameIn(test.Left.Locator);
                Assert.That(test.Name.StartsWith(left + "-vs-", StringComparison.Ordinal), Is.True, test.Name);

                ViewClash clash = new ViewClash(test.Name, "Clash1", test.Left.Locator, test.Right.Locator,
                    ClashStatus.New, ClashPriority.C, null, null, null, null, null, null);
                string folder = DisciplineFolders.DisciplineFolderFor(clash, true, map, '-');
                string code = CodeOf.Set(left, known, '-');

                folders[folder] = (folders.ContainsKey(folder) ? folders[folder] : 0) + 1;
                codes[code] = (codes.ContainsKey(code) ? codes[code] : 0) + 1;
            }

            Assert.That(document.Tests.Count, Is.EqualTo(1830));
            Assert.That(codes, Is.EqualTo(new Dictionary<string, int>
            {
                { "AR", 120 }, { "ST", 111 }, { "ME", 234 }, { "PL", 195 }, { "FF", 201 }, { "DR", 220 }, { "EL", 692 }, { string.Empty, 57 }
            }));
            Assert.That(folders, Is.EqualTo(new Dictionary<string, int>
            {
                { "Architecture", 120 }, { "Structure", 111 }, { "Mechanical", 850 }, { "Electrical", 692 }, { "Other", 57 }
            }));
        }
    }
}
