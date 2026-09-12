using System;
using System.Collections.Generic;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// The two files this tool reads off the Navisworks install at run time and never
    /// copies into this repo: the stylesheet Navisworks renders its own clash reports
    /// with, and the logo it puts on them. One locator for both, F36, because the two
    /// followed the same rule in two files.
    ///
    /// THE STYLESHEET. The client does not receive a native xlsx. Autodesk state that
    /// exporting clash results to xls is not possible and that the route is HTML
    /// (Tabular) opened in Excel, which is why samples\client-report\*.xlsx is an HTML
    /// page saved out of Excel and why it declares 53 columns with 17 populated. So the
    /// layout is not ours to invent. It is clash_report_html_tabular.xsl in the install,
    /// under the language folder.
    ///
    /// THE LOGO. The client accepts the report as it comes out of Clash Detective, logo
    /// and all, so the report this tool writes carries the same one. Measured on
    /// 2026-09-01: Images\logo.jpg at the top of the install, 6137 bytes, byte for byte
    /// the logo.jpg in the _files folder of both reports Bader supplied, MD5
    /// b5df301defc444485c0461748c56e0d9. A language folder is tried as well in case
    /// another install puts it there.
    ///
    /// NOTHING OF AUTODESK'S IS IN THIS REPO. No copy of either is committed, none ships
    /// in the bundle, and install.ps1 does not carry one. Both are read from the install
    /// on every run, so what goes out is a file that was already on the machine that
    /// wrote the report.
    ///
    /// Every path is built and tested DIRECTLY, never searched for and never wildcarded,
    /// which is the same rule the build uses for the Navisworks DLLs. The language folder
    /// is the only unknown, so the candidates are the language the application reports
    /// and then en-US, each tested as one full path.
    /// </summary>
    public static class InstallFiles
    {
        public const string StylesheetFolder = "stylesheets";

        public const string TabularStylesheet = "clash_report_html_tabular.xsl";

        public const string ImagesFolder = "Images";

        public const string LogoName = "logo.jpg";

        /// <summary>The fallback language folder, which every install has.</summary>
        public const string DefaultLanguage = "en-US";

        // ---------- the stylesheet ----------

        /// <summary>
        /// Every full path the stylesheet would be looked for at, in order, so a failure
        /// can name all of them rather than only the last.
        /// </summary>
        public static IList<string> StylesheetCandidates(string installFolder, string language)
        {
            List<string> paths = new List<string>();

            if (string.IsNullOrEmpty(installFolder))
            {
                return paths;
            }

            foreach (string folder in Languages(language))
            {
                Add(paths, Path.Combine(
                    Path.Combine(Path.Combine(installFolder, folder), StylesheetFolder), TabularStylesheet));
            }

            return paths;
        }

        /// <summary>
        /// The stylesheet, or an empty string when none of the candidates is on disk.
        /// Never throws, because a missing stylesheet turns one optional output off and is
        /// not a reason to lose a run.
        /// </summary>
        public static string FindStylesheet(string installFolder, string language)
        {
            return FirstOnDisk(StylesheetCandidates(installFolder, language));
        }

        /// <summary>
        /// What the log says when the stylesheet cannot be found. It names every path that
        /// was tried, because not found without the paths is the least useful line a log
        /// can carry.
        /// </summary>
        public static IList<string> WhyNoStylesheet(string installFolder, string language)
        {
            return WhyNot(
                "HTML     the client report layout comes from Autodesk's own "
                    + TabularStylesheet + ", read from the Navisworks install at run time. It is not "
                    + "there, so no HTML was written. The workbook and the XML are unaffected.",
                StylesheetCandidates(installFolder, language));
        }

        // ---------- the logo ----------

        /// <summary>
        /// Every full path the logo would be looked for at, in order. The one that is
        /// really there on 2025 is first, Images at the top of the install.
        /// </summary>
        public static IList<string> LogoCandidates(string installFolder, string language)
        {
            List<string> paths = new List<string>();

            if (string.IsNullOrEmpty(installFolder))
            {
                return paths;
            }

            Add(paths, Path.Combine(Path.Combine(installFolder, ImagesFolder), LogoName));

            foreach (string folder in Languages(language))
            {
                Add(paths, Path.Combine(
                    Path.Combine(Path.Combine(installFolder, folder), ImagesFolder), LogoName));
            }

            return paths;
        }

        /// <summary>
        /// The logo, or an empty string when none of the candidates is on disk. Never
        /// throws, because a missing picture turns one part of a page off and is not a
        /// reason to lose a report.
        /// </summary>
        public static string FindLogo(string installFolder, string language)
        {
            return FirstOnDisk(LogoCandidates(installFolder, language));
        }

        /// <summary>What the log says when the logo cannot be found, naming every path tried.</summary>
        public static IList<string> WhyNoLogo(string installFolder, string language)
        {
            return WhyNot(
                "LOGO     the report carries the logo Navisworks puts on its own, read "
                    + "from the install at run time. It is not there, so the page was written "
                    + "without one. Everything else on it is unaffected.",
                LogoCandidates(installFolder, language));
        }

        // ---------- shared ----------

        private static string FirstOnDisk(IList<string> candidates)
        {
            foreach (string path in candidates)
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

        private static IList<string> WhyNot(string first, IList<string> tried)
        {
            List<string> lines = new List<string> { first };

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
