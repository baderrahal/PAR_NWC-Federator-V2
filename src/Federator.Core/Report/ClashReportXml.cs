using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;

namespace Federator.Core.Report
{
    /// <summary>
    /// The optional XML written beside the workbook. Off by default.
    ///
    /// It is built from the same <see cref="ClashReport"/> the workbook is built from. It
    /// never reads the workbook and the workbook never reads it, so a fault in one cannot
    /// corrupt the other.
    ///
    /// WHERE THE SHAPE CAME FROM. There is no clash report XSD anywhere in the Navisworks
    /// install. The schemas folder holds only nw-exchange and nw-Takeoff schemas, listed
    /// in docs/scan.md section 7. The shape is still shipped, in the three stylesheets
    /// Navisworks uses to render its own clash reports:
    ///
    ///     Navisworks Manage 2025\en-US\stylesheets\clash_report_html.xsl
    ///     Navisworks Manage 2025\en-US\stylesheets\clash_report_html_tabular.xsl
    ///     Navisworks Manage 2025\en-US\stylesheets\clash_report_text.xsl
    ///
    /// An XSL says exactly which elements and attributes the XML it transforms carries, so
    /// every name below was read off those files rather than invented. See docs/scan.md
    /// section 4h for the whole shape.
    ///
    /// WHAT IS FILLED AND WHAT IS LEFT OUT. Filled: exchange, batchtest, clashtests,
    /// clashtest, summary, clashresults, clashgroup, clashresult, resultstatus,
    /// clashpoint, pos3f, gridlocation, createddate, date, clashobjects, clashobject,
    /// layer, objectattribute. Left out, because this tool holds nothing to put in them:
    /// approveddate, approvedby, assignedto, clashtasklink and everything under it,
    /// linkage, linkedanimation, clipplaneset, view and camera.
    ///
    /// WHAT THIS IS FOR NOW. It is no longer only a file beside the workbook. It is what
    /// the HTML Tabular report is rendered from, by Autodesk's own stylesheet, so every
    /// element here is chosen because that stylesheet reads it. See docs\scan.md
    /// section 4m for our XML tested against every one of its column tests.
    /// Left out rather than written empty, so nobody reads a blank as a measured blank.
    /// </summary>
    public sealed class ClashReportXml
    {
        /// <summary>
        /// The two quick properties the client's own report carries. The stylesheet takes
        /// its Item Name and Item Type headings from the smarttag names in the data, so
        /// these words are what appear on the page.
        /// </summary>
        public static readonly string QuickName = ClientReportColumns.QuickProperties[0];

        public static readonly string QuickType = ClientReportColumns.QuickProperties[1];

        /// <summary>
        /// The client's report has exactly TWO quick properties, so this XML writes
        /// exactly two. The stylesheet makes a column out of every smarttag it finds, so
        /// anything of ours here becomes a column on the page the client receives.
        ///
        /// Family, Type Name, Material, Source File and Discipline used to be written
        /// here as well. They belong in the workbook, which is ours, and never on the
        /// page, which is theirs.
        /// </summary>
        public static readonly string[] QuickProperties = ClientReportColumns.QuickProperties;

        /// <summary>
        /// The logo the page shows, or empty for none. The stylesheet reads //logo/@href,
        /// so the logo is data rather than something baked into the layout, and writing no
        /// logo element leaves the src empty and puts no picture on the page.
        ///
        /// Autodesk's own logo.jpg ships in the install's Images folder and is theirs.
        /// Nothing here copies it. This is empty until somebody points it at their own.
        /// </summary>
        public string LogoHref { get; set; }

        /// <summary>The stylesheets the shape was read from, named so the claim is checkable.</summary>
        public static readonly string[] ShapeReadFrom =
        {
            "clash_report_html.xsl",
            "clash_report_html_tabular.xsl",
            "clash_report_text.xsl"
        };

        /// <summary>Elements this tool fills.</summary>
        public static readonly string[] Filled =
        {
            "exchange", "batchtest", "clashtests", "clashtest", "summary", "clashresults",
            "clashgroup", "clashresult", "resultstatus", "clashpoint", "pos3f",
            "gridlocation", "createddate", "date", "clashobjects", "clashobject", "layer",
            "objectattribute", "description", "smarttags", "smarttag", "logo"
        };

