using System;

namespace Federator.Core.Clash
{
    /// <summary>
    /// What shapes the mirrored tests, F132, a setting and never a constant. Bader's answer D
    /// to Q133: in Clash Detective the mirror test stays, its name ending with (mirror).
    /// </summary>
    public sealed class MirrorSettings
    {
        /// <summary>His word for the ending.</summary>
        public const string DefaultEnding = "(mirror)";

        private string ending;

        public MirrorSettings()
        {
            ending = DefaultEnding;
        }

        /// <summary>
        /// The ending a mirror this tool creates carries after its name and one space. A
        /// blank ending, or one that starts or ends with a space, is refused where it is set,
        /// because a mirror named as the XML names it could not be told from the test a
        /// person made, and a space at either end cannot be read off a name.
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
        /// The name a mirror this tool creates carries: the XML's name, one space and the
        /// ending. A name that already ends with the space and the ending is given back as it
        /// is, so a rerun finds the mirror it created and never adds a second ending.
        /// </summary>
        internal string NameOf(string testName)
        {
            string name = testName ?? string.Empty;
            string withSpace = " " + ending;

            return name.EndsWith(withSpace, StringComparison.Ordinal) ? name : name + withSpace;
        }
    }
}
