using System;
using System.Globalization;
using Federator.Core.Views;

namespace Federator.Core.Clash
{
    /// <summary>What one side of a clash is, by its item category.</summary>
    public enum SideKind
    {
        /// <summary>Neither a service nor a solid, or no category could be read.</summary>
        Neither = 0,

        /// <summary>A pipe, a duct, a tray, a conduit or one of their fittings.</summary>
        Service = 1,

        /// <summary>A wall, a floor, a roof or a structural foundation. Q63.</summary>
        Solid = 2
    }

    /// <summary>Why a clash was moved, or why it was left exactly as it was.</summary>
    public enum PenetrationVerdict
    {
        /// <summary>A small service through a solid, still at New or Active. Moved.</summary>
        Reviewed = 0,

        /// <summary>Both sides are a service. Q44 answers this: leave it alone.</summary>
        BothService = 1,

        /// <summary>Both sides are a solid. A wall against a floor is not a penetration.</summary>
        BothSolid = 2,

        /// <summary>One side is neither, so there is no penetration to recognise.</summary>
        NotAPenetration = 3,

        /// <summary>A service over the threshold. A real coordination item, left at New.</summary>
        ServiceTooLarge = 4,

        /// <summary>No size could be read off the service. Left alone on purpose.</summary>
        SizeUnknown = 5,

        /// <summary>A person already set this one. Never overwritten.</summary>
        AlreadyDecided = 6
    }

    /// <summary>One side of a clash, as the add-in read it.</summary>
    public sealed class PenetrationSide
    {
        public PenetrationSide(string itemName, string category, double? largestMillimetres)
        {
            ItemName = itemName ?? string.Empty;
            Category = category ?? string.Empty;
            LargestMillimetres = largestMillimetres;
        }

        /// <summary>The item's own name, for the log line.</summary>
        public string ItemName { get; private set; }

        /// <summary>The item category as it was read off the model, never tidied here.</summary>
        public string Category { get; private set; }

        /// <summary>
        /// The LARGEST size property on the item, already converted to millimetres by
        /// Federator.Core.Views.SizeRule, or null where none could be read.
        /// </summary>
        public double? LargestMillimetres { get; private set; }
    }

    /// <summary>What the rule decided about one clash.</summary>
    public sealed class PenetrationDecision
    {
        internal PenetrationDecision(
            PenetrationVerdict verdict, string reason, PenetrationSide service, PenetrationSide solid)
        {
            Verdict = verdict;
            Reason = reason;
            Service = service;
            Solid = solid;
        }

        public PenetrationVerdict Verdict { get; private set; }

        /// <summary>Whether this clash moves to Reviewed.</summary>
        public bool Moves
        {
            get { return Verdict == PenetrationVerdict.Reviewed; }
        }

        /// <summary>Why, in the words a person would use.</summary>
        public string Reason { get; private set; }

        /// <summary>Which side was the service, or null where neither was.</summary>
        public PenetrationSide Service { get; private set; }

        /// <summary>Which side was the solid, or null where neither was.</summary>
        public PenetrationSide Solid { get; private set; }
    }

    /// <summary>
    /// Whether one clash is a penetration this tool marks Reviewed. F72, and the answer to
    /// Q33.
    ///
    /// ALL FOUR HAVE TO BE TRUE:
    ///   1. one side is a SERVICE by item category
    ///   2. the OTHER side is a SOLID by item category
    ///   3. the service measures the threshold OR LESS, largest size property
    ///   4. the clash is at New or Active
    ///
    /// Everything else is left exactly as it is and counted by reason, because a run that
    /// quietly changed ninety statuses and said nothing is the fault this repo exists to
    /// avoid.
    ///
    /// THE UNREADABLE CASE GOES THE OPPOSITE WAY FROM F53 AND THAT IS DELIBERATE. F53
    /// INCLUDES an item whose size cannot be read, because a fitting usually carries no
    /// size property and the safe mistake there is showing something unnecessary in a
    /// viewpoint. F72 LEAVES ALONE a service whose size cannot be read, because the safe
    /// mistake here is leaving a clash at New for a person to look at. Nobody should make
    /// these two agree: they are the same unknown and the safe answer is different because
    /// what happens next is different. F53's half of the reasoning is written at
    /// Federator.Core.Views.SizeRule and at SizeSettings.NameEveryUnknown.
    ///
    /// No Navisworks type reaches this. The add-in reads the categories and the sizes and
    /// makes no decision of its own.
    /// </summary>
    public static class PenetrationRule
    {
        /// <summary>
        /// Decides one clash. Never throws on its inputs: a side with nothing on it is
        /// Neither, which is a real answer meaning there is no penetration to recognise.
        /// </summary>
        public static PenetrationDecision Decide(
            PenetrationSide first,
            PenetrationSide second,
            ClashStatus status,
            PenetrationSettings settings,
            SizeSettings sizes)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (sizes == null)
            {
                throw new ArgumentNullException("sizes");
            }

            SideKind firstKind = KindOf(first, settings);
            SideKind secondKind = KindOf(second, settings);

            if (firstKind == SideKind.Service && secondKind == SideKind.Service)
            {
                return new PenetrationDecision(
                    PenetrationVerdict.BothService,
                    "both sides are a service, so neither is going through the other",
                    null,
                    null);
            }

