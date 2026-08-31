using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Naming;
using Federator.Core.Report;
using CoreClashStatus = Federator.Core.Clash.ClashStatus;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Turns the results of one clash test into rows the workbook can be written from.
    ///
    /// Everything here reads. Nothing is created, nothing is mutated, so nothing here can
    /// invalidate a handle the runner is holding. Every wrapper it resolves is disposed,
    /// which is safe because a SavedItem read out of a collection is created with
    /// eEXTERNAL ownership. See docs\scan.md section 4g.
    ///
    /// Nothing about any one project is in here. The property names it looks for are
    /// settings, and where a property is not there the cell is left empty rather than
    /// filled with a guess.
    /// </summary>
    public sealed class ClashHarvest
    {
        private readonly RunLog log;
        private readonly ContainerNameSettings settings;

        public ClashHarvest(RunLog log, ContainerNameSettings settings)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
            this.settings = settings ?? new ContainerNameSettings();

            FamilyNames = new[] { "Family", "Family Name" };
            TypeNames = new[] { "Type", "Type Name" };
            MaterialNames = new[] { "Material", "Material Name", "Structural Material" };
            ElementIdNames = new[] { "Id", "Element Id", "ElementId", "Element ID" };
        }

        /// <summary>
        /// Where the pictures are written, and what they cost. Null leaves every Image
        /// cell empty, which is what a run with images switched off does.
        /// </summary>
        public ClashImages Images { get; set; }

        /// <summary>The workbook the pictures sit beside. Empty writes none.</summary>
        public string WorkbookPath { get; set; }

        /// <summary>
        /// The property display names to look for, in order, the first that resolves
        /// winning. Settings, because every project and every exporter names them
        /// differently, and a name nobody has seen leaves the cell empty rather than
        /// guessing.
        /// </summary>
        public string[] FamilyNames { get; set; }

        public string[] TypeNames { get; set; }

        public string[] MaterialNames { get; set; }

        public string[] ElementIdNames { get; set; }

        /// <summary>
        /// Every row for one test. A result group is one row carrying the count behind it,
        /// and an ungrouped clash is its own row.
        /// </summary>
        public void Into(Document document, ClashTest test, TestReport into)
        {
            Into(document, null, test, null, into);
        }

        /// <summary>
        /// Every row for one test, and a picture for each row that asked for one.
        ///
        /// The picture is rendered here rather than in a second pass because this is
        /// where the result handle already is, and it is rendered only once the row has
        /// been fully read, so a render that somehow invalidated the handle could not
        /// cost a single column.
        /// </summary>
        public void Into(
            Document document,
            DocumentClashTests clashTests,
            ClashTest test,
            ClashReport report,
            TestReport into)
        {
            if (into == null)
            {
                throw new ArgumentNullException("into");
            }

            GridSystem grid = ActiveGrid(document);

            try
            {
                Walk(document, clashTests, grid, test.Children, report, into);
            }
            finally
            {
                if (grid != null)
                {
                    grid.Dispose();
                }
            }
        }

        private void Walk(
            Document document,
            DocumentClashTests clashTests,
            GridSystem grid,
            SavedItemCollection children,
            ClashReport report,
            TestReport into)
        {
            if (children == null)
            {
                return;
            }

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    ClashResultGroup group = child as ClashResultGroup;

                    if (group != null)
                    {
                        ClashRow groupRow = GroupRow(document, grid, group);
                        into.Add(groupRow);
                        Picture(clashTests, group, report, into, groupRow);
                        continue;
                    }

                    ClashResult result = child as ClashResult;

                    if (result != null)
                    {
                        ClashRow resultRow = ResultRow(document, grid, result);
                        into.Add(resultRow);
                        Picture(clashTests, result, report, into, resultRow);
                    }
                }
            }
        }

        /// <summary>
        /// One group, as one row. Its distance is the most severe of the clashes inside
        /// it, worked out from the children rather than taken on trust, and it carries the
        /// count of those children so the grouping hides nothing.
        /// </summary>
        private ClashRow GroupRow(Document document, GridSystem grid, ClashResultGroup group)
        {
            List<double> distances = new List<double>();
            int raw = CountLeaves(group.Children, distances);

            ClashRow row = new ClashRow();
            row.IsGroup = true;
            row.RawClashes = raw < 1 ? 1 : raw;
            row.Name = Or(group.DisplayName, "group");
            row.Status = (CoreClashStatus)(int)group.Status;
            row.Distance = ClashRow.MostSevere(distances, group.Distance);
            row.Description = Or(group.Description, string.Empty);

            // The representative result is the clash Navisworks itself shows for the
            // group, so its items are the ones a reader would be looking at.
            using (ClashResult representative = group.RepresentativeResult)
            {
                if (representative != null)
                {
                    Fill(document, grid, row, representative);
                }
                else
                {
                    Place(grid, row, group.Center);
                }
            }

            return row;
        }

        private ClashRow ResultRow(Document document, GridSystem grid, ClashResult result)
        {
            ClashRow row = new ClashRow();
            row.IsGroup = false;
            row.RawClashes = 1;
            row.Name = Or(result.DisplayName, "clash");
            row.Status = (CoreClashStatus)(int)result.Status;
            row.Distance = result.Distance;

            Fill(document, grid, row, result);
            return row;
        }

        private void Fill(Document document, GridSystem grid, ClashRow row, ClashResult result)
        {
            row.Found = result.CreatedTime;
            row.Description = Or(result.Description, string.Empty);
            Place(grid, row, result.Center);

            using (ModelItem left = result.Item1)
            {
                Describe(document, left, row.Left);
            }

            using (ModelItem right = result.Item2)
            {
                Describe(document, right, row.Right);
            }
        }

        /// <summary>
        /// The position, and the grid and level Navisworks itself would show for it.
        /// Left empty when the model carries no grid, which is not a fault.
        /// </summary>
        private void Place(GridSystem grid, ClashRow row, Point3D centre)
        {
            if (centre != null)
            {
                row.X = centre.X;
                row.Y = centre.Y;
                row.Z = centre.Z;
            }

            if (grid == null || centre == null)
            {
                return;
            }

            try
            {
                using (GridIntersection intersection = grid.ClosestIntersection(centre))
                {
                    if (intersection == null)
                    {
                        return;
                    }

                    row.GridLocation = Or(intersection.DisplayName, string.Empty);

                    using (GridLevel level = intersection.Level)
                    {
                        if (level != null)
                        {
                            row.Level = Or(level.DisplayName, string.Empty);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading the grid location for a clash",
                    error,
                    "kept going, the grid and level columns are left empty for this row");
            }
        }

        /// <summary>
        /// One item. The family, the type and the material matter because without them
        /// whoever fixes it has to open the model to see what they are looking at.
        /// </summary>
        private void Describe(Document document, ModelItem item, ClashItem into)
        {
            if (item == null)
            {
                return;
            }

            try
            {
                into.Name = Or(item.DisplayName, string.Empty);
                into.Family = FirstProperty(item, FamilyNames);
                into.Type = FirstProperty(item, TypeNames);
                into.Material = FirstProperty(item, MaterialNames);

                // The client's Item Type column, which reads Solid on every item cell of
                // the accepted report. ClassDisplayName is what the Item tab shows as the
                // type, so it is what that column is.
                into.ItemType = Or(item.ClassDisplayName, string.Empty);

                // The id and, separately, the name of whatever property carried it,
                // because the client's Item ID column is that name and the value in one
                // field. Element ID on a Revit sourced NWC.
                string idFrom;
                into.ElementId = FirstProperty(item, ElementIdNames, out idFrom);
                into.IdLabel = idFrom.Length == 0 ? ClientFormat.DefaultIdLabel : idFrom;

                if (into.ElementId.Length == 0)
                {
                    // No id property anywhere, so the instance GUID is what is left. It
                    // still finds the thing again, which is the point of the column, and
                    // the label says which it is rather than claiming an element id.
                    into.ElementId = item.InstanceGuid.ToString();
                    into.IdLabel = "Instance GUID";
                }

                string source = SourceFileOf(item);
                into.SourceFile = source;

                // The discipline comes out of the same parser the NWC names go through,
                // never a second rule and never a list of codes in this tool.
                ParsedContainerName parsed = ContainerName.Parse(source, settings);
                into.Discipline = parsed.IsReadable ? parsed.Discipline : string.Empty;
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading the properties of a clashing item",
                    error,
                    "kept going, the columns this item did not fill are left empty");
            }
        }

        /// <summary>
        /// One picture for one row, where one was asked for. Everything about whether to
        /// write it lives in the writer, so this only has to hand it the row.
        /// </summary>
        private void Picture(
            DocumentClashTests clashTests,
            IClashResult result,
            ClashReport report,
            TestReport test,
            ClashRow row)
        {
            if (Images == null || report == null || string.IsNullOrEmpty(WorkbookPath))
            {
                return;
            }

            Images.Write(clashTests, result, report, test, row, WorkbookPath);
        }

        private static string SourceFileOf(ModelItem item)
        {
            using (Model model = item.Model)
            {
                return model == null ? string.Empty : Or(model.FileName, string.Empty);
            }
        }

        /// <summary>
        /// The first property with one of these display names, looked for in any category,
        /// because which category holds Family differs between exporters. An empty string
        /// where none of them is there, never a guess.
        /// </summary>
        private static string FirstProperty(ModelItem item, string[] wanted)
        {
            string which;
            return FirstProperty(item, wanted, out which);
        }

        /// <summary>
        /// The same, and it also reports the display name that matched, which is what the
        /// client's Item ID column puts in front of the value.
        /// </summary>
        private static string FirstProperty(ModelItem item, string[] wanted, out string matched)
        {
            matched = string.Empty;

            if (wanted == null || wanted.Length == 0)
            {
                return string.Empty;
            }

            PropertyCategoryCollection categories = item.PropertyCategories;

            if (categories == null)
            {
                return string.Empty;
            }

            foreach (string name in wanted)
            {
                foreach (PropertyCategory category in categories)
                {
                    foreach (DataProperty property in category.Properties)
                    {
                        if (!string.Equals(property.DisplayName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        using (VariantData value = property.Value)
                        {
                            if (value == null)
                            {
                                continue;
                            }

                            string text = value.ToDisplayString();

                            if (!string.IsNullOrEmpty(text))
                            {
                                matched = property.DisplayName ?? string.Empty;
                                return text;
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Counts every clash under a group, however deep, and collects their distances so
        /// the most severe one can be worked out.
        /// </summary>
        private static int CountLeaves(SavedItemCollection children, IList<double> distances)
        {
            if (children == null)
            {
                return 0;
            }

            int leaves = 0;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    ClashResultGroup group = child as ClashResultGroup;

                    if (group != null)
                    {
                        leaves += CountLeaves(group.Children, distances);
                        continue;
                    }

                    ClashResult result = child as ClashResult;

                    if (result != null)
                    {
                        distances.Add(result.Distance);
                        leaves++;
                    }
                }
            }

            return leaves;
        }

        private GridSystem ActiveGrid(Document document)
        {
            try
            {
                return document.Grids == null ? null : document.Grids.ActiveSystem;
            }
            catch (Exception error)
            {
                log.Failure(
                    "reading the grid system",
                    error,
                    "kept going, every grid and level column is left empty");
                return null;
            }
        }

        private static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
