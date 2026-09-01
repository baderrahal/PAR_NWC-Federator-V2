using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// What a real run on 2026-09-01 got wrong against two real Navisworks reports, and
    /// what fixed it. Every expected value here came out of those files:
    ///
    ///   samples\client-report\1104-PAR-1A02WN-XXX-BM-RPT-000001.html
    ///   samples\client-report\1104-PAR-1A04WN-XXX-BM-RPT-000001.html
    ///   samples\our-report\1104-PAR-1C07BC-ZZZ-BM-RPT-000001.html and .xml
    ///   docs\logs\run-20260901-191711.log
    /// </summary>
    [TestFixture]
    public class MatchColumnsTests
    {
        private const string Root = "lcop_selection_set_tree";

        // ---------- the value that would not read ----------

        // VariantData.ToString is the one member that returns a value whatever kind it
        // holds. Its IL switches on GetDataType and calls the matching accessor, so it
        // never throws, but it prefixes the kind name. This is that prefix coming off.
        [Test]
        public void TheKindPrefixComesOffTheFallbackValue()
        {
            Assert.That(VariantText.Clean("Int32:702888", "Int32"), Is.EqualTo("702888"));
            Assert.That(VariantText.Clean("DisplayString:Basic Wall", "DisplayString"),
                Is.EqualTo("Basic Wall"));
            Assert.That(VariantText.Clean("IdentifierString:abc", "IdentifierString"),
                Is.EqualTo("abc"));
            Assert.That(VariantText.Clean("Double:1.5", "Double"), Is.EqualTo("1.5"));
        }

        // A display string can hold a colon of its own, and eating half of it would be
        // worse than leaving the prefix on.
        [Test]
        public void OnlyTheKindThatWasReportedIsEverStripped()
        {
            Assert.That(VariantText.Clean("Element ID: 702888", "Int32"),
                Is.EqualTo("Element ID: 702888"));
            Assert.That(VariantText.Clean("Int32:702888", "DisplayString"),
                Is.EqualTo("Int32:702888"));
            Assert.That(VariantText.Clean("Level 03 : ROF", "DisplayString"),
                Is.EqualTo("Level 03 : ROF"));
        }

        [Test]
        public void NothingAtAllIsNothingRatherThanAThrow()
        {
            Assert.That(VariantText.Clean(null, "Int32"), Is.EqualTo(string.Empty));
            Assert.That(VariantText.Clean(string.Empty, "Int32"), Is.EqualTo(string.Empty));
            Assert.That(VariantText.Clean("Int32:702888", null), Is.EqualTo("Int32:702888"));
        }

        // The words ToString hands back when there is no value. None of them is a value.
        [Test]
        public void TheWordsThatMeanNoValueAreReadAsNothing()
        {
            foreach (string word in new[] { "None", "Disposed", "Unknown", "<null>", "", "  " })
            {
                Assert.That(VariantText.IsNothing(word), Is.True, "[" + word + "]");
            }

            Assert.That(VariantText.IsNothing("702888"), Is.False);
            Assert.That(VariantText.IsNothing("Basic Wall"), Is.False);
        }

        // ---------- the element id reaches the page ----------

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-RPT-000001");
            report.SetTreeRoot = Root;
            report.DocumentUnits = "ft";
            report.RunAt = new DateTime(2026, 9, 1, 19, 21, 0);

            TestReport test = report.AddTest("BLD-AR-Walls-vs-BLD-AR-Columns");
            test.LeftLocator = Root + "/Architecture/BLD-AR-Walls";
            test.RightLocator = Root + "/Architecture/BLD-AR-Columns";
            test.Tolerance = 0.2460629921;
            test.ToleranceUnits = "ft";
            test.TestTypeName = "hard_conservative";
            test.StatusWord = "OK";
            test.State = TestState.FoundClashes;

            ClashRow row = new ClashRow();
            row.Name = "Clash1";
            row.Status = ClashStatus.Active;
            row.Distance = -0.328084;
            row.GridLocation = "D-8 : LGF";
            row.Level = "LGF";
            row.Description = "Hard (Conservative)";
            row.Found = new DateTime(2026, 8, 31, 10, 15, 0);
            row.X = 33.170986;
            row.Y = -10.410352;
            row.Z = 0.328084;
            row.ImageFile = "cd000001.jpg";
            row.ImageLink = "1104-PAR-1C07BC-ZZZ-BM-RPT-000001_files/cd000001.jpg";

            // The element id that was empty on the real run, and the four properties that
            // followed it and were lost with it.
            row.Left.ElementId = "702888";
            row.Left.IdLabel = "Element ID";
            row.Left.Name = "PAR-CONC-FOUNDATION";
            row.Left.ItemType = "Solid";
            row.Left.Family = "Basic Wall";
            row.Left.Material = "Concrete";
            row.Left.SourceFile = @"C:\in\1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc";
            row.Left.Discipline = "AR";
            row.Right.ElementId = "707077";
            row.Right.IdLabel = "Element ID";
            row.Right.Name = "Mx_Exterior_EW6_ Alum Sheet (Dark)";
            row.Right.ItemType = "Solid";
            test.Add(row);

            return report;
        }

        private static XDocument Xml()
        {
            return new ClashReportXml().Build(Report());
        }

        // The one the brief asks for by name. Ours had no Item ID column at all because
        // no objectattribute was written, because the id was never read.
        [Test]
        public void TheElementIdIsWrittenAsTheObjectattributeTheStylesheetReads()
        {
            List<XElement> items = new List<XElement>(Xml().Root.Descendants("clashobject"));

            Assert.That(items.Count, Is.EqualTo(2));

            foreach (XElement item in items)
            {
                List<XElement> attributes = new List<XElement>(item.Elements("objectattribute"));

                // Exactly one. The Item ID cell is value-of over ./objectattribute/name,
                // which takes the first, so a second would never be seen anyway.
                Assert.That(attributes.Count, Is.EqualTo(1));
                Assert.That(attributes[0].Element("name").Value, Is.EqualTo("Element ID"));
                Assert.That(attributes[0].Element("value").Value, Is.Not.Empty);
            }

            Assert.That(items[0].Element("objectattribute").Element("value").Value,
                Is.EqualTo("702888"));
        }

        // ---------- no column of ours in the client's XML ----------

        // The one the brief asks for by name. The stylesheet makes a column out of every
        // smarttag it finds, so ours would appear on the page the client receives.
        [Test]
        public void OnlyTheTwoQuickPropertiesTheClientReportHasAreWritten()
        {
            foreach (XElement item in Xml().Root.Descendants("clashobject"))
            {
                List<string> names = new List<string>();

                foreach (XElement tag in item.Element("smarttags").Elements("smarttag"))
                {
                    names.Add(tag.Element("name").Value);
                }

                Assert.That(names, Is.EqualTo(new[] { "Item Name", "Item Type" }));
            }

            Assert.That(ClashReportXml.QuickProperties,
                Is.EqualTo(new[] { "Item Name", "Item Type" }));
        }

        // Family, Material, Source File and Discipline are all filled on the report above,
        // and not one of them reaches the XML. They are workbook columns.
        [Test]
        public void OurItemPropertiesAreNowhereInTheXmlEvenWhenTheyAreFilled()
        {
            string xml = Xml().ToString();

            foreach (string ours in new[] { "Family", "Type Name", "Material", "Source File",
                                            "Discipline", "createddate" })
            {
                Assert.That(xml, Does.Not.Contain(ours), ours + " is in the client's XML");
            }
        }

        // ---------- the tolerance ----------

        // The one the brief asks for by name. Ours read 0.2460629921ft where theirs reads
        // 0.025m. The units differ because the two documents do. The precision did not.
        [Test]
        public void TheToleranceIsWrittenToThreeDecimals()
        {
            XElement test = Xml().Root.Element("batchtest").Element("clashtests")
                .Element("clashtest");

            Assert.That(test.Attribute("tolerance").Value, Is.EqualTo("0.246"));

            // The stylesheet writes the attribute and then the units with nothing between
            // them, so the cell reads 0.246ft.
            Assert.That(Xml().Root.Attribute("units").Value, Is.EqualTo("ft"));
            Assert.That(ClientFormat.Tolerance(0.2460629921, "ft"), Is.EqualTo("0.246ft"));
            Assert.That(ClientFormat.Tolerance(0.025, "m"), Is.EqualTo("0.025m"),
                "theirs, unchanged");
        }

        // ---------- the separator ----------

        // The one the brief asks for by name. Both supplied reports write a backslash, for
        // the clash pictures and for the logo. The client opens these in Excel on Windows.
        [Test]
        public void EveryPictureReferenceInTheXmlUsesABackslash()
        {
            ClashReportXml writer = new ClashReportXml();
            writer.LogoHref = "1104-PAR-1C07BC-ZZZ-BM-RPT-000001_files/logo.jpg";

            XDocument x = writer.Build(Report());

            Assert.That(x.Root.Element("logo").Attribute("href").Value,
                Is.EqualTo(@"1104-PAR-1C07BC-ZZZ-BM-RPT-000001_files\logo.jpg"));

            foreach (XElement result in x.Root.Descendants("clashresult"))
            {
                XAttribute href = result.Attribute("href");

                if (href == null)
                {
                    continue;
                }

                Assert.That(href.Value, Does.Contain(@"\"));
                Assert.That(href.Value, Does.Not.Contain("/"));
                Assert.That(href.Value,
                    Is.EqualTo(@"1104-PAR-1C07BC-ZZZ-BM-RPT-000001_files\cd000001.jpg"));
            }
        }

        [Test]
        public void TheSeparatorSwapIsExactlyThatAndNothingElse()
        {
            Assert.That(ClashReportXml.Windows("a/b/c.jpg"), Is.EqualTo(@"a\b\c.jpg"));
            Assert.That(ClashReportXml.Windows(@"a\b\c.jpg"), Is.EqualTo(@"a\b\c.jpg"));
            Assert.That(ClashReportXml.Windows(string.Empty), Is.EqualTo(string.Empty));
            Assert.That(ClashReportXml.Windows(null), Is.EqualTo(string.Empty));
        }

        // The workbook keeps a forward slash, because a hyperlink there is a Uri and their
        // own xlsx carries no picture hyperlinks at all to match against.
        [Test]
        public void TheWorkbookLinkIsStillAForwardSlashUri()
        {
            Assert.That(ImageNaming.LinkFor(@"C:\out\name.xlsx", 0, 1),
                Is.EqualTo("name_files/cd000001.jpg"));
        }
    }
}
