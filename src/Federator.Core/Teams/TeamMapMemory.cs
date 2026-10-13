using System;
using System.Globalization;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;

namespace Federator.Core.Teams
{
    /// <summary>
    /// The team map the window keeps for a run with no clash XML, Q123 answered B by Bader on
    /// 2026-10-05: the window keeps the last team map used, for runs with no XML, and the log
    /// names it. The plain weekly run picks no XML, so this is how its views come in team
    /// folders.
    ///
    /// KEPT THE WAY FolderMemory KEEPS THE PICKER FOLDERS: one small file beside the logs, in
    /// the fixed folder that never depends on anything picked, read when the window opens and
    /// written when a run uses a map. It holds ONE FULL PATH, the map that run read, never a copy
    /// of the map, so the map a run with no XML reads is the file a person keeps beside the XML
    /// and edits there. That path is tested with File.Exists, never searched for.
    ///
    /// ONLY A MAP READ WHOLE WITH A TEAM IS KEPT, so a pick with no map beside it, one that
    /// cannot be read or one holding no team never takes the teams away from the next run with
    /// no XML. The line says which map stays.
    ///
    /// NEVER A THROW AND NEVER SILENT. A memory that cannot be read, a line it does not know or a
    /// path that is not a full one, is Unread with why, and a run with no XML then maps nothing
    /// and its TEAMS line says so. A memory that cannot be written is said on the line of the
    /// run that used the map.
    /// </summary>
    public sealed class TeamMapMemory
    {
        /// <summary>The memory's file name, beside the logs and beside FolderMemory's.</summary>
        public const string FileName = "team-map.txt";

        /// <summary>The line naming the kept map.</summary>
        internal const string KeptMarker = "kept:";

        private TeamMapMemory(string path)
        {
            Path = path;
            LastMap = string.Empty;
        }

        /// <summary>Where this was read from and where it is written.</summary>
        public string Path { get; private set; }

        /// <summary>The full path of the map the last run with a clash XML used, or empty where none is kept.</summary>
        public string LastMap { get; private set; }

        /// <summary>Why the memory could not be read, or null where it was read or is not there.</summary>
        public string Unread { get; private set; }

        /// <summary>The memory in the fixed folder beside the logs, read when the window opens.</summary>
        public static TeamMapMemory Load()
        {
            return Load(System.IO.Path.Combine(RunLog.DefaultLogFolder(), FileName));
        }

        /// <summary>The memory at that full path. Not there, nothing is kept. Never a throw.</summary>
        public static TeamMapMemory Load(string path)
        {
            TeamMapMemory memory = new TeamMapMemory(path);

            if (!File.Exists(path))
            {
                return memory;
            }

            string unread = ListFile.Read(path, memory.ReadKept, why => why);

            if (unread != null)
            {
                memory.Unread = unread;
                memory.LastMap = string.Empty;
            }

            return memory;
        }

        /// <summary>Reads the one kept line, and gives why where a line is not one this knows, or null.</summary>
        private string ReadKept(TextReader text)
        {
            string line;
            int number = 0;

            while ((line = text.ReadLine()) != null)
            {
                number++;

                if (ListFile.Skipped(line))
                {
                    continue;
                }

                string at = "line " + number.ToString(CultureInfo.InvariantCulture) + ", \"" + line + "\", ";
                string kept = ListFile.After(line, KeptMarker);

                if (kept == null)
                {
                    return at + "is not a line of this memory";
                }

                if (LastMap.Length > 0)
                {
                    return at + "names a second map";
                }

                if (!IsFull(kept))
                {
                    return at + "names a map by a path that is not a full one";
                }

                LastMap = kept;
            }

            return null;
        }

        /// <summary>Whether that path is one full path, the same once Windows expands it, never a relative one.</summary>
        private static bool IsFull(string path)
        {
            try
            {
                return System.IO.Path.IsPathRooted(path)
                    && string.Equals(System.IO.Path.GetFullPath(path), path, StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        /// <summary>
        /// The map of a run with no clash matrix read: the kept one, read at its path, or none kept,
        /// or why the memory could not be read. Its TEAMS lines say which.
        /// </summary>
        public TeamMap ForNoXml(TeamMapSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (Unread != null)
            {
                return TeamMap.MemoryUnread(Path, Unread, settings);
            }

            return LastMap.Length == 0 ? TeamMap.NoXml(settings) : TeamMap.Kept(LastMap, settings);
        }

        /// <summary>
        /// The map a run uses, the one rule for every run of the window: the map read beside the
        /// clash matrix, which ReadPack or ReadPicked carries in Teams, or the kept one where no matrix is read.
        /// A document read by neither carries no map and is refused, never given one it was
        /// not read with, the breaker's finding on attempt 1.
        /// </summary>
        public TeamMap ForRun(ExchangeDocument picked, TeamMapSettings settings)
        {
            if (picked == null)
            {
                return ForNoXml(settings);
            }

            if (picked.Teams == null)
            {
                throw new ArgumentException("A run reads the clash matrix through ReadPack or ReadPicked, which read its team map too.", "picked");
            }

            return picked.Teams;
        }
    }
}
