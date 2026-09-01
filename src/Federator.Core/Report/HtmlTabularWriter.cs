using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace Federator.Core.Report
{
    /// <summary>
    /// Writes the report in the format the client actually receives.
    ///
    /// Bader does not export an xlsx. Clash Detective cannot write one. He exports HTML
    /// (Tabular) and opens it in Excel, so the file the client accepted is an HTML page.
    /// This writes that page, by handing our own XML to Autodesk's own stylesheet, so the
    /// layout is theirs rather than a second implementation of it that can drift.
    ///
    /// Nothing of the stylesheet is copied. It is read from the install at run time and it
    /// stays Autodesk's file.
    ///
    /// The transform is XSLT 1.0, which is what the stylesheet declares, and it uses no
    /// document() and no script, so it runs under the default XsltSettings with both
    /// switched off.
    /// </summary>
    public sealed class HtmlTabularWriter
    {
        public const string Extension = ".html";

        /// <summary>
        /// Transforms one report and writes the page. Returns the path, or an empty string
        /// when nothing was written, and never throws for a missing stylesheet.
        /// </summary>
        public string Write(XDocument document, string stylesheetPath, string path)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("The page needs a path.", "path");
            }

            if (string.IsNullOrEmpty(stylesheetPath) || !File.Exists(stylesheetPath))
            {
                return string.Empty;
            }

            string folder = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            XslCompiledTransform transform = new XslCompiledTransform();

            // The defaults, so the stylesheet cannot pull in a document or run a script.
            // Autodesk's does neither, checked on 2026-09-01, and a stylesheet that did
            // would be doing something this tool has no business allowing.
            transform.Load(stylesheetPath, XsltSettings.Default, new XmlUrlResolver());

            using (XmlReader reader = document.CreateReader())
            using (XmlWriter writer = XmlWriter.Create(path, transform.OutputSettings))
            {
                transform.Transform(reader, writer);
            }

            return path;
        }

        /// <summary>The page for a workbook, which is the same name with an html extension.</summary>
        public static string PathFor(string workbookPath)
        {
            if (string.IsNullOrEmpty(workbookPath))
            {
                return string.Empty;
            }

            string folder = Path.GetDirectoryName(workbookPath) ?? string.Empty;
            string name = Path.GetFileNameWithoutExtension(workbookPath) ?? string.Empty;

            return Path.Combine(folder, name + Extension);
        }
    }
}
