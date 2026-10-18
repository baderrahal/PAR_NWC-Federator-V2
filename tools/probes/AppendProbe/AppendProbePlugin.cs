using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;

namespace AppendProbe
{
    /// <summary>
    /// F231, the append measured against a manual Append, Bader's round of 19 Oct 2026, fix 1. Read only: each file it is
    /// handed, an NWF or an NWD, is opened in turn by Document.TryOpenFile, and per model it writes one row of the file name,
    /// the source file name, the model's units, the translation of the model's transform and the min and max of its root
    /// item's box. Nothing is saved, published, appended, hidden or changed, so the file on the disk is as it was, which the
    /// host proves by its SHA-256 before and after.
    ///
    /// The translation and the box are in the document's units, as Navisworks hands them, and in millimetres through
    /// UnitConversion.ScaleFactor, docs\history\scan.md 5z-g, read the way DocumentReadProbe reads it: ScaleFactor(from, to),
    /// judged only where ScaleFactor(Millimeters, Meters) reads below one, and UNKNOWN otherwise.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Append probe", ToolTip = "F231, where each model of an NWF or NWD sits")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class AppendProbePlugin : AddInPlugin
    {
        public const string PluginName = "AppendProbe";
        public const string DeveloperCode = "PARS";

        private const string Unknown = "UNKNOWN";
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private StreamWriter results;

        /// <summary>parameters: read, the output file, the files joined by |, their labels joined by |.</summary>
        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 4 || parameters[0] != "read")
            {
                return 2;
            }

            using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                Say("probe started, mode read");

                try
                {
                    string[] files = parameters[2].Split('|');
                    string[] labels = parameters[3].Split('|');

                    if (files.Length != labels.Length)
                    {
                        Say("UNKNOWN: " + files.Length + " files and " + labels.Length + " labels");
                        return 3;
                    }

                    Say("ROWHEAD\tlabel\tfile opened\tmodel\tfile name\tsource file name\tmodel units\tdocument units\ttranslation x\ty\tz"
                        + "\tbox min x\ty\tz\tbox max x\ty\tz\tmm per document unit\ttranslation x mm\ty mm\tz mm\tbox min x mm\ty mm\tz mm\tbox max x mm\ty mm\tz mm");
                    int code = 0;

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (!ReadOne(files[i], labels[i]))
                        {
                            code = 1;
                        }
                    }

                    Say("probe finished, code " + code);
                    return code;
                }
                catch (Exception error)
                {
                    Exception inner = error is TargetInvocationException && error.InnerException != null ? error.InnerException : error;
                    Say("THREW " + inner.GetType().Name + ": " + inner.Message);
                    Say(inner.StackTrace ?? string.Empty);
                    return 1;
                }
            }
        }

        private void Say(string line)
        {
            results.WriteLine(line);
        }

        private bool ReadOne(string file, string label)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return false;
            }

            Say("---- " + label + "  " + file);

            if (!File.Exists(file))
            {
                Say("UNKNOWN: the file is not there");
                return false;
            }

            bool opened = document.TryOpenFile(file);
            Say("TryOpenFile returned " + opened + ", models " + document.Models.Count + ", document units " + document.Units);

            if (!opened)
            {
                return false;
            }

            // An open returning true does not mean the models are in, F74. Wait until the count stops moving over three
            // readings a quarter second apart, and give up at thirty seconds, as ModelLoadWait reads it.
            int last = -1;
            int same = 0;
            DateTime until = DateTime.UtcNow.AddSeconds(30);

            while (DateTime.UtcNow < until)
            {
                int now = document.Models.Count;
                same = now == last && now > 0 ? same + 1 : 0;
                last = now;

                if (same >= 2)
                {
                    break;
                }

                Thread.Sleep(250);
            }

            Say("models settled at " + document.Models.Count);
            double perUnit = MillimetresPerUnit(document.Units);

            for (int m = 0; m < document.Models.Count; m++)
            {
                using (Model model = document.Models[m])
                {
                    Say(Row(label, file, m, model, document.Units, perUnit));
                }
            }

            return true;
        }

        private string Row(string label, string file, int index, Model model, Units documentUnits, double perUnit)
        {
            string fileName = Read(() => model.FileName);
            string source = Read(() => model.SourceFileName);
            string units = Read(() => model.Units.ToString());
            double tx = double.NaN, ty = double.NaN, tz = double.NaN;
            double minX = double.NaN, minY = double.NaN, minZ = double.NaN, maxX = double.NaN, maxY = double.NaN, maxZ = double.NaN;
            string note = string.Empty;

            try
            {
                Transform3D transform = model.Transform;

                if (transform != null)
                {
                    Vector3D t = transform.Translation;
                    tx = t.X;
                    ty = t.Y;
                    tz = t.Z;
                }
            }
            catch (Exception error)
            {
                note += " translation threw " + error.GetType().Name + ": " + error.Message;
            }

            try
            {
                using (ModelItem root = model.RootItem)
                {
                    BoundingBox3D box = root.BoundingBox();

                    if (box != null && box.Min != null && box.Max != null)
                    {
                        minX = box.Min.X; minY = box.Min.Y; minZ = box.Min.Z;
                        maxX = box.Max.X; maxY = box.Max.Y; maxZ = box.Max.Z;
                    }
                }
            }
            catch (Exception error)
            {
                note += " box threw " + error.GetType().Name + ": " + error.Message;
            }

            StringBuilder row = new StringBuilder("ROW\t");
            row.Append(label).Append('\t').Append(file).Append('\t').Append(index.ToString(Inv)).Append('\t')
               .Append(Path.GetFileName(fileName)).Append('\t').Append(source).Append('\t').Append(units).Append('\t')
               .Append(documentUnits.ToString());

            foreach (double v in new[] { tx, ty, tz, minX, minY, minZ, maxX, maxY, maxZ })
            {
                row.Append('\t').Append(N(v));
            }

            row.Append('\t').Append(N(perUnit));

            foreach (double v in new[] { tx, ty, tz, minX, minY, minZ, maxX, maxY, maxZ })
            {
                row.Append('\t').Append(N(v * perUnit));
            }

            if (note.Length > 0)
            {
                row.Append('\t').Append("NOTE").Append(note);
            }

            return row.ToString();
        }

        private double MillimetresPerUnit(Units units)
        {
            try
            {
                double millimetreInMetres = UnitConversion.ScaleFactor(Units.Millimeters, Units.Meters);

                if (!(millimetreInMetres < 1.0))
                {
                    Say("ScaleFactor(Millimeters, Meters) reads " + N(millimetreInMetres) + ", not below 1, so which way it converts is UNKNOWN and every mm value is UNKNOWN");
                    return double.NaN;
                }

                return UnitConversion.ScaleFactor(units, Units.Millimeters);
            }
            catch (Exception error)
            {
                Say("ScaleFactor threw " + error.GetType().Name + ": " + error.Message + ", so every mm value is UNKNOWN");
                return double.NaN;
            }
        }

        private static string Read(Func<string> read)
        {
            try
            {
                string value = read();
                return value ?? string.Empty;
            }
            catch (Exception error)
            {
                return Unknown + " " + error.GetType().Name;
            }
        }

        private static string N(double value)
        {
            return double.IsNaN(value) ? Unknown : value.ToString("0.######", Inv);
        }
    }
}
