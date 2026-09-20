using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Health
{
    /// <summary>
    /// Where ONE model in a group sits, as plain numbers, so Core can compare them with
    /// no Navisworks type anywhere near it. The add-in reads them off the model root.
    /// </summary>
    public sealed class ModelPlacement
    {
        /// <summary>A placement whose numbers could not be read at all is still a model and still says so.</summary>
        public const double NotRead = double.NaN;

        public ModelPlacement(string file, string discipline, string sharedCoordinate, double x, double y, double z)
        {
            File = file ?? string.Empty;
            Discipline = discipline ?? string.Empty;
            SharedCoordinate = sharedCoordinate ?? string.Empty;
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>The NWC file name, which is what a person looks for on disk.</summary>
        public string File { get; private set; }

        /// <summary>Part 5 of the file name, the discipline code, or empty where the name would not parse.</summary>
        public string Discipline { get; private set; }

        /// <summary>
        /// The NAME of the Revit shared site the model was exported on, read off the model
        /// root's Location tab as revit_ProjectLocation, docs\history\scan.md 5q, or empty
        /// where the model carries none.
        /// </summary>
        public string SharedCoordinate { get; private set; }

        /// <summary>How far the model is moved in X, in millimetres, or NotRead.</summary>
        public double X { get; private set; }

        /// <summary>How far the model is moved in Y, in millimetres, or NotRead.</summary>
        public double Y { get; private set; }

        /// <summary>How far the model is moved in Z, in millimetres, or NotRead.</summary>
        public double Z { get; private set; }

        /// <summary>Whether all three numbers were read. A placement that was not read is never called different.</summary>
        public bool Placed
        {
            get { return !double.IsNaN(X) && !double.IsNaN(Y) && !double.IsNaN(Z); }
        }

        /// <summary>Whether the model names a shared site at all.</summary>
        public bool NamesASharedCoordinate
        {
            get { return SharedCoordinate.Length > 0; }
        }
    }

    /// <summary>
    /// Whether the models of one group agree about where they are, Q64 answered on
    /// 2026-09-20: BY SHARED COORDINATE, and by eye where that cannot be read.
    ///
    /// IT IS NOT A BOUNDING BOX AND MUST NEVER BECOME ONE. An electrical model
    /// legitimately covers a smaller area than the architecture, so a box comparison
    /// would flag a correct model every time, which is the fastest way to make a person
    /// stop reading a block.
    ///
    /// WHAT IS COMPARED WAS MEASURED FIRST, docs\history\scan.md 5q. Every NWC this
    /// project exports carries, on its model ROOT and nowhere else:
    ///     revit_ProjectLocation, the NAME of the Revit shared site
    ///     a Transform with a translation in X, Y and Z
    /// So both are reported, side by side, per model against a reference. Two models on
    /// the same named site should sit at the same offset, and two on different sites may
    /// or may not, which is exactly the judgement a person makes and this tool does not.
    ///
    /// THE IDENTITY IS NOT THE TEST. Every model in this project returns IsIdentity false,
    /// including the ones whose translation is (0, 0, 0), because the export carries a
    /// scale too. What matters is whether the models in one group AGREE WITH EACH OTHER.
    ///
    /// Nothing here fails a group, Q65 answered: report it and run anyway, never skip a
    /// group and never stop a run for it.
    /// </summary>
    public static class AlignmentCheck
    {
        /// <summary>The block's title, which the add-in puts the building after.</summary>
        public const string BlockTitle = "ALIGNMENT";

        /// <summary>
        /// The discipline whose model everything else is compared against. Architecture,
        /// because it is the one discipline every building in this project carries and
        /// the one a coordinator opens first. A SETTING and not a constant: a group with
        /// no model of this discipline uses its first model instead and SAYS which.
        /// </summary>
        public const string DefaultReferenceDiscipline = "AR";

        /// <summary>
        /// How far apart two models may sit, in millimetres, before it is called a
        /// difference.
        ///
        /// ONE MILLIMETRE IS CHOSEN AND NOT MEASURED, and it says so because the rule is
        /// to say UNKNOWN rather than fill a gap. Anything at or under a millimetre is
        /// the export rounding a number. Anything above it is an offset somebody put
        /// there. The real ones seen on 2026-09-20 were 312 mm between two models of one
        /// group and 169 metres between two of another, so nothing rests on the exact
        /// value, and it is a setting so the next person can move it without a build.
        /// </summary>
        public const double DefaultToleranceMillimetres = 1.0;

        /// <summary>
        /// What Revit calls the site of a model that was NOT exported on a shared
        /// location. A SETTING, because it is a word read out of somebody else's file and
        /// nothing in this code decides what another project's Revit calls it.
        /// </summary>
        public const string DefaultInternalName = "Internal";

        /// <summary>The block. Written even when everything agrees, because a missing block reads as a check that did not run.</summary>
        public static IList<string> Lines(IList<ModelPlacement> models)
        {
            return Lines(models, DefaultReferenceDiscipline, DefaultToleranceMillimetres, DefaultInternalName);
        }

        public static IList<string> Lines(
            IList<ModelPlacement> models, string referenceDiscipline, double toleranceMillimetres, string internalName)
        {
            List<string> lines = new List<string>();

            if (models == null || models.Count == 0)
            {
                lines.Add("no model was read, so nothing could be compared");
                return lines;
            }

            ModelPlacement reference = ReferenceIn(models, referenceDiscipline);

            if (reference == null)
            {
                lines.Add("no model in this group could be placed, so the models were not compared."
                    + " Check by eye that they sit on the same coordinates.");
                return lines;
            }

            lines.Add(Named(reference) + " is the reference"
                + (string.Equals(reference.Discipline, referenceDiscipline, StringComparison.Ordinal)
                    ? string.Empty
                    : ", because this group carries no " + referenceDiscipline + " model"));

            int different = 0;
            int notPlaced = 0;
            int onInternal = 0;
            HashSet<string> sites = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < models.Count; i++)
            {
                ModelPlacement model = models[i];

                if (model.NamesASharedCoordinate)
                {
                    sites.Add(model.SharedCoordinate);

                    if (string.Equals(model.SharedCoordinate, internalName, StringComparison.Ordinal))
                    {
                        onInternal++;
                    }
                }

                if (model == reference)
                {
                    lines.Add("   " + Named(model) + "   reference, " + Site(model));
                    continue;
                }

                if (!model.Placed)
                {
                    notPlaced++;
                    lines.Add("   " + Named(model) + "   NOT READ, its placement could not be read, " + Site(model));
                    continue;
                }

                double dx = model.X - reference.X;
                double dy = model.Y - reference.Y;
                double dz = model.Z - reference.Z;

                if (Within(dx, toleranceMillimetres) && Within(dy, toleranceMillimetres) && Within(dz, toleranceMillimetres))
                {
                    lines.Add("   " + Named(model) + "   same placement, " + Site(model));
                    continue;
                }

                different++;
                lines.Add("   " + Named(model) + "   DIFFERENT  dx " + Millimetres(dx)
                    + "  dy " + Millimetres(dy) + "  dz " + Millimetres(dz) + ", " + Site(model));
                lines.Add("      ask the " + Words(model.Discipline, "model's") + " originator to re-export on the project shared coordinates");
            }

            lines.Add(Sentence(models.Count, different, notPlaced, toleranceMillimetres));

            if (sites.Count > 1)
            {
                lines.Add("this group names " + sites.Count + " different shared coordinates: " + Joined(sites));
            }

            if (onInternal > 0)
            {
                lines.Add(onInternal + " model(s) name their site \"" + internalName
                    + "\", which is what Revit calls a model that was not exported on a shared site at all");
            }

            return lines;
        }

        /// <summary>How many models sit somewhere the reference does not, for the run line. Never fails anything.</summary>
        public static int DifferentCount(IList<ModelPlacement> models, double toleranceMillimetres)
        {
            if (models == null || models.Count == 0)
            {
                return 0;
            }

            ModelPlacement reference = ReferenceIn(models, DefaultReferenceDiscipline);

            if (reference == null)
            {
                return 0;
            }

            int different = 0;

            for (int i = 0; i < models.Count; i++)
            {
                ModelPlacement model = models[i];

                if (model == reference || !model.Placed)
                {
                    continue;
                }

                if (!Within(model.X - reference.X, toleranceMillimetres)
                    || !Within(model.Y - reference.Y, toleranceMillimetres)
                    || !Within(model.Z - reference.Z, toleranceMillimetres))
                {
                    different++;
                }
            }

            return different;
        }

        /// <summary>
        /// The model everything else is compared against: the first of the reference
        /// discipline that could be placed, then the first that could be placed at all,
        /// or null where none could.
        /// </summary>
        private static ModelPlacement ReferenceIn(IList<ModelPlacement> models, string discipline)
        {
            for (int i = 0; i < models.Count; i++)
            {
                if (models[i].Placed && string.Equals(models[i].Discipline, discipline, StringComparison.Ordinal))
                {
                    return models[i];
                }
            }

            for (int i = 0; i < models.Count; i++)
            {
                if (models[i].Placed)
                {
                    return models[i];
                }
            }

            return null;
        }

        private static string Sentence(int models, int different, int notPlaced, double tolerance)
        {
            if (different == 0 && notPlaced == 0)
            {
                return "all " + models + " model(s) sit within "
                    + Millimetres(tolerance) + " of the reference";
            }

            string said = different + " of " + models + " model(s) sit somewhere the reference does not";

            if (notPlaced > 0)
            {
                said += ", and " + notPlaced + " could not be placed and were not compared";
            }

            return said + ". Nothing is changed and the run goes on.";
        }

        private static bool Within(double difference, double tolerance)
        {
            return Math.Abs(difference) <= Math.Abs(tolerance);
        }

        private static string Named(ModelPlacement model)
        {
            string discipline = Words(model.Discipline, "??");
            return discipline + "  " + Words(model.File, "a model with no name");
        }

        private static string Site(ModelPlacement model)
        {
            return model.NamesASharedCoordinate
                ? "shared coordinate \"" + model.SharedCoordinate + "\""
                : "NO shared coordinate on the model at all, so check this one by eye";
        }

        private static string Millimetres(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture) + " mm";
        }

        private static string Words(string value, string instead)
        {
            return string.IsNullOrEmpty(value) ? instead : value;
        }

        private static string Joined(HashSet<string> values)
        {
            string[] array = new string[values.Count];
            values.CopyTo(array);
            Array.Sort(array, StringComparer.Ordinal);
            return string.Join(", ", array);
        }
    }
}
