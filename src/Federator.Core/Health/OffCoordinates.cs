using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Diagnostics;

namespace Federator.Core.Health
{
    /// <summary>
    /// The models of one group that are NOT ON THE SAME SHARED COORDINATES as its reference
    /// model, Bader's answer to Q99 and Q100 on 2026-10-04, each said in one line naming its
    /// file, its shared site and its distance from the reference in X, Y and Z.
    ///
    /// A model is not on the same coordinates when it names Internal as its shared site,
    /// which is Revit's own origin, or when it sits more than the far model setting from the
    /// reference model, measured as the straight line of its X, Y and Z offsets.
    ///
    /// IN A GROUP HOLDING ONE, ONLY THE CLASH IS SKIPPED, and only where this run would have
    /// run a clash test in it, SkipsTheClash. The tests whose sides both find something are
    /// created and none is run, no viewpoint and no clash report is made, and the group ends
    /// PARTIAL with ClashSkippedReason. The rule is read on every run, so once the models are
    /// exported again the next run clashes the group with the tests already saved in its NWF.
    /// A setting switches the rule off.
    /// </summary>
    public sealed class OffCoordinates
    {
        /// <summary>The group's reason, in Bader's words.</summary>
        public const string ClashSkippedReason = "clash skipped, models not on the same shared coordinates";

        /// <summary>
        /// How a note's file name ends, after the name of the NWD it sits beside or of the
        /// report it stands in for in the Clash Report folder.
        /// </summary>
        public const string NoteEnding = " clash skipped.txt";

        /// <summary>
        /// The words for the tests of a skipped group, F77: a test whose side finds nothing is
        /// not created at all, so not every test of the file is in the NWF. The engine's CLASH
        /// line, the ALIGNMENT heading, the note, the list and RESULT all read this one sentence.
        /// </summary>
        public const string TestsCreatedNoneRun = "The tests whose sides both find something are created and none is run";

        internal OffCoordinates(string reference, IList<string> models, int notJudged, int modelsRead)
        {
            Reference = reference;
            Models = new ReadOnlyCollection<string>(models);
            NotJudged = notJudged;
            ModelsRead = modelsRead;
        }

        /// <summary>The reference model the distances are measured from, as the block names it, or null where no model could be placed.</summary>
        public string Reference { get; private set; }

        /// <summary>One line per model not on the same shared coordinates, the line the log, the note and the run's list all carry.</summary>
        public IList<string> Models { get; private set; }

        /// <summary>
        /// How many models this rule could not judge: not named above, and either their
        /// placement or their site UNKNOWN. A site that was not read could be Internal and a
        /// placement that was not read could be anywhere, so neither is a pass.
        /// </summary>
        internal int NotJudged { get; private set; }

        /// <summary>How many models were read at all. None read is a group nothing was judged in.</summary>
        internal int ModelsRead { get; private set; }

        /// <summary>Whether the group holds any such model.</summary>
        public bool Any
        {
            get { return Models.Count > 0; }
        }

        /// <summary>
        /// Whether this group's clash is skipped: the rule on, a model not on the same shared
        /// coordinates, and a clash test this run would have run in it. A group with nothing
        /// to clash, no XML and no test saved, an XML of sets alone, or one discipline, has no
        /// clash to skip and is judged as before, the breaker's second finding at c5d8aa8, Q70's
        /// failure for a model on Internal included, AlignmentCheck.WhyItFailsTheGroup.
        /// </summary>
        public bool SkipsTheClash(bool ruleOn, bool runsATest)
        {
            return ruleOn && runsATest && Any;
        }

        /// <summary>
        /// Why a note an earlier run left for a group whose clash this run did not skip stays,
        /// or null where it goes. It goes only when this run judged every model of the group
        /// and either found none of them off or clashed the group. A model whose placement or
        /// site is UNKNOWN, a group where no model was read, or a read that threw, judged null,
        /// keeps it, because an unknown is not a pass. A group whose model is still on Internal
        /// or still far, with no clash test run in it, keeps it too, because what the note says
        /// is still so, which attempt 2 deleted.
        /// </summary>
        public static string EarlierNoteKeptBecause(OffCoordinates judged, bool ruleOn, bool runsATest)
        {
            if (judged == null || judged.ModelsRead == 0 || judged.NotJudged > 0)
            {
                return "this run did not judge every model of the group, so the note may still be true";
            }

            bool clashed = runsATest && !judged.SkipsTheClash(ruleOn, runsATest);

            if (judged.Any && !clashed)
            {
                return "a model of the group is still not on the same shared coordinates and this run ran no clash"
                    + " test in it, so the note may still be true";
            }

            return null;
        }

        /// <summary>
        /// The note that goes beside the NWD and into the Clash Report folder, so whoever
        /// looks for the report finds the reason. The same lines as the log, then only what
        /// was checked: the tests in the words of the CLASH line, the tests an earlier run
        /// left in the NWF, the NWF and the NWD off the disk after the NWF was looked at the
        /// last time, and every report an earlier run left at the names this run would have
        /// written. testsAlreadyThere is minus one where it is UNKNOWN.
        /// </summary>
        public IList<string> Note(string building, NwfAndNwd files, int testsAlreadyThere, IList<string> earlierReports)
        {
            List<string> lines = new List<string>();
            lines.Add(Words.Or(building, "this group") + ": " + ClashSkippedReason);
            lines.Add("No clash test was run and no clash report or viewpoint was made, because these models are not on the"
                + " same shared coordinates"
                + (Reference == null ? string.Empty : ", measured from the reference model " + Reference) + ":");

            foreach (string model in Models)
            {
                lines.Add("   " + model);
            }

            lines.Add(TestsCreatedNoneRun + ".");
            lines.Add("Any clash test already saved in the NWF keeps the results of an earlier run, because this run ran none."
                + (testsAlreadyThere < 0
                    ? " How many of this run's tests were already there is UNKNOWN."
                    : testsAlreadyThere == 0
                        ? string.Empty
                        : " " + testsAlreadyThere + " of this run's tests were already there."));

            // A skipped group makes no viewpoint and removes none, so an earlier run's stay.
            lines.Add("Any viewpoint an earlier run saved in the NWF is still in it, and in an NWD published from it.");

            if (files != null)
            {
                lines.AddRange(files.Lines());
            }

            if (earlierReports == null || earlierReports.Count == 0)
            {
                lines.Add("No report file is at the names this run would have written, so no report of an earlier run"
                    + " stands beside this note.");
            }
            else
            {
                lines.Add("These report files are at the names this run would have written. Each is from an earlier run"
                    + " and was not written by this run:");
                lines.AddRange(earlierReports);
            }

            lines.Add("Once these models are exported again on the project's shared coordinates, the next run clashes the"
                + " group with the tests already saved in its NWF.");
            return lines;
        }
    }
}
