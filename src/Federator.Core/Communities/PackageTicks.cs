using System;
using System.Collections.Generic;

namespace Federator.Core.Communities
{
    /// <summary>
    /// Which packages of one tree run, F208, Bader's message of 17 Oct 2026. NOTHING IS TICKED AT THE START, so a parent
    /// holding ten communities never runs all ten by mistake. A community's tick sets every one of its packages, and a
    /// package is ticked on its own. A run is refused with a reason while none is ticked. Each package is known by its path
    /// from the parent, compared without case as Windows names a folder.
    /// </summary>
    public sealed class PackageTicks
    {
        private readonly PackageTree tree;
        private readonly HashSet<string> ticked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public PackageTicks(PackageTree tree)
        {
            if (tree == null)
            {
                throw new ArgumentNullException("tree");
            }

            this.tree = tree;
        }

        public PackageTree Tree
        {
            get { return tree; }
        }

        public bool IsTicked(Package package)
        {
            return package != null && ticked.Contains(package.RelativePath);
        }

        public void Tick(Package package, bool on)
        {
            if (package == null || tree.Find(package.RelativePath) == null)
            {
                return;
            }

            if (on)
            {
                ticked.Add(package.RelativePath);
            }
            else
            {
                ticked.Remove(package.RelativePath);
            }
        }

        /// <summary>Sets every package of the community to the one state.</summary>
        public void TickCommunity(Community community, bool on)
        {
            if (community == null)
            {
                return;
            }

            foreach (Package package in community.Packages)
            {
                Tick(package, on);
            }
        }

        /// <summary>True where every package of the community is ticked, false where none is, null where some are.</summary>
        public bool? StateOf(Community community)
        {
            if (community == null || community.Packages.Count == 0)
            {
                return false;
            }

            int on = 0;

            foreach (Package package in community.Packages)
            {
                if (IsTicked(package))
                {
                    on++;
                }
            }

            if (on == 0)
            {
                return false;
            }

            if (on == community.Packages.Count)
            {
                return true;
            }

            return null;
        }

        public void TickAll()
        {
            foreach (Package package in tree.Packages())
            {
                ticked.Add(package.RelativePath);
            }
        }

        public void ClearAll()
        {
            ticked.Clear();
        }

        /// <summary>The ticked packages in the tree's order, the order they run in.</summary>
        public IList<Package> Ticked()
        {
            return new List<Package>(tree.Packages()).FindAll(IsTicked);
        }

        /// <summary>The packages left out, in the tree's order.</summary>
        public IList<Package> LeftOut()
        {
            return new List<Package>(tree.Packages()).FindAll(p => !IsTicked(p));
        }

        /// <summary>Why a run cannot start, or null where it can: no tree read, or nothing ticked.</summary>
        public string WhyNoRun()
        {
            if (!tree.WasRead)
            {
                return tree.WhyNoTree;
            }

            return Ticked().Count == 0 ? PackageWords.NothingTicked : null;
        }

        /// <summary>
        /// The PACKAGES TICKED block: every package ticked and every one left out, each with its full path from the parent
        /// and its files, then the count of each.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            IList<Package> on = Ticked();
            IList<Package> off = LeftOut();

            lines.Add("parent      : " + tree.Parent);

            foreach (Package package in on)
            {
                lines.Add("ticked      " + package.RelativePath + "  " + PackageWords.FilesWord(package.Files.Count, tree.Extension));
            }

            foreach (Package package in off)
            {
                lines.Add("left out    " + package.RelativePath + "  " + PackageWords.FilesWord(package.Files.Count, tree.Extension));
            }

            lines.Add("ticked      : " + PackageWords.Count(on.Count, "package", "packages") + " holding "
                + PackageWords.FilesWord(FilesIn(on), tree.Extension));
            lines.Add("left out    : " + PackageWords.Count(off.Count, "package", "packages") + " holding "
                + PackageWords.FilesWord(FilesIn(off), tree.Extension));

            return lines;
        }

        private static int FilesIn(IList<Package> packages)
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
