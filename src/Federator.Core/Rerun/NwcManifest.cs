using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// The manifest of the NWC files an NWF links, F187, Bader's order of 14 Oct 2026, task 7.3. Navisworks does not
    /// say which NWC changed, so before a refresh the tool writes down each linked file's path, size and last write
    /// time in UTC, and a SHA-256 only where the size or the time differs from the last manifest, and compares it
    /// with that one. The list of changed files is shown before the refresh. The manifest is the tool's own state
    /// file, one fixed name in the Log folder, overwritten by the tool on each run.
    ///
    /// A file is CHANGED where its size or time differs and its hash differs or there was none to compare, TOUCHED
    /// where only the time or size differs and the hash is the same, ADDED where the last manifest has no such path
    /// and REMOVED where the last manifest had one the NWF no longer links. With no earlier manifest every file is
    /// FIRST SEEN and nothing is called changed, since there is nothing to compare it with.
    /// </summary>
    public sealed class NwcManifest
    {
        /// <summary>The fixed name of the file in the Log folder.</summary>
        public const string FileName = "nwc-manifest.tsv";

        private const string Header = "NWC-MANIFEST v1";
        private const char Tab = '\t';

        private readonly List<ManifestEntry> entries = new List<ManifestEntry>();

        public NwcManifest()
        {
        }

        /// <summary>The files, in the order they were added.</summary>
        public ReadOnlyCollection<ManifestEntry> Entries
        {
            get { return new ReadOnlyCollection<ManifestEntry>(entries); }
        }

        public void Add(ManifestEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            entries.Add(entry);
        }

        /// <summary>The entry for that path, compared without case as Windows does, or null.</summary>
        public ManifestEntry Find(string path)
        {
            foreach (ManifestEntry entry in entries)
            {
                if (string.Equals(entry.Path, path, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>The lines of the file: the header, then one tab separated line a file.</summary>
        public IList<string> ToLines()
        {
            List<string> lines = new List<string> { Header };

            foreach (ManifestEntry entry in entries)
            {
                lines.Add(entry.Path + Tab
                    + entry.Size.ToString(CultureInfo.InvariantCulture) + Tab
                    + entry.LastWriteUtc.ToString("o", CultureInfo.InvariantCulture) + Tab
                    + (entry.Sha256 ?? string.Empty));
            }

            return lines;
        }

        /// <summary>
        /// The manifest the lines give, or null where they are not one: no header, or a line that is not four
        /// fields with a size and a time. A manifest that does not read is no manifest, and the next run treats
        /// every file as first seen rather than guessing.
        /// </summary>
        public static NwcManifest Parse(IList<string> lines)
        {
            if (lines == null || lines.Count == 0 || !string.Equals(lines[0], Header, StringComparison.Ordinal))
            {
                return null;
            }

            NwcManifest manifest = new NwcManifest();

            for (int i = 1; i < lines.Count; i++)
            {
                if (string.IsNullOrEmpty(lines[i]))
                {
                    continue;
                }

                string[] parts = lines[i].Split(Tab);
                long size;
                DateTime when;

                if (parts.Length != 4
                    || !long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out size)
                    || !DateTime.TryParse(parts[2], CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out when))
                {
                    return null;
                }

                manifest.Add(new ManifestEntry(parts[0], size, when, parts[3].Length == 0 ? null : parts[3]));
            }

            return manifest;
        }

        /// <summary>
        /// What changed between the last manifest, or null, and the files now. The hash is asked for only
        /// where the size or the time differs, by the function handed in, which returns null where it cannot read
        /// the file. The manifest of the files now is returned with the hash kept where one was taken, and the hash of
        /// the last manifest carried over for a file that did not change, so it is there for the run after.
        /// </summary>
        public static ManifestComparison Compare(NwcManifest last, IList<ManifestEntry> now, Func<string, string> hashOf)
        {
            if (now == null)
            {
                throw new ArgumentNullException("now");
            }

            ManifestComparison result = new ManifestComparison(last != null);
            NwcManifest next = new NwcManifest();

            foreach (ManifestEntry file in now)
            {
                ManifestEntry before = last == null ? null : last.Find(file.Path);

                if (last == null)
                {
                    result.FirstSeen.Add(file.Path);
                    next.Add(file);
                    continue;
                }

                if (before == null)
                {
                    result.Added.Add(file.Path);
                    next.Add(file);
                    continue;
                }

                bool same = before.Size == file.Size && before.LastWriteUtc == file.LastWriteUtc;

                if (same)
                {
                    result.Unchanged.Add(file.Path);
                    next.Add(new ManifestEntry(file.Path, file.Size, file.LastWriteUtc, before.Sha256));
                    continue;
                }

                string hash = hashOf == null ? null : hashOf(file.Path);

                if (hash != null && before.Sha256 != null && string.Equals(hash, before.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    result.Touched.Add(file.Path);
                }
                else
                {
                    result.Changed.Add(file.Path);
                }

                next.Add(new ManifestEntry(file.Path, file.Size, file.LastWriteUtc, hash));
            }

            if (last != null)
            {
                foreach (ManifestEntry old in last.Entries)
                {
                    bool still = false;

                    foreach (ManifestEntry file in now)
                    {
                        if (string.Equals(file.Path, old.Path, StringComparison.OrdinalIgnoreCase))
                        {
                            still = true;
                            break;
                        }
                    }

                    if (!still)
                    {
                        result.Removed.Add(old.Path);
                    }
                }
            }

            result.Next = next;
            return result;
        }
    }

    /// <summary>One linked NWC file as the manifest records it.</summary>
    public sealed class ManifestEntry
    {
        public ManifestEntry(string path, long size, DateTime lastWriteUtc, string sha256)
        {
            Path = path ?? string.Empty;
            Size = size;
            LastWriteUtc = lastWriteUtc.ToUniversalTime();
            Sha256 = sha256;
        }

        public string Path { get; private set; }

        public long Size { get; private set; }

        public DateTime LastWriteUtc { get; private set; }

        /// <summary>Null where none was taken, which is where the file was not different from the last manifest's.</summary>
        public string Sha256 { get; private set; }
    }

    /// <summary>What the files linked now are against the last manifest.</summary>
    public sealed class ManifestComparison
    {
        internal ManifestComparison(bool hadAnEarlierManifest)
        {
            HadAnEarlierManifest = hadAnEarlierManifest;
            FirstSeen = new List<string>();
            Added = new List<string>();
            Changed = new List<string>();
            Touched = new List<string>();
            Unchanged = new List<string>();
            Removed = new List<string>();
        }

        public bool HadAnEarlierManifest { get; private set; }

        /// <summary>Every file, where there was no earlier manifest to compare it with.</summary>
        public List<string> FirstSeen { get; private set; }

        public List<string> Added { get; private set; }

        public List<string> Changed { get; private set; }

        /// <summary>Written again with the same content, size or time moved and the hash the same.</summary>
        public List<string> Touched { get; private set; }

        public List<string> Unchanged { get; private set; }

        public List<string> Removed { get; private set; }

        /// <summary>The manifest of the files now, to be written once the refresh is done.</summary>
        public NwcManifest Next { get; internal set; }

        /// <summary>The list shown before the refresh: each changed, added or removed file, or what stands in its place.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (!HadAnEarlierManifest)
            {
                lines.Add("no earlier manifest, so " + FirstSeen.Count + " linked files are written down now and none is called changed");
                return lines;
            }

            foreach (string path in Changed)
            {
                lines.Add("changed  " + path);
            }

            foreach (string path in Added)
            {
                lines.Add("added    " + path);
            }

            foreach (string path in Removed)
            {
                lines.Add("removed  " + path + "  (the NWF no longer links it)");
            }

            if (lines.Count == 0)
            {
                lines.Add("no linked file changed since the last manifest, " + Unchanged.Count + " unchanged"
                    + (Touched.Count > 0 ? ", " + Touched.Count + " written again with the same content" : string.Empty));
            }

            return lines;
        }

        /// <summary>The count of files the refresh will bring in, changed and added.</summary>
        public int ChangedCount
        {
            get { return Changed.Count + Added.Count; }
        }
    }
}
