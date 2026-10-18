using System;

namespace Federator.Core.Diagnostics
{
    /// <summary>What Navisworks' Auto-Save switch reads as, from the registry or from a running Navisworks.</summary>
    public enum AutoSaveState
    {
        /// <summary>The value could not be read, or reads as nothing this tool knows.</summary>
        Unknown,

        /// <summary>The registry value reads "0", a value never set, Navisworks' default, on by the autosaves this machine made before F223 and not by a start measured with it.</summary>
        NeverSet,

        On,

        Off
    }

    /// <summary>
    /// F223, Bader's answer to Q207 of 18 Oct 2026: the tool turns Navisworks Auto-Save off while it works and puts it back
    /// exactly as it was when the run ends, even when a group fails or Navisworks closes, reading the value before, logging
    /// it and logging the value put back. Not an append setting, so the manual append rule of Q202 stands.
    ///
    /// Two places hold the switch. The registry, the value enable of GlobalOptions\general\autosave under the 22.0 key, which
    /// a Navisworks reads as it starts: "0" was read on every Auto-Save value never set, and "3 0" and "3 1" on the set
    /// booleans of that key, measured on 2026-10-05, and a start with "3 0" wrote no autosave, F138 and scan.md 5z-zl. And the
    /// running Navisworks, the option general.autosave.enable, read and set through LcUOption, a set value reaching no
    /// registry value while the session runs, 5z-zl, and whether it stops the autosaves of a session started on is UNKNOWN,
    /// row F223-R1. So the runner switches the registry off before each
    /// group's Navisworks starts and puts it back once, as the run ends, AutoSaveKeeper, and the window switches
    /// the option off for a run in the Navisworks a person started and puts it back as the run ends. Every line is made here.
    /// </summary>
    public static class AutoSaveSwitch
    {
        /// <summary>The key under HKEY_CURRENT_USER that holds the switch, measured on 2026-10-05.</summary>
        public const string RegistryKey = @"Software\Autodesk\Navisworks Manage\22.0\GlobalOptions\general\autosave";

        public const string RegistryValue = "enable";

        /// <summary>The value a set boolean of that key reads off, written to switch Auto-Save off for a start, F138.</summary>
        public const string OffValue = "3 0";

        /// <summary>The option path of the switch in a running Navisworks, read off its option set on 2026-10-18, 5z-zl.</summary>
        public const string OptionPath = "general.autosave.enable";

        public const string Prefix = "AUTOSAVE";

        /// <summary>What a registry value reads as: "3 0" off, "3 1" on, "0" never set, anything else or none UNKNOWN.</summary>
        public static AutoSaveState OfRegistryValue(string value)
        {
            switch (value)
            {
                case OffValue:
                    return AutoSaveState.Off;
                case "3 1":
                    return AutoSaveState.On;
                case "0":
                    return AutoSaveState.NeverSet;
                default:
                    return AutoSaveState.Unknown;
            }
        }

