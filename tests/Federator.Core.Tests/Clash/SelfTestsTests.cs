using System.Collections.Generic;
using System.Linq;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using Federator.Core.Teams;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F248, Bader's answer Q232 of 20 Oct 2026: a test where a set meets itself for every set of the Mechanical and Electrical
    /// teams by the pack's team map, 25 and 14 in 1104-PAR with Security Devices, each priority A, made from the pack's sets
    /// and team map by a rule, and read by the plan, the rule of two set codes, the mirror rule and the folders as any test.
    /// Proved on the pack built into the tool and broken one thing at a time.
    /// </summary>
    [TestFixture]
    public class SelfTestsTests
    {
        private static MatrixPack ThePack()
        {
            string why;
            MatrixPack pack = MatrixPack.TheOne(MatrixPack.All(), out why);
            Assert.That(pack, Is.Not.Null, why);
            return pack;
        }

        private static SelfTestOutcome OnThePack()
        {
            MatrixPack pack = ThePack();
            return SelfTests.For(MatrixCorrections.ReadPack(pack), pack.Rules(), '-');
        }

        [Test]
        public void ThePacksRulesNameMechanicalAndElectricalAtA()
        {
            PackRules rules = ThePack().Rules();

            Assert.That(rules.IsRead, Is.True, rules.Unread);
            PackRulesTests.AreTheCodes(rules.SelfTestTeams, "Mechanical", "Electrical");
            Assert.That(rules.SelfTestPriority, Is.EqualTo(ClashPriority.A));
            PackRulesTests.AreTheCodes(rules.SameCodeLeftOut, "AR", "ST");
        }

        [Test]
        public void ThePackGets25MechanicalAnd14ElectricalSelfTestsWithSecurityDevices()
        {
            ExchangeDocument read = MatrixCorrections.ReadPack(ThePack());
            SelfTestOutcome outcome = SelfTests.For(read, ThePack().Rules(), '-');
            ExchangeDocument matrix = outcome.Matrix;

            Assert.That(read.Tests.Count, Is.EqualTo(1830), "the matrix read off the pack is never changed");
            Assert.That(matrix.Tests.Count, Is.EqualTo(1869));
            Assert.That(matrix.SelfTestNames.Count, Is.EqualTo(39));
            Assert.That(matrix.SelfTestPriority, Is.EqualTo(ClashPriority.A));
            Assert.That(matrix.SelfTestNames, Has.Member("BLD-Security Devices-vs-BLD-Security Devices"));
            Assert.That(matrix.SelfTestNames, Has.Member("BLD-ME-Ducts&Duct Fittings-vs-BLD-ME-Ducts&Duct Fittings"));
            Assert.That(matrix.SelfTestNames, Has.Member("BLD-EL-Cable Tray&Cable Tray Fittings-vs-BLD-EL-Cable Tray&Cable Tray Fittings"));
            Assert.That(matrix.SelfTestNames.Any(n => n.StartsWith("BLD-AR-") || n.StartsWith("BLD-ST-")), Is.False);
            Assert.That(outcome.Lines[0], Is.EqualTo("SELF    39 tests of a set against itself added to the 1830 of the matrix, one for every"
                + " set of Mechanical and Electrical, Mechanical 25, Electrical 14, each at priority A, 0 not made, Bader's answer to Q232"));
            Assert.That(matrix.Sets.Count, Is.EqualTo(read.Sets.Count));
            Assert.That(matrix.Teams, Is.SameAs(read.Teams));
            Assert.That(matrix.Corrections.Count, Is.EqualTo(read.Corrections.Count));
            Assert.That(matrix.Pack, Is.SameAs(read.Pack), "the reviewer's finding 1, the window's open reads the pack's name");
            Assert.That(matrix.PackFile, Is.EqualTo(read.PackFile));
            Assert.That(matrix.Sha256(), Is.EqualTo(read.Sha256()), "the run's fingerprint is the pack's file, as before");
            Assert.That(matrix.SourcePath, Is.EqualTo(read.SourcePath));
            Assert.That(ClashTestPlan.From(matrix, "ft").PlanLine("1A02MM"), Does.StartWith(
                "CLASH    1A02MM, 1830 in the file, and 39 tests of a set against itself the pack's rules add, "));
            Assert.That(ClashTestPlan.From(read, "ft").PlanLine("1A02MM"), Does.StartWith("CLASH    1A02MM, 1830 in the file, 0 skipped"));
            Assert.That(outcome.Lines, Has.Member("SELF    BLD-EL-Telecom Fixtures and BLD-EL-Telephone Devices ask the same whole question,"
                + " so their tests against themselves find the same clashes, and so does a test of the matrix between them, each keeping"
                + " its own, for Bader's decision"), "the reviewer's finding 2, said and never acted on");
            Assert.That(outcome.Lines.Count, Is.EqualTo(2), "no other line on the pack");
        }

        /// <summary>Each self test points both sides at its set and takes the settings of the first test of the matrix naming it.</summary>
        [Test]
        public void EachSelfTestSetsItsSetOnBothSidesWithTheMatrixsSettings()
        {
            ExchangeDocument read = MatrixCorrections.ReadPack(ThePack());
            ExchangeDocument matrix = OnThePack().Matrix;

            for (int i = read.Tests.Count; i < matrix.Tests.Count; i++)
            {
                ClashTestDefinition test = matrix.Tests[i];
                ClashTestDefinition first = read.Tests.First(t => t.Left.Locator == test.Left.Locator);

                Assert.That(test.Left.Locator, Is.EqualTo(test.Right.Locator), test.Name);
                Assert.That(test.Name, Is.EqualTo(SelfTests.NameFor(ByDesignRule.SetNameIn(test.Left.Locator))));
                Assert.That(read.Sets.Any(s => s.Path == test.Left.Locator), Is.True, test.Name);
                Assert.That(test.TestType, Is.EqualTo(first.TestType), test.Name);
                Assert.That(test.Left.SelfIntersect, Is.EqualTo(first.Left.SelfIntersect), test.Name);
                Assert.That(test.Right.PrimitiveTypes, Is.EqualTo(first.Left.PrimitiveTypes), test.Name);
                Assert.That(test.MergeComposites, Is.EqualTo(first.MergeComposites), test.Name);
            }
        }

        /// <summary>The priorities give every self test A, and the file's own 1830 rows are kept as they were.</summary>
        [Test]
        public void EverySelfTestReadsPriorityA()
        {
            MatrixPack pack = ThePack();
            ExchangeDocument matrix = OnThePack().Matrix;
            PriorityMap file = pack.Priorities();
            PriorityMap with = file.With(matrix.SelfTestNames, matrix.SelfTestPriority);

            Assert.That(file.RowCount, Is.EqualTo(1830));
            Assert.That(with.RowCount, Is.EqualTo(1869));
            Assert.That(with.Problems, Is.Empty);

            foreach (string name in matrix.SelfTestNames)
            {
                Assert.That(with.Of(name), Is.EqualTo(ClashPriority.A), name);
                Assert.That(file.Of(name), Is.EqualTo(ClashPriority.None), "the file itself is not changed");
            }

            Assert.That(with.Of("BLD-ME-Ducts&Duct Fittings-vs-BLD-ST-Floors"), Is.EqualTo(file.Of("BLD-ME-Ducts&Duct Fittings-vs-BLD-ST-Floors")));
            Assert.That(PriorityMap.NothingPicked().With(matrix.SelfTestNames, ClashPriority.A).RowCount, Is.EqualTo(0), "no pack, no letter");
            Assert.That(file.With(matrix.SelfTestNames, ClashPriority.None).RowCount, Is.EqualTo(1830));
        }

        /// <summary>A test the file names keeps the file's letter, and that is said, never overwritten in silence.</summary>
        [Test]
        public void ATestTheFileNamesKeepsTheFilesLetter()
        {
            PriorityMap file = PriorityMap.Read("test_name,left_set,right_set,priority\nX-vs-X,X,X,C\n", "f");
            PriorityMap with = file.With(new[] { "X-vs-X", "Y-vs-Y" }, ClashPriority.A);

            Assert.That(with.Of("X-vs-X"), Is.EqualTo(ClashPriority.C));
            Assert.That(with.Of("Y-vs-Y"), Is.EqualTo(ClashPriority.A));
            Assert.That(with.Problems.Count, Is.EqualTo(1));
            Assert.That(with.Problems[0], Does.Contain("X-vs-X").And.Contain("the file's letter is kept"));
        }

        /// <summary>
        /// The plan reads them as tests of the matrix: none is left out by the rule of two set codes, since no Mechanical or
        /// Electrical code is AR or ST, none has a mirror, and each one's folder is its team's, Security Devices Electrical.
        /// </summary>
        [Test]
        public void ThePlanTheCodeRuleTheMirrorsAndTheFoldersReadThemAsAnyTest()
        {
            MatrixPack pack = ThePack();
            ExchangeDocument matrix = OnThePack().Matrix;
            ClashTestPlan plan = ClashTestPlan.From(matrix, "ft");
            IList<string> known = matrix.Teams.KnownCodes(null);
            IDictionary<string, int> byCode;
            ClashTestPlan across = plan.AcrossTwoSetCodes(known, pack.Rules().SameCodeLeftOut, '-', out byCode);
            HashSet<string> selfNames = new HashSet<string>(matrix.SelfTestNames);
            List<PlannedClashTest> self = across.Buildable.Where(t => selfNames.Contains(t.Name)).ToList();
            ViewTeams teams = new ViewTeams(matrix.Teams, matrix.Sets, null, new ViewpointSettings());

            Assert.That(self.Count, Is.EqualTo(39), "every self test is planned and none is left out");
            Assert.That(byCode.Keys, Is.EquivalentTo(new[] { "AR", "ST" }));

            foreach (PlannedClashTest test in self)
            {
                Assert.That(MirrorRule.SetsKey(test), Is.Null, test.Name + " is no test's mirror");

                ViewClash clash = new ViewClash(test.Name, "Clash1", test.Left.Locator, test.Right.Locator,
                    ClashStatus.New, ClashPriority.A, null, null, null, null, null, null);
                string folder = DisciplineFolders.DisciplineFolderFor(clash, true, teams, '-');
                Assert.That(folder, Is.EqualTo(test.Name.StartsWith("BLD-EL-") || test.Name.StartsWith("BLD-Security") ? "Electrical" : "Mechanical"), test.Name);
            }
        }

        [Test]
        public void RulesNotReadOrNamingNoTeamMakeNone()
        {
            ExchangeDocument read = MatrixCorrections.ReadPack(ThePack());

            SelfTestOutcome unread = SelfTests.For(read, PackRules.NoneRead("f", "why"), '-');
            Assert.That(unread.Matrix, Is.SameAs(read));
            Assert.That(unread.Lines[0], Does.Contain("no test of a set against itself is made, the pack's rules not read, why"));

            SelfTestOutcome noTeam = SelfTests.For(read, PackRules.Read("same-code-left-out: AR | ST\n", "f"), '-');
            Assert.That(noTeam.Matrix, Is.SameAs(read));
            Assert.That(noTeam.Lines[0], Does.Contain("naming no team for them"));

            SelfTestOutcome nobody = SelfTests.For(read, PackRules.Read("self-test-teams: Plumbing\nself-test-priority: B\n", "f"), '-');
            Assert.That(nobody.Matrix.SelfTestNames, Is.Empty, "a team the map does not name has no set");
            Assert.That(nobody.Matrix.SelfTestPriority, Is.EqualTo(ClashPriority.None));
            Assert.That(nobody.Lines[0], Does.StartWith("SELF    0 tests of a set against itself"));
            Assert.That(nobody.Lines, Has.Member("SELF    the team map names no team Plumbing, so no set of it gets a test against itself"),
                "the reviewer's finding 5, a team spelled wrong is said");

            SelfTestOutcome electrical = SelfTests.For(read, PackRules.Read("self-test-teams: Electrical\nself-test-priority: B\n", "f"), '-');
            Assert.That(electrical.Matrix.SelfTestNames.Count, Is.EqualTo(14));
            Assert.That(electrical.Matrix.SelfTestPriority, Is.EqualTo(ClashPriority.B));
        }

        /// <summary>A set no test of the matrix names, and a name the matrix holds already, are not made and are said.</summary>
        [Test]
        public void ASetNoTestNamesAndANameTakenAreNotMadeAndSaid()
        {
            SelectionSetDefinition ducts = Set("BLD-ME-Ducts", "Mechanical");
            SelectionSetDefinition pipes = Set("BLD-ME-Pipes", "Mechanical");
            SelectionSetDefinition lonely = Set("BLD-EL-Trays", "Electrical");
            ClashTestDefinition across = Test("BLD-ME-Ducts-vs-BLD-ME-Pipes", ducts.Path, pipes.Path);
            ClashTestDefinition taken = Test("BLD-ME-Pipes-vs-BLD-ME-Pipes", pipes.Path, ducts.Path);
            ExchangeDocument matrix = new ExchangeDocument(null, "ft", new List<BatchTestDefinition>(),
                new List<ClashTestDefinition> { across, taken }, new List<SelectionSetDefinition> { ducts, pipes, lonely });
            matrix.TeamsBeside(TeamMapTests.MapOf(TeamMapTests.BadersMap));

            SelfTestOutcome outcome = SelfTests.For(matrix, PackRules.Read("self-test-teams: Mechanical | Electrical\nself-test-priority: A\n", "f"), '-');

            PackRulesTests.AreTheCodes(outcome.Matrix.SelfTestNames, "BLD-ME-Ducts-vs-BLD-ME-Ducts");
            Assert.That(outcome.Lines[0], Does.Contain("1 test of a set against itself added to the 2 of the matrix").And.Contain("2 not made"));
            Assert.That(outcome.Lines, Has.Member("SELF    not made: BLD-ME-Pipes-vs-BLD-ME-Pipes, a test of that name is in the matrix already"));
            Assert.That(outcome.Lines, Has.Member("SELF    not made: BLD-EL-Trays, no test of the matrix names the set, so the settings of its sides are UNKNOWN"));

            ExchangeDocument noMap = new ExchangeDocument(null, "ft", new List<BatchTestDefinition>(),
                new List<ClashTestDefinition> { across }, new List<SelectionSetDefinition> { ducts, pipes });
            SelfTestOutcome unmapped = SelfTests.For(noMap, PackRules.Read("self-test-teams: Mechanical\nself-test-priority: A\n", "f"), '-');
            Assert.That(unmapped.Matrix, Is.SameAs(noMap));
            Assert.That(unmapped.Lines[0], Does.Contain("no team map was read"));
        }

        /// <summary>
        /// The reviewer's finding 6. A self test carries the settings left out by the test itself and by the side it was built
        /// from, named for both its sides, and never one the template's other side left out.
        /// </summary>
        [Test]
        public void ASelfTestCarriesOnlyTheMissingSettingsOfItsOwnSide()
        {
            SelectionSetDefinition ducts = Set("BLD-ME-Ducts", "Mechanical");
            SelectionSetDefinition pipes = Set("BLD-ME-Pipes", "Mechanical");
            ClashTestDefinition across = new ClashTestDefinition("BLD-ME-Ducts-vs-BLD-ME-Pipes", "hard_conservative", "new", false, "none", null,
                new ClashSideDefinition(false, 1, ducts.Path), new ClashSideDefinition(false, 1, pipes.Path),
                new List<string> { "merge_composites", "right primtypes", "left selfintersect" });
            ExchangeDocument matrix = new ExchangeDocument(null, "ft", new List<BatchTestDefinition>(),
                new List<ClashTestDefinition> { across }, new List<SelectionSetDefinition> { ducts, pipes });
            matrix.TeamsBeside(TeamMapTests.MapOf(TeamMapTests.BadersMap));

            ExchangeDocument with = SelfTests.For(matrix, PackRules.Read("self-test-teams: Mechanical\nself-test-priority: A\n", "f"), '-').Matrix;
            ClashTestDefinition ductsSelf = with.Tests.First(t => t.Name == "BLD-ME-Ducts-vs-BLD-ME-Ducts");
            ClashTestDefinition pipesSelf = with.Tests.First(t => t.Name == "BLD-ME-Pipes-vs-BLD-ME-Pipes");

            PackRulesTests.AreTheCodes(ductsSelf.MissingSettings, "merge_composites", "left selfintersect", "right selfintersect");
            PackRulesTests.AreTheCodes(pipesSelf.MissingSettings, "merge_composites", "left primtypes", "right primtypes");
        }

        /// <summary>
        /// The breaker's findings. Two sets of one name in two folders: the second is said as such, never as a test the matrix
        /// held. More than five not made: five are named and the rest counted. A plan made from the plan keeps the count of
        /// self tests, so the clash block says the file's count apart on every plan of the run.
        /// </summary>
        [Test]
        public void ADuplicateSetNameIsSaidAsOneAndNotMadeLinesStopAtFive()
        {
            List<SelectionSetDefinition> sets = new List<SelectionSetDefinition>
            {
                Set("BLD-ME-Ducts", "Mechanical"), Set("BLD-ME-Ducts", "Mechanical", "Other")
            };
            List<ClashTestDefinition> tests = new List<ClashTestDefinition> { Test("T", sets[0].Path, sets[1].Path) };

            for (int i = 0; i < 7; i++)
            {
                sets.Add(Set("BLD-EL-Lonely " + i, "Electrical"));
            }

            ExchangeDocument matrix = new ExchangeDocument(null, "ft", new List<BatchTestDefinition>(), tests, sets);
            matrix.TeamsBeside(TeamMapTests.MapOf(TeamMapTests.BadersMap));
            SelfTestOutcome outcome = SelfTests.For(matrix, PackRules.Read("self-test-teams: Mechanical | Electrical\nself-test-priority: A\n", "f"), '-');
            List<string> notMade = outcome.Lines.Where(l => l.StartsWith("SELF    not made: ")).ToList();

            Assert.That(outcome.Matrix.SelfTestNames.Count, Is.EqualTo(1));
            Assert.That(notMade[0], Is.EqualTo("SELF    not made: BLD-ME-Ducts-vs-BLD-ME-Ducts, another set of that name in the set tree has it, at "
                + sets[0].Path + ", so this one at " + sets[1].Path + " has none"));
            Assert.That(notMade.Count, Is.EqualTo(6), "five named and one line counting the rest");
            Assert.That(notMade[5], Is.EqualTo("SELF    not made: and 3 more"));
        }

        [Test]
        public void EveryPlanOfTheRunSaysTheFilesCountApart()
        {
            MatrixPack pack = ThePack();
            ExchangeDocument matrix = OnThePack().Matrix;
            IDictionary<string, int> byCode;
            ClashTestPlan across = ClashTestPlan.From(matrix, "ft").AcrossTwoSetCodes(matrix.Teams.KnownCodes(null), pack.Rules().SameCodeLeftOut, '-', out byCode);
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.TestsInFile = across.TestsInFile;
            outcome.SelfTestsInMatrix = across.SelfTestsInMatrix;

            Assert.That(across.SelfTestsInMatrix, Is.EqualTo(39));
            Assert.That(across.TestsInFile, Is.EqualTo(1869));
            Assert.That(outcome.Lines(), Has.Member("tests in the file : 1830, and 39 tests of a set against itself the pack's rules add"));
        }

        /// <summary>The PRIORITY line says the file's rows apart from the self tests the rules gave a letter.</summary>
        [Test]
        public void ThePriorityLineSaysTheFilesRowsApart()
        {
            PriorityMap file = PriorityMap.Read("test_name,left_set,right_set,priority\nA-vs-B,A,B,B\n", "f");
            PriorityMap with = file.With(new[] { "X-vs-X", "Y-vs-Y" }, ClashPriority.A);

            Assert.That(with.MatchLines(new[] { "A-vs-B", "X-vs-X", "Y-vs-Y" })[0], Does.StartWith(
                "PRIORITY 1 in the file, and 2 tests of a set against itself given a letter by the pack's rules, 3 tests in this run"));
            Assert.That(file.MatchLines(new[] { "A-vs-B" })[0], Does.StartWith("PRIORITY 1 in the file, 1 tests in this run"));
        }

        [TestCase("self-test-teams: Mechanical\n", "names teams and no rule self-test-priority gives their tests a letter")]
        [TestCase("self-test-priority: A\n", "gives a letter and no rule self-test-teams names a team")]
        [TestCase("self-test-teams: Mechanical\nself-test-priority: D\n", "gives D, where it takes one letter, A, B or C")]
        [TestCase("self-test-teams: Mechanical\nself-test-priority: A | B\n", "gives A | B, where it takes one letter")]
        [TestCase("self-test-teams: Mechanical\nself-test-priority: Aa\n", "where it takes one letter")]
        [TestCase("self-test-teams: Mech​anical\nself-test-priority: A\n", "which has")]
        public void ASelfTestRuleThatCannotBeFollowedMakesTheFileUnread(string text, string named)
        {
            PackRules rules = PackRules.Read(text, "f");

            Assert.That(rules.IsRead, Is.False);
            Assert.That(rules.Unread, Does.Contain(named));
            Assert.That(rules.SelfTestTeams, Is.Empty);
            Assert.That(rules.SelfTestPriority, Is.EqualTo(ClashPriority.None));
        }

        [Test]
        public void ATeamNameMayHoldAnOrdinarySpace()
        {
            PackRules rules = PackRules.Read("self-test-teams: Fire Protection\nself-test-priority: a\n", "f");

            Assert.That(rules.IsRead, Is.True, rules.Unread);
            PackRulesTests.AreTheCodes(rules.SelfTestTeams, "Fire Protection");
            Assert.That(rules.SelfTestPriority, Is.EqualTo(ClashPriority.A));
        }

        private static SelectionSetDefinition Set(string name, params string[] folders)
        {
            return new SelectionSetDefinition(
                name, folders, "lcop_selection_set_tree/" + string.Join("/", folders) + "/" + name, "all", false, null, null);
        }

        private static ClashTestDefinition Test(string name, string left, string right)
        {
            return new ClashTestDefinition(name, "hard_conservative", "new", false, "none", null,
                new ClashSideDefinition(false, 1, left), new ClashSideDefinition(true, 3, right), null);
        }
    }
}
