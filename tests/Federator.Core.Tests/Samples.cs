using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Finds the samples folder by walking up from the test assembly, so the tests read
    /// the real files rather than a copy that could drift.
    /// </summary>
    public static class Samples
    {
        /// <summary>
        /// The clash export. The name on disk carries spaces and brackets. The underscore
        /// spelling is accepted too, in case the file is ever renamed on the way in.
        /// </summary>
        public static readonly string[] AllInOneNames =
        {
            "1104-PAR_CLASH_AllInOne (2) (1).xml",
            "1104-PAR_CLASH_AllInOne__2___1_.xml"
        };

        public static readonly string[] BuildingNames =
        {
            "Search Set Building.xml",
            "Search_Set_Building.xml"
        };

        public static readonly string[] InfraNames =
        {
            "Search Set Infra.xml",
            "Search_Set_Infra.xml"
        };

        public static string AllInOne()
        {
            return Resolve(AllInOneNames);
        }

        public static string Building()
        {
            return Resolve(BuildingNames);
        }

        public static string Infra()
        {
            return Resolve(InfraNames);
        }

        /// <summary>
        /// The checkout itself, found the same way the samples folder is. The fixtures
        /// that read the client's exports and the stylesheet need the root rather than
        /// the samples folder, and this is the one place that walk is written.
        /// </summary>
        public static string Repo()
        {
            DirectoryInfo directory = new DirectoryInfo(
                Path.GetDirectoryName(new Uri(typeof(Samples).Assembly.CodeBase).LocalPath));

            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ParsonsNwcFederator.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }

        public static string Folder()
        {
            string start = Path.GetDirectoryName(new Uri(typeof(Samples).Assembly.CodeBase).LocalPath);
            DirectoryInfo directory = new DirectoryInfo(start);

            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "samples");

                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException(
                "No samples folder found at or above " + start + ".");
        }

        private static string Resolve(IEnumerable<string> candidates)
        {
            string folder = Folder();
            List<string> tried = new List<string>();

            foreach (string candidate in candidates)
            {
                string full = Path.Combine(folder, candidate);
                tried.Add(candidate);

                if (File.Exists(full))
                {
                    return full;
                }
            }

            throw new FileNotFoundException(
                "None of these sample files exist in " + folder + ": "
                    + string.Join(", ", tried.ToArray()));
        }
    }
}
