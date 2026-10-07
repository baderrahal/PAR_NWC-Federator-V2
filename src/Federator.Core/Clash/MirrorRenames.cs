using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Clash
{
    /// <summary>
    /// One saved test an XML run renames before the tests are found by name, F132, its
    /// statuses kept, where it sits. A test saved before the mirror rule, Bader's answer A to
    /// Q136, renamed as the mirror it was saved for, to its own name with (mirror) at the end,
    /// the name this tool gives that mirror and never the kept test's. Or, since attempt 8, the
    /// mirror an earlier run made of a test the XML now runs under its own name, after a change
    /// of roles, renamed back to that name, so no second test of its question runs beside it.
    /// A test renamed either way was not created by this run, so it keeps its own clashes and
    /// no pair it is in is merged, F132 attempt 12. Built by MirrorRule.Renames and nothing
    /// else. The add-in half is to make the rename, not built yet.
    /// </summary>
    public sealed class MirrorRename
    {
        internal MirrorRename(PlannedClashTest saved, string newName, string keptName)
        {
            Saved = saved;
            NewName = newName;
            KeptName = keptName;
        }

        /// <summary>The test as it is saved, its name as the document holds it and its address where it sits.</summary>
        public PlannedClashTest Saved { get; private set; }

        /// <summary>
        /// The name it runs under: the name this tool gives the mirror, its own name with the
        /// ending, MirrorPair.MirrorName, or the name of the test the XML runs under its own name.
        /// </summary>
        public string NewName { get; private set; }

        /// <summary>
        /// The test kept whose mirror it is, for a mirror, and its own new name, for a test
        /// renamed back to the test the XML runs under its own name.
        /// </summary>
        public string KeptName { get; private set; }
    }

    /// <summary>
    /// The renames MirrorRule.Renames planned and its MIRROR lines: one counting the renames
    /// of Q136 A, then one naming each rename and each one refused, with why, then one
    /// counting the mirrors renamed back after a change of roles, then each of those and each
    /// refused.
    /// </summary>
    public sealed class MirrorRenames
    {
        private readonly List<string> lines;

        internal MirrorRenames(List<MirrorRename> planned, List<string> lines)
        {
            Planned = new ReadOnlyCollection<MirrorRename>(planned);
            this.lines = lines;
        }

        /// <summary>Every rename planned, those of Q136 A in the order of the pairs, then those renamed back in the XML's order.</summary>
        public ReadOnlyCollection<MirrorRename> Planned { get; private set; }

        /// <summary>The MIRROR lines for the log, every rename and every refusal named.</summary>
        public IList<string> Lines()
        {
            return new List<string>(lines);
        }
    }
}
