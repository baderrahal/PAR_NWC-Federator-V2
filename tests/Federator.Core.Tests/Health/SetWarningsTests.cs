using System;
using System.Collections.Generic;
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
        /// The list is measured off a real federation and has not been. A check with
        /// nothing to compare against says so rather than calling every category in the
        /// client's file one nobody has heard of.
        /// </summary>
        [Test]
        public void WithNoCategoryListNothingIsReportedAndTheBlockSaysWhy()
        {
            Assert.That(RevitCategories.Measured, Is.False,
                "the list is measured off a real federation, see the scan notes");
            Assert.That(RevitCategories.Count, Is.EqualTo(0));
            Assert.That(RevitCategories.Line(), Does.Contain("none yet"));
            Assert.That(RevitCategories.Line(), Does.Contain("no set was checked"));

            Assert.That(SetWarnings.FindCategoriesNobodyHas(
                Sets(Set("A", null, Condition("equals", Category, "Telephone Equipment"))),
                Category), Is.Empty);
        }

        [Test]
        public void AnUnmeasuredListHoldsEverythingRatherThanNothing()
        {
            Assert.That(RevitCategories.Holds("anything at all"), Is.True,
                "an empty list must never read as a list that excludes everything");
            Assert.That(RevitCategories.All(), Is.Empty);
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

        [Test]
        public void TheHealthBlockCarriesAllThreeCountsAndSaysWhichCheckDidNotRun()
        {
            HealthCheckResult result = HealthCheck.Run(
                new ExchangeReader().ReadFile(Samples.CorrectedMatrix()));
            string all = string.Join("\n", new List<string>(result.Summary()).ToArray());

            Assert.That(all, Does.Contain("Sets asking exactly the same question: 1"));
            Assert.That(all, Does.Contain("Sets asking for a category no model carries: 0"));
            Assert.That(all, Does.Contain("Revit categories known: none yet"));
            Assert.That(all, Does.Contain("Set names breaking their folder's pattern: 1"));
            Assert.That(all, Does.Contain("BLD-Security Devices"));
        }

        [Test]
        public void AFindingListShowsFiveExamplesAndThenACount()
        {
            string body = string.Empty;

            for (int i = 0; i < 8; i++)
            {
                body += "<selectionset name=\"ODD" + i + "-x\"><findspec mode=\"all\" disjoint=\"0\">"
                    + "<conditions>" + Condition("equals", Category, "v" + i)
                    + "</conditions></findspec></selectionset>";
            }

            IList<SelectionSetDefinition> sets = Sets(
                "<viewfolder name=\"F\">"
                + "<selectionset name=\"SAME-a\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "1") + "</conditions></findspec></selectionset>"
                + "<selectionset name=\"SAME-b\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + Condition("equals", Category, "2") + "</conditions></findspec></selectionset>"
                + body + "</viewfolder>");

            IList<OddSetName> odd = SetWarnings.FindOddNames(sets, '-');

            Assert.That(odd.Count, Is.EqualTo(8));
        }
    }
}
