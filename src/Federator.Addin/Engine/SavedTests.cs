using System;
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
    ///
    /// THE SIDES, F132's add-in half. A census or a rebuild count reads the tests alone and
    /// hands the placeholders SavedClashTest.LeftAsSaved and RightAsSaved for the sides,
    /// Read(Document). The mirror rule pairs saved tests by their sides and never by a
    /// name, so the clash runner hands a reader of which set a side points at, built over
    /// the set index it holds, Read(Document, Func), and each side is then the set's path
    /// as a test locator names it, or UNKNOWN where it was not read, which Core never
    /// pairs. The reader is called inside the one read of each ClashSelection, so a side
    /// costs the one wrapper it already cost.
    /// </summary>
    public static class SavedTests
    {
        public static IList<SavedClashTest> Read(Document document)
        {
            return Read(document, null);
        }

        /// <summary>
        /// Every saved test with each side as the set it points at, read by locatorOf, or
        /// with the placeholders where locatorOf is null.
        /// </summary>
        public static IList<SavedClashTest> Read(Document document, Func<ClashSelection, string> locatorOf)
        {
            List<SavedClashTest> saved = new List<SavedClashTest>();

            if (document == null)
            {
                return saved;
            }

            DocumentClashTests clashTests = document.GetClash().TestsData;
            Walk(clashTests.Tests, new List<int>(), saved, locatorOf);
            return saved;
        }

        public static int Count(Document document)
        {
            return Read(document).Count;
        }

        private static void Walk(
            SavedItemCollection items,
            List<int> path,
            List<SavedClashTest> saved,
            Func<ClashSelection, string> locatorOf)
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
                                locatorOf == null ? SavedClashTest.LeftAsSaved : locatorOf(left),
                                right.SelfIntersect,
                                (int)right.PrimitiveTypes,
                                locatorOf == null ? SavedClashTest.RightAsSaved : locatorOf(right),
                                path));
                        }
                    }
                    else
                    {
                        GroupItem folder = item as GroupItem;

                        if (folder != null)
                        {
                            Walk(folder.Children, path, saved, locatorOf);
                        }
                    }

                    path.RemoveAt(path.Count - 1);
                }
            }
        }
    }
}
