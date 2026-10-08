using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Clash;
using Federator.Core.Report;

namespace Federator.Core.Views
{
    /// <summary>
    /// One clash as the merged report holds it, F132 attempt 2 of the add-in half, the
    /// breaker's finding R4: the row and the test the report holds it under. The view of a
    /// clash is built from this and not from the document, because the mirror merge changes
    /// which test a row sits under, its status by Q138 B and whether its test is still on the
    /// report at all. What the document still supplies, the camera, the two items and the
    /// size, the add-in resolves by the address the harvest recorded for the row.
    /// </summary>
    public sealed class ReportClash
    {
        private readonly TestReport test;
        private readonly ClashRow row;

        internal ReportClash(TestReport test, ClashRow row)
        {
            if (test == null)
            {
                throw new ArgumentNullException("test");
            }

            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            this.test = test;
            this.row = row;
        }

        /// <summary>The row the report holds, the one key the add-in finds its address by.</summary>
        public ClashRow Row
        {
            get { return row; }
        }

        /// <summary>The test the report holds the row under, which for a clash only a mirror found is the kept test.</summary>
        public string TestName
        {
            get { return test.Name; }
        }

        /// <summary>
        /// The clash's name as the workbook writes it, ClashRow.WrittenName: its own name,
        /// or for a clash only a mirror found its name and the mirror's, since its own name
        /// can be one the kept test's own rows already carry and the view says where Clash
        /// Detective shows it.
        /// </summary>
        public string ClashName
        {
            get { return row.WrittenName(); }
        }

        /// <summary>The row's status, which the merge restated where the two tests of a pair differ, Q138 B.</summary>
        public ClashStatus Status
        {
            get { return row.Status; }
        }

        /// <summary>
        /// What the per clash plan is handed for this row, with the size the add-in read off
        /// the resolved result, or null where none was read.
        /// </summary>
        public ClashToPlan ToPlan(SizeVerdict? serviceSize)
        {
            return new ClashToPlan(
                test.Name,
                ClashName,
                ByDesignRule.SetNameIn(test.LeftLocator),
                ByDesignRule.SetNameIn(test.RightLocator),
                row.Status,
                test.Priority,
                serviceSize);
        }
    }

    /// <summary>The clashes to view, and the test names the report carried twice, each read once.</summary>
    public sealed class ReportClashesOutcome
    {
        private readonly List<ReportClash> clashes = new List<ReportClash>();
        private readonly List<string> namedTwice = new List<string>();

        internal ReportClashesOutcome()
        {
        }

        public ReadOnlyCollection<ReportClash> Clashes
        {
            get { return clashes.AsReadOnly(); }
        }

        /// <summary>
        /// A test name the report carries twice is read once, because both would resolve to
        /// the same document test and every clash of it would be planned twice under one name.
        /// </summary>
        public ReadOnlyCollection<string> NamedTwice
        {
            get { return namedTwice.AsReadOnly(); }
        }

        internal void Add(ReportClash clash)
        {
            clashes.Add(clash);
        }

        internal void NameTwice(string testName)
        {
            namedTwice.Add(testName);
        }
    }

    /// <summary>
    /// THE CLASHES OF A TEST ARE THE MERGED REPORT'S ROWS FOR IT, WITH THEIR ROW STATUSES.
    /// One rule in one place for the per clash views, ClashViewpointPlan, and for the one
    /// view per test F114's add-in pass will build over the same rows. Rows come in the
    /// report's order, so two runs of one model give the same tree.
    /// </summary>
    public static class ReportClashes
    {
        public static ReportClashesOutcome Of(ClashReport report)
        {
            ReportClashesOutcome outcome = new ReportClashesOutcome();

            if (report == null)
            {
                return outcome;
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (TestReport test in report.Tests)
            {
                if (test == null || !test.HasRows)
                {
                    continue;
                }

                if (!seen.Add(test.Name))
                {
                    outcome.NameTwice(test.Name);
                    continue;
                }

                foreach (ClashRow row in test.Rows)
                {
                    if (row != null)
                    {
                        outcome.Add(new ReportClash(test, row));
                    }
                }
            }

            return outcome;
        }
    }
}
