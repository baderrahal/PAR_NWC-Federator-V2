using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using Federator.Core.Clash;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// A MATRIX PACK IS ONE PROJECT'S CLASH MATRIX BUILT INTO THE TOOL, F154 and F155, Bader's decision of
    /// 2026-10-13, Q162-4 and Q162-5: the clash matrix is built into the tool, later projects get their own,
    /// and Bader shares the matrix for each new project and a session adds it. A pack is one folder under
    /// src\Federator.Core\Packs, every file in it an embedded resource named
    /// Federator.Core.Packs\the folder\the file, by the one wildcard of Federator.Core.csproj, so a project is
    /// added by adding a folder and no code names any one project's file. The packs are found by listing the
    /// resource names under that prefix, and a pack's name is its folder's.
    ///
    /// The two CSVs of a pack are read by their file names here, the same names the samples carry, through
    /// PriorityMap.Read and ByDesignPairs.Read, with no change to either rule.
    /// </summary>
    public sealed class MatrixPack
    {
        /// <summary>What every pack's resource name starts with, then a separator, the pack, a separator and the file.</summary>
        public const string Prefix = "Federator.Core.Packs";

        /// <summary>The clash priorities of the pack's tests, test_name, left_set, right_set and priority, F83.</summary>
        public const string PriorityFile = "clash-priority-map.csv";

        /// <summary>The by design pairs of the pack, left_set, right_set and reason, F72b.</summary>
        public const string ByDesignFile = "by-design-pairs.csv";

        private readonly Assembly assembly;
        private readonly Dictionary<string, string> resources = new Dictionary<string, string>(StringComparer.Ordinal);

        private MatrixPack(Assembly assembly, string name)
        {
            this.assembly = assembly;
            Name = name;
        }

        /// <summary>The pack's name, its folder's, such as the project code.</summary>
        public string Name { get; private set; }

        /// <summary>The files the pack holds, in Ordinal order.</summary>
        public IList<string> Files
        {
            get
            {
                List<string> names = new List<string>(resources.Keys);
                names.Sort(StringComparer.Ordinal);
                return new ReadOnlyCollection<string>(names);
            }
        }

        /// <summary>Every pack built into this tool, by name in Ordinal order.</summary>
        public static IList<MatrixPack> All()
        {
            return All(typeof(MatrixPack).Assembly.GetManifestResourceNames(), typeof(MatrixPack).Assembly);
        }

        /// <summary>The packs a list of resource names holds, a name not under the prefix or naming no file left out.</summary>
        internal static IList<MatrixPack> All(IEnumerable<string> resourceNames, Assembly assembly)
        {
            SortedDictionary<string, MatrixPack> packs = new SortedDictionary<string, MatrixPack>(StringComparer.Ordinal);

            foreach (string resource in resourceNames)
            {
                if (resource == null || resource.Length <= Prefix.Length + 1 || !resource.StartsWith(Prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                char after = resource[Prefix.Length];

                if (after != '\\' && after != '/')
                {
                    continue;
                }

                string rest = resource.Substring(Prefix.Length + 1);
                int split = rest.IndexOfAny(new[] { '\\', '/' });

                if (split <= 0 || split == rest.Length - 1)
                {
                    continue;
                }

                string name = rest.Substring(0, split);
                string file = rest.Substring(split + 1);
                MatrixPack pack;

                if (!packs.TryGetValue(name, out pack))
                {
                    pack = new MatrixPack(assembly, name);
                    packs.Add(name, pack);
                }

                pack.resources[file] = resource;
            }

            return new List<MatrixPack>(packs.Values);
        }

        /// <summary>
        /// The one pack a run uses, or null with why. With one pack built in it is that pack. With none or with
        /// two or more there is none, since no picker chooses between packs yet, Q164, and the reason names them.
        /// </summary>
        public static MatrixPack TheOne(IList<MatrixPack> packs, out string whyNone)
        {
            if (packs == null || packs.Count == 0)
            {
                whyNone = "no matrix pack is built into this tool";
                return null;
            }

            if (packs.Count > 1)
            {
                List<string> names = new List<string>();

                foreach (MatrixPack pack in packs)
                {
                    names.Add(pack.Name);
                }

                whyNone = packs.Count + " matrix packs are built into this tool, " + string.Join(", ", names.ToArray())
                    + ", and nothing chooses between them yet";
                return null;
            }

            whyNone = null;
            return packs[0];
        }

        /// <summary>Whether the pack holds a file of that name, Ordinal.</summary>
        public bool Holds(string file)
        {
            return file != null && resources.ContainsKey(file);
        }

        /// <summary>What the log line about the pack starts with, PACK and four spaces as the other prefixes are padded.</summary>
        public const string LinePrefix = "PACK    ";

        /// <summary>
        /// The one log line a run writes about its pack: the pack's name and its files, or that no pack was read
        /// and why, so the log always says which matrix the run read.
        /// </summary>
        public static string Line(MatrixPack pack, string whyNone)
        {
            if (pack == null)
            {
                return LinePrefix + " none read, " + (whyNone ?? "why is UNKNOWN");
            }

            List<string> files = new List<string>(pack.Files);
            return LinePrefix + " the built in matrix pack " + pack.Name + ", holding " + string.Join(", ", files.ToArray());
        }

        /// <summary>The words a log line names one of its files by: the built in matrix pack, its name and the file.</summary>
        public string Describe(string file)
        {
            return "the built in matrix pack " + Name + ", " + file;
        }

        /// <summary>A file of the pack as text, in the encoding its byte order mark gives and UTF-8 where it has none.</summary>
        public string ReadText(string file)
        {
            string resource;

            if (file == null || !resources.TryGetValue(file, out resource))
            {
                throw new FileNotFoundException("The " + Describe(file ?? string.Empty) + " is not built in.", file);
            }

            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("The " + Describe(file) + " could not be opened.", file);
                }

                using (StreamReader reader = new StreamReader(stream, new UTF8Encoding(false), true))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>The clash priorities of the pack, through PriorityMap.Read, the rule unchanged.</summary>
        public PriorityMap Priorities()
        {
            return PriorityMap.Read(ReadText(PriorityFile), Describe(PriorityFile));
        }

        /// <summary>The by design pairs of the pack, through ByDesignPairs.Read, the rule unchanged.</summary>
        public ByDesignPairs ByDesign()
        {
            return ByDesignPairs.Read(ReadText(ByDesignFile), Describe(ByDesignFile));
        }
    }
}
