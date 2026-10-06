using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Clash
{
    /// <summary>
    /// What shapes the mirrored tests, F132, a setting and never a constant. Bader's answer D
    /// to Q133: in Clash Detective the mirror test stays, its name ending with (mirror).
    ///
    /// A MIRROR IS NAMED AFTER THE TEST KEPT, F132 attempt 5: the kept test's name, one space
    /// and the ending, X (mirror). An NWF holding X and X (mirror) says by its names which
    /// test the mirror's clashes go to and which one is kept. Attempt 4 named the mirror after
    /// its own XML name, which no run with no XML can read back to the test it mirrors.
    ///
    /// THE NAME ALONE IS NOT ENOUGH, F132 attempt 6. A test the XML itself names X (mirror)
    /// may ask another question than X, and a person's test X 3 sits beside the third mirror
    /// of X, X 3 (mirror). So KeptNamesOf gives every test a name could have been made for,
    /// and MirrorRule pairs a saved test with one of them only where its sides ask that
    /// test's question, the rule MirrorRule.Of pairs the XML's tests by.
    /// </summary>
    public sealed class MirrorSettings
    {
        /// <summary>His word for the ending.</summary>
        public const string DefaultEnding = "(mirror)";

        /// <summary>The number the second mirror of one test kept carries before the ending.</summary>
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
        /// The name a mirror this tool creates carries: the name of the test kept, one space
        /// and the ending. Where that name is taken, by another test of the XML or by an
        /// earlier mirror of the same test kept, Q121 B, the next number from 2 goes before
        /// the ending, X 2 (mirror), so every mirror's name ends with the ending and no two
        /// tests share one name. Ordinal, as every test name is compared.
        /// </summary>
        internal string NameFor(string keptName, ICollection<string> taken)
        {
            string name = WithTheEnding(keptName ?? string.Empty);

            for (int number = FirstNumber; taken != null && taken.Contains(name); number++)
            {
                name = WithTheEnding((keptName ?? string.Empty) + " " + number.ToString(CultureInfo.InvariantCulture));
            }

            return name;
        }

        /// <summary>Whether a name ends with one space and the ending, Ordinal and never trimmed.</summary>
        internal bool CarriesTheEnding(string name)
        {
            return name != null && name.Length > Suffix.Length
                && name.EndsWith(Suffix, StringComparison.Ordinal);
        }

        /// <summary>
        /// The names of the saved tests a saved name with the ending could have been made for
        /// by NameFor, the exact name before the ending first, then that name with a number of
        /// 2 or more taken off its end, each only where a saved test carries it. Empty where
        /// the name carries no ending or no saved test carries a name before it. A name read
        /// back is never a pair by itself: MirrorRule pairs the saved test with the first of
        /// them whose question its sides ask, F132 attempt 6.
        /// </summary>
        internal IList<string> KeptNamesOf(string savedName, ICollection<string> savedNames)
        {
            List<string> names = new List<string>();

            if (!CarriesTheEnding(savedName) || savedNames == null)
            {
                return names;
            }

            string before = savedName.Substring(0, savedName.Length - Suffix.Length);

            if (savedNames.Contains(before))
            {
                names.Add(before);
            }

            int space = before.LastIndexOf(' ');
            int number;

            if (space > 0
                && int.TryParse(before.Substring(space + 1), NumberStyles.None, CultureInfo.InvariantCulture, out number)
                && number >= FirstNumber
                && string.Equals(number.ToString(CultureInfo.InvariantCulture), before.Substring(space + 1), StringComparison.Ordinal)
                && savedNames.Contains(before.Substring(0, space)))
            {
                names.Add(before.Substring(0, space));
            }

            return names;
        }

        /// <summary>The name before the ending, for a name that carries it.</summary>
        internal string Before(string name)
        {
            return CarriesTheEnding(name) ? name.Substring(0, name.Length - Suffix.Length) : name;
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
