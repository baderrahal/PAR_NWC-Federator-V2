namespace Federator.Core.Diagnostics
{
    /// <summary>How one group ended. The three counts in the result block are these.</summary>
    public enum GroupOutcome
    {
        /// <summary>Every file appended and both outputs are on disk.</summary>
        Done,

        /// <summary>At least one file failed to append, but the outputs are on disk.</summary>
        Partial,

        /// <summary>Nothing usable came out of this group.</summary>
        Failed
    }

    /// <summary>A file the log verified on disk, with the size it read back.</summary>
    public sealed class WrittenFile
    {
        internal WrittenFile(string kind, string path, long sizeInBytes)
        {
            Kind = kind;
            Path = path;
            SizeInBytes = sizeInBytes;
        }

        /// <summary>NWF, NWD, or whatever the caller named it.</summary>
        public string Kind { get; private set; }

        public string Path { get; private set; }

        /// <summary>Read back off the disk. Never a size that was assumed.</summary>
        public long SizeInBytes { get; private set; }
    }

    /// <summary>One failure, kept whole so the result block can repeat it in full.</summary>
    public sealed class LoggedFailure
    {
        internal LoggedFailure(string what, string detail, string whatNext)
        {
            What = what;
            Detail = detail;
            WhatNext = whatNext;
        }

        public string What { get; private set; }

        /// <summary>Type name, message, inner exception and stack trace, already laid out.</summary>
        public string Detail { get; private set; }

        /// <summary>What the tool did after this failure, kept going or stopped.</summary>
        public string WhatNext { get; private set; }
    }
}
