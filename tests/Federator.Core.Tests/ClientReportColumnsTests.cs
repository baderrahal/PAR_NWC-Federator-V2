using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The client column set is not a list somebody typed. It came from the stylesheet in
    /// the Navisworks install and from the exports in samples\client-report.
    ///
    /// These re-read both of those and fail if the set no longer matches them, so the
    /// constant is checked against the files rather than trusted. That is the whole point:
    /// a column set that drifts from the client's own report is exactly the fault the
    /// report checks exist to catch, and it must not be able to hide in this tool's own
    /// idea of what the set is.
    /// </summary>
    [TestFixture]
    public class ClientReportColumnsTests
    {
        private const string Install = @"C:\Program Files\Autodesk\Navisworks Manage 2025";

        private static string RepoRoot()
        {
            DirectoryInfo at = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);

            while (at != null)
            {
                if (File.Exists(Path.Combine(at.FullName, "ParsonsNwcFederator.sln")))
                {
                    return at.FullName;
                }

                at = at.Parent;
            }

            return null;
        }

        private static IList<string> Samples()
        {
            string repo = RepoRoot();

            if (repo == null)
            {
                Assert.Ignore("The checkout is not beside the test binaries.");
            }

            string folder = Path.Combine(repo, @"samples\client-report");

            if (!Directory.Exists(folder))
            {
                Assert.Ignore("The supplied client exports are not in this checkout.");
            }

            string[] found = Directory.GetFiles(folder, "*.html");

            Assert.That(found, Is.Not.Empty, "there is no client export to read the set from");

            return found;
        }

        /// <summary>The header of one export, split the way the stylesheet writes it.</summary>
        private static void HeaderOf(
            string path, out IList<string> general, out IList<string> item1, out IList<string> item2)
        {
            string html = File.ReadAllText(path);
            int at = html.IndexOf("Clash Name", StringComparison.Ordinal);

            Assert.That(at, Is.GreaterThan(0), path + " has no clash table");

            string row = html.Substring(
                html.LastIndexOf("<tr", at, StringComparison.Ordinal),
                html.IndexOf("</tr>", at, StringComparison.Ordinal)
                    - html.LastIndexOf("<tr", at, StringComparison.Ordinal));

            general = Cells(row, "generalHeader");
            item1 = Cells(row, "item1Header");
            item2 = Cells(row, "item2Header");
        }

        private static IList<string> Cells(string row, string cssClass)
        {
            List<string> cells = new List<string>();

            foreach (Match cell in Regex.Matches(
                row, "class=\"" + cssClass + "\"[^>]*>(.*?)</td>", RegexOptions.Singleline))
            {
                string text = Regex.Replace(
                    Regex.Replace(cell.Groups[1].Value, "<[^>]+>", string.Empty),
                    "\\s+", " ").Trim();

                if (text.Length > 0)
                {
                    cells.Add(text);
                }
            }

            return cells;
        }

        // ---------- the set matches every supplied export ----------

        // The one the brief asks for by name. Where the set came from, proved rather than
        // stated.
        [Test]
        public void TheSetMatchesEverySuppliedClientExport()
        {
            foreach (string sample in Samples())
            {
                IList<string> general;
                IList<string> item1;
                IList<string> item2;

                HeaderOf(sample, out general, out item1, out item2);

                string which = Path.GetFileName(sample);

                Assert.That(general, Is.EqualTo(ClientReportColumns.General), which);
                Assert.That(item1, Is.EqualTo(ClientReportColumns.PerItem), which);
                Assert.That(item2, Is.EqualTo(ClientReportColumns.PerItem), which);
            }
        }

        [Test]
        public void EverySuppliedExportAgreesWithTheOthers()
        {
            IList<string> sample = Samples();
            string first = null;

            foreach (string path in sample)
            {
                IList<string> general;
                IList<string> item1;
                IList<string> item2;

                HeaderOf(path, out general, out item1, out item2);

                string whole = string.Join("|", new List<string>(general).ToArray())
                    + "||" + string.Join("|", new List<string>(item1).ToArray());

                if (first == null)
                {
                    first = whole;
                    continue;
                }

                Assert.That(whole, Is.EqualTo(first),
                    Path.GetFileName(path) + " has a different header from the first export");
            }
        }

        // ---------- and against the stylesheet ----------

        // Every one of theirs that is a stylesheet literal has to be in the stylesheet. The
        // two that are not are the quick properties, which come from the data.
        [Test]
        public void EveryColumnIsEitherAStylesheetLiteralOrAQuickProperty()
        {
            string path = StylesheetLocator.Find(Install, "en-US");

            if (path.Length == 0)
            {
                Assert.Ignore("Navisworks is not on this machine, so its stylesheet cannot be read.");
            }

            // The LIVE header template only. The stylesheet also carries an ItemHeaderCells
            // template that nothing calls, dead code in Autodesk's own file, and it writes
            // Item Type as a literal and reads ./Name with a capital where the rest of the
            // file reads name. Searching the whole file would read that dead template as
            // the truth.
            string xsl = File.ReadAllText(path);
            int from = xsl.IndexOf("name=\"mainTableHeader\"", StringComparison.Ordinal);

            Assert.That(from, Is.GreaterThan(0), "the stylesheet has no mainTableHeader");

            int to = xsl.IndexOf("</xsl:template>", from, StringComparison.Ordinal);
            string header = xsl.Substring(from, to - from);

            foreach (string column in ClientReportColumns.All())
            {
                if (IsQuickProperty(column))
                {
                    Assert.That(header, Does.Not.Contain(">" + column + "</td>"),
                        column + " is a literal in the live header, so it is not a quick "
                        + "property");
                    continue;
                }

                Assert.That(header, Does.Contain(">" + column + "</td>"),
                    column + " is not written in the live header template");
            }

            // And the quick properties reach it the only way they can, one column per
            // smarttag with the heading taken from the data.
            Assert.That(header, Does.Contain("smarttags)[1]/smarttag"));
            Assert.That(header, Does.Contain("<xsl:value-of select=\"name\"/>"));
        }

        private static bool IsQuickProperty(string column)
        {
            foreach (string quick in ClientReportColumns.QuickProperties)
            {
                if (string.Equals(quick, column, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        // ---------- what the set is used for ----------

        [Test]
        public void TheirsAreTheirsAndAnythingElseIsNot()
        {
            foreach (string column in ClientReportColumns.All())
            {
                Assert.That(ClientReportColumns.IsTheirs(column), Is.True, column);
            }

            foreach (string ours in new[]
            {
                "Family", "Type Name", "Material", "Source File", "Discipline", "Date Found",
                "Raw clashes", "", null
            })
            {
                Assert.That(ClientReportColumns.IsTheirs(ours), Is.False, ours ?? "null");
            }
        }

        [Test]
        public void TheWholeHeaderIsSevenPlusFourTwice()
        {
            Assert.That(ClientReportColumns.General.Length, Is.EqualTo(7));
            Assert.That(ClientReportColumns.PerItem.Length, Is.EqualTo(4));
            Assert.That(ClientReportColumns.All().Count, Is.EqualTo(15));
        }

        [Test]
        public void ItSaysWhereItCameFrom()
        {
            string said = ClientReportColumns.ReadFrom();

            Assert.That(said, Does.Contain("clash_report_html_tabular.xsl"));
            Assert.That(said, Does.Contain(@"samples\client-report"));
            Assert.That(said, Does.Contain("never from a list typed into this tool"));
        }
    }
}
