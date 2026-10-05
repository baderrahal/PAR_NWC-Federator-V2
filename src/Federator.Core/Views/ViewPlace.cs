using System.Collections.Generic;

namespace Federator.Core.Views
{
    /// <summary>
    /// The place of a view or a folder in the saved viewpoints tree, its folders outermost first
    /// and its name, F114. Written here once and read by every type that names or compares a
    /// place: PlannedTestView, ViewNode, ToolViewMark, ViewsTree, ViewsInventory and the VIEWS
    /// TREE checks, the last three for the views this run wrote too. The mark compares the place it was written with against the place
    /// a view sits in now, and the read backs are keyed by the place of the view they were read
    /// off, so a second way of writing it would make every tool view read as a person's, or every
    /// read back miss.
    ///
    /// TWO FORMS. The written place joins the folders and the name by a slash, the way a person
    /// reads it in the log, the mark and the window. A folder name may hold a slash, so the
    /// written place of folder A/B holding C is the same as that of A holding B/C. The key is
    /// what a place is compared by: every part joined by a character no name is typed with, and
    /// whether it is a folder or a view, so neither of those two can stand for the other.
    /// </summary>
    public static class ViewPlace
    {
        private const string Slash = "/";
        private const string KeySeparator = "\u001f";
        private const string FolderKind = "folder";
        private const string ViewKind = "view";

        /// <summary>The folders joined by a slash, empty at the root.</summary>
        public static string FolderPath(IList<string> folders)
        {
            return string.Join(Slash, Parts(folders).ToArray());
        }

        /// <summary>The folders and the name joined by a slash, the name alone at the root.</summary>
        public static string Of(IList<string> folders, string name)
        {
            List<string> parts = Parts(folders);
            parts.Add(name ?? string.Empty);
            return string.Join(Slash, parts.ToArray());
        }

        /// <summary>The key a place is compared by, Ordinal, telling a folder from a view of the same name.</summary>
        internal static string Key(IList<string> folders, string name, bool isFolder)
        {
            List<string> parts = Parts(folders);
            parts.Add(name ?? string.Empty);
            parts.Add(isFolder ? FolderKind : ViewKind);
            return string.Join(KeySeparator, parts.ToArray());
        }

        /// <summary>The key of the folder those folders end in, or empty at the root.</summary>
        internal static string ParentKey(IList<string> folders)
        {
            List<string> parts = Parts(folders);

            if (parts.Count == 0)
            {
                return string.Empty;
            }

            return Key(parts.GetRange(0, parts.Count - 1), parts[parts.Count - 1], true);
        }

        private static List<string> Parts(IList<string> folders)
        {
            List<string> parts = new List<string>();

            if (folders != null)
            {
                foreach (string folder in folders)
                {
                    parts.Add(folder ?? string.Empty);
                }
            }

            return parts;
        }
    }
}
