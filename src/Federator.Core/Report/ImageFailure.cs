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

        /// <summary>
        /// The test the clash was read from was not found again at its place once every test had
        /// run, F149, so none of the pictures read from it could be asked for.
        /// </summary>
        public const string TestNotFoundAgain = "the test it was read from was not found again";

        /// <summary>
        /// The clash was not found again in the test it was read from, by its index or by its
        /// name among its siblings, F149, so its picture could not be asked for.
        /// </summary>
        public const string ClashNotFoundAgain = "the clash was not found again in its test";

        private ImageFailure(string reason, string detail)
        {
            Reason = reason;
            Detail = detail;
        }

        /// <summary>What the guard counts. The same words for the same fault, whichever picture.</summary>
        public string Reason { get; private set; }

        /// <summary>What is said beside the reason on the line, or null where the reason says it all.</summary>
        public string Detail { get; private set; }

        /// <summary>
        /// The exception in full, its type, message, inner exceptions and stack trace, F157, or null
        /// where the failure was not a throw. Written under the failure line and never read by the
        /// guard, so it carries a path and a trace without making one reason per picture.
        /// </summary>
        public string Full { get; private set; }

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

            ImageFailure thrown = new ImageFailure(error.GetType().Name + ": " + error.Message, null);
            thrown.Full = error.ToString();
            return thrown;
        }

        /// <summary>
        /// A render or a save that threw while its picture was written to path, F120-R1. A
        /// framework message such as access denied names the file, which made one reason per
        /// path, so fifty such throws never stopped the run. The reason says the picture's path
        /// and the picture's folder where the message named them, in any capitals, and the
        /// path is said beside it on the line.
        /// </summary>
        public static ImageFailure BecauseItThrewWriting(Exception error, string path)
        {
            ImageFailure thrown = BecauseItThrew(error);

            if (string.IsNullOrEmpty(path))
            {
                return thrown;
            }

            string reason = WithoutThe(thrown.Reason, path, "the picture's path");
            int slash = path.LastIndexOfAny(new[] { '\\', '/' });
            string folder = slash > 0 ? path.Substring(0, slash) : null;
            reason = WithoutThe(reason, folder, "the picture's folder");
            ImageFailure written = new ImageFailure(reason, "at " + path);
            written.Full = thrown.Full;
            return written;
        }

        private static string WithoutThe(string text, string what, string instead)
        {
            if (string.IsNullOrEmpty(what))
            {
                return text;
            }

            System.Text.StringBuilder kept = new System.Text.StringBuilder();
            int from = 0;
            int at;

            while ((at = text.IndexOf(what, from, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                kept.Append(text, from, at - from).Append(instead);
                from = at + what.Length;
            }

            return kept.Append(text, from, text.Length - from).ToString();
        }

        /// <summary>
        /// A picture whose test was not found again once every test had run, F149, the breaker's
        /// first point on attempt 1, which left its cell empty and counted it nowhere. Where it was
        /// looked for is said beside the reason.
        /// </summary>
        public static ImageFailure BecauseItsTestWasNotFoundAgain(string where)
        {
            return new ImageFailure(TestNotFoundAgain, Beside(where));
        }

        /// <summary>A picture whose clash was not found again in its test, F149, with what was found instead beside the reason.</summary>
        public static ImageFailure BecauseItWasNotFoundAgain(string whyNot)
        {
            return new ImageFailure(ClashNotFoundAgain, Beside(whyNot));
        }

        /// <summary>
        /// A picture not yet asked for when a throw ended the pictures of its stretch, F149. It
        /// failed for that throw, so it is counted under the throw's own reason, the one a render
        /// that threw the same way is counted under, and the line says the throw ended its stretch.
        /// </summary>
        public static ImageFailure BecauseItsStretchEnded(Exception error)
        {
            ImageFailure thrown = BecauseItThrew(error);
            return new ImageFailure(thrown.Reason, "(it ended the pictures of its stretch before this one's turn)");
        }

        private static string Beside(string detail)
        {
            return string.IsNullOrEmpty(detail) ? null : "(" + detail + ")";
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
