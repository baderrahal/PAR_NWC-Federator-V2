using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Federator.Core.Diagnostics;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// The corrections one project needs, read off the list kept BESIDE the picked clash XML and
    /// named after it, Q113 answered B by Bader on 2026-10-04: the renames, the catch-all sets,
    /// the Source File rules of Q103 and the workset spellings measured in that project's
    /// models, and since F131 the also-ask lines Bader approves out of a silent miss. The tool serves many projects, so no project's list is inside it, and the code
    /// that applies a list, MatrixCorrections, names no set, folder, category or spelling.
    ///
    /// ONE FULL PATH, tested with File.Exists, CorrectionListSettings. A picked XML with no list
    /// there is corrected by nothing and the log says so. That is a project needing no
    /// correction as often as a list left behind, so it is said plainly and not as a fault.
    ///
    /// A LIST THAT CANNOT BE READ IS SAID, NEVER AN EMPTY ONE. An empty list corrects nothing and
    /// looks like a file that needed nothing, so a list that cannot be opened, is not UTF-8 text
    /// or holds a line this does not know is Unread, carrying why, and the picked file is
    /// corrected not at all and never in part. Never a throw. The file is read, and its lines
    /// split, by ListFile, the one way the team map beside the XML is read too, F131.
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
        /// A workset value a set asks and every other spelling it also accepts, one line, F131,
        /// FR-181: the correction SilentMisses drafts for a set that cannot reach a model of its
        /// own team, Q114 point 3, which Bader approves by copying it into the list. The tool never
        /// writes it into a list.
        /// </summary>
        internal const string AlsoAskMarker = "also-ask:";

        private MatrixCorrectionList(
            string listPath,
            bool missing,
            IList<SetRename> renames,
            IList<string[]> catchAlls,
            IList<SourceFileRule> sourceFiles,
            IList<string> worksets,
            IList<string[]> alsoAsks,
            string unread)
        {
            ListPath = listPath;
            Missing = missing;
            Renames = new ReadOnlyCollection<SetRename>(renames);
            CatchAlls = new ReadOnlyCollection<string[]>(catchAlls);
            SourceFiles = new ReadOnlyCollection<SourceFileRule>(sourceFiles);
            Worksets = new ReadOnlyCollection<string>(worksets);
            AlsoAsks = new ReadOnlyCollection<string[]>(alsoAsks);
            Unread = unread;
        }

        /// <summary>
        /// Each also-ask line, the value first and then every other spelling it accepts. No
        /// spelling is on two lines, so no two lines act on one condition, F131.
        /// </summary>
        internal ReadOnlyCollection<string[]> AlsoAsks { get; private set; }

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
        /// RevitWorksets.With puts beside the names inside Core for the case corrections of Q102.
        /// </summary>
        internal ReadOnlyCollection<string> Worksets { get; private set; }

        /// <summary>
        /// Every workset spelling the list says a model of the project carries, each once: its
        /// workset lines, then every spelling an also-ask line accepts beside its value. The value
        /// of an also-ask line is what a set asks, measured or not, so it is not among them. The
        /// judge of a set that found nothing is handed these beside the names inside Core,
        /// RevitWorksets.With, so every spelling the corrections ask is one it knows, F131 on the
        /// readers' finding against F116's rule.
        /// </summary>
        internal IList<string> Spellings
        {
            get
            {
                List<string> spellings = new List<string>(Worksets);

                foreach (string[] line in AlsoAsks)
                {
                    for (int i = 1; i < line.Length; i++)
                    {
                        if (!spellings.Contains(line[i]))
                        {
                            spellings.Add(line[i]);
                        }
                    }
                }

                return spellings;
            }
        }

        /// <summary>Whether the list is there, was read and holds no correction and no workset spelling, F116.</summary>
        internal bool HoldsNone
        {
            get
            {
                return !Missing
                    && Unread == null
                    && Renames.Count + CatchAlls.Count + SourceFiles.Count + Worksets.Count + AlsoAsks.Count == 0;
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
                    path, true, new List<SetRename>(), new List<string[]>(), new List<SourceFileRule>(), new List<string>(), new List<string[]>(), null);
            }

            return ListFile.Read(path, reader => Read(reader, path), why => Refused(path, why));
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
            List<string[]> alsoAsks = new List<string[]>();
            Dictionary<string, int> namedOn = new Dictionary<string, int>(StringComparer.Ordinal);
            string line;
            int number = 0;

            while ((line = text.ReadLine()) != null)
            {
                number++;

                if (ListFile.Skipped(line))
                {
                    continue;
                }

                string[] parts = ListFile.PartsAfter(line, RenameMarker);

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

                parts = ListFile.PartsAfter(line, CatchAllMarker);

                if (parts != null && parts.Length == 2 && parts[0].Length > 0 && parts[1].Length > 0)
                {
                    catchAlls.Add(parts);
                    continue;
                }

                parts = ListFile.PartsAfter(line, SourceFileMarker);

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

                string spelling = ListFile.After(line, WorksetMarker);

                if (!string.IsNullOrEmpty(spelling))
                {
                    if (!worksets.Contains(spelling))
                    {
                        worksets.Add(spelling);
                    }

                    continue;
                }

                parts = ListFile.PartsAfter(line, AlsoAskMarker);

                if (parts != null)
                {
                    string fault = AlsoAskFault(parts, alsoAsks, namedOn);

                    if (fault != null)
                    {
                        return Refused(path, "line " + number + ", \"" + line + "\", " + fault);
                    }

                    if (!alsoAsks.Exists(one => SameLine(one, parts)))
                    {
                        alsoAsks.Add(parts);

                        foreach (string named in parts)
                        {
                            namedOn[named] = number;
                        }
                    }

                    continue;
                }

                return Refused(path, "line " + number + ", \"" + line + "\", is not a correction this tool knows");
            }

            return new MatrixCorrectionList(path, false, renames, catchAlls, sourceFiles, worksets, alsoAsks, null);
        }

        /// <summary>
        /// What is wrong with an also-ask line, or null where nothing is. It needs a value and at
        /// least one other spelling, each named once. A spelling another line names is refused,
        /// unless the two lines are the same line written twice, which is kept once: two lines
        /// sharing a spelling would ask each value where the other is asked, or move the file on
        /// every run, F131.
        /// </summary>
        private static string AlsoAskFault(string[] parts, List<string[]> alsoAsks, IDictionary<string, int> namedOn)
        {
            List<string> seen = new List<string>();

            foreach (string named in parts)
            {
                if (named.Length == 0 || seen.Contains(named))
                {
                    return "is an also-ask that cannot be used: it needs the value a set asks and at least one other spelling"
                        + " to accept beside it, each named once";
                }

                seen.Add(named);
            }

            if (parts.Length < 2)
            {
                return "is an also-ask that cannot be used: it needs the value a set asks and at least one other spelling"
                    + " to accept beside it, each named once";
            }

            if (alsoAsks.Exists(one => SameLine(one, parts)))
            {
                return null;
            }

            foreach (string named in parts)
            {
                int earlier;

                if (namedOn.TryGetValue(named, out earlier))
                {
                    return "names " + named + ", which line " + earlier
                        + " names already. A value and every spelling it also accepts go on one line";
                }
            }

            return null;
        }

        private static bool SameLine(string[] one, string[] other)
        {
            if (one.Length != other.Length)
            {
                return false;
            }

            for (int i = 0; i < one.Length; i++)
            {
                if (!string.Equals(one[i], other[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
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

            // The also-asks are named only where the list holds one, so the line of a list
            // holding none reads as it did before F131.
            string kinds = AlsoAsks.Count == 0
                ? Words.Counted(Renames.Count, "rename", "renames") + ", "
                    + Words.Counted(CatchAlls.Count, "catch-all", "catch-alls") + " and "
                    + Words.Counted(SourceFiles.Count, "Source File rule", "Source File rules")
                : Words.Counted(Renames.Count, "rename", "renames") + ", "
                    + Words.Counted(CatchAlls.Count, "catch-all", "catch-alls") + ", "
                    + Words.Counted(SourceFiles.Count, "Source File rule", "Source File rules") + " and "
                    + Words.Counted(AlsoAsks.Count, "also-ask", "also-asks");

            return "the corrections are read from " + ListPath + ", the list beside this file. It holds "
                + Words.Counted(Renames.Count + CatchAlls.Count + SourceFiles.Count + AlsoAsks.Count, "correction", "corrections") + ", "
                + kinds + ", and "
                + Words.Counted(Worksets.Count, "workset spelling", "workset spellings");
        }

        private static MatrixCorrectionList Refused(string path, string why)
        {
            return new MatrixCorrectionList(
                path, false, new List<SetRename>(), new List<string[]>(), new List<SourceFileRule>(), new List<string>(), new List<string[]>(), why);
        }
    }
}
