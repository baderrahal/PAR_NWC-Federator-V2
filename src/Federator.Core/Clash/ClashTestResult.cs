using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Federator.Core.Clash
{
    /// <summary>One test that actually ran, and what it found.</summary>
    public sealed class ClashTestResult
    {
        internal ClashTestResult(
            string name, int leftItems, int rightItems, ClashTally tally, double seconds)
        {
            Name = name;
            LeftItems = leftItems;
            RightItems = rightItems;
            Tally = tally ?? new ClashTally();
            Seconds = seconds;
        }

        public string Name { get; private set; }

        public int LeftItems { get; private set; }

        public int RightItems { get; private set; }

        public ClashTally Tally { get; private set; }

        public double Seconds { get; private set; }

        /// <summary>Ran and found nothing. This is what passed means, and it is not skipped.</summary>
        public bool Passed
        {
            get { return Tally.Total == 0; }
        }

        public string Line()
        {
            return (Passed ? "passed  " : "clashes ") + Name
                + "  items " + LeftItems + " v " + RightItems
                + "  " + Tally.Describe()
                + "  " + Seconds.ToString("0.0", CultureInfo.InvariantCulture) + "s";
        }

        public override string ToString()
        {
            return Line();
        }
    }
}
