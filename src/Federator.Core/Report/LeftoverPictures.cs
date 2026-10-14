using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// F177, Bader's order of 14 Oct 2026, round 4, task 3. The pictures a run leaves in its group's
    /// NAME_files folder that an earlier run wrote and this run did not are removed.
    ///
    /// WHAT IS REMOVED, and nothing else:
    ///   a FILE, at the TOP LEVEL of that group's NAME_files folder, whose name is the tool's own
    ///   picture name, cd and six or more digits and .jpg, all lower case as the tool writes it, and
    ///   which this run did not write.
    /// It never touches the logo, any other file or any folder, a sub folder or the folder itself, a file
    /// whose name is not that pattern, a file that is a link, or anything outside the folder it is
    /// handed. The folder must be named for a report, so it ends in _files, and be a real folder, not a
    /// link, or nothing is walked.
    ///
    /// WHEN NOTHING IS REMOVED. A run that did not reach its picture step, or failed, or wrote no picture
    /// at all removes nothing and says why in one line. A run that wrote none is not read as a run that
    /// has no leftovers, since an empty list of what it wrote may be a fault and not a fact.
    ///
    /// AN OLDER EXCEL in the Log folder points at jpgs this run overwrites by name, so it then shows
    /// this run's pictures. That is Bader's order and the one line says it.
    /// </summary>
    public static class LeftoverPictures
    {
        /// <summary>The tool's own picture name: cd, six or more digits, .jpg, as ImageNaming writes it.</summary>
        public static bool IsOurPictureName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            if (!fileName.StartsWith(ImageNaming.Prefix, StringComparison.Ordinal)
                || !fileName.EndsWith(ImageNaming.Extension, StringComparison.Ordinal))
            {
                return false;
            }

            string digits = fileName.Substring(
                ImageNaming.Prefix.Length,
                fileName.Length - ImageNaming.Prefix.Length - ImageNaming.Extension.Length);

            if (digits.Length < 6)
            {
                return false;
            }

            foreach (char c in digits)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The names of the files at the top level of the folder that are the tool's own pictures and not
        /// in the set the run wrote. Empty where the folder is not a report's pictures folder, is a link,
        /// or is not there. Nothing is changed.
        /// </summary>
        public static IList<string> Find(string pictureFolder, ICollection<string> writtenByThisRun, out string whyNot)
        {
            List<string> found = new List<string>();
            whyNot = Refusal(pictureFolder);

            if (whyNot != null)
            {
                return found;
            }

            HashSet<string> written = new HashSet<string>(StringComparer.Ordinal);

            if (writtenByThisRun != null)
            {
                foreach (string name in writtenByThisRun)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        written.Add(Path.GetFileName(name));
                    }
                }
            }

            foreach (string path in Directory.GetFiles(pictureFolder, "*", SearchOption.TopDirectoryOnly))
            {
                string name = Path.GetFileName(path);

                if (!IsOurPictureName(name) || written.Contains(name))
                {
                    continue;
                }

                FileAttributes attributes = File.GetAttributes(path);

                if ((attributes & FileAttributes.ReparsePoint) != 0 || (attributes & FileAttributes.Directory) != 0)
                {
                    continue;
                }

                found.Add(name);
            }

            found.Sort(StringComparer.Ordinal);
            return found;
        }

        /// <summary>
        /// Removes them, once the run's pictures are written. Never throws: a file that cannot be removed
        /// is named and kept, and the run goes on.
        /// </summary>
        public static LeftoverPicturesOutcome Remove(
            string pictureFolder, ICollection<string> writtenByThisRun, bool pictureStepFinished, string runFailedBecause)
        {
            if (!pictureStepFinished)
            {
                return LeftoverPicturesOutcome.NotDone(pictureFolder, "the run did not reach the end of its picture step");
            }

            if (!string.IsNullOrEmpty(runFailedBecause))
            {
                return LeftoverPicturesOutcome.NotDone(pictureFolder, "the run failed, " + runFailedBecause);
            }

            if (writtenByThisRun == null || writtenByThisRun.Count == 0)
            {
                return LeftoverPicturesOutcome.NotDone(pictureFolder, "this run wrote no picture, so none is told from a leftover");
            }

            IList<string> names;
            string whyNot;

            try
            {
                names = Find(pictureFolder, writtenByThisRun, out whyNot);
            }
            catch (Exception error)
            {
                if (error is IOException || error is UnauthorizedAccessException)
                {
                    return LeftoverPicturesOutcome.NotDone(pictureFolder, "the folder could not be read, " + error.GetType().Name);
                }

                throw;
            }

            if (whyNot != null)
            {
                return LeftoverPicturesOutcome.NotDone(pictureFolder, whyNot);
            }

            List<string> removed = new List<string>();
            List<string> kept = new List<string>();

            foreach (string name in names)
            {
                string path = Path.Combine(pictureFolder, name);

                try
                {
                    File.Delete(path);
                    removed.Add(name);
                }
                catch (Exception error)
                {
                    if (error is IOException || error is UnauthorizedAccessException)
                    {
                        kept.Add(name + " (" + error.GetType().Name + ")");
                        continue;
                    }

                    throw;
                }
            }

            return LeftoverPicturesOutcome.Done(pictureFolder, removed, kept, writtenByThisRun.Count);
        }

        private static string Refusal(string pictureFolder)
        {
            if (string.IsNullOrEmpty(pictureFolder) || !Path.IsPathRooted(pictureFolder))
            {
                return "no full path of a pictures folder was given";
            }

            string trimmed = pictureFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string name = Path.GetFileName(trimmed);

            if (name.Length <= ImageNaming.FolderSuffix.Length
                || !name.EndsWith(ImageNaming.FolderSuffix, StringComparison.Ordinal))
            {
                return pictureFolder + " is not a report's pictures folder, its name does not end in "
                    + ImageNaming.FolderSuffix;
            }

            if (!Directory.Exists(pictureFolder))
            {
                return pictureFolder + " is not there";
            }

            if ((File.GetAttributes(pictureFolder) & FileAttributes.ReparsePoint) != 0)
            {
                return pictureFolder + " is a link to another folder, so it is not walked";
            }

            return null;
        }
    }

    /// <summary>What a removal of leftover pictures did, said in one line.</summary>
    public sealed class LeftoverPicturesOutcome
    {
        private LeftoverPicturesOutcome()
        {
            Removed = new List<string>();
            Kept = new List<string>();
        }

        public string Folder { get; private set; }

        /// <summary>Null where it ran. Otherwise why nothing was removed.</summary>
        public string NotDoneBecause { get; private set; }

        /// <summary>The names removed.</summary>
        public IList<string> Removed { get; private set; }

        /// <summary>The names that could not be removed, each with why.</summary>
        public IList<string> Kept { get; private set; }

        /// <summary>The pictures this run wrote in the folder.</summary>
        public int WrittenByThisRun { get; private set; }

        internal static LeftoverPicturesOutcome NotDone(string folder, string why)
        {
            LeftoverPicturesOutcome outcome = new LeftoverPicturesOutcome();
            outcome.Folder = folder ?? string.Empty;
            outcome.NotDoneBecause = why;
            return outcome;
        }

        internal static LeftoverPicturesOutcome Done(string folder, IList<string> removed, IList<string> kept, int written)
        {
            LeftoverPicturesOutcome outcome = new LeftoverPicturesOutcome();
            outcome.Folder = folder;
            outcome.WrittenByThisRun = written;

            foreach (string name in removed)
            {
                outcome.Removed.Add(name);
            }

            foreach (string name in kept)
            {
                outcome.Kept.Add(name);
            }

            return outcome;
        }

        /// <summary>The one line for the run log.</summary>
        public string Line()
        {
            if (NotDoneBecause != null)
            {
                return "no leftover picture was removed from " + Folder + ", " + NotDoneBecause;
            }

            return "removed " + Removed.Count.ToString(CultureInfo.InvariantCulture)
                + " leftover picture(s) from " + Folder + ", files named cd and digits.jpg that this run did not write"
                + ", kept the " + WrittenByThisRun.ToString(CultureInfo.InvariantCulture) + " it wrote and every other file"
                + (Kept.Count == 0
                    ? string.Empty
                    : ". " + Kept.Count.ToString(CultureInfo.InvariantCulture) + " could not be removed: " + string.Join(", ", Kept))
                + ". An older Excel in the Log folder links to these names, so it now shows this run's pictures";
        }
    }
}
