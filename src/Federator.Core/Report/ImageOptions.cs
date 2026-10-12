using System;
using System.Collections.Generic;
using Federator.Core.Clash;

namespace Federator.Core.Report
{
    /// <summary>
    /// What the run does about clash images.
    ///
    /// On by default, because the accepted report has them and a report without them is
    /// not the thing the client agreed to receive.
    /// </summary>
    public sealed class ImageOptions
    {
        /// <summary>
        /// The size the accepted report's own pictures are, measured off all 60 of them
        /// on 2026-08-31. Every one is 1024 by 1024.
        /// </summary>
        public const int DefaultPixels = 1024;

        /// <summary>
        /// How many failures of the same kind end the run. Read off the clash step's own
        /// guard rather than typed again, because the rule is that both stop the same way
        /// after the same count. Fifty images that all failed the same way is a broken
        /// run, not a bad model.
        /// </summary>
        public const int DefaultStopAfterFailures = Federator.Core.Clash.RepeatedFailureGuard.DefaultThreshold;

        private readonly HashSet<ClashStatus> statuses = new HashSet<ClashStatus>();

        public ImageOptions()
        {
            Write = true;
            CapPerTest = 0;
            Width = DefaultPixels;
            Height = DefaultPixels;
            EmbedThumbnail = false;
            StopAfterFailures = DefaultStopAfterFailures;
            OnlyTheTestsModels = true;
            GhostTheOtherItems = false;

            // New, Active and Reviewed. The same three Navisworks itself calls open, but
            // set out here in its own right, because this is which clashes are worth a
            // picture and that is a different question from what the matrix counts.
            foreach (ClashStatus status in OpenClashes.StatusesFor(OpenClashCount.NavisworksOpen))
            {
                statuses.Add(status);
            }
        }

        /// <summary>
        /// On, and the window no longer sets it. The accepted report has images, so ours
        /// does too. Off only when every status is unticked, which is no picture said a
        /// longer way. F34.
        /// </summary>
        public bool Write { get; set; }

