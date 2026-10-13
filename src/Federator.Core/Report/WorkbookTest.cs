namespace Federator.Core.Report
{
    /// <summary>
    /// One test as the client's sheet carries it, read back off the file, F127. A full block
    /// gives the clash rows under it and its Clashes cell. Since F153 a test that found
    /// nothing writes no block and is read as one holding none, CountCheck.
    /// </summary>
    public sealed class WorkbookTest
    {
        internal WorkbookTest(string name, int row, int rows, int clashes)
        {
            Name = name ?? string.Empty;
            Row = row;
            Rows = rows;
            Clashes = clashes;
        }

        /// <summary>The name in column A, exactly as written, never trimmed.</summary>
        public string Name { get; private set; }

        /// <summary>The sheet row its name is on.</summary>
        public int Row { get; private set; }

        /// <summary>The clash rows under it, minus one where its clash table was not found.</summary>
        public int Rows { get; private set; }

        /// <summary>Its Clashes cell as a whole number, minus one where the cell holds none.</summary>
        public int Clashes { get; private set; }
    }
}
