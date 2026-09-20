using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Exchange;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F84. A selection set that cannot match anything is a set whose every clash test can
    /// never find a clash, and none of these three findings was visible anywhere before.
    ///
    /// All three are INFORMATION. Nothing is corrected, nothing is dropped and no group is
    /// judged on any of them.
    /// </summary>
    [TestFixture]
    public class SetWarningsTests
    {
        private const string Category = "LcRevitPropertyElementCategory";

        private static string Set(string name, string folder, string conditions)
        {
            string body = "<selectionset name=\"" + name + "\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + conditions + "</conditions></findspec></selectionset>";

            return folder == null
                ? body
                : "<viewfolder name=\"" + folder + "\">" + body + "</viewfolder>";
        }

        private static string Condition(string test, string property, string value)
        {
            return "<condition test=\"" + test + "\" flags=\"0\">"
                + "<category><name internal=\"LcRevitData_Element\">Element</name></category>"
                + "<property><name internal=\"" + property + "\">Category</name></property>"
                + "<value><data type=\"wstring\">" + value + "</data></value></condition>";
        }

        private static IList<SelectionSetDefinition> Sets(string body)
        {
            ExchangeDocument document = new ExchangeReader().ReadText(
                "<exchange units=\"ft\"><selectionsets>" + body + "</selectionsets></exchange>");

            return document.Sets;
        }

        // ---------- sets asking the same question ----------

        /// <summary>
        /// The client's matrix holds two such pairs and they are already measured: Telecom
        /// Fixtures with Telephone Devices, and Electrical Fixtures with Devices. That is
        /// why 61 sets carry only 59 distinct rule lists.
        /// </summary>
        [Test]
        public void TwoSetsAskingTheSameQuestionAreNamedTogether()
        {
            IList<IdenticalSets> found = SetWarnings.FindIdentical(Sets(
                Set("BLD-EL-Telecom Fixtures", null, Condition("equals", Category, "Telephone Devices"))
                + Set("BLD-EL-Telephone Devices", null, Condition("equals", Category, "Telephone Devices"))
                + Set("BLD-AR-Walls", null, Condition("equals", Category, "Walls"))));

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].Count, Is.EqualTo(2));
            Assert.That(found[0].ToString(), Does.Contain("BLD-EL-Telecom Fixtures"));
            Assert.That(found[0].ToString(), Does.Contain("BLD-EL-Telephone Devices"));
        }

        [Test]
        public void TwoSetsAskingDifferentQuestionsAreNotAPair()
        {
            Assert.That(SetWarnings.FindIdentical(Sets(
                Set("A", null, Condition("equals", Category, "Walls"))
                + Set("B", null, Condition("equals", Category, "Floors")))), Is.Empty);
        }

        /// <summary>
        /// The ORDERED list. The same two conditions in the other order are a different
        /// question once the grouping is taken into account, and comparing them unordered
        /// would report a pair that is not one.
        /// </summary>
        [Test]
        public void TheSameTwoConditionsInTheOtherOrderAreNotAPair()
        {
            Assert.That(SetWarnings.FindIdentical(Sets(
                Set("A", null, Condition("equals", Category, "Walls")
                    + Condition("equals", Category, "Floors"))
                + Set("B", null, Condition("equals", Category, "Floors")
                    + Condition("equals", Category, "Walls")))), Is.Empty);
        }

        [Test]
        public void ASetWithNoConditionIsNotACopyOfEveryOtherEmptySet()
        {
            Assert.That(SetWarnings.FindIdentical(Sets(
                Set("A", null, string.Empty) + Set("B", null, string.Empty))), Is.Empty);
        }

        /// <summary>
        /// Read off the client's own matrix BEFORE F87. Two pairs, which is exactly why 61
        /// sets carry only 59 distinct rule lists, and that number is already recorded:
        /// Telecom Fixtures with Telephone Devices, and Electrical Fixtures with Devices.
        /// </summary>
        [Test]
        public void TheUncorrectedMatrixHoldsTwoSuchPairs()
        {
            ExchangeDocument document = new ExchangeReader().ReadFile(Samples.Matrix());
            IList<IdenticalSets> found = SetWarnings.FindIdentical(document.Sets);

            Assert.That(found.Count, Is.EqualTo(2));

            foreach (IdenticalSets pair in found)
            {
                Assert.That(pair.Count, Is.EqualTo(2));
            }

            Assert.That(document.Sets.Count, Is.EqualTo(61));
        }

        /// <summary>
        /// And AFTER F87 there is one, which is the correction working. BLD-EL-Devices
        /// asked for Electrical Fixtures, the same as BLD-EL-Electrical Fixtures, and now
        /// asks for Nurse Call Devices. The other pair is left exactly as it is: the two
        /// Telecom sets are waiting on the client and F84 reporting them is correct.
        /// </summary>
        [Test]
        public void TheCorrectedMatrixHoldsOneAndTheOneLeftIsTheTelecomPair()
        {
            ExchangeDocument document = new ExchangeReader().ReadFile(Samples.CorrectedMatrix());
            IList<IdenticalSets> found = SetWarnings.FindIdentical(document.Sets);

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].ToString(), Does.Contain("Telecom Fixtures"));
            Assert.That(found[0].ToString(), Does.Contain("Telephone Devices"));
            Assert.That(document.Sets.Count, Is.EqualTo(61));
        }

        // ---------- a category no model carries ----------

        /// <summary>
        /// The list was measured off the ten C02 federations on 2026-09-20, scan.md 5i,
        /// so the check runs: a category the models carry passes, one they do not is
        /// reported, and the block carries the count rather than none yet.
        /// </summary>
        [Test]
        public void WithTheMeasuredListACategoryNobodyHasIsReportedAndOneTheyHaveIsNot()
        {
            Assert.That(RevitCategories.Measured, Is.True,
                "the list was measured off the C02 federations, see the scan notes");
            Assert.That(RevitCategories.Count, Is.EqualTo(374));
            Assert.That(RevitCategories.Line(), Is.EqualTo("Revit categories known: 374"));

            Assert.That(SetWarnings.FindCategoriesNobodyHas(
                Sets(Set("A", null, Condition("equals", Category, "Telephone Equipment"))),
                Category).Count, Is.EqualTo(1));
            Assert.That(SetWarnings.FindCategoriesNobodyHas(
                Sets(Set("A", null, Condition("equals", Category, "Cable Trays"))),
                Category), Is.Empty);
            Assert.That(SetWarnings.FindCategoriesNobodyHas(
                Sets(Set("A", null, Condition("contains", Category, "Cable Tray"))),
                Category), Is.Empty, "a stem the measured names hold is known");
        }

        /// <summary>
        /// A measured list holds what it names and nothing else, Ordinal, so a category
        /// spelt with a different case or an extra space is one the models do not carry.
        /// </summary>
        [Test]
        public void AMeasuredListHoldsWhatItNamesAndNothingElse()
        {
            Assert.That(RevitCategories.Holds("Walls"), Is.True);
            Assert.That(RevitCategories.Holds("walls"), Is.False);
            Assert.That(RevitCategories.Holds("Walls "), Is.False);
            Assert.That(RevitCategories.Holds("anything at all"), Is.False);
            Assert.That(RevitCategories.All().Count, Is.EqualTo(374));
        }

        /// <summary>
        /// The list in the DLL is EXACTLY the CATEGORY lines of the probe result it was
        /// measured from, in the same order, so the two cannot drift, which is the rule
        /// for anything this tool wrote from a measurement. The result file is kept
        /// beside the probe that wrote it.
        /// </summary>
        [Test]
        public void TheListIsExactlyWhatTheWalkMeasured()
        {
            string result = Samples.ProbeResult("5i-result-20260920.txt");
            Assert.That(File.Exists(result), Is.True, result);

            List<string> measured = new List<string>();

            foreach (string line in File.ReadAllLines(result))
            {
                int at = line.IndexOf("CATEGORY\t", StringComparison.Ordinal);

                if (at < 0)
                {
                    continue;
                }

                string[] parts = line.Substring(at).Split('\t');
                measured.Add(parts[1]);
            }

            Assert.That(measured.Count, Is.EqualTo(374));
            Assert.That(RevitCategories.All(), Is.EqualTo(measured));
        }

        // ---------- a name breaking its folder's pattern ----------

        [Test]
        public void TheShapeOfANameIsEverythingBeforeItsLastPart()
        {
            Assert.That(SetWarnings.ShapeOf("BLD-EL-Devices", '-'), Is.EqualTo("BLD-EL"));
            Assert.That(SetWarnings.ShapeOf("BLD-Security Devices", '-'), Is.EqualTo("BLD"));
            Assert.That(SetWarnings.ShapeOf("BLD-EL-Cable Tray&Cable Tray Fittings", '-'),
                Is.EqualTo("BLD-EL"));
            Assert.That(SetWarnings.ShapeOf("Nohyphen", '-'), Is.EqualTo(string.Empty));
            Assert.That(SetWarnings.ShapeOf(null, '-'), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ANameBreakingItsFoldersPatternIsNamedWithWhatTheOthersRead()
        {
            IList<OddSetName> found = SetWarnings.FindOddNames(Sets(
                "<viewfolder name=\"Electrical\">"
                + "<selectionset name=\"BLD-EL-Devices\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "a") + "</conditions></findspec></selectionset>"
                + "<selectionset name=\"BLD-EL-Data Devices\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "b") + "</conditions></findspec></selectionset>"
                + "<selectionset name=\"BLD-Security Devices\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "c") + "</conditions></findspec></selectionset>"
                + "</viewfolder>"), '-');

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].Set.Name, Is.EqualTo("BLD-Security Devices"));
            Assert.That(found[0].Shape, Is.EqualTo("BLD"));
            Assert.That(found[0].TheOthers, Is.EqualTo("BLD-EL"));
            Assert.That(found[0].HowManyOthers, Is.EqualTo(2));
        }

        /// <summary>
        /// The majority guard. With every shape held once there is no majority to differ
        /// from, so a folder of differently named sets reports nothing rather than
        /// reporting every one of them.
        /// </summary>
        [Test]
        public void AFolderWhereEveryNameIsDifferentReportsNothing()
        {
            IList<OddSetName> found = SetWarnings.FindOddNames(Sets(
                "<viewfolder name=\"F\">"
                + "<selectionset name=\"AA-one\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "a") + "</conditions></findspec></selectionset>"
                + "<selectionset name=\"BB-two\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "b") + "</conditions></findspec></selectionset>"
                + "<selectionset name=\"CC-three\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "c") + "</conditions></findspec></selectionset>"
                + "</viewfolder>"), '-');

            Assert.That(found, Is.Empty);
        }

        /// <summary>
        /// A sibling group is a FOLDER and not the whole file. Comparing across the file
        /// would report every folder as odd against every other.
        /// </summary>
        [Test]
        public void TwoFoldersOfDifferentShapesAreBothFine()
        {
            IList<OddSetName> found = SetWarnings.FindOddNames(Sets(
                "<viewfolder name=\"Electrical\">"
                + "<selectionset name=\"BLD-EL-a\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "a") + "</conditions></findspec></selectionset>"
                + "<selectionset name=\"BLD-EL-b\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "b") + "</conditions></findspec></selectionset>"
                + "</viewfolder>"
                + "<viewfolder name=\"Architecture\">"
                + "<selectionset name=\"BLD-AR-a\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "c") + "</conditions></findspec></selectionset>"
                + "<selectionset name=\"BLD-AR-b\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "d") + "</conditions></findspec></selectionset>"
                + "</viewfolder>"), '-');

            Assert.That(found, Is.Empty);
        }

        /// <summary>
        /// Measured off the client's own file. AFTER F87 exactly one set breaks its
        /// folder's pattern, BLD-Security Devices in Electrical, and that is correct: it
        /// is a real naming fault the client has to decide about.
        /// </summary>
        [Test]
        public void TheCorrectedMatrixBreaksThePatternExactlyOnce()
        {
            ExchangeDocument document = new ExchangeReader().ReadFile(Samples.CorrectedMatrix());
            IList<OddSetName> found = SetWarnings.FindOddNames(document.Sets, '-');

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].Set.Name, Is.EqualTo("BLD-Security Devices"));
        }

        /// <summary>
        /// The break, and it is F87's fault made visible. BEFORE the rename the Drainage
        /// folder holds four BLD-DR sets and one BLD-DRPipe Accessories, so the check
        /// reports two and not one.
        /// </summary>
        [Test]
        public void TheUncorrectedMatrixBreaksThePatternTwice()
        {
            ExchangeDocument document = new ExchangeReader().ReadFile(Samples.Matrix());
            IList<OddSetName> found = SetWarnings.FindOddNames(document.Sets, '-');

            Assert.That(found.Count, Is.EqualTo(2));

            List<string> names = new List<string>();

            foreach (OddSetName odd in found)
            {
                names.Add(odd.Set.Name);
            }

            Assert.That(names, Does.Contain("BLD-DRPipe Accessories"));
            Assert.That(names, Does.Contain("BLD-Security Devices"));
        }

        // ---------- the block ----------

        /// <summary>
        /// All three checks run on the client's corrected matrix. Since the category list
        /// was measured off the ten C02 federations, 5i, fourteen of the 61 sets ask for a
        /// category none of those models carries, Ramps and Roofs among them, which is
        /// information about C02 and not a fault in the file: those buildings have no
        /// item of that category. The block names five and counts the rest.
        /// </summary>
        [Test]
        public void TheHealthBlockCarriesAllThreeCountsAndEveryCheckRan()
        {
            HealthCheckResult result = HealthCheck.Run(
                new ExchangeReader().ReadFile(Samples.CorrectedMatrix()));
            string all = string.Join("\n", new List<string>(result.Summary()).ToArray());

            Assert.That(all, Does.Contain("Sets asking exactly the same question: 1"));
            Assert.That(all, Does.Contain("Revit categories known: 374"));
            Assert.That(all, Does.Contain("Sets asking for a category no model carries: 14"));
            Assert.That(all, Does.Contain("BLD-AR-Roofs asks for \"Roofs\""));
            Assert.That(all, Does.Contain("and 9 more, counted and not listed"));
            Assert.That(all, Does.Contain("Set names breaking their folder's pattern: 1"));
            Assert.That(all, Does.Contain("BLD-Security Devices"));
        }


        /// <summary>
        /// Categories the measured list holds, one per synthetic set, so the odd sets
        /// exercise the pattern check alone and the category check, live since 5i, has
        /// nothing to say about them.
        /// </summary>
        private static readonly string[] RealCategories =
            { "Walls", "Floors", "Doors", "Windows", "Ceilings", "Stairs", "Furniture", "Ducts", "Pipes", "Conduits" };

        private static string OddSetsUnder(string folder, int odd)
        {
            string body = string.Empty;

            for (int i = 0; i < odd; i++)
            {
                body += "<selectionset name=\"ODD" + i + "-x\"><findspec mode=\"all\" disjoint=\"0\">"
                    + "<conditions>" + Condition("equals", Category, RealCategories[i + 2])
                    + "</conditions></findspec></selectionset>";
            }

            return "<viewfolder name=\"" + folder + "\">"
                + "<selectionset name=\"SAME-a\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, RealCategories[0]) + "</conditions></findspec></selectionset>"
                + "<selectionset name=\"SAME-b\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, RealCategories[1]) + "</conditions></findspec></selectionset>"
                + body + "</viewfolder>";
        }

        private static string HealthSummaryOf(string body)
        {
            HealthCheckResult result = HealthCheck.Run(new ExchangeReader().ReadText(
                "<exchange units=\"ft\"><selectionsets>" + body + "</selectionsets></exchange>"));

            return string.Join("\n", new List<string>(result.Summary()).ToArray());
        }

        /// <summary>
        /// A18. The block, through HealthCheck.Run, names five and counts the rest. The
        /// test before this one built eight odd names and asserted a count of eight, which
        /// Examples could have stopped truncating without it noticing.
        /// </summary>
        [Test]
        public void AFindingListShowsFiveExamplesAndThenACount()
        {
            string all = HealthSummaryOf(OddSetsUnder("F", 8));

            Assert.That(all, Does.Contain("Set names breaking their folder's pattern: 8"));
            Assert.That(all, Does.Contain("ODD4"));
            Assert.That(all, Does.Not.Contain("ODD5"));
            Assert.That(all, Does.Contain("and 3 more, counted and not listed"));
        }

        /// <summary>A19. Exactly five are all named with no count, and six is the first that counts.</summary>
        [Test]
        public void ExactlyFiveFindingsAreAllNamedAndOnePastItIsCounted()
        {
            string at = HealthSummaryOf(OddSetsUnder("F", HealthCheckResult.ExamplesShown));

            Assert.That(at, Does.Contain("ODD" + (HealthCheckResult.ExamplesShown - 1)));
            Assert.That(at, Does.Not.Contain("more, counted and not listed"));

            string past = HealthSummaryOf(OddSetsUnder("F", HealthCheckResult.ExamplesShown + 1));

            Assert.That(past, Does.Not.Contain("ODD" + HealthCheckResult.ExamplesShown + "-x"));
            Assert.That(past, Does.Contain("and 1 more, counted and not listed"));
        }

        /// <summary>
        /// A11. The resource IS in the DLL and the list in it is measured, which are two
        /// different facts, and this pins both so a build that loses the resource goes red
        /// here rather than saying UNKNOWN on every run.
        /// </summary>
        [Test]
        public void TheCategoryResourceIsInTheDllAndTheListIsMeasured()
        {
            Assert.That(RevitCategories.ResourceFound, Is.True,
                "the embedded resource " + RevitCategories.ResourceName + " is not in Federator.Core.dll");
            Assert.That(RevitCategories.Measured, Is.True);
            Assert.That(RevitCategories.Line(), Does.Not.Contain("none yet"));
            Assert.That(RevitCategories.Line(), Does.Not.Contain("UNKNOWN"));
        }
    }
}
