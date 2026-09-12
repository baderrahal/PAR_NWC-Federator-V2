using System;
using System.Text;

namespace Federator.Core.Report
{
    /// <summary>
    /// What the one worksheet is called, and what Excel refuses in a name.
    ///
    /// Excel stops a sheet name at 31 characters. The workbook is one sheet named after
    /// the report, cut at 31 the way the client's own export is, so no sheet is ever
    /// named after a test.
    /// </summary>
    public static class SheetNames
    {
        /// <summary>Excel's limit. Not a preference, a limit.</summary>
        public const int MaxLength = 31;

        /// <summary>
        /// The one sheet, named after the report the way theirs is. Excel stops a sheet
        /// name at 31 characters, so theirs reads 1104-PAR-1A04WN-XXX-BM-RPT-0000, which
        /// is the file name cut at 31.
        /// </summary>
        public static string ForReport(string outputName)
        {
            string name = Sanitise(outputName ?? string.Empty, "Clash Report");

            return name.Length <= MaxLength ? name : name.Substring(0, MaxLength);
        }

        /// <summary>
        /// Characters Excel refuses in a sheet name. Reading them off one list rather than
        /// off several is what stops the two halves disagreeing.
        /// </summary>
        public static readonly char[] Refused = { ':', '\\', '/', '?', '*', '[', ']' };

        /// <summary>
        /// True when Excel will accept this exactly as it stands. Everything this tool
        /// generates has to pass this, which is what the tests assert.
        /// </summary>
        internal static bool IsAcceptable(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length > MaxLength)
            {
                return false;
            }

            foreach (char refused in Refused)
            {
                if (name.IndexOf(refused) >= 0)
                {
                    return false;
                }
            }

            // Excel refuses a leading or trailing apostrophe, and a name that is only
            // whitespace.
            return name[0] != '\'' && name[name.Length - 1] != '\'' && name.Trim().Length > 0;
        }

        /// <summary>
        /// Anything Excel would refuse, made acceptable. A name that reached a sheet
        /// unchecked would throw at the point of writing and lose the whole workbook, so
        /// this is the floor the one sheet name goes through.
        /// </summary>
        internal static string Sanitise(string name)
        {
            return Sanitise(name, "Sheet");
        }

        public static string Sanitise(string name, string fallback)
        {
            if (name == null)
            {
                name = string.Empty;
            }

            StringBuilder tidied = new StringBuilder(name.Length);

            foreach (char c in name)
            {
                bool bad = false;

                foreach (char refused in Refused)
                {
                    if (c == refused)
                    {
                        bad = true;
                        break;
                    }
                }

                tidied.Append(bad ? ' ' : c);
            }

            string result = tidied.ToString().Trim().Trim('\'').Trim();

            if (result.Length > MaxLength)
            {
                result = result.Substring(0, MaxLength).Trim().Trim('\'').Trim();
            }

            return result.Length == 0
                ? (string.IsNullOrEmpty(fallback) ? "Sheet" : fallback)
                : result;
        }
    }
}
