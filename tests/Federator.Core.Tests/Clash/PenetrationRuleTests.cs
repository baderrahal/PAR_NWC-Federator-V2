using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The penetration rule, F72, and the answer to Q33.
    ///
    /// Four things here are worth more than the rest. The 600 by 150 duct, because taking
    /// the first size property rather than the largest would move a clash nobody agreed
    /// to. The unreadable size, because it goes the OPPOSITE way from F53 on purpose and
    /// somebody will one day try to make the two agree. The clash a person already set,
    /// because overwriting one is the one thing this feature must never do. And the
    /// wording, because the block is the only record of what was changed.
    /// </summary>
    [TestFixture]
    public class PenetrationRuleTests
    {
        private static PenetrationSettings Settings()
        {
            return new PenetrationSettings();
        }

        private static SizeSettings Sizes()
        {
            return new SizeSettings();
        }

        private static PenetrationSide Side(string category, double? millimetres)
        {
            return new PenetrationSide("an item", category, millimetres);
        }

        private static PenetrationDecision Decide(
            PenetrationSide first, PenetrationSide second, ClashStatus status)
        {
            return PenetrationRule.Decide(first, second, status, Settings(), Sizes());
        }

        // ---------- the four the brief names by size ----------

        [Test]
        public void A100PipeAgainstAWallMoves()
        {
            PenetrationDecision decision =
                Decide(Side("Pipes", 100.0), Side("Walls", null), ClashStatus.New);

            Assert.That(decision.Moves, Is.True);
            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.Reviewed));
        }

        [Test]
        public void A200PipeAgainstAWallDoesNotMove()
        {
            PenetrationDecision decision =
                Decide(Side("Pipes", 200.0), Side("Walls", null), ClashStatus.New);

            Assert.That(decision.Moves, Is.False);
            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.ServiceTooLarge));
        }

        [Test]
        public void AServiceExactlyAtTheThresholdMoves()
        {
            PenetrationDecision decision =
                Decide(Side("Pipes", 150.0), Side("Walls", null), ClashStatus.New);

            Assert.That(decision.Moves, Is.True);
        }

        [Test]
        public void AServiceOneMillimetreOverTheThresholdDoesNotMove()
        {
            PenetrationDecision decision =
                Decide(Side("Pipes", 151.0), Side("Walls", null), ClashStatus.New);

            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.ServiceTooLarge));
        }

        /// <summary>
        /// The one this rule turns on. 150 is IN here and OUT in F53, and the two are the
        /// same number read in opposite directions with no gap between them.
        /// </summary>
        [Test]
        public void ExactlyTheThresholdIsInHereAndOutOfTheViewpointRule()
        {
            SizeSettings sizes = Sizes();

            PenetrationDecision penetration = PenetrationRule.Decide(
                Side("Pipes", sizes.ThresholdMillimetres),
                Side("Walls", null),
                ClashStatus.New,
                Settings(),
                sizes);

            Dictionary<string, double> read = new Dictionary<string, double>();
            read.Add("Diameter", sizes.ThresholdMillimetres);
            SizeDecision viewpoint = SizeRule.Decide(read, "Millimeters", sizes);

            Assert.That(penetration.Moves, Is.True);
            Assert.That(viewpoint.Verdict, Is.EqualTo(SizeVerdict.Small));
        }

        [Test]
        public void TheThresholdIsTheOneOnTheSizeSettingsAndNotACopy()
        {
            SizeSettings sizes = Sizes();
            sizes.ThresholdMillimetres = 300.0;

            PenetrationDecision decision = PenetrationRule.Decide(
                Side("Pipes", 250.0), Side("Walls", null), ClashStatus.New, Settings(), sizes);

            Assert.That(decision.Moves, Is.True);
        }

        // ---------- the duct, which is the reason the largest is taken ----------

        [Test]
        public void ASixHundredByOneFiftyDuctAgainstAFloorDoesNotMove()
        {
            Dictionary<string, double> read = new Dictionary<string, double>();
            read.Add("Width", 600.0);
            read.Add("Height", 150.0);

            double? largest = SizeRule.LargestMillimetres(read, "Millimeters", Sizes());

            PenetrationDecision decision =
                Decide(Side("Ducts", largest), Side("Floors", null), ClashStatus.New);

            Assert.That(largest, Is.EqualTo(600.0));
            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.ServiceTooLarge));
        }

        [Test]
        public void AOneHundredByOneHundredDuctAgainstAFloorMoves()
        {
            Dictionary<string, double> read = new Dictionary<string, double>();
            read.Add("Width", 100.0);
            read.Add("Height", 100.0);

            double? largest = SizeRule.LargestMillimetres(read, "Millimeters", Sizes());

            PenetrationDecision decision =
                Decide(Side("Ducts", largest), Side("Floors", null), ClashStatus.New);

            Assert.That(largest, Is.EqualTo(100.0));
            Assert.That(decision.Moves, Is.True);
        }

        /// <summary>
        /// Taking the FIRST property rather than the largest is what this pins. Width is
        /// first on the default list, so a 150 wide by 600 high duct would read 150 and
        /// move, which is wrong: the 600 is what has to fit through the slab.
        /// </summary>
        [Test]
        public void TheLargestIsTakenWhicheverOrderTheListNamesThem()
        {
            Dictionary<string, double> read = new Dictionary<string, double>();
            read.Add("Width", 150.0);
            read.Add("Height", 600.0);

            Assert.That(SizeRule.LargestMillimetres(read, "Millimeters", Sizes()), Is.EqualTo(600.0));
            Assert.That(SizeRule.Decide(read, "Millimeters", Sizes()).Millimetres, Is.EqualTo(150.0));
        }

        [Test]
        public void TheLargestIsConvertedThroughTheUnitTableAndNeverComparedRaw()
        {
            Dictionary<string, double> read = new Dictionary<string, double>();
            read.Add("Diameter", 0.5);

            double? inFeet = SizeRule.LargestMillimetres(read, "Feet", Sizes());

            Assert.That(inFeet, Is.EqualTo(152.4).Within(0.001));

            PenetrationDecision decision =
                Decide(Side("Pipes", inFeet), Side("Walls", null), ClashStatus.New);

            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.ServiceTooLarge));
        }

        [Test]
        public void AUnitTheTableDoesNotKnowThrowsRatherThanFallingBack()
        {
            Dictionary<string, double> read = new Dictionary<string, double>();
            read.Add("Diameter", 100.0);

            Assert.Throws<NotSupportedException>(
                delegate { SizeRule.LargestMillimetres(read, "Furlongs", Sizes()); });
        }

        [Test]
        public void NoSizePropertyAtAllReadsAsNoLargest()
        {
            Assert.That(
                SizeRule.LargestMillimetres(new Dictionary<string, double>(), "Millimeters", Sizes()),
                Is.Null);
            Assert.That(SizeRule.LargestMillimetres(null, "Millimeters", Sizes()), Is.Null);
        }

        [Test]
        public void APropertyNotOnTheSettingsListIsNotASize()
        {
            Dictionary<string, double> read = new Dictionary<string, double>();
            read.Add("Length", 5000.0);

            Assert.That(SizeRule.LargestMillimetres(read, "Millimeters", Sizes()), Is.Null);
        }

        [Test]
        public void TheLargestPropertyIsNamed()
        {
            Dictionary<string, double> read = new Dictionary<string, double>();
            read.Add("Width", 150.0);
            read.Add("Height", 600.0);

            Assert.That(SizeRule.LargestProperty(read, "Millimeters", Sizes()), Is.EqualTo("Height"));
        }

        // ---------- the other categories the brief names ----------

        [Test]
        public void AConduitAgainstARoofMoves()
        {
            PenetrationDecision decision =
                Decide(Side("Conduits", 25.0), Side("Roofs", null), ClashStatus.New);

            Assert.That(decision.Moves, Is.True);
        }

        [Test]
        public void APipeAgainstAPipeDoesNotMove()
        {
            PenetrationDecision decision =
                Decide(Side("Pipes", 50.0), Side("Pipes", 50.0), ClashStatus.New);

            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.BothService));
        }

        [Test]
        public void AWallAgainstAFloorDoesNotMove()
        {
            PenetrationDecision decision =
                Decide(Side("Walls", null), Side("Floors", null), ClashStatus.New);

            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.BothSolid));
        }

        [Test]
        public void ASizeThatCannotBeReadDoesNotMove()
        {
            PenetrationDecision decision =
                Decide(Side("Pipes", null), Side("Walls", null), ClashStatus.New);

            Assert.That(decision.Moves, Is.False);
            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.SizeUnknown));
        }

        /// <summary>
        /// The one somebody will try to make agree with F53. F53 INCLUDES an unreadable
        /// size and F72 LEAVES IT ALONE, and both are right because the safe mistake is
        /// different: showing something unnecessary against leaving a clash for a person.
        /// </summary>
        [Test]
        public void TheUnreadableSizeGoesTheOppositeWayFromTheViewpointRule()
        {
            PenetrationDecision penetration =
                Decide(Side("Pipes", null), Side("Walls", null), ClashStatus.New);

            SizeDecision viewpoint =
                SizeRule.Decide(new Dictionary<string, double>(), "Millimeters", Sizes());

            Assert.That(penetration.Moves, Is.False);
            Assert.That(viewpoint.Included, Is.True);
        }

        [Test]
        public void SomethingThatIsNeitherDoesNotMove()
        {
            PenetrationDecision decision =
                Decide(Side("Structural Framing", 50.0), Side("Walls", null), ClashStatus.New);

            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.NotAPenetration));
        }

        [Test]
        public void AnItemWithNoCategoryAtAllDoesNotMove()
        {
            PenetrationDecision decision =
                Decide(Side(string.Empty, 50.0), Side("Walls", null), ClashStatus.New);

            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.NotAPenetration));
            Assert.That(decision.Reason, Does.Contain("no category"));
        }

        [Test]
        public void ANullSideDoesNotThrow()
        {
            Assert.That(
                Decide(null, Side("Walls", null), ClashStatus.New).Verdict,
                Is.EqualTo(PenetrationVerdict.NotAPenetration));
        }

        [Test]
        public void TheOrderOfTheTwoSidesDoesNotMatter()
        {
            Assert.That(Decide(Side("Pipes", 100.0), Side("Walls", null), ClashStatus.New).Moves, Is.True);
            Assert.That(Decide(Side("Walls", null), Side("Pipes", 100.0), ClashStatus.New).Moves, Is.True);
        }

        // ---------- never overwrite a decision ----------

        [Test]
        public void AClashAlreadyReviewedIsUntouched()
        {
            PenetrationDecision decision =
                Decide(Side("Pipes", 100.0), Side("Walls", null), ClashStatus.Reviewed);

            Assert.That(decision.Moves, Is.False);
            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.AlreadyDecided));
        }

        [Test]
        public void AClashAlreadyApprovedIsUntouched()
        {
            PenetrationDecision decision =
                Decide(Side("Pipes", 100.0), Side("Walls", null), ClashStatus.Approved);

            Assert.That(decision.Moves, Is.False);
            Assert.That(decision.Verdict, Is.EqualTo(PenetrationVerdict.AlreadyDecided));
            Assert.That(decision.Reason, Does.Contain("Approved"));
        }

        [Test]
        public void AClashAlreadyResolvedIsUntouched()
        {
            Assert.That(
                Decide(Side("Pipes", 100.0), Side("Walls", null), ClashStatus.Resolved).Moves,
                Is.False);
        }

        [Test]
        public void AnActiveClashMoves()
        {
            Assert.That(
                Decide(Side("Pipes", 100.0), Side("Walls", null), ClashStatus.Active).Moves,
                Is.True);
        }

        [Test]
        public void OnlyNewAndActiveMayBeMovedFrom()
        {
            Assert.That(StatusesThisToolMayMoveFrom.Allows(ClashStatus.New), Is.True);
            Assert.That(StatusesThisToolMayMoveFrom.Allows(ClashStatus.Active), Is.True);
            Assert.That(StatusesThisToolMayMoveFrom.Allows(ClashStatus.Reviewed), Is.False);
            Assert.That(StatusesThisToolMayMoveFrom.Allows(ClashStatus.Approved), Is.False);
            Assert.That(StatusesThisToolMayMoveFrom.Allows(ClashStatus.Resolved), Is.False);
        }

        [Test]
        public void TheTwoThisToolMayMoveFromAreNewAndActive()
        {
            ClashStatus[] all = StatusesThisToolMayMoveFrom.All();

            Assert.That(all.Length, Is.EqualTo(2));
            Assert.That(all[0], Is.EqualTo(ClashStatus.New));
            Assert.That(all[1], Is.EqualTo(ClashStatus.Active));
        }

        [Test]
        public void TheRefusalNamesTheStatusTheClashIsAt()
        {
            Assert.That(StatusesThisToolMayMoveFrom.WhyNot(ClashStatus.Approved), Does.Contain("Approved"));
            Assert.That(StatusesThisToolMayMoveFrom.WhyNot(ClashStatus.New), Is.Null);
        }

        [Test]
        public void ReviewedIsStillTheOnlyStatusThisToolSets()
        {
            Assert.That(StatusesThisToolMaySet.Allows(ClashStatus.Reviewed), Is.True);
            Assert.That(StatusesThisToolMaySet.Allows(ClashStatus.Approved), Is.False);
            Assert.That(StatusesThisToolMaySet.All().Length, Is.EqualTo(1));
        }

        // ---------- the categories are settings ----------

        [Test]
        public void TheThirteenServiceCategoriesAreTheDefaults()
        {
            IList<string> services = new PenetrationSettings().ServiceCategories;

            // Twelve until F72a. Pipe Insulation is the thirteenth and it sits beside the
            // pipe it wraps, so the list reads in the order a person would name them.
            Assert.That(services.Count, Is.EqualTo(13));
            Assert.That(services[0], Is.EqualTo("Pipes"));
            Assert.That(services[3], Is.EqualTo("Pipe Insulation"));
            Assert.That(services[12], Is.EqualTo("Conduit Fittings"));
        }

        [Test]
        public void TheThreeSolidCategoriesAreWallsFloorsAndRoofs()
        {
            IList<string> solids = new PenetrationSettings().SolidCategories;

            Assert.That(solids.Count, Is.EqualTo(3));
            Assert.That(solids[0], Is.EqualTo("Walls"));
            Assert.That(solids[1], Is.EqualTo("Floors"));
            Assert.That(solids[2], Is.EqualTo("Roofs"));
        }

        [Test]
        public void EveryDefaultServiceCategoryIsRecognised()
        {
            PenetrationSettings settings = Settings();

            foreach (string category in PenetrationSettings.DefaultServiceCategories)
            {
                Assert.That(settings.IsService(category), Is.True, category);
                Assert.That(settings.IsSolid(category), Is.False, category);
            }
        }

        [Test]
        public void EveryDefaultSolidCategoryIsRecognised()
        {
            PenetrationSettings settings = Settings();

            foreach (string category in PenetrationSettings.DefaultSolidCategories)
            {
                Assert.That(settings.IsSolid(category), Is.True, category);
                Assert.That(settings.IsService(category), Is.False, category);
            }
        }

        [Test]
        public void ACategoryIsMatchedWithoutCaseAndTrimmed()
        {
            PenetrationSettings settings = Settings();

            Assert.That(settings.IsService("pipes"), Is.True);
            Assert.That(settings.IsService(" Pipe Fittings "), Is.True);
            Assert.That(settings.IsSolid("WALLS"), Is.True);
        }

        [Test]
        public void TheListsCanBeChangedForAProjectThatNamesThemDifferently()
        {
            PenetrationSettings settings = Settings();
            settings.ServiceCategories = new List<string> { "Tuyaux" };
            settings.SolidCategories = new List<string> { "Murs" };

            PenetrationDecision decision = PenetrationRule.Decide(
                Side("Tuyaux", 100.0), Side("Murs", null), ClashStatus.New, settings, Sizes());

            Assert.That(decision.Moves, Is.True);
            Assert.That(settings.IsService("Pipes"), Is.False);
        }

        /// <summary>
        /// Q42. No discipline filter on the solid side. A wall is a wall whichever file it
        /// came in, and nothing in this rule reads part 5 of a name.
        /// </summary>
        [Test]
        public void NothingInTheRuleKnowsWhatADisciplineIs()
        {
            Assert.That(
                Decide(Side("Pipes", 100.0), Side("Walls", null), ClashStatus.New).Moves, Is.True);
            Assert.That(
                Decide(Side("Pipes", 100.0), Side("Walls", null), ClashStatus.New).Reason,
                Does.Not.Contain("discipline"));
        }

        // ---------- the words ----------

        [Test]
        public void TheReasonForAMoveNamesBothCategoriesAndTheSize()
        {
            PenetrationDecision decision =
                Decide(Side("Pipes", 100.0), Side("Walls", null), ClashStatus.New);

            Assert.That(decision.Reason, Does.Contain("100mm"));
            Assert.That(decision.Reason, Does.Contain("Pipes"));
            Assert.That(decision.Reason, Does.Contain("Walls"));
            Assert.That(decision.Reason, Does.Contain("New"));
        }

        [Test]
        public void TheReasonForTooLargeNamesBothNumbers()
        {
            PenetrationDecision decision =
                Decide(Side("Ducts", 600.0), Side("Floors", null), ClashStatus.New);

            Assert.That(decision.Reason, Does.Contain("600mm"));
            Assert.That(decision.Reason, Does.Contain("150mm"));
            Assert.That(decision.Reason, Does.Contain("coordination"));
        }

        [Test]
        public void TheReasonForAnUnreadableSizeSaysSomebodyShouldLook()
        {
            PenetrationDecision decision =
                Decide(Side("Pipes", null), Side("Walls", null), ClashStatus.New);

            Assert.That(decision.Reason, Does.Contain("no size could be read"));
            Assert.That(decision.Reason, Does.Contain("look at"));
        }

        [Test]
        public void EveryVerdictHasWordsAndNoneOfThemReadsUnknown()
        {
            foreach (PenetrationVerdict verdict in PenetrationRule.InOrder())
            {
                string words = PenetrationRule.Describe(verdict);

                Assert.That(words, Is.Not.Null);
                Assert.That(words.Length, Is.GreaterThan(0));
                Assert.That(words, Is.Not.EqualTo("UNKNOWN"));
            }
        }

        [Test]
        public void EverySevenVerdictsAreListedInOrderAndReviewedIsFirst()
        {
            PenetrationVerdict[] order = PenetrationRule.InOrder();

            Assert.That(order.Length, Is.EqualTo(7));
            Assert.That(order[0], Is.EqualTo(PenetrationVerdict.Reviewed));
        }

        // ---------- the tick box wording ----------

        [Test]
        public void TheLabelIsAtMostEightWords()
        {
            Assert.That(
                PenetrationSettings.TickLabel.Split(' ').Length, Is.LessThanOrEqualTo(8));
        }

        [Test]
        public void TheHelpLineIsAtMostTwelveWords()
        {
            Assert.That(
                PenetrationSettings.HelpLine(Sizes()).Split(' ').Length, Is.LessThanOrEqualTo(12));
        }

        [Test]
        public void TheHelpLineCarriesTheThresholdReadOffTheSettings()
        {
            Assert.That(PenetrationSettings.HelpLine(Sizes()), Does.Contain("150mm"));

            SizeSettings other = Sizes();
            other.ThresholdMillimetres = 300.0;

            Assert.That(PenetrationSettings.HelpLine(other), Does.Contain("300mm"));
            Assert.That(PenetrationSettings.HelpLine(other), Does.Not.Contain("150mm"));
        }

        [Test]
        public void TheHelpLineSaysWhatItCoversAndWhichStatusesMove()
        {
            string help = PenetrationSettings.HelpLine(Sizes());

            Assert.That(help, Does.Contain("walls"));
            Assert.That(help, Does.Contain("floors"));
            Assert.That(help, Does.Contain("roofs"));
            Assert.That(help, Does.Contain("New"));
            Assert.That(help, Does.Contain("Active"));
        }

        [Test]
        public void NoLabelShoutsInCapitals()
        {
            Assert.That(PenetrationSettings.TickLabel, Is.Not.EqualTo(PenetrationSettings.TickLabel.ToUpperInvariant()));
        }
        // ---------- F72a, the insulation ----------

        /// <summary>
        /// The one F72a exists for. An insulated 100 mm pipe through a wall makes TWO
        /// clashes, the pipe and its insulation, and they are the same hole through the
        /// same wall. Before Pipe Insulation was on the service list the pipe moved to
        /// Reviewed and the insulation stayed at New, so one penetration came back with
        /// two different answers.
        /// </summary>
        [Test]
        public void AnInsulatedPipeThroughAWallMovesOnBothSides()
        {
            PenetrationDecision pipe =
                Decide(Side("Pipes", 100.0), Side("Walls", null), ClashStatus.New);

            PenetrationDecision insulation =
                Decide(Side("Pipe Insulation", 120.0), Side("Walls", null), ClashStatus.New);

            Assert.That(pipe.Verdict, Is.EqualTo(PenetrationVerdict.Reviewed));
            Assert.That(insulation.Verdict, Is.EqualTo(PenetrationVerdict.Reviewed),
                "the insulation is the same penetration as the pipe inside it");
            Assert.That(insulation.Service.Category, Is.EqualTo("Pipe Insulation"));
        }

        [Test]
        public void InsulationIsStillMeasuredLikeEveryOtherService()
        {
            // Over the threshold is over the threshold, insulation or not. A 200 mm
            // insulated riser is a coordination item exactly as a 200 mm pipe is.
            Assert.That(
                Decide(Side("Pipe Insulation", 200.0), Side("Walls", null), ClashStatus.New).Verdict,
                Is.Not.EqualTo(PenetrationVerdict.Reviewed));
        }

        [Test]
        public void PipeInsulationIsOnTheServiceList()
        {
            Assert.That(PenetrationSettings.DefaultServiceCategories, Does.Contain("Pipe Insulation"));
            Assert.That(new PenetrationSettings().IsService("Pipe Insulation"), Is.True);
        }

    }
}
