using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;

namespace Federator.Core.Report
{
    /// <summary>One test as the page the tool wrote carries it, read back off the file, F236.</summary>
    public sealed class PageTest
    {
        internal PageTest(string name, int clashes, int rows, int pictures)
        {
            Name = name ?? string.Empty;
            Clashes = clashes;
            Rows = rows;
            Pictures = pictures;
        }

        /// <summary>The test's name as the page writes it, its markup read back to the words.</summary>
        public string Name { get; private set; }

        /// <summary>The Clashes cell of its block, minus one where the cell holds no whole number.</summary>
        public int Clashes { get; private set; }

        /// <summary>The clash rows of its table.</summary>
        public int Rows { get; private set; }

        /// <summary>The clash rows of its table that show a picture.</summary>
        public int Pictures { get; private set; }
    }

    /// <summary>
    /// The HTML page of a group read back test by test, F236, Bader's round of 19 Oct 2026, fix 6, the clash count check, off
    /// the shape Navisworks' own stylesheet gives the page from the clash XML: each test opens with its name in the cell of
    /// the class testName, its Clashes cell is the second content cell after it, and its clash table, the table of the class
    /// mainTable, holds one content row a clash, a picture where the row holds an img. Read as text, the file being the thing
    /// read and never the object that wrote it. Nothing here throws on a page of another shape, it reads what it can.
    /// </summary>
    public sealed class PageTests
    {
        private const string NameMark = "class=\"testName\">";
        private const string ContentCell = "class=\"contentCell\">";
        private const string MainTable = "class=\"mainTable\"";
        private const string ContentRow = "<tr class=\"contentRow\">";
        private const string Picture = "<img ";

        private readonly List<PageTest> tests = new List<PageTest>();

        private PageTests()
        {
        }

        public ReadOnlyCollection<PageTest> Tests
        {
            get { return tests.AsReadOnly(); }
        }

        /// <summary>Every test of the page with that name, Ordinal and never trimmed.</summary>
        public IList<PageTest> Named(string name)
        {
            return tests.FindAll(t => string.Equals(t.Name, name, StringComparison.Ordinal));
        }

        public static PageTests Read(string page)
        {
            PageTests read = new PageTests();
            string text = page ?? string.Empty;
            int at = text.IndexOf(NameMark, StringComparison.Ordinal);

            while (at >= 0)
            {
                int start = at + NameMark.Length;
                int next = text.IndexOf(NameMark, start, StringComparison.Ordinal);
                int end = next < 0 ? text.Length : next;
                int close = text.IndexOf('<', start);
                string name = WebUtility.HtmlDecode(close < 0 || close > end ? text.Substring(start, end - start) : text.Substring(start, close - start));
                string block = text.Substring(start, end - start);
                read.tests.Add(new PageTest(name, ClashesOf(block), CountInTheTable(block, ContentRow), CountInTheTable(block, Picture)));
                at = next;
            }

            return read;
        }

        /// <summary>The second content cell of the block, the Clashes cell under its header, or minus one.</summary>
        private static int ClashesOf(string block)
        {
            int first = block.IndexOf(ContentCell, StringComparison.Ordinal);
            int second = first < 0 ? -1 : block.IndexOf(ContentCell, first + ContentCell.Length, StringComparison.Ordinal);

            if (second < 0)
            {
                return -1;
            }

            int start = second + ContentCell.Length;
            int close = block.IndexOf('<', start);
            int clashes;
            return close > start && int.TryParse(block.Substring(start, close - start).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out clashes)
                ? clashes
                : -1;
        }

        /// <summary>How many times the mark stands in the block once its clash table opens, nought where it has none.</summary>
        private static int CountInTheTable(string block, string mark)
        {
            int from = block.IndexOf(MainTable, StringComparison.Ordinal);

            if (from < 0)
            {
                return 0;
            }

            int count = 0;
            int at = block.IndexOf(mark, from, StringComparison.Ordinal);

            while (at >= 0)
            {
                count++;
                at = block.IndexOf(mark, at + mark.Length, StringComparison.Ordinal);
            }

            return count;
        }
    }
}
