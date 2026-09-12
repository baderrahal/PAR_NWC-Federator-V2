using System;

namespace Federator.Core.Clash
{
    /// <summary>
    /// What the matrix cell counts as still outstanding.
    ///
    /// The clash API has no open against closed notion at all, see docs\scan.md section
    /// 4h, so neither of these is read off it. Both are stated rules and the sheet says
    /// which one it used, so nobody reads an API meaning into a number that has none.
    /// </summary>
    public enum OpenClashCount
    {
        /// <summary>New plus Active. What this tool counted before there was a choice.</summary>
        NewAndActive,

        /// <summary>
        /// New plus Active plus Reviewed, which is what Navisworks itself treats as open,
        /// with Approved and Resolved as closed. The default, because it is the product's
        /// own definition rather than one this tool invented.
        /// </summary>
        NavisworksOpen
    }

    public static class OpenClashes
    {
        /// <summary>The statuses one choice counts. Read once, used everywhere.</summary>
        public static ClashStatus[] StatusesFor(OpenClashCount which)
        {
            return which == OpenClashCount.NewAndActive
                ? new[] { ClashStatus.New, ClashStatus.Active }
                : new[] { ClashStatus.New, ClashStatus.Active, ClashStatus.Reviewed };
        }
    }
}
