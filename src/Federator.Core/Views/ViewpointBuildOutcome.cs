using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Views
{
    /// <summary>One viewpoint, as it turned out.</summary>
    public sealed class ViewResult
    {
        internal ViewResult(string path, string shows, int hidden, bool present, string error)
        {
            Path = path;
            Shows = shows;
            Hidden = hidden;
            Present = present;
            Error = error;
        }

        public string Path { get; private set; }

        /// <summary>The discipline code it shows.</summary>
        public string Shows { get; private set; }

        /// <summary>How many disciplines it hides. Zero is a real answer in a single discipline group.</summary>
        public int Hidden { get; private set; }

        /// <summary>Already at that path and left exactly as it was. Never in the created count.</summary>
        public bool Present { get; private set; }

        /// <summary>Why it failed, or null where it did not.</summary>
        public string Error { get; private set; }

        public bool Failed
        {
            get { return Error != null; }
        }

        /// <summary>The one line the VIEWS block carries for this viewpoint.</summary>
        public string Line()
        {
            if (Failed)
            {
                return "VIEW     " + Path + "  FAILED, " + Error;
            }

            if (Present)
            {
                return "VIEW     " + Path + "  already there, left alone, not made again";
            }

            return "VIEW     " + Path + "  created, shows " + Shows
                + ", hides " + Hidden + (Hidden == 1 ? " discipline" : " disciplines");
        }
    }

    /// <summary>
    /// The running total for the viewpoints of one group. The counts always agree with the
    /// lines, because both come from the same list, which is how SetBuildOutcome does it
    /// and for the same reason: a count kept beside a list drifts from it.
    ///
    /// A viewpoint already at its path is left exactly as it is and counted as already
    /// there, never as created. That is the rule F28 set for sets and it is here for the
    /// same reason: a second copy at one path leaves the tree holding both, and whichever
    /// came first is the one anything resolving that path will find.
    /// </summary>
    public sealed class ViewpointBuildOutcome
    {
        private readonly List<ViewResult> results = new List<ViewResult>();

        public ReadOnlyCollection<ViewResult> Results
        {
            get { return new ReadOnlyCollection<ViewResult>(results); }
        }

        /// <summary>Made by this run.</summary>
        public ViewResult AddCreated(string path, string shows, int hidden)
        {
            ViewResult result = new ViewResult(path, shows, hidden, false, null);
            results.Add(result);
            return result;
        }

        /// <summary>Already at that path from an earlier run, and left exactly as it was.</summary>
        public ViewResult AddAlreadyPresent(string path, string shows, int hidden)
        {
            ViewResult result = new ViewResult(path, shows, hidden, true, null);
            results.Add(result);
            return result;
        }

        /// <summary>
        /// Threw or produced nothing. A reason is required, because a failure with no
        /// reason is what makes a RESULT block say something failed and name nothing.
        /// </summary>
        public ViewResult AddFailed(string path, string shows, string error)
        {
            ViewResult result = new ViewResult(
                path, shows, 0, false,
                string.IsNullOrEmpty(error) ? "UNKNOWN, it failed and no reason was given" : error);

            results.Add(result);
            return result;
        }

        public int CreatedCount
        {
            get { return CountWhere(false, false); }
        }

        public int AlreadyPresentCount
        {
            get { return CountWhere(true, false); }
        }

        public int FailedCount
        {
            get { return CountWhere(false, true); }
        }

        /// <summary>Whether anything at all was put in, which is what asks for another NWF save.</summary>
        public bool PutAnythingIn
        {
            get { return CreatedCount > 0; }
        }

        private int CountWhere(bool present, bool failed)
        {
            int count = 0;

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Failed == failed && (failed || results[i].Present == present))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// One line for the window and the log, in the same words however the viewpoints
        /// were built. Nothing planned is a real answer and says so rather than reading as
        /// a step that silently did nothing.
        /// </summary>
        public string Summary()
        {
            if (results.Count == 0)
            {
                return "No viewpoint was planned for this group.";
            }

            return CreatedCount + " created, "
                + AlreadyPresentCount + " already there"
                + (FailedCount > 0 ? ", " + FailedCount + " failed" : string.Empty)
                + ".";
        }

        /// <summary>
        /// One line per viewpoint, then the totals, counted off the same list the lines
        /// came from so the two cannot disagree. This is the VIEWS block and it is shaped
        /// on the SETS block on purpose.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < results.Count; i++)
            {
                lines.Add(results[i].Line());
            }

            lines.Add(string.Empty);
            lines.Add("views created     : " + CreatedCount);

            if (AlreadyPresentCount > 0)
            {
                lines.Add("already there     : " + AlreadyPresentCount + ", left alone, not made again");
            }

            if (FailedCount > 0)
            {
                lines.Add("views that failed : " + FailedCount);
            }

            lines.Add("put into the document: " + CreatedCount + " created, "
                + AlreadyPresentCount + " already there and left alone");

            return lines;
        }
    }
}
