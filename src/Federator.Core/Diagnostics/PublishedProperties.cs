using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// Every publish property one run set on an NWD, in the order it set them, as one log
    /// line.
    ///
    /// WHY. An NWD that will not open in ACC is diagnosed off the log and nothing else,
    /// because the file is by then on someone else's server and the run that wrote it is
    /// over. Reading the code of whatever build produced it answers a different question,
    /// which is what the code says now. So the line records what this run actually set,
    /// value by value, and a property nothing set is not on it.
    ///
    /// The list is what was SET and never what the type offers. A name here that the add-in
    /// stopped setting would read as still set, which is the one way this line can lie.
    /// </summary>
    public sealed class PublishedProperties
    {
        private readonly List<string> names = new List<string>();
        private readonly List<string> values = new List<string>();

        /// <summary>How many properties were set.</summary>
        public int Count
        {
            get { return names.Count; }
        }

        /// <summary>
        /// Records one property. The value is written as it will read in the log, so a
        /// caller never formats a value twice in two places.
        /// </summary>
        public void Set(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A publish property needs a name.", "name");
            }

            names.Add(name);
            values.Add(value == null ? string.Empty : value);
        }

        /// <summary>
        /// Records one boolean property as true or false in lower case, which is how the
        /// API names them and how anyone comparing the log against Autodesk's own wording
        /// will read them.
        /// </summary>
        public void Set(string name, bool value)
        {
            Set(name, value ? "true" : "false");
        }

        /// <summary>Records one number, invariant, so a machine in another locale writes the same line.</summary>
        public void Set(string name, int value)
        {
            Set(name, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// The line the log carries. Nothing set says so in words rather than trailing off
        /// after the word set, because an empty list is the interesting answer here: it
        /// means the publish went out with the defaults and the ACC warning is explained.
        /// </summary>
        public string Line()
        {
            if (names.Count == 0)
            {
                return "NWD      publish properties set: none, the NWD went out with whatever the defaults are";
            }

            string[] pairs = new string[names.Count];

            for (int i = 0; i < names.Count; i++)
            {
                pairs[i] = names[i] + "=" + values[i];
            }

            return "NWD      publish properties set: " + string.Join(", ", pairs);
        }

        /// <summary>
        /// Whether a property of that name was set, so a check can ask the question without
        /// reading the line back as text.
        /// </summary>
        public bool WasSet(string name)
        {
            return names.Contains(name);
        }

        /// <summary>
        /// What was recorded against that name, or null where nothing was. Null and the
        /// empty string are different answers: the second means it was set to nothing.
        /// </summary>
        public string ValueOf(string name)
        {
            int at = names.IndexOf(name);
            return at < 0 ? null : values[at];
        }
    }
}
