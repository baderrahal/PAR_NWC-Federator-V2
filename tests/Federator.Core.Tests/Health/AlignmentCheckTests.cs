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
            string block = Joined(AlignmentCheck.Lines(TheRealGroup()));

            Assert.That(block, Does.Contain("AR  1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc is the reference"));
            Assert.That(block, Does.Contain("reference, shared coordinate \"SWLS-02-SharedCoordinate\""));
        }

        [Test]
        public void AModelAtADifferentHeightIsNamedWithTheDifferenceInXYAndZSeparately()
        {
            string block = Joined(AlignmentCheck.Lines(TheRealGroup()));

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
            string block = Joined(AlignmentCheck.Lines(TheRealGroup()));

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

            string block = Joined(AlignmentCheck.Lines(models));

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

            string block = Joined(AlignmentCheck.Lines(models));

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

            string block = Joined(AlignmentCheck.Lines(models));

            Assert.That(block, Does.Contain("NOT READ, its placement could not be read"));
            Assert.That(block, Does.Not.Contain("DIFFERENT"));
            Assert.That(block, Does.Contain("1 could not be placed and were not compared"));
            Assert.That(AlignmentCheck.DifferentCount(models, AlignmentCheck.DefaultToleranceMillimetres), Is.EqualTo(0));
        }

        [Test]
        public void AModelCarryingNoSharedCoordinateIsSentToBeCheckedByEyeWhichIsWhatQ64Asks()
        {
            IList<ModelPlacement> models = new List<ModelPlacement> { At("AR", string.Empty, 0.0, 0.0, 0.0) };

            Assert.That(Joined(AlignmentCheck.Lines(models)),
                Does.Contain("NO shared coordinate on the model at all, so check this one by eye"));
        }

        [Test]
        public void WithNoModelAtAllItSaysSoRatherThanWritingAnEmptyBlock()
        {
            Assert.That(Joined(AlignmentCheck.Lines(new List<ModelPlacement>())),
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

            Assert.That(Joined(AlignmentCheck.Lines(TheRealGroup())), Does.Contain("THIS GROUP IS FAILED"));
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
            Assert.That(Joined(AlignmentCheck.Lines(models)), Does.Not.Contain("THIS GROUP IS FAILED"));
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

            string block = Joined(AlignmentCheck.Lines(models));

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
    }
}
