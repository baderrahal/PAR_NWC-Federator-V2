using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Federator.Core.Disciplines;
using Federator.Core.Generic;
using Federator.Core.Sets;
using Federator.Core.Units;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// What the discipline reports read off each model of the open document, F209, by the shape tools\probes\DisciplineProbe
    /// measured on C06 and C02 on 18 Oct 2026, docs\history\scan.md 5z-zg: since F234 the translation of the model's transform
    /// in millimetres, read as the ALIGNMENT block reads it, ModelFactsReader.TranslationOf, the shared site its root names, every CAD file imported into one of its families, and every element of the category
    /// Generic Models. Every rule and every word is Core's, Federator.Core.Disciplines. It reads only and never throws: a
    /// part that would not read is handed on with why, and the reports say not read and never none found.
    ///
    /// THE WALK is one pass over each model's items, as ModelFactsReader's EXPORT CHECK walks them, each item released as the
    /// walk leaves it. A CAD import is a TOPMOST item whose Item tab Type reads Imports in Families, its element the nearest
    /// item above it carrying the Element tab's Id, since the id inside the import repeats across instances of one family.
    /// A Generic Models element is an item whose Element tab Category reads the setting's category.
    /// </summary>
    public static class DisciplineFactsReader
    {
        public static IList<ModelFacts> Read(Document document, DisciplineReportSettings settings)
        {
            List<ModelFacts> all = new List<ModelFacts>();

            if (document == null)
            {
                return all;
            }

            double toMillimetres = ModelFactsReader.MillimetresPerUnit(document);

            for (int i = 0; i < document.Models.Count; i++)
            {
                using (Model model = document.Models[i])
                {
                    ModelFacts facts = new ModelFacts(Words(model.FileName));
                    all.Add(facts);
                    ReadThePlacement(model, toMillimetres, facts);
                    ModelItem root = null;

                    try
                    {
                        root = model.RootItem;
                    }
                    catch (Exception error)
                    {
                        string why = "its root item would not read, " + error.GetType().Name + ": " + error.Message;
                        facts.SiteNot(why).CadNot(why).GenericNot(why);
                        continue;
                    }

                    using (root)
                    {
                        ReadTheSite(root, facts);
                        WalkTheItems(root, settings, facts);
                    }
                }
            }

            return all;
        }

        /// <summary>F234. The translation of the model's transform in millimetres, by the ALIGNMENT block's own read, never the box.</summary>
        private static void ReadThePlacement(Model model, double toMillimetres, ModelFacts facts)
        {
            double x;
            double y;
            double z;
            string why = ModelFactsReader.TranslationOf(model, toMillimetres, out x, out y, out z);

            if (why == null)
            {
                facts.WithPlacement(x, y, z);
            }
            else
            {
                facts.PlacementNot(why);
            }
        }

        private static void ReadTheSite(ModelItem root, ModelFacts facts)
        {
            try
            {
                facts.WithSite(ModelFactsReader.SharedCoordinateOn(root));
            }
            catch (Exception error)
            {
                facts.SiteNot(error.GetType().Name + ": " + error.Message);
            }
        }

        private static void WalkTheItems(ModelItem root, DisciplineReportSettings settings, ModelFacts facts)
        {
            List<ElementFact> cad = new List<ElementFact>();
            List<ElementFact> generic = new List<ElementFact>();

            try
            {
                foreach (ModelItem item in root.DescendantsAndSelf)
                {
                    using (item)
                    {
                        ItemRead read = ItemRead.Of(item);

                        if (string.Equals(read.Category, settings.GenericCategory, StringComparison.Ordinal))
                        {
                            generic.Add(new ElementFact(read.Id, read.Name, read.Family, read.Type, read.LevelName()));
                        }

                        if (string.Equals(read.ItemType, DisciplineProperties.ImportType, StringComparison.Ordinal) && !ParentIsAnImport(item))
                        {
                            cad.Add(TheImport(item));
                        }
                    }
                }
            }
            catch (Exception error)
            {
                string why = error.GetType().Name + ": " + error.Message;
                facts.CadNot(why).GenericNot(why);
                return;
            }

            facts.WithCad(cad).WithGeneric(generic);
        }

        private static bool ParentIsAnImport(ModelItem item)
        {
            using (ModelItem parent = item.Parent)
            {
                return parent != null && string.Equals(ItemRead.Of(parent).ItemType, DisciplineProperties.ImportType, StringComparison.Ordinal);
            }
        }

        /// <summary>One import as an element: the element it hangs under, its file's name where its own name carries one, its level.</summary>
        private static ElementFact TheImport(ModelItem import)
        {
            string fileName = Federator.Core.Disciplines.ElementChecks.CadFileName(Words(import.DisplayName));
            ModelItem at = import.Parent;

            while (at != null)
            {
                ItemRead read = ItemRead.Of(at);

                if (read.Id != null)
                {
                    using (at)
                    {
                        return new ElementFact(read.Id, fileName, read.Family, read.Type, read.LevelName());
                    }
                }

                ModelItem up = at.Parent;
                at.Dispose();
                at = up;
            }

            return new ElementFact(null, fileName, null, null, null);
        }

        private static string Words(string value)
        {
            return value ?? string.Empty;
        }

        /// <summary>The few properties of one item the reports read, its two tabs read once.</summary>
        private sealed class ItemRead
        {
            public string ItemType;
            public string Layer;
            public string Category;
            public string Id;
            public string Name;
            public string Family;
            public string Type;
            public string Level;
            public string ScheduleLevel;

            public string LevelName()
            {
                return ElementChecks.LevelName(Level) ?? ElementChecks.LevelName(ScheduleLevel) ?? ElementChecks.LevelName(Layer);
            }

            public static ItemRead Of(ModelItem item)
            {
                ItemRead read = new ItemRead();

                using (PropertyCategoryCollection tabs = item.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        return read;
                    }

                    foreach (PropertyCategory tab in tabs)
                    {
                        using (tab)
                        {
                            bool element = string.Equals(tab.Name, GenericModelsSettings.ElementCategoryInternalName, StringComparison.Ordinal);
                            bool itemTab = string.Equals(tab.Name, DisciplineProperties.ItemTab, StringComparison.Ordinal);

                            if (!element && !itemTab)
                            {
                                continue;
                            }

                            using (DataPropertyCollection properties = tab.Properties)
                            {
                                foreach (DataProperty property in properties)
                                {
                                    using (property)
                                    {
                                        if (itemTab)
                                        {
                                            if (property.Name == DisciplineProperties.ItemType) { read.ItemType = TextOf(property); }
                                            else if (property.Name == DisciplineProperties.Layer) { read.Layer = TextOf(property); }
                                            continue;
                                        }

                                        if (property.Name == EmptySets.CategoryProperty) { read.Category = TextOf(property); }
                                        else if (property.Name == DisciplineProperties.ElementId) { read.Id = TextOf(property); }
                                        else if (property.Name == DisciplineProperties.Name) { read.Name = TextOf(property); }
                                        else if (property.Name == DisciplineProperties.Family) { read.Family = TextOf(property); }
                                        else if (property.Name == DisciplineProperties.Type) { read.Type = TextOf(property); }
                                        else if (property.Name == DisciplineProperties.Level) { read.Level = TextOf(property); }
                                        else if (property.Name == DisciplineProperties.ScheduleLevel) { read.ScheduleLevel = TextOf(property); }
                                    }
                                }
                            }
                        }
                    }
                }

                return read;
            }

            /// <summary>The value by its kind, ClashHarvest.Text, null where it is empty or would not read.</summary>
            private static string TextOf(DataProperty property)
            {
                try
                {
                    using (VariantData value = property.Value)
                    {
                        string text = value == null ? string.Empty : ClashHarvest.Text(value);
                        return string.IsNullOrEmpty(text) ? null : text;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
