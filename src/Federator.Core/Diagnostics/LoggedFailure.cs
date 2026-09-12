namespace Federator.Core.Diagnostics
{
    /// <summary>One failure, kept whole so the result block can repeat it in full.</summary>
    public sealed class LoggedFailure
    {
        internal LoggedFailure(string what, string detail, string whatNext)
        {
            What = what;
            Detail = detail;
            WhatNext = whatNext;
            Times = 1;
        }

        public string What { get; private set; }

        /// <summary>Type name, message, inner exception and stack trace, already laid out.</summary>
        public string Detail { get; private set; }

        /// <summary>What the tool did after this failure, kept going or stopped.</summary>
        public string WhatNext { get; private set; }

        /// <summary>
        /// How many times this exact failure happened. One run threw the same exception
        /// tens of thousands of times and wrote a 17.8 MB log that was almost entirely one
        /// stack trace, so the trace is kept once and the rest are counted.
        /// </summary>
        public int Times { get; private set; }

        internal void AgainOnce()
        {
            Times++;
        }
    }
}
