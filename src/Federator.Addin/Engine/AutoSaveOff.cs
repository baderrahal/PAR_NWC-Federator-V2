using System;
using Autodesk.Navisworks.Api.Interop;
using Federator.Core.Diagnostics;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// F223, Bader's answer to Q207 of 18 Oct 2026: Navisworks' Auto-Save switched off for one run in the Navisworks the
    /// window runs in, and put back exactly as it was when the run ends, made in a using around the run so a group that
    /// fails or a run that throws still puts it back. It reads the option general.autosave.enable through LcUOption, says
    /// what it read, and where it reads on keeps the registry's value on the record first, Core's AutoSaveKeeper, counts the
    /// switch as made, sets it off and reads it back. Dispose sets back what it read, reads it again and puts the registry
    /// back off the record, which deletes the record, each said in Core's words, AutoSaveSwitch. A value set in a session
    /// reaches no registry value while the session runs, scan.md 5z-zl, and what a Navisworks closed before Dispose writes
    /// into the registry as it quits is UNKNOWN, so the record stays for the next opening of the window,
    /// PutBackWhatARunLeft. Whether the option set off stops the autosaves of a session started with it on is UNKNOWN, row
    /// F223-R1. Every throw is said and the run goes on, since the switch never stops a run. The runner switches the
    /// registry for a group's own Navisworks, so in one this reads off already and changes nothing.
    /// </summary>
    public sealed class AutoSaveOff : IDisposable
    {
        private readonly Action<string> say;
        private readonly bool? before;
        private readonly bool changed;
        private readonly AutoSaveKeeper keeper;
        private bool disposed;

        public AutoSaveOff(Action<string> say)
        {
            this.say = say ?? delegate { };
            string why = null;
            bool? setReturned = null;
            bool? readBack = null;

            try
            {
                before = Read();

                if (before == null)
                {
                    why = "the option does not read as a boolean";
                }
                else if (before.Value)
                {
                    keeper = AutoSaveKeeper.Begin(new WindowsAutoSaveRegistry(), new FileAutoSaveRecord(AutoSaveKeeper.RecordPath()), this.say);

                    // Counted before the set, so a set that throws after it took is still put back, true over true doing no harm.
                    changed = true;
                    setReturned = LcUOption.SetBoolean(AutoSaveSwitch.OptionPath, false);
                    readBack = Read();
                }
            }
            catch (Exception error)
            {
                why = error.GetType().Name + ": " + error.Message;
            }

            this.say(AutoSaveSwitch.AsTheRunStarts(before, why, setReturned, readBack));
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            string why = null;
            bool? setReturned = null;
            bool? readBack = null;

            try
            {
                if (changed && before != null)
                {
                    setReturned = LcUOption.SetBoolean(AutoSaveSwitch.OptionPath, before.Value);
                }

                readBack = Read();
            }
            catch (Exception error)
            {
                why = error.GetType().Name + ": " + error.Message;
            }

            say(AutoSaveSwitch.AsTheRunEnds(before, changed, setReturned, readBack, why));

            if (keeper != null)
            {
                keeper.PutBack(false);
            }
        }

        /// <summary>
        /// The record a run left, put back into the registry and deleted, as the window opens and once a run of packages
        /// ends, and where it says the switch was on and this Navisworks reads it off, the option set back on too, so this
        /// session does not keep it off. Never throws.
        /// </summary>
        public static void PutBackWhatARunLeft(Action<string> say, bool aNavisworksOfTheRunStillRuns)
        {
            say = say ?? delegate { };
            IAutoSaveRecord record = new FileAutoSaveRecord(AutoSaveKeeper.RecordPath());
            string text = null;
            string before = null;
            string why;

            try
            {
                text = record.Read();
            }
            catch (Exception)
            {
                // The keeper reads it again below and says what threw.
            }

            bool held = text != null && AutoSaveKeeper.TryReadRecord(text, out before, out why);
            AutoSaveKeeper.PutBackWhatARunLeft(new WindowsAutoSaveRegistry(), record, say, aNavisworksOfTheRunStillRuns);

            if (!held)
            {
                return;
            }

            AutoSaveState state = AutoSaveSwitch.OfRegistryValue(before);

            if (state != AutoSaveState.On && state != AutoSaveState.NeverSet)
            {
                return;
            }

            try
            {
                if (Read() == false)
                {
                    bool returned = LcUOption.SetBoolean(AutoSaveSwitch.OptionPath, true);
                    say(AutoSaveSwitch.SessionPutBack(returned, Read()));
                }
            }
            catch (Exception error)
            {
                say(AutoSaveSwitch.SessionNotPutBack(error.GetType().Name + ": " + error.Message));
            }
        }

        private static bool? Read()
        {
            bool value = false;
            return LcUOption.TryGetBoolean(AutoSaveSwitch.OptionPath, ref value) ? value : (bool?)null;
        }
    }
}