            if (firstKind == SideKind.Solid && secondKind == SideKind.Solid)
            {
                return new PenetrationDecision(
                    PenetrationVerdict.BothSolid,
                    "both sides are a solid, which is not a penetration",
                    null,
                    null);
            }

            PenetrationSide service = firstKind == SideKind.Service ? first : second;
            PenetrationSide solid = firstKind == SideKind.Solid ? first : second;
            bool haveBoth =
                (firstKind == SideKind.Service && secondKind == SideKind.Solid)
                || (firstKind == SideKind.Solid && secondKind == SideKind.Service);

            if (!haveBoth)
            {
                return new PenetrationDecision(
                    PenetrationVerdict.NotAPenetration,
                    Why(first, firstKind, second, secondKind),
                    null,
                    null);
            }

            // THE SIZE IS CHECKED BEFORE THE STATUS ON PURPOSE. A person reading the block
            // wants to know what the thing WAS before they are told what was done about it,
            // and a large service that is also already Approved is more usefully reported
            // as large than as decided.
            if (!service.LargestMillimetres.HasValue)
            {
                return new PenetrationDecision(
                    PenetrationVerdict.SizeUnknown,
                    "no size could be read off the service, so it is left at "
                        + status + " for somebody to look at",
                    service,
                    solid);
            }

            double millimetres = service.LargestMillimetres.Value;

            if (millimetres > sizes.ThresholdMillimetres)
            {
                return new PenetrationDecision(
                    PenetrationVerdict.ServiceTooLarge,
                    "the service is " + Round(millimetres) + "mm, over "
                        + Round(sizes.ThresholdMillimetres)
                        + "mm, so it is a real coordination item and stays at " + status,
                    service,
                    solid);
            }

            string whyNotMoved = StatusesThisToolMayMoveFrom.WhyNot(status);

            if (whyNotMoved != null)
            {
                return new PenetrationDecision(
                    PenetrationVerdict.AlreadyDecided, whyNotMoved, service, solid);
            }

            return new PenetrationDecision(
                PenetrationVerdict.Reviewed,
                "a " + Round(millimetres) + "mm " + Words(service.Category)
                    + " through a " + Words(solid.Category)
                    + ", " + Round(sizes.ThresholdMillimetres) + "mm or under, was " + status,
                service,
                solid);
        }

        /// <summary>What one side is, by its category. Neither is a real answer.</summary>
        public static SideKind KindOf(PenetrationSide side, PenetrationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (side == null || side.Category.Length == 0)
            {
                return SideKind.Neither;
            }

            if (settings.IsService(side.Category))
            {
                return SideKind.Service;
            }

            return settings.IsSolid(side.Category) ? SideKind.Solid : SideKind.Neither;
        }

        /// <summary>The words each verdict is counted under in the block.</summary>
        public static string Describe(PenetrationVerdict verdict)
        {
            switch (verdict)
            {
                case PenetrationVerdict.Reviewed:
                    return "moved to Reviewed";
                case PenetrationVerdict.BothService:
                    return "both sides a service, left alone";
                case PenetrationVerdict.BothSolid:
                    return "both sides a solid, left alone";
                case PenetrationVerdict.NotAPenetration:
                    return "not a service against a solid, left alone";
                case PenetrationVerdict.ServiceTooLarge:
                    return "the service is over the size, left alone";
                case PenetrationVerdict.SizeUnknown:
                    return "no size could be read off the service, left alone";
                case PenetrationVerdict.AlreadyDecided:
                    return "a person had already set it, left alone";
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>The verdicts in the order the block lists them.</summary>
        public static PenetrationVerdict[] InOrder()
        {
            return new[]
            {
                PenetrationVerdict.Reviewed,
                PenetrationVerdict.ServiceTooLarge,
                PenetrationVerdict.SizeUnknown,
                PenetrationVerdict.AlreadyDecided,
                PenetrationVerdict.BothService,
                PenetrationVerdict.BothSolid,
                PenetrationVerdict.NotAPenetration
            };
        }

        private static string Why(
            PenetrationSide first, SideKind firstKind, PenetrationSide second, SideKind secondKind)
        {
            if (firstKind == SideKind.Neither && secondKind == SideKind.Neither)
            {
                return "neither " + Words(Category(first)) + " nor " + Words(Category(second))
                    + " is a service or a solid this tool knows";
            }

            PenetrationSide odd = firstKind == SideKind.Neither ? first : second;

            return Words(Category(odd)) + " is neither a service nor a solid this tool knows";
        }

        private static string Category(PenetrationSide side)
        {
            return side == null || side.Category.Length == 0 ? string.Empty : side.Category;
        }

        /// <summary>A category with nothing in it reads as a sentence and never as a blank.</summary>
        private static string Words(string category)
        {
            return string.IsNullOrEmpty(category) ? "an item with no category" : category;
        }

        /// <summary>
        /// ONE DECIMAL AT MOST, the same as the PENETRATION block, so the number in the
        /// clash comment saved into the NWF and the number in the log are the same number.
        ///
        /// It was three decimals until 2026-09-21, which meant a pipe read `21mm` in the
        /// block and `20.997mm` in the comment a person opens in Clash Detective, and the
        /// comment is the half that travels with the file. The rule still compares the
        /// FULL value off `PenetrationSide.LargestMillimetres` and never either string.
        /// </summary>
        private static string Round(double value)
        {
            return value.ToString(PenetrationTally.ShownSizeFormat, CultureInfo.InvariantCulture);
        }
    }
}
