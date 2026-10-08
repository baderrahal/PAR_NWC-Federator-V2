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

        // FR-061's side. What the first write that threw said, kept so the text log can say once that this file
        // stopped taking rows, and so the sentences RunLog writes about what this file holds stop saying it holds
        // the rows after that.
        private string fault;
        private bool faultTold;

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

        /// <summary>True while the file is open and has taken every row it was given. False when none opened and after a write threw.</summary>
        public bool IsWritingToDisk
        {
            get
            {
                lock (gate)
                {
                    return writer != null && fault == null;
                }
            }
        }

        /// <summary>True when the file opened and a write to it then threw, so it holds the rows before that and none after.</summary>
        public bool Stopped
        {
            get
            {
                lock (gate)
                {
                    return fault != null;
                }
            }
        }

        /// <summary>
        /// What the first failed write threw, type and message, the first time it is asked after the fault and null
        /// ever after or where there was none. The text log says it once, as it says its own file stopping, FR-057.
        /// </summary>
        internal string TakeTheFaultToTell()
        {
            lock (gate)
            {
                if (fault == null || faultTold)
                {
                    return null;
                }

                faultTold = true;
                return fault;
            }
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

                return Opened(path, stream);
            }
            catch (Exception error)
            {
                return new RowLog(null, null, Cause(error));
            }
        }

        /// <summary>
        /// The file once it is open. A header that could not be written is a file that could not be used, as it is
        /// for the text log, so it is given up with what threw as the reason and the line that names the file says
        /// there is none, where it would say the file is there and then say it holds nothing.
        /// </summary>
        internal static RowLog Opened(string path, FileStream stream)
        {
            RowLog rows = new RowLog(path, stream, null);
            rows.WriteLine(EventRow.Header());

            if (rows.fault == null)
            {
                return rows;
            }

            string cause = rows.fault;
            rows.Dispose();
            return new RowLog(null, null, cause);
        }

        /// <summary>
        /// The type and what it said, without the full stop or line break it ended on, because the sentences that
        /// carry it add their own, as the text log's FileStopped does.
        /// </summary>
        private static string Cause(Exception error)
        {
            return error.GetType().Name + ": " + (error.Message ?? string.Empty).TrimEnd('.', ' ', '\r', '\n');
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
                if (closed || writer == null || fault != null)
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
                catch (Exception error)
                {
                    // Never thrown on. This file is a convenience and the text log is the record, so a
                    // disk that filled up must not stop a run here. It is kept and told once instead of
                    // swallowed, because the text log goes on saying every collapsed line is in this file,
                    // and nothing after this row is.
                    fault = Cause(error);
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

                if (writer == null)
                {
                    return;
                }

                // Each step on its own, as RunLog.Dispose does, so a flush that fails after a full disk still
                // reaches the dispose that closes the handle. Closing a log is never worth throwing over.
                Quietly(delegate { writer.Flush(); });
                Quietly(delegate { stream.Flush(true); });
                Quietly(delegate { writer.Dispose(); });
            }
        }

        private static void Quietly(Action step)
        {
            try
            {
                step();
            }
            catch (Exception)
            {
            }
        }
    }
}
