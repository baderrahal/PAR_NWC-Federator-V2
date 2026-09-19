using System;
using System.Collections.Generic;

namespace Federator.Core.Probe
{
    /// <summary>
    /// What the probe says when it has finished, F86. A block in the log and in the
    /// window, so the answer is read off the run rather than by opening the CSV and
    /// searching it.
    ///
    /// THE QUESTION THE PROBE WAS ASKED. Whether Fire Suppression pipework is told apart
    /// from domestic pipework by a property, and if it is, which property and what value.
    /// So the block says whether FS or Fire Suppression appears anywhere at all, and where.
    /// A NO is as useful an answer as a YES and is said as plainly, because a probe that
    /// only speaks up when it finds something reads as a probe that found nothing rather
    /// than as one that ran.
    /// </summary>
    public static class ProbeVerdict
    {
        /// <summary>The two spellings looked for, and the reason each is on the list.</summary>
        public static readonly string[] FireSuppressionWords = { "Fire Suppression", "FS" };

        /// <summary>
        /// Whether that text names fire suppression. "Fire Suppression" is matched
        /// anywhere, without case. "FS" is matched only as a WHOLE TOKEN, because it is
        /// two letters and matching it anywhere makes OFFSET, TRANSFER and DEFAULT all
        /// read as fire suppression. A token ends at anything that is not a letter or a
        /// digit, so FS, FS-01, FS_MAIN and "Duct FS" all match and OFFSET does not.
        /// </summary>
        public static bool NamesFireSuppression(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (text.IndexOf("Fire Suppression", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            for (int i = 0; i + 1 < text.Length; i++)
            {
                char first = text[i];
                char second = text[i + 1];

                bool isFs = (first == 'F' || first == 'f') && (second == 'S' || second == 's');

                if (!isFs)
                {
                    continue;
                }

                bool startsAToken = i == 0 || !IsTokenCharacter(text[i - 1]);
                bool endsAToken = i + 2 >= text.Length || !IsTokenCharacter(text[i + 2]);

                if (startsAToken && endsAToken)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Every row that names fire suppression, in the order the rows came. The whole
        /// row is looked at, tab, property and value alike, because the answer may be a
        /// property CALLED System Type whose value is FS, or a tab called Fire Suppression
        /// whose properties are ordinary, and either one answers the question.
        /// </summary>
        public static IList<ProbeRow> FireSuppressionRows(IEnumerable<ProbeRow> rows)
        {
            List<ProbeRow> found = new List<ProbeRow>();

            if (rows == null)
            {
                return found;
            }

            foreach (ProbeRow row in rows)
            {
                if (row == null || row.IsTheCapLine)
                {
                    continue;
                }

                if (NamesFireSuppression(row.Tab)
                    || NamesFireSuppression(row.Property)
                    || NamesFireSuppression(row.Value))
                {
                    found.Add(row);
                }
            }

            return found;
        }

        /// <summary>How many rows of the fire suppression list are named in the block.</summary>
        public const int ExamplesShown = 5;

        /// <summary>
        /// The block. Every number on it is READ off the tally or the rows and none is
        /// worked out from another, so a count that disagrees with the CSV is a fault
        /// rather than a rounding.
        /// </summary>
        public static IList<string> Lines(
            string modelName, string csvPath, ProbeTally tally, IList<string> categoriesAsked)
        {
            List<string> lines = new List<string>();

            lines.Add("PROBE    " + Said(modelName, "an unnamed model"));

            if (tally == null)
            {
                lines.Add("         nothing was read, so there is no verdict");
                return lines;
            }

            IList<ProbeRow> rows = tally.Rows();
            IList<string> found = tally.CategoriesFound();
            int asked = categoriesAsked == null ? 0 : categoriesAsked.Count;

            lines.Add("         categories asked for : " + asked);
            lines.Add("         categories found     : " + found.Count);

            if (categoriesAsked != null)
            {
                List<string> missing = new List<string>();

                foreach (string category in categoriesAsked)
                {
                    if (tally.ElementsIn(category) == 0)
                    {
                        missing.Add(category);
                    }
                }

                lines.Add("         categories with no elements at all : " + missing.Count
                    + (missing.Count == 0
                        ? string.Empty
                        : ", " + string.Join(", ", missing.ToArray())));
            }

            lines.Add("         properties           : " + tally.PropertyCount);
            lines.Add("         distinct values      : " + tally.DistinctValueCount);
            lines.Add("         rows written         : " + rows.Count);

            IList<string> capped = tally.Capped();

            lines.Add("         properties capped at " + tally.DistinctValueCap + " : " + capped.Count);

            foreach (string property in capped)
            {
                lines.Add("             " + property);
            }

            IList<ProbeRow> fire = FireSuppressionRows(rows);

            if (fire.Count == 0)
            {
                lines.Add("         FS and Fire Suppression appear in NO property tab, "
                    + "property name or value anywhere in this model");
            }
            else
            {
                lines.Add("         FS or Fire Suppression appears in " + fire.Count
                    + (fire.Count == 1 ? " row" : " rows") + ", first "
                    + Math.Min(ExamplesShown, fire.Count) + " shown");

                int shown = 0;

                foreach (ProbeRow row in fire)
                {
                    if (shown == ExamplesShown)
                    {
                        break;
                    }

                    lines.Add("             " + row);
                    shown++;
                }
            }

            lines.Add("         written to           : " + Said(csvPath, "UNKNOWN"));
            lines.Add("         nothing in the model was changed, nothing was saved and "
                + "nothing was published");

            return lines;
        }

        private static string Said(string value, string instead)
        {
            return string.IsNullOrEmpty(value) ? instead : value;
        }

        private static bool IsTokenCharacter(char c)
        {
            return char.IsLetterOrDigit(c);
        }
    }
}
