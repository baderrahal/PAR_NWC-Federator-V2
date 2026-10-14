using System;
using System.Collections.Generic;

namespace Federator.Core.Report
{
    /// <summary>
    /// Two clashes that would write one jpg file, F157, read after F161. One file name for two
    /// clashes means the second picture silently replaces the first. Only a file that WAS WRITTEN
    /// counts, Wrote, because a picture that failed gives its number back, ReleaseImageIndex, so
    /// the next clash is handed the same file name for a file nobody wrote. Run 20261014-105308
    /// named one file for ten failed clashes, and the first form of this watch warned ten times
    /// of a replace that could not happen. Compared on the full path without case, as Windows
    /// compares it.
    /// </summary>
    public sealed class PictureNameWatch
    {
        private readonly Dictionary<string, string> firstClashOf =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Records that the clash's picture reached the disk at the path. Called once the file is
        /// written and read back, and never for a picture that failed.
        /// </summary>
        public void Wrote(string path, string clashName)
        {
            if (!string.IsNullOrEmpty(path) && !firstClashOf.ContainsKey(path))
            {
                firstClashOf[path] = clashName;
            }
        }

        /// <summary>
        /// Null where no earlier clash wrote the file, and the WARNING line naming the file, the
        /// test and both clashes where one did, so the picture about to be written replaces it.
        /// Records nothing.
        /// </summary>
        public string WouldReplace(string path, string testName, string clashName)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string earlier;

            if (!firstClashOf.TryGetValue(path, out earlier))
            {
                return null;
            }

            return "WARNING  two clashes would write the same picture file " + path + ". In "
                + (string.IsNullOrEmpty(testName) ? "an unnamed test" : testName) + ", the clash "
                + (string.IsNullOrEmpty(earlier) ? "an unnamed clash" : earlier) + " has it already and the clash "
                + (string.IsNullOrEmpty(clashName) ? "an unnamed clash" : clashName)
                + " would replace it. The earlier picture is lost if this one is written";
        }
    }
}
