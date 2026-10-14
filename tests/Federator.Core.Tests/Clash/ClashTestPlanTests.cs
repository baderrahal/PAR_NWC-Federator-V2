using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Everything that can be decided about a clash test before Navisworks is involved.
    /// The sample values in here are sample data, not settings. Nothing about any one
    /// project is in the code they test.
    /// </summary>
    [TestFixture]
    public class ClashTestPlanTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string LeftPath = Root + "/Mechanical/Mechanical-HVAC/BLD-ME-Air Terminals";
        private const string RightPath = Root + "/Architecture/BLD-AR-Floors";

        /// <summary>
        /// One test written the way the reference file writes them. Every part of it is a
        /// parameter, so a test can change exactly the one thing it is about.
        /// </summary>
        private static string Xml(
            string units = "ft",
            string testType = "hard_conservative",
            string tolerance = "0.2460629921",
            string mergeComposites = "1",
            string left = LeftPath,
            string right = RightPath,
            string name = "AR-Floors v ME-Air Terminals",
            string selfIntersect = "0",
            string primTypes = "1")
        {
            string leftSide = left == null
                ? "<left><clashselection selfintersect=\"" + selfIntersect + "\" primtypes=\"" + primTypes
                    + "\"></clashselection></left>"
                : "<left><clashselection selfintersect=\"" + selfIntersect + "\" primtypes=\"" + primTypes
                    + "\"><locator>" + left + "</locator></clashselection></left>";

            string rightSide = right == null
                ? "<right><clashselection selfintersect=\"0\" primtypes=\"1\"></clashselection></right>"
                : "<right><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>" + right
                    + "</locator></clashselection></right>";

            return "<exchange units=\"" + units + "\">"
                + "<batchtest name=\"b\" internal_name=\"b\">"
                + "<clashtest name=\"" + name + "\" test_type=\"" + testType + "\" status=\"new\""
                + " tolerance=\"" + tolerance + "\" merge_composites=\"" + mergeComposites + "\">"
                + leftSide + rightSide
                + "</clashtest>"
                + "</batchtest></exchange>";
        }

        private static ClashTestPlan Plan(string xml, string documentUnits)
        {
            return ClashTestPlan.From(new ExchangeReader().ReadText(xml), documentUnits);
        }

        private static ClashTestPlan Plan(string xml)
        {
            return Plan(xml, "m");
        }

        // ---------- the tolerance is the drop down's alone, F167 ----------

        /// <summary>
        /// Bader's answer A to Q167 of 14 Oct 2026. A test is planned whatever tolerance its file
        /// gives, and the plan carries none. The value of the drop down is the one set on every test.
        /// </summary>
        [Test]
        public void ATestIsPlannedWhateverToleranceTheFileGivesAndThePlanCarriesNone()
        {
            ClashTestPlan plan = Plan(Xml(units: "mm", tolerance: "100"), "m");

            Assert.That(plan.Skipped.Count, Is.EqualTo(0));
            Assert.That(plan.Buildable.Count, Is.EqualTo(1));

            foreach (System.Reflection.PropertyInfo property in typeof(PlannedClashTest).GetProperties())
            {
                Assert.That(property.Name, Does.Not.Contain("Tolerance"), "the plan carries a tolerance of the file");
                Assert.That(property.Name, Does.Not.Contain("Units"), "the plan carries the units of the file");
            }
        }

        /// <summary>
        /// A matrix test at 100 mm with the drop down at 25 mm runs at 25 mm: the value set is the drop
        /// down's converted into the document units, and the file's 100 reaches nothing.
        /// </summary>
        [Test]
        public void AMatrixTestAt100MmWithTheDropDownAt25MmIsSetTo25Mm()
        {
            ClashTestPlan big = Plan(Xml(units: "mm", tolerance: "100"), "m");
            ClashTestPlan small = Plan(Xml(units: "mm", tolerance: "1"), "m");
            ToleranceChoice chosen = ToleranceChoice.Of(25.0);

            Assert.That(big.Buildable.Count, Is.EqualTo(1));
            Assert.That(small.Buildable.Count, Is.EqualTo(1));
            Assert.That(chosen.InDocumentUnits("m"), Is.EqualTo(0.025).Within(0.0000001));
            Assert.That(chosen.InDocumentUnits("mm"), Is.EqualTo(25.0).Within(0.0000001));
            Assert.That(chosen.InDocumentUnits("ft"), Is.EqualTo(0.0820209974).Within(0.0000001));
        }

        [Test]
        public void ATestWithNoToleranceAttributeIsPlannedLikeAnyOther()
        {
            ClashTestPlan plan = Plan(Xml().Replace(" tolerance=\"0.2460629921\"", string.Empty), "m");

            Assert.That(plan.Skipped.Count, Is.EqualTo(0));
            Assert.That(plan.Buildable.Count, Is.EqualTo(1));
            Assert.That(plan.Buildable[0].Name, Is.EqualTo("AR-Floors v ME-Air Terminals"));
        }

        [Test]
        public void AnEmptyOrZeroToleranceIsPlannedToo()
        {
            Assert.That(Plan(Xml(tolerance: string.Empty), "m").Buildable.Count, Is.EqualTo(1));
            Assert.That(Plan(Xml(tolerance: "0"), "m").Buildable.Count, Is.EqualTo(1));
        }

        [Test]
        public void AFileWithNoUnitsIsPlannedLikeAnyOther()
        {
            ClashTestPlan plan = ClashTestPlan.From(
                new ExchangeReader().ReadText(
                    "<exchange><batchtest name=\"b\"><clashtest name=\"T\" test_type=\"hard\""
                    + " tolerance=\"0.25\" merge_composites=\"1\" status=\"new\">"
                    + "<left><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>" + LeftPath + "</locator></clashselection></left>"
                    + "<right><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>" + RightPath + "</locator></clashselection></right>"
                    + "</clashtest></batchtest></exchange>"),
                "m");

            Assert.That(plan.Skipped.Count, Is.EqualTo(0));
            Assert.That(plan.Buildable.Count, Is.EqualTo(1));
        }

        [Test]
        public void AUnitTheToolDoesNotKnowInTheFileOrTheDocumentStopsNothing()
        {
            Assert.That(Plan(Xml(units: "cubits"), "m").Buildable.Count, Is.EqualTo(1));
            Assert.That(Plan(Xml(), "cubits").Buildable.Count, Is.EqualTo(1));
            Assert.That(Plan(Xml(), null).Buildable.Count, Is.EqualTo(1));
        }

        [Test]
        public void TheTwoOldSkipReasonsAreGone()
        {
            foreach (string name in Enum.GetNames(typeof(ClashSkipReason)))
            {
                Assert.That(name, Is.Not.EqualTo("NoTolerance"));
                Assert.That(name, Is.Not.EqualTo("UnknownUnits"));
            }
        }
        // ---------- an unknown test type is skipped, never approximated ----------

        [Test]
        public void AnUnknownTestTypeIsSkippedByNameAndNotApproximated()
        {
            ClashTestPlan plan = Plan(Xml(testType: "hard_ish", name: "T1"));

            Assert.That(plan.Buildable.Count, Is.EqualTo(0),
                "a test type this tool does not know was approximated into one it does");
            Assert.That(plan.Skipped.Count, Is.EqualTo(1));
            Assert.That(plan.Skipped[0].Name, Is.EqualTo("T1"));
            Assert.That(plan.Skipped[0].Kind, Is.EqualTo(ClashSkipReason.UnknownTestType));
            Assert.That(plan.Skipped[0].Reason, Does.Contain("hard_ish"));
            Assert.That(plan.UnknownTestTypes, Does.Contain("hard_ish"));
        }

        // The nearest one by spelling is hard, and that is exactly the answer that must
        // not come back, because a hard test standing in for a clearance test reports a
        // number that reads as real and is not.
        [Test]
        public void ATestTypeThatLooksLikeAKnownOneIsStillSkipped()
        {
            foreach (string type in new[] { "hard_", "_hard", "hardconservative", "clearances", "" })
            {
                ClashTestPlan plan = Plan(Xml(testType: type));

                Assert.That(plan.Buildable.Count, Is.EqualTo(0), "\"" + type + "\" was accepted");
                Assert.That(plan.Skipped[0].Kind, Is.EqualTo(ClashSkipReason.UnknownTestType),
                    "\"" + type + "\"");
            }
        }

        [Test]
        public void EveryTestTypeTheApiHasIsAccepted()
        {
            ClashTestKind[] expected =
            {
                ClashTestKind.Hard,
                ClashTestKind.HardConservative,
                ClashTestKind.Clearance,
                ClashTestKind.Duplicate,
                ClashTestKind.Custom
            };

            string[] names = { "hard", "hard_conservative", "clearance", "duplicate", "custom" };

            for (int i = 0; i < names.Length; i++)
            {
                ClashTestPlan plan = Plan(Xml(testType: names[i]));

                Assert.That(plan.Buildable.Count, Is.EqualTo(1), names[i]);
                Assert.That(plan.Buildable[0].TestType, Is.EqualTo(expected[i]), names[i]);
                Assert.That(plan.Buildable[0].TestTypeName, Is.EqualTo(names[i]),
                    "the raw string is kept so the log names what the file said");
            }
        }

        // These are the numbers of Autodesk.Navisworks.Api.Clash.ClashTestType, read off
        // the installed DLL. The runner casts straight through, so they have to match.
        [Test]
        public void TheKindNumbersAreTheOnesTheApiUses()
        {
            Assert.That((int)ClashTestKind.Hard, Is.EqualTo(0));
            Assert.That((int)ClashTestKind.HardConservative, Is.EqualTo(1));
            Assert.That((int)ClashTestKind.Clearance, Is.EqualTo(2));
            Assert.That((int)ClashTestKind.Duplicate, Is.EqualTo(3));
            Assert.That((int)ClashTestKind.Custom, Is.EqualTo(4));
        }

        [Test]
        public void TheTestTypeIsMatchedWithoutCaseOrSurroundingSpace()
        {
            Assert.That(Plan(Xml(testType: "  HARD_Conservative ")).Buildable.Count, Is.EqualTo(1));
        }

        // ---------- a side that names no set ----------

        [Test]
        public void ASideWithNoLocatorIsSkippedRatherThanCreatedEmpty()
        {
            ClashTestPlan plan = Plan(Xml(left: null, name: "T2"));

            Assert.That(plan.Buildable.Count, Is.EqualTo(0),
                "a test with an empty side was created, it returns zero and reads as passed");
            Assert.That(plan.Skipped[0].Name, Is.EqualTo("T2"));
            Assert.That(plan.Skipped[0].Kind, Is.EqualTo(ClashSkipReason.NoLocator));
            Assert.That(plan.Skipped[0].Reason, Does.Contain("left"));
        }

        [Test]
        public void TheEmptySideIsNamedWhicheverOneItIs()
        {
            Assert.That(Plan(Xml(right: null)).Skipped[0].Reason, Does.Contain("right"));
            Assert.That(Plan(Xml(left: null, right: null)).Skipped[0].Reason, Does.Contain("neither"));
        }

        [Test]
        public void ATestWithNoNameIsSkippedBecauseItCouldNeverBeFoundAgain()
        {
            ClashTestPlan plan = ClashTestPlan.From(
                new ExchangeReader().ReadText(
                    "<exchange units=\"ft\"><batchtest name=\"b\"><clashtest test_type=\"hard\""
                    + " tolerance=\"0.25\" merge_composites=\"1\">"
                    + "<left><clashselection><locator>" + LeftPath + "</locator></clashselection></left>"
                    + "<right><clashselection><locator>" + RightPath + "</locator></clashselection></right>"
                    + "</clashtest></batchtest></exchange>"),
                "m");

            Assert.That(plan.Buildable.Count, Is.EqualTo(0));
            Assert.That(plan.Skipped[0].Kind, Is.EqualTo(ClashSkipReason.NoName));
        }

        // ---------- an unresolved locator skips rather than creating ----------

        [Test]
        public void ALocatorThatIsNotInTheDocumentSkipsTheTestByName()
        {
            ClashTestPlan plan = Plan(Xml(name: "T3"));

            Assert.That(plan.Buildable.Count, Is.EqualTo(1), "the plan should start with it buildable");

            // Only one of the two sets is in this model, which is the ordinary case.
            ClashTestPlan resolved = plan.ResolveAgainst(new[] { RightPath });

            Assert.That(resolved.Buildable.Count, Is.EqualTo(0),
                "a test naming a set that is not there was created anyway");
            Assert.That(resolved.Skipped.Count, Is.EqualTo(1));
            Assert.That(resolved.Skipped[0].Name, Is.EqualTo("T3"));
            Assert.That(resolved.Skipped[0].Kind, Is.EqualTo(ClashSkipReason.LocatorNotResolved));
            Assert.That(resolved.Skipped[0].Reason, Does.Contain(LeftPath));
        }

        [Test]
        public void BothSetsPresentLeavesTheTestBuildable()
        {
            ClashTestPlan resolved = Plan(Xml()).ResolveAgainst(new[] { LeftPath, RightPath });

            Assert.That(resolved.Buildable.Count, Is.EqualTo(1));
            Assert.That(resolved.Skipped.Count, Is.EqualTo(0));
        }

        [Test]
        public void NeitherSetPresentNamesBothInTheReason()
        {
            ClashTestPlan resolved = Plan(Xml()).ResolveAgainst(new string[0]);

            Assert.That(resolved.Buildable.Count, Is.EqualTo(0));
            Assert.That(resolved.Skipped[0].Reason, Does.Contain(LeftPath));
            Assert.That(resolved.Skipped[0].Reason, Does.Contain(RightPath));
        }

        // Two set names in the reference file end in a space, so a trimmed comparison
        // would resolve a locator to the wrong set or to none.
        [Test]
        public void ASetNameEndingInASpaceIsMatchedExactlyAndNeverTrimmed()
        {
            string withSpace = Root + "/Electrical/BLD-EL-Devices ";
            ClashTestPlan plan = Plan(Xml(left: withSpace, right: RightPath));

            Assert.That(plan.Buildable[0].Left.Locator, Is.EqualTo(withSpace));
            Assert.That(plan.ResolveAgainst(new[] { withSpace, RightPath }).Buildable.Count, Is.EqualTo(1));

            // The trimmed spelling is a different set and must not stand in for it.
            Assert.That(plan.ResolveAgainst(new[] { withSpace.Trim(), RightPath }).Buildable.Count,
                Is.EqualTo(0), "a set name was trimmed and matched the wrong set");
        }

        [Test]
        public void TheMatchIsCaseSensitiveBecauseTheApiNamesAre()
        {
            Assert.That(
                Plan(Xml()).ResolveAgainst(new[] { LeftPath.ToUpperInvariant(), RightPath }).Buildable.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void ResolvingKeepsWhatWasAlreadySkipped()
        {
            // The good test's sides carry selfintersect and primtypes, as every real file
            // writes them. Left out, it is skipped by name too, FR-155.
            ClashTestPlan plan = Plan(
                "<exchange units=\"ft\"><batchtest name=\"b\">"
                + "<clashtest name=\"good\" test_type=\"hard\" tolerance=\"0.25\" merge_composites=\"1\">"
                + "<left><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>" + LeftPath
                + "</locator></clashselection></left>"
                + "<right><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>" + RightPath
                + "</locator></clashselection></right>"
                + "</clashtest>"
                + "<clashtest name=\"bad type\" test_type=\"nonsense\" tolerance=\"0.25\" merge_composites=\"1\">"
                + "<left><clashselection><locator>" + LeftPath + "</locator></clashselection></left>"
                + "<right><clashselection><locator>" + RightPath + "</locator></clashselection></right>"
                + "</clashtest>"
                + "</batchtest></exchange>");

            Assert.That(plan.Skipped.Count, Is.EqualTo(1));

            ClashTestPlan resolved = plan.ResolveAgainst(new string[0]);

            Assert.That(resolved.Skipped.Count, Is.EqualTo(2),
                "the earlier skip was dropped when the locators were resolved");
            Assert.That(resolved.TestsInFile, Is.EqualTo(2), "the file count must survive resolving");
        }

        // ---------- the side flags come from the file ----------

        [Test]
        public void TheSideFlagsAreReadFromTheFileAndNotAssumed()
        {
            PlannedClashTest test = Plan(Xml(selfIntersect: "1", primTypes: "7")).Buildable[0];

            Assert.That(test.Left.SelfIntersect, Is.True);
            Assert.That(test.Left.PrimitiveTypes, Is.EqualTo(7));
            Assert.That(test.Right.SelfIntersect, Is.False, "the right side carries its own flags");
            Assert.That(test.Right.PrimitiveTypes, Is.EqualTo(1));
        }

        [Test]
        public void MergeCompositesComesFromTheFile()
        {
            Assert.That(Plan(Xml(mergeComposites: "1")).Buildable[0].MergeComposites, Is.True);
            Assert.That(Plan(Xml(mergeComposites: "0")).Buildable[0].MergeComposites, Is.False);
        }

        // ---------- the three file shapes ----------

        // A project keeping its sets in the model and supplying only tests is a normal
        // case, not an error.
        [Test]
        public void AFileHoldingTestsOnlyIsAccepted()
        {
            ExchangeDocument exchange = new ExchangeReader().ReadText(Xml());

            Assert.That(exchange.HasTests, Is.True);
            Assert.That(exchange.HasSets, Is.False, "this file deliberately holds no set");

            ClashTestPlan plan = ClashTestPlan.From(exchange, "m");

            Assert.That(plan.TestsInFile, Is.EqualTo(1));
            Assert.That(plan.HasWork, Is.True, "a tests only file was refused");
            Assert.That(plan.Buildable.Count, Is.EqualTo(1));

            // The sets it names live in the model, so it resolves against those.
            Assert.That(plan.ResolveAgainst(new[] { LeftPath, RightPath }).Buildable.Count, Is.EqualTo(1));
        }

        // Search Set Infra is the sets only file. Search Set Building is NOT, it holds 1830
        // tests as well, which docs\history\scan.md records and which reading it here confirms.
        [Test]
        public void AFileHoldingSetsOnlyPlansNoTestAndIsNotAnError()
        {
            ExchangeDocument setsOnly = new ExchangeReader().ReadFile(Samples.Infra());

            Assert.That(setsOnly.HasSets, Is.True);
            Assert.That(setsOnly.HasTests, Is.False, "this sample is the sets only one");

            ClashTestPlan plan = ClashTestPlan.From(setsOnly, "m");

            Assert.That(plan.TestsInFile, Is.EqualTo(0));
            Assert.That(plan.HasWork, Is.False);
            Assert.That(plan.Buildable.Count, Is.EqualTo(0));
            Assert.That(plan.Skipped.Count, Is.EqualTo(0), "nothing to skip when there is nothing there");
        }

        [Test]
        public void AFileHoldingBothIsReadInOnePickAndPlansEveryTest()
        {
            ExchangeDocument exchange = new ExchangeReader().ReadFile(Samples.AllInOne());

            Assert.That(exchange.HasSets, Is.True);
            Assert.That(exchange.HasTests, Is.True);

            ClashTestPlan plan = ClashTestPlan.From(exchange, "m");

            Assert.That(plan.TestsInFile, Is.EqualTo(exchange.Tests.Count));
            Assert.That(plan.Buildable.Count, Is.EqualTo(exchange.Tests.Count),
                "every test in the reference file should plan cleanly");
            Assert.That(plan.Skipped.Count, Is.EqualTo(0));
            Assert.That(plan.UnknownTestTypes.Count, Is.EqualTo(0));
        }

        // The reference file's own tests all name the reference file's own sets, so the
        // whole file resolves against itself. That is what makes it the reference file.
        [Test]
        public void EveryLocatorInTheReferenceFileResolvesAgainstItsOwnSets()
        {
            ExchangeDocument exchange = new ExchangeReader().ReadFile(Samples.AllInOne());
            List<string> paths = new List<string>();

            foreach (SelectionSetDefinition set in exchange.Sets)
            {
                paths.Add(set.Path);
            }

            ClashTestPlan resolved = ClashTestPlan.From(exchange, "m").ResolveAgainst(paths);

            Assert.That(resolved.Skipped.Count, Is.EqualTo(0),
                "a locator in the reference file did not resolve against the reference file");
            Assert.That(resolved.Buildable.Count, Is.EqualTo(exchange.Tests.Count));
        }

        // Against a model holding one discipline, most pairs name a set that is not there.
        // That is the ordinary case, not a failure.
        [Test]
        public void AgainstOneDisciplineMostTestsSkipAndTheRestStillPlan()
        {
            ExchangeDocument exchange = new ExchangeReader().ReadFile(Samples.AllInOne());
            List<string> some = new List<string>();

            foreach (SelectionSetDefinition set in exchange.Sets)
            {
                if (set.Folders.Count > 0 && some.Count < 3)
                {
                    some.Add(set.Path);
                }
            }

            ClashTestPlan resolved = ClashTestPlan.From(exchange, "m").ResolveAgainst(some);

            Assert.That(resolved.Buildable.Count + resolved.Skipped.Count,
                Is.EqualTo(exchange.Tests.Count), "a test went missing between the two lists");
            Assert.That(resolved.Skipped.Count, Is.GreaterThan(0));

            foreach (SkippedClashTest skipped in resolved.Skipped)
            {
                Assert.That(skipped.Name, Is.Not.Null.And.Not.Empty,
                    "every skipped test has to be reported by name");
                Assert.That(skipped.Reason, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void EveryDistinctLocatorIsReportedOnce()
        {
            ClashTestPlan plan = ClashTestPlan.From(
                new ExchangeReader().ReadFile(Samples.AllInOne()), "m");

            IList<string> locators = plan.DistinctLocators();
            Assert.That(locators.Count, Is.EqualTo(new HashSet<string>(locators).Count));
            Assert.That(locators.Count, Is.GreaterThan(0));
        }

        [Test]
        public void NoExchangeAtAllIsRefusedRatherThanPlanned()
        {
            Assert.Throws<ArgumentNullException>(delegate { ClashTestPlan.From(null, "m"); });
            Assert.Throws<ArgumentNullException>(delegate { Plan(Xml()).ResolveAgainst(null); });
        }

        [Test]
        public void EveryReasonHasWordsForIt()
        {
            foreach (ClashSkipReason reason in Enum.GetValues(typeof(ClashSkipReason)))
            {
                Assert.That(ClashTestPlan.Describe(reason), Is.Not.EqualTo("UNKNOWN"),
                    reason + " has no words, so its count in the totals reads as nothing");
            }
        }

        // ---------- the first CLASH line of a group, FR-058 ----------

        // Set 03 log:289 read 1830 to create and log:297, eight lines on, 36 created. The
        // first line is written before the sides are counted, so it cannot say how many are
        // created and must not say a number that reads as that.
        [Test]
        public void TheFirstClashLineOfAPickedFileSaysNothingIsToBeCreatedYet()
        {
            ClashTestPlan plan = Plan(Xml());
            string line = plan.PlanLine("100000");

            Assert.That(line, Does.StartWith("CLASH    100000, 1 in the file, 0 skipped before the model"));
            Assert.That(line, Does.Not.Contain("to create"),
                "the creation plan has not run yet, so no count of tests to create is known");
            Assert.That(line, Does.Contain("once the sets are counted"),
                "the line names where the count of created tests is said");
        }

        // With no XML nothing is created, so the saved tests are the ones to run, and the
        // line keeps saying so.
        [Test]
        public void TheFirstClashLineWithNoFileCountsTheSavedTestsToRun()
        {
            ClashTestPlan plan = MirrorRuleTests.SavedPlan("Saved one");

            Assert.That(plan.PlanLine("1C07BC"),
                Is.EqualTo("CLASH    1C07BC, 1 saved in the document, 1 to run, 0 skipped before the model"));
        }

        [Test]
        public void TheFirstClashLineOfAGroupWithNoNameSaysUnknown()
        {
            Assert.That(Plan(Xml()).PlanLine(null), Does.StartWith("CLASH    UNKNOWN, "));
        }
    }
}
