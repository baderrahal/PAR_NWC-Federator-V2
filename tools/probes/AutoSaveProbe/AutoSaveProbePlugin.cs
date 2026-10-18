using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Interop;
using Autodesk.Navisworks.Api.Plugins;
using Microsoft.Win32;

namespace AutoSaveProbe
{
    /// <summary>
    /// F223, Q207, 2026-10-18. Measured before anything is built, Bader's answer to Q207: the tool turns Navisworks
    /// Auto-Save off while it works and puts it back as it was when the run ends. What the tool would call is unmeasured:
    /// whether Navisworks' Auto-Save set can be read and written through LcUOption inside a running Navisworks, whether its
    /// autosave timer follows a value set there, and what reaches the registry.
    ///
    /// Mode list: every option of the set general.autosave with its index, name, type and value, read by the set under a
    /// lock, and the registry's value enable of GlobalOptions\general\autosave.
    ///
    /// Mode timer, handed the folder of NWC copies, a folder of the probe's own and the names the list mode read: every
    /// option read, then the location pointed at the probe's folder and read back, and only where it reads back there the
    /// time between saves set to one minute and the switch on, the document cleared and an NWC copy appended so it holds
    /// unsaved changes, and the main thread's messages pumped for the wait handed in, then the probe's folder listed. Then
    /// the switch off, another copy appended, the wait, the folder listed, then the switch on again, another copy, the wait
    /// and the folder listed. Then every option put back as it was read, each read back, and the registry read again. The
    /// document is cleared at the end. The mode timer saves its document once, probe-document.nwf in the work folder, and no file outside the probe's folder is written.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Auto-Save probe", ToolTip = "F223, Navisworks Auto-Save through LcUOption")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class AutoSaveProbePlugin : AddInPlugin
    {
        public const string PluginName = "AutoSaveProbe";
        public const string DeveloperCode = "PARS";

        private const string AutoSaveSet = "general.autosave";
        private const string RegistryKey = @"Software\Autodesk\Navisworks Manage\22.0\GlobalOptions\general\autosave";

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private StreamWriter results;

        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 2 || (parameters[0] != "list" && parameters[0] != "timer"))
            {
                return 2;
            }

            using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                Say("probe started, mode " + parameters[0] + ", pid " + Process.GetCurrentProcess().Id + ", main thread "
                    + Thread.CurrentThread.ManagedThreadId + ", apartment " + Thread.CurrentThread.GetApartmentState());

                try
                {
                    int code = parameters[0] == "list" ? List() : Timer(parameters);
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
            results.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", Inv) + "  " + line);
        }

        private int List()
        {
            Registry("before");
            Dictionary<string, string> read = ReadTheSet("the set as Navisworks started");
            return read == null ? 3 : 0;
        }

