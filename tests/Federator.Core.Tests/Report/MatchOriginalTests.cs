using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Ours against theirs, on the three things Bader found by opening both side by side:
    /// the order of the tests, the id label, and the two number formats.
    ///
    /// The sort rule is not asserted from a description of it. It is re-derived from the
    /// two exports in samples\client-report and from the exchange file the tests came out
    /// of, on every test run, so a rule that drifts fails here.
    /// </summary>
    [TestFixture]
    public class MatchOriginalTests
    {
        private const string Root = "lcop_selection_set_tree";

        /// <summary>Every test block of one export, name and clash count, in page order.</summary>
        private static IList<KeyValuePair<string, int>> BlocksOf(string path)
        {
            string html = File.ReadAllText(path);
            List<KeyValuePair<string, int>> blocks = new List<KeyValuePair<string, int>>();

            foreach (string part in Regex.Split(html, "<table class=\"testSummaryTable\">"))
            {
                Match name = Regex.Match(part, "class=\"testName\">(.*?)</td>", RegexOptions.Singleline);

                if (!name.Success)
                {
                    continue;
                }

                MatchCollection cells = Regex.Matches(
                    part, "class=\"contentCell\">(.*?)</td>", RegexOptions.Singleline);

                int count;

                if (cells.Count > 1 && int.TryParse(cells[1].Groups[1].Value.Trim(), out count))
                {
                    blocks.Add(new KeyValuePair<string, int>(
                        System.Net.WebUtility.HtmlDecode(name.Groups[1].Value.Trim()), count));
                }
            }

            return blocks;
        }

        /// <summary>The order the tests sit in the exchange file they came out of.</summary>
        private static IList<string> ExchangeOrder(string repo)
        {
            // Samples.AllInOne finds it whichever of its two names it carries, and joins
            // the path with this machine's own separator. Joined with a backslash it was
            // not found off Windows, and the skip then said the checkout was missing a
            // file it holds.
            string path = Samples.AllInOne();

            if (!File.Exists(path))
            {
                Assert.Ignore("The exchange file is not in this checkout.");
            }

            List<string> names = new List<string>();

            foreach (Match found in Regex.Matches(
                File.ReadAllText(path), "<clashtest\\s[^>]*name=\"([^\"]*)\""))
            {
                names.Add(System.Net.WebUtility.HtmlDecode(found.Groups[1].Value));
            }

            return names;
        }

        // ---------- the sort rule, re-derived from their own files ----------

        // The one the brief asks for by name. Both exports are strictly descending by
        // clash count over all 1830 blocks.
        [Test]
        public void TheirReportsArePutTheMostClashesFirst()
        {
            foreach (string path in Exports())
            {
                IList<KeyValuePair<string, int>> blocks = BlocksOf(path);

                Assert.That(blocks.Count, Is.EqualTo(1830), Path.GetFileName(path));

                for (int i = 1; i < blocks.Count; i++)
                {
                    Assert.That(blocks[i].Value, Is.LessThanOrEqualTo(blocks[i - 1].Value),
                        Path.GetFileName(path) + " block " + (i + 1));
                }
            }
        }

        // The one the brief asks for by name. The second key, measured rather than assumed.
        // Every tie group in both exports is in the order the tests sit in the exchange
        // file, and none is alphabetical, including one group of 1807.
        [Test]
        public void TheirTieRuleIsTheOrderTheTestsWereCreatedInAndNotTheName()
        {
            string repo = Samples.Repo();

            if (repo == null)
            {
                Assert.Ignore("The checkout is not beside the test binaries.");
            }

            IList<string> exchange = ExchangeOrder(repo);
            Dictionary<string, int> at = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < exchange.Count; i++)
            {
                at[exchange[i]] = i;
            }

            bool sawALargeGroup = false;

            foreach (string path in Exports())
            {
                IList<KeyValuePair<string, int>> blocks = BlocksOf(path);
                int start = 0;

                while (start < blocks.Count)
                {
                    int end = start;

                    while (end + 1 < blocks.Count && blocks[end + 1].Value == blocks[start].Value)
                    {
                        end++;
                    }

                    if (end - start >= 2)
                    {
                        List<int> positions = new List<int>();
                        List<string> names = new List<string>();

                        for (int i = start; i <= end; i++)
                        {
                            Assert.That(at.ContainsKey(blocks[i].Key),
                                Is.True, blocks[i].Key + " is not in the exchange file");

                            positions.Add(at[blocks[i].Key]);
                            names.Add(blocks[i].Key);
                        }

                        List<int> ascending = new List<int>(positions);
                        ascending.Sort();

                        Assert.That(positions, Is.EqualTo(ascending),
                            Path.GetFileName(path) + ", the tie group of "
                            + blocks[start].Value + " clashes is not in the order the tests "
                            + "were created in");

                        List<string> alphabetical = new List<string>(names);
                        alphabetical.Sort(StringComparer.Ordinal);

                        Assert.That(names, Is.Not.EqualTo(alphabetical),
                            "the tie group is alphabetical, so the rule is not what was measured");

                        if (positions.Count > 100)
                        {
                            sawALargeGroup = true;
                        }
                    }

                    start = end + 1;
                }
            }

            Assert.That(sawALargeGroup, Is.True,
                "the evidence rests on a large tie group and none was found");
        }

        // And ours reproduces their exact sequence when fed the same tests in the same
        // original order with the same counts.
        [Test]
        public void OurOrderReproducesTheirsBlockForBlock()
        {
            string repo = Samples.Repo();

            if (repo == null)
            {
                Assert.Ignore("The checkout is not beside the test binaries.");
            }

            IList<string> exchange = ExchangeOrder(repo);

            foreach (string path in Exports())
            {
                IList<KeyValuePair<string, int>> theirs = BlocksOf(path);
                Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);

                foreach (KeyValuePair<string, int> block in theirs)
                {
                    counts[block.Key] = block.Value;
                }

                // Built in the exchange file's order, which is the order a run creates
                // them in, and with their counts.
                ClashReport report = new ClashReport("x", "x");

                foreach (string name in exchange)
                {
                    TestReport test = report.AddTest(name);

                    for (int i = 0; i < counts[name]; i++)
                    {
                        ClashRow row = new ClashRow();
                        row.Name = "Clash" + (i + 1);
                        test.Add(row);
                    }
                }

                IList<TestReport> ours = report.InReportOrder();

                Assert.That(ours.Count, Is.EqualTo(theirs.Count));

                for (int i = 0; i < ours.Count; i++)
                {
                    Assert.That(ours[i].Name, Is.EqualTo(theirs[i].Key),
                        Path.GetFileName(path) + " differs at block " + (i + 1));
                }
            }
        }

        private static IList<string> Exports()
        {
            string repo = Samples.Repo();

            if (repo == null)
            {
                Assert.Ignore("The checkout is not beside the test binaries.");
            }

            string folder = Path.Combine(repo, "samples", "client-report");

            if (!Directory.Exists(folder))
            {
                Assert.Ignore("The supplied client exports are not in this checkout.");
            }

            string[] found = Directory.GetFiles(folder, "*.html");
            Assert.That(found, Is.Not.Empty);
            return found;
        }

        // ---------- the id label ----------

        // The one the brief asks for by name. Ours read "Id: 990299" because the search
        // list puts Id first and the label was the matched property's display name. Theirs
        // reads "Element ID: 702888". We choose the label, so it is theirs.
        [Test]
        public void TheIdLabelIsTheirs()
        {
            Assert.That(ClientFormat.DefaultIdLabel, Is.EqualTo("Element ID"));

            ClashItem item = new ClashItem();
            item.ElementId = "990299";

            Assert.That(item.IdLabel, Is.EqualTo("Element ID"));
            Assert.That(item.ClientId(), Is.EqualTo("Element ID: 990299"));
            Assert.That(item.ClientId(), Does.Not.StartWith("Id:"));
        }

        [Test]
        public void WhichPropertySuppliedTheIdIsStillKept()
        {
            ClashItem item = new ClashItem();
            item.ElementId = "990299";
            item.IdFrom = "Id";

            // Renaming a value is only honest while what was renamed is still visible.
            Assert.That(item.IdFrom, Is.EqualTo("Id"));
            Assert.That(item.ClientId(), Is.EqualTo("Element ID: 990299"));
        }

        // ---------- the numbers ----------

        // The one the brief asks for by name.
        [Test]
        public void DistancesAndCoordinatesAreThreeDecimals()
        {
            Assert.That(ClientFormat.Fixed(-0.328083992004395), Is.EqualTo("-0.328"));
            Assert.That(ClientFormat.Fixed(33.170986), Is.EqualTo("33.171"));
            Assert.That(ClientFormat.Fixed(8.31), Is.EqualTo("8.310"),
                "trailing zeros are kept, theirs writes 8.310");
            Assert.That(ClientFormat.Fixed(-0.44), Is.EqualTo("-0.440"));
            Assert.That(ClientFormat.Fixed(0), Is.EqualTo("0.000"));
        }

        // The nine values in their files that are NOT three decimals, every one of them a
        // value that would otherwise read as zero when it is not.
        [Test]
        public void AValueTooSmallForThreeDecimalsIsNeverWrittenAsZero()
        {
            Assert.That(ClientFormat.Fixed(3.73e-14), Is.EqualTo("0.0000000000000373"));
            Assert.That(ClientFormat.Fixed(2.43e-14), Is.EqualTo("0.0000000000000243"));
            Assert.That(ClientFormat.Fixed(3.65e-14), Is.EqualTo("0.0000000000000365"));
            Assert.That(ClientFormat.Fixed(4.86e-14), Is.EqualTo("0.0000000000000486"));
            Assert.That(ClientFormat.Fixed(6.84e-08), Is.EqualTo("0.0000000684"));
            Assert.That(ClientFormat.Fixed(-0.000432), Is.EqualTo("-0.000432"));
        }

        [Test]
        public void NothingIsEverWrittenInExponentForm()
        {
            foreach (double value in new[] { 3.73e-14, 6.84e-08, -0.000432, 1e-30, 12345.6789 })
            {
                Assert.That(ClientFormat.Fixed(value), Does.Not.Contain("E").IgnoreCase,
                    value.ToString());
            }
        }

        // Their own files carry neither, on any coordinate, which is the whole reason for
        // the rule above.
        [Test]
        public void NoCoordinateInTheirFilesIsEverZeroPointZeroZeroZero()
        {
            foreach (string path in Exports())
            {
                string html = File.ReadAllText(path);

                foreach (Match found in Regex.Matches(html, @"[xyz]:(-?\d+(?:\.\d+)?)"))
                {
                    Assert.That(found.Groups[1].Value, Is.Not.EqualTo("0.000"),
                        Path.GetFileName(path));
                    Assert.That(found.Groups[1].Value, Is.Not.EqualTo("-0.000"),
                        Path.GetFileName(path));
                }
            }
        }

        [Test]
        public void TheXmlCarriesThoseSameNumbers()
        {
            ClashReport report = new ClashReport("1C07BC", "out");
            report.SetTreeRoot = Root;
            report.DocumentUnits = "m";

            TestReport test = report.AddTest("one");
            test.Tolerance = 0.025;
            test.ToleranceUnits = "m";
            test.State = TestState.FoundClashes;

            ClashRow row = new ClashRow();
            row.Name = "Clash1";
            row.Distance = -0.328083992004395;
            row.X = 33.170986;
            row.Y = 8.31;
            row.Z = 3.73e-14;
            test.Add(row);

            XDocument x = new ClashReportXml().Build(report);
            XElement result = x.Root.Descendants("clashresult").First();
            XElement point = result.Element("clashpoint").Element("pos3f");

            Assert.That(result.Attribute("distance").Value, Is.EqualTo("-0.328"));
            Assert.That(point.Attribute("x").Value, Is.EqualTo("33.171"));
            Assert.That(point.Attribute("y").Value, Is.EqualTo("8.310"));
            Assert.That(point.Attribute("z").Value, Is.EqualTo("0.0000000000000373"));
        }

        // ---------- the check catches what presence alone would pass ----------

        // The one the brief asks for by name. Every one of these pages has all fifteen
        // columns present, so the old check reported nothing wrong with any of them.
        [Test]
        public void TheCheckNamesADifferencePresenceAloneWouldPass()
        {
            string good = Page();

            Assert.That(PageCheck.Of(good, ".", "x").Passed, Is.True,
                "the page it is compared against should itself pass");

            // Order. Two columns swapped, both still present.
            string swapped = good
                .Replace("<td class=\"item1Header\">Item ID</td>", "@@ID@@")
                .Replace("<td class=\"item1Header\">Layer</td>",
                    "<td class=\"item1Header\">Item ID</td>")
                .Replace("@@ID@@", "<td class=\"item1Header\">Layer</td>");

            PageCheck order = PageCheck.Of(swapped, ".", "x");

            Assert.That(order.Passed, Is.False, "two columns swapped and it said nothing");
            Assert.That(order.FirstProblem, Does.Contain("Column 8"));
            Assert.That(order.FirstProblem, Does.Contain("\"Layer\""));
            Assert.That(order.FirstProblem, Does.Contain("\"Item ID\""));

            // Shape. The id label wrong, the cell still there and still filled.
            PageCheck label = PageCheck.Of(
                good.Replace("<i>Element ID</i>", "<i>Id</i>"), ".", "x");

            Assert.That(label.Passed, Is.False, "the wrong id label and it said nothing");
            Assert.That(label.FirstProblem, Does.Contain("Item ID cell is the wrong shape"));
            Assert.That(label.FirstProblem, Does.Contain("Id: 990299"));
            Assert.That(label.FirstProblem, Does.Contain(ClientShapes.ExampleItemId));

            // Shape again. The whole double in a coordinate.
            PageCheck point = PageCheck.Of(
                good.Replace("x:33.171, y:8.310, z:-0.441", "x:33.170986, y:8.31, z:-0.441"),
                ".", "x");

            Assert.That(point.Passed, Is.False, "the whole double and it said nothing");
            Assert.That(point.FirstProblem, Does.Contain("Clash Point cell is the wrong shape"));

            // Shape again. The distance carrying its full precision.
            PageCheck distance = PageCheck.Of(
                good.Replace(">-0.116<", ">-0.116000000001<"), ".", "x");

            Assert.That(distance.Passed, Is.False);
            Assert.That(distance.FirstProblem, Does.Contain("Distance cell is the wrong shape"));
        }

        // The one the brief asks for by name, for the order.
        [Test]
        public void TheCheckNamesTestsThatAreInTheWrongOrder()
        {
            PageCheck check = PageCheck.Of(TwoBlocks(1, 9), ".", "x");

            Assert.That(check.BlockCounts, Is.EqualTo(new[] { 1, 9 }));
            Assert.That(check.Passed, Is.False, "an empty test first and it said nothing");
            Assert.That(check.FirstProblem, Does.Contain("wrong order"));
            Assert.That(check.FirstProblem, Does.Contain("Block 1 holds 1"));
            Assert.That(check.FirstProblem, Does.Contain("block 2 holds 9"));

            Assert.That(PageCheck.Of(TwoBlocks(9, 1), ".", "x").BlockCounts,
                Is.EqualTo(new[] { 9, 1 }));
        }

        // ---------- a page to compare against, built the way the stylesheet builds one ----------

        private static string Page()
        {
            return Head() + Block("one", 1, Row()) + "</body></html>";
        }

        private static string TwoBlocks(int first, int second)
        {
            return Head() + Block("first", first, Row()) + Block("second", second, Row())
                + "</body></html>";
        }

        private static string Head()
        {
            return "<html><head><style>table.mainTable td.item1Content{}</style></head><body>";
        }

        private static string Block(string name, int clashes, string row)
        {
            string html = "<table class=\"testSummaryTable\"><tr class=\"headerRow\">"
                + "<td class=\"testName\">" + name + "</td>"
                + "<td class=\"headerCell\">Tolerance</td></tr>"
                + "<tr class=\"contentRow\"><td class=\"contentCell\">0.025m</td>"
                + "<td class=\"contentCell\">" + clashes + "</td></tr></table>"
                + "<table class=\"mainTable\"><tr class=\"headerRow\">"
                + "<td colspan=\"11\" class=\"generalHeader\"></td>"
                + "<td class=\"item1HeaderGroup\">Item 1</td>"
                + "<td class=\"item2HeaderGroup\">Item 2</td></tr>"
                + "<tr class=\"headerRow\">";

            foreach (string column in ClientReportColumns.General)
            {
                html += "<td class=\"generalHeader\">" + column + "</td>";
            }

            for (int side = 1; side <= 2; side++)
            {
                foreach (string column in ClientReportColumns.PerItem)
                {
                    html += "<td class=\"item" + side + "Header\">" + column + "</td>";
                }
            }

            html += "</tr>";

            for (int i = 0; i < clashes; i++)
            {
                html += row;
            }

            return html + "</table>";
        }

        private static string Row()
        {
            return "<tr class=\"contentRow\">"
                + "<td class=\"contentCell\"></td>"
                + "<td class=\"contentCell\">Clash1</td>"
                + "<td class=\"contentCell\">New</td>"
                + "<td class=\"contentCell\">-0.116</td>"
                + "<td class=\"contentCell\">A-1 : LGF</td>"
                + "<td class=\"contentCell\">Hard (Conservative)</td>"
                + "<td class=\"contentCell\">x:33.171, y:8.310, z:-0.441</td>"
                + "<td class=\"item1Content\"><i>Element ID</i>: 990299</td>"
                + "<td class=\"item1Content\">GRF</td>"
                + "<td class=\"item1Content\">ELE-CND-Standard</td>"
                + "<td class=\"item1Content\">Solid</td>"
                + "<td class=\"item2Content\"><i>Element ID</i>: 1369872</td>"
                + "<td class=\"item2Content\">GRF</td>"
                + "<td class=\"item2Content\">Concrete</td>"
                + "<td class=\"item2Content\">Solid</td>"
                + "</tr>";
        }
    }
}
