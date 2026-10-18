using System.Globalization;

namespace Federator.Core.Communities
{
    /// <summary>
    /// Every word the packages put in the window and the log, F208, so a test reads them and no label carries a code
    /// identifier or a framework message. A label with a reason the disk gave says where to read it instead.
    /// </summary>
    public static class PackageWords
    {
        public const string Prefix = "PACKAGES";

        public const string TreeTitle = "PACKAGE TREE";

        public const string ChosenTitle = "PACKAGES TICKED";

        public const string StepHeader = "1. Packages";

        public const string StepHeading = "Pick the parent folder, whose first level holds the communities, and a Report folder "
            + "outside it, and press Read. Every folder below a community that holds NWC files directly is one package, and a "
            + "parent folder that holds NWC files itself is one package named after it. Nothing is ticked until you tick it. "
            + "The steps 2. Grouping, 3. Outputs and 4. Clash apply to each ticked package. The findings below read the NWC "
            + "files of the ticked packages.";

        /// <summary>The heading of step 3. Outputs, F232: the naming boxes and the logo only, and where every output goes.</summary>
        public const string OutputsHeading = "The names of the NWF, the NWD and the workbook of each group, and the logo of the "
            + "report page. The name is project, originator, building, level, discipline, type, number. Every output goes under "
            + "the Report folder at the package's path from the parent folder, in its NWF, NWD, Log and Clash report folders.";

        /// <summary>Why a group of a ticked package is left out of the run, F232: its box on step 2 is unticked.</summary>
        public const string UntickedOnStep2 = "unticked on 2. Grouping";

        /// <summary>Why a ticked package runs nothing, F232, each of its groups named with why in the PACKAGE GROUPS block.</summary>
        public const string NoGroupOfItRuns = "no group of it runs, each is named with why in the PACKAGE GROUPS block";

        /// <summary>The heading of the NWF page's packages, F232: the tree is read off the Report folder alone.</summary>
        public const string NwfPageHeading = "Packages. Pick the Report folder the NWC page wrote into and press Read. Tick a "
            + "community, a package or one NWF. Each ticked NWF runs in its own Navisworks.";

        public const string ParentLabel = "Parent folder";

        public const string ReportLabel = "Report folder";

        public const string ReadButton = "Read";

        public const string TickAllButton = "Tick all";

        public const string ClearAllButton = "Clear all";

        public const string ReportHelp = "Each package writes under the same path, such as Report\\C06\\Infrastructure.";

        public const string PickTheParent = "Pick the parent folder first, the folder whose first level holds the communities.";

        public const string PickTheReport = "Pick the Report folder first, outside the parent folder.";

        public const string NothingTicked = "Nothing is ticked. Tick a community or a package first.";

        public const string NotReadYet = "Pick the parent folder and press Read.";

        /// <summary>The line under the NWF page's tree before its Report folder is read, F232.</summary>
        public const string NwfNotReadYet = "Pick the Report folder and press Read.";

        /// <summary>The NWF page's refusal while no NWF is ticked, F232.</summary>
        public const string NoNwfTicked = "Nothing is ticked. Tick a community, a package or an NWF first.";

        public const string NwfTreeTitle = "NWF TREE";

        public const string NwfChosenTitle = "NWFS TICKED";

        public const string ReportWouldNotReadForALabel = "The Report folder could not be read. The log says why.";

        public const string TreeWouldNotReadForALabel = "The parent folder could not be read. The log says why.";

        /// <summary>Where the NWF page's run of packages copies each NWF before it opens it, for its confirmation.</summary>
        public const string BackupOfEach = "the Backup folder under its package's report folder, one copy of each NWF";

        public static string ParentNotThere(string parent)
        {
            return "The parent folder is not there: " + parent;
        }

        public static string ParentWouldNotRead(string parent, string said)
        {
            return "The parent folder " + parent + " could not be read, " + Or(said, "UNKNOWN why");
        }

        /// <summary>
        /// F232. The parent's own files would be a package named after it, and a folder under it carries that name too, so
        /// the two would be one community twice. The files are left out and said, Q213.
        /// </summary>
        public static string ParentItselfNamedTwice(int count, string extension, string name)
        {
            return FilesWord(count, extension) + " sit in the parent folder itself, which would be the package " + name
                + ", and a folder under it carries that name too, so they are in no package";
        }

        public static string ReportNotThere(string report)
        {
            return "The Report folder is not there: " + report;
        }