        /// <summary>Every option of the set, by index, its name, its type and its value, or null where the set would not read.</summary>
        private Dictionary<string, string> ReadTheSet(string when)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.Ordinal);

            try
            {
                using (LcUOptionLock optionLock = new LcUOptionLock())
                {
                    LcUOptionSet set = LcUOption.GetSet(AutoSaveSet, optionLock);

                    if (set == null)
                    {
                        Say("SET  " + AutoSaveSet + " gives no set, " + when);
                        return null;
                    }

                    Say("SET  " + AutoSaveSet + " holds " + set.GetNumOptions() + " options, " + when);
                    Walk(set, string.Empty, values, 0);
                }
            }
            catch (Exception error)
            {
                Say("SET  " + AutoSaveSet + " threw " + error.GetType().Name + ": " + error.Message + ", " + when);
                return null;
            }

            return values;
        }

        /// <summary>Every option of a set by its path under general.autosave, a sub-set walked into, at most four deep.</summary>
        private void Walk(LcUOptionSet set, string prefix, Dictionary<string, string> values, int depth)
        {
            int count = set.GetNumOptions();

            for (int i = 0; i < count; i++)
            {
                string name = prefix + set.GetName(i);
                string type = set.GetType(i).ToString();
                LcUOptionSet sub = null;

                try
                {
                    sub = set.GetSubOptions(i);
                }
                catch (Exception error)
                {
                    Say("  option " + name + "  its sub-set threw " + error.GetType().Name + ": " + error.Message);
                }

                if (sub != null && depth < 4)
                {
                    Say("  option " + name + "  " + type + "  a set of " + sub.GetNumOptions() + " options");
                    Walk(sub, name + ".", values, depth + 1);
                    continue;
                }

                string value;

                using (VariantData data = new VariantData())
                {
                    value = set.GetDataValue(i, data) ? Describe(data) : "UNKNOWN, GetDataValue returned false";
                }

                values[name] = value;
                Say("  option " + name + "  " + type + "  " + value);
            }
        }

        private static string Describe(VariantData data)
        {
            try
            {
                if (data.IsBoolean)
                {
                    return "Boolean:" + data.ToBoolean();
                }

                if (data.IsInt32)
                {
                    return "Int32:" + data.ToInt32().ToString(Inv);
                }

                if (data.IsDouble)
                {
                    return "Double:" + data.ToDouble().ToString("R", Inv);
                }

                if (data.IsDisplayString)
                {
                    return "DisplayString:" + data.ToDisplayString();
                }

                return data.ToString();
            }
            catch (Exception error)
            {
                return "UNKNOWN, " + error.GetType().Name + ": " + error.Message;
            }
        }

        private void Registry(string when)
        {
            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegistryKey, false))
                {
                    if (key == null)
                    {
                        Say("REGISTRY  " + when + ", the key is not there");
                        return;
                    }

                    List<string> names = new List<string>(key.GetValueNames());
                    names.Sort(StringComparer.Ordinal);
                    List<string> parts = new List<string>();

                    foreach (string name in names)
                    {
                        parts.Add(name + " = '" + key.GetValue(name) + "'");
                    }

                    Say("REGISTRY  " + when + ", " + (parts.Count == 0 ? "no value" : string.Join(", ", parts.ToArray())));
                }
            }
            catch (Exception error)
            {
                Say("REGISTRY  " + when + " threw " + error.GetType().Name + ": " + error.Message);
            }
        }

        /// <summary>
        /// The timer mode. parameters: timer, output, folder of NWC copies, the probe's own folder for the autosaves, the
        /// seconds to wait each time, then the names of the switch, the time between saves and the location as the list mode
        /// read them, and the name of the option that says the location is the default one, or a dash where there is none.
        /// </summary>
        private int Timer(string[] parameters)
        {
            if (parameters.Length != 9)
            {
                Say("REFUSED: the timer mode takes 9 arguments and was handed " + parameters.Length);
                return 2;
            }

            string copies = parameters[2];
            string folder = parameters[3];
            int wait = int.Parse(parameters[4], Inv);
            string enable = parameters[5];
            string interval = parameters[6];
            string location = parameters[7];
            string useDefault = parameters[8] == "-" ? null : parameters[8];
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;
            string[] files = Directory.GetFiles(copies, "*.nwc");
            Array.Sort(files, StringComparer.Ordinal);

            if (document == null || files.Length == 0 || !Directory.Exists(folder))
            {
                Say("UNKNOWN: no active document, no NWC copy or no folder for the autosaves");
                return 4;
            }

            Registry("before");
            Dictionary<string, string> before = ReadTheSet("as Navisworks started");

            if (before == null)
            {
                return 3;
            }

            bool? enableBefore = ReadBoolean(enable);
            int? intervalBefore = ReadInt(interval);
            string locationBefore = ReadString(location);
            bool? useDefaultBefore = useDefault == null ? (bool?)null : ReadBoolean(useDefault);
            Say("READ  " + enable + " " + Words(enableBefore) + ", " + interval + " " + Words(intervalBefore) + ", " + location + " '"
                + (locationBefore ?? "UNKNOWN") + "'" + (useDefault == null ? string.Empty : ", " + useDefault + " " + Words(useDefaultBefore)));

            try
            {
                if (useDefault != null)
                {
                    Say("SET  " + useDefault + " false returned " + LcUOption.SetBoolean(AutoSaveSet + "." + useDefault, false) + ", reads " + Words(ReadBoolean(useDefault)));
                }

                Say("SET  " + location + " to the probe's folder returned " + LcUOption.SetWideString(AutoSaveSet + "." + location, folder));
                string locationNow = ReadString(location);
                Say("READ  " + location + " now '" + (locationNow ?? "UNKNOWN") + "'");

                if (!string.Equals(locationNow, folder, StringComparison.OrdinalIgnoreCase))
                {
                    Say("STOP: the location does not read back as the probe's folder, so the switch is never set on and no autosave can reach any other folder");
                    return 5;
                }

                Say("SET  " + interval + " 1 returned " + LcUOption.SetInt32(AutoSaveSet + "." + interval, 1) + ", reads " + Words(ReadInt(interval)));
                Registry("with the location and the time set");

                // A document with a name, saved in the probe's own work folder, so an autosave has a name to take. Each
                // stretch then appends one more copy, which leaves it holding unsaved changes.
                string named = Path.Combine(Path.GetDirectoryName(folder.TrimEnd('\\')), "probe-document.nwf");
                document.Clear();
                Say("TryAppendFile " + Path.GetFileName(files[0]) + " returned " + document.TryAppendFile(files[0]));
                Say("TrySaveFile " + named + " returned " + document.TrySaveFile(named) + ", the document is now " + document.FileName);

                Stretch(document, files[Math.Min(1, files.Length - 1)], enable, true, folder, wait, "1, the switch ON");
                Stretch(document, files[Math.Min(2, files.Length - 1)], enable, false, folder, wait, "2, the switch OFF");
                Stretch(document, files[0], enable, true, folder, wait, "3, the switch ON again");
            }
            finally
            {
                Say("PUT BACK");

                if (enableBefore != null)
                {
                    Say("  " + enable + " " + enableBefore.Value + " returned " + LcUOption.SetBoolean(AutoSaveSet + "." + enable, enableBefore.Value) + ", reads " + Words(ReadBoolean(enable)));
                }

                if (intervalBefore != null)
                {
                    Say("  " + interval + " " + intervalBefore.Value + " returned " + LcUOption.SetInt32(AutoSaveSet + "." + interval, intervalBefore.Value) + ", reads " + Words(ReadInt(interval)));
                }

                if (locationBefore != null)
                {
                    Say("  " + location + " returned " + LcUOption.SetWideString(AutoSaveSet + "." + location, locationBefore) + ", reads '" + (ReadString(location) ?? "UNKNOWN") + "'");
                }

                if (useDefault != null && useDefaultBefore != null)
                {
                    Say("  " + useDefault + " " + useDefaultBefore.Value + " returned " + LcUOption.SetBoolean(AutoSaveSet + "." + useDefault, useDefaultBefore.Value) + ", reads " + Words(ReadBoolean(useDefault)));
                }

                ReadTheSet("after the put back");
                Registry("after the put back");

                try
                {
                    document.Clear();
                    Say("the document cleared at the end");
                }
                catch (Exception error)
                {
                    Say("the clear at the end threw " + error.GetType().Name + ": " + error.Message);
                }
            }

            return 0;
        }

        /// <summary>
        /// One stretch: the switch set and read back, one more copy appended into the named document, the wait, then the
        /// probe's folder listed and the files of the user's own AutoSave folder counted, any new one there named.
        /// </summary>
        private void Stretch(Document document, string file, string enable, bool on, string folder, int wait, string what)
        {
            Say("");
            Say("==== STRETCH " + what + " ====");
            Say("SET  " + enable + " " + on + " returned " + LcUOption.SetBoolean(AutoSaveSet + "." + enable, on) + ", reads " + Words(ReadBoolean(enable)));
            Registry("after the switch was set");
            int before = Directory.GetFiles(folder).Length;
            string users = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\Navisworks Manage 2025\AutoSave");
            HashSet<string> usersBefore = new HashSet<string>(Directory.Exists(users) ? Directory.GetFiles(users) : new string[0], StringComparer.OrdinalIgnoreCase);
            Stopwatch append = Stopwatch.StartNew();
            bool appended = document.TryAppendFile(file);
            Say("TryAppendFile " + Path.GetFileName(file) + " returned " + appended + " after " + append.Elapsed.TotalSeconds.ToString("0.00", Inv)
                + " s, the document holds " + document.Models.Count + " models");
            Pump(wait);
            string[] after = Directory.GetFiles(folder);
            Say("FOLDER after " + wait + " s of pumped messages: " + after.Length + " files, " + (after.Length - before) + " new in this stretch");

            foreach (string f in after)
            {
                FileInfo info = new FileInfo(f);
                Say("  " + info.Name + "  " + info.Length + " bytes, written " + info.LastWriteTime.ToString("HH:mm:ss.fff", Inv));
            }

            string[] usersAfter = Directory.Exists(users) ? Directory.GetFiles(users) : new string[0];
            int added = 0;

            foreach (string f in usersAfter)
            {
                if (!usersBefore.Contains(f))
                {
                    added++;
                    Say("  NEW IN THE USER'S AUTOSAVE FOLDER  " + Path.GetFileName(f) + "  " + new FileInfo(f).Length + " bytes");
                }
            }

            Say("USER'S AUTOSAVE FOLDER " + usersBefore.Count + " files before this stretch, " + usersAfter.Length + " after, " + added + " new");
        }

        /// <summary>The main thread's messages pumped for so many seconds, so Navisworks' own timers can fire meanwhile.</summary>
        private static void Pump(int seconds)
        {
            DispatcherFrame frame = new DispatcherFrame();
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
            timer.Tick += delegate
            {
                timer.Stop();
                frame.Continue = false;
            };
            timer.Start();
            Dispatcher.PushFrame(frame);
        }

        private static string Words<T>(T? value) where T : struct
        {
            return value == null ? "UNKNOWN" : value.Value.ToString();
        }

        private static bool? ReadBoolean(string name)
        {
            bool value = false;
            return LcUOption.TryGetBoolean(AutoSaveSet + "." + name, ref value) ? value : (bool?)null;
        }

        private static int? ReadInt(string name)
        {
            int value = 0;
            return LcUOption.TryGetInt32(AutoSaveSet + "." + name, ref value) ? value : (int?)null;
        }

        private static string ReadString(string name)
        {
            string value = null;
            return LcUOption.TryGetWideString(AutoSaveSet + "." + name, out value) ? value : null;
        }
    }
}
