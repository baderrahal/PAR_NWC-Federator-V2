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
    /// a code and the two are the same, Ordinal. A side whose name carries no code, such as BLD-Security Devices, leaves the
    /// test as it is, since which code it is is UNKNOWN and a test is never left out on a guess.
    /// </summary>
    public static class SetCodeRule
    {
        /// <summary>The one code both sides of the test read, or null where the two differ or a side reads none.</summary>
        public static string SameCodeOf(PlannedClashTest test, IEnumerable<string> knownCodes, char separator)
        {
            if (test == null || knownCodes == null)
            {
                return null;
            }

            List<string> known = new List<string>(knownCodes);
            string left = CodeOf.Set(ByDesignRule.SetNameIn(test.Left.Locator), known, separator);
            string right = CodeOf.Set(ByDesignRule.SetNameIn(test.Right.Locator), known, separator);

            return left.Length > 0 && string.Equals(left, right, StringComparison.Ordinal) ? left : null;
        }

        /// <summary>The reason a test the rule leaves out carries, naming the code.</summary>
        public static string Reason(string code)
        {
            return "both sides carry the set code " + code + ", and only tests across two set codes run, Bader's answer to Q209";
        }

        /// <summary>
        /// The one CLASH line a group writes for the rule, how many it left out of how many, every group, a nought said, and
        /// where no code was known that the rule could read none and left out nothing, so a nought is never read as checked.
        /// </summary>
        public static string Line(int leftOut, int planned, IDictionary<string, int> byCode, bool codesKnown)
        {
            if (!codesKnown)
            {
                return "CLASH    the rule of two set codes read no set code, no code being known for this group, so it left out none of the "
                    + planned + " tests planned, Bader's answer to Q209";
            }

            if (leftOut == 0)
            {
                return "CLASH    the rule of two set codes left out no test of the " + planned + " planned, Bader's answer to Q209";
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
                + ", so none of them is created or run, Bader's answer to Q209";
        }
    }
}
