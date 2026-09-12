using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Federator.Core.Clash
{
    /// <summary>Clash counts by status. The total is counted off the same numbers.</summary>
    public sealed class ClashTally
    {
        private readonly Dictionary<ClashStatus, int> counts = new Dictionary<ClashStatus, int>();

        /// <summary>The statuses in the order they are reported, so every block reads the same.</summary>
        public static readonly ClashStatus[] AllStatuses =
        {
            ClashStatus.New,
            ClashStatus.Active,
            ClashStatus.Reviewed,
            ClashStatus.Approved,
            ClashStatus.Resolved
        };

        public void Add(ClashStatus status)
        {
            Add(status, 1);
        }

        public void Add(ClashStatus status, int howMany)
        {
            int already;
            counts.TryGetValue(status, out already);
            counts[status] = already + howMany;
        }

        public void Add(ClashTally other)
        {
            if (other == null)
            {
                return;
            }

            foreach (ClashStatus status in AllStatuses)
            {
                Add(status, other.Of(status));
            }
        }

        public int Of(ClashStatus status)
        {
            int count;
            return counts.TryGetValue(status, out count) ? count : 0;
        }

        public int Total
        {
            get
            {
                int total = 0;

                foreach (ClashStatus status in AllStatuses)
                {
                    total += Of(status);
                }

                return total;
            }
        }

        /// <summary>New 12, Active 3. Only the statuses that carry a number.</summary>
        public string Describe()
        {
            StringBuilder text = new StringBuilder();

            foreach (ClashStatus status in AllStatuses)
            {
                int count = Of(status);

                if (count == 0)
                {
                    continue;
                }

                if (text.Length > 0)
                {
                    text.Append(", ");
                }

                text.Append(status).Append(' ').Append(count);
            }

            return text.Length == 0 ? "none" : text.ToString();
        }

        public override string ToString()
        {
            return Describe();
        }
    }
}
