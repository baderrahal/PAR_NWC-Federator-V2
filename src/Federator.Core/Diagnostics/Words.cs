using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The one place a missing value is given a word, and a count its noun. Twelve files each
    /// carried a private Or doing exactly this, F36, and one copy is enough.
    /// </summary>
    public static class Words
    {
        /// <summary>The value, or the fallback where the value is null or empty.</summary>
        public static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        /// <summary>
        /// The number and its noun, 1 test and 2 tests, the one rule for it. Five files each wrote it
        /// out, and four of them printed the number in the running culture, which differs only in the
        /// sign of a negative one.
        /// </summary>
        public static string Counted(int count, string one, string many)
        {
            return count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);
        }

        /// <summary>
        /// A reason with its closing full stop and spaces taken off, so it reads inside a sentence that goes on, and UNKNOWN
        /// where there is none. F219, the reviewer's finding that a parser's reason read "part 5., so".
        /// </summary>
        public static string TrimStop(string reason)
        {
            string trimmed = (reason ?? string.Empty).Trim().TrimEnd('.').TrimEnd();
            return trimmed.Length == 0 ? "UNKNOWN" : trimmed;
        }
    }
}
