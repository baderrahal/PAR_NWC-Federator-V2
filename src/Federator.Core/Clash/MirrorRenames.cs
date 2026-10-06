using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Clash
{
    /// <summary>
    /// One test saved before the mirror rule to be renamed as the mirror it was saved for,
    /// F132, Bader's answer A to Q136: renamed to its own name with (mirror) at the end, the
    /// name this tool gives that mirror and never the kept test's, its statuses kept, and run
    /// as the mirror, its clashes merged into the kept test's as his answer D to Q133 says.
    /// Built by MirrorRule.RenamesIn and nothing else. The add-in half is to make the rename,
    /// not built yet.
    /// </summary>
    public sealed class MirrorRename
    {
        internal MirrorRename(PlannedClashTest saved, string newName, string keptName)
        {
            Saved = saved;
            NewName = newName;
            KeptName = keptName;
        }

        /// <summary>The test as it is saved, its name the XML's name of the mirror and its address where it sits.</summary>
        public PlannedClashTest Saved { get; private set; }

        /// <summary>The name this tool gives the mirror, its own name with the ending, MirrorPair.MirrorName.</summary>
        public string NewName { get; private set; }

        /// <summary>The test kept, whose report the mirror's clashes are merged into.</summary>
        public string KeptName { get; private set; }
    }

    /// <summary>
    /// The renames MirrorRule.RenamesIn planned, Q136 A, and its MIRROR lines: one counting
    /// them, then one naming each rename and each one refused, with why.
    /// </summary>
    public sealed class MirrorRenames
    {
        private readonly List<string> lines;

        internal MirrorRenames(List<MirrorRename> planned, List<string> lines)
        {
            Planned = new ReadOnlyCollection<MirrorRename>(planned);
            this.lines = lines;
        }

        /// <summary>Every rename planned, in the order of the pairs.</summary>
        public ReadOnlyCollection<MirrorRename> Planned { get; private set; }

        /// <summary>The MIRROR lines for the log, every rename and every refusal named.</summary>
        public IList<string> Lines()
        {
            return new List<string>(lines);
        }
    }
}
