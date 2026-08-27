using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Naming
{
    /// <summary>
    /// The result of reading one container name. Either it is readable and carries a
    /// building and a discipline, or it is unreadable and carries the reason why.
    /// Nothing is guessed at.
    /// </summary>
    public sealed class ParsedContainerName
    {
        private ParsedContainerName(
            string sourceName,
            string stem,
            IList<string> parts,
            bool isReadable,
            string building,
            string discipline,
            string unreadableReason)
        {
            SourceName = sourceName;
            Stem = stem;
            Parts = new ReadOnlyCollection<string>(parts ?? new List<string>());
            IsReadable = isReadable;
            Building = building;
            Discipline = discipline;
            UnreadableReason = unreadableReason;
        }

        /// <summary>The name exactly as it was handed in, folder and extension included.</summary>
        public string SourceName { get; private set; }

        /// <summary>The file name with any folder and extension removed.</summary>
        public string Stem { get; private set; }

        public ReadOnlyCollection<string> Parts { get; private set; }

        public bool IsReadable { get; private set; }

        public string Building { get; private set; }

        public string Discipline { get; private set; }

        /// <summary>Null when <see cref="IsReadable"/> is true.</summary>
        public string UnreadableReason { get; private set; }

        internal static ParsedContainerName Readable(
            string sourceName, string stem, IList<string> parts, string building, string discipline)
        {
            return new ParsedContainerName(sourceName, stem, parts, true, building, discipline, null);
        }

        internal static ParsedContainerName Unreadable(
            string sourceName, string stem, IList<string> parts, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw new ArgumentException("An unreadable name needs a reason.", "reason");
            }

            return new ParsedContainerName(sourceName, stem, parts, false, null, null, reason);
        }

        public override string ToString()
        {
            return IsReadable
                ? Stem + " [building " + Building + ", discipline " + Discipline + "]"
                : Stem + " [unreadable: " + UnreadableReason + "]";
        }
    }
}
