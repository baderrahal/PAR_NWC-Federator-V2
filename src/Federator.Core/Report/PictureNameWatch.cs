using System;
using System.Collections.Generic;

namespace Federator.Core.Report
{
    /// <summary>
    /// Two clashes that would write one jpg file, F157. One file name for two clashes means the
    /// second picture silently replaces the first, and a run that stopped with Clash35 and Clash36
    /// both naming cd300035.jpg could not say whether that was so. Compared on the full path
    /// without case, as Windows compares it, and told once per pair.
    /// </summary>
    public sealed class PictureNameWatch
    {
        private readonly Dictionary<string, string> firstClashOf =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Records the clash for its file. Returns null where the file is new to the pass, and the
        /// WARNING line naming the file, the test and both clashes where an earlier clash took it.
        /// </summary>
        public string Take(string path, string testName, string clashName)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string earlier;

            if (!firstClashOf.TryGetValue(path, out earlier))
            {
                firstClashOf[path] = clashName;
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
