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

        /// <summary>
        /// The file left out merge_composites, or a side's selfintersect or primtypes, FR-155.
        /// Each read as 0 or false, a setting nobody wrote, so the test is skipped by name.
        /// </summary>
        MissingSetting,

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
        /// F224, Bader's answer to Q209: both sides carry one set code, BLD-AR against BLD-AR, so it is neither created nor
        /// run, and a test saved in the NWF keeps its earlier results there. Since F247, Q230, only where that code is one the
        /// pack's rules leave out, AR and ST for 1104-PAR.
        /// </summary>
        SameSetCode,



        /// <summary>Creating or running the test threw.</summary>
        Failed
    }
}
