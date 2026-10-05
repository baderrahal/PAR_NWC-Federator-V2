using System;
using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Two tests whose two sides are the same two sets swapped, F132, Bader's Q114 points
    /// 4 to 6. The one kept is created and run. Its mirror is not, because his point 4 takes
    /// the two to find the same clashes, so the workbook, the viewpoints and every count
    /// would hold each of them twice. Whether they do is UNKNOWN until probe P1 measures it.
    /// Built by MirrorRule and nothing else.
    /// </summary>
    public sealed class MirrorPair
    {
        private readonly ClashPriority keptPriority;
        private readonly ClashPriority mirrorPriority;

        internal MirrorPair(
            PlannedClashTest kept, ClashPriority keptPriority, PlannedClashTest mirror, ClashPriority mirrorPriority)
        {
            if (kept == null)
            {
                throw new ArgumentNullException("kept");
            }

            if (mirror == null)
            {
                throw new ArgumentNullException("mirror");
            }

            Kept = kept;
            Mirror = mirror;
            this.keptPriority = keptPriority;
            this.mirrorPriority = mirrorPriority;
        }

        /// <summary>The test kept, the higher priority, and where equal the first in the XML.</summary>
        public PlannedClashTest Kept { get; private set; }

        /// <summary>The test not created and not run.</summary>
        public PlannedClashTest Mirror { get; private set; }

        /// <summary>
        /// The reason the mirror carries on the skip list, so the log, the workbook row and the
        /// coverage sheet all name the test it is a mirror of in the same words.
        /// </summary>
        internal string Why()
        {
            return "a mirror of " + Kept.Name + ", the same two sets swapped";
        }

        /// <summary>
        /// The pair's MIRROR line. Where the two carry different priorities, test types or
        /// tolerances, both values are named, the kept test's first, because Bader asked for
        /// both in the log. A mirror read off the document already exists, so it is not
        /// run, and one off the XML is not created either.
        /// </summary>
        internal string Line()
        {
            string line = MirrorRule.Prefix + "   " + Kept.Name + " is kept, " + Mirror.Name
                + " is its mirror and is " + (Mirror.IsFromDocument ? "not run" : "not created and not run");

            IList<string> differences = Differences();

            return differences.Count == 0
                ? line
                : line + ". The two differ, the kept test first: "
                    + string.Join(", ", new List<string>(differences).ToArray());
        }

        /// <summary>Whether the two carry a different priority, type or tolerance.</summary>
        internal bool Differs()
        {
            return Differences().Count > 0;
        }

        private IList<string> Differences()
        {
            List<string> found = new List<string>();

            if (keptPriority != mirrorPriority)
            {
                found.Add("priority " + Priorities.Words(keptPriority) + " and " + Priorities.Words(mirrorPriority));
            }

            if (Kept.TestType != Mirror.TestType)
            {
                found.Add("type " + Kept.TestTypeName + " and " + Mirror.TestTypeName);
            }

            // TestDrift's own epsilon, because a tolerance travels through a unit conversion
            // and an exact comparison would call two equal tolerances different.
            if (Math.Abs(Kept.Tolerance - Mirror.Tolerance) > TestDrift.ToleranceEpsilon)
            {
                found.Add("tolerance " + TestDrift.Number(Kept.Tolerance) + " " + Kept.DocumentUnits
                    + " and " + TestDrift.Number(Mirror.Tolerance) + " " + Mirror.DocumentUnits);
            }

            return found;
        }
    }
}