        /// <summary>
        /// The most images one test may write, or zero for no cap, which is the default.
        /// A cap is there for the test that finds 1244 clashes and would otherwise spend
        /// the whole run rendering them.
        /// </summary>
        public int CapPerTest { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        /// <summary>
        /// Off. The accepted report links to its pictures and does not paste them into
        /// cells, so a thumbnail in the cell is an addition rather than a match.
        /// </summary>
        public bool EmbedThumbnail { get; set; }

        /// <summary>How many failures in a row end the run. Zero switches the guard off.</summary>
        public int StopAfterFailures { get; set; }

        /// <summary>
        /// On. Each test's pictures are taken with only the models its two sides live in shown,
        /// every other model hidden once before them and the hidden state put back after, F145,
        /// Bader's choice of Q150 on 2026-10-11. PictureModels holds the rule.
        /// </summary>
        public bool OnlyTheTestsModels { get; set; }

        /// <summary>
        /// Off. For each group's picture pass the add-in asks Clash Detective to hide the other items,
        /// setting its Hide Other on and putting the user's value back after, so the pictures do not
        /// hang on each PC's display settings, F150, Bader's answer to Q151. On, it asks for them shown,
        /// Hide Other set off, the ghosted look of the accepted reports where Dim Other reads on.
        /// Whether the pictures follow is UNKNOWN until a run's picture sizes show it. OtherItemsInPictures
        /// holds the rule.
        /// </summary>
        public bool GhostTheOtherItems { get; set; }

        /// <summary>Is this clash one to render.</summary>
        public bool Wants(ClashStatus status)
        {
            return Write && statuses.Contains(status);
        }

        /// <summary>
        /// Whether a picture of this row is taken now: pictures on, the run not stopped by the
        /// pictures' guard, FR-076, the row of a status asked for and its test under the cap. A row
        /// passed over on its status or the cap is counted on the report's tally here and nowhere
        /// else, so the writer and the picture pass, which reads this before it resolves the row's
        /// result, count it once. F149, the reviewer's eighth point on attempt 1: a row that takes
        /// no picture costs no walk to its result, and a stretch none of whose rows takes one is
        /// not resolved or hidden for. It replaced F145's WouldTakeAPicture over a test's tally,
        /// since a stretch is passed over row by row and the cap is read as its pictures are taken.
        /// A guard of null is a run with no guard.
        /// </summary>
        public bool TakesAPictureOf(ClashReport report, TestReport test, ClashRow row, RepeatedFailureGuard guard)
        {
            if (report == null || test == null || row == null || !Write)
            {
                return false;
            }

            // Nothing is counted once the guard has fired, since the run stops and the rest is not attempted.
            if (guard != null && guard.ShouldStopTheRun)
            {
                return false;
            }

            if (!Wants(row.Status))
            {
                report.Images.WrongStatus();
                return false;
            }

            if (!RoomFor(test.ImageCount))
            {
                report.Images.CappedOne();
                return false;
            }

            return true;
        }

        /// <summary>
        /// The rows from one on that ONE fault left with no picture asked for, F149: their test not
        /// found again once every test has run, a throw that ended their stretch of pictures, or one
        /// clash not found again in its test. Each is counted on the report's tally as the writer
        /// would have counted it had it been asked for: a row of a status not asked for passed over
        /// on its status, then as many as the room under the cap counted as failed, the room taken
        /// as if each had been written, and the rest passed over on the cap. The guard is told ONCE,
        /// for the one fault, under its reason, and only where a row was counted as failed, the
        /// second reading's blocking point: told once a row, one ObjectDisposedException at row 11
        /// of a 1556 row test fired the fifty in a row guard and stopped the run saying fifty images
        /// failed when not one render was tried. Nothing is counted once the guard has fired. A row
        /// that already has its picture is not one of them. A guard of null is a run with no guard.
        /// </summary>
        public RowsLeftCount CountTheRowsLeft(
            ClashReport report, TestReport test, IList<ClashRow> rows, int from, string reason, RepeatedFailureGuard guard)
        {
            RowsLeftCount counted = new RowsLeftCount();

            if (report == null || test == null || rows == null || !Write || (guard != null && guard.ShouldStopTheRun))
            {
                return counted;
            }

            int taken = test.ImageCount;

            for (int i = Math.Max(0, from); i < rows.Count; i++)
            {
                ClashRow row = rows[i];

                if (row == null || row.HasImage)
                {
                    continue;
                }

                if (!Wants(row.Status))
                {
                    report.Images.WrongStatus();
                    counted.OnItsStatus();
                    continue;
                }

                if (!RoomFor(taken))
                {
                    report.Images.CappedOne();
                    counted.OnTheCap();
                    continue;
                }

                taken++;
                report.Images.RenderFailed(row.Name, 0.0);
                counted.FailedOne(row);
            }

            if (counted.Failed.Count > 0 && guard != null)
            {
                guard.RecordFailure(reason);
                counted.ToldTheGuard();
            }

            return counted;
        }

        /// <summary>
        /// Is there room under the cap for one more on this test. The count is what has
        /// already been written for it.
        /// </summary>
        public bool RoomFor(int alreadyWritten)
        {
            return CapPerTest <= 0 || alreadyWritten < CapPerTest;
        }

        /// <summary>Only the statuses asked for, in the order the tally reports them.</summary>
        public IList<ClashStatus> ChosenStatuses()
        {
            List<ClashStatus> chosen = new List<ClashStatus>();

            foreach (ClashStatus status in ClashTally.AllStatuses)
            {
                if (statuses.Contains(status))
                {
                    chosen.Add(status);
                }
            }

            return chosen;
        }

        /// <summary>Replaces the status filter. Empty is refused, because it means no images at all.</summary>
        public void OnlyFor(IEnumerable<ClashStatus> wanted)
        {
            if (wanted == null)
            {
                throw new ArgumentNullException("wanted");
            }

            List<ClashStatus> asked = new List<ClashStatus>(wanted);

            if (asked.Count == 0)
            {
                throw new ArgumentException(
                    "An empty status filter writes no images. Switch images off instead, "
                    + "so the log says that is what was meant.",
                    "wanted");
            }

            statuses.Clear();

            foreach (ClashStatus status in asked)
            {
                statuses.Add(status);
            }
        }

        /// <summary>One line saying what these settings will do, for the log and the window.</summary>
        public string Describe()
        {
            if (!Write)
            {
                return "Images are off, so no picture is rendered and every Image cell is empty.";
            }

            string statusList = string.Join(", ", Names(ChosenStatuses()));
            string cap = CapPerTest > 0
                ? "at most " + CapPerTest + " per test"
                : "no cap per test";

            return "Images on, " + Width + " by " + Height + " pixels, " + statusList
                + " only, " + cap + ", "
                + (EmbedThumbnail ? "with a thumbnail in the cell" : "linked rather than pasted in")
                + (OnlyTheTestsModels ? ", only the models of each test shown" : ", every model shown")
                + (GhostTheOtherItems ? ", Clash Detective asked to show the other items" : ", Clash Detective asked to hide the other items")
                + ".";
        }

        private static string[] Names(IList<ClashStatus> chosen)
        {
            string[] names = new string[chosen.Count];

            for (int i = 0; i < chosen.Count; i++)
            {
                names[i] = chosen[i].ToString();
            }

            return names;
        }
    }
}
