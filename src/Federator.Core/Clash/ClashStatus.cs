using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Federator.Core.Clash
{
    /// <summary>
    /// The result statuses, the numbers of Autodesk.Navisworks.Api.Clash.ClashResultStatus
    /// read off the installed DLL and recorded in docs\scan.md. Repeated here because Core
    /// never references the Navisworks API.
    /// </summary>
    public enum ClashStatus
    {
        New = 0,
        Active = 1,
        Reviewed = 2,
        Approved = 3,
        Resolved = 4
    }
}