        public static string Words(AutoSaveState state)
        {
            switch (state)
            {
                case AutoSaveState.NeverSet:
                    return "never set, Navisworks' own default";
                case AutoSaveState.On:
                    return "on";
                case AutoSaveState.Off:
                    return "off";
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>A registry value as a line reads it, quoted, or the words for none.</summary>
        public static string Quoted(string value)
        {
            return value == null ? "no value" : "'" + value + "'";
        }

        // ---------- the runner, the registry across a run, AutoSaveKeeper ----------

        /// <summary>The run's line where the value before was read off the registry and recorded.</summary>
        public static string Kept(string before)
        {
            return Prefix + " the registry's switch reads " + Quoted(before) + ", " + Words(OfRegistryValue(before))
                + ", kept in " + AutoSaveKeeper.RecordFileName + " as the value to put back once the run ends";
        }

        /// <summary>The run's line where a record was there already, left by a run that has the switch off or that ended before its put back.</summary>
        public static string KeptFromAnEarlierRun(string before)
        {
            return Prefix + " " + AutoSaveKeeper.RecordFileName + " is there already, written by another run that has the switch off or left by a run that ended before"
                + " its put back, and it holds " + Quoted(before) + ", " + Words(OfRegistryValue(before)) + ", taken as the value to put back once the run ends";
        }

        /// <summary>The run's line where the value to put back is not known, so nothing is switched for the whole run.</summary>
        public static string NotKept(string why)
        {
            return Prefix + " " + why + ", so the switch is never written this run and each Navisworks may autosave";
        }

        public static string NotSwitched(string group)
        {
            return group + ": " + Prefix + " not switched off, the value to put back is not known, so this group's Navisworks may autosave";
        }

        public static string NotSwitched(string group, string why)
        {
            return group + ": " + Prefix + " not switched off, " + why + ", so this group's Navisworks may autosave";
        }

        public static string OffAlready(string group)
        {
            return group + ": " + Prefix + " the registry's switch reads " + Quoted(OffValue) + ", off already for this group's Navisworks";
        }

        public static string SwitchedOff(string group, string was, string readBack)
        {
            return group + ": " + Prefix + " the registry's switch read " + Quoted(was) + ", "
                + (readBack == OffValue
                    ? "switched off for this group's Navisworks, it reads " + Quoted(readBack)
                    : "and switching it off did not read back, it reads " + Quoted(readBack) + ", so this group's Navisworks may autosave");
        }

        public static string ReadsAfterTheQuit(string group, string now)
        {
            return group + ": " + Prefix + " the registry's switch reads " + Quoted(now) + " once its Navisworks is gone";
        }

        public static string NotReadAfterTheQuit(string group, string why)
        {
            return group + ": " + Prefix + " the registry's switch could not be read once its Navisworks was gone, " + why;
        }

        /// <summary>The line where the value could not be put back, the record kept so the next opening of the window tries again.</summary>
        public static string NotPutBack(string before, string why, bool leftByARun)
        {
            return Prefix + " the registry's switch could not be put back to " + Quoted(before) + (leftByARun ? ", the value a run left in " + AutoSaveKeeper.RecordFileName : string.Empty)
                + ", " + why + ". " + AutoSaveKeeper.RecordFileName + " is kept, so the next opening of the tool tries again. Set Auto-Save back by hand if it stays";
        }

        /// <summary>The line of a put back: what it read, the value put back and what it reads, and whether the record went.</summary>
        public static string PutBack(string now, string before, string readBack, bool leftByARun, bool stillRuns, bool deleted, string notDeleted)
        {
            bool asItWas = string.Equals(readBack, before, StringComparison.Ordinal);
            string what = (leftByARun ? " a run ended before it put the switch back, so" : string.Empty)
                + " the registry's switch read " + Quoted(now)
                + (string.Equals(now, before, StringComparison.Ordinal) ? ", as it was before the run," : ", put back to " + Quoted(before) + ",")
                + " it reads " + Quoted(readBack);

            if (!asItWas)
            {
                return Prefix + what + ", NOT AS IT WAS, " + AutoSaveKeeper.RecordFileName + " is kept and the next opening of the tool tries again."
                    + " Set Auto-Save back by hand if it stays";
            }

            if (stillRuns)
            {
                return Prefix + what + ", and a Navisworks of the run still runs and may write the switch as it quits, so "
                    + AutoSaveKeeper.RecordFileName + " is kept and the next opening of the tool reads it again";
            }

            return Prefix + what + (deleted
                ? ", " + AutoSaveKeeper.RecordFileName + " deleted"
                : ", and " + AutoSaveKeeper.RecordFileName + " could not be deleted, " + notDeleted + ", so the next opening of the tool reads it again");
        }

        // ---------- the window, the option of the running Navisworks across one run ----------

        /// <summary>The window's line as a run starts: what the option read and that it was switched off, or why not.</summary>
        public static string AsTheRunStarts(bool? read, string why, bool? setReturned, bool? readBack)
        {
            if (read == null)
            {
                return Prefix + " the option " + OptionPath + " could not be read, " + (why ?? "UNKNOWN why") + ", so it is left as it is for this run";
            }

            if (!read.Value)
            {
                return Prefix + " the option " + OptionPath + " reads off as the run starts, left as it is";
            }

            if (why != null)
            {
                return Prefix + " the option " + OptionPath + " reads on as the run starts, and switching it off threw " + why
                    + ", it reads " + OnOff(readBack) + ", so whether this run can autosave is UNKNOWN. It is set back on as the run ends";
            }

            return Prefix + " the option " + OptionPath + " reads on as the run starts, switched off for the run, the set returned "
                + Said(setReturned) + " and it reads " + OnOff(readBack)
                + (readBack == false
                    ? ". Whether a value set off in a Navisworks started with it on stops its autosaves is UNKNOWN, scan.md 5z-zl"
                    : ", so whether this run can autosave is UNKNOWN");
        }

        /// <summary>The window's line as the run ends: the value put back and what it reads, or that nothing was changed.</summary>
        public static string AsTheRunEnds(bool? before, bool changed, bool? setReturned, bool? readBack, string why)
        {
            if (!changed)
            {
                return Prefix + " the option " + OptionPath + " was not changed by this run, it reads " + OnOff(readBack) + " as the run ends"
                    + (why == null ? string.Empty : ", reading it threw " + why);
            }

            if (why != null)
            {
                return Prefix + " the option " + OptionPath + " could not be put back, " + why + ". It read " + OnOff(before)
                    + " as the run started, set Auto-Save back to that in Options";
            }

            return Prefix + " the option " + OptionPath + " put back to " + OnOff(before) + " as the run ends, the set returned "
                + Said(setReturned) + " and it reads " + OnOff(readBack)
                + (readBack == before ? string.Empty : ", NOT AS IT WAS, set Auto-Save back to " + OnOff(before) + " in Options");
        }

        /// <summary>The window's line where a record said the switch was on and this Navisworks read it off, set back on.</summary>
        public static string SessionPutBack(bool setReturned, bool? readBack)
        {
            return Prefix + " the option " + OptionPath + " read off in this Navisworks where the record said on, set back on, the set returned "
                + Said(setReturned) + " and it reads " + OnOff(readBack);
        }

        public static string SessionNotPutBack(string why)
        {
            return Prefix + " the option " + OptionPath + " could not be set back on in this Navisworks, " + why + ", set Auto-Save on in Options";
        }

        private static string OnOff(bool? value)
        {
            return value == null ? "UNKNOWN" : value.Value ? "on" : "off";
        }

        private static string Said(bool? value)
        {
            return value == null ? "UNKNOWN" : value.Value ? "true" : "false";
        }
    }
}
