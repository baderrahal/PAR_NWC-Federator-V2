using System;

namespace Federator.Core.Report
{
    /// <summary>
    /// Why one picture did not happen, in two parts: the reason, which is what the fifty
    /// failures guard counts and so carries no path, and the detail said beside it on the
    /// log line, which does. FR-076: a render that finished with no file carried the path
    /// in its reason, so fifty of them were fifty reasons and the guard never fired.
    ///
    /// The words for a run the guard stops live here too, beside RepeatedFailureGuard's for
    /// the clash step, worded for images rather than borrowed, because a line saying tests
    /// failed when what failed was pictures sends the next person looking in the wrong place.
    /// </summary>
    public sealed class ImageFailure
    {
        /// <summary>The render returned and the file is not on the disk, or is empty.</summary>
        public const string NoFileArrived = "the render finished but no file arrived";

        private ImageFailure(string reason, string detail)
        {
            Reason = reason;
            Detail = detail;
        }

        /// <summary>What the guard counts. The same words for the same fault, whichever picture.</summary>
        public string Reason { get; private set; }

        /// <summary>What is said beside the reason on the line, or null where the reason says it all.</summary>
        public string Detail { get; private set; }

        public static ImageFailure BecauseNoFileArrived(string path)
        {
            return new ImageFailure(NoFileArrived, "at " + path);
        }

        public static ImageFailure BecauseItThrew(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            return new ImageFailure(error.GetType().Name + ": " + error.Message, null);
        }

        /// <summary>The reason with its detail, for the log line.</summary>
        public string Line()
        {
            return string.IsNullOrEmpty(Detail) ? Reason : Reason + " " + Detail;
        }

        /// <summary>
        /// The log's line when the guard stops the run on pictures. The count is the streak
        /// and never the first so many, FR-129, since any number of good pictures can come
        /// before it. The reason is the streak's, so the line carries what went wrong.
        /// </summary>
        public static string RunStopped(int consecutive, string reason)
        {
            return "the last " + consecutive
                + " clash images all failed for the same reason, so the rest of the run "
                + "was not attempted. Switch images off to run without them. "
                + (reason ?? string.Empty);
        }

        /// <summary>
        /// The same for a label. It never carries what was thrown, because a label carries no
        /// framework message and no type name, and the log holds the line for it.
        /// </summary>
        public static string RunStoppedInPlainWords(int consecutive)
        {
            return "The run was stopped. " + consecutive
                + " clash images in a row all failed the same way, so the rest was not attempted. "
                + "The log says what the failure was.";
        }
    }
}
