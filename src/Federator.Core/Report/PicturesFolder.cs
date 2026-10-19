using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// A group's NAME_files folder in the Clash report folder deleted whole before this run's clash report of the group is
    /// written, F243, Bader's order of 19 Oct 2026 point 2: "delete the old pictures". Only the folder it is handed, never a
    /// file beside it. One line says the folder, how many clash pictures it held, the files named cd and digits as ImageNaming
    /// reads them, and apart from them the logo and by name every other file and folder that went with it, attempt 2 on the
    /// breaker's W4.
    ///
    /// THE FILES GO ONE BY ONE, attempt 2 on the reviewer's N3 and the breaker's C3, so a file another program holds is the
    /// one named, by its full path, for the caller's line of FileInUse, the rest still go, and the pictures already deleted
    /// are counted. A folder that held one is not deleted whole and says so. Any other failure is thrown to the caller, which
    /// logs it, and never taken for this.
    /// </summary>
    public sealed class PicturesFolder
    {
        private readonly List<string> held = new List<string>();
        private readonly List<string> others = new List<string>();

        private PicturesFolder(string folder)
        {
            Folder = folder ?? string.Empty;
        }

        /// <summary>The folder handed in.</summary>
        internal string Folder { get; private set; }

        /// <summary>True where the folder was there and is gone.</summary>
        internal bool Deleted { get; private set; }

        /// <summary>The clash pictures it held when it was read, nought where it was not there.</summary>
        internal int Pictures { get; private set; }

        /// <summary>The clash pictures deleted, all of them where the folder went whole.</summary>
        internal int PicturesDeleted { get; private set; }

        /// <summary>True where the folder held the logo the page links.</summary>
        private bool logo;

        /// <summary>The full path of each file another program held, so it and its folder stayed.</summary>
        public IList<string> Held
        {
            get { return held.AsReadOnly(); }
        }

        /// <summary>True where another program held a file of it, so it was not deleted whole.</summary>
        internal bool InUse
        {
            get { return held.Count > 0; }
        }

        /// <summary>
        /// True where a picture of an earlier run went, so a page or a workbook of that run left at its path now links this
        /// run's pictures, EarlierFileLine.
        /// </summary>
        public bool AnyPictureWent
        {
            get { return PicturesDeleted > 0 || Deleted; }
        }

        /// <summary>Deletes the folder and everything in it, a file at a time, counting its clash pictures first.</summary>
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

            List<string> top = new List<string>(Directory.GetFiles(folder));
            top.Sort(StringComparer.Ordinal);
            List<string> pictures = new List<string>();

            foreach (string file in top)
            {
                string name = Path.GetFileName(file);

                if (IsAPicture(name))
                {
                    pictures.Add(file);
                }
                else if (string.Equals(name, ImageNaming.LogoName, StringComparison.OrdinalIgnoreCase))
                {
                    outcome.logo = true;
                }
                else
                {
                    outcome.others.Add(name);
                }
            }

            List<string> folders = new List<string>(Directory.GetDirectories(folder));
            folders.Sort(StringComparer.Ordinal);

            foreach (string inner in folders)
            {
                outcome.others.Add(Path.GetFileName(inner));
            }

            outcome.Pictures = pictures.Count;

            foreach (string file in top)
            {
                if (outcome.DeleteOne(file) && pictures.Contains(file))
                {
                    outcome.PicturesDeleted++;
                }
            }

            foreach (string inner in folders)
            {
                List<string> below = new List<string>(Directory.GetFiles(inner, "*", SearchOption.AllDirectories));
                below.Sort(StringComparer.Ordinal);

                foreach (string file in below)
                {
                    outcome.DeleteOne(file);
                }
            }

            if (outcome.held.Count == 0)
            {
                Directory.Delete(folder, true);
                outcome.Deleted = true;
            }

            return outcome;
        }

        private static bool IsAPicture(string name)
        {
            int test;
            int clash;

            return ImageNaming.TryRead(name, out test, out clash)
                && string.Equals(Path.GetExtension(name), ImageNaming.Extension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Deletes one file, true where it went, and keeps its path where another program held it.</summary>
        private bool DeleteOne(string file)
        {
            try
            {
                File.Delete(file);
                return true;
            }
            catch (IOException error)
            {
                if (!FileInUse.IsInUse(error))
                {
                    throw;
                }

                held.Add(file);
                return false;
            }
        }

        /// <summary>The log's line. A file held is named after it by the caller in FileInUse's words, one line a file.</summary>
        public string Line()
        {
            if (InUse)
            {
                return "IMAGES   " + Folder + " not deleted whole, " + Number(PicturesDeleted) + " of its " + Number(Pictures)
                    + (Pictures == 1 ? " picture" : " pictures") + " deleted and " + Number(held.Count)
                    + (held.Count == 1 ? " file held open by another program, named on the line after"
                        : " files held open by another program, each named on the lines after");
            }

            if (!Deleted)
            {
                return "IMAGES   " + Folder + " is not there, so no picture of an earlier run is deleted";
            }

            string rest = string.Empty;

            if (logo)
            {
                rest += others.Count == 0 ? " and " + ImageNaming.LogoName : ", " + ImageNaming.LogoName;
            }

            if (others.Count > 0)
            {
                rest += " and " + Number(others.Count) + (others.Count == 1 ? " other file or folder, " : " other files and folders, ")
                    + string.Join(", ", others.ToArray());
            }

            return "IMAGES   deleted " + Folder + " with its " + Number(Pictures) + (Pictures == 1 ? " picture" : " pictures") + rest
                + ", before this run's clash report of the group is written";
        }

        /// <summary>
        /// The line for a page or a workbook of an earlier run this run did not write, left at its path after the pictures
        /// folder went, attempt 2 on the breaker's S3 and W5. The file is not touched.
        /// </summary>
        public static string EarlierFileLine(string path)
        {
            return "IMAGES   " + path + " is an earlier run's, left as it is since this run did not write it, and its picture"
                + " links now reach this run's pictures";
        }

        private static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
