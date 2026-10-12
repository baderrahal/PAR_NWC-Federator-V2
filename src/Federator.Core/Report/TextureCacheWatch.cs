using System;
using System.Collections.Generic;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// Watches Navisworks' texture cache across one picture pass, F147, by TextureCache's rule:
    /// read as the pass begins, between two tests' pictures at most once a check interval, cleared
    /// there once a size read whole is past the limit with its size said before and after, and read
    /// once more as the pass ends, never clearing there, for one line. Each line is handed to the
    /// caller to write.
    ///
    /// THE WALK NEVER LEAVES THE FOLDER. It is made by hand, a folder at a time, and a folder that is
    /// a link, a junction or a symbolic link, is counted and never stepped into, since the framework's
    /// own walk with AllDirectories follows links and a clearing would then remove another folder's
    /// files. A cache folder that is itself a link is not walked at all, and a folder that is not a
    /// rooted folder named ogs is refused before anything is read. A clearing removes only the files
    /// the walk just read, never a folder, and counts a file removed only when it was there to remove.
    ///
    /// It never throws. A walk that fails part way is said with why, counted as not whole, and
    /// clears nothing. A clearing that removed nothing is not tried again in the same pass, so files
    /// held open are not walked and refused once a minute for hours.
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
        private bool stopClearing;
        private bool folderThere;
        private bool linkSaid;
        private long atStart = -1;
        private int filesAtStart;
        private int reads;
        private int notWhole;
        private int clearings;
        private int removedInAll;
        private int linksLeft;
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

        /// <summary>As the pass begins: read once, and cleared if it is already past the limit. A refused folder is said here.</summary>
        public void AtTheStart()
        {
            if (limitBytes > 0 && refused)
            {
                say(TextureCache.RefusedLine(folder));
                return;
            }

            Check(true);
        }

        /// <summary>Between two tests' pictures: read once the interval has passed, and cleared past the limit.</summary>
        public void BetweenTests()
        {
            Check(false);
        }

        /// <summary>As the pass ends: read once more and never cleared, and the pass's one line handed back.</summary>
        public string AtTheEnd()
        {
            if (limitBytes <= 0)
            {
                return TextureCache.PassLine(folder, limitBytes, false, -1, 0, -1, 0, 0, 0, 0, 0, 0, 0);
            }

            if (refused)
            {
                return TextureCache.RefusedLine(folder);
            }

            try
            {
                Reading end = Read();

                return TextureCache.PassLine(
                    folder, limitBytes, folderThere, atStart, filesAtStart, end.Bytes, end.Files.Count,
                    reads, notWhole, clearings, removedInAll, linksLeft, seconds);
            }
            catch (Exception error)
            {
                return TextureCache.NotReadWholeLine(folder, error.GetType().Name + ": " + error.Message)
                    + ", as the pictures ended, cleared " + clearings + " time(s) before";
            }
        }

        private void Check(bool first)
        {
            if (limitBytes <= 0 || refused)
            {
                return;
            }

            try
            {
                if (!first && !TextureCache.DueToMeasure(measuredBefore, clock() - lastAt, everySeconds))
                {
                    return;
                }

                measuredBefore = true;
                Reading now = Read();
                lastAt = clock();

                if (first)
                {
                    atStart = now.Bytes;
                    filesAtStart = now.Files.Count;
                }

                if (!stopClearing && TextureCache.ShouldClear(now.Bytes, limitBytes))
                {
                    Clear(now);
                }
            }
            catch (Exception error)
            {
                say(TextureCache.NotReadWholeLine(folder, error.GetType().Name + ": " + error.Message) + ", kept going with the pictures");
            }
        }

        /// <summary>
        /// The bytes and the files under the folder, read a folder at a time and never into a link.
        /// Zero where there is no folder, and below zero, said once with why, where the walk failed.
        /// </summary>
        private Reading Read()
        {
            double from = clock();
            Reading reading = new Reading();
            reads++;

            try
            {
                DirectoryInfo root = new DirectoryInfo(folder);
                folderThere = root.Exists;

                if (!folderThere)
                {
                    return reading;
                }

                if ((root.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    if (!linkSaid)
                    {
                        say(TextureCache.LinkedLine(folder));
                        linkSaid = true;
                    }

                    stopClearing = true;
                    reading.Bytes = -1;
                    return reading;
                }

                int links = 0;
                Stack<DirectoryInfo> waiting = new Stack<DirectoryInfo>();
                waiting.Push(root);

                while (waiting.Count > 0)
                {
                    foreach (FileSystemInfo entry in waiting.Pop().EnumerateFileSystemInfos())
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

                        FileInfo file = (FileInfo)entry;
                        reading.Bytes += file.Length;
                        reading.Files.Add(file.FullName);
                    }
                }

                linksLeft = Math.Max(linksLeft, links);
                return reading;
            }
            catch (Exception error)
            {
                if (!(error is IOException) && !(error is UnauthorizedAccessException) && !(error is System.Security.SecurityException))
                {
                    throw;
                }

                notWhole++;
                say(TextureCache.NotReadWholeLine(folder, error.GetType().Name + ": " + error.Message));
                return new Reading { Bytes = -1 };
            }
            finally
            {
                seconds += clock() - from;
            }
        }

        private void Clear(Reading before)
        {
            double from = clock();
            int removed = 0;
            int notRemoved = 0;

            foreach (string path in before.Files)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    File.Delete(path);
                    removed++;
                }
                catch (IOException)
                {
                    notRemoved++;
                }
                catch (UnauthorizedAccessException)
                {
                    notRemoved++;
                }
            }

            seconds += clock() - from;
            clearings++;
            removedInAll += removed;

            if (removed == 0)
            {
                stopClearing = true;
            }

            Reading after = Read();

            say(TextureCache.ClearedLine(
                folder, before.Bytes, before.Files.Count, limitBytes, removed, notRemoved, after.Bytes, after.Files.Count,
                clock() - from));
        }

        private sealed class Reading
        {
            public long Bytes;
            public readonly List<string> Files = new List<string>();
        }
    }
}
