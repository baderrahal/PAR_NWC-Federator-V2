using System;
using System.Collections.Generic;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// Finds the stylesheet Navisworks renders its own clash reports with.
    ///
    /// The client does not receive a native xlsx. Autodesk state that exporting clash
    /// results to xls is not possible and that the route is HTML (Tabular) opened in
    /// Excel, which is why samples\client-report\*.xlsx is an HTML page saved out of
    /// Excel and why it declares 53 columns with 17 populated.
    ///
    /// So the layout is not ours to invent. It is
    /// clash_report_html_tabular.xsl in the install, and this tool reads it from there at
    /// run time. It is Autodesk's file and no copy of it goes in this repo.
    ///
    /// The path is built and tested DIRECTLY, never searched for and never wildcarded,
    /// which is the same rule the build uses for the Navisworks DLLs. The language folder
    /// is the only unknown, so the candidates are the language the application reports and
    /// then en-US, each tested as one full path.
    /// </summary>
    public static class StylesheetLocator
    {
        public const string FolderName = "stylesheets";

        public const string TabularName = "clash_report_html_tabular.xsl";

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

            foreach (string folder in Languages(language))
            {
                string path = Path.Combine(
                    Path.Combine(Path.Combine(installFolder, folder), FolderName), TabularName);

                if (!paths.Contains(path))
                {
                    paths.Add(path);
                }
            }

            return paths;
        }

        /// <summary>
        /// The stylesheet, or an empty string when none of the candidates is on disk.
        /// Never throws, because a missing stylesheet turns one optional output off and is
        /// not a reason to lose a run.
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

            lines.Add("HTML     the client report layout comes from Autodesk's own "
                + TabularName + ", read from the Navisworks install at run time. It is not "
                + "there, so no HTML was written. The workbook and the XML are unaffected.");

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
