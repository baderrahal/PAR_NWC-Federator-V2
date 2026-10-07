using System.Globalization;

namespace Federator.Core.Views
{
    /// <summary>
    /// When the VIEWS step writes a progress line to the log, and what it says, FR-071. Set 03
    /// left the log still for up to 1269 s inside the step, log:4476 to 4477, so a busy step and
    /// a hung one looked alike. A line is due once the ProgressEverySeconds setting has passed
    /// since the step began or since the last line, on the log's own clock, and at every view
    /// where the setting is zero or below. It says the view it reached, how many it wrote and
    /// its seconds into the step.
    /// </summary>
    public sealed class ViewsProgress
    {
        private readonly double everySeconds;
        private readonly double startSeconds;
        private double lastSeconds;

        public ViewsProgress(double everySeconds, double startSeconds)
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

        /// <summary>The line at that point of the step, which starts the wait for the next one.</summary>
        public string Line(int done, int planned, int written, double nowSeconds)
        {
            lastSeconds = nowSeconds;

            return "VIEWS view " + done + " of " + planned + ", " + written + " written, "
                + (nowSeconds - startSeconds).ToString("0.###", CultureInfo.InvariantCulture) + "s into the step";
        }
    }
}
