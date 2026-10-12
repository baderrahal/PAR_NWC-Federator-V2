using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Addin.Engine;
using Federator.Core.Views;

namespace PictureSpeedProbe
{
    /// <summary>
    /// Mode genimage, Bader's item 4 of his answer to Q150 on 2026-10-11, measured on 2026-10-12 once
    /// Navisworks' hardware acceleration was on. Two questions on the same first rows of one test.
    ///
    /// WHAT TestsImageForResult CHANGES IN THE DOCUMENT. The installed SDK reference is not there, no
    /// api folder in the install and no Autodesk.Navisworks.Clash.xml beside the DLL, so it is read
    /// off the document around the call: the current viewpoint's position and the next undo step's
    /// name before and after, setting 6, and the seconds of the call alone.
    ///
    /// HOW FAST A PICTURE OF ONLY THE CLASH'S TWO ITEMS IS THROUGH View.GenerateImage, settings 7 and
    /// 8: per row every item but the two clashing ones hidden, at each level below a model root on the
    /// two items' paths every child off them, the two painted red and green by the views' own
    /// PaintMany and colours, the camera the clash's own, TestsViewpointForResult set on the active
    /// view by a jump cut, then View.GenerateImage with ScenePlusOverlay at 1024 by 1024, each part
    /// timed alone, and the paint and the hiding taken off after each picture. Setting 9 is the
    /// tool's call again in the same session. The hidden state and the viewpoint are put back at the
    /// end and nothing is saved.
    /// </summary>
    public sealed partial class PictureSpeedProbePlugin
    {
        private void GenerateImageStudy(
            Document document, DocumentClashTests clashTests, TestAddress address, string testName, int count, string picturesRoot, int projectTo)
        {
            ImageCodecInfo jpeg = null;

            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    jpeg = codec;
                }
            }

            Directory.CreateDirectory(picturesRoot);
            string csv = Path.Combine(picturesRoot, "times.csv");
            File.WriteAllText(csv, "setting,row,clash,hide_s,camera_s,paint_s,render_s,save_s,restore_s,items_hidden,bytes\r\n", new UTF8Encoding(false));
            List<string> summary = new List<string>();

            using (HiddenSnapshot snapshot = SavedViewpoints.SnapshotHidden(document))
            using (Viewpoint original = document.CurrentViewpoint.CreateCopy())
            {
                try
                {
                    summary.Add(Watched(document, clashTests, address, testName, Math.Min(5, count)));
                    summary.Add(TwoItems("7-generate-two-items", document, clashTests, address, testName, count, picturesRoot, jpeg, csv, projectTo));
                    summary.Add(TwoItems("8-generate-two-items-again", document, clashTests, address, testName, count, picturesRoot, jpeg, csv, projectTo));
                    document.CurrentViewpoint.CopyFrom(original);
                    summary.Add(ToolsCallAgain(document, clashTests, address, testName, count, picturesRoot, jpeg, csv, projectTo));
                }
                finally
                {
                    SavedViewpoints.Undim(document);
                    document.CurrentViewpoint.CopyFrom(original);
                    bool back = SavedViewpoints.RestoreHiddenState(document, snapshot);
                    Say("PUT BACK  the paint taken off, the viewpoint the document opened with, and the hidden state read back as before: " + back);
                }
            }

            Say(string.Empty);
            Say("==== SUMMARY, mode genimage ====");

