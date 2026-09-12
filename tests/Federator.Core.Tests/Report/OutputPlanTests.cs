using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// L3. Each output answers to its own flag. The page used to ride on the workbook
    /// flag and the pictures on the workbook flag alone.
    /// </summary>
    [TestFixture]
    public class OutputPlanTests
    {
        private static ReportOptions With(bool workbook, bool xml, bool html, bool images)
        {
            ReportOptions options = new ReportOptions();
            options.WriteWorkbook = workbook;
            options.WriteXml = xml;
            options.WriteHtml = html;
            options.Images.Write = images;
            return options;
        }

        [TestCase(false, false, false, false)]
        [TestCase(true, false, false, true)]
        [TestCase(false, true, false, true)]
        [TestCase(false, false, true, true)]
        [TestCase(true, true, false, true)]
        [TestCase(true, false, true, true)]
        [TestCase(false, true, true, true)]
        [TestCase(true, true, true, true)]
        public void TheReportIsBuiltWhenAnyOfTheThreeIsWantedAndEachFlagIsItsOwn(
            bool workbook, bool xml, bool html, bool report)
        {
            OutputPlan plan = OutputPlan.From(With(workbook, xml, html, true));

            Assert.That(plan.BuildReport, Is.EqualTo(report), "report");
            Assert.That(plan.WriteWorkbook, Is.EqualTo(workbook), "workbook");
            Assert.That(plan.WriteXml, Is.EqualTo(xml), "xml");
            Assert.That(plan.WriteHtml, Is.EqualTo(html), "html");
            if (report)
            {
                Assert.That(plan.ReportSkipReason, Is.Null);
            }
            else
            {
                Assert.That(plan.ReportSkipReason, Is.EqualTo(OutputPlan.NotWanted));
            }
        }

        [Test]
        public void ThePageAloneIsEnoughToBuildTheReport()
        {
            // The case L3 named. Workbook off, XML off, page on, and the page must still
            // have a report to render.
            OutputPlan plan = OutputPlan.From(With(false, false, true, true));

            Assert.That(plan.BuildReport, Is.True);
            Assert.That(plan.WriteHtml, Is.True);
            Assert.That(plan.WriteWorkbook, Is.False);
        }

        [Test]
        public void ImagesNeedToBeWantedAndSomewhereToBeLinkedFrom()
        {
            Assert.That(OutputPlan.From(With(true, false, false, true)).WriteImages, Is.True, "workbook");
            Assert.That(OutputPlan.From(With(false, false, true, true)).WriteImages, Is.True, "page");
            Assert.That(OutputPlan.From(With(false, true, false, true)).WriteImages, Is.True, "xml");

            OutputPlan nowhere = OutputPlan.From(With(false, false, false, true));
            Assert.That(nowhere.WriteImages, Is.False);
            Assert.That(nowhere.ImagesSkipReason, Is.EqualTo(OutputPlan.NowhereToLinkImages));

            OutputPlan off = OutputPlan.From(With(true, true, true, false));
            Assert.That(off.WriteImages, Is.False);
            Assert.That(off.ImagesSkipReason, Is.EqualTo(OutputPlan.ImagesOff));

            Assert.That(OutputPlan.From(With(true, false, false, true)).ImagesSkipReason, Is.Null);
        }

        [Test]
        public void NoOptionsMeansNothingIsWrittenRatherThanAThrow()
        {
            OutputPlan plan = OutputPlan.From(null);

            Assert.That(plan.BuildReport, Is.False);
            Assert.That(plan.WriteImages, Is.False);
            Assert.That(plan.WriteWorkbook, Is.False);
            Assert.That(plan.WriteXml, Is.False);
            Assert.That(plan.WriteHtml, Is.False);
        }

        [Test]
        public void TheDefaultsAreWorkbookPageAndImagesOnAndXmlOff()
        {
            OutputPlan plan = OutputPlan.From(new ReportOptions());

            Assert.That(plan.WriteWorkbook, Is.True);
            Assert.That(plan.WriteHtml, Is.True);
            Assert.That(plan.WriteImages, Is.True);
            Assert.That(plan.WriteXml, Is.False);
        }
    }
}
