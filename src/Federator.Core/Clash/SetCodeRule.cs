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
    ///
    /// SINCE F247, BADER'S ANSWER Q230 OF 20 OCT 2026, a test of one set code is left out only where that code is one the
    /// pack's rules name, PackRules.SameCodeLeftOut, AR and ST in 1104-PAR, so the tests of one set code of ME, PL, DR, FF
    /// and EL run again on both pages. Which codes is the pack's, never a constant here, and a pack whose rules are not
    /// read leaves out none. The CLASH line names the codes left out and counts the tests of one set code that run.
    /// </summary>
    public static class SetCodeRule
    {
        /// <summary>The one code both sides of the test read, or null where the two differ or a side reads none.</summary>
        public static string SameCodeOf(PlannedClashTest test, IEnumerable<string> knownCodes, char separator)
        {
            if (test == null || test.Left == null || test.Right == null)
            {
                return null;
            }

            return SameCodeOfSets(ByDesignRule.SetNameIn(test.Left.Locator), ByDesignRule.SetNameIn(test.Right.Locator), knownCodes, separator);
        }

        /// <summary>
        /// The one code two set names read, or null where the two differ or a name reads none, the one reading of one set code
        /// for a test and for a by design pair alike, F247.
        /// </summary>
        public static string SameCodeOfSets(string leftSetName, string rightSetName, IEnumerable<string> knownCodes, char separator)
        {
            string left;
            string right;

            if (!NamesRead(leftSetName, rightSetName, knownCodes, separator, out left, out right))
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

        /// <summary>
        /// How many of these tests set one set code against itself, by that code, the tests of one set code the pack's rules
        /// let run when handed the plan the rule kept.
        /// </summary>
        public static IDictionary<string, int> OfOneCode(IEnumerable<PlannedClashTest> tests, IEnumerable<string> knownCodes, char separator)
        {
            Dictionary<string, int> byCode = new Dictionary<string, int>(StringComparer.Ordinal);

            if (tests == null || knownCodes == null)
            {
                return byCode;
            }

            List<string> known = new List<string>(knownCodes);

            foreach (PlannedClashTest test in tests)
            {
                string code = SameCodeOf(test, known, separator);

                if (code != null)
                {
                    int seen;
                    byCode[code] = byCode.TryGetValue(code, out seen) ? seen + 1 : 1;
                }
            }

            return byCode;
        }

        /// <summary>The reason a test the rule leaves out carries, naming the code, in the words of the workbook.</summary>
        public static string Reason(string code)
        {
            return "both sides carry the set code " + code + ", a code whose tests against itself the project's rules leave out";
        }

        /// <summary>
        /// The one CLASH line a group writes for the rule, how many it left out of how many, every group, a nought said, the
        /// codes the pack's rules leave out, how many tests of one set code of another code run, how many it could not judge
        /// because a side reads no code, and where no code was known that the rule could read none and left out nothing, so a
        /// nought is never read as checked.
        /// </summary>
        public static string Line(int leftOut, int planned, IDictionary<string, int> byCode, bool codesKnown, int sideOfNoCode,
            IList<string> leftOutCodes, IDictionary<string, int> runOfOneCode)
        {
            const string Answers = ", Bader's answers to Q209 and Q230";

            if (!codesKnown)
            {
                return "CLASH    the rule of two set codes read no set code, no code being known for this group, so it left out none of the "
                    + planned + " tests planned" + Answers;
            }

            string leaves = leftOutCodes == null
                ? "the pack's rules not read, so no set code is left out against itself"
                : leftOutCodes.Count == 0
                ? "the pack's rules leaving out no set code against itself"
                : "the pack's rules leaving out " + Federator.Core.Exchange.PackRules.Joined(leftOutCodes) + " against "
                    + (leftOutCodes.Count == 1 ? "itself" : "themselves") + " alone";

            int running = 0;

            foreach (int count in (runOfOneCode ?? new Dictionary<string, int>()).Values)
            {
                running += count;
            }

            string run = running == 0
                ? ", and no test of one set code runs"
                : ", and " + running + (running == 1 ? " test" : " tests") + " of one set code "
                    + (running == 1 ? "runs, " : "run, ") + ByCode(runOfOneCode);

            string notJudged = sideOfNoCode > 0
                ? ", and " + sideOfNoCode + " of the tests that run have a side whose set name reads no set code, so the rule could"
                    + " not judge " + (sideOfNoCode == 1 ? "it" : "them") + " and " + (sideOfNoCode == 1 ? "it runs" : "they run")
                : string.Empty;

            if (leftOut == 0)
            {
                return "CLASH    the rule of two set codes left out no test of the " + planned + " planned, " + leaves + run + notJudged
                    + Answers;
            }

            return "CLASH    the rule of two set codes left out " + leftOut + " of the " + planned + " tests planned, "
                + "each setting a set code against itself, " + ByCode(byCode)
                + ", so none of them is created or run, " + leaves + run + notJudged + Answers;
        }

        /// <summary>The counts by code, AR 120, ST 15, in Ordinal order of the code.</summary>
        private static string ByCode(IDictionary<string, int> byCode)
        {
            List<string> codes = new List<string>(byCode.Keys);
            codes.Sort(StringComparer.Ordinal);
            List<string> parts = new List<string>();

            foreach (string code in codes)
            {
                parts.Add(code + " " + byCode[code]);
            }

            return string.Join(", ", parts.ToArray());
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

            if (test == null || test.Left == null || test.Right == null)
            {
                return false;
            }

            return NamesRead(ByDesignRule.SetNameIn(test.Left.Locator), ByDesignRule.SetNameIn(test.Right.Locator), knownCodes, separator,
                out left, out right);
        }

        /// <summary>Both set names' codes, true only where each name reads one.</summary>
        private static bool NamesRead(string leftSetName, string rightSetName, IEnumerable<string> knownCodes, char separator,
            out string left, out string right)
        {
            left = string.Empty;
            right = string.Empty;

            if (knownCodes == null)
            {
                return false;
            }

            left = CodeOf.Set(leftSetName, knownCodes, separator);
            right = CodeOf.Set(rightSetName, knownCodes, separator);

            return left.Length > 0 && right.Length > 0;
        }

        /// <summary>
        /// The words of the NWF page's confirm on the tests of one set code, F247, Bader's answer Q230: which codes the
        /// project's rules leave out, read off the pack, or that every one runs where they leave out none or were not read.
        /// </summary>
        public static string ConfirmWords(IList<string> leftOutCodes)
        {
            return leftOutCodes == null || leftOutCodes.Count == 0
                ? "A test whose two sides carry one set code runs, the project's rules leaving out none. "
                : "A test whose two sides carry one set code is not run where that code is "
                    + Federator.Core.Exchange.PackRules.Joined(leftOutCodes).Replace(" and ", " or ")
                    + ", as the project's rules say, and one saved in the NWF keeps its earlier results there. ";
        }
    }
}
