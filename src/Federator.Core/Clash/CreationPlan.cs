using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Which planned tests are worth CREATING in this document, F77.
    ///
    /// WHAT THE RUN SHOWED. TESTS CREATE took 631 seconds of a 1424 second run, which is
    /// 44 per cent of it, and most of what it created was thrown away moments later by the
    /// empty side check. 1830 tests were created and 1619 of them could not clash with
    /// anything, because one of their two sides finds nothing in that model. A group holds
    /// two or three disciplines out of seven, so most pairs of 61 sets were never going to
    /// find anything.
    ///
    /// THE SIDE COUNTS ARE ALREADY KNOWN BEFORE ANY TEST IS CREATED. The sets are built
    /// first and every one of them is resolved against the document, so the number of
    /// items each locator finds is in hand. Creating a test to discover a thing already
    /// known is the whole of the cost.
    ///
    /// THE OLD CHECK STAYS AS THE SECOND LINE OF DEFENCE. ClashSideCheck still runs before
    /// a test is run, and this plan calls it rather than carrying a second copy of the
    /// rule. A test that gets created anyway, because its counts were UNKNOWN here, is
    /// still caught there and still counts as skipped rather than passed.
    ///
    /// A COUNT THIS RULE DOES NOT HAVE MEANS CREATE IT. A locator missing from the counts
    /// is not a locator finding nothing, it is one nobody counted, and the safe mistake is
    /// to create the test and let the run-time check answer. The unsafe one is to leave a
    /// real test out of the NWF because a count was not taken.
    ///
    /// Not creating a test changes what goes in the document, never what goes in the report:
    /// the page carries a block for every test and the .tsv a row for every test of the
    /// file. The clash workbook carries a block for every test with a clash and nothing for a
    /// test that found none since F153, BlockCountLine below. A mirror merged into its kept
    /// test, F132, is carried in that test's block, and the check counts it there.
    /// </summary>
    public sealed class CreationPlan
    {
        /// <summary>The words that begin the counted line.</summary>
        public const string Prefix = "CLASH";

        private readonly List<PlannedClashTest> create;
        private readonly List<SkippedClashTest> notCreated;

        private CreationPlan(List<PlannedClashTest> create, List<SkippedClashTest> notCreated)
        {
            this.create = create;
            this.notCreated = notCreated;
            Create = new ReadOnlyCollection<PlannedClashTest>(create);
            NotCreated = new ReadOnlyCollection<SkippedClashTest>(notCreated);
        }

        /// <summary>The tests to create, in the order they came.</summary>
        public ReadOnlyCollection<PlannedClashTest> Create { get; private set; }

        /// <summary>
        /// The tests not created, each naming the side that finds nothing, ready to be
        /// counted by the same skip machinery every other reason goes through.
        /// </summary>
        public ReadOnlyCollection<SkippedClashTest> NotCreated { get; private set; }

        public int CreateCount
        {
            get { return create.Count; }
        }

        public int NotCreatedCount
        {
            get { return notCreated.Count; }
        }

        /// <summary>
        /// The plan, from the tests that survived ClashTestPlan and how many items each
        /// locator finds in this document. Locators are compared Ordinal and never
        /// trimmed, because two set names in the reference file end in a space.
        /// </summary>
        public static CreationPlan For(
            IEnumerable<PlannedClashTest> planned, IDictionary<string, int> itemsByLocator)
        {
            List<PlannedClashTest> create = new List<PlannedClashTest>();
            List<SkippedClashTest> notCreated = new List<SkippedClashTest>();

            if (planned == null)
            {
                return new CreationPlan(create, notCreated);
            }

            foreach (PlannedClashTest test in planned)
            {
                if (test == null)
                {
                    continue;
                }

                int left;
                int right;

                if (!PlannedClashSide.Counted(itemsByLocator, test.Left, out left)
                    || !PlannedClashSide.Counted(itemsByLocator, test.Right, out right))
                {
                    // Nobody counted one of the sides, so nothing is known about it. The
                    // test is created and the run-time check answers.
                    create.Add(test);
                    continue;
                }

                string why;

                if (ClashSideCheck.CanRun(test, left, right, out why))
                {
                    create.Add(test);
                }
                else
                {
                    notCreated.Add(new SkippedClashTest(
                        test.Name, ClashSkipReason.EmptySide, why, test.FileIndex));
                }
            }

            return new CreationPlan(create, notCreated);
        }

        /// <summary>
        /// The one counted line, F77. One line and not 1619, because a log that says the
        /// same thing sixteen hundred times buries everything worth reading. Which tests
        /// they were is said by the ordinary skip block, a count and five examples, which
        /// is where every other skip reason is already said.
        /// </summary>
        public string CountedLine(int testsInTheFile)
        {
            return Prefix + " " + testsInTheFile + " in the file, " + CreateCount
                + " created, " + NotCreatedCount + " not created, a side finds nothing";
        }

        /// <summary>
        /// Whether the workbook carries a block for every test with a clash and for no other, F153, Bader's
        /// decision of 2026-10-13, which retires the rule of F77 and Q73 that it carried one for every test in
        /// the file. A test with no clash writes nothing in the workbook and is on the page and the Coverage
        /// sheet alone. The counts are the report's own, after every mirror merged into its kept test was taken
        /// out of it, F132, so a merged mirror is in its kept test's block and is said. A count that differs is
        /// a fault in the report and is said in capitals the way the ROWS line says its own.
        /// </summary>
        public static string BlockCountLine(int blocksInTheWorkbook, int testsWithAClash, int testsInTheReport, int mirrorsMerged)
        {
            if (testsWithAClash < 0 || testsInTheReport < testsWithAClash || mirrorsMerged < 0)
            {
                throw new ArgumentOutOfRangeException("testsWithAClash", "The counts of the report cannot be below zero, and the tests with a clash are some of the tests.");
            }

            string merged = mirrorsMerged == 0
                ? string.Empty
                : ", and " + mirrorsMerged + (mirrorsMerged == 1 ? " mirror is" : " mirrors are")
                    + " merged into " + (mirrorsMerged == 1 ? "its kept test's block" : "their kept tests' blocks");
            int none = testsInTheReport - testsWithAClash;

            if (blocksInTheWorkbook == testsWithAClash)
            {
                return "BLOCKS   " + blocksInTheWorkbook + " in the workbook, one for every test with a clash. The other "
                    + none + " of the " + testsInTheReport + " tests of the report found none and are on the page and in the "
                    + ".tsv alone" + merged;
            }

            return "BLOCKS   " + blocksInTheWorkbook + " in the workbook against " + testsWithAClash
                + " tests with a clash of the " + testsInTheReport + " tests of the report" + merged
                + ". THE WORKBOOK MUST CARRY A BLOCK FOR EVERY TEST WITH A CLASH AND NONE FOR A TEST THAT FOUND NONE, F153";
        }
    }
}
