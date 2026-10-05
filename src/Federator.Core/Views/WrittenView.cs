using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Views
{
    /// <summary>
    /// One view this run wrote, where it went and whether it was marked and read back, F114,
    /// design S2. The run knows what it wrote, so a view it could not mark is still known as its
    /// own and removed at once.
    /// </summary>
    public sealed class WrittenView
    {
        public WrittenView(IList<string> folders, string name, int indexInParent, bool marked, bool readBack)
        {
            Folders = new ReadOnlyCollection<string>(new List<string>(folders ?? new string[0]));
            Name = name ?? string.Empty;
            IndexInParent = indexInParent;
            Marked = marked;
            ReadBack = readBack;
        }

        public ReadOnlyCollection<string> Folders { get; private set; }

        public string Name { get; private set; }

        /// <summary>Its index in its folder when it was added.</summary>
        public int IndexInParent { get; private set; }

        /// <summary>Whether the mark was written on it.</summary>
        public bool Marked { get; private set; }

        /// <summary>Whether it read back as written, camera, hiding, dimming and paint.</summary>
        public bool ReadBack { get; private set; }

        /// <summary>Whether it was marked and read back, so it may replace what it stands for.</summary>
        public bool Sound
        {
            get { return Marked && ReadBack; }
        }
    }
}
