using System;
using System.Collections.Generic;
using Federator.Core.Disciplines;

namespace Federator.Core.Communities
{
    /// <summary>
    /// F222, Bader's answer to Q211 of 18 Oct 2026: each group runs in a Navisworks of its own, so no one Navisworks holds
    /// every group of a package and the discipline reports of F209, which read every group of it, are written by the window
    /// that started the run, once every group of the package has run, off the facts each group's result carried back. This
    /// is the one rule that turns those results into the groups a package's workbooks read. Where a group has no facts the
    /// group still stands with its files and why, so the checks of the file names still run on it and the others say not
    /// read, as F209 says of a group that did not run.
    /// </summary>
    public static class PackageRunFacts
    {
        /// <summary>The words of a plan group the run left out, an NWF already there on the NWC page among them.</summary>
        public const string DidNotRun = "this group did not run in this run";

        /// <summary>
        /// The groups of one package for its discipline workbooks. On the NWC page, a plan, every group of the plan in its
        /// order, each with the facts of the group of the run whose NWF it writes. On the NWF page, no plan, each group of the
        /// run in this package in the run's order, with the NWF's files as its result names them. A result not there, one
        /// whose Navisworks ran nothing and one whose facts were not read each give the group no facts and say why.
        /// </summary>
        public static IList<DisciplineGroup> GroupsOf(
            Package package, PackagePlan plan, IList<PackageRunUnit> units, IList<PackageRunResult> results, IList<string> noResultWhy)
        {
            if (package == null || units == null || results == null || noResultWhy == null
                || results.Count != units.Count || noResultWhy.Count != units.Count)
            {
                throw new ArgumentException("a package, and one result or one reason for each group of the run");
            }

            List<DisciplineGroup> groups = new List<DisciplineGroup>();

            if (plan != null)
            {
                foreach (PackageGroup group in plan.Groups)
                {
                    int i = IndexOf(units, package, group.NwfPath);

                    if (i < 0)
                    {
                        groups.Add(new DisciplineGroup(group.Building, group.Files, null, DidNotRun));
                        continue;
                    }

                    PackageRunGroup ran = TheGroupOf(results[i]);
                    groups.Add(new DisciplineGroup(group.Building, group.Files, ran == null ? null : ran.Facts, WhyNoFacts(results[i], noResultWhy[i], ran)));
                }

                return groups;
            }

            for (int i = 0; i < units.Count; i++)
            {
                if (!string.Equals(units[i].Package, package.RelativePath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                PackageRunGroup ran = TheGroupOf(results[i]);
                List<string> files = new List<string>();

                if (ran != null && ran.Facts != null)
                {
                    foreach (ModelFacts model in ran.Facts)
                    {
                        files.Add(model.File);
                    }
                }
                else if (ran != null)
                {
                    files.AddRange(ran.Files);
                }

                groups.Add(new DisciplineGroup(ran == null ? units[i].Name : ran.Group, files, ran == null ? null : ran.Facts,
                    WhyNoFacts(results[i], noResultWhy[i], ran)));
            }

            return groups;
        }

        private static int IndexOf(IList<PackageRunUnit> units, Package package, string nwf)
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (string.Equals(units[i].Package, package.RelativePath, StringComparison.OrdinalIgnoreCase)
                    && units[i].IsTheGroupOf(nwf))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The one group a group's Navisworks ran, or null where it ran none.</summary>
        private static PackageRunGroup TheGroupOf(PackageRunResult result)
        {
            return result == null || result.Groups.Count == 0 ? null : result.Groups[0];
        }

        private static string WhyNoFacts(PackageRunResult result, string noResultWhy, PackageRunGroup ran)
        {
            if (result == null)
            {
                return "its Navisworks wrote no result, " + (string.IsNullOrEmpty(noResultWhy) ? "UNKNOWN why" : noResultWhy);
            }

            if (ran == null)
            {
                return "its own Navisworks ran nothing, " + (result.NoGroupBecause ?? "UNKNOWN why");
            }

            return ran.FactsNotRead;
        }
    }
}
