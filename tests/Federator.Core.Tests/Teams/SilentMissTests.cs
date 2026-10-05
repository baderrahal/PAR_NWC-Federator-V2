using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Exchange;
using Federator.Core.Health;
using Federator.Core.Teams;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// FR-181, Q114 point 3: a set that can find nothing in a model of its own team with another
    /// code, because every group of it asks a workset that model does not carry or a Source File
    /// its file name does not hold. Named only where the coverage count shows the model holding
    /// items of the set's categories that no set catches, with a correction drafted the way the
    /// both-spellings rows of Q102 are and never applied.
    ///
    /// The sets are the client's, read off the corrected XML the way the tool reads a picked one,
    /// with this project's list beside it, so every mechanical set asks its workset in two
    /// spellings, Q102. The file names, codes and worksets of the models are SAMPLE DATA,
    /// measure-teams.md section 1.
    /// </summary>
    [TestFixture]
    public class SilentMissTests
    {
        private const char Hyphen = ViewpointSettings.DefaultSetNameSeparator;

        private const string HvFile = "1104-PAR-1A04PK-ZZZ-HV-MOD-000001.nwc";

        private const string MeFile = "1104-PAR-1A04PK-ZZZ-ME-MOD-000001.nwc";

        private const string NotListed =
            " set and model pair(s) of one team where the set cannot reach the model are not listed, because the model"
            + " holds no item of the set's categories that no set catches";

        private const string NotCounted =
            " set and model pair(s) of one team where the set cannot reach the model are not listed, because whether the"
            + " model holds items of the set's categories that no set catches is UNKNOWN until the coverage counts them";

        /// <summary>The line for the sets whose name carries no code the map or a model of the group knows, the client's BLD-Security Devices among them.</summary>
        private const string NoCodeSets =
            " set(s) carry no discipline code in their name that the map or a model of the group knows, so their team is"
            + " UNKNOWN and they were judged against no model";

        private const string AllClear =
            "no set asks a workset or a file name that a model of its own team with another code does not carry";

        /// <summary>The start of the line for nothing judged, its why after it.</summary>
        private const string NothingJudged = "no set was judged against the models of its team, because ";

        private const string StandsIn = "   the model's file name stands in for the Source File of its items, which is not read here";

        /// <summary>One spelling of every workset value the picked file's mechanical sets ask, so a model carrying them is reached by every one.</summary>
        private static readonly string[] EveryMechanicalWorkset =
        {
            "ME-Ductwork", "ME-Equipment", "ME-Piping", "FF-Fire Fighting", "FP-PIPING", "PL-Domestic water", "PL-Drainage"
        };

        private static IList<SelectionSetDefinition> TheSets()
        {
            return MatrixCorrections.ReadPicked(Samples.CorrectedMatrix()).Sets;
        }

        private static TeamMap Map()
        {
            return TeamMapTests.MapOf(TeamMapTests.BadersMap);
        }

        private static ModelExport Model(string file, string code, params string[] worksets)
        {
            return new ModelExport(file, code, 100, 100, 100, worksets);
        }

        private static List<SilentMiss> Found(SilentMisses misses)
        {
            return new List<SilentMiss>(misses.Found);
        }

        /// <summary>The coverage count of one category in one model, and zero for every other.</summary>
        private static Func<string, string, int?> Uncaught(string file, string category, int count)
        {
            return (model, asked) => model == file && asked == category ? count : 0;
        }

        /// <summary>
        /// Point 3's own case. The HV model carries its ducts on HV-Ductwork, so the four sets
        /// asking ME-DUCTWORK or ME-Ductwork reach none of them, and the coverage count shows 12
        /// Ducts in it that no set catches. The one set asking Ducts is named, with one also-ask
        /// line drafted, and that line reads back as a line of the list of corrections. The other
        /// 24 mechanical sets cannot reach the HV model either and are counted, its count of their
        /// categories being zero.
        /// </summary>
        [Test]
        public void ASetAskingAWorksetTheModelDoesNotCarryIsNamedWhenTheCoverageConfirmsIt()
        {
            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[] { Model(HvFile, "HV", "HV-Ductwork"), Model(MeFile, "ME", EveryMechanicalWorkset) },
                Map(),
                Hyphen,
                Uncaught(HvFile, "Ducts", 12));

            List<SilentMiss> confirmed = Found(misses).FindAll(one => one.Confirmed);

            Assert.That(misses.Found.Count, Is.EqualTo(25));
            Assert.That(misses.Unjudged, Is.EqualTo(0));
            Assert.That(confirmed.Count, Is.EqualTo(1));

            SilentMiss ducts = confirmed[0];

            Assert.That(ducts.SetName, Is.EqualTo("BLD-ME-Ducts&Duct Fittings"));
            Assert.That(ducts.SetCode, Is.EqualTo("ME"));
            Assert.That(ducts.Team, Is.EqualTo("Mechanical"));
            Assert.That(ducts.Model, Is.EqualTo(HvFile));
            Assert.That(ducts.ModelCode, Is.EqualTo("HV"));
            TeamMapTests.Same(ducts.WorksetsAsked, "ME-DUCTWORK", "ME-Ductwork");
            TeamMapTests.Same(ducts.FileNameAsks);
            TeamMapTests.Same(ducts.Categories, "Ducts", "Duct Fittings");
            Assert.That(ducts.Uncaught, Is.EqualTo(12));
            TeamMapTests.Same(ducts.Drafted, "also-ask: ME-DUCTWORK | HV-Ductwork");

            TeamMapTests.Same(
                misses.Lines(),
                "SILENT MISS  BLD-ME-Ducts&Duct Fittings finds nothing in " + HvFile + ", HV in Mechanical, because it asks"
                    + " the workset ME-DUCTWORK or ME-Ductwork, which that model does not carry. That model holds 12 item(s) of"
                    + " Ducts or Duct Fittings that no set catches",
                "   to approve it, copy this line into the list of corrections beside the XML: also-ask: ME-DUCTWORK | HV-Ductwork",
                "24" + NotListed,
                "1" + NoCodeSets);

            Assert.That(ducts.UncaughtWhole, Is.True);
            Assert.That(misses.SetsWithNoCode, Is.EqualTo(1));
            Assert.That(misses.ModelsWithNoCode, Is.EqualTo(0));

            MatrixCorrectionList approved = MatrixCorrectionList.Read(new StringReader(ducts.Drafted[0] + "\n"), "a list in a test");

            Assert.That(approved.Unread, Is.Null);
            Assert.That(approved.AlsoAsks.Count, Is.EqualTo(1));
            TeamMapTests.Same(approved.AlsoAsks[0], "ME-DUCTWORK", "HV-Ductwork");
        }

        /// <summary>Two spellings a model carries of one value go on one line, the only way the list takes them.</summary>
        [Test]
        public void TwoSpellingsTheModelCarriesGoOnOneLine()
        {
            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[] { Model(HvFile, "HV", "HV-Ductwork", "HV-Equipment", "HV-DUCTWORK"), Model(MeFile, "ME", EveryMechanicalWorkset) },
                Map(),
                Hyphen,
                Uncaught(HvFile, "Ducts", 12));

            SilentMiss ducts = Found(misses).Find(one => one.Confirmed);

            TeamMapTests.Same(ducts.Drafted, "also-ask: ME-DUCTWORK | HV-Ductwork | HV-DUCTWORK");
            Assert.That(MatrixCorrectionList.Read(new StringReader(ducts.Drafted[0] + "\n"), "a list in a test").Unread, Is.Null);
        }

        /// <summary>
        /// Without the coverage count nothing is named, because a model with no ducts would then
        /// be named against every duct set. Each pair is counted, the line saying whether the model
        /// holds such items is UNKNOWN, and the drafts are still carried for when it is counted.
        /// </summary>
        [Test]
        public void WithNoCoverageCountTheCandidatesAreCountedAndCalledUnknown()
        {
            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[] { Model(HvFile, "HV", "HV-Ductwork"), Model(MeFile, "ME", EveryMechanicalWorkset) },
                Map(),
                Hyphen,
                null);

            Assert.That(misses.Found.Count, Is.EqualTo(25));
            Assert.That(Found(misses).TrueForAll(one => !one.Uncaught.HasValue && !one.Confirmed), Is.True);
            TeamMapTests.Same(misses.Lines(), "25" + NotCounted, "1" + NoCodeSets);

            SilentMiss accessory = Found(misses).Find(one => one.SetName == "BLD-ME-Duct Accessory");

            TeamMapTests.Same(accessory.Drafted, "also-ask: ME-DUCTWORK | HV-Ductwork");
        }

        [Test]
        public void AModelCarryingTheAskedWorksetIsReached()
        {
            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[] { Model(HvFile, "HV", EveryMechanicalWorkset), Model(MeFile, "ME", EveryMechanicalWorkset) },
                Map(),
                Hyphen,
                Uncaught(HvFile, "Ducts", 12));

            Assert.That(misses.Found.Count, Is.EqualTo(0));
            TeamMapTests.Same(misses.Lines(), AllClear, "1" + NoCodeSets);
        }

        /// <summary>A model of another team is never named against a set, whatever it holds.</summary>
        [Test]
        public void AModelOfAnotherTeamIsNeverNamed()
        {
            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[]
                {
                    new ModelExport("1104-PAR-1A04PK-ZZZ-AR-MOD-000001.nwc", "AR", 10, 0, 10, new string[0]),
                    Model("1104-PAR-1A04PK-ZZZ-EV-MOD-000001.nwc", "EV")
                },
                Map(),
                Hyphen,
                (model, category) => 99);

            Assert.That(misses.Found.Count, Is.EqualTo(0));
            Assert.That(misses.Unjudged, Is.EqualTo(0));
        }

        /// <summary>A model whose worksets were not all read is never called missed. Each pair is counted as not judged.</summary>
        [Test]
        public void AModelWhoseWorksetsWereNotAllReadIsNotJudged()
        {
            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[]
                {
                    new ModelExport(HvFile, "HV", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted, new[] { "HV-Ductwork" }),
                    Model(MeFile, "ME", EveryMechanicalWorkset)
                },
                Map(),
                Hyphen,
                Uncaught(HvFile, "Ducts", 12));

            Assert.That(misses.Found.Count, Is.EqualTo(0));
            Assert.That(misses.Unjudged, Is.EqualTo(25));
            TeamMapTests.Same(
                misses.Lines(),
                "25 set and model pair(s) of one team could not be judged, because the model's worksets were not all read",
                "1" + NoCodeSets);
        }

        /// <summary>
        /// Point 3's file name case. With AX on Architecture's line, an AX model's name does not
        /// hold -AR-, so the seven AR sets asking Source File contains -AR-, Q103 among them, reach
        /// none of its items. Only BLD-AR-Walls is confirmed, and no line is drafted for a Source File.
        /// The line says the model's file name stands in for the Source File its items carry, which
        /// is not read, the readers' finding on attempt 1. The 45 sets of other teams carry no code
        /// this map or the AX model knows, and are counted.
        /// </summary>
        [Test]
        public void ASetAskingASourceFileTheModelsNameDoesNotHoldIsNamed()
        {
            const string AxFile = "1104-PAR-1A04PK-ZZZ-AX-MOD-000001.nwc";

            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[] { Model(AxFile, "AX", "AR-INTERIOR") },
                TeamMapTests.MapOf("team: Architecture | AR | AX\n"),
                Hyphen,
                Uncaught(AxFile, "Walls", 5));

            TeamMapTests.Same(
                Found(misses).ConvertAll(one => one.SetName),
                "BLD-AR-Floors", "BLD-AR-Stairs", "BLD-AR-Ramps", "BLD-AR-Walls", "BLD-AR-Furniture", "BLD-AR-Railings", "BLD-AR-Site");

            SilentMiss walls = Found(misses).Find(one => one.Confirmed);

            TeamMapTests.Same(walls.FileNameAsks, "-AR-");
            TeamMapTests.Same(walls.WorksetsAsked);
            TeamMapTests.Same(walls.Drafted);
            TeamMapTests.Same(
                misses.Lines(),
                "SILENT MISS  BLD-AR-Walls finds nothing in " + AxFile + ", AX in Architecture, because it asks a Source File"
                    + " holding -AR-, which that model's file name does not hold. That model holds 5 item(s) of Walls that no set catches",
                StandsIn,
                "   no line is drafted for a Source File, which the list of corrections has no line for",
                "6" + NotListed,
                "45" + NoCodeSets);
        }

        /// <summary>A confirmed miss whose model carries no spelling of the value after its prefix drafts nothing and says the spelling is UNKNOWN.</summary>
        [Test]
        public void NoSpellingAfterThePrefixDraftsNothingAndSaysUnknown()
        {
            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[] { Model(HvFile, "HV", "HV-Air"), Model(MeFile, "ME", EveryMechanicalWorkset) },
                Map(),
                Hyphen,
                Uncaught(HvFile, "Ducts", 12));

            Assert.That(misses.Lines()[1], Is.EqualTo(
                "   no workset of that model is spelled like ME-DUCTWORK or ME-Ductwork after its prefix, so the spelling to"
                    + " accept is UNKNOWN and no line is drafted"));
        }

        /// <summary>
        /// 1A02MM's four models, AR, EL, ME and ST, the ME model carrying the five spellings its
        /// sets found items with in the baseline, measure-teams.md section 4. The six FF sets,
        /// which found nothing there, are candidates against the ME model, Mechanical with
        /// another code, and none is named without a coverage count. The design guessed 1A02MM
        /// would give none, and its own rule gives these six.
        /// </summary>
        [Test]
        public void OneA02MMGivesTheSixFireSetsAsCandidatesAgainstItsMeModel()
        {
            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[]
                {
                    Model("1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc", "AR"),
                    Model("1104-PAR-1A02MM-ZZZ-EL-MOD-000001.nwc", "EL"),
                    Model("1104-PAR-1A02MM-ZZZ-ME-MOD-000001.nwc", "ME", "ME-Ductwork", "ME-Equipment", "ME-Piping", "PL-Domestic water", "PL-Drainage"),
                    Model("1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc", "ST")
                },
                Map(),
                Hyphen,
                null);

            Assert.That(misses.Found.Count, Is.EqualTo(6));
            Assert.That(Found(misses).TrueForAll(one => one.SetName.StartsWith("BLD-FF-", StringComparison.Ordinal)), Is.True);
            Assert.That(Found(misses).TrueForAll(one => one.ModelCode == "ME"), Is.True);
            TeamMapTests.Same(misses.Lines(), "6" + NotCounted, "1" + NoCodeSets);
        }

        /// <summary>Q123 by its default A: with no map every code is a team of its own, so no set is judged against another code.</summary>
        [Test]
        public void WithNoMapNoSetIsJudged()
        {
            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[] { Model(HvFile, "HV", "HV-Ductwork"), Model(MeFile, "ME") },
                TeamMap.NoXml(new TeamMapSettings()),
                Hyphen,
                Uncaught(HvFile, "Ducts", 12));

            Assert.That(misses.Found.Count, Is.EqualTo(0));
            TeamMapTests.Same(misses.Lines(), "no set was judged against the models of its team, because no team map maps a code");
            Assert.That(() => SilentMisses.Find(TheSets(), new ModelExport[0], null, Hyphen, null), Throws.ArgumentNullException);
        }

        // ---------- attempt 2, the readers' findings on attempt 1 ----------

        /// <summary>
        /// The reviewer's finding. A workset name's prefix is split by the one rule the
        /// disagreements read, WorksetDisagreements.PrefixOf, and never at the separator of a set
        /// name's parts, which is another setting. With set names split on an underscore the draft
        /// is the same, and a name with nothing before its separator, which PrefixOf reads as
        /// carrying no prefix, is never drafted.
        /// </summary>
        [Test]
        public void AWorksetsPrefixIsSplitByTheDisagreementsRuleAndNotTheSetNameSeparator()
        {
            List<SelectionSetDefinition> underscored = new List<SelectionSetDefinition>();

            foreach (SelectionSetDefinition set in TheSets())
            {
                underscored.Add(new SelectionSetDefinition(
                    set.Name.Replace('-', '_'), set.Folders, set.Path, set.FindSpecMode, set.Disjoint, set.FindSpecLocator, set.Conditions));
            }

            SilentMisses misses = SilentMisses.Find(
                underscored,
                new[] { Model(HvFile, "HV", "HV-Ductwork"), Model(MeFile, "ME", EveryMechanicalWorkset) },
                Map(),
                '_',
                Uncaught(HvFile, "Ducts", 12));

            SilentMiss ducts = Found(misses).Find(one => one.Confirmed);

            Assert.That(ducts.SetName, Is.EqualTo("BLD_ME_Ducts&Duct Fittings"));
            TeamMapTests.Same(ducts.Drafted, "also-ask: ME-DUCTWORK | HV-Ductwork");

            SilentMisses noPrefix = SilentMisses.Find(
                TheSets(),
                new[] { Model(HvFile, "HV", "-Ductwork"), Model(MeFile, "ME", EveryMechanicalWorkset) },
                Map(),
                Hyphen,
                Uncaught(HvFile, "Ducts", 12));

            TeamMapTests.Same(Found(noPrefix).Find(one => one.Confirmed).Drafted);
        }

        /// <summary>
        /// The breaker's finding. A count that could not be taken is minus one in this repo,
        /// ModelExport.NotCounted, and never a zero. With every count minus one each pair is
        /// UNKNOWN and counted so, never called a model holding no such item.
        /// </summary>
        [Test]
        public void ACountThatCouldNotBeTakenIsUnknownAndNeverZero()
        {
            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[] { Model(HvFile, "HV", "HV-Ductwork"), Model(MeFile, "ME", EveryMechanicalWorkset) },
                Map(),
                Hyphen,
                (model, category) => ModelExport.NotCounted);

            Assert.That(misses.Found.Count, Is.EqualTo(25));
            Assert.That(Found(misses).TrueForAll(one => !one.Uncaught.HasValue && !one.Confirmed), Is.True);
            TeamMapTests.Same(misses.Lines(), "25" + NotCounted, "1" + NoCodeSets);
        }

        /// <summary>
        /// The breaker's finding. Where one category is counted above zero and another could not be
        /// counted, minus one or null, the sum is a lower bound: the miss is confirmed and its line
        /// says at least, and why.
        /// </summary>
        [Test]
        public void ASumBesideACountNotTakenIsALowerBoundAndSaysSo()
        {
            foreach (int? notTaken in new int?[] { ModelExport.NotCounted, null })
            {
                SilentMisses misses = SilentMisses.Find(
                    TheSets(),
                    new[] { Model(HvFile, "HV", "HV-Ductwork"), Model(MeFile, "ME", EveryMechanicalWorkset) },
                    Map(),
                    Hyphen,
                    (model, category) => model != HvFile ? 0 : category == "Ducts" ? 12 : category == "Duct Fittings" ? notTaken : 0);

                SilentMiss ducts = Found(misses).Find(one => one.Confirmed);

                Assert.That(ducts.Uncaught, Is.EqualTo(12), "not taken " + notTaken);
                Assert.That(ducts.UncaughtWhole, Is.False, "not taken " + notTaken);
                Assert.That(misses.Lines()[0], Does.EndWith(
                    "That model holds at least 12 item(s) of Ducts or Duct Fittings that no set catches, because a count of"
                        + " some of them was not taken"), "not taken " + notTaken);
            }
        }

        /// <summary>
        /// The readers' findings. A set whose name carries no code the map or a model of the group
        /// knows, the client's BLD-Security Devices, and a model whose code was not read are judged
        /// against nothing, and the lines count them, so the all clear is never read as every set
        /// and every model judged. Nothing handed in is said too.
        /// </summary>
        [Test]
        public void ASetOrAModelWithNoCodeIsCountedAndSaid()
        {
            SilentMisses misses = SilentMisses.Find(
                TheSets(),
                new[]
                {
                    Model(HvFile, "HV", EveryMechanicalWorkset),
                    Model(MeFile, "ME", EveryMechanicalWorkset),
                    Model("1104-PAR-1A04PK-ZZZ-MOD-000001.nwc", string.Empty, "HV-Ductwork")
                },
                Map(),
                Hyphen,
                Uncaught(HvFile, "Ducts", 12));

            Assert.That(misses.Found.Count, Is.EqualTo(0));
            Assert.That(misses.SetsWithNoCode, Is.EqualTo(1));
            Assert.That(misses.ModelsWithNoCode, Is.EqualTo(1));
            TeamMapTests.Same(
                misses.Lines(),
                AllClear,
                "1" + NoCodeSets,
                "1 model(s) carry no discipline code in their file name, so their team is UNKNOWN and no set was judged against them");

            TeamMapTests.Same(SilentMisses.Find(null, new[] { Model(MeFile, "ME") }, Map(), Hyphen, null).Lines(), NothingJudged + "no set was handed in");
            TeamMapTests.Same(SilentMisses.Find(TheSets(), null, Map(), Hyphen, null).Lines(), NothingJudged + "no model was handed in");
        }

        /// <summary>
        /// The breaker's finding on F131's second attempt. An EMPTY list of sets or of models,
        /// a group whose models were all dropped or a picked file holding no set, judges nothing
        /// just as a list not handed in does, and a group where no model shares a set's team
        /// with another code judges nothing too. Each is said as nothing judged, with why, and
        /// never as the all clear, which would be a statement about sets and models never read.
        /// </summary>
        [Test]
        public void NothingJudgedIsSaidAsNothingJudgedWithWhyAndNeverAsTheAllClear()
        {
            ModelExport[] me = { Model(MeFile, "ME", EveryMechanicalWorkset) };
            SelectionSetDefinition[] noSet = new SelectionSetDefinition[0];
            ModelExport[] noModel = new ModelExport[0];

            TeamMapTests.Same(SilentMisses.Find(noSet, me, Map(), Hyphen, null).Lines(), NothingJudged + "no set was handed in");
            TeamMapTests.Same(SilentMisses.Find(TheSets(), noModel, Map(), Hyphen, null).Lines(), NothingJudged + "no model was handed in");
            TeamMapTests.Same(SilentMisses.Find(noSet, noModel, Map(), Hyphen, null).Lines(), NothingJudged + "no set and no model were handed in");
            TeamMapTests.Same(SilentMisses.Find(null, null, Map(), Hyphen, null).Lines(), NothingJudged + "no set and no model were handed in");

            const string NoPair = "no model of the group is of a set's team with a code other than the set's";

            SilentMisses architecture = SilentMisses.Find(
                TheSets(),
                new[] { Model("1104-PAR-1A04PK-ZZZ-AR-MOD-000001.nwc", "AR", "AR-Walls") },
                Map(),
                Hyphen,
                (model, category) => 99);

            Assert.That(architecture.Found.Count, Is.EqualTo(0));
            TeamMapTests.Same(architecture.Lines(), NothingJudged + NoPair, "1" + NoCodeSets);

            List<SelectionSetDefinition> noCode = new List<SelectionSetDefinition>(TheSets()).FindAll(
                set => set.Name == "BLD-Security Devices");

            Assert.That(noCode.Count, Is.EqualTo(1));
            TeamMapTests.Same(
                SilentMisses.Find(noCode, me, Map(), Hyphen, null).Lines(),
                NothingJudged + NoPair,
                "1" + NoCodeSets);
        }

        /// <summary>
        /// The breaker's finding on F131's second attempt. A workset condition whose value is
        /// empty asks no name, so it closes nothing, as every other reader of workset values
        /// skips it. It is never a set that cannot reach a model, never a line naming the
        /// workset as nothing, and the pair is judged open.
        /// </summary>
        [Test]
        public void AnEmptyWorksetValueAsksNoNameAndIsNeverAMiss()
        {
            const string Element = "<category><name internal=\"LcRevitData_Element\">Element</name></category>";
            string xml = "<?xml version='1.0' encoding='UTF-8'?>\n<exchange units=\"ft\"><selectionsets><viewfolder name=\"Mechanical\">"
                + "<selectionset name=\"BLD-ME-Ducts\" guid=\"x\"><findspec mode=\"all\" disjoint=\"0\"><conditions>"
                + "<condition test=\"equals\" flags=\"0\">" + Element
                + "<property><name internal=\"LcRevitPropertyElementCategory\">Category</name></property>"
                + "<value><data type=\"wstring\">Ducts</data></value></condition>"
                + "<condition test=\"equals\" flags=\"0\">" + Element
                + "<property><name internal=\"lcldrevit_parameter_-1002053\">Workset</name></property>"
                + "<value><data type=\"wstring\"></data></value></condition>"
                + "</conditions><locator>/</locator></findspec></selectionset></viewfolder></selectionsets></exchange>\n";

            SilentMisses misses = SilentMisses.Find(
                new ExchangeReader().ReadText(xml).Sets,
                new[] { Model(HvFile, "HV", "HV-Ductwork"), Model(MeFile, "ME", "ME-Ductwork") },
                Map(),
                Hyphen,
                (model, category) => 7);

            Assert.That(misses.Found.Count, Is.EqualTo(0));
            TeamMapTests.Same(misses.Lines(), AllClear);
        }

        /// <summary>
        /// The breaker's finding. A set asking its category only by contains has no category the
        /// coverage counts by its whole name, so no count can ever confirm it, and its line says
        /// that and not UNKNOWN until the coverage counts them.
        /// </summary>
        [Test]
        public void ASetAskingNoCategoryByItsWholeNameIsSaidSo()
        {
            const string Element = "<category><name internal=\"LcRevitData_Element\">Element</name></category>";
            string xml = "<?xml version='1.0' encoding='UTF-8'?>\n<exchange units=\"ft\"><selectionsets><viewfolder name=\"Mechanical\">"
                + "<selectionset name=\"BLD-ME-Any Duct\" guid=\"x\"><findspec mode=\"all\" disjoint=\"0\"><conditions>"
                + "<condition test=\"contains\" flags=\"0\">" + Element
                + "<property><name internal=\"LcRevitPropertyElementCategory\">Category</name></property>"
                + "<value><data type=\"wstring\">Duct</data></value></condition>"
                + "<condition test=\"equals\" flags=\"0\">" + Element
                + "<property><name internal=\"lcldrevit_parameter_-1002053\">Workset</name></property>"
                + "<value><data type=\"wstring\">ME-Ductwork</data></value></condition>"
                + "</conditions><locator>/</locator></findspec></selectionset></viewfolder></selectionsets></exchange>\n";

            SilentMisses misses = SilentMisses.Find(
                new ExchangeReader().ReadText(xml).Sets,
                new[] { Model(HvFile, "HV", "HV-Ductwork"), Model(MeFile, "ME", "ME-Ductwork") },
                Map(),
                Hyphen,
                (model, category) => 7);

            Assert.That(misses.Found.Count, Is.EqualTo(1));
            Assert.That(misses.Found[0].Categories.Count, Is.EqualTo(0));
            TeamMapTests.Same(
                misses.Lines(),
                "1 set and model pair(s) of one team where the set cannot reach the model are not listed, because the set"
                    + " asks no category by its whole name, so no coverage count can confirm them");
        }
    }
}
