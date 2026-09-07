using System;
using System.Collections.Generic;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// Which of the two workflows a group is going to take, in the words the window shows
    /// and the log carries. Bader confirmed the two on 2026-09-07.
    ///
    /// First run. The NWC folder is scanned and grouped, output folders picked, a clash
    /// XML picked, Run pressed. Per group the tool builds the NWF, builds the sets, adds
    /// and runs the tests from the XML, writes Excel, HTML, images and the NWD.
    ///
    /// Weekly run. The same NWC folder, Run pressed with no XML. Per group the tool finds
    /// the NWF already there, opens it (OPENED), lets Navisworks reload the newer NWCs,
    /// runs the tests saved inside the NWF, writes Excel, HTML, images and the NWD. An XML
    /// is optional and only adds or updates tests.
    ///
    /// The label is read off the Decide result and whether an XML is picked, and nothing
    /// else. This changes what the person is told, never what the engine does.
    /// </summary>
    public static class RunPath
    {
        public const string FirstRun = "First run";

        public const string WeeklyRun = "Weekly run";

        public const string WeeklyRunPlusXml = "Weekly run plus XML";

        public const string Skipped = "Skipped (changed on disk)";

        /// <summary>For a decision the rule cannot name. Never shown for the three known ones.</summary>
        public const string Unknown = "Unknown";

        /// <summary>The five labels, in the order the counts are listed.</summary>
        public static readonly string[] All = { FirstRun, WeeklyRun, WeeklyRunPlusXml, Skipped, Unknown };

        /// <summary>The label for a group once Decide has run.</summary>
        public static string Label(RerunDecision decision, bool xmlPicked)
        {
            switch (decision)
            {
                case RerunDecision.Build:
                    return FirstRun;
                case RerunDecision.Open:
                    return xmlPicked ? WeeklyRunPlusXml : WeeklyRun;
                case RerunDecision.Changed:
                    return Skipped;
                default:
                    return Unknown;
            }
        }

        /// <summary>
        /// The label before the run, from whether the NWF is already at its output path.
        /// CHANGED can only be known once the NWF is opened and its file list read, so it
        /// never appears here. It is reported in the log when it happens.
        /// </summary>
        public static string Expected(bool nwfOnDisk, bool xmlPicked)
        {
            return Label(nwfOnDisk ? RerunDecision.Open : RerunDecision.Build, xmlPicked);
        }

        /// <summary>How many groups carry each label. Every label is present, at zero where none does.</summary>
        public static IDictionary<string, int> Count(IEnumerable<string> labels)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (string label in All)
            {
                counts[label] = 0;
            }

            if (labels != null)
            {
                foreach (string label in labels)
                {
                    string known = label != null && counts.ContainsKey(label) ? label : Unknown;
                    counts[known]++;
                }
            }

            return counts;
        }

        /// <summary>
        /// The first lines of the confirm dialog, one count per label. The clearing
        /// sentence is only said where it is true, which is the First run groups. Skipped
        /// is only known once each NWF is opened, and the line says so rather than
        /// pretending to a number.
        /// </summary>
        public static IList<string> ConfirmLines(IEnumerable<string> labels)
        {
            IDictionary<string, int> counts = Count(labels);
            int total = 0;

            foreach (string label in All)
            {
                total += counts[label];
            }

            List<string> lines = new List<string>();
            lines.Add("This run federates " + total + (total == 1 ? " group." : " groups."));
            lines.Add(string.Empty);
            lines.Add(FirstRun + ": " + counts[FirstRun]
                + (counts[FirstRun] > 0
                    ? ". The NWF is built new, and the document is cleared before each one."
                    : "."));
            lines.Add(WeeklyRun + ": " + counts[WeeklyRun]
                + (counts[WeeklyRun] > 0
                    ? ". The NWF already there is opened and the saved tests run. Nothing is cleared."
                    : "."));
            lines.Add(WeeklyRunPlusXml + ": " + counts[WeeklyRunPlusXml]
                + (counts[WeeklyRunPlusXml] > 0
                    ? ". The NWF already there is opened and the XML adds or updates tests. Nothing is cleared."
                    : "."));
            lines.Add(Skipped + ": " + counts[Skipped]
                + ". Only known once each NWF is opened, so it is reported in the log.");

            if (counts[Unknown] > 0)
            {
                lines.Add(Unknown + ": " + counts[Unknown] + ".");
            }

            return lines;
        }

        /// <summary>The lines the RESULT block carries, one per label, in a fixed order.</summary>
        public static IList<string> ResultLines(IEnumerable<string> labels)
        {
            IDictionary<string, int> counts = Count(labels);
            List<string> lines = new List<string>();

            lines.Add("first run      : " + counts[FirstRun]);
            lines.Add("weekly run     : " + counts[WeeklyRun]);
            lines.Add("weekly + XML   : " + counts[WeeklyRunPlusXml]);
            lines.Add("skipped        : " + counts[Skipped]);

            if (counts[Unknown] > 0)
            {
                lines.Add("path unknown   : " + counts[Unknown]);
            }

            return lines;
        }
    }
}
