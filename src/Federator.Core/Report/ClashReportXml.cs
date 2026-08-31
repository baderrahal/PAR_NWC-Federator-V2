using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Federator.Core.Clash;

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
    /// approveddate, approvedby, assignedto, description, smarttags, clashtasklink and
    /// everything under it, linkage, linkedanimation, clipplaneset, view and camera.
    /// Left out rather than written empty, so nobody reads a blank as a measured blank.
    /// </summary>
    public sealed class ClashReportXml
    {
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
            "objectattribute"
        };

        /// <summary>
        /// Elements of that shape this tool leaves out, because it holds nothing to put in
        /// them. Left out rather than written empty.
        /// </summary>
        public static readonly string[] LeftOut =
        {
            "approveddate", "approvedby", "assignedto", "description", "smarttags",
            "smarttag", "clashtasklink", "starttime", "endtime", "taskname", "tasklink",
            "taskuid", "animatorscene", "animatoranim", "linkage", "linkedanimation",
            "clipplaneset", "view", "camera", "href"
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
                new XAttribute("units", Or(report.DocumentUnits, "unknown")));

            XElement tests = new XElement("clashtests");

            foreach (TestReport test in report.Tests)
            {
                tests.Add(Test(test));
            }

            exchange.Add(new XElement("batchtest",
                new XAttribute("name", Or(report.Building, "clash")),
                new XAttribute("internal_name", Or(report.OutputName, report.Building)),
                tests));

            return new XDocument(new XDeclaration("1.0", "UTF-8", null), exchange);
        }

        private static XElement Test(TestReport test)
        {
            ClashTally tally = test.Tally;

            XElement element = new XElement("clashtest",
                new XAttribute("name", test.Name),
                new XAttribute("test_type", Or(test.TestTypeName, "unknown")),
                new XAttribute("status", test.State == TestState.Skipped ? "skipped" : "ok"),
                new XAttribute("tolerance",
                    test.Tolerance.ToString("0.##########", CultureInfo.InvariantCulture)),
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
        private static XElement Result(ClashRow row)
        {
            XElement element = new XElement(row.IsGroup ? "clashgroup" : "clashresult",
                new XAttribute("name", Or(row.Name, "clash")),
                new XAttribute("distance",
                    row.Distance.ToString("0.######", CultureInfo.InvariantCulture)));

            element.Add(new XElement("resultstatus", row.Status.ToString()));

            if (row.GridLocation.Length > 0)
            {
                element.Add(new XElement("gridlocation", row.GridLocation));
            }

            element.Add(new XElement("clashpoint",
                new XElement("pos3f",
                    new XAttribute("x", row.X.ToString("0.######", CultureInfo.InvariantCulture)),
                    new XAttribute("y", row.Y.ToString("0.######", CultureInfo.InvariantCulture)),
                    new XAttribute("z", row.Z.ToString("0.######", CultureInfo.InvariantCulture)))));

            if (row.Found.HasValue)
            {
                DateTime found = row.Found.Value;

                element.Add(new XElement("createddate",
                    new XElement("date",
                        new XAttribute("year", found.Year),
                        new XAttribute("month", found.Month),
                        new XAttribute("day", found.Day))));
            }

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
        private static XElement Item(ClashItem item, string level)
        {
            XElement element = new XElement("clashobject");

            if (!string.IsNullOrEmpty(level))
            {
                element.Add(new XElement("layer", level));
            }

            Attribute(element, "Name", item.Name);
            Attribute(element, "Family", item.Family);
            Attribute(element, "Type", item.Type);
            Attribute(element, "Material", item.Material);
            Attribute(element, "Source File", item.SourceFile);
            Attribute(element, "Discipline", item.Discipline);
            Attribute(element, "Element Id", item.ElementId);

            return element;
        }

        private static void Attribute(XElement parent, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                parent.Add(new XElement("objectattribute",
                    new XElement("name", name),
                    new XElement("value", value)));
            }
        }

        private static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
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
