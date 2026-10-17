using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Federator.Core.Generic;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using Federator.Core.Rerun;

namespace Federator.Core.Communities
{
    /// <summary>
    /// Where a package writes, F208, Bader's message of 17 Oct 2026: under the Report folder he picks, at the same path the
    /// package has under the parent, such as Report\C06\Infrastructure. The NWF and the NWD go in their own folders under
    /// it, so ReportPaths.LogFolder puts the Log and the Clash report there as well, and every output of a package is under
    /// its one folder. A package not ticked has no folder made and nothing written under it.
    /// </summary>
    public static class PackageOutput
    {
        /// <summary>The folder under a package's report folder that holds its NWFs. A setting, never typed elsewhere.</summary>
        public const string DefaultNwfSubfolder = "NWF";

        /// <summary>The folder under a package's report folder that holds its NWDs.</summary>
        public const string DefaultNwdSubfolder = "NWD";

        /// <summary>
        /// Why the Report folder cannot be used, or null where it can: not picked, not a full path, the parent itself or
        /// inside it. Inside the parent a report would sit in a folder the next Read walks, which the rule that the reports
        /// never go inside the folder being scanned refuses.
        /// </summary>
        public static string WhyNot(string parent, string reportRoot)
        {
            string report = reportRoot == null ? string.Empty : reportRoot.Trim();

            if (report.Length == 0)
            {
                return PackageWords.PickTheReport;
            }

            if (!IsFull(report))
            {
                return "The Report folder must be a full path with its drive: " + report;
            }

            string from = parent == null ? string.Empty : parent.Trim();

            if (from.Length > 0 && IsFull(from) && IsInsideOrSame(report, from))
            {
                return "The Report folder " + report + " is the parent folder or inside it, so a report could be read as a "
                    + "package. Pick a folder outside " + from + ".";
            }

            return null;
        }

        /// <summary>The package's report folder, the Report folder and the package's path from the parent.</summary>
        public static string FolderFor(string reportRoot, Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            return Path.Combine(Trimmed(reportRoot), package.RelativePath);
        }

        public static string NwfFolderFor(string reportRoot, Package package)
        {
            return Path.Combine(FolderFor(reportRoot, package), DefaultNwfSubfolder);
        }

        public static string NwdFolderFor(string reportRoot, Package package)
        {
            return Path.Combine(FolderFor(reportRoot, package), DefaultNwdSubfolder);
        }

        /// <summary>The folders a ticked package needs before its first group, its own, then the NWF and the NWD folders.</summary>
        public static IList<string> FoldersToMake(string reportRoot, Package package)
        {
            return new List<string> { FolderFor(reportRoot, package), NwfFolderFor(reportRoot, package), NwdFolderFor(reportRoot, package) };
        }

        /// <summary>The line for a folder made by the run.</summary>
        public static string MadeLine(string folder)
        {
            return PackageWords.Prefix + " folder made     " + folder;
        }

        /// <summary>The line for a folder that was already there.</summary>
        public static string ThereLine(string folder)
        {
            return PackageWords.Prefix + " folder there    " + folder;
        }

