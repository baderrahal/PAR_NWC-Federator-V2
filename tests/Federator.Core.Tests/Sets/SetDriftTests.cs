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

        private static ReadCondition Asked(string property, string test, string value, int flags = 0)
        {
            return new ReadCondition(Element, property, test, value, flags);
        }

        /// <summary>One condition of the picked file, the way SetBuildPlan plans it.</summary>
        private static PlannedCondition Wants(string property, ConditionTest test, string value, int flags = 0)
        {
            return new PlannedCondition(test, flags, Element, "Element", property, null, "wstring", value);
        }

        /// <summary>A set of the picked file at that path, asking those conditions in that order.</summary>
        private static PlannedSet Planned(string path, params PlannedCondition[] conditions)
        {
            string name = path.Substring(path.LastIndexOf('/') + 1);
            return new PlannedSet(name, path, new List<string>(), new List<PlannedCondition>(conditions));
        }

        /// <summary>
        /// The real one. 50 conditions across his groups ask ME-DUCTWORK and the
        /// corrected file asks ME-Ductwork, which the models actually carry.
        /// </summary>
        [Test]
        public void ASetAskingTheOldSpellingHasDrifted()
        {
            SetDrift drift = SetDrift.Compare(
                new List<ReadCondition> { Asked(Workset, "equals", "ME-DUCTWORK") },
                Planned("a/path/BLD-ME-Ducts&Duct Fittings", Wants(Workset, ConditionTest.Equals, "ME-Ductwork")));

            Assert.That(drift.Drifted, Is.True);
            Assert.That(drift.CouldNotRead, Is.False);
            Assert.That(drift.AskedNow(), Does.Contain("ME-DUCTWORK"));
            Assert.That(drift.WantedNow(), Does.Contain("ME-Ductwork"));
        }

        [Test]
        public void ASetAskingExactlyWhatTheFileAsksHasNotDrifted()
        {
            SetDrift drift = SetDrift.Compare(
                new List<ReadCondition> { Asked(Category, "equals", "Floors") },
                Planned("a/path/BLD-AR-Floors", Wants(Category, ConditionTest.Equals, "Floors")));

            Assert.That(drift.Drifted, Is.False);
        }

        [Test]
        public void ASetWithADifferentNumberOfConditionsHasDrifted()
        {
            SetDrift drift = SetDrift.Compare(
                new List<ReadCondition> { Asked(Category, "equals", "Floors") },
                Planned(
                    "a/path/BLD-AR-Floors",
                    Wants(Category, ConditionTest.Equals, "Floors"),
                    Wants(Workset, ConditionTest.Equals, "AR-EXTERIOR")));

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
                null, Planned("a/path/BLD-AR-Floors", Wants(Category, ConditionTest.Equals, "Floors")));

            Assert.That(drift.CouldNotRead, Is.True);
            Assert.That(drift.Drifted, Is.False);
            Assert.That(drift.AskedNow(), Does.Contain("UNKNOWN"));
        }

        [Test]
        public void TheTwoLinesNameThePathTheOldQuestionAndTheNewOne()
        {
            SetDrift drift = SetDrift.Compare(
                new List<ReadCondition> { Asked(Workset, "equals", "ME-DUCTWORK") },
                Planned(
                    "lcop_selection_set_tree/Mechanical/BLD-ME-Ducts",
                    Wants(Workset, ConditionTest.Equals, "ME-Ductwork")));

            IList<string> lines = drift.Lines();

            Assert.That(lines[0], Does.Contain("lcop_selection_set_tree/Mechanical/BLD-ME-Ducts"));
            Assert.That(lines[1], Does.Contain("it asks"));
            Assert.That(lines[1], Does.Contain("ME-DUCTWORK"));
            Assert.That(lines[2], Does.Contain("file asks"));
            Assert.That(lines[2], Does.Contain("ME-Ductwork"));
        }

        // ---------- what is part of the question, FR-015 ----------

        /// <summary>
        /// BLD-ME-Ducts&amp;Duct Fittings as the file asks it, two groups of two, the third
        /// condition starting the second with flags 64, F78. The same four conditions in one
        /// group ask an And that no element answers, so a set carrying them so has drifted.
        /// The key carried no flag and called the two the same question.
        /// </summary>
        [Test]
        public void ASetMissingTheFilesOrGroupHasDrifted()
        {
            SetDrift drift = SetDrift.Compare(
                new List<ReadCondition>
                {
                    Asked(Category, "equals", "Ducts"),
                    Asked(Workset, "equals", "ME-Ductwork"),
                    Asked(Category, "equals", "Duct Fittings"),
                    Asked(Workset, "equals", "ME-Ductwork")
                },
                Planned(
                    "a/Mechanical/BLD-ME-Ducts&Duct Fittings",
                    Wants(Category, ConditionTest.Equals, "Ducts"),
                    Wants(Workset, ConditionTest.Equals, "ME-Ductwork"),
                    Wants(Category, ConditionTest.Equals, "Duct Fittings", PlannedCondition.StartGroupFlag),
                    Wants(Workset, ConditionTest.Equals, "ME-Ductwork")));

            Assert.That(drift.Drifted, Is.True);
        }

        /// <summary>
        /// A condition and its negation, flags 32, ask opposite questions. BLD-EL-Devices asks
        /// contains Devices and NOT each Devices category a sibling set claims, 5g.
        /// </summary>
        [Test]
        public void ASetDifferingOnlyByANegationHasDrifted()
        {
            SetDrift drift = SetDrift.Compare(
                new List<ReadCondition>
                {
                    Asked(Category, "contains", "Devices"),
                    Asked(Category, "equals", "Telephone Devices")
                },
                Planned(
                    "a/Electrical/BLD-EL-Devices",
                    Wants(Category, ConditionTest.Contains, "Devices"),
                    Wants(Category, ConditionTest.Equals, "Telephone Devices", PlannedCondition.NegateFlag)));

            Assert.That(drift.Drifted, Is.True);
        }

        /// <summary>
        /// THE IGNORE BITS ARE NOT PART OF THE QUESTION. A set this tool built reads them on every
        /// condition, 37 for a negated one, 5g, and 1A02MM's original import reads none, 5w, and
        /// those sets find the same items. Comparing every bit would rebuild 61 sets over nothing.
        /// </summary>
        [Test]
        public void TheIgnoreBitsASetCarriesAreNotADrift()
        {
            const int IgnoreDisplayNames = 5;

            SetDrift drift = SetDrift.Compare(
                new List<ReadCondition>
                {
                    Asked(Category, "contains", "Devices", IgnoreDisplayNames),
                    Asked(Category, "equals", "Telephone Devices", PlannedCondition.NegateFlag | IgnoreDisplayNames)
                },
                Planned(
                    "a/Electrical/BLD-EL-Devices",
                    Wants(Category, ConditionTest.Contains, "Devices"),
                    Wants(Category, ConditionTest.Equals, "Telephone Devices", PlannedCondition.NegateFlag)));

            Assert.That(drift.Drifted, Is.False);
        }

        /// <summary>
        /// A comparison other than the two the file writes keeps its own name and is a different
        /// question. The add-in read every comparison but contains as equals, so a set asking
        /// NotEqual read as the same set as one asking Equal. Its half of this is in SetBuilder.
        /// </summary>
        [Test]
        public void AComparisonOtherThanEqualsAndContainsIsADifferentQuestion()
        {
            SetDrift drift = SetDrift.Compare(
                new List<ReadCondition> { Asked(Category, "NotEqual", "Floors") },
                Planned("a/path/BLD-AR-Floors", Wants(Category, ConditionTest.Equals, "Floors")));

            Assert.That(drift.Drifted, Is.True);
        }

        /// <summary>
        /// The file's side and the document's side are put in one key by one rule in Core, so a
        /// condition read back exactly as it was built is the same question. The add-in built the
        /// file's key a second time, with no flags.
        /// </summary>
        [Test]
        public void AConditionReadBackAsItWasBuiltHasTheSameKey()
        {
            PlannedCondition wanted = Wants(Category, ConditionTest.Equals, "Duct Fittings", PlannedCondition.StartGroupFlag);
            ReadCondition read = Asked(Category, "equals", "Duct Fittings", PlannedCondition.StartGroupFlag | 5);

            Assert.That(read.Key(), Is.EqualTo(wanted.Key()));
            Assert.That(
                Asked(Category, "equals", "Duct Fittings").Key(), Is.Not.EqualTo(wanted.Key()),
                "the same condition without the group bit is another question");
        }

        // ---------- what the lines say, FR-016 ----------

        /// <summary>
        /// BLD-ME-Ducts&amp;Duct Fittings, an Or of two groups, read off the document and asked by
        /// the file. Both lines say the two bracketed groups joined by or, the way F78 made a
        /// created set's line read. They joined every condition with and, a four way And no
        /// element can answer, in the SET DRIFT lines and the SETS ACROSS THE RUN line.
        /// </summary>
        [Test]
        public void BothSidesOfAnOrSetReadAsItsGroupsJoinedByOr()
        {
            SetDrift drift = SetDrift.Compare(
                new List<ReadCondition>
                {
                    Asked(Category, "equals", "Ducts"),
                    Asked(Workset, "equals", "ME-DUCTWORK"),
                    Asked(Category, "equals", "Duct Fittings", PlannedCondition.StartGroupFlag),
                    Asked(Workset, "equals", "ME-DUCTWORK")
                },
                Planned(
                    "a/Mechanical/BLD-ME-Ducts&Duct Fittings",
                    Wants(Category, ConditionTest.Equals, "Ducts"),
                    Wants(Workset, ConditionTest.Equals, "ME-Ductwork"),
                    Wants(Category, ConditionTest.Equals, "Duct Fittings", PlannedCondition.StartGroupFlag),
                    Wants(Workset, ConditionTest.Equals, "ME-Ductwork")));

            Assert.That(
                drift.AskedNow(),
                Is.EqualTo("(" + Element + "/" + Category + " equals \"Ducts\" and " + Element + "/" + Workset + " equals \"ME-DUCTWORK\")"
                    + " or (" + Element + "/" + Category + " equals \"Duct Fittings\" and " + Element + "/" + Workset + " equals \"ME-DUCTWORK\")"));
            Assert.That(drift.WantedNow(), Does.StartWith("(").And.Contain("\") or (").And.EndWith("\"ME-Ductwork\")"));
            Assert.That(drift.WantedNow(), Does.Not.Contain("\" and " + Element + "/" + Category + " equals \"Duct Fittings\""));
        }

        /// <summary>
        /// A negated condition says not. Since FR-015 a condition and its negation are two
        /// questions, and the two lines of a set that drifted by a negation read the same.
        /// </summary>
        [Test]
        public void ANegatedConditionReadsAsNotOnBothSides()
        {
            SetDrift drift = SetDrift.Compare(
                new List<ReadCondition>
                {
                    Asked(Category, "contains", "Devices"),
                    Asked(Category, "equals", "Telephone Devices")
                },
                Planned(
                    "a/Electrical/BLD-EL-Devices",
                    Wants(Category, ConditionTest.Contains, "Devices"),
                    Wants(Category, ConditionTest.Equals, "Telephone Devices", PlannedCondition.NegateFlag)));

            Assert.That(drift.WantedNow(), Does.Contain(Category + " not equals \"Telephone Devices\""));
            Assert.That(drift.AskedNow(), Does.Contain(Category + " equals \"Telephone Devices\""));
            Assert.That(drift.AskedNow(), Does.Not.Contain(" not "));
        }

        /// <summary>A comparison the file never writes is said by its own name and never as equals.</summary>
        [Test]
        public void AnotherComparisonIsSaidByItsOwnName()
        {
            SetDrift drift = SetDrift.Compare(
                new List<ReadCondition> { Asked(Category, "NotEqual", "Floors") },
                Planned("a/path/BLD-AR-Floors", Wants(Category, ConditionTest.Equals, "Floors")));

            Assert.That(drift.AskedNow(), Is.EqualTo(Element + "/" + Category + " NotEqual \"Floors\""));
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
                new List<ReadCondition> { Asked(Workset, "equals", "ME-DUCTWORK") },
                Planned("a", Wants(Workset, ConditionTest.Equals, "ME-Ductwork")));

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
                new List<ReadCondition> { Asked(Workset, "equals", "ME-DUCTWORK") },
                Planned("a/BLD-ME-Ducts", Wants(Workset, ConditionTest.Equals, "ME-Ductwork")));
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
