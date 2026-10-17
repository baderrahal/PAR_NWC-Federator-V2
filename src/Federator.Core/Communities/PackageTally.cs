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

        private readonly List<GroupEnd> ends = new List<GroupEnd>();
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

        public int GroupCount
        {
            get { return ends.Count; }
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
                lines.Add("package        " + package.RelativePath + " : "
                    + (notRun.TryGetValue(package.RelativePath, out why) && totals.Groups == 0 ? "not run, " + why : totals.Words()));

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
                    + ", " + community.Words());
            }

            lines.Add("run            : " + PackageWords.Count(communities.Count, "community", "communities") + ", "
                + PackageWords.Count(order.Count, "package", "packages") + ", " + run.Words());

            return lines;
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
