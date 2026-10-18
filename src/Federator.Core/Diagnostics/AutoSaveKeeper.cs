using System;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace Federator.Core.Diagnostics
{
    /// <summary>The registry's Auto-Save switch, read and written one way, so the rule above it is proved on a fake.</summary>
    public interface IAutoSaveRegistry
    {
        /// <summary>
        /// The value, null where the key holds none. False with why where the key is not there or the value is not a
        /// string, so it is never written.
        /// </summary>
        bool TryRead(out string value, out string why);

        /// <summary>Writes the value as a string. Throws where it cannot.</summary>
        void Write(string value);

        /// <summary>Takes the value out. Throws where it cannot.</summary>
        void Remove();
    }

    /// <summary>The file that holds the value to put back while a run has the switch off.</summary>
    public interface IAutoSaveRecord
    {
        /// <summary>The file's text, null where there is no file. Throws where it cannot be read.</summary>
        string Read();

        void Write(string text);

        void Delete();
    }

    /// <summary>
    /// F223, Bader's answer to Q207 of 18 Oct 2026, on the breaker's reading of its first pass: the value the switch held
    /// before a run, kept ON DISK until it is put back, so a runner that dies, a runner the window ends, two runs at once or
    /// a group whose put back failed never hands the next group or the next run "3 0" as the person's own value. Begin takes
    /// the value off the record where one is there, since an earlier run left it there with the switch off, and otherwise
    /// off the registry, and writes the record before anything is switched. Nothing is switched where the value or the
    /// record could not be read or the record could not be written, since a switch with no record of what to put back could
    /// leave Auto-Save off for good. SwitchOffFor writes "3 0" before each group's Navisworks starts and writes the record
    /// again where another run deleted it. PutBack, once, as the run ends, writes the value back, or takes it out where there
    /// was none, reads it back and deletes the record only where it reads as it was and no Navisworks of the run still runs,
    /// so the next opening of the window puts it back again. PutBackWhatARunLeft is that: a record found where no run holds
    /// it is put back and deleted. Every line is AutoSaveSwitch's. Never throws.
    /// </summary>
    public sealed class AutoSaveKeeper
    {
        /// <summary>The record's file under the tool's own folder of LocalApplicationData.</summary>
        public const string RecordFileName = "auto-save-to-put-back.txt";

        private const string Header = "parsons nwc federator, the auto-save switch to put back, 1";
        private const string BeforeKey = "before\t";
        private const string ValueWord = "value ";
        private const string NoValueWord = "no value";

        private readonly IAutoSaveRegistry registry;
        private readonly IAutoSaveRecord record;
        private readonly Action<string> say;

        private AutoSaveKeeper(IAutoSaveRegistry registry, IAutoSaveRecord record, Action<string> say)
        {
            this.registry = registry;
            this.record = record;
            this.say = say ?? delegate { };
        }

        /// <summary>Whether the value to put back is known and on the record, so the switch may be written.</summary>
        public bool Known { get; private set; }

        /// <summary>The value to put back, null for no value.</summary>
        public string Before { get; private set; }

        /// <summary>The record's path for this user.</summary>
        public static string RecordPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), RunLog.AppFolderName, RecordFileName);
        }

        /// <summary>The record's text for that value.</summary>
        public static string RecordText(string before)
        {
            return Header + "\r\n" + BeforeKey + (before == null ? NoValueWord : ValueWord + before) + "\r\n";
        }

        /// <summary>The value a record holds, or false with why where it is not a record of this shape.</summary>
        public static bool TryReadRecord(string text, out string before, out string why)
        {
            before = null;
            why = null;
            string[] lines = (text ?? string.Empty).Replace("\r\n", "\n").Split('\n');

            if (lines.Length < 2 || lines[0] != Header)
            {
                why = "its first line is not '" + Header + "'";
                return false;
            }

            if (!lines[1].StartsWith(BeforeKey, StringComparison.Ordinal))
            {
                why = "its second line does not start with before";
                return false;
            }

            string held = lines[1].Substring(BeforeKey.Length);

            if (held == NoValueWord)
            {
                return true;
            }

            if (held.StartsWith(ValueWord, StringComparison.Ordinal))
            {
                before = held.Substring(ValueWord.Length);
                return true;
            }

            why = "its value is neither 'no value' nor 'value' and the value";
            return false;
        }

        /// <summary>The value to put back for this run, off the record an earlier run left or off the registry, recorded.</summary>
        public static AutoSaveKeeper Begin(IAutoSaveRegistry registry, IAutoSaveRecord record, Action<string> say)
        {
            AutoSaveKeeper keeper = new AutoSaveKeeper(registry, record, say);
            string text;

            try
            {
                text = record.Read();
            }
            catch (Exception error)
            {
                keeper.say(AutoSaveSwitch.NotKept("the record " + RecordFileName + " could not be read, " + Why(error)));
                return keeper;
            }

            string before;
            string why;

            if (text != null)
            {
                if (!TryReadRecord(text, out before, out why))
                {
                    keeper.say(AutoSaveSwitch.NotKept("the record " + RecordFileName + " is not one, " + why + ", and it is left as it is"));
                    return keeper;
                }

                keeper.Known = true;
                keeper.Before = before;
                keeper.say(AutoSaveSwitch.KeptFromAnEarlierRun(before));
                return keeper;
            }

            if (!registry.TryRead(out before, out why))
            {
                keeper.say(AutoSaveSwitch.NotKept("the registry's switch could not be read, " + why));
                return keeper;
            }

            try
            {
                record.Write(RecordText(before));
            }
            catch (Exception error)
            {
                keeper.say(AutoSaveSwitch.NotKept("the registry's switch reads " + AutoSaveSwitch.Quoted(before) + " and the record "
                    + RecordFileName + " could not be written, " + Why(error)));
                return keeper;
            }

            keeper.Known = true;
            keeper.Before = before;
            keeper.say(AutoSaveSwitch.Kept(before));
            return keeper;
        }

        /// <summary>The switch written off before one group's Navisworks starts, the record written again where it was gone.</summary>
        public void SwitchOffFor(string group)
        {
            if (!Known)
            {
                say(AutoSaveSwitch.NotSwitched(group));
                return;
            }

            try
            {
                if (record.Read() == null)
                {
                    record.Write(RecordText(Before));
                }
            }
            catch (Exception error)
            {
                say(AutoSaveSwitch.NotSwitched(group, "the record " + RecordFileName + " could not be written again, " + Why(error)));
                return;
            }

            string now;
            string why;

            if (!registry.TryRead(out now, out why))
            {
                say(AutoSaveSwitch.NotSwitched(group, "the registry's switch could not be read, " + why));
                return;
            }

            if (now == AutoSaveSwitch.OffValue)
            {
                say(AutoSaveSwitch.OffAlready(group));
                return;
            }

            try
            {
                registry.Write(AutoSaveSwitch.OffValue);
            }
            catch (Exception error)
            {
                say(AutoSaveSwitch.NotSwitched(group, "writing it threw " + Why(error)));
                return;
            }

            string readBack;
            registry.TryRead(out readBack, out why);
            say(AutoSaveSwitch.SwitchedOff(group, now, readBack));
        }

        /// <summary>What the switch reads once a group's Navisworks is gone, which that Navisworks may have written as it quit.</summary>
        public void ReadAfter(string group)
        {
            string now;
            string why;
            say(registry.TryRead(out now, out why) ? AutoSaveSwitch.ReadsAfterTheQuit(group, now) : AutoSaveSwitch.NotReadAfterTheQuit(group, why));
        }

        /// <summary>
        /// The value put back as the run ends and read back, the record deleted only where it reads as it was and no
        /// Navisworks of the run still runs. True where it reads as it was or nothing was switched.
        /// </summary>
        public bool PutBack(bool aNavisworksOfTheRunStillRuns)
        {
            if (!Known)
            {
                return true;
            }

            return PutBackAndDelete(registry, record, say, Before, aNavisworksOfTheRunStillRuns, false);
        }

        /// <summary>
        /// A record no run holds, left by a run that ended before its put back, put back and deleted, said, and nothing said
        /// where there is none. True where the switch now reads as the record says or there was no record.
        /// </summary>
        public static bool PutBackWhatARunLeft(IAutoSaveRegistry registry, IAutoSaveRecord record, Action<string> say, bool aNavisworksOfTheRunStillRuns)
        {
            say = say ?? delegate { };
            string text;

            try
            {
                text = record.Read();
            }
            catch (Exception error)
            {
                say(AutoSaveSwitch.NotKept("the record " + RecordFileName + " could not be read, " + Why(error)));
                return false;
            }

            if (text == null)
            {
                return true;
            }

            string before;
            string why;

            if (!TryReadRecord(text, out before, out why))
            {
                say(AutoSaveSwitch.NotKept("the record " + RecordFileName + " is not one, " + why + ", and it is left as it is"));
                return false;
            }

            return PutBackAndDelete(registry, record, say, before, aNavisworksOfTheRunStillRuns, true);
        }

        private static bool PutBackAndDelete(
            IAutoSaveRegistry registry, IAutoSaveRecord record, Action<string> say, string before, bool stillRuns, bool leftByARun)
        {
            string now;
            string why;
            // A switch that will not read, its key gone or a value of another kind, is never written over, and the record stays.
            if (!registry.TryRead(out now, out why))
            {
                say(AutoSaveSwitch.NotPutBack(before, why, leftByARun));
                return false;
            }

            if (!string.Equals(now, before, StringComparison.Ordinal))
            {
                try
                {
                    if (before == null)
                    {
                        registry.Remove();
                    }
                    else
                    {
                        registry.Write(before);
                    }
                }
                catch (Exception error)
                {
                    say(AutoSaveSwitch.NotPutBack(before, Why(error), leftByARun));
                    return false;
                }
            }

            string readBack;
            registry.TryRead(out readBack, out why);
            bool asItWas = string.Equals(readBack, before, StringComparison.Ordinal);
            bool deleted = false;
            string notDeleted = null;

            if (asItWas && !stillRuns)
            {
                try
                {
                    record.Delete();
                    deleted = true;
                }
                catch (Exception error)
                {
                    notDeleted = Why(error);
                }
            }

            say(AutoSaveSwitch.PutBack(now, before, readBack, leftByARun, stillRuns, deleted, notDeleted));
            return asItWas;
        }

        private static string Why(Exception error)
        {
            return error.GetType().Name + ": " + error.Message;
        }
    }

    /// <summary>The registry's switch under HKEY_CURRENT_USER, the key never made, a value of another kind never written.</summary>
    public sealed class WindowsAutoSaveRegistry : IAutoSaveRegistry
    {
        public bool TryRead(out string value, out string why)
        {
            value = null;
            why = null;

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(AutoSaveSwitch.RegistryKey, false))
                {
                    if (key == null)
                    {
                        why = "the key is not there and a key is never made";
                        return false;
                    }

                    object held = key.GetValue(AutoSaveSwitch.RegistryValue, null, RegistryValueOptions.DoNotExpandEnvironmentNames);

                    if (held == null)
                    {
                        return true;
                    }

                    RegistryValueKind kind = key.GetValueKind(AutoSaveSwitch.RegistryValue);

                    if (kind != RegistryValueKind.String)
                    {
                        why = "the value is of the kind " + kind + ", not a string, and is never written";
                        return false;
                    }

                    value = (string)held;
                    return true;
                }
            }
            catch (Exception error)
            {
                why = error.GetType().Name + ": " + error.Message;
                return false;
            }
        }

        public void Write(string value)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(AutoSaveSwitch.RegistryKey, true))
            {
                if (key == null)
                {
                    throw new InvalidOperationException("the key is not there and a key is never made");
                }

                key.SetValue(AutoSaveSwitch.RegistryValue, value, RegistryValueKind.String);
            }
        }

        public void Remove()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(AutoSaveSwitch.RegistryKey, true))
            {
                if (key != null)
                {
                    key.DeleteValue(AutoSaveSwitch.RegistryValue, false);
                }
            }
        }
    }

    /// <summary>The record as a file, written whole through a file beside it so a cut write never reads as a record.</summary>
    public sealed class FileAutoSaveRecord : IAutoSaveRecord
    {
        private readonly string path;

        public FileAutoSaveRecord(string path)
        {
            this.path = path;
        }

        public string Read()
        {
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
        }

        public void Write(string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string part = path + ".writing";
            File.WriteAllText(part, text, new UTF8Encoding(false));

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Move(part, path);
        }

        public void Delete()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
