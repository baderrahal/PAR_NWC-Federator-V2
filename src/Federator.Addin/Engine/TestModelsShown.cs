using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Report;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// ONLY THE TEST'S MODELS SHOWN FOR ITS PICTURES, F145, Bader's choice of Q150 on 2026-10-11.
    /// Before a test's pictures every model its two sides do not live in is hidden once, and after
    /// them the hidden state the document held before the first hide is put back, whatever
    /// happened, at the same size and style. Measured on the first 20 clashes of
    /// BLD-AR-Walls-vs-BLD-AR-Floors in 1A04PK, docs\history\scan.md 5z-zc: 7.76 s a picture with
    /// every model shown and 1.78 s with only the test's.
    ///
    /// The sides are the ones the test itself holds, read off a resolved test by Read, each item
    /// walked to its model by SavedViewpoints.ModelFileOf, so a test that keeps sides other than the
    /// file's plan, as a test found in the document does, is pictured with the models its clashes
    /// live in. The caller releases that test before Hide, and resolves the one it takes the
    /// pictures through after, so no clash handle is held across the hide, the order the probe of
    /// 5z-zc ran in. Which models stay shown is PictureModels' rule in Core, and every doubt keeps
    /// every model shown and says why. Hide adds hiding to the other models and leaves every hide
    /// the document held, and the state is read once before the first hide by
    /// SavedViewpoints.SnapshotHidden, measured in 5k, and put back after each test by
    /// RestoreHiddenState. The runner makes one for its clash step and disposes it at the end, which
    /// puts back any state still changed, releases the snapshot and writes the one IMAGES line.
    /// </summary>
    internal sealed class TestModelsShown : IDisposable
    {
        private readonly Document document;
        private readonly RunLog log;
        private readonly bool on;
        private readonly PictureModelsTally tally = new PictureModelsTally();
        private Dictionary<string, int> indexOfFile;
        private HashSet<string> sharedFiles;
        private HiddenSnapshot snapshot;
        private bool needsPutBack;
        private bool stopped;

        internal TestModelsShown(Document document, RunLog log, bool on)
        {
            this.document = document;
            this.log = log;
            this.on = on;
        }

        /// <summary>
        /// Which models the test's own two sides live in, read off the test handed in, or null where
        /// the setting is off, the hiding has stopped or the sides could not be read. A rule that
        /// keeps every model shown is said on an IMAGES line with why.
        /// </summary>
        internal PictureModels Read(string testName, ClashTest test)
        {
            if (!on || stopped || document == null || test == null)
            {
                return null;
            }

            Stopwatch clock = Stopwatch.StartNew();
            PictureModels rule;

            try
            {
                List<PictureSide> sides = new List<PictureSide>();

                using (ClashSelection left = test.SelectionA)
                {
                    sides.Add(SideOf(left));
                }

                using (ClashSelection right = test.SelectionB)
                {
                    sides.Add(SideOf(right));
                }

                rule = PictureModels.For(document.Models.Count, sides);
            }
            catch (Exception error)
            {
                tally.SidesRead(clock.Elapsed.TotalSeconds);
                tally.KeptEvery();
                log.Failure(
                    "reading which models the sides of " + testName + " live in",
                    error,
                    "kept going, its pictures are taken with every model shown");
                return null;
            }

            tally.SidesRead(clock.Elapsed.TotalSeconds);

            if (!rule.HidesAny)
            {
                tally.KeptEvery();
                log.NumberedRepeat(
                    "IMAGES every model shown for a test",
                    "IMAGES   " + testName + ": " + rule.WhyEveryModel,
                    "pictures models",
                    testName,
                    EventRow.Count(0),
                    rule.WhyEveryModel);
            }

            return rule;
        }

        /// <summary>
        /// Hides every model the rule does not keep and returns what puts the hidden state back when
        /// disposed, or null where nothing was hidden.
        /// </summary>
        internal IDisposable Hide(string testName, PictureModels rule)
        {
            if (rule == null || !rule.HidesAny || stopped)
            {
                return null;
            }

            Stopwatch clock = Stopwatch.StartNew();

            if (snapshot == null)
            {
                try
                {
                    snapshot = SavedViewpoints.SnapshotHidden(document);
                }
                catch (Exception error)
                {
                    stopped = true;
                    tally.KeptEvery();
                    log.Failure(
                        "reading the hidden state before the first test's pictures",
                        error,
                        "kept going, no model is hidden for any picture of this clash step, so every test's pictures keep every model shown");
                    return null;
                }
            }

            try
            {
                needsPutBack = true;
                int hidden = SavedViewpoints.HideTheOtherModels(document, rule.Shown);
                return new PutBack(this, testName, hidden, clock.Elapsed.TotalSeconds);
            }
            catch (Exception error)
            {
                tally.KeptEvery();
                log.Failure(
                    "hiding the models other than those of " + testName + " for its pictures",
                    error,
                    "kept going, the hidden state is put back and its pictures are taken with every model the document shows");
                PutTheStateBack(testName);
                return null;
            }
        }

        public void Dispose()
        {
            try
            {
                if (needsPutBack && !PutTheStateBack("the last test, tried again"))
                {
                    log.Line("IMAGES   the hidden state could not be put back after the pictures, so the views step, the NWF and the NWD carry what is hidden now");
                }

                log.Line(tally.Line(on));
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

        /// <summary>
        /// Puts the hidden state back, true where it read back. One that fails is said and left marked,
        /// so the end of the clash step tries it again.
        /// </summary>
        private bool PutTheStateBack(string testName)
        {
            if (snapshot == null)
            {
                needsPutBack = false;
                return true;
            }

            try
            {
                if (!SavedViewpoints.RestoreHiddenState(document, snapshot))
                {
                    log.Line("IMAGES   after the pictures of " + testName + " the items hidden before them do not read back as hidden");
                }

                needsPutBack = false;
                return true;
            }
            catch (Exception error)
            {
                log.Failure(
                    "putting the hidden state back after the pictures of " + testName,
                    error,
                    "kept going, it is tried again at the end of the clash step");
                return false;
            }
        }

        /// <summary>What one side of the test was read to, or a side not read.</summary>
        private PictureSide SideOf(ClashSelection side)
        {
            if (side == null)
            {
                return PictureSide.NotRead();
            }

            IndexTheFiles();

            using (Selection selection = side.Selection)
            using (ModelItemCollection found = selection.GetSelectedItems(document))
            {
                if (found == null)
                {
                    return PictureSide.NotRead();
                }

                HashSet<int> models = new HashSet<int>();
                int read = 0;
                int noModel = 0;
                int shared = 0;

                // The items are enumerated and not disposed one by one, as the read of the hidden
                // items does, since disposing each item enumerated off a collection threw on every
                // clash of F85's fifth run. The collection is released whole by the using.
                foreach (ModelItem item in found)
                {
                    read++;
                    string file = SavedViewpoints.ModelFileOf(item);
                    int index;

                    if (file == null)
                    {
                        noModel++;
                    }
                    else if (sharedFiles.Contains(file))
                    {
                        shared++;
                    }
                    else if (indexOfFile.TryGetValue(file, out index))
                    {
                        models.Add(index);
                    }
                    else
                    {
                        noModel++;
                    }
                }

                return new PictureSide(models, read, noModel, shared);
            }
        }

        /// <summary>Each model's index by its file name, read once, and every file name two models share.</summary>
        private void IndexTheFiles()
        {
            if (indexOfFile != null)
            {
                return;
            }

            indexOfFile = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            sharedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < document.Models.Count; i++)
            {
                using (Model model = document.Models[i])
                {
                    string file = model.FileName ?? string.Empty;

                    if (indexOfFile.ContainsKey(file))
                    {
                        sharedFiles.Add(file);
                    }
                    else
                    {
                        indexOfFile.Add(file, i);
                    }
                }
            }
        }

        /// <summary>What a test's hiding hands back: disposed once its pictures are taken, it puts the hidden state back and counts the test.</summary>
        private sealed class PutBack : IDisposable
        {
            private readonly TestModelsShown owner;
            private readonly string testName;
            private readonly int hidden;
            private readonly double hidingSeconds;
            private bool done;

            internal PutBack(TestModelsShown owner, string testName, int hidden, double hidingSeconds)
            {
                this.owner = owner;
                this.testName = testName;
                this.hidden = hidden;
                this.hidingSeconds = hidingSeconds;
            }

            public void Dispose()
            {
                if (done)
                {
                    return;
                }

                done = true;
                Stopwatch back = Stopwatch.StartNew();
                owner.PutTheStateBack(testName);
                owner.tally.Hid(hidden, hidingSeconds + back.Elapsed.TotalSeconds);
            }
        }
    }
}
