using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Views;
using System.IO;
using System.Text.RegularExpressions;
using Federator.Core.Probe;
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

        /// <summary>
        /// F72a. The service list answers to the client's matrix and not to whoever typed
        /// it. Every category a SERVICE DISCIPLINE set asks for has to have been DECIDED
        /// about, either a service or one of the four decided not to be one, so a set added
        /// to the matrix later cannot be silently missed.
        ///
        /// IT IS NOT "IS IT A SERVICE" AND THAT MATTERS. The literal reading would force
        /// Mechanical Equipment onto the service list, and an air handling unit against a
        /// wall would then be moved to Reviewed automatically, which is the opposite of
        /// what the penetration rule is for.
        ///
        /// A condition written as contains is a STEM and not a category. Cable Tray
        /// matches Cable Trays and Cable Tray Fittings, and Conduit matches Conduits and
        /// Conduit Fittings, which are four real Revit categories and all four are on the
        /// list.
        /// </summary>
        [Test]
        public void EveryCategoryAServiceSetAsksForHasBeenDecidedAbout()
        {
            PenetrationSettings settings = new PenetrationSettings();
            string xml = File.ReadAllText(Samples.CorrectedMatrix());

            // The four whole service disciplines. Electrical is NOT one of them: most of
            // its sets are equipment and fixtures, and only two of them are services. The
            // two are named below as sample data, the same way every other name off the
            // client's file appears in a test.
            string[] serviceDisciplines = { "BLD-ME-", "BLD-FF-", "BLD-PL-", "BLD-DR-" };
            string[] electricalServiceSets =
            {
                "BLD-EL-Cable Tray&Cable Tray Fittings",
                "BLD-EL-Conduits & Conduit Fittings"
            };
            List<string> unaccounted = new List<string>();
            int looked = 0;
            int electricalSeen = 0;

            foreach (Match set in Regex.Matches(
                xml, "<selectionset name=\"([^\"]*)\"(.*?)</selectionset>", RegexOptions.Singleline))
            {
                // The attribute is XML, so an ampersand in a set name arrives escaped.
                // Comparing the raw text would silently match nothing and the test would
                // pass without having looked at the two sets it was written for.
                string name = set.Groups[1].Value.Replace("&amp;", "&");
                bool isService = false;

                foreach (string prefix in serviceDisciplines)
                {
                    if (name.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        isService = true;
                    }
                }

                foreach (string named in electricalServiceSets)
                {
                    if (string.Equals(name, named, StringComparison.Ordinal))
                    {
                        isService = true;
                        electricalSeen++;
                    }
                }

                if (!isService)
                {
                    continue;
                }

                foreach (Match condition in Regex.Matches(
                    set.Groups[2].Value, "<condition test=\"([^\"]*)\"(.*?)</condition>",
                    RegexOptions.Singleline))
                {
                    if (condition.Groups[2].Value.IndexOf(
                        "LcRevitPropertyElementCategory", StringComparison.Ordinal) < 0)
                    {
                        continue;
                    }

                    Match value = Regex.Match(
                        condition.Groups[2].Value, "<data type=\"wstring\">([^<]*)</data>");

                    if (!value.Success)
                    {
                        continue;
                    }

                    string asked = value.Groups[1].Value;
                    bool contains = string.Equals(
                        condition.Groups[1].Value, "contains", StringComparison.Ordinal);

                    looked++;

                    if (!Decided(settings, asked, contains))
                    {
                        unaccounted.Add(name + " asks for \"" + asked + "\"");
                    }
                }
            }

            Assert.That(looked, Is.GreaterThan(20), "the service sets were found at all");
            Assert.That(electricalSeen, Is.EqualTo(electricalServiceSets.Length),
                "both electrical service sets were looked at, rather than silently missed");
            Assert.That(unaccounted, Is.Empty,
                "a category the matrix asks for that nobody has decided about: "
                    + string.Join("; ", unaccounted.ToArray()));
        }

        /// <summary>
        /// Whether somebody has decided about that asked-for value. A condition written as
        /// CONTAINS is a stem and not a category: Cable Tray matches Cable Trays and Cable
        /// Tray Fittings, and Conduit matches Conduits and Conduit Fittings, which are four
        /// real Revit categories and all four are on the service list.
        /// </summary>
        private static bool Decided(PenetrationSettings settings, string asked, bool contains)
        {
            if (settings.IsDecided(asked))
            {
                return true;
            }

            if (!contains)
            {
                return false;
            }

            foreach (string category in ProbeSettings.DefaultCategories())
            {
                if (category.IndexOf(asked, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The break. A category nobody has decided about is caught.</summary>
        [Test]
        public void ACategoryNobodyHasDecidedAboutIsCaught()
        {
            PenetrationSettings settings = new PenetrationSettings();

            Assert.That(Decided(settings, "Ducts", false), Is.True, "a service");
            Assert.That(Decided(settings, "Mechanical Equipment", false), Is.True,
                "decided NOT to be a service, which is still decided");
            Assert.That(Decided(settings, "Cable Tray", true), Is.True, "a stem, not a category");
            Assert.That(Decided(settings, "Structural Framing", false), Is.False);
            Assert.That(Decided(settings, "Telephone Equipment", false), Is.False);
        }

        [Test]
        public void PipeInsulationIsOnTheServiceList()
        {
            Assert.That(PenetrationSettings.DefaultServiceCategories, Does.Contain("Pipe Insulation"));
            Assert.That(new PenetrationSettings().IsService("Pipe Insulation"), Is.True);
        }

    }
}
