using System;
using System.Collections.Generic;

namespace Federator.Core.Views
{
    /// <summary>The two disciplines a clash is between, and the folder they share.</summary>
    public sealed class DisciplinePair
    {
        internal DisciplinePair(string first, string second, string folder, bool bothKnown)
        {
            First = first;
            Second = second;
            Folder = folder;
            BothKnown = bothKnown;
        }

        /// <summary>The first code, sorted.</summary>
        public string First { get; private set; }

        /// <summary>The second code, sorted.</summary>
        public string Second { get; private set; }

        /// <summary>What the folder is called.</summary>
        public string Folder { get; private set; }

        /// <summary>
        /// Whether both sides carried a code this tool knows. False is a real answer and
        /// is COUNTED rather than guessed at: the client's own file holds a set name that
        /// breaks the pattern its siblings follow.
        /// </summary>
        public bool BothKnown { get; private set; }

        public override string ToString()
        {
            return Folder;
        }
    }

    /// <summary>
    /// Which two disciplines a clash is between, F85 layer 2.
    ///
    /// A SET NAME CARRIES ITS DISCIPLINE. BLD-ME-Ducts is Mechanical, BLD-AR-Walls is
    /// Architecture. The code is whichever hyphen separated part of the name is exactly
    /// one of the codes this tool knows, so a name with a different prefix still reads and
    /// a name that carries no code at all reads as UNKNOWN rather than as whatever sat in
    /// the second position.
    ///
    /// THE TWO ARE SORTED, so AR against ST and ST against AR are ONE folder. Without
    /// that, half the clashes of every pair go in one folder and half in another, and a
    /// person looking for the architecture against structure work finds it in two places.
    ///
    /// A NAME WITH NO KNOWN CODE IS REPORTED AND NEVER GUESSED. The client's own matrix
    /// holds BLD-Security Devices, which breaks the BLD-EL- pattern its siblings follow,
    /// so a set with no code in the usual place is a thing that really happens. The clash
    /// still gets its viewpoint, the folder says UNKNOWN, and the count of them goes in
    /// the block. The tool reports what it noticed and Bader decides.
    /// </summary>
    public static class DisciplinePairRule
    {
        /// <summary>
        /// The discipline code in that set name, or empty where it carries none this tool
        /// knows. Matched Ordinal and never trimmed or cased, because a code is read off a
        /// name and every other comparison here treats it exactly as it was read.
        /// </summary>
        public static string CodeIn(string setName, ViewpointSettings settings)
        {
            if (string.IsNullOrEmpty(setName) || settings == null)
            {
                return string.Empty;
            }

            string[] parts = setName.Split(settings.SetNameSeparator);

            for (int i = 0; i < parts.Length; i++)
            {
                if (settings.IsADisciplineCode(parts[i]))
                {
                    return parts[i];
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// The pair those two set names make. The codes are sorted Ordinal so the pair is
        /// one folder whichever way round the test named them.
        /// </summary>
        public static DisciplinePair For(string leftSet, string rightSet, ViewpointSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            string left = CodeIn(leftSet, settings);
            string right = CodeIn(rightSet, settings);
            bool bothKnown = left.Length > 0 && right.Length > 0;

            string first = left.Length > 0 ? left : settings.UnknownDiscipline;
            string second = right.Length > 0 ? right : settings.UnknownDiscipline;

            if (string.Compare(first, second, StringComparison.Ordinal) > 0)
            {
                string swap = first;
                first = second;
                second = swap;
            }

            return new DisciplinePair(
                first, second, first + settings.PairSeparator + second, bothKnown);
        }

        /// <summary>
        /// Whether this pair carries a size sub folder, F85 layer 3. Only where one of the
        /// two is a discipline the settings name, which defaults to Mechanical and
        /// Electrical, because that is where the pipes, ducts, cable trays and conduits
        /// are. It reads ViewpointSettings.HasSubGroup, which F53 already asks the same
        /// question through, so the two cannot come to different answers.
        /// </summary>
        public static bool CarriesASizeFolder(DisciplinePair pair, ViewpointSettings settings)
        {
            if (pair == null || settings == null)
            {
                return false;
            }

            return settings.HasSubGroup(pair.First) || settings.HasSubGroup(pair.Second);
        }
    }
}
