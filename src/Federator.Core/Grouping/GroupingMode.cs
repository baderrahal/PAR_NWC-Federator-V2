using System;

namespace Federator.Core.Grouping
{
    /// <summary>
    /// How the scanned files are gathered into federations. Per building is the default,
    /// because that is what this project runs weekly, but it is a choice rather than a
    /// rule. A coordination meeting about one discipline across the whole site wants a
    /// different split from a building handover.
    /// </summary>
    public enum GroupingMode
    {
        /// <summary>
        /// One federation per building, every discipline of it together. The default.
        /// </summary>
        PerBuilding,

        /// <summary>One federation per building and discipline, so 1C07BC AR is its own file.</summary>
        PerBuildingAndDiscipline,

        /// <summary>One federation per discipline, spanning every building in the run.</summary>
        PerDiscipline,

        /// <summary>One federation holding everything that was ticked.</summary>
        Everything
    }

    /// <summary>The words the window shows for each mode, in one place so they cannot drift.</summary>
    public static class GroupingModes
    {
        public const GroupingMode Default = GroupingMode.PerBuilding;

        /// <summary>The key a group holding everything is called.</summary>
        public const string EverythingKey = "ALL";

        public static string Describe(GroupingMode mode)
        {
            switch (mode)
            {
                case GroupingMode.PerBuilding:
                    return "One file per building";
                case GroupingMode.PerBuildingAndDiscipline:
                    return "One file per building and discipline";
                case GroupingMode.PerDiscipline:
                    return "One file per discipline, across every building";
                case GroupingMode.Everything:
                    return "One file for everything";
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>Every mode, in the order the window offers them.</summary>
        public static GroupingMode[] All()
        {
            return new[]
            {
                GroupingMode.PerBuilding,
                GroupingMode.PerBuildingAndDiscipline,
                GroupingMode.PerDiscipline,
                GroupingMode.Everything
            };
        }

        /// <summary>True when every file in a group shares one building code.</summary>
        public static bool OneBuildingPerGroup(GroupingMode mode)
        {
            return mode == GroupingMode.PerBuilding || mode == GroupingMode.PerBuildingAndDiscipline;
        }

        /// <summary>True when every file in a group shares one discipline.</summary>
        public static bool OneDisciplinePerGroup(GroupingMode mode)
        {
            return mode == GroupingMode.PerBuildingAndDiscipline || mode == GroupingMode.PerDiscipline;
        }
    }
}
