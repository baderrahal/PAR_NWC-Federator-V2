using System;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Where one row of the report came from in the document, F132's add-in half: the test
    /// it was read from, where that test sits, and the index path of its result under it.
    /// Recorded for every row as the test is read, so the pictures of a test in a merge
    /// are rendered after the merge and, since attempt 2, the views resolve each row of the
    /// merged report by it, under the test the report holds the row under.
    /// </summary>
    public sealed class RowAddress
    {
        public RowAddress(TestAddress address, string testName, RowUnderTest row)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            Address = address;
            TestName = testName ?? string.Empty;
            Row = row;
        }

        /// <summary>Where the test the row was read from sits in the tests tree.</summary>
        public TestAddress Address { get; private set; }

        /// <summary>The name the test ran under, read back on every resolve so a moved test is refused.</summary>
        public string TestName { get; private set; }

        /// <summary>The row and the path of its result under that test.</summary>
        public RowUnderTest Row { get; private set; }

        /// <summary>The one key the rows of one test share, so a test is resolved once for all of them.</summary>
        public string TestKey
        {
            get { return Address + " " + TestName; }
        }
    }
}
