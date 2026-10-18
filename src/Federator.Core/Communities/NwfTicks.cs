using System;
using System.Collections.Generic;

namespace Federator.Core.Communities
{
    /// <summary>
    /// Which NWFs of the NWF page's tree run, F232, Bader's round of 19 Oct 2026, fix 2. The tree is PackageTree.ReadNwfs,
    /// whose packages hold their NWFs as their files. NOTHING IS TICKED AT THE START, as on the NWC page. Ticking a
    /// community or a package ticks every NWF under it, and one NWF is ticked alone. A run is refused with a reason while
    /// none is ticked. Each NWF is known by its full path, compared without case as Windows names a file.
    /// </summary>
    public sealed class NwfTicks
    {
        private readonly PackageTree tree;
        private readonly HashSet<string> ticked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public NwfTicks(PackageTree tree)
        {
            if (tree == null)
            {
                throw new ArgumentNullException("tree");
            }

            this.tree = tree;
        }

        internal PackageTree Tree
        {
            get { return tree; }
        }

        public bool IsTicked(string nwf)
        {
            return nwf != null && ticked.Contains(nwf);
        }

        /// <summary>Ticks or unticks one NWF of the tree. A path the tree does not hold is left alone.</summary>
        public void Tick(string nwf, bool on)
        {
            if (nwf == null || PackageOf(nwf) == null)
            {
                return;
            }

            if (on)
            {
                ticked.Add(nwf);
            }
            else
            {
                ticked.Remove(nwf);
            }
        }

        /// <summary>Sets every NWF of the package to the one state.</summary>
        public void TickPackage(Package package, bool on)
        {
            if (package == null)
            {
                return;
            }

            foreach (string nwf in package.Files)
            {
                Tick(nwf, on);
            }
        }

        /// <summary>Sets every NWF of every package of the community to the one state.</summary>
        public void TickCommunity(Community community, bool on)
        {
            if (community == null)
            {
                return;
            }

            foreach (Package package in community.Packages)
            {
                TickPackage(package, on);
            }
        }

        /// <summary>True where every NWF of the package is ticked, false where none is, null where some are.</summary>
        public bool? StateOf(Package package)
        {
            return package == null ? false : StateOf(package.Files);
        }

        /// <summary>True where every NWF of the community is ticked, false where none is, null where some are.</summary>
        public bool? StateOf(Community community)
        {
            if (community == null)
            {
                return false;
            }

            List<string> all = new List<string>();

            foreach (Package package in community.Packages)
            {
                all.AddRange(package.Files);
            }

            return StateOf(all);
        }

        public void TickAll()
        {
            foreach (Package package in tree.Packages())
            {
                TickPackage(package, true);
            }
        }

        public void ClearAll()
        {
            ticked.Clear();
        }

        /// <summary>The ticked NWFs in the tree's order, the order they run in.</summary>
        public IList<string> Ticked()
        {
            List<string> on = new List<string>();

            foreach (Package package in tree.Packages())
            {
                foreach (string nwf in package.Files)
                {
                    if (IsTicked(nwf))
                    {
                        on.Add(nwf);
                    }
                }
            }

            return on;
        }

        /// <summary>The packages holding at least one ticked NWF, in the tree's order.</summary>
        public IList<Package> PackagesTicked()
        {
            return new List<Package>(tree.Packages()).FindAll(package => StateOf(package) != false);
        }

        /// <summary>Why a run cannot start, or null where it can: no tree read, or no NWF ticked.</summary>
        public string WhyNoRun()
        {
            if (!tree.WasRead)
            {
                return tree.WhyNoTree;
            }

            return Ticked().Count == 0 ? PackageWords.NoNwfTicked : null;
        }

        /// <summary>
        /// The NWFS TICKED block: the Report folder, every NWF ticked and every one left out, each with its package's path
        /// and its full path, then the count of each.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            int on = 0;
            int off = 0;

            lines.Add("report      : " + tree.Parent);

            foreach (Package package in tree.Packages())
            {
                foreach (string nwf in package.Files)
                {
                    bool isOn = IsTicked(nwf);
                    lines.Add((isOn ? "ticked      " : "left out    ") + package.RelativePath + "  " + nwf);

                    if (isOn)
                    {
                        on++;
                    }
                    else
                    {
                        off++;
                    }
                }
            }

            lines.Add("ticked      : " + PackageWords.FilesWord(on, tree.Extension));
            lines.Add("left out    : " + PackageWords.FilesWord(off, tree.Extension));
            return lines;
        }

        private bool? StateOf(IEnumerable<string> nwfs)
        {
            int all = 0;
            int on = 0;

            foreach (string nwf in nwfs)
            {
                all++;

                if (IsTicked(nwf))
                {
                    on++;
                }
            }

            if (on == 0)
            {
                return false;
            }

            return on == all ? true : (bool?)null;
        }

        private Package PackageOf(string nwf)
        {
            foreach (Package package in tree.Packages())
            {
                foreach (string file in package.Files)
                {
                    if (string.Equals(file, nwf, StringComparison.OrdinalIgnoreCase))
                    {
                        return package;
                    }
                }
            }

            return null;
        }
    }
}
