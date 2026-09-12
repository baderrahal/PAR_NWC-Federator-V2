using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Exchange;

namespace Federator.Core.Clash
{
    /// <summary>A test that will not be created, or will not be run, and why.</summary>
    public sealed class SkippedClashTest
    {
        internal SkippedClashTest(string name, ClashSkipReason kind, string reason)
            : this(name, kind, reason, -1)
        {
        }

        internal SkippedClashTest(string name, ClashSkipReason kind, string reason, int fileIndex)
        {
            Name = name;
            Kind = kind;
            Reason = reason;
            FileIndex = fileIndex;
        }

        /// <summary>Where this test sat in the file, from zero. Minus one when unknown.</summary>
        public int FileIndex { get; private set; }

        /// <summary>The test name, so it can be reported by name as the rule requires.</summary>
        public string Name { get; private set; }

        public ClashSkipReason Kind { get; private set; }

        public string Reason { get; private set; }

        public override string ToString()
        {
            return Name + "  " + Reason;
        }
    }
}
