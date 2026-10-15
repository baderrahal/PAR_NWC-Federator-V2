using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Federator.Core.Units;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// F201. Each model of the open document as Core's PlacementCheck reads it: its file, its units and the centre of the
    /// bounding box of its root item in metres. ModelItem.BoundingBox gives the box in the document's units, converted
    /// through UnitTable, the one table of units. Measured on 15 Oct 2026 on the ten NWCs of 1A04PK by the work probe's
    /// boxes mode. It reads only, and a model it cannot read comes back with why and never throws.
    /// </summary>
    public static class ModelBoxReader
    {
        /// <summary>Every model of the document in its order, each read or with why it was not.</summary>
        public static IList<ModelBox> Read(Document document)
        {
            List<ModelBox> boxes = new List<ModelBox>();

            if (document == null)
            {
                return boxes;
            }

            UnitRow units = UnitTable.FindByEnumName(document.Units.ToString());
            double metresPerUnit = units == null ? 0.0 : units.MillimetresPerUnit / 1000.0;

            foreach (Model model in document.Models)
            {
                string file = string.Empty;
                string modelUnits = string.Empty;

                try
                {
                    file = System.IO.Path.GetFileName(model.FileName ?? string.Empty);
                    modelUnits = model.Units.ToString();

                    if (metresPerUnit <= 0.0)
                    {
                        boxes.Add(new ModelBox(file, modelUnits, "the document's units, " + document.Units + ", are not in the table of units"));
                        continue;
                    }

                    using (ModelItem root = model.RootItem)
                    {
                        BoundingBox3D box = root.BoundingBox();

                        if (box == null || box.IsEmpty)
                        {
                            boxes.Add(new ModelBox(file, modelUnits, "its root item has no box"));
                            continue;
                        }

                        Point3D centre = box.Center;
                        boxes.Add(new ModelBox(file, modelUnits, centre.X * metresPerUnit, centre.Y * metresPerUnit, centre.Z * metresPerUnit));
                    }
                }
                catch (Exception error)
                {
                    boxes.Add(new ModelBox(file, modelUnits, error.GetType().Name + ": " + error.Message));
                }
            }

            return boxes;
        }
    }
}
