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
using Autodesk.Navisworks.Api.DocumentParts;
using Autodesk.Navisworks.Api.Plugins;
using Federator.Addin.Engine;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Views;

namespace Federator.Addin
{
    /// <summary>
    /// NavisworksFacts names this type for the build stamp. It is the add-in's plugin class in
    /// src, and here it is an empty stand-in, so the probe carries no second plugin under the
    /// add-in's name, as in Q133Import.
    /// </summary>
    internal sealed class FederatorPlugin
    {
    }
}

namespace PictureSpeedProbe
{
    /// <summary>
    /// Q150 on 1A04PK, 2026-10-11, Bader's answer NOT YET A CHOICE. Set 08's three test cut
    /// measured 830.7 s of a group of 884.7 s inside TestsImageForResult, 8.0 s a picture. Bader
    /// asked what makes that call faster, on the same first clashes of one test, each against
    /// today's: 1, only the models of the test shown, every other model hidden, 2, shaded drawing
    /// instead of full rendering on top of 1, 3, a picture of 800 by 600 on top of 1, and 4, all
    /// three. The test is created and run the way the add-in does it, by ClashRunner's
    /// PlanTheCreation and Create called on a real ClashRunner, as Q133Import does. Then the first
    /// rows of the test, in Clash Detective's order, are rendered under each setting in turn,
    /// today's first and today's again last so the order's own effect is seen, each render and
    /// each JPEG save timed alone, each picture saved in a folder of its own setting. The
    /// models of the test are the models the items of its two selection sets live in. Shaded is
    /// the current viewpoint's RenderStyle, measured settable off the DLL on 2026-10-11, and
    /// whether TestsImageForResult obeys it is read off the times and the pictures. The hidden
    /// state and the viewpoint are put back at the end and nothing is saved.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Picture speed probe", ToolTip = "Q150, what makes a clash picture faster")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed partial class PictureSpeedProbePlugin : AddInPlugin
    {
        public const string PluginName = "PictureSpeedProbe";
        public const string DeveloperCode = "PARS";

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private StreamWriter results;

        /// <summary>picspeed, the four ways of Q150, or genimage, Bader's item 4 of his answer to Q150.</summary>
        private string mode;

        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 9 || (parameters[0] != "picspeed" && parameters[0] != "genimage" && parameters[0] != "hideapi"))
            {
                return 2;
            }

            using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                mode = parameters[0];
                Say("probe started, mode " + parameters[0]);

