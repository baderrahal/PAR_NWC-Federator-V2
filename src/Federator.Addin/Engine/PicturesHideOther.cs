using System;
using Autodesk.Navisworks.Api.Interop;
using Federator.Core.Diagnostics;
using Federator.Core.Report;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// CLASH DETECTIVE'S HIDE OTHER FOR ONE GROUP'S PICTURE PASS, F150, Bader's answer to Q151 and his
    /// order of 2026-10-12. Made around the pass inside the PICTURES step and disposed at its end on
    /// every path. It touches nothing until Begin, which the pass calls once a stretch takes a picture,
    /// the breaker's fifth point on attempt 1, so a group that takes none reads and sets nothing. Begin
    /// reads the user's Hide Other, sets it to the value asked for where OtherItemsInPictures says so,
    /// on where ImageOptions.GhostTheOtherItems is unticked and off where it is ticked, and reads it back
    /// at once. Dispose puts back the user's value as the run's first group read it, reads it once more
    /// and writes the one IMAGES line. Every throw is a failure line and the pass goes on. A put back
    /// still owed when the run ends is tried once more by AtTheEnd. The rule and the lines are Core's.
    ///
    /// WHY THIS WAY, docs\history\scan.md 5z-zf. The option path,
    /// LcUOption.SetBoolean("tools.clash.displaysettings.hide_other", true), read back True inside a
    /// running Navisworks and the pictures did not follow, 547 KB each way. So the value is set on
    /// Clash Detective's own highlighter, read off the DLLs and not yet run:
    /// LcOpState.GetActiveInstance(), static in Autodesk.Navisworks.Api.dll, then
    /// LcClClashResultHighlighter.GetInstance(state), static in Autodesk.Navisworks.Clash.dll, then
    /// its DisplaySettings, an LcClClashDisplaySettings with HideOther. None of them is in the
    /// install's Api.xml. Whether the picture call draws through this highlighter is UNKNOWN until a
    /// run's picture sizes show it, and the IMAGES line says so.
    ///
    /// THE HANDLES. Each of the three is a NativeHandle made with ownership eEXTERNAL, read off the IL
    /// of GetActiveInstance, GetInstance and GetDisplaySettings on 2026-10-12, scan.md 5z-zf, so
    /// disposing one releases the wrapper and never the native object. All three are made, used and
    /// disposed inside one read or one write, and none is kept across the pass.
    /// </summary>
    internal sealed class PicturesHideOther : IDisposable
    {
        private readonly RunLog log;
        private readonly OtherItemsInPictures rule;
        private bool begun;
        private bool ended;

        internal PicturesHideOther(RunLog log, bool ghostTheOtherItems, HideOtherAcrossTheRun run)
        {
            this.log = log;
            this.rule = new OtherItemsInPictures(ghostTheOtherItems, run);
        }

        /// <summary>Reads, sets and reads back, once, before the first picture of the pass.</summary>
        internal void Begin()
        {
            if (begun || ended)
            {
                return;
            }

            begun = true;

            try
            {
                rule.Read(Read());
            }
            catch (Exception error)
            {
                rule.NotRead(error.Message);
                log.Failure(
                    "reading Clash Detective's Hide Other before the pictures",
                    error,
                    "kept going, nothing is set and the pictures follow the user's own setting");
                return;
            }

            if (!rule.SetsIt)
            {
                return;
            }

            try
            {
                Write(rule.Wanted);
                rule.Set();
            }
            catch (Exception error)
            {
                rule.SetThrew(error.Message);
                log.Failure(
                    "setting Clash Detective's Hide Other for the pictures",
                    error,
                    "kept going, it is read back now and the user's value is put back after the pictures");
            }

            try
            {
                rule.ReadBack(Read());
            }
            catch (Exception error)
            {
                rule.NotReadBack(error.Message);
                log.Failure(
                    "reading Clash Detective's Hide Other back after setting it",
                    error,
                    "kept going, the IMAGES line after the pictures says it could not be read back");
            }
        }

        /// <summary>Puts the user's value back where the rule says so, reads it once more and writes the IMAGES line. Once.</summary>
        public void Dispose()
        {
            if (ended)
            {
                return;
            }

            ended = true;

            if (!begun)
            {
                log.Line(rule.Line());
                return;
            }

            if (rule.PutsBack)
            {
                try
                {
                    Write(rule.PutBackTo);
                    rule.PutBack();
                }
                catch (Exception error)
                {
                    rule.PutBackThrew(error.Message);
                    log.Failure(
                        "putting Clash Detective's Hide Other back after the pictures",
                        error,
                        "kept going, it is tried once more at the end of the run and the group carries the error");
                }
            }

            try
            {
                rule.ReadAfter(Read());
            }
            catch (Exception error)
            {
                rule.NotReadAfter(error.Message);
                log.Failure(
                    "reading Clash Detective's Hide Other after the pictures",
                    error,
                    "kept going, the IMAGES line after this says the user's setting is UNKNOWN");
            }

            log.Line(rule.Line());
        }

        /// <summary>
        /// The put back a group of the run left owed, tried once more when the run ends, the breaker's
        /// second point on attempt 1, with its one IMAGES line. Nothing where none is owed.
        /// </summary>
        internal static void AtTheEnd(RunLog log, HideOtherAcrossTheRun run)
        {
            if (log == null || run == null || !run.OwesAPutBack)
            {
                return;
            }

            string whyItThrew = null;
            bool? after = null;
            string whyNotRead = null;

            try
            {
                Write(run.UsersValue.Value);
            }
            catch (Exception error)
            {
                whyItThrew = error.Message;
                log.Failure(
                    "putting Clash Detective's Hide Other back at the end of the run",
                    error,
                    "kept going, the IMAGES line after this says where the user sets it back");
            }

            try
            {
                after = Read();
            }
            catch (Exception error)
            {
                whyNotRead = error.Message;
                log.Failure(
                    "reading Clash Detective's Hide Other at the end of the run",
                    error,
                    "kept going, the IMAGES line after this says the user's setting is UNKNOWN");
            }

            log.Line(run.AtTheEnd(whyItThrew, after, whyNotRead));
        }

        /// <summary>Hide Other as Clash Detective's highlighter holds it now.</summary>
        private static bool Read()
        {
            using (LcOpState state = LcOpState.GetActiveInstance())
            using (LcClClashResultHighlighter highlighter = LcClClashResultHighlighter.GetInstance(state))
            using (LcClClashDisplaySettings settings = highlighter.DisplaySettings)
            {
                return settings.HideOther;
            }
        }

        /// <summary>Sets Hide Other on Clash Detective's highlighter.</summary>
        private static void Write(bool hideOther)
        {
            using (LcOpState state = LcOpState.GetActiveInstance())
            using (LcClClashResultHighlighter highlighter = LcClClashResultHighlighter.GetInstance(state))
            using (LcClClashDisplaySettings settings = highlighter.DisplaySettings)
            {
                settings.HideOther = hideOther;
            }
        }
    }
}
