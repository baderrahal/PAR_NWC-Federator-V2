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

        /// <summary>
        /// A site whose read threw, which says nothing about the model. It is NOT an empty
        /// site: an empty one is a model that names no shared site, which fails its group,
        /// Q70, and a read that threw used to come back as that and fail the group with a
        /// reason that read as a fact about the model. Null, because every site that was
        /// read is a string.
        /// </summary>
        public const string SiteNotRead = null;

        public ModelPlacement(string file, string discipline, string sharedCoordinate, double x, double y, double z)
        {
            File = file ?? string.Empty;
            Discipline = discipline ?? string.Empty;
            SiteRead = sharedCoordinate != SiteNotRead;
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
        /// where the model carries none or the read threw, which SiteRead tells apart.
        /// </summary>
        public string SharedCoordinate { get; private set; }

        /// <summary>Whether the site was read at all. A site that was not read fails nothing and is said UNKNOWN.</summary>
        public bool SiteRead { get; private set; }

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

        /// <summary>Whether the model names a shared site at all. False too where the site was not read.</summary>
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
    /// Nothing here skips a group or stops a run, Q65 answered: report it and run anyway.
    /// Two answers given later decide how a group ENDS, and the group still writes its NWF
    /// and its NWD either way. Q70: a model naming no site at all fails its group. Q98 B2
    /// with Bader's answer to Q99 and Q100: a model naming Internal, or sitting more than
    /// the far model setting from its group's reference, is not on the same shared
    /// coordinates, and the group's clash is skipped and nothing else, OffCoordinates.
    /// That replaces Q70 for a model on Internal, which failed its group until then and
    /// still does with the rule switched off.
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

        /// <summary>
        /// How far a model may sit from its group's reference model, in millimetres and in
        /// a straight line, before it is not on the same shared coordinates. Bader's B2 of
        /// Q98 on 2026-10-04, put to him again with the 40 distances of the C06 run as Q99
        /// and answered the same day.
        ///
        /// ONE METRE IS HIS NUMBER AND NOT A MEASUREMENT. It is the default of the setting
        /// a run reads, ReportOptions.FarModelMillimetres, and only that default reads it.
        ///
        /// THE STRAIGHT LINE OF DX, DY AND DZ, AND NOT EACH AXIS ON ITS OWN. The ME of
        /// 1B06WM in that run sat 996.2 mm, 663.15 mm and 150 mm off its reference, under a
        /// metre on every axis and 1.206 m away, and a rule reading each axis would have
        /// called it in place.
        /// </summary>
        public const double DefaultFarModelMillimetres = 1000.0;

        /// <summary>
        /// Whether a group holding a model not on the same shared coordinates skips its
        /// clash, Bader's answer to Q99 and Q100. ON, and a setting,
        /// ReportOptions.SkipClashOffCoordinates, because a building is run once more with it
        /// off so every other fix is proved on groups that clash.
        /// </summary>
        public const bool DefaultSkipClashOffCoordinates = true;

        /// <summary>
        /// The block, with the models not on the same shared coordinates measured against
        /// the given distance in millimetres, and said the way the rule that skips the
        /// clash is set. Written even when everything agrees, because a missing block reads
        /// as a check that did not run.
        /// </summary>
        public static IList<string> Lines(
            IList<ModelPlacement> models, double farModelMillimetres, bool skipClashOffCoordinates)
        {
            return Lines(
                models,
                DefaultReferenceDiscipline,
                DefaultToleranceMillimetres,
                DefaultInternalName,
                farModelMillimetres,
                skipClashOffCoordinates);
        }

        public static IList<string> Lines(
            IList<ModelPlacement> models,
            string referenceDiscipline,
            double toleranceMillimetres,
            string internalName,
            double farModelMillimetres,
            bool skipClashOffCoordinates)
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
                lines.Add("no model in this group could be placed, so the models were not compared"
                    + " and none is called a far model. Check by eye that they sit on the same coordinates.");

                // A model on Internal is known by its site alone, so it still skips the
                // clash or fails the group here, and the block says so like any other.
                OffCoordinates unplaced = NotOnTheSameCoordinates(models, null, internalName, farModelMillimetres);

                if (unplaced.Any)
                {
                    AddOffCoordinates(lines, unplaced, skipClashOffCoordinates);
                }

                AddFailure(lines, models, internalName, skipClashOffCoordinates);
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
                    lines.Add("   " + Named(model) + "   NOT READ, its placement could not be read,"
                        + " so it is not compared and is not called a far model, " + Site(model));
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

            // Bader's answer to Q99 and Q100. The same lines the note and the run's list
            // carry, out of the same rule, so the log and the files cannot disagree.
            OffCoordinates off = NotOnTheSameCoordinates(models, reference, internalName, farModelMillimetres);

            if (!off.Any)
            {
                lines.Add("no model names \"" + internalName + "\" or sits more than " + Metres(farModelMillimetres)
                    + " from the reference model in a straight line");
            }
            else
            {
                AddOffCoordinates(lines, off, skipClashOffCoordinates);
            }

            if (sites.Count > 1)
            {
                lines.Add("this group names " + sites.Count + " different shared coordinates: " + Joined(sites));
            }

            if (onInternal > 0)
            {
                lines.Add(onInternal + " model(s) name their site \"" + internalName
                    + "\", which is what Revit calls a model that was not exported on a shared site at all");
            }

            AddFailure(lines, models, internalName, skipClashOffCoordinates);
            return lines;
        }

        /// <summary>The models not on the same shared coordinates under a heading that says what the rule did with them.</summary>
        private static void AddOffCoordinates(IList<string> lines, OffCoordinates off, bool skipClashOffCoordinates)
        {
            // Not "as the reference model", because the reference itself can be on Internal.
            lines.Add(skipClashOffCoordinates
                ? "CLASH SKIPPED. " + off.Models.Count + " model(s) are not on the same shared coordinates, so the"
                    + " clash tests are created and none is run, and no viewpoint and no clash report is made:"
                : off.Models.Count + " model(s) are not on the same shared coordinates. The rule that skips the"
                    + " clash for them is off for this run, so the group is clashed as before:");

            for (int i = 0; i < off.Models.Count; i++)
            {
                lines.Add("   " + off.Models[i]);
            }
        }

        /// <summary>
        /// Q70. The block says the group is failed and why, and the group still writes its
        /// NWF and its NWD, which are the evidence.
        /// </summary>
        private static void AddFailure(
            IList<string> lines, IList<ModelPlacement> models, string internalName, bool skipClashOffCoordinates)
        {
            string fails = WhyItFailsTheGroup(models, internalName, skipClashOffCoordinates);

            if (fails != null)
            {
                lines.Add("THIS GROUP IS FAILED. " + fails);
            }
        }

        /// <summary>
        /// Why this group is FAILED, or null where it is not, Q70 answered b on
        /// 2026-09-20. A group fails when any of its models names the internal origin as
        /// its shared site, or names no site at all. A model whose site could not be read
        /// does neither, because a read that threw is not a fact about the model.
        ///
        /// BADER'S ANSWER TO Q100 REPLACES THE FIRST HALF while the rule that skips the
        /// clash is on: a model named Internal no longer fails its group, it skips the
        /// clash, NotOnTheSameCoordinates. A model naming no site at all still fails it,
        /// because his answer named Internal and the distance and not that. With the rule
        /// off both halves fail the group as they did before his answer.
        ///
        /// FAILED DOES NOT MEAN THE GROUP PRODUCES NOTHING. The federation and the NWD are
        /// still written, and the clash report too unless the clash was skipped, because
        /// Bader needs the evidence to take to the people who own the models, and a group
        /// that produces nothing gives him nothing to send. This says the group is not DONE and names the model, and
        /// the engine carries on.
        ///
        /// WHY IT IS A FAILURE AND NOT A WARNING. A model on the internal origin is not
        /// slightly out of place, it is in a different coordinate system, so every clash
        /// the run reports against it is either a clash that is not there or a miss that
        /// is. The numbers are worse than useless because they read as real.
        /// </summary>
        public static string WhyItFailsTheGroup(IList<ModelPlacement> models, bool skipClashOffCoordinates)
        {
            return WhyItFailsTheGroup(models, DefaultInternalName, skipClashOffCoordinates);
        }

        public static string WhyItFailsTheGroup(
            IList<ModelPlacement> models, string internalName, bool skipClashOffCoordinates)
        {
            if (models == null || models.Count == 0)
            {
                return null;
            }

            List<string> onInternal = new List<string>();
            List<string> withNoSite = new List<string>();

            for (int i = 0; i < models.Count; i++)
            {
                // A site whose read threw says nothing about the model, so the model is
                // judged on neither half of this rule and the block says UNKNOWN for it.
                if (!models[i].SiteRead)
                {
                    continue;
                }

                if (!models[i].NamesASharedCoordinate)
                {
                    withNoSite.Add(Named(models[i]));
                }
                else if (!skipClashOffCoordinates
                    && string.Equals(models[i].SharedCoordinate, internalName, StringComparison.Ordinal))
                {
                    onInternal.Add(Named(models[i]));
                }
            }

            if (onInternal.Count == 0 && withNoSite.Count == 0)
            {
                return null;
            }

            string why = string.Empty;

            if (onInternal.Count > 0)
            {
                why = onInternal.Count + " model(s) were exported on Revit's internal origin and not on a"
                    + " shared site, which puts them in a different coordinate system from the rest of"
                    + " the group: " + string.Join(", ", onInternal.ToArray());
            }

            if (withNoSite.Count > 0)
            {
                why += (why.Length > 0 ? ". And " : string.Empty)
                    + withNoSite.Count + " model(s) name no shared site at all: "
                    + string.Join(", ", withNoSite.ToArray());
            }

            // With the rule on the same group may also have its clash skipped, and then no
            // report is written, so only what is written either way is claimed.
            return why + (skipClashOffCoordinates
                ? ". Its NWF and its NWD were still written, so the evidence is there to send."
                : ". Every output of this group was still written, so the evidence is there to send.");
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
        /// The models of this group not on the same shared coordinates as its reference
        /// model, Bader's answer to Q99 and Q100: each one naming Internal as its shared
        /// site, or sitting more than farModelMillimetres from the reference in a straight
        /// line, with the line that names its file, its shared site and its distance in X,
        /// Y and Z. The reference is the one the block measures against and is named too
        /// when it is on Internal itself. A model whose placement could not be read is
        /// never judged on its distance, the same way it is never called different, and
        /// one on Internal is still named, with its distance UNKNOWN.
        /// </summary>
        public static OffCoordinates NotOnTheSameCoordinates(IList<ModelPlacement> models, double farModelMillimetres)
        {
            if (models == null || models.Count == 0)
            {
                return new OffCoordinates(null, new List<string>());
            }

            return NotOnTheSameCoordinates(
                models, ReferenceIn(models, DefaultReferenceDiscipline), DefaultInternalName, farModelMillimetres);
        }

        private static OffCoordinates NotOnTheSameCoordinates(
            IList<ModelPlacement> models, ModelPlacement reference, string internalName, double farModelMillimetres)
        {
            List<string> off = new List<string>();

            for (int i = 0; i < models.Count; i++)
            {
                ModelPlacement model = models[i];

                // A site whose read threw is not Internal, FR-002, and SiteRead says so.
                bool onInternal = model.SiteRead
                    && string.Equals(model.SharedCoordinate, internalName, StringComparison.Ordinal);
                bool measured = reference != null && model != reference && model.Placed;
                double dx = measured ? model.X - reference.X : 0.0;
                double dy = measured ? model.Y - reference.Y : 0.0;
                double dz = measured ? model.Z - reference.Z : 0.0;
                double distance = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
                bool far = measured && distance > farModelMillimetres;

                if (!onInternal && !far)
                {
                    continue;
                }

                string where = model == reference
                    ? "the reference model itself"
                    : !measured
                        ? "distance from the reference UNKNOWN, its placement could not be read"
                        : "X " + Millimetres(dx) + "  Y " + Millimetres(dy) + "  Z " + Millimetres(dz)
                            + " from the reference, " + Metres(distance) + " in a straight line"
                            + (far ? ", more than " + Metres(farModelMillimetres) : string.Empty);

                off.Add(Named(model) + "   shared site " + SiteSaid(model)
                    + (onInternal ? ", Revit's own origin and not a shared site" : string.Empty)
                    + "   " + where);
            }

            return new OffCoordinates(reference == null ? null : Named(reference), off);
        }

        /// <summary>The site in a line about coordinates: its name in quotes, or why there is none.</summary>
        private static string SiteSaid(ModelPlacement model)
        {
            if (!model.SiteRead)
            {
                return "UNKNOWN, it could not be read";
            }

            return model.NamesASharedCoordinate ? "\"" + model.SharedCoordinate + "\"" : "none named";
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
            if (!model.SiteRead)
            {
                return "shared coordinate UNKNOWN, it could not be read off the model, so the model is not judged on it";
            }

            return model.NamesASharedCoordinate
                ? "shared coordinate \"" + model.SharedCoordinate + "\""
                : "NO shared coordinate on the model at all, so check this one by eye";
        }

        /// <summary>
        /// The site as the row file carries it: its name, or that the model names none, or
        /// that it could not be read. Here and not in the add-in, because the row file said
        /// no shared coordinate for a read that threw, the same false claim the block made.
        /// </summary>
        public static string SiteName(ModelPlacement model)
        {
            if (model == null || !model.SiteRead)
            {
                return "UNKNOWN, the site could not be read";
            }

            return model.NamesASharedCoordinate ? model.SharedCoordinate : "no shared coordinate on the model";
        }

        private static string Millimetres(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture) + " mm";
        }

        /// <summary>A length in millimetres said in metres to the millimetre, the unit Bader gave the far model rule in.</summary>
        private static string Metres(double millimetres)
        {
            return (millimetres / 1000.0).ToString("0.###", CultureInfo.InvariantCulture) + " m";
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
