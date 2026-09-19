using System;
using System.Collections.Generic;
using Federator.Core.Views;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Which item categories count as a SERVICE and which count as a SOLID, F72.
    ///
    /// A penetration is a small service going through a wall, a floor or a roof. It is the
    /// ordinary way a building goes together and not a coordination problem, so a person
    /// should not have to look at every one of them week after week. Marking it Reviewed
    /// says somebody looked and moved on, which is true when the rule that picked it is a
    /// rule that person agreed to. That is Q33, answered on 2026-09-19, and Q41 to Q44
    /// answer the four details.
    ///
    /// BOTH LISTS ARE SETTINGS WITH THE DEFAULTS BADER GAVE, because this tool serves
    /// different projects and a project whose exporter names its categories differently
    /// changes the list rather than the code.
    ///
    /// THE THRESHOLD IS NOT HERE. It is Federator.Core.Views.SizeSettings, which F53 also
    /// reads, and the comment at that number says which way round each of the two reads
    /// it. Two copies of 150 would drift.
    ///
    /// NO DISCIPLINE FILTER ANYWHERE IN HERE. Q42 asked whether the solid side should be
    /// limited to walls that came in on an architecture file, and the answer is no: ANY
    /// wall counts whichever file it came in. So nothing here reads part 5 of a name and
    /// nothing here knows what a discipline is.
    /// </summary>
    public sealed class PenetrationSettings
    {
        /// <summary>
        /// The categories that are a service. Pipes, ducts, cable trays, conduits and the
        /// fittings and accessories of each. Given by Bader on 2026-09-19.
        /// </summary>
        public static readonly string[] DefaultServiceCategories =
        {
            "Pipes",
            "Pipe Fittings",
            "Pipe Accessories",
            "Ducts",
            "Duct Fittings",
            "Duct Accessories",
            "Flex Pipes",
            "Flex Ducts",
            "Cable Trays",
            "Cable Tray Fittings",
            "Conduits",
            "Conduit Fittings"
        };

        /// <summary>
        /// The categories that are a solid to go through. Q41 answered: floors and roofs
        /// count as well as walls, because a service dropping through a slab is the same
        /// kind of thing as one going through a wall.
        /// </summary>
        public static readonly string[] DefaultSolidCategories =
        {
            "Walls",
            "Floors",
            "Roofs"
        };

        /// <summary>
        /// The property display names a category is looked for under, in order, the first
        /// that answers winning. The same shape ClashHarvest already uses for the family,
        /// the type and the material, and a setting for the same reason: every exporter
        /// names them differently.
        /// </summary>
        public static readonly string[] DefaultCategoryNames =
        {
            "Category",
            "Revit Category",
            "Element Category"
        };

        public PenetrationSettings()
        {
            ServiceCategories = new List<string>(DefaultServiceCategories);
            SolidCategories = new List<string>(DefaultSolidCategories);
            CategoryNames = new List<string>(DefaultCategoryNames);
        }

        /// <summary>The categories that are a service.</summary>
        public IList<string> ServiceCategories { get; set; }

        /// <summary>The categories that are a solid to go through.</summary>
        public IList<string> SolidCategories { get; set; }

        /// <summary>Where a category is read from, in order.</summary>
        public IList<string> CategoryNames { get; set; }

        /// <summary>Whether that category is one this tool calls a service.</summary>
        public bool IsService(string category)
        {
            return Holds(ServiceCategories, category);
        }

        /// <summary>Whether that category is one this tool calls a solid.</summary>
        public bool IsSolid(string category)
        {
            return Holds(SolidCategories, category);
        }

        /// <summary>
        /// The grey line under the tick box. The threshold is READ from the size settings
        /// rather than typed, because a number the window shows and a number the rule uses
        /// have to be the same number, and the XAML is the one copy nothing can test.
        ///
        /// Twelve words is the limit a tick box help line has, and this one is twelve.
        /// </summary>
        public static string HelpLine(SizeSettings sizes)
        {
            SizeSettings how = sizes ?? new SizeSettings();

            return "Services " + Millimetres(how.ThresholdMillimetres)
                + " and under through walls, floors, roofs. New and Active only";
        }

        /// <summary>
        /// The label on the tick box. Four words, and it names the thing rather than the
        /// mechanism.
        /// </summary>
        public const string TickLabel = "Mark penetrations as Reviewed";

        private static string Millimetres(double value)
        {
            return value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "mm";
        }

        /// <summary>
        /// Compared without case and trimmed, because a category comes off a model
        /// property and an exporter writing "Pipe Fittings " is naming the same thing.
        /// This is NOT the set name rule, which is Ordinal and never trimmed because two
        /// set names in the reference file end in a space on purpose. A category is not a
        /// set name.
        /// </summary>
        private static bool Holds(IList<string> list, string category)
        {
            if (list == null || string.IsNullOrEmpty(category))
            {
                return false;
            }

            string tidied = category.Trim();

            for (int i = 0; i < list.Count; i++)
            {
                if (string.IsNullOrEmpty(list[i]))
                {
                    continue;
                }

                if (string.Equals(list[i].Trim(), tidied, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
