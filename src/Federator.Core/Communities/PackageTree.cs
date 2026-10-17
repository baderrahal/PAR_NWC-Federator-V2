using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Federator.Core.Rerun;

namespace Federator.Core.Communities
{
    /// <summary>
    /// One folder that directly holds files of the kind read, F208, Bader's message of 17 Oct 2026. Its own files only,
    /// never a file of a folder under it, so two folders are never mixed into one package.
    /// </summary>
    public sealed class Package
    {
        internal Package(string community, string relativePath, string fullPath, IList<string> files)
        {
            Community = community ?? string.Empty;
            RelativePath = relativePath ?? string.Empty;
            FullPath = fullPath ?? string.Empty;
            Files = new ReadOnlyCollection<string>(new List<string>(files ?? new List<string>()));
        }

        /// <summary>The folder at the first level under the parent that this package sits in.</summary>
        public string Community { get; private set; }

        /// <summary>The path from the parent, the community first, such as C06 or C06\Infrastructure and Landscape.</summary>
        public string RelativePath { get; private set; }

        public string FullPath { get; private set; }

        /// <summary>The full paths of its files, in name order without case.</summary>
        public ReadOnlyCollection<string> Files { get; private set; }

        /// <summary>True where the package is the community folder itself, the flat layout.</summary>
        public bool IsTheCommunityItself
        {
            get { return string.Equals(RelativePath, Community, StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>The path below the community, or the words for the community's own files.</summary>
        public string Name
        {
            get
            {
                return IsTheCommunityItself
                    ? PackageWords.ItselfName(Community)
                    : RelativePath.Substring(Community.Length + 1);
            }
        }

        public override string ToString()
        {
            return RelativePath;
        }
    }

    /// <summary>One folder at the first level under the parent, with the packages found anywhere below it.</summary>
    public sealed class Community
    {
        private readonly List<Package> packages = new List<Package>();

        internal Community(string name, string fullPath)
        {
            Name = name ?? string.Empty;
            FullPath = fullPath ?? string.Empty;
        }

        public string Name { get; private set; }

        public string FullPath { get; private set; }

        public IList<Package> Packages
        {
            get { return packages.AsReadOnly(); }
        }

        internal void Add(Package package)
        {
            packages.Add(package);
        }

        public int FileCount
        {
            get
            {
                int count = 0;

                foreach (Package package in packages)
                {
                    count += package.Files.Count;
                }

                return count;
            }
        }
    }

    /// <summary>
    /// The communities and packages under one parent folder, F208, Bader's message of 17 Oct 2026. The first level under
    /// the parent is the community, such as C06 and C07. Every folder at any depth below a community that directly holds
    /// files of the kind read is one package, its own files only, so a folder holding files and subfolders with files is
    /// one package and each subfolder another. The local test folder, a community holding its files itself, and the ACC
    /// layout, a community holding folders of files, are read by the one rule. A folder is never matched by a wildcard:
    /// each is listed whole and a file is judged by its extension alone, read case blind. A link or a junction is never
    /// followed and is named, so a folder that points back up the tree cannot make the walk go round. A folder that will
    /// not read is named with what the disk said and the rest are read. Nothing here acts on what it finds.
    /// </summary>
    public sealed class PackageTree
    {
        /// <summary>The extension the NWC page's packages hold.</summary>
        public const string NwcExtension = ".nwc";

        private readonly List<Community> communities = new List<Community>();
        private readonly List<string> leftOut = new List<string>();

        private PackageTree(string parent, string extension)
        {
            Parent = parent ?? string.Empty;
            Extension = extension ?? string.Empty;
            WhyNoTree = string.Empty;
            WhyNoTreeForALabel = string.Empty;
        }

        public string Parent { get; private set; }

        /// <summary>The extension of the files a package holds, with its dot, such as .nwc.</summary>
        public string Extension { get; private set; }

        /// <summary>The communities that hold at least one package, in name order without case.</summary>
        public IList<Community> Communities
        {
            get { return communities.AsReadOnly(); }
        }

        /// <summary>
        /// What was found and is in no package, each with why: files in the parent itself, a community with no file of
        /// the kind anywhere below it, a folder that would not read, a link not followed. Said in the log, never dropped.
        /// </summary>
        public IList<string> LeftOut
        {
            get { return leftOut.AsReadOnly(); }
        }

        /// <summary>Why no tree was read at all, for the log and the warning, or empty where one was.</summary>
        public string WhyNoTree { get; private set; }

        /// <summary>The same reason with nothing the disk said in it, since a label never carries a framework message.</summary>
        public string WhyNoTreeForALabel { get; private set; }

        public bool WasRead
        {
            get { return WhyNoTree.Length == 0; }
        }

        /// <summary>Every package of every community, communities in order and each community's packages in walk order.</summary>
        public IList<Package> Packages()
        {
            List<Package> all = new List<Package>();

            foreach (Community community in communities)
            {
                all.AddRange(community.Packages);
            }

            return all;
        }

        public int FileCount
        {
            get
            {
                int count = 0;

                foreach (Community community in communities)
                {
                    count += community.FileCount;
                }

                return count;
            }
        }

        /// <summary>The package at a path from the parent, compared without case as Windows names a folder, or null.</summary>
        public Package Find(string relativePath)
        {
            foreach (Package package in Packages())
            {
                if (string.Equals(package.RelativePath, relativePath, StringComparison.OrdinalIgnoreCase))
                {
                    return package;
                }
            }

            return null;
        }

        /// <summary>Whether a path is a file of the kind read, by its extension alone, case blind.</summary>
        public static bool IsOfTheKind(string path, string extension)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(extension))
            {
                return false;
            }

            string found;

            try
            {
                found = Path.GetExtension(path);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return string.Equals(found, extension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>A folder Windows marks as a reparse point, which a link and a junction are. Never throws.</summary>
        public static bool IsALinkOnTheDisk(string folder)
        {
            try
            {
                return (File.GetAttributes(folder) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>The tree off the disk itself.</summary>
        public static PackageTree Read(string parent, string extension)
        {
            return Read(parent, extension, NwfPickDisk.Real, IsALinkOnTheDisk);
        }

        /// <summary>The tree off a disk handed in, so every rule is proved on folders a test makes.</summary>
        public static PackageTree Read(string parent, string extension, NwfPickDisk disk, Func<string, bool> isLink)
        {
            if (disk == null)
            {
                throw new ArgumentNullException("disk");
            }

            if (isLink == null)
            {
                throw new ArgumentNullException("isLink");
            }

            string trimmed = parent == null ? string.Empty : parent.Trim();
            PackageTree tree = new PackageTree(trimmed.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), extension);

            if (trimmed.Length == 0)
            {
                tree.WhyNoTree = PackageWords.PickTheParent;
                tree.WhyNoTreeForALabel = tree.WhyNoTree;
                return tree;
            }

            if (!disk.IsFolder(tree.Parent))
            {
                tree.WhyNoTree = PackageWords.ParentNotThere(tree.Parent);
                tree.WhyNoTreeForALabel = tree.WhyNoTree;
                return tree;
            }

            IList<string> files;
            IList<string> folders;

            try
            {
                files = disk.FilesIn(tree.Parent);
                folders = disk.FoldersIn(tree.Parent);
            }
            catch (Exception error)
            {
                tree.WhyNoTree = PackageWords.ParentWouldNotRead(tree.Parent, error.Message);
                tree.WhyNoTreeForALabel = PackageWords.TreeWouldNotReadForALabel;
                return tree;
            }

            List<string> loose = Sorted(OfTheKind(files, tree.Extension));

            if (loose.Count > 0)
            {
                tree.leftOut.Add(PackageWords.InTheParentItself(loose.Count, tree.Extension));
            }

            foreach (string folder in Sorted(folders))
            {
                string name = Leaf(folder);

                if (isLink(folder))
                {
                    tree.leftOut.Add(PackageWords.LinkNotFollowed(name));
                    continue;
                }

                Community community = new Community(name, folder);
                tree.Walk(community, folder, name, disk, isLink);

                if (community.Packages.Count == 0)
                {
                    tree.leftOut.Add(PackageWords.CommunityHoldsNone(name, tree.Extension));
                    continue;
                }

                tree.communities.Add(community);
            }

            return tree;
        }

        private void Walk(Community community, string folder, string relativePath, NwfPickDisk disk, Func<string, bool> isLink)
        {
            IList<string> files;
            IList<string> folders;

            try
            {
                files = disk.FilesIn(folder);
                folders = disk.FoldersIn(folder);
            }
            catch (Exception error)
            {
                leftOut.Add(PackageWords.FolderWouldNotRead(relativePath, error.Message));
                return;
            }

            List<string> own = Sorted(OfTheKind(files, Extension));

            if (own.Count > 0)
            {
                community.Add(new Package(community.Name, relativePath, folder, own));
            }

            foreach (string sub in Sorted(folders))
            {
                string path = relativePath + Path.DirectorySeparatorChar + Leaf(sub);

                if (isLink(sub))
                {
                    leftOut.Add(PackageWords.LinkNotFollowed(path));
                    continue;
                }

                Walk(community, sub, path, disk, isLink);
            }
        }

        private static List<string> OfTheKind(IList<string> files, string extension)
        {
            List<string> kept = new List<string>();

            foreach (string file in files ?? new List<string>())
            {
                if (IsOfTheKind(file, extension))
                {
                    kept.Add(file);
                }
            }

            return kept;
        }

        private static List<string> Sorted(IEnumerable<string> paths)
        {
            List<string> sorted = new List<string>(paths ?? new List<string>());
            sorted.Sort(StringComparer.OrdinalIgnoreCase);
            return sorted;
        }

        /// <summary>The last name of a path, read the way Windows splits it, never by a separator this machine guesses.</summary>
        private static string Leaf(string path)
        {
            string trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            int at = trimmed.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            return at < 0 ? trimmed : trimmed.Substring(at + 1);
        }

        /// <summary>The PACKAGE TREE block: the parent, the counts, every community and package with its path, and what was left out.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            lines.Add("parent      : " + (Parent.Length == 0 ? "none" : Parent));

            if (!WasRead)
            {
                lines.Add("not read    : " + WhyNoTree);
                return lines;
            }

            lines.Add("found       : " + PackageWords.Count(communities.Count, "community", "communities") + ", "
                + PackageWords.Count(Packages().Count, "package", "packages") + ", "
                + PackageWords.FilesWord(FileCount, Extension));

            foreach (Community community in communities)
            {
                lines.Add("community   " + community.Name + "  " + PackageWords.Count(community.Packages.Count, "package", "packages")
                    + ", " + PackageWords.FilesWord(community.FileCount, Extension));

                foreach (Package package in community.Packages)
                {
                    lines.Add("   package  " + package.RelativePath + "  " + PackageWords.FilesWord(package.Files.Count, Extension));
                }
            }

            foreach (string line in leftOut)
            {
                lines.Add("left out    : " + line);
            }

            if (leftOut.Count == 0)
            {
                lines.Add("left out    : nothing, every file of the kind is in a package");
            }

            return lines;
        }
    }
}
