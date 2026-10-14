using System;
using System.IO;
using Federator.Core.Rerun;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The file of where each NWF's NWD and Log folder are, F189, kept in the tool's own folder under
    /// %LOCALAPPDATA%\ParsonsNwcFederator. Read and written here and nowhere else, never throwing, because a list
    /// that will not read is no list and the run goes on with the folders beside the file.
    /// </summary>
    internal static class NwfPlacesStore
    {
        internal static string FilePath()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ParsonsNwcFederator");

            return Path.Combine(folder, NwfPlaces.FileName);
        }

        internal static NwfPlaces Load()
        {
            try
            {
                string path = FilePath();
                return File.Exists(path) ? NwfPlaces.Read(File.ReadAllLines(path)) : new NwfPlaces();
            }
            catch (Exception)
            {
                return new NwfPlaces();
            }
        }

        internal static void Remember(string nwf, string nwd, string logFolder)
        {
            try
            {
                NwfPlaces places = Load();
                places.Remember(nwf, nwd, logFolder);
                string path = FilePath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllLines(path, places.ToLines());
            }
            catch (Exception)
            {
                // The list is a convenience. A run never fails for it.
            }
        }
    }
}
