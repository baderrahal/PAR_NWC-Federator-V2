using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Exchange;

namespace Federator.Core.Clash
{
    /// <summary>Where a plan's tests came from.</summary>
    public enum ClashPlanSource
    {
        /// <summary>The picked exchange file. Tests are created where missing and run.</summary>
        ExchangeFile,

        /// <summary>
        /// The tests already saved in the open document. Nothing is created, nothing is
        /// compared against a file, and every test is run where it sits.
        /// </summary>
        Document
    }
}
