using System;
using System.Collections.Generic;
using System.Globalization;
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
            LookUpLevels = DefaultLookUpLevels;
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
            row.Name = Words.Or(group.DisplayName, "group");
            row.Status = (CoreClashStatus)(int)group.Status;
            row.Distance = ClashRow.MostSevere(distances, group.Distance);
            row.Description = Words.Or(group.Description, string.Empty);

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
            row.Name = Words.Or(result.DisplayName, "clash");
            row.Status = (CoreClashStatus)(int)result.Status;
            row.Distance = result.Distance;

            Fill(document, grid, row, result);
            return row;
        }

        private void Fill(Document document, GridSystem grid, ClashRow row, ClashResult result)
        {
            row.Found = result.CreatedTime;
            row.Description = Words.Or(result.Description, string.Empty);
            Place(grid, row, result.Center);

            // Item1 is the geometry the clash was found on, which for a Revit sourced
            // NWC is a leaf carrying a material name and no Revit properties at all.
            // CompositeItem1 is the element that leaf belongs to, which is where the id,
            // the family and the type live. A real run read Item1 alone and wrote an all
            // zero GUID into every one of the 426 item cells.
            using (ModelItem left = result.Item1)
            using (ModelItem leftWhole = result.CompositeItem1)
            {
                Describe(document, left, leftWhole, row.Left);
            }

            using (ModelItem right = result.Item2)
            using (ModelItem rightWhole = result.CompositeItem2)
            {
                Describe(document, right, rightWhole, row.Right);
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

                    row.GridLocation = Words.Or(intersection.DisplayName, string.Empty);

                    using (GridLevel level = intersection.Level)
                    {
                        if (level != null)
                        {
                            row.Level = Words.Or(level.DisplayName, string.Empty);
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
        private void Describe(Document document, ModelItem item, ModelItem whole, ClashItem into)
        {
            if (item == null)
            {
                return;
            }

            try
            {
                // Where a property is looked for: the item itself, then the element it
                // belongs to, then up the tree. Navisworks' own report finds an Element ID
                // for these, so it is somewhere above the geometry, and the first one
                // found wins.
                IList<ModelItem> lookIn = Upwards(item, whole);

                into.Name = Words.Or(item.DisplayName, string.Empty);
                into.Family = FirstProperty(lookIn, FamilyNames);
                into.Type = FirstProperty(lookIn, TypeNames);
                into.Material = FirstProperty(lookIn, MaterialNames);

                // The client's Item Type column, which reads Solid on every item cell of
                // the accepted report. ClassDisplayName is what the Item tab shows as the
                // type, so it is what that column is.
                into.ItemType = Words.Or(item.ClassDisplayName, string.Empty);

                // The id and, separately, the name of whatever property carried it,
                // because the client's Item ID column is that name and the value in one
                // field. Element ID on a Revit sourced NWC.
                string idFrom;
                into.ElementId = FirstProperty(lookIn, ElementIdNames, out idFrom);

                // The label is OURS to choose and it is always Element ID, because that is
                // what both client exports read and this report has to match them. It used
                // to be the display name of whichever property matched, and since Id is
                // first in the list above that came out as "Id: 990299" against their
                // "Element ID: 702888".
                //
                // Which property actually supplied the value is not lost, it goes in the
                // log, because renaming a value is only honest if what was renamed is
                // still visible somewhere.
                into.IdLabel = ClientFormat.DefaultIdLabel;
                into.IdFrom = idFrom;

                if (into.ElementId.Length == 0)
                {
                    // No id property anywhere above this item either, so the instance GUID
                    // is what is left. It is only used when it is a real one. An all zero
                    // GUID identifies nothing, so it is left out and the cell stays empty
                    // rather than carrying something that reads like an id.
                    string guid = GuidOf(lookIn);

                    if (!ClientFormat.NoIdAtAll(guid))
                    {
                        into.ElementId = guid;
                        into.IdLabel = "Instance GUID";
                    }
                }

            }
            catch (Exception error)
            {
                log.Failure(
                    "reading the properties of a clashing item",
                    error,
                    "kept going, the columns this item did not fill are left empty");
            }

            // In its OWN try, so a property that will not read cannot take these with it.
            // That is exactly what happened on 2026-09-01: one NotSupportedException out
            // of ToDisplayString aborted the whole of this method, and the source file and
            // the discipline came out empty on all 426 items because they are read last.
            try
            {
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
                    "reading which file a clashing item came from",
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
                return model == null ? string.Empty : Words.Or(model.FileName, string.Empty);
            }
        }

        /// <summary>
        /// How far up the tree a property is looked for. The most a real model needs is a
        /// few steps, and a bound stops a malformed tree turning one cell into a walk.
        /// </summary>
        public int LookUpLevels { get; set; }

        /// <summary>
        /// The item, then the element it belongs to, then its ancestors, in the order they
        /// should be searched. Nothing is disposed here, because the caller owns the two
        /// it passed in and the ancestors are borrowed the same way.
        /// </summary>
        private IList<ModelItem> Upwards(ModelItem item, ModelItem whole)
        {
            List<ModelItem> found = new List<ModelItem> { item };

            if (whole != null && !ReferenceEquals(whole, item))
            {
                found.Add(whole);
            }

            try
            {
                ModelItem walk = item.Parent;
                int levels = LookUpLevels < 1 ? DefaultLookUpLevels : LookUpLevels;

                for (int i = 0; i < levels && walk != null; i++)
                {
                    found.Add(walk);
                    walk = walk.Parent;
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "walking up from a clashing item to find its properties",
                    error,
                    "kept going, only the item itself was searched");
            }

            return found;
        }

        public const int DefaultLookUpLevels = 8;

        private static string GuidOf(IList<ModelItem> lookIn)
        {
            foreach (ModelItem item in lookIn)
            {
                string guid = item.InstanceGuid.ToString();

                if (!ClientFormat.NoIdAtAll(guid))
                {
                    return guid;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// The first property with one of these display names, looked for in any category,
        /// because which category holds Family differs between exporters. An empty string
        /// where none of them is there, never a guess.
        /// </summary>
        private static string FirstProperty(IList<ModelItem> lookIn, string[] wanted)
        {
            string which;
            return FirstProperty(lookIn, wanted, out which);
        }

        /// <summary>The first of these items to carry one of the names wins.</summary>
        private static string FirstProperty(
            IList<ModelItem> lookIn, string[] wanted, out string matched)
        {
            matched = string.Empty;

            foreach (ModelItem item in lookIn)
            {
                string found = FirstProperty(item, wanted, out matched);

                if (found.Length > 0)
                {
                    return found;
                }
            }

            matched = string.Empty;
            return string.Empty;
        }

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

            if (item == null || wanted == null || wanted.Length == 0)
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

                        string text = Text(property);

                        if (text.Length > 0)
                        {
                            matched = property.DisplayName ?? string.Empty;
                            return text;
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// One property as text, whatever kind of value it holds.
        ///
        /// This used to be ToDisplayString() alone, which is kind specific and throws
        /// NotSupportedException on anything that is not a display string. A Revit element
        /// id is an Int32, so it threw, 426 times on the run of 2026-09-01, and because
        /// one throw took the whole of Describe with it the element id, the source file
        /// and the discipline were all left empty and the client report lost its Item ID
        /// column.
        ///
        /// Every To&lt;Kind&gt;() on VariantData is kind specific in the same way. The one
        /// member that returns a value regardless of kind is ToString(), whose IL switches
        /// on GetDataType and calls the matching accessor, but it prefixes the kind name
        /// and hands back "Int32:702888". So the kind is read here and the right accessor
        /// called, which gives the value clean, and ToString is the fallback for a kind
        /// this does not know. See docs\scan.md section 4n.
        ///
        /// Never throws. A property that cannot be read is one empty cell, not a lost row.
        /// </summary>
        private static string Text(DataProperty property)
        {
            try
            {
                using (VariantData value = property.Value)
                {
                    if (value == null)
                    {
                        return string.Empty;
                    }

                    return Text(value);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static string Text(VariantData value)
        {
            switch (value.DataType)
            {
                case VariantDataType.None:
                    return string.Empty;

                case VariantDataType.DisplayString:
                    return value.ToDisplayString();

                case VariantDataType.IdentifierString:
                    return value.ToIdentifierString();

                case VariantDataType.Int32:
                    return value.ToInt32().ToString(CultureInfo.InvariantCulture);

                case VariantDataType.Boolean:
                    return value.ToBoolean().ToString(CultureInfo.InvariantCulture);

                case VariantDataType.DateTime:
                    return value.ToDateTime().ToString(
                        "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                case VariantDataType.Double:
                case VariantDataType.DoubleLength:
                case VariantDataType.DoubleAngle:
                case VariantDataType.DoubleArea:
                case VariantDataType.DoubleVolume:

                    // ToAnyDouble covers all five, which is what its name says and what
                    // saves five separate cases that would each throw on the other four.
                    return value.ToAnyDouble().ToString("0.######", CultureInfo.InvariantCulture);

                case VariantDataType.NamedConstant:
                    using (NamedConstant named = value.ToNamedConstant())
                    {
                        return named == null ? string.Empty : Words.Or(named.DisplayName, string.Empty);
                    }

                default:

                    // A kind nobody here has seen. ToString never throws, so the value is
                    // still read, and the kind prefix it adds is taken back off.
                    string fallback = VariantText.Clean(
                        value.ToString(), value.DataType.ToString());

                    return VariantText.IsNothing(fallback) ? string.Empty : fallback;
            }
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

    }
}
