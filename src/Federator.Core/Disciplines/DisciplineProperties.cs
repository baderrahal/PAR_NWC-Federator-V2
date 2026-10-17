namespace Federator.Core.Disciplines
{
    /// <summary>
    /// The internal names the discipline reports read off an NWC exported from Revit, F209, each MEASURED by
    /// tools\probes\DisciplineProbe on copies of the 64 NWCs of C06 and the 4 of C02 on 18 Oct 2026, docs\history\scan.md
    /// 5z-zg, and typed here once. The Element tab and its Category are GenericModelsSettings' and EmptySets', where they
    /// were measured before, and the shared site ModelFactsReader's, 5q.
    /// </summary>
    public static class DisciplineProperties
    {
        /// <summary>The Element tab's Id, an Int32, 982 of 982 Generic Models elements of C06.</summary>
        public const string ElementId = "LcRevitPropertyElementId";

        public const string Family = "LcRevitPropertyElementFamily";

        public const string Type = "LcRevitPropertyElementType";

        public const string Name = "LcRevitPropertyElementName";

        /// <summary>The Element tab's Level, a named constant read as Level "GRF", #151434, 821 of 982 Generic Models elements.</summary>
        public const string Level = "lcldrevit_parameter_-1001352";

        /// <summary>The Element tab's Schedule Level, the same shape, where an element hosted on a floor carries no Level.</summary>
        public const string ScheduleLevel = "lcldrevit_parameter_-1001365";

        /// <summary>The Item tab every item carries.</summary>
        public const string ItemTab = "LcOaNode";

        /// <summary>The Item tab's Type, the class a person reads in the Selection Tree.</summary>
        public const string ItemType = "LcOaSceneBaseClassUserName";

        /// <summary>The Item tab's Layer, which Navisworks fills with the Revit level, the last place a level is read from.</summary>
        public const string Layer = "LcOaNodeLayer";

        /// <summary>
        /// The Item tab's Type of a CAD file imported into a Revit family, 35 topmost such items in C06, the DWG, IAM or IPT
        /// file's name its own name where it carries one, under the family instance whose Element tab Id is the element.
        /// </summary>
        public const string ImportType = "Imports in Families";
    }
}
