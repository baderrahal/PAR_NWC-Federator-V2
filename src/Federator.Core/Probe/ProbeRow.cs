using System;

namespace Federator.Core.Probe
{
    /// <summary>
    /// One line of the probe's CSV, F86: a category, a property tab, a property name, one
    /// distinct value of that property, and how many elements carry it.
    ///
    /// The value is kept EXACTLY as it was read, spaces and all, because the whole point
    /// is to find out what the models carry and a trimmed value hides a value with a space
    /// on the end, which is a real thing in this project's files.
    /// </summary>
    public sealed class ProbeRow
    {
        public ProbeRow(string category, string tab, string property, string value, int elements)
        {
            Category = category ?? string.Empty;
            Tab = tab ?? string.Empty;
            Property = property ?? string.Empty;
            Value = value ?? string.Empty;
            Elements = elements;
        }

        public string Category { get; private set; }

        /// <summary>The property tab, which is what a person sees in the properties panel.</summary>
        public string Tab { get; private set; }

        public string Property { get; private set; }

        /// <summary>One distinct value, exactly as it was read.</summary>
        public string Value { get; private set; }

        /// <summary>How many elements carry that value.</summary>
        public int Elements { get; private set; }

        /// <summary>Whether this row is the one that says values were left out.</summary>
        public bool IsTheCapLine
        {
            get { return Value.StartsWith(ProbeTally.CappedMarker, StringComparison.Ordinal); }
        }

        public override string ToString()
        {
            return Category + " / " + Tab + " / " + Property + " = " + Value + " (" + Elements + ")";
        }
    }
}
