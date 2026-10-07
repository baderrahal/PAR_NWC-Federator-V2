using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Naming;
using Federator.Core.Teams;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The code and the team of a side and of a model, F131, Q116 answered A. A side's code
    /// is the first part of its set name that is a known code, the map's codes and those of the
    /// group's own models, and its team is the map's. A model's code is part 5 of its file name,
    /// read by ContainerName.Parse as it always was, and its team is the map's.
    ///
    /// The set names and file names below are the client's, SAMPLE DATA for the rule.
    /// </summary>
    [TestFixture]
    public class CodeOfTests
    {
        private const char Hyphen = ViewpointSettings.DefaultSetNameSeparator;

        /// <summary>The codes of 1A02MM's four models, AR, EL, ME and ST, measure-teams.md section 1.</summary>
        private static readonly string[] OneA02MM = { "AR", "EL", "ME", "ST" };

        private static TeamMap Map()
        {
            return TeamMapTests.MapOf(TeamMapTests.BadersMap);
        }

        private static string TeamOfSide(string setName, IEnumerable<string> groupCodes)
        {
            TeamMap map = Map();

            return map.TeamOf(CodeOf.Set(setName, map.KnownCodes(groupCodes), Hyphen));
        }

        [Test]
        public void ASetNameGivesTheFirstKnownCodeAndItsTeam()
        {
            TeamMap map = Map();
            IList<string> known = map.KnownCodes(OneA02MM);

            Assert.That(CodeOf.Set("BLD-ME-Ducts&Duct Fittings", known, Hyphen), Is.EqualTo("ME"));
            Assert.That(CodeOf.Set("BLD-FF-Sprinklers", known, Hyphen), Is.EqualTo("FF"));
            Assert.That(CodeOf.Set("BLD-DR-Pipes & Pipe Fittings", known, Hyphen), Is.EqualTo("DR"));
            Assert.That(CodeOf.Set("BLD-EL-Lighting Fixtures", known, Hyphen), Is.EqualTo("EL"));
            Assert.That(TeamOfSide("BLD-ME-Ducts&Duct Fittings", OneA02MM), Is.EqualTo("Mechanical"));
            Assert.That(TeamOfSide("BLD-FF-Sprinklers", OneA02MM), Is.EqualTo("Mechanical"));
            Assert.That(TeamOfSide("BLD-DR-Pipes & Pipe Fittings", OneA02MM), Is.EqualTo("Mechanical"));
            Assert.That(TeamOfSide("BLD-EL-Lighting Fixtures", OneA02MM), Is.EqualTo("Electrical"));
            Assert.That(CodeOf.Set("BLD-ST-AR-Walls", known, Hyphen), Is.EqualTo("ST"), "the first of two codes");
        }

        /// <summary>A set name carrying no known code gives no code, never a guess. Its team is then the folder's or UNKNOWN, Q117 answered C and A, TeamMapTests.</summary>
        [Test]
        public void ASetNameWithNoKnownCodeIsUnknown()
        {
            Assert.That(CodeOf.Set("BLD-Security Devices", Map().KnownCodes(OneA02MM), Hyphen), Is.EqualTo(string.Empty));
            Assert.That(TeamOfSide("BLD-Security Devices", OneA02MM), Is.EqualTo("UNKNOWN"));
            Assert.That(CodeOf.Set(null, Map().Codes, Hyphen), Is.EqualTo(string.Empty));
            Assert.That(CodeOf.Set(string.Empty, Map().Codes, Hyphen), Is.EqualTo(string.Empty));
            Assert.That(CodeOf.Set("BLD-ME-Ducts", null, Hyphen), Is.EqualTo(string.Empty));
            Assert.That(CodeOf.Set("BLD--Ducts", new[] { string.Empty, "ME" }, Hyphen), Is.EqualTo(string.Empty), "an empty part is never a code");
        }

        /// <summary>A code on no line of the map is found in a set name when a model of the group carries it, and is a team of its own.</summary>
        [Test]
        public void ACodeOfTheGroupsOwnModelsIsFoundAndIsATeamOfItsOwn()
        {
            Assert.That(TeamOfSide("BLD-LS-Trees", new[] { "AR", "LS" }), Is.EqualTo("LS"));
            Assert.That(TeamOfSide("BLD-LS-Trees", OneA02MM), Is.EqualTo("UNKNOWN"));
        }

        /// <summary>Nothing is trimmed, because two of the client's set names end in a space, and a code is matched as it is read.</summary>
        [Test]
        public void ASetNameIsReadAsItIsAndNeverTrimmed()
        {
            IList<string> known = Map().KnownCodes(OneA02MM);

            Assert.That(CodeOf.Set("BLD-ST-Stair ", known, Hyphen), Is.EqualTo("ST"));
            Assert.That(CodeOf.Set("BLD-ST -Stair", known, Hyphen), Is.EqualTo(string.Empty));
            Assert.That(CodeOf.Set("BLD-me-Ducts", known, Hyphen), Is.EqualTo(string.Empty));
            Assert.That(CodeOf.Set("BLD.ME.Ducts", known, '.'), Is.EqualTo("ME"), "the separator is a setting");
        }

        /// <summary>A model's code is part 5 of its file name through the one parser, and its team is the map's.</summary>
        [Test]
        public void AModelsTeamIsTheTeamOfItsFileNamesCode()
        {
            ParsedContainerName hv = ContainerName.Parse("1104-PAR-1A04PK-ZZZ-HV-MOD-000001.nwc", new ContainerNameSettings());
            ParsedContainerName four = ContainerName.Parse("1104-PAR-1A04PK-HV.nwc", new ContainerNameSettings());

            Assert.That(hv.Discipline, Is.EqualTo("HV"));
            Assert.That(Map().TeamOf(hv.Discipline), Is.EqualTo("Mechanical"));
            Assert.That(Map().CodeWithTeam(hv.Discipline), Is.EqualTo("HV in Mechanical"));
            Assert.That(four.IsReadable, Is.False);
            Assert.That(Map().TeamOf(four.Discipline), Is.EqualTo("UNKNOWN"));
        }

        /// <summary>
        /// Every set of the corrected XML read BY ITS CODE ALONE with this project's map: 16
        /// Architecture, 6 Structure, 25 Mechanical, 13 Electrical and one that carries no code,
        /// BLD-Security Devices, which is the 61, measure-teams.md section 2. That set is UNKNOWN by
        /// its code only. Its folder makes it Electrical under Q117 answered C, which
        /// ProjectTeamMapTests reads through TeamMap.TeamOfSet.
        /// </summary>
        [Test]
        public void EverySetOfTheCorrectedXmlHasATeamByItsCodeButSecurityDevices()
        {
            ExchangeDocument document = MatrixCorrections.ReadPicked(Samples.CorrectedMatrix());
            TeamMap map = document.Teams;
            IList<string> known = map.KnownCodes(null);
            Dictionary<string, int> perTeam = new Dictionary<string, int>();
            List<string> unknown = new List<string>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                string team = map.TeamOf(CodeOf.Set(set.Name, known, Hyphen));
                int count;

                perTeam.TryGetValue(team, out count);
                perTeam[team] = count + 1;

                if (team == "UNKNOWN")
                {
                    unknown.Add(set.Name);
                }
            }

            Assert.That(document.Sets.Count, Is.EqualTo(61));
            Assert.That(perTeam["Architecture"], Is.EqualTo(16));
            Assert.That(perTeam["Structure"], Is.EqualTo(6));
            Assert.That(perTeam["Mechanical"], Is.EqualTo(25));
            Assert.That(perTeam["Electrical"], Is.EqualTo(13));
            TeamMapTests.Same(unknown, "BLD-Security Devices");
            Assert.That(perTeam.Count, Is.EqualTo(5));
        }

        /// <summary>One rule in one place: the views' pair rule reads a set name's code through CodeOf.Set, with its own codes.</summary>
        [Test]
        public void TheViewsPairRuleReadsTheCodeTheSameWay()
        {
            ViewpointSettings settings = new ViewpointSettings();

            foreach (string name in new[] { "BLD-ME-Ducts&Duct Fittings", "BLD-Security Devices", "BLD-ST-AR-Walls", "BLD-ST -Stair", "BLD-HV-Ducts" })
            {
                Assert.That(
                    DisciplinePairRule.CodeIn(name, settings),
                    Is.EqualTo(CodeOf.Set(name, settings.DisciplineCodes, settings.SetNameSeparator)),
                    name);
            }

            Assert.That(DisciplinePairRule.CodeIn("BLD-HV-Ducts", settings), Is.EqualTo(string.Empty), "HV is none of the seven codes the views know");
        }
    }
}
