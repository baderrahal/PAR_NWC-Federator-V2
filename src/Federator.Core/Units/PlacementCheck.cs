using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Units
{
    /// <summary>One model as the add-in read it: its file, its units and the centre of its bounding box in metres.</summary>
    public sealed class ModelBox
    {
        /// <summary>A model that was read.</summary>
        public ModelBox(string file, string units, double xMetres, double yMetres, double zMetres)
        {
            File = file ?? string.Empty;
            Units = units ?? string.Empty;
            X = xMetres;
            Y = yMetres;
            Z = zMetres;
        }

        /// <summary>A model whose box could not be read, and why.</summary>
        public ModelBox(string file, string units, string notRead)
        {
            File = file ?? string.Empty;
            Units = units ?? string.Empty;
            NotRead = string.IsNullOrEmpty(notRead) ? "UNKNOWN why" : notRead;
        }

        /// <summary>The model's file name, as Navisworks gives it.</summary>
        public string File { get; private set; }

        /// <summary>The model's units by the name on the Navisworks enum, such as Feet.</summary>
        public string Units { get; private set; }

        public double X { get; private set; }

        public double Y { get; private set; }

        public double Z { get; private set; }

        /// <summary>Why the box was not read, or null where it was.</summary>
        public string NotRead { get; private set; }
    }

    /// <summary>
    /// F201, the alignment of 04 PK. Bader opened an NWD the tool made and its models did not line up, where the same NWC
    /// files appended by hand did. MEASURED on 15 Oct 2026 on a copy of the ten NWCs of 1A04PK: appended alone, every model
    /// stands in Feet and their boxes lie about one building. Then every model set to Meters
    /// with its own transform handed back, which is what the tool did before the clash step until F201, made each model
    /// 3.28 times larger about its own origin, and the centres moved apart by up to 667 m, ME-000001. The tool no longer sets the
    /// units of a model. This check proves it on every group: each model as it was appended or opened is read again as
    /// the NWD is published, and a model whose centre moved more than the tolerance is named. A model whose units are not
    /// the document's is named too, because an NWF a run built before F201 still holds its models set to Meters.
    ///
    /// It reports and never acts, CLAUDE.md. Nothing here moves a model.
    /// </summary>
    public static class PlacementCheck
    {
        /// <summary>The prefix of every line, aligned with the other step prefixes.</summary>
        public const string Prefix = "PLACE    ";

        /// <summary>A model that moved less than this is held, 1 mm, Bader's order of 15 Oct 2026, task 5. A setting.</summary>
        public const double DefaultToleranceMillimetres = 1.0;

        /// <summary>
        /// One line a model, then one line of the counts. appended is the models as the group's document held them once
        /// every NWC was appended or the NWF opened, published the same models as the NWD is published. A model is paired
        /// by its place in the list where the file names agree there, and by its file name where they do not.
        /// </summary>
        public static IList<string> Lines(IList<ModelBox> appended, IList<ModelBox> published, string documentUnits, double toleranceMillimetres)
        {
            List<string> lines = new List<string>();
            IList<ModelBox> before = appended ?? new ModelBox[0];
            IList<ModelBox> after = published ?? new ModelBox[0];
            int held = 0;
            int moved = 0;
            int notRead = 0;
            int otherUnits = 0;

            lines.Add(Prefix + "each model as it was appended or opened, against the same model as the NWD is published, F201. "
                + "The tool sets no model's units and moves no model, as the Append command does not. Held means it moved "
                + Millimetres(toleranceMillimetres) + " mm or less");

            for (int i = 0; i < before.Count; i++)
            {
                ModelBox first = before[i];
                ModelBox last = PairOf(first, i, after);
                string head = Prefix + Words(first.File) + "  " + Words(first.Units) + "  ";

                if (first.NotRead != null || last == null || last.NotRead != null)
                {
                    notRead++;
                    lines.Add(head + "UNKNOWN, " + (first.NotRead != null
                        ? "its box was not read when it was appended, " + first.NotRead
                        : last == null
                            ? "it was not found as the NWD is published"
                            : "its box was not read as the NWD is published, " + last.NotRead));
                }
                else
                {
                    double distance = DistanceMillimetres(first, last);
                    bool isHeld = distance <= toleranceMillimetres;

                    if (isHeld)
                    {
                        held++;
                    }
                    else
                    {
                        moved++;
                    }

                    lines.Add(head + "centre E " + Metres(first.X) + " N " + Metres(first.Y) + " Z " + Metres(first.Z)
                        + " m when appended, moved " + Millimetres(distance) + " mm before the NWD, "
                        + (isHeld ? "held" : "MOVED more than " + Millimetres(toleranceMillimetres) + " mm"));
                }

                if (!string.IsNullOrEmpty(documentUnits) && first.Units.Length > 0
                    && !string.Equals(first.Units, documentUnits, StringComparison.Ordinal))
                {
                    otherUnits++;
                    lines.Add(Prefix + Words(first.File) + "  its units " + first.Units + " are not the document's " + documentUnits
                        + ". A run of this tool before F201 set every model to Meters, which scales a model about its own origin"
                        + " and moves it. Where this NWF was built before F201, Rebuild the group on the NWC page");
                }
            }

            lines.Add(Prefix + held + " of " + before.Count + " models held, " + moved + " moved, " + notRead + " not read, "
                + otherUnits + " in units that are not the document's");
            return lines;
        }

        /// <summary>The distance between the two centres, in millimetres.</summary>
        public static double DistanceMillimetres(ModelBox a, ModelBox b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            double dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz) * 1000.0;
        }

        private static ModelBox PairOf(ModelBox model, int at, IList<ModelBox> published)
        {
            if (at < published.Count && string.Equals(published[at].File, model.File, StringComparison.Ordinal))
            {
                return published[at];
            }

            foreach (ModelBox other in published)
            {
                if (string.Equals(other.File, model.File, StringComparison.Ordinal))
                {
                    return other;
                }
            }

            return null;
        }

        private static string Words(string text)
        {
            return string.IsNullOrEmpty(text) ? "a model with no name" : text;
        }

        private static string Metres(double value)
        {
            return value.ToString("0.000", CultureInfo.InvariantCulture);
        }

        private static string Millimetres(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
