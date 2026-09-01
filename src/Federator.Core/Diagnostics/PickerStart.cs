using System;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// Where one Browse button opens.
    ///
    /// This lives in Core so every picker can be proved rather than clicked. The window
    /// has five of them and Bader reported that not all of them remembered, which nothing
    /// could confirm either way because the run that raised it said "Nothing remembered
    /// yet" on its first line.
    ///
    /// The rule is the same for all five and there is one of it:
    ///   what is already in the box wins, because it is what the person is looking at
    ///   otherwise the folder this picker was last pointed at
    ///   otherwise nothing, and the dialog opens wherever Windows would
    ///
    /// Every answer is a FOLDER, including for the picker that chooses a file. A dialog
    /// that picks a file still opens at a folder, and the caller must not take the parent
    /// of what comes back. Doing that opened the clash XML picker one level above the
    /// folder it had remembered.
    /// </summary>
    public static class PickerStart
    {
        /// <summary>
        /// The folder this picker should open at. Never throws, because it runs on a
        /// button press and a bad remembered path is not a reason to refuse to browse.
        /// </summary>
        public static string For(FolderMemory memory, PickerKind kind, string inTheBox)
        {
            string typed = FolderMemory.NearestExisting(Tidy(inTheBox));

            if (typed.Length > 0)
            {
                return typed;
            }

            return memory == null ? string.Empty : memory.OpenAt(kind);
        }

        /// <summary>
        /// True when this picker has a folder of its own to go back to, whatever is in the
        /// box. What the per picker tests assert.
        /// </summary>
        public static bool Remembers(FolderMemory memory, PickerKind kind)
        {
            return memory != null && memory.LastFor(kind).Length > 0;
        }

        private static string Tidy(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }
}
