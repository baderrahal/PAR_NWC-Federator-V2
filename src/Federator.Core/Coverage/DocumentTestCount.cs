namespace Federator.Core.Coverage
{
    /// <summary>
    /// One clash test as the document holds it after the clash step, F127, plain numbers the
    /// add-in fills from one walk of the tests, so the counts are what the NWF holds after
    /// Compact and any status edit, and include the tests a weekly run did not run. A count
    /// that could not be taken is minus one and never zero. Statuses are not read, because
    /// they are not judged here, PQ4 of F104 being unmeasured.
    /// </summary>
    public sealed class DocumentTestCount
    {
        public DocumentTestCount(string name, string folder, int topLevel, int leaves)
        {
            Name = name ?? string.Empty;
            Folder = folder ?? string.Empty;
            TopLevel = topLevel;
            Leaves = leaves;
        }

        /// <summary>The test's name in Clash Detective, exactly as it reads there.</summary>
        public string Name { get; private set; }

        /// <summary>The folder the test sits in, empty at the root.</summary>
        public string Folder { get; private set; }

        /// <summary>The results at the top level of the test, a result group counting as one. One workbook row each.</summary>
        public int TopLevel { get; private set; }

        /// <summary>Every clash in the test, the clashes inside each result group counted. The Clashes cell.</summary>
        public int Leaves { get; private set; }

        /// <summary>Whether both counts were taken.</summary>
        public bool Counted
        {
            get { return TopLevel >= 0 && Leaves >= 0; }
        }
    }
}
