using System;
using System.Globalization;
using Federator.Core.Diagnostics;

namespace Federator.Core.Report
{
    /// <summary>
    /// One thing the run measured and the report does not show.
    ///
    /// NAME, VALUE, AND WHERE IT WOULD BELONG. All three, because a gap with no value is
    /// a complaint and a gap with no place to go is a shrug. The value is what the run
    /// actually read, counted off the report it just built, so a gap says how much is
    /// being held back and not merely that something is.
    /// </summary>
    public sealed class ReportGap
    {
        public ReportGap(string name, string value, string wouldBelong)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A gap has to name what is missing.", "name");
            }

            Name = name;
            Value = value;
            WouldBelong = wouldBelong;
        }

        /// <summary>What was measured, in the words the code calls it.</summary>
        public string Name { get; private set; }

        /// <summary>What it came to on this group, measured and never estimated.</summary>
        public string Value { get; private set; }

        /// <summary>Where it would go if anyone wanted it.</summary>
        public string WouldBelong { get; private set; }

        /// <summary>
        /// How many item cells carried it, kept as a number so the run total can be
        /// added up without reading the words back.
        /// </summary>
        public int Carried { get; set; }

        /// <summary>How many item cells there were in all.</summary>
        public int OutOf { get; set; }

        public string Line()
        {
            return Name.PadRight(NameWidth) + "  " + Words.Or(Value, "UNKNOWN")
                + "  " + Words.Or(WouldBelong, "UNKNOWN where it would belong");
        }

        /// <summary>The name column, wide enough for the longest thing that goes in it.</summary>
        public const int NameWidth = 18;

        /// <summary>
        /// The value a counted gap carries, with the thousands separators the rest of the
        /// log uses so a big number reads at a glance.
        /// </summary>
        public static string Counted(int carried, int outOf)
        {
            return carried.ToString("#,##0", CultureInfo.InvariantCulture)
                + " of " + outOf.ToString("#,##0", CultureInfo.InvariantCulture)
                + (outOf == 1 ? " item cell carries it" : " item cells carry it");
        }
    }
}
