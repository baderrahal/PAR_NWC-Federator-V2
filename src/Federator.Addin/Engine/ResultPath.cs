using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Views;

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
        /// caller disposes, or null with whyNot saying what was found instead. The last
        /// level is decided by Federator.Core.Views.ResultSiblings: the result at the
        /// recorded index where it carries the row's name, else the one sibling carrying
        /// that name, since TestsCompactAllTests after the merge removes every Resolved
        /// result and moves the live ones after it one index left, and movedFromRecorded
        /// says so. Every wrapper walked past, and every sibling read for its name, is
        /// disposed here.
        /// </summary>
        public static SavedItem ResultAt(
            ClashTest test, RowUnderTest recorded, string under, out string whyNot, out bool movedFromRecorded)
        {
            whyNot = null;
            movedFromRecorded = false;

            if (test == null || recorded == null)
            {
                whyNot = "no test or no recorded row was handed in";
                return null;
            }

            SavedItemCollection children = test.Children;
            SavedItem walked = null;

            try
            {
                int last = recorded.Path.Count - 1;

                for (int level = 0; level < last; level++)
                {
                    int index = recorded.Path[level];

                    if (children == null || index < 0 || index >= children.Count)
                    {
                        whyNot = "no result group is at " + recorded + " under " + under + " any more";
                        return null;
                    }

                    SavedItem next = children[index];

                    if (walked != null)
                    {
                        walked.Dispose();
                    }

                    walked = next;
                    ClashResultGroup group = walked as ClashResultGroup;

                    if (group == null)
                    {
                        whyNot = recorded + " under " + under + " does not lead through a result group any more";
                        return null;
                    }

                    children = group.Children;
                }

                SavedItemCollection siblings = children;
                int count = siblings == null ? 0 : siblings.Count;
                SiblingPick pick = ResultSiblings.Pick(
                    recorded.Row.Name, recorded.Path[last], count, i => NameOf(siblings, i));

                if (!pick.Found)
                {
                    whyNot = pick.Carrying == 0
                        ? "no result named \"" + recorded.Row.Name + "\" is among the " + count + " at the level of "
                            + recorded + " under " + under + " any more"
                        : pick.Carrying + " results at the level of " + recorded + " under " + under + " are named \""
                            + recorded.Row.Name + "\", so which is the row's is UNKNOWN";
                    return null;
                }

                SavedItem item = siblings[pick.Index];

                if (!(item is IClashResult))
                {
                    item.Dispose();
                    whyNot = "what is named \"" + recorded.Row.Name + "\" at the level of " + recorded + " under " + under
                        + " is not a clash result";
                    return null;
                }

                movedFromRecorded = !pick.AtRecorded;
                return item;
            }
            finally
            {
                if (walked != null)
                {
                    walked.Dispose();
                }
            }
        }

        /// <summary>One sibling's display name, its wrapper released as soon as it is read.</summary>
        private static string NameOf(SavedItemCollection siblings, int index)
        {
            using (SavedItem sibling = siblings[index])
            {
                return sibling.DisplayName ?? string.Empty;
            }
        }
    }
}
