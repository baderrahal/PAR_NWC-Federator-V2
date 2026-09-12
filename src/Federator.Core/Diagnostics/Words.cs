namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The one place a missing value is given a word. Twelve files each carried a private
    /// Or doing exactly this, F36, and one copy is enough.
    /// </summary>
    public static class Words
    {
        /// <summary>The value, or the fallback where the value is null or empty.</summary>
        public static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
