using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The CALL line for one Navisworks call that took the setting's seconds or more, F143. One
    /// call runs on the window's thread and cannot be split, so for its length the window cannot
    /// answer, and the line names the call, what it was on and its seconds, so a long wait inside
    /// a single call is never silent. Null under the setting and for a length not known.
    /// </summary>
    public static class LongCall
    {
        /// <summary>The kind the line starts with, padded as the log's other kinds are.</summary>
        public const string Kind = "CALL     ";

        public static string Line(string call, string on, double seconds, double longCallSeconds)
        {
            if (double.IsNaN(seconds) || seconds < 0.0 || seconds < longCallSeconds)
            {
                return null;
            }

            return Kind + call + " on " + on + " took "
                + seconds.ToString("0.0", CultureInfo.InvariantCulture)
                + "s, one Navisworks call, so the window could not answer for that long";
        }

        /// <summary>
        /// Writes the line where there is one, with its .tsv row, the seconds as its number, through
        /// RunLog.Numbered, the one way a line carrying a number reaches the log. Called by the
        /// add-in in the finally around the call, so a call that throws after a long wait is named
        /// too. A null log writes nothing.
        /// </summary>
        public static void Say(RunLog log, string call, string on, double seconds, double longCallSeconds)
        {
            string line = Line(call, on, seconds, longCallSeconds);

            if (log == null || line == null)
            {
                return;
            }

            log.Numbered(line, "long call", call, seconds.ToString("0.0", CultureInfo.InvariantCulture), on);
        }
    }
}
