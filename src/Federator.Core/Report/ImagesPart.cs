namespace Federator.Core.Report
{
    /// <summary>
    /// One call the IMAGES step's seconds go to, FR-077, in the order a picture meets them.
    /// The words for each are ImagesSeconds.Describe and nothing else spells them.
    /// </summary>
    public enum ImagesPart
    {
        /// <summary>The one Navisworks call, TestsImageForResult, which gives the bitmap back.</summary>
        Rendering = 0,

        /// <summary>The bitmap written to the disk as a JPEG.</summary>
        Saving = 1,

        /// <summary>The written file read back off the disk for its size.</summary>
        ReadingBack = 2
    }
}
