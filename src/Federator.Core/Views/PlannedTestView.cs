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

        /// <summary>
        /// The models its clashing items live in, each clash's first home then its second, in the
        /// order the clashes were read, empty where one could not be read. The one place a view's
        /// homes are gathered, F114 attempt 4, read by the tree line and check 3 through ShownModels.
        /// </summary>
        public ReadOnlyCollection<string> Homes
        {
            get
            {
                List<string> homes = new List<string>();

                foreach (ViewClash clash in clashes)
                {
                    homes.Add(clash.FirstHome);
                    homes.Add(clash.SecondHome);
                }

                return new ReadOnlyCollection<string>(homes);
            }
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

        /// <summary>
        /// The one key the add-in keeps this view's read backs under in ViewsTreeFacts, F114
        /// attempt 3, ViewPlace's key of the view. Not the written place, which two views can share
        /// where a folder or a test name holds a slash.
        /// </summary>
        public string Key
        {
            get { return ViewPlace.Key(Folders, Name, false); }
        }

        /// <summary>Its written place, the way a person reads it in the log.</summary>
        public override string ToString()
        {
            return ViewPlace.Of(Folders, Name);
        }
    }
}
