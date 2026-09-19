using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F85. The saved viewpoints, in three layers, one per CLASH and not per test.
    ///
    ///     Layer 1   the priority off the matrix, A, B, C or No priority. Dropped when no
    ///               priority file was picked
    ///     Layer 2   the two disciplines, sorted, so AR vs ST and ST vs AR are one folder
    ///     Layer 3   Over 150mm, and only under a pair involving Mechanical or Electrical
    ///
    /// Written by the add-in, ViewpointBuilder, since the viewpoints round on 2026-09-19,
    /// when scan.md 5j measured that a captured viewpoint records its hiding. A planned
    /// viewpoint that was not written is not a viewpoint.
    /// </summary>
    [TestFixture]
    public class ClashViewpointPlanTests
    {
        private static ViewpointSettings Settings()
        {
            return new ViewpointSettings();
        }

        private static ClashToPlan Clash(
            string test, string name, string left, string right,
            ClashStatus status, ClashPriority priority, SizeVerdict? size)
        {
            return new ClashToPlan(test, name, left, right, status, priority, size);
        }

        private static ClashToPlan Simple(string left, string right)
        {
            return Clash("T", "Clash1", left, right, ClashStatus.New, ClashPriority.None, null);
        }

        private static ClashViewpointPlanOutcome Plan(bool priorityPicked, params ClashToPlan[] clashes)
        {
            return ClashViewpointPlan.For(clashes, Settings(), priorityPicked);
        }

        // ---------- layer 2, the discipline pair ----------

        [Test]
        public void TheCodeIsReadOffTheSetName()
        {
            ViewpointSettings settings = Settings();

            Assert.That(DisciplinePairRule.CodeIn("BLD-ME-Ducts", settings), Is.EqualTo("ME"));
            Assert.That(DisciplinePairRule.CodeIn("BLD-AR-Walls", settings), Is.EqualTo("AR"));
            Assert.That(DisciplinePairRule.CodeIn("BLD-EL-Cable Tray&Cable Tray Fittings", settings),
                Is.EqualTo("EL"));
            Assert.That(DisciplinePairRule.CodeIn("BLD-DR-Pipe Accessories", settings),
                Is.EqualTo("DR"));
        }

        /// <summary>
        /// The client's own matrix holds one. BLD-Security Devices breaks the BLD-EL-
        /// pattern its siblings follow, so a set with no code in the usual place is a thing
        /// that really happens.
        /// </summary>
        [Test]
        public void ASetNameWithNoKnownCodeReadsAsNoneAndIsNeverGuessedAt()
        {
            ViewpointSettings settings = Settings();

            Assert.That(DisciplinePairRule.CodeIn("BLD-Security Devices", settings),
                Is.EqualTo(string.Empty));
            Assert.That(DisciplinePairRule.CodeIn("BLD-DRPipe Accessories", settings),
                Is.EqualTo(string.Empty), "the name F87 corrects");
            Assert.That(DisciplinePairRule.CodeIn(null, settings), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ACodeIsMatchedOrdinalAndNeverCased()
        {
            Assert.That(DisciplinePairRule.CodeIn("BLD-me-Ducts", Settings()),
                Is.EqualTo(string.Empty));
        }

        /// <summary>
        /// The break. Without sorting, half the clashes of every pair go in one folder and
        /// half in another, and a person looking for the architecture against structure
        /// work finds it in two places.
        /// </summary>
        [Test]
        public void ArchitectureAgainstStructureIsOneFolderWhicheverWayRound()
        {
            DisciplinePair one = DisciplinePairRule.For("BLD-AR-Walls", "BLD-ST-Columns", Settings());
            DisciplinePair other = DisciplinePairRule.For("BLD-ST-Columns", "BLD-AR-Walls", Settings());

            Assert.That(one.Folder, Is.EqualTo("AR vs ST"));
            Assert.That(other.Folder, Is.EqualTo("AR vs ST"));
            Assert.That(one.BothKnown, Is.True);
        }

        [Test]
        public void APairWithAnUnknownSideSaysSoInItsFolderName()
        {
            DisciplinePair pair = DisciplinePairRule.For(
                "BLD-Security Devices", "BLD-ST-Walls", Settings());

            Assert.That(pair.BothKnown, Is.False);
            Assert.That(pair.Folder, Does.Contain("UNKNOWN"));
            Assert.That(pair.Folder, Does.Contain("ST"));
        }

        // ---------- layer 3, the size folder ----------

        [Test]
        public void OverTheThresholdUnderAMechanicalPairGoesInTheSizeFolder()
        {
            ClashViewpointPlanOutcome outcome = Plan(false,
                Clash("T", "C", "BLD-ME-Ducts", "BLD-AR-Walls",
                    ClashStatus.New, ClashPriority.None, SizeVerdict.Large));

            Assert.That(outcome.Planned.Count, Is.EqualTo(1));
            Assert.That(outcome.Planned[0].SizeFolder, Is.EqualTo("Over 150mm"));
            Assert.That(outcome.Planned[0].Path,
                Is.EqualTo("AR vs ME/Over 150mm/T  C"));
        }

        [Test]
        public void OverTheThresholdUnderAPairWithNoServiceDisciplineHasNoSizeFolder()
        {
            ClashViewpointPlanOutcome outcome = Plan(false,
                Clash("T", "C", "BLD-AR-Walls", "BLD-ST-Columns",
                    ClashStatus.New, ClashPriority.None, SizeVerdict.Large));

            Assert.That(outcome.Planned[0].SizeFolder, Is.Null);
            Assert.That(outcome.Planned[0].Path, Is.EqualTo("AR vs ST/T  C"));
        }

        [Test]
        public void ElectricalCarriesASizeFolderAndArchitectureDoesNot()
        {
            ViewpointSettings settings = Settings();

            Assert.That(DisciplinePairRule.CarriesASizeFolder(
                DisciplinePairRule.For("BLD-EL-Conduits", "BLD-AR-Walls", settings), settings), Is.True);
            Assert.That(DisciplinePairRule.CarriesASizeFolder(
                DisciplinePairRule.For("BLD-AR-Walls", "BLD-ST-Columns", settings), settings), Is.False);
        }

        /// <summary>
        /// Small services stay out. This is F85's OWN size rule and it is not inherited
        /// from F72a: F72a leaves a service against another service exactly as it was, and
        /// F72a is off by default, so a run with that box off would fill this tree with
        /// every small pipe through every wall if the rule read the status instead.
        /// </summary>
        [Test]
        public void AServiceAtOrUnderTheThresholdStaysOutOfTheTree()
        {
            ClashViewpointPlanOutcome outcome = Plan(false,
                Clash("T", "C", "BLD-ME-Pipes", "BLD-AR-Walls",
                    ClashStatus.New, ClashPriority.None, SizeVerdict.Small));

            Assert.That(outcome.Planned, Is.Empty);
            Assert.That(outcome.Of(ViewpointLeftOutReason.SmallService), Is.EqualTo(1));
        }

        /// <summary>
        /// The other half of the same trap. Reviewed is one of the three statuses this tree
        /// carries, so a service F72a DID move is still in scope and is kept out by the
        /// SIZE branch rather than by the status filter.
        /// </summary>
        [Test]
        public void ASmallServiceAlreadyReviewedIsStillKeptOutBySizeAndNotByStatus()
        {
            ClashViewpointPlanOutcome outcome = Plan(false,
                Clash("T", "C", "BLD-ME-Pipes", "BLD-AR-Walls",
                    ClashStatus.Reviewed, ClashPriority.None, SizeVerdict.Small));

            Assert.That(outcome.Planned, Is.Empty);
            Assert.That(outcome.Of(ViewpointLeftOutReason.SmallService), Is.EqualTo(1));
            Assert.That(outcome.Of(ViewpointLeftOutReason.NotOpen), Is.EqualTo(0));
        }

        /// <summary>
        /// Branched on the verdict and never on SizeDecision.Included, which folds Large
        /// and SizeUnknown together for F53's own reasons and would put every fitting with
        /// no size property into Over 150mm.
        /// </summary>
        [Test]
        public void ASizeThatCouldNotBeReadGoesInThePairFolderAndIsCounted()
        {
            ClashViewpointPlanOutcome outcome = Plan(false,
                Clash("T", "C", "BLD-ME-Pipe Fittings", "BLD-AR-Walls",
                    ClashStatus.New, ClashPriority.None, SizeVerdict.SizeUnknown));

            Assert.That(outcome.Planned.Count, Is.EqualTo(1));
            Assert.That(outcome.Planned[0].SizeFolder, Is.Null, "the pair folder, not Over 150mm");
            Assert.That(outcome.SizeUnknownCount, Is.EqualTo(1));
        }

        [Test]
        public void AClashWithNoServiceSideAtAllHasNoSizeBranch()
        {
            ClashViewpointPlanOutcome outcome = Plan(false, Simple("BLD-AR-Doors", "BLD-AR-Walls"));

            Assert.That(outcome.Planned.Count, Is.EqualTo(1));
            Assert.That(outcome.Planned[0].SizeFolder, Is.Null);
            Assert.That(outcome.SizeUnknownCount, Is.EqualTo(0));
        }

        [Test]
        public void TheSizeFolderNameComesFromTheThresholdAndIsNeverTyped()
        {
            ViewpointSettings settings = Settings();
            settings.Sizes.ThresholdMillimetres = 250;

            ClashViewpointPlanOutcome outcome = ClashViewpointPlan.For(
                new[]
                {
                    Clash("T", "C", "BLD-ME-Ducts", "BLD-AR-Walls",
                        ClashStatus.New, ClashPriority.None, SizeVerdict.Large)
                },
                settings,
                false);

            Assert.That(outcome.Planned[0].SizeFolder, Is.EqualTo("Over 250mm"));
        }

        // ---------- layer 1, the priority ----------

        [Test]
        public void WithAPriorityFileTheTreeStartsWithTheLetter()
        {
            ClashViewpointPlanOutcome outcome = Plan(true,
                Clash("T", "C", "BLD-ME-Ducts", "BLD-AR-Walls",
                    ClashStatus.New, ClashPriority.A, SizeVerdict.Large));

            Assert.That(outcome.Planned[0].PriorityFolder, Is.EqualTo("A"));
            Assert.That(outcome.Planned[0].Path, Is.EqualTo("A/AR vs ME/Over 150mm/T  C"));
            Assert.That(outcome.Planned[0].Folders.Count, Is.EqualTo(3));
        }

        [Test]
        public void ATestThePriorityFileSaysNothingAboutGetsAFolderOfItsOwn()
        {
            ClashViewpointPlanOutcome outcome = Plan(true,
                Clash("T", "C", "BLD-AR-Walls", "BLD-ST-Columns",
                    ClashStatus.New, ClashPriority.None, null));

            Assert.That(outcome.Planned[0].PriorityFolder, Is.EqualTo("No priority"),
                "an unnamed folder is not a folder");
            Assert.That(outcome.Planned[0].Path, Is.EqualTo("No priority/AR vs ST/T  C"));
        }

        [Test]
        public void WithNoPriorityFileLayerOneIsDroppedAndTheBlockSaysSo()
        {
            ClashViewpointPlanOutcome outcome = Plan(false,
                Clash("T", "C", "BLD-AR-Walls", "BLD-ST-Columns",
                    ClashStatus.New, ClashPriority.A, null));

            Assert.That(outcome.Planned[0].PriorityFolder, Is.Null);
            Assert.That(outcome.Planned[0].Path, Is.EqualTo("AR vs ST/T  C"));
            Assert.That(outcome.Planned[0].Folders.Count, Is.EqualTo(1));

            string all = string.Join("\n", new List<string>(outcome.Lines()).ToArray());

            Assert.That(all, Does.Contain("NO PRIORITY FILE was picked"));
        }

        [Test]
        public void WithAPriorityFileTheBlockDoesNotSayItIsMissing()
        {
            string all = string.Join("\n", new List<string>(
                Plan(true, Simple("BLD-AR-Walls", "BLD-ST-Columns")).Lines()).ToArray());

            Assert.That(all, Does.Not.Contain("NO PRIORITY FILE"));
        }

        // ---------- which clashes are in scope ----------

        [Test]
        public void OnlyNewActiveAndReviewedGetAViewpoint()
        {
            Assert.That(ClashViewpointPlan.InScope(ClashStatus.New), Is.True);
            Assert.That(ClashViewpointPlan.InScope(ClashStatus.Active), Is.True);
            Assert.That(ClashViewpointPlan.InScope(ClashStatus.Reviewed), Is.True);
            Assert.That(ClashViewpointPlan.InScope(ClashStatus.Approved), Is.False);
            Assert.That(ClashViewpointPlan.InScope(ClashStatus.Resolved), Is.False);
        }

        /// <summary>
        /// The three statuses are read off OpenClashes and never typed here, because that
        /// is Navisworks's own definition of open and the image filter reads the same
        /// place, so the two cannot come to different answers.
        /// </summary>
        [Test]
        public void TheThreeStatusesAreReadOffTheOnePlaceThatNamesThem()
        {
            Assert.That(ClashViewpointPlan.StatusesInScope(),
                Is.EqualTo(OpenClashes.StatusesFor(OpenClashCount.NavisworksOpen)).AsCollection);
        }

        [Test]
        public void AClosedClashIsLeftOutAndCounted()
        {
            ClashViewpointPlanOutcome outcome = Plan(false,
                Clash("T", "C", "BLD-AR-Walls", "BLD-ST-Columns",
                    ClashStatus.Approved, ClashPriority.None, null),
                Clash("T", "D", "BLD-AR-Walls", "BLD-ST-Columns",
                    ClashStatus.Resolved, ClashPriority.None, null));

            Assert.That(outcome.Planned, Is.Empty);
            Assert.That(outcome.Of(ViewpointLeftOutReason.NotOpen), Is.EqualTo(2));
            Assert.That(outcome.Considered, Is.EqualTo(2));
        }

        // ---------- one viewpoint per clash ----------

        [Test]
        public void EveryClashOfATestGetsItsOwnViewpoint()
        {
            ClashViewpointPlanOutcome outcome = Plan(false,
                Simple2("T", "Clash1"), Simple2("T", "Clash2"), Simple2("T", "Clash3"));

            Assert.That(outcome.Planned.Count, Is.EqualTo(3));
        }

        /// <summary>
        /// A clash name is unique only within its TEST, and this tree puts clashes from
        /// many tests into one pair folder. A leaf named after the clash alone would
        /// collide, and then the already there check and the read back both stop meaning
        /// anything.
        /// </summary>
        [Test]
        public void AViewpointNameCarriesTheTestAsWellAsTheClash()
        {
            ClashViewpointPlanOutcome outcome = Plan(false,
                Simple2("Test one", "Clash1"), Simple2("Test two", "Clash1"));

            Assert.That(outcome.Planned[0].Name, Is.EqualTo("Test one  Clash1"));
            Assert.That(outcome.Planned[1].Name, Is.EqualTo("Test two  Clash1"));
            Assert.That(outcome.Planned[0].Path, Is.Not.EqualTo(outcome.Planned[1].Path));
        }

        [Test]
        public void ThePlanKeepsTheOrderTheClashesCameIn()
        {
            ClashViewpointPlanOutcome outcome = Plan(false,
                Simple2("T", "c"), Simple2("T", "a"), Simple2("T", "b"));

            Assert.That(outcome.Planned[0].Name, Does.EndWith("c"));
            Assert.That(outcome.Planned[1].Name, Does.EndWith("a"));
            Assert.That(outcome.Planned[2].Name, Does.EndWith("b"));
        }

        /// <summary>
        /// One artefact per clash across 1830 tests is how a run stops fitting in forty
        /// five minutes, which is the same problem the images cap already solved. Off by
        /// default, so nothing is capped unless somebody asks.
        /// </summary>
        [Test]
        public void ACapPerTestKeepsTheRestOutAndCountsThem()
        {
            ViewpointSettings settings = Settings();
            settings.MaxPerTest = 2;

            ClashViewpointPlanOutcome outcome = ClashViewpointPlan.For(
                new[]
                {
                    Simple2("T", "a"), Simple2("T", "b"), Simple2("T", "c"),
                    Simple2("U", "a")
                },
                settings,
                false);

            Assert.That(outcome.Planned.Count, Is.EqualTo(3), "two of T and one of U");
            Assert.That(outcome.OverTheCapCount, Is.EqualTo(1));
            Assert.That(string.Join("\n", new List<string>(outcome.Lines()).ToArray()),
                Does.Contain("over the cap"));
        }

        [Test]
        public void WithNoCapNothingIsCapped()
        {
            Assert.That(Settings().MaxPerTest, Is.EqualTo(0));
            Assert.That(Plan(false, Simple2("T", "a"), Simple2("T", "b")).OverTheCapCount,
                Is.EqualTo(0));
        }

        // ---------- the block ----------

        [Test]
        public void EveryReasonIsInTheBlockIncludingTheOnesAtZero()
        {
            string all = string.Join("\n", new List<string>(
                Plan(false, Simple("BLD-AR-Walls", "BLD-ST-Columns")).Lines()).ToArray());

            Assert.That(all, Does.Contain("clashes looked at : 1"));
            Assert.That(all, Does.Contain("viewpoints planned: 1"));

            foreach (ViewpointLeftOutReason reason in ClashViewpointPlanOutcome.AllReasons)
            {
                Assert.That(all, Does.Contain(ClashViewpointPlanOutcome.Describe(reason)));
            }

            Assert.That(all, Does.Contain("size could not be read : 0"));
            Assert.That(all, Does.Contain("none was dropped"));
            Assert.That(all, Does.Contain("none was guessed at"));
        }

        [Test]
        public void AnUnknownCodeIsCountedInTheBlock()
        {
            ClashViewpointPlanOutcome outcome = Plan(false,
                Simple("BLD-Security Devices", "BLD-ST-Walls"));

            Assert.That(outcome.UnknownDisciplineCount, Is.EqualTo(1));
            Assert.That(outcome.Planned.Count, Is.EqualTo(1), "it still gets a viewpoint");
        }

        [Test]
        public void NothingToPlanIsNotAThrow()
        {
            Assert.That(ClashViewpointPlan.For(null, Settings(), false).Planned, Is.Empty);
            Assert.That(ClashViewpointPlan.For(new ClashToPlan[] { null }, Settings(), false).Planned,
                Is.Empty);
            Assert.That(() => ClashViewpointPlan.For(null, null, false), Throws.ArgumentNullException);
        }

        // ---------- the plan on its own ----------

        /// <summary>
        /// The plan is complete on its own, with no writer in the room. Nothing in Core can
        /// write a viewpoint or assert on the add-in that does, so this pins the shape one
        /// clash gives the plan and nothing about the writing. A planned viewpoint that was not
        /// written is not a viewpoint.
        /// </summary>
        [Test]
        public void ThePlanIsBuiltFromOneClash()
        {
            Assert.That(Plan(false, Simple("BLD-AR-Walls", "BLD-ST-Columns")).Planned.Count,
                Is.EqualTo(1),
                "the plan is complete on its own, and ViewpointBuilder in the add-in is what writes it");
        }

        private static ClashToPlan Simple2(string test, string clash)
        {
            return Clash(test, clash, "BLD-AR-Walls", "BLD-ST-Columns",
                ClashStatus.New, ClashPriority.None, null);
        }
    }
}
