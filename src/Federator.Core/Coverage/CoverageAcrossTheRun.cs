using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;

namespace Federator.Core.Coverage
{
    /// <summary>
    /// The count check across the run, F127, the part of the design's section 1.7 that
    /// RESULT reads. Bader's answer to the lead's notes under Q112: a count in Clash Detective
    /// that differs from the workbook rows is a FAILED line in COVERAGE and RESULT, and the
    /// group keeps its own result. So every group's FAILED lines are kept whole and RESULT
    /// writes them all by default, CoverageSettings.FailedLinesInResult.
    ///
    /// THE TOTALS ARE KEPT BESIDE THE FAILED COUNT, so nought FAILED is never read without
    /// how many were compared. Both wave buildings skip their clash under F112's rule, so
    /// every test there is NOT COMPARED, and a run that compared nothing says UNKNOWN.
    ///
    /// It is handed to RunLog.WriteResultBlock by the run that made it and never kept on
    /// the log, the way OffCoordinatesAcrossTheRun is, so a second run of a window never
    /// carries the first run's groups. Nothing here touches a group's facts.
    /// </summary>
    public sealed class CoverageAcrossTheRun
    {
        /// <summary>
        /// The label of the totals line. Not the FAILED prefix, so a reader looking for FAILED
        /// lines never takes the count for one.
        /// </summary>
        private const string CheckedLabel = "COVERAGE checked : ";

        private readonly List<string> failedLines = new List<string>();
        private readonly int failedLinesShown;

        // Q127 answered A: each test once across the run by name, with the sums over the
        // groups beside it. A name is in the document once it was created or already there in
        // any group, run once it ran in any, and with clashes once it had them in any.
        private readonly HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> inTheDocumentOnce = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> ranOnce = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> withClashesOnce = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> notNamed = new HashSet<string>(StringComparer.Ordinal);
        private int notNamedPlaces;
        private int groupsWithTests;
        private int places;
        private int created;
        private int alreadyThere;
        private int notCreated;
        private int presenceUnknown;
        private int ran;
        private int withClashes;
        private int inTheDocumentNotRun;

        public CoverageAcrossTheRun(CoverageSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            failedLinesShown = settings.FailedLinesInResult;
        }

        /// <summary>How many groups were added, a group whose check could not be made among them.</summary>
        public int Groups { get; private set; }

        /// <summary>The groups added with no check, because their coverage could not be taken.</summary>
        public int GroupsNotChecked { get; private set; }

        /// <summary>The groups with at least one FAILED line.</summary>
        public int GroupsWithAFailedLine { get; private set; }

        public int Agree { get; private set; }

        /// <summary>The tests neither side holds, a test F77 did not create. Not a count that was checked.</summary>
        public int HeldByNeither { get; private set; }

        /// <summary>
        /// The tests Clash Detective holds that the picked file does not name, each name once across the
        /// groups as Q127 A counts every other test, so one old test in the NWFs of 46 groups is one. Never
        /// compared, since the workbook carries a block for the file's tests only, and said so.
        /// </summary>
        public int NotInTheXml
        {
            get { return notNamed.Count; }
        }

        public int Failed { get; private set; }

        public int NotCompared { get; private set; }

        /// <summary>
        /// One group's check, rolled in. A null check is a group whose coverage could not be
        /// taken, counted as not checked and never as a group that agreed.
        /// </summary>
        public void Add(string group, CountCheck check)
        {
            Groups++;

            if (check == null)
            {
                GroupsNotChecked++;
                return;
            }

            Agree += check.CountOf(CountVerdict.Agree);
            HeldByNeither += check.CountOf(CountVerdict.HeldByNeither);
            notNamedPlaces += check.NotInTheXml.Count;
            notNamed.UnionWith(check.NotInTheXml);
            NotCompared += check.CountOf(CountVerdict.NotCompared);

            IList<string> lines = check.FailedLines(group);

            if (lines.Count > 0)
            {
                Failed += lines.Count;
                GroupsWithAFailedLine++;
                failedLines.AddRange(lines);
            }
        }

        /// <summary>
        /// One group's tests, rolled in, FR-176 and Q127 answered A: the RESULT block counts the
        /// tests of the picked file once across the run by name, and the sums over the groups
        /// beside them. Counted apart from the check, so a group whose check could not be
        /// made still counts its tests.
        /// </summary>
        public void AddTests(string group, IList<TestCoverage> tests)
        {
            if (tests == null)
            {
                return;
            }

            groupsWithTests++;

            foreach (TestCoverage test in tests)
            {
                if (test == null)
                {
                    continue;
                }

                places++;
                names.Add(test.Name);

                switch (test.Presence)
                {
                    case TestPresence.CreatedThisRun:
                        created++;
                        inTheDocumentOnce.Add(test.Name);
                        break;
                    case TestPresence.AlreadyThere:
                        alreadyThere++;
                        inTheDocumentOnce.Add(test.Name);
                        break;
                    case TestPresence.NotInDocument:
                        notCreated++;
                        break;
                    default:
                        presenceUnknown++;
                        break;
                }

                if (test.Ran)
                {
                    ran++;
                    ranOnce.Add(test.Name);

                    if (test.Reason == CoverageReason.HasClashes)
                    {
                        withClashes++;
                        withClashesOnce.Add(test.Name);
                    }
                }
                else if (test.Presence == TestPresence.CreatedThisRun || test.Presence == TestPresence.AlreadyThere)
                {
                    inTheDocumentNotRun++;
                }
            }
        }

