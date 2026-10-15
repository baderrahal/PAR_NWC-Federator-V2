using System;
using System.Collections.Generic;
using Federator.Core.Clash;

namespace Federator.Core.Views
{
    /// <summary>
    /// The one folder every saved viewpoint this tool makes goes in, F186, Bader's answer 1 of 14 Oct 2026, Q174.
    /// It sits at the root of the saved viewpoints, is found by its name AND its marker comment, and is deleted with
    /// everything in it and built again on every run. Under it A, B and C, then one view for each test, directly.
    /// There is no team pair level. A folder or a view anywhere else is a person's and is never read for removal,
    /// changed or deleted, a view a person moved into this folder by hand included, which is deleted with it.
    /// </summary>
    public static class ToolViewFolder
    {
        /// <summary>The name of the top folder.</summary>
        public const string Name = "Clash Status";

        /// <summary>The first words of the marker comment on the top folder.</summary>
        public const string MarkerStart = "NWCF-AUTO";

        private const string RunField = " run=";

        /// <summary>The comment body put on the top folder, for the run with this id.</summary>
        public static string MarkerBody(string runId)
        {
            return MarkerStart + RunField + (runId ?? string.Empty);
        }

        /// <summary>
        /// Whether a comment is the marker. Its first words are the marker start and then the run field,
        /// so a coordinator's comment of any other words never reads as one.
        /// </summary>
        public static bool IsMarker(string comment)
        {
            if (comment == null)
            {
                return false;
            }

            string text = comment.Trim();
            string start = MarkerStart + RunField;

            return text.StartsWith(start, StringComparison.Ordinal) && text.IndexOf('\n') < 0;
        }

        /// <summary>The run id a marker carries, or null where the comment is not a marker.</summary>
        public static string RunIdOf(string comment)
        {
            if (!IsMarker(comment))
            {
                return null;
            }

            return comment.Trim().Substring((MarkerStart + RunField).Length);
        }

        /// <summary>
        /// The letter folder a test's views go in: A, B or C. A test with no priority is C, the same rule the
        /// pictures use, F168 and F175, so no test is left out of the tree for its priority.
        /// </summary>
        public static string LetterOf(ClashPriority priority)
        {
            switch (priority)
            {
                case ClashPriority.A: return "A";
                case ClashPriority.B: return "B";
                default: return "C";
            }
        }

        /// <summary>
        /// The nodes of a walk that are the tool's top folder or sit anywhere inside it, which is all the inventory
        /// and the tree checks are given. A folder or a view anywhere else is a person's and is never read for
        /// removal, so it cannot be changed or deleted by the run.
        /// </summary>
        public static List<ViewNode> NodesUnder(IList<ViewNode> walk)
        {
            List<ViewNode> mine = new List<ViewNode>();

            if (walk == null)
            {
                return mine;
            }

            foreach (ViewNode node in walk)
            {
                if (node == null)
                {
                    continue;
                }

                bool inside = node.Folders.Count > 0 && string.Equals(node.Folders[0], Name, StringComparison.Ordinal);
                bool itself = node.IsFolder && node.Folders.Count == 0 && string.Equals(node.Name, Name, StringComparison.Ordinal);

                if (inside || itself)
                {
                    mine.Add(node);
                }
            }

            return mine;
        }

        /// <summary>
        /// The line that answers whether deleting saved viewpoints changed the clash results, F186 task 6.5, read off
        /// the counts of tests and results before and after the delete. Where a count was not read it says UNKNOWN. A
        /// count that did not move proves the results were not removed, and says nothing of their statuses, which
        /// the snapshot of the NWF page compares.
        /// </summary>
        public static string DeletedWithoutChangingTheResultsLine(int testsBefore, int resultsBefore, int testsAfter, int resultsAfter)
        {
            if (testsBefore < 0 || resultsBefore < 0 || testsAfter < 0 || resultsAfter < 0)
            {
                return "VIEWS    PROOF the clash tests and results before and after the delete were not both read, so whether deleting"
                    + " the saved viewpoints left them as they were is UNKNOWN";
            }

            bool same = testsBefore == testsAfter && resultsBefore == resultsAfter;

            return "VIEWS    PROOF " + (same ? "the delete left the clash tests and results as they were: " : "THE DELETE CHANGED THE CLASH COUNTS: ")
                + testsBefore + " tests and " + resultsBefore + " results before, " + testsAfter + " tests and " + resultsAfter
                + " results after. The counts say nothing of statuses, which the snapshot of a weekly run compares";
        }

        /// <summary>The three letter folders, the only folders under the top one.</summary>
        public static string[] Letters
        {
            get { return new[] { "A", "B", "C" }; }
        }
    }
}
