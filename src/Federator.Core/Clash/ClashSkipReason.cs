using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Exchange;

namespace Federator.Core.Clash
{
    /// <summary>Why a test in the file is not going to be created or not going to be run.</summary>
    public enum ClashSkipReason
    {
        /// <summary>The test_type string is not one this tool creates.</summary>
        UnknownTestType,

        /// <summary>A side carried no locator, so it names no set.</summary>
        NoLocator,

        /// <summary>The units are missing or not ones this tool converts.</summary>
        UnknownUnits,

        /// <summary>
        /// The file carried no tolerance attribute for this test. Everything about a test
        /// comes from the file and never from a constant, and a zero tolerance reads as a
        /// real one, so the test is skipped by name the way an unknown test type is.
        /// </summary>
        NoTolerance,

        /// <summary>The test carried no name, so it could never be found again.</summary>
        NoName,

        /// <summary>A locator names a set that is not in the document.</summary>
        LocatorNotResolved,

        /// <summary>Both sides resolved, but one of them finds nothing in this model.</summary>
        EmptySide,

        /// <summary>
        /// The group holds fewer than two disciplines, so nothing in it can clash with
        /// anything else whatever the tests say. Counted apart from EmptySide on purpose,
        /// for the reason in .claude\rules\core.md. D5.
        /// </summary>
        SingleDiscipline,

        /// <summary>
        /// A model of the group is not on the same shared coordinates as its reference
        /// model, so the group's clash is skipped and only its clash, Bader's answer to Q99
        /// and Q100. The tests are created and none is run, the path SingleDiscipline takes,
        /// and counted apart from it because it is a different fact about a different problem.
        /// </summary>
        NotOnTheSameCoordinates,

        /// <summary>Creating or running the test threw.</summary>
        Failed,

        /// <summary>
        /// Its two sides are another test's two sets swapped, and the other test is the one
        /// kept, so this one is not created and not run, F132, Bader's Q114 points 4 to 6.
        /// The reason names the test kept. MirrorRule decides it.
        /// </summary>
        Mirror
    }
}
