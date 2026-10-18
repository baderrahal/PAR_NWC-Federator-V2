using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Coverage;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// The message box a bad NWC opens inside TryAppendFile, F213, F210-R6. Measured in scan.md 5z-zi: a random file, a
    /// file cut short and a file another program holds open each open one window of the class #32770 with one Button,
    /// control id 2, and a Static whose text starts with the file's name, says it could not be opened and ends with its
    /// full path, and the append waits there for a person. The tool answers it the way a person's click on OK does,
    /// WM_COMMAND with the id of that one button, and the append returns false. This holds which window is answered,
    /// what it is answered with and the words the log and the group carry. It answers no other window: one whose text
    /// does not name the file being appended, or that holds no button or more than one, is left alone and only said.
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
        /// Whether a window of this class is a message box at all, the one test of the class. The watch reads the class
        /// of every window of the process and reads the children only of one this says is a box.
        /// </summary>
        public static bool IsAMessageBox(string windowClass)
        {
            return string.Equals(windowClass, BoxClass, StringComparison.Ordinal);
        }

        /// <summary>
        /// Whether a window is the box the append of this file opened: a message box whose one button is its only
        /// button and one of whose texts names the file. The name is compared without case, as Windows reads a path.
        /// </summary>
        public static bool IsTheBoxOf(string file, string windowClass, IList<BoxPart> parts)
        {
            if (string.IsNullOrEmpty(file) || !IsAMessageBox(windowClass) || parts == null)
            {
                return false;
            }

            return TheOneButton(parts) != null && TextNaming(file, parts) != null;
        }

        /// <summary>The control id to answer the box with, the id of its one button, or null where it holds none or two.</summary>
        public static int? ButtonToAnswer(IList<BoxPart> parts)
        {
            BoxPart button = parts == null ? null : TheOneButton(parts);
            return button == null ? (int?)null : button.ControlId;
        }

        /// <summary>
        /// What Navisworks said is wrong with the file, read off the box: the first line of the text naming the file,
        /// with the file's name in front of it taken off, such as could not be opened because the contents are corrupt
        /// or it is currently unavailable. UNKNOWN where the box holds no text naming the file.
        /// </summary>
        public static string Reason(string file, IList<BoxPart> parts)
        {
            string text = parts == null ? null : TextNaming(file, parts);

            if (text == null)
            {
                return "UNKNOWN";
            }

            string first = string.Empty;

            foreach (string line in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
            {
                if (line.Trim().Length > 0)
                {
                    first = line.Trim();
                    break;
                }
            }

            string name = Path.GetFileName(file);

            if (first.StartsWith(name, StringComparison.OrdinalIgnoreCase))
            {
                first = first.Substring(name.Length).Trim();
            }

            first = first.TrimEnd('.').Trim();
            return first.Length == 0 ? "UNKNOWN" : first;
        }

        /// <summary>The log line of a box of the file whose one button the tool pressed, the first time it pressed it.</summary>
        public static string AnsweredLine(string file, int buttonId, string reason, long afterMilliseconds)
        {
            return "APPEND   Navisworks opened a message box on " + file + " and waited for a person. The tool pressed its one "
                + "button, id " + buttonId + ", after " + afterMilliseconds + " ms. Navisworks said the file " + reason;
        }

        /// <summary>The log line of a box of the file still standing after the tool pressed its button this many times.</summary>
        public static string StillStandsLine(string file, int presses)
        {
            return "APPEND   the message box on " + file + " still stands after the tool pressed its button " + presses
                + " times, so the run may wait for a person. The tool goes on pressing it while the file appends";
        }

        /// <summary>The log line of a box the tool answered on a file Navisworks then appended all the same.</summary>
        public static string AnsweredAndAppendedLine(string file)
        {
            return "APPEND   the tool answered a message box on " + file + " and Navisworks then appended the file, so the "
                + "group is not failed on it";
        }

        /// <summary>The log line of a box seen while a file was appended that is not the box of that file, left alone.</summary>
        public static string LeftAloneLine(string file, string windowText)
        {
            return "APPEND   a message box stood while " + file + " was appended and it is not the box of that file, so the "
                + "tool left it alone: \"" + CoverageBlock.OneLine(windowText).Trim() + "\"";
        }

        /// <summary>
        /// The words the group carries for a file Navisworks refused, so it ends FAILED with the file named, Bader's words
        /// of 18 Oct 2026: the file's full path, what Navisworks said and that the tool answered the box. No full stop at
        /// the end, since the judgement joins them with and.
        /// </summary>
        public static string GroupError(string file, string reason)
        {
            return file + " could not be appended, Navisworks said the file " + reason + " and the tool answered its message box";
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
                    && NamesWhole(part.Text, name))
                {
                    return part.Text;
                }
            }

            return null;
        }

        /// <summary>
        /// Whether the text holds the name as a whole name: at the start of the text or of a line, or after a slash, a
        /// backslash or a quote, and followed by the end, a space, a line break, a quote or a stop, so a box naming
        /// my-a.nwc or my a.nwc is never the box of a.nwc.
        /// </summary>
        private static bool NamesWhole(string text, string name)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            int at = text.IndexOf(name, StringComparison.OrdinalIgnoreCase);

            while (at >= 0)
            {
                int after = at + name.Length;
                bool startsWhole = at == 0 || "\\/\"'\r\n".IndexOf(text[at - 1]) >= 0;
                bool endsWhole = after == text.Length || " \t\r\n\"'.,:)".IndexOf(text[after]) >= 0;

                if (startsWhole && endsWhole)
                {
                    return true;
                }

                at = text.IndexOf(name, at + 1, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
