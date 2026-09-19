using System;
using System.Globalization;
using System.Text;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// One row of the machine readable log: eight fields, tab separated.
    ///
    /// WHAT IT IS FOR. The text log is written for a person, and reading a number out of
    /// it means a regular expression over a line whose shape is free to change. This file
    /// is the same run in a shape a spreadsheet opens and a script reads: one row per
    /// event, one column per thing, no blocks, no indenting and no sentences that wrap.
    /// Twenty two groups over forty minutes is a few thousand rows, which sorts and
    /// filters in Excel in a second.
    ///
    /// THE TEXT LOG IS STILL THE ONE A PERSON READS and nothing about it gets worse for
    /// this file existing. Where the two disagree the text log is right, because it is
    /// the one that has been read against a real run.
    ///
    /// TAB SEPARATED AND NOT COMMA. A clash name, a set locator and a file path all hold
    /// commas and none of them holds a tab, and Excel opens a tab separated file without
    /// asking anything. A tab or a newline that does turn up inside a value is escaped,
    /// because one stray tab moves every column after it on that row and a reader has no
    /// way of telling.
    /// </summary>
    public sealed class EventRow
    {
        /// <summary>What separates the fields.</summary>
        public const string Separator = "\t";

        /// <summary>The extension, beside the text log's own.</summary>
        public const string FileNameExtension = ".tsv";

        public EventRow(
            string time,
            double seconds,
            string group,
            string step,
            string happening,
            string name,
            string number,
            string text)
        {
            Time = time;
            Seconds = seconds;
            Group = group;
            Step = step;
            Happening = happening;
            Name = name;
            Number = number;
            Text = text;
        }

        /// <summary>The wall clock, in the same format the text log stamps with.</summary>
        public string Time { get; private set; }

        /// <summary>Seconds since the run started, off the same monotonic clock.</summary>
        public double Seconds { get; private set; }

        /// <summary>The building, or empty outside a group.</summary>
        public string Group { get; private set; }

        /// <summary>The step, or empty outside one.</summary>
        public string Step { get; private set; }

        /// <summary>What happened, in one word this tool chose.</summary>
        public string Happening { get; private set; }

        /// <summary>What it happened to: a file, a step, a count, a test.</summary>
        public string Name { get; private set; }

        /// <summary>
        /// The number, as text rather than as a double, because a count, a size in bytes
        /// and a duration in seconds are three different kinds of number and forcing them
        /// through one type would lose either the precision or the meaning.
        /// </summary>
        public string Number { get; private set; }

        /// <summary>Anything else worth carrying, escaped like every other field.</summary>
        public string Text { get; private set; }

        /// <summary>
        /// The header, written once at the top of the file. The names are the ones the
        /// brief set and nothing reorders them, because a script written against this
        /// file reads by position as often as by name.
        /// </summary>
        public static string Header()
        {
            return string.Join(
                Separator,
                new[] { "time", "seconds", "group", "step", "event", "name", "number", "text" });
        }

        /// <summary>How many fields every row has. The header and every row agree.</summary>
        public const int FieldCount = 8;

        public string Line()
        {
            return string.Join(
                Separator,
                new[]
                {
                    Escape(Time),
                    Seconds.ToString("0.000", CultureInfo.InvariantCulture),
                    Escape(Group),
                    Escape(Step),
                    Escape(Happening),
                    Escape(Name),
                    Escape(Number),
                    Escape(Text)
                });
        }

        /// <summary>
        /// A tab, a newline, a carriage return or a backslash inside a value, written so
        /// it can be read back. The backslash goes FIRST, or unescaping a value that held
        /// one would turn it into whatever followed.
        /// </summary>
        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder text = new StringBuilder(value.Length);

            foreach (char one in value)
            {
                switch (one)
                {
                    case '\\': text.Append("\\\\"); break;
                    case '\t': text.Append("\\t"); break;
                    case '\r': text.Append("\\r"); break;
                    case '\n': text.Append("\\n"); break;
                    default: text.Append(one); break;
                }
            }

            return text.ToString();
        }

        /// <summary>
        /// The other way, so a test can prove the escaping is reversible rather than
        /// merely that it changed something.
        /// </summary>
        public static string Unescape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder text = new StringBuilder(value.Length);

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] != '\\' || i + 1 >= value.Length)
                {
                    text.Append(value[i]);
                    continue;
                }

                i++;

                switch (value[i])
                {
                    case '\\': text.Append('\\'); break;
                    case 't': text.Append('\t'); break;
                    case 'r': text.Append('\r'); break;
                    case 'n': text.Append('\n'); break;
                    default: text.Append('\\').Append(value[i]); break;
                }
            }

            return text.ToString();
        }

        /// <summary>A number for the Number column, with no thousands separators in it.</summary>
        public static string Count(int number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>A size or a duration, with three decimals and no separators.</summary>
        public static string Exact(double number)
        {
            return number.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
