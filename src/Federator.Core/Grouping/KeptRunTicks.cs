using System;
using System.Collections.Generic;

namespace Federator.Core.Grouping
{
    /// <summary>
    /// The Run boxes a regroup keeps, FR-153. Unticking one file makes the groups again, and every
    /// group came back ticked, so a group the person had unticked in another building ran. The
    /// boxes are read before the groups are made again and each group is matched by its key
    /// exactly, as the name table finds a row. A group the person left unticked starts unticked.
    /// Every other group starts ticked, as every group always has: one ticked before, one new to
    /// the list, and one that was blocked, whose empty box was never the person's choice.
    /// </summary>
    public sealed class KeptRunTicks
    {
        private readonly HashSet<string> unticked = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>One group's Run box, read before the groups are made again.</summary>
        public void Read(string group, bool blocked, bool ticked)
        {
            if (!blocked && !ticked)
            {
                unticked.Add(group);
            }
        }

        /// <summary>Whether a group made again starts with its Run box ticked.</summary>
        public bool StartsTicked(string group)
        {
            return !unticked.Contains(group);
        }
    }
}
