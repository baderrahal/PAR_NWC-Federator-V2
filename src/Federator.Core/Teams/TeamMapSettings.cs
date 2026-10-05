using Federator.Core.Exchange;

namespace Federator.Core.Teams
{
    /// <summary>
    /// Where the team map of a picked clash XML is kept, and the word a code no name carries
    /// reads as, Q114 points 1 and 2 and Q115 answered by default A: the map is a plain file of
    /// its own BESIDE the XML and named after it, the XML's file name without its extension
    /// followed by a suffix, so a.xml is read with a.teams.txt in the same folder, beside the
    /// list of corrections of Q113 and never inside it, so a fault in one list cannot leave the
    /// other unread.
    ///
    /// ONE FULL PATH, NEVER A SEARCH, ListFile.PathBeside, the join the list of corrections is
    /// found by. Nothing looks around the folder for a map that might be meant.
    ///
    /// EVERY NAME HERE IS A SETTING, the rule for every name that shapes a run.
    /// </summary>
    public sealed class TeamMapSettings
    {
        /// <summary>What follows the XML's name without its extension to make the map's name.</summary>
        public const string DefaultSuffix = ".teams.txt";

        /// <summary>
        /// The team of a side or a model whose name carries no discipline code this tool can
        /// read. The client's own matrix holds one such set, BLD-Security Devices, and Q117's
        /// default A keeps it UNKNOWN and named, never guessed. The one place the word is
        /// set, ViewpointSettings.DefaultUnknownDiscipline reads it.
        /// </summary>
        public const string DefaultUnknownTeam = "UNKNOWN";

        public TeamMapSettings()
        {
            Suffix = DefaultSuffix;
            UnknownTeam = DefaultUnknownTeam;
        }

        /// <summary>What follows the XML's name without its extension to make the map's name.</summary>
        public string Suffix { get; set; }

        /// <summary>The team of a name that carries no code this tool can read.</summary>
        public string UnknownTeam { get; set; }

        /// <summary>The full path of the map beside that XML, whether or not a file is there.</summary>
        public string PathBeside(string xmlPath)
        {
            return ListFile.PathBeside(xmlPath, Suffix);
        }
    }
}
