using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// A test already in the document is left exactly as it is, because that is where its
    /// Active and Resolved clashes live. So a tolerance changed in the XML never reaches
    /// it, and until now nothing said so.
    ///
    /// These pin that the difference is reported and that nothing is changed by reporting
    /// it. The values are sample data off the reference file, not settings.
    /// </summary>
    [TestFixture]
    public class TestDriftTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string Left = Root + "/Architecture/BLD-AR-Floors";
        private const string Right = Root + "/Mechanical/BLD-ME-Ducts";

        private static TestSettings FromFile()
        {
            return new TestSettings
            {
                Tolerance = 0.075,
                TestType = ClashTestKind.HardConservative,
                TestTypeName = "hard_conservative",
                MergeComposites = true,
                LeftLocator = Left,
                RightLocator = Right,
                LeftSelfIntersect = false,
                RightSelfIntersect = false,
                LeftPrimitiveTypes = 1,
                RightPrimitiveTypes = 1
            };
        }

        private static TestSettings InDocument()
        {
            return FromFile();
        }

        private static IList<TestDifference> Compare(TestSettings inDocument)
        {
            return TestDrift.Compare("AR-Floors v ME-Ducts", FromFile(), inDocument);
        }

        // ---------- a side that could not be read ----------

        /// <summary>
        /// A side this tool could not read comes back as the word UNKNOWN, which is a real
        /// string and compares like any other, so it used to be reported as drift when the
        /// truth was that nothing was read. It is left out, and counted as left out.
        /// </summary>
        [Test]
        public void ASideThatCouldNotBeReadIsNotReportedAsDrift()
        {
            TestSettings inDocument = InDocument();
            inDocument.LeftLocator = TestSettings.UnknownLocator;

            Assert.That(Compare(inDocument).Count, Is.EqualTo(0));
        }

        [Test]
        public void TheOtherSideIsStillCompared()
        {
            TestSettings inDocument = InDocument();
            inDocument.LeftLocator = TestSettings.UnknownLocator;
            inDocument.RightLocator = "lcop_selection_set_tree/somewhere else";

            IList<TestDifference> found = Compare(inDocument);

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].Sentence(), Does.Contain("right side"));
        }

        [Test]
        public void EachSideThatWasNotReadIsCounted()
        {
            TestSettings inDocument = InDocument();

            Assert.That(TestDrift.SidesNotCompared(FromFile(), inDocument), Is.EqualTo(0));

            inDocument.LeftLocator = TestSettings.UnknownLocator;
            Assert.That(TestDrift.SidesNotCompared(FromFile(), inDocument), Is.EqualTo(1));

            inDocument.RightLocator = TestSettings.UnknownLocator;
            Assert.That(TestDrift.SidesNotCompared(FromFile(), inDocument), Is.EqualTo(2));
        }

        [Test]
        public void TheBlockSaysHowManySidesWereLeftOutRatherThanLettingThemReadAsMatching()
        {
            string block = string.Join("\n",
                new List<string>(
                    TestDrift.Lines(new List<TestDifference>(), 1830, false, 7)).ToArray());

            Assert.That(block, Does.Contain("7 sides were left out"));
            Assert.That(block, Does.Contain("Not compared is not the same as matching"));
            Assert.That(block, Does.Not.Contain("Every one of them matches the file"));
        }

        // ---------- nothing drifted ----------

        [Test]
        public void TwoThatAgreeReportNothing()
        {
            Assert.That(Compare(InDocument()).Count, Is.EqualTo(0));
        }

        // A tolerance travels through a unit conversion on the way in, so an exact
        // comparison would report drift on every test in the file.
        [Test]
        public void ATinyRoundingDifferenceIsNotDrift()
        {
            TestSettings inDocument = InDocument();
            inDocument.Tolerance = 0.075 + (TestDrift.ToleranceEpsilon / 10);

            Assert.That(Compare(inDocument).Count, Is.EqualTo(0),
                "rounding would report drift on every test in the file");
        }

        // ---------- the tolerance, which is the one Bader asked about ----------

        // The one the brief asks for by name.
        [Test]
        public void AChangedToleranceIsReportedByTestName()
        {
            TestSettings inDocument = InDocument();
            inDocument.Tolerance = 0.05;

            IList<TestDifference> found = Compare(inDocument);

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].TestName, Is.EqualTo("AR-Floors v ME-Ducts"));
            Assert.That(found[0].Field, Is.EqualTo("the tolerance"));
            Assert.That(found[0].InFile, Is.EqualTo("0.075"));
            Assert.That(found[0].InDocument, Is.EqualTo("0.05"));
            Assert.That(found[0].Sentence(),
                Does.Contain("AR-Floors v ME-Ducts: the file says the tolerance 0.075"));
        }

        // Comparing must not touch either side. This pins that nothing is written back.
        [Test]
        public void ReportingTheDifferenceChangesNeitherSide()
        {
            TestSettings inFile = FromFile();
            TestSettings inDocument = InDocument();
            inDocument.Tolerance = 0.05;

            TestDrift.Compare("T", inFile, inDocument);

            Assert.That(inFile.Tolerance, Is.EqualTo(0.075),
                "the file's settings were changed by comparing them");
            Assert.That(inDocument.Tolerance, Is.EqualTo(0.05),
                "the test in the document was changed by comparing it");
        }

        // ---------- everything else the brief lists ----------

        [Test]
        public void AChangedTestTypeIsReported()
        {
            TestSettings inDocument = InDocument();
            inDocument.TestType = ClashTestKind.Clearance;

            IList<TestDifference> found = Compare(inDocument);

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].Field, Is.EqualTo("the test type"));
            Assert.That(found[0].InFile, Is.EqualTo("HardConservative"));
            Assert.That(found[0].InDocument, Is.EqualTo("Clearance"));
        }

        [Test]
        public void ChangedMergeCompositesIsReported()
        {
            TestSettings inDocument = InDocument();
            inDocument.MergeComposites = false;

            IList<TestDifference> found = Compare(inDocument);

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].Field, Is.EqualTo("merge composites"));
            Assert.That(found[0].InFile, Is.EqualTo("on"));
            Assert.That(found[0].InDocument, Is.EqualTo("off"));
        }

        [Test]
        public void EitherSidePointingSomewhereElseIsReported()
        {
            TestSettings inDocument = InDocument();
            inDocument.RightLocator = Root + "/Electrical/BLD-EL-Cables";

            IList<TestDifference> found = Compare(inDocument);

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].Field, Is.EqualTo("the right side set"));
            Assert.That(found[0].InFile, Does.Contain("BLD-ME-Ducts"));
            Assert.That(found[0].InDocument, Does.Contain("BLD-EL-Cables"));
        }

        [Test]
        public void TheSideFlagsAreComparedToo()
        {
            TestSettings inDocument = InDocument();
            inDocument.LeftSelfIntersect = true;
            inDocument.RightPrimitiveTypes = 7;

            IList<TestDifference> found = Compare(inDocument);

            Assert.That(found.Count, Is.EqualTo(2));
            Assert.That(found[0].Field, Is.EqualTo("the left side self intersect"));
            Assert.That(found[1].Field, Is.EqualTo("the right side primitive types"));
        }

        // Two set names in the reference file end in a space, so nothing may trim one.
        [Test]
        public void ASetNameEndingInASpaceIsComparedExactly()
        {
            TestSettings inFile = FromFile();
            inFile.LeftLocator = Root + "/Electrical/BLD-EL-Devices ";

            TestSettings inDocument = InDocument();
            inDocument.LeftLocator = Root + "/Electrical/BLD-EL-Devices";

            Assert.That(TestDrift.Compare("T", inFile, inDocument).Count, Is.EqualTo(1),
                "a trimmed comparison would call two different sets the same");
        }

        [Test]
        public void EverythingThatDriftedIsReportedRatherThanTheFirst()
        {
            TestSettings inDocument = InDocument();
            inDocument.Tolerance = 0.05;
            inDocument.MergeComposites = false;
            inDocument.LeftLocator = Root + "/Other/Thing";

            Assert.That(Compare(inDocument).Count, Is.EqualTo(3));
        }

        // ---------- what the block says ----------

        [Test]
        public void TheBlockSaysNothingWasChangedWhenApplyingIsOff()
        {
            TestSettings inDocument = InDocument();
            inDocument.Tolerance = 0.05;

            string block = string.Join("\n",
                new List<string>(TestDrift.Lines(Compare(inDocument), 1830, false, 0)).ToArray());

            Assert.That(block, Does.Contain("1830 tests were already in the document"));
            Assert.That(block, Does.Contain("Nothing was changed"));
            Assert.That(block, Does.Contain("Changing a test resets its results"));
            Assert.That(block, Does.Contain("the file says the tolerance 0.075"));
        }

        [Test]
        public void TheBlockWarnsLoudlyWhenApplyingIsOn()
        {
            TestSettings inDocument = InDocument();
            inDocument.Tolerance = 0.05;

            string block = string.Join("\n",
                new List<string>(TestDrift.Lines(Compare(inDocument), 3, true, 0)).ToArray());

            Assert.That(block, Does.Contain("RESETS their results"));
            Assert.That(block, Does.Contain("goes back to New"));
        }

        [Test]
        public void NothingDriftedSaysSoRatherThanShowingAnEmptyBlock()
        {
            string block = string.Join("\n",
                new List<string>(TestDrift.Lines(new List<TestDifference>(), 1830, false, 0)).ToArray());

            Assert.That(block, Does.Contain("Nothing has drifted"));
            Assert.That(block, Does.Not.Contain("Nothing was changed"));
        }

        // ---------- reading the file's settings off the plan ----------

        [Test]
        public void TheFilesSettingsComeStraightOffThePlan()
        {
            string xml = "<exchange units=\"ft\"><batchtest name=\"b\">"
                + "<clashtest name=\"T\" test_type=\"hard_conservative\" tolerance=\"0.2460629921\""
                + " merge_composites=\"1\">"
                + "<left><clashselection selfintersect=\"1\" primtypes=\"7\"><locator>" + Left
                + "</locator></clashselection></left>"
                + "<right><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>" + Right
                + "</locator></clashselection></right>"
                + "</clashtest></batchtest></exchange>";

            PlannedClashTest planned = ClashTestPlan.From(
                new ExchangeReader().ReadText(xml), "m").Buildable[0];

            TestSettings settings = TestSettings.FromFile(planned);

            Assert.That(settings.Tolerance, Is.EqualTo(0.075).Within(0.0000001));
            Assert.That(settings.TestType, Is.EqualTo(ClashTestKind.HardConservative));
            Assert.That(settings.MergeComposites, Is.True);
            Assert.That(settings.LeftLocator, Is.EqualTo(Left));
            Assert.That(settings.LeftSelfIntersect, Is.True);
            Assert.That(settings.LeftPrimitiveTypes, Is.EqualTo(7));
            Assert.That(settings.RightSelfIntersect, Is.False);
        }

        [Test]
        public void NothingAtAllIsRefusedRatherThanCompared()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { TestDrift.Compare("T", null, InDocument()); });
            Assert.Throws<ArgumentNullException>(
                delegate { TestDrift.Compare("T", FromFile(), null); });
            Assert.Throws<ArgumentNullException>(
                delegate { TestSettings.FromFile(null); });
        }
    }
}