        /// <summary>
        /// The lines RESULT writes for the tests, Q127 answered A: each name once across the
        /// run, then the sums over the groups. None where no group's tests were handed in,
        /// since RESULT then says no coverage was taken.
        /// </summary>
        public IList<string> TestLines()
        {
            List<string> lines = new List<string>();

            if (groupsWithTests == 0)
            {
                return lines;
            }

            int neverInTheDocument = names.Count - inTheDocumentOnce.Count;
            int ranNeverWithAClash = ranOnce.Count - withClashesOnce.Count;

            lines.Add("COVERAGE tests counted            : " + names.Count + " names, each once across "
                + Words.Counted(groupsWithTests, "group", "groups"));
            lines.Add("COVERAGE in the document in one group at least : " + inTheDocumentOnce.Count
                + ", never in the document " + neverInTheDocument);
            lines.Add("COVERAGE run in one group at least : " + ranOnce.Count);
            lines.Add("COVERAGE with clashes in one group at least : " + withClashesOnce.Count);
            lines.Add("COVERAGE run, never with a clash  : " + ranNeverWithAClash);
            lines.Add("COVERAGE over " + Words.Counted(groupsWithTests, "group", "groups") + ", added up : " + places
                + " places, " + created + " created this run, " + alreadyThere + " already there, " + notCreated
                + " not created" + (presenceUnknown > 0 ? ", " + presenceUnknown + " whose presence is UNKNOWN" : string.Empty)
                + ", " + ran + " run, " + withClashes + " with clashes, " + (ran - withClashes) + " without, "
                + inTheDocumentNotRun + " in the document and not run");
            return lines;
        }

        /// <summary>
        /// The lines RESULT writes: the tests of Q127 where any were handed in, the totals of
        /// the check, then every FAILED line in the order the groups were added, or as many as
        /// the setting lists and a line counting the rest.
        /// </summary>
        public IList<string> ResultLines()
        {
            List<string> lines = new List<string>(TestLines());
            int compared = Agree + Failed;
            string notChecked = GroupsNotChecked > 0
                ? ", " + Words.Counted(GroupsNotChecked, "group", "groups") + " not checked, "
                    + (GroupsNotChecked == 1 ? "its tests" : "their tests") + " in none of these counts"
                : string.Empty;

            if (Groups == 0)
            {
                lines.Add(CheckedLabel + "UNKNOWN, no group's counts were handed in");
                return lines;
            }

            string neither = HeldByNeither > 0
                ? ", " + HeldByNeither + " held by neither side, tests no count was set beside"
                : string.Empty;

            if (compared == 0)
            {
                lines.Add(CheckedLabel + "UNKNOWN, 0 compared in " + Words.Counted(Groups, "group", "groups") + ", "
                    + NotCompared + " not compared" + neither + notChecked + ", so no count was checked");
                AddNotInTheXml(lines);
                return lines;
            }

            lines.Add(CheckedLabel + compared + " compared, " + Agree + " agree, " + Failed + " FAILED in "
                + GroupsWithAFailedLine + " of " + Words.Counted(Groups, "group", "groups") + ", " + NotCompared
                + " not compared" + neither + notChecked + ", every group keeps its own result, "
                + Words.Counted(compared + NotCompared + HeldByNeither, "test place", "test places")
                + " in the groups checked, a test once for each group it is in");
            AddNotInTheXml(lines);

            int shown = 0;

            foreach (string line in failedLines)
            {
                if (failedLinesShown > 0 && shown == failedLinesShown)
                {
                    break;
                }

                lines.Add(line);
                shown++;
            }

            if (failedLines.Count > shown)
            {
                int rest = failedLines.Count - shown;
                lines.Add("and " + rest + " more " + (rest == 1 ? "COVERAGE FAILED line" : "COVERAGE FAILED lines")
                    + ", counted and not listed here, because RESULT is set to list " + failedLinesShown);
            }

            return lines;
        }

        private void AddNotInTheXml(IList<string> lines)
        {
            if (NotInTheXml > 0)
            {
                lines.Add("COVERAGE not named    : " + Words.Counted(NotInTheXml, "test", "tests")
                    + " in Clash Detective that the picked file does not name"
                    + (notNamedPlaces != NotInTheXml
                        ? " (each name once here, " + notNamedPlaces + " places over the groups)"
                        : string.Empty)
                    + ", not compared, because the workbook"
                    + " carries a block for the file's tests only");
            }
        }

        /// <summary>
        /// The words RESULT closes on where no group failed and no error was logged but a
        /// count differs, so Nothing failed is never written beside a FAILED line. Null where
        /// no count differs.
        /// </summary>
        public string InsteadOfNothingFailed()
        {
            if (Failed == 0)
            {
                return null;
            }

            return "No group failed and no error was logged, and "
                + (Failed == 1
                    ? "1 coverage count differs, on its COVERAGE FAILED line above"
                    : Failed + " coverage counts differ, each on its COVERAGE FAILED line above")
                + ", every group keeping its own result";
        }

        /// <summary>The line RESULT writes where the run handed it no coverage, because a missing line reads as a check that did not run.</summary>
        public static string NoneTaken()
        {
            return CheckedLabel + "UNKNOWN, no coverage was taken in this run";
        }
    }
}
