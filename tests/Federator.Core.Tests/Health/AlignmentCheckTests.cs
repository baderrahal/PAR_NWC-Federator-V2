using System.Collections.Generic;
using Federator.Core.Health;
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
            string block = Joined(AlignmentCheck.Lines(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres));

            Assert.That(block, Does.Contain("AR  1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc is the reference"));
            Assert.That(block, Does.Contain("reference, shared coordinate \"SWLS-02-SharedCoordinate\""));
        }

        [Test]
        public void AModelAtADifferentHeightIsNamedWithTheDifferenceInXYAndZSeparately()
        {
            string block = Joined(AlignmentCheck.Lines(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres));

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
            string block = Joined(AlignmentCheck.Lines(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres));

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

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres));

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

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres));

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

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres));

            Assert.That(block, Does.Contain("NOT READ, its placement could not be read"));
            Assert.That(block, Does.Not.Contain("DIFFERENT"));
            Assert.That(block, Does.Contain("1 could not be placed and were not compared"));
            Assert.That(AlignmentCheck.DifferentCount(models, AlignmentCheck.DefaultToleranceMillimetres), Is.EqualTo(0));
        }

        [Test]
        public void AModelCarryingNoSharedCoordinateIsSentToBeCheckedByEyeWhichIsWhatQ64Asks()
        {
            IList<ModelPlacement> models = new List<ModelPlacement> { At("AR", string.Empty, 0.0, 0.0, 0.0) };

            Assert.That(Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres)),
                Does.Contain("NO shared coordinate on the model at all, so check this one by eye"));
        }

        [Test]
        public void WithNoModelAtAllItSaysSoRatherThanWritingAnEmptyBlock()
        {
            Assert.That(Joined(AlignmentCheck.Lines(new List<ModelPlacement>(), AlignmentCheck.DefaultFarModelMillimetres)),
                Does.Contain("no model was read, so nothing could be compared"));
        }

        [Test]
        public void TheDifferenceCountIsTheRunLineAndCountsOnlyWhatMoved()
        {
            Assert.That(
                AlignmentCheck.DifferentCount(TheRealGroup(), AlignmentCheck.DefaultToleranceMillimetres),
                Is.EqualTo(3), "three of the four sit somewhere the architecture does not");
        }

        // ---------- Q70, a model on the internal origin fails its group ----------

        /// <summary>
        /// 1A02MM's real shape on 2026-09-20: four models, one of them on Internal. The
        /// group is FAILED and the block says which model and why.
        /// </summary>
        [Test]
        public void AGroupWithAModelOnTheInternalOriginIsFailedAndTheBlockSaysWhich()
        {
            string why = AlignmentCheck.WhyItFailsTheGroup(TheRealGroup());

            Assert.That(why, Is.Not.Null);
            Assert.That(why, Does.Contain("1 model(s) were exported on Revit's internal origin"));
            Assert.That(why, Does.Contain("ST  1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc"));

            Assert.That(Joined(AlignmentCheck.Lines(TheRealGroup(), AlignmentCheck.DefaultFarModelMillimetres)), Does.Contain("THIS GROUP IS FAILED"));
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
                AlignmentCheck.WhyItFailsTheGroup(TheRealGroup()),
                Does.Contain("Every output of this group was still written"));
        }

        [Test]
        public void AGroupWithEveryModelOnOneNamedSiteIsNotFailed()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "SWLS-02-SharedCoordinate", 0.0, 0.0, 0.0),
                At("ST", "SWLS-02-SharedCoordinate", 0.0, 0.0, 0.0)
            };

            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models), Is.Null);
            Assert.That(Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres)), Does.Not.Contain("THIS GROUP IS FAILED"));
        }

        /// <summary>
        /// The case that must NOT fail, and it is the one most of C02 is in. Models on
        /// different REAL shared sites are reported, with the difference in X, Y and Z,
        /// and the group runs, because different named sites can still be the same
        /// coordinates and only a person can say.
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

            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models), Is.Null, "different real sites is not a failure");

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres));

            Assert.That(block, Does.Contain("this group names 3 different shared coordinates"));
            Assert.That(block, Does.Contain("DIFFERENT"));
            Assert.That(block, Does.Not.Contain("THIS GROUP IS FAILED"));
        }

        [Test]
        public void AModelNamingNoSiteAtAllAlsoFailsTheGroup()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "A site", 0.0, 0.0, 0.0),
                At("ST", string.Empty, 0.0, 0.0, 0.0)
            };

            string why = AlignmentCheck.WhyItFailsTheGroup(models);

            Assert.That(why, Is.Not.Null);
            Assert.That(why, Does.Contain("1 model(s) name no shared site at all"));
        }

        // ---------- Q98 B2, a model far from its reference keeps the group from DONE ----------

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

        [Test]
        public void AModelMoreThanTheSettingFromTheReferenceGetsOneLineNamingItTheDistanceAndTheTrust()
        {
            IList<string> far = AlignmentCheck.FarModels(TheReal1B06K1(), AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(far.Count, Is.EqualTo(1), "the ME at 21 mm is not far, the ST at 2,823 km is");
            Assert.That(far[0], Is.EqualTo(
                "ST  1104-PAR-1B06K1-ZZZ-ST-MOD-000001.nwc sits 2823783.398 m from the reference model"
                + " in a straight line, more than 1 m, so its clashes with the other disciplines cannot be trusted"));
        }

        [Test]
        public void TheBlockCarriesTheSameLineUnderAHeadingThatCountsThem()
        {
            IList<string> far = AlignmentCheck.FarModels(TheReal1B06K1(), AlignmentCheck.DefaultFarModelMillimetres);
            string block = Joined(AlignmentCheck.Lines(TheReal1B06K1(), AlignmentCheck.DefaultFarModelMillimetres));

            Assert.That(block, Does.Contain(
                "1 model(s) sit more than 1 m from the reference model in a straight line, which keeps this group from DONE:"));
            Assert.That(block, Does.Contain("\n   " + far[0]), "one rule writes the line, and the block and the reason are that line");
        }

        [Test]
        public void TheOneMetreIsTheDefault()
        {
            Assert.That(AlignmentCheck.DefaultFarModelMillimetres, Is.EqualTo(1000.0));
        }

        /// <summary>The proof the fix list asks for: a model at 0.9 m gives no line.</summary>
        [Test]
        public void AModelUnderTheSettingGetsNoFarLine()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", 0.0, 0.0, 0.0),
                At("ME", "Site", 900.0, 0.0, 0.0)
            };

            Assert.That(AlignmentCheck.FarModels(models, AlignmentCheck.DefaultFarModelMillimetres), Is.Empty);
            Assert.That(
                Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres)),
                Does.Contain("no model sits more than 1 m from the reference model in a straight line"),
                "a check that found nothing says so, because a missing line reads as a check that did not run");
        }

        /// <summary>More than the setting is what was asked, so exactly a metre is not far.</summary>
        [Test]
        public void ExactlyTheSettingIsNotMoreThanIt()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", 0.0, 0.0, 0.0),
                At("ST", "Site", 0.0, 0.0, 1000.0)
            };

            Assert.That(AlignmentCheck.FarModels(models, AlignmentCheck.DefaultFarModelMillimetres), Is.Empty);
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

            IList<string> far = AlignmentCheck.FarModels(models, AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(far.Count, Is.EqualTo(1));
            Assert.That(far[0], Does.Contain("ME  1104-PAR-1B06WM-ZZZ-ME-MOD-000001.nwc sits 1.206 m from the reference model"));
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

            Assert.That(AlignmentCheck.FarModels(models, 1000.0).Count, Is.EqualTo(1));
            Assert.That(AlignmentCheck.FarModels(models, 2000.0), Is.Empty);
            Assert.That(AlignmentCheck.FarModels(models, 1000.0)[0], Does.Contain("more than 1 m,"));
            Assert.That(
                Joined(AlignmentCheck.Lines(models, 2000.0)),
                Does.Contain("no model sits more than 2 m from the reference model"));
        }

        /// <summary>
        /// A model whose placement could not be read is not a far model, the same way it is
        /// never called different, and the block says so rather than going quiet about it.
        /// </summary>
        [Test]
        public void AModelThatCouldNotBePlacedIsNotAFarModelAndTheBlockSaysSo()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", 0.0, 0.0, 0.0),
                At("ST", "Site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead)
            };

            Assert.That(AlignmentCheck.FarModels(models, AlignmentCheck.DefaultFarModelMillimetres), Is.Empty);
            Assert.That(
                Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres)),
                Does.Contain("NOT READ, its placement could not be read, so it is not compared and is not called a far model"));
        }

        [Test]
        public void WhenNoModelCouldBePlacedNoneIsCalledFarAndTheBlockSaysSo()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead),
                At("ST", "Site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead)
            };

            Assert.That(AlignmentCheck.FarModels(models, AlignmentCheck.DefaultFarModelMillimetres), Is.Empty);
            Assert.That(
                Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres)),
                Does.Contain("so the models were not compared and none is called a far model"));
        }

        /// <summary>
        /// The real 1B06BC, log lines 583 and 584. Its AR reference names Internal, so the
        /// group FAILED, and its EL sits 2,774 km away, which the FAILED reason never named.
        /// A FAILED group stays FAILED and the far line is written for it too.
        /// </summary>
        [Test]
        public void AGroupFailedOnItsSiteStillGetsItsFarLine()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                Real("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc", "AR", "Internal", 0.0, 0.0, 0.0),
                Real("1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc", "EL", "SITEWIDE PHASE 3",
                    323886396.13, 2755364306.53, 11033.3)
            };

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres));

            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models), Is.Not.Null);
            Assert.That(block, Does.Contain("THIS GROUP IS FAILED"));
            Assert.That(block, Does.Contain(
                "EL  1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc sits 2774335.03 m from the reference model"));
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

            Assert.That(AlignmentCheck.WhyItFailsTheGroup(models), Is.Null);
        }

        [Test]
        public void TheBlockSaysUnknownForASiteThatCouldNotBeRead()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "A site", 0.0, 0.0, 0.0),
                At("ST", ModelPlacement.SiteNotRead, 0.0, 0.0, 0.0)
            };

            string block = Joined(AlignmentCheck.Lines(models, AlignmentCheck.DefaultFarModelMillimetres));

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
                AlignmentCheck.WhyItFailsTheGroup(new List<ModelPlacement> { At("AR", "A site", 0.0, 0.0, 0.0), none }),
                Does.Contain("1 model(s) name no shared site at all"));
        }

        [Test]
        public void AModelOnInternalStillFailsTheGroupBesideASiteThatCouldNotBeRead()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                At("AR", "Internal", 0.0, 0.0, 0.0),
                At("ST", ModelPlacement.SiteNotRead, 0.0, 0.0, 0.0)
            };

            string why = AlignmentCheck.WhyItFailsTheGroup(models);

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
    }
}
