using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F168, Bader's answer A to Q168 of 14 Oct 2026. A test saved in the NWF that the built in matrix does
    /// not name is run too, where it sits, after the tests of the matrix.
    /// </summary>
    [TestFixture]
    public class SavedTestsTheMatrixDoesNotNameTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string Left = Root + "/Architecture/BLD-AR-Walls";
        private const string Right = Root + "/Architecture/BLD-AR-Floors";

        private static ClashTestPlan Matrix(params string[] names)
        {
            string tests = string.Empty;

            foreach (string name in names)
            {
                tests += "<clashtest name=\"" + name + "\" test_type=\"hard_conservative\" status=\"new\" tolerance=\"0.1\""
                    + " merge_composites=\"1\"><left><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>" + Left
                    + "</locator></clashselection></left><right><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>"
                    + Right + "</locator></clashselection></right></clashtest>";
            }

            return ClashTestPlan.From(
                new ExchangeReader().ReadText("<exchange units=\"m\"><batchtest name=\"b\">" + tests + "</batchtest></exchange>"), "m");
        }

        private static SavedClashTest Saved(string name, int type, params int[] address)
        {
            return new SavedClashTest(name, type, true, false, 1, Left, false, 1, Right, address);
        }

        [Test]
        public void ASavedTestNotInTheMatrixIsAddedAfterTheMatrixTestsAndRunsWhereItSits()
        {
            ClashTestPlan matrix = Matrix("A", "B");
            ClashTestPlan saved = ClashTestPlan.FromDocument(
                new List<SavedClashTest> { Saved("B", 1, 0), Saved("By hand", 1, 3), Saved("By hand too", 1, 4, 0) }, "m");

            IList<string> added;
            ClashTestPlan together = matrix.WithTheSavedTestsTheMatrixDoesNotName(saved, out added);

            Assert.That(added, Is.EqualTo(new[] { "By hand", "By hand too" }));
            Assert.That(together.Buildable.Count, Is.EqualTo(4));
            Assert.That(together.Buildable[0].Name, Is.EqualTo("A"));
            Assert.That(together.Buildable[1].Name, Is.EqualTo("B"));
            Assert.That(together.Buildable[1].IsFromDocument, Is.False, "the matrix's own test is still the matrix's");
            Assert.That(together.Buildable[2].Name, Is.EqualTo("By hand"));
            Assert.That(together.Buildable[2].IsFromDocument, Is.True);
            Assert.That(together.Buildable[2].Address, Is.EqualTo(new[] { 3 }));
            Assert.That(together.Buildable[3].Address, Is.EqualTo(new[] { 4, 0 }));
            Assert.That(together.Buildable[2].FileIndex, Is.GreaterThanOrEqualTo(matrix.TestsInFile));
            Assert.That(together.Buildable[3].FileIndex, Is.GreaterThan(together.Buildable[2].FileIndex));
            Assert.That(together.Source, Is.EqualTo(ClashPlanSource.ExchangeFile));
            Assert.That(together.TestsInFile, Is.EqualTo(2), "the matrix still holds two");
        }

        [Test]
        public void ASavedTestTheMatrixNamesIsNotAddedTwiceEvenWhenTheMatrixSkippedIt()
        {
            ClashTestPlan matrix = Matrix("A");
            ClashTestPlan resolved = matrix.ResolveAgainst(new List<string>());
            ClashTestPlan saved = ClashTestPlan.FromDocument(new List<SavedClashTest> { Saved("A", 1, 0) }, "m");

            IList<string> added;
            ClashTestPlan together = resolved.WithTheSavedTestsTheMatrixDoesNotName(saved, out added);

            Assert.That(added, Is.Empty);
            Assert.That(together.Buildable.Count, Is.EqualTo(0));
            Assert.That(together.Skipped.Count, Is.EqualTo(1));
        }

        [Test]
        public void ASavedTestOfATypeTheToolDoesNotRunIsSkippedByNameAndNotRun()
        {
            ClashTestPlan matrix = Matrix("A");
            ClashTestPlan saved = ClashTestPlan.FromDocument(new List<SavedClashTest> { Saved("Odd", 99, 2) }, "m");

            IList<string> added;
            ClashTestPlan together = matrix.WithTheSavedTestsTheMatrixDoesNotName(saved, out added);

            Assert.That(added, Is.Empty);
            Assert.That(together.Buildable.Count, Is.EqualTo(1));
            Assert.That(together.Skipped.Count, Is.EqualTo(1));
            Assert.That(together.Skipped[0].Name, Is.EqualTo("Odd"));
            Assert.That(together.Skipped[0].Kind, Is.EqualTo(ClashSkipReason.UnknownTestType));
        }

        [Test]
        public void ANoneAtAllChangesNothing()
        {
            ClashTestPlan matrix = Matrix("A", "B");
            IList<string> added;
            ClashTestPlan together = matrix.WithTheSavedTestsTheMatrixDoesNotName(
                ClashTestPlan.FromDocument(new List<SavedClashTest>(), "m"), out added);

            Assert.That(added, Is.Empty);
            Assert.That(together.Buildable.Count, Is.EqualTo(2));
        }

        /// <summary>
        /// A saved test the matrix does not name goes under priority C unless the priority map names it.
        /// </summary>
        [Test]
        public void ASavedTestTheMapDoesNotNameIsPriorityCAndOneItNamesKeepsItsOwn()
        {
            PriorityMap map = PriorityMap.Read(
                "test_name,left_set,right_set,priority\r\nBLD-AR-Walls-vs-BLD-AR-Floors,BLD-AR-Walls,BLD-AR-Floors,A\r\n",
                "a map made in the test");

            Assert.That(map.OfASavedTest("By hand"), Is.EqualTo(ClashPriority.C));
            Assert.That(map.OfASavedTest("BLD-AR-Walls-vs-BLD-AR-Floors"), Is.EqualTo(ClashPriority.A));
            Assert.That(map.Of("By hand"), Is.EqualTo(ClashPriority.None), "a test of the matrix with no row stays with none");
        }
    }
}