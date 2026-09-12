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
            string stem,
            IList<string> parts,
            bool isReadable,
            string project,
            string originator,
            string building,
            string discipline,
            string unreadableReason)
        {
            Stem = stem;
            Parts = new ReadOnlyCollection<string>(parts ?? new List<string>());
            IsReadable = isReadable;
            Project = project;
            Originator = originator;
            Building = building;
            Discipline = discipline;
            UnreadableReason = unreadableReason;
        }

        /// <summary>The file name with any folder and extension removed.</summary>
        public string Stem { get; private set; }

        internal ReadOnlyCollection<string> Parts { get; private set; }

        public bool IsReadable { get; private set; }

        /// <summary>The project code. Every file in a group has to agree on it.</summary>
        public string Project { get; private set; }

        /// <summary>The originator. Every file in a group has to agree on it.</summary>
        public string Originator { get; private set; }

        public string Building { get; private set; }

        public string Discipline { get; private set; }

        /// <summary>Null when <see cref="IsReadable"/> is true.</summary>
        public string UnreadableReason { get; private set; }

        internal static ParsedContainerName Readable(
            string stem,
            IList<string> parts,
            string project,
            string originator,
            string building,
            string discipline)
        {
            return new ParsedContainerName(
                stem, parts, true, project, originator, building, discipline, null);
        }

        internal static ParsedContainerName Unreadable(
            string stem, IList<string> parts, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw new ArgumentException("An unreadable name needs a reason.", "reason");
            }

            return new ParsedContainerName(stem, parts, false, null, null, null, null, reason);
        }

        public override string ToString()
        {
            return IsReadable
                ? Stem + " [building " + Building + ", discipline " + Discipline + "]"
                : Stem + " [unreadable: " + UnreadableReason + "]";
        }
    }
}
