using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Clash;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>
    /// One view this run means to write, F114: one clash test's open clashes, or the half of
    /// them over the size threshold in a pair that carries the size folder, Q114 points 9, 13
    /// and 14.
    /// </summary>
    public sealed class PlannedTestView
    {
        private readonly List<ViewClash> clashes;

        internal PlannedTestView(
            ClashPriority priority,
            string priorityFolder,
            TeamPair pair,
            string sizeFolder,
            string name,
            IList<ViewClash> clashes)
        {
            Priority = priority;
            PriorityFolder = priorityFolder;
            Pair = pair;
            SizeFolder = sizeFolder;
            Name = name;
            this.clashes = new List<ViewClash>(clashes);
        }

        public ClashPriority Priority { get; private set; }

        /// <summary>A, B, C or No priority. Always there, point 9.</summary>
        public string PriorityFolder { get; private set; }

        /// <summary>The team pair whose folder the view sits in.</summary>
        public TeamPair Pair { get; private set; }

        /// <summary>Over 150mm for the view of the large services, or null.</summary>
        public string SizeFolder { get; private set; }

        /// <summary>The view's name, exactly the test's name and never trimmed.</summary>
        public string Name { get; private set; }

        /// <summary>The open clashes the view shows, in the order they were read.</summary>
        public ReadOnlyCollection<ViewClash> Clashes
        {
            get { return new ReadOnlyCollection<ViewClash>(clashes); }
        }

        /// <summary>The clash whose Clash Detective camera the view starts from, its first open clash.</summary>
        public ViewClash CameraClash
        {
            get { return clashes[0]; }
        }

        /// <summary>The folders it sits in, outermost first.</summary>
        public IList<string> Folders
        {
            get
            {
                List<string> folders = new List<string> { PriorityFolder, Pair.Folder };

                if (SizeFolder != null)
                {
                    folders.Add(SizeFolder);
                }

                return folders;
            }
        }

        /// <summary>The folders joined by a slash, the way the mark and the log name a place.</summary>
        public string FolderPath
        {
            get { return string.Join("/", new List<string>(Folders).ToArray()); }
        }

        public override string ToString()
        {
            return FolderPath + "/" + Name;
        }
    }
}
