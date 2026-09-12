using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Clash;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Reads the clash tests already saved in the open document, with no Navisworks type
    /// left on what it hands back, so Core can plan a run from them. This is what runs
    /// when no XML is picked: the tests the NWF already holds, where they sit.
    ///
    /// Walked once for the whole group, never once per test. Every wrapper is disposed
    /// on the way out, because a SavedItem is an eEXTERNAL handle onto something the
    /// document owns. Only the address is kept, which is how the test is found again
    /// after any mutation.
    ///
    /// A ClashTest is itself a GroupItem holding its results, so it is tested for before
    /// a folder is, or the walk would descend into the results.
    /// </summary>
    public static class SavedTests
    {
        /// <summary>What a side reads as when it came out of the document rather than a file.</summary>
        public const string SideA = "side A as saved";

        public const string SideB = "side B as saved";

        public static IList<SavedClashTest> Read(Document document)
        {
            List<SavedClashTest> saved = new List<SavedClashTest>();

            if (document == null)
            {
                return saved;
            }

            DocumentClashTests clashTests = document.GetClash().TestsData;
            Walk(clashTests.Tests, new List<int>(), saved);
            return saved;
        }

        public static int Count(Document document)
        {
            return Read(document).Count;
        }

        private static void Walk(SavedItemCollection items, List<int> path, List<SavedClashTest> saved)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    path.Add(i);

                    ClashTest test = item as ClashTest;

                    if (test != null)
                    {
                        // Each read of SelectionA or SelectionB creates a wrapper, so they
                        // are read once each and the four values taken from inside rather
                        // than four wrappers a test being left for a finalizer.
                        using (ClashSelection left = test.SelectionA)
                        using (ClashSelection right = test.SelectionB)
                        {
                            saved.Add(new SavedClashTest(
                                test.DisplayName,
                                (int)test.TestType,
                                test.Tolerance,
                                test.MergeComposites,
                                left.SelfIntersect,
                                (int)left.PrimitiveTypes,
                                SideA,
                                right.SelfIntersect,
                                (int)right.PrimitiveTypes,
                                SideB,
                                path));
                        }
                    }
                    else
                    {
                        GroupItem folder = item as GroupItem;

                        if (folder != null)
                        {
                            Walk(folder.Children, path, saved);
                        }
                    }

                    path.RemoveAt(path.Count - 1);
                }
            }
        }
    }
}
