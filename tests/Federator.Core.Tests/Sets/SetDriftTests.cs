using System.Collections.Generic;
using Federator.Core.Report;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests.Sets
{
    /// <summary>
    /// Q72 answered a on 2026-09-20. A set already in the NWF keeps the question it was
    /// built with, so a value corrected in the picked file since then never reaches it.
    /// The values in these tests are the real ones, measured off his ten groups in 5w.
    /// </summary>
    [TestFixture]
    public class SetDriftTests
    {
        private const string Element = "LcRevitData_Element";
        private const string Category = "LcRevitPropertyElementCategory";
        private const string Workset = "lcldrevit_parameter_-1002053";

        private static ReadCondition Asked(string property, string test, string value)
        {
            return new ReadCondition(Element, property, test, value);
        }

        private static string Key(string property, string test, string value)
        {
            return Element + "|" + property + "|" + test + "|" + value;
        }

        /// <summary>
        /// The real one. 50 conditions across his groups ask ME-DUCTWORK and the
        /// corrected file asks ME-Ductwork, which the models actually carry.
        /// </summary>
        [Test]
        public void ASetAskingTheOldSpellingHasDrifted()
        {
            SetDrift drift = SetDrift.Compare(
                "a/path/BLD-ME-Ducts&Duct Fittings",
                new List<ReadCondition> { Asked(Workset, "equals", "ME-DUCTWORK") },
                new List<string> { Key(Workset, "equals", "ME-Ductwork") },
                new List<string> { "asks ME-Ductwork" });

            Assert.That(drift.Drifted, Is.True);
            Assert.That(drift.CouldNotRead, Is.False);
            Assert.That(drift.AskedNow(), Does.Contain("ME-DUCTWORK"));
            Assert.That(drift.WantedNow(), Does.Contain("ME-Ductwork"));
        }

        [Test]
        public void ASetAskingExactlyWhatTheFileAsksHasNotDrifted()
        {
            SetDrift drift = SetDrift.Compare(
                "a/path/BLD-AR-Floors",
                new List<ReadCondition> { Asked(Category, "equals", "Floors") },
                new List<string> { Key(Category, "equals", "Floors") },
                new List<string> { "asks Floors" });

            Assert.That(drift.Drifted, Is.False);
        }

        [Test]
        public void ASetWithADifferentNumberOfConditionsHasDrifted()
        {
            SetDrift drift = SetDrift.Compare(
                "a/path/BLD-AR-Floors",
                new List<ReadCondition> { Asked(Category, "equals", "Floors") },
                new List<string> { Key(Category, "equals", "Floors"), Key(Workset, "equals", "AR-EXTERIOR") },
                new List<string> { "asks Floors", "and a workset" });

            Assert.That(drift.Drifted, Is.True);
        }

        /// <summary>
        /// A search that will not read is never called drifted, the same way a census
        /// count that could not be taken is never called a move. Rebuilding on a read
        /// that failed would replace a set on no evidence at all.
        /// </summary>
        [Test]
        public void ASetWhoseSearchWouldNotReadIsNeverCalledDrifted()
        {
            SetDrift drift = SetDrift.Compare(
                "a/path/BLD-AR-Floors", null,
                new List<string> { Key(Category, "equals", "Floors") },
                new List<string> { "asks Floors" });

            Assert.That(drift.CouldNotRead, Is.True);
            Assert.That(drift.Drifted, Is.False);
            Assert.That(drift.AskedNow(), Does.Contain("UNKNOWN"));
        }

        [Test]
        public void TheTwoLinesNameThePathTheOldQuestionAndTheNewOne()
        {
            SetDrift drift = SetDrift.Compare(
                "lcop_selection_set_tree/Mechanical/BLD-ME-Ducts",
                new List<ReadCondition> { Asked(Workset, "equals", "ME-DUCTWORK") },
                new List<string> { Key(Workset, "equals", "ME-Ductwork") },
                new List<string> { "LcRevitData_Element/" + Workset + " equals \"ME-Ductwork\"" });

            IList<string> lines = drift.Lines();

            Assert.That(lines[0], Does.Contain("lcop_selection_set_tree/Mechanical/BLD-ME-Ducts"));
            Assert.That(lines[1], Does.Contain("it asks"));
            Assert.That(lines[1], Does.Contain("ME-DUCTWORK"));
            Assert.That(lines[2], Does.Contain("file asks"));
            Assert.That(lines[2], Does.Contain("ME-Ductwork"));
        }

        // ---------- the tick box ----------

        [Test]
        public void TheBoxIsOffByDefaultBecauseItChangesTheNwf()
        {
            Assert.That(SetRebuildSettings.DefaultRebuildDriftedSets, Is.False);
            Assert.That(new SetRebuildSettings().RebuildDriftedSets, Is.False);
            Assert.That(new ReportOptions().RebuildDriftedSets, Is.False);
        }

        [Test]
        public void TheLabelIsAtMostEightWordsAndTheHelpLineAtMostTwelve()
        {
            Assert.That(SetRebuildSettings.TickLabel.Split(' ').Length, Is.LessThanOrEqualTo(8),
                SetRebuildSettings.TickLabel);
            Assert.That(SetRebuildSettings.HelpLine.Split(' ').Length, Is.LessThanOrEqualTo(12),
                SetRebuildSettings.HelpLine);
        }

        /// <summary>
        /// The confirm line says nothing when the box is off, the same way the tolerance
        /// line does, because a sentence on every run saying nothing will happen teaches
        /// people to skip the screen.
        /// </summary>
        [Test]
        public void TheConfirmLineIsThereOnlyWhenTheBoxIsOn()
        {
            Assert.That(SetRebuildSettings.ConfirmLine(false), Is.Null);
            Assert.That(SetRebuildSettings.ConfirmLine(true), Does.Contain("REBUILT"));
            Assert.That(SetRebuildSettings.ConfirmLine(true), Does.Contain("keep their results"));
        }

        [Test]
        public void TheOutcomeCountsDriftedSetsApartFromRebuiltOnes()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();

            SetDrift one = SetDrift.Compare(
                "a", new List<ReadCondition> { Asked(Workset, "equals", "ME-DUCTWORK") },
                new List<string> { Key(Workset, "equals", "ME-Ductwork") }, new List<string> { "x" });

            outcome.AddDrift(one, true);
            outcome.AddDrift(one, false);

            Assert.That(outcome.Drifted.Count, Is.EqualTo(2), "both are drift");
            Assert.That(outcome.RebuiltCount, Is.EqualTo(1), "and only one was rebuilt");
        }

        private static SetBuildOutcome OutcomeWithOnePresentSet()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("a/BLD-ME-Ducts", "BLD-ME-Ducts", 1, 0);
            return outcome;
        }

        private static string SetsBlock(SetBuildOutcome outcome)
        {
            return string.Join("\n", new List<string>(outcome.Lines()).ToArray());
        }

        private static SetDrift OneDrift()
        {
            return SetDrift.Compare(
                "a/BLD-ME-Ducts", new List<ReadCondition> { Asked(Workset, "equals", "ME-DUCTWORK") },
                new List<string> { Key(Workset, "equals", "ME-Ductwork") }, new List<string> { "x" });
        }

        /// <summary>
        /// The SETS block used to say a set finding nothing MAY be asking a question the
        /// file no longer asks, because nothing had ever read the question. It is read
        /// now, so the block says what this run FOUND. Three states and a test each,
        /// because a block that reads the same whatever happened proves nothing.
        /// </summary>
        [Test]
        public void ThePresentSetsBlockSaysNoneDriftedWhenNoneDid()
        {
            string block = SetsBlock(OutcomeWithOnePresentSet());

            Assert.That(block, Does.Contain("none of them drifted"));
            Assert.That(block, Does.Not.Contain("DRIFTED and"));
        }

        [Test]
        public void ThePresentSetsBlockCountsTheDriftWhenTheBoxIsOff()
        {
            SetBuildOutcome outcome = OutcomeWithOnePresentSet();
            outcome.AddDrift(OneDrift(), false);

            string block = SetsBlock(outcome);

            Assert.That(block, Does.Contain("1 of them DRIFTED and none was rebuilt"));
            Assert.That(block, Does.Contain("because the box is off"));
            Assert.That(block, Does.Not.Contain("none of them drifted"));
        }

        [Test]
        public void ThePresentSetsBlockSaysHowManyWereRebuiltWhenTheBoxIsOn()
        {
            SetBuildOutcome outcome = OutcomeWithOnePresentSet();
            outcome.AddDrift(OneDrift(), true);
            outcome.AddDrift(OneDrift(), false);

            string block = SetsBlock(outcome);

            Assert.That(block, Does.Contain("2 of them DRIFTED and 1 were REBUILT"));
            Assert.That(block, Does.Contain("keep their results and their statuses, measured 5v"));
        }

        /// <summary>
        /// The whole block is under the already-there count, so a run that created every
        /// set fresh carries none of it. Nothing drifted, because nothing was there.
        /// </summary>
        [Test]
        public void ThereIsNoDriftBlockAtAllWhenNoSetWasAlreadyThere()
        {
            Assert.That(SetsBlock(new SetBuildOutcome()), Does.Not.Contain("drifted"));
        }
    }
}
