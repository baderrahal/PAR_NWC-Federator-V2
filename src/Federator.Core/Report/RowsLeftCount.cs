using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Report
{
    /// <summary>
    /// How the rows one fault left with no picture asked for were counted, F149,
    /// ImageOptions.CountTheRowsLeft: the rows counted as failed, each named on a line by the
    /// writer, the rows passed over on their status or on the cap, and whether the guard was told.
    /// </summary>
    public sealed class RowsLeftCount
    {
        private readonly List<ClashRow> failed = new List<ClashRow>();

        internal RowsLeftCount()
        {
        }

        /// <summary>The rows counted as failed, in their order.</summary>
        public IList<ClashRow> Failed
        {
            get { return new ReadOnlyCollection<ClashRow>(failed); }
        }

        public int PassedOverOnStatus { get; private set; }

        public int PassedOverOnTheCap { get; private set; }

        /// <summary>Whether the guard was told of the fault, once, which it is only where a row was counted as failed.</summary>
        public bool GuardTold { get; private set; }

        internal void FailedOne(ClashRow row)
        {
            failed.Add(row);
        }

        internal void OnItsStatus()
        {
            PassedOverOnStatus++;
        }

        internal void OnTheCap()
        {
            PassedOverOnTheCap++;
        }

        internal void ToldTheGuard()
        {
            GuardTold = true;
        }
    }
}
