namespace Federator.Core.Exchange
{
    /// <summary>
    /// Where the list of corrections for a picked clash XML is kept, Q113 answered B by Bader on
    /// 2026-10-04: beside the XML and named after it, the XML's file name without its extension
    /// followed by a suffix. So a.xml is corrected by a.corrections.txt in the same folder.
    ///
    /// ONE FULL PATH, NEVER A SEARCH. The list is found by joining the XML's folder, its name and
    /// the suffix, and that one path is tested with File.Exists. Nothing looks around the folder
    /// for a file that might be meant, because a list picked up by a near match would correct a
    /// file with another project's decisions and the log would read as if it had been asked.
    /// The join is ListFile.PathBeside, which the team map beside the XML is found by too, F131.
    ///
    /// THE SUFFIX IS A SETTING, the rule for every name that shapes a run.
    /// </summary>
    public sealed class CorrectionListSettings
    {
        /// <summary>What follows the XML's name without its extension to make the list's name.</summary>
        public const string DefaultSuffix = ".corrections.txt";

        public CorrectionListSettings()
        {
            Suffix = DefaultSuffix;
        }

        /// <summary>What follows the XML's name without its extension to make the list's name.</summary>
        public string Suffix { get; set; }

        /// <summary>The full path of the list beside that XML, whether or not a file is there, ListFile.PathBeside.</summary>
        public string PathBeside(string xmlPath)
        {
            return ListFile.PathBeside(xmlPath, Suffix);
        }
    }
}
