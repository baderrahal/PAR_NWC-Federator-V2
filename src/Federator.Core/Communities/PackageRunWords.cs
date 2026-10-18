using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Coverage;
using Federator.Core.Diagnostics;

namespace Federator.Core.Communities
{
    /// <summary>
    /// The words of a run of packages in fresh starts, F214, and since F222, Q211, one group in each Navisworks: the lines
    /// the runner writes into the status file as each group's Navisworks starts and quits, which the parent copies into its
    /// log, the parent's live line and warning, and the lines of the one RESULT the parent writes once every group's
    /// Navisworks has closed. Every line the runner and the parent write about a fresh start is made here.
    /// </summary>
    public static class PackageRunWords
    {
        /// <summary>The start of every status line in the parent's log.</summary>
        public const string Prefix = "PACKAGES";

        /// <summary>The confirm's line in place of what opening would discard, since no group opens anything in this Navisworks.</summary>
        public const string NothingHereIsTouched =
            "Each group runs in a fresh Navisworks of its own, one after the other, so nothing open in this one is touched.";

        /// <summary>A text on one line, every line break a space, CoverageBlock.OneLine's rule, trimmed.</summary>
        public static string OneLine(string text)
        {
            return CoverageBlock.OneLine(text).Trim();
        }

        public static string RunnerStarted(int groups, int packages, string order)
        {
            return "the runner started for " + PackageWords.Count(groups, "group", "groups") + " of "
                + PackageWords.Count(packages, "package", "packages")
                + ", each group in a fresh Navisworks, the next only once the last has closed, order " + order;
        }

        public static string RunnerRefused(string why)
        {
            return "the runner ran no group, " + why;
        }

        public static string Starting(int index, int count, string group)
        {
            return "group " + (index + 1).ToString(CultureInfo.InvariantCulture) + " of " + count.ToString(CultureInfo.InvariantCulture)
                + ", " + group + ", starting a fresh Navisworks";
        }

        public static string Started(string group, int pid, double seconds)
        {
            return group + ": Navisworks started in " + Seconds(seconds)
                + (pid > 0 ? ", pid " + pid.ToString(CultureInfo.InvariantCulture) : ", which process it is could not be told");
        }

        public static string Returned(string group, object code, double seconds)
        {
            return group + ": its run returned " + (code == null ? "nothing" : code.ToString()) + " after " + Seconds(seconds);
        }

        public static string Threw(string group, string what, string type, string message)
        {
            return group + ": " + what + " threw " + type + ": " + OneLine(message) + ". The next group still runs";
        }

        public static string Ceiling(string group, int minutes)
        {
            return group + ": still not done after the ceiling of " + PackageWords.Count(minutes, "minute", "minutes")
                + ", so the runner ended its Navisworks and goes on to the next group";
        }

        public static string CeilingNotTold(string group, int minutes)
        {
            return group + ": still not done after the ceiling of " + PackageWords.Count(minutes, "minute", "minutes")
                + ", and which Navisworks is its own could not be told, so none was ended";
        }

        public static string Gone(string group, double seconds)
        {
            return group + ": its Navisworks is gone " + Seconds(seconds) + " after it was told to quit";
        }

        public static string Ended(string group, int seconds)
        {
            return group + ": its Navisworks was still there " + PackageWords.Count(seconds, "second", "seconds")
                + " after it was told to quit, so the runner ended it before the next group starts";
        }

        /// <summary>A group whose Navisworks could not be told from another, never run, since the runner cannot end what it cannot tell.</summary>
        public static string NotTold(string group)
        {
            return group + ": more than one Navisworks started with it, or none, so which is its own could not be told. It is not "
                + "run, since the runner never ends a Navisworks it cannot tell is its own";
        }

        public static string NotToldGone(string group, double seconds)
        {
            return group + ": every Navisworks new since its start is gone " + Seconds(seconds) + " after it was told to quit";
        }

        public static string NotToldStillThere(string group, int seconds)
        {
            return group + ": a Navisworks new since its start is still there " + PackageWords.Count(seconds, "second", "seconds")
                + " after it was told to quit, and none is ended since none can be told as its own. The next group starts beside it";
        }

        public static string NotEnded(int pid, string type, string message)
        {
            return "the Navisworks of pid " + pid.ToString(CultureInfo.InvariantCulture) + " could not be ended, " + type + ": " + OneLine(message);
        }

        /// <summary>Said after the next line that reached the status file, where lines before it could not be written.</summary>
        public static string LinesLost(int count)
        {
            return ". " + PackageWords.Count(count, "line", "lines") + " before this one could not be written to the status file";
        }

