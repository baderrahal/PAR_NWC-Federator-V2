using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Views
{
    /// <summary>
    /// One item of the saved viewpoints tree as a fresh walk read it, F114, a view or a folder,
    /// with what the mark's judge and the legacy shape need. No Navisworks type reaches Core.
    /// </summary>
    public sealed class ViewNode
    {
        public ViewNode(
            IList<string> folders,
            string name,
            bool isFolder,
            int indexInParent,
            IList<string> comments,
            int? redlines,
            Point3 camera,
            string guid,
            bool emptyBeforeTheRun)
        {
            Folders = new ReadOnlyCollection<string>(new List<string>(folders ?? new string[0]));
            Name = name ?? string.Empty;
            IsFolder = isFolder;
            IndexInParent = indexInParent;
            Comments = new ReadOnlyCollection<string>(new List<string>(comments ?? new string[0]));
            Redlines = redlines;
            Camera = camera;
            Guid = guid;
            EmptyBeforeTheRun = emptyBeforeTheRun;
        }

        /// <summary>The folders it sits in, outermost first, empty at the root.</summary>
        public ReadOnlyCollection<string> Folders { get; private set; }

        public string Name { get; private set; }

        public bool IsFolder { get; private set; }

        /// <summary>Its index among its parent's children, which a removal is given by, 5z.</summary>
        public int IndexInParent { get; private set; }

        /// <summary>Every comment body it carries.</summary>
        public ReadOnlyCollection<string> Comments { get; private set; }

        /// <summary>How many redlines it carries, or null where they could not be read.</summary>
        public int? Redlines { get; private set; }

        /// <summary>Its camera position, or null for a folder or where it could not be read.</summary>
        public Point3 Camera { get; private set; }

        /// <summary>Its Guid, or null where it could not be read.</summary>
        public string Guid { get; private set; }

        /// <summary>For a folder, whether it held nothing before this run wrote anything.</summary>
        public bool EmptyBeforeTheRun { get; private set; }

        /// <summary>
        /// The keys of the folders that hold nothing in that walk, ViewPlace.Key, which the add-in
        /// reads off its walk before anything is written and hands to the walk it takes after, so
        /// a folder already empty before the run is known again and never removed. A folder holding
        /// a folder is not empty. Null or no walk gives none.
        /// </summary>
        public static ICollection<string> EmptyFolderKeys(IList<ViewNode> walk)
        {
            HashSet<string> empty = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> parents = new HashSet<string>(StringComparer.Ordinal);

            if (walk == null)
            {
                return empty;
            }

            foreach (ViewNode node in walk)
            {
                if (node == null)
                {
                    continue;
                }

                parents.Add(ViewPlace.ParentKey(node.Folders));

                if (node.IsFolder)
                {
                    empty.Add(ViewPlace.Key(node.Folders, node.Name, true));
                }
            }

            empty.ExceptWith(parents);
            return empty;
        }

        /// <summary>The folders joined by a slash, ViewPlace's written place.</summary>
        public string FolderPath
        {
            get { return ViewPlace.FolderPath(Folders); }
        }

        public override string ToString()
        {
            return ViewPlace.Of(Folders, Name);
        }
    }
}
