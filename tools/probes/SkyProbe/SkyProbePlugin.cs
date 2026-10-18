using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.Plugins;
using Color = System.Drawing.Color;

namespace SkyProbe
{
    /// <summary>
    /// F215, F210-R3, Bader's message of 18 Oct 2026: the pictures of 1B06BC and 1B06P1 that show only the sky, probed on
    /// copies. Opens a copy of an NWF the tool built, which holds its tests and their results, and for the first clashes of
    /// each test named reads what Clash Detective gives the picture: the clash's centre, the camera of the clash's own
    /// viewpoint, TestsViewpointForResult, every readable property of it, and the distance from the camera to the centre.
    /// Then it takes the picture the way the tool does, TestsImageForResult, with every model shown and again with only the
    /// models of the clash's two items shown, the shape of F145, reads the camera the document holds after each, and
    /// measures each picture: the share of its pixels that differ from their row's most common colour and the colour at its
    /// centre, the sky being 136,169,238. Every picture is saved. The models' boxes are read first. Nothing is saved into
    /// the NWF and the hidden state is put back.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Sky probe", ToolTip = "F215, the pictures that show only the sky")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class SkyProbePlugin : AddInPlugin
    {
        public const string PluginName = "SkyProbe";
        public const string DeveloperCode = "PARS";

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private StreamWriter results;

        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 6 || parameters[0] != "sky")
            {
                return 2;
            }

            using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                Say("probe started, mode sky");

                try
                {
                    Measure(parameters[2], parameters[3].Split('|'), int.Parse(parameters[4], Inv), parameters[5]);
                }
                catch (Exception error)
                {
                    Exception inner = error is TargetInvocationException && error.InnerException != null ? error.InnerException : error;
                    Say("THREW " + inner.GetType().Name + ": " + inner.Message);
                    Say(inner.StackTrace ?? string.Empty);
                    return 1;
                }

