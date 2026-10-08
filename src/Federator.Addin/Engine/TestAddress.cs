using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.DocumentParts;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Naming;
using Federator.Core.Report;
using Federator.Core.Units;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;
using CoreClashStatus = Federator.Core.Clash.ClashStatus;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Where a clash test sits, as the path of child indexes from the root of the tests
    /// tree. A test is addressed rather than held, because every mutation through
    /// DocumentClashTests replaces the native object and kills any handle onto it.
    /// </summary>
    public sealed class TestAddress
    {
        private readonly int[] path;

        private TestAddress(int[] path)
        {
            this.path = path;
        }

        public static TestAddress At(int index)
        {
            return new TestAddress(new[] { index });
        }

        public static TestAddress At(IList<int> path)
        {
            if (path == null || path.Count == 0)
            {
                throw new ArgumentException("A test address needs at least one index.", "path");
            }

            int[] copy = new int[path.Count];

            for (int i = 0; i < path.Count; i++)
            {
                copy[i] = path[i];
            }

            return new TestAddress(copy);
        }

        public int Depth
        {
            get { return path.Length; }
        }

        public int IndexAt(int level)
        {
            return path[level];
        }

        /// <summary>
        /// The test at this address in a freshly read collection, checked to be the test
        /// that name says, or null: nothing is at the address, what is there is not a test,
        /// or it is a test of another name, which nowNamed then carries so the caller can
        /// say so. Null rather than the wrong test, because running or reading the wrong
        /// test writes into somebody else's. The caller disposes what comes back. The
        /// wrapper is created with eEXTERNAL ownership, so disposing it releases the
        /// wrapper and never the document's test. Here since F132 attempt 2, so the views
        /// resolve a test the one way the runner does.
        /// </summary>
        public ClashTest ResolveIn(DocumentClashTests clashTests, string name, out string nowNamed)
        {
            nowNamed = null;
            SavedItemCollection children = clashTests.Tests;
            SavedItem item = null;
            GroupItem walked = null;

            for (int level = 0; level < path.Length; level++)
            {
                int index = path[level];

                if (children == null || index < 0 || index >= children.Count)
                {
                    if (walked != null)
                    {
                        walked.Dispose();
                    }

                    return null;
                }

                item = children[index];

                // The level walked past is released only once the child below it has been
                // read, which is the order ResolveFolders in SetBuilder uses.
                if (walked != null)
                {
                    walked.Dispose();
                    walked = null;
                }

                if (level + 1 == path.Length)
                {
                    break;
                }

                GroupItem group = item as GroupItem;

                if (group == null)
                {
                    item.Dispose();
                    return null;
                }

                children = group.Children;
                walked = group;
                item = null;
            }

            ClashTest test = item as ClashTest;

            if (test == null)
            {
                if (item != null)
                {
                    item.Dispose();
                }

                return null;
            }

            if (!string.Equals(test.DisplayName, name, StringComparison.Ordinal))
            {
                nowNamed = test.DisplayName ?? string.Empty;
                test.Dispose();
                return null;
            }

            return test;
        }

        public override string ToString()
        {
            string[] parts = new string[path.Length];

            for (int i = 0; i < path.Length; i++)
            {
                parts[i] = path[i].ToString();
            }

            return "tests[" + string.Join("][", parts) + "]";
        }
    }
}
