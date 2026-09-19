using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Reading one line of a CSV somebody typed, for the two small files this tool is
    /// handed, F83 and F72b. One rule in one place, because a second copy would drift on
    /// the quoting.
    ///
    /// A quoted cell may hold a comma and a doubled quote. A newline inside a quoted cell
    /// is NOT supported and never will be: these files have one row per line and are read
    /// line by line so a fault in one row cannot swallow the next twenty.
    /// </summary>
    public static class Csv
    {
        public static IList<string> Cells(string line)
        {
            List<string> cells = new List<string>();

            if (line == null)
            {
                return cells;
            }

            System.Text.StringBuilder cell = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c != '"')
                    {
                        cell.Append(c);
                        continue;
                    }

                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                        continue;
                    }

                    inQuotes = false;
                    continue;
                }

                if (c == '"' && cell.Length == 0)
                {
                    inQuotes = true;
                    continue;
                }

                if (c == ',')
                {
                    cells.Add(cell.ToString());
                    cell.Length = 0;
                    continue;
                }

                cell.Append(c);
            }

            cells.Add(cell.ToString());
            return cells;
        }
    }
}
