using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Diagnostics;

namespace Federator.Core.Views
{
    /// <summary>One view, as it turned out.</summary>
    public sealed class ViewResult
    {
        internal ViewResult(string path, string shows, int hidden, string error)
        {
            Path = path;
            Shows = shows;
            Hidden = hidden;
            Error = error;
        }

        public string Path { get; private set; }

        /// <summary>The models it shows, by their codes.</summary>
        public string Shows { get; private set; }

        /// <summary>How many models it hides. Zero is a real answer where every model holds a clashing item.</summary>
        public int Hidden { get; private set; }

        /// <summary>Why it failed, or null where it did not.</summary>
        public string Error { get; private set; }

        public bool Failed
        {
            get { return Error != null; }
        }

        /// <summary>The one line the VIEWS BUILT block carries for this view.</summary>
        public string Line()
        {
            if (Failed)
            {
                return "VIEW     " + Path + "  FAILED, " + Error;
            }

            return "VIEW     " + Path + "  created, shows " + Shows
                + ", hides " + Hidden + (Hidden == 1 ? " model" : " models");
        }
    }

    /// <summary>
    /// The running total for the views of one group. The counts always agree with the
    /// lines, because both come from the same list, which is how SetBuildOutcome does it
    /// and for the same reason: a count kept beside a list drifts from it.
    ///
    /// Since F114's add-in pass the views are made fresh every run and the inventory
    /// removes the tool's earlier ones after, so there is no already there case here: a
    /// view is created or it failed with its reason.
    /// </summary>
    public sealed class ViewpointBuildOutcome
    {
        private readonly List<ViewResult> results = new List<ViewResult>();

        public ReadOnlyCollection<ViewResult> Results
        {
            get { return new ReadOnlyCollection<ViewResult>(results); }
        }

        /// <summary>Made by this run, marked and read back.</summary>
        public ViewResult AddCreated(string path, string shows, int hidden)
        {
            ViewResult result = new ViewResult(path, shows, hidden, null);
            results.Add(result);
            return result;
        }

        /// <summary>
        /// Threw, produced nothing, or did not read back. A reason is required, because a
        /// failure with no reason is what makes a RESULT block say something failed and
        /// name nothing.
        /// </summary>
        public ViewResult AddFailed(string path, string shows, string error)
        {
            ViewResult result = new ViewResult(
                path, shows, 0,
                string.IsNullOrEmpty(error) ? "UNKNOWN, it failed and no reason was given" : error);

            results.Add(result);
            return result;
        }

        public int CreatedCount
        {
            get { return CountWhere(false); }
        }

        public int FailedCount
        {
            get { return CountWhere(true); }
        }

        /// <summary>Whether anything at all was put in, which is what asks for another NWF save.</summary>
        public bool PutAnythingIn
        {
            get { return CreatedCount > 0; }
        }

        private int CountWhere(bool failed)
        {
            int count = 0;

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Failed == failed)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// One line for the window and the log, in the same words however the views were
        /// built. Nothing planned is a real answer and says so rather than reading as a
        /// step that silently did nothing.
        /// </summary>
        public string Summary()
        {
            if (results.Count == 0)
            {
                return "No view was planned for this group.";
            }

            return CreatedCount + " created"
                + (FailedCount > 0 ? ", " + FailedCount + " failed" : string.Empty)
                + ".";
        }

        /// <summary>
        /// The block. Every FAILED view is named, because each says something different.
        /// The created ones are named five deep and then counted, RunLog.KeptOfARepeat,
        /// the fault every other list in this log already guards against. The totals
        /// under it are counted off the same list the lines came from.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            int shown = 0;
            int notShown = 0;

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Failed)
                {
                    lines.Add(results[i].Line());
                    continue;
                }

                if (shown < RunLog.KeptOfARepeat)
                {
                    lines.Add(results[i].Line());
                    shown++;
                    continue;
                }

                notShown++;
            }

            if (notShown > 0)
            {
                lines.Add("VIEW     and " + notShown + " more created, counted and not listed");
            }

            lines.Add(string.Empty);
            lines.Add("views created     : " + CreatedCount);

            if (FailedCount > 0)
            {
                lines.Add("views that failed : " + FailedCount);
            }

            lines.Add("put into the document: " + CreatedCount + " created");
            return lines;
        }
    }
}
