namespace Federator.Core.Diagnostics
{
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
        public long SizeInBytes { get; internal set; }
    }
}
