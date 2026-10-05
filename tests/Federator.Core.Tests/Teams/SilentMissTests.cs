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
                "24" + NotListed);

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
            TeamMapTests.Same(misses.Lines(), "25" + NotCounted);

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
            TeamMapTests.Same(misses.Lines(), "no set asks a workset or a file name that a model of its own team with another code does not carry");
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
                "25 set and model pair(s) of one team could not be judged, because the model's worksets were not all read");
        }

        /// <summary>
        /// Point 3's file name case. With AX on Architecture's line, an AX model's name does not
        /// hold -AR-, so the seven AR sets asking Source File contains -AR-, Q103 among them, reach
        /// none of its items. Only BLD-AR-Walls is confirmed, and no line is drafted for a Source File.
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
                "   no line is drafted for a Source File, which the list of corrections has no line for",
                "6" + NotListed);
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
            TeamMapTests.Same(misses.Lines(), "6" + NotCounted);
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
    }
}
