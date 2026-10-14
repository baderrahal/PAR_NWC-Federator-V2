using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Diagnostics;

namespace Federator.Core.Health
{
    /// <summary>
    /// What the shared coordinates check found across ONE run, for the RESULT block and for the
    /// one list the run writes for the modellers, Bader's answer to Q99 and Q100: "the RESULT
    /// block lists these groups, and the run writes one short list Bader can forward to the
    /// modellers, one file for the run".
    ///
    /// SINCE F159 NOTHING IS SKIPPED. The list says which models sit off the shared coordinates and
    /// that the clash results may be wrong for them, as a NOTICE, and says that every clash was run.
    ///
    /// ONE PER RUN, made when the run starts with that run's start and its count of groups, and
    /// never kept on the window's log. It lived on the log until c5d8aa8, one per window, and its
    /// groups were never cleared, so the second run of a window listed the first run's groups
    /// again, the breaker's first finding.
    ///
    /// EVERY GROUP THE CHECK REACHED IS ADDED, judged or not, so the count of groups judged,
    /// not reached and not read, and of models not judged, is said beside any zero, and a
    /// run that judged three groups of 46 never reads as 46 clean ones, the breaker's fourth.
    /// </summary>
    public sealed class OffCoordinatesAcrossTheRun
    {
        private const string ListNameStart = "Models not on the same shared coordinates, run ";

        private readonly DateTime runStarted;
        private readonly int groupsInTheRun;
        private readonly List<Entry> entries = new List<Entry>();

        public OffCoordinatesAcrossTheRun(DateTime runStarted, int groupsInTheRun)
        {
            this.runStarted = runStarted;
            this.groupsInTheRun = groupsInTheRun;
        }

        /// <summary>
        /// The name of this run's list, its own file named for the second the run started.
        /// One fixed name let a smaller run, or the open file run of one group, write over
        /// the list of a fuller run, so every run's list is kept and none stands for another.
        /// </summary>
        public string ListName
        {
            get { return ListNameStart + runStarted.ToString("yyyy-MM-dd HHmmss", CultureInfo.InvariantCulture) + ".txt"; }
        }

        /// <summary>
        /// One group, added as its ALIGNMENT block is written: what the check judged of its
        /// models, or null where the read threw, and whether this run runs a clash test in it.
        /// </summary>
        public void Add(string building, OffCoordinates off, bool runsATest)
        {
            entries.Add(new Entry(string.IsNullOrEmpty(building) ? "UNKNOWN" : building, off, runsATest));
        }

        /// <summary>How many groups held at least one model not on the same shared coordinates.</summary>
        public int GroupsWithOffModels
        {
            get { return Count(e => e.Off != null && e.Off.Any); }
        }

        /// <summary>The RESULT lines: what the check did, how many groups it judged, and one line per group holding such a model.</summary>
        public IList<string> ResultLines()
        {
            List<string> lines = new List<string>();
            int off = GroupsWithOffModels;

            lines.Add("clash skipped  : none, every clash is run whatever the coordinates are"
                + (off == 0
                    ? string.Empty
                    : ". " + Words.Counted(off, "group", "groups") + (off == 1 ? " holds" : " hold")
                        + " a model not on the same shared coordinates, so the clash results may be wrong for "
                        + (off == 1 ? "it" : "them")));

            lines.Add("coordinates    : " + Judged() + " of " + Words.Counted(groupsInTheRun, "group", "groups") + " judged" + NotJudgedResult());

            foreach (Entry entry in entries)
            {
                if (entry.Off != null && entry.Off.Any)
                {
                    lines.Add("      " + entry.Building.PadRight(10) + " " + entry.Off.Models.Count
                        + " model(s) not on the same shared coordinates, the clash results may be wrong"
                        + (entry.Off.OnInternal.Count == 0
                            ? string.Empty
                            : ". NOTICE, on Revit's internal origin: " + string.Join(", ", entry.Off.OnInternal)
                                + ", the group is not FAILED for it"));
                }
            }

            return lines;
        }

