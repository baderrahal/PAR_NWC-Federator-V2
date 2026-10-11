using System.Collections.Generic;

namespace Federator.Core.Grouping
{
    /// <summary>
    /// What one click on a Run box sets, F130: the rows, in the order the grid shows them,
    /// and the one state every one of them takes. Made by ShiftRange.Of and nowhere else.
    /// </summary>
    public sealed class RowsToSet<T> where T : class
    {
        internal RowsToSet(IList<T> rows, bool state)
        {
            Rows = new List<T>(rows).AsReadOnly();
            State = state;
        }

        public IList<T> Rows { get; private set; }

        public bool State { get; private set; }
    }
}