        public static string ReportWouldNotRead(string report, string said)
        {
            return "The Report folder " + report + " could not be read, " + Or(said, "UNKNOWN why");
        }

        public static string LinkNotFollowed(string path)
        {
            return path + " is a link or a junction and was not followed";
        }

        public static string CommunityHoldsNone(string community, string extension)
        {
            return "community " + community + " holds no " + KindOf(extension) + " file in any folder below it, so it has no package";
        }

        public static string FolderWouldNotRead(string relativePath, string said)
        {
            return relativePath + " could not be read, " + Or(said, "UNKNOWN why") + ", so nothing under it is in a package";
        }

        /// <summary>The name of the package that is a community folder's own files, the flat layout.</summary>
        public static string ItselfName(string community)
        {
            return community + " itself";
        }

        /// <summary>The words beside a package in the tree: its files and how many groups they make.</summary>
        public static string Detail(int files, string extension, int groups, int blocked)
        {
            return FilesWord(files, extension) + ", " + Count(groups, "group", "groups")
                + (blocked > 0 ? ", " + blocked + " blocked" : string.Empty);
        }

        /// <summary>The words beside a community in the tree.</summary>
        public static string CommunityDetail(int packages, int files, string extension)
        {
            return Count(packages, "package", "packages") + ", " + FilesWord(files, extension);
        }

        /// <summary>The one line under the tree, what was read and how much is ticked.</summary>
        public static string Summary(PackageTree tree, PackageTicks ticks)
        {
            if (tree == null)
            {
                return NotReadYet;
            }

            if (!tree.WasRead)
            {
                return tree.WhyNoTreeForALabel;
            }

            int ticked = ticks == null ? 0 : ticks.Ticked().Count;

            return Count(tree.Communities.Count, "community", "communities") + ", "
                + Count(tree.Packages().Count, "package", "packages") + ", "
                + FilesWord(tree.FileCount, tree.Extension) + ". "
                + Count(ticked, "package", "packages") + " ticked."
                + (tree.LeftOut.Count > 0 ? " " + Count(tree.LeftOut.Count, "thing", "things") + " left out, the log names each." : string.Empty);
        }

        /// <summary>The one line under the NWF page's tree, F232: what was read off the Report folder and how many NWFs are ticked.</summary>
        public static string NwfSummary(PackageTree tree, NwfTicks ticks)
        {
            if (tree == null)
            {
                return NwfNotReadYet;
            }

            if (!tree.WasRead)
            {
                return tree.WhyNoTreeForALabel;
            }

            int ticked = ticks == null ? 0 : ticks.Ticked().Count;

            return Count(tree.Communities.Count, "community", "communities") + ", "
                + Count(tree.Packages().Count, "package", "packages") + ", "
                + FilesWord(tree.FileCount, tree.Extension) + ". "
                + FilesWord(ticked, tree.Extension) + " ticked."
                + (tree.LeftOut.Count > 0 ? " " + Count(tree.LeftOut.Count, "thing", "things") + " left out, the log names each." : string.Empty);
        }

        /// <summary>The NWF page's line for a ticked package with no NWF in its NWF folder.</summary>
        public static string NoNwfYet(string relativePath, string nwfFolder)
        {
            return relativePath + " has no NWF in " + nwfFolder + ", so nothing of it was run. Run the NWC page on it first";
        }

        /// <summary>The word for the kind of file, NWC for .nwc, read off the extension and never typed per kind.</summary>
        public static string KindOf(string extension)
        {
            string trimmed = (extension ?? string.Empty).TrimStart('.');
            return trimmed.Length == 0 ? "matching" : trimmed.ToUpperInvariant();
        }

        /// <summary>
        /// One package on a line of the PACKAGE TREE and PACKAGES TICKED blocks: its path from the parent, which every group
        /// line names it by, its files, then its full path, Bader's F-R4 of 18 Oct 2026, F216, so each line alone says where
        /// the package is.
        /// </summary>
        public static string PackageLine(Package package, string extension)
        {
            return package.RelativePath + "  " + FilesWord(package.Files.Count, extension) + ", at " + package.FullPath;
        }

        public static string FilesWord(int count, string extension)
        {
            return count.ToString(CultureInfo.InvariantCulture) + " " + KindOf(extension) + (count == 1 ? " file" : " files");
        }

        public static string Count(int count, string one, string many)
        {
            return count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);
        }

        private static string Or(string value, string otherwise)
        {
            return string.IsNullOrEmpty(value) ? otherwise : value;
        }
    }
}
