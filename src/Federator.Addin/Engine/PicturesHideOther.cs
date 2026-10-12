using System;
using Autodesk.Navisworks.Api.Interop;
using Federator.Core.Diagnostics;
using Federator.Core.Report;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// CLASH DETECTIVE'S HIDE OTHER FOR THE PICTURE PASS, F150, Bader's answer to Q151 and his order
    /// of 2026-10-12. Made at the start of the PICTURES step and disposed at its end, on every path:
    /// it reads the user's Hide Other, sets it on where OtherItemsInPictures says so, and on Dispose
    /// puts the user's value back, reads it once more and writes the one IMAGES line. The rule and
    /// the line are Core's, OtherItemsInPictures. Never set where ImageOptions.GhostTheOtherItems is
    /// on. Every throw is a failure line and the pass goes on with the user's own setting.
    ///
    /// WHY THIS WAY, docs\history\scan.md 5z-zf. The option path,
    /// LcUOption.SetBoolean("tools.clash.displaysettings.hide_other", true), read back True inside a
    /// running Navisworks and the pictures did not follow, 547 KB each way. So the value is set on
    /// Clash Detective's own highlighter, the way read off the DLLs and not yet run:
    /// LcOpState.GetActiveInstance(), static in Autodesk.Navisworks.Api.dll, then
    /// LcClClashResultHighlighter.GetInstance(state), static in Autodesk.Navisworks.Clash.dll, then
    /// its DisplaySettings, an LcClClashDisplaySettings with HideOther. None of them is in the
    /// install's Api.xml, so each is undocumented. Whether the picture call draws through this
    /// highlighter is UNKNOWN until the 1A02MM check shows picture files near 48 KB.
    ///
    /// THE HANDLES. Each of the three is a NativeHandle, and each is made with ownership eEXTERNAL,
    /// read off the IL of GetActiveInstance, GetInstance and GetDisplaySettings on 2026-10-12, so
    /// disposing one releases the wrapper and never the native object. All three are made, used and
    /// disposed inside one read or one write, and none is kept across the pass.
    /// </summary>
    internal sealed class PicturesHideOther : IDisposable
    {
        private readonly RunLog log;
        private readonly OtherItemsInPictures rule;
        private bool ended;

        internal PicturesHideOther(RunLog log, bool ghostTheOtherItems)
        {
            this.log = log;
            this.rule = new OtherItemsInPictures(ghostTheOtherItems);

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
                Write(true);
                rule.Set();
            }
            catch (Exception error)
            {
                rule.SetThrew(error.Message);
                log.Failure(
                    "setting Clash Detective's Hide Other on for the pictures",
                    error,
                    "kept going, the pictures follow the user's own setting where the set did not reach it, and the user's value is put back after them");
            }
        }

        /// <summary>Puts the user's value back where a set was tried, reads it once more and writes the IMAGES line. Once.</summary>
        public void Dispose()
        {
            if (ended)
            {
                return;
            }

            ended = true;

            if (rule.PutsBack)
            {
                try
                {
                    Write(rule.PutBackTo);
                }
                catch (Exception error)
                {
                    rule.PutBackThrew(error.Message);
                    log.Failure(
                        "putting Clash Detective's Hide Other back after the pictures",
                        error,
                        "kept going, the IMAGES line after this says what it reads now and where the user sets it back");
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
