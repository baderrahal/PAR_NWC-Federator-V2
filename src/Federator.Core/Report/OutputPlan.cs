using System;

namespace Federator.Core.Report
{
    /// <summary>
    /// Which outputs a run writes, each answered by its own flag and nothing else.
    ///
    /// WHY THIS EXISTS. The engine built the ClashReport only when the workbook or the
    /// XML was wanted, and rendered the pictures only when the workbook was wanted. The
    /// HTML page, which is the report the client receives and is fixed on, was therefore
    /// written only as a side effect of the workbook being on, and the pictures it links
    /// were rendered only as a side effect of the workbook being on. Both are fixed on
    /// today, so nothing was lost yet, and the day the workbook is switched off the page
    /// and the photos would have gone with it. Now every output answers to its own flag.
    ///
    /// This changes WHEN each output is written, never what is in it.
    /// </summary>
    public sealed class OutputPlan
    {
        public const string NotWanted = "not wanted this run";

        public const string ImagesOff = "images are switched off for this run";

        public const string NowhereToLinkImages =
            "no workbook, page or XML is wanted, so there is nowhere to link a picture";

        private OutputPlan(bool workbook, bool xml, bool html, bool images)
        {
            WriteWorkbook = workbook;
            WriteXml = xml;
            WriteHtml = html;
            imagesWanted = images;
        }

        private readonly bool imagesWanted;

        public static OutputPlan From(ReportOptions options)
        {
            if (options == null)
            {
                return new OutputPlan(false, false, false, false);
            }

            return new OutputPlan(
                options.WriteWorkbook,
                options.WriteXml,
                options.WriteHtml,
                options.Images != null && options.Images.Write);
        }

        public bool WriteWorkbook { get; private set; }

        public bool WriteXml { get; private set; }

        public bool WriteHtml { get; private set; }

        /// <summary>
        /// The ClashReport feeds the workbook, the XML and the page, so it is built when
        /// any one of the three is wanted.
        /// </summary>
        public bool BuildReport
        {
            get { return WriteWorkbook || WriteXml || WriteHtml; }
        }

        /// <summary>
        /// Pictures are rendered when they are wanted and at least one report is wanted to
        /// link them from. The page embeds them, the workbook links them and the XML
        /// carries their href, so any of the three is a home for them. With none wanted
        /// there is nowhere a picture could be found, so none is rendered.
        /// </summary>
        public bool WriteImages
        {
            get { return imagesWanted && BuildReport; }
        }

        /// <summary>Why no picture is rendered, or null when they are.</summary>
        public string ImagesSkipReason
        {
            get
            {
                if (WriteImages)
                {
                    return null;
                }

                return imagesWanted ? NowhereToLinkImages : ImagesOff;
            }
        }

        /// <summary>Why a report is not built at all, or null when one is.</summary>
        public string ReportSkipReason
        {
            get { return BuildReport ? null : NotWanted; }
        }

        public override string ToString()
        {
            return "workbook " + (WriteWorkbook ? "on" : "off")
                + ", XML " + (WriteXml ? "on" : "off")
                + ", HTML " + (WriteHtml ? "on" : "off")
                + ", images " + (WriteImages ? "on" : "off");
        }
    }
}
