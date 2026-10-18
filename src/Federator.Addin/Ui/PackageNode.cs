using System;
using System.Collections.ObjectModel;
using Federator.Core.Communities;

namespace Federator.Addin.Ui
{
    /// <summary>
    /// One line of the tree of packages, F208: a community with its packages under it, or one package, each with a tick
    /// box, its files and its groups. The tick is not held here. It is Core's PackageTicks, read and set through the window,
    /// so the tree on the NWC page and the one on the NWF page show one set of ticks.
    /// </summary>
    public sealed class PackageNode : ObservableObject
    {
        private readonly Func<PackageNode, bool?> read;
        private readonly Action<PackageNode, bool> write;
        private string detail;

        private PackageNode(
            string title, string detail, Community community, Package package, string nwf, Func<PackageNode, bool?> read, Action<PackageNode, bool> write)
        {
            Title = title ?? string.Empty;
            this.detail = detail ?? string.Empty;
            Community = community;
            Package = package;
            Nwf = nwf;
            this.read = read;
            this.write = write;
            Children = new ObservableCollection<PackageNode>();
        }

        public static PackageNode ForCommunity(
            Community community, string detail, Func<PackageNode, bool?> read, Action<PackageNode, bool> write)
        {
            return new PackageNode(community.Name, detail, community, null, null, read, write);
        }

        public static PackageNode ForPackage(
            Package package, string detail, Func<PackageNode, bool?> read, Action<PackageNode, bool> write)
        {
            return new PackageNode(package.Name, detail, null, package, null, read, write);
        }

        /// <summary>One NWF of the NWF page's tree, F232, a line of its own under its package with its own tick.</summary>
        public static PackageNode ForNwf(
            string nwf, string detail, Func<PackageNode, bool?> read, Action<PackageNode, bool> write)
        {
            return new PackageNode(System.IO.Path.GetFileName(nwf), detail, null, null, nwf, read, write);
        }

        public string Title { get; private set; }

        /// <summary>The files and groups beside the title, grey.</summary>
        public string Detail
        {
            get { return detail; }
            set
            {
                detail = value ?? string.Empty;
                Raise("Detail");
            }
        }

        /// <summary>The community this line is, or null for a package.</summary>
        public Community Community { get; private set; }

        /// <summary>The package this line is, or null for a community or an NWF.</summary>
        public Package Package { get; private set; }

        /// <summary>The full path of the NWF this line is, or null for a community or a package.</summary>
        public string Nwf { get; private set; }

        public ObservableCollection<PackageNode> Children { get; private set; }

        /// <summary>
        /// Ticked, unticked, or for a community whose packages are partly ticked neither. A click on a community that
        /// reads neither ticks all its packages, the tick box's own next state.
        /// </summary>
        public bool? IsTicked
        {
            get { return read(this); }
            set { write(this, value == true); }
        }

        /// <summary>Says the tick may have moved, after any tick of the tree.</summary>
        public void TickMoved()
        {
            Raise("IsTicked");

            foreach (PackageNode child in Children)
            {
                child.TickMoved();
            }
        }
    }

    /// <summary>
    /// What both trees show, F208 and F232: the parent folder and the tree of packages of the NWC page, the tree of NWFs of the
    /// NWF page, each with the one line under it, and the Report folder both pages read. One instance is the data of both
    /// pages, so the Report folder typed on one reads the same on the other.
    /// </summary>
    public sealed class PackagePanel : ObservableObject
    {
        private string parent = string.Empty;
        private string report = string.Empty;
        private string summary = PackageWords.NotReadYet;
        private string nwfSummary = PackageWords.NwfNotReadYet;

        public PackagePanel()
        {
            Nodes = new ObservableCollection<PackageNode>();
            NwfNodes = new ObservableCollection<PackageNode>();
        }

        public string Parent
        {
            get { return parent; }
            set
            {
                parent = value ?? string.Empty;
                Raise("Parent");
            }
        }

        public string Report
        {
            get { return report; }
            set
            {
                report = value ?? string.Empty;
                Raise("Report");
            }
        }

        public string Summary
        {
            get { return summary; }
            set
            {
                summary = value ?? string.Empty;
                Raise("Summary");
            }
        }

        public ObservableCollection<PackageNode> Nodes { get; private set; }

        /// <summary>The lines of the NWF page's tree, community, package and NWF, F232.</summary>
        public ObservableCollection<PackageNode> NwfNodes { get; private set; }

        public string NwfSummary
        {
            get { return nwfSummary; }
            set
            {
                nwfSummary = value ?? string.Empty;
                Raise("NwfSummary");
            }
        }

        public string Heading
        {
            get { return PackageWords.StepHeading; }
        }

        public string NwfHeading
        {
            get { return PackageWords.NwfPageHeading; }
        }

        public string ParentLabel
        {
            get { return PackageWords.ParentLabel; }
        }

        public string ReportLabel
        {
            get { return PackageWords.ReportLabel; }
        }

        public string ReportHelp
        {
            get { return PackageWords.ReportHelp; }
        }

        public string ReadWord
        {
            get { return PackageWords.ReadButton; }
        }

        public string TickAllWord
        {
            get { return PackageWords.TickAllButton; }
        }

        public string ClearAllWord
        {
            get { return PackageWords.ClearAllButton; }
        }
    }
}
