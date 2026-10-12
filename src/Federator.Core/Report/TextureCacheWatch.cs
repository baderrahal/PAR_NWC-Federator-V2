using System;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// Watches Navisworks' texture cache across one picture pass, F147, by TextureCache's rule:
    /// measured as the pass begins, between two tests' pictures at most once a check interval,
    /// cleared there once it is past the limit with its size said before and after, and measured
    /// once more as the pass ends for one line. Each line is handed to the caller to write.
    ///
    /// It never throws. A cache that cannot be read or cleared is said and the pass goes on, since
    /// logging never stops a run. A file in use is left where it is and counted. Folders are never
    /// removed, only the files under them.
    /// </summary>
    public sealed class TextureCacheWatch
    {
        private readonly string folder;
        private readonly long limitBytes;
        private readonly int everySeconds;
        private readonly Func<double> clock;
        private readonly Action<string> say;

        private bool measuredBefore;
        private double lastAt;
        private long atStart = -1;
        private int measured;
        private int cleared;
        private bool folderThere;

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
        }

        /// <summary>The cache folder under a temp folder, where Navisworks keeps it.</summary>
        public static string FolderIn(string tempFolder)
        {
            return Path.Combine(tempFolder ?? string.Empty, TextureCache.FolderUnderTemp);
        }

        /// <summary>How many times it was cleared in this pass.</summary>
        public int Cleared
        {
            get { return cleared; }
        }

        /// <summary>As the pass begins: measured once, and cleared if it is already past the limit.</summary>
        public void AtTheStart()
        {
            Check(true);
        }

        /// <summary>Between two tests' pictures: measured once the interval has passed, and cleared past the limit.</summary>
        public void BetweenTests()
        {
            Check(false);
        }

        /// <summary>As the pass ends: measured once more, and the pass's one line handed back.</summary>
        public string AtTheEnd()
        {
            if (limitBytes <= 0)
            {
                return TextureCache.PassLine(limitBytes, -1, -1, measured, cleared, false);
            }

            try
            {
                int files;
                long atEnd = Measure(out files);

                return TextureCache.PassLine(limitBytes, atStart, atEnd, measured, cleared, folderThere);
            }
            catch (Exception error)
            {
                return "the texture cache under %TEMP%\\" + TextureCache.FolderUnderTemp + " could not be measured as the pictures ended, "
                    + error.GetType().Name + ": " + error.Message + ", cleared " + cleared + " time(s) before";
            }
        }

        private void Check(bool first)
        {
            if (limitBytes <= 0)
            {
                return;
            }

            try
            {
                double now = clock();

                if (!first && !TextureCache.DueToMeasure(measuredBefore, now - lastAt, everySeconds))
                {
                    return;
                }

                measuredBefore = true;
                lastAt = now;

                int files;
                long bytes = Measure(out files);

                if (first)
                {
                    atStart = bytes;
                }

                if (TextureCache.ShouldClear(bytes, limitBytes))
                {
                    Clear(bytes, files);
                }
            }
            catch (Exception error)
            {
                say("the texture cache under %TEMP%\\" + TextureCache.FolderUnderTemp + " could not be checked, "
                    + error.GetType().Name + ": " + error.Message + ", kept going with the pictures");
            }
        }

        /// <summary>The bytes of every file under the folder, zero where there is no folder, below zero where the walk failed.</summary>
        private long Measure(out int files)
        {
            files = 0;
            measured++;
            folderThere = Directory.Exists(folder);

            if (!folderThere)
            {
                return 0;
            }

            long total = 0;

            try
            {
                foreach (string path in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        total += new FileInfo(path).Length;
                        files++;
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            }
            catch (IOException)
            {
                return -1;
            }
            catch (UnauthorizedAccessException)
            {
                return -1;
            }

            return total;
        }

        private void Clear(long before, int filesBefore)
        {
            string[] paths;

            try
            {
                paths = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            }
            catch (Exception error)
            {
                say("the texture cache under %TEMP%\\" + TextureCache.FolderUnderTemp + " held " + TextureCache.Size(before)
                    + ", past the limit of " + TextureCache.Size(limitBytes) + ", and could not be listed to clear it, "
                    + error.GetType().Name + ": " + error.Message + ", so nothing was removed");
                return;
            }

            int removed = 0;
            int left = 0;

            foreach (string path in paths)
            {
                try
                {
                    File.Delete(path);
                    removed++;
                }
                catch (IOException)
                {
                    left++;
                }
                catch (UnauthorizedAccessException)
                {
                    left++;
                }
            }

            cleared++;

            int filesAfter;
            long after = Measure(out filesAfter);

            say(TextureCache.ClearedLine(before, filesBefore, limitBytes, removed, left, after));
        }
    }
}
