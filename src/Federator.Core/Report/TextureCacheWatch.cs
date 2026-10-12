using System;
using System.Collections.Generic;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// Watches Navisworks' texture cache across one picture pass, F147, by TextureCache's rule:
    /// checked as the pass begins and between two tests' pictures at most once a check interval,
    /// cleared there once a size read whole is past the limit with its size said before and after,
    /// and read once more as the pass ends, never clearing there, for one line. Each line is handed
    /// to the caller to write.
    ///
    /// THE WALK NEVER LEAVES THE FOLDER. It is made by hand, a folder at a time, and a folder that is
    /// a link, a junction or a symbolic link, is counted and never stepped into, since the framework's
    /// own walk with AllDirectories follows links and a clearing would then remove another folder's
    /// files. A cache folder that is itself a link is not walked at all, and a folder that is not a
    /// fully qualified folder named ogs is refused before anything is read. A clearing removes only
    /// the files its own walk read, never a folder, and counts a file removed only where it was there
    /// to remove. A folder inside that goes while it is walked is passed over, since its files are gone.
    ///
    /// It never throws. A walk that fails is said once with why, counted as not whole, and clears
    /// nothing. Once a clearing leaves the cache not under the limit, files held open among them, the
    /// cache is not walked again until the pass ends, so a cache that cannot be cleared costs one
    /// clearing and not one a minute for hours. File paths are kept only by the walk of a clearing.
    /// </summary>
    public sealed class TextureCacheWatch
    {
        private readonly string folder;
        private readonly long limitBytes;
        private readonly int everySeconds;
        private readonly Func<double> clock;
        private readonly Action<string> say;
        private readonly bool refused;

        private bool measuredBefore;
        private double lastAt;
        private bool stopChecking;
        private bool everThere;
        private bool linked;
        private bool notWholeSaid;
        private long atStart = -1;
        private int filesAtStart = -1;
        private int checks;
        private int notWhole;
        private int clearings;
        private int removedInAll;
        private int linksLeft;
        private int foldersGone;
        private double seconds;

        public TextureCacheWatch(string folder, long limitBytes, int everySeconds, Func<double> clock, Action<string> say)
        {
            if (clock == null)
            {
                throw new ArgumentNullException("clock");
            }

            if (say == null)
            {
                throw new ArgumentNullException("say");
            }

            this.folder = folder ?? string.Empty;
            this.limitBytes = limitBytes;
            this.everySeconds = everySeconds;
            this.clock = clock;
            this.say = say;
            refused = !TextureCache.IsACacheFolder(this.folder);
        }

        /// <summary>The cache folder under a temp folder, where Navisworks keeps it. Null where there is no temp folder.</summary>
        public static string FolderIn(string tempFolder)
        {
            return string.IsNullOrEmpty(tempFolder) ? null : Path.Combine(tempFolder, TextureCache.FolderUnderTemp);
        }

        /// <summary>As the pass begins: checked once, and cleared if it is already past the limit.</summary>
        public void AtTheStart()
        {
            Check(true);
        }

        /// <summary>Between two tests' pictures: checked once the interval has passed, and cleared past the limit.</summary>
        public void BetweenTests()
        {
            Check(false);
        }

        /// <summary>As the pass ends: read once more and never cleared, and the pass's one line handed back, a refused folder's line among them.</summary>
        public string AtTheEnd()
        {
            if (limitBytes <= 0)
            {
                return TextureCache.PassLine(folder, limitBytes, false, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0);
            }

            if (refused)
            {
                return TextureCache.RefusedLine(folder);
            }

            Reading end = Reading.Unknown;

            try
            {
                if (!linked)
                {
                    end = Read(false);
                }
            }
            catch (Exception error)
            {
                notWhole++;
                SayNotWholeOnce(error);
            }

            return TextureCache.PassLine(
                folder, limitBytes, everThere, atStart, filesAtStart, end.Bytes, end.FileCount,
                checks, notWhole, clearings, removedInAll, linksLeft, foldersGone, seconds);
        }

        private void Check(bool first)
        {
            if (limitBytes <= 0 || refused || stopChecking)
            {
                return;
            }

            if (!first && !TextureCache.DueToMeasure(measuredBefore, clock() - lastAt, everySeconds))
            {
                return;
            }

            measuredBefore = true;

            try
            {
                Reading now = Read(false);
                checks++;

                if (first)
                {
                    atStart = now.Bytes;
                    filesAtStart = now.FileCount;
                }

                if (TextureCache.ShouldClear(now.Bytes, limitBytes))
                {
                    Clear();
                }
            }
            catch (Exception error)
            {
                checks++;
                notWhole++;
                SayNotWholeOnce(error);
            }
            finally
            {
                lastAt = clock();
            }
        }

        /// <summary>
        /// The bytes and the files under the folder, read a folder at a time and never into a link,
        /// the paths kept only where asked. Zero where there is no folder, UNKNOWN where the folder is
        /// a link. A walk that fails throws, for the caller to count and say.
        /// </summary>
        private Reading Read(bool keepPaths)
        {
            double from = clock();

            try
            {
                DirectoryInfo root = new DirectoryInfo(folder);

                if (!root.Exists)
                {
                    return new Reading(0, 0, keepPaths);
                }

                everThere = true;

                if ((root.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    say(TextureCache.LinkedLine(folder));
                    linked = true;
                    stopChecking = true;
                    notWhole++;
                    return Reading.Unknown;
                }

                Reading reading = new Reading(0, 0, keepPaths);
                int links = 0;
                Stack<DirectoryInfo> waiting = new Stack<DirectoryInfo>();
                waiting.Push(root);

                while (waiting.Count > 0)
                {
                    DirectoryInfo current = waiting.Pop();

                    try
                    {
                        foreach (FileSystemInfo entry in current.EnumerateFileSystemInfos())
                        {
                            DirectoryInfo inner = entry as DirectoryInfo;

                            if (inner != null)
                            {
                                if ((inner.Attributes & FileAttributes.ReparsePoint) != 0)
                                {
                                    links++;
                                }
                                else
                                {
                                    waiting.Push(inner);
                                }

                                continue;
                            }

                            reading.Add((FileInfo)entry);
                        }
                    }
                    catch (DirectoryNotFoundException) when (!ReferenceEquals(current, root))
                    {
                        foldersGone++;
                    }
                }

                linksLeft = Math.Max(linksLeft, links);
                return reading;
            }
            finally
            {
                seconds += clock() - from;
            }
        }

        private void Clear()
        {
            double from = clock();
            Reading before;

            try
            {
                before = Read(true);
            }
            catch (Exception error)
            {
                notWhole++;
                SayNotWholeOnce(error);
                return;
            }

            if (!TextureCache.ShouldClear(before.Bytes, limitBytes))
            {
                return;
            }

            int removed = 0;
            int notRemoved = 0;
            int goneBefore = 0;

            foreach (string path in before.Paths)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        goneBefore++;
                        continue;
                    }

                    File.Delete(path);
                    removed++;
                }
                catch (Exception)
                {
                    notRemoved++;
                }
            }

            clearings++;
            removedInAll += removed;

            Reading after = Reading.Unknown;

            try
            {
                after = Read(false);
            }
            catch (Exception error)
            {
                notWhole++;
                SayNotWholeOnce(error);
            }

            if (TextureCache.StillPast(after.Bytes, limitBytes))
            {
                stopChecking = true;
            }

            say(TextureCache.ClearedLine(
                folder, before.Bytes, before.FileCount, limitBytes, removed, notRemoved, goneBefore,
                after.Bytes, after.FileCount, clock() - from));
        }

        private void SayNotWholeOnce(Exception error)
        {
            if (notWholeSaid)
            {
                return;
            }

            notWholeSaid = true;
            say(TextureCache.NotReadWholeLine(folder, error.GetType().Name + ": " + error.Message));
        }

        /// <summary>What one walk read: the bytes and the count of files, and their paths where kept.</summary>
        private sealed class Reading
        {
            public static readonly Reading Unknown = new Reading(-1, -1, false);

            private readonly List<string> paths;

            public Reading(long bytes, int fileCount, bool keepPaths)
            {
                Bytes = bytes;
                FileCount = fileCount;
                paths = keepPaths ? new List<string>() : null;
            }

            public long Bytes { get; private set; }

            public int FileCount { get; private set; }

            public IList<string> Paths
            {
                get { return paths ?? (IList<string>)new string[0]; }
            }

            public void Add(FileInfo file)
            {
                Bytes += file.Length;
                FileCount++;

                if (paths != null)
                {
                    paths.Add(file.FullName);
                }
            }
        }
    }
}
