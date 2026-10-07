using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Federator.Core.Views
{
    /// <summary>
    /// Where one model item sits in the document, the index at each level from the root down,
    /// as the add-in's SavedViewpoints.PathOf reads it, 5o. Two paths are the same item when
    /// every index is the same, so a clashing item met in two clashes is painted once.
    /// </summary>
    public sealed class ItemPath : IEquatable<ItemPath>
    {
        private readonly int[] parts;

        public ItemPath(IEnumerable<int> indexes)
        {
            if (indexes == null)
            {
                throw new ArgumentNullException("indexes");
            }

            parts = new List<int>(indexes).ToArray();
        }

        /// <summary>The index at each level, the root's first.</summary>
        public ReadOnlyCollection<int> Parts
        {
            get { return new ReadOnlyCollection<int>(parts); }
        }

        public bool Equals(ItemPath other)
        {
            if (other == null || other.parts.Length != parts.Length)
            {
                return false;
            }

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] != other.parts[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ItemPath);
        }

        public override int GetHashCode()
        {
            int hash = 17;

            foreach (int part in parts)
            {
                hash = unchecked((hash * 31) + part);
            }

            return hash;
        }

        /// <summary>The indexes joined by a slash, the way a log line names the item.</summary>
        public override string ToString()
        {
            List<string> words = new List<string>();

            foreach (int part in parts)
            {
                words.Add(part.ToString(CultureInfo.InvariantCulture));
            }

            return string.Join("/", words.ToArray());
        }
    }
}