        /// <summary>
        /// The one list for the run, which Bader forwards to the modellers: how many groups
        /// the check judged, then a NOTICE for each group holding such a model with the
        /// reference it was measured from and the same line per model the log carries.
        /// Says only what was checked, and never that no model was found off while any group
        /// or model was not judged.
        /// </summary>
        public IList<string> ForModellers()
        {
            List<string> lines = new List<string>();
            lines.Add("Models not on the same shared coordinates, from the run started "
                + runStarted.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            lines.Add("The check judged " + Judged() + " of " + Words.Counted(groupsInTheRun, "group", "groups") + " of this run." + NotJudgedList());

            if (GroupsWithOffModels == 0)
            {
                lines.Add(Judged() == groupsInTheRun && ModelsNotJudged() == 0
                    ? "No model in this run was found off its group's shared coordinates, so no group is listed."
                    : "No group is listed, and that is not a clean bill for the groups and models above that were not judged.");
                return lines;
            }

            lines.Add("NOTICE. Every clash was run and none was skipped. " + OffCoordinates.ResultsMayBeWrong
                + " Export them again on the project's shared coordinates and run again.");

            foreach (Entry entry in entries)
            {
                if (entry.Off == null || !entry.Off.Any)
                {
                    continue;
                }

                lines.Add(string.Empty);
                IList<string> notice = entry.Off.Notice(entry.Building);
                lines.Add("NOTICE. " + notice[0]);

                for (int i = 1; i < notice.Count; i++)
                {
                    lines.Add(notice[i]);
                }

                if (!entry.RunsATest)
                {
                    lines.Add("   No clash test was to run in this group this run, so there are no results of it to be wrong.");
                }
            }

            return lines;
        }

        /// <summary>
        /// The ALIGNMENT run line, which ended "Nothing was changed and every group ran." It says
        /// what the check found. The models counted are those the block calls different, over the
        /// 1 mm tolerance.
        /// </summary>
        public string AlignmentRunLine(int differentModels)
        {
            int off = GroupsWithOffModels;
            string line = "ALIGNMENT across the run: " + differentModels
                + " model(s) sit somewhere their group's reference model does not";

            if (differentModels == 0 && off == 0)
            {
                return line;
            }

            return line + ". Nothing was changed in any model"
                + (off == 0
                    ? "."
                    : ", and no clash was skipped. " + Words.Counted(off, "group", "groups")
                        + " held models not on the same shared coordinates, so the clash results may be wrong for them,"
                        + " which RESULT lists.");
        }

        private int Judged()
        {
            return Count(e => e.Off != null && e.Off.ModelsRead > 0);
        }

        private int ModelsNotJudged()
        {
            int models = 0;

            foreach (Entry entry in entries)
            {
                if (entry.Off != null)
                {
                    models += entry.Off.NotJudged;
                }
            }

            return models;
        }

        /// <summary>The groups the check never reached: the run stopped, or the group ended before its models were read.</summary>
        private int NotReached()
        {
            return Math.Max(0, groupsInTheRun - entries.Count);
        }

        /// <summary>The groups it reached and could not judge: the read threw, or no model was read.</summary>
        private int NotRead()
        {
            return entries.Count - Judged();
        }

        private string NotJudgedResult()
        {
            string said = string.Empty;

            if (NotReached() > 0)
            {
                said += ", " + NotReached() + " not reached, the run stopped or the group ended before its models were read";
            }

            if (NotRead() > 0)
            {
                said += ", " + NotRead() + " whose models could not be read";
            }

            if (ModelsNotJudged() > 0)
            {
                said += ", " + ModelsNotJudged() + " model(s) in the groups judged not judged, a placement or a site UNKNOWN";
            }

            return said;
        }

        private string NotJudgedList()
        {
            string said = string.Empty;

            if (NotReached() > 0)
            {
                said += " " + NotReached() + (NotReached() == 1 ? " was" : " were")
                    + " not reached, the run stopped or the group ended before its models were read.";
            }

            if (NotRead() > 0)
            {
                said += " " + NotRead() + " could not have their models read.";
            }

            if (ModelsNotJudged() > 0)
            {
                said += " " + ModelsNotJudged() + " model(s) in the groups judged could not be judged, a placement or a"
                    + " site UNKNOWN, and are not listed.";
            }

            return said;
        }

        private int Count(Predicate<Entry> which)
        {
            return entries.FindAll(which).Count;
        }

        /// <summary>One group as the check left it.</summary>
        private sealed class Entry
        {
            public Entry(string building, OffCoordinates off, bool runsATest)
            {
                Building = building;
                Off = off;
                RunsATest = runsATest;
            }

            public string Building { get; private set; }

            public OffCoordinates Off { get; private set; }

            public bool RunsATest { get; private set; }
        }
    }
}
