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
