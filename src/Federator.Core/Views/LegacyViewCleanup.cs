using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Federator.Core.Views
{
    /// <summary>
    /// The one time clean up of an NWF the tool wrote before F186, Bader's answer 1 of Q174 and his order of 14 Oct
    /// 2026, task 6.3. The old tool tree sat in root folders named A, B and C, or by the no priority word, with a
    /// team pair folder under each, an Over 150mm folder under a pair that carries it, and a view for each test.
    ///
    /// A ROOT FOLDER IS DELETED ONLY WHERE EVERY ITEM INSIDE IT IS THE TOOL'S OWN NAMING, and nothing else is
    /// ever touched. Inside it: a folder directly under it is a team pair folder, two team names joined by the
    /// pair separator, the teams being the map's, the unknown team and the group's codes. A folder under that is
    /// the size folder. A view under a pair folder or its size folder is named exactly as one of the tests the
    /// built in matrix or the document holds, or as F85 named a per clash viewpoint, the test, the name separator,
    /// the word Clash and digits. One item that is none of these, a view of a coordinator's, a folder of their own,
    /// a view at another depth, leaves the whole root folder where it is, and the plan names it and says why.
    /// A root folder that holds nothing is left too, since nothing proves it is the tool's.
    /// </summary>
    public sealed class LegacyViewCleanup
    {
        private readonly List<LegacyRoot> roots = new List<LegacyRoot>();

        private LegacyViewCleanup()
        {
        }

        /// <summary>One root folder, with what the rule decided.</summary>
        public ReadOnlyCollection<LegacyRoot> Roots
        {
            get { return new ReadOnlyCollection<LegacyRoot>(roots); }
        }

        /// <summary>The root folders to delete, each with everything under it in the one call.</summary>
        public IList<LegacyRoot> ToDelete()
        {
            List<LegacyRoot> take = new List<LegacyRoot>();

            foreach (LegacyRoot root in roots)
            {
                if (root.Delete)
                {
                    take.Add(root);
                }
            }

            return take;
        }

        /// <summary>The plan over a walk of the saved viewpoints, the root folders named A, B, C or the no priority word judged.</summary>
        public static LegacyViewCleanup Plan(
            IList<ViewNode> walk,
            ICollection<string> testNames,
            ICollection<string> teams,
            ViewpointSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            LegacyViewCleanup plan = new LegacyViewCleanup();

            if (walk == null)
            {
                return plan;
            }

            foreach (ViewNode node in walk)
            {
                if (node == null || !node.IsFolder || node.Folders.Count != 0 || !IsALegacyRootName(node.Name, settings))
                {
                    continue;
                }

                plan.roots.Add(Judge(node, walk, testNames ?? new string[0], teams ?? new string[0], settings));
            }

            return plan;
        }

        /// <summary>The report lines, one for each root folder, naming the counts or the item that kept it.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (roots.Count == 0)
            {
                lines.Add("no root folder named A, B, C or the no priority word, so there is no tree of an earlier version to clean up");
                return lines;
            }

            foreach (LegacyRoot root in roots)
            {
                lines.Add(root.Line());
            }

            return lines;
        }

        private static bool IsALegacyRootName(string name, ViewpointSettings settings)
        {
            foreach (string letter in ToolViewFolder.Letters)
            {
                if (string.Equals(name, letter, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return string.Equals(name, settings.NoPriorityFolder, StringComparison.Ordinal);
        }

        private static LegacyRoot Judge(
            ViewNode root, IList<ViewNode> walk, ICollection<string> testNames, ICollection<string> teams, ViewpointSettings settings)
        {
            int views = 0;
            int folders = 0;
            string why = null;
            int items = 0;

            foreach (ViewNode node in walk)
            {
                if (node == null || node.Folders.Count == 0 || !string.Equals(node.Folders[0], root.Name, StringComparison.Ordinal))
                {
                    continue;
                }

                items++;
                string reason = WhyNotTheTools(node, testNames, teams, settings);

                if (reason != null)
                {
                    if (why == null)
                    {
                        why = node + " is not the tool's own naming, " + reason;
                    }

                    continue;
                }

                if (node.IsFolder)
                {
                    folders++;
                }
                else
                {
                    views++;
                }
            }

            if (items == 0)
            {
                return new LegacyRoot(root, false, 0, 0, "it holds nothing, and nothing proves it is the tool's");
            }

            return new LegacyRoot(root, why == null, views, folders, why);
        }

        /// <summary>Null where the node is the tool's own naming at its depth, or why it is not.</summary>
        private static string WhyNotTheTools(ViewNode node, ICollection<string> testNames, ICollection<string> teams, ViewpointSettings settings)
        {
            int depth = node.Folders.Count;

            if (node.IsFolder)
            {
                if (depth == 1)
                {
                    return IsATeamPairFolder(node.Name, teams, settings)
                        ? null
                        : "a folder directly under the root that is not two team names joined by \"" + settings.PairSeparator + "\"";
                }

                if (depth == 2)
                {
                    return string.Equals(node.Name, settings.SubGroupFolderName(), StringComparison.Ordinal)
                        ? null
                        : "a folder under a team pair folder that is not the size folder \"" + settings.SubGroupFolderName() + "\"";
                }

                return "a folder at depth " + (depth + 1).ToString(CultureInfo.InvariantCulture) + ", which the old tree never made";
            }

            if (depth != 2 && depth != 3)
            {
                return "a view at depth " + (depth + 1).ToString(CultureInfo.InvariantCulture)
                    + ", where the old tree put its views under a team pair folder or its size folder";
            }

            return IsATestsView(node.Name, testNames, settings)
                ? null
                : "a view whose name is no test of the built in matrix or the document";
        }

        private static bool IsATeamPairFolder(string folder, ICollection<string> teams, ViewpointSettings settings)
        {
            if (string.IsNullOrEmpty(settings.PairSeparator) || folder == null)
            {
                return false;
            }

            int at = folder.IndexOf(settings.PairSeparator, StringComparison.Ordinal);

            if (at <= 0)
            {
                return false;
            }

            string first = folder.Substring(0, at);
            string second = folder.Substring(at + settings.PairSeparator.Length);

            return Holds(teams, first) && Holds(teams, second);
        }

        private static bool IsATestsView(string name, ICollection<string> testNames, ViewpointSettings settings)
        {
            if (name == null)
            {
                return false;
            }

            if (Holds(testNames, name))
            {
                return true;
            }

            string marker = settings.NameSeparator + settings.LegacyClashPrefix;
            int at = name.LastIndexOf(marker, StringComparison.Ordinal);

            if (at <= 0 || string.IsNullOrEmpty(settings.LegacyClashPrefix))
            {
                return false;
            }

            string digits = name.Substring(at + marker.Length);

            if (digits.Length == 0)
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

            return Holds(testNames, name.Substring(0, at));
        }

        private static bool Holds(ICollection<string> set, string word)
        {
            foreach (string item in set)
            {
                if (string.Equals(item, word, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>One root folder of an earlier version's tree and what the clean up decided for it.</summary>
    public sealed class LegacyRoot
    {
        internal LegacyRoot(ViewNode node, bool delete, int views, int folders, string why)
        {
            Node = node;
            Delete = delete;
            Views = views;
            Folders = folders;
            Why = why;
        }

        /// <summary>The root folder.</summary>
        public ViewNode Node { get; private set; }

        /// <summary>Whether every item inside is the tool's naming, so the folder is deleted with all of them.</summary>
        public bool Delete { get; private set; }

        /// <summary>The views inside, counted where all of them matched.</summary>
        public int Views { get; private set; }

        /// <summary>The folders inside it, counted where all of them matched.</summary>
        public int Folders { get; private set; }

        /// <summary>Why it is left, or null where it is deleted.</summary>
        public string Why { get; private set; }

        /// <summary>The words for the log.</summary>
        public string Line()
        {
            return Delete
                ? "root folder " + Node.Name + ": every item is the tool's own naming, deleted with its "
                    + Views.ToString(CultureInfo.InvariantCulture) + " views and "
                    + Folders.ToString(CultureInfo.InvariantCulture) + " folders"
                : "root folder " + Node.Name + ": left as it is, " + Why;
        }
    }
}
