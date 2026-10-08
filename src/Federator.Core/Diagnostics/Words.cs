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
        /// The number and its noun, 1 test and 2 tests, the one rule for it. Four files each wrote it
        /// out, T1-N47's shape, and one of them printed the number in the running culture.
        /// </summary>
        public static string Counted(int count, string one, string many)
        {
            return count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);
        }
    }
}
