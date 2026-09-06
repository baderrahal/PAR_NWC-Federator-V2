using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Clash
{
    /// <summary>
    /// One clash test as it sits in the open document, read off DocumentClashTests by the
    /// add-in and handed here with no Navisworks types on it. This is the second way a
    /// plan is built: with no XML picked, the tests already saved in the NWF are run.
    ///
    /// The tolerance is already in the document's units, because it was read out of the
    /// document. The sides are whatever the test was saved with and are not resolved
    /// against the sets, because there is no file to resolve them from.
    /// </summary>
    public sealed class SavedClashTest
    {
        public SavedClashTest(
            string name,
            int testTypeNumber,
            double tolerance,
            bool mergeComposites,
            bool leftSelfIntersect,
            int leftPrimitiveTypes,
            string leftLocator,
            bool rightSelfIntersect,
            int rightPrimitiveTypes,
            string rightLocator,
            IList<int> address)
        {
            if (address == null || address.Count == 0)
            {
                throw new ArgumentException("A saved test needs the address it was found at.", "address");
            }

            Name = name ?? string.Empty;
            TestTypeNumber = testTypeNumber;
            Tolerance = tolerance;
            MergeComposites = mergeComposites;
            LeftSelfIntersect = leftSelfIntersect;
            LeftPrimitiveTypes = leftPrimitiveTypes;
            LeftLocator = leftLocator ?? string.Empty;
            RightSelfIntersect = rightSelfIntersect;
            RightPrimitiveTypes = rightPrimitiveTypes;
            RightLocator = rightLocator ?? string.Empty;
            Address = new ReadOnlyCollection<int>(new List<int>(address));
        }

        public string Name { get; private set; }

        /// <summary>
        /// Autodesk.Navisworks.Api.Clash.ClashTestType as an int, which is the same
        /// numbering ClashTestKind carries. An int here because Core never references the
        /// Navisworks API.
        /// </summary>
        public int TestTypeNumber { get; private set; }

        /// <summary>Already in the document's units.</summary>
        public double Tolerance { get; private set; }

        public bool MergeComposites { get; private set; }

        public bool LeftSelfIntersect { get; private set; }

        public int LeftPrimitiveTypes { get; private set; }

        /// <summary>What the left side reads as. Empty where the add-in could not say.</summary>
        public string LeftLocator { get; private set; }

        public bool RightSelfIntersect { get; private set; }

        public int RightPrimitiveTypes { get; private set; }

        public string RightLocator { get; private set; }

        /// <summary>
        /// The path of child indexes from the root of the tests tree, which is how a test
        /// is found again. A test is addressed rather than held, because every mutation
        /// through DocumentClashTests kills any handle onto it.
        /// </summary>
        public ReadOnlyCollection<int> Address { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