        /// <summary>
        /// Elements of that shape this tool leaves out, because it holds nothing to put in
        /// them. Left out rather than written empty.
        /// </summary>
        public static readonly string[] LeftOut =
        {
            "approveddate", "approvedby", "assignedto", "parentgroup", "comments",
            "pathlink", "clashtasklink", "starttime", "endtime", "taskname", "tasklink",
            "taskuid", "animatorscene", "animatoranim", "linkage", "linkedanimation",
            "clipplaneset", "view", "camera"
        };

        public string Write(ClashReport report, string path)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("A report needs a path.", "path");
            }

            string folder = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = new System.Text.UTF8Encoding(false)
            };

            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                Build(report).WriteTo(writer);
            }

            return path;
        }

        /// <summary>The document, so a test can read it without writing a file.</summary>
        public XDocument Build(ClashReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            XElement exchange = new XElement("exchange",
                new XAttribute("units", Words.Or(report.DocumentUnits, "unknown")));

            XElement tests = new XElement("clashtests");

            // Most clashes first, ties in the order they were created, which is what both
            // client exports do. Ours followed the order the tests sat in the file, so it
            // opened with empty tests and a reader scrolled past hundreds of them.
            foreach (TestReport test in report.InReportOrder())
            {
                tests.Add(Test(test));
            }

            // Read by the stylesheet as //logo/@href. Left out entirely when nobody picked
            // one, so the page carries no logo rather than a borrowed one.
            if (!string.IsNullOrEmpty(LogoHref))
            {
                exchange.Add(new XElement("logo", new XAttribute("href", Windows(LogoHref))));
            }

            exchange.Add(new XElement("batchtest",
                new XAttribute("name", Words.Or(report.Building, "clash")),
                new XAttribute("internal_name", Words.Or(report.OutputName, report.Building)),
                tests));

            return new XDocument(new XDeclaration("1.0", "UTF-8", null), exchange);
        }

        private XElement Test(TestReport test)
        {
            ClashTally tally = test.Tally;

            XElement element = new XElement("clashtest",
                new XAttribute("name", test.Name),
                new XAttribute("test_type", Words.Or(test.TestTypeName, "unknown")),
                new XAttribute("status", test.State == TestState.Skipped
                    ? "skipped"
                    : ClientFormat.StatusWording(test.StatusWord)),
                // Three decimals. The stylesheet writes this attribute straight into the
                // cell followed by the units, so ours read 0.2460629921ft where theirs
                // read 0.025m. The units differ because the documents do and that is
                // right. The precision was ours to fix.
                new XAttribute("tolerance",
                    test.Tolerance.ToString(ClientFormat.ToleranceFormat, CultureInfo.InvariantCulture)),
                new XElement("summary",
                    new XAttribute("total", tally.Total),
                    new XAttribute("new", tally.Of(ClashStatus.New)),
                    new XAttribute("active", tally.Of(ClashStatus.Active)),
                    new XAttribute("reviewed", tally.Of(ClashStatus.Reviewed)),
                    new XAttribute("approved", tally.Of(ClashStatus.Approved)),
                    new XAttribute("resolved", tally.Of(ClashStatus.Resolved))));

            if (!test.HasSheet)
            {
                return element;
            }

            XElement results = new XElement("clashresults");

            foreach (ClashRow row in test.Rows)
            {
                results.Add(Result(row));
            }

            element.Add(results);
            return element;
        }

        /// <summary>
        /// A group is a clashgroup and a plain clash is a clashresult. They are siblings
        /// under clashresults and carry the same children, which is how the stylesheets
        /// treat them.
        /// </summary>
        private XElement Result(ClashRow row)
        {
            XElement element = new XElement(row.IsGroup ? "clashgroup" : "clashresult",
                new XAttribute("name", Words.Or(row.Name, "clash")),
                new XAttribute("distance", ClientFormat.Fixed(row.Distance)));

            // The stylesheet turns its Image column on for boolean(//@href) and reads the
            // picture off this one attribute. No picture, no attribute, no column.
            if (row.HasImage && row.ImageLink.Length > 0)
            {
                element.Add(new XAttribute("href", Windows(row.ImageLink)));
            }

            element.Add(new XElement("resultstatus", row.Status.ToString()));

            // The Description column. Ours had no description element at all, so the
            // stylesheet left the column out where the client's report has it.
            if (row.Description.Length > 0)
            {
                element.Add(new XElement("description", row.Description));
            }

            // The single joined field, "B-1 : LGF", the same one the workbook writes.
            // Writing row.GridLocation alone put the grid on the page without its level.
            string grid = row.ClientGridLocation();

            if (grid.Length > 0)
            {
                element.Add(new XElement("gridlocation", grid));
            }

            element.Add(new XElement("clashpoint",
                new XElement("pos3f",
                    new XAttribute("x", ClientFormat.Fixed(row.X)),
                    new XAttribute("y", ClientFormat.Fixed(row.Y)),
                    new XAttribute("z", ClientFormat.Fixed(row.Z)))));

            // No createddate. The stylesheet writes a Date Found column for any it finds
            // and the client's report has no such column, measured on both 1A02WN and
            // 1A04WN. Found is still in the workbook, which is ours.

            element.Add(new XElement("clashobjects",
                Item(row.Left, row.Level),
                Item(row.Right, row.Level)));

            return element;
        }

        /// <summary>
        /// One side. The family, type, material, source file and discipline go in as
        /// objectattribute name and value pairs, which is the shape the stylesheets read
        /// arbitrary item properties out of.
        /// </summary>
        private XElement Item(ClashItem item, string level)
        {
            XElement element = new XElement("clashobject");

            if (!string.IsNullOrEmpty(level))
            {
                element.Add(new XElement("layer", level));
            }

            // EXACTLY ONE objectattribute, and it is the id. The stylesheet's Item ID
            // cell is value-of over ./objectattribute/name and ./objectattribute/value,
            // which takes the FIRST of them, so writing several put "Name:
            // PAR-CONC-FOUNDATION" in the id column instead of the element id.
            //
            // An all zero GUID is not an id, so it is left out and the cell is empty.
            if (!ClientFormat.NoIdAtAll(item.ElementId))
            {
                element.Add(new XElement("objectattribute",
                    new XElement("name", Words.Or(item.IdLabel, ClientFormat.DefaultIdLabel)),
                    new XElement("value", item.ElementId)));
            }

            // Everything else is a quick property, which is how the client's report
            // carries Item Name and Item Type. The stylesheet takes the column count from
            // the FIRST clashobject's smarttags, so the same list is written every time,
            // empty values included, or the columns slide sideways from one row to the
            // next.
            element.Add(new XElement("smarttags",
                Tag(QuickName, item.Name),
                Tag(QuickType, item.ItemType)));

            return element;
        }

        /// <summary>
        /// One quick property. Always written, even with nothing in it, because the
        /// stylesheet counts them once and uses that count for every row.
        /// </summary>
        private static XElement Tag(string name, string value)
        {
            return new XElement("smarttag",
                new XElement("name", name),
                new XElement("value", value ?? string.Empty));
        }

        /// <summary>
        /// A picture reference the way theirs writes it, with a backslash. Both supplied
        /// reports use one, for the clash pictures and for the logo, and the client opens
        /// these in Excel on Windows.
        ///
        /// The workbook keeps a forward slash, because a hyperlink there is a Uri and
        /// there is nothing of theirs to match: their own xlsx carries no picture
        /// hyperlinks at all.
        /// </summary>
        public static string Windows(string link)
        {
            return string.IsNullOrEmpty(link) ? string.Empty : link.Replace('/', '\\');
        }


        /// <summary>What was filled and what was left out, for the log and the report.</summary>
        public static IList<string> Explain()
        {
            List<string> lines = new List<string>();

            lines.Add("The shape came from the stylesheets Navisworks ships, "
                + string.Join(", ", ShapeReadFrom)
                + ", because there is no clash report schema anywhere in the install.");
            lines.Add("filled  : " + string.Join(", ", Filled));
            lines.Add("left out: " + string.Join(", ", LeftOut));
            lines.Add("Left out rather than written empty, so a blank is never read as a measured blank.");

            return lines;
        }
    }
}
