using System;
using System.IO;
using System.Text;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// THE MACHINE READABLE LOG. WHAT IT IS FOR, WRITTEN HERE BECAUSE IT IS THE WRITER.
    ///
    /// The text log beside this file is written for a person: blocks, indenting,
    /// sentences, and a shape that is free to change when a line reads badly. Getting a
    /// number out of it means a regular expression against that shape, and every round
    /// that improves a line breaks whatever was reading it.
    ///
    /// This file is the same run in a shape nothing has to parse: one row per event,
    /// eight columns, tab separated, with a header. It opens in Excel with no import
    /// dialog and it reads in three lines of any scripting language. Twenty two groups
    /// over forty minutes is a few thousand rows.
    ///
    /// ONE WRITER, SO THE TWO CANNOT DRIFT. Nothing writes a row on its own. RunLog owns
    /// this object and every numbered line goes through one method that writes the text
    /// line and the row together, so a line cannot be added without its row and a row
    /// cannot say something the text log does not.
    ///
    /// WHAT IS NOT IN IT, AND THIS IS THE HONEST LIMIT. Only what goes through that one
    /// method. A caller that writes a sentence straight to the text log writes no row,
    /// which is right for a sentence and would be wrong for a count, so the rule is that
    /// anything carrying a NUMBER goes through the method and anything else does not.
    ///
    /// LINE BY LINE AND FLUSHED, exactly like the text log, because a run that dies mid
    /// group has to leave both files whole up to that moment. Nothing is buffered.
    ///
    /// AND IT NEVER STOPS A RUN. If this file cannot be opened the run carries on with
    /// the text log alone and one line says so. A second log is a convenience and the
    /// first one is the record.
    /// </summary>
    public sealed class RowLog : IDisposable
    {
        private readonly object gate = new object();
        private readonly FileStream stream;
        private readonly StreamWriter writer;
        private bool closed;

        private RowLog(string path, FileStream stream, string whyNot)
        {
            Path = path;
            WhyNot = whyNot;
            this.stream = stream;

            if (stream != null)
            {
                writer = new StreamWriter(stream, new UTF8Encoding(true));
            }
        }

        /// <summary>Where it is, or null when none could be opened.</summary>
        public string Path { get; private set; }

        /// <summary>Why there is none, in plain words, or null when there is one.</summary>
        public string WhyNot { get; private set; }

        public bool IsWritingToDisk
        {
            get { return writer != null; }
        }

        /// <summary>
        /// The same name as the text log with a different extension, so the two sit
        /// beside each other and nobody has to work out which row file goes with which
        /// run.
        /// </summary>
        public static string PathFor(string logPath)
        {
            if (string.IsNullOrEmpty(logPath))
            {
                return null;
            }

            string folder = System.IO.Path.GetDirectoryName(logPath);
            string name = System.IO.Path.GetFileNameWithoutExtension(logPath) + EventRow.FileNameExtension;

            return string.IsNullOrEmpty(folder) ? name : System.IO.Path.Combine(folder, name);
        }

        /// <summary>
        /// Opens the row file beside the text log and writes its header. Never throws:
        /// a run with no row file is a run with one log instead of two.
        /// </summary>
        public static RowLog StartBeside(string logPath)
        {
            string path = PathFor(logPath);

            if (string.IsNullOrEmpty(path))
            {
                return new RowLog(null, null, "there is no text log to sit beside");
            }

            try
            {
                FileStream stream = new FileStream(
                    path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 1024, false);

                RowLog rows = new RowLog(path, stream, null);
                rows.WriteLine(EventRow.Header());
                return rows;
            }
            catch (Exception error)
            {
                return new RowLog(
                    null, null, error.GetType().Name + ": " + error.Message);
            }
        }

        /// <summary>The one line the text log carries about this file.</summary>
        public string WhereItIs()
        {
            return IsWritingToDisk
                ? "ROWS     the machine readable log is " + Path
                : "ROWS     no machine readable log. " + Words.Or(WhyNot, "UNKNOWN")
                    + ". The text log is unaffected";
        }

        /// <summary>One row, flushed all the way to the disk before returning.</summary>
        public void Write(EventRow row)
        {
            if (row == null)
            {
                return;
            }

            WriteLine(row.Line());
        }

        private void WriteLine(string line)
        {
            lock (gate)
            {
                if (closed || writer == null)
                {
                    return;
                }

                try
                {
                    writer.WriteLine(line);
                    writer.Flush();

                    // The same Flush(true) the text log uses. A hard crash inside a
                    // Navisworks call leaves both files whole up to that moment.
                    stream.Flush(true);
                }
                catch (Exception)
                {
                    // Swallowed on purpose. This file is a convenience and the text log
                    // is the record. A disk that filled up must not stop a run here.
                }
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                if (closed)
                {
                    return;
                }

                closed = true;

                try
                {
                    if (writer != null)
                    {
                        writer.Flush();
                        stream.Flush(true);
                        writer.Dispose();
                    }
                }
                catch (Exception)
                {
                    // Closing a log is never worth throwing over.
                }
            }
        }
    }
}
