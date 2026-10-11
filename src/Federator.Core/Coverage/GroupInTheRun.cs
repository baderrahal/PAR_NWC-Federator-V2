using System.Collections.Generic;

namespace Federator.Core.Coverage
{
    /// <summary>
    /// One group of the run in the coverage across the run, F127b, the breaker's second and fifth
    /// findings: every group the run took is counted, and each once. A group whose count check was made
    /// is Checked, with its tests. Every other, a group refused, whose rebuild failed, that threw before
    /// its COVERAGE step or found no document, is NotChecked when the group ends. The first word on a
    /// group is the one counted and every later one changes nothing, so a throw after a group's check was
    /// added never adds it a second time as not checked.
    /// </summary>
    public sealed class GroupInTheRun
    {
        private readonly CoverageAcrossTheRun run;
        private readonly string group;

        internal GroupInTheRun(CoverageAcrossTheRun run, string group)
        {
            this.run = run;
            this.group = group;
        }

        /// <summary>Whether the group is counted yet.</summary>
        public bool Added { get; private set; }

        /// <summary>The group's count check and its tests, counted once, where nothing was said of the group before.</summary>
        public void Checked(CountCheck check, IList<TestCoverage> tests)
        {
            if (Added)
            {
                return;
            }

            // Marked first, so a throw inside the tally never lets the group be added again.
            Added = true;
            run.Add(group, check);
            run.AddTests(group, tests);
        }

        /// <summary>The group counted as not checked, where nothing was said of it before.</summary>
        public void NotChecked()
        {
            if (Added)
            {
                return;
            }

            Added = true;
            run.Add(group, null);
        }
    }
}
