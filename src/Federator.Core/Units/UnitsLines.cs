using Federator.Core.Report;

namespace Federator.Core.Units
{
    /// <summary>
    /// The UNITS lines a group writes when the open document is put into the units the
    /// report needs, FR-062. They were built in the add-in, where no test reads them, and
    /// where the second read of the document's units threw the last line still named the
    /// units read before the models were set, as though the document had been read again.
    /// Every unit is handed in as the Navisworks enum name, the name UnitTable keys on.
    /// </summary>
    public static class UnitsLines
    {
        /// <summary>The document is in the wanted units already, so nothing was set.</summary>
        public static string AlreadyIn(string wanted)
        {
            return "UNITS    the document is already in " + Name(wanted) + ", so nothing was changed.";
        }

        /// <summary>The document is in other units, so every model is about to be set.</summary>
        public static string Setting(string before, string wanted)
        {
            return "UNITS    the document is in " + Name(before) + " and the report needs "
                + Name(wanted) + ". Setting every model.";
        }

        /// <summary>
        /// What setting the models did. After is the document's units read again once every
        /// model was set, or null where that read threw. Whether the document follows its
        /// models is UNKNOWN, so a read that threw names no unit as shown, and the units read
        /// before are said as that and nothing more.
        /// </summary>
        public static string AfterSetting(int changed, int failed, string before, string after)
        {
            string shows = after == null
                ? "the document's units could not be read again, so what it shows now is UNKNOWN, "
                    + "it showed " + Name(before) + " before"
                : "document shows " + Name(after);

            return "UNITS    " + changed + " model" + (changed == 1 ? string.Empty : "s") + " set, "
                + failed + " that would not, " + shows
                + ". Every report number is converted to " + ReportUnits.Name
                + " before it is written, so the report is metric whatever this says.";
        }

        /// <summary>
        /// The enum name with the short name UnitTable gives it, such as Feet (ft). A name the
        /// table does not carry is written alone, which is what a person needs to add the row.
        /// </summary>
        private static string Name(string enumName)
        {
            if (string.IsNullOrEmpty(enumName))
            {
                return "UNKNOWN";
            }

            UnitRow row = UnitTable.FindByEnumName(enumName);
            return enumName + " (" + (row == null ? enumName : row.Short) + ")";
        }
    }
}
