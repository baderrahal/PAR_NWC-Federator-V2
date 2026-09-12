using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// L1. With no XML picked the tool said it was running the tests already in the
    /// document and ran nothing. The second way to build a plan is from the tests saved
    /// in the document, by address, and this is the shape of it.
    /// </summary>
    [TestFixture]
    public class SavedTestPlanTests
    {
        private static SavedClashTest Saved(string name, params int[] address)
        {
            return new SavedClashTest(
                name, 1, 0.075, true, false, 1, "side A as saved", false, 1, "side B as saved", address);
        }

        [Test]
        public void BuildsFromAListOfSavedTestsByAddress()
        {
            List<SavedClashTest> saved = new List<SavedClashTest>
            {
                Saved("AR v ST", 0),
                Saved("AR v ME", 1),
                Saved("ME v EL", 2, 0)
            };

            ClashTestPlan plan = ClashTestPlan.FromDocument(saved, "m");

            Assert.That(plan.Source, Is.EqualTo(ClashPlanSource.Document));
            Assert.That(plan.TestsInFile, Is.EqualTo(3));
            Assert.That(plan.Buildable.Count, Is.EqualTo(3));
            Assert.That(plan.Skipped.Count, Is.EqualTo(0));

            Assert.That(plan.Buildable[0].Name, Is.EqualTo("AR v ST"));
            Assert.That(plan.Buildable[0].IsFromDocument, Is.True);
            Assert.That(plan.Buildable[0].Address, Is.EqualTo(new[] { 0 }));
            Assert.That(plan.Buildable[2].Address, Is.EqualTo(new[] { 2, 0 }));
            Assert.That(plan.Buildable[2].FileIndex, Is.EqualTo(2));
        }

        [Test]
        public void AnEmptyListGivesAnEmptyPlan()
        {
            ClashTestPlan plan = ClashTestPlan.FromDocument(new List<SavedClashTest>(), "m");

            Assert.That(plan.Source, Is.EqualTo(ClashPlanSource.Document));
            Assert.That(plan.TestsInFile, Is.EqualTo(0));
            Assert.That(plan.Buildable.Count, Is.EqualTo(0));
            Assert.That(plan.HasWork, Is.False);
        }

        [Test]
        public void ClashWorkAnswersTrueForADocumentPlanWithTests()
        {
            ClashTestPlan plan = ClashTestPlan.FromDocument(
                new List<SavedClashTest> { Saved("AR v ST", 0) }, "m");

            Assert.That(plan.HasWork, Is.True);
            Assert.That(ClashWork.SourceFor(null, 1), Is.EqualTo(ClashSource.TestsSavedInDocument));
            Assert.That(ClashWork.SourceFor(null, 0), Is.EqualTo(ClashSource.Nothing));
        }

        [Test]
        public void TheToleranceStaysInTheDocumentUnitsBecauseItCameOutOfTheDocument()
        {
            ClashTestPlan plan = ClashTestPlan.FromDocument(
                new List<SavedClashTest> { Saved("AR v ST", 0) }, "m");

            PlannedClashTest test = plan.Buildable[0];

            Assert.That(test.Tolerance, Is.EqualTo(0.075));
            Assert.That(test.ToleranceInFileUnits, Is.EqualTo(0.075));
            Assert.That(test.FileUnits, Is.EqualTo("m"));
            Assert.That(test.DocumentUnits, Is.EqualTo("m"));
            Assert.That(test.TestType, Is.EqualTo(ClashTestKind.HardConservative));
        }

        [Test]
        public void ASavedTestWithNoNameOrAnUnknownTypeIsSkippedByReason()
        {
            List<SavedClashTest> saved = new List<SavedClashTest>
            {
                Saved(string.Empty, 0),
                new SavedClashTest("Odd", 9, 0.1, true, false, 1, "a", false, 1, "b", new[] { 1 }),
                Saved("Fine", 2)
            };

            ClashTestPlan plan = ClashTestPlan.FromDocument(saved, "m");

            Assert.That(plan.Buildable.Count, Is.EqualTo(1));
            Assert.That(plan.Skipped.Count, Is.EqualTo(2));
            Assert.That(plan.Skipped[0].Kind, Is.EqualTo(ClashSkipReason.NoName));
            Assert.That(plan.Skipped[1].Kind, Is.EqualTo(ClashSkipReason.UnknownTestType));
            Assert.That(plan.Skipped[1].Name, Is.EqualTo("Odd"));
            Assert.That(plan.UnknownTestTypes, Has.Count.EqualTo(1));
        }

        [Test]
        public void ASavedTestNeedsItsAddress()
        {
            Assert.That(
                () => new SavedClashTest("T", 1, 0.1, true, false, 1, "a", false, 1, "b", new int[0]),
                Throws.ArgumentException);
        }

        // ---------- the three things the clash step can do ----------

        private static ExchangeDocument TestsOnly()
        {
            return new ExchangeReader().ReadText(
                "<exchange units=\"ft\"><batchtest name=\"b\">"
                + "<clashtest name=\"T\" test_type=\"hard\" tolerance=\"0.25\" merge_composites=\"1\">"
                + "<left><clashselection><locator>lcop_selection_set_tree/A</locator>"
                + "</clashselection></left>"
                + "<right><clashselection><locator>lcop_selection_set_tree/B</locator>"
                + "</clashselection></right>"
                + "</clashtest></batchtest></exchange>");
        }

        [Test]
        public void AnXmlDecidesEverythingWhateverTheDocumentHolds()
        {
            Assert.That(ClashWork.SourceFor(TestsOnly(), 0), Is.EqualTo(ClashSource.TestsFromXml));
            Assert.That(ClashWork.SourceFor(TestsOnly(), 1830), Is.EqualTo(ClashSource.TestsFromXml));
        }

        [Test]
        public void NoXmlAndSavedTestsMeansTheSavedTestsRun()
        {
            Assert.That(ClashWork.SourceFor(null, 1830), Is.EqualTo(ClashSource.TestsSavedInDocument));
        }

        [Test]
        public void NoXmlAndNoSavedTestMeansNothing()
        {
            Assert.That(ClashWork.SourceFor(null, 0), Is.EqualTo(ClashSource.Nothing));
        }

        [Test]
        public void AnXmlHoldingNeitherIsStillNothingRatherThanTheSavedTests()
        {
            // With an XML picked, today's behaviour holds whatever the document has in it.
            ExchangeDocument empty = new ExchangeReader().ReadText("<exchange units=\"ft\" />");

            Assert.That(ClashWork.SourceFor(empty, 1830), Is.EqualTo(ClashSource.Nothing));
        }

        [Test]
        public void TheThreeSourceLinesReadDifferentlyAndNameTheCounts()
        {
            string xml = ClashWork.DescribeSource(ClashSource.TestsFromXml, TestsOnly(), 0);
            string saved = ClashWork.DescribeSource(ClashSource.TestsSavedInDocument, null, 1830);
            string nothing = ClashWork.DescribeSource(ClashSource.Nothing, null, 0);

            Assert.That(xml, Does.StartWith("tests from XML"));
            Assert.That(xml, Does.Contain("1 test"));
            Assert.That(saved, Does.StartWith("tests saved in the document"));
            Assert.That(saved, Does.Contain("1830 of them"));
            Assert.That(saved, Does.Contain("no XML picked"));
            Assert.That(nothing, Does.StartWith("nothing"));
            Assert.That(nothing, Does.Contain("holds no clash test"));
            Assert.That(nothing, Does.Contain("nothing ran"));
        }
    }
}