                Say("probe finished");
            }

            return 0;
        }

        private void Say(string line)
        {
            results.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", Inv) + "  " + line);
        }

        private static string N(double value)
        {
            return value.ToString("0.###", Inv);
        }

        private static string P(Point3D point)
        {
            return point == null ? "none" : "(" + N(point.X) + ", " + N(point.Y) + ", " + N(point.Z) + ")";
        }

        private void Measure(string nwf, string[] tests, int count, string pictures)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;
            Say("opening " + Path.GetFileName(nwf));
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + ", models " + document.Models.Count + ", document units " + document.Units);

            if (!opened)
            {
                return;
            }

            Say("==== THE MODELS, each root's box in the document's units ====");

            for (int m = 0; m < document.Models.Count; m++)
            {
                using (Model model = document.Models[m])
                {
                    ModelItem root = model.RootItem;
                    BoundingBox3D box = root.BoundingBox();
                    Say("   model " + m + "  " + Path.GetFileName(model.FileName) + "  units " + model.Units + "  root '" + root.DisplayName
                        + "'  box " + P(box.Min) + " to " + P(box.Max) + "  hidden " + root.IsHidden);
                    Say("      its model, every readable property: " + Properties(model));
                }
            }

            int hiddenBefore = 0;

            foreach (ModelItem root in document.Models.RootItems)
            {
                if (root.IsHidden)
                {
                    hiddenBefore++;
                }
            }

            Say("roots hidden as the NWF opened: " + hiddenBefore + ", every item shown now by ResetAllHidden");
            document.Models.ResetAllHidden();
            Directory.CreateDirectory(pictures);
            DocumentClashTests clashTests = document.GetClash().TestsData;

            foreach (string name in tests)
            {
                ClashTest test = Find(clashTests.Tests, name);
                Say("==== TEST " + name + " " + (test == null ? "NOT FOUND" : "found, " + test.Children.Count + " results at its top level") + " ====");

                if (test != null)
                {
                    MeasureTest(document, clashTests, test, count, pictures);
                }
            }

            document.Models.ResetAllHidden();
            Say("every item shown again, as the probe leaves it, the NWF not saved");
        }

        private static ClashTest Find(SavedItemCollection items, string name)
        {
            foreach (SavedItem item in items)
            {
                ClashTest test = item as ClashTest;

                if (test != null && test.DisplayName == name)
                {
                    return test;
                }

                GroupItem group = item as GroupItem;

                if (group != null)
                {
                    ClashTest inner = Find(group.Children, name);

                    if (inner != null)
                    {
                        return inner;
                    }
                }
            }

            return null;
        }

        private void MeasureTest(Document document, DocumentClashTests clashTests, ClashTest test, int count, string pictures)
        {
            List<ClashResult> clashes = new List<ClashResult>();

            foreach (SavedItem item in test.Children)
            {
                ClashResult result = item as ClashResult;

                if (result != null)
                {
                    clashes.Add(result);
                }

                ClashResultGroup group = item as ClashResultGroup;

                if (group != null)
                {
                    foreach (SavedItem inner in group.Children)
                    {
                        ClashResult member = inner as ClashResult;

                        if (member != null)
                        {
                            clashes.Add(member);
                        }
                    }
                }

                if (clashes.Count >= count)
                {
                    break;
                }
            }

            for (int i = 0; i < clashes.Count && i < count; i++)
            {
                ClashResult clash = clashes[i];
                string tag = Safe(test.DisplayName) + "-" + (i + 1).ToString("000", Inv);
                Point3D centre = clash.Center;
                Say("-- clash " + (i + 1) + "  '" + clash.DisplayName + "'  status " + clash.Status + "  distance " + N(clash.Distance)
                    + "  centre " + P(centre));
                Say("     item 1 in " + ModelOf(document, clash.Item1) + ", item 2 in " + ModelOf(document, clash.Item2));

                using (Viewpoint own = clashTests.TestsViewpointForResult(clash))
                {
                    Say("     its viewpoint: " + Camera(own) + "  camera to centre " + Gap(own, centre));
                    Say("     its viewpoint, every readable property: " + Properties(own));
                    Say("     its clip planes, every readable property: " + (own.ClipPlanes == null ? "none" : Properties(own.ClipPlanes)));
                }

                Picture(document, clashTests, clash, centre, Path.Combine(pictures, tag + "-all.jpg"), "every model shown");

                List<ModelItem> others = OtherRoots(document, clash);
                document.Models.SetHidden(others, true);
                Picture(document, clashTests, clash, centre, Path.Combine(pictures, tag + "-own.jpg"), "only its items' models shown, " + others.Count + " roots hidden");
                document.Models.ResetAllHidden();
                BackedOff(document, clashTests, clash, centre, pictures, tag);
            }
        }

        /// <summary>
        /// The same view from the clash's own camera and from the camera moved back along the line from the centre through
        /// it to 30, 100 and 300 feet, every model shown, each set on the active view by CopyViewpointFrom and rendered by
        /// the active view's GenerateImage, so whether a camera close to its clash is what leaves the picture empty is read
        /// off the pictures.
        /// </summary>
        private void BackedOff(Document document, DocumentClashTests clashTests, ClashResult clash, Point3D centre, string pictures, string tag)
        {
            View active = document.ActiveView;

            using (Viewpoint own = clashTests.TestsViewpointForResult(clash))
            {
                Point3D at = own.Position;
                double dx = at.X - centre.X;
                double dy = at.Y - centre.Y;
                double dz = at.Z - centre.Z;
                double length = Math.Sqrt(dx * dx + dy * dy + dz * dz);

                foreach (double feet in new[] { length, 30.0, 100.0, 300.0 })
                {
                    using (Viewpoint moved = own.CreateCopy())
                    {
                        if (length > 0)
                        {
                            double k = feet / length;
                            moved.Position = new Point3D(centre.X + dx * k, centre.Y + dy * k, centre.Z + dz * k);
                        }

                        active.CopyViewpointFrom(moved, ViewChange.JumpCut);
                    }

                    string path = Path.Combine(pictures, tag + "-back-" + ((int)Math.Round(feet)).ToString(Inv) + ".jpg");

                    using (Bitmap bitmap = active.GenerateImage(ImageGenerationStyle.ScenePlusOverlay, 512, 512, false))
                    {
                        bitmap.Save(path, ImageFormat.Jpeg);
                        Color middle = bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2);
                        Say("     active view from " + N(feet) + " ft back: " + N(Differs(bitmap)) + " percent differs from its row, colour at the middle "
                            + middle.R + "," + middle.G + "," + middle.B + ", " + Path.GetFileName(path));
                    }
                }
            }
        }

        private void Picture(Document document, DocumentClashTests clashTests, ClashResult clash, Point3D centre, string path, string how)
        {
            using (Bitmap bitmap = clashTests.TestsImageForResult(clash, ImageGenerationStyle.ScenePlusOverlay, 512, 512))
            {
                if (bitmap == null)
                {
                    Say("     picture, " + how + ": none returned");
                    return;
                }

                bitmap.Save(path, ImageFormat.Jpeg);
                Color middle = bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 3);
                Say("     picture, " + how + ": " + N(Differs(bitmap)) + " percent differs from its row, centre colour "
                    + middle.R + "," + middle.G + "," + middle.B + ", " + Path.GetFileName(path));
            }

            using (Viewpoint now = document.CurrentViewpoint.CreateCopy())
            {
                Say("     the document's camera after it: " + Camera(now) + "  camera to centre " + Gap(now, centre));
            }
        }

        private static string Camera(Viewpoint viewpoint)
        {
            return "position " + P(viewpoint.Position) + "  focal " + (viewpoint.HasFocalDistance ? N(viewpoint.FocalDistance) : "none")
                + "  projection " + viewpoint.Projection;
        }

        private static string Gap(Viewpoint viewpoint, Point3D centre)
        {
            if (viewpoint.Position == null || centre == null)
            {
                return "UNKNOWN";
            }

            double dx = viewpoint.Position.X - centre.X;
            double dy = viewpoint.Position.Y - centre.Y;
            double dz = viewpoint.Position.Z - centre.Z;
            return N(Math.Sqrt(dx * dx + dy * dy + dz * dz));
        }

        private static string Properties(object target)
        {
            List<string> parts = new List<string>();

            foreach (PropertyInfo property in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                try
                {
                    object value = property.GetValue(target, null);
                    Point3D point = value as Point3D;
                    parts.Add(property.Name + "=" + (point != null ? P(point) : value == null ? "null" : value.ToString()));
                }
                catch (Exception error)
                {
                    Exception inner = error is TargetInvocationException && error.InnerException != null ? error.InnerException : error;
                    parts.Add(property.Name + " threw " + inner.GetType().Name);
                }
            }

            return string.Join("; ", parts.ToArray());
        }

        /// <summary>The file name of the model an item lives in, read off its topmost ancestor's display name.</summary>
        private static string ModelOf(Document document, ModelItem item)
        {
            if (item == null)
            {
                return "no item";
            }

            ModelItem top = item;

            while (top.Parent != null)
            {
                top = top.Parent;
            }

            return "'" + top.DisplayName + "'";
        }

        /// <summary>Every model root but the roots of the clash's two items, matched by display name.</summary>
        private static List<ModelItem> OtherRoots(Document document, ClashResult clash)
        {
            string a = ModelOf(document, clash.Item1);
            string b = ModelOf(document, clash.Item2);
            List<ModelItem> others = new List<ModelItem>();

            foreach (ModelItem root in document.Models.RootItems)
            {
                string name = "'" + root.DisplayName + "'";

                if (name != a && name != b)
                {
                    others.Add(root);
                }
            }

            return others;
        }

        /// <summary>The percent of pixels, every 4th of every 4th row, that differ by more than 24 in a channel from their row's most common colour.</summary>
        private static double Differs(Bitmap bitmap)
        {
            int differ = 0;
            int all = 0;

            for (int y = 0; y < bitmap.Height; y += 4)
            {
                Dictionary<int, int> counts = new Dictionary<int, int>();
                List<Color> row = new List<Color>();

                for (int x = 0; x < bitmap.Width; x += 4)
                {
                    Color c = bitmap.GetPixel(x, y);
                    row.Add(c);
                    int key = c.ToArgb();
                    int n;
                    counts.TryGetValue(key, out n);
                    counts[key] = n + 1;
                }

                int mode = 0;
                int best = -1;

                foreach (KeyValuePair<int, int> pair in counts)
                {
                    if (pair.Value > best)
                    {
                        best = pair.Value;
                        mode = pair.Key;
                    }
                }

                Color m = Color.FromArgb(mode);

                foreach (Color c in row)
                {
                    all++;

                    if (Math.Abs(c.R - m.R) > 24 || Math.Abs(c.G - m.G) > 24 || Math.Abs(c.B - m.B) > 24)
                    {
                        differ++;
                    }
                }
            }

            return all == 0 ? 0 : 100.0 * differ / all;
        }

        private static string Safe(string name)
        {
            StringBuilder safe = new StringBuilder();

            foreach (char c in name)
            {
                safe.Append(char.IsLetterOrDigit(c) ? c : '_');
            }

            return safe.Length > 40 ? safe.ToString(0, 40) : safe.ToString();
        }
    }
}
