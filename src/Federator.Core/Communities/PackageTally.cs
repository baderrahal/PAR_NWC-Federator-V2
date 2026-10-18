using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Diagnostics;

namespace Federator.Core.Communities
{
    /// <summary>
    /// How every group of a run of packages ended, F208, Bader's message of 17 Oct 2026: each group named with its
    /// community and package, one total per package, one per community and one for the run, in RESULT. A package that ran
    /// no group is named with why, so a package never drops out of the count in silence. A clash count not taken is UNKNOWN
    /// and never nought, and a total holding one is a lower bound that says at least.
    /// </summary>
    public sealed class PackageTally
    {
        private sealed class GroupEnd
        {
            public Package Package;
            public string Group;
            public GroupOutcome Outcome;
            public long Clashes;
        }

        private sealed class Totals
        {
            public int Groups;
            public int Done;
            public int Partial;
            public int Failed;
            public long Clashes;
            public bool ClashesUnknown;
            public readonly List<string> Packages = new List<string>();

            public void Add(GroupEnd end)
            {
                Groups++;

                switch (end.Outcome)
                {
                    case GroupOutcome.Done:
                        Done++;
                        break;
                    case GroupOutcome.Partial:
                        Partial++;
                        break;
                    default:
                        Failed++;
                        break;
                }

                if (end.Clashes < 0)
                {
                    ClashesUnknown = true;
                }
                else
                {
                    Clashes += end.Clashes;
                }
            }

            public string Words()
            {
                return PackageWords.Count(Groups, "group", "groups") + ", " + Done + " done, " + Partial + " partial, "
                    + Failed + " failed, " + ClashWords(Clashes, ClashesUnknown);
            }
        }

        /// <summary>A group that has no end to count, F222: its Navisworks wrote no result, or it ran nothing there.</summary>
        private sealed class NoEnd
        {
            public Package Package;
            public string Group;
            public string Words;
            public bool NoResult;
        }

        private readonly List<GroupEnd> ends = new List<GroupEnd>();
        private readonly List<NoEnd> noEnds = new List<NoEnd>();
        private readonly List<NoEnd> notBuilt = new List<NoEnd>();
        private readonly List<Package> order = new List<Package>();
        private readonly Dictionary<string, string> notRun = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>A package of the run, in the order it runs, so a package that ran nothing is still counted.</summary>
        public void Expect(Package package)
        {
            if (package != null && !order.Exists(p => string.Equals(p.RelativePath, package.RelativePath, StringComparison.OrdinalIgnoreCase)))
            {
                order.Add(package);
            }
        }

        /// <summary>One group's end. Clashes is minus one where the count was not taken.</summary>
        public void Add(Package package, string group, GroupOutcome outcome, long clashes)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            Expect(package);
            ends.Add(new GroupEnd { Package = package, Group = group ?? string.Empty, Outcome = outcome, Clashes = clashes });
        }

        /// <summary>A package that ran no group, and why. The first reason given is the one kept.</summary>
        public void NotRun(Package package, string why)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            Expect(package);

