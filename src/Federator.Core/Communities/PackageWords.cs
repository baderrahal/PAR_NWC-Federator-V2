using System;
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

        public const string AcrossTheRunTitle = "PACKAGES ACROSS THE RUN";

        public const string StepHeader = "5. Packages";

        public const string StepHeading = "Many communities at once. Pick the parent folder, whose first level holds the "
            + "communities, and a Report folder outside it. Every folder below a community that holds NWC files directly "
            + "is one package. The steps 2 to 4 apply to each. Nothing is ticked until you tick it.";

        public const string NwfPageHeading = "Many communities at once, the tree of the NWC page. Each ticked package runs "
            + "the NWFs in the NWF folder under its report folder.";

        public const string ParentLabel = "Parent folder";

        public const string ReportLabel = "Report folder";

        public const string ReadButton = "Read";

        public const string TickAllButton = "Tick all";

        public const string ClearAllButton = "Clear all";

        public const string RunButton = "Run ticked packages";

        public const string ReportHelp = "Each package writes under the same path, such as Report\\C06\\Infrastructure.";

        public const string PickTheParent = "Pick the parent folder first, the folder whose first level holds the communities.";

        public const string PickTheReport = "Pick the Report folder first, outside the parent folder.";

        public const string NothingTicked = "Nothing is ticked. Tick a community or a package first.";

        public const string NotReadYet = "Pick the parent folder and press Read.";

        public const string TreeWouldNotReadForALabel = "The parent folder could not be read. The log says why.";

        public static string ParentNotThere(string parent)
        {
            return "The parent folder is not there: " + parent;
        }

        public static string ParentWouldNotRead(string parent, string said)
        {
            return "The parent folder " + parent + " could not be read, " + Or(said, "UNKNOWN why");
        }

        public static string InTheParentItself(int count, string extension)
        {
            return FilesWord(count, extension) + " sit in the parent folder itself, in no community, so they are in no package";
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
