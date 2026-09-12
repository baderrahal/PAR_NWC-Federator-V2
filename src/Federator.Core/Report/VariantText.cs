using System;

namespace Federator.Core.Report
{
    /// <summary>
    /// Turning a Navisworks property value into text, for the one case the add-in cannot
    /// handle by kind.
    ///
    /// WHY THIS EXISTS. A run on 2026-09-01 threw 426 times with
    ///
    ///     System.NotSupportedException: Not supported if '!IsDisplayString'
    ///        at Autodesk.Navisworks.Api.VariantData.ToDisplayString()
    ///        at Federator.Addin.Engine.ClashHarvest.FirstProperty
    ///
    /// because every To&lt;Kind&gt;() on VariantData is kind specific and a Revit element id
    /// is an Int32, not a display string. One throw took the whole of Describe with it, so
    /// the element id, the source file and the discipline were all left empty and the
    /// client report lost its Item ID column entirely.
    ///
    /// The one member that returns a value REGARDLESS of kind is VariantData.ToString().
    /// Its IL, read off the installed DLL on 2026-09-01, calls GetDataType and then
    /// switches to the matching To&lt;Kind&gt;() accessor, so it never throws. It does prefix
    /// the kind name though, so it hands back "Int32:702888" rather than "702888". See
    /// docs\history\scan.md section 4n.
    ///
    /// So the add-in reads by kind, which gives the value clean, and falls back to
    /// ToString() for a kind it does not know. This strips the prefix off that fallback.
    /// </summary>
    public static class VariantText
    {
        /// <summary>
        /// The value out of a VariantData.ToString(), with the kind prefix removed.
        ///
        /// The prefix is only removed when it is exactly the kind that was reported,
        /// because a display string can legitimately hold a colon and nothing should eat
        /// half of it. "Element ID: 702888" as a value keeps every character.
        /// </summary>
        public static string Clean(string toString, string dataTypeName)
        {
            if (string.IsNullOrEmpty(toString))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(dataTypeName))
            {
                return toString.Trim();
            }

            string prefix = dataTypeName + ":";

            if (!toString.StartsWith(prefix, StringComparison.Ordinal))
            {
                return toString.Trim();
            }

            return toString.Substring(prefix.Length).Trim();
        }

        /// <summary>
        /// What ToString gives for a value it cannot represent. Both come back as the
        /// word itself, so neither is a value and both read as nothing.
        /// </summary>
        public static bool IsNothing(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            string tidied = value.Trim();

            // Spaces alone are nothing too. Left out, "  " came back as a value and would
            // have gone into a cell as an invisible one.
            if (tidied.Length == 0)
            {
                return true;
            }

            return string.Equals(tidied, "None", StringComparison.Ordinal)
                || string.Equals(tidied, "Disposed", StringComparison.Ordinal)
                || string.Equals(tidied, "Unknown", StringComparison.Ordinal)
                || string.Equals(tidied, "<null>", StringComparison.Ordinal);
        }
    }
}
