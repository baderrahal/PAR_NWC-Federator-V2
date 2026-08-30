using System;
using System.IO;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// Where a group's outputs go. One place builds these, so the path the NWF is looked
    /// for at and the path it is written to cannot drift apart. That drift is exactly the
    /// bug that would make a rerun rebuild an NWF it should have opened, and it would be
    /// invisible in a log because both lines would read the same.
    /// </summary>
    public static class OutputPaths
    {
        public const string NwfExtension = ".nwf";
        public const string NwdExtension = ".nwd";

        public static string Nwf(string folder, string outputName)
        {
            return For(folder, outputName, NwfExtension);
        }

        public static string Nwd(string folder, string outputName)
        {
            return For(folder, outputName, NwdExtension);
        }

        /// <summary>
        /// The full path for one output. The name comes from the grouping and already
        /// carries no extension, so the extension is added here and nowhere else.
        /// </summary>
        public static string For(string folder, string outputName, string extension)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            if (string.IsNullOrEmpty(outputName))
            {
                throw new ArgumentException("An output needs a name.", "outputName");
            }

            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("An output needs an extension.", "extension");
            }

            return Path.Combine(folder, outputName + extension);
        }
    }
}
