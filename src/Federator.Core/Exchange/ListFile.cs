using System;
using System.IO;
using System.Text;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// How a plain list kept beside the picked XML is read, the one way for every such list:
    /// the list of corrections, Q113, and the team map, Q114 and Q115. Moved out of
    /// MatrixCorrectionList when the team map came, F131, so the two lists are read by one rule
    /// and cannot come to read a line two ways.
    ///
    /// A LINE is a word and a colon, a space, then its parts split by a space, a bar and a
    /// space. A line starting with # is a comment and a blank line is skipped. NOTHING IS
    /// TRIMMED, because two of the client's set names end in a space and a name is matched
    /// exactly, core.md.
    ///
    /// THE BYTES MUST BE UTF-8, or UTF-16 with its byte order mark, and a list that is not is
    /// refused with why, never read with a replacement character in place of a byte. A list
    /// that cannot be opened is refused with why. Never a throw.
    /// </summary>
    internal static class ListFile
    {
        /// <summary>What splits the parts of a line.</summary>
        internal const string Bar = " | ";

        /// <summary>
        /// UTF-8 that refuses a byte it cannot read. The default puts a replacement character
        /// in its place, and a measured spelling holding a no-break space would then ask a
        /// workset no model carries and say nothing.
        /// </summary>
        private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

        /// <summary>
        /// The list at that path read by that reader, or what refused gives where the bytes are
        /// not UTF-8 text or the file could not be opened, with why. The path is one full path
        /// the caller tested with File.Exists.
        /// </summary>
        internal static T Read<T>(string path, Func<TextReader, T> read, Func<string, T> refused)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path, StrictUtf8, true))
                {
                    return read(reader);
                }
            }
            catch (DecoderFallbackException)
            {
                return refused("it is not UTF-8 text");
            }
            catch (IOException failed)
            {
                return refused("it could not be opened, " + failed.Message.TrimEnd('.'));
            }
            catch (UnauthorizedAccessException failed)
            {
                return refused("it could not be opened, " + failed.Message.TrimEnd('.'));
            }
        }

        /// <summary>Whether a line is a comment or blank, and so skipped.</summary>
        internal static bool Skipped(string line)
        {
            return line.Trim().Length == 0 || line[0] == '#';
        }

        /// <summary>
        /// The parts after that marker and one space, split on a bar with a space each side,
        /// or null where the line does not start with it. Never trimmed.
        /// </summary>
        internal static string[] PartsAfter(string line, string marker)
        {
            string rest = After(line, marker);

            return rest == null ? null : rest.Split(new[] { Bar }, StringSplitOptions.None);
        }

        /// <summary>Everything after that marker and one space, never trimmed, or null where the line does not start with it.</summary>
        internal static string After(string line, string marker)
        {
            if (!line.StartsWith(marker + " ", StringComparison.Ordinal))
            {
                return null;
            }

            return line.Substring(marker.Length + 1);
        }

        /// <summary>
        /// The full path of the list beside that XML: its folder, its file name without the
        /// extension, and that suffix, whether or not a file is there. ONE FULL PATH, NEVER A
        /// SEARCH, because a list picked up by a near match would answer for a file with another
        /// project's decisions and the log would read as if it had been asked.
        /// </summary>
        internal static string PathBeside(string xmlPath, string suffix)
        {
            if (string.IsNullOrEmpty(xmlPath))
            {
                throw new ArgumentException("A list is found beside a picked XML, and no XML was named.", "xmlPath");
            }

            string full = Path.GetFullPath(xmlPath);

            return Path.Combine(
                Path.GetDirectoryName(full),
                Path.GetFileNameWithoutExtension(full) + (suffix ?? string.Empty));
        }
    }
}
