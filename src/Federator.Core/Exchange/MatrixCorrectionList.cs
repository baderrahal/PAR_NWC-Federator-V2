using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// The corrections one project needs, read off the list kept BESIDE the picked clash XML and
    /// named after it, Q113 answered B by Bader on 2026-10-04: the renames, the catch-all sets,
    /// the Source File rules of Q103 and the workset spellings measured in that project's
    /// models. The tool serves many projects, so no project's list is inside it, and the code
    /// that applies a list, MatrixCorrections, names no set, folder, category or spelling.
    ///
    /// ONE FULL PATH, tested with File.Exists, CorrectionListSettings. A picked XML with no list
    /// there is corrected by nothing and the log says so. That is a project needing no
    /// correction as often as a list left behind, so it is said plainly and not as a fault.
    ///
    /// A LIST THAT CANNOT BE READ IS SAID, NEVER AN EMPTY ONE. An empty list corrects nothing and
    /// looks like a file that needed nothing, so a list that cannot be opened, is not UTF-8 text
    /// or holds a line this does not know is Unread, carrying why, and the picked file is
    /// corrected not at all and never in part. Never a throw.
    ///
    /// A LIST HOLDING NONE CORRECTS NOTHING, AS NO LIST DOES, and its first line says so, F116 on
    /// the breaker's finding: a file of no bytes, of comments or of blank lines. And a workset
    /// value is corrected only where the list names a spelling of it, NamesASpellingOf, so a list
    /// never corrects more than it says.
    /// </summary>
    internal sealed class MatrixCorrectionList
    {
        internal const string RenameMarker = "rename:";

        internal const string CatchAllMarker = "catch-all:";

        internal const string SourceFileMarker = "source-file:";

        /// <summary>One workset spelling measured in the project's models, a line of its own, Q102.</summary>
        internal const string WorksetMarker = "workset:";

        /// <summary>
        /// UTF-8 that refuses a byte it cannot read. The default puts a replacement character
        /// in its place, and a measured spelling holding a no-break space would then ask a
        /// workset no model carries and say nothing.
        /// </summary>
        private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

        private MatrixCorrectionList(
            string listPath,
            bool missing,
            IList<SetRename> renames,
            IList<string[]> catchAlls,
            IList<SourceFileRule> sourceFiles,
            IList<string> worksets,
            string unread)
        {
            ListPath = listPath;
            Missing = missing;
            Renames = new ReadOnlyCollection<SetRename>(renames);
            CatchAlls = new ReadOnlyCollection<string[]>(catchAlls);
            SourceFiles = new ReadOnlyCollection<SourceFileRule>(sourceFiles);
            Worksets = new ReadOnlyCollection<string>(worksets);
            Unread = unread;
        }

        /// <summary>The full path the list was read from, or looked for where none is there.</summary>
        internal string ListPath { get; private set; }

        /// <summary>Whether no file is at that path, so nothing corrects the picked XML.</summary>
        internal bool Missing { get; private set; }

        /// <summary>The names wrong everywhere they appear, each with the name it should be.</summary>
        internal ReadOnlyCollection<SetRename> Renames { get; private set; }

        /// <summary>Each catch-all set, its name and the value its category must hold.</summary>
        internal ReadOnlyCollection<string[]> CatchAlls { get; private set; }

        /// <summary>The Source File rules of Q103.</summary>
        internal ReadOnlyCollection<SourceFileRule> SourceFiles { get; private set; }

        /// <summary>
        /// The workset spellings measured in the project's models, each once, which
        /// RevitWorksets.With puts beside the names inside Core for the corrections and for the
        /// judge of a set that found nothing.
        /// </summary>
        internal ReadOnlyCollection<string> Worksets { get; private set; }

        /// <summary>Whether the list is there, was read and holds no correction and no workset spelling, F116.</summary>
        internal bool HoldsNone
        {
            get
            {
                return !Missing
                    && Unread == null
                    && Renames.Count + CatchAlls.Count + SourceFiles.Count + Worksets.Count == 0;
            }
        }

        /// <summary>
        /// Whether one of the list's workset lines is a spelling of that value, the same but for
        /// its case. A workset value of the picked file is corrected only where it is, because a
        /// value is corrected only from what the list beside the file says, F116.
        /// </summary>
        internal bool NamesASpellingOf(string value)
        {
            foreach (string spelling in Worksets)
            {
                if (string.Equals(spelling, value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Why the list could not be read, or null where it was read whole or is not there.</summary>
        internal string Unread { get; private set; }

        /// <summary>
        /// The list beside that XML, read when the XML is picked. Not there, it is Missing. There
        /// and unreadable, it is Unread with why. Never a throw.
        /// </summary>
        internal static MatrixCorrectionList Beside(string xmlPath, CorrectionListSettings settings)
        {
            string path = settings.PathBeside(xmlPath);

            if (!File.Exists(path))
            {
                return new MatrixCorrectionList(
                    path, true, new List<SetRename>(), new List<string[]>(), new List<SourceFileRule>(), new List<string>(), null);
            }

            try
            {
                using (StreamReader reader = new StreamReader(path, StrictUtf8, true))
                {
                    return Read(reader, path);
                }
            }
            catch (DecoderFallbackException)
            {
                return Refused(path, "it is not UTF-8 text");
            }
            catch (IOException failed)
            {
                return Refused(path, "it could not be opened, " + failed.Message.TrimEnd('.'));
            }
            catch (UnauthorizedAccessException failed)
            {
                return Refused(path, "it could not be opened, " + failed.Message.TrimEnd('.'));
            }
        }

        /// <summary>
        /// A list read off that text, which came from that path. The first line it does not know
        /// makes the whole list unread, saying which line, so a typo in the list never corrects
        /// a file in part.
        /// </summary>
        internal static MatrixCorrectionList Read(TextReader text, string path)
        {
            List<SetRename> renames = new List<SetRename>();
            List<string[]> catchAlls = new List<string[]>();
            List<SourceFileRule> sourceFiles = new List<SourceFileRule>();
            List<string> worksets = new List<string>();
            string line;
            int number = 0;

            while ((line = text.ReadLine()) != null)
            {
                number++;

                if (line.Trim().Length == 0 || line[0] == '#')
                {
                    continue;
                }

                string[] parts = PartsAfter(line, RenameMarker);

                try
                {
                    if (parts != null && parts.Length == 2)
                    {
                        renames.Add(new SetRename(parts[0], parts[1]));
                        continue;
                    }
                }
                catch (ArgumentException refused)
                {
                    return Refused(path, "line " + number + ", \"" + line + "\", is a rename that cannot be used: " + refused.Message);
                }

                parts = PartsAfter(line, CatchAllMarker);

                if (parts != null && parts.Length == 2 && parts[0].Length > 0 && parts[1].Length > 0)
                {
                    catchAlls.Add(parts);
                    continue;
                }

                parts = PartsAfter(line, SourceFileMarker);

                if (parts != null && parts.Length >= 1 && parts[0].Length > 0)
                {
                    List<string> measured = new List<string>();

                    for (int i = 1; i < parts.Length; i++)
                    {
                        if (parts[i].Length > 0)
                        {
                            measured.Add(parts[i]);
                        }
                    }

                    sourceFiles.Add(new SourceFileRule(parts[0], measured));
                    continue;
                }

                string spelling = After(line, WorksetMarker);

                if (!string.IsNullOrEmpty(spelling))
                {
                    if (!worksets.Contains(spelling))
                    {
                        worksets.Add(spelling);
                    }

                    continue;
                }

                return Refused(path, "line " + number + ", \"" + line + "\", is not a correction this tool knows");
            }

            return new MatrixCorrectionList(path, false, renames, catchAlls, sourceFiles, worksets, null);
        }

        /// <summary>
        /// What the first MATRIX line says about the list, with its full path: what it holds, or
        /// that none is there, or why it could not be read, Q113, or that it holds none, F116.
        /// </summary>
        internal string Said()
        {
            if (Missing)
            {
                return "no correction was made to this file, because no list of corrections is beside it: "
                    + ListPath + " was looked for and is not there. Every set is built exactly as the file asks";
            }

            if (Unread != null)
            {
                return "NO CORRECTION WAS MADE TO THIS FILE, because the list of corrections beside it, "
                    + ListPath + ", could not be read: " + Unread + ". Every set is built exactly as the file asks";
            }

            if (HoldsNone)
            {
                return "no correction was made to this file, because the list of corrections beside it, "
                    + ListPath + ", holds none. Every set is built exactly as the file asks";
            }

            return "the corrections are read from " + ListPath + ", the list beside this file. It holds "
                + Counted(Renames.Count + CatchAlls.Count + SourceFiles.Count, "correction", "corrections") + ", "
                + Counted(Renames.Count, "rename", "renames") + ", "
                + Counted(CatchAlls.Count, "catch-all", "catch-alls") + " and "
                + Counted(SourceFiles.Count, "Source File rule", "Source File rules") + ", and "
                + Counted(Worksets.Count, "workset spelling", "workset spellings");
        }

        private static string Counted(int count, string one, string many)
        {
            return count + " " + (count == 1 ? one : many);
        }

        private static MatrixCorrectionList Refused(string path, string why)
        {
            return new MatrixCorrectionList(
                path, false, new List<SetRename>(), new List<string[]>(), new List<SourceFileRule>(), new List<string>(), why);
        }

        /// <summary>
        /// The parts after that marker and one space, split on a bar with a space each side,
        /// or null where the line does not start with it. NEVER TRIMMED, because two of the
        /// client's set names end in a space and a name is matched exactly, core.md.
        /// </summary>
        private static string[] PartsAfter(string line, string marker)
        {
            string rest = After(line, marker);

            return rest == null ? null : rest.Split(new[] { Bar }, StringSplitOptions.None);
        }

        /// <summary>Everything after that marker and one space, never trimmed, or null where the line does not start with it.</summary>
        private static string After(string line, string marker)
        {
            if (!line.StartsWith(marker + " ", StringComparison.Ordinal))
            {
                return null;
            }

            return line.Substring(marker.Length + 1);
        }

        /// <summary>What splits the parts of a line.</summary>
        private const string Bar = " | ";
    }
}
