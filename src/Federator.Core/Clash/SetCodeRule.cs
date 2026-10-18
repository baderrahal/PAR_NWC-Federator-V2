using System;
using System.Collections.Generic;
using Federator.Core.Teams;

namespace Federator.Core.Clash
{
    /// <summary>
    /// F224, Bader's answer to Q209 of 18 Oct 2026: run only the tests across two different set codes. BLD-ST against BLD-ST
    /// and BLD-AR against BLD-AR are not run, BLD-PL against BLD-DR and BLD-HV against BLD-FP still run, the same rule on both
    /// pages, and a group of one set code runs nothing, which keeps Q203. Each side's code is read by the one reader,
    /// CodeOf.Set, off the set name at the end of its locator, against the codes the run knows, the team map's and the
    /// group's own models', so a code that is a team of its own is still read. A test is left out only where BOTH sides read
    /// a code and the two are the same, Ordinal. A side whose name carries no code, such as BLD-Security Devices or
    /// BLD-DRPipe Accessories with its dash missing, leaves the test as it is, since which code it is is UNKNOWN and a test
    /// is never left out on a guess, and the CLASH line counts those tests so a nought is never read as every test judged.
    /// </summary>
    public static class SetCodeRule
    {
        /// <summary>The one code both sides of the test read, or null where the two differ or a side reads none.</summary>
        public static string SameCodeOf(PlannedClashTest test, IEnumerable<string> knownCodes, char separator)
        {
            string left;
            string right;

            if (!SidesRead(test, knownCodes, separator, out left, out right))
            {
                return null;
            }

            return string.Equals(left, right, StringComparison.Ordinal) ? left : null;
        }

        /// <summary>
        /// How many of these tests have a side whose set name reads no code the run knows, so the rule could not judge them
        /// and they run.
        /// </summary>
        public static int WithASideOfNoCode(IEnumerable<PlannedClashTest> tests, IEnumerable<string> knownCodes, char separator)
        {
            if (tests == null || knownCodes == null)
            {
                return 0;
            }

            List<string> known = new List<string>(knownCodes);
            int count = 0;

            foreach (PlannedClashTest test in tests)
            {
                string left;
                string right;

                if (test != null && !SidesRead(test, known, separator, out left, out right))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>The reason a test the rule leaves out carries, naming the code, in the words of the workbook.</summary>
        public static string Reason(string code)
        {
            return "both sides carry the set code " + code + ", and only tests across two set codes run";
        }

        /// <summary>
        /// The one CLASH line a group writes for the rule, how many it left out of how many, every group, a nought said, how
        /// many it could not judge because a side reads no code, and where no code was known that the rule could read none
        /// and left out nothing, so a nought is never read as checked.
        /// </summary>
        public static string Line(int leftOut, int planned, IDictionary<string, int> byCode, bool codesKnown, int sideOfNoCode)
        {
            if (!codesKnown)
            {
                return "CLASH    the rule of two set codes read no set code, no code being known for this group, so it left out none of the "
                    + planned + " tests planned, Bader's answer to Q209";
            }

            string notJudged = sideOfNoCode > 0
                ? ", and " + sideOfNoCode + " of the tests that run have a side whose set name reads no set code, so the rule could"
                    + " not judge " + (sideOfNoCode == 1 ? "it" : "them") + " and " + (sideOfNoCode == 1 ? "it runs" : "they run")
                : string.Empty;

            if (leftOut == 0)
            {
                return "CLASH    the rule of two set codes left out no test of the " + planned + " planned" + notJudged
                    + ", Bader's answer to Q209";
            }

            List<string> codes = new List<string>(byCode.Keys);
            codes.Sort(StringComparer.Ordinal);
            List<string> parts = new List<string>();

            foreach (string code in codes)
            {
                parts.Add(code + " " + byCode[code]);
            }

            return "CLASH    the rule of two set codes left out " + leftOut + " of the " + planned + " tests planned, "
                + "each setting a set code against itself, " + string.Join(", ", parts.ToArray())
                + ", so none of them is created or run" + notJudged + ", Bader's answer to Q209";
        }

        /// <summary>
        /// The CLASH line of a group of one discipline, where Q203 runs none of its tests, so the rule of two set codes is
        /// not read and every test keeps the one reason of Q203.
        /// </summary>
        public static string NotReadInAGroupOfOne(int planned)
        {
            return "CLASH    the rule of two set codes is not read, the group holds one discipline, so none of its " + planned
                + " tests planned runs, Bader's answers to Q203 and Q209";
        }

        /// <summary>Both sides' codes, true only where each side reads one.</summary>
        private static bool SidesRead(PlannedClashTest test, IEnumerable<string> knownCodes, char separator, out string left, out string right)
        {
            left = string.Empty;
            right = string.Empty;

            if (test == null || knownCodes == null || test.Left == null || test.Right == null)
            {
                return false;
            }

            left = CodeOf.Set(ByDesignRule.SetNameIn(test.Left.Locator), knownCodes, separator);
            right = CodeOf.Set(ByDesignRule.SetNameIn(test.Right.Locator), knownCodes, separator);

            return left.Length > 0 && right.Length > 0;
        }
    }
}
