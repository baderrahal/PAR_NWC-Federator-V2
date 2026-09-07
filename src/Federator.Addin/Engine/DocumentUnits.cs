using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Putting the open document into the units the report has to be in.
    ///
    /// WHY. This is a Saudi project and every report the team sends is metric. A run on a
    /// document in feet wrote tolerance 0.246ft and distance -1.706, which is 520 mm, and
    /// a report in feet is not usable. Converting the numbers ourselves is not an option:
    /// the tolerance, the distances and the coordinates all come out of the document in
    /// the document's units, so a report that says metres while the document says feet is
    /// a report that lies.
    ///
    /// WHAT THE DLL SAYS, measured on 2026-09-01, see docs\scan.md section 4q.
    ///
    ///   Document.Units   read only, no setter at all
    ///   Model.Units      read only, no setter at all
    ///
    /// and across all 4027 types in Autodesk.Navisworks.Api.dll there is no settable Units
    /// property and no Set, Convert or Change units method, except these:
    ///
    ///   public void DocumentModels.SetModelUnitsAndTransform(
    ///       Model model, Units units, Transform3D transform, bool transformReflected)
    ///
    ///   Autodesk.Navisworks.Api.Interop.LcOpModel.SetOriginalUnits(Units)
    ///   Autodesk.Navisworks.Api.Interop.LcVwDocument.SetModelUnitsAndTransform(...)
    ///
    /// So the DOCUMENT's units cannot be set. Each MODEL's can, through the one public
    /// managed member above, which is reached as Document.Models. Whether the document's
    /// own Units then follows from the models it holds is UNKNOWN, because it cannot be
    /// read off reflection and needs a run to see.
    ///
    /// This therefore sets every model, logs what the document said before and after, and
    /// says plainly when the document did not follow. It never claims the change worked.
    ///
    /// IT IS A MUTATION. Setting a model's units changes what the NWF holds, so it is
    /// logged with what the document was and what it became. It is on for every run,
    /// because the window sets it on and every report the team sends is metric. It
    /// happens before the clash step so tolerances and distances are read in the new
    /// units, and each model keeps its own transform rather than being handed a new one.
    /// </summary>
    public sealed class DocumentUnits
    {
        private readonly Federator.Core.Diagnostics.RunLog log;

        public DocumentUnits(Federator.Core.Diagnostics.RunLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
        }

        /// <summary>
        /// Puts the document into these units, as far as the API allows, and says what
        /// happened. Returns the units the document reports afterwards.
        ///
        /// Never throws. A document that will not change units is a report in the wrong
        /// units, which is worth saying loudly, and is not worth losing a run over.
        /// </summary>
        public Units Apply(Document document, Units wanted)
        {
            if (document == null)
            {
                return wanted;
            }

            Units before;

            try
            {
                before = document.Units;
            }
            catch (Exception error)
            {
                log.Failure("reading the document units", error,
                    "kept going, the report carries whatever the document is in");
                return wanted;
            }

            if (before == wanted)
            {
                log.Line("UNITS    the document is already in " + Name(wanted)
                    + ", so nothing was changed.");
                return before;
            }

            log.Line("UNITS    the document is in " + Name(before) + " and the report needs "
                + Name(wanted) + ". Setting every model.");

            int changed = 0;
            int failed = 0;

            foreach (Model model in Models(document))
            {
                if (Set(document, model, wanted))
                {
                    changed++;
                }
                else
                {
                    failed++;
                }
            }

            Units after = before;

            try
            {
                after = document.Units;
            }
            catch (Exception)
            {
                // The line below still says what was attempted.
            }

            log.Line("UNITS    " + changed + " model" + (changed == 1 ? "" : "s") + " set, "
                + failed + " that would not. The document was " + Name(before)
                + " and reports " + Name(after) + " now.");

            if (after != wanted)
            {
                log.Line("UNITS    THE DOCUMENT DID NOT FOLLOW. Every number in this "
                    + "report is in " + Name(after) + ", the tolerance included, and the "
                    + "report says so rather than pretending otherwise.");
            }

            return after;
        }

        private IList<Model> Models(Document document)
        {
            List<Model> models = new List<Model>();

            try
            {
                foreach (Model model in document.Models)
                {
                    models.Add(model);
                }
            }
            catch (Exception error)
            {
                log.Failure("reading the models to set their units", error,
                    "kept going, no model was changed");
            }

            return models;
        }

        /// <summary>
        /// One model. Its OWN transform is handed back to it, because the only member that
        /// sets units takes a transform as well and a new one would move the geometry.
        /// </summary>
        private bool Set(Document document, Model model, Units wanted)
        {
            try
            {
                document.Models.SetModelUnitsAndTransform(
                    model, wanted, model.Transform, model.IsTransformReflected);
                return true;
            }
            catch (Exception error)
            {
                log.Failure(
                    "setting the units of " + Or(model.FileName, "a model"),
                    error,
                    "kept going, that model keeps the units it had");
                return false;
            }
        }

        /// <summary>The short name the report writes, which is what the samples use.</summary>
        public static string Short(Units units)
        {
            switch (units)
            {
                case Units.Meters: return "m";
                case Units.Centimeters: return "cm";
                case Units.Millimeters: return "mm";
                case Units.Feet: return "ft";
                case Units.Inches: return "in";
                case Units.Yards: return "yd";
                case Units.Kilometers: return "km";
                case Units.Miles: return "mi";
                default: return units.ToString().ToLowerInvariant();
            }
        }

        private static string Name(Units units)
        {
            return units.ToString() + " (" + Short(units) + ")";
        }

        private static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
