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
    /// The client's report is HTML (Tabular). Clash Detective cannot write an xlsx at all,
    /// so Bader exports the page and opens it in Excel, which is why the sample declares
    /// 53 columns with 17 populated and carries merged cells and absolute file:/// links.
    ///
    /// The layout therefore is not ours. It is clash_report_html_tabular.xsl in the
    /// install, and these run our own XML through that file so the page is theirs rather
    /// than a second implementation that can drift from it.
    ///
    /// Every test that needs the stylesheet says so and is ignored where the install is
    /// not on the machine, because a missing Navisworks is not a failing test.
    /// </summary>
    [TestFixture]
    public class HtmlTabularTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string Install = @"C:\Program Files\Autodesk\Navisworks Manage 2025";

        /// <summary>What theirs uses in every picture reference.</summary>
        private const string Sep = "\\";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(Path.GetTempPath(), "FederatorHtml", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void RemoveFolder()
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (IOException)
            {
                // A leftover temp folder is not worth failing a test over.
            }
        }

        private static string Stylesheet()
        {
            string path = StylesheetLocator.Find(Install, "en-US");

            if (path.Length == 0)
            {
                Assert.Ignore("Navisworks is not on this machine, so its stylesheet cannot be read.");
            }

            return path;
        }

        // ---------- the report the page is built from ----------

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-RPT-000001");
            report.SetTreeRoot = Root;
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 9, 1, 11, 0, 0);

            TestReport test = report.AddTest("BLD-ST-Walls-vs-BLD-AR-Floors");
            test.LeftLocator = Root + "/Structural/BLD-ST-Walls";
            test.RightLocator = Root + "/Architecture/BLD-AR-Floors";
            test.Tolerance = 0.025;
            test.ToleranceUnits = "m";
            test.TestTypeName = "hard_conservative";
            test.StatusWord = "OK";
            test.State = TestState.FoundClashes;
            test.Add(Clash());

            return report;
        }

        /// <summary>Clash1 of the client's own 1A02WE report, field for field.</summary>
        private static ClashRow Clash()
        {
            ClashRow row = new ClashRow();
            row.Name = "Clash1";
            row.Status = ClashStatus.New;
            row.Distance = -0.05;
            row.GridLocation = "B-1";
            row.Level = "LGF";
            row.Description = "Hard (Conservative)";
            row.Found = new DateTime(2026, 8, 31, 10, 15, 0);
            row.X = -2.100;
            row.Y = -2.726;
            row.Z = -0.051;
            row.ImageFile = "cd000001.jpg";
            row.ImageLink = "1104-PAR-1C07BC-ZZZ-BM-RPT-000001_files/cd000001.jpg";
            row.Left.ElementId = "2635048";
            row.Left.IdLabel = "Element ID";
            row.Left.Name = "Concrete, Cast In Situ Fc' 35MPa";
            row.Left.ItemType = "Solid";
            row.Left.Family = "Basic Wall";
            row.Right.ElementId = "814542";
            row.Right.IdLabel = "Element ID";
            row.Right.Name = "TRENCH";
            row.Right.ItemType = "Solid";
            row.Right.Family = "Floor";
            return row;
        }

        private string Transform(ClashReportXml writer, ClashReport report)
        {
            string path = Path.Combine(folder, report.OutputName + ".html");
            XDocument document = writer.Build(report);

            string written = new HtmlTabularWriter().Write(document, Stylesheet(), path);

            Assert.That(written, Is.EqualTo(path));
            Assert.That(File.Exists(path), Is.True);

            return File.ReadAllText(path);
        }

        /// <summary>The clash table's own header row, which is the second headerRow.</summary>
        private static IList<string> HeaderCells(string html)
        {
            MatchCollection rows = Regex.Matches(
                html, "<tr class=\"headerRow\">(.*?)</tr>", RegexOptions.Singleline);

            foreach (Match row in rows)
            {
                if (row.Groups[1].Value.IndexOf("Clash Name", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                List<string> cells = new List<string>();

                foreach (Match cell in Regex.Matches(
                    row.Groups[1].Value, "<td[^>]*>(.*?)</td>", RegexOptions.Singleline))
                {
                    cells.Add(Regex.Replace(cell.Groups[1].Value, "\\s+", " ").Trim());
                }

                return cells;
            }

            Assert.Fail("the page has no clash table header");
            return null;
        }

        // ---------- our XML feeds every column test the stylesheet makes ----------

        // The one the brief asks for by name. These are the show variables at the top of
        // clash_report_html_tabular.xsl, each one a boolean over an XPath, and this asserts
        // what our own XML answers to each.
        [Test]
        public void OurXmlAnswersEveryColumnTestTheStylesheetMakes()
        {
            XDocument x = new ClashReportXml().Build(Report());

            Assert.That(x.XPathHas("/exchange/batchtest/clashtests/clashtest/summary"), Is.True,
                "showSummary, the nine cell test header");
            Assert.That(x.XPathHas("//clashpoint"), Is.True, "showClashPoint");
            // showDateFound is deliberately FALSE. The client's report has no Date Found
            // column, measured on both 1A02WN and 1A04WN, and the stylesheet writes one
            // for any createddate it finds.
            Assert.That(x.XPathHas("//clashresult/createddate"), Is.False, "showDateFound");
            Assert.That(x.XPathHas("//clashobjects/clashobject/layer"), Is.True, "showLayer");
            Assert.That(x.XPathHas("//clashobjects/clashobject/objectattribute"), Is.True,
                "showItemID");
            Assert.That(x.XPathHas("//resultstatus"), Is.True, "showStatus");
            Assert.That(x.XPathHas("//gridlocation"), Is.True, "showGridLocation");

            // The three that were missing before this session, each one a whole column of
            // the client's report that our XML could not produce.
            Assert.That(x.XPathHas("//description"), Is.True, "showDescription");
            Assert.That(x.XPathHas("//clashobjects/clashobject/smarttags"), Is.True,
                "showQuickProperties, which is Item Name and Item Type");
            Assert.That(x.Root.Descendants("clashresult").First().Attribute("href"),
                Is.Not.Null, "showImage");

            // And the ones the client's own report does not carry either.
            Assert.That(x.XPathHas("//approveddate"), Is.False);
            Assert.That(x.XPathHas("//approvedby"), Is.False);
            Assert.That(x.XPathHas("//assignedto"), Is.False);
            Assert.That(x.XPathHas("//parentgroup"), Is.False);
            Assert.That(x.XPathHas("//comments/comment"), Is.False);
            Assert.That(x.XPathHas("//clashobjects/clashobject/pathlink"), Is.False);
        }

        // The Item ID cell is value-of over ./objectattribute/name, which takes the FIRST
        // one. Writing several put the item's Name in the id column.
        [Test]
        public void ThereIsExactlyOneObjectAttributeAndItIsTheId()
        {
            XDocument x = new ClashReportXml().Build(Report());

            foreach (XElement item in x.Root.Descendants("clashobject"))
            {
                Assert.That(item.Elements("objectattribute").Count(), Is.EqualTo(1));
                Assert.That(item.Element("objectattribute").Element("name").Value,
                    Is.EqualTo("Element ID"));
            }
        }

        [Test]
        public void AnAllZeroGuidWritesNoIdAtAll()
        {
            ClashReport report = Report();
            report.Tests[0].Rows[0].Left.ElementId = "00000000-0000-0000-0000-000000000000";

            XDocument x = new ClashReportXml().Build(report);
            XElement first = x.Root.Descendants("clashobject").First();

            Assert.That(first.Elements("objectattribute").Count(), Is.EqualTo(0),
                "a row of zeros reads like an id and identifies nothing");
        }

        // The stylesheet counts the smarttags of the FIRST clashobject and uses that count
        // for every row, so an item with fewer of them slides every later column sideways.
        [Test]
        public void EveryItemCarriesTheSameQuickProperties()
        {
            ClashReport report = Report();
            report.Tests[0].Rows[0].Right.Family = string.Empty;
            report.Tests[0].Rows[0].Right.ItemType = string.Empty;

            XDocument x = new ClashReportXml().Build(report);
            int expected = -1;

            foreach (XElement item in x.Root.Descendants("clashobject"))
            {
                int count = item.Element("smarttags").Elements("smarttag").Count();

                if (expected < 0)
                {
                    expected = count;
                }

                Assert.That(count, Is.EqualTo(expected),
                    "a differing count slides every column after it");
            }

            Assert.That(expected, Is.EqualTo(2), "exactly the two the client's report has");
        }

        // Their report has no Date Found column, and the stylesheet writes one for any
        // createddate it finds, so that element is ours too.
        [Test]
        public void ThePageHasNoDateFoundColumn()
        {
            ClashReportXml writer = new ClashReportXml();

            Assert.That(writer.Build(Report()).XPathHas("//createddate"), Is.False);
            Assert.That(HeaderCells(Transform(writer, Report())), Does.Not.Contain("Date Found"));
        }

        [Test]
        public void TheClientLayoutCarriesTheirTwoQuickPropertiesAndNoneOfOurs()
        {
            ClashReportXml writer = new ClashReportXml();

            XDocument x = writer.Build(Report());
            XElement tags = x.Root.Descendants("smarttags").First();

            List<string> names = new List<string>();

            foreach (XElement tag in tags.Elements("smarttag"))
            {
                names.Add(tag.Element("name").Value);
            }

            Assert.That(names, Is.EqualTo(new[] { "Item Name", "Item Type" }));
        }

        // ---------- the page itself, through their stylesheet ----------

        // The one the brief asks for by name.
        [Test]
        public void ThePageCarriesTheClientsColumnsInTheirOrder()
        {
            ClashReportXml writer = new ClashReportXml();

            IList<string> header = HeaderCells(Transform(writer, Report()));

            Assert.That(header, Is.EqualTo(new[]
            {
                "Image", "Clash Name", "Status", "Distance", "Grid Location", "Description",
                "Clash Point",
                "Item ID", "Layer", "Item Name", "Item Type",
                "Item ID", "Layer", "Item Name", "Item Type"
            }));
        }

        // The 1A04WE export has a Layer column on both items and the two committed earlier
        // do not, because the stylesheet only writes it when the XML carries a layer
        // element. Ours always does, so ours always has it.
        [Test]
        public void TheLayerColumnIsThereBecauseOurXmlCarriesALayer()
        {
            ClashReportXml writer = new ClashReportXml();

            string html = Transform(writer, Report());

            Assert.That(HeaderCells(html), Does.Contain("Layer"));
            Assert.That(html, Does.Contain(">LGF<"));
        }

        // The one the brief asks for by name. The stylesheet makes a column out of every
        // smarttag, so anything of ours in the XML becomes a column on the page the client
        // receives. Ours belong in the workbook.
        [Test]
        public void NoColumnOfOursReachesThePage()
        {
            string html = Transform(new ClashReportXml(), Report());
            IList<string> header = HeaderCells(html);

            foreach (string ours in new[]
            {
                "Family", "Type Name", "Material", "Source File", "Discipline", "Date Found"
            })
            {
                Assert.That(header, Does.Not.Contain(ours), ours + " reached the client page");
                Assert.That(html, Does.Not.Contain(">" + ours + "</td>"), ours);
            }

            // And theirs, which is the whole of it, exactly as both samples have it.
            Assert.That(header, Is.EqualTo(new[]
            {
                "Image", "Clash Name", "Status", "Distance", "Grid Location", "Description",
                "Clash Point",
                "Item ID", "Layer", "Item Name", "Item Type",
                "Item ID", "Layer", "Item Name", "Item Type"
            }));
        }

        [Test]
        public void ThePageCarriesTheTestHeaderInTheirWords()
        {
            string html = Transform(new ClashReportXml(), Report());

            foreach (string word in new[]
            {
                "Tolerance", "Clashes", "New", "Active", "Reviewed", "Approved", "Resolved",
                "Type", "Status"
            })
            {
                Assert.That(html, Does.Contain(">" + word + "</td>"), word);
            }

            Assert.That(html, Does.Contain("0.025m"), "the tolerance with its unit");
            Assert.That(html, Does.Contain("Element ID"));
            Assert.That(html, Does.Contain("B-1 : LGF"));
            Assert.That(html, Does.Contain("Clash1"));
        }

        [Test]
        public void ThePageLinksToTheClashPicture()
        {
            string html = Transform(new ClashReportXml(), Report());

            // A BACKSLASH, which is what both supplied reports write. The client opens
            // these in Excel on Windows.
            Assert.That(html, Does.Contain("cd000001.jpg"));
            Assert.That(html, Does.Contain(
                "1104-PAR-1C07BC-ZZZ-BM-RPT-000001_files" + Sep + "cd000001.jpg"));
            Assert.That(html, Does.Not.Contain(
                "1104-PAR-1C07BC-ZZZ-BM-RPT-000001_files/cd000001.jpg"));
            Assert.That(html, Does.Not.Contain("file:///"),
                "an absolute link only works on the machine that wrote it");
        }

        // ---------- the logo is ours to leave out ----------

        // The one the brief asks for by name. The stylesheet reads //logo/@href, so the
        // logo is data. Autodesk's own logo.jpg lives in the install's Images folder and
        // nothing here copies it.
        [Test]
        public void NoLogoIsWrittenWhenNoneIsPicked()
        {
            ClashReportXml writer = new ClashReportXml();

            Assert.That(writer.LogoHref, Is.Null.Or.Empty, "empty by default means no logo");

            XDocument x = writer.Build(Report());

            Assert.That(x.Root.Element("logo"), Is.Null);

            string html = Transform(writer, Report());

            Assert.That(html, Does.Not.Contain("logo.jpg"),
                "Autodesk's logo ships with their install and is not ours to redistribute");
        }

        [Test]
        public void APickedLogoIsTheOneThatGoesOnThePage()
        {
            ClashReportXml writer = new ClashReportXml();
            writer.LogoHref = "parsons.png";

            XDocument x = writer.Build(Report());

            Assert.That(x.Root.Element("logo").Attribute("href").Value, Is.EqualTo("parsons.png"));
            Assert.That(Transform(writer, Report()), Does.Contain("parsons.png"));
        }

        // ---------- a missing stylesheet turns one output off and nothing else ----------

        [Test]
        public void AMissingStylesheetWritesNothingAndDoesNotThrow()
        {
            string path = Path.Combine(folder, "nothing.html");

            Assert.That(new HtmlTabularWriter().Write(
                new ClashReportXml().Build(Report()), @"Q:\nowhere\missing.xsl", path),
                Is.EqualTo(string.Empty));
            Assert.That(File.Exists(path), Is.False);

            Assert.That(new HtmlTabularWriter().Write(
                new ClashReportXml().Build(Report()), string.Empty, path),
                Is.EqualTo(string.Empty));
        }

        [Test]
        public void ItSaysEveryPathItLookedAtWhenItCannotFindOne()
        {
            IList<string> lines = StylesheetLocator.WhyNotFound(@"C:\Nowhere", "fr-FR");
            string block = string.Join("\n", new List<string>(lines).ToArray());

            Assert.That(block, Does.Contain("clash_report_html_tabular.xsl"));
            Assert.That(block, Does.Contain(@"C:\Nowhere\fr-FR\stylesheets"));
            Assert.That(block, Does.Contain(@"C:\Nowhere\en-US\stylesheets"),
                "en-US is the fallback every install has");
            Assert.That(block, Does.Contain("workbook and the XML are unaffected"));
        }

        [Test]
        public void TheLanguageTheApplicationReportsIsTriedFirst()
        {
            IList<string> tried = StylesheetLocator.CandidatesIn(@"C:\NW", "de-DE");

            Assert.That(tried.Count, Is.EqualTo(2));
            Assert.That(tried[0], Does.Contain(@"\de-DE\"));
            Assert.That(tried[1], Does.Contain(@"\en-US\"));
        }

        [Test]
        public void OneLanguageIsOneCandidateRatherThanTheSameTwice()
        {
            Assert.That(StylesheetLocator.CandidatesIn(@"C:\NW", "en-US").Count, Is.EqualTo(1));
            Assert.That(StylesheetLocator.CandidatesIn(@"C:\NW", null).Count, Is.EqualTo(1));
            Assert.That(StylesheetLocator.CandidatesIn(string.Empty, "en-US").Count, Is.EqualTo(0));
        }

        [Test]
        public void ThePageSitsBesideTheWorkbookUnderTheSameName()
        {
            Assert.That(HtmlTabularWriter.PathFor(@"C:\out\1104-PAR-1C07BC-ZZZ-BM-RPT-000001.xlsx"),
                Is.EqualTo(@"C:\out\1104-PAR-1C07BC-ZZZ-BM-RPT-000001.html"));
            Assert.That(HtmlTabularWriter.PathFor(string.Empty), Is.EqualTo(string.Empty));
        }
    }

    /// <summary>Small helpers so the column tests read like the stylesheet does.</summary>
    internal static class XDocumentReading
    {
        public static bool XPathHas(this XDocument document, string path)
        {
            return System.Xml.XPath.Extensions.XPathSelectElements(document, path)
                .GetEnumerator().MoveNext();
        }

        public static XElement First(this IEnumerable<XElement> items)
        {
            foreach (XElement item in items)
            {
                return item;
            }

            Assert.Fail("nothing matched");
            return null;
        }

        public static int Count(this IEnumerable<XElement> items)
        {
            int count = 0;

            foreach (XElement item in items)
            {
                count++;
            }

            return count;
        }
    }
}
