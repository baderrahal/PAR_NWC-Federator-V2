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

        /// <summary>The test carried no name, so it could never be found again.</summary>
        NoName,

        /// <summary>A locator names a set that is not in the document.</summary>
        LocatorNotResolved,

        /// <summary>Both sides resolved, but one of them finds nothing in this model.</summary>
        EmptySide,

        /// <summary>
        /// The group holds fewer than two disciplines, so nothing in it can clash with
        /// anything else whatever the tests say. Counted apart from EmptySide on purpose.
        /// A side finding nothing says a discipline was not exported. One discipline says
        /// the group was never going to clash, and no export would change that. D5.
        /// </summary>
        SingleDiscipline,

        /// <summary>Creating or running the test threw.</summary>
        Failed
    }
}
