using System;
using System.Collections.Generic;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// Finds the logo Navisworks puts on its own clash reports.
    ///
    /// The client accepts the report as it comes out of Clash Detective, logo and all, so
    /// the report this tool writes carries the same one. It is not a Parsons mark and
    /// there is nothing to pick before a first run.
    ///
    /// WHERE IT IS. Measured on 2026-09-01:
    ///
    ///     C:\Program Files\Autodesk\Navisworks Manage 2025\Images\logo.jpg   6137 bytes
    ///
    /// and it is byte for byte the same file as the logo.jpg sitting in the _files folder
    /// of both reports Bader supplied, MD5 b5df301defc444485c0461748c56e0d9. That is what
    /// proves Navisworks copies this exact file into the report folder, which is what this
    /// tool now does too.
    ///
    /// NOTHING OF AUTODESK'S IS IN THIS REPO. No copy of logo.jpg is committed, none ships
    /// in the bundle, and install.ps1 does not carry one. It is read from the install on
    /// every run, the same way the stylesheet is, so what goes out is a file that was
    /// already on the machine that wrote the report.
    ///
    /// The path is built and tested DIRECTLY, never searched for and never wildcarded,
    /// which is the same rule the build uses for the Navisworks DLLs. Images sits at the
    /// top of the install with no language folder, checked, but a language folder is tried
    /// as well in case another install puts it there.
    /// </summary>
    public static class LogoLocator
    {
        public const string FolderName = "Images";

        public const string FileName = "logo.jpg";

        /// <summary>The fallback language folder, which every install has.</summary>
        public const string DefaultLanguage = "en-US";

        /// <summary>
        /// Every full path that would be tested, in order, so a failure can name all of
        /// them rather than only the last.
        /// </summary>
        public static IList<string> CandidatesIn(string installFolder, string language)
        {
            List<string> paths = new List<string>();

            if (string.IsNullOrEmpty(installFolder))
            {
                return paths;
            }

            // The one that is really there on 2025. Images sits at the top of the install.
            Add(paths, Path.Combine(Path.Combine(installFolder, FolderName), FileName));

            foreach (string folder in Languages(language))
            {
                Add(paths, Path.Combine(
                    Path.Combine(Path.Combine(installFolder, folder), FolderName), FileName));
            }

            return paths;
        }

        /// <summary>
        /// The logo, or an empty string when none of the candidates is on disk. Never
        /// throws, because a missing picture turns one part of a page off and is not a
        /// reason to lose a report.
        /// </summary>
        public static string Find(string installFolder, string language)
        {
            foreach (string path in CandidatesIn(installFolder, language))
            {
                try
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
                catch (Exception)
                {
                    // A path Windows will not even look at is not a match. The next
                    // candidate is tried and the caller is told if none of them is there.
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// What the log says when it cannot be found. It names every path that was tried,
        /// because "not found" without the paths is the least useful line a log can carry.
        /// </summary>
        public static IList<string> WhyNotFound(string installFolder, string language)
        {
            List<string> lines = new List<string>();

            lines.Add("LOGO     the report carries the logo Navisworks puts on its own, read "
                + "from the install at run time. It is not there, so the page was written "
                + "without one. Everything else on it is unaffected.");

            IList<string> tried = CandidatesIn(installFolder, language);

            if (tried.Count == 0)
            {
                lines.Add("         No install folder was given, so nothing could be looked for.");
                return lines;
            }

            foreach (string path in tried)
            {
                lines.Add("         looked at " + path);
            }

            return lines;
        }

        private static void Add(IList<string> paths, string path)
        {
            if (!paths.Contains(path))
            {
                paths.Add(path);
            }
        }

        private static IList<string> Languages(string language)
        {
            List<string> folders = new List<string>();

            if (!string.IsNullOrEmpty(language))
            {
                folders.Add(language.Trim());
            }

            if (!folders.Contains(DefaultLanguage))
            {
                folders.Add(DefaultLanguage);
            }

            return folders;
        }
    }
}
