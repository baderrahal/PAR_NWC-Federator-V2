namespace Federator.Core.Clash
{
    /// <summary>
    /// The status the report shows for a clash both tests of a mirrored pair find, F132,
    /// Bader's answer B to Q138. Under Q133 D such a clash is kept once, under the kept test,
    /// and its two copies can carry two statuses: an old test renamed under Q136 A keeps the
    /// statuses people set while the kept test's copy reads New, and Navisworks marks the
    /// kept test's copy Resolved when the kept test no longer finds it while the mirror still
    /// finds it live.
    ///
    /// HIS ORDER. A status a person sets, Reviewed or Approved, wins over the other statuses,
    /// and otherwise New or Active, a live clash, wins over Resolved. Where his words do not
    /// choose, two statuses both a person's that differ, Reviewed against Approved, or both
    /// live that differ, New against Active, the kept test's status stays, the lead's note
    /// under Q138. A status not among the five this Core names is UNKNOWN to the rule, and the
    /// kept test's status stays.
    ///
    /// Which statuses Navisworks sets by itself on this install is not measured, so Reviewed
    /// and Approved are read as a person's by their names, as Q138 says. This is not
    /// StatusesAPersonSet, which counts every status but New as a decision a rebuild must keep,
    /// F50, a different question.
    ///
    /// MirrorMerge reads it and sets the status on the kept test's row, TestReport.Restate,
    /// and the workbook and the clash XML read that row and its test's counts by status.
    /// </summary>
    public static class BothFoundStatus
    {
        private const int Unknown = -1;
        private const int Done = 0;
        private const int Live = 1;
        private const int APersons = 2;

        /// <summary>The status the report shows for the clash, the kept test's copy then the mirror's.</summary>
        public static ClashStatus Shown(ClashStatus underKept, ClashStatus underMirror)
        {
            int kept = Rank(underKept);

            return kept != Unknown && Rank(underMirror) > kept ? underMirror : underKept;
        }

        /// <summary>The words for the log of why Shown gives what it gives, without a test's name.</summary>
        public static string Why(ClashStatus underKept, ClashStatus underMirror)
        {
            int kept = Rank(underKept);
            int mirror = Rank(underMirror);

            if (underKept == underMirror)
            {
                return "both carry it";
            }

            if (kept == Unknown || mirror == Unknown)
            {
                return "a status not among the five is UNKNOWN to the rule and the kept test's stays";
            }

            if (kept == mirror)
            {
                return kept == APersons
                    ? "both are a person's and the kept test's stays"
                    : "both are live and the kept test's stays";
            }

            return kept == APersons || mirror == APersons
                ? "a status a person sets wins"
                : "a live status wins over Resolved";
        }

        private static int Rank(ClashStatus status)
        {
            switch (status)
            {
                case ClashStatus.Reviewed:
                case ClashStatus.Approved:
                    return APersons;
                case ClashStatus.New:
                case ClashStatus.Active:
                    return Live;
                case ClashStatus.Resolved:
                    return Done;
                default:
                    return Unknown;
            }
        }
    }
}
