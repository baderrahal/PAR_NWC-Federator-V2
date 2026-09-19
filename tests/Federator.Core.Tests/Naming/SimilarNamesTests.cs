using System;
using System.Collections.Generic;
using Federator.Core.Naming;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The near miss rule, F71.
    ///
    /// Bader pressed Run on buildings that already had an NWF and got First run, because
    /// the NWF folder and the name pattern together did not resolve to his file. The rule
    /// here is what notices that, and the two things worth more than the rest are that it
    /// never reports an exact match, which is a different case the engine already handles,
    /// and that it names WHICH field differs, because a sentence saying only that
    /// something is similar sends the reader back to the folder.
    /// </summary>
    [TestFixture]
    public class SimilarNamesTests
    {
        private const string Wanted = "1104-PAR-1B06PH-ZZZ-BM-MOD-000001";

        private static ContainerNameSettings Settings()
        {
            return new ContainerNameSettings();
        }

        private static IList<SimilarNames.NearbyName> In(params string[] names)
        {
            return SimilarNames.In(Wanted, names, Settings());
        }

        [Test]
        public void AnEmptyFolderIsTheOrdinaryAnswerAndIsNeverNull()
        {
            IList<SimilarNames.NearbyName> found = In();

            Assert.That(found, Is.Not.Null);
            Assert.That(found.Count, Is.EqualTo(0));
        }

        [Test]
        public void NoNamesAtAllIsNotAFailure()
        {
            Assert.That(SimilarNames.In(Wanted, null, Settings()).Count, Is.EqualTo(0));
            Assert.That(SimilarNames.In(null, new[] { Wanted }, Settings()).Count, Is.EqualTo(0));
            Assert.That(SimilarNames.In(string.Empty, new[] { Wanted }, Settings()).Count, Is.EqualTo(0));
        }

        [Test]
        public void SettingsAreOptionalAndTheDefaultsAreUsed()
        {
            IList<SimilarNames.NearbyName> found =
                SimilarNames.In(Wanted, new[] { "1104-PAR-1B06PH-L01-BM-MOD-000001" }, null);

            Assert.That(found.Count, Is.EqualTo(1));
        }

        // ---------- the exact match is not a near miss ----------

        [Test]
        public void TheExactNameIsNotReported()
        {
            Assert.That(In(Wanted).Count, Is.EqualTo(0));
        }

        [Test]
        public void TheExactNameWithAnExtensionIsNotReported()
        {
            Assert.That(In(Wanted + ".nwf").Count, Is.EqualTo(0));
        }

        [Test]
        public void TheExactNameInAnotherCaseIsNotReported()
        {
            Assert.That(In(Wanted.ToLowerInvariant()).Count, Is.EqualTo(0));
        }

        // ---------- the four supplied fields ----------

        [Test]
        public void ADifferentLevelIsCloseAndTheLevelIsNamed()
        {
            IList<SimilarNames.NearbyName> found = In("1104-PAR-1B06PH-L01-BM-MOD-000001");

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].How, Is.EqualTo(SimilarNames.Closeness.SuppliedFieldsDiffer));
            Assert.That(found[0].Reason, Does.Contain("level"));
        }

        [Test]
        public void ADifferentDisciplineIsCloseAndTheDisciplineIsNamed()
        {
            IList<SimilarNames.NearbyName> found = In("1104-PAR-1B06PH-ZZZ-AR-MOD-000001");

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].How, Is.EqualTo(SimilarNames.Closeness.SuppliedFieldsDiffer));
            Assert.That(found[0].Reason, Does.Contain("discipline"));
        }

        [Test]
        public void ADifferentTypeCodeIsCloseAndTheTypeIsNamed()
        {
            IList<SimilarNames.NearbyName> found = In("1104-PAR-1B06PH-ZZZ-BM-FED-000001");

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].How, Is.EqualTo(SimilarNames.Closeness.SuppliedFieldsDiffer));
            Assert.That(found[0].Reason, Does.Contain("type code"));
        }

        [Test]
        public void ADifferentNumberIsCloseAndTheNumberIsNamed()
        {
            IList<SimilarNames.NearbyName> found = In("1104-PAR-1B06PH-ZZZ-BM-MOD-000002");

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].How, Is.EqualTo(SimilarNames.Closeness.SuppliedFieldsDiffer));
            Assert.That(found[0].Reason, Does.Contain("number"));
        }

        [Test]
        public void TwoFieldsDifferingNamesTheFirstOfThem()
        {
            IList<SimilarNames.NearbyName> found = In("1104-PAR-1B06PH-L01-AR-MOD-000001");

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].Reason, Does.Contain("level"));
            Assert.That(found[0].Reason, Does.Not.Contain("discipline"));
        }

        [Test]
        public void ADifferentProjectCodeIsNotTheSuppliedFieldsReason()
        {
            IList<SimilarNames.NearbyName> found = In("9999-PAR-1B06PH-L01-BM-MOD-000001");

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].How, Is.EqualTo(SimilarNames.Closeness.SameBuilding));
        }

        [Test]
        public void ADifferentOriginatorIsNotTheSuppliedFieldsReason()
        {
            IList<SimilarNames.NearbyName> found = In("1104-XXX-1B06PH-L01-BM-MOD-000001");

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].How, Is.EqualTo(SimilarNames.Closeness.SameBuilding));
        }

        [Test]
        public void ANameWithADifferentNumberOfPartsIsNotTheSuppliedFieldsReason()
        {
            IList<SimilarNames.NearbyName> found = In("1104-PAR-1B06PH-ZZZ-BM-MOD");

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].How, Is.EqualTo(SimilarNames.Closeness.SameBuilding));
        }

        // ---------- the building code ----------

        [Test]
        public void TheSameBuildingCodeIsCloseAndTheCodeIsNamed()
        {
            IList<SimilarNames.NearbyName> found = In("SOMETHING-ELSE-1B06PH-WHATEVER");

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].How, Is.EqualTo(SimilarNames.Closeness.SameBuilding));
            Assert.That(found[0].Reason, Does.Contain("1B06PH"));
        }

        [Test]
        public void TheSameBuildingCodeInAnotherCaseIsStillClose()
        {
            Assert.That(In("1104-PAR-1b06ph-L01-BM-MOD-000001").Count, Is.EqualTo(1));
        }

        [Test]
        public void ADifferentBuildingIsNotClose()
        {
            Assert.That(In("1104-PAR-1C07BC-ZZZ-BM-MOD-000001").Count, Is.EqualTo(0));
        }

        [Test]
        public void ANameWithNoBuildingPartIsNotClose()
        {
            Assert.That(In("readme").Count, Is.EqualTo(0));
            Assert.That(In("1104-PAR").Count, Is.EqualTo(0));
        }

        [Test]
        public void AnEmptyNameInTheFolderIsSkippedAndDoesNotThrow()
        {
            IList<SimilarNames.NearbyName> found = In(string.Empty, null, "1104-PAR-1B06PH-L01-BM-MOD-000001");

            Assert.That(found.Count, Is.EqualTo(1));
        }

        // ---------- the order and the closest ----------

        [Test]
        public void EveryCloseNameComesBackInTheOrderTheyWereHandedIn()
        {
            IList<SimilarNames.NearbyName> found = In(
                "1104-PAR-1B06PH-L01-BM-MOD-000001",
                "1104-PAR-1C07BC-ZZZ-BM-MOD-000001",
                "1104-PAR-1B06PH-ZZZ-BM-MOD-000002");

            Assert.That(found.Count, Is.EqualTo(2));
            Assert.That(found[0].Name, Is.EqualTo("1104-PAR-1B06PH-L01-BM-MOD-000001"));
            Assert.That(found[1].Name, Is.EqualTo("1104-PAR-1B06PH-ZZZ-BM-MOD-000002"));
        }

        [Test]
        public void TheClosestIsTheOneThatDiffersOnlyInASuppliedField()
        {
            SimilarNames.NearbyName closest = SimilarNames.ClosestIn(
                Wanted,
                new[] { "OTHER-FILE-1B06PH-ANYTHING", "1104-PAR-1B06PH-L01-BM-MOD-000001" },
                Settings());

            Assert.That(closest, Is.Not.Null);
            Assert.That(closest.Name, Is.EqualTo("1104-PAR-1B06PH-L01-BM-MOD-000001"));
        }

        [Test]
        public void TheClosestFallsBackToTheBuildingCodeWhenNothingElseIsNearer()
        {
            SimilarNames.NearbyName closest = SimilarNames.ClosestIn(
                Wanted, new[] { "OTHER-FILE-1B06PH-ANYTHING" }, Settings());

            Assert.That(closest, Is.Not.Null);
            Assert.That(closest.How, Is.EqualTo(SimilarNames.Closeness.SameBuilding));
        }

        [Test]
        public void TheClosestIsNullWhenNothingIsClose()
        {
            Assert.That(
                SimilarNames.ClosestIn(Wanted, new[] { "1104-PAR-1C07BC-ZZZ-BM-MOD-000001" }, Settings()),
                Is.Null);
        }

        // ---------- the words the window and the log carry ----------

        [Test]
        public void TheNoteNamesTheFileItFound()
        {
            SimilarNames.NearbyName closest = SimilarNames.ClosestIn(
                Wanted, new[] { "1104-PAR-1B06PH-L01-BM-MOD-000001.nwf" }, Settings());

            string note = SimilarNames.Note(closest);

            Assert.That(note, Does.Contain("1104-PAR-1B06PH-L01-BM-MOD-000001.nwf"));
            Assert.That(note, Does.Contain("similar name"));
        }

        [Test]
        public void TheNoteIsEmptyWhenNothingWasFound()
        {
            Assert.That(SimilarNames.Note(null), Is.EqualTo(string.Empty));
        }

        [Test]
        public void TheNoteKeepsTheNameExactlyAsItWasHandedIn()
        {
            SimilarNames.NearbyName closest = SimilarNames.ClosestIn(
                Wanted, new[] { "1104-par-1B06PH-L01-bm-MOD-000001.NWF" }, Settings());

            Assert.That(SimilarNames.Note(closest), Does.Contain("1104-par-1B06PH-L01-bm-MOD-000001.NWF"));
        }

        [Test]
        public void TheLineUnderTheTableSaysToTypeTheCellOver()
        {
            string line = SimilarNames.WhatToDo(3);

            Assert.That(line, Does.Contain("3 groups are"));
            Assert.That(line, Does.Contain("NWF Name"));
            Assert.That(line, Does.Contain("Type the NWF Name cell over"));
        }

        [Test]
        public void TheLineUnderTheTableCountsOneGroupInTheSingular()
        {
            Assert.That(SimilarNames.WhatToDo(1), Does.Contain("1 group is"));
            Assert.That(SimilarNames.WhatToDo(1), Does.Not.Contain("groups are"));
        }

        [Test]
        public void TheLineUnderTheTableNamesBothChoicesAndActsOnNeither()
        {
            string line = SimilarNames.WhatToDo(2);

            Assert.That(line, Does.Contain("existing file"));
            Assert.That(line, Does.Contain("build a new one"));
        }

        // ---------- what the label does and does not become ----------

        [Test]
        public void TheShownLabelCarriesTheNoteAfterTheLabel()
        {
            string shown = RunPath.Shown(RunPath.FirstRun, ", but the NWF folder holds a similar name: x.nwf");

            Assert.That(shown, Does.StartWith(RunPath.FirstRun));
            Assert.That(shown, Does.Contain("x.nwf"));
        }

        [Test]
        public void TheShownLabelIsJustTheLabelWithNoNote()
        {
            Assert.That(RunPath.Shown(RunPath.FirstRun, null), Is.EqualTo(RunPath.FirstRun));
            Assert.That(RunPath.Shown(RunPath.FirstRun, string.Empty), Is.EqualTo(RunPath.FirstRun));
        }

        [Test]
        public void AMissingLabelReadsUnknownRatherThanEmpty()
        {
            Assert.That(RunPath.Shown(null, null), Is.EqualTo(RunPath.Unknown));
        }

        /// <summary>
        /// The one that matters. A widened label must never reach the counting, because
        /// Count maps an unknown label onto Unknown and both the confirm dialog and the
        /// RESULT block read those counts.
        /// </summary>
        [Test]
        public void TheWidenedLabelIsNeverWhatIsCounted()
        {
            string shown = RunPath.Shown(RunPath.FirstRun, ", but the NWF folder holds a similar name: x.nwf");

            IDictionary<string, int> widened = RunPath.Count(new[] { shown });
            IDictionary<string, int> plain = RunPath.Count(new[] { RunPath.FirstRun });

            Assert.That(widened[RunPath.Unknown], Is.EqualTo(1));
            Assert.That(widened[RunPath.FirstRun], Is.EqualTo(0));
            Assert.That(plain[RunPath.FirstRun], Is.EqualTo(1));
            Assert.That(plain[RunPath.Unknown], Is.EqualTo(0));
        }
    }
}
