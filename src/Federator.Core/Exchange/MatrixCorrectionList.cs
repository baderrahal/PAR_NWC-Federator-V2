using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// The corrections this project needs, read off the list shipped inside Core,
    /// matrix-corrections.txt, Q104. The renames, the catch-all sets and the Source File rules,
    /// as data, so the code names no set. The list itself holds two set names and four
    /// categories of the client's matrix. Bader answered Q113 on 2026-10-04, B: the list
    /// becomes a plain file kept beside the picked XML, which F116 carries out after its fix
    /// attempt 2 and before it merges, so until then it ships in Core.
    ///
    /// A LIST THAT CANNOT BE READ IS SAID, NEVER AN EMPTY ONE. An empty list corrects nothing
    /// and looks like a file that needed nothing, so a list that is missing or holds a line
    /// this does not know is Unread, carrying why, and the picked file is corrected not at all
    /// and never in part.
    /// </summary>
    internal sealed class MatrixCorrectionList
    {
        /// <summary>The resource the list lives in, named once.</summary>
        internal const string ResourceName = "Federator.Core.Exchange.matrix-corrections.txt";

        internal const string RenameMarker = "rename:";

        internal const string CatchAllMarker = "catch-all:";

        internal const string SourceFileMarker = "source-file:";

        private static readonly object Gate = new object();
        private static MatrixCorrectionList shipped;

        private MatrixCorrectionList(
            IList<SetRename> renames, IList<string[]> catchAlls, IList<SourceFileRule> sourceFiles, string unread)
        {
            Renames = new ReadOnlyCollection<SetRename>(renames);
            CatchAlls = new ReadOnlyCollection<string[]>(catchAlls);
            SourceFiles = new ReadOnlyCollection<SourceFileRule>(sourceFiles);
            Unread = unread;
        }

        /// <summary>The names wrong everywhere they appear, each with the name it should be.</summary>
        internal ReadOnlyCollection<SetRename> Renames { get; private set; }

        /// <summary>Each catch-all set, its name and the value its category must hold.</summary>
        internal ReadOnlyCollection<string[]> CatchAlls { get; private set; }

        /// <summary>The Source File rules of Q103.</summary>
        internal ReadOnlyCollection<SourceFileRule> SourceFiles { get; private set; }

        /// <summary>Why the list could not be read, or null where it was read whole.</summary>
        internal string Unread { get; private set; }

        /// <summary>The list inside this build, read once.</summary>
        internal static MatrixCorrectionList Shipped
        {
            get
            {
                lock (Gate)
                {
                    if (shipped == null)
                    {
                        shipped = ReadShipped();
                    }

                    return shipped;
                }
            }
        }

        /// <summary>
        /// A list read off that text. The first line it does not know makes the whole list
        /// unread, saying which line, so a typo in the list never corrects a file in part.
        /// </summary>
        internal static MatrixCorrectionList Read(TextReader text)
        {
            List<SetRename> renames = new List<SetRename>();
            List<string[]> catchAlls = new List<string[]>();
            List<SourceFileRule> sourceFiles = new List<SourceFileRule>();
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
                    return Refused("line " + number + ", \"" + line + "\", is a rename that cannot be used: " + refused.Message);
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

                return Refused("line " + number + ", \"" + line + "\", is not a correction this tool knows");
            }

            return new MatrixCorrectionList(renames, catchAlls, sourceFiles, null);
        }

        private static MatrixCorrectionList ReadShipped()
        {
            Stream stream = typeof(MatrixCorrectionList).Assembly.GetManifestResourceStream(ResourceName);

            if (stream == null)
            {
                return Refused("the list " + ResourceName + " is not inside this build");
            }

            using (stream)
            using (StreamReader reader = new StreamReader(stream))
            {
                return Read(reader);
            }
        }

        private static MatrixCorrectionList Refused(string why)
        {
            return new MatrixCorrectionList(new List<SetRename>(), new List<string[]>(), new List<SourceFileRule>(), why);
        }

        /// <summary>
        /// The parts after that marker and one space, split on a bar with a space each side,
        /// or null where the line does not start with it. NEVER TRIMMED, because two of the
        /// client's set names end in a space and a name is matched exactly, core.md.
        /// </summary>
        private static string[] PartsAfter(string line, string marker)
        {
            if (!line.StartsWith(marker + " ", StringComparison.Ordinal))
            {
                return null;
            }

            return line.Substring(marker.Length + 1).Split(new[] { Bar }, StringSplitOptions.None);
        }

        /// <summary>What splits the parts of a line.</summary>
        private const string Bar = " | ";
    }
}
