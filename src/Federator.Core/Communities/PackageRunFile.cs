using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;

namespace Federator.Core.Communities
{
    /// <summary>
    /// The one shape of the two files a run of packages passes between Navisworks starts, F214, the order and each
    /// package's result: a first line naming what the file is, then one line a fact, its key and its values joined by a
    /// tab, each value escaped by EventRow.Escape, the machine readable log's own rule, so a path holding a tab or a line
    /// break reads back whole. Written UTF-8 with no byte order mark, read whole.
    /// </summary>
    internal static class PackageRunFile
    {
        public static string Line(string key, params string[] values)
        {
            StringBuilder line = new StringBuilder(key);

            foreach (string value in values)
            {
                line.Append('\t').Append(EventRow.Escape(value ?? string.Empty));
            }

            return line.ToString();
        }

        public static void Write(string path, string header, IList<string> lines)
        {
            List<string> all = new List<string> { header };
            all.AddRange(lines);
            File.WriteAllText(path, string.Join("\r\n", all.ToArray()) + "\r\n", new UTF8Encoding(false));
        }

        /// <summary>
        /// Every line after the header, split at its tabs and unescaped, or null with why: the file not there, not read,
        /// empty, or a first line that is not the header asked for.
        /// </summary>
        public static IList<string[]> Read(string path, string header, out string why)
        {
            why = null;

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                why = "the file is not there, " + (string.IsNullOrEmpty(path) ? "no path was given" : path);
                return null;
            }

            string text;

            try
            {
                text = File.ReadAllText(path, new UTF8Encoding(false, true));
            }
            catch (Exception error)
            {
                why = "the file " + path + " could not be read, " + error.GetType().Name + ": " + error.Message;
                return null;
            }

            string[] lines = text.Replace("\r\n", "\n").Split('\n');

            if (lines.Length == 0 || !string.Equals(lines[0], header, StringComparison.Ordinal))
            {
                why = "the file " + path + " does not start with the line " + header;
                return null;
            }

            List<string[]> read = new List<string[]>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Length == 0)
                {
                    continue;
                }

                string[] parts = lines[i].Split('\t');

                for (int p = 1; p < parts.Length; p++)
                {
                    parts[p] = EventRow.Unescape(parts[p]);
                }

                read.Add(parts);
            }

            return read;
        }
    }
}