        /// <summary>
        /// The runner's last line, F222, Q211: the groups and packages it ran, its whole time, and how long every start and
        /// every quit took added up, a start from the call that makes the Navisworks to its process told, a quit from the call
        /// that tells it to quit to its process gone.
        /// </summary>
        public static string RunnerEnded(int groups, int packages, double seconds, int starts, double startSeconds, int quits, double quitSeconds)
        {
            return "the runner ended after " + PackageWords.Count(groups, "group", "groups") + " of "
                + PackageWords.Count(packages, "package", "packages") + " in " + Seconds(seconds) + ", "
                + PackageWords.Count(starts, "start", "starts") + " took " + Seconds(startSeconds) + " in all and "
                + PackageWords.Count(quits, "quit", "quits") + " " + Seconds(quitSeconds);
        }

        public static string ChildStarted(int index, int count, string group, string parentLog)
        {
            return "this Navisworks runs group " + (index + 1).ToString(CultureInfo.InvariantCulture) + " of "
                + count.ToString(CultureInfo.InvariantCulture) + ", " + group + ", for the run whose log is "
                + Words.Or(parentLog, "UNKNOWN");
        }

        /// <summary>F222. Why a group's own Navisworks ran nothing where its package's plan there holds no group of its NWF.</summary>
        public static string NotTheGroup(string group, string nwf)
        {
            return "the group " + group + " is not among the groups its package plans in its own Navisworks, no group there writes "
                + nwf + ", so nothing runs";
        }

        public static string ControlNotHere(string kind, string name)
        {
            return "the " + kind + " control " + name + " of the order could not be set in this window";
        }

        public static string ControlsCopied(int count)
        {
            return PackageWords.Count(count, "control", "controls") + " of the window set as the run that started it left them";
        }

        /// <summary>Why a group does not run where a control of the order could not be set, so it never runs on settings nobody chose.</summary>
        public static string ControlsNotAllSet(int notSet, int all)
        {
            return notSet.ToString(CultureInfo.InvariantCulture) + " of the " + PackageWords.Count(all, "control", "controls")
                + " of the order could not be set in its own Navisworks, each named in its log, so it was not run on settings the person did not choose";
        }

        /// <summary>A box a group's run would have shown, logged in its place, since nobody is at its window.</summary>
        public static string NoBox(string text)
        {
            return "no box is shown in a group's own Navisworks, it would have said: " + OneLine(text);
        }

        public static string ConfirmedByTheParent(string what)
        {
            return what + " was confirmed once in the window that started the run, so it is not asked again here";
        }

        public static string ResultWritten(string path)
        {
            return "the result of this group is written to " + path;
        }

        public static string RunnerStarting(string runner, int groups, int packages, string order)
        {
            return "starting " + runner + " for " + PackageWords.Count(groups, "group", "groups") + " of "
                + PackageWords.Count(packages, "package", "packages") + ", order " + order
                + ". This window waits and answers while each group runs in its own Navisworks";
        }

        public static string RunnerEndedWith(int code)
        {
            return "the runner ended with code " + code.ToString(CultureInfo.InvariantCulture);
        }

        public static string RunnerPastItsLimit(int minutes)
        {
            return "the runner was still running " + PackageWords.Count(minutes, "minute", "minutes")
                + " after it started, every group's ceiling and wait added up, so this window ended the runner and reads the results there are";
        }

        public static string WaitFailed(string type, string message)
        {
            return "this window's wait on the runner threw " + type + ": " + OneLine(message) + ". It is said once and the wait goes on";
        }

        public static string StatusNotRead(string message)
        {
            return "the runner's status file could not be read, " + OneLine(message) + ". It is read again on every pass and this is said once";
        }

        /// <summary>The window's live line while the groups run, in plain words, a label and never the runner's lines.</summary>
        public static string Waiting(int groups)
        {
            return PackageWords.Count(groups, "group runs", "groups run") + " one after the other, each in its own Navisworks. The log follows each.";
        }

        /// <summary>The window's live line once the run ends, counting only the groups that wrote a result.</summary>
        public static string Finished(int withResult, int groups)
        {
            if (withResult == groups)
            {
                return "Run finished. " + PackageWords.Count(groups, "group", "groups") + " ran, each in its own Navisworks. The log's RESULT joins them.";
            }

            return "Run finished. " + withResult.ToString(CultureInfo.InvariantCulture) + " of " + PackageWords.Count(groups, "group", "groups")
                + " wrote a result. The log's RESULT names each and why.";
        }

