using System;
using System.Globalization;
using System.Text;

namespace Federator.Core.Report
{
    /// <summary>
    /// What a worksheet is called.
    ///
    /// Excel stops a sheet name at 31 characters, and 1703 of the 1830 test names in the
    /// reference file are longer than that, so a sheet is never named after its test. It
    /// is T0001 upward and the full name lives in the Summary sheet, where a cell can hold
    /// anything.
    /// </summary>
    public static class SheetNames
    {
        /// <summary>Excel's limit. Not a preference, a limit.</summary>
        public const int MaxLength = 31;

        /// <summary>The prefix a test sheet is named with. A setting, not a constant.</summary>
        public const string TestPrefix = "T";

        /// <summary>How many digits a test number is padded to, so T0001 sorts before T0010.</summary>
        public const int TestDigits = 4;

        public const string SummarySheet = "Summary";

        public const string MatrixSheet = "Matrix";

        /// <summary>
        /// Characters Excel refuses in a sheet name. Reading them off one list rather than
        /// off several is what stops the two halves disagreeing.
        /// </summary>
        public static readonly char[] Refused = { ':', '\\', '/', '?', '*', '[', ']' };

        /// <summary>
        /// The sheet for one test, numbered from one. Padded to TestDigits, and past that
        /// it simply grows, because a wrong name is worse than an unpadded one. T0001 at
        /// one, T1000 at a thousand, T10000 at ten thousand.
        /// </summary>
        public static string ForTest(int number)
        {
            if (number < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "number", "Test numbers start at one, so there is no sheet for " + number + ".");
            }

            return TestPrefix + number.ToString(
                new string('0', TestDigits), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// True when Excel will accept this exactly as it stands. Everything this tool
        /// generates has to pass this, which is what the tests assert.
        /// </summary>
        public static bool IsAcceptable(string name)
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
        /// Anything Excel would refuse, made acceptable. Nothing this tool writes should
        /// need it, because sheets are numbered, but a name that reached a sheet unchecked
        /// would throw at the point of writing and lose the whole workbook, so it is here
        /// as the floor rather than as the plan.
        /// </summary>
        public static string Sanitise(string name)
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
