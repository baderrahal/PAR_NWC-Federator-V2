using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Diagnostics;

namespace Federator.Core.Communities
{
    /// <summary>
    /// The words of a run of packages in fresh starts, F214: the lines the runner writes into the status file as each
    /// package starts and ends, which the parent copies into its log and its live line, and the lines of the one RESULT
    /// the parent writes once every package's Navisworks has closed. Every line the runner and the parent write about a
    /// fresh start is made here.
    /// </summary>
    public static class PackageRunWords
    {
        /// <summary>The start of every status line in the parent's log.</summary>
        public const string Prefix = "PACKAGES";

        public static string RunnerStarted(int packages, string order)
        {
            return "the runner started for " + PackageWords.Count(packages, "package", "packages")
                + ", each in a fresh Navisworks, the next only once the last has closed, order " + order;
        }

        public static string Starting(int index, int count, string package)
        {
            return "package " + (index + 1).ToString(CultureInfo.InvariantCulture) + " of " + count.ToString(CultureInfo.InvariantCulture)
                + ", " + package + ", starting a fresh Navisworks";
        }

        public static string Started(string package, int pid, double seconds)
        {
            return package + ": Navisworks started in " + Seconds(seconds)
                + (pid > 0 ? ", pid " + pid.ToString(CultureInfo.InvariantCulture) : ", which process it is could not be told");
        }

        public static string Returned(string package, object code, double seconds)
        {
            return package + ": its run returned " + (code == null ? "nothing" : code.ToString()) + " after " + Seconds(seconds);
        }

        public static string Threw(string package, string what, string type, string message)
        {
            return package + ": " + what + " threw " + type + ": " + message + ". The next package still runs";
        }

        public static string Ceiling(string package, int minutes)
        {
            return package + ": still running after the ceiling of " + PackageWords.Count(minutes, "minute", "minutes")
                + ", so the runner ended its Navisworks and goes on to the next package";
        }

        public static string Gone(string package, double seconds)
        {
            return package + ": its Navisworks is gone " + Seconds(seconds) + " after it was told to quit";
        }

        public static string Ended(string package, int seconds)
        {
            return package + ": its Navisworks was still there " + PackageWords.Count(seconds, "second", "seconds")
                + " after it was told to quit, so the runner ended it before the next package starts";
        }

        public static string NotEnded(int pid, string type, string message)
        {
            return "the Navisworks of pid " + pid.ToString(CultureInfo.InvariantCulture) + " could not be ended, " + type + ": " + message;
        }

        /// <summary>Said after the next line that reached the status file, where lines before it could not be written.</summary>
        public static string LinesLost(int count)
        {
            return ". " + PackageWords.Count(count, "line", "lines") + " before this one could not be written to the status file";
        }

        public static string NotTold(string package)
        {
            return package + ": which Navisworks it ran in could not be told, so the runner waited on none before the next";
        }

        public static string RunnerEnded(int packages, double seconds)
        {
            return "the runner ended after " + PackageWords.Count(packages, "package", "packages") + " in " + Seconds(seconds);
        }

        public static string ChildStarted(int index, int count, string package, string parentLog)
        {
            return "this Navisworks runs package " + (index + 1).ToString(CultureInfo.InvariantCulture) + " of "
                + count.ToString(CultureInfo.InvariantCulture) + ", " + package + ", for the run whose log is "
                + Words.Or(parentLog, "UNKNOWN");
        }

        public static string ControlNotHere(string kind, string name)
        {
            return "the " + kind + " control " + name + " of the order is not in this window, so it is left as it opened";
        }

        public static string ControlsCopied(int count)
        {
            return PackageWords.Count(count, "control", "controls") + " of the window set as the run that started it left them";
        }

        /// <summary>A box a package's run would have shown, logged in its place, since nobody is at its window.</summary>
        public static string NoBox(string text)
        {
            return "no box is shown in a package's own Navisworks, it would have said: "
                + (text ?? string.Empty).Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
        }

