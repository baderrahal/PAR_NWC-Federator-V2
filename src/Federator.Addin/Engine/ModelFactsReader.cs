using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Navisworks.Api;
using Federator.Core.Diagnostics;
using Federator.Core.Health;
using Federator.Core.Naming;
using Federator.Core.Units;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Reads, per model in the open document, the facts the ALIGNMENT block and the
    /// EXPORT CHECK block are built on. Core decides what they mean and this decides
    /// nothing at all.
    ///
    /// EVERY MEMBER HERE WAS MEASURED BEFORE IT WAS CALLED, docs\history\scan.md 5q,
    /// by tools\probes\ViewpointProbe in its survey mode against two real groups:
    ///
    ///     Model.Transform            a Transform3D with Translation, in MODEL units
    ///     the model ROOT carries a [Location] tab, internal LcRevitPropertyLocation,
    ///         with revit_ProjectLocation on it, the NAME of the Revit shared site
    ///     a Revit ELEMENT is a composite item carrying LcRevitData_Element, with Id
    ///         and Workset on that tab. The geometry solids under it carry NEITHER,
    ///         so a count taken over items with geometry reads zero
    ///
    /// NOTHING HERE FAILS A GROUP. Q65 answered on 2026-09-20: report it and run anyway,
    /// never skip a group and never stop a run for it. Every read is in its own try and
    /// a count that could not be taken comes back as NotCounted and never as zero.
    /// </summary>
    public static class ModelFactsReader
    {
        /// <summary>The internal name of the tab a Revit element carries, measured 2026-09-20.</summary>
        private const string ElementTab = "LcRevitData_Element";

        /// <summary>The internal name of the tab a model root carries its site on, measured 2026-09-20.</summary>
        private const string LocationTab = "LcRevitPropertyLocation";

        /// <summary>The internal name of the property holding the shared site's name, measured 2026-09-20.</summary>
        private const string SharedCoordinateProperty = "revit_ProjectLocation";

        /// <summary>The display name of the element id property. The same list ClashHarvest reads by.</summary>
        private static readonly string[] IdNames = { "Id", "Element Id", "ElementId", "Element ID" };

        /// <summary>The display name of the workset property, as the client's matrix names it.</summary>
        private const string WorksetName = "Workset";

        /// <summary>
        /// Where every model of the open document sits, for the ALIGNMENT block. The
        /// translation is converted to MILLIMETRES through UnitTable, like every other
        /// measurement in this tool, so a document in feet and a document in metres give
        /// the same answer. A unit the table does not know leaves the model unplaced
        /// rather than failing the group, because this is a diagnostic.
        /// </summary>
        public static IList<ModelPlacement> Placements(Document document, ContainerNameSettings names, RunLog log)
        {
            List<ModelPlacement> placements = new List<ModelPlacement>();

            if (document == null)
            {
                return placements;
            }

            double toMillimetres = MillimetresPerUnit(document);

            for (int i = 0; i < document.Models.Count; i++)
            {
                try
                {
                    using (Model model = document.Models[i])
                    {
                        placements.Add(OnePlacement(model, toMillimetres, names));
                    }
                }
                catch (Exception error)
                {
                    Say(log, "ALIGNMENT could not read model " + i + ", " + error.GetType().Name + ": " + error.Message);
                }
            }

            return placements;
        }

        /// <summary>
        /// What every model carries of worksets and element ids, for the EXPORT CHECK
        /// block. ONE WALK PER MODEL and one property read per element, because a group
        /// holds thousands of them and the walk shape that once built 1.7 million native
        /// handles is the thing this repo is most careful about.
        /// </summary>
        public static IList<ModelExport> Exports(Document document, ContainerNameSettings names, RunLog log)
        {
            List<ModelExport> exports = new List<ModelExport>();

            if (document == null)
            {
                return exports;
            }

            for (int i = 0; i < document.Models.Count; i++)
            {
                try
                {
                    using (Model model = document.Models[i])
                    {
                        exports.Add(OneExport(model, names));
                    }
                }
                catch (Exception error)
                {
                    Say(log, "EXPORT CHECK could not read model " + i + ", " + error.GetType().Name + ": " + error.Message);
                }
            }

            return exports;
        }

        private static ModelPlacement OnePlacement(Model model, double toMillimetres, ContainerNameSettings names)
        {
            string file = NameOf(model);
            string discipline = DisciplineOf(file, names);
            string site = string.Empty;
            double x = ModelPlacement.NotRead;
            double y = ModelPlacement.NotRead;
            double z = ModelPlacement.NotRead;

            try
            {
                Transform3D transform = model.Transform;

                if (transform != null && toMillimetres > 0.0)
                {
                    Vector3D translation = transform.Translation;
                    x = translation.X * toMillimetres;
                    y = translation.Y * toMillimetres;
                    z = translation.Z * toMillimetres;
                }
            }
            catch (Exception)
            {
                // Left unplaced on purpose. A model that could not be placed is never
                // called different, the same way a census that could not count is never
                // called a move.
            }

            try
            {
                using (ModelItem root = model.RootItem)
                {
                    site = SharedCoordinateOn(root);
                }
            }
            catch (Exception)
            {
                site = string.Empty;
            }

            return new ModelPlacement(file, discipline, site, x, y, z);
        }

        private static ModelExport OneExport(Model model, ContainerNameSettings names)
        {
            string file = NameOf(model);
            string discipline = DisciplineOf(file, names);
            int elements = 0;
            int withWorkset = 0;
            int withId = 0;
            List<string> worksets = new List<string>();

            try
            {
                using (ModelItem root = model.RootItem)
                {
                    foreach (ModelItem item in root.DescendantsAndSelf)
                    {
                        using (item)
                        {
                            string id;
                            string workset;

                            if (!ReadElementTab(item, out id, out workset))
                            {
                                continue;
                            }

                            elements++;

                            if (id.Length > 0)
                            {
                                withId++;
                            }

                            if (workset.Length > 0)
                            {
                                withWorkset++;

                                if (!worksets.Contains(workset))
                                {
                                    worksets.Add(workset);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // A walk that threw part way has counted part of the model, and part of a
                // count is not a count, so all three come back as not counted.
                return new ModelExport(file, discipline, ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted, worksets);
            }

            return new ModelExport(file, discipline, elements, withWorkset, withId, worksets);
        }

        /// <summary>
        /// The Id and the Workset off that item's own Element tab, and whether it has one
        /// at all, which is what tells a Revit element from a geometry solid. Matched on
        /// the INTERNAL name, because that is what the API matches on and what the
        /// client's matrix carries, and the friendly word rides beside it, never in place
        /// of it.
        /// </summary>
        private static bool ReadElementTab(ModelItem item, out string id, out string workset)
        {
            id = string.Empty;
            workset = string.Empty;

            using (PropertyCategoryCollection tabs = item.PropertyCategories)
            {
                if (tabs == null)
                {
                    return false;
                }

                foreach (PropertyCategory tab in tabs)
                {
                    if (!string.Equals(Words(tab.Name), ElementTab, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    using (DataPropertyCollection properties = tab.Properties)
                    {
                        for (int i = 0; i < properties.Count; i++)
                        {
                            using (DataProperty property = properties[i])
                            {
                                string name = Words(property.DisplayName);

                                if (string.Equals(name, WorksetName, StringComparison.OrdinalIgnoreCase))
                                {
                                    workset = ClashHarvest.Text(property.Value);
                                }
                                else if (IsAnIdName(name))
                                {
                                    id = ClashHarvest.Text(property.Value);
                                }
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private static bool IsAnIdName(string name)
        {
            for (int i = 0; i < IdNames.Length; i++)
            {
                if (string.Equals(name, IdNames[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string SharedCoordinateOn(ModelItem root)
        {
            using (PropertyCategoryCollection tabs = root.PropertyCategories)
            {
                if (tabs == null)
                {
                    return string.Empty;
                }

                foreach (PropertyCategory tab in tabs)
                {
                    if (!string.Equals(Words(tab.Name), LocationTab, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    using (DataPropertyCollection properties = tab.Properties)
                    {
                        for (int i = 0; i < properties.Count; i++)
                        {
                            using (DataProperty property = properties[i])
                            {
                                if (string.Equals(Words(property.Name), SharedCoordinateProperty, StringComparison.OrdinalIgnoreCase))
                                {
                                    return ClashHarvest.Text(property.Value);
                                }
                            }
                        }
                    }

                    return string.Empty;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// How many millimetres one document unit is, through UnitTable, which is the one
        /// unit table in this repo. A unit the table has not been taught gives zero, which
        /// leaves every model unplaced and says nothing wrong, rather than falling back to
        /// a factor nobody chose.
        /// </summary>
        private static double MillimetresPerUnit(Document document)
        {
            try
            {
                UnitRow row = UnitTable.FindByEnumName(document.Units.ToString());
                return row == null ? 0.0 : row.MillimetresPerUnit;
            }
            catch (Exception)
            {
                return 0.0;
            }
        }

        /// <summary>The NWC file name, from Model.FileName and never SourceFileName, which is the Revit container.</summary>
        private static string NameOf(Model model)
        {
            try
            {
                string path = Words(model.FileName);
                return path.Length == 0 ? string.Empty : Path.GetFileName(path);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>Part 5 of the file name, read by the one parser, or empty where the name will not parse.</summary>
        private static string DisciplineOf(string file, ContainerNameSettings names)
        {
            if (file.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                ParsedContainerName parsed = ContainerName.Parse(
                    Path.GetFileNameWithoutExtension(file), names ?? new ContainerNameSettings());

                return parsed == null || !parsed.IsReadable ? string.Empty : Words(parsed.Discipline);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static void Say(RunLog log, string line)
        {
            if (log != null)
            {
                log.Line(line);
            }
        }

        private static string Words(string value)
        {
            return value ?? string.Empty;
        }
    }
}
