using System;
using System.Globalization;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// A group's NAME_files folder in the Clash report folder deleted whole before this run's clash report of the group is
    /// written, F243, Bader's order of 19 Oct 2026 point 2: "delete the old pictures". Only the folder it is handed, never a
    /// file beside it, and one line saying the folder and how many clash pictures it held, the files named cd and digits as
    /// ImageNaming reads them, the logo and anything else in it going with them.
    ///
    /// A folder another program holds a file of is not deleted and the outcome says so, InUse, for the caller's line of
    /// FileInUse. Any other failure is thrown to the caller, which logs it, and never taken for this.
    /// </summary>
    public sealed class PicturesFolder
    {
        private PicturesFolder(string folder)
        {
            Folder = folder ?? string.Empty;
        }

        /// <summary>The folder handed in.</summary>
        public string Folder { get; private set; }

        /// <summary>True where the folder was there and is gone.</summary>
        public bool Deleted { get; private set; }

        /// <summary>The clash pictures it held when it was read, nought where it was not there.</summary>
        public int Pictures { get; private set; }

        /// <summary>True where another program held a file of it, so it was not deleted, or not whole.</summary>
        public bool InUse { get; private set; }

        /// <summary>Deletes the folder and everything in it, counting its clash pictures first.</summary>
        public static PicturesFolder Delete(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !folder.EndsWith(ImageNaming.FolderSuffix, StringComparison.Ordinal))
            {
                throw new ArgumentException("Only a folder of pictures, its name ending " + ImageNaming.FolderSuffix + ", is deleted here.", "folder");
            }

            PicturesFolder outcome = new PicturesFolder(folder);

            if (!Directory.Exists(folder))
            {
                return outcome;
            }

            foreach (string file in Directory.GetFiles(folder))
            {
                int test;
                int clash;

                if (ImageNaming.TryRead(Path.GetFileName(file), out test, out clash)
                    && string.Equals(Path.GetExtension(file), ImageNaming.Extension, StringComparison.OrdinalIgnoreCase))
                {
                    outcome.Pictures++;
                }
            }

            try
            {
                Directory.Delete(folder, true);
                outcome.Deleted = true;
            }
            catch (IOException error)
            {
                if (!FileInUse.IsInUse(error))
                {
                    throw;
                }

                outcome.InUse = true;
            }

            return outcome;
        }

        /// <summary>The log's line, or null where another program held it open, which the caller says in FileInUse's words.</summary>
        public string Line()
        {
            if (InUse)
            {
                return null;
            }

            return Deleted
                ? "IMAGES   deleted " + Folder + " with its " + Pictures.ToString(CultureInfo.InvariantCulture)
                    + (Pictures == 1 ? " picture" : " pictures") + ", before this run's clash report of the group is written"
                : "IMAGES   " + Folder + " is not there, so no picture of an earlier run is deleted";
        }
    }
}