        public static string ConfirmedByTheParent(string what)
        {
            return what + " was confirmed once in the window that started the run, so it is not asked again here";
        }

        public static string ResultWritten(string path)
        {
            return "the result of this package is written to " + path;
        }

        public static string RunnerStarting(string runner, int packages, string order)
        {
            return "starting " + runner + " for " + PackageWords.Count(packages, "package", "packages") + ", order " + order
                + ". This window waits and answers while each package runs in its own Navisworks";
        }

        /// <summary>The window's live line while the packages run, in plain words, a label and never the runner's lines.</summary>
        public static string Waiting(int packages)
        {
            return PackageWords.Count(packages, "package runs", "packages run") + " one after the other, each in its own Navisworks. The log follows each.";
        }

        public static string Finished(int packages)
        {
            return "Run finished. " + PackageWords.Count(packages, "package", "packages") + " ran, each in its own Navisworks. The log's RESULT joins them.";
        }

        public static string StatusNotRead(string message)
        {
            return "the runner's status file could not be read, " + message + ". It is read again on every pass and this is said once";
        }

        public static string RunnerEndedWith(int code)
        {
            return "the runner ended with code " + code.ToString(CultureInfo.InvariantCulture);
        }

        public static string NoResult(string package, string why)
        {
            return package + ": no result, " + why;
        }

        /// <summary>
        /// The lines of the one RESULT of a run of packages: the group counts over every package's groups, the package tally
        /// the parent filled from the results, then each package's own RESULT block under its name and its log, or why it
        /// has none.
        /// </summary>
        public static IList<string> JoinedResult(
            IList<string> packages, IList<PackageRunResult> results, IList<string> noResultWhy, IList<string> tallyLines)
        {
            if (packages == null || results == null || noResultWhy == null || results.Count != packages.Count || noResultWhy.Count != packages.Count)
            {
                throw new ArgumentException("one result or one reason for each package");
            }

            int done = 0;
            int partial = 0;
            int failed = 0;

            foreach (PackageRunResult result in results)
            {
                if (result == null)
                {
                    continue;
                }

                foreach (PackageRunGroup group in result.Groups)
                {
                    if (group.Outcome == GroupOutcome.Done)
                    {
                        done++;
                    }
                    else if (group.Outcome == GroupOutcome.Partial)
                    {
                        partial++;
                    }
                    else
                    {
                        failed++;
                    }
                }
            }

            List<string> lines = new List<string>
            {
                "fresh starts   : " + PackageWords.Count(packages.Count, "package", "packages")
                    + ", each in its own Navisworks, one after the other",
                "groups done    : " + done.ToString(CultureInfo.InvariantCulture),
                "groups partial : " + partial.ToString(CultureInfo.InvariantCulture),
                "groups failed  : " + failed.ToString(CultureInfo.InvariantCulture)
            };

            int without = 0;

            for (int i = 0; i < packages.Count; i++)
            {
                if (results[i] == null)
                {
                    without++;
                }
            }

            if (without > 0)
            {
                lines.Add("no result      : " + PackageWords.Count(without, "package", "packages")
                    + ", so the counts above leave out every group of them, each named below");
            }

            lines.Add(string.Empty);

            if (tallyLines != null)
            {
                lines.AddRange(tallyLines);
            }

            for (int i = 0; i < packages.Count; i++)
            {
                lines.Add(string.Empty);

                if (results[i] == null)
                {
                    lines.Add("PACKAGE " + packages[i] + " : NO RESULT, " + Words.Or(noResultWhy[i], "UNKNOWN why"));
                    continue;
                }

                lines.Add("PACKAGE " + packages[i] + " : its own RESULT, from its log " + Words.Or(results[i].Log, "UNKNOWN, it opened none"));

                if (results[i].NoGroupBecause != null)
                {
                    lines.Add("    no group of it ran, " + results[i].NoGroupBecause);
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