            if (!notRun.ContainsKey(package.RelativePath))
            {
                notRun.Add(package.RelativePath, string.IsNullOrEmpty(why) ? "UNKNOWN why" : why);
            }
        }

        /// <summary>
        /// A group whose own Navisworks wrote no result, F214 and since F222 one group a Navisworks, and why. It is never
        /// said as not run, since it may have run and written files before its Navisworks ended.
        /// </summary>
        public void NoResult(Package package, string group, string why)
        {
            AddNoEnd(package, group, PackageRunWords.NoResult(why), true);
        }

        /// <summary>A group whose own Navisworks ran nothing, F222, and why, its result read and naming no group.</summary>
        public void NotRun(Package package, string group, string why)
        {
            AddNoEnd(package, group, PackageRunWords.GroupNotRun(why), false);
        }

        /// <summary>
        /// A group of one discipline, not built, F233, Bader's round of 19 Oct 2026, fix 3: named on its own line and counted
        /// per community, never among the groups done, partial or failed, and never as a group with no end to count.
        /// </summary>
        public void NotBuilt(Package package, string group)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            Expect(package);
            notBuilt.Add(new NoEnd { Package = package, Group = group ?? string.Empty, Words = OneDisciplineGroups.Words });
        }

        /// <summary>How many groups were not built for one discipline, F233.</summary>
        public int NotBuiltCount
        {
            get { return notBuilt.Count; }
        }

        private void AddNoEnd(Package package, string group, string words, bool noResult)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            Expect(package);
            noEnds.Add(new NoEnd { Package = package, Group = group ?? string.Empty, Words = words, NoResult = noResult });
        }

        public int GroupCount
        {
            get { return ends.Count; }
        }

        /// <summary>How many groups wrote no result, F222, read by the one RESULT of a run in fresh starts.</summary>
        public int NoResultCount
        {
            get { return noEnds.FindAll(end => end.NoResult).Count; }
        }

        /// <summary>How many groups ran nothing in their own Navisworks, F222.</summary>
        public int NotRunCount
        {
            get { return noEnds.FindAll(end => !end.NoResult).Count; }
        }

        /// <summary>How many of the groups ended this way, F214, read by the one RESULT of a run in fresh starts.</summary>
        public int CountOf(GroupOutcome outcome)
        {
            return ends.FindAll(end => end.Outcome == outcome).Count;
        }

        /// <summary>The lines RESULT carries: each group, then each package, each community and the run.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (order.Count == 0)
            {
                lines.Add("packages       : none ran in this run");
                return lines;
            }

            foreach (GroupEnd end in ends)
            {
                lines.Add("group          " + end.Package.RelativePath + "  " + end.Group + "  "
                    + end.Outcome.ToString().ToUpperInvariant() + "  " + ClashWords(end.Clashes, end.Clashes < 0));
            }

            foreach (NoEnd end in noEnds)
            {
                lines.Add("group          " + end.Package.RelativePath + "  " + end.Group + "  " + end.Words);
            }

            foreach (NoEnd end in notBuilt)
            {
                lines.Add("group          " + end.Package.RelativePath + "  " + end.Group + "  " + end.Words);
            }

            Dictionary<string, Totals> byCommunity = new Dictionary<string, Totals>(StringComparer.OrdinalIgnoreCase);
            List<string> communities = new List<string>();
            Totals run = new Totals();

            foreach (Package package in order)
            {
                Totals totals = new Totals();

                foreach (GroupEnd end in ends)
                {
                    if (string.Equals(end.Package.RelativePath, package.RelativePath, StringComparison.OrdinalIgnoreCase))
                    {
                        totals.Add(end);
                    }
                }

                string why;
                int without = noEnds.FindAll(end => string.Equals(end.Package.RelativePath, package.RelativePath, StringComparison.OrdinalIgnoreCase)).Count;
                lines.Add("package        " + package.RelativePath + " : "
                    + (notRun.TryGetValue(package.RelativePath, out why) && totals.Groups == 0 && without == 0
                        ? "not run, " + why
                        : totals.Words() + (without == 0 ? string.Empty
                            : ", and " + PackageWords.Count(without, "group", "groups") + " with no end to count, each named above")));

                Totals community;

                if (!byCommunity.TryGetValue(package.Community, out community))
                {
                    community = new Totals();
                    byCommunity.Add(package.Community, community);
                    communities.Add(package.Community);
                }

                community.Packages.Add(package.RelativePath);

                foreach (GroupEnd end in ends)
                {
                    if (string.Equals(end.Package.RelativePath, package.RelativePath, StringComparison.OrdinalIgnoreCase))
                    {
                        community.Add(end);
                        run.Add(end);
                    }
                }
            }

            foreach (string name in communities)
            {
                Totals community = byCommunity[name];
                lines.Add("community      " + name + " : " + PackageWords.Count(community.Packages.Count, "package", "packages")
                    + ", " + community.Words() + NoEndWords(noEnds.FindAll(end => string.Equals(end.Package.Community, name, StringComparison.OrdinalIgnoreCase)).Count));
            }

            // F233. One line per community that holds a group of one discipline, with the count.
            foreach (string name in communities)
            {
                int count = notBuilt.FindAll(end => string.Equals(end.Package.Community, name, StringComparison.OrdinalIgnoreCase)).Count;

                if (count > 0)
                {
                    lines.Add(OneDisciplineGroups.ResultLine(name, count));
                }
            }

            lines.Add("run            : " + PackageWords.Count(communities.Count, "community", "communities") + ", "
                + PackageWords.Count(order.Count, "package", "packages") + ", " + run.Words() + NoEndWords(noEnds.Count));

            return lines;
        }

        /// <summary>
        /// F222. What a total of the community or the run leaves out: its groups with no end to count, which may hold clashes in
        /// files on the disk, so the clashes of the total are a lower bound and never read as exact.
        /// </summary>
        private static string NoEndWords(int count)
        {
            return count == 0
                ? string.Empty
                : ", and " + PackageWords.Count(count, "group", "groups") + " with no end to count, each named above, so the clashes are at least that";
        }

        private static string ClashWords(long clashes, bool unknown)
        {
            if (unknown)
            {
                return clashes > 0
                    ? "at least " + clashes.ToString(CultureInfo.InvariantCulture) + " clashes, a count was not taken"
                    : "clashes UNKNOWN";
            }

            return clashes.ToString(CultureInfo.InvariantCulture) + (clashes == 1 ? " clash" : " clashes");
        }
    }
}