        private static string Trimmed(string folder)
        {
            return (folder ?? string.Empty).Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static bool IsFull(string path)
        {
            try
            {
                return Path.IsPathRooted(path) && Path.GetPathRoot(path).Length > 1;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static bool IsInsideOrSame(string inner, string outer)
        {
            string a;
            string b;

            try
            {
                a = Trimmed(Path.GetFullPath(inner));
                b = Trimmed(Path.GetFullPath(outer));
            }
            catch (Exception)
            {
                return false;
            }

            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase)
                || a.StartsWith(b + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>One group of one package, decided before the run starts: its files, where its NWF and NWD go, its names.</summary>
    public sealed class PackageGroup
    {
        internal PackageGroup(
            Package package,
            string building,
            IList<string> files,
            string nwfName,
            string nwfPath,
            string nwdPath,
            string workbookName,
            IList<string> disciplines)
        {
            Package = package;
            Building = building ?? string.Empty;
            Files = new ReadOnlyCollection<string>(new List<string>(files ?? new List<string>()));
            NwfName = nwfName ?? string.Empty;
            NwfPath = nwfPath ?? string.Empty;
            NwdPath = nwdPath ?? string.Empty;
            WorkbookName = workbookName ?? string.Empty;
            Disciplines = new ReadOnlyCollection<string>(new List<string>(disciplines ?? new List<string>()));
        }

        public Package Package { get; private set; }

        public string Building { get; private set; }

        public ReadOnlyCollection<string> Files { get; private set; }

        public string NwfName { get; private set; }

        public string NwfPath { get; private set; }

        public string NwdPath { get; private set; }

        public string WorkbookName { get; private set; }

        public ReadOnlyCollection<string> Disciplines { get; private set; }

        /// <summary>The group named with its community and package, the words every GROUP line and RESULT carry.</summary>
        public string Label
        {
            get { return PackagePlan.LabelFor(Package == null ? string.Empty : Package.RelativePath, Building); }
        }
    }

    /// <summary>
    /// What one package runs, F208: its files parsed with the naming settings, gathered by the grouping chosen on the
    /// Grouping step, named by the patterns of the Outputs step, each group's NWF and NWD in the package's own folders. A
    /// name two groups of the package would share is refused before anything runs, by the guard the scanned run uses, and a
    /// package refused that way runs no group while the others run.
    /// </summary>
    public sealed class PackagePlan
    {
        private readonly List<PackageGroup> groups = new List<PackageGroup>();
        private readonly List<string> notRun = new List<string>();

        private PackagePlan(Package package, string reportRoot)
        {
            Package = package;
            ReportFolder = PackageOutput.FolderFor(reportRoot, package);
            NwfFolder = PackageOutput.NwfFolderFor(reportRoot, package);
            NwdFolder = PackageOutput.NwdFolderFor(reportRoot, package);
        }

        public Package Package { get; private set; }

        public string ReportFolder { get; private set; }

        public string NwfFolder { get; private set; }

        public string NwdFolder { get; private set; }

        /// <summary>The groups to run, in the grouping's order.</summary>
        public IList<PackageGroup> Groups
        {
            get { return groups.AsReadOnly(); }
        }

        /// <summary>A group blocked by the grouping and a file whose name does not read, each with why.</summary>
        public IList<string> NotRun
        {
            get { return notRun.AsReadOnly(); }
        }

        public int Blocked { get; private set; }

        /// <summary>Why no group of this package can run, the name guard's words, or null where they can.</summary>
        public string WhyNot { get; private set; }

        /// <summary>The group named with its package, the package's path from the parent then the group.</summary>
        public static string LabelFor(string relativePath, string building)
        {
            return string.IsNullOrEmpty(relativePath) ? (building ?? string.Empty) : relativePath + "  " + building;
        }

        public static PackagePlan For(
            Package package,
            string reportRoot,
            GroupingMode mode,
            ContainerNameSettings settings,
            OutputNaming naming,
            GenericModelsSettings internalWorkbook,
            DateTime today)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (naming == null)
            {
                throw new ArgumentNullException("naming");
            }

            if (internalWorkbook == null)
            {
                throw new ArgumentNullException("internalWorkbook");
            }

            PackagePlan plan = new PackagePlan(package, reportRoot);
            List<ParsedContainerName> readable = new List<ParsedContainerName>();
            Dictionary<string, string> pathOfStem = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (string file in package.Files)
            {
                ParsedContainerName parsed = ContainerName.Parse(file, settings);

                if (!parsed.IsReadable)
                {
                    plan.notRun.Add("cannot be read  " + Path.GetFileName(file) + "  " + parsed.UnreadableReason);
                    continue;
                }

                readable.Add(parsed);
                pathOfStem[parsed.Stem] = file;
            }

            BuildingGroupingResult result = BuildingGrouping.Group(readable, mode, settings);

            foreach (SkippedBuildingGroup skipped in result.Skipped)
            {
                plan.notRun.Add("BLOCKED  " + skipped.Building + "  " + skipped.Reason);
                plan.Blocked++;
            }

            OutputNameTable names = OutputNameTable.From(result.Groups, naming, settings, today);
            plan.WhyNot = names.WhyTheRunCannotStart(internalWorkbook);

            foreach (BuildingGroup group in result.Groups)
            {
                OutputNameRow row = names.Find(group.Building);
                List<string> files = new List<string>();

                foreach (ParsedContainerName parsed in group.Files)
                {
                    string path;

                    if (pathOfStem.TryGetValue(parsed.Stem, out path) && !files.Contains(path))
                    {
                        files.Add(path);
                    }
                }

                plan.groups.Add(new PackageGroup(
                    package,
                    group.Building,
                    files,
                    row.NwfName,
                    OutputPaths.Nwf(plan.NwfFolder, row.NwfName),
                    OutputPaths.Nwd(plan.NwdFolder, row.NwdName),
                    row.WorkbookName,
                    group.Disciplines));
            }

            if (plan.WhyNot == null && plan.groups.Count == 0)
            {
                plan.WhyNot = package.RelativePath + " makes no group that can run, so nothing of it is run";
            }

            return plan;
        }
    }
}
