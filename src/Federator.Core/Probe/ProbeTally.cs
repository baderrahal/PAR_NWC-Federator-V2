using System;
using System.Collections.Generic;

namespace Federator.Core.Probe
{
    /// <summary>
    /// What the probe found, F86. The add-in walks the items and calls Add for every
    /// property of every element in one of the named categories, and this counts, caps and
    /// sorts. No Navisworks type reaches it, so the counting, the cap and the order are
    /// all proved without Navisworks.
    /// </summary>
    public sealed class ProbeTally
    {
        /// <summary>
        /// The words that begin the value cell of the line saying values were left out.
        /// A sentence rather than a code, because the CSV goes to a person.
        /// </summary>
        public const string CappedMarker = "MORE VALUES NOT LISTED";

        private readonly int cap;
        private readonly Dictionary<string, Dictionary<string, int>> counts;
        private readonly Dictionary<string, string[]> parts;
        private readonly Dictionary<string, int> elementsPerCategory;

        public ProbeTally()
            : this(ProbeSettings.DefaultDistinctValueCap)
        {
        }

        public ProbeTally(int distinctValueCap)
        {
            if (distinctValueCap < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "distinctValueCap",
                    "A cap below one would list nothing at all, which is not a probe.");
            }

            cap = distinctValueCap;
            counts = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
            parts = new Dictionary<string, string[]>(StringComparer.Ordinal);
            elementsPerCategory = new Dictionary<string, int>(StringComparer.Ordinal);
        }

        public int DistinctValueCap
        {
            get { return cap; }
        }

        /// <summary>
        /// One property of one element. Everything is compared Ordinal and nothing is
        /// trimmed, because a value with a space on the end is a real thing in this
        /// project's files and the probe exists to find out what is really there.
        /// </summary>
        public void Add(string category, string tab, string property, string value)
        {
            string key = Key(category, tab, property);

            if (!counts.ContainsKey(key))
            {
                counts[key] = new Dictionary<string, int>(StringComparer.Ordinal);
                parts[key] = new[] { Said(category), Said(tab), Said(property) };
            }

            Dictionary<string, int> forThisProperty = counts[key];
            string said = Said(value);

            forThisProperty[said] = forThisProperty.ContainsKey(said) ? forThisProperty[said] + 1 : 1;
        }

        /// <summary>
        /// How many elements of that category were walked. Counted on its own rather than
        /// added up out of the property counts, because one element carries many
        /// properties and adding those would count it many times.
        /// </summary>
        public void AddElement(string category)
        {
            string said = Said(category);
            elementsPerCategory[said] = elementsPerCategory.ContainsKey(said)
                ? elementsPerCategory[said] + 1
                : 1;
        }

        public int ElementsIn(string category)
        {
            string said = Said(category);
            return elementsPerCategory.ContainsKey(said) ? elementsPerCategory[said] : 0;
        }

        /// <summary>Every category that was walked and found something, in the order read.</summary>
        public IList<string> CategoriesFound()
        {
            List<string> found = new List<string>(elementsPerCategory.Keys);
            found.Sort(StringComparer.Ordinal);
            return found;
        }

        /// <summary>How many distinct properties, over every category.</summary>
        public int PropertyCount
        {
            get { return counts.Count; }
        }

        /// <summary>How many distinct values were seen in all, before any cap.</summary>
        public int DistinctValueCount
        {
            get
            {
                int total = 0;

                foreach (Dictionary<string, int> forThisProperty in counts.Values)
                {
                    total += forThisProperty.Count;
                }

                return total;
            }
        }

        /// <summary>
        /// The rows, sorted by category, then by property tab, then by property name, then
        /// by how many elements carry the value, highest first. Where two values are
        /// carried by the same number of elements they are ordered by the value itself, so
        /// the same model gives the same file twice running.
        ///
        /// A property with more distinct values than the cap keeps the commonest and ends
        /// with ONE line saying how many were left out and how many elements they covered,
        /// so nothing is hidden and nothing is guessed at.
        /// </summary>
        public IList<ProbeRow> Rows()
        {
            List<string> keys = new List<string>(counts.Keys);

            keys.Sort(delegate (string a, string b)
            {
                string[] left = parts[a];
                string[] right = parts[b];

                for (int i = 0; i < 3; i++)
                {
                    int order = string.Compare(left[i], right[i], StringComparison.Ordinal);

                    if (order != 0)
                    {
                        return order;
                    }
                }

                return 0;
            });

            List<ProbeRow> rows = new List<ProbeRow>();

            foreach (string key in keys)
            {
                string[] part = parts[key];
                List<KeyValuePair<string, int>> values =
                    new List<KeyValuePair<string, int>>(counts[key]);

                values.Sort(delegate (KeyValuePair<string, int> a, KeyValuePair<string, int> b)
                {
                    int order = b.Value.CompareTo(a.Value);
                    return order != 0 ? order : string.Compare(a.Key, b.Key, StringComparison.Ordinal);
                });

                int shown = 0;
                int leftOut = 0;
                int elementsLeftOut = 0;

                foreach (KeyValuePair<string, int> value in values)
                {
                    if (shown < cap)
                    {
                        rows.Add(new ProbeRow(part[0], part[1], part[2], value.Key, value.Value));
                        shown++;
                    }
                    else
                    {
                        leftOut++;
                        elementsLeftOut += value.Value;
                    }
                }

                if (leftOut > 0)
                {
                    rows.Add(new ProbeRow(
                        part[0],
                        part[1],
                        part[2],
                        CappedMarker + ", " + leftOut + " more distinct "
                            + (leftOut == 1 ? "value" : "values") + " here, the cap is " + cap,
                        elementsLeftOut));
                }
            }

            return rows;
        }

        /// <summary>The properties that hit the cap, named so the verdict block can say so.</summary>
        public IList<string> Capped()
        {
            List<string> capped = new List<string>();

            foreach (ProbeRow row in Rows())
            {
                if (row.IsTheCapLine)
                {
                    capped.Add(row.Category + " / " + row.Tab + " / " + row.Property);
                }
            }

            return capped;
        }

        private static string Key(string category, string tab, string property)
        {
            // A separator no property tab or name in any model carries, so two different
            // properties can never collide into one key.
            return Said(category) + "\u0001" + Said(tab) + "\u0001" + Said(property);
        }

        private static string Said(string value)
        {
            return value == null ? string.Empty : value;
        }
    }
}
