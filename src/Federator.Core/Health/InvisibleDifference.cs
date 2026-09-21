using System;
using System.Globalization;

namespace Federator.Core.Health
{
    /// <summary>
    /// TWO STRINGS THAT LOOK IDENTICAL AND ARE NOT, NAMED BY THE CHARACTER THAT DIFFERS
    /// AND WHERE IT SITS, 4e.
    ///
    /// WHY IT EXISTS. C04 carries `EL-Fire alarm` in two spellings, and one of them holds
    /// a NON-BREAKING SPACE where the other holds an ordinary one. Today no single group
    /// carries both models, so nothing compares them and nothing is reported. That stops
    /// being true the moment one group gets both, or somebody types the matrix value with
    /// an ordinary space. Then a set finds nothing, and every line on screen shows two
    /// strings a person would swear are the same.
    ///
    /// SAYING "TWO SPELLINGS" IS USELESS HERE. A person reading that looks at the two,
    /// sees no difference, and concludes the tool is wrong. Saying NON-BREAKING SPACE,
    /// U+00A0, AT CHARACTER 8 is something they can act on in thirty seconds.
    ///
    /// IT COVERS EVERY INVISIBLE DIFFERENCE AND NOT ONLY THAT ONE: a non-breaking space, a
    /// tab, a double space, a trailing or leading space, and the zero width characters
    /// that arrive when a name is pasted out of a web page.
    /// </summary>
    public static class InvisibleDifference
    {
        /// <summary>A no break space, which is the one C04 actually carries.</summary>
        public const char NoBreakSpace = ' ';

        /// <summary>A zero width space.</summary>
        public const char ZeroWidthSpace = '​';

        /// <summary>A zero width non joiner.</summary>
        public const char ZeroWidthNonJoiner = '‌';

        /// <summary>A zero width joiner.</summary>
        public const char ZeroWidthJoiner = '‍';

        /// <summary>A byte order mark arriving mid string as a zero width no break space.</summary>
        public const char ByteOrderMark = '﻿';

        /// <summary>
        /// What is invisibly different about those two, or null when the difference is one
        /// a person can see. Null and not an empty string, because "nothing invisible"
        /// and "I found an invisible thing with no name" are different answers.
        /// </summary>
        public static string Between(string left, string right)
        {
            if (left == null || right == null || string.Equals(left, right, StringComparison.Ordinal))
            {
                return null;
            }

            // THE COMMON CASE FIRST: the same length, and exactly one position differing,
            // where at least one side is invisible.
            if (left.Length == right.Length)
            {
                int at = -1;

                for (int i = 0; i < left.Length; i++)
                {
                    if (left[i] == right[i])
                    {
                        continue;
                    }

                    if (at >= 0)
                    {
                        at = -2;
                        break;
                    }

                    at = i;
                }

                if (at >= 0 && (IsInvisible(left[at]) || IsInvisible(right[at])))
                {
                    return Describe(left[at]) + " against " + Describe(right[at])
                        + " AT CHARACTER " + (at + 1);
                }
            }

            // THE LENGTHS DIFFER. Where the two are the same once every invisible
            // character is taken out, the difference is entirely invisible even though no
            // single position lines up, which is what a double space or a trailing space
            // does.
            if (string.Equals(WithoutInvisibles(left), WithoutInvisibles(right), StringComparison.Ordinal))
            {
                string extra = FirstInvisibleIn(left, right);

                return extra ?? "they differ only in invisible characters";
            }

            return null;
        }

        /// <summary>Whether that character is one a person cannot see in a name.</summary>
        public static bool IsInvisible(char value)
        {
            return value == NoBreakSpace
                || value == ZeroWidthSpace
                || value == ZeroWidthNonJoiner
                || value == ZeroWidthJoiner
                || value == ByteOrderMark
                || value == '\t';
        }

        /// <summary>
        /// The character named the way a person can act on: what it is called, its code
        /// point, and nothing that needs a lookup table to read.
        /// </summary>
        public static string Describe(char value)
        {
            string name;

            switch (value)
            {
                case NoBreakSpace: name = "NON-BREAKING SPACE"; break;
                case ZeroWidthSpace: name = "ZERO WIDTH SPACE"; break;
                case ZeroWidthNonJoiner: name = "ZERO WIDTH NON-JOINER"; break;
                case ZeroWidthJoiner: name = "ZERO WIDTH JOINER"; break;
                case ByteOrderMark: name = "ZERO WIDTH NO-BREAK SPACE"; break;
                case '\t': name = "TAB"; break;
                case ' ': name = "an ordinary space"; break;
                default: name = "\"" + value + "\""; break;
            }

            return name + " (U+" + ((int)value).ToString("X4", CultureInfo.InvariantCulture) + ")";
        }

        private static string FirstInvisibleIn(string left, string right)
        {
            for (int i = 0; i < left.Length; i++)
            {
                if (IsInvisible(left[i]))
                {
                    return Describe(left[i]) + " AT CHARACTER " + (i + 1) + " of the first";
                }
            }

            for (int i = 0; i < right.Length; i++)
            {
                if (IsInvisible(right[i]))
                {
                    return Describe(right[i]) + " AT CHARACTER " + (i + 1) + " of the second";
                }
            }

            return null;
        }

        private static string WithoutInvisibles(string value)
        {
            System.Text.StringBuilder kept = new System.Text.StringBuilder(value.Length);

            foreach (char one in value)
            {
                if (!IsInvisible(one) && one != ' ')
                {
                    kept.Append(one);
                }
            }

            return kept.ToString();
        }
    }
}
