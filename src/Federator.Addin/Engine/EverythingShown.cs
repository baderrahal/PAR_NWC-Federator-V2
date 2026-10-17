using System;
using Autodesk.Navisworks.Api;
using Federator.Core.Diagnostics;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// EVERY ITEM SHOWN FOR ONE STEP AND HIDDEN AGAIN AFTER IT, F204 for the clash step and F206 for the picture pass taken
    /// after the NWF is saved. Clash Detective finds no clash on a hidden item, and a picture shows nothing of one. Made, it
    /// reads the hidden state by the snapshot the viewpoints use, SavedViewpoints.SnapshotHidden, counts the models whose
    /// root item is hidden, says what it found on one line and shows everything where anything was hidden. Disposed, it puts
    /// the snapshot back by SavedViewpoints.RestoreHiddenState, read back and said. Neither the showing nor the putting back
    /// stops the run. The words are the step's own, Federator.Core.Clash.HiddenForTheClash's or HiddenForThePictures'.
    /// </summary>
    internal sealed class EverythingShown : IDisposable
    {
        private readonly Document document;
        private readonly RunLog log;
        private readonly string step;
        private readonly Func<bool, int, string> putBack;
        private readonly Func<string, string, string> threw;
        private HiddenSnapshot snapshot;
        private bool shown;

        /// <param name="step">The step in the words of a NOTICE, the clash or the pictures.</param>
        internal EverythingShown(
            Document document,
            RunLog log,
            string step,
            Func<int, int, int, string> found,
            Func<bool, int, string> putBack,
            Func<string, string, string> threw)
        {
            this.document = document;
            this.log = log;
            this.step = step;
            this.putBack = putBack;
            this.threw = threw;

            try
            {
                if (document == null)
                {
                    return;
                }

                snapshot = SavedViewpoints.SnapshotHidden(document);
                int hiddenModels = 0;

                foreach (Model model in document.Models)
                {
                    using (ModelItem root = model.RootItem)
                    {
                        if (root != null && root.IsHidden)
                        {
                            hiddenModels++;
                        }
                    }
                }

                log.Line(found(snapshot.HiddenCount, hiddenModels, document.Models.Count));

                if (snapshot.HiddenCount > 0 || hiddenModels > 0)
                {
                    document.Models.ResetAllHidden();
                    shown = true;
                }
            }
            catch (Exception error)
            {
                log.Line(threw("showing every item for " + step, error.Message));
            }
        }

        public void Dispose()
        {
            try
            {
                if (shown)
                {
                    shown = false;

                    try
                    {
                        log.Line(putBack(SavedViewpoints.RestoreHiddenState(document, snapshot), snapshot.HiddenCount));
                    }
                    catch (Exception error)
                    {
                        log.Line(threw("hiding again what was hidden before " + step, error.Message));
                    }
                }
            }
            finally
            {
                if (snapshot != null)
                {
                    snapshot.Dispose();
                    snapshot = null;
                }
            }
        }
    }
}
