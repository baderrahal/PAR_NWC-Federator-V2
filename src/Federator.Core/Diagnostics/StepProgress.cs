using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// When a long step writes a PROGRESS line to the log, and what it says, F143. A line is due
    /// once the seconds of the setting have passed since the step began or since the last line,
    /// on the log's own clock, and at every piece where the setting is zero or below. It names
    /// the step, what the step is on, how far through it is and its seconds into the step. The
    /// one rule of when a line is due: ViewsProgress, FR-071, reads it for the VIEWS step.
    /// </summary>
    public sealed class StepProgress
    {
        private readonly double everySeconds;
        private readonly double startSeconds;
        private double lastSeconds;

        public StepProgress(double everySeconds, double startSeconds)
        {
            this.everySeconds = everySeconds;
            this.startSeconds = startSeconds;
            lastSeconds = startSeconds;
        }

        /// <summary>Whether a line is due at that moment of the log's clock.</summary>
        public bool Due(double nowSeconds)
        {
            return everySeconds <= 0.0 || nowSeconds - lastSeconds >= everySeconds;
        }

        /// <summary>Seconds since the step began, on the clock the lines read.</summary>
        public double SecondsIn(double nowSeconds)
        {
            return nowSeconds - startSeconds;
        }

        /// <summary>Starts the wait for the next line at that moment, for a caller that writes its own words.</summary>
        public void Said(double nowSeconds)
        {
            lastSeconds = nowSeconds;
        }

        /// <summary>
        /// The line at that point of the step, which starts the wait for the next one. A total
        /// below nought is a count not known, written UNKNOWN and never nought.
        /// </summary>
        public string Line(string step, string on, string piece, int done, int total, double nowSeconds)
        {
            Said(nowSeconds);

            return "PROGRESS " + step + " " + on + " " + piece + " " + Count(done, total) + ", "
                + SecondsIn(nowSeconds).ToString("0.#", CultureInfo.InvariantCulture) + "s into the step";
        }

        /// <summary>How far through, N of M, a total below nought written UNKNOWN and never nought.</summary>
        public static string Count(int done, int total)
        {
            return done.ToString(CultureInfo.InvariantCulture) + " of "
                + (total < 0 ? "UNKNOWN" : total.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// The live line's sentence of one piece, what it is on, the piece and how far, the one form
        /// the harvest's rows and pictures and the renumber's pictures say, F146.
        /// </summary>
        public static string Live(string on, string piece, int done, int total)
        {
            return on + ", " + piece + " " + Count(done, total);
        }

        /// <summary>The text of a PROGRESS line's .tsv row, the step, the piece and how far, F146.</summary>
        public static string RowText(string step, string piece, int done, int total)
        {
            return step + " " + piece + " " + Count(done, total);
        }
    }
}
