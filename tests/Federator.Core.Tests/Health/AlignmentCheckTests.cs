using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Grouping;
using Federator.Core.Health;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests.Health
{
    /// <summary>
    /// PART 4, Q64 answered on 2026-09-20: alignment is by SHARED COORDINATE, and by eye
    /// where that cannot be read. The numbers in these tests are the ones read off two of
    /// the client's real groups on that day, docs\history\scan.md 5q.
    /// </summary>
    [TestFixture]
    public class AlignmentCheckTests
    {
        private static ModelPlacement At(string discipline, string site, double x, double y, double z)
        {
            return new ModelPlacement(
                "1104-PAR-1A02MM-ZZZ-" + discipline + "-MOD-000001.nwc", discipline, site, x, y, z);
        }

        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        /// <summary>The real 1A02MM, where all four agree in X and Y and differ in Z by up to 312 mm.</summary>
        private static IList<ModelPlacement> TheRealGroup()
        {
            return new List<ModelPlacement>
            {
                At("AR", "SWLS-02-SharedCoordinate", 0.0, -13419.0, -509.0),
                At("EL", "LTB2", 0.0, -13419.0, -197.0),
                At("ME", "PW3_Shared_Location", 0.0, -13419.0, -492.0),
                At("ST", "Internal", 0.0, -13419.0, -254.0)
            };
        }

        [Test]
        public void TheArchitectureModelIsTheReference()
        {
            string block = Joined(AlignmentCheck.Lines(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres, AlignmentCheck.DefaultSkipClashOffCoordinates, true));

            Assert.That(block, Does.Contain("AR  1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc is the reference"));
            Assert.That(block, Does.Contain("reference, shared coordinate \"SWLS-02-SharedCoordinate\""));
        }

        [Test]
        public void AModelAtADifferentHeightIsNamedWithTheDifferenceInXYAndZSeparately()
        {
            string block = Joined(AlignmentCheck.Lines(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres, AlignmentCheck.DefaultSkipClashOffCoordinates, true));

            Assert.That(block, Does.Contain("EL  1104-PAR-1A02MM-ZZZ-EL-MOD-000001.nwc   DIFFERENT"));
            Assert.That(block, Does.Contain("dx 0 mm"));
            Assert.That(block, Does.Contain("dy 0 mm"));
            Assert.That(block, Does.Contain("dz 312 mm"),
                "Z on its own matters most, because a model one storey out reads as nothing in plan");
            Assert.That(block, Does.Contain("ask the EL originator to re-export on the project shared coordinates"));
        }

        [Test]
        public void ItNamesTheDifferentSharedCoordinatesAndTheModelsOnRevitsInternalOrigin()
        {
            string block = Joined(AlignmentCheck.Lines(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres, AlignmentCheck.DefaultSkipClashOffCoordinates, true));

            Assert.That(block, Does.Contain("this group names 4 different shared coordinates"));
            Assert.That(block, Does.Contain("1 model(s) name their site \"Internal\""));
            Assert.That(block, Does.Contain("not exported on a shared site at all"));
        }

        [Test]
        public void AGroupWhereEveryModelAgreesSaysSoRatherThanSayingNothing()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", 0.0, 0.0, 0.0),
                At("ST", "Site", 0.0, 0.0, 0.5)
            };

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, AlignmentCheck.DefaultSkipClashOffCoordinates, true));

            Assert.That(block, Does.Contain("all 2 model(s) sit within 1 mm of the reference"),
                "half a millimetre is the export rounding a number and not an offset somebody put there");
            Assert.That(block, Does.Not.Contain("DIFFERENT"));
        }

        [Test]
        public void WithNoArchitectureModelAnotherIsUsedAndTheBlockSaysWhich()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("ME", "Site", 0.0, 0.0, 0.0),
                At("ST", "Site", 1000.0, 0.0, 0.0)
            };

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, AlignmentCheck.DefaultSkipClashOffCoordinates, true));

            Assert.That(block, Does.Contain("ME  1104-PAR-1A02MM-ZZZ-ME-MOD-000001.nwc is the reference"));
            Assert.That(block, Does.Contain("because this group carries no AR model"));
            Assert.That(block, Does.Contain("DIFFERENT"));
        }

        /// <summary>
        /// A model that could not be placed is never called different, the same way a
        /// census count that could not be taken is never called a move.
        /// </summary>
        [Test]
        public void AModelThatCouldNotBePlacedIsSaidAndIsNeverCalledDifferent()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", 0.0, 0.0, 0.0),
                At("ST", "Site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead)
            };

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, AlignmentCheck.DefaultSkipClashOffCoordinates, true));

            Assert.That(block, Does.Contain("NOT READ, its placement could not be read"));
            Assert.That(block, Does.Not.Contain("DIFFERENT"));
            Assert.That(block, Does.Contain("1 could not be placed and were not compared"));
            Assert.That(AlignmentCheck.DifferentCount(models, AlignmentCheck.DefaultToleranceMillimetres), Is.EqualTo(0));
        }

        [Test]
        public void AModelCarryingNoSharedCoordinateIsSentToBeCheckedByEyeWhichIsWhatQ64Asks()
        {
            IList<ModelPlacement> models = new List<ModelPlacement> { At("AR", string.Empty, 0.0, 0.0, 0.0) };

            Assert.That(Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, AlignmentCheck.DefaultSkipClashOffCoordinates, true)),
                Does.Contain("NO shared coordinate on the model at all, so check this one by eye"));
        }

        [Test]
        public void WithNoModelAtAllItSaysSoRatherThanWritingAnEmptyBlock()
        {
            Assert.That(Joined(AlignmentCheck.Lines(new List<ModelPlacement>(), AlignmentCheck.DefaultFarModelMillimetres, AlignmentCheck.DefaultSkipClashOffCoordinates, true)),
                Does.Contain("no model was read, so nothing could be compared"));
        }

        [Test]
        public void TheDifferenceCountIsTheRunLineAndCountsOnlyWhatMoved()
        {
            Assert.That(
                AlignmentCheck.DifferentCount(TheRealGroup(), AlignmentCheck.DefaultToleranceMillimetres),
                Is.EqualTo(3), "three of the four sit somewhere the architecture does not");
        }

        // ---------- Q70, and Bader's answer to Q100, a model on the internal origin ----------

        private const bool RuleOn = true;
        private const bool RuleOff = false;

        /// <summary>The run runs a clash test in the group, which every test here took for granted before attempt 3.</summary>
        private const bool ATestRuns = true;

        /// <summary>
        /// 1A02MM's real shape on 2026-09-20: four models, one of them on Internal. With the
        /// rule that skips the clash switched OFF the group is judged as before Bader's answer
        /// to Q100, FAILED as Q70 answered, and the block says which model and why.
        /// </summary>
        [Test]
        public void WithTheRuleOffAGroupWithAModelOnTheInternalOriginIsFailedAndTheBlockSaysWhich()
        {
            string why = AlignmentCheck.WhyItFailsTheGroup(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres, RuleOff, ATestRuns);

            Assert.That(why, Is.Not.Null);
            Assert.That(why, Does.Contain("1 model(s) were exported on Revit's internal origin"));
            Assert.That(why, Does.Contain("ST  1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc"));

            Assert.That(
                Joined(AlignmentCheck.Lines(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres, RuleOff, true)),
                Does.Contain("THIS GROUP IS FAILED"));
        }

        /// <summary>
        /// The half that stops this being a blunt instrument. A failed group still writes
        /// its federation, its NWD and its report, because Bader needs the evidence to
        /// take to the people who own the models and a group that produces nothing gives
        /// him nothing to send.
        /// </summary>
        [Test]
        public void AFailedGroupStillSaysEveryOutputWasWritten()
        {
            Assert.That(
                AlignmentCheck.WhyItFailsTheGroup(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres, RuleOff, ATestRuns),
                Does.Contain("Every output of this group was still written"));
        }

        /// <summary>
        /// Bader's answer to Q100 on 2026-10-04: a model named Internal no longer FAILS its
        /// group, it skips the clash the same way a model more than the setting away does.
        /// The real 1A02MM, a building of the wave 1 test, held one when 5q measured it.
        /// </summary>
        [Test]
        public void WithTheRuleOnAModelOnTheInternalOriginSkipsTheClashAndDoesNotFailTheGroup()
        {
            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres);
            string block = Joined(AlignmentCheck.Lines(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres, RuleOn, true));

            Assert.That(AlignmentCheck.WhyItFailsTheGroup(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres, RuleOn, ATestRuns), Is.Null);
            Assert.That(off.Models.Count, Is.EqualTo(1), "the EL at 312 mm and the ME at 17 mm name real sites and sit under a metre");
            Assert.That(off.Models[0], Is.EqualTo(
                "ST  1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc   shared site \"Internal\", Revit's own origin and not a"
                + " shared site   X 0 mm  Y 0 mm  Z 255 mm from the reference, 0.255 m in a straight line"));
            Assert.That(block, Does.Contain("CLASH SKIPPED. 1 model(s) are not on the same shared coordinates"));
            Assert.That(block, Does.Not.Contain("THIS GROUP IS FAILED"));
        }

        [Test]
        public void AGroupWithEveryModelOnOneNamedSiteIsNotFailed()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "SWLS-02-SharedCoordinate", 0.0, 0.0, 0.0),
                At("ST", "SWLS-02-SharedCoordinate", 0.0, 0.0, 0.0)
            };

            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOn, ATestRuns), Is.Null);
            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOff, ATestRuns), Is.Null);
            Assert.That(
                Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOn, true)),
                Does.Not.Contain("THIS GROUP IS FAILED"));
        }

        /// <summary>
        /// The case that must NOT fail, and it is the one most of C02 is in. Models on
        /// different REAL shared sites are reported, with the difference in X, Y and Z,
        /// and the group runs, because different named sites can still be the same
        /// coordinates and only a person can say. Under a metre apart, so not skipped either.
        /// </summary>
        [Test]
        public void AGroupWhoseModelsNameDifferentRealSitesIsReportedAndNotFailed()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "SWLS-02-SharedCoordinate", 0.0, 0.0, 0.0),
                At("EL", "LTB2", 0.0, 0.0, 95.0),
                At("ME", "PW3_Shared_Location", 0.0, 0.0, 5.0)
            };

            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOn, ATestRuns), Is.Null, "different real sites is not a failure");

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOn, true));

            Assert.That(block, Does.Contain("this group names 3 different shared coordinates"));
            Assert.That(block, Does.Contain("DIFFERENT"));
            Assert.That(block, Does.Not.Contain("THIS GROUP IS FAILED"));
            Assert.That(block, Does.Not.Contain("CLASH SKIPPED"));
        }

        /// <summary>
        /// Q70's other half stays as it was. Bader's answer named Internal and the distance,
        /// and a model naming no site at all is neither, so it still fails its group with
        /// the rule on or off.
        /// </summary>
        [Test]
        public void AModelNamingNoSiteAtAllAlsoFailsTheGroup()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "A site", 0.0, 0.0, 0.0),
                At("ST", string.Empty, 0.0, 0.0, 0.0)
            };

            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOn, ATestRuns), Does.Contain("1 model(s) name no shared site at all"));
            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOff, ATestRuns), Does.Contain("1 model(s) name no shared site at all"));
            Assert.That(AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres).Any, Is.False);
        }

        /// <summary>
        /// With the rule on, a group failed on a model naming no site may also have its clash
        /// skipped, and then no report is written, so the reason claims only the NWF and the
        /// NWD, which are written either way.
        /// </summary>
        [Test]
        public void WithTheRuleOnAFailedGroupClaimsOnlyItsNwfAndItsNwd()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "A site", 0.0, 0.0, 0.0),
                At("EL", "Internal", 0.0, 0.0, 0.0),
                At("ST", string.Empty, 0.0, 0.0, 0.0)
            };

            string why = AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOn, ATestRuns);

            Assert.That(why, Does.EndWith("Its NWF and its NWD were still written, so the evidence is there to send."));
            Assert.That(why, Does.Not.Contain("Every output"));
            Assert.That(AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres).Any, Is.True);
        }

        /// <summary>
        /// FR-006, the real 1B06M1 of the C06 run, log lines 2489 and 2774: its ST names
        /// Internal and sits 72.5 mm above the ME reference, all of it in height, and the
        /// group FAILED on the name alone while the placement agreed. Its clash is skipped
        /// now and it is not failed.
        /// </summary>
        [Test]
        public void TheInternalModelOf1B06M1SkipsTheClashAndNoLongerFailsTheGroup()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                Real("1104-PAR-1B06M1-ZZZ-ME-MOD-000001.nwc", "ME", "a shared site", 0.0, 0.0, 0.0),
                Real("1104-PAR-1B06M1-ZZZ-ST-MOD-000001.nwc", "ST", "Internal", 0.0, 0.0, 72.5)
            };

            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOn, ATestRuns), Is.Null);
            Assert.That(off.Models.Count, Is.EqualTo(1));
            Assert.That(off.Models[0], Does.StartWith(
                "ST  1104-PAR-1B06M1-ZZZ-ST-MOD-000001.nwc   shared site \"Internal\", Revit's own origin and not a"
                + " shared site   X 0 mm  Y 0 mm  Z 72.5 mm from the reference, "));
            Assert.That(off.Models[0], Does.Not.Contain("more than"), "it is listed for its site, not its distance");
        }

        /// <summary>
        /// The real 1B06BC, log lines 583 and 584: its AR reference itself names Internal,
        /// and its EL sits 2,774 km away. Both are named, the reference as itself.
        /// </summary>
        [Test]
        public void AReferenceOnTheInternalOriginIsNamedAsTheReferenceItself()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                Real("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc", "AR", "Internal", 0.0, 0.0, 0.0),
                Real("1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc", "EL", "SITEWIDE PHASE 3",
                    323886396.13, 2755364306.53, 11033.3)
            };

            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOn, ATestRuns), Is.Null);
            Assert.That(off.Models.Count, Is.EqualTo(2));
            Assert.That(off.Models[0], Is.EqualTo(
                "AR  1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc   shared site \"Internal\", Revit's own origin and not a"
                + " shared site   the reference model itself"));
            Assert.That(off.Models[1], Does.StartWith("EL  1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc   shared site \"SITEWIDE PHASE 3\""));
        }

        /// <summary>The real 1B06PS ST, log line 5514: named Internal AND 2,822 km away, said in one line.</summary>
        [Test]
        public void AModelOnInternalAndFarAwayIsOneLineSayingBoth()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                Real("1104-PAR-1B06PS-ZZZ-AR-MOD-000001.nwc", "AR", "a shared site", 0.0, 0.0, 0.0),
                Real("1104-PAR-1B06PS-ZZZ-ST-MOD-000001.nwc", "ST", "Internal",
                    -660063927.46, -2744376692.73, -675666.8)
            };

            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(off.Models.Count, Is.EqualTo(1));
            Assert.That(off.Models[0], Is.EqualTo(
                "ST  1104-PAR-1B06PS-ZZZ-ST-MOD-000001.nwc   shared site \"Internal\", Revit's own origin and not a"
                + " shared site   X -660063927.46 mm  Y -2744376692.73 mm  Z -675666.8 mm from the reference,"
                + " 2822638.531 m in a straight line, more than 1 m"));
        }

        /// <summary>
        /// Where no model could be placed the block stops before the distances, and a model
        /// on Internal still skips the clash, so the block still says so, and says why the
        /// group failed where it did, rather than leaving the reason to the engine alone.
        /// </summary>
        [Test]
        public void AGroupWhereNoModelCouldBePlacedStillSaysTheClashIsSkippedForAModelOnInternal()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "A site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead),
                At("ST", "Internal", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead)
            };

            string on = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOn, true));
            string off = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOff, true));

            Assert.That(on, Does.Contain("no model in this group could be placed"));
            Assert.That(on, Does.Contain("CLASH SKIPPED. 1 model(s) are not on the same shared coordinates"));
            Assert.That(on, Does.Contain("distance from the reference UNKNOWN, its placement could not be read"));
            Assert.That(off, Does.Contain("THIS GROUP IS FAILED. 1 model(s) were exported on Revit's internal origin"));
        }

        /// <summary>A model on Internal whose placement could not be read is still named, by its site, and its distance is UNKNOWN.</summary>
        [Test]
        public void AModelOnInternalWhosePlacementCouldNotBeReadIsNamedWithItsDistanceUnknown()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "A site", 0.0, 0.0, 0.0),
                At("ST", "Internal", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead)
            };

            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(off.Models.Count, Is.EqualTo(1));
            Assert.That(off.Models[0], Does.EndWith(
                "shared site \"Internal\", Revit's own origin and not a shared site   distance from the reference"
                + " UNKNOWN, its placement could not be read"));
        }

        // ---------- Q98 B2 and Bader's answer to Q99 and Q100, the clash is skipped ----------

        private static ModelPlacement Real(string file, string discipline, string site, double x, double y, double z)
        {
            return new ModelPlacement(file, discipline, site, x, y, z);
        }

        /// <summary>
        /// The real 1B06K1 of the C06 run of 2026-10-01, log line 1834. Its ST model sits
        /// 2,823 km from the AR reference and the group still ended DONE, log line 2128, so
        /// its clash count read as whole. The numbers are that line's dx, dy and dz.
        /// </summary>
        private static IList<ModelPlacement> TheReal1B06K1()
        {
            return new List<ModelPlacement>
            {
                Real("1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc", "AR", "COMMUNITY 4A", 0.0, 0.0, 0.0),
                Real("1104-PAR-1B06K1-ZZZ-ME-MOD-000001.nwc", "ME", "16-S01", 7.35, -19.52, 0.0),
                Real("1104-PAR-1B06K1-ZZZ-ST-MOD-000001.nwc", "ST", "COMMUNITY 4A",
                    -658144882.33, -2746014844.6, -683828.41)
            };
        }

        /// <summary>
        /// Bader's answer of 2026-10-04: a model more than the setting from the reference in a
        /// straight line is not on the same shared coordinates, and its line names the file,
        /// its shared site and its distance from the reference in X, Y and Z.
        /// </summary>
        [Test]
        public void AModelMoreThanTheSettingFromTheReferenceIsNamedWithItsSiteAndItsDistanceInXYAndZ()
        {
            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(TheReal1B06K1(), AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(off.Models.Count, Is.EqualTo(1), "the ME at 21 mm is on the same coordinates, the ST at 2,823 km is not");
            Assert.That(off.Models[0], Is.EqualTo(
                "ST  1104-PAR-1B06K1-ZZZ-ST-MOD-000001.nwc   shared site \"COMMUNITY 4A\"   X -658144882.33 mm  Y -2746014844.6 mm"
                + "  Z -683828.41 mm from the reference, 2823783.398 m in a straight line, more than 1 m"));
            Assert.That(off.Reference, Is.EqualTo("AR  1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc"));
        }

        /// <summary>
        /// The heading said "so the clash tests are created and none is run", which F77 makes
        /// false wherever a side finds nothing, while the note, the list and RESULT said it in
        /// the CLASH line's words, the reviewer's and the breaker's note on attempt 2. All of
        /// them read the one sentence now, OffCoordinates.TestsCreatedNoneRun.
        /// </summary>
        [Test]
        public void WithTheRuleOnTheBlockSaysTheClashIsSkippedAndCarriesTheSameLines()
        {
            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(TheReal1B06K1(), AlignmentCheck.DefaultFarModelMillimetres);
            string block = Joined(AlignmentCheck.Lines(TheReal1B06K1(), AlignmentCheck.DefaultFarModelMillimetres, true, true));

            Assert.That(block, Does.Contain(
                "CLASH SKIPPED. 1 model(s) are not on the same shared coordinates, so the clash is skipped. The tests"
                + " whose sides both find something are created and none is run, and no viewpoint and no clash report is"
                + " made:"));
            Assert.That(block, Does.Contain(OffCoordinates.TestsCreatedNoneRun));
            Assert.That(block, Does.Not.Contain("so the clash tests are created"));
            Assert.That(block, Does.Contain("\n   " + off.Models[0]), "one rule writes the line, and the block, the note and the list carry it");
        }

        /// <summary>
        /// The rule can be switched off, because a building is run once more with it off so
        /// every other fix is proved on groups that clash. The block still names the models.
        /// </summary>
        [Test]
        public void WithTheRuleOffTheBlockStillNamesThemAndSaysTheGroupIsClashed()
        {
            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(TheReal1B06K1(), AlignmentCheck.DefaultFarModelMillimetres);
            string block = Joined(AlignmentCheck.Lines(TheReal1B06K1(), AlignmentCheck.DefaultFarModelMillimetres, false, true));

            Assert.That(block, Does.Not.Contain("CLASH SKIPPED"));
            Assert.That(block, Does.Contain(
                "1 model(s) are not on the same shared coordinates. The rule that skips the clash for them is off for"
                + " this run, so the group is clashed as before:"));
            Assert.That(block, Does.Contain("\n   " + off.Models[0]));
        }

        [Test]
        public void TheOneMetreIsTheDefaultAndTheRuleIsOn()
        {
            Assert.That(AlignmentCheck.DefaultFarModelMillimetres, Is.EqualTo(1000.0));
            Assert.That(AlignmentCheck.DefaultSkipClashOffCoordinates, Is.True);
        }

        /// <summary>
        /// A building is run once more with the rule off, so the switch is a tick box on the
        /// Clash step a person or the driver can reach, and not a setting only a build can
        /// change. Its label is at most eight plain words with no capitals for emphasis.
        /// </summary>
        [Test]
        public void TheTickBoxLabelIsEightPlainWordsAtMost()
        {
            Assert.That(AlignmentCheck.TickLabel, Is.EqualTo("Skip clash when models sit off shared coordinates"));
            Assert.That(AlignmentCheck.TickLabel.Split(' ').Length, Is.LessThanOrEqualTo(8));
            Assert.That(AlignmentCheck.TickLabel, Does.Not.Contain("SkipClash"), "no code identifier in a label");
        }

        /// <summary>
        /// The grey line is at most twelve words and carries the distance read off the
        /// setting, never typed into the window as a second copy that would drift.
        /// </summary>
        [Test]
        public void TheGreyLineIsTwelveWordsAtMostAndCarriesTheDistanceOffTheSetting()
        {
            Assert.That(
                AlignmentCheck.HelpLine(AlignmentCheck.DefaultFarModelMillimetres),
                Is.EqualTo("Internal site or over 1 m away. NWF and NWD still made"));
            Assert.That(AlignmentCheck.HelpLine(AlignmentCheck.DefaultFarModelMillimetres).Split(' ').Length, Is.LessThanOrEqualTo(12));
            Assert.That(AlignmentCheck.HelpLine(2500.0), Does.Contain("over 2.5 m away"));
        }

        /// <summary>The proof the fix list asks for: a model at 0.9 m gives no line.</summary>
        [Test]
        public void AModelUnderTheSettingIsOnTheSameCoordinates()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", 0.0, 0.0, 0.0),
                At("ME", "Site", 900.0, 0.0, 0.0)
            };

            Assert.That(AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres).Any, Is.False);
            Assert.That(
                Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, true, true)),
                Does.Contain("no model names \"Internal\" or sits more than 1 m from the reference model in a straight line"),
                "a check that found nothing says so, because a missing line reads as a check that did not run");
        }

        /// <summary>More than the setting is what was asked, so exactly a metre is not more.</summary>
        [Test]
        public void ExactlyTheSettingIsNotMoreThanIt()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", 0.0, 0.0, 0.0),
                At("ST", "Site", 0.0, 0.0, 1000.0)
            };

            Assert.That(AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres).Any, Is.False);
        }

        /// <summary>
        /// THE STRAIGHT LINE AND NOT EACH AXIS. The real 1B06WM, log line 6838: its ME sits
        /// under a metre on every axis and 1.206 m away, and a rule reading each axis on its
        /// own would call it in place.
        /// </summary>
        [Test]
        public void TheDistanceIsTheStraightLineOfDxDyAndDz()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                Real("1104-PAR-1B06WM-ZZZ-AR-MOD-000001.nwc", "AR", "PW3_Shared_Location", 0.0, 0.0, 0.0),
                Real("1104-PAR-1B06WM-ZZZ-ME-MOD-000001.nwc", "ME", "PW3_Shared_Location", 996.2, 663.15, 150.0)
            };

            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(off.Models.Count, Is.EqualTo(1));
            Assert.That(off.Models[0], Does.Contain("X 996.2 mm  Y 663.15 mm  Z 150 mm from the reference, 1.206 m in a straight line, more than 1 m"));
        }

        /// <summary>The number is the setting the run hands in and not a constant read behind its back.</summary>
        [Test]
        public void TheSettingDecidesAndNotAConstant()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", 0.0, 0.0, 0.0),
                At("EL", "Site", 1500.0, 0.0, 0.0)
            };

            Assert.That(AlignmentCheck.NotOnTheSameCoordinates(models, 1000.0).Models.Count, Is.EqualTo(1));
            Assert.That(AlignmentCheck.NotOnTheSameCoordinates(models, 2000.0).Any, Is.False);
            Assert.That(AlignmentCheck.NotOnTheSameCoordinates(models, 1000.0).Models[0], Does.EndWith("more than 1 m"));
            Assert.That(
                Joined(AlignmentCheck.Lines(models, 2000.0, true, true)),
                Does.Contain("no model names \"Internal\" or sits more than 2 m from the reference model"));
        }

        /// <summary>
        /// A model whose placement could not be read is not judged on its distance, the same
        /// way it is never called different, and the block says so rather than going quiet.
        /// </summary>
        [Test]
        public void AModelThatCouldNotBePlacedIsNotJudgedOnItsDistanceAndTheBlockSaysSo()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", 0.0, 0.0, 0.0),
                At("ST", "Site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead)
            };

            Assert.That(AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres).Any, Is.False);
            Assert.That(
                Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, true, true)),
                Does.Contain("NOT READ, its placement could not be read, so it is not compared and is not called a far model"));
        }

        [Test]
        public void WhenNoModelCouldBePlacedNoneIsJudgedOnDistanceAndTheBlockSaysSo()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead),
                At("ST", "Site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead)
            };

            Assert.That(AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres).Any, Is.False);
            Assert.That(
                Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, true, true)),
                Does.Contain("so the models were not compared and none is called a far model"));
        }

        /// <summary>
        /// The real 1B06BC, log lines 583 and 584. Its EL sits 2,774 km from the AR
        /// reference, which the FAILED reason of the C06 run never named.
        /// </summary>
        [Test]
        public void TheFarModelOf1B06BCIsNamed()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                Real("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc", "AR", "Internal", 0.0, 0.0, 0.0),
                Real("1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc", "EL", "SITEWIDE PHASE 3",
                    323886396.13, 2755364306.53, 11033.3)
            };

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, true, true));

            Assert.That(block, Does.Contain(
                "   EL  1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc   shared site \"SITEWIDE PHASE 3\"   X 323886396.13 mm"
                + "  Y 2755364306.53 mm  Z 11033.3 mm from the reference, 2774335.03 m in a straight line, more than 1 m"));
        }

        // ---------- a site that could not be read is not a model naming no site ----------

        /// <summary>
        /// A read of the site that threw came back as an empty site, which the Q70 rule
        /// reads as a model naming no site at all, so the group FAILED with a reason that
        /// read as a fact about the model and nothing said the read threw. A site that was
        /// not read is UNKNOWN, the way a placement that was not read is, and fails nothing.
        /// </summary>
        [Test]
        public void ASiteThatCouldNotBeReadDoesNotFailTheGroupAsNamingNoSite()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "A site", 0.0, 0.0, 0.0),
                At("ST", ModelPlacement.SiteNotRead, 0.0, 0.0, 0.0)
            };

            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOn, ATestRuns), Is.Null);
            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOff, ATestRuns), Is.Null);
        }

        [Test]
        public void TheBlockSaysUnknownForASiteThatCouldNotBeRead()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "A site", 0.0, 0.0, 0.0),
                At("ST", ModelPlacement.SiteNotRead, 0.0, 0.0, 0.0)
            };

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, AlignmentCheck.DefaultSkipClashOffCoordinates, true));

            Assert.That(block, Does.Contain("ST  1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc   same placement, shared coordinate UNKNOWN"));
            Assert.That(block, Does.Not.Contain("NO shared coordinate on the model at all"));
            Assert.That(block, Does.Not.Contain("THIS GROUP IS FAILED"));
        }

        /// <summary>The two answers stay apart: a site read as empty still fails the group, Q70.</summary>
        [Test]
        public void ASiteNotReadAndASiteReadEmptyAreTwoDifferentThings()
        {
            ModelPlacement notRead = At("ST", ModelPlacement.SiteNotRead, 0.0, 0.0, 0.0);
            ModelPlacement none = At("ST", string.Empty, 0.0, 0.0, 0.0);

            Assert.That(notRead.SiteRead, Is.False);
            Assert.That(none.SiteRead, Is.True);
            Assert.That(notRead.NamesASharedCoordinate, Is.False);
            Assert.That(none.NamesASharedCoordinate, Is.False);

            Assert.That(
                AlignmentCheck.WhyItFailsTheGroup(new List<ModelPlacement> { At("AR", "A site", 0.0, 0.0, 0.0), none }, AlignmentCheck.DefaultFarModelMillimetres, RuleOn, ATestRuns),
                Does.Contain("1 model(s) name no shared site at all"));
        }

        [Test]
        public void WithTheRuleOffAModelOnInternalStillFailsTheGroupBesideASiteThatCouldNotBeRead()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Internal", 0.0, 0.0, 0.0),
                At("ST", ModelPlacement.SiteNotRead, 0.0, 0.0, 0.0)
            };

            string why = AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, RuleOff, ATestRuns);

            Assert.That(why, Does.Contain("1 model(s) were exported on Revit's internal origin"));
            Assert.That(why, Does.Contain("AR  1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc"));
            Assert.That(why, Does.Not.Contain("name no shared site"));
            Assert.That(why, Does.Not.Contain("ST  1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc"));
        }

        /// <summary>The row file carried the same false claim, so its words come from the same rule.</summary>
        [Test]
        public void TheRowFileNamesTheSiteOrSaysWhyThereIsNone()
        {
            Assert.That(AlignmentCheck.SiteName(At("ST", "PW3_Shared_Location", 0.0, 0.0, 0.0)), Is.EqualTo("PW3_Shared_Location"));
            Assert.That(AlignmentCheck.SiteName(At("ST", string.Empty, 0.0, 0.0, 0.0)), Is.EqualTo("no shared coordinate on the model"));
            Assert.That(
                AlignmentCheck.SiteName(At("ST", ModelPlacement.SiteNotRead, 0.0, 0.0, 0.0)),
                Is.EqualTo("UNKNOWN, the site could not be read"));
        }

        // ---------- the all clear line, the breaker's eighth finding at c5d8aa8 ----------

        /// <summary>
        /// The all clear line spoke for every model while one was NOT READ two lines above
        /// it. It is not written while a model was not measured, and the line that stands in
        /// for it says how many were not.
        /// </summary>
        [Test]
        public void AModelNotMeasuredMeansNoAllClearLineAndTheBlockSaysHowMany()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", 0.0, 0.0, 0.0),
                At("ST", "Site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead)
            };

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, true, true));

            Assert.That(block, Does.Not.Contain("no model names \"Internal\" or sits more than"));
            Assert.That(block, Does.Contain(
                "no model that could be measured names \"Internal\" or sits more than 1 m from the reference model in a"
                + " straight line, and 1 model(s) were not measured, a placement or a site UNKNOWN, so nothing is said"
                + " about them"));
        }

        /// <summary>A site that could not be read could be Internal, so its model is not measured either.</summary>
        [Test]
        public void AModelWhoseSiteWasNotReadIsNotMeasuredEither()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "A site", 0.0, 0.0, 0.0),
                At("ST", ModelPlacement.SiteNotRead, 0.0, 0.0, 0.0)
            };

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, true, true));

            Assert.That(block, Does.Not.Contain("no model names \"Internal\" or sits more than"));
            Assert.That(block, Does.Contain("and 1 model(s) were not measured, a placement or a site UNKNOWN"));
        }

        /// <summary>A model named off beside one that was not measured: the off one is named and the count is said.</summary>
        [Test]
        public void AModelOffBesideOneNotMeasuredSaysBoth()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", 0.0, 0.0, 0.0),
                At("ST", "Site", 0.0, 0.0, 5000.0),
                At("EL", "Site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead)
            };

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, true, true));

            Assert.That(block, Does.Contain("CLASH SKIPPED. 1 model(s) are not on the same shared coordinates"));
            Assert.That(block, Does.Contain(
                "1 model(s) were not measured, a placement or a site UNKNOWN, so nothing is said about them"));
        }

        // ---------- a group with nothing to clash, the breaker's second finding at c5d8aa8 ----------

        /// <summary>
        /// A group where this run runs no clash test is judged as before. Its block still
        /// names the models not on the same shared coordinates, and never says CLASH SKIPPED.
        /// </summary>
        [Test]
        public void AGroupWithNothingToClashNamesItsOffModelsAndSkipsNothing()
        {
            foreach (bool rule in new[] { true, false })
            {
                string block = Joined(AlignmentCheck.Lines(TheReal1B06K1(), AlignmentCheck.DefaultFarModelMillimetres, rule, false));

                Assert.That(block, Does.Not.Contain("CLASH SKIPPED"));
                Assert.That(block, Does.Not.Contain("so the group is clashed as before"));
                Assert.That(block, Does.Contain(
                    "1 model(s) are not on the same shared coordinates. This run runs no clash test in this group, so"
                    + " no clash is skipped for them:"));
                Assert.That(block, Does.Contain("ST  1104-PAR-1B06K1-ZZZ-ST-MOD-000001.nwc   shared site \"COMMUNITY 4A\""));
            }
        }

        // ---------- a model on Internal where no clash is skipped, F112 attempt 3 ----------

        /// <summary>
        /// The blocking finding of both readings of attempt 2. With the rule on, a model on
        /// Internal stopped failing its group whatever the run did, and the skip needed a
        /// clash test to run, so a group with nothing to clash got neither Q70's FAILED nor
        /// Bader's PARTIAL and ended DONE. Wherever no clash is skipped, a model on Internal
        /// fails its group as Q70 answered, the rule on or off. Each case reads whether a test
        /// runs from ClashWork.RunsATest, the call the engine makes, and the group's end from
        /// GroupJudgement, which the engine hands its facts to.
        /// </summary>
        private static void AssertFailedOnInternal(IList<ModelPlacement> models, bool runsATest, string onInternal)
        {
            Assert.That(runsATest, Is.False, "this case runs no clash test in the group");

            foreach (bool rule in new[] { RuleOn, RuleOff })
            {
                string why = AlignmentCheck.WhyItFailsTheGroup(models, AlignmentCheck.DefaultFarModelMillimetres, rule, runsATest);
                Assert.That(why, Does.Contain("1 model(s) were exported on Revit's internal origin"), "rule " + rule);
                Assert.That(why, Does.Contain(onInternal), "rule " + rule);

                string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres, rule, runsATest));
                Assert.That(block, Does.Contain("THIS GROUP IS FAILED. 1 model(s) were exported on Revit's internal origin"), "rule " + rule);
                Assert.That(block, Does.Not.Contain("CLASH SKIPPED"), "rule " + rule);

                OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres);
                GroupFacts facts = new GroupFacts();
                facts.NwfOnDisk = true;
                facts.AddError(why);
                facts.ClashSkippedOffCoordinates = off.SkipsTheClash(rule, runsATest);

                string reason;
                Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Failed), "rule " + rule);
                Assert.That(reason, Does.Contain("internal origin"), "rule " + rule);
            }
        }

        [Test]
        public void AModelOnInternalFailsItsGroupWithNoXmlAndNoSavedTest()
        {
            AssertFailedOnInternal(
                TheRealGroup(),
                ClashWork.RunsATest(ClashWork.SourceFor(null, 0), null, false),
                "ST  1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc");
        }

        [Test]
        public void AModelOnInternalFailsItsGroupWithAnXmlOfSetsAlone()
        {
            ExchangeDocument setsOnly = new ExchangeReader().ReadFile(Samples.Infra());

            AssertFailedOnInternal(
                TheRealGroup(),
                ClashWork.RunsATest(ClashWork.SourceFor(setsOnly, 0), setsOnly, false),
                "ST  1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc");
        }

        [Test]
        public void AModelOnInternalFailsAGroupOfOneDiscipline()
        {
            ExchangeDocument both = new ExchangeReader().ReadFile(Samples.AllInOne());
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                new ModelPlacement("1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc", "ST", "SWLS-02-SharedCoordinate", 0.0, -13419.0, -509.0),
                new ModelPlacement("1104-PAR-1A02MM-ZZZ-ST-MOD-000002.nwc", "ST", "Internal", 0.0, -13419.0, -254.0)
            };

            AssertFailedOnInternal(
                models,
                ClashWork.RunsATest(ClashWork.SourceFor(both, 0), both, BuildingGroup.CannotClashWith(1)),
                "ST  1104-PAR-1A02MM-ZZZ-ST-MOD-000002.nwc");
        }

        [Test]
        public void AModelOnInternalFailsAGroupOfOneNwc()
        {
            ExchangeDocument both = new ExchangeReader().ReadFile(Samples.AllInOne());
            IList<ModelPlacement> models = new List<ModelPlacement> { At("ST", "Internal", 0.0, -13419.0, -254.0) };

            AssertFailedOnInternal(
                models,
                ClashWork.RunsATest(ClashWork.SourceFor(both, 0), both, BuildingGroup.CannotClashWith(1)),
                "ST  1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc");
        }

        /// <summary>
        /// The same group where a test would run skips its clash and is not failed, so the
        /// two outcomes Bader's words allow are the only two it can reach.
        /// </summary>
        [Test]
        public void TheSameGroupWithATestToRunSkipsItsClashAndIsNotFailed()
        {
            ExchangeDocument both = new ExchangeReader().ReadFile(Samples.AllInOne());
            bool runsATest = ClashWork.RunsATest(ClashWork.SourceFor(both, 0), both, false);

            Assert.That(runsATest, Is.True);
            Assert.That(AlignmentCheck.WhyItFailsTheGroup(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres, RuleOn, runsATest), Is.Null);
            Assert.That(
                AlignmentCheck.NotOnTheSameCoordinates(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres).SkipsTheClash(RuleOn, runsATest),
                Is.True);
        }

        /// <summary>
        /// The ALIGNMENT failed run line said, with the rule on, that each group failed
        /// because a model names no site, which a group failed on Internal with nothing to
        /// clash makes false. It names both causes whichever way the rule is set, and says
        /// what the files written list shows rather than that every NWD was written.
        /// </summary>
        [Test]
        public void TheFailedRunLineNamesBothCausesAndClaimsNoFile()
        {
            Assert.That(AlignmentCheck.FailedRunLine(2), Is.EqualTo(
                "ALIGNMENT failed 2 group(s), each because a model names no shared site, or was exported on the internal"
                + " origin in a group whose clash was not skipped. A failed group still goes on to its NWF and its NWD,"
                + " and the files written list says which were written."));
            Assert.That(AlignmentCheck.FailedRunLine(0), Is.EqualTo(
                "ALIGNMENT failed 0 group(s), each because a model names no shared site, or was exported on the internal"
                + " origin in a group whose clash was not skipped"));
        }
    }
}
