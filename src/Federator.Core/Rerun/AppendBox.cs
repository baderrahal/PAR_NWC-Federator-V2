using System;
using System.Collections.Generic;
using System.IO;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// The message box a bad NWC opens inside TryAppendFile, F213, F210-R6. Measured in scan.md 5z-zi: a random file, a
    /// file cut short and a file another program holds open each open one window of the class #32770 with one Button,
    /// control id 2, and a Static whose text names the file and says it could not be opened, and the append waits there
    /// for a person. The tool answers it the way a person's click on OK does, WM_COMMAND with the id of that one button,
    /// and the append returns false. This holds which window is answered, what it is answered with and the words the log
    /// and the group carry. It answers no other window: one whose text does not name the file being appended, or that
    /// holds no button or more than one, is left alone and only said.
    /// </summary>
    public static class AppendBox
    {
        /// <summary>The window class of a Windows message box, the class 5z-zi read.</summary>
        public const string BoxClass = "#32770";

        /// <summary>The class of a button among a box's children.</summary>
        public const string ButtonClass = "Button";

        /// <summary>The class of a box's text among its children.</summary>
        public const string TextClass = "Static";

        /// <summary>
        /// Whether a window is the box the append of this file opened: a message box whose one button is its only
        /// button and one of whose texts names the file. The name is compared without case, as Windows reads a path.
        /// </summary>
        public static bool IsTheBoxOf(string file, string windowClass, IList<BoxPart> parts)
        {
            if (string.IsNullOrEmpty(file) || !string.Equals(windowClass, BoxClass, StringComparison.Ordinal) || parts == null)
            {
                return false;
            }

            return TheOneButton(parts) != null && TextNaming(file, parts) != null;
        }

        /// <summary>The control id to answer the box with, the id of its one button, or null where it holds none or two.</summary>
        public static int? ButtonToAnswer(IList<BoxPart> parts)
        {
            BoxPart button = TheOneButton(parts);
            return button == null ? (int?)null : button.ControlId;
        }

        /// <summary>
        /// What Navisworks said is wrong with the file, read off the box: its first paragraph with the file's name in
        /// front of it taken off, such as could not be opened because the contents are corrupt or it is currently
        /// unavailable. UNKNOWN where the box holds no text naming the file.
        /// </summary>
        public static string Reason(string file, IList<BoxPart> parts)
        {
            string text = parts == null ? null : TextNaming(file, parts);

            if (text == null)
            {
                return "UNKNOWN";
            }

            string first = text.Replace("\r\n", "\n").Split(new[] { "\n\n" }, StringSplitOptions.None)[0].Trim();
            string name = Path.GetFileName(file);

            if (first.StartsWith(name, StringComparison.OrdinalIgnoreCase))
            {
                first = first.Substring(name.Length).Trim();
            }

            first = first.TrimEnd('.').Trim();
            return first.Length == 0 ? "UNKNOWN" : first;
        }

        /// <summary>The log line of a box answered while a file was appended.</summary>
        public static string AnsweredLine(string file, string reason, long afterMilliseconds)
        {
            return "APPEND   Navisworks opened a message box on " + file + " and waited for a person. The tool answered it OK after "
                + afterMilliseconds + " ms, so the run goes on. Navisworks said the file " + reason;
        }

        /// <summary>The log line of a box seen while a file was appended that is not the box of that file, left alone.</summary>
        public static string LeftAloneLine(string file, string windowText)
        {
            return "APPEND   a message box stood while " + file + " was appended and it does not name that file, so the tool "
                + "left it alone: \"" + OneLine(windowText) + "\"";
        }

        /// <summary>
        /// The reason the group carries, so it ends FAILED with the file named, Bader's words of 18 Oct 2026. It names
        /// the file, what Navisworks said and that the box was answered by the tool and not by a person.
        /// </summary>
        public static string GroupError(string file, string reason)
        {
            return Path.GetFileName(file) + " could not be appended, Navisworks said the file " + reason
                + ". Its message box was answered by the tool, so the run went on with no one at the machine.";
        }

        private static BoxPart TheOneButton(IList<BoxPart> parts)
        {
            BoxPart found = null;

            foreach (BoxPart part in parts)
            {
                if (part != null && string.Equals(part.WindowClass, ButtonClass, StringComparison.Ordinal))
                {
                    if (found != null)
                    {
                        return null;
                    }

                    found = part;
                }
            }

            return found;
        }

        private static string TextNaming(string file, IList<BoxPart> parts)
        {
            string name = Path.GetFileName(file);

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            foreach (BoxPart part in parts)
            {
                if (part != null && string.Equals(part.WindowClass, TextClass, StringComparison.Ordinal)
                    && part.Text.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return part.Text;
                }
            }

            return null;
        }

        private static string OneLine(string text)
        {
            return (text ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }
}
