using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.Interop;
using Federator.Addin.Engine;

namespace PictureSpeedProbe
{
    /// <summary>
    /// F150, Bader's answer of 2026-10-12 to Q151 and his yes to the one short API test: can the tool
    /// set Clash Detective's Hide Other for its own pictures inside a running Navisworks, and put the
    /// user's value back. The installed DLLs, read on 2026-10-12 with no Navisworks started, hold
    /// LcUOption's static GetBoolean, TryGetBoolean and SetBoolean on an option path, and the registry
    /// keeps the value at GlobalOptions\tools\clash\displaysettings\hide_other, docs\history\scan.md
    /// 5z-ze. This mode reads the three display values two ways, the full path and the set under a
    /// lock, then takes three pictures with the user's value, three with Hide Other set through the
    /// API, and three once the value is put back, each timed and its size kept. scan.md 5z-ze measured
    /// the two-items look at 48 KB against about 560 KB with the defaults.
    /// </summary>
    public sealed partial class PictureSpeedProbePlugin
    {
        private const string DisplaySet = "tools.clash.displaysettings";

        private void HideOtherApiStudy(DocumentClashTests clashTests, TestAddress address, string testName, int count, string picturesRoot)
        {
            Say("==== THE OPTIONS, read before anything is set ====");
            bool? hideBefore = ReadOption("hide_other");
            ReadOption("dim_other");
            ReadOption("transparent_dimming");

            if (hideBefore == null)
            {
                Say("UNKNOWN: hide_other could not be read by either way, so nothing is set and nothing more is measured");
                return;
            }

            ImageCodecInfo jpeg = null;

            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    jpeg = codec;
                }
            }

            List<string> summary = new List<string>();

            try
            {
                summary.Add(Shots("A-as-the-user-has-it", clashTests, address, testName, count, picturesRoot, jpeg));

                bool set = LcUOption.SetBoolean(DisplaySet + ".hide_other", true);
                Say("SET  LcUOption.SetBoolean(" + DisplaySet + ".hide_other, true) returned " + set);
                ReadOption("hide_other");
                summary.Add(Shots("B-hide-other-set-by-the-api", clashTests, address, testName, count, picturesRoot, jpeg));
            }
            finally
            {
                bool back = LcUOption.SetBoolean(DisplaySet + ".hide_other", hideBefore.Value);
                bool? now = ReadOption("hide_other");
                Say("PUT BACK  hide_other set to " + hideBefore.Value + ", returned " + back + ", it reads " + (now == null ? "UNKNOWN" : now.Value.ToString())
                    + (now == hideBefore ? ", as before" : ", NOT AS BEFORE"));
            }

            summary.Add(Shots("C-after-the-put-back", clashTests, address, testName, count, picturesRoot, jpeg));

            Say(string.Empty);
            Say("==== SUMMARY, mode hideapi ====");

            foreach (string line in summary)
            {
                Say(line);
            }
        }

        /// <summary>One display value read by the full path and by the set under a lock, each said. Null where neither reads.</summary>
        private bool? ReadOption(string name)
        {
            bool? byPath = null;
            bool? bySet = null;
            string pathWhy = string.Empty;
            string setWhy = string.Empty;

            try
            {
                bool value = false;

                if (LcUOption.TryGetBoolean(DisplaySet + "." + name, ref value))
                {
                    byPath = value;
                }
                else
                {
                    pathWhy = "TryGetBoolean returned false";
                }
            }
            catch (Exception error)
            {
                pathWhy = error.GetType().Name + ": " + error.Message;
            }

            try
            {
                using (LcUOptionLock optionLock = new LcUOptionLock())
                {
                    LcUOptionSet options = LcUOption.GetSet(DisplaySet, optionLock);

                    if (options == null)
                    {
                        setWhy = "GetSet gave no set";
                    }
                    else
                    {
                        bool value = false;

                        if (options.TryGetBoolean(name, ref value))
                        {
                            bySet = value;
                        }
                        else
                        {
                            setWhy = "the set's TryGetBoolean returned false";
                        }
                    }
                }
            }
            catch (Exception error)
            {
                setWhy = error.GetType().Name + ": " + error.Message;
            }

            Say("OPTION  " + name + ": by the path " + (byPath == null ? "UNKNOWN, " + pathWhy : byPath.Value.ToString())
                + ", by the set " + (bySet == null ? "UNKNOWN, " + setWhy : bySet.Value.ToString()));

            return byPath ?? bySet;
        }

        /// <summary>The first rows' pictures through the tool's own call, each timed and its size kept, under one setting's folder.</summary>
        private string Shots(string setting, DocumentClashTests clashTests, TestAddress address, string testName, int count, string picturesRoot, ImageCodecInfo jpeg)
        {
            Say("==== SETTING " + setting + ", TestsImageForResult as the tool calls it, 1024 by 1024 ====");
            string folder = Path.Combine(picturesRoot, setting);
            Directory.CreateDirectory(folder);
            List<double> seconds = new List<double>();
            List<long> sizes = new List<long>();
            string nowNamed;

            using (ClashTest test = address.ResolveIn(clashTests, testName, out nowNamed))
            {
                SavedItemCollection children = test.Children;

                for (int i = 0; i < count && i < children.Count; i++)
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
                            bitmap.Save(path, jpeg, null);
                        }

                        seconds.Add(clock.Elapsed.TotalSeconds);
                        sizes.Add(new FileInfo(path).Length);
                        Say((i + 1).ToString("00") + "  " + item.DisplayName + "  render and save " + S(seconds[seconds.Count - 1]) + " s, " + sizes[sizes.Count - 1] + " bytes");
                    }
                }
            }

            double meanSeconds = 0;
            double meanBytes = 0;

            for (int i = 0; i < seconds.Count; i++)
            {
                meanSeconds += seconds[i] / seconds.Count;
                meanBytes += (double)sizes[i] / seconds.Count;
            }

            return setting + "  " + seconds.Count + " pictures, mean " + S(meanSeconds) + " s, mean " + Math.Round(meanBytes / 1024) + " KB";
        }
    }
}
