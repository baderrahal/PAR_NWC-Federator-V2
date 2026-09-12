using System;
using System.Globalization;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// Where the images go and what they are called, matching the report the client
    /// already receives so the workbook and its pictures travel together.
    ///
    /// Measured on 2026-08-31 off two real exports, one small and one large, in
    /// C:\00_NM\Clash report. See docs\history\scan.md section 4k.
    ///
    /// The folder is the report name with _files on the end, beside the report. The
    /// pictures are loose jpg in it, plus a logo.jpg. Navisworks puts one there on its own
    /// reports and this tool copies one there too, off the machine that is running, so the
    /// page and its picture travel together.
    ///
    /// The name is NOT one running sequence, which is what it looks like from the first
    /// dozen. It is cd, then the test, then the clash within that test:
    ///
    ///     cd000001.jpg   test 0, clash 1
    ///     cd000013.jpg   test 0, clash 13
    ///     cd010001.jpg   test 1, clash 1
    ///     cd200001.jpg   test 20, clash 1
    ///     cd1000001.jpg  test 100, clash 1
    ///
    /// So the test is written with a floor of two digits and no ceiling, and the clash
    /// with a floor of four. Test 100 gives three digits and the name grows to seven,
    /// which is exactly what the 2672 image export holds and is the case a fixed width
    /// would have got wrong.
    /// </summary>
    public static class ImageNaming
    {
        /// <summary>What Navisworks appends to the report name to make the folder.</summary>
        public const string FolderSuffix = "_files";

        public const string Extension = ".jpg";

        /// <summary>The prefix on every clash picture.</summary>
        public const string Prefix = "cd";

        /// <summary>
        /// The logo goes in the same folder, which is where Navisworks puts it and where
        /// both supplied reports have it. Named here so a count of the pictures this tool
        /// rendered can leave it out, because it was copied rather than made. The name is
        /// read off InstallFiles rather than typed again, because the copy in the report
        /// folder is that file.
        /// </summary>
        public const string LogoName = InstallFiles.LogoName;

        /// <summary>Where the logo is copied to, beside the clash pictures.</summary>
        public static string LogoPathFor(string workbookPath)
        {
            string folder = FolderFor(workbookPath);

            return folder.Length == 0 ? string.Empty : Path.Combine(folder, LogoName);
        }

        /// <summary>
        /// What the page links the logo by. Relative and forward slashed, the same as
        /// every clash picture, so the page and its folder can be sent anywhere together.
        ///
        /// The accepted xlsx carries absolute file:/// links, which is exactly why its
        /// pictures break on any machine but the one that made it. Ours must not.
        /// </summary>
        public static string LogoLinkFor(string workbookPath)
        {
            string folder = FolderNameFor(workbookPath);

            return folder.Length == 0 ? string.Empty : folder + "/" + LogoName;
        }

        /// <summary>
        /// The images folder for a workbook, which is a sibling of it named after it.
        /// Takes the workbook path with or without its extension.
        /// </summary>
        public static string FolderFor(string workbookPath)
        {
            if (string.IsNullOrEmpty(workbookPath))
            {
                return string.Empty;
            }

            string folder = Path.GetDirectoryName(workbookPath) ?? string.Empty;
            string name = Path.GetFileNameWithoutExtension(workbookPath) ?? string.Empty;

            return Path.Combine(folder, name + FolderSuffix);
        }

        /// <summary>The folder name on its own, which is what a relative link starts with.</summary>
        public static string FolderNameFor(string workbookPath)
        {
            if (string.IsNullOrEmpty(workbookPath))
            {
                return string.Empty;
            }

            return (Path.GetFileNameWithoutExtension(workbookPath) ?? string.Empty) + FolderSuffix;
        }

        /// <summary>
        /// The file name for one clash. The test index is zero based and the clash index
        /// is one based, which is how the accepted report numbers them.
        /// </summary>
        public static string FileNameFor(int testIndex, int clashIndex)
        {
            if (testIndex < 0)
            {
                throw new ArgumentOutOfRangeException("testIndex", "A test index starts at zero.");
            }

            if (clashIndex < 1)
            {
                throw new ArgumentOutOfRangeException("clashIndex", "A clash index starts at one.");
            }

            return Prefix
                + testIndex.ToString("00", CultureInfo.InvariantCulture)
                + clashIndex.ToString("0000", CultureInfo.InvariantCulture)
                + Extension;
        }

        /// <summary>The full path for one clash picture beside one workbook.</summary>
        public static string PathFor(string workbookPath, int testIndex, int clashIndex)
        {
            return Path.Combine(FolderFor(workbookPath), FileNameFor(testIndex, clashIndex));
        }

        /// <summary>
        /// What the workbook cell links to. Relative and forward slashed, so the workbook
        /// and its folder can be moved together and the link still opens, which is the
        /// whole point of writing them side by side.
        /// </summary>
        public static string LinkFor(string workbookPath, int testIndex, int clashIndex)
        {
            return FolderNameFor(workbookPath) + "/" + FileNameFor(testIndex, clashIndex);
        }

        /// <summary>
        /// Reads a name back into the two numbers, or returns false. Here so a test can
        /// prove the numbering round trips rather than only that it prints.
        /// </summary>
        internal static bool TryRead(string fileName, out int testIndex, out int clashIndex)
        {
            testIndex = 0;
            clashIndex = 0;

            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            string name = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;

            if (!name.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return false;
            }

            string digits = name.Substring(Prefix.Length);

            // Four for the clash and whatever is left for the test, which is how a seven
            // digit name past test 99 reads correctly.
            if (digits.Length < 6)
            {
                return false;
            }

            string test = digits.Substring(0, digits.Length - 4);
            string clash = digits.Substring(digits.Length - 4);

            return int.TryParse(test, NumberStyles.None, CultureInfo.InvariantCulture, out testIndex)
                && int.TryParse(clash, NumberStyles.None, CultureInfo.InvariantCulture, out clashIndex);
        }
    }
}