        /// <summary>The warning the person sees once a run in fresh starts ends with a group that wrote no result, or null where every one did.</summary>
        public static string Warning(int withResult, int groups)
        {
            return withResult == groups
                ? null
                : (groups - withResult).ToString(CultureInfo.InvariantCulture) + " of " + PackageWords.Count(groups, "group", "groups")
                    + " wrote no result, so whether they wrote files is not known. The log's RESULT names each and why.";
        }

        /// <summary>The words the package tally carries for a group whose Navisworks wrote no result.</summary>
        public static string NoResult(string why)
        {
            return "NO RESULT, " + Words.Or(why, "UNKNOWN why") + ". Whether it wrote files is UNKNOWN";
        }

        /// <summary>F222. The words the package tally carries for a group whose own Navisworks ran nothing.</summary>
        public static string GroupNotRun(string why)
        {
            return "NOT RUN in its own Navisworks, " + Words.Or(why, "UNKNOWN why");
        }

        /// <summary>
        /// The lines of the one RESULT of a run of packages, since F222 one result a group: the group counts off the package
        /// tally the parent filled from the results, how many groups wrote no result and how many ran nothing, the tally, each
        /// group that is not DONE with its reason, then each group's own RESULT under its package and name and its log, or NO
        /// RESULT with why.
        /// </summary>
        public static IList<string> JoinedResult(
            IList<PackageRunUnit> units, IList<PackageRunResult> results, IList<string> noResultWhy, PackageTally tally)
        {
            if (units == null || results == null || noResultWhy == null || tally == null
                || results.Count != units.Count || noResultWhy.Count != units.Count)
            {
                throw new ArgumentException("one result or one reason for each group, and the tally");
            }

            HashSet<string> packages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (PackageRunUnit unit in units)
            {
                packages.Add(unit.Package);
            }

            List<string> lines = new List<string>
            {
                "fresh starts   : " + PackageWords.Count(units.Count, "group", "groups") + " of "
                    + PackageWords.Count(packages.Count, "package", "packages")
                    + ", each group in its own Navisworks, one after the other",
                "groups done    : " + tally.CountOf(GroupOutcome.Done).ToString(CultureInfo.InvariantCulture),
                "groups partial : " + tally.CountOf(GroupOutcome.Partial).ToString(CultureInfo.InvariantCulture),
                "groups failed  : " + tally.CountOf(GroupOutcome.Failed).ToString(CultureInfo.InvariantCulture)
            };

            if (tally.NoResultCount > 0)
            {
                lines.Add("no result      : " + PackageWords.Count(tally.NoResultCount, "group", "groups")
                    + ", the counts above leave them out and whether they wrote files is UNKNOWN, each named below");
            }

            if (tally.NotRunCount > 0)
            {
                lines.Add("ran nothing    : " + PackageWords.Count(tally.NotRunCount, "group", "groups") + ", each named below with why");
            }

            lines.Add(string.Empty);
            lines.AddRange(tally.Lines());

            for (int i = 0; i < units.Count; i++)
            {
                if (results[i] == null)
                {
                    continue;
                }

                foreach (PackageRunGroup group in results[i].Groups)
                {
                    if (group.Outcome != GroupOutcome.Done && !string.IsNullOrEmpty(group.Reason))
                    {
                        lines.Add("why            " + units[i].Package + "  " + group.Group + " : " + OneLine(group.Reason));
                    }
                }
            }

            for (int i = 0; i < units.Count; i++)
            {
                lines.Add(string.Empty);

                if (results[i] == null)
                {
                    lines.Add("GROUP " + units[i].Label + " : NO RESULT, " + Words.Or(noResultWhy[i], "UNKNOWN why"));
                    continue;
                }

                lines.Add("GROUP " + units[i].Label + " : its own RESULT, from its log " + Words.Or(results[i].Log, "UNKNOWN, it opened none"));

                if (results[i].NoGroupBecause != null)
                {
                    lines.Add("    it ran nothing, " + results[i].NoGroupBecause);
                }

                if (results[i].ResultLines.Count == 0)
                {
                    lines.Add("    its log holds no RESULT block");
                }

                foreach (string line in results[i].ResultLines)
                {
                    lines.Add("    " + line);
                }
            }

            return lines;
        }

        private static string Seconds(double seconds)
        {
            return seconds.ToString("0.0", CultureInfo.InvariantCulture) + " s";
        }
    }
}
