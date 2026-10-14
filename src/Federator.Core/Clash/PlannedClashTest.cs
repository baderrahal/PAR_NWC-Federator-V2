using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;

namespace Federator.Core.Clash
{
    /// <summary>
    /// One test that can be created. It carries no tolerance since F167, Bader's answer A to Q167:
    /// the value of the drop down is set on every test, ToleranceChoice, and nothing a matrix, an XML
    /// or a saved test says is read.
    /// </summary>
    public sealed class PlannedClashTest
    {
        internal PlannedClashTest(
            string name,
            ClashTestKind testType,
            string testTypeName,
            bool mergeComposites,
            PlannedClashSide left,
            PlannedClashSide right,
            int fileIndex)
            : this(name, testType, testTypeName, mergeComposites, left, right, fileIndex, null)
        {
        }

        internal PlannedClashTest(
            string name,
            ClashTestKind testType,
            string testTypeName,
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
            MergeComposites = mergeComposites;
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Where this test sat in the file, from zero. Carried so the report can number the
        /// tests in file order rather than in the order the plan happened to sort them into,
        /// which would move a number between runs.
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



        public bool MergeComposites { get; private set; }

        public PlannedClashSide Left { get; private set; }

        public PlannedClashSide Right { get; private set; }

        /// <summary>The same test at another place in the order, everything else as it was, F168.</summary>
        internal PlannedClashTest AtFileIndex(int fileIndex)
        {
            return new PlannedClashTest(
                Name, TestType, TestTypeName, MergeComposites, Left, Right, fileIndex, Address);
        }

        /// <summary>The same test under another name, everything else as it was, F132's mirror name.</summary>
        internal PlannedClashTest Named(string name)
        {
            return new PlannedClashTest(
                name, TestType, TestTypeName, MergeComposites, Left, Right, FileIndex, Address);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
