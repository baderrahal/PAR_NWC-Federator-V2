using System;
using System.Collections.Generic;
using System.Net;

namespace Federator.Core.Report
{
    /// <summary>One test as the page the tool wrote carries it, read back off the file, F236.</summary>
    public sealed class PageTest
    {
        internal PageTest(string name, int rows)
        {
            Name = name ?? string.Empty;
            Rows = rows;
        }

        /// <summary>The test's name as the page writes it, its markup read back to the words.</summary>
        public string Name { get; private set; }

        /// <summary>The clash rows of its table.</summary>
        public int Rows { get; private set; }
    }

    /// <summary>
    /// The HTML page of a group read back test by test, F236, Bader's round of 19 Oct 2026, fix 6, the clash count check, off
    /// the shape Navisworks' own stylesheet gives the page from the clash XML: each test opens with its name in the cell of
    /// the class testName, and its clash table, the table of the class mainTable, holds one content row a clash. Read as
    /// text, the file being the thing read and never the object that wrote it. Nothing here throws on a page of another
    /// shape, it reads what it can.
    /// </summary>
    public sealed class PageTests
    {
        private const string NameMark = "class=\"testName\">";
        private const string MainTable = "class=\"mainTable\"";
        private const string ContentRow = "<tr class=\"contentRow\">";

        private readonly List<PageTest> tests = new List<PageTest>();

        private PageTests()
        {
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
                read.tests.Add(new PageTest(name, RowsInTheTable(text.Substring(start, end - start))));
                at = next;
            }

            return read;
        }

        /// <summary>The content rows of the block once its clash table opens, nought where it has none.</summary>
        private static int RowsInTheTable(string block)
        {
            int from = block.IndexOf(MainTable, StringComparison.Ordinal);

            if (from < 0)
            {
                return 0;
            }

            int count = 0;
            int at = block.IndexOf(ContentRow, from, StringComparison.Ordinal);

            while (at >= 0)
            {
                count++;
                at = block.IndexOf(ContentRow, at + ContentRow.Length, StringComparison.Ordinal);
            }

            return count;
        }
    }
}
