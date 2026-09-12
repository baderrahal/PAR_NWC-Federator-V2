using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Exchange;

namespace Federator.Core.Clash
{
    /// <summary>One test that can be created, with its tolerance already in document units.</summary>
    public sealed class PlannedClashTest
    {
        internal PlannedClashTest(
            string name,
            ClashTestKind testType,
            string testTypeName,
            double toleranceInFileUnits,
            string fileUnits,
            double tolerance,
            string documentUnits,
            bool mergeComposites,
            PlannedClashSide left,
            PlannedClashSide right,
            int fileIndex)
            : this(name, testType, testTypeName, toleranceInFileUnits, fileUnits, tolerance,
                documentUnits, mergeComposites, left, right, fileIndex, null)
        {
        }

        internal PlannedClashTest(
            string name,
            ClashTestKind testType,
            string testTypeName,
            double toleranceInFileUnits,
            string fileUnits,
            double tolerance,
            string documentUnits,
            bool mergeComposites,
            PlannedClashSide left,
            PlannedClashSide right,
            int fileIndex,
            IList<int> address)
        {
            Address = address == null ? null : new ReadOnlyCollection<int>(new List<int>(address));
            FileIndex = fileIndex;
            Name = name;
            TestType = testType;
            TestTypeName = testTypeName;
            ToleranceInFileUnits = toleranceInFileUnits;
            FileUnits = fileUnits;
            Tolerance = tolerance;
            DocumentUnits = documentUnits;
            MergeComposites = mergeComposites;
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Where this test sat in the file, from zero. Carried so the workbook can number
        /// its sheets in file order rather than in the order the plan happened to sort
        /// them into, which would move a sheet number between runs.
        /// </summary>
        public int FileIndex { get; private set; }

        /// <summary>
        /// Where the test already sits in the document, as the path of child indexes from
        /// the root of the tests tree. Null for a test planned from a file, which is
        /// found by name or created. Set for a test planned from the document, which is
        /// run where it is and never created or compared.
        /// </summary>
        public ReadOnlyCollection<int> Address { get; private set; }

        /// <summary>True for a test read out of the document rather than out of a file.</summary>
        public bool IsFromDocument
        {
            get { return Address != null; }
        }

        public string Name { get; private set; }

        public ClashTestKind TestType { get; private set; }

        /// <summary>The raw test_type string, kept so the log names what the file said.</summary>
        public string TestTypeName { get; private set; }

        public double ToleranceInFileUnits { get; private set; }

        public string FileUnits { get; private set; }

        /// <summary>The tolerance converted into the units the open document is in.</summary>
        public double Tolerance { get; private set; }

        public string DocumentUnits { get; private set; }

        public bool MergeComposites { get; private set; }

        public PlannedClashSide Left { get; private set; }

        public PlannedClashSide Right { get; private set; }

        /// <summary>
        /// Both tolerances with both unit names. Which units ClashTest.Tolerance is
        /// measured in is UNKNOWN until a test runs against a real model, so both numbers
        /// go in the log and the first real run settles it from the log alone.
        /// </summary>
        public string DescribeTolerance()
        {
            return Format(ToleranceInFileUnits) + " " + FileUnits
                + " is " + Format(Tolerance) + " " + DocumentUnits;
        }

        private static string Format(double value)
        {
            return value.ToString("0.##########", CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
