using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The pickers, each of which remembers its own last folder. One shared last folder
    /// is worse than none, because picking an NWD folder then moves the source picker.
    /// </summary>
    public enum PickerKind
    {
        Source,
        Nwf,
        Nwd,
        Excel,
        ClashXml,

        /// <summary>The picture the client report page carries, if anyone picks one.</summary>
        Logo
    }

    /// <summary>
    /// Where each picker was last pointed, kept across sessions.
    ///
    /// Written beside the logs, in the fixed folder that never depends on anything the
    /// user picked, because a settings file that lives in a picked folder disappears the
    /// moment the folder does.
    ///
    /// Nothing here ever throws. Losing the remembered folders is a small annoyance and
    /// stopping a run over it is not, so every read and write swallows its own failure the
    /// same way the log does.
    /// </summary>
    public sealed class FolderMemory
    {
        public const string FileName = "folders.txt";

        private readonly Dictionary<PickerKind, string> folders =
            new Dictionary<PickerKind, string>();

        private FolderMemory(string path)
        {
            Path = path;
        }

        /// <summary>Where this was read from and where it will be written. Null when disabled.</summary>
        public string Path { get; private set; }

        /// <summary>Why nothing is being remembered, or null when it is.</summary>
        public string DisabledReason { get; private set; }

        public bool IsRemembering
        {
            get { return Path != null; }
        }

        public static string DefaultPath()
        {
            return System.IO.Path.Combine(RunLog.DefaultLogFolder(), FileName);
        }

        /// <summary>Reads what was remembered, or an empty memory when there is nothing to read.</summary>
        public static FolderMemory Load()
        {
            try
            {
                return Load(DefaultPath());
            }
            catch (Exception error)
            {
                FolderMemory memory = new FolderMemory(null);
                memory.DisabledReason = error.Message;
                return memory;
            }
        }

        public static FolderMemory Load(string path)
        {
            FolderMemory memory = new FolderMemory(path);

            try
            {
                if (path == null || !File.Exists(path))
                {
                    return memory;
                }

                foreach (string line in File.ReadAllLines(path))
                {
                    int equals = line.IndexOf('=');

                    if (equals <= 0)
                    {
                        continue;
                    }

                    string key = line.Substring(0, equals).Trim();
                    string value = line.Substring(equals + 1).Trim();

                    foreach (PickerKind kind in AllKinds())
                    {
                        if (string.Equals(kind.ToString(), key, StringComparison.OrdinalIgnoreCase)
                            && value.Length > 0)
                        {
                            memory.folders[kind] = value;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                // A settings file that will not read is not worth a message. The pickers
                // simply open where they always did.
                memory.DisabledReason = error.Message;
            }

            return memory;
        }

        public static PickerKind[] AllKinds()
        {
            return new[]
            {
                PickerKind.Source,
                PickerKind.Nwf,
                PickerKind.Nwd,
                PickerKind.Excel,
                PickerKind.ClashXml,
                PickerKind.Logo
            };
        }

        /// <summary>What was remembered for this picker, exactly as it was remembered.</summary>
        public string LastFor(PickerKind kind)
        {
            string folder;
            return folders.TryGetValue(kind, out folder) ? folder : string.Empty;
        }

        /// <summary>
        /// Where this picker should open. The remembered folder when it is still there,
        /// and its nearest existing parent when it is not, so a folder that has been moved
        /// or unmounted opens somewhere near rather than failing.
        /// </summary>
        public string OpenAt(PickerKind kind)
        {
            return NearestExisting(LastFor(kind));
        }

        /// <summary>
        /// This folder if it exists, otherwise the closest parent that does, otherwise
        /// empty. Never throws, whatever the string holds.
        /// </summary>
        public static string NearestExisting(string folder)
        {
            if (string.IsNullOrEmpty(folder) || folder.Trim().Length == 0)
            {
                return string.Empty;
            }

            string current = folder.Trim();

            try
            {
                current = System.IO.Path.GetFullPath(current);
            }
            catch (Exception)
            {
                // A path Windows will not expand is walked as it came, which still finds a
                // parent when one of its ancestors is real.
                current = folder.Trim();
            }

            // Bounded rather than while(true), because a path that will not shorten would
            // otherwise spin. No real path is anywhere near this deep.
            for (int step = 0; step < 64 && current.Length > 0; step++)
            {
                try
                {
                    if (Directory.Exists(current))
                    {
                        return current;
                    }

                    string parent = System.IO.Path.GetDirectoryName(current);

                    if (string.IsNullOrEmpty(parent) || string.Equals(parent, current, StringComparison.Ordinal))
                    {
                        return string.Empty;
                    }

                    current = parent;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Records where a picker was last pointed and writes it out. A file path is
        /// reduced to its folder, so remembering the clash XML remembers where it lives.
        /// </summary>
        public void Remember(PickerKind kind, string folderOrFile)
        {
            if (string.IsNullOrEmpty(folderOrFile) || folderOrFile.Trim().Length == 0)
            {
                return;
            }

            string value = folderOrFile.Trim();

            try
            {
                if (File.Exists(value))
                {
                    value = System.IO.Path.GetDirectoryName(value);
                }
            }
            catch (Exception)
            {
                // Kept as it came. Worst case the picker opens at its parent next time.
            }

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            folders[kind] = value;
            Save();
        }

        /// <summary>
        /// Writes what is remembered. Returns false and records why rather than throwing,
        /// because remembering a folder is never worth stopping anything for.
        /// </summary>
        public bool Save()
        {
            if (Path == null)
            {
                return false;
            }

            try
            {
                string folder = System.IO.Path.GetDirectoryName(Path);

                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                StringBuilder text = new StringBuilder();
                text.Append("# Where each picker was last pointed. Safe to delete.")
                    .Append(Environment.NewLine);

                foreach (PickerKind kind in AllKinds())
                {
                    string value = LastFor(kind);

                    if (value.Length > 0)
                    {
                        text.Append(kind).Append('=').Append(value).Append(Environment.NewLine);
                    }
                }

                File.WriteAllText(Path, text.ToString());
                DisabledReason = null;
                return true;
            }
            catch (Exception error)
            {
                DisabledReason = error.Message;
                return false;
            }
        }

        /// <summary>One line per picker, for the log, so what it opened with is on the record.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            foreach (PickerKind kind in AllKinds())
            {
                string remembered = LastFor(kind);

                if (remembered.Length == 0)
                {
                    continue;
                }

                string opening = OpenAt(kind);

                lines.Add(kind.ToString().PadRight(10) + remembered
                    + (string.Equals(opening, remembered, StringComparison.OrdinalIgnoreCase)
                        ? string.Empty
                        : "   [gone, opening at " + Words.Or(opening, "nowhere") + "]"));
            }

            if (lines.Count == 0)
            {
                lines.Add("Nothing remembered yet. Every picker opens where it always did.");
            }

            return lines;
        }

    }
}
