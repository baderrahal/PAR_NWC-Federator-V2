using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Naming;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// Which built in matrix pack a run reads, F166, Bader's answer B to Q164 of 14 Oct 2026. The
    /// pack is chosen by the project code read off part 1 of the NWC file names, such as 1104 in
    /// 1104-PAR-..., and each pack carries its own code, MatrixPack.ProjectCode. There is no drop
    /// down and no picker.
    ///
    /// A run stops before anything is opened, and says why in simple words, where
    ///  - a file name has no project code part,
    ///  - the files of one group carry different project codes,
    ///  - the groups of the run carry different codes, since one run reads one pack,
    ///  - no pack carries the code, or
    ///  - more than one pack carries it.
    /// The words are made here once so the window, the log and the tests read the same.
    /// </summary>
    public sealed class PackChoice
    {
        private PackChoice(MatrixPack pack, string why, IList<string> codes)
        {
            Pack = pack;
            Why = why;
            Codes = new ReadOnlyCollection<string>(codes);
        }

        /// <summary>The pack the run reads, or null where the run stops.</summary>
        public MatrixPack Pack { get; private set; }

        /// <summary>Why the run stops, in simple words, or null where a pack was chosen.</summary>
        public string Why { get; private set; }

        /// <summary>Every project code found in the names, each once, in the order met.</summary>
        public IList<string> Codes { get; private set; }

        public bool Stopped
        {
            get { return Pack == null; }
        }

        /// <summary>The sentence for a code no pack carries, in Bader's words.</summary>
        public static string NoPackSentence(string code)
        {
            return "No matrix pack for project code " + code + ". Send the clash matrix and a session will add it.";
        }

        /// <summary>One group of the run, its label and the paths or names of its files.</summary>
        public static KeyValuePair<string, IList<string>> Group(string label, IList<string> files)
        {
            return new KeyValuePair<string, IList<string>>(label, files);
        }

        /// <summary>The choice for one group of files, such as the NWFs picked or the file open.</summary>
        public static PackChoice For(string label, IList<string> files, IList<MatrixPack> packs, ContainerNameSettings settings)
        {
            return For(new List<KeyValuePair<string, IList<string>>> { Group(label, files) }, packs, settings);
        }

        /// <summary>
        /// The choice for every group of a run. Nothing is read off the disk, only the names, so
        /// the answer is known before any file is opened.
        /// </summary>
        public static PackChoice For(
            IList<KeyValuePair<string, IList<string>>> groups, IList<MatrixPack> packs, ContainerNameSettings settings)
        {
            if (settings == null)
            {
                settings = new ContainerNameSettings();
            }

            List<string> all = new List<string>();
            List<string> unreadable = new List<string>();
            int names = 0;

            if (groups != null)
            {
                foreach (KeyValuePair<string, IList<string>> group in groups)
                {
                    List<string> inGroup = new List<string>();

                    foreach (string file in group.Value ?? new List<string>())
                    {
                        names++;
                        ParsedContainerName parsed = ContainerName.Parse(file, settings);

                        if (!parsed.IsReadable)
                        {
                            unreadable.Add(System.IO.Path.GetFileName(file ?? string.Empty));
                            continue;
                        }

                        AddOnce(inGroup, parsed.Project);
                        AddOnce(all, parsed.Project);
                    }

                    if (inGroup.Count > 1 && unreadable.Count == 0)
                    {
                        return new PackChoice(
                            null,
                            "The NWC files of the group " + group.Key + " carry different project codes, "
                                + Joined(inGroup) + ". A group has to carry one. The run is stopped before anything is opened.",
                            all);
                    }
                }
            }

            if (names == 0)
            {
                return new PackChoice(
                    null, "No NWC file name was given, so no project code could be read. The run is stopped before anything is opened.", all);
            }

            if (unreadable.Count > 0)
            {
                return new PackChoice(
                    null,
                    "The project code, part 1 of the name, cannot be read from " + Joined(unreadable)
                        + ". The run is stopped before anything is opened.",
                    all);
            }

            if (all.Count > 1)
            {
                return new PackChoice(
                    null,
                    "The files of this run carry different project codes, " + Joined(all)
                        + ". One run reads one matrix pack. Run one project at a time. The run is stopped before anything is opened.",
                    all);
            }

            string code = all[0];
            List<string> matching = new List<string>();

            foreach (MatrixPack pack in packs ?? new List<MatrixPack>())
            {
                if (pack.ProjectCode != null && string.Equals(pack.ProjectCode, code, StringComparison.OrdinalIgnoreCase))
                {
                    matching.Add(pack.Name);
                }
            }

            if (matching.Count == 0)
            {
                return new PackChoice(null, NoPackSentence(code) + " The code found is " + code + ". The run is stopped before anything is opened.", all);
            }

            if (matching.Count > 1)
            {
                return new PackChoice(
                    null,
                    "More than one matrix pack carries the project code " + code + ", " + Joined(matching)
                        + ". A session has to remove one. The run is stopped before anything is opened.",
                    all);
            }

            foreach (MatrixPack pack in packs)
            {
                if (pack.Name == matching[0])
                {
                    return new PackChoice(pack, null, all);
                }
            }

            return new PackChoice(null, NoPackSentence(code), all);
        }

        private static void AddOnce(List<string> into, string code)
        {
            foreach (string seen in into)
            {
                if (string.Equals(seen, code, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            into.Add(code);
        }

        private static string Joined(IList<string> items)
        {
            return string.Join(", ", new List<string>(items).ToArray());
        }
    }
}
