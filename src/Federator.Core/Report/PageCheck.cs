using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Federator.Core.Report
{
    /// <summary>
    /// Reads a client report page back off the disk and says what is in it.
    ///
    /// The file, never the object that produced it. That distinction has caught two faults
    /// already: a broken image link the object model reported as fine, and a test that was
    /// silently skipping instead of running. Bader has been opening every report in Excel
    /// and searching it by hand, and the tool wrote the file, so the tool can look.
    ///
    /// Nothing here throws. A check that fails is one line saying what is wrong, and a
    /// check that cannot run says that instead. Checking a report must never be the reason
    /// a run fails.
    /// </summary>
    public sealed class PageCheck
    {
        private readonly List<string> extraColumns = new List<string>();
        private readonly List<string> headerColumns = new List<string>();
        private readonly List<int> blockCounts = new List<int>();
        private readonly List<string> missingPictures = new List<string>();
        private readonly List<string> problems = new List<string>();

        private PageCheck()
        {
            FirstItemId = string.Empty;
            FirstClashPoint = string.Empty;
            FirstDistance = string.Empty;
            Tolerance = string.Empty;
            LogoReference = string.Empty;
            Path = string.Empty;
            CouldNotRead = string.Empty;
        }

        /// <summary>The page that was read.</summary>
        public string Path { get; private set; }

        /// <summary>Empty unless the file could not be read at all.</summary>
        public string CouldNotRead { get; private set; }

        public bool Ran
        {
            get { return CouldNotRead.Length == 0; }
        }

        /// <summary>Clash rows on the page, counted off the written file.</summary>
        public int Rows { get; private set; }

        public int ItemIdOnItem1 { get; private set; }

        public int ItemIdOnItem2 { get; private set; }

        /// <summary>The first one in full, so its shape is visible rather than described.</summary>
        public string FirstItemId { get; private set; }

        /// <summary>Any header on the page that is not one of the client's own.</summary>
        internal IList<string> ExtraColumns
        {
            get { return extraColumns.AsReadOnly(); }
        }

        /// <summary>The first clash point cell in full, so its shape is visible.</summary>
        public string FirstClashPoint { get; private set; }

        /// <summary>The first distance cell exactly as written.</summary>
        public string FirstDistance { get; private set; }

        /// <summary>The tolerance cell exactly as written, unit and all.</summary>
        public string Tolerance { get; private set; }

        public int Pictures { get; private set; }

        public int PicturesWithBackslash { get; private set; }

        public int PicturesOnDisk { get; private set; }

        /// <summary>The picture references that point at nothing, up to a readable few.</summary>
        internal IList<string> MissingPictures
        {
            get { return missingPictures.AsReadOnly(); }
        }

        public bool LogoReferenced { get; private set; }

        public bool LogoOnDisk { get; private set; }

        /// <summary>The logo reference exactly as written.</summary>
        public string LogoReference { get; private set; }

        /// <summary>Everything wrong, each as a plain sentence. Empty means nothing is.</summary>
        internal IList<string> Problems
        {
            get { return problems.AsReadOnly(); }
        }

        public bool Passed
        {
            get { return Ran && problems.Count == 0; }
        }

        /// <summary>
        /// The first thing that differs, or an empty string. One fault often has forty
        /// consequences and the first is the one to act on.
        /// </summary>
        internal string FirstProblem
        {
            get { return problems.Count == 0 ? string.Empty : problems[0]; }
        }

        /// <summary>
        /// Reads the page at this path. The folder it sits in is where a relative picture
        /// reference is resolved from, which is how the _files folder is found.
        /// </summary>
        public static PageCheck Of(string path)
        {
            PageCheck check = new PageCheck();
            check.Path = path ?? string.Empty;

            string html;

            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    check.CouldNotRead = "there is no file at " + check.Path;
                    return check;
                }

                html = File.ReadAllText(path);
            }
            catch (Exception error)
            {
                check.CouldNotRead = "it could not be opened, " + error.GetType().Name;
                return check;
            }

            try
            {
                check.Read(html, System.IO.Path.GetDirectoryName(path) ?? string.Empty);
            }
            catch (Exception error)
            {
                check.CouldNotRead = "it could not be read, " + error.GetType().Name;
            }

            return check;
        }

        /// <summary>The same, on text already in hand, so a test needs no file.</summary>
        internal static PageCheck Of(string html, string beside, string path)
        {
            PageCheck check = new PageCheck();
            check.Path = path ?? string.Empty;

            try
            {
                check.Read(html ?? string.Empty, beside ?? string.Empty);
            }
            catch (Exception error)
            {
                check.CouldNotRead = "it could not be read, " + error.GetType().Name;
            }

            return check;
        }

        private void Read(string html, string beside)
        {
            ReadColumns(html);
            ReadBlocks(html);
            ReadRows(html);
            ReadTolerance(html);
            ReadPictures(html, beside);
            ReadLogo(html, beside);
            Judge();
        }

        // ---------- the header ----------

        private void ReadColumns(string html)
        {
            Match header = Regex.Match(
                html, "<tr class=\"headerRow\">(?:(?!</tr>).)*?Clash Name.*?</tr>",
                RegexOptions.Singleline);

            if (!header.Success)
            {
                problems.Add("The page has no clash table at all. Nothing was written to check.");
                return;
            }

            foreach (Match cell in Regex.Matches(
                header.Value, "<td[^>]*>(.*?)</td>", RegexOptions.Singleline))
            {
                string name = Text(cell.Groups[1].Value);

                if (name.Length == 0)
                {
                    continue;
                }

                headerColumns.Add(name);

                if (ClientReportColumns.IsTheirs(name))
                {
                    continue;
                }

                if (!extraColumns.Contains(name))
                {
                    extraColumns.Add(name);
                }
            }

            // Presence is not enough. The old check passed while the order, the id label
            // and both number formats differed from the samples, because a column being
            // there says nothing about where it is.
            //
            // Only where there is something to compare. A group where nothing clashed has
            // no items in it, so the stylesheet leaves out every column that depends on
            // one, and a shorter header there is the ordinary answer rather than a fault.
            // The attribute, not the word. The page's own CSS block declares
            // td.item1Content, so looking for the bare word finds it on every page.
            if (html.IndexOf("class=\"item1Content\"", StringComparison.Ordinal) < 0)
            {
                return;
            }

            IList<string> theirs = ClientReportColumns.All();

            for (int i = 0; i < theirs.Count; i++)
            {
                string mine = i < headerColumns.Count ? headerColumns[i] : string.Empty;

                if (string.Equals(mine, theirs[i], StringComparison.Ordinal))
                {
                    continue;
                }

                problems.Add("Column " + (i + 1) + " of the clash table is wrong. Ours reads \""
                    + mine + "\" and the client's report reads \"" + theirs[i]
                    + "\". The whole order should be " + string.Join(", ", Array(theirs)) + ".");
                break;
            }
        }

        // ---------- the rows and their item ids ----------

        /// <summary>
        /// How many clashes each test block holds, in the order the page has them. Their
        /// report puts the most first, and ours followed the order the tests sat in the
        /// file, so it opened with four empty tests.
        /// </summary>
        private void ReadBlocks(string html)
        {
            foreach (Match block in Regex.Matches(
                html, "<table class=\"testSummaryTable\">(.*?)</table>", RegexOptions.Singleline))
            {
                Match cell = Regex.Match(
                    block.Groups[1].Value,
                    "class=\"contentCell\">(.*?)</td>.*?class=\"contentCell\">(.*?)</td>",
                    RegexOptions.Singleline);

                int count;

                if (cell.Success && int.TryParse(Text(cell.Groups[2].Value), out count))
                {
                    blockCounts.Add(count);
                }
            }

            for (int i = 1; i < blockCounts.Count; i++)
            {
                if (blockCounts[i] <= blockCounts[i - 1])
                {
                    continue;
                }

                problems.Add("The tests are in the wrong order. Block " + i + " holds "
                    + blockCounts[i - 1] + " clashes and block " + (i + 1) + " holds "
                    + blockCounts[i] + ". The client's report puts the most clashes first, "
                    + "so a reader is not scrolling past empty tests.");
                break;
            }
        }

        /// <summary>The clashes in each test block, in page order.</summary>
        internal IList<int> BlockCounts
        {
            get { return blockCounts.AsReadOnly(); }
        }

        private void ReadRows(string html)
        {
            foreach (Match row in Regex.Matches(
                html, "<tr class=\"(?:contentRow|clashGroupRow|childRow|childRowLast)\">(.*?)</tr>",
                RegexOptions.Singleline))
            {
                string body = row.Groups[1].Value;

                // A clash row is one carrying item cells. The test header block above the
                // table is a contentRow too and has none.
                if (body.IndexOf("item1Content", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                Rows++;

                string one = FirstCell(body, "item1Content");
                string two = FirstCell(body, "item2Content");

                if (FirstClashPoint.Length == 0)
                {
                    FirstClashPoint = Cell(body, "contentCell", ClientFormat.FirstItemColumn - 1);
                    FirstDistance = Cell(body, "contentCell", 3);
                }

                if (LooksLikeAnId(one))
                {
                    ItemIdOnItem1++;

                    if (FirstItemId.Length == 0)
                    {
                        FirstItemId = one;
                    }
                }

                if (LooksLikeAnId(two))
                {
                    ItemIdOnItem2++;

                    if (FirstItemId.Length == 0)
                    {
                        FirstItemId = two;
                    }
                }
            }
        }

        /// <summary>
        /// The nth cell of a row carrying this class, zero based. The Item ID cell is the
        /// FIRST cell of an item block, because Item ID is the first of the client's four
        /// per item columns.
        /// </summary>
        private static string Cell(string row, string cssClass, int nth)
        {
            MatchCollection found = Regex.Matches(
                row, "class=\"" + cssClass + "\"[^>]*>(.*?)</td>", RegexOptions.Singleline);

            return nth >= 0 && nth < found.Count
                ? Text(found[nth].Groups[1].Value)
                : string.Empty;
        }

        private static string FirstCell(string row, string cssClass)
        {
            Match found = Regex.Match(
                row, "class=\"" + cssClass + "\"[^>]*>(.*?)</td>", RegexOptions.Singleline);

            return found.Success ? Text(found.Groups[1].Value) : string.Empty;
        }

        /// <summary>
        /// An id cell reads "Element ID: 702888", a label then a colon then a value. An
        /// empty cell is the one this exists to count, because that is what a run wrote
        /// into all 426 of them.
        /// </summary>
        private static bool LooksLikeAnId(string cell)
        {
            if (cell.Length == 0)
            {
                return false;
            }

            int colon = cell.IndexOf(':');

            return colon > 0 && cell.Substring(colon + 1).Trim().Length > 0;
        }

        // ---------- the tolerance ----------

        private void ReadTolerance(string html)
        {
            // The first content cell of the first test summary table, which is where the
            // stylesheet writes the tolerance and its units.
            Match table = Regex.Match(
                html, "<table class=\"testSummaryTable\">(.*?)</table>", RegexOptions.Singleline);

            if (!table.Success)
            {
                return;
            }

            Match cell = Regex.Match(
                table.Value, "class=\"contentCell\">(.*?)</td>", RegexOptions.Singleline);

            if (cell.Success)
            {
                Tolerance = Text(cell.Groups[1].Value);
            }
        }

        // ---------- the pictures ----------

        private void ReadPictures(string html, string beside)
        {
            foreach (Match img in Regex.Matches(
                html, "<img[^>]*src=\"([^\"]*)\"[^>]*>", RegexOptions.IgnoreCase))
            {
                string src = img.Groups[1].Value.Trim();

                if (src.Length == 0 || IsLogo(img.Value))
                {
                    continue;
                }

                Pictures++;

                if (src.IndexOf('\\') >= 0)
                {
                    PicturesWithBackslash++;
                }

                if (OnDisk(src, beside))
                {
                    PicturesOnDisk++;
                }
                else if (missingPictures.Count < 5)
                {
                    missingPictures.Add(src);
                }
            }
        }

        private void ReadLogo(string html, string beside)
        {
            Match logo = Regex.Match(
                html, "<img[^>]*alt=\"logo\"[^>]*>", RegexOptions.IgnoreCase);

            if (!logo.Success)
            {
                return;
            }

            Match src = Regex.Match(logo.Value, "src=\"([^\"]*)\"", RegexOptions.IgnoreCase);
            LogoReference = src.Success ? src.Groups[1].Value.Trim() : string.Empty;
            LogoReferenced = LogoReference.Length > 0;
            LogoOnDisk = LogoReferenced && OnDisk(LogoReference, beside);
        }

        private static bool IsLogo(string img)
        {
            return img.IndexOf("alt=\"logo\"", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Is the file a reference points at really there. Relative and backslashed, which
        /// is what the page writes, so it is resolved against the page's own folder.
        /// </summary>
        private static bool OnDisk(string reference, string beside)
        {
            try
            {
                string tidied = reference.Trim().Replace('/', System.IO.Path.DirectorySeparatorChar)
                    .Replace('\\', System.IO.Path.DirectorySeparatorChar);

                if (tidied.Length == 0)
                {
                    return false;
                }

                string full = System.IO.Path.IsPathRooted(tidied)
                    ? tidied
                    : System.IO.Path.Combine(beside, tidied);

                return File.Exists(full);
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ---------- what is wrong ----------

        private void Judge()
        {
            if (Rows == 0)
            {
                // Not a fault on its own. A group where nothing clashed writes a page with
                // no rows, and that is the ordinary answer.
                return;
            }

            if (ItemIdOnItem1 < Rows || ItemIdOnItem2 < Rows)
            {
                problems.Add("The Item ID column is empty on some rows. Item 1 has one on "
                    + ItemIdOnItem1 + " of " + Rows + " and item 2 on " + ItemIdOnItem2
                    + " of " + Rows + ". The client's report has one on every row.");
            }

            if (extraColumns.Count > 0)
            {
                problems.Add("The page carries " + extraColumns.Count
                    + " column" + (extraColumns.Count == 1 ? "" : "s")
                    + " the client's report does not have, "
                    + string.Join(", ", extraColumns.ToArray())
                    + ". Ours belong in the workbook.");
            }

            if (Pictures > 0 && PicturesWithBackslash < Pictures)
            {
                problems.Add("Some picture references use a forward slash. "
                    + PicturesWithBackslash + " of " + Pictures
                    + " use a backslash, which is what the client's report uses.");
            }

            if (Pictures > PicturesOnDisk)
            {
                problems.Add("Some pictures the page points at are not on disk. "
                    + PicturesOnDisk + " of " + Pictures + " are there"
                    + (missingPictures.Count == 0
                        ? "."
                        : ". Missing: " + string.Join(", ", missingPictures.ToArray()) + "."));
            }

            // Shape, not just presence. Every one of these was wrong at some point while
            // the old check reported nothing, because it only asked whether the cell was
            // there at all.
            if (FirstItemId.Length > 0 && !ClientShapes.LooksLikeAnItemId(FirstItemId))
            {
                problems.Add("The Item ID cell is the wrong shape. Ours reads \""
                    + FirstItemId + "\" and the client's report reads \""
                    + ClientShapes.ExampleItemId + "\".");
            }

            if (FirstClashPoint.Length > 0
                && !ClientShapes.LooksLikeAClashPoint(FirstClashPoint))
            {
                problems.Add("The Clash Point cell is the wrong shape. Ours reads \""
                    + FirstClashPoint + "\" and the client's report reads \""
                    + ClientShapes.ExampleClashPoint + "\".");
            }

            if (FirstDistance.Length > 0 && !ClientShapes.LooksLikeADistance(FirstDistance))
            {
                problems.Add("The Distance cell is the wrong shape. Ours reads \""
                    + FirstDistance + "\" and the client's report reads \""
                    + ClientShapes.ExampleDistance + "\", three decimals.");
            }

            if (Tolerance.Length > 0 && !ClientShapes.LooksLikeATolerance(Tolerance))
            {
                problems.Add("The Tolerance cell is the wrong shape. Ours reads \""
                    + Tolerance + "\" and the client's report reads \""
                    + ClientShapes.ExampleTolerance + "\".");
            }

            if (LogoReferenced && !LogoOnDisk)
            {
                problems.Add("The page shows a logo at " + LogoReference
                    + " and that file is not beside it, so it will show as a broken picture.");
            }
        }

        // ---------- what it says ----------

        /// <summary>The block for the log.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (!Ran)
            {
                lines.Add("CHECK    the client report could not be checked, " + CouldNotRead);
                return lines;
            }

            lines.Add("CHECK    " + Rows + " clash "
                + (Rows == 1 ? "row" : "rows") + " on the page.");
            lines.Add("         Item ID filled on " + ItemIdOnItem1 + " of " + Rows
                + " for item 1 and " + ItemIdOnItem2 + " of " + Rows + " for item 2.");

            lines.Add(FirstItemId.Length > 0
                ? "         the first one reads " + FirstItemId
                : "         no Item ID was found at all.");

            lines.Add(extraColumns.Count == 0
                ? "         no column the client's report does not have."
                : "         columns that are not theirs: "
                    + string.Join(", ", extraColumns.ToArray()));

            lines.Add(Tolerance.Length > 0
                ? "         the tolerance cell reads " + Tolerance
                : "         no tolerance cell was found.");

            lines.Add("         " + Pictures + " picture "
                + (Pictures == 1 ? "reference" : "references") + ", "
                + PicturesWithBackslash + " with a backslash, "
                + PicturesOnDisk + " on disk.");

            foreach (string missing in missingPictures)
            {
                lines.Add("           not on disk: " + missing);
            }

            lines.Add(LogoReferenced
                ? "         the logo is " + LogoReference
                    + (LogoOnDisk ? " and it is on disk." : " and it is NOT on disk.")
                : "         no logo on the page.");

            foreach (string problem in problems)
            {
                lines.Add("         " + problem);
            }

            if (problems.Count == 0)
            {
                lines.Add("         Nothing wrong with it.");
            }

            lines.Add("         " + ClientReportColumns.ReadFrom());

            return lines;
        }

        /// <summary>
        /// One line for the run view, in the words a person would use. This is what makes
        /// a bad report visible without opening the log.
        /// </summary>
        public string Summary()
        {
            if (!Ran)
            {
                return "Client report not checked, " + CouldNotRead + ".";
            }

            if (Rows == 0)
            {
                return "Client report written, no clashes on it.";
            }

            if (problems.Count > 0)
            {
                return "Client report: " + problems[0]
                    + (problems.Count > 1
                        ? " And " + (problems.Count - 1) + " more."
                        : string.Empty);
            }

            return "Client report: Item ID filled on " + ItemIdOnItem1 + " of " + Rows
                + " rows, no extra columns, all " + PicturesOnDisk + " pictures on disk.";
        }

        private static string[] Array(IList<string> values)
        {
            string[] all = new string[values.Count];
            values.CopyTo(all, 0);
            return all;
        }

        private static string Text(string html)
        {
            return Regex.Replace(Regex.Replace(html, "<[^>]+>", string.Empty), "\\s+", " ").Trim();
        }
    }
}