                try
                {
                    int count = int.Parse(parameters[5], Inv);
                    int projectTo = int.Parse(parameters[8], Inv);
                    Measure(parameters[2], parameters[3], parameters[4], count, parameters[6], parameters[7], projectTo);
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
            results.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "  " + line);
        }

        private static string S(double seconds)
        {
            return seconds.ToString("0.000", Inv);
        }

        private void Measure(string nwf, string xml, string testName, int count, string picturesRoot, string logFolder, int projectTo)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + S(clock.Elapsed.TotalSeconds) + " s");

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count + ", document units " + document.Units);

            for (int m = 0; m < document.Models.Count; m++)
            {
                using (Model model = document.Models[m])
                {
                    Say("   model " + m + "  " + Path.GetFileName(model.FileName)
                        + "  under the loop folder " + (model.FileName ?? string.Empty).StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase));
                }
            }

            DocumentClashTests clashTests = document.GetClash().TestsData;
            Say("clash tests at the root " + clashTests.Tests.Count);

            using (RunLog log = RunLog.Start(logFolder, DateTime.Now, 5))
            {
                MeasureWithLog(document, clashTests, xml, testName, count, picturesRoot, log, projectTo);
            }
        }

        private void MeasureWithLog(
            Document document, DocumentClashTests clashTests, string xml, string testName, int count, string picturesRoot, RunLog log, int projectTo)
        {
            Say("==== THE TEST, made by the add-in's own code ====");
            ExchangeDocument exchange = MatrixCorrections.ReadPicked(xml);
            Say("MatrixCorrections.ReadPicked: sets in the file " + exchange.Sets.Count + ", tests in the file " + exchange.Tests.Count);

            string units = ClashRunner.DocumentUnits();
            ClashTestPlan plan = ClashTestPlan.From(exchange, units);
            ClashRunner runner = new ClashRunner(delegate { }, log, new RepeatedFailureGuard());
            Type rt = typeof(ClashRunner);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo unitsField = rt.GetField("documentUnits", hidden);
            MethodInfo planTheCreation = rt.GetMethod("PlanTheCreation", hidden);
            MethodInfo create = rt.GetMethod("Create", hidden, null,
                new[] { typeof(DocumentSelectionSets), typeof(DocumentClashTests), typeof(Dictionary<string, SelectionSet>), typeof(PlannedClashTest) }, null);
            MethodInfo release = rt.GetMethod("ReleaseTheIndex", hidden);

            if (unitsField == null || planTheCreation == null || create == null || release == null)
            {
                Say("UNKNOWN: ClashRunner does not carry documentUnits, PlanTheCreation, Create and ReleaseTheIndex as read, nothing is created");
                return;
            }

            unitsField.SetValue(runner, ClashRunner.UnitName(document.Units));
            DocumentSelectionSets docSets = document.SelectionSets;
            Dictionary<string, SelectionSet> byPath = runner.IndexSets(docSets);
            TestAddress address = null;
            HashSet<int> keep = new HashSet<int>();

            try
            {
                ClashTestPlan resolved = plan.ResolveAgainst(byPath.Keys);
                Dictionary<string, TestAddress> present = runner.IndexTests(clashTests);
                ClashRunOutcome outcome = new ClashRunOutcome();
                outcome.TestsInFile = plan.TestsInFile;
                IList<PlannedClashTest> toRun = (IList<PlannedClashTest>)planTheCreation.Invoke(runner, new object[] { document, byPath, present, resolved, outcome });
                Say("sets indexed " + byPath.Count + ", tests already in the document " + present.Count + ", the tool would create " + toRun.Count);

                PlannedClashTest planned = null;

                foreach (PlannedClashTest p in toRun)
                {
                    if (p.Name == testName)
                    {
                        planned = p;
                        break;
                    }
                }

                if (planned == null)
                {
                    Say("UNKNOWN: the tool would not create a test named " + testName + " in this document, nothing is measured");
                    return;
                }

                Say("the planned test: " + planned.Name + ", " + planned.TestTypeName + ", tolerance " + planned.Tolerance.ToString("0.##########", Inv)
                    + " " + planned.DocumentUnits + ", merge composites " + planned.MergeComposites
                    + ", left " + planned.Left.Locator + ", right " + planned.Right.Locator);

                keep = ModelsOf(document, byPath, planned);

                System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
                address = (TestAddress)create.Invoke(runner, new object[] { docSets, clashTests, byPath, planned });
                Say("the tool's Create took " + S(clock.Elapsed.TotalSeconds) + " s, address " + (address == null ? "none" : "read"));
            }
            finally
            {
                release.Invoke(runner, new object[] { byPath });
            }

            if (address == null)
            {
                Say("UNKNOWN: no test was created, nothing is measured");
                return;
            }

            string nowNamed;

            using (ClashTest test = address.ResolveIn(clashTests, testName, out nowNamed))
            {
                if (test == null)
                {
                    Say("UNKNOWN: the created test does not resolve at its address, now named " + (nowNamed ?? "nothing"));
                    return;
                }

                System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
                clashTests.TestsRunTest(test);
                Say("TestsRunTest took " + S(clock.Elapsed.TotalSeconds) + " s");
            }

            int rows;
            int leaves;

            using (ClashTest test = address.ResolveIn(clashTests, testName, out nowNamed))
            {
                rows = test.Children.Count;
                leaves = Leaves(test.Children);
            }

            Say("the test holds " + rows + " row(s) at its top and " + leaves + " clash(es) in all");
            Say(string.Empty);

            if (rows == 0)
            {
                Say("UNKNOWN: the test found no clash, nothing is measured");
                return;
            }

            if (mode == "hideapi")
            {
                HideOtherApiStudy(clashTests, address, testName, Math.Min(count, rows), picturesRoot);
                return;
            }

            if (mode == "genimage")
            {
                GenerateImageStudy(document, clashTests, address, testName, Math.Min(count, rows), picturesRoot, projectTo);
                return;
            }

            Pictures(document, clashTests, address, testName, Math.Min(count, rows), picturesRoot, keep, projectTo);
        }

        /// <summary>
        /// The index of every model an item of the test's two sets lives in, read by walking each
        /// item up to its top and taking that top's model by its file name, as ViewpointBuilder's
        /// home walk does.
        /// </summary>
        private HashSet<int> ModelsOf(Document document, Dictionary<string, SelectionSet> byPath, PlannedClashTest planned)
        {
            Dictionary<string, int> indexOf = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int m = 0; m < document.Models.Count; m++)
            {
                using (Model model = document.Models[m])
                {
                    indexOf[model.FileName ?? string.Empty] = m;
                }
            }

            HashSet<int> keep = new HashSet<int>();

            foreach (string locator in new[] { planned.Left.Locator, planned.Right.Locator })
            {
                SelectionSet set;

                if (locator == null || !byPath.TryGetValue(locator, out set))
                {
                    Say("UNKNOWN: no set indexed at " + (locator ?? "nothing"));
                    continue;
                }

                int items = 0;

                using (ModelItemCollection found = set.GetSelectedItems(document))
                {
                    foreach (ModelItem item in found)
                    {
                        items++;
                        string home = HomeOf(item);
                        int at;

                        if (home != null && indexOf.TryGetValue(home, out at))
                        {
                            keep.Add(at);
                        }
                    }
                }

                Say("the set " + locator + " finds " + items + " item(s)");
            }

            List<string> names = new List<string>();

            foreach (int m in keep)
            {
                using (Model model = document.Models[m])
                {
                    names.Add(m + " " + Path.GetFileName(model.FileName));
                }
            }

            Say("the models of the test, " + keep.Count + " of " + document.Models.Count + ": " + string.Join(", ", names.ToArray()));
            return keep;
        }

        private static string HomeOf(ModelItem item)
        {
            List<ModelItem> chain = new List<ModelItem>();
            ModelItem walker = item;

            try
            {
                while (chain.Count < 64)
                {
                    ModelItem parent = walker.Parent;

                    if (parent == null)
                    {
                        break;
                    }

                    chain.Add(parent);
                    walker = parent;
                }

                if (!walker.HasModel)
                {
                    return null;
                }

                using (Model model = walker.Model)
                {
                    return model == null ? null : model.FileName;
                }
            }
            finally
            {
                foreach (ModelItem walked in chain)
                {
                    walked.Dispose();
                }
            }
        }

        private static int Leaves(SavedItemCollection children)
        {
            int n = 0;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    ClashResultGroup group = child as ClashResultGroup;
                    n += group != null ? Leaves(group.Children) : (child is ClashResult ? 1 : 0);
                }
            }

            return n;
        }

        private sealed class Setting
        {
            internal Setting(string folder, string words, bool onlyTheTestsModels, ViewpointRenderStyle? style, int width, int height)
            {
                Folder = folder;
                Words = words;
                OnlyTheTestsModels = onlyTheTestsModels;
                Style = style;
                Width = width;
                Height = height;
            }

            internal string Folder { get; private set; }

            internal string Words { get; private set; }

            internal bool OnlyTheTestsModels { get; private set; }

            internal ViewpointRenderStyle? Style { get; private set; }

            internal int Width { get; private set; }

            internal int Height { get; private set; }
        }

        private void Pictures(
            Document document, DocumentClashTests clashTests, TestAddress address, string testName, int count,
            string picturesRoot, HashSet<int> keep, int projectTo)
        {
            ImageCodecInfo jpeg = null;

            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.MimeType == "image/jpeg")
                {
                    jpeg = codec;
                }
            }

            if (jpeg == null)
            {
                Say("UNKNOWN: no JPEG encoder on this machine");
                return;
            }

            ViewpointRenderStyle styleBefore;
            ViewpointLighting lightingBefore;

            using (Viewpoint now = document.CurrentViewpoint.CreateCopy())
            {
                styleBefore = now.RenderStyle;
                lightingBefore = now.Lighting;
            }

            Say("the current viewpoint before: render style " + styleBefore + ", lighting " + lightingBefore);

            // Q150's gap, Bader's order of 2026-10-12: the 3D view's own size in pixels, read off the
            // active view, since a picture that followed the window would follow this.
            try
            {
                View active = document.ActiveView;
                Say("the active view: " + (active == null ? "none" : active.Width + " by " + active.Height + " pixels, interactive " + active.IsInteractive));
            }
            catch (Exception error)
            {
                Say("the active view could not be read, " + error.GetType().Name + ": " + error.Message + ", the probe goes on");
            }

            List<Setting> settings = new List<Setting>
            {
                new Setting("0-today", "today's, every model shown, the render style the document opened with, 1024 by 1024", false, null, 1024, 1024),
                new Setting("1-models", "only the models of the test shown, 1024 by 1024", true, null, 1024, 1024),
                new Setting("2-shaded", "only the models of the test shown, Shaded, 1024 by 1024", true, ViewpointRenderStyle.Shaded, 1024, 1024),
                new Setting("3-800x600", "only the models of the test shown, 800 by 600", true, null, 800, 600),
                new Setting("4-all-three", "only the models of the test shown, Shaded, 800 by 600", true, ViewpointRenderStyle.Shaded, 800, 600),
                new Setting("5-today-again", "today's again, every model shown, the render style the document opened with, 1024 by 1024", false, null, 1024, 1024),
            };

            string csv = Path.Combine(picturesRoot, "times.csv");
            Directory.CreateDirectory(picturesRoot);
            File.WriteAllText(csv, "setting,row,clash,render_s,save_s,bytes,width,height\r\n", new UTF8Encoding(false));
            List<string> summary = new List<string>();

            using (HiddenSnapshot snapshot = SavedViewpoints.SnapshotHidden(document))
            using (Viewpoint original = document.CurrentViewpoint.CreateCopy())
            {
                try
                {
                    foreach (Setting setting in settings)
                    {
                        summary.Add(OneSetting(document, clashTests, address, testName, count, picturesRoot, keep, setting, original, jpeg, csv, projectTo));

                        if (setting.Folder == "0-today" || setting.Folder == "2-shaded")
                        {
                            ClashViewpointSays(document, clashTests, address, testName);
                        }
                    }
                }
                finally
                {
                    document.CurrentViewpoint.CopyFrom(original);
                    bool back = SavedViewpoints.RestoreHiddenState(document, snapshot);
                    Say("PUT BACK  the viewpoint the document opened with, and the hidden state read back as before: " + back);
                }
            }

            Say(string.Empty);
            Say("==== SUMMARY, " + count + " picture(s) a setting, the mean of the render and the save, and the total for " + projectTo + " pictures ====");

            foreach (string line in summary)
            {
                Say(line);
            }
        }

        private string OneSetting(
            Document document, DocumentClashTests clashTests, TestAddress address, string testName, int count, string picturesRoot,
            HashSet<int> keep, Setting setting, Viewpoint original, ImageCodecInfo jpeg, string csv, int projectTo)
        {
            Say("==== SETTING " + setting.Folder + ", " + setting.Words + " ====");
            string folder = Path.Combine(picturesRoot, setting.Folder);
            Directory.CreateDirectory(folder);

            document.CurrentViewpoint.CopyFrom(original);

            if (setting.OnlyTheTestsModels)
            {
                int hiddenModels = SavedViewpoints.ShowOnlyModels(document, keep);
                Say("hidden " + hiddenModels + " model(s), shown " + keep.Count);
            }
            else
            {
                document.Models.ResetAllHidden();
                Say("every model shown");
            }

            if (setting.Style.HasValue)
            {
                using (Viewpoint styled = document.CurrentViewpoint.CreateCopy())
                {
                    styled.RenderStyle = setting.Style.Value;
                    document.CurrentViewpoint.CopyFrom(styled);
                }
            }

            using (Viewpoint now = document.CurrentViewpoint.CreateCopy())
            {
                Say("the current viewpoint reads render style " + now.RenderStyle + ", lighting " + now.Lighting);
            }

            List<double> renders = new List<double>();
            List<double> totals = new List<double>();
            string nowNamed;

            using (ClashTest test = address.ResolveIn(clashTests, testName, out nowNamed))
            {
                if (test == null)
                {
                    Say("UNKNOWN: the test does not resolve, now named " + (nowNamed ?? "nothing"));
                    return setting.Folder + "  UNKNOWN, the test did not resolve";
                }

                SavedItemCollection children = test.Children;

                for (int i = 0; i < count; i++)
                {
                    using (SavedItem item = children[i])
                    {
                        IClashResult result = item as IClashResult;
                        string row = (i + 1).ToString("00", Inv);

                        if (result == null)
                        {
                            Say(row + "  " + item.DisplayName + "  UNKNOWN, not a clash result");
                            continue;
                        }

                        string path = Path.Combine(folder, row + ".jpg");

                        try
                        {
                            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
                            Bitmap bitmap = clashTests.TestsImageForResult(result, ImageGenerationStyle.ScenePlusOverlay, setting.Width, setting.Height);
                            double render = clock.Elapsed.TotalSeconds;
                            double save;
                            int width;
                            int height;

                            using (bitmap)
                            {
                                if (bitmap == null)
                                {
                                    Say(row + "  " + item.DisplayName + "  render " + S(render) + " s, TestsImageForResult returned nothing");
                                    continue;
                                }

                                width = bitmap.Width;
                                height = bitmap.Height;
                                clock = System.Diagnostics.Stopwatch.StartNew();
                                bitmap.Save(path, jpeg, null);
                                save = clock.Elapsed.TotalSeconds;
                            }

                            long bytes = new FileInfo(path).Length;
                            renders.Add(render);
                            totals.Add(render + save);
                            Say(row + "  " + item.DisplayName + "  render " + S(render) + " s, save " + S(save) + " s, " + bytes + " bytes, " + width + " by " + height);
                            File.AppendAllText(csv, setting.Folder + "," + row + "," + item.DisplayName.Replace(",", " ") + "," + S(render) + "," + S(save)
                                + "," + bytes + "," + width + "," + height + "\r\n", new UTF8Encoding(false));
                        }
                        catch (Exception error)
                        {
                            Say(row + "  " + item.DisplayName + "  THREW " + error.GetType().Name + ": " + error.Message + ", the next picture is tried");
                        }
                    }
                }
            }

            if (totals.Count == 0)
            {
                return setting.Folder + "  no picture was written";
            }

            double mean = Mean(totals);
            double projected = mean * projectTo;
            string line = setting.Folder + "  " + totals.Count + " pictures, render mean " + S(Mean(renders)) + " s, median " + S(Median(renders))
                + " s, least " + S(Min(renders)) + " s, most " + S(Max(renders)) + " s, render and save mean " + S(mean) + " s, for " + projectTo
                + " pictures " + projected.ToString("0", Inv) + " s, " + ((int)(projected / 3600)).ToString(Inv) + " h "
                + ((int)(projected % 3600 / 60)).ToString(Inv) + " min";
            Say(line);
            Say(string.Empty);
            return line;
        }

        /// <summary>What the clash's own viewpoint carries, read after a setting's pictures so it times no picture.</summary>
        private void ClashViewpointSays(Document document, DocumentClashTests clashTests, TestAddress address, string testName)
        {
            string nowNamed;

            try
            {
                using (ClashTest test = address.ResolveIn(clashTests, testName, out nowNamed))
                using (SavedItem item = test.Children[0])
                using (Viewpoint own = clashTests.TestsViewpointForResult((IClashResult)item))
                {
                    Say("the first row's own viewpoint: has a render style " + own.HasRenderStyle + ", render style " + own.RenderStyle
                        + ", has lighting " + own.HasLighting + ", lighting " + own.Lighting);
                }
            }
            catch (Exception error)
            {
                Say("the first row's own viewpoint could not be read, " + error.GetType().Name + ": " + error.Message + ", the probe goes on");
            }
        }

        private static double Mean(List<double> values)
        {
            double sum = 0;

            foreach (double v in values)
            {
                sum += v;
            }

            return sum / values.Count;
        }

        private static double Median(List<double> values)
        {
            List<double> sorted = new List<double>(values);
            sorted.Sort();
            int n = sorted.Count;
            return n % 2 == 1 ? sorted[n / 2] : (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
        }

        private static double Min(List<double> values)
        {
            double m = double.MaxValue;

            foreach (double v in values)
            {
                m = Math.Min(m, v);
            }

            return m;
        }

        private static double Max(List<double> values)
        {
            double m = double.MinValue;

            foreach (double v in values)
            {
                m = Math.Max(m, v);
            }

            return m;
        }
    }
}
