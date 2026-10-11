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
    }
}
