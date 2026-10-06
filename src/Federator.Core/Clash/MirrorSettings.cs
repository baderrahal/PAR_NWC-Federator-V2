using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Clash
{
    /// <summary>
    /// What shapes the mirrored tests, F132, a setting and never a constant. Bader's answer D
    /// to Q133: in Clash Detective the mirror test stays, its name ending with (mirror).
    ///
    /// A MIRROR IS NAMED BY ITS OWN NAME, his answers D to Q133 and A to Q136: the mirror's
    /// own name, one space and the ending, Y (mirror) for the mirror Y of the kept test X,
    /// never X (mirror). NameFor is the one place a mirror's name is made, for a mirror an
    /// XML run creates and for a test saved before the rule that is renamed as one. The name
    /// is never read back to a test: a run with no XML pairs a saved test with the ending by
    /// its sides, MirrorRule.
    /// </summary>
    public sealed class MirrorSettings
    {
        /// <summary>His word for the ending.</summary>
        public const string DefaultEnding = "(mirror)";

        /// <summary>The number a mirror carries before the ending where its own name with the ending is taken.</summary>
        private const int FirstNumber = 2;

        private string ending;

        public MirrorSettings()
        {
            ending = DefaultEnding;
        }

        /// <summary>
        /// The ending a mirror this tool creates carries after its name and one space. A
        /// blank ending, or one that starts or ends with a space, is refused where it is set,
        /// because a mirror with no ending could not be told by its name from any other test,
        /// and a space at either end cannot be read off a name.
        /// </summary>
        public string Ending
        {
            get
            {
                return ending;
            }

            set
            {
                if (string.IsNullOrEmpty(value) || value.Trim().Length != value.Length)
                {
                    throw new ArgumentException(
                        "The mirror's ending needs at least one character and no space at either end.", "value");
                }

                ending = value;
            }
        }

        /// <summary>
        /// The name a mirror carries in Clash Detective: its own name, one space and the
        /// ending. A name that already ends with the space and the ending is given back as it
        /// is, so a mirror never carries the ending twice. Where the name with the ending is
        /// taken by another test of the XML, or by an earlier mirror of the same own name, the
        /// next number from 2 goes before the ending, Y 2 (mirror), so every mirror's name
        /// ends with the ending and no two tests share one name. Ordinal, as every test name
        /// is compared.
        /// </summary>
        internal string NameFor(string mirrorName, ICollection<string> taken)
        {
            string own = mirrorName ?? string.Empty;

            if (CarriesTheEnding(own))
            {
                return own;
            }

            string name = WithTheEnding(own);

            for (int number = FirstNumber; taken != null && taken.Contains(name); number++)
            {
                name = WithTheEnding(own + " " + number.ToString(CultureInfo.InvariantCulture));
            }

            return name;
        }

        /// <summary>Whether a name ends with one space and the ending, Ordinal and never trimmed.</summary>
        internal bool CarriesTheEnding(string name)
        {
            return name != null && name.Length > Suffix.Length
                && name.EndsWith(Suffix, StringComparison.Ordinal);
        }

        private string Suffix
        {
            get { return " " + ending; }
        }

        private string WithTheEnding(string name)
        {
            return name + Suffix;
        }
    }
}
