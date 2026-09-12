using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Exchange;

namespace Federator.Core.Clash
{
    /// <summary>
    /// The clash test types this tool knows how to create. The numbers are
    /// Autodesk.Navisworks.Api.Clash.ClashTestType, read off the installed DLL on
    /// 2026-08-30 and recorded in docs\scan.md. They are repeated here because Core never
    /// references the Navisworks API, exactly as ConditionTest does for the sets.
    ///
    /// A test type that is not one of these is reported by name and skipped. It is never
    /// approximated to the nearest one, because a hard test standing in for a clearance
    /// test reports a number that reads as real and is not.
    /// </summary>
    public enum ClashTestKind
    {
        Hard = 0,
        HardConservative = 1,
        Clearance = 2,
        Duplicate = 3,
        Custom = 4
    }
}
