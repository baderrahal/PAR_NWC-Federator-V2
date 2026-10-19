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
    /// A LINK IS NEVER ENTERED, attempt 3: a subfolder carrying the reparse point attribute, a junction or a symbolic link, is
    /// removed as the link alone, and any other subfolder whole, so nothing outside the folder is deleted through it. A file
    /// that will not go for another reason than a program holding it is kept in Failed with what it threw.
    ///
    /// THE FILES GO ONE BY ONE, attempt 2 on the reviewer's N3 and the breaker's C3, so a file another program holds is the
    /// one named, by its full path, for the caller's line of FileInUse, the rest still go, and the pictures already deleted
    /// are counted. A folder that held one, or one that would not go, is not deleted whole and says so. Only a failure to read
    /// the folder itself is thrown to the caller, which logs it.
    /// </summary>
    public sealed class PicturesFolder
    {
        private readonly List<string> held = new List<string>();
        private readonly List<string> others = new List<string>();
        private readonly List<KeyValuePair<string, Exception>> failed = new List<KeyValuePair<string, Exception>>();

        /// <summary>True where the folder handed in was itself a link, removed alone.</summary>
        private bool wasALink;

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

        /// <summary>Each entry that would not go for another reason than a program holding it, with what it threw, for the caller to log.</summary>
        public IList<KeyValuePair<string, Exception>> Failed
        {
            get { return failed.AsReadOnly(); }
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

            // The guard pass on Bader's decision: a folder handed in that is itself a link is removed as the link alone, and
            // nothing inside it is listed or deleted.
            if (IsALink(folder))
            {
                Directory.Delete(folder, false);
                outcome.Deleted = true;
                outcome.wasALink = true;
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
                outcome.others.Add(Path.GetFileName(inner) + (IsALink(inner) ? ", a link removed alone" : string.Empty));
            }

            outcome.Pictures = pictures.Count;

            foreach (string file in top)
            {
                if (outcome.DeleteOne(file, () => File.Delete(file)) && pictures.Contains(file))
                {
                    outcome.PicturesDeleted++;
                }
            }

            // Attempt 3, both readers' blocking finding. A link, a junction or a symbolic link to a folder, is removed as the
            // link alone and never entered, so nothing outside the folder is deleted through it. Any other folder goes whole,
            // and Directory.Delete does not follow a link below it on .NET Framework.
            foreach (string inner in folders)
            {
                bool link = IsALink(inner);
                outcome.DeleteOne(inner, () => Directory.Delete(inner, !link));
            }

            if (outcome.held.Count == 0 && outcome.failed.Count == 0)
            {
                Directory.Delete(folder, false);
                outcome.Deleted = true;
            }

            return outcome;
        }

        private static bool IsALink(string folder)
        {
            return (File.GetAttributes(folder) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
        }
        private static bool IsAPicture(string name)
        {
            int test;
            int clash;

            return ImageNaming.TryRead(name, out test, out clash)
                && string.Equals(Path.GetExtension(name), ImageNaming.Extension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Deletes one entry, true where it went. One another program holds is kept in Held, and since attempt 3, the breaker's
        /// 6, one that will not go for another reason, a file read only or a denied folder among them, is kept in Failed with
        /// what it threw, so what already went is still counted and the rest still go. Neither is ever taken for deleted.
        /// </summary>
        private bool DeleteOne(string path, Action delete)
        {
            try
            {
                delete();
                return true;
            }
            catch (IOException error)
            {
                if (FileInUse.IsInUse(error))
                {
                    held.Add(path);
                }
                else
                {
                    failed.Add(new KeyValuePair<string, Exception>(path, error));
                }

                return false;
            }
            catch (UnauthorizedAccessException error)
            {
                failed.Add(new KeyValuePair<string, Exception>(path, error));
                return false;
            }
        }
        /// <summary>
        /// The log's line. Each file held is named after it by the caller in FileInUse's words, and each that would not go with
        /// what it threw, one line each.
        /// </summary>
        public string Line()
        {
            if (wasALink)
            {
                return "IMAGES   " + Folder + " was a link, removed alone, nothing it points at was touched";
            }

            if (held.Count > 0 || failed.Count > 0)
            {
                return "IMAGES   " + Folder + " not deleted whole, " + Number(PicturesDeleted) + " of its " + Number(Pictures)
                    + (Pictures == 1 ? " picture" : " pictures") + " deleted, " + Number(held.Count) + " held open by another program and "
                    + Number(failed.Count) + " that would not go, each named on the lines after";
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
        /// folder went, attempt 2 on the breaker's S3 and W5. Since attempt 3, the reviewer's N3 and N4, it says what the links
        /// reach: this run's pictures where this run took pictures, and otherwise that the pictures they pointed at are gone.
        /// The file is not touched.
        /// </summary>
        public static string EarlierFileLine(string path, bool tookPictures)
        {
            return "IMAGES   " + path + " is an earlier run's, left as it is since this run did not write it, and "
                + (tookPictures ? "its picture links now reach this run's pictures" : "the pictures its links pointed at are gone");
        }
        private static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
