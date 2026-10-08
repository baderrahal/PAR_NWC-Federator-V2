using System;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The one walk from a test to the result a recorded row points at, by the index path
    /// the harvest recorded, RowUnderTest, F132 attempt 2 of the add-in half. The pictures
    /// rendered after the mirror merge and the views both resolve a row this way, so the
    /// rule of what counts as found again lives here once.
    ///
    /// THE NAME IS READ BACK. TestsCompactAllTests removes every Resolved result after the
    /// merge and before the views, so a path recorded while the test was read can lead to
    /// another result by then. What is found is the row's result only where its display
    /// name is the row's name, and anything else is refused and said rather than viewed or
    /// pictured as the wrong clash.
    /// </summary>
    internal static class ResultPath
    {
        /// <summary>
        /// The item at the recorded path under the test, always an IClashResult, which the
        /// caller disposes, or null with whyNot saying what was found instead. Every wrapper
        /// walked past is disposed here.
        /// </summary>
        public static SavedItem ResultAt(ClashTest test, RowUnderTest recorded, string under, out string whyNot)
        {
            whyNot = null;

            if (test == null || recorded == null)
            {
                whyNot = "no test or no recorded row was handed in";
                return null;
            }

            SavedItemCollection children = test.Children;
            SavedItem item = null;

            try
            {
                for (int level = 0; level < recorded.Path.Count; level++)
                {
                    int index = recorded.Path[level];

                    if (children == null || index < 0 || index >= children.Count)
                    {
                        whyNot = "no result is at " + recorded + " under " + under + " any more";
                        return null;
                    }

                    SavedItem next = children[index];

                    if (item != null)
                    {
                        item.Dispose();
                    }

                    item = next;

                    if (level + 1 == recorded.Path.Count)
                    {
                        break;
                    }

                    ClashResultGroup group = item as ClashResultGroup;

                    if (group == null)
                    {
                        whyNot = recorded + " under " + under + " does not lead through a result group any more";
                        return null;
                    }

                    children = group.Children;
                }

                if (!(item is IClashResult))
                {
                    whyNot = "what is at " + recorded + " under " + under + " is not a clash result";
                    return null;
                }

                string found = item.DisplayName ?? string.Empty;

                if (!string.Equals(found, recorded.Row.Name, StringComparison.Ordinal))
                {
                    whyNot = "what is at " + recorded + " under " + under + " is named \"" + found + "\" and not \""
                        + recorded.Row.Name + "\"";
                    return null;
                }

                SavedItem result = item;
                item = null;
                return result;
            }
            finally
            {
                if (item != null)
                {
                    item.Dispose();
                }
            }
        }
    }
}
