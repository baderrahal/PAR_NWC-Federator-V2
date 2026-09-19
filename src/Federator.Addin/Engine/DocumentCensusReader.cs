using System;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Diagnostics;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The ONE place that reads the five counts out of the open document. F61.
    ///
    /// WHY ONE PLACE. Each of these is walked, not asked for, and a walk that leaves a
    /// wrapper behind leaves it for a finalizer on a thread Navisworks does not own. One
    /// run built 1.7 million handles in a group doing exactly that. Five walks written in
    /// five places would be five chances to get the disposal wrong, so they are written
    /// once, here, in the shape FederationEngine.CountSets was already using: a count and
    /// never a handle, every wrapper disposed on the way down, because a handle onto
    /// anything the document owns dies the moment the document replaces the object behind
    /// it, which a clear does.
    ///
    /// NOTHING HERE JUDGES. It counts and hands five numbers to Core, which owns every
    /// rule about what a move in them means.
    ///
    /// A COUNT THAT CANNOT BE TAKEN IS MINUS ONE AND NEVER ZERO. Zero reads as a real
    /// count and would let a run throw 1830 clash tests away and report that nothing
    /// moved. Every read here is in its own try for that reason: one count failing must
    /// not turn the other four into UNKNOWN as well.
    /// </summary>
    public static class DocumentCensusReader
    {
        /// <summary>
        /// The five counts, as of right now. Never throws, because a census is a
        /// diagnostic and a diagnostic never stops a run.
        /// </summary>
        public static DocumentCensus Read(Document document)
        {
            if (document == null)
            {
                return DocumentCensus.Unknown();
            }

            return new DocumentCensus(
                Models(document),
                Sets(document),
                Tests(document),
                Results(document),
                SavedViewpoints.Count(document));
        }

        /// <summary>How many models the document holds, or minus one.</summary>
        public static int Models(Document document)
        {
            try
            {
                return document == null ? DocumentCensus.NotCounted : document.Models.Count;
            }
            catch (Exception)
            {
                return DocumentCensus.NotCounted;
            }
        }

        /// <summary>
        /// How many selection sets the tree holds, walked from the root with every wrapper
        /// disposed on the way, or minus one.
        ///
        /// This is the walk FederationEngine used to carry as CountSets. It moved here so
        /// the rebuild and the census read the same number the same way, rather than two
        /// copies of one walk drifting apart.
        /// </summary>
        public static int Sets(Document document)
        {
            if (document == null)
            {
                return DocumentCensus.NotCounted;
            }

            try
            {
                return Sets(document.SelectionSets);
            }
            catch (Exception)
            {
                return DocumentCensus.NotCounted;
            }
        }

        /// <summary>The same walk, from the collection, for the rebuild.</summary>
        public static int Sets(DocumentSelectionSets sets)
        {
            if (sets == null)
            {
                return 0;
            }

            using (FolderItem root = sets.RootItem)
            {
                return SetsUnder(root);
            }
        }

        /// <summary>How many clash tests the document holds, or minus one.</summary>
        public static int Tests(Document document)
        {
            try
            {
                return document == null
                    ? DocumentCensus.NotCounted
                    : SavedTests.Count(document);
            }
            catch (Exception)
            {
                return DocumentCensus.NotCounted;
            }
        }

        /// <summary>
        /// How many clash results the document holds across every test, or minus one.
        ///
        /// LEAVES AND NEVER GROUPS, which is the same rule ClashRunner counts by. A result
        /// group is one row in the panel holding several clashes, and counting the group
        /// as one would let a run lose every clash inside it and report the same number.
        /// </summary>
        public static int Results(Document document)
        {
            if (document == null)
            {
                return DocumentCensus.NotCounted;
            }

            try
            {
                DocumentClash clash = document.GetClash();

                if (clash == null)
                {
                    return DocumentCensus.NotCounted;
                }

                DocumentClashTests tests = clash.TestsData;

                return tests == null ? DocumentCensus.NotCounted : ResultsUnderTests(tests.Tests);
            }
            catch (Exception)
            {
                return DocumentCensus.NotCounted;
            }
        }

        private static int SetsUnder(GroupItem parent)
        {
            if (parent == null)
            {
                return 0;
            }

            int count = 0;
            SavedItemCollection children = parent.Children;

            if (children == null)
            {
                return 0;
            }

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    if (child is SelectionSet)
                    {
                        count++;
                        continue;
                    }

                    GroupItem folder = child as GroupItem;

                    if (folder != null)
                    {
                        count += SetsUnder(folder);
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Every test under this collection, folders included. A ClashTest is itself a
        /// GroupItem holding its results, so it is tested for BEFORE a folder is, or the
        /// walk descends into the results and counts them as tests.
        /// </summary>
        private static int ResultsUnderTests(SavedItemCollection items)
        {
            if (items == null)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    ClashTest test = item as ClashTest;

                    if (test != null)
                    {
                        count += ResultsUnder(test.Children);
                        continue;
                    }

                    GroupItem folder = item as GroupItem;

                    if (folder != null)
                    {
                        count += ResultsUnderTests(folder.Children);
                    }
                }
            }

            return count;
        }

        /// <summary>Every leaf result under one test, descending its result groups.</summary>
        private static int ResultsUnder(SavedItemCollection children)
        {
            if (children == null)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    ClashResultGroup group = child as ClashResultGroup;

                    if (group != null)
                    {
                        count += ResultsUnder(group.Children);
                        continue;
                    }

                    if (child is ClashResult)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}
