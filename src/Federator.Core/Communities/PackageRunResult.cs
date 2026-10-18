using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Diagnostics;

namespace Federator.Core.Communities
{
    /// <summary>How one group of a package ended in that package's own Navisworks.</summary>
    public sealed class PackageRunGroup
    {
        public PackageRunGroup(string group, GroupOutcome outcome, long clashes, string reason)
        {
            Group = group ?? string.Empty;
            Outcome = outcome;
            Clashes = clashes;
            Reason = reason;
        }

        /// <summary>The group as the package tally names it, the building on the NWC page and the NWF on the NWF page.</summary>
        public string Group { get; private set; }

        public GroupOutcome Outcome { get; private set; }

        /// <summary>The clashes the group found, minus one where the count was not taken.</summary>
        public long Clashes { get; private set; }

        /// <summary>Why the group is not DONE, or null.</summary>
        public string Reason { get; private set; }
    }

    /// <summary>
    /// What one package's Navisworks wrote once its run ended, F214: the package, its own log, every group with how it
    /// ended, and the lines of its own RESULT block, so the parent joins them into one RESULT. A package whose Navisworks
    /// ended before it wrote this has no result, and the parent says so with why.
    /// </summary>
    public sealed class PackageRunResult
    {
        /// <summary>The first line of a result file.</summary>
        public const string Header = "parsons nwc federator, the result of one package of a run of packages, 1";

        /// <summary>The line a RESULT block's title is written on, between two rules, RunLog.Section.</summary>
        public const string ResultTitle = "RESULT";

        public PackageRunResult(string package, string log)
        {
            Package = package ?? string.Empty;
            Log = log;
            Groups = new List<PackageRunGroup>();
            ResultLines = new List<string>();
        }

        public string Package { get; private set; }

        /// <summary>The package's own log, or null where it opened none.</summary>
        public string Log { get; private set; }

        public IList<PackageRunGroup> Groups { get; private set; }

        public IList<string> ResultLines { get; private set; }

        /// <summary>Why the package ran no group, or null where one ran.</summary>
        public string NoGroupBecause { get; set; }

        /// <summary>
        /// The lines of the last RESULT block of a log, every line after the rule under its title, or none where the log
        /// holds no RESULT block.
        /// </summary>
        public static IList<string> ResultLinesOf(IList<string> logLines)
        {
            List<string> lines = new List<string>();

            if (logLines == null)
            {
                return lines;
            }

            int start = -1;

            for (int i = 1; i + 1 < logLines.Count; i++)
            {
                if (logLines[i] == ResultTitle && IsARule(logLines[i - 1]) && IsARule(logLines[i + 1]))
                {
                    start = i + 2;
                }
            }

            if (start < 0)
            {
                return lines;
            }

            for (int i = start; i < logLines.Count; i++)
            {
                lines.Add(logLines[i]);
            }

            while (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
            {
                lines.RemoveAt(lines.Count - 1);
            }

            return lines;
        }

        private static bool IsARule(string line)
        {
            return line != null && line.Length > 0 && line.Trim('=').Length == 0;
        }

        public void Write(string path)
        {
            List<string> lines = new List<string>
            {
                PackageRunFile.Line("package", Package),
                PackageRunFile.Line("log", Log ?? string.Empty)
            };

            if (NoGroupBecause != null)
            {
                lines.Add(PackageRunFile.Line("nogroup", NoGroupBecause));
            }

            foreach (PackageRunGroup group in Groups)
            {
                lines.Add(PackageRunFile.Line(
                    "group",
                    group.Group,
                    group.Outcome.ToString(),
                    group.Clashes.ToString(CultureInfo.InvariantCulture),
                    group.Reason ?? string.Empty));
            }

            foreach (string line in ResultLines)
            {
                lines.Add(PackageRunFile.Line("result", line));
            }

            lines.Add(PackageRunFile.Line("end", "written whole"));
            PackageRunFile.Write(path, Header, lines);
        }

        /// <summary>
        /// The result at this path, or null with why: the file not there or not read, a line that does not read, or no
        /// end line, which is a file its Navisworks stopped writing part way.
        /// </summary>
        public static PackageRunResult Read(string path, out string why)
        {
            IList<string[]> lines = PackageRunFile.Read(path, Header, out why);

            if (lines == null)
            {
                return null;
            }

            string package = null;
            string log = null;
            string noGroup = null;
            bool ended = false;
            List<PackageRunGroup> groups = new List<PackageRunGroup>();
            List<string> result = new List<string>();

            foreach (string[] line in lines)
            {
                string fault = null;

                if (line[0] == "group")
                {
                    GroupOutcome outcome;
                    long clashes;

                    if (line.Length != 5)
                    {
                        fault = "a group line holds " + (line.Length - 1) + " values where it holds 4";
                    }
                    else if (!Enum.TryParse(line[2], false, out outcome) || !Enum.IsDefined(typeof(GroupOutcome), outcome)
                        || line[2] != outcome.ToString())
                    {
                        fault = "the outcome " + line[2] + " of " + line[1] + " is not one a group ends in";
                    }
                    else if (!long.TryParse(line[3], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out clashes) || clashes < -1)
                    {
                        fault = "the clashes " + line[3] + " of " + line[1] + " are not a count";
                    }
                    else
                    {
                        groups.Add(new PackageRunGroup(line[1], outcome, clashes, line[4].Length == 0 ? null : line[4]));
                    }
                }
                else if (line.Length != 2)
                {
                    fault = "the line " + line[0] + " holds " + (line.Length - 1) + " values where it holds 1";
                }
                else if (line[0] == "package")
                {
                    package = line[1];
                }
                else if (line[0] == "log")
                {
                    log = line[1].Length == 0 ? null : line[1];
                }
                else if (line[0] == "nogroup")
                {
                    noGroup = line[1];
                }
                else if (line[0] == "result")
                {
                    result.Add(line[1]);
                }
                else if (line[0] == "end")
                {
                    ended = true;
                }
                else
                {
                    fault = "the key " + line[0] + " is not one a result holds";
                }

                if (fault != null)
                {
                    why = "the result " + path + " has a line that does not read, " + fault;
                    return null;
                }
            }

            if (!ended || package == null)
            {
                why = "the result " + path + " stops before its end, so its Navisworks stopped writing it part way";
                return null;
            }

            PackageRunResult read = new PackageRunResult(package, log) { NoGroupBecause = noGroup };

            foreach (PackageRunGroup group in groups)
            {
                read.Groups.Add(group);
            }

            foreach (string line in result)
            {
                read.ResultLines.Add(line);
            }

            why = null;
            return read;
        }
    }
}
