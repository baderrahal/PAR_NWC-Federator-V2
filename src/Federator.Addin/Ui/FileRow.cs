using System;
using Federator.Core.Naming;

namespace Federator.Addin.Ui
{
    /// <summary>
    /// One NWC found by the scan. A row whose name could not be read arrives unticked
    /// with the reason showing, and is never hidden from the table.
    /// </summary>
    public sealed class FileRow : ObservableObject
    {
        private bool include;

        public FileRow(string fullPath, ParsedContainerName parsed)
        {
            FullPath = fullPath;
            FileName = System.IO.Path.GetFileName(fullPath);
            Parsed = parsed;
            include = parsed.IsReadable;
        }

        public string FullPath { get; private set; }

        public string FileName { get; private set; }

        public ParsedContainerName Parsed { get; private set; }

        public bool IsReadable
        {
            get { return Parsed.IsReadable; }
        }

        public bool Include
        {
            get { return include; }
            set
            {
                if (include == value)
                {
                    return;
                }

                // An unreadable name has no building to group on, so it can never be
                // ticked back on.
                include = value && Parsed.IsReadable;
                Raise("Include");
            }
        }

        public bool CanInclude
        {
            get { return Parsed.IsReadable; }
        }

        public string Building
        {
            get { return Parsed.IsReadable ? Parsed.Building : string.Empty; }
        }

        public string Discipline
        {
            get { return Parsed.IsReadable ? Parsed.Discipline : string.Empty; }
        }

        public string Project
        {
            get { return Parsed.IsReadable ? Parsed.Project : string.Empty; }
        }

        public string Originator
        {
            get { return Parsed.IsReadable ? Parsed.Originator : string.Empty; }
        }

        /// <summary>Empty when the name read cleanly, the reason when it did not.</summary>
        public string Reason
        {
            get { return Parsed.IsReadable ? string.Empty : Parsed.UnreadableReason; }
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