            foreach (string line in summary)
            {
                Say(line);
            }
        }

        /// <summary>Setting 6: the tool's call, and what it changes in the document around it.</summary>
        private string Watched(Document document, DocumentClashTests clashTests, TestAddress address, string testName, int count)
        {
            Say("==== SETTING 6-tests-image-watched, TestsImageForResult as the tool calls it, the document read before and after ====");
            List<double> renders = new List<double>();
            string nowNamed;

            using (ClashTest test = address.ResolveIn(clashTests, testName, out nowNamed))
            {
                SavedItemCollection children = test.Children;

                for (int i = 0; i < count; i++)
                {
                    using (SavedItem item = children[i])
                    {
                        IClashResult result = item as IClashResult;

                        if (result == null)
                        {
                            continue;
                        }

                        string before = State(document);
                        Stopwatch clock = Stopwatch.StartNew();

                        using (Bitmap bitmap = clashTests.TestsImageForResult(result, ImageGenerationStyle.ScenePlusOverlay, 1024, 1024))
                        {
                            renders.Add(clock.Elapsed.TotalSeconds);
                        }

                        string after = State(document);
                        Say((i + 1).ToString("00") + "  " + item.DisplayName + "  render " + S(renders[renders.Count - 1]) + " s, before: " + before
                            + ", after: " + after + (before == after ? ", nothing read changed" : ", CHANGED"));
                    }
                }
            }

            return "6-tests-image-watched  " + renders.Count + " pictures, render mean " + (renders.Count == 0 ? "UNKNOWN" : S(Mean(renders)) + " s");
        }

        /// <summary>The camera's position and the next undo step's name, as read.</summary>
        private static string State(Document document)
        {
            string position;

            try
            {
                using (Viewpoint now = document.CurrentViewpoint.CreateCopy())
                using (Point3D at = now.Position)
                {
                    position = at.X.ToString("0.###", Inv) + " " + at.Y.ToString("0.###", Inv) + " " + at.Z.ToString("0.###", Inv);
                }
            }
            catch (Exception error)
            {
                position = "UNKNOWN, " + error.GetType().Name;
            }

            string undo;

            try
            {
                undo = document.NextUndo ?? "none";
            }
            catch (Exception error)
            {
                undo = "UNKNOWN, " + error.GetType().Name;
            }

            return "camera at " + position + ", next undo " + (undo.Length == 0 ? "none" : undo);
        }

        /// <summary>Settings 7 and 8: only the clash's two items, the clash's own camera, View.GenerateImage.</summary>
        private string TwoItems(
            string setting, Document document, DocumentClashTests clashTests, TestAddress address, string testName, int count,
            string picturesRoot, ImageCodecInfo jpeg, string csv, int projectTo)
        {
            Say("==== SETTING " + setting + ", only the clash's two items shown, painted red and green, its own camera, View.GenerateImage 1024 by 1024 ====");
            string folder = Path.Combine(picturesRoot, setting);
            Directory.CreateDirectory(folder);
            List<double> totals = new List<double>();
            List<double> renders = new List<double>();
            string nowNamed;

            using (ClashTest test = address.ResolveIn(clashTests, testName, out nowNamed))
            {
                SavedItemCollection children = test.Children;

                for (int i = 0; i < count; i++)
                {
                    using (SavedItem item = children[i])
                    {
                        ClashResult result = item as ClashResult;
                        string row = (i + 1).ToString("00");

                        if (result == null)
                        {
                            Say(row + "  " + item.DisplayName + "  UNKNOWN, not a single clash, skipped");
                            continue;
                        }

                        try
                        {
                            double hide, camera, paint, render, save, restore;
                            int hidden;
                            long bytes;
                            Stopwatch clock = Stopwatch.StartNew();

                            using (ModelItem first = result.Item1)
                            using (ModelItem second = result.Item2)
                            {
                                hidden = ShowOnly(document, first, second);
                                hide = Lap(clock);

                                // The active view is the document's own and is never disposed here.
                                View active = document.ActiveView;

                                if (active == null)
                                {
                                    throw new InvalidOperationException("the document has no active view");
                                }

                                using (Viewpoint own = clashTests.TestsViewpointForResult(result))
                                {
                                    if (own == null)
                                    {
                                        throw new InvalidOperationException("TestsViewpointForResult gave no viewpoint");
                                    }

                                    active.CopyViewpointFrom(own, ViewChange.JumpCut);
                                }

                                camera = Lap(clock);

                                using (ModelItemCollection red = new ModelItemCollection())
                                using (ModelItemCollection green = new ModelItemCollection())
                                {
                                    red.Add(first);
                                    green.Add(second);
                                    SavedViewpoints.PaintMany(document, red, ViewpointColour.DefaultFirst());
                                    SavedViewpoints.PaintMany(document, green, ViewpointColour.DefaultSecond());
                                }

                                paint = Lap(clock);
                            }

                            string path = Path.Combine(folder, row + ".jpg");

                            using (Bitmap bitmap = document.ActiveView.GenerateImage(ImageGenerationStyle.ScenePlusOverlay, 1024, 1024, false))
                            {
                                render = Lap(clock);
                                bitmap.Save(path, jpeg, null);
                                save = Lap(clock);
                            }

                            SavedViewpoints.Undim(document);
                            document.Models.ResetAllHidden();
                            restore = Lap(clock);
                            bytes = new FileInfo(path).Length;
                            double total = hide + camera + paint + render + save + restore;
                            totals.Add(total);
                            renders.Add(render);
                            Say(row + "  " + item.DisplayName + "  hide " + S(hide) + " s (" + hidden + " items), camera " + S(camera) + " s, paint " + S(paint)
                                + " s, render " + S(render) + " s, save " + S(save) + " s, restore " + S(restore) + " s, all " + S(total) + " s, " + bytes + " bytes");
                            File.AppendAllText(csv, setting + "," + row + "," + item.DisplayName + "," + S(hide) + "," + S(camera) + "," + S(paint) + "," + S(render)
                                + "," + S(save) + "," + S(restore) + "," + hidden + "," + bytes + "\r\n", new UTF8Encoding(false));
                        }
                        catch (Exception error)
                        {
                            Say(row + "  " + item.DisplayName + "  THREW " + error.GetType().Name + ": " + error.Message + ", the paint and the hiding are taken off and the next row is tried");

                            if (i == 0)
                            {
                                Say(error.StackTrace ?? "no stack trace");
                            }
                            SavedViewpoints.Undim(document);
                            document.Models.ResetAllHidden();
                        }
                    }
                }
            }

            if (totals.Count == 0)
            {
                return setting + "  no picture was written";
            }

            double mean = Mean(totals);
            return setting + "  " + totals.Count + " pictures, render mean " + S(Mean(renders)) + " s, median " + S(Median(renders))
                + " s, every part mean " + S(mean) + " s, for " + projectTo + " pictures " + (mean * projectTo).ToString("0", Inv) + " s";
        }

        /// <summary>Setting 9: the tool's own call again in the same session, every model shown.</summary>
        private string ToolsCallAgain(
            Document document, DocumentClashTests clashTests, TestAddress address, string testName, int count, string picturesRoot, ImageCodecInfo jpeg, string csv, int projectTo)
        {
            Say("==== SETTING 9-tests-image-again, TestsImageForResult as the tool calls it, every model shown, 1024 by 1024 ====");
            string folder = Path.Combine(picturesRoot, "9-tests-image-again");
            Directory.CreateDirectory(folder);
            List<double> totals = new List<double>();
            string nowNamed;

            using (ClashTest test = address.ResolveIn(clashTests, testName, out nowNamed))
            {
                SavedItemCollection children = test.Children;

                for (int i = 0; i < count; i++)
                {
                    using (SavedItem item = children[i])
                    {
                        IClashResult result = item as IClashResult;

                        if (result == null)
                        {
                            continue;
                        }

                        string path = Path.Combine(folder, (i + 1).ToString("00") + ".jpg");
                        Stopwatch clock = Stopwatch.StartNew();

                        using (Bitmap bitmap = clashTests.TestsImageForResult(result, ImageGenerationStyle.ScenePlusOverlay, 1024, 1024))
                        {
                            double render = Lap(clock);
                            bitmap.Save(path, jpeg, null);
                            double save = Lap(clock);
                            totals.Add(render + save);
                            File.AppendAllText(csv, "9-tests-image-again," + (i + 1).ToString("00") + "," + item.DisplayName + ",0,0,0," + S(render) + "," + S(save)
                                + ",0,0," + new FileInfo(path).Length + "\r\n", new UTF8Encoding(false));
                        }
                    }
                }
            }

            return "9-tests-image-again  " + totals.Count + " pictures, render and save mean " + (totals.Count == 0 ? "UNKNOWN" : S(Mean(totals)) + " s, for "
                + projectTo + " pictures " + (Mean(totals) * projectTo).ToString("0", Inv) + " s");
        }

        /// <summary>
        /// Every item hidden but the two and what lies on their paths: every model root off both paths,
        /// and at every level below a root on a path every child off both paths. The paths are the index
        /// paths CreateIndexPath gives, a model's index then a child's index at each level. Returns how many
        /// items were hidden, or nought where a path could not be read, nothing then hidden.
        /// </summary>
        private int ShowOnly(Document document, ModelItem first, ModelItem second)
        {
            int[] a = SavedViewpoints.PathOf(document, first);
            int[] b = SavedViewpoints.PathOf(document, second);

            if (a == null || b == null)
            {
                return 0;
            }

            HashSet<string> onPath = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> ends = new HashSet<string>(StringComparer.Ordinal) { Key(a, a.Length), Key(b, b.Length) };

            foreach (int[] p in new[] { a, b })
            {
                for (int n = 1; n <= p.Length; n++)
                {
                    onPath.Add(Key(p, n));
                }
            }

            using (ModelItemCollection hide = new ModelItemCollection())
            {
                for (int m = 0; m < document.Models.Count; m++)
                {
                    if (onPath.Contains(m.ToString(Inv)))
                    {
                        continue;
                    }

                    using (Model model = document.Models[m])
                    using (ModelItem root = model.RootItem)
                    {
                        hide.Add(root);
                    }
                }

                foreach (string prefix in onPath)
                {
                    if (ends.Contains(prefix))
                    {
                        continue;
                    }

                    using (ModelItem node = SavedViewpoints.ItemAt(document, Parse(prefix)))
                    {
                        if (node == null)
                        {
                            continue;
                        }

                        int index = 0;

                        foreach (ModelItem child in node.Children)
                        {
                            if (!onPath.Contains(prefix + "/" + index.ToString(Inv)))
                            {
                                hide.Add(child);
                            }

                            index++;
                        }
                    }
                }

                if (hide.Count > 0)
                {
                    document.Models.SetHidden(hide, true);
                }

                return hide.Count;
            }
        }

        private static string Key(int[] path, int length)
        {
            StringBuilder key = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                if (i > 0)
                {
                    key.Append('/');
                }

                key.Append(path[i].ToString(Inv));
            }

            return key.ToString();
        }

        private static int[] Parse(string key)
        {
            string[] parts = key.Split('/');
            int[] path = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                path[i] = int.Parse(parts[i], Inv);
            }

            return path;
        }

        private static double Lap(Stopwatch clock)
        {
            double seconds = clock.Elapsed.TotalSeconds;
            clock.Restart();
            return seconds;
        }
    }
}
